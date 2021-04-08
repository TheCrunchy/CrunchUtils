//using Sandbox.Game.Entities;
//using Sandbox.Game.Entities.Cube;
//using Sandbox.ModAPI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using Torch.Managers.PatchManager;
//using VRage.Game.ModAPI;
//using VRage.Utils;

//namespace CrunchUtilities
//{
//    [PatchShim]
//    public static class SlimBlockPatch
//    {
//        internal static readonly MethodInfo DamageRequest =
//typeof(MySlimBlock).GetMethod("DoDamage", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(float), typeof(MyStringHash), typeof(bool), typeof(MyHitInfo?), typeof(long) }, null) ??
//throw new Exception("Failed to find patch method");
//        internal static readonly MethodInfo patchSlimDamage =
//        typeof(SlimBlockPatch).GetMethod(nameof(OnDamageRequest), BindingFlags.Static | BindingFlags.Public) ??
//        throw new Exception("Failed to find patch method");
//        public static void Patch(PatchContext ctx)
//        {

//            ctx.GetPattern(DamageRequest).Prefixes.Add(patchSlimDamage);
//        }
//        public static Boolean OnDamageRequest(MySlimBlock __instance, float damage,
//      MyStringHash damageType,
//      bool sync,
//      MyHitInfo? hitInfo,
//      long attackerId)
//        {
//            if (MyAPIGateway.Entities.GetEntityById(attackerId) is MyCubeGrid grid)
//            {

//                if (grid.BlocksCount >= 100 && __instance.CubeGrid.BlocksCount >= 100 && damageType.ToString().Equals("Deformation"))
//                {
                   
//                    return false;
//                }
//                else
//                {
//                    return true;
//                }
       
//            }
//            return true;
//        }
//    }
//}
