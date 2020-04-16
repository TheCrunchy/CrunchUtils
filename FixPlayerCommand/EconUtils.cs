using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrunchUtilities
{
    public static class EconUtils
    {
        //getBalance of Wallet
        public static long getBalance(long walletID)
        {
            MyAccountInfo info;
            if (MyBankingSystem.Static.TryGetAccountInfo(walletID, out info))
            {
                return info.Balance;
            }
            return 0L;
        }
        public static void addMoney(long walletID, Int64 amount)
        {
            MyBankingSystem.ChangeBalance(walletID, amount);

            return;
        }
        public static void takeMoney(long walletID, Int64 amount)
        {
            if (getBalance(walletID) > amount)
            {
                MyBankingSystem.ChangeBalance(walletID, amount);
            }
            return;
        }

        public static void doPayment2Players(long walletIDSender, long walletIDRecipient, Int64 amount)
        {
            if (getBalance(walletIDSender) > amount)
            {
                MyBankingSystem.ChangeBalance(walletIDSender, amount);
                // Int64 amountTaxApplied = amount * tax;
                Int64 amountTaxApplied = amount;
                MyBankingSystem.ChangeBalance(walletIDRecipient, amountTaxApplied);
            }
            return;
        }
    }
}
