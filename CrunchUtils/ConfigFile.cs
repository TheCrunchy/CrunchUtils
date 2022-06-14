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
        public Boolean PlayerAlertEnabled = false;
        public int SecondsBetweenPlayerAlert = 10;
        public Boolean ShipSaleCommands = false;
        public Boolean NobodyPatch = false;
        public Boolean ShowFactionTagsOnDamageGrid = false;
        public Boolean LogFactionStuff = true;
        public Boolean EconomyChangesInLog = true;
        public Boolean LogNeutralsDamagingEachOther = false;
        public Boolean PcuCountShowProjPCU = true;
        public Boolean EcoChatMessages = true;
        public bool ScrapMetalPatch = false;
        public bool projectorOwnershipPatch = false;
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
        public int ClaimPercent = 70;
        public int CooldownInSeconds { get => cooldownInSeconds; set => SetValue(ref cooldownInSeconds, value); }
        public int RespawnCooldownInSeconds { get => respawncooldownInSeconds; set => SetValue(ref respawncooldownInSeconds, value); }
    }
}
