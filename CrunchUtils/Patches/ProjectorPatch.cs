using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Network;
using VRageMath;

namespace CrunchUtilities
{
    [PatchShim]
    public static class ProjectorPatch
    {
        internal static readonly MethodInfo build =
            typeof(MyProjectorBase).GetMethod("Build", BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo buildPatch =
                 typeof(ProjectorPatch).GetMethod(nameof(BuildPatch), BindingFlags.Static | BindingFlags.Public) ??
                 throw new Exception("Failed to find patch method");


        internal static readonly MethodInfo addChild =
            typeof(MyHierarchyComponentBase).GetMethod("AddChild", BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo addChildPatch =
            typeof(ProjectorPatch).GetMethod(nameof(ChildPatch), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(build).Prefixes.Add(buildPatch);

            CrunchUtilitiesPlugin.Log.Info("Patching Successful Crunch Projector!");
        }

        public static bool ChildPatch(IMyEntity child, bool preserveWorldPos = false, bool insertIntoSceneIfNeeded = true)
        {

            CrunchUtilitiesPlugin.Log.Info("Add Child " + child.GetType());
            return true;
        }


        public static void BuildPatch(MyProjectorBase __instance, MySlimBlock cubeBlock,
      long owner,
      long builder,
      bool requestInstant = true,
      long builtBy = 0)
        {
            
            if (CrunchUtilitiesPlugin.file == null)
            {

                return;
            }
            if (!CrunchUtilitiesPlugin.file.projectorOwnershipPatch)
            {
                return;
            }
            
            if (__instance != null && owner == 0)
            {


                if (cubeBlock.FatBlock != null)
                {
             
                    MyCubeBlock block = cubeBlock.FatBlock;
             

                    block.CubeGrid.ChangeOwner(block, 0, __instance.OwnerId);

                }

           
            }
        }
    }
}
