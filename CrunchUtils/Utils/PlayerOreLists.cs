using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Sandbox.Game.World;

namespace CrunchUtilities
{
    // CHANGE (new file): per player stone removal ore lists.
    // Each player can have their own list in <instance>\CrunchOres\<steamid>.json, on top of the master list in
    // CrunchUtils.xml (StoneRemovalOreSubtypes). The ores removed for a player are master + personal, so a player can
    // never stop the master list from applying, they can only add to it.
    //  - drills (named !stone or via !togglestone) use the drill OWNER's list
    //  - the !stone command uses the list of the player RUNNING it, even on faction members' containers
    // Files are only read the first time a player's list is needed, then kept in memory. Changes update memory and
    // the file together. The caches only live until the server restarts.
    public static class PlayerOreLists
    {
        public const string FolderName = "CrunchOres";

        // a player's list starts with this when they first create it
        private static readonly string[] DefaultOres = { "Stone" };

        // identity id -> steam id. Lookups for 0 (no steam id, e.g. NPCs) are not cached, so a player that has no
        // mapping yet is picked up later. The game's own lookup is a dictionary read, so that is still cheap.
        private static readonly ConcurrentDictionary<long, ulong> SteamIds = new ConcurrentDictionary<long, ulong>();

        // steam id -> that player's personal ores. Players without a file get an empty set so the disk is only hit once.
        // Sets are never edited after being stored, changes swap in a new set (the drill patch can read from other threads).
        private static readonly ConcurrentDictionary<ulong, HashSet<string>> Lists = new ConcurrentDictionary<ulong, HashSet<string>>();

        // steam ids that have a file (or had one created this session), used to know when to apply DefaultOres
        private static readonly ConcurrentDictionary<ulong, bool> HasFile = new ConcurrentDictionary<ulong, bool>();

        private static readonly HashSet<string> Empty = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object WriteLock = new object();

        private static string Folder => Path.Combine(CrunchUtilitiesPlugin.path, FolderName);
        private static string FilePath(ulong steamId) => Path.Combine(Folder, steamId + ".json");

        public static ulong GetSteamId(long identityId)
        {
            if (identityId == 0)
                return 0;
            if (SteamIds.TryGetValue(identityId, out ulong steamId))
                return steamId;

            steamId = MySession.Static?.Players?.TryGetSteamId(identityId) ?? 0;
            if (steamId != 0)
                SteamIds[identityId] = steamId;
            return steamId;
        }

        // personal ores for an identity (drill owner or command player), empty if they have none
        public static HashSet<string> GetForIdentity(long identityId)
        {
            ulong steamId = GetSteamId(identityId);
            return steamId == 0 ? Empty : Get(steamId);
        }

        // personal ores for a steam id, read from disk on first use only
        public static HashSet<string> Get(ulong steamId)
        {
            return Lists.GetOrAdd(steamId, Load);
        }

        public static bool Exists(ulong steamId)
        {
            Get(steamId);
            return HasFile.ContainsKey(steamId);
        }

        // used by the player !stoneore add/remove commands. A player's first change starts from DefaultOres (Stone).
        // Updates memory first, then writes the file.
        public static HashSet<string> Update(ulong steamId, Func<HashSet<string>, IEnumerable<string>> change)
        {
            var current = Exists(steamId) ? Get(steamId) : new HashSet<string>(DefaultOres, StringComparer.OrdinalIgnoreCase);
            var updated = new HashSet<string>(change(current), StringComparer.OrdinalIgnoreCase);
            Lists[steamId] = updated;
            HasFile[steamId] = true;
            Save(steamId, updated);
            return updated;
        }

        private static HashSet<string> Load(ulong steamId)
        {
            string file = FilePath(steamId);
            try
            {
                if (!File.Exists(file))
                    return Empty;

                using (var stream = File.OpenRead(file))
                {
                    var data = (PlayerOreFile)new DataContractJsonSerializer(typeof(PlayerOreFile)).ReadObject(stream);
                    HasFile[steamId] = true;
                    var ores = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var entry in data?.Ores ?? new List<string>())
                    {
                        var subtype = ConfigFile.ToOreSubtype(entry);
                        if (subtype.Length > 0)
                            ores.Add(subtype);
                    }
                    return ores;
                }
            }
            catch (Exception e)
            {
                // a broken file is treated as empty for this session (the master list still applies). If the player
                // changes their list it gets rewritten.
                CrunchUtilitiesPlugin.Log.Error(e, $"Could not read stone ore list {file}, using an empty list");
                return Empty;
            }
        }

        private static void Save(ulong steamId, HashSet<string> ores)
        {
            string file = FilePath(steamId);
            string temp = file + ".tmp";
            lock (WriteLock)
            {
                try
                {
                    Directory.CreateDirectory(Folder);
                    // write to a temp file then swap it in, so a crash mid write cant leave a half written list
                    using (var stream = File.Create(temp))
                    using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, false, true))
                    {
                        new DataContractJsonSerializer(typeof(PlayerOreFile)).WriteObject(writer, new PlayerOreFile { Ores = ores.ToList() });
                    }
                    if (File.Exists(file))
                        File.Replace(temp, file, null);
                    else
                        File.Move(temp, file);
                }
                catch (Exception e)
                {
                    CrunchUtilitiesPlugin.Log.Error(e, $"Could not save stone ore list {file}");
                }
            }
        }

        // file layout: { "Ores": [ "Stone", "Ice" ] }
        [DataContract]
        private class PlayerOreFile
        {
            [DataMember(Name = "Ores")]
            public List<string> Ores { get; set; }
        }
    }
}
