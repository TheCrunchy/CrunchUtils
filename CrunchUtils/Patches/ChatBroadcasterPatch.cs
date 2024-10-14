using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Sandbox.Game.Gui;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.EntityComponents.Blocks;
using Torch.Managers.PatchManager;
using VRage.GameServices;
using VRage.Network;

namespace CrunchUtilities.Patches
{
    [PatchShim]
    public static class ChatBroadcasterPatch
    {
        public static void Patch(PatchContext ctx)
        {
              ctx.GetPattern(update).Suffixes.Add(updatePatch);
              ctx.GetPattern(transmitMessageToCharacterUpdate).Prefixes.Add(transmitMessageToCharacterUpdatePatch);

        }

        internal static readonly MethodInfo update =
         typeof(MyChatBroadcastEntityComponent).GetMethod("IsValidTarget", BindingFlags.Instance | BindingFlags.NonPublic) ??
         throw new Exception("Failed to find patch method IsValidTarget");
        internal static readonly MethodInfo updatePatch =
                typeof(ChatBroadcasterPatch).GetMethod(nameof(IsValidTarget), BindingFlags.Static | BindingFlags.NonPublic) ??
                throw new Exception("Failed to find patch method");

        private static void IsValidTarget(MyChatBroadcastEntityComponent __instance, long targetEntityId, ref bool __result)
        {
            if (CrunchUtilitiesPlugin.file.DisableChatEveryoneBlock && __instance.BroadcastTarget == BroadcastTarget.Everyone)
            {
                __result = false;
            }
        }


        internal static readonly MethodInfo transmitMessageToCharacterUpdate =
            typeof(MyChatBroadcastEntityComponent).GetMethod("TransmitMessageOverAntenna", BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new Exception("Failed to find patch method TransmitMessageOverAntenna");

        internal static readonly MethodInfo transmitMessageToCharacterUpdatePatch =
            typeof(ChatBroadcasterPatch).GetMethod(nameof(TransmitMessageToCharacter), BindingFlags.Static | BindingFlags.NonPublic) ??
            throw new Exception("Failed to find patch method");

        private static bool TransmitMessageToCharacter(MyChatBroadcastEntityComponent __instance, string message)
        {
            if (CrunchUtilitiesPlugin.file.SendFactionBroadcastsToFactionChat && __instance.BroadcastTarget == BroadcastTarget.Faction)
            {
                var block = __instance.Entity as IMyFunctionalBlock;
                var ownerFaction = FacUtils.GetPlayersFaction(block.OwnerId);
                if (ownerFaction == null)
                {
                    return false;
                }
                CrunchUtilitiesPlugin.Log.Info($"{MyEventContext.Current.Sender.Value}");
                MyMultiplayer.Static.SendChatMessage(message, ChatChannel.Faction, ownerFaction.FactionId, new ChatMessageCustomData(){AuthorName = __instance.CustomName, SenderId = new ulong?((ulong)block.EntityId) } );
                // Prevent message transmission
                return false;
            }

            return true;
            // Call the original method if the condition is not met
            // Assuming you have a way to call the original method, like a delegate or other means
        }
    }
}
