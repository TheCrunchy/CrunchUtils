using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;

namespace CrunchUtilities
{
    public class ConfigFile : ViewModel
    {
        public bool PlayerMakeShip = false;
        public bool PlayerFixMe = false;
        public bool DeleteStone = false;
        public bool Withdraw = false;
        public bool PlayerEcoPay = false;
        public bool Deposit = false;
        public bool convertInGravity = false;
        public bool FactionShareDeposit = true;
        private int _cooldownInSeconds = 10 * 60;
        public bool IdentityUpdate = false;
        public bool Claim = false;
        public int ClaimPercent = 70;
        public int CooldownInSeconds { get => _cooldownInSeconds; set => SetValue(ref _cooldownInSeconds, value); }
    }
}
