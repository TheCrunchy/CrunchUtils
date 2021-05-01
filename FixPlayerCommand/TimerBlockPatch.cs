using Sandbox.Game.Entities.Blocks;
using SpaceEngineers.Game.Entities.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers.PatchManager;
using VRage.Game.Entity;

namespace CrunchUtilities
{
    [PatchShim]
    public static class TimerBlockPatch
    {
        internal static readonly MethodInfo update =
             typeof(MyTimerBlock).GetMethod("UpdateOnceBeforeFrame", BindingFlags.Instance | BindingFlags.Public) ?? throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo patchStart =
        typeof(TimerBlockPatch).GetMethod(nameof(StartMethod), BindingFlags.Static | BindingFlags.Public) ??
        throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo update2 =
           typeof(MySensorBlock).GetMethod("ShouldDetect", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo patchStart2 =
        typeof(TimerBlockPatch).GetMethod(nameof(DetectMethod), BindingFlags.Static | BindingFlags.Public) ??
        throw new Exception("Failed to find patch method");
        public static void Patch(PatchContext ctx)
        {

            ctx.GetPattern(update).Prefixes.Add(patchStart);
        }
        public static Boolean DetectMethod(MySensorBlock __instance, MyEntity entity)
        {
            if (CrunchUtilitiesPlugin.file != null && CrunchUtilitiesPlugin.file.SensorNobodyPatch)
            {
                if (__instance.OwnerId > 0)
                {
                    return true;
                }
                else
                {
                    __instance.Enabled = false;
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        public static Boolean StartMethod(MyTimerBlock __instance)
        {
            if (CrunchUtilitiesPlugin.file != null && CrunchUtilitiesPlugin.file.TimerNobodyPatch)
            {
                if (__instance.OwnerId > 0)
                {
                    return true;
                }
                else
                {
                    __instance.Enabled = false;
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
