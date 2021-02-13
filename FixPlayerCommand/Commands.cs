﻿using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using Torch.Session;
using Sandbox.Common;
using Sandbox.Game;
using Sandbox.Game.World;
using Sandbox.Game.Entities;
using VRage.Game.Entity;
using VRage.Groups;
using System.Collections.Concurrent;
using VRage.Game;
using Sandbox.ModAPI;
using Sandbox.Game.GameSystems;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Torch.Managers;
using Torch.API.Plugins;
using Torch.API.Managers;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Definitions;
using System.Collections;
using Torch.Managers.ChatManager;
using Torch.API.Session;
using Sandbox.Game.Multiplayer;
using System.Reflection;
using VRage;
using Torch.Mod.Messages;
using static Sandbox.Game.Multiplayer.MyFactionCollection;
using Torch.Mod;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using Sandbox.Game.Entities.Blocks;
using System.IO;

namespace CrunchUtilities
{
    public class Commands : CommandModule
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        private static Dictionary<long, int> warnedGrids = new Dictionary<long, int>();
        private static Dictionary<long, long> confirmations = new Dictionary<long, long>();

        private Vector3 defaultColour = new Vector3(50, 168, 168);
        [Command("crunch reload", "Reload the config")]
        [Permission(MyPromoteLevel.Admin)]
        public void ReloadConfig()
        {
            CrunchUtilitiesPlugin.LoadConfig();
            Context.Respond("Reloaded config");
        }
        [Command("enablestone", "Reload the config")]
        [Permission(MyPromoteLevel.Admin)]
        public void enableconfig()
        {

            CrunchUtilitiesPlugin.LoadConfig();
            CrunchUtilitiesPlugin.file.DeleteStoneAuto = true;
            CrunchUtilitiesPlugin.SaveConfig();
            Context.Respond("Reloaded config");
        }
        [Command("crunch config", "Reload the config")]
        [Permission(MyPromoteLevel.Admin)]
        public void ReloadConfig(string option, string value)
        {
            switch (option)
            {
                case "playermakeship":
                    CrunchUtilitiesPlugin.file.PlayerMakeShip = Boolean.TryParse(value, out bool result);
                    break;
                case "playerfixme":
                    CrunchUtilitiesPlugin.file.PlayerFixMe = Boolean.TryParse(value, out bool result2);
                    break;
                case "deletestone":
                    CrunchUtilitiesPlugin.file.DeleteStone = Boolean.TryParse(value, out bool result3);
                    break;
                case "withdraw":
                    CrunchUtilitiesPlugin.file.Withdraw = Boolean.TryParse(value, out bool result4);
                    break;
                case "deposit":
                    CrunchUtilitiesPlugin.file.Deposit = Boolean.TryParse(value, out bool result5);
                    break;
                case "factionsharedeposit":
                    CrunchUtilitiesPlugin.file.FactionShareDeposit = Boolean.TryParse(value, out bool result6);
                    break;
                case "identityupdate":
                    CrunchUtilitiesPlugin.file.IdentityUpdate = Boolean.TryParse(value, out bool result7);
                    break;
                case "cooldowninseconds":
                    CrunchUtilitiesPlugin.file.CooldownInSeconds = int.Parse(value);
                    break;
            }

        }
        private static Dictionary<long, long> cooldowns = new Dictionary<long, long>();
        [Command("admin makeship", "Admin command, Turn a station and connected grids into a ship")]
        [Permission(MyPromoteLevel.Admin)]
        public void MakeShip(String name = "")
        {
            if (name.Equals(""))
            {
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;

                        if (grid.IsStatic)
                        {
                            Action m_convertToShipResult = null;
                            grid.RequestConversionToShip(m_convertToShipResult);
                            Context.Respond("Converting to ship " + grid.DisplayName);
                        }
                    }
                }
            }
            else
            {

                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindGridGroup(name);
                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;

                        if (grid.IsStatic)
                        {
                            Action m_convertToShipResult = null;
                            grid.RequestConversionToShip(m_convertToShipResult);
                            Context.Respond("Converting to ship " + grid.DisplayName);
                        }
                    }
                }
            }
        }

        public static IMyPlayer GetPlayerByNameOrId(string nameOrPlayerId)
        {
            if (!long.TryParse(nameOrPlayerId, out long id))
            {
                foreach (var identity in MySession.Static.Players.GetAllIdentities())
                {
                    if (identity.DisplayName == nameOrPlayerId)
                    {
                        id = identity.IdentityId;
                    }
                }
            }

            if (MySession.Static.Players.TryGetPlayerId(id, out MyPlayer.PlayerId playerId))
            {
                if (MySession.Static.Players.TryGetPlayerById(playerId, out MyPlayer player))
                {
                    return player;
                }
            }

            return null;
        }

        [Command("removebody", "Removes every body with this display name")]
        [Permission(MyPromoteLevel.Admin)]
        public void DeleteBody(string playerName)
        {
            //essentials code
            //https://github.com/TorchAPI/Essentials/blob/415a7e12809c75fc1efcfbc878cfee1730efa6ff/Essentials/Commands/EntityModule.cs#L297
            //      //essentials code
            //https://github.com/TorchAPI/Essentials/blob/415a7e12809c75fc1efcfbc878cfee1730efa6ff/Essentials/Commands/EntityModule.cs#L297
            //      //essentials code
            //https://github.com/TorchAPI/Essentials/blob/415a7e12809c75fc1efcfbc878cfee1730efa6ff/Essentials/Commands/EntityModule.cs#L297
            //      //essentials code
            //https://github.com/TorchAPI/Essentials/blob/415a7e12809c75fc1efcfbc878cfee1730efa6ff/Essentials/Commands/EntityModule.cs#L297
            //
            Boolean found = false;
            var player = GetPlayerByNameOrId(playerName);
            if (player != null)
            {
                /* If he is online we check if he is currently seated. If he is eject him. */
                if (player?.Controller.ControlledEntity is MyCockpit controller && !found)
                {
                    controller.Use();
                    Context.Respond($"Player '{playerName}' ejected and murdered.");
                    player.Character.Kill();
                    player.Character.Delete();
                    MyMultiplayer.Static.DisconnectClient(player.SteamUserId);
                    found = true;
                }
                else
                {
                    Context.Respond("Player murdered.");
                    player.Character.Kill();
                    player.Character.Delete();
                    MyMultiplayer.Static.DisconnectClient(player.SteamUserId);
                }

                return;
            }


            foreach (var grid in MyEntities.GetEntities().OfType<MyCubeGrid>().ToList())
            {
                foreach (var controller in grid.GetFatBlocks<MyShipController>())
                {
                    var pilot = controller.Pilot;

                    if (pilot != null && pilot.DisplayName == playerName)
                    {
                        controller.Use();
                        IMyCharacter character = pilot as IMyCharacter;
                        character.Kill();
                        character.Delete();
                        Context.Respond("Murdering " + pilot.EntityId);
                        return;
                    }
                }
            }

            Context.Respond("If it got here, the playername you entered could not be found");

        }
        [Command("admin makestation", "Admin command, Turn a station and connected grids into a ship")]
        [Permission(MyPromoteLevel.Admin)]
        public void MakeStation(String name = "")
        {
            if (name.Equals(""))
            {
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;

                        if (!grid.IsStatic)
                            grid.OnConvertedToStationRequest();
                        Context.Respond("Converting to station " + grid.DisplayName);
                    }
                }
            }
            else
            {
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindGridGroup(name);
                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;

                        if (!grid.IsStatic)
                            grid.OnConvertedToStationRequest();
                        Context.Respond("Converting to station " + grid.DisplayName);
                    }
                }
            }
        }
        private static CurrentCooldown CreateNewCooldown(Dictionary<long, CurrentCooldown> cooldownMap, long playerId, long cooldown)
        {

            var currentCooldown = new CurrentCooldown(cooldown);

            if (cooldownMap.ContainsKey(playerId))
                cooldownMap[playerId] = currentCooldown;
            else
                cooldownMap.Add(playerId, currentCooldown);

            return currentCooldown;
        }
        [Command("togglestone", "Delete all stone in a grid")]
        [Permission(MyPromoteLevel.None)]
        public void ToggleDeleteStone()
        {
            if (Context.Player == null)
            {
                CrunchUtilitiesPlugin.Log.Info("How the fuck isnt this a player");
                return;

            }
            if (CrunchUtilitiesPlugin.file.DeleteStoneAuto)
            {
                CrunchUtilitiesPlugin.Log.Info(Context.Player.Identity.IdentityId);
                if (CrunchUtilitiesPlugin.ids.Contains(Context.Player.Identity.IdentityId))
                {
                    CrunchUtilitiesPlugin.ids.Remove(Context.Player.Identity.IdentityId);
                    Context.Respond("No longer deleting stone", Color.Cyan, "Tiny Drill Elves");
                }
                else
                {
                    CrunchUtilitiesPlugin.ids.Add(Context.Player.Identity.IdentityId);
                    Context.Respond("Now deleting stone, if you own the drill", Color.Cyan, "Tiny Drill Elves");
                }

            }
            else
            {
                Context.Respond("not enabled");
            }
        }
        public static int totalcount = 0;
        [Command("stone", "Delete all stone in a grid")]
        [Permission(MyPromoteLevel.None)]
        public void DeleteStone(bool outputcount = false)
        {

            if (CrunchUtilitiesPlugin.file.DeleteStone)
            {

                CrunchUtilitiesPlugin plugin = (CrunchUtilitiesPlugin)Context.Plugin;
                var currentCooldownMap = plugin.CurrentCooldownMap;
                if (currentCooldownMap.TryGetValue(Context.Player.IdentityId, out CurrentCooldown currentCooldown))
                {

                    long remainingSeconds = currentCooldown.GetRemainingSeconds(null);

                    if (remainingSeconds > 0)
                    {

                        CrunchUtilitiesPlugin.Log.Info("Cooldown for Player " + Context.Player.DisplayName + " still running! " + remainingSeconds + " seconds remaining!");
                        Context.Respond("Command is still on cooldown for " + remainingSeconds + " seconds.");
                        return;
                    }
                    currentCooldown = CreateNewCooldown(currentCooldownMap, Context.Player.IdentityId, plugin.Cooldown);
                    currentCooldown.StartCooldown(null);
                }
                else
                {
                    currentCooldown = CreateNewCooldown(currentCooldownMap, Context.Player.IdentityId, plugin.Cooldown);
                    currentCooldown.StartCooldown(null);
                }
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
                int count = 0;

                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        //     MyObjectBuilder_PhysicalObject stone = new MyObjectBuilder_PhysicalObject("MyObjectBuilder_Ore/Stone");
                        MyCubeGrid grid = groupNodes.NodeData;
                        var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);


                        var blockList = new List<Sandbox.ModAPI.IMyTerminalBlock>();
                        gts.GetBlocksOfType<Sandbox.ModAPI.IMyTerminalBlock>(blockList);
                        if (!FacUtils.IsOwnerOrFactionOwned(grid, Context.Player.IdentityId, true))
                        {
                            Context.Respond("You dont own this");
                            continue;
                        }
                        else
                        {
                            foreach (var block in blockList)
                            {

                                //MyVisualScriptLogicProvider.SendChatMessage("blocks blocklist");
                                if (block != null && block.HasInventory)
                                {

                                    var items = block.GetInventory().GetItems();
                                    for (int i = 0; i < items.Count; i++)
                                    {

                                        if (items[i].Content.SubtypeId.ToString().Contains("Stone") && items[i].Content.TypeId.ToString().Contains("Ore"))
                                        {
                                            count += items[i].Amount.ToIntSafe();
                                            block.GetInventory().RemoveItems(items[i].ItemId);
                                        }
                                    }
                                }
                            }
                        }


                    }

                }
                if (count == 0)
                {
                    currentCooldownMap.Remove(Context.Player.IdentityId);

                }
                Context.Respond(count + " Stone Deleted");
                totalcount += count;
                if (outputcount)
                {
                    Context.Respond(String.Format("{0:n0}", totalcount) + " Deleted in total on this instance.");
                }
            }
            else
            {
                Context.Respond("stone not enabled");
            }

        }
        //[Command("addgas", "add gas offers to look at grid")]
        //[Permission(MyPromoteLevel.None)]
        //public void gasOffer(int price)
        //{
        //    ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
        //    if (gridWithSubGrids.Count > 0)
        //    {
        //        foreach (var item in gridWithSubGrids)
        //        {

        //            foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
        //            {
        //                MyCubeGrid grid = groupNodes.NodeData;
        //                var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
        //                var blockList = new List<Sandbox.ModAPI.IMyStoreBlock>();
        //                gts.GetBlocksOfType<Sandbox.ModAPI.IMyStoreBlock>(blockList);
        //                foreach (Sandbox.ModAPI.IMyStoreBlock store in blockList)
        //                {

        //                    SerializableDefinitionId itemId = new SerializableDefinitionId();
        //                    itemId.SubtypeId = "Hydrogen";
        //                    itemId.TypeIdString = "MyObjectBuilder_GasProperties";

        //                    MyStoreItemData data = new MyStoreItemData(itemId, 1, 1000, (Action<int, int, long, long, long>)null, (Action)null);
        //                    store.InsertOffer(data, out long IdentityId);
        //                    //  store.InsertOffer
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Context.Respond("Cant find a grid");
        //    }

        //}
        [Command("claim", "Player command, claim a shared grid")]
        [Permission(MyPromoteLevel.None)]
        public void ClaimCommand()
        {

            if (CrunchUtilitiesPlugin.file.Claim)
            {
                double blockCount = 0;
                double factionShared = 0;
                double sharedWithAll = 0;
                IMyFaction fac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
                if (fac == null)
                {
                    Context.Respond("Not in a faction.");
                    return;
                }
                if (fac.IsLeader(Context.Player.IdentityId) || fac.IsFounder(Context.Player.IdentityId))
                {
                    //do nothing im lazy
                }
                else
                {
                    Context.Respond("Not a founder or leader");
                    return;
                }
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
                if (gridWithSubGrids.Count > 0)
                {

                    foreach (var item in gridWithSubGrids)
                    {
                        bool isStatic = false;
                        bool isDynamic = false;
                        foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                        {

                            MyCubeGrid grid = groupNodes.NodeData;

                            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                            var blockList = new List<Sandbox.ModAPI.IMyStoreBlock>();
                            gts.GetBlocksOfType<Sandbox.ModAPI.IMyStoreBlock>(blockList);
                            Boolean npc = false;
                            foreach (long id in grid.BigOwners)
                            {
                                if (FacUtils.GetFactionTag(id).Length > 3)
                                {
                                    npc = true;
                                }
                            }
                            //rewrite this shit eventually to not be trash
                            if (blockList.Count == 0 && !npc)
                            {


                                foreach (MySlimBlock block in grid.GetBlocks())
                                {


                                    if (block.FatBlock != null && block.FatBlock.OwnerId > 0)
                                    {
                                        blockCount += 1;

                                        switch (block.FatBlock.GetUserRelationToOwner(Context.Player.IdentityId))
                                        {
                                            case MyRelationsBetweenPlayerAndBlock.Owner:

                                                factionShared += 1;
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.FactionShare:
                                                factionShared += 1;
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                                                //sharedWithAll += 1;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                            double totalShared = factionShared + sharedWithAll;

                            double sharedPercent = (totalShared / blockCount) * 100;

                            Context.Respond(CrunchUtilitiesPlugin.file.ClaimPercent.ToString());
                            if (sharedPercent >= CrunchUtilitiesPlugin.file.ClaimPercent)
                            {
                                grid.ChangeGridOwner(Context.Player.IdentityId, MyOwnershipShareModeEnum.None);
                                Context.Respond("Giving ownership to you, Owned: " + sharedPercent.ToString());
                            }
                            else
                            {
                                Context.Respond("Not enough shared percentage, Owned: " + sharedPercent.ToString());
                            }
                        }



                    }
                }
                else
                {
                    Context.Respond("Cant find a grid");
                }


            }
            else
            {
                Context.Respond("Claim not enabled");
            }

        }
        [Command("fixrespawn", "remove the respawnship status")]
        [Permission(MyPromoteLevel.None)]
        public void norespawn()
        {

            CrunchUtilitiesPlugin plugin = (CrunchUtilitiesPlugin)Context.Plugin;
            var currentCooldownMap = plugin.CurrentCooldownMap2;
            if (currentCooldownMap.TryGetValue(Context.Player.IdentityId, out CurrentCooldown currentCooldown))
            {

                long remainingSeconds = currentCooldown.GetRemainingSeconds(null);

                if (remainingSeconds > 0)
                {

                    CrunchUtilitiesPlugin.Log.Info("Cooldown for Player " + Context.Player.DisplayName + " still running! " + remainingSeconds + " seconds remaining!");
                    Context.Respond("Command is still on cooldown for " + remainingSeconds + " seconds.");
                    return;
                }
                currentCooldown = CreateNewCooldown(currentCooldownMap, Context.Player.IdentityId, plugin.CooldownRespawn);
                currentCooldown.StartCooldown(null);
            }
            else
            {
                currentCooldown = CreateNewCooldown(currentCooldownMap, Context.Player.IdentityId, plugin.CooldownRespawn);
                currentCooldown.StartCooldown(null);
            }
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
            if (gridWithSubGrids.Count > 0)
            {

                foreach (var item in gridWithSubGrids) {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;
                        grid.IsRespawnGrid = false;
                        Context.Respond("Ship wont get deleted by Keen. Try not to die.");
                    }
                }


            }
        }
        [Command("prediction", "remove the prediction shit")]
        [Permission(MyPromoteLevel.Admin)]
        public void noprediction()
        {

            CrunchUtilitiesPlugin plugin = (CrunchUtilitiesPlugin)Context.Plugin;
           
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
            if (gridWithSubGrids.Count > 0)
            {

                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;
                        grid.ForceDisablePrediction = !grid.ForceDisablePrediction;
                        Context.Respond("Prediction set to " + grid.ForceDisablePrediction);
                    }
                }


            }
        }

        [Command("convert", "Player command, Turn a ship and connected grids into a station")]
        [Permission(MyPromoteLevel.None)]
        public void MakeStationPlayer()
        {
            if (CrunchUtilitiesPlugin.file.PlayerMakeShip)
            {

                if (MyGravityProviderSystem.IsPositionInNaturalGravity(Context.Player.GetPosition()) && !CrunchUtilitiesPlugin.file.convertInGravity)
                {

                    Context.Respond("You cannot use this command in natural gravity!");
                    return;
                }

                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
                if (gridWithSubGrids.Count > 0)
                {

                    foreach (var item in gridWithSubGrids)
                    {
                        bool isStatic = false;
                        bool isDynamic = false;
                        foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                        {

                            MyCubeGrid grid = groupNodes.NodeData;

                            if (!FacUtils.IsOwnerOrFactionOwned(grid, Context.Player.IdentityId, true))
                            {

                                continue;
                            }
                            else
                            {
                                //fix this lmao, one grid static, others dynamic it turns the dynamics to static and static to dynamic

                                if (grid.IsStatic)
                                {
                                    if (isDynamic)
                                    {
                                        break;
                                    }
                                    Action m_convertToShipResult = null;
                                    grid.RequestConversionToShip(m_convertToShipResult);
                                    Context.Respond("Converting to ship " + grid.DisplayName);
                                    isStatic = true;

                                }
                                else
                                {
                                    if (isStatic)
                                    {
                                        break;
                                    }
                                    try
                                    {
                                        isDynamic = true;
                                        if (grid.GridSizeEnum == MyCubeSize.Small)
                                        {
                                            break;
                                        }
                                        grid.Physics.ClearSpeed();
                                        grid.OnConvertedToStationRequest();

                                        Context.Respond("Converting to station IF grid is not moving." + grid.DisplayName);
                                    }
                                    catch (Exception)
                                    {
                                        Context.Respond("Grid cannot be moving!");

                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Context.Respond("Cant find a grid");
                }
            }
            else
            {
                Context.Respond("PlayerMakeShip not enabled");
            }
        }
        [Command("pcucount", "Player command, count PCU of connected grids")]
        [Permission(MyPromoteLevel.None)]
        public void CountPCU()
        {
            int totalPCU = 0;
            StringBuilder sb = new StringBuilder();
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
            if (gridWithSubGrids.Count > 0)
            {

                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {

                        MyCubeGrid grid = groupNodes.NodeData;


                        sb.Append(grid.DisplayName + " : " + grid.BlocksPCU + ",");
                        totalPCU += grid.BlocksPCU;



                    }
                }
            }
            else
            {
                Context.Respond("Cant find a grid");
            }
            //sb.Append("Total : " + totalPCU);
            Context.Respond(totalPCU.ToString(), "Total PCU");
            Context.Respond(sb.ToString(), "PCU");

        }
        [Command("admin rename", "admin command, rename a ship")]
        [Permission(MyPromoteLevel.Admin)]
        public void RenameGridAdmin(string gridname, string newname)
        {

            bool changed = false;

            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindGridGroup(gridname);
            foreach (var item in gridWithSubGrids)
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                {
                    MyCubeGrid grid = groupNodes.NodeData;

                    Context.Respond("Renaming " + grid.DisplayName + ". You may need to relog to see changes.");
                    grid.ChangeDisplayNameRequest(newname);


                    changed = true;
                    return;

                }
            }
            if (!changed)
            {
                Context.Respond("Couldnt find that grid, are you sure its owned by you or faction?");
            }


        }
        [Command("rename", "Player command, Rename a ship")]
        [Permission(MyPromoteLevel.None)]
        public void RenameGrid(string gridname, string newname)
        {

            bool changed = false;
            if (CrunchUtilitiesPlugin.file.PlayerMakeShip)
            {
                if (Context.Player == null)
                {
                    Context.Respond("Currently only a player can use this command :(");
                    return;
                }
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindGridGroup(gridname);
                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;
                        if (FacUtils.IsOwnerOrFactionOwned(grid, Context.Player.IdentityId, true) && grid.DisplayName.Equals(gridname))
                        {
                            Context.Respond("Renaming " + grid.DisplayName + ". You may need to relog to see changes.");
                            grid.ChangeDisplayNameRequest(newname);


                            changed = true;
                            return;
                        }
                    }
                }
                if (!changed)
                {
                    Context.Respond("Couldnt find that grid, are you sure its owned by you or faction?");
                }
            }
            else
            {
                Context.Respond("PlayerMakeShip not enabled");
            }
        }

        [Command("fixme", "Murder a player then respawn them at their current location")]
        [Permission(MyPromoteLevel.None)]
        public void FixPlayer()
        {
            if (CrunchUtilitiesPlugin.file.PlayerFixMe)
            {
                IMyPlayer player = Context.Player;
                long playerId;
                if (player == null)
                {
                    Context.Respond("Console cant do this");
                    return;
                }
                else
                {
                    playerId = player.IdentityId;
                }
                //rewrite this

                if (confirmations.ContainsKey(playerId))
                {


                    confirmations.TryGetValue(playerId, out long time);

                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() <= time)
                    {
                        try
                        {
                            Context.Respond("You should be fixed after respawning");
                            player.Character.Kill();
                            player.Character.Delete();
                            MyMultiplayer.Static.DisconnectClient(player.SteamUserId);
                            confirmations.Remove(player.Identity.IdentityId);
                        }
                        catch (Exception)
                        {
                            Context.Respond("You are really broken, this might help");
                            player.Character.Kill();
                            player.Character.Delete();
                            MyMultiplayer.Static.DisconnectClient(player.SteamUserId);
                            return;
                        }
                    }
                    else
                    {
                        Context.Respond("Time ran out, use !fixme again");
                        confirmations.Remove(Context.Player.IdentityId);
                    }
                }
                else
                {
                    long timeToAdd = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 20000);

                    confirmations.Add(playerId, timeToAdd);
                    Context.Respond("You will be killed and disconnected - run command again within 20 seconds to confirm.");
                }
            }
            else
            {
                Context.Respond("PlayerFixMe not enabled");
            }
        }
        [Command("getsteamid", "Get a specific identities steam name")]
        [Permission(MyPromoteLevel.None)]
        public void getSTEAMID(string target, bool online = false)
        {
            bool console = false;
            if (Context.Player == null)
            {
                console = true;
            }
            if (online)
            {
                Dictionary<String, String> badNames = new Dictionary<string, string>();
                foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                {

                    MyIdentity identity = CrunchUtilitiesPlugin.GetIdentityByNameOrId(player.Id.SteamId.ToString());
                    if (!identity.DisplayName.Equals(target))
                    {
                        string name = MyMultiplayer.Static.GetMemberName(player.Id.SteamId);
                        badNames.Add(name + " : " + identity.DisplayName, player.Id.SteamId.ToString());
                    }
                }
                if (badNames.Count == 0)
                {
                    if (console)
                    {
                        Context.Respond("No player with that name");
                        return;
                    }
                    SendMessage("[C]", "No player with that name", Color.Green, (long)Context.Player.SteamUserId);
                }
                foreach (KeyValuePair<string, string> pair in badNames)
                {
                    if (console)
                    {
                        Context.Respond("Names: " + pair.Key + " || ID: " + pair.Value);
                        return;
                    }
                    SendMessage("[C]", "Names: " + pair.Key + " || ID: " + pair.Value, Color.Green, (long)Context.Player.SteamUserId);
                }
            }
            else
            {
                IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(target);
                if (id == null)
                {
                    Context.Respond("Cant find that player.");
                    return;
                }

                string name = MyMultiplayer.Static.GetMemberName(MySession.Static.Players.TryGetSteamId(id.IdentityId));
                Context.Respond(name + " : " + id.DisplayName + MySession.Static.Players.TryGetSteamId(id.IdentityId).ToString());


            }
        }
        [Command("fac search", "shouldnt be required but keen wont add a search bar")]
        [Permission(MyPromoteLevel.None)]
        public void searchAllFactions(String name)
        {
            bool console = false;
            if (Context.Player == null)
            {
                console = true;
            }
            int count = 0;
            var sb = new StringBuilder();
            foreach (KeyValuePair<long, MyFaction> facs in MySession.Static.Factions)
            {
                if (facs.Value != null && facs.Value.Name != null && facs.Value.Name.ToLower().Contains(name.ToLower()))
                {
                    count++;
                    sb.Append("\n" + facs.Value.Name + " - [" + facs.Value.Tag + "]");
                }
            }

         

            if (!console)
            {
                DialogMessage m = new DialogMessage("Search for '" + name.ToLower() + "'", count + " Results", "For detailed information use !fac info TAG, the tag is the letters between the [] \n" +  sb.ToString());
                ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);

            }
            else
            {
                Context.Respond("Tags of online players", sb.ToString());
            }

        }
        [Command("tags", "show players faction tags")]
        [Permission(MyPromoteLevel.None)]
        public void showFactionTags()
        {
            bool console = false;
            if (Context.Player == null)
            {
                console = true;
            }
            Dictionary<String, String> tagsAndNames = new Dictionary<string, string>();
            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
            {
                string name = MyMultiplayer.Static.GetMemberName(player.Id.SteamId);
                MyIdentity identity = CrunchUtilitiesPlugin.GetIdentityByNameOrId(player.Id.SteamId.ToString());
                if (FacUtils.GetPlayersFaction(player.Identity.IdentityId) != null)
                {
                    if (tagsAndNames.ContainsKey(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag))
                    {
                        tagsAndNames.TryGetValue(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, out String temp);
                       

                        if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                        {
                            temp += "\n " + player.DisplayName + " (Founder) - [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "]";
                        }
                        else
                        {
                            if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                            {
                                temp += "\n " + player.DisplayName + " (Leader) - [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "]";
                            }

                            else
                            {
                                temp += "\n " + player.DisplayName + " - [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "]";
                            }
                        }
                            tagsAndNames.Remove(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag);
                            tagsAndNames.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                    }
                    else
                    {
                      String temp = "\n " + player.DisplayName + " - [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "]";
                        tagsAndNames.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                    }
                }
           
              
            }
            var sb = new StringBuilder();


            foreach (KeyValuePair<String, String> keys in tagsAndNames)
                {
                sb.Append("\nMembers of " + MySession.Static.Factions.TryGetFactionByTag(keys.Key).Name + " - [" + keys.Key + "]");
                sb.Append(keys.Value);
                }

            if (!console)
            {
                DialogMessage m = new DialogMessage("Tags of online players", "", sb.ToString());
                ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);

            }
            else {
                Context.Respond("Tags of online players", sb.ToString());
            }
           
        }
        [Command("listids", "Lists players steam IDs")]
        [Permission(MyPromoteLevel.None)]
        public void listSteamIDs()
        {
            bool console = false;
            if (Context.Player == null)
            {
                console = true;
            }
            Dictionary<String, String> badNames = new Dictionary<string, string>();
            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
            {
                string name = MyMultiplayer.Static.GetMemberName(player.Id.SteamId);
                MyIdentity identity = CrunchUtilitiesPlugin.GetIdentityByNameOrId(player.Id.SteamId.ToString());
                if (player.DisplayName.Equals(identity.DisplayName))
                {
                    if (badNames.ContainsKey(name + " : " + identity.DisplayName))
                    {
                        Context.Respond("Possibly bugged body " + name);
                        break;
                    }
                    badNames.Add(name + " : " + identity.DisplayName, player.Id.SteamId.ToString());
                }
                else
                {
                    if (badNames.ContainsKey(name + " : " + identity.DisplayName))
                    {
                        Context.Respond("Possibly bugged body " + name);
                        break;
                    }
                    badNames.Add(name + " : " + identity.DisplayName, player.Id.SteamId.ToString());
                }
            }
            if (badNames.Count == 0)
            {
                if (console)
                {
                    Context.Respond("No players online");
                    return;
                }
                SendMessage("[C]", "No players online", Color.Green, (long)Context.Player.SteamUserId);
            }
            foreach (KeyValuePair<string, string> pair in badNames)
            {
                if (console)
                {
                    Context.Respond("Names: " + pair.Key + " | ID: " + pair.Value);
                    CrunchUtilitiesPlugin.Log.Info("Names: " + pair.Key + " | ID: " + pair.Value);
                }
                else
                {
                    SendMessage("[C]", "Names: " + pair.Key + " | ID: " + pair.Value, Color.Green, (long)Context.Player.SteamUserId);
                    CrunchUtilitiesPlugin.Log.Info("Names: " + pair.Key + " | ID: " + pair.Value);
                }
            }
        }

        [Command("listnames", "Lists players names if they dont match steam names")]
        [Permission(MyPromoteLevel.None)]
        public void listNames()
        {
            bool console = false;
            if (Context.Player == null)
            {
                console = true;
            }
            Dictionary<String, String> badNames = new Dictionary<string, string>();
            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
            {
                string name = MyMultiplayer.Static.GetMemberName(player.Id.SteamId);
                MyIdentity identity = CrunchUtilitiesPlugin.GetIdentityByNameOrId(player.Id.SteamId.ToString());
                if (!identity.DisplayName.Equals(name))
                {
                    if (badNames.ContainsKey(name))
                    {
                        Context.Respond("Possibly bugged body " + name);
                        break;
                    }
                    badNames.Add(name, identity.DisplayName);
                }
            }
            if (badNames.Count == 0)
            {
                if (console)
                {
                    Context.Respond("No players with mismatching names :D");
                    return;
                }
                else
                {
                    SendMessage("[C]", "No players with mismatching names :D", Color.Green, (long)Context.Player.SteamUserId);
                    return;
                }
            }
            foreach (KeyValuePair<string, string> pair in badNames)
            {
                string temp;
                if (console)
                {
                    Context.Respond("Steam: " + pair.Key + " | Identity: " + pair.Value);
                    CrunchUtilitiesPlugin.Log.Info("Steam: " + pair.Key + " | Identity: " + pair.Value);
                }
                else
                {
                    SendMessage("[C]", "Steam: " + pair.Key + " | Identity: " + pair.Value, Color.Green, (long)Context.Player.SteamUserId);
                    CrunchUtilitiesPlugin.Log.Info("Steam: " + pair.Key + " | Identity: " + pair.Value);
                }
            }
        }
        [Command("updatename", "updates identity names")]
        [Permission(MyPromoteLevel.Admin)]
        public void UpdateIdentities(String playerNameOrId, String newName)
        {


            MyIdentity identity = CrunchUtilitiesPlugin.GetIdentityByNameOrId(playerNameOrId);

            if (identity == null)
            {
                Context.Respond("Error cant find that guy");
                return;
            }
            else
            {
                identity.SetDisplayName(newName);
                Context.Respond("New Identity Name : " + identity.DisplayName);
            }

        }






        [Command("eco", "list commands")]
        [Permission(MyPromoteLevel.None)]
        public void EcoHelp()
        {
            Context.Respond("\n"
            + "!eco balance <player/faction> <name/tag> - Shows a players balance \n"
            + "!eco give <player/faction> <name/tag> amount \n"
            + "!eco take <player/faction> <name/tag> amount \n"
            + "!eco pay <player/faction> <name/tag> amount \n"
            + "!eco deposit - Deposits credits in the grid you are looking at \n"
            + "!eco withdraw <amount> - Withdraws credits into the grid you are looking at");

        }

        [Command("eco balance", "See a players balance")]
        [Permission(MyPromoteLevel.Admin)]
        public void CheckMoneysPlayer(string type, string target)
        {
            type = type.ToLower();
            switch (type)
            {
                case "player":
                    //Context.Respond("Error Player not online");
                    IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(target);
                    if (id == null)
                    {
                        Context.Respond("Cant find that player.");
                        return;
                    }
                    Context.Respond(id.DisplayName + " Player Balance : " + String.Format("{0:n0}", EconUtils.getBalance(id.IdentityId)));


                    break;
                case "faction":
                    IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(target);
                    if (fac == null)
                    {
                        Context.Respond("Cant find that faction");
                        return;
                    }

                    Context.Respond(fac.Name + " Faction Balance : " + String.Format("{0:n0}", EconUtils.getBalance(fac.FactionId)));

                    break;

                default:
                    Context.Respond("Incorrect usage, example - !eco balance player PlayerName or !eco balance faction tag");
                    break;


            }
        }
        [Command("eco top", "moneys")]
        [Permission(MyPromoteLevel.Admin)]
        public void ecotop(int limit = 30, bool factions = false)
        {
            //essentials eco stuff but with factions and formatting for the numbers
            StringBuilder data = new StringBuilder();
            StringBuilder data2 = new StringBuilder();

            int iteration = 0;
            if (factions == false) {
                Dictionary<ulong, long> moneys = new Dictionary<ulong, long>();
                foreach (var p in MySession.Static.Players.GetAllPlayers())
                {
                    long IdentityID = MySession.Static.Players.TryGetIdentityId(p.SteamId);

                    moneys.Add(p.SteamId, EconUtils.getBalance(IdentityID));
                }
                var sortedmoneys = moneys.OrderByDescending(x => x.Value).ThenBy(x => x.Key);

                foreach (var value in sortedmoneys)
                {
                    if (iteration <= limit) {
                        iteration++;
                        data.AppendLine(MySession.Static.Players.TryGetIdentityNameFromSteamId(value.Key).ToString() + " - Balance: " + String.Format("{0:n0}", value.Value));
                        CrunchUtilitiesPlugin.Log.Info(MySession.Static.Players.TryGetIdentityNameFromSteamId(value.Key).ToString() + " - Balance: " + String.Format("{0:n0}", value.Value));
                        data2.AppendLine(MySession.Static.Players.TryGetIdentityNameFromSteamId(value.Key).ToString() + "," + value.Value);
                    }
                    else
                    {
                        break;
                    }
                }


                File.WriteAllText(CrunchUtilitiesPlugin.path + "//eco.csv", data2.ToString());
                File.WriteAllText(CrunchUtilitiesPlugin.path + "//eco-" + string.Format("{0:yyyy-MM-dd_HH-mm-ss-fff}", DateTime.Now) + ".csv", data2.ToString());
                if (Context.Player == null)
                {
                    Context.Respond("Top " + limit + " player balances\n" + data.ToString());
                    return;
                }
                ModCommunication.SendMessageTo(new DialogMessage("Top " + limit + " Player Balances", "", data.ToString()), Context.Player.SteamUserId);
            }
            else
            {
                Dictionary<String, long> moneys = new Dictionary<String, long>();
                foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
                {
                    if (f.Value.Tag.Length > 3)
                    {
                        if (f.Value.Tag.ToLower().Equals("unin") || f.Value.Tag.ToLower().Equals("fedr") || f.Value.Tag.ToLower().Equals("cons"))
                        {
                            moneys.Add(f.Value.Name + " - " + f.Value.Tag, EconUtils.getBalance(f.Value.FactionId));
                        }
                    }
                    else
                    {
                        moneys.Add(f.Value.Name + " - " + f.Value.Tag, EconUtils.getBalance(f.Value.FactionId));
                    }


                }
                var sortedmoneys = moneys.OrderByDescending(x => x.Value).ThenBy(x => x.Key);
                foreach (var value in sortedmoneys)
                {
                    if (iteration <= limit)
                    {

                        iteration++;
                        data.AppendLine(value.Key + " - Balance: " + String.Format("{0:n0}", value.Value));
                        CrunchUtilitiesPlugin.Log.Info(value.Key + " - Balance: " + String.Format("{0:n0}", value.Value));
                        data2.AppendLine(value.Key + value.Key + "," + value.Value);
                    }
                    else
                    {
                        break;
                    }
                }
                File.WriteAllText(CrunchUtilitiesPlugin.path + "//ecofac.csv", data2.ToString());

                File.WriteAllText(CrunchUtilitiesPlugin.path + "//ecofac-" + string.Format("{0:yyyy-MM-dd_HH-mm-ss-fff}", DateTime.Now) + ".csv", data2.ToString());


                if (Context.Player == null)
                {
                    Context.Respond("Top " + limit + " faction balances\n" + data.ToString());
                    return;
                }
                ModCommunication.SendMessageTo(new DialogMessage("Top " + limit + " Faction Balances", "", data.ToString()), Context.Player.SteamUserId);
            }

        }





        [Command("warstatus", "check war status")]
        [Permission(MyPromoteLevel.None)]
        public void declareWar(string tag1, string tag2)
        {


            IMyFaction fac1 = MySession.Static.Factions.TryGetFactionByTag(tag1);
            IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(tag2);
            if (fac1 == null)
            {
                Context.Respond("Cant find the first faction.");
                return;
            }
            if (fac2 == null)
            {
                Context.Respond("Cant find the second faction.");
                return;
            }

            if (MySession.Static.Factions.AreFactionsNeutrals(fac1.FactionId, fac2.FactionId))
            {
                Context.Respond(fac1.Name + " " + fac1.Tag + " are Neutral with " + fac2.Name + " " + fac2.Tag);
            }
            else
            {
                Context.Respond(fac1.Name + " " + fac1.Tag + " are at war with " + fac2.Name + " " + fac2.Tag);

            }








        }
        [Command("declarewar", "declare war")]
        [Permission(MyPromoteLevel.None)]
        public void declareWar(string tag)
        {
            if (CrunchUtilitiesPlugin.file.facInfo)
            {
                bool console = false;

                if (Context.Player == null)
                {
                    console = true;
                }
                IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (fac == null)
                {
                    MyPlayer player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(tag) as MyPlayer;
                    if (player == null)
                    {
                        IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(tag);
                        if (id == null)
                        {
                            Context.Respond("Cant find that faction or player.");
                            return;
                        }
                        else
                        {
                            fac = FacUtils.GetPlayersFaction(id.IdentityId);
                            if (fac == null)
                            {
                                Context.Respond("The player that was found does not have a faction.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        fac = FacUtils.GetPlayersFaction(player.Identity.IdentityId);
                        if (fac == null)
                        {
                            Context.Respond("The player that was found does not have a faction.");
                            return;
                        }
                    }



                }
                //now do the send peace request
                IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
                if (playerFac == null)
                {
                    Context.Respond("You dont have a faction.");
                    return;
                }
                if (playerFac.IsLeader(Context.Player.IdentityId) || playerFac.IsFounder(Context.Player.IdentityId))
                {
                    Sandbox.Game.Multiplayer.MyFactionCollection.DeclareWar(playerFac.FactionId, fac.FactionId);
                    Context.Respond("War were declared.");
                }
                else
                {
                    Context.Respond("You are not a faction leader or founder!");
                }
            }
            else
            {
                Context.Respond("Fac info not enabled");
            }

        }

        //[Command("createzone", "")]
        //[Permission(MyPromoteLevel.Admin)]
        //public void createzone(string safezonename, string tag, bool whitelist = true)
        //{
        //        if (Context.Player == null)
        //        {
        //        Context.Respond("No console");
        //        return;
        //        }
        //        IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
        //        if (fac == null)
        //        {
        //        Context.Respond("Cant find faction");
        //        return;


        //        }
        //    long[] factions = new long[1];
        //    factions.SetValue(fac.FactionId, 0);
        //    Vector3D Position = Context.Player.GetPosition();
        //    MyObjectBuilder_SafeZone objectBuilderSafeZone = new MyObjectBuilder_SafeZone();
        //    objectBuilderSafeZone.PositionAndOrientation = new MyPositionAndOrientation?(new MyPositionAndOrientation(Position, Vector3.Forward, Vector3.Up));
        //    objectBuilderSafeZone.PersistentFlags = MyPersistentEntityFlags2.InScene;
        //    objectBuilderSafeZone.Shape = MySafeZoneShape.Sphere;
        //    objectBuilderSafeZone.Radius = (float) 1000;
        //    objectBuilderSafeZone.Enabled = true;
        //    objectBuilderSafeZone.DisplayName = string.Format("A Safezone", (object)Position);
        //    objectBuilderSafeZone.AccessTypeGrids = MySafeZoneAccess.Blacklist;
        //    objectBuilderSafeZone.AccessTypeFloatingObjects = MySafeZoneAccess.Blacklist;
        //    objectBuilderSafeZone.AccessTypeFactions = MySafeZoneAccess.Whitelist;
        //    objectBuilderSafeZone.AccessTypePlayers = MySafeZoneAccess.Blacklist;
        //    objectBuilderSafeZone.Factions = factions;
        //    Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilderAndAdd((MyObjectBuilder_EntityBase)objectBuilderSafeZone, true);




        //}

        [Command("nofriendforyou", "make a faction declare war on everyone")]
        [Permission(MyPromoteLevel.Admin)]
        public void declarewaronall(string tag)
        {
            if (CrunchUtilitiesPlugin.file.facInfo)
            {
                bool console = false;

                if (Context.Player == null)
                {
                    console = true;
                }
                IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (fac == null)
                {
                    MyPlayer player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(tag) as MyPlayer;
                    if (player == null)
                    {
                        IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(tag);
                        if (id == null)
                        {
                            Context.Respond("Cant find that faction or player.");
                            return;
                        }
                        else
                        {
                            fac = FacUtils.GetPlayersFaction(id.IdentityId);
                            if (fac == null)
                            {
                                Context.Respond("The player that was found does not have a faction.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        fac = FacUtils.GetPlayersFaction(player.Identity.IdentityId);
                        if (fac == null)
                        {
                            Context.Respond("The player that was found does not have a faction.");
                            return;
                        }
                    }



                }
                foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
                {
                    if (f.Value != fac)
                    {
                        Sandbox.Game.Multiplayer.MyFactionCollection.DeclareWar(fac.FactionId, f.Value.FactionId);
                    }
                }
                Context.Respond("That faction has now declared war on all factions");

            }
            else
            {
                Context.Respond("Fac info not enabled");
            }

        }

        //[Command("joinfac", "send a request to join a faction")]
        //[Permission(MyPromoteLevel.None)]
        //public void sendJoinRequest(string tag)
        //{

        //    if (CrunchUtilitiesPlugin.file.facInfo)
        //    {
        //        bool console = false;

        //        if (Context.Player == null)
        //        {
        //            console = true;
        //        }
        //        IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
        //        if (fac == null)
        //        {
        //            MyPlayer player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(tag) as MyPlayer;
        //            if (player == null)
        //            {
        //                IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(tag);
        //                if (id == null)
        //                {
        //                    Context.Respond("Cant find that faction or player.");
        //                    return;
        //                }
        //                else
        //                {
        //                    fac = FacUtils.GetPlayersFaction(id.IdentityId);
        //                    if (fac == null)
        //                    {
        //                        Context.Respond("The player that was found does not have a faction.");
        //                        return;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                fac = FacUtils.GetPlayersFaction(player.Identity.IdentityId);
        //                if (fac == null)
        //                {
        //                    Context.Respond("The player that was found does not have a faction.");
        //                    return;
        //                }
        //            }



        //        }
        //        //now do the send peace request
        //        IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
        //        if (playerFac != null)
        //        {
        //            Context.Respond("You are already in a faction!");
        //            return;
        //        }

        //        MySession.Static.Factions.SendJoinRequest(fac.FactionId, (long) Context.Player.SteamUserId);
        //        Context.Respond("Sent a join request!");



        //    }
        //    else
        //    {
        //        Context.Respond("Fac info not enabled");
        //    }

        //}


        [Command("sendpeace", "send a peace request")]
        [Permission(MyPromoteLevel.None)]
        public void sendPeaceRequest(string tag)
        {
            if (CrunchUtilitiesPlugin.file.facInfo)
            {
                bool console = false;

                if (Context.Player == null)
                {
                    console = true;
                }
                IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (fac == null)
                {
                    MyPlayer player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(tag) as MyPlayer;
                    if (player == null)
                    {
                        IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(tag);
                        if (id == null)
                        {
                            Context.Respond("Cant find that faction or player.");
                            return;
                        }
                        else
                        {
                            fac = FacUtils.GetPlayersFaction(id.IdentityId);
                            if (fac == null)
                            {
                                Context.Respond("The player that was found does not have a faction.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        fac = FacUtils.GetPlayersFaction(player.Identity.IdentityId);
                        if (fac == null)
                        {
                            Context.Respond("The player that was found does not have a faction.");
                            return;
                        }
                    }



                }
                //now do the send peace request
                IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
                if (playerFac == null)
                {
                    Context.Respond("You dont have a faction.");
                    return;
                }
                if (playerFac.IsLeader(Context.Player.IdentityId) || playerFac.IsFounder(Context.Player.IdentityId))
                {
                    MyFactionPeaceRequestState state = MySession.Static.Factions.GetRequestState(playerFac.FactionId, fac.FactionId);


                    if (state != MyFactionPeaceRequestState.Sent)
                    {
                        Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, fac.FactionId);

                    }
                    if (state == MyFactionPeaceRequestState.Pending)
                    {
                        Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(playerFac.FactionId, fac.FactionId);


                    }
                    Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, fac.FactionId);
                    Context.Respond("Peace request sent");
                }
                else
                {
                    Context.Respond("You are not a faction leader or founder!");
                }


            }
            else
            {
                Context.Respond("Fac info not enabled");
            }

        }

        private static MethodInfo _factionChangeSuccessInfo = typeof(MyFactionCollection).GetMethod("FactionStateChangeSuccess", BindingFlags.NonPublic | BindingFlags.Static);
        [Command("fac info", "display a factions description")]
        [Permission(MyPromoteLevel.None)]
        public void DisplayFactionInfo2(string tag, bool members = false)
        {
            DisplayFactionInfo(tag, members);
        }

        [Command("facinfo", "display a factions description")]
        [Permission(MyPromoteLevel.None)]
        public void DisplayFactionInfo(string tag, bool members = false)
        {
            if (CrunchUtilitiesPlugin.file.facInfo)
            {
                bool console = false;
                members = true;
                if (Context.Player == null)
                {
                    console = true;
                }


                IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (fac == null)
                {
                    MyPlayer player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(tag) as MyPlayer;
                    if (player == null)
                    {
                        IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(tag);
                        if (id == null)
                        {
                            Context.Respond("Cant find that faction or player.");
                            return;
                        }
                        else
                        {
                            fac = FacUtils.GetPlayersFaction(id.IdentityId);
                            if (fac == null)
                            {
                                Context.Respond("The player that was found does not have a faction.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        fac = FacUtils.GetPlayersFaction(player.Identity.IdentityId);
                        if (fac == null)
                        {
                            Context.Respond("The player that was found does not have a faction.");
                            return;
                        }
                    }

                }
                string warstatus = "Relationship : ";
                IMyFaction playerfac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
                if (playerfac != null)
                {
                    if (MySession.Static.Factions.AreFactionsFriends(fac.FactionId, playerfac.FactionId))
                    {
                        warstatus += "Friends";

                    }
                    else
                    {
                        if (MySession.Static.Factions.AreFactionsNeutrals(fac.FactionId, playerfac.FactionId))
                        {
                            warstatus += "Neutral";
                        }
                        else
                        {
                            warstatus += "At War";

                        }
                    }
                }
                else
                {
                    warstatus += "Unknown";

                }
                var sb = new StringBuilder();
                if (members)
                {


                    foreach (KeyValuePair<long, MyFactionMember> m in fac.Members)
                    {
                        MyIdentity test = MySession.Static.Players.TryGetIdentity(m.Key);
                        if (test != null && test.DisplayName != null)
                        {
                            sb.Append("\n" + test.DisplayName);


                        }
                    }
                    if (!console)
                    {
                        DialogMessage m = new DialogMessage("Faction Info", fac.Name, "\nTag: " + fac.Tag + "\n" + warstatus + "\nDescription: " + fac.Description + "\nMembers: " + fac.Members.Count + "\n" + sb.ToString());
                        ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);
                    }
                    else
                    {
                        Context.Respond("Name: " + fac.Name + "\nTag: " + fac.Tag + "\n" + warstatus + "\nDescription: " + fac.Description + "\nMembers: " + fac.Members.Count + "\n" + sb.ToString());
                    }
                    return;
                }
                else
                {
                    Context.Respond("\nName: " + fac.Name + "\nTag: " + fac.Tag + "\n" + warstatus + "\nDescription: " + fac.Description + "\nMembers: " + fac.Members.Count);

                }
            }
            else
            {
                Context.Respond("Fac info not enabled");
            }

        }

        [Command("eco withdrawall", "Withdraw all moneys, buggy as fuck, try not to use this")]
        [Permission(MyPromoteLevel.Admin)]
        public void PlayerWithdrawAll()
        {
            Int64 balance;
            Int64 withdrew = 0;
            IMyPlayer player = Context.Player;
            balance = EconUtils.getBalance(player.IdentityId);

            if (player == null)
            {
                Context.Respond("Console cant withdraw money.....");
            }
            MyCubeBlock container = null;
            VRage.Game.ModAPI.IMyInventory invent = null;
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(player.Character);
            if (gridWithSubGrids.Count < 1)
            {
                Context.Respond("Couldnt find a grid");
                return;
            }

            MyItemType itemType = new MyInventoryItemFilter("MyObjectBuilder_PhysicalObject/SpaceCredit").ItemType;
            foreach (var item in gridWithSubGrids)
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                {
                    MyCubeGrid grid = groupNodes.NodeData;

                    if (!FacUtils.IsOwnerOrFactionOwned(grid, Context.Player.IdentityId, true))
                        continue;
                    else
                    {
                        foreach (VRage.Game.ModAPI.IMySlimBlock block in grid.GetBlocks())

                        {
                            if (block != null && block.BlockDefinition.Id.SubtypeName.Contains("Container"))
                            {
                                Int64 min = Int64.Parse(block.FatBlock.GetInventory().CurrentVolume.RawValue.ToString());
                                Int64 max = Int64.Parse(block.FatBlock.GetInventory().MaxVolume.RawValue.ToString());
                                Int64 difference = (max - min) * 1000;
                                if (balance >= Int32.MaxValue)
                                {
                                    Int64 newBalance = balance;
                                    bool canAdd = true;
                                    while (canAdd)
                                    {
                                        if (newBalance >= Int32.MaxValue)
                                        {
                                            newBalance -= Int32.MaxValue;
                                            if ((block.FatBlock.GetInventory().CanItemsBeAdded(VRage.MyFixedPoint.DeserializeStringSafe(Int32.MaxValue.ToString()), itemType)))
                                            {
                                                container = block.FatBlock as MyCubeBlock;
                                                invent = container.GetInventory();
                                                invent.AddItems(VRage.MyFixedPoint.DeserializeStringSafe(Int32.MaxValue.ToString()), new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
                                                EconUtils.takeMoney(player.IdentityId, Int32.MaxValue);
                                                withdrew += Int32.MaxValue;
                                                Context.Respond("Added the credits to " + container.DisplayNameText);
                                            }
                                            else
                                            {
                                                canAdd = false;
                                            }
                                        }
                                        else
                                        {
                                            if ((block.FatBlock.GetInventory().CanItemsBeAdded(VRage.MyFixedPoint.DeserializeStringSafe(newBalance.ToString()), itemType)))
                                            {
                                                container = block.FatBlock as MyCubeBlock;
                                                invent = container.GetInventory();
                                                invent.AddItems(VRage.MyFixedPoint.DeserializeStringSafe(newBalance.ToString()), new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
                                                EconUtils.takeMoney(player.IdentityId, newBalance);
                                                withdrew += newBalance;
                                                Context.Respond("Added the credits to " + container.DisplayNameText);
                                            }
                                            else
                                            {
                                                canAdd = false;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (block.FatBlock.GetInventory().CanItemsBeAdded(VRage.MyFixedPoint.DeserializeStringSafe(balance.ToString()), itemType))
                                    {

                                        container = block.FatBlock as MyCubeBlock;
                                        invent = container.GetInventory();

                                        invent.AddItems(VRage.MyFixedPoint.DeserializeStringSafe(balance.ToString()), new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
                                        EconUtils.takeMoney(player.IdentityId, balance);

                                        withdrew += balance;
                                        balance = 0;
                                        Context.Respond("Added the credits to " + container.DisplayNameText);

                                        Context.Respond("Withdrew : " + String.Format("{0:n0}", withdrew));
                                        return;
                                    }
                                }
                            }
                        }
                    }

                }
            }
            Context.Respond("Withdrew : " + String.Format("{0:n0}", withdrew));
        }


        [Command("drives", "organise jump drive gps list")]
        [Permission(MyPromoteLevel.None)]
        public void fixDrives()
        {
            List<IMyGps> playergpsList = MyAPIGateway.Session?.GPS.GetGpsList(Context.Player.Identity.IdentityId);

            if (playergpsList == null)
            {


                Context.Respond("You have no gps!");
                return;
            }
         
            Dictionary<int, IMyGps> someOrganisation = new Dictionary<int, IMyGps>();
            List<IMyGps> unsorted = new List<IMyGps>();
            int highest = 0;
            foreach (IMyGps gps in playergpsList)
            {
                if (gps != null && gps.Name != null && gps.Name.StartsWith("#"))
                {
                 
                        String part1 = gps.Name.Split(' ')[0].Replace("#", "");
                       if (int.TryParse(part1, out int result))
                    {
                        if (!someOrganisation.ContainsKey(result))
                        {
                            if (result > highest)
                            {
                                highest = result;
                            }
                            if (result <= 100) { 
                            someOrganisation.Add(result, gps);
                            }
                            else
                            {
                                unsorted.Add(gps);
                            }
                        }
                        else
                        {
                            unsorted.Add(gps);
                        }
                    }
                       else
                    {
                        unsorted.Add(gps);
                    }
               
                }
                else
                {
                    unsorted.Add(gps);
                }
 

            }
            Context.Respond("Sorting list!");
            if (highest > 100)
            {
                highest = 100;
            }
         foreach (IMyGps g in playergpsList)
            {
                MyAPIGateway.Session?.GPS.RemoveGps(Context.Player.Identity.IdentityId, g);
            }
            MyGpsCollection gpsCollection = (MyGpsCollection)MyAPIGateway.Session?.GPS;
            if (unsorted.Count > 0)
            {
                foreach (IMyGps gps in unsorted)
                {
                    MyGps gpsRef = gps as MyGps;
                    long entityId = 0L;
                    entityId = gpsRef.EntityId;
                    gpsCollection.SendAddGps(Context.Player.Identity.IdentityId, ref gpsRef, entityId, false);
                }
            }
           
            if (someOrganisation.Count > 0)
            {
                for (int i = highest; i > 0; i--)
                
                    {
                    if (someOrganisation.ContainsKey(i))
                    {
                        someOrganisation.TryGetValue(i, out IMyGps gps);
                        MyGps gpsRef = gps as MyGps;
                        long entityId = 0L;
                        entityId = gpsRef.EntityId;
                        gpsCollection.SendAddGps(Context.Player.Identity.IdentityId, ref gpsRef, entityId, false);

                    }
                }
            }
    
            
        }

        [Command("econ top", "block econ top")]
        [Permission(MyPromoteLevel.Admin)]
        public void EconTop()
        {
            Context.Respond("Use !eco top");
        }
        [Command("eco deposit", "Deposit moneys")]
        [Permission(MyPromoteLevel.None)]
        public void PlayerDeposit(bool playerOwned = false)
        {
            if (CrunchUtilitiesPlugin.file.Deposit)
            {
                IMyPlayer player = Context.Player;
                Int64 deposited = 0;
                if (player == null)
                {
                    Context.Respond("Console cant withdraw money.....");
                }
                String gridname = "";
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(player.Character);
                if (gridWithSubGrids.Count < 1)
                {
                    Context.Respond("Couldnt find a grid");
                    return;
                }
                //dpeosit from player inventory
                List<VRage.Game.ModAPI.IMyInventoryItem> itemList3 = new List<VRage.Game.ModAPI.IMyInventoryItem>();
                itemList3 = player.Character.GetInventory().GetItems();
                int i = 0;
                for (i = 0; i < itemList3.Count; i++)
                {

                    string itemId = itemList3[i].Content.SubtypeId.ToString();
                    if (itemId.Contains("SpaceCredit"))
                    {

                        float amountToMakeInt = float.Parse(itemList3[i].Amount.ToString());
                        Int64 amount = Convert.ToInt64(amountToMakeInt);
                        if (amount >= Int32.MaxValue)
                        {
                            bool hasCredits = true;
                            while (hasCredits)
                            {
                                deposited += amount;

                                player.Character.GetInventory().RemoveItemAmount(itemList3[i], VRage.MyFixedPoint.DeserializeStringSafe(amount.ToString()));
                                //Context.Respond("Stack exceeds 2.147 billion, split the stack!");
                                if (!player.Character.GetInventory().GetItems().Contains(itemList3[i]))
                                {
                                    hasCredits = false;
                                }
                            }
                            EconUtils.addMoney(player.IdentityId, amount);
                        }
                        else
                        {
                            deposited += itemList3[i].Amount.ToIntSafe();
                            EconUtils.addMoney(player.IdentityId, itemList3[i].Amount.ToIntSafe());
                            player.Character.GetInventory().RemoveItemAmount(itemList3[i], VRage.MyFixedPoint.DeserializeStringSafe(amount.ToString()));

                        }

                    }

                }

                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;
                        if (!FacUtils.IsOwnerOrFactionOwned(grid, Context.Player.IdentityId, true))
                            continue;
                        else
                        {
                            foreach (VRage.Game.ModAPI.IMySlimBlock block in grid.GetBlocks())
                            {
                                if (block.FatBlock != null && block.FatBlock.HasInventory)
                                {
                                    bool owned = false;
                                    switch (block.FatBlock.GetUserRelationToOwner(this.Context.Player.IdentityId))
                                    {
                                        case MyRelationsBetweenPlayerAndBlock.Owner:
                                            owned = true;
                                            break;
                                        case MyRelationsBetweenPlayerAndBlock.FactionShare:
                                            if (!playerOwned)
                                            {
                                                owned = false;
                                                break;
                                            }
                                            if (CrunchUtilitiesPlugin.file.FactionShareDeposit)
                                            {
                                                owned = true;
                                            }
                                            break;
                                        case MyRelationsBetweenPlayerAndBlock.Neutral:
                                            if (!playerOwned)
                                            {
                                                owned = false;
                                                break;
                                            }
                                            owned = true;
                                            break;
                                        case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                                            if (!playerOwned)
                                            {
                                                owned = false;
                                                break;
                                            }
                                            owned = false;
                                            break;
                                        case MyRelationsBetweenPlayerAndBlock.Enemies:
                                            if (!playerOwned)
                                            {
                                                owned = false;
                                                break;
                                            }
                                            owned = false;
                                            break;
                                    }
                                    List<VRage.Game.ModAPI.IMyInventoryItem> itemList2 = new List<VRage.Game.ModAPI.IMyInventoryItem>();
                                    itemList2 = block.FatBlock.GetInventory().GetItems();
                                    i = 0;
                                    if (owned)
                                    {

                                        for (i = 0; i < itemList2.Count; i++)
                                        {

                                            string itemId = itemList2[i].Content.SubtypeId.ToString();
                                            if (itemId.Contains("SpaceCredit"))
                                            {

                                                float amountToMakeInt = float.Parse(itemList2[i].Amount.ToString());
                                                Int64 amount = Convert.ToInt64(amountToMakeInt);
                                                if (amount >= Int32.MaxValue)
                                                {
                                                    bool hasCredits = true;
                                                    while (hasCredits)
                                                    {
                                                        deposited += amount;

                                                        block.FatBlock.GetInventory().RemoveItemAmount(itemList2[i], VRage.MyFixedPoint.DeserializeStringSafe(amount.ToString()));
                                                        //Context.Respond("Stack exceeds 2.147 billion, split the stack!");
                                                        if (!block.FatBlock.GetInventory().GetItems().Contains(itemList2[i]))
                                                        {
                                                            hasCredits = false;
                                                        }
                                                    }
                                                    EconUtils.addMoney(player.IdentityId, amount);
                                                    gridname += grid.DisplayName + ", ";
                                                }
                                                else
                                                {
                                                    deposited += itemList2[i].Amount.ToIntSafe();
                                                    EconUtils.addMoney(player.IdentityId, itemList2[i].Amount.ToIntSafe());
                                                    block.FatBlock.GetInventory().RemoveItemAmount(itemList2[i], VRage.MyFixedPoint.DeserializeStringSafe(amount.ToString()));
                                                    gridname += grid.DisplayName + ", ";
                                                }

                                            }

                                        }
                                    }

                                }
                            }

                        }
                    }

                }
                Context.Respond("Deposited : " + String.Format("{0:n0}", deposited));
                CrunchUtilitiesPlugin.Log.Info(player.SteamUserId + " deposited " + String.Format("{0:n0}", deposited) + " from " + gridname);
            }
            else
            {
                Context.Respond("Deposit not enabled.");
            }
        }


        [Command("eco withdraw", "Withdraw moneys")]
        [Permission(MyPromoteLevel.None)]
        public void PlayerWithdraw(Int64 amount)
        {
            if (CrunchUtilitiesPlugin.file.Withdraw)
            {
                Int64 balance;
                if (amount >= Int32.MaxValue)
                {
                    Context.Respond("Keen code doesnt allow stacks over 2.147 billion, try again with a smaller number");
                    return;
                }
                if (amount > CrunchUtilitiesPlugin.file.EcoWithdrawMax)
                {
                    Context.Respond("Cant withdraw over the maximum of " + String.Format("{0:n0}", CrunchUtilitiesPlugin.file.EcoWithdrawMax));
                    return;
                }
                IMyPlayer player = Context.Player;
                balance = EconUtils.getBalance(player.IdentityId);
                String gridname = "";
                if (balance < amount)
                {
                    Context.Respond("You dont have that much money.");
                    return;
                }
                if (amount < 0 || amount == 0)
                {
                    Context.Respond("No.");
                    return;
                }
                if (player == null)
                {
                    Context.Respond("Console cant withdraw money.....");
                    return;
                }
                MyCubeBlock container = null;
                VRage.Game.ModAPI.IMyInventory invent = null;
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(player.Character);
                if (gridWithSubGrids.Count < 1)
                {
                    Context.Respond("Couldnt find a grid");
                    return;
                }
                MyItemType itemType = new MyInventoryItemFilter("MyObjectBuilder_PhysicalObject/SpaceCredit").ItemType;
                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;
                        if (!FacUtils.IsOwnerOrFactionOwned(grid, Context.Player.IdentityId, true))
                            continue;
                        else
                        {

                            foreach (VRage.Game.ModAPI.IMySlimBlock block in grid.GetBlocks())
                            {
                                if (block != null && block.BlockDefinition.Id.SubtypeName.Contains("Container"))
                                {
                                    if (block.FatBlock.GetInventory().CanItemsBeAdded(VRage.MyFixedPoint.DeserializeStringSafe(amount.ToString()), itemType))
                                    {

                                        switch (block.FatBlock.GetUserRelationToOwner(this.Context.Player.IdentityId))
                                        {
                                            case MyRelationsBetweenPlayerAndBlock.Owner:
                                                container = block.FatBlock as MyCubeBlock;
                                                invent = container.GetInventory();

                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.FactionShare:
                                                container = block.FatBlock as MyCubeBlock;
                                                invent = container.GetInventory();

                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.Neutral:
                                                container = block.FatBlock as MyCubeBlock;
                                                invent = container.GetInventory();
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                                                Context.Respond("You dont own this.");
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.Enemies:
                                                Context.Respond("You dont own this.");
                                                break;
                                        }

                                        break;
                                    }
                                }

                            }
                            if (container == null)
                            {
                                Context.Respond("No container has free space for that many credits.");
                            }
                        }
                    }



                    if (invent != null)
                    {

                        if (invent.CanItemsBeAdded(VRage.MyFixedPoint.DeserializeStringSafe(amount.ToString()), itemType))
                        {
                            invent.AddItems(VRage.MyFixedPoint.DeserializeStringSafe(amount.ToString()), new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
                            EconUtils.takeMoney(player.IdentityId, amount);

                            Context.Respond("Added the credits to " + container.DisplayNameText);
                            CrunchUtilitiesPlugin.Log.Info(player.SteamUserId + " withdrew " + String.Format("{0:n0}", amount) + " to " + container.CubeGrid.DisplayName);
                        }
                        else
                        {
                            Context.Respond("Cant add that many");
                        }
                    }
                }
            }
            else
            {
                Context.Respond("Withdraw not enabled.");
            }
        }



        [Command("giveitem", "Give target player an item")]
        [Permission(MyPromoteLevel.Admin)]
        public void PlayerWithdraw(string PlayerName, string type, string subtypeName, int amount, bool force = false)
        {

            IMyPlayer player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(PlayerName) as MyPlayer;
            if (player == null)
            {
                Context.Respond("Cant find that player");
                return;
            }
           
            MyInventory invent = player.Character.GetInventory() as MyInventory;
            
            MyObjectBuilder_PhysicalObject item = null;
            MyItemType itemType;
            string newType;
            if (type.ToLower().Equals("ammo"))
            {
                newType = "AmmoMagazine";
            }
            else
            {
                newType = type;
            }

            string itemtype = "MyObjectBuilder_" + newType;
            MyDefinitionId.TryParse(itemtype, subtypeName, out MyDefinitionId id);
            if (id.ToString().Contains("null"))
            {
                Context.Respond("Invalid item, try Ammo, Ore, Ingot, Component, PhysicalGunObject, PhysicalItem");
                return;
            }

            VRage.Game.MyDefinitionId.TryParse(type, subtypeName, out VRage.Game.MyDefinitionId itemDefinition);

          
     
            if (!force)
            {
          
                    Sandbox.Game.MyVisualScriptLogicProvider.AddToPlayersInventory(player.IdentityId, itemDefinition, amount);
             
                    Context.Respond("Added the items");
                    SendMessage("[C]", "You were given " + amount + " " + subtypeName, Color.Green, (long)player.SteamUserId);
                    return;
            
            }
            else
            {
                switch (type.ToLower())
                {
                    //Eventually add some checks to see if the item exists before adding it
                    case "object":
                        item = new MyObjectBuilder_PhysicalGunObject { SubtypeName = subtypeName };
                        itemType = new MyInventoryItemFilter("MyObjectBuilder_PhysicalGunObject/" + subtypeName).ItemType;
                        break;
                    case "ammo":
                        item = new MyObjectBuilder_AmmoMagazine { SubtypeName = subtypeName };
                        itemType = new MyInventoryItemFilter("MyObjectBuilder_AmmoMagazine/" + subtypeName).ItemType;
                        break;
                    case "ore":
                        item = new MyObjectBuilder_Ore() { SubtypeName = subtypeName };
                        itemType = new MyInventoryItemFilter("MyObjectBuilder_Ore/" + subtypeName).ItemType;
                        break;
                    case "ingot":
                        item = new MyObjectBuilder_Ingot() { SubtypeName = subtypeName };
                        itemType = new MyInventoryItemFilter("MyObjectBuilder_Ingot/" + subtypeName).ItemType;
                        break;

                    case "component":
                        item = new MyObjectBuilder_Component() { SubtypeName = subtypeName };
                        itemType = new MyInventoryItemFilter("MyObjectBuilder_Component/" + subtypeName).ItemType;
                        break;
                    case "physicalgunobject":
                        item = new MyObjectBuilder_PhysicalGunObject { SubtypeName = subtypeName };
                        itemType = new MyInventoryItemFilter("MyObjectBuilder_PhysicalGunObject/" + subtypeName).ItemType;
                        break;
                    default:
                        Context.Respond("Error : use Object, Ammo, Ore, Ingot, Component, PhysicalGunObject");
                        return;

                }
                MethodInfo method = typeof(MyInventory).GetMethod("AddItemsInternal", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, new Type[4]
                {
             typeof (MyFixedPoint), typeof (MyObjectBuilder_PhysicalObject), typeof (uint?), typeof (int)
                },
                (ParameterModifier[])null);
                if (method == (MethodInfo)null)
                    throw new Exception("reflection error");
                method.Invoke((object)invent, new object[4]
                {VRage.MyFixedPoint.DeserializeString(amount.ToString()), item, new uint?(),-1});
                //refresh this or buggy stuff happens
                invent.Refresh();
                SendMessage("[C]", "You were given " + amount + " " + subtypeName, Color.Green, (long)player.SteamUserId);
                Context.Respond("items are added");
            }


        }


        //this command is broken af, might fix it eventually
        [Command("fac promote", "Broken command")]
        [Permission(MyPromoteLevel.Admin)]
        public void FactionPromoteFounder(string playerName)
        {
            Context.Respond("Doesnt work currently");
            return;
            if (Context.Player == null)
            {
                ; Context.Respond("Player command");
                return;
            }
            IMyFaction fac = MySession.Static.Factions.TryGetPlayerFaction(Context.Player.IdentityId);
            if (fac != null)
            {
                MyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(playerName);
                VRage.Collections.DictionaryReader<long, MyFactionMember> members = fac.Members;
                if (id == null)
                {
                    Context.Respond("Couldnt find that player");
                    return;
                }
                MyFactionMember currentFounder;
                MyFactionMember newFounder;
                bool foundPlayer = false;
                if (fac.IsFounder(Context.Player.IdentityId))
                {
                    foreach (VRage.Game.MyFactionMember key in members.Values)
                    {

                        if (id.IdentityId.Equals(key.PlayerId))
                        {
                            foundPlayer = true;
                            newFounder = key;
                        }
                        if (id.IdentityId.Equals(Context.Player.IdentityId))
                        {
                            currentFounder = key;
                        }
                    }

                }
                else
                {
                    Context.Respond("You need to be the founder to use this command.");
                }
            }
            else
            {
                Context.Respond("Error, no faction");
            }
        }

        [Command("broadcast", "Send a message in a noticable colour, false parameter will not show up in discord")]
        [Permission(MyPromoteLevel.Admin)]
        public void SendThisMessage(string message, string author = "Broadcast", int r = 50, int g = 168, int b = 168, Boolean global = true)
        {
            String context;
            if (global)
            {
                Color col = new Color(r, g, b);

                SendMessage(author, message, col, 0L);
                if (r == 50 && g == 168 && b == 168)
                {
                    Context.Respond("Sending to global");
                }
            }
            else
            {
                Context.Respond("Sending to players;");

                foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                {
                    MyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(player.Identity.IdentityId.ToString());
                    Color col = new Color(r, g, b);
                    SendMessage(author, message, col, (long)player.Id.SteamId);
                }
            }
        }

        [Command("eco take", "Admin command to take money")]
        [Permission(MyPromoteLevel.Admin)]
        public void TakeMoney(string type, string recipient, string inputAmount)
        {
            Int64 amount;
            inputAmount = inputAmount.Replace(",", "");
            inputAmount = inputAmount.Replace(".", "");
            inputAmount = inputAmount.Replace(" ", "");
            try
            {
                amount = Int64.Parse(inputAmount);
            }
            catch (Exception)
            {
                Context.Respond("Error parsing into number");
                return;
            }
            if (amount < 0 || amount == 0)
            {
                Context.Respond("Amount must be positive.");
                return;
            }
            type = type.ToLower();
            switch (type)
            {
                case "player":
                    //Context.Respond("Error Player not online");
                    IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                    if (id == null)
                    {
                        Context.Respond("Cant find that player.");
                        return;
                    }
                    if (EconUtils.getBalance(id.IdentityId) >= amount)
                    {
                        Context.Respond(id.DisplayName + " Balance Before Change : " + String.Format("{0:n0}", EconUtils.getBalance(id.IdentityId)));

                        EconUtils.takeMoney(id.IdentityId, amount);

                        Context.Respond(id.DisplayName + " Balance After Change : " + String.Format("{0:n0}", EconUtils.getBalance(id.IdentityId)));
                    }
                    else
                    {
                        Context.Respond("They cant afford that.");
                        Context.Respond(id.DisplayName + " Current Balance : " + String.Format("{0:n0}", EconUtils.getBalance(id.IdentityId)));
                    }
                    break;
                case "faction":
                    IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(recipient);
                    if (fac == null)
                    {
                        Context.Respond("Cant find that faction");
                        return;
                    }
                    if (EconUtils.getBalance(fac.FactionId) >= amount)
                    {
                        Context.Respond(fac.Name + " FACTION Balance Before Change : " + String.Format("{0:n0}", EconUtils.getBalance(fac.FactionId)));
                        EconUtils.takeMoney(fac.FactionId, amount);
                        Context.Respond(fac.Name + " FACTION Balance After Change : " + String.Format("{0:n0}", EconUtils.getBalance(fac.FactionId)));
                    }
                    else
                    {
                        Context.Respond("They cant afford that.");
                        Context.Respond(fac.Name + " Current Balance : " + String.Format("{0:n0}", EconUtils.getBalance(fac.FactionId)));
                    }
                    break;

                default:
                    Context.Respond("Incorrect usage, example - !eco take player PlayerName amount or !eco take faction tag amount");
                    break;


            }
        }
        [Command("eco give", "Admin command to give money")]
        [Permission(MyPromoteLevel.Admin)]
        public void GiveMoney(string type, string recipient, string inputAmount)
        {
            Int64 amount;
            inputAmount = inputAmount.Replace(",", "");
            inputAmount = inputAmount.Replace(".", "");
            inputAmount = inputAmount.Replace(" ", "");
            try
            {
                amount = Int64.Parse(inputAmount);
            }
            catch (Exception)
            {
                Context.Respond("Error parsing into number");
                return;
            }
            if (amount < 0 || amount == 0)
            {
                Context.Respond("Amount must be positive.");
                return;
            }
            type = type.ToLower();
            switch (type)
            {


                case "player":
                    if (recipient == "*")
                    {
                        foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
                        {
                            EconUtils.addMoney(p.Identity.IdentityId, amount);

                        }
                        SendMessage("CrunchEcon", "You got money : " + amount, Color.Cyan, 0);
                        return;
                    }
                    //Context.Respond("Error Player not online");
                    IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                    if (id == null)
                    {
                        Context.Respond("Cant find that player.");
                        return;
                    }
                    Context.Respond(id.DisplayName + " Balance Before Change : " + String.Format("{0:n0}", EconUtils.getBalance(id.IdentityId)));

                    EconUtils.addMoney(id.IdentityId, amount);

                    Context.Respond(id.DisplayName + " Balance After Change : " + String.Format("{0:n0}", EconUtils.getBalance(id.IdentityId)));

                    break;
                case "faction":
                    IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(recipient);
                    if (fac == null)
                    {
                        Context.Respond("Cant find that faction");
                        return;
                    }
                    Context.Respond(fac.Name + " FACTION Balance Before Change : " + String.Format("{0:n0}", EconUtils.getBalance(fac.FactionId)));
                    EconUtils.addMoney(fac.FactionId, amount);
                    Context.Respond(fac.Name + " FACTION Balance After Change : " + String.Format("{0:n0}", EconUtils.getBalance(fac.FactionId)));
                    break;

                default:
                    Context.Respond("Incorrect usage, example - !eco give player PlayerName amount or !eco give faction tag amount");
                    break;


            }
        }

        [Command("eco pay", "Transfer money from one player to another")]
        [Permission(MyPromoteLevel.None)]
        public void PayPlayer(string type, string recipient, string inputAmount, bool acrossInstances = false)
        {
            if (Context.Player == null)
            {
                Context.Respond("Only players can use this command");
                return;
            }
            if (CrunchUtilitiesPlugin.file.PlayerEcoPay)
            {
                Int64 amount;
                inputAmount = inputAmount.Replace(",", "");
                inputAmount = inputAmount.Replace(".", "");
                inputAmount = inputAmount.Replace(" ", "");
                try
                {
                    amount = Int64.Parse(inputAmount);
                }
                catch (Exception)
                {
                    SendMessage("[CrunchEcon]", "Error parsing amount", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }
                if (amount < 0 || amount == 0)
                {
                    SendMessage("[CrunchEcon]", "Must be a positive number", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }
                type = type.ToLower();
                switch (type)
                {
                    case "player":
                        //Context.Respond("Error Player not online");
                        MyPlayer player = null;
                        bool found = false;

                        if (acrossInstances)
                        {
                            IMyIdentity targetid = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                            if (targetid == null)
                            {
                                SendMessage("[CrunchEcon]", "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                                return;
                            }
                            if (EconUtils.getBalance(Context.Player.IdentityId) >= amount)
                            {
                                EconUtils.takeMoney(Context.Player.IdentityId, amount);
                                EconUtils.addMoney(targetid.IdentityId, amount);


                                //SendMessage("[CrunchEcon]", Context.Player.DisplayName + " Has sent you : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                                SendMessage("[CrunchEcon]", "You sent " + targetid.DisplayName + " : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                            }
                            else
                            {
                                SendMessage("[CrunchEcon]", "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                            }
                            return;
                        }
                        player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(recipient) as MyPlayer;
                        if (player == null)
                        {
                            SendMessage("[CrunchEcon]", "They arent online, trying across instance", Color.Red, (long)Context.Player.SteamUserId);
                            IMyIdentity targetid = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                            if (targetid == null)
                            {
                                SendMessage("[CrunchEcon]", "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                                return;
                            }
                            if (EconUtils.getBalance(Context.Player.IdentityId) >= amount)
                            {
                                EconUtils.takeMoney(Context.Player.IdentityId, amount);
                                EconUtils.addMoney(targetid.IdentityId, amount);


                                //SendMessage("[CrunchEcon]", Context.Player.DisplayName + " Has sent you : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                                SendMessage("[CrunchEcon]", "You sent " + targetid.DisplayName + " : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                                return;
                            }
                            else
                            {
                                SendMessage("[CrunchEcon]", "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                                return;
                            }
                        }

                        if (EconUtils.getBalance(Context.Player.IdentityId) >= amount)
                        {
                            EconUtils.takeMoney(Context.Player.IdentityId, amount);
                            EconUtils.addMoney(player.Identity.IdentityId, amount);


                            SendMessage("[CrunchEcon]", MyMultiplayer.Static.GetMemberName(Context.Player.SteamUserId) + " Has sent you : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                            SendMessage("[CrunchEcon]", "You sent " + MyMultiplayer.Static.GetMemberName(player.Id.SteamId) + " : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                        }
                        else
                        {
                            SendMessage("[CrunchEcon]", "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                        }
                        break;
                    case "faction":
                        IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(recipient);
                        if (fac == null)
                        {
                            SendMessage("[CrunchEcon]", "Cant find that faction", Color.Red, (long)Context.Player.SteamUserId);
                            return;
                        }
                        if (EconUtils.getBalance(Context.Player.IdentityId) >= amount)
                        {
                            //Probablty need to do some reflection/patching shit to add the transfer to the activity log
                            //EconUtils.takeMoney(Context.Player.IdentityId, amount);
                            // EconUtils.addMoney(fac.FactionId, amount);
                            EconUtils.TransferToFactionAccount(Context.Player.IdentityId, fac.FactionId, amount);
                            //I can add to the activity log with this but its not a great idea, it sets the balances to insane values
                            // EconUtils.TransferToFactionAccount(Context.Player.IdentityId, fac.FactionId, amount);
                            List<ulong> temp = new List<ulong>();
                            foreach (MyFactionMember mb in fac.Members.Values)
                            {


                                ulong steamid = MySession.Static.Players.TryGetSteamId(mb.PlayerId);
                                if (temp.Contains(steamid))
                                {
                                    break;
                                }
                                if (steamid == 0)
                                {
                                    break;
                                }
                                SendMessage("[CrunchEcon]", Context.Player.DisplayName + " Has sent : " + String.Format("{0:n0}", amount) + " SC to the faction bank.", Color.DarkGreen, (long)steamid);
                                temp.Add(steamid);

                            }


                            SendMessage("[CrunchEcon]", "You sent " + fac.Name + " : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                        }
                        else
                        {
                            SendMessage("[CrunchEcon]", "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                        }
                        break;
                    case "steam":
                        //Context.Respond("Error Player not online");
                        IMyIdentity id2 = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                        if (id2 == null)
                        {
                            SendMessage("[CrunchEcon]", "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                            return;
                        }
                        if (EconUtils.getBalance(Context.Player.IdentityId) >= amount)
                        {
                            EconUtils.takeMoney(Context.Player.IdentityId, amount);
                            EconUtils.addMoney(id2.IdentityId, amount);


                            //SendMessage("[CrunchEcon]", Context.Player.DisplayName + " Has sent you : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                            SendMessage("[CrunchEcon]", "You sent " + id2.DisplayName + " : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                        }
                        else
                        {
                            SendMessage("[CrunchEcon]", "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                        }
                        break;
                    default:
                        SendMessage("[CrunchEcon]", "Incorrect usage, example - !eco pay player PlayerName amount or !eco pay faction tag amount", Color.Red, (long)Context.Player.SteamUserId);
                        break;

                }
            }
            else
            {
                SendMessage("[CrunchEcon]", "Player pay not enabled", Color.Red, (long)Context.Player.SteamUserId);
            }
        }


        [Command("whis", "Message another player")]
        [Permission(MyPromoteLevel.None)]
        public void PrivateMessageW(string name, string message)
        {
            if (Context.Player == null)
            {
                Context.Respond("Only players can use this command");
                return;
            }

            MyPlayer player = null;

            //why is this even necessary
            var msgIndex = Context.RawArgs.IndexOf(" ", message.Length);
            if (msgIndex == -1 || msgIndex > Context.RawArgs.Length - 1)
                return;

            message = Context.RawArgs.Substring(msgIndex);
            player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(name) as MyPlayer;
            if (player == null)
            {
                SendMessage("[]", "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                return;
            }

            SendMessage("From " + MyMultiplayer.Static.GetMemberName(Context.Player.SteamUserId) + " >> " + message.ToString(), "", Color.MediumPurple, (long)player.Id.SteamId);
            SendMessage("To " + MyMultiplayer.Static.GetMemberName(player.Id.SteamId) + " >> " + message.ToString(), "", Color.MediumPurple, (long)Context.Player.SteamUserId);
        }
        [Command("msg", "Message another player")]
        [Permission(MyPromoteLevel.None)]
        public void PrivateMessage(string name, string message)
        {
            if (Context.Player == null)
            {
                Context.Respond("Only players can use this command");
                return;
            }

            MyPlayer player = null;

            //why is this even necessary
            var msgIndex = Context.RawArgs.IndexOf(" ", message.Length);
            if (msgIndex == -1 || msgIndex > Context.RawArgs.Length - 1)
                return;

            message = Context.RawArgs.Substring(msgIndex);
            player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(name) as MyPlayer;
            if (player == null)
            {
                SendMessage("[]", "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                return;
            }

            SendMessage("From " + MyMultiplayer.Static.GetMemberName(Context.Player.SteamUserId) + " >> " + message.ToString(), "", Color.MediumPurple, (long)player.Id.SteamId);
            SendMessage("To " + MyMultiplayer.Static.GetMemberName(player.Id.SteamId) + " >> " + message.ToString(), "", Color.MediumPurple, (long)Context.Player.SteamUserId);
        }


        public static void SendMessage(string author, string message, Color color, long steamID)
        {


            Logger _chatLog = LogManager.GetLogger("Chat");
            ScriptedChatMsg scriptedChatMsg1 = new ScriptedChatMsg();
            scriptedChatMsg1.Author = author;
            scriptedChatMsg1.Text = message;
            scriptedChatMsg1.Font = "White";
            scriptedChatMsg1.Color = color;
            scriptedChatMsg1.Target = Sync.Players.TryGetIdentityId((ulong)steamID);
            ScriptedChatMsg scriptedChatMsg2 = scriptedChatMsg1;
            MyMultiplayerBase.SendScriptedChatMessage(ref scriptedChatMsg2);
        }

        [Command("eco giveplayer", "gibs money to a player")]
        [Permission(MyPromoteLevel.Admin)]
        public void AddMoneysPlayer(string playerNameOrId, Int64 amount)
        {
            Context.Respond("Legacy command. Use !eco give player PlayerName amount or !eco give faction tag amount");
            return;
            //Context.Respond("Error Player not online");
            IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(playerNameOrId);
            if (id == null)
            {
                Context.Respond("Error cant find that guy");
                return;
            }
            Context.Respond(id.DisplayName + " Balance Before Change : " + EconUtils.getBalance(id.IdentityId));

            //could use EconUtils.addMoney here
            MyBankingSystem.ChangeBalance(id.IdentityId, amount);

            Context.Respond(id.DisplayName + " Balance After Change : " + EconUtils.getBalance(id.IdentityId));
            return;
        }

        [Command("eco resetplayer", "set a players balance to 0")]
        [Permission(MyPromoteLevel.Admin)]
        public void ResetMoneysPlayer(string playerNameOrId)
        {
            //Context.Respond("Error Player not online");
            IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(playerNameOrId);
            if (id == null)
            {
                Context.Respond("Error cant find that guy");
                return;
            }
            Context.Respond(id.DisplayName + " Balance Before Change : " + EconUtils.getBalance(id.IdentityId));

            EconUtils.takeMoney(id.IdentityId, EconUtils.getBalance(id.IdentityId));

            Context.Respond(id.DisplayName + " Balance After Change : " + EconUtils.getBalance(id.IdentityId));
            return;
        }

        [Command("eco resetfac", "Reset a factions balance")]
        [Permission(MyPromoteLevel.Admin)]
        public void AddMoneysFaction(string tag)
        {
            IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac != null)
            {
                Context.Respond(fac.Name + " FACTION Balance Before Change : " + fac.GetBalanceShortString());
                EconUtils.takeMoney(fac.FactionId, EconUtils.getBalance(fac.FactionId));
                Context.Respond(fac.Name + " FACTION Balance After Change : " + fac.GetBalanceShortString());
                return;
            }
            else
            {
                Context.Respond("Error faction not found");
            }
            return;

        }

        [Command("getfacid", "Get a factions ID")]
        [Permission(MyPromoteLevel.Admin)]
        public void GetFacId(string tag)
        {
            IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac != null)
            {
                Context.Respond(fac.FactionId.ToString());
                return;
            }
            else
            {
                Context.Respond("Error faction not found");
            }
            return;

        }
        [Command("cleargrid", "clear a grids inventory")]
        [Permission(MyPromoteLevel.Admin)]
        public void cleargrid()
        {
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
            int inventory = 0;
            foreach (var item in gridWithSubGrids)
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                {
                    MyCubeGrid grid = groupNodes.NodeData;
                    foreach (MySlimBlock b in grid.GetBlocks())
                    {
                        if (b.FatBlock != null && b.FatBlock.HasInventory)
                        {
                            b.FatBlock.GetInventory().Clear();
                            inventory++;
                        }
                    }
                }
            }
            Context.Respond("Cleared " + inventory + " inventories");
        }


        [Command("deletenoworkingbeacon", "delete beacons if they arent working")]
        [Permission(MyPromoteLevel.Admin)]
        public void deleteTheseGrids()
        {

            foreach (var group in MyCubeGridGroups.Static.Logical.Groups)
            {
                bool NPC = false;
                foreach (var item in group.Nodes)
                {
                    MyCubeGrid grid = item.NodeData;
                    if (((int)grid.Flags & 4) != 0)
                    {
                        //concealed
                        break;
                    }
                    if (item.NodeData.Projector != null)
                    {
                        //projection
                        break;
                    }
                    IEnumerable<MyBeacon> beacons = grid.GetFatBlocks().OfType<MyBeacon>();

                    bool delete = true;
                    foreach (long l in grid.BigOwners)
                    {
                        if (FacUtils.GetFactionTag(l) != null && FacUtils.GetFactionTag(l).Length > 3)
                        {
                            NPC = true;
                        }
                    }

                    if (NPC)
                    {
                        break;
                    }


                    foreach (MyBeacon b in beacons)
                    {

                        List<Sandbox.ModAPI.Ingame.IMyPowerProducer> PowerProducers = new List<Sandbox.ModAPI.Ingame.IMyPowerProducer>();
                        var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                        float power = 0f;
                        gts.GetBlocksOfType(PowerProducers);
                        if (PowerProducers.Count != 0)
                        {
                            foreach (var powerProducer in PowerProducers)
                            {
                                power += powerProducer.CurrentOutput;

                            }
                        }
                        if (b.IsFunctional && power > 0f)
                        {
                            delete = false;
                        }

                    }

                    if (delete)
                    {
                        if (warnedGrids.ContainsKey(grid.EntityId))
                        {
                            warnedGrids.TryGetValue(grid.EntityId, out int temp);
                            if (temp > 4)
                            {
                                var b = grid.GetFatBlocks<MyCockpit>();
                                foreach (var c in b)
                                {
                                    c.RemovePilot();
                                }
                                grid.Close();
                                foreach (long l in grid.BigOwners)
                                {
                                    SendMessage("Delete Notification", "The grid " + grid.DisplayName + " Was deleted for lacking a functional beacon and power", Color.DarkRed, (long)MySession.Static.Players.TryGetSteamId(l));

                                    CrunchUtilitiesPlugin.Log.Info("Deleting " + grid.DisplayName);
                                }
                            }
                            else
                            {
                                warnedGrids.Remove(grid.EntityId);
                                foreach (long l in grid.BigOwners)
                                {
                                    CrunchUtilitiesPlugin.Log.Info("Warning " + temp + " " + grid.DisplayName);
                                    SendMessage("Delete Warning", "The grid " + grid.DisplayName + " Will be deleted in " + (5 - temp) + " minutes if a beacon is not functional and powered", Color.DarkRed, (long)MySession.Static.Players.TryGetSteamId(l));

                                }

                                warnedGrids.Add(grid.EntityId, temp += 1);

                            }

                        }
                        else
                        {
                            foreach (long l in grid.BigOwners)
                            {
                                CrunchUtilitiesPlugin.Log.Info("Warning " + grid.DisplayName);
                                SendMessage("Delete Warning", "The grid " + grid.DisplayName + " Will be deleted in 5 minutes if a beacon is not functional and powered", Color.DarkRed, (long)MySession.Static.Players.TryGetSteamId(l));

                            }

                            warnedGrids.Add(grid.EntityId, 1);
                        }

                        //check if its had 5 warnings
                        //grid.Close();
                        //log it
                    }
                }
            }
        }
        [Command("gridtype", "return if its a station or not")]
        [Permission(MyPromoteLevel.None)]
        public void GetStaticOrDynamic(string gridname = "")
        {
            bool found = false;
            if (gridname.Equals(""))
            {

                if (Context.Player != null)
                {
                    ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
                    foreach (var item in gridWithSubGrids)
                    {
                        foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                        {

                            found = true;
                            MyCubeGrid grid = groupNodes.NodeData;

                            if (grid.IsStatic)
                            {
                                Context.Respond(grid.DisplayName + " : STATION");
                            }
                            else
                            {
                                Context.Respond(grid.DisplayName + " : SHIP");
                            }

                        }
                    }
                    if (!found)
                    {
                        Context.Respond("Couldnt find that grid.");
                    }
                }
                else
                {
                    Context.Respond("Console has to input a gridname!");
                }
            }
            else
            {

                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindGridGroup(gridname);
                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;

                        if (grid.IsStatic)
                        {
                            Context.Respond(grid.DisplayName + " : STATION");
                        }
                        else
                        {
                            Context.Respond(grid.DisplayName + " : SHIP");
                        }
                    }
                }
            }

        }


        [Command("eco takeplayer", "removes money from a player")]
        [Permission(MyPromoteLevel.Admin)]
        public void RemoveMoneysPlayer(string playerNameOrId, Int64 amount)
        {
            Context.Respond("Legacy command. Use !eco take player PlayerName amount or !eco take faction tag amount");
            return;
            IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(playerNameOrId);
            if (id == null)
            {
                Context.Respond("Error cant find that guy");
                return;
            }
            long Balance = EconUtils.getBalance(id.IdentityId);
            if (Balance >= amount)
            {
                amount = amount * -1;
                Context.Respond(id.DisplayName + " Balance Before Change : " + EconUtils.getBalance(id.IdentityId));
                //could use EconUtils.takeMoney here
                MyBankingSystem.ChangeBalance(id.IdentityId, amount);
                Context.Respond(id.DisplayName + " Balance After Change : " + EconUtils.getBalance(id.IdentityId));
                return;
            }
            else
            {
                Context.Respond("Player doesnt have that much, player balance : " + EconUtils.getBalance(id.IdentityId));
            }

            return;

        }

        [Command("eco givefac", "gibs money to a faction")]
        [Permission(MyPromoteLevel.Admin)]
        public void AddMoneysFaction(string tag, Int64 amount)
        {
            Context.Respond("Legacy command. Use !eco give player PlayerName amount or !eco give faction tag amount");
            return;
            IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac != null)
            {
                if (amount > 0)
                {
                    Context.Respond(fac.Name + " FACTION Balance Before Change : " + fac.GetBalanceShortString());
                    fac.RequestChangeBalance(amount);
                    Context.Respond(fac.Name + " FACTION Balance After Change : " + fac.GetBalanceShortString());
                    return;
                }
                else
                {
                    Context.Respond("Error must be a positive number");
                }
            }
            else
            {
                Context.Respond("Error faction not found");
            }
            return;

        }



        [Command("faction rep change", "Change repuation between factions")]
        [Permission(MyPromoteLevel.Admin)]
        public void ChangeFactionRep(string tag, string tag2, Int64 amount)
        {
            IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
            IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(tag2);
            if (fac != null && fac2 != null)
            {
                Context.Respond(fac.Name + " FACTION Reputation Before Change : " + MySession.Static.Factions.GetRelationBetweenFactions(fac.FactionId, fac2.FactionId));
                MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, fac2.FactionId, int.Parse(amount.ToString()));
                Context.Respond(fac.Name + " FACTION Reputation After Change : " + MySession.Static.Factions.GetRelationBetweenFactions(fac.FactionId, fac2.FactionId));

                


            }
            else
            {
                Context.Respond("Error faction not found");
            }
            return;

        }

        [Command("player rep change", "Change repuation between faction and player")]
        [Permission(MyPromoteLevel.Admin)]
        public void ChangePlayerRep(string playerNameOrId, string tag, Int64 amount)
        {
            MyIdentity player = CrunchUtilitiesPlugin.GetIdentityByNameOrId(playerNameOrId);
            IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (player != null && fac2 != null)
            {
                Context.Respond(player.DisplayName + " FACTION Reputation Before Change : " + MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(Context.Player.IdentityId, fac2.FactionId));
                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(player.IdentityId, fac2.FactionId, int.Parse(amount.ToString()));
                Context.Respond(player.DisplayName + " FACTION Reputation After Change : " + MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(Context.Player.IdentityId, fac2.FactionId));
            }
            else
            {
                Context.Respond("Error faction not found");
            }
            return;

        }

        [Command("eco takefac", "remove money from a faction")]
        [Permission(MyPromoteLevel.Admin)]
        public void removeMoneysFaction(string tag, Int64 amount)
        {
            Context.Respond("Legacy command. Use !eco take player PlayerName amount or !eco take faction tag amount");
            return;
            IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac != null)
            {
                if (amount > 0)
                {
                    string temp = fac.GetBalanceShortString();
                    temp = temp.Replace("SC", "");
                    temp = temp.Replace(",", "");
                    temp = temp.Replace(" ", "");

                    //could maybe use econUtils.getBalance but i havent tested with a faction
                    long Balance = long.Parse(temp);
                    if (Balance >= amount)
                    {
                        amount = amount * -1;
                        Context.Respond(fac.Name + " FACTION Balance Before Change : " + fac.GetBalanceShortString());

                        fac.RequestChangeBalance(amount);
                        Context.Respond(fac.Name + " FACTION Balance After Change : " + fac.GetBalanceShortString());
                        return;
                    }
                    else
                    {
                        Context.Respond("Error deducting too much, Faction Balance :  " + fac.GetBalanceShortString());
                    }
                }
                else
                {
                    Context.Respond("Error must be a positive number");
                }
            }
            else
            {
                Context.Respond("Error faction not found");
            }
            return;

        }
    }

}
