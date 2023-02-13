using Sandbox.Definitions;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game;
using Torch.Managers.PatchManager;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.ObjectBuilders;
using VRageMath;

namespace CrunchUtilities
{
    [PatchShim]
    public static class ScrapFloatingPatch
    {



        //  internal static readonly MethodInfo update =
        //      typeof(MyFloatingObjects).GetMethod("Spawn", BindingFlags.Static | BindingFlags.Public,null, new Type[] { typeof(MyPhysicalItemDefinition), typeof(Vector3D), typeof(Vector3D), typeof(Vector3D), typeof(int), typeof(float) }, null) ??
        //      throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo update =
            typeof(MyFloatingObjects).GetMethod("AddToPos", BindingFlags.Static | BindingFlags.NonPublic) ??
            throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo spawnScrap =
            typeof(MyInventory).GetMethod("AddItems", BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo updatePatch =
                typeof(ScrapFloatingPatch).GetMethod(nameof(TestPatchMethod), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");


        internal static readonly MethodInfo updatePatchSpawn =
            typeof(ScrapFloatingPatch).GetMethod(nameof(TestPatchMethod2), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        //MyInventoryBase
        //    public abstract bool AddItems(MyFixedPoint amount, MyObjectBuilder_Base objectBuilder);

        public static void Patch(PatchContext ctx)
        {

            ctx.GetPattern(update).Prefixes.Add(updatePatch);
            ctx.GetPattern(spawnScrap).Prefixes.Add(updatePatchSpawn);

        }

        public static bool TestPatchMethod(MyEntity thrownEntity,
  Vector3D pos,
  MyPhysicsComponentBase motionInheritedFrom)
        {
            if (CrunchUtilitiesPlugin.file == null || !CrunchUtilitiesPlugin.file.ScrapMetalPatch) return true;
            if (thrownEntity != null && thrownEntity is MyFloatingObject obj)
            {
                return !obj.ItemDefinition.DisplayNameText.Equals("Scrap Metal");
            }

            return true;
        }

        public static bool TestPatchMethod2(ref MyFixedPoint amount, MyObjectBuilder_Base objectBuilder)
        {
            if (CrunchUtilitiesPlugin.file == null || !CrunchUtilitiesPlugin.file.ScrapMetalPatchNeverSpawn) return true;
            if (objectBuilder != null && objectBuilder.SubtypeId != null && objectBuilder.SubtypeId.ToString() == "Scrap")
            {
                return false;
            }

            return true;
        }
    }
}
