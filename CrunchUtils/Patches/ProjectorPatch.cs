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

        //internal static readonly MethodInfo update =
        //    typeof(MyProjectorBase).GetMethod("InitializeClipboard", BindingFlags.Instance | BindingFlags.NonPublic) ??
        //    throw new Exception("Failed to find patch method");

        //internal static readonly MethodInfo updatePatch =
        //    typeof(ProjectorPatch).GetMethod(nameof(TestPatchMethod), BindingFlags.Static | BindingFlags.Public) ??
        //    throw new Exception("Failed to find patch method");


        //internal static readonly MethodInfo remove =
        // typeof(MyProjectorBase).GetMethod("RemoveProjection", BindingFlags.Instance | BindingFlags.NonPublic) ??
        // throw new Exception("Failed to find patch method");

        //internal static readonly MethodInfo removePatch =
        //    typeof(ProjectorPatch).GetMethod(nameof(removeM), BindingFlags.Static | BindingFlags.Public) ??
        //    throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo addChild =
            typeof(MyHierarchyComponentBase).GetMethod("AddChild", BindingFlags.Instance | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo addChildPatch =
            typeof(ProjectorPatch).GetMethod(nameof(ChildPatch), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {
          
          //  ctx.GetPattern(update).Suffixes.Add(updatePatch);
            ctx.GetPattern(addChild).Prefixes.Add(addChildPatch);
            ctx.GetPattern(build).Prefixes.Add(buildPatch);
        //    ctx.GetPattern(remove).Prefixes.Add(removePatch);

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
        //public static void TestPatchMethod(MyProjectorBase __instance)
        //{
        //    if (CrunchUtilitiesPlugin.file == null)
        //    {

        //        return;
        //    }
        //    if (!CrunchUtilitiesPlugin.file.projectorPatch)
        //    {
        //        return;
        //    }


        //    if (__instance != null)
        //    {
        //        List<MyObjectBuilder_CubeGrid> grids = __instance.Clipboard.CopiedGrids;
        //        int count = 0;
        //        if (grids == null)
        //        {
        //            return;
        //        }
        //        foreach (MyObjectBuilder_CubeGrid objectBuilderCubeGrid in grids)
        //        {
        //            count += objectBuilderCubeGrid.CubeBlocks.Count;
        //        }
        //        MyCubeGrid grid = __instance.CubeGrid;
        //    //    CrunchUtilitiesPlugin.Log.Info("Removing? " + grid.BlocksPCU);
             
        //            grid.BlocksPCU -= count;



        //      //  CrunchUtilitiesPlugin.Log.Info("Removing? " + grid.BlocksPCU);
        //    }


        //}

        //public static void removeM(MyProjectorBase __instance)
        //{
        //    if (CrunchUtilitiesPlugin.file == null)
        //    {

        //        return;
        //    }
        //    if (!CrunchUtilitiesPlugin.file.projectorPatch)
        //    {
        //        return;
        //    }
        //    if (__instance != null)
        //    {
        //        List<MyCubeGrid> grids2 = __instance?.Clipboard?.PreviewGrids;
        //        int count = 0;
        //        if (grids2 == null)
        //        {
        //            return;
        //        }
        //        foreach (MyCubeGrid griid in grids2)
        //        {

        //            count += griid.CubeBlocks.Count;
        //            //  Log.Info("count " + count);
        //        }
        //       //Log.Info("count " + count);
        //        MyCubeGrid grid = __instance.CubeGrid;
        //       // CrunchUtilitiesPlugin.Log.Info("Adding? " + grid.BlocksPCU);
        //        grid.BlocksPCU += count;


        //      //  CrunchUtilitiesPlugin.Log.Info("Adding? " + grid.BlocksPCU);

        //    }


        //}
    }

}
