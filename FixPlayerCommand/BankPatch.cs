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
        internal static readonly MethodInfo update =
         typeof(MyBankingSystem).GetMethod("ChangeBalance", BindingFlags.Static | BindingFlags.Public) ??
         throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo updatePatch =
                typeof(BankPatch).GetMethod(nameof(BalanceChangedMethod), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {

            ctx.GetPattern(update).Suffixes.Add(updatePatch);

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
