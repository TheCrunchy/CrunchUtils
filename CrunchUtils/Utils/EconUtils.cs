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
        public static long GetBalance(long walletID)
        {
            if (MyBankingSystem.Static.TryGetAccountInfo(walletID, out MyAccountInfo info))              
                return info.Balance;

            return 0L;
        }

        public static void AddMoney(long walletID, long amount)
        {
            MyBankingSystem.ChangeBalance(walletID, amount);

            return;
        }

        //This thing is broke, dont use it
        public static void TransferToFactionAccount(long id, long factionID, long amount)
        {
       
           // MyBankingSystem.Static.TryGetAccountInfo(factionID, out account);
          //      List<MyAccountLogEntry> Log = account.Log.ToList();
          //  MyAccountLogEntry entry = new MyAccountLogEntry()
           // {
            //    Amount = amount,
            //    DateTime = DateTime.Now,
            //    ChangeIdentifier = factionID
           // };
           // //Log.Add(entry);
            //MyAccountInfo account2 = account;
           // account2.Log = Log.ToArray();
          
        //    foreach (MyAccountLogEntry e in account.Log.ToList())
            //    {
               //     CrunchUtilitiesPlugin.Log.Info(e.Amount);
              //  }
            EconUtils.AddMoney(factionID, amount);
            EconUtils.TakeMoney(id, amount);
            //MyBankingSystem.ChangeBalance(factionID, factionBalance);
            //MyBankingSystem.ChangeBalance(id, playerBalance);
    
            //MyBankingSystem.Static.SaveData();
       
            return;
        }

        public static void TakeMoney(long walletID, long amount)
        {
            if (GetBalance(walletID) >= amount)
            {
                amount *= -1;
                MyBankingSystem.ChangeBalance(walletID, amount);
            }
            return;
        }
    }
}
