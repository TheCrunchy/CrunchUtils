using Sandbox.Definitions;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers.PatchManager;
using VRage;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.ObjectBuilders.Private;

namespace CrunchUtilities.Patches
{
    [PatchShim]
    public static class BuilderPlannerPatch
    {
        internal static readonly MethodInfo toPatch =
    typeof(MyCharacter).GetMethod("MakeBlueprintFromBuildPlanItem", BindingFlags.Static | BindingFlags.NonPublic) ??
    throw new Exception("Failed to find patch method From Keen");

        internal static readonly MethodInfo toPatch2 =
            typeof(MyCompositeBlueprintDefinition).GetMethod("AddToItemList", BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new Exception("Failed to find patch method From Keen");

        internal static readonly MethodInfo patched =
            typeof(BuilderPlannerPatch).GetMethod(nameof(MakeBlueprintFromBuildPlanItem), BindingFlags.Static | BindingFlags.NonPublic) ??
            throw new Exception("Failed to find patched");

        internal static readonly MethodInfo patched2 =
            typeof(BuilderPlannerPatch).GetMethod(nameof(AddToItemList), BindingFlags.Static | BindingFlags.NonPublic) ??
            throw new Exception("Failed to find patched");

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(toPatch).Prefixes.Add(patched);
            ctx.GetPattern(toPatch2).Prefixes.Add(patched2);
        }

        private static bool AddToItemList(
            List<MyBlueprintDefinitionBase.Item> items,
            MyBlueprintDefinitionBase.Item toAdd)
        {
            try
            {
                MyBlueprintDefinitionBase.Item obj = new MyBlueprintDefinitionBase.Item();
                int index;
                for (index = 0; index < items.Count; ++index)
                {
                    var valid = items.IsValidIndex(index);
                    if (!valid)
                    {
                        return false;
                    }
                    obj = items[index];
                    if (obj.Id == toAdd.Id)
                        break;
                }
                if (index >= items.Count)
                {
               
                }
                else
                {
                    var valid = items.IsValidIndex(index);
                   if (!valid)
                   {
                       return false;
                   }
                }
            }
            catch (Exception e)
            {
                CrunchUtilitiesPlugin.Log.Error($"AddToItemList error {e}");
                return false;
            }

            return true;
        }

        private static bool MakeBlueprintFromBuildPlanItem(MyCharacter __instance, MyIdentity.BuildPlanItem buildPlanItem)
        {
            try
            {
                MyObjectBuilder_CompositeBlueprintDefinition newObject = MyObjectBuilderSerializerKeen.CreateNewObject<MyObjectBuilder_CompositeBlueprintDefinition>();
                if (buildPlanItem.BlockDefinition == null)
                    return true;
                newObject.Id = new SerializableDefinitionId((MyObjectBuilderType)typeof(MyObjectBuilder_BlueprintDefinition), buildPlanItem.BlockDefinition.Id.ToString().Replace("MyObjectBuilder_", "BuildPlanItem_"));
                Dictionary<MyDefinitionId, MyFixedPoint> dictionary = new Dictionary<MyDefinitionId, MyFixedPoint>();
                foreach (MyIdentity.BuildPlanItem.Component component in buildPlanItem.Components)
                {
                    MyDefinitionId id = component.ComponentDefinition.Id;
                    if (!dictionary.ContainsKey(id))
                        dictionary[id] = (MyFixedPoint)0;
                    dictionary[id] += (MyFixedPoint)component.Count;
                }
                newObject.Blueprints = new BlueprintItem[dictionary.Count];
                int index = 0;
                foreach (KeyValuePair<MyDefinitionId, MyFixedPoint> keyValuePair in dictionary)
                {
                    MyBlueprintDefinitionBase definitionByResultId;
                    if ((definitionByResultId = MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(keyValuePair.Key)) == null)
                        return true;
                    newObject.Blueprints[index] = new BlueprintItem()
                    {
                        Id = new SerializableDefinitionId(definitionByResultId.Id.TypeId, definitionByResultId.Id.SubtypeName),
                        Amount = keyValuePair.Value.ToString()
                    };
                    ++index;
                }
                newObject.Icons = buildPlanItem.BlockDefinition.Icons;
                newObject.DisplayName = buildPlanItem.BlockDefinition.DisplayNameEnum.HasValue ? buildPlanItem.BlockDefinition.DisplayNameEnum.Value.ToString() : buildPlanItem.BlockDefinition.DisplayNameText;
                newObject.Public = buildPlanItem.BlockDefinition.Public;
                MyCompositeBlueprintDefinition blueprintDefinition = new MyCompositeBlueprintDefinition();
                blueprintDefinition.Init((MyObjectBuilder_DefinitionBase)newObject, MyModContext.BaseGame);
                blueprintDefinition.Postprocess();
                return true;
            }
            catch (Exception e)
            {
                for (int i = __instance.BuildPlanner.Count - 1; i >= 0; i--)
                {
                    __instance.RemoveAtBuildPlanner(i);
                }
   
                CrunchUtilitiesPlugin.Log.Error($"MakeBlueprintFromBuildPlanItem error {e}");
                return false;
            }
        }
    }
}
