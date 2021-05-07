using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Game.Entity;

namespace CrunchUtilities
{
    [PatchShim]
    public static class NobodyPatchKEEEEEN
    {
        //internal static readonly MethodInfo update =
        //     typeof(MyTimerBlock).GetMethod("UpdateOnceBeforeFrame", BindingFlags.Instance | BindingFlags.Public) ?? throw new Exception("Failed to find patch method");
        //internal static readonly MethodInfo patchStart =
        //typeof(NobodyPatchKEEEEEN).GetMethod(nameof(StartMethod), BindingFlags.Static | BindingFlags.Public) ??
        //throw new Exception("Failed to find patch method");

        //internal static readonly MethodInfo detectUpdate =
        //   typeof(MySensorBlock).GetMethod("ShouldDetect", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");
        //internal static readonly MethodInfo patchDetection =
        //typeof(NobodyPatchKEEEEEN).GetMethod(nameof(DetectMethod), BindingFlags.Static | BindingFlags.Public) ??
        //throw new Exception("Failed to find patch method");


        internal static readonly MethodInfo enabledUpdate =
             typeof(MyFunctionalBlock).GetMethod("OnEnabledChanged", BindingFlags.Instance | BindingFlags.NonPublic) ??
             throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo functionalBlockPatch =
            typeof(NobodyPatchKEEEEEN).GetMethod(nameof(PatchTurningOn), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

     //   internal static readonly MethodInfo ownershipChange =
     //typeof(MyCubeGrid).GetMethod("ChangeOwnerRequest", BindingFlags.Instance | BindingFlags.Public) ??
     //throw new Exception("Failed to find patch method");

     //   internal static readonly MethodInfo ownershipPatch =
     //       typeof(NobodyPatchKEEEEEN).GetMethod(nameof(PatchOwnership), BindingFlags.Static | BindingFlags.Public) ??
     //       throw new Exception("Failed to find patch method");


        //public static void PatchOwnership(MyCubeGrid grid, MyCubeBlock block, long playerId, MyOwnershipShareModeEnum shareMode)
        //{
        //    if (block != null && block is MyFunctionalBlock fblock && playerId == 0)
        //    {
        //      //  fblock.Enabled = false;
        //        IMyFunctionalBlock ffblock = fblock;
        //        ffblock.Enabled = false;
        //    }
        //}
        public static bool PatchTurningOn(MyFunctionalBlock __instance)
        {
            if (CrunchUtilitiesPlugin.file != null && CrunchUtilitiesPlugin.file.NobodyPatch)
            {
                // Log.Info("Button");
                MyFunctionalBlock block = __instance;
                if (block.BlockDefinition?.ContainsComputer() == false)
                {
                    return true;
                }
                if (block != null && block.OwnerId == 0)
                    {

                        block.Enabled = false;
                        // Log.Info("Beacon button");
                        return false;
                    }
                
            }
            return true;
        }


        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(enabledUpdate).Prefixes.Add(functionalBlockPatch);
         //   ctx.GetPattern(ownershipChange).Suffixes.Add(ownershipPatch);
        }
    }
}
