using Sandbox.Definitions;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers.PatchManager;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
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
        internal static readonly MethodInfo updatePatch =
                typeof(ScrapFloatingPatch).GetMethod(nameof(TestPatchMethod), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");



        public static void Patch(PatchContext ctx)
        {

            ctx.GetPattern(update).Prefixes.Add(updatePatch);

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
    }
}
