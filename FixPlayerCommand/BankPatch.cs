using NLog;
using NLog.Config;
using NLog.Targets;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers.PatchManager;
using VRage.Game.ModAPI;
using VRageMath;

namespace CrunchUtilities
{
    [PatchShim]
    public static class BankPatch
    {
        public static Logger log = LogManager.GetLogger("Econ");
        public static void ApplyLogging()
        {

            var rules = LogManager.Configuration.LoggingRules;

            for (int i = rules.Count - 1; i >= 0; i--)
            {

                var rule = rules[i];

                if (rule.LoggerNamePattern == "Econ")
                    rules.RemoveAt(i);
            }



            var logTarget = new FileTarget
            {
                FileName = "Logs/Econ-" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-"+ DateTime.Now.Year + ".txt",
                Layout = "${var:logStamp} ${var:logContent}"
            };

            var logRule = new LoggingRule("Econ", LogLevel.Debug, logTarget)
            {
                Final = true
            };

            rules.Insert(0, logRule);

            LogManager.Configuration.Reload();
        }
        internal static readonly MethodInfo update =
         typeof(MyBankingSystem).GetMethod("ChangeBalance", BindingFlags.Static | BindingFlags.Public) ??
         throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo updatePatch =
                typeof(BankPatch).GetMethod(nameof(BalanceChangedMethod), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");



        public static void Patch(PatchContext ctx)
        {
            ApplyLogging();
            ctx.GetPattern(update).Suffixes.Add(updatePatch);

        }

        public static void BalanceChangedMethod2(
      MyAccountInfo oldAccountInfo,
      MyAccountInfo newAccountInfo)
        {
           
            if (CrunchUtilitiesPlugin.file != null && CrunchUtilitiesPlugin.file.EconomyChangesInLog)
            {
                if (MySession.Static.Factions.TryGetFactionById(newAccountInfo.OwnerIdentifier) != null && newAccountInfo.Log.Count() > 0)
                {
                    log.Info("FACTIONLOG Faction balance change " + MySession.Static.Factions.TryGetFactionById(newAccountInfo.OwnerIdentifier).Tag + " " + newAccountInfo.OwnerIdentifier + " amount: " + String.Format("{0:n0}", newAccountInfo.Log.Last().Amount) + " SC. by " + newAccountInfo.Log.Last().ChangeIdentifier);
                }
                else
                {
                    if (MySession.Static.Players.TryGetSteamId(oldAccountInfo.OwnerIdentifier) > 0)
                    {
                        long change;
                        if (oldAccountInfo.Balance > newAccountInfo.Balance)
                        {
                            change = oldAccountInfo.Balance - newAccountInfo.Balance;
                            log.Info("Player Balance decreased by: "+ String.Format("{0:n0}", change) + " from " + String.Format("{0:n0}", oldAccountInfo.Balance) + " SC to " + String.Format("{0:n0}", newAccountInfo.Balance) + " SC. steam id: " + MySession.Static.Players.TryGetSteamId(oldAccountInfo.OwnerIdentifier) + " identity id: " + oldAccountInfo.OwnerIdentifier);
                        }
                        else
                        {
                            change = newAccountInfo.Balance - oldAccountInfo.Balance;
                            log.Info("Player Balance increased by: " + String.Format("{0:n0}", change) + " from " + String.Format("{0:n0}", oldAccountInfo.Balance) + " SC to " + String.Format("{0:n0}", newAccountInfo.Balance) + " SC. steam id: " + MySession.Static.Players.TryGetSteamId(oldAccountInfo.OwnerIdentifier) + " identity id: " + oldAccountInfo.OwnerIdentifier);
                        }
                  
                    }
                }
            }
        }


        public static void BalanceChangedMethod(long identifierId, long amount)
        {
            if (amount == 0)
            {

                return;
            }

            if (CrunchUtilitiesPlugin.file != null && CrunchUtilitiesPlugin.file.EcoChatMessages)
            {
                if (Sync.Players.TryGetPlayerId(identifierId, out MyPlayer.PlayerId player))
                {
                    if (MySession.Static.Players.TryGetPlayerById(player, out MyPlayer pp))
                    {


                        //  foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                        //    {
                        //     if (player.Identity.IdentityId == identifierId)
                        //    {
                        if (amount > 0)
                        {
                            Commands.SendMessage("CrunchEcon", "Balance increased by: " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)pp.Id.SteamId);

                        }
                        else
                        {
                            Commands.SendMessage("CrunchEcon", "Balance decreased by: " + String.Format("{0:n0}", amount) + " SC", Color.Red, (long)pp.Id.SteamId);
                        }

                    }
                }
            }
            //     }
            //   }
        }
    }
}
