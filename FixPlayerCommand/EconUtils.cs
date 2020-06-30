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

        //This thing is broke, dont use it
        public static void TransferToFactionAccount(long id, long factionID, Int64 amount)
        {
                MyAccountInfo account;
        
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
            long factionBalance = EconUtils.getBalance(factionID) + amount;
            long playerBalance = EconUtils.getBalance(id) - amount;
            CrunchUtilitiesPlugin.Log.Info("Ignore this error");
           MyBankingSystem.RequestTransfer_BroadcastToClients(id, factionID, amount, EconUtils.getBalance(id), EconUtils.getBalance(factionID));
            EconUtils.addMoney(factionID, amount);
            EconUtils.takeMoney(id, amount);
            //MyBankingSystem.ChangeBalance(factionID, factionBalance);
            //MyBankingSystem.ChangeBalance(id, playerBalance);
    
            //MyBankingSystem.Static.SaveData();
       
            return;
        }

        public static void takeMoney(long walletID, Int64 amount)
        {
            if (getBalance(walletID) >= amount)
            {
                amount = amount * -1;
                MyBankingSystem.ChangeBalance(walletID, amount);
            }
            return;
        }
    }
}
