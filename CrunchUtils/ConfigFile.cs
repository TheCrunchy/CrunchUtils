using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Torch;
using Torch.Views;

namespace CrunchUtilities
{
    public class ConfigFile : ViewModel
    {
        // ---------- Economy ----------
        private string _econName = "CrunchEcon";
        [Display(Name = "Economy name", GroupName = "Economy", Order = 0, Description = "Name of the economy/currency shown to players.")]
        public string EconName { get => _econName; set => SetValue(ref _econName, value); }

        private bool _economyChangesInLog = true;
        [Display(Name = "Economy changes in log", GroupName = "Economy", Order = 1)]
        public bool EconomyChangesInLog { get => _economyChangesInLog; set => SetValue(ref _economyChangesInLog, value); }

        private bool _ecoChatMessages = true;
        [Display(Name = "Economy chat messages", GroupName = "Economy", Order = 2)]
        public bool EcoChatMessages { get => _ecoChatMessages; set => SetValue(ref _ecoChatMessages, value); }

        private bool _playerEcoPay = false;
        [Display(Name = "Player pay command", GroupName = "Economy", Order = 3)]
        public bool PlayerEcoPay { get => _playerEcoPay; set => SetValue(ref _playerEcoPay, value); }

        private bool _withdraw = false;
        [Display(Name = "Enable withdraw", GroupName = "Economy", Order = 4)]
        public bool Withdraw { get => _withdraw; set => SetValue(ref _withdraw, value); }

        private long _ecoWithdrawMax = int.MaxValue;
        [Display(Name = "Max withdraw", GroupName = "Economy", Order = 5)]
        public long EcoWithdrawMax { get => _ecoWithdrawMax; set => SetValue(ref _ecoWithdrawMax, value); }

        private bool _deposit = false;
        [Display(Name = "Enable deposit", GroupName = "Economy", Order = 6)]
        public bool Deposit { get => _deposit; set => SetValue(ref _deposit, value); }

        private bool _factionShareDeposit = true;
        [Display(Name = "Faction share deposit", GroupName = "Economy", Order = 7)]
        public bool FactionShareDeposit { get => _factionShareDeposit; set => SetValue(ref _factionShareDeposit, value); }

        private bool _convertInGravity = false;
        [Display(Name = "Convert in gravity", GroupName = "Economy", Order = 8)]
        public bool convertInGravity { get => _convertInGravity; set => SetValue(ref _convertInGravity, value); }

        // ---------- Logging / Info ----------
        private bool _logFactionStuff = true;
        [Display(Name = "Log faction events", GroupName = "Logging", Order = 0)]
        public bool LogFactionStuff { get => _logFactionStuff; set => SetValue(ref _logFactionStuff, value); }

        private bool _logNeutralsDamagingEachOther = false;
        [Display(Name = "Log neutrals damaging each other", GroupName = "Logging", Order = 1)]
        public bool LogNeutralsDamagingEachOther { get => _logNeutralsDamagingEachOther; set => SetValue(ref _logNeutralsDamagingEachOther, value); }

        private bool _showFactionTagsOnDamageGrid = false;
        [Display(Name = "Show faction tags on damage grid", GroupName = "Logging", Order = 2)]
        public bool ShowFactionTagsOnDamageGrid { get => _showFactionTagsOnDamageGrid; set => SetValue(ref _showFactionTagsOnDamageGrid, value); }

        private bool _pcuCountShowProjPCU = true;
        [Display(Name = "PCU count shows projected PCU", GroupName = "Logging", Order = 3)]
        public bool PcuCountShowProjPCU { get => _pcuCountShowProjPCU; set => SetValue(ref _pcuCountShowProjPCU, value); }

        private bool _facInfo = true;
        [Display(Name = "Faction info command", GroupName = "Logging", Order = 4)]
        public bool facInfo { get => _facInfo; set => SetValue(ref _facInfo, value); }

        // ---------- Player commands ----------
        private bool _shipSaleCommands = false;
        [Display(Name = "Ship sale commands", GroupName = "Player Commands", Order = 0)]
        public bool ShipSaleCommands { get => _shipSaleCommands; set => SetValue(ref _shipSaleCommands, value); }

        private bool _playerMakeShip = false;
        [Display(Name = "Player make ship", GroupName = "Player Commands", Order = 1)]
        public bool PlayerMakeShip { get => _playerMakeShip; set => SetValue(ref _playerMakeShip, value); }

        private bool _playerFixMe = false;
        [Display(Name = "Player fixme", GroupName = "Player Commands", Order = 2)]
        public bool PlayerFixMe { get => _playerFixMe; set => SetValue(ref _playerFixMe, value); }

        private int _cooldownInSeconds = 10 * 60;
        [Display(Name = "Command cooldown (s)", GroupName = "Player Commands", Order = 3)]
        public int CooldownInSeconds { get => _cooldownInSeconds; set => SetValue(ref _cooldownInSeconds, value); }

        private int _respawnCooldownInSeconds = 20 * 60;
        [Display(Name = "Respawn cooldown (s)", GroupName = "Player Commands", Order = 4)]
        public int RespawnCooldownInSeconds { get => _respawnCooldownInSeconds; set => SetValue(ref _respawnCooldownInSeconds, value); }

