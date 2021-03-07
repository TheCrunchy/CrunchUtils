using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers.PatchManager;
using Torch.Utils;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;

//namespace CrunchUtilities
//{

  //  [PatchShim]
  //  public static class CubeGridPatch
  //  {
//        private static Int64 priceWorth(MyCubeBlockDefinition.Component component)
//        {
//            MyBlueprintDefinitionBase bpDef = MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(component.Definition.Id);
//            int p = 0;
//            float price = 0;
//            //calculate by the minimal price per unit for modded components, vanilla is aids
//            if (component.Definition.MinimalPricePerUnit > 1)
//            {
//                Int64 amn = Math.Abs(component.Definition.MinimalPricePerUnit);
//                price = price + amn;
//            }
//            //if keen didnt give the fucker a minimal price calculate by the ores that make up the ingots, because fuck having an integer for an economy right?
//            //else
//            //{
//            //    for (p = 0; p < bpDef.Prerequisites.Length; p++)
//            //    {
//            //        if (bpDef.Prerequisites[p].Id != null)
//            //        {
//            //            MyDefinitionBase oreDef = MyDefinitionManager.Static.GetDefinition(bpDef.Prerequisites[p].Id);
//            //            if (oreDef != null)
//            //            {
//            //                MyPhysicalItemDefinition ore = oreDef as MyPhysicalItemDefinition;
//            //                float amn = Math.Abs(ore.MinimalPricePerUnit);
//            //                float count = (float)bpDef.Prerequisites[p].Amount;
//            //                amn = (float)Math.Round(amn * count * 3);
//            //                price = price + amn;
//            //            }
//            //        }
//            //    }
//            //}
//            return Convert.ToInt64(price);
//        }

//        internal static readonly MethodInfo DestroyRequest =
//typeof(MyCubeBlock).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.Public) ??
//throw new Exception("Failed to find patch method");
//        internal static readonly MethodInfo patchDestroyRequestCube =
//        typeof(CubeGridPatch).GetMethod(nameof(OnDestroyRequest), BindingFlags.Static | BindingFlags.Public) ??
//        throw new Exception("Failed to find patch method");
//        public static void Patch(PatchContext ctx)
//        {

//            ctx.GetPattern(DestroyRequest).Prefixes.Add(patchDestroyRequestCube);
//        }
//        public static void OnDestroyRequest(MyCubeBlock __instance)
//        {
//            // long cause = CrunchUtilitiesPlugin.Damager.Get(__instance.EntityId);
//            MyCubeBlock block = __instance;
//            long creditCosts = 0;
            
//            var components = block.Components;


//            MyComponentList addToList = new MyComponentList();


//                MyComponentStack.GetMountedComponents(addToList, block.GetObjectBuilderCubeBlock());

        
//           var definition = block.BlockDefinition as MyCubeBlockDefinition;
          
//            //if (definition != null)
//            //{
//            //    int n = 0;
//            //    for (n = 0; n < definition.Components.Length; n++)
//            //    {
//            //        if (definition.Components[n] != null)
//            //        {


//            //            Int64 amn = priceWorth(definition.Components[n]);
//            //            Int64 count2 = definition.Components[n].Count;
//            //            creditCosts = creditCosts + (amn * count2);

//            //        }
//            //    }
//            //}
//            if (CrunchUtilitiesPlugin.attackers.TryGetValue(block.EntityId, out long Id))
//            {
//                CrunchUtilitiesPlugin.Log.Info("It existed?");
//                MyIdentity id = MySession.Static.Players.TryGetIdentity(Id);

//                Commands.SendMessage("THE BANK", "You got paid " + String.Format("{0:n0}", creditCosts) + " for destroying that block.", Color.Cyan, (long)MySession.Static.Players.TryGetSteamId(id.IdentityId));
//            }
//            else
//            {
//                CrunchUtilitiesPlugin.Log.Info("It didnt");
//            }


//        }
  //  }
//}
