using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;

namespace CrunchUtilities.Patches
{
    [PatchShim]
    public static class DisableAIBlockFlee
    {
        internal static readonly MethodInfo flee =
            typeof(MyDefensiveCombatBlock).GetMethod("Flee", BindingFlags.Instance | BindingFlags.Public, null,
                new Type[] { typeof(Boolean) }, null) ??
            throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo patchFlee =
            typeof(DisableAIBlockFlee).GetMethod(nameof(Flee), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {

            ctx.GetPattern(flee).Prefixes.Add(patchFlee);
        }

        public static bool Flee(MyDefensiveCombatBlock __instance, bool force = false)
        {
            if (__instance.Enemy == null)
            {
                //   Core.Log.Info("flee intercepted, null enemy");
                return false;
            }

            //    Core.Log.Info("enemy not null, allowing");
            return true;
        }
    }
}
