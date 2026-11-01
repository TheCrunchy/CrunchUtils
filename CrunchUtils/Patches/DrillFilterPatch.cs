using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Sandbox.Engine.Physics;
using Sandbox.Game.Weapons;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRage.ObjectBuilders;
using VRageMath;

namespace CrunchUtilities.Patches
{

    public static class DrillFilterPatch
    {
        internal static readonly MethodInfo update =
            typeof(MyDrillBase).GetMethod("TryHarvestOreMaterial", BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo updatePatch =
            typeof(DrillFilterPatch).GetMethod(nameof(TestPatchMethod), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(update).Prefixes.Add(updatePatch);
        }

        public static bool TestPatchMethod(MyDrillBase __instance, MyVoxelMaterialDefinition material)
        {
            if (string.IsNullOrEmpty(material.MinedOre))
                return false;

            List<IMyEntity> l = new List<IMyEntity>();
            if (!(__instance.OutputInventory?.Owner?.GetBaseEntity() is MyShipDrill drill))
            {
                return true;
            }


            if ((drill.DisplayNameText != null && containsName(drill.DisplayNameText)) ||
                CrunchUtilitiesPlugin.filteredDrillIdentityIds.Contains(drill.OwnerId))
            {
                return material.MinedOre != "Stone";
            }

            return true;

        }

        public static bool containsName(String name)
        {
            if (name.ToLower().Contains("no stone") || name.ToLower().Contains("!stone"))
            {
                return true;
            }

            return false;
        }

    }
}
