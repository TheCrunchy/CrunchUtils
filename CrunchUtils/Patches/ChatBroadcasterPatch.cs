using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.EntityComponents.Blocks;
using Torch.Managers.PatchManager;

namespace CrunchUtilities.Patches
{
    [PatchShim]
    public static class ChatBroadcasterPatch
    {
        public static void Patch(PatchContext ctx)
        {
              ctx.GetPattern(update).Suffixes.Add(updatePatch);

        }

        internal static readonly MethodInfo update =
         typeof(MyChatBroadcastEntityComponent).GetMethod("IsValidTarget", BindingFlags.Instance | BindingFlags.NonPublic) ??
         throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo updatePatch =
                typeof(ChatBroadcasterPatch).GetMethod(nameof(IsValidTarget), BindingFlags.Static | BindingFlags.NonPublic) ??
                throw new Exception("Failed to find patch method");

        private static void IsValidTarget(MyChatBroadcastEntityComponent __instance, long targetEntityId, ref bool __result)
        {
            if (CrunchUtilitiesPlugin.file.DisableChatEveryoneBlock && __instance.BroadcastTarget == BroadcastTarget.Everyone)
            {
                __result = false;
            }
        }
    }
}