        // ---------- Claim ----------
        private bool _claim = false;
        [Display(Name = "Enable claim command", GroupName = "Claim", Order = 0)]
        public bool Claim { get => _claim; set => SetValue(ref _claim, value); }

        private bool _claimOnlyForLeaders = true;
        [Display(Name = "Only leaders/founders can claim", GroupName = "Claim", Order = 1)]
        public bool ClaimOnlyForLeaders { get => _claimOnlyForLeaders; set => SetValue(ref _claimOnlyForLeaders, value); }

        private bool _claimOnlyFactionGrids = false;
        [Display(Name = "Only faction grids can be claimed", GroupName = "Claim", Order = 2)]
        public bool ClaimOnlyFactionGrids { get => _claimOnlyFactionGrids; set => SetValue(ref _claimOnlyFactionGrids, value); }

        private bool _claimFactionGridsOnlyLeaders = false;
        [Display(Name = "Only leaders can claim faction grids", GroupName = "Claim", Order = 3)]
        public bool ClaimFactionGridsOnlyLeaders { get => _claimFactionGridsOnlyLeaders; set => SetValue(ref _claimFactionGridsOnlyLeaders, value); }

        private int _claimPercent = 70;
        [Display(Name = "Claim percent required", GroupName = "Claim", Order = 4)]
        public int ClaimPercent { get => _claimPercent; set => SetValue(ref _claimPercent, value); }

        private bool _disableClaimCommandInSafezone = false;
        [Display(Name = "Disable claim in safezone", GroupName = "Claim", Order = 5, Description = "Players cannot claim a grid that is inside a safezone.")]
        public bool DisableClaimCommandInSafezone { get => _disableClaimCommandInSafezone; set => SetValue(ref _disableClaimCommandInSafezone, value); }

        // ---------- Scrap / stone ----------
        private bool _scrapMetalPatch = false;
        [Display(Name = "Scrap metal patch", GroupName = "Scrap / Stone", Order = 0)]
        public bool ScrapMetalPatch { get => _scrapMetalPatch; set => SetValue(ref _scrapMetalPatch, value); }

        private bool _scrapMetalPatchNeverSpawn = false;
        [Display(Name = "Scrap metal never spawn", GroupName = "Scrap / Stone", Order = 1)]
        public bool ScrapMetalPatchNeverSpawn { get => _scrapMetalPatchNeverSpawn; set => SetValue(ref _scrapMetalPatchNeverSpawn, value); }

        private bool _deleteStone = false;
        [Display(Name = "Delete stone command", GroupName = "Scrap / Stone", Order = 2)]
        public bool DeleteStone { get => _deleteStone; set => SetValue(ref _deleteStone, value); }

        private bool _deleteStoneAuto = false;
        [Display(Name = "Delete stone automatically", GroupName = "Scrap / Stone", Order = 3)]
        public bool DeleteStoneAuto { get => _deleteStoneAuto; set => SetValue(ref _deleteStoneAuto, value); }

        // ---------- Misc ----------
        private bool _fixTradeStation = false;
        [Display(Name = "Fix trade station", GroupName = "Misc", Order = 0)]
        public bool FixTradeStation { get => _fixTradeStation; set => SetValue(ref _fixTradeStation, value); }

        private bool _disableChatEveryoneBlock = false;
        [Display(Name = "Disable chat everyone block", GroupName = "Misc", Order = 1)]
        public bool DisableChatEveryoneBlock { get => _disableChatEveryoneBlock; set => SetValue(ref _disableChatEveryoneBlock, value); }

        private bool _sendFactionBroadcastsToFactionChat = false;
        [Display(Name = "Send faction broadcasts to faction chat", GroupName = "Misc", Order = 2)]
        public bool SendFactionBroadcastsToFactionChat { get => _sendFactionBroadcastsToFactionChat; set => SetValue(ref _sendFactionBroadcastsToFactionChat, value); }

        // ---------- Factions ----------
        private bool _factionsNeutralOnCreation = false;
        [Display(Name = "Factions neutral on creation", GroupName = "Factions", Order = 0)]
        public bool FactionsNeutralOnCreation { get => _factionsNeutralOnCreation; set => SetValue(ref _factionsNeutralOnCreation, value); }

        private string _excludedFactionTags = "SPRT,EXAMPLE1,EXAMPLE2";
        [Display(Name = "Excluded faction tags", GroupName = "Factions", Order = 1, Description = "Comma separated list of faction tags.")]
        public string ExcludedFactionTags
        {
            get => _excludedFactionTags;
            set
            {
                SetValue(ref _excludedFactionTags, value);
                _split = false; // rebuild the cached list next time IsExcluded is called
            }
        }

        [XmlIgnore]
        private bool _split = false;
        [XmlIgnore]
        private List<string> _excluded = new List<string>();

        public bool IsExcluded(string tag)
        {
            if (!_split)
            {
                _excluded = (ExcludedFactionTags ?? "").Trim().Replace(" ", "").Split(',').ToList();
                _split = true;
            }
            return _excluded.Contains(tag);
        }
    }
}