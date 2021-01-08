using NLog;
using Sandbox.Game.World;
using System;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Torch;
using Torch.API;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using Torch.Session;
using System.Timers;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;
using System.Collections;
using Torch.API.Managers;
using Torch.API.Session;
using System.IO;
using Sandbox.Game.Entities;
using VRage.Groups;
using VRage.Game;
using System.Collections.Concurrent;
using Sandbox.Game.Entities.Cube;
using System.Collections.Generic;
using Sandbox.Engine.Multiplayer;

namespace CrunchUtilities
{
    public class CrunchUtilitiesPlugin : TorchPluginBase
    {

        public static Logger Log = LogManager.GetCurrentClassLogger();
        public static ConfigFile file;
        public static string path;
        public Dictionary<long, CurrentCooldown> CurrentCooldownMap { get; } = new Dictionary<long, CurrentCooldown>();
 
        public long Cooldown { get { return file.CooldownInSeconds * 1000; } }
        private static TorchSessionState derp;
        public Dictionary<long, CurrentCooldown> CurrentCooldownMapFix { get; } = new Dictionary<long, CurrentCooldown>();

        private static Timer aTimer = new Timer();
        public static ConfigFile LoadConfig()
        {
            FileUtils utils = new FileUtils();
            file = utils.ReadFromXmlFile<ConfigFile>(path + "\\CrunchUtils.xml");
            if (file.IdentityUpdate)
            {
                aTimer.Enabled = false;
                aTimer.Interval = 150000;
                aTimer.Elapsed += OnTimedEventA;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
            return file;
        }
        public static ConfigFile SaveConfig()
        {
            FileUtils utils = new FileUtils();
            utils.WriteToXmlFile<ConfigFile>(path + "\\CrunchUtils.xml", file);
            if (file.IdentityUpdate)
            {
                aTimer.Enabled = false;
                aTimer.Interval = 150000;
                aTimer.Elapsed += OnTimedEventA;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
            return file;
        }
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            Log.Info("Loading Crunch Utils");
            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            SetupConfig();
            if (sessionManager != null)
            {
                sessionManager.SessionStateChanged += SessionChanged;
            }
            else
            {
                Log.Warn("No session manager loaded!");
            }

        }
        private void SetupConfig()
        {
            FileUtils utils = new FileUtils();
            path = StoragePath;
            if (File.Exists(StoragePath + "\\CrunchUtils.xml"))
            {
                file = utils.ReadFromXmlFile<ConfigFile>(StoragePath + "\\CrunchUtils.xml");
                utils.WriteToXmlFile<ConfigFile>(StoragePath + "\\CrunchUtils.xml", file, false);
            }
            else { 
                file = new ConfigFile();
                utils.WriteToXmlFile<ConfigFile>(StoragePath + "\\CrunchUtils.xml", file, false);
            }
            if (file.IdentityUpdate)
            {
                aTimer.Enabled = false;
                aTimer.Interval = 150000;
                aTimer.Elapsed += OnTimedEventA;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
        }

        //method from lord tylus
        public static MyIdentity GetIdentityByNameOrId(string playerNameOrSteamId)
        {
            foreach (var identity in MySession.Static.Players.GetAllIdentities())
            {
                if (identity.DisplayName == playerNameOrSteamId)
                    return identity;
                if (ulong.TryParse(playerNameOrSteamId, out ulong steamId))
                {
                    ulong id = MySession.Static.Players.TryGetSteamId(identity.IdentityId);
                    if (id == steamId)
                        return identity;
                }
            }
            return null;
        }

        private static void OnTimedEventA(Object source, System.Timers.ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                UpdateNamesTask();
            });
        }
        public static void UpdateNamesTask()
        {
          
            
        //    if (derp == TorchSessionState.Loaded) {
                Log.Info("Updating names");
                foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                {
                    string name = MyMultiplayer.Static.GetMemberName(player.Id.SteamId);
                    MyIdentity identity = GetIdentityByNameOrId(player.Id.SteamId.ToString());

                    if (!player.DisplayName.Equals(name))
                    {
                        Log.Info("Updating name of : " + name + " from : " + player.DisplayName);
                        player.Identity.SetDisplayName(MyMultiplayer.Static.GetMemberName(player.Id.SteamId));
                        identity.SetDisplayName(MyMultiplayer.Static.GetMemberName(player.Id.SteamId));

                    }
               // }
            }
        }
        private void SessionChanged(ITorchSession session, TorchSessionState newState)
        {
            //Do something here in the future
            Log.Info("Session-State is now " + newState);
            if (newState == TorchSessionState.Loaded)
            {
                derp = TorchSessionState.Loaded;
            }

        }
    }
}
