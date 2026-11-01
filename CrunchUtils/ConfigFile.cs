using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;
using VRage;

namespace CrunchUtilities
{
    public class ConfigFile : ViewModel
    {
        public string EconName = "CrunchEcon";
        public Boolean ShipSaleCommands = false;
        public Boolean ShowFactionTagsOnDamageGrid = false;
        public Boolean LogFactionStuff = true;
        public Boolean EconomyChangesInLog = true;
        public Boolean LogNeutralsDamagingEachOther = false;
        public Boolean PcuCountShowProjPCU = true;
        public Boolean EcoChatMessages = true;
        public bool ScrapMetalPatch = false;
        public bool ScrapMetalPatchNeverSpawn = false;
        public bool FixTradeStation = false;
        public bool PlayerMakeShip = false;
        public bool PlayerFixMe = false;
        public bool DeleteStone = false;
       // public bool UsingDraconisEliteDrills = false;
       /// public double Drill2xAmount = 0.8;
       // public double Drill4xAmount = 0.6;
      ///  public double Drill8xAmount = 0.2;
        public long EcoWithdrawMax = int.MaxValue;
        public bool DeleteStoneAuto = false;
        // CHANGE: configurable list of ore subtypes treated as "stone". Used by the !stone command, drills named
        // "!stone" and !togglestone so all three always remove exactly the same ores (they used to match differently).
        // Comma separated like ExcludedFactionTags, e.g. "Stone,Ice" or "Ore/Stone,Ore/Ice". Defaults to vanilla stone.
        // This is the MASTER list, it applies to every player. Players can add their own ores on top of it with
        // !stoneore (see PlayerOreLists), but cant remove master ores. An empty master list means no master rules.
        // Ore only on purpose, drills only produce MyObjectBuilder_Ore. A string is used instead of List<string>
        // because XmlSerializer appends to an initialised list on load, which would duplicate the default every restart.
        public string StoneRemovalOreSubtypes = "Stone";
        // parsed cache of StoneRemovalOreSubtypes, not serialized (private). !crunch reload creates a new ConfigFile so it rebuilds.
        private HashSet<string> stoneRemovalOres;
        public bool Withdraw = false;
        public bool facInfo = true;
        public bool PlayerEcoPay = false;
        public bool Deposit = false;
        public bool convertInGravity = false;
        public bool FactionShareDeposit = true;
        private int cooldownInSeconds = 10 * 60;
        private int respawncooldownInSeconds = 20 * 60;
        public bool IdentityUpdate = false;
        public bool Claim = false;
        public bool ClaimOnlyForLeaders = true;
        public bool ClaimOnlyFactionGrids = false;
        public bool ClaimFactionGridsOnlyLeaders = false;
        public int ClaimPercent = 70;
        public int CooldownInSeconds { get => cooldownInSeconds; set => SetValue(ref cooldownInSeconds, value); }
        public int RespawnCooldownInSeconds { get => respawncooldownInSeconds; set => SetValue(ref respawncooldownInSeconds, value); }
        public bool FactionsNeutralOnCreation = false;
        public string ExcludedFactionTags = "SPRT,EXAMPLE1,EXAMPLE2";
        private bool Split = false;
        private List<string> excluded = new List<string>();

        public bool DisableChatEveryoneBlock = false;
        public bool SendFactionBroadcastsToFactionChat = false;
        public bool IsExcluded(string tag)
        {
            if (Split) return excluded.Contains(tag);
            excluded = ExcludedFactionTags.Trim().Replace(" ", "").Split(',').ToList();
            Split = true;

            return excluded.Contains(tag);
        }

        // CHANGE: single shared matcher for stone removal. Exact, case-insensitive match on the ore subtype.
        // Previously the drill patch used ToLower().Contains("stone") and the command used a case-sensitive Contains("Stone"),
        // so modded ores like "Moonstone" were voided by drills but kept by !stone. Exact matching fixes that.
        // CHANGE: identityId is whose personal list also applies: the drill owner for drills, the player running !stone.
        // Removed if the ore is on the master list OR on that player's personal list.
        public bool IsStoneRemovalOre(string oreSubtype, long identityId)
        {
            if (string.IsNullOrEmpty(oreSubtype)) return false;
            return GetStoneRemovalOres().Contains(oreSubtype) || PlayerOreLists.GetForIdentity(identityId).Contains(oreSubtype);
        }

        // CHANGE: returns the parsed stone removal ore set, building it from StoneRemovalOreSubtypes on first use.
        // Treat the returned set as read only, it is shared with the drill patch. To change it use SetStoneRemovalOres.
        public HashSet<string> GetStoneRemovalOres()
        {
            var ores = stoneRemovalOres;
            if (ores == null)
            {
                // drill callbacks can run off the game thread, so build into a local and assign once finished
                // (worst case two threads both build the same set, never a half-built one)
                ores = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in (StoneRemovalOreSubtypes ?? "").Split(','))
                {
                    var subtype = ToOreSubtype(entry);
                    if (subtype.Length > 0)
                        ores.Add(subtype);
                }
                // empty/blank master list means no master rules, only players' personal lists apply
                stoneRemovalOres = ores;
            }
            return ores;
        }

        // CHANGE: allow "Stone", "Ore/Stone" or "MyObjectBuilder_Ore/Stone" (only ore can be drilled anyway).
        // Shared by the config parser and the !crunch stoneore commands so both accept the same input.
        public static string ToOreSubtype(string entry)
        {
            if (entry == null) return "";
            return entry.Substring(entry.LastIndexOf('/') + 1).Trim();
        }

        // CHANGE: used by the !crunch stoneore add/remove commands. Updates the saved string and swaps in a brand new set
        // (never edits the live one) so a drill reading the old set on another thread is never affected mid-change.
        // The caller is responsible for saving the config.
        public void SetStoneRemovalOres(IEnumerable<string> oreSubtypes)
        {
            var ores = new HashSet<string>(oreSubtypes, StringComparer.OrdinalIgnoreCase);
            StoneRemovalOreSubtypes = string.Join(",", ores);
            stoneRemovalOres = ores;
        }
    }
}
