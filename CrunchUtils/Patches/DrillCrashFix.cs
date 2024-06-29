using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.Weapons;
using Sandbox.Game.Weapons.Guns;
using Sandbox.Game.WorldEnvironment;
using Sandbox.Game.WorldEnvironment.Modules;
using Torch.Managers.PatchManager;
using VRage.Utils;

namespace CrunchUtilities.Patches
{
    [PatchShim]
    public static class DrillCrashFix
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(methodToPatch).Prefixes.Add(patchMethod);
        }

        internal static readonly MethodInfo methodToPatch =
            typeof(MyDrillBase).GetMethod("DrillEnvironmentSector",
                BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new Exception("Failed to find patch method contract");

        internal static readonly MethodInfo patchMethod =
            typeof(DrillCrashFix).GetMethod(nameof(DrillEnvironment), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        public static bool DrillEnvironment(MyDrillSensorBase.DetectionInfo entry,
            float speedMultiplier,
            out MyStringHash targetMaterial)
        {

            targetMaterial = MyStringHash.GetOrCompute("Wood");

            if (entry.Entity == null)
            {
                return false;
            }

            var environmentSector = entry.Entity as MyEnvironmentSector;
            if (environmentSector == null)
            {
                return false;
            }


            var module = environmentSector.GetModule<MyBreakableEnvironmentProxy>();
            if (module == null)
            {
                CrunchUtilitiesPlugin.Log.Info("DrillEnvironmentSector crash prevented.");
                return false;
            }

            return true;
        }
    }
}
