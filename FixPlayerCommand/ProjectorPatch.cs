using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace CrunchUtilities
{
    [PatchShim]
    public static class ProjectorPatch
    {

      

        internal static readonly MethodInfo update =
            typeof(MyProjectorBase).GetMethod("InitializeClipboard", BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo updatePatch =
            typeof(ProjectorPatch).GetMethod(nameof(TestPatchMethod), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");


        internal static readonly MethodInfo remove =
         typeof(MyProjectorBase).GetMethod("RemoveProjection", BindingFlags.Instance | BindingFlags.NonPublic) ??
         throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo removePatch =
            typeof(ProjectorPatch).GetMethod(nameof(removeM), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");
        public static void Patch(PatchContext ctx)
        {

            ctx.GetPattern(update).Suffixes.Add(updatePatch);
            ctx.GetPattern(remove).Prefixes.Add(removePatch);
            CrunchUtilitiesPlugin.Log.Info("Patching Successful Crunch Projector!");
        }

        public static void TestPatchMethod(MyProjectorBase __instance)
        {
            if (CrunchUtilitiesPlugin.file == null)
            {

                return;
            }
            if (!CrunchUtilitiesPlugin.file.projectorPatch)
            {
                return;
            }


            if (__instance != null)
            {
                List<MyObjectBuilder_CubeGrid> grids = __instance.Clipboard.CopiedGrids;
                int count = 0;
                if (grids == null)
                {
                    return;
                }
                foreach (MyObjectBuilder_CubeGrid objectBuilderCubeGrid in grids)
                {
                    count += objectBuilderCubeGrid.CubeBlocks.Count;
                }
                MyCubeGrid grid = __instance.CubeGrid;
                //Log.Info("Removing? " + grid.BlocksPCU);
                grid.BlocksPCU -= count;
                // Log.Info("Removing? " + grid.BlocksPCU);
            }


        }

        public static void removeM(MyProjectorBase __instance)
        {
            if (CrunchUtilitiesPlugin.file == null)
            {

                return;
            }
            if (!CrunchUtilitiesPlugin.file.projectorPatch)
            {
                return;
            }
            if (__instance != null)
            {
                List<MyCubeGrid> grids2 = __instance?.Clipboard?.PreviewGrids;
                int count = 0;
                if (grids2 == null)
                {
                    return;
                }
                foreach (MyCubeGrid griid in grids2)
                {

                    count += griid.CubeBlocks.Count;
                    //  Log.Info("count " + count);
                }
                // Log.Info("count " + count);
                MyCubeGrid grid = __instance.CubeGrid;
                //  Log.Info("Adding? " + grid.BlocksPCU);
                grid.BlocksPCU += count;
                //   Log.Info("Adding? " + grid.BlocksPCU);

            }


        }
    }

}
