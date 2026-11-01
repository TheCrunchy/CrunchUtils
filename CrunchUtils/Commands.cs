using NLog;
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
using VRage.Network;
using SpaceEngineers.Game.Entities.Blocks;
using VRage.Voxels;
using Sandbox.Engine.Voxels;
using VRage.Library.Utils;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ObjectBuilders.ComponentSystem;
using Sandbox.Game.Entities.Character.Components;
using System.Media;
using Sandbox.Game.SessionComponents;
using IMyBeacon = Sandbox.ModAPI.IMyBeacon;

namespace CrunchUtilities
{
    public class Commands : CommandModule
    {
        private static readonly Dictionary<long, long> confirmations = new Dictionary<long, long>();

        [Command("crunch reload", "Reload the config")]
        [Permission(MyPromoteLevel.Admin)]
        public void ReloadConfig()
        {
            CrunchUtilitiesPlugin.LoadConfig();
            Context.Respond("Reloaded config");
        }

        [Command("admin makeship", "Admin command, Turn a station and connected grids into a ship")]
        [Permission(MyPromoteLevel.Admin)]
        public void MakeShip(string name = "")
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

            var player = GetPlayerByNameOrId(playerName);
            if (player != null)
            {
                /* If he is online we check if he is currently seated. If he is eject him. */
                if (player?.Controller.ControlledEntity is MyCockpit controller)
                {
                    controller.Use();
                    Context.Respond($"Player '{playerName}' ejected and murdered.");
                    player.Character.Kill();
                    player.Character.Delete();
                    MyMultiplayer.Static.DisconnectClient(player.SteamUserId);
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
                        IMyCharacter character = pilot;
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
        public void MakeStation(string name = "")
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
                if (CrunchUtilitiesPlugin.filteredDrillIdentityIds.Contains(Context.Player.Identity.IdentityId))
                {
                    CrunchUtilitiesPlugin.filteredDrillIdentityIds.Remove(Context.Player.Identity.IdentityId);
                    Context.Respond("No longer deleting stone", Color.Cyan, "Tiny Drill Elves");
                }
                else
                {
                    CrunchUtilitiesPlugin.filteredDrillIdentityIds.Add(Context.Player.Identity.IdentityId);
                    Context.Respond("Now deleting stone, if you own the drill", Color.Cyan, "Tiny Drill Elves");
                }

            }
            else
            {
                Context.Respond("not enabled");
            }
        }
        public int DeleteStone(List<MyCubeGrid> gridsToSearch)
        {
            int count = 0;
            long playerId = Context.Player.IdentityId;
            // CHANGE: only tell the player about skipped (not owned) containers once per command instead of once per block/grid
            bool notOwnedReported = false;
            foreach (MyCubeGrid grid in gridsToSearch)
            {
                // CHANGE: iterate only this grid's own blocks instead of GetTerminalSystemForGrid(grid).
                // The terminal system is the grid's LOGICAL group, and a locked connector adds a logical link
                // (MyShipConnector OnConstraintAdded(GridLinkTypeEnum.Logical)), so stone was also being deleted
                // from other players' grids docked by connector.
                // Rotor/piston subgrids are still covered because the caller now passes the whole mechanical group.
                foreach (MyCubeBlock block in grid.GetFatBlocks())
                {
                    // HasInventory checks the inventory component, so this covers modded blocks with inventories too
                    if (block == null || !block.HasInventory)
                        continue;

                    // CHANGE: ownership is now checked per inventory block instead of per grid (was
                    // FacUtils.IsOwnerOrFactionOwned(grid, ...) on grid.BigOwners). This means:
                    //  - another player's container on your grid (or on a mechanically attached grid) is left alone
                    //  - bare rotor/piston heads and armor-only subgrids no longer fail the check and spam the message,
                    //    they have no inventory blocks so they are never checked at all
                    //  - blocks owned by "Nobody" are skipped (safe default)
                    if (!CanRemoveStoneFrom(block, playerId))
                    {
                        if (!notOwnedReported)
                        {
                            // CHANGE: was "You dont own this", reworded since stone in the player's own blocks is still removed
                            Context.Respond("Skipped containers you dont own");
                            notOwnedReported = true;
                        }
                        continue;
                    }

                    MyInventory inventory = block.GetInventory();
                    if (inventory == null)
                        continue;

                    // CHANGE: loop backwards. MyInventory.GetItems() returns the live internal list, so removing while
                    // looping forwards shifted the list and skipped the item after every removed stack.
                    var items = inventory.GetItems();
                    for (int i = items.Count - 1; i >= 0; i--)
                    {
                        // CHANGE: exact type check + the shared configurable matcher (same one the drill patch uses),
                        // replacing SubtypeId.Contains("Stone") && TypeId.Contains("Ore") substring checks.
                        // Master list + the personal list of the player RUNNING the command, the container owner's
                        // personal list doesnt matter (they didnt run it)
                        if (items[i].Content.TypeId == typeof(MyObjectBuilder_Ore) && items[i].Content.SubtypeName == "Stone")
                        {
                            count += items[i].Amount.ToIntSafe();
                            inventory.RemoveItems(items[i].ItemId);
                        }
                    }
                }
            }
            return count;
        }

        // CHANGE: per block ownership check for !stone. Owner, or a member of the owner's faction.
        // Blocks owned by "Nobody" (0) are skipped. InSameFaction requires a real shared faction (see FacUtils).
        private static bool CanRemoveStoneFrom(MyCubeBlock block, long playerId)
        {
            long ownerId = block.OwnerId;
            if (ownerId == 0)
                return false;
            return ownerId == playerId || FacUtils.InSameFaction(playerId, ownerId);
        }

        public static int totalcount = 0;
        [Command("stone", "Delete all stone in a grid")]
        [Permission(MyPromoteLevel.None)]
        public void DeleteStone(bool outputcount = false)
        {

            if (CrunchUtilitiesPlugin.file.DeleteStone)
            {
                if (CrunchUtilitiesPlugin.file.DeleteStoneAuto)
                {
                    Context.Respond("Never want to mine stone? Put !stone in the drill name or use !togglestone");
                }

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

                List<MyCubeGrid> tempList = new List<MyCubeGrid>();
                if (Context.Player.Character != null && Context.Player?.Controller.ControlledEntity is MyCockpit controller)
                {
                    tempList.AddRange(MyCubeGridGroups.Static.Mechanical.GetGroupNodes(controller.CubeGrid));
                }
                else
                {
                    ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroupMechanical(Context.Player.Character);
                    foreach (var item in gridWithSubGrids)
                    {
                        foreach (MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node groupNodes in item.Nodes)
                        {
                            tempList.Add(groupNodes.NodeData);
                        }
                    }
                }

                int count = DeleteStone(tempList);
                if (count == 0)
                    currentCooldownMap.Remove(Context.Player.IdentityId);

                Context.Respond(count + " Stone Deleted");
                totalcount += count;
                if (outputcount)
                    Context.Respond(string.Format("{0:n0}", totalcount) + " Deleted in total on this instance.");
            }
            else
            {
                Context.Respond("stone not enabled");
            }
        }

        [Command("TestEconSync", "admin command no use")]
        [Permission(MyPromoteLevel.Admin)]
        public void TestEconSync()
        {
            var balance = EconUtils.GetBalance(Context.Player.IdentityId);
            MyBankingSystem.Static.RemoveAccount(Context.Player.IdentityId);
            MyBankingSystem.Static.CreateAccount(Context.Player.IdentityId, balance);
            Context.Respond("Syncing your balance");
        }

        [Command("fulleconsync", "admin command no use")]
        [Permission(MyPromoteLevel.Admin)]
        public void FullEconSyncLower()
        {
            FullEconSync();
        }

        [Command("FullEconSync", "admin command no use")]
        [Permission(MyPromoteLevel.Admin)]
        public void FullEconSync()
        {
            foreach (var p in MySession.Static.Players.GetAllPlayers())
            {
                long IdentityID = MySession.Static.Players.TryGetIdentityId(p.SteamId);
                if (MyBankingSystem.Static.TryGetAccountInfo(IdentityID, out var account))
                {
                    CrunchUtilitiesPlugin.IdsToYEET.Push(IdentityID);
                }
            }
            Context.Respond($"{CrunchUtilitiesPlugin.IdsToYEET.Count} balances to sync");
        }


        [Command("SingleEconSync", "admin command no use")]
        [Permission(MyPromoteLevel.Admin)]
        public void EconSync(string playerNameOrSteamId)
        {
            EconSyncLower(playerNameOrSteamId);
        }

        [Command("singleeconsync", "admin command no use")]
        [Permission(MyPromoteLevel.Admin)]
        public void EconSyncLower(string playerNameOrSteamId)
        {
            var identityId = CrunchUtilitiesPlugin.GetIdentityByNameOrId(playerNameOrSteamId);
            if (identityId != null)
            {
                if (MyBankingSystem.Static.TryGetAccountInfo(identityId.IdentityId, out var account))
                {
                    CrunchUtilitiesPlugin.IdsToYEET.Push(identityId.IdentityId);
                }
                Context.Respond($"{CrunchUtilitiesPlugin.IdsToYEET.Count} balances to sync");
                return;
            }
            Context.Respond($"Couldnt find that player");
        }

        [Command("eco resetbalances", "admin command to wipe every identity balance")]
        [Permission(MyPromoteLevel.Admin)]
        public void ResetAll()
        {
            int count = 0;
            foreach (var p in MySession.Static.Players.GetAllPlayers())
            {
                long IdentityID = MySession.Static.Players.TryGetIdentityId(p.SteamId);
                if (MyBankingSystem.Static.TryGetAccountInfo(IdentityID, out var account))
                {
                    count++;
                    MyBankingSystem.Static.RemoveAccount(IdentityID);
                    MyBankingSystem.Static.CreateAccount(IdentityID, MyBankingSystem.BankingSystemDefinition.StartingBalance);
                }
            }
            foreach (MyFaction f in MySession.Static.Factions.GetAllFactions())
            {
                MyBankingSystem.Static.RemoveAccount(f.FactionId);
                MyBankingSystem.Static.CreateAccount(f.FactionId, 0);
                count++;
            }
            Context.Respond($"{count} balances deleted");
        }


        [Command("eco resetplayers", "admin command to wipe every identity balance")]
        [Permission(MyPromoteLevel.Admin)]
        public void ResetPlayers()
        {
            int count = 0;
            foreach (var p in MySession.Static.Players.GetAllPlayers())
            {
                long IdentityID = MySession.Static.Players.TryGetIdentityId(p.SteamId);
                if (MyBankingSystem.Static.TryGetAccountInfo(IdentityID, out var account))
                {
                    count++;
                    MyBankingSystem.Static.RemoveAccount(IdentityID);
                    MyBankingSystem.Static.CreateAccount(IdentityID, MyBankingSystem.BankingSystemDefinition.StartingBalance);
                }
            }
            Context.Respond($"{count} balances deleted");
        }
        [Command("eco resetfactions", "admin command to wipe every identity balance")]
        [Permission(MyPromoteLevel.Admin)]
        public void ResetFactions()
        {
            int count = 0;
            foreach (MyFaction f in MySession.Static.Factions.GetAllFactions())
            {
                MyBankingSystem.Static.RemoveAccount(f.FactionId);
                MyBankingSystem.Static.CreateAccount(f.FactionId, 0);
                count++;
            }
            Context.Respond($"{count} balances deleted");
        }

        [Command("claim", "Player command, claim a shared grid")]
        [Permission(MyPromoteLevel.None)]
        public void ClaimCommand(bool shareWithFaction = false)
        {
            if (CrunchUtilitiesPlugin.file.Claim)
            {
                double blockCount = 0;
                double factionShared = 0;
                double sharedWithAll = 0;
                double nonfunc = 0;
                IMyFaction fac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
                if (fac == null)
                {
                    Context.Respond("Not in a faction.");
                    return;
                }
                if (CrunchUtilitiesPlugin.file.ClaimOnlyForLeaders)
                {
                    if (!fac.IsLeader(Context.Player.IdentityId) && !fac.IsFounder(Context.Player.IdentityId))
                    {
                        Context.Respond("Not a founder or leader");
                        return;
                    }
                }

                ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroupMechanical(Context.Player.Character);

                if (gridWithSubGrids.Count > 0)
                {
                    foreach (var item in gridWithSubGrids)
                    {
                        foreach (MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node groupNodes in item.Nodes)
                        {
                            MyCubeGrid grid = groupNodes.NodeData;
                            if (!grid.Editable)
                            {
                                SendMessage("Server", Context.Player.DisplayName + " is trying to claim an admin protected grid.", Color.Red, 0L);
                                return;
                            }

                            if (!grid.DestructibleBlocks)
                            {
                                SendMessage("Server", Context.Player.DisplayName + " is trying to claim an admin protected grid.", Color.Red, 0L);
                                return;
                            }
                            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);

                            //rewrite this shit eventually to not be trash
                            if (MySession.Static.Factions.GetStationByGridId(grid.EntityId) == null)
                            {
                                foreach (MyCubeBlock block in grid.GetFatBlocks())
                                {
                                    if (block.OwnerId > 0)
                                    {
                                        blockCount += 1;
                                        if (!block.IsFunctional && !CrunchUtilitiesPlugin.file.ClaimOnlyFactionGrids)
                                        {
                                            nonfunc++;
                                            break;
                                        }
                                        switch (block.GetUserRelationToOwner(Context.Player.IdentityId))
                                        {
                                            case MyRelationsBetweenPlayerAndBlock.Owner:
                                                factionShared += 1;
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.FactionShare:
                                                if (CrunchUtilitiesPlugin.file.ClaimFactionGridsOnlyLeaders && !fac.IsLeader(Context.Player.IdentityId) && !fac.IsFounder(Context.Player.IdentityId))
                                                {
                                                    Context.Respond("Claiming faction grids is not allowed unless a founder or leader.");
                                                    return;
                                                }
                                                factionShared += 1;
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                                                //sharedWithAll += 1;
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.Neutral:
                                            case MyRelationsBetweenPlayerAndBlock.Enemies:
                                            case MyRelationsBetweenPlayerAndBlock.Friends:
                                                {
                                                    if (CrunchUtilitiesPlugin.file.ClaimOnlyFactionGrids)
                                                    {
                                                        Context.Respond("Grid is not owned by faction, no claim for you!");
                                                        return;
                                                    }
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Context.Respond("Cannot claim economy stations!");
                                return;
                            }
                            double totalShared = factionShared + sharedWithAll;

                            double sharedPercent = (totalShared / blockCount) * 100;

                            Context.Respond("Percent required : " + CrunchUtilitiesPlugin.file.ClaimPercent.ToString());
                            if (sharedPercent >= CrunchUtilitiesPlugin.file.ClaimPercent)
                            {
                                if (shareWithFaction)
                                {
                                    grid.ChangeGridOwner(Context.Player.IdentityId, MyOwnershipShareModeEnum.Faction);
                                }
                                else
                                {
                                    grid.ChangeGridOwner(Context.Player.IdentityId, MyOwnershipShareModeEnum.None);

                                }

                                Context.Respond("Giving ownership to you, Owned: " + sharedPercent.ToString());
                            }
                            else
                            {
                                Context.Respond("Not enough shared percentage, Owned: " + sharedPercent.ToString());
                                if (nonfunc > 0)
                                {
                                    Context.Respond($"Non functional blocks present: {nonfunc}, these do not count towards ownership.");
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
                Context.Respond("Claim not enabled");
            }
        }

        public static Dictionary<long, ShipOffer> saleOffers = new Dictionary<long, ShipOffer>();
        [Command("sellgrid", "send an offer to sell a ship for a set price.")]
        [Permission(MyPromoteLevel.None)]
        public void SellShip(string amount)
        {
            if (!CrunchUtilitiesPlugin.file.ShipSaleCommands)
            {
                Context.Respond("Commands not enabled, enable ShipSaleCommands in config");
                return;
            }
            long playerId = 0;
            ulong playerSteamId = 0;
            string name = "";
            BoundingSphereD sphere = new BoundingSphereD(Context.Player.Character.PositionComp.GetPosition(), 10);
            var l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
            int players = 0;
            foreach (VRage.ModAPI.IMyEntity entity in l)
            {
                if (entity is MyCharacter character)
                {
                    if (character.ControlSteamId.Equals(Context.Player.SteamUserId))
                        continue;

                    name = character.DisplayName;
                    playerId = character.GetPlayerIdentityId();
                    playerSteamId = character.ControlSteamId;
                    players++;
                }
            }

            if (players > 1)
            {
                Context.Respond("Too many players within 10m!");
                return;
            }
            if (players == 0)
            {
                Context.Respond("No player within 10m to sell grid to.");
                return;
            }

            amount = amount.Replace(",", "");
            amount = amount.Replace(".", "");
            long.TryParse(amount, out long price);
            if (price < 0)
            {
                Context.Respond("No, the price cannot be below 0.");
                return;
            }

            ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroupMechanical(Context.Player.Character);
            bool found = false;

            ShipOffer offer = new ShipOffer();
            if (gridWithSubGrids.Count > 0)
            {
                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;

                        if (FacUtils.IsOwnerOrFactionOwned(grid, Context.Player.IdentityId, false))
                        {
                            offer.gridsInOffer.Add(grid.EntityId);
                            found = true;
                        }
                    }
                }
            }

            if (found)
            {
                offer.SellerIdentityId = Context.Player.IdentityId;
                offer.price = price;
                offer.SellerSteamId = (long)Context.Player.SteamUserId;
                if (!saleOffers.ContainsKey(playerId))
                {
                    saleOffers.Add(playerId, offer);
                    Context.Respond("Offer to sell grid sent to " + name + " For " + string.Format("{0:n0}", price) + " SC. Offer is active for 30 seconds.");
                    SendMessage("The Government", Context.Player.Character.DisplayName + " wants to sell you this grid for " + string.Format("{0:n0}", price) + " SC. To accept use !acceptgrid within 30 seconds", Color.Cyan, (long)playerSteamId);
                }
                else
                {
                    Context.Respond("They already have an offer. Wait or have them use !denygrid");
                }
            }
            else
            {
                Context.Respond("No grids found, are you looking at it and own it?");
                return;
            }
        }

        [Command("denygrid", "deny ownership of the grid")]
        [Permission(MyPromoteLevel.None)]
        public void DenyGridOffer()
        {
            if (!CrunchUtilitiesPlugin.file.ShipSaleCommands)
            {
                Context.Respond("Commands not enabled, enable ShipSaleCommands in config");
                return;
            }
            if (saleOffers.TryGetValue(Context.Player.IdentityId, out ShipOffer offer))
            {
                saleOffers.Remove(Context.Player.IdentityId);
                Context.Respond("Denied the offer");

                SendMessage("The Government", "Player denied your offer to sell grid.", Color.Red, offer.SellerSteamId);
            }
            else
            {
                Context.Respond("You have no offers available to deny");
            }
        }
        [Command("acceptgrid", "accept ownership of the grid")]
        [Permission(MyPromoteLevel.None)]
        public void AcceptGridOffer()
        {
            if (!CrunchUtilitiesPlugin.file.ShipSaleCommands)
            {
                Context.Respond("Commands not enabled, enable ShipSaleCommands in config");
                return;
            }
            if (saleOffers.TryGetValue(Context.Player.IdentityId, out ShipOffer offer))
            {
                saleOffers.Remove(Context.Player.IdentityId);
                Context.Respond("Accepting the offer if the grids exist.");
                List<MyCubeGrid> grids = new List<MyCubeGrid>();
                if (EconUtils.GetBalance(Context.Player.IdentityId) >= offer.price)
                {
                    foreach (long l in offer.gridsInOffer)
                    {
                        if (MyAPIGateway.Entities.GetEntityById(l) is MyCubeGrid grid)
                        {
                            grids.Add(grid);
                        }
                        else
                        {
                            Context.Respond("One of the grids does not exist, cancelling purchase.");
                            saleOffers.Remove(Context.Player.IdentityId);
                        }
                    }
                    foreach (MyCubeGrid grid in grids)
                    {
                        grid.ChangeGridOwnership(Context.Player.IdentityId, MyOwnershipShareModeEnum.Faction);
                        foreach (MySlimBlock block in grid.GetBlocks())
                        {

                            MyMultiplayer.RaiseEvent(grid, x => new Action<long, long>(x.TransferBlocksBuiltByID), block.BuiltBy, Context.Player.IdentityId, new EndpointId());
                        }
                    }
                    EconUtils.TakeMoney(Context.Player.IdentityId, offer.price);
                    EconUtils.AddMoney(offer.SellerIdentityId, offer.price);
                    SendMessage("The Government", "Player accepted your offer to sell grid.", Color.Red, offer.SellerSteamId);
                }
                else
                {
                    Context.Respond("You cannot afford to accept this grid.");
                }
            }
            else
            {
                Context.Respond("You have no offers available to accept");
            }
        }

        [Command("fixrespawn", "remove the respawnship status")]
        [Permission(MyPromoteLevel.None)]
        public void Norespawn()
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

            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
            bool found = false;
            if (gridWithSubGrids.Count > 0)
            {
                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;
                        grid.IsRespawnGrid = false;
                        found = true;

                        CrunchUtilitiesPlugin.Log.Info("Removing respawn status of grid " + grid.EntityId + " " + grid.DisplayName);
                        Context.Respond("Ship wont get deleted by Keen. Try not to die.");
                    }
                }
            }

            if (found)
            {
                currentCooldown = CreateNewCooldown(currentCooldownMap, Context.Player.IdentityId, plugin.CooldownRespawn);
                currentCooldown.StartCooldown(null);
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
                                        continue;
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
                                        continue;
                                    }
                                    try
                                    {
                                        isDynamic = true;
                                        if (grid.GridSizeEnum == MyCubeSize.Small)
                                        {
                                            break;
                                        }

                                        if (grid.Physics.Speed > 10)
                                        {
                                            Context.Respond(grid.DisplayName + " Moving too fast to convert.");
                                            continue;
                                        }

                                        grid.Physics.Clear();
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

        private long CountProjectionPCU(MyCubeGrid grid)
        {

            long pcu = 0;
            /*loop over the projectors in the grid */
            foreach (var projector in grid.GetFatBlocks().OfType<MyProjectorBase>())
            {
                /*if the projector isn't enabled, dont count its projected pcu*/

                if (!projector.Enabled)
                    continue;

                List<MyCubeGrid> grids = projector.Clipboard.PreviewGrids;
                /*count the blocks in the projected grid*/
                foreach (MyCubeGrid CubeGrid in grids)
                    pcu += CubeGrid.CubeBlocks.Count;

            }

            return pcu;
        }


        [Command("pcucount", "Player command, count PCU of connected grids")]
        [Permission(MyPromoteLevel.None)]
        public void CountPCU()
        {
            long totalPCU = 0;
            long projection = 0;
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

                        projection += CountProjectionPCU(grid);
                        totalPCU -= CountProjectionPCU(grid);
                    }
                }
            }
            else
            {
                Context.Respond("Cant find a grid");
            }
            //sb.Append("Total : " + totalPCU);
            Context.Respond(sb.ToString(), "PCU");

            if (projection > 0 && CrunchUtilitiesPlugin.file.PcuCountShowProjPCU)
            {
                Context.Respond((totalPCU + projection).ToString(), "Grid and Projection PCU");
            }
            else
            {
                Context.Respond(totalPCU.ToString(), "Grid PCU");
            }
        }

        [Command("fillhydro", "admin command, fill hydro tanks")]
        [Permission(MyPromoteLevel.Admin)]
        public void FillTank(string percent = "100")
        {

            bool changed = false;
            float fill = float.Parse(percent) / 100;
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
            foreach (var item in gridWithSubGrids)
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                {
                    MyCubeGrid grid = groupNodes.NodeData;

                    var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);

                    var blockList = new List<Sandbox.ModAPI.IMyGasTank>();
                    gts.GetBlocksOfType(blockList);
                    changed = true;

                    foreach (Sandbox.ModAPI.IMyGasTank tank in blockList)
                    {

                        tank.ChangeFilledRatio(fill, true);

                    }
                    Context.Respond(blockList.Count + " Tanks filled");


                }
            }
            if (!changed)
            {
                Context.Respond("Couldnt find that grid, are you looking at it?");
            }


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

        [Command("getsteamid", "Get a specific identities steam name")]
        [Permission(MyPromoteLevel.None)]
        public void GetSTEAMID(string target, bool online = false)
        {
            bool console = false;
            if (Context.Player == null)
            {
                console = true;
            }
            if (online)
            {
                Dictionary<string, string> badNames = new Dictionary<string, string>();
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
                List<IMyIdentity> identities = CrunchUtilitiesPlugin.GetAllIdentitiesByNameOrId(target);
                if (identities == null || identities.Count == 0)
                {
                    Context.Respond("Couldnt find that player.");
                    return;
                }

                foreach (var id in identities)
                {
                    string name = MyMultiplayer.Static.GetMemberName(MySession.Static.Players.TryGetSteamId(id.IdentityId));
                    Context.Respond("\n Steam Name " + name + "\n Display Name " + id.DisplayName + "\n STEAM " + MySession.Static.Players.TryGetSteamId(id.IdentityId).ToString() + "\n IDENTITY " + id.IdentityId);
                }

            }
        }

        [Command("fixallstations", "check every trade station for duplicates and delete the duplicates")]
        [Permission(MyPromoteLevel.Admin)]
        public void FixAllStations()
        {
            //loop through every factions trade stations to check for duplicates
            foreach (KeyValuePair<long, MyFaction> keyValuePair in MySession.Static.Factions)
            {
                foreach (MyFactionStation myStation in keyValuePair.Value.Stations)
                {

                    //check the entities near these locations for duplicates
                    List<VRage.ModAPI.IMyEntity> l = new List<VRage.ModAPI.IMyEntity>();

                    BoundingSphereD sphere = new BoundingSphereD(myStation.Position, 250);
                    l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);

                    List<VRage.Game.ModAPI.IMyCubeGrid> NPCGrids = new List<VRage.Game.ModAPI.IMyCubeGrid>();
                    foreach (IMyEntity e in l)
                    {
                        if (e is VRage.Game.ModAPI.IMyCubeGrid grid)
                        {

                            //dont want to delete any station that keen sees as a trade station
                            if (FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)) != null)
                            {
                                if (grid != null && grid.EntityId != myStation.StationEntityId && FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)).Tag.Length > 3 && MySession.Static.Factions.GetStationByGridId(grid.EntityId) == null)
                                {
                                    //If they have a store delete them
                                    var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                                    var blockList = new List<Sandbox.ModAPI.IMyStoreBlock>();
                                    gts.GetBlocksOfType(blockList);
                                    if (blockList.Count > 0)
                                    {
                                        NPCGrids.Add(grid);
                                    }

                                    //disconnect the connectors so ships dont get deleted
                                    var blockList2 = new List<Sandbox.ModAPI.IMyShipConnector>();
                                    gts.GetBlocksOfType(blockList2);
                                    foreach (Sandbox.ModAPI.IMyShipConnector con in blockList2)
                                    {
                                        con.Disconnect();

                                    }
                                }
                            }
                        }
                    }

                    foreach (MyCubeGrid grid in NPCGrids)
                    {
                        if (!FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid)).Tag.Equals("ACME") && !FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid)).Tag.Equals("UNIN") && !FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid)).Tag.Equals("FEDR") && !FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid)).Tag.Equals("CONS"))
                        {
                            grid.Close();
                        }
                    }
                }
            }
        }

        [Command("worldpcu", "output the worlds pcu")]
        [Permission(MyPromoteLevel.Admin)]
        public void Worldpcu()
        {
            Context.Respond("Total world PCU : " + MySession.Static.TotalSessionPCU);
        }

        [Command("zone", "edit safezone whitelist or blacklist")]
        [Permission(MyPromoteLevel.Admin)]
        public void EditZone(string addOrRemove, string playerOrFac, string nameOrTag)
        {

            if (!addOrRemove.ToLower().Contains("add") && !addOrRemove.ToLower().Contains("remove"))
            {
                Context.Respond("Couldnt read input! Use add or remove");
                return;
            }
            if (!playerOrFac.ToLower().Contains("player") && !playerOrFac.ToLower().Contains("fac"))
            {
                Context.Respond("Couldnt read input! Use player or fac");
                return;
            }
            bool isAdding = false;
            if (addOrRemove.ToLower().Contains("add"))
            {
                isAdding = true;
            }
            BoundingSphereD sphere = new BoundingSphereD(Context.Player.Character.PositionComp.GetPosition(), 500);
            var l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
            MySafeZone zone = null;

            foreach (IMyEntity e in l)
            {
                if (e is MySafeZone z)
                {
                    zone = z;
                    // CrunchUtilitiesPlugin.Log.Info("Zone");
                    break;
                }
            }
            if (zone == null)
            {
                Context.Respond("Cannot find a safezone, are you in one?");
                return;
            }
            List<long> players = zone.Players;

            List<MyFaction> factions = zone.Factions;

            switch (playerOrFac.ToLower())
            {
                case "player":
                    MyIdentity player = CrunchUtilitiesPlugin.GetIdentityByNameOrId(nameOrTag);
                    if (player == null)
                    {
                        Context.Respond("Cant find that player :(");
                        return;
                    }
                    if (isAdding)
                    {
                        if (players.Contains(player.IdentityId))
                        {
                            players.Remove(player.IdentityId);
                        }

                        players.Add(player.IdentityId);
                        Context.Respond("Added");
                    }
                    else
                    {
                        players.Remove(player.IdentityId);
                        Context.Respond("Removed");
                    }

                    zone.Players = players;
                    MySessionComponentSafeZones.UpdateSafeZone((MyObjectBuilder_SafeZone)zone.GetObjectBuilder(), true);


                    break;
                case "fac":
                    MyFaction faction = MySession.Static.Factions.TryGetFactionByTag(nameOrTag);
                    if (faction == null)
                    {
                        Context.Respond("Cant find that faction :(");
                        return;
                    }
                    if (isAdding)
                    {

                        if (factions.Contains(faction))
                        {
                            factions.Remove(faction);
                        }
                        factions.Add(faction);
                        Context.Respond("Added");
                    }
                    else
                    {
                        if (factions.Contains(faction))
                        {
                            factions.Remove(faction);
                        }
                        Context.Respond("Removed");
                    }

                    zone.Factions = factions;
                    // MyMultiplayer.RaiseStaticEvent<MyObjectBuilder_SafeZone>((Func<IMyEventOwner, Action<MyObjectBuilder_SafeZone>>)(x => new Action<MyObjectBuilder_SafeZone>(MySessionComponentSafeZones.UpdateSafeZone_Broadcast)), zone, new EndpointId(), new Vector3D?());
                    //  MySessionComponentSafeZones.UpdateSafeZone((MyObjectBuilder_SafeZone)zone.GetObjectBuilder(), true);
                    MySessionComponentSafeZones.RequestUpdateSafeZone((MyObjectBuilder_SafeZone)zone.GetObjectBuilder());

                    break;
            }
        }

        [Command("fixstation", "delete duplicate npc stations around player location")]
        [Permission(MyPromoteLevel.None)]
        public void FixNearbyStations()
        {
            if (CrunchUtilitiesPlugin.file.FixTradeStation)
            {
                BoundingSphereD sphere = new BoundingSphereD(Context.Player.Character.PositionComp.GetPosition(), 500);
                List<VRage.ModAPI.IMyEntity> l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
                MySafeZone zone = null;
                List<VRage.Game.ModAPI.IMyCubeGrid> NPCGrids = new List<VRage.Game.ModAPI.IMyCubeGrid>();
                List<VRage.Game.ModAPI.IMyCubeGrid> GridsOutsideZone = new List<VRage.Game.ModAPI.IMyCubeGrid>();
                foreach (IMyEntity e in l)
                {
                    if (e is MySafeZone z)
                    {
                        zone = z;
                        // CrunchUtilitiesPlugin.Log.Info("Zone");
                        break;
                    }
                }

                foreach (IMyEntity e in l)
                {
                    if (e is MyCubeGrid grid)
                    {
                        if (FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid)) != null)
                        {
                            if (grid != null && FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid)).Tag.Length > 3)
                            {
                                var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                                var blockList = new List<Sandbox.ModAPI.IMyStoreBlock>();
                                gts.GetBlocksOfType(blockList);

                                if (blockList.Count > 0)
                                    NPCGrids.Add(grid);
                            }
                        }
                    }
                }

                if (zone == null)
                {
                    foreach (VRage.Game.ModAPI.IMyCubeGrid grid in NPCGrids)
                    {
                        if (!FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)).Tag.Equals("ACME") && !FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)).Tag.Equals("UNIN") && !FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)).Tag.Equals("FEDR") && !FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)).Tag.Equals("CONS"))
                        {
                            grid.Close();
                        }
                    }
                    Context.Respond("Deleted all of them.");
                    return;
                }

                VRage.Game.ModAPI.IMyCubeGrid original = null;

                foreach (VRage.Game.ModAPI.IMyCubeGrid grid in NPCGrids)
                {
                    if (MySession.Static.Factions.GetStationByGridId(grid.EntityId) != null && grid.PositionComp.GetPosition() == zone.PositionComp.GetPosition())
                    {
                        CrunchUtilitiesPlugin.Log.Info("Found the original");
                        original = grid;
                    }
                }

                if (original == null)
                {
                    Context.Respond("Cant find the original station, report in discord.");
                    return;
                }

                string deletedGrids = "";
                NPCGrids.Remove(original);
                if (NPCGrids.Count > 0)
                {
                    if (confirmations.ContainsKey(Context.Player.Identity.IdentityId))
                    {


                        confirmations.TryGetValue(Context.Player.Identity.IdentityId, out long time);

                        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() <= time)
                        {
                            foreach (VRage.Game.ModAPI.IMyCubeGrid grid in NPCGrids)
                            {
                                if (FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)).Tag.Length > 3)
                                {




                                    if (FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)).Tag != "ACME" && FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)).Tag != "UNIN" && FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)).Tag != "FEDR" && FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid as MyCubeGrid)).Tag != "CONS")
                                    {
                                        grid.Close();
                                    }


                                    confirmations.Remove(Context.Player.Identity.IdentityId);
                                    deletedGrids += grid.DisplayName + " ";
                                    Context.Respond("Deleting a grid.");

                                }
                            }

                        }
                        else
                        {
                            Context.Respond("Time ran out, use !fixstation again");
                            confirmations.Remove(Context.Player.IdentityId);
                        }
                    }
                    else
                    {
                        long timeToAdd = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 20000);

                        confirmations.Add(Context.Player.Identity.IdentityId, timeToAdd);
                        Context.Respond("Ensure you have no grids connected to the station and use !fixstation again within 20 seconds.");
                    }
                }
                else
                {
                    Context.Respond("Cannot find a duplicate station.");
                }
                CrunchUtilitiesPlugin.Log.Info("Deleted these grids in !fixstation " + deletedGrids);
            }

            else
            {
                Context.Respond("Command not enabled.");
            }
        }

        [Command("fac search", "shouldnt be required but keen wont add a search bar")]
        [Permission(MyPromoteLevel.None)]
        public void SearchAllFactions(string name)
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
                else
                {
                    if (facs.Value != null && facs.Value.Tag != null && facs.Value.Tag.ToLower().Contains(name.ToLower()))
                    {
                        count++;
                        sb.Append("\n" + facs.Value.Name + " - [" + facs.Value.Tag + "]");
                    }
                }
            }



            if (!console)
            {
                DialogMessage m = new DialogMessage("Search for '" + name.ToLower() + "'", count + " Results", "For detailed information use !fac info TAG, the tag is the letters between the [] \n" + sb.ToString());
                ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);

            }
            else
            {
                Context.Respond("Tags of online players", sb.ToString());
            }

        }
        [Command("tags", "show players faction tags")]
        [Permission(MyPromoteLevel.None)]
        public void ShowFactionTags()
        {
            bool console = false;
            if (Context.Player == null)
            {
                console = true;
            }
            Dictionary<string, string> tagsAndNames = new Dictionary<string, string>();
            Dictionary<string, string> friends = new Dictionary<string, string>();
            Dictionary<string, string> neutrals = new Dictionary<string, string>();

            //SPAGHETTI AF 
            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
            {
                if (FacUtils.GetPlayersFaction(player.Identity.IdentityId) != null)
                {
                    if (!console)
                    {
                        IMyFaction playerFac = null;
                        if (FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId) != null)
                        {

                            playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
                        }
                        if (playerFac == null)
                        {
                            Context.Respond("Make a faction. This command does not work without being in a faction.");
                            return;
                        }
                        if (MySession.Static.Factions.AreFactionsFriends(playerFac.FactionId, FacUtils.GetPlayersFaction(player.Identity.IdentityId).FactionId))
                        {
                            if (friends.ContainsKey(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag))
                            {
                                friends.TryGetValue(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, out string temp);


                                if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                {
                                    temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                }
                                else
                                {
                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                    }

                                    else
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                    }
                                }
                                friends.Remove(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag);
                                friends.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                            }
                            else
                            {
                                string temp = "";
                                if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                {
                                    temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                }
                                else
                                {
                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                    }

                                    else
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                    }
                                }

                                friends.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                            }
                        }
                        else
                        {
                            if (MySession.Static.Factions.AreFactionsEnemies(playerFac.FactionId, FacUtils.GetPlayersFaction(player.Identity.IdentityId).FactionId))
                            {
                                if (tagsAndNames.ContainsKey(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag))
                                {
                                    tagsAndNames.TryGetValue(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, out string temp);


                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                    }
                                    else
                                    {
                                        if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                        }

                                        else
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                        }
                                    }
                                    tagsAndNames.Remove(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag);
                                    tagsAndNames.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                                }
                                else
                                {
                                    string temp = "";
                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                    }
                                    else
                                    {
                                        if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                        }

                                        else
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                        }
                                    }

                                    tagsAndNames.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                                }
                            }
                            else
                            {
                                if (neutrals.ContainsKey(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag))
                                {
                                    neutrals.TryGetValue(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, out string temp);


                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                    }
                                    else
                                    {
                                        if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                        }

                                        else
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                        }
                                    }
                                    neutrals.Remove(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag);
                                    neutrals.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                                }
                                else
                                {
                                    string temp = "";
                                    if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                                    {
                                        temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                                    }
                                    else
                                    {
                                        if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                        }

                                        else
                                        {
                                            temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                        }
                                    }

                                    neutrals.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                                }
                            }
                        }



                    }
                    else
                    {


                        if (tagsAndNames.ContainsKey(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag))
                        {
                            tagsAndNames.TryGetValue(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, out string temp);


                            if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                            {
                                temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                            }
                            else
                            {
                                if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                {
                                    temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                }

                                else
                                {
                                    temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                }
                            }
                            tagsAndNames.Remove(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag);
                            tagsAndNames.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                        }
                        else
                        {
                            string temp = "";
                            if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsFounder(player.Identity.IdentityId))
                            {
                                temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Founder)";
                            }
                            else
                            {
                                if (FacUtils.GetPlayersFaction(player.Identity.IdentityId).IsLeader(player.Identity.IdentityId))
                                {
                                    temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName + " (Leader)";
                                }

                                else
                                {
                                    temp += "\n [" + FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag + "] - " + player.DisplayName;
                                }
                            }

                            tagsAndNames.Add(FacUtils.GetPlayersFaction(player.Identity.IdentityId).Tag, temp);
                        }
                    }
                }


            }
            var sb = new StringBuilder();

            sb.Append("\n At War");
            foreach (KeyValuePair<string, string> keys in tagsAndNames)
            {

                sb.Append(keys.Value);

            }
            sb.Append("\n ");
            sb.Append("\n Friends");
            foreach (KeyValuePair<string, string> keys in friends)
            {

                sb.Append(keys.Value);

            }
            sb.Append("\n ");
            sb.Append("\n Neutral");
            foreach (KeyValuePair<string, string> keys in neutrals)
            {

                sb.Append(keys.Value);

            }
            if (!console)
            {
                DialogMessage m = new DialogMessage("Tags of online players", "", sb.ToString());
                ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);
            }
            else
            {
                Context.Respond("Tags of online players", sb.ToString());
            }
        }

        [Command("listids", "Lists players steam IDs")]
        [Permission(MyPromoteLevel.None)]
        public void ListSteamIDs()
        {
            bool console = false;
            if (Context.Player == null)
            {
                console = true;
            }
            Dictionary<string, string> badNames = new Dictionary<string, string>();
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
        public void ListNames()
        {
            bool console = false;
            if (Context.Player == null)
            {
                console = true;
            }
            Dictionary<string, string> badNames = new Dictionary<string, string>();
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
        public void UpdateIdentities(string playerNameOrId, string newName)
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
                    Context.Respond(id.DisplayName + " Player Balance : " + string.Format("{0:n0}", EconUtils.GetBalance(id.IdentityId)));


                    break;
                case "faction":
                    IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(target);
                    if (fac == null)
                    {
                        Context.Respond("Cant find that faction");
                        return;
                    }

                    Context.Respond(fac.Name + " Faction Balance : " + string.Format("{0:n0}", EconUtils.GetBalance(fac.FactionId)));

                    break;

                default:
                    Context.Respond("Incorrect usage, example - !eco balance player PlayerName or !eco balance faction tag");
                    break;


            }
        }

        [Command("lastlogin", "output when a user last logged in")]
        [Permission(MyPromoteLevel.Admin)]
        public void CheckLastLogin(string name)
        {
            MyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(name);
            if (id == null)
            {
                Context.Respond("Cant find that player.");
                return;
            }
            Context.Respond(id.LastLoginTime.ToString());
        }

        [Command("eco top", "moneys")]
        [Permission(MyPromoteLevel.Admin)]
        public void Ecotop(int limit = 30, bool factions = false)
        {
            //essentials eco stuff but with factions and formatting for the numbers
            StringBuilder data = new StringBuilder();
            StringBuilder data2 = new StringBuilder();

            int iteration = 0;
            if (factions == false)
            {
                Dictionary<ulong, long> moneys = new Dictionary<ulong, long>();
                foreach (var p in MySession.Static.Players.GetAllPlayers())
                {
                    long IdentityID = MySession.Static.Players.TryGetIdentityId(p.SteamId);
                    if (!moneys.ContainsKey(p.SteamId))
                    {
                        moneys.Add(p.SteamId, EconUtils.GetBalance(IdentityID));
                    }
                    else
                    {
                        moneys[p.SteamId] += EconUtils.GetBalance(IdentityID);
                    }
                }
                var sortedmoneys = moneys.OrderByDescending(x => x.Value).ThenBy(x => x.Key);

                foreach (var value in sortedmoneys)
                {
                    if (iteration <= limit)
                    {
                        iteration++;
                        data.AppendLine(MySession.Static.Players.TryGetIdentityNameFromSteamId(value.Key).ToString() + " - Balance: " + string.Format("{0:n0}", value.Value));
                        CrunchUtilitiesPlugin.Log.Info(MySession.Static.Players.TryGetIdentityNameFromSteamId(value.Key).ToString() + " - Balance: " + string.Format("{0:n0}", value.Value));
                        data2.AppendLine(MySession.Static.Players.TryGetIdentityNameFromSteamId(value.Key).ToString() + "," + value.Value);
                    }
                    else
                    {
                        break;
                    }
                }
                //   MyReactor reactor;
                // reactor.BlockDefinition.FuelProductionToCapacityMultiplier
                File.WriteAllText(CrunchUtilitiesPlugin.path.Replace("\\Instance", "\\Logs") + "//eco.csv", data2.ToString());
                File.WriteAllText(CrunchUtilitiesPlugin.path.Replace("\\Instance", "\\Logs") + "//eco-" + string.Format("{0:yyyy-MM-dd_HH-mm-ss-fff}", DateTime.Now) + ".csv", data2.ToString());
                if (Context.Player == null)
                {
                    Context.Respond("Top " + limit + " player balances\n" + data.ToString());
                    return;
                }
                ModCommunication.SendMessageTo(new DialogMessage("Top " + limit + " Player Balances", "", data.ToString()), Context.Player.SteamUserId);
            }
            else
            {
                Dictionary<string, long> moneys = new Dictionary<string, long>();
                foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
                {
                    if (f.Value.Tag.Length > 3)
                    {
                        if (f.Value.Tag.ToLower().Equals("unin") || f.Value.Tag.ToLower().Equals("fedr") || f.Value.Tag.ToLower().Equals("cons"))
                        {
                            moneys.Add(f.Value.Name + " - " + f.Value.Tag, EconUtils.GetBalance(f.Value.FactionId));
                        }
                    }
                    else
                    {
                        moneys.Add(f.Value.Name + " - " + f.Value.Tag, EconUtils.GetBalance(f.Value.FactionId));
                    }


                }
                var sortedmoneys = moneys.OrderByDescending(x => x.Value).ThenBy(x => x.Key);
                foreach (var value in sortedmoneys)
                {
                    if (iteration <= limit)
                    {

                        iteration++;
                        data.AppendLine(value.Key + " - Balance: " + string.Format("{0:n0}", value.Value));
                        CrunchUtilitiesPlugin.Log.Info(value.Key + " - Balance: " + string.Format("{0:n0}", value.Value));
                        data2.AppendLine(value.Key + value.Key + "," + value.Value);
                    }
                    else
                    {
                        break;
                    }
                }
                File.WriteAllText(CrunchUtilitiesPlugin.path.Replace("\\Instance", "\\Logs") + "//ecofac.csv", data2.ToString());

                File.WriteAllText(CrunchUtilitiesPlugin.path.Replace("\\Instance", "\\Logs") + "//ecofac-" + string.Format("{0:yyyy-MM-dd_HH-mm-ss-fff}", DateTime.Now) + ".csv", data2.ToString());


                if (Context.Player == null)
                {
                    Context.Respond("Top " + limit + " faction balances\n" + data.ToString());
                    return;
                }
                ModCommunication.SendMessageTo(new DialogMessage("Top " + limit + " Faction Balances", "", data.ToString()), Context.Player.SteamUserId);
            }

        }

        [Command("faction rep", "check rep between factions")]
        [Permission(MyPromoteLevel.None)]
        public void RepCheck(string tag1, string tag2)
        {
            WarStatus(tag1, tag2);
        }

        [Command("warstatus", "check war status")]
        [Permission(MyPromoteLevel.None)]
        public void WarStatus(string tag1, string tag2)
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

            var repValue = MySession.Static.Factions.GetRelationBetweenFactions(fac1.FactionId, fac2.FactionId);
            Context.Respond($"Reputation: {repValue.Item2}");
            if (MySession.Static.Factions.AreFactionsNeutrals(fac1.FactionId, fac2.FactionId))
            {
                Context.Respond(fac1.Name + " " + fac1.Tag + " are Neutral with " + fac2.Name + " " + fac2.Tag);
                return;
            }
            if (MySession.Static.Factions.AreFactionsFriends(fac1.FactionId, fac2.FactionId))
            {
                Context.Respond(fac1.Name + " " + fac1.Tag + " are Friends with " + fac2.Name + " " + fac2.Tag);
                return;
            }
            else
            {
                Context.Respond(fac1.Name + " " + fac1.Tag + " are at war with " + fac2.Name + " " + fac2.Tag);
                return;
            }
        }

        [Command("declarewar", "declare war")]
        [Permission(MyPromoteLevel.None)]
        public void WarStatus(string tag)
        {
            if (CrunchUtilitiesPlugin.file.facInfo)
            {
                IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (fac == null)
                {
                    if (!(Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(tag) is MyPlayer player))
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
                    MyFactionCollection.DeclareWar(playerFac.FactionId, fac.FactionId);
                    Context.Respond("War were declared.");
                    if (fac.Tag.Length > 3)
                    {
                        foreach (KeyValuePair<long, MyFactionMember> m in playerFac.Members)
                        {

                            MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, fac.FactionId, -3000, ReputationChangeReason.Admin);

                            MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, fac.FactionId, 0, ReputationChangeReason.Admin);

                        }
                    }
                    else
                    {
                        MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, fac.FactionId, -3000);
                    }
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

        [Command("nofriendforyou", "make a faction declare war on everyone")]
        [Permission(MyPromoteLevel.Admin)]
        public void Declarewaronall(string tag)
        {
            if (CrunchUtilitiesPlugin.file.facInfo)
            {
                IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (fac == null)
                {
                    if (!(Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(tag) is MyPlayer player))
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
                        MyFactionCollection.DeclareWar(fac.FactionId, f.Value.FactionId);
                    }
                }
                Context.Respond("That faction has now declared war on all factions");
            }
            else
            {
                Context.Respond("Fac info not enabled");
            }
        }

        [Command("sendpeace", "send a peace request")]
        [Permission(MyPromoteLevel.None)]
        public void SendPeaceRequest(string tag)
        {
            if (CrunchUtilitiesPlugin.file.facInfo)
            {
                IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (fac == null)
                {
                    if (!(Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(tag) is MyPlayer player))
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
                        MyFactionCollection.SendPeaceRequest(playerFac.FactionId, fac.FactionId);

                    }
                    if (state == MyFactionPeaceRequestState.Pending)
                    {
                        AcceptPeace(playerFac.FactionId, fac.FactionId);
                        if (fac.Tag.Length > 3)
                        {
                            foreach (KeyValuePair<long, MyFactionMember> m in playerFac.Members)
                            {

                                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, fac.FactionId, 0, ReputationChangeReason.Admin);

                                MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, fac.FactionId, 0, ReputationChangeReason.Admin);

                            }
                        }
                        else
                        {
                            MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, fac.FactionId, 0);
                        }

                    }
                    MyFactionCollection.SendPeaceRequest(playerFac.FactionId, fac.FactionId);
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

        private string GetRandomStationName(MyStationsListDefinition stationsListDef)
        {
            if (stationsListDef == null)
                return "Economy_SpaceStation_1";
            int index = MyRandom.Instance.Next(0, stationsListDef.StationNames.Count);
            return stationsListDef.StationNames[index].ToString();
        }

        [Command("place station", "place an npc economy station, types are SpaceStations, OrbitalStations, Outposts or MiningStation")]
        [Permission(MyPromoteLevel.Admin)]
        public void PlaceStation(string npctag, string type)
        {
            if (!(MySession.Static.Factions.TryGetFactionByTag(npctag) is MyFaction npcfac))
            {
                Context.Respond("Cant find faction.");
                return;
            }

            if (Context.Player == null)
            {
                Context.Respond("Command has to be ran from ingame.");
                return;
            }

            MyDefinitionId subtypeId;
            MyStationTypeEnum stationType;

            switch (type.ToLower())
            {
                case "spacestations":
                    subtypeId = new MyDefinitionId(typeof(MyObjectBuilder_StationsListDefinition), "SpaceStations");
                    stationType = MyStationTypeEnum.SpaceStation;
                    break;
                case "orbitalstations":
                    subtypeId = new MyDefinitionId(typeof(MyObjectBuilder_StationsListDefinition), "OrbitalStations");
                    stationType = MyStationTypeEnum.OrbitalStation;
                    break;
                case "outposts":
                    subtypeId = new MyDefinitionId(typeof(MyObjectBuilder_StationsListDefinition), "Outposts");
                    stationType = MyStationTypeEnum.Outpost;
                    break;
                case "miningstations":
                    subtypeId = new MyDefinitionId(typeof(MyObjectBuilder_StationsListDefinition), "MiningStations");
                    stationType = MyStationTypeEnum.MiningStation;
                    break;
                default:
                    Context.Respond("Cannot find that type, use SpaceStations, OrbitalStations, Outposts or OrbitalStations");
                    return;
            }

            MyStationsListDefinition stationDefinition = MyDefinitionManager.Static.GetDefinition<MyStationsListDefinition>(subtypeId);
            Vector3 position = Context.Player.Character.PositionComp.GetPosition();
            var stationName = GetRandomStationName(stationDefinition);
            MyFactionStation station = new MyFactionStation(MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.STATION, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM), stationName ,position, stationType, npcfac, stationName, stationDefinition.GeneratedItemsContainerType);

            npcfac.AddStation(station);
        }

        [Command("resetnpcrep", "reset rep for all members")]
        [Permission(MyPromoteLevel.Admin)]
        public void RepReset(string factiontag, string npctag)
        {
            IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(factiontag);
            IMyFaction npcfac = MySession.Static.Factions.TryGetFactionByTag(npctag);
            if (fac == null)
            {
                Context.Respond("Cant find faction.");
                return;
            }

            if (npcfac == null)
            {
                Context.Respond("Cant find npc faction.");
                return;
            }
            foreach (KeyValuePair<long, MyFactionMember> m in fac.Members)
            {
                Tuple<MyRelationsBetweenFactions, int> rep = MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(m.Value.PlayerId, npcfac.FactionId);
                MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, npcfac.FactionId, 0);
                if (rep.Item2 < 0)
                {
                    MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, npcfac.FactionId, 0, ReputationChangeReason.Admin);

                    MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, npcfac.FactionId, 0, ReputationChangeReason.Admin);
                }
            }
            Context.Respond("Done!");

        }
        [Command("resetallrep", "reset rep for all players")]
        [Permission(MyPromoteLevel.Admin)]
        public void RepResetAll(string npctag)
        {
            IMyFaction npcfac = MySession.Static.Factions.TryGetFactionByTag(npctag);

            if (npcfac == null)
            {
                Context.Respond("Cant find npc faction.");
                return;
            }
            foreach (MyIdentity id2 in MySession.Static.Players.GetAllIdentities())
            {
                if (MySession.Static.Players.IdentityIsNpc(id2.IdentityId))
                    continue;

                if (FacUtils.GetPlayersFaction(id2.IdentityId) != null)
                {
                    MySession.Static.Factions.SetReputationBetweenFactions(FacUtils.GetPlayersFaction(id2.IdentityId).FactionId, npcfac.FactionId, 0);
                }
                Tuple<MyRelationsBetweenFactions, int> rep = MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(id2.IdentityId, npcfac.FactionId);
                if (rep.Item2 < 0)
                {
                    MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id2.IdentityId, npcfac.FactionId, 0, ReputationChangeReason.Admin);
                    MySession.Static.Factions.AddFactionPlayerReputation(id2.IdentityId, npcfac.FactionId, 0, ReputationChangeReason.Admin);
                }
            }


            Context.Respond("Done!");

        }

        internal static MethodInfo _factionChangeSuccessInfo = typeof(MyFactionCollection).GetMethod("FactionStateChangeSuccess", BindingFlags.NonPublic | BindingFlags.Static);

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
                IMyFaction fac = null;
                try
                {
                    fac = MySession.Static.Factions.TryGetFactionById(long.Parse(tag));
                }
                catch (Exception)
                {
                }

                if (fac == null)
                {
                    fac = MySession.Static.Factions.TryGetFactionByTag(tag);
                    if (fac == null)
                    {
                        if (!(Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(tag) is MyPlayer player))
                        {
                            IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(tag);
                            if (id == null)
                            {
                                Context.Respond("Cant find that faction or player.");
                                return;
                            }

                            fac = FacUtils.GetPlayersFaction(id.IdentityId);
                            if (fac == null)
                            {
                                Context.Respond("The player that was found does not have a faction.");
                                return;
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
                }
                string warstatus = "Relationship : ";
                if (Context.Player != null)
                {
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
                        warstatus += "At War";
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
                            if (FacUtils.GetPlayersFaction(test.IdentityId).IsFounder(test.IdentityId))
                            {
                                sb.Append("\n " + test.DisplayName + " (Founder)");
                            }
                            else
                            {
                                if (FacUtils.GetPlayersFaction(test.IdentityId).IsLeader(test.IdentityId))
                                {
                                    sb.Append("\n " + test.DisplayName + " (Leader)");
                                }

                                else
                                {
                                    sb.Append("\n " + test.DisplayName);
                                }
                            }


                        }
                    }
                    if (!console)
                    {
                        DialogMessage m = new DialogMessage("Faction Info", fac.Name + " " + fac.FactionId, "\nTag: " + fac.Tag + "\n" + warstatus + "\nDescription: " + fac.Description + "\nMembers: " + fac.Members.Count + "\n" + sb.ToString());
                        ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);
                    }
                    else
                    {
                        Context.Respond("Name: " + fac.Name + " " + fac.FactionId + "\nTag: " + fac.Tag + "\n" + warstatus + "\nDescription: " + fac.Description + "\nMembers: " + fac.Members.Count + "\n" + sb.ToString());
                    }
                }
            }
            else
            {
                Context.Respond("Fac info not enabled");
            }
        }


        [Command("eco deposit", "Deposit moneys")]
        [Permission(MyPromoteLevel.None)]
        public void PlayerDeposit(bool playerOwned = false)
        {
            if (CrunchUtilitiesPlugin.file.Deposit)
            {
                IMyPlayer player = Context.Player;
                MyFixedPoint deposited = 0;

                if (player == null)
                    Context.Respond("Console cant withdraw money.....");

                string gridname = "";
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(player.Character);

                if (gridWithSubGrids.Count < 1)
                {
                    Context.Respond("Couldnt find a grid");
                    return;
                }

                //dpeosit from player inventory
                MyInventory inventory = (MyInventory)player.Character.GetInventory();
                List<MyPhysicalInventoryItem> CreditsinPlayerInvList = new List<MyPhysicalInventoryItem>();

                foreach (var Item in inventory.GetItems())
                {
                    if (Item.Content.SubtypeId.ToString() == "SpaceCredit")
                        CreditsinPlayerInvList.Add(Item);
                }

                foreach (var PlayerItem in CreditsinPlayerInvList)
                {
                    var amount = PlayerItem.Amount;

                    if (amount >= int.MaxValue)
                    {
                        bool hasCredits = true;
                        while (hasCredits)
                        {
                            deposited += amount;

                            player.Character.GetInventory().RemoveItemAmount(PlayerItem, amount);

                            MyInventory inventory2 = (MyInventory)player.Character.GetInventory();

                            if (!inventory2.GetItems().Contains(PlayerItem))
                                hasCredits = false;
                        }
                        EconUtils.AddMoney(player.IdentityId, (long)amount);
                    }
                    else
                    {
                        deposited += PlayerItem.Amount;
                        EconUtils.AddMoney(player.IdentityId, (long)PlayerItem.Amount);
                        player.Character.GetInventory().RemoveItemAmount(PlayerItem, amount);
                    }
                }

                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;

                        if (!FacUtils.IsOwnerOrFactionOwned(grid, player.IdentityId, true))
                            continue;
                        else
                        {
                            foreach (VRage.Game.ModAPI.IMySlimBlock block in grid.GetBlocks())
                            {
                                if (block?.FatBlock != null && block.FatBlock.HasInventory)
                                {
                                    bool owned = false;
                                    switch (block.FatBlock.GetUserRelationToOwner(player.IdentityId))
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
                                                owned = true;

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

                                    if (owned)
                                    {
                                        MyInventory inventory3 = (MyInventory)block.FatBlock.GetInventory();
                                        List<MyPhysicalInventoryItem> CreditsInCargoList = new List<MyPhysicalInventoryItem>();

                                        foreach (var Item in inventory3.GetItems())
                                        {
                                            if (Item.Content.SubtypeId.ToString() == "SpaceCredit")
                                                CreditsInCargoList.Add(Item);
                                        }

                                        foreach (var Item in CreditsInCargoList)
                                        {
                                            var amount = Item.Amount;

                                            if (amount >= int.MaxValue)
                                            {
                                                bool hasCredits = true;
                                                while (hasCredits)
                                                {
                                                    deposited += amount;

                                                    block.FatBlock.GetInventory().RemoveItemAmount(Item, amount);

                                                    MyInventory inventory4 = (MyInventory)block.FatBlock.GetInventory();

                                                    if (!inventory4.GetItems().Contains(Item))
                                                        hasCredits = false;
                                                }
                                                gridname += grid.DisplayName + ", ";
                                                EconUtils.AddMoney(player.IdentityId, (long)amount);
                                            }
                                            else
                                            {
                                                deposited += Item.Amount;
                                                EconUtils.AddMoney(player.IdentityId, (long)amount);
                                                block.FatBlock.GetInventory().RemoveItemAmount(Item, amount);
                                                gridname += grid.DisplayName + ", ";
                                            }
                                        }
                                     
                                    }
                                }
                            }
                        }
                    }
                }
                Context.Respond("Deposited : " + string.Format("{0:n0}", deposited));
                CrunchUtilitiesPlugin.Log.Info(player.SteamUserId + " deposited " + string.Format("{0:n0}", deposited) + " from " + gridname);
            }
            else
            {
                Context.Respond("Deposit not enabled.");
            }
        }

        [Command("eco withdraw", "Withdraw moneys")]
        [Permission(MyPromoteLevel.None)]
        public void PlayerWithdraw(long amount)
        {
            if (CrunchUtilitiesPlugin.file.Withdraw)
            {
                IMyPlayer player = Context.Player;

                if (player == null)
                {
                    Context.Respond("Console cant withdraw money.....");
                    return;
                }

                long balance;
                if (amount >= int.MaxValue)
                {
                    Context.Respond("Keen code doesnt allow stacks over 2.147 billion, try again with a smaller number");
                    return;
                }

                if (amount > CrunchUtilitiesPlugin.file.EcoWithdrawMax)
                {
                    Context.Respond("Cant withdraw over the maximum of " + string.Format("{0:n0}", CrunchUtilitiesPlugin.file.EcoWithdrawMax));
                    return;
                }

                balance = EconUtils.GetBalance(player.IdentityId);

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

                MyCargoContainer container = null;
                VRage.Game.ModAPI.IMyInventory invent = null;
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(player.Character);

                if (gridWithSubGrids.Count < 1)
                {
                    Context.Respond("Couldnt find a grid");
                    return;
                }

                MyItemType itemType = new MyInventoryItemFilter("MyObjectBuilder_PhysicalObject/SpaceCredit").ItemType;
                MyFixedPoint AmountFixed = (MyFixedPoint)(double)amount;

                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;

                        if (!FacUtils.IsOwnerOrFactionOwned(grid, player.IdentityId, true))
                            continue;
                        else
                        {
                            foreach (var block in grid.GetBlocks())
                            {
                                if (block?.FatBlock is MyCargoContainer cargo && cargo.IsFunctional)
                                {
                                    if (cargo.GetInventory().CanItemsBeAdded(AmountFixed, itemType))
                                    {
                                        switch (cargo.GetUserRelationToOwner(player.IdentityId))
                                        {
                                            case MyRelationsBetweenPlayerAndBlock.Owner:
                                                container = cargo;
                                                invent = cargo.GetInventory();
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.FactionShare:
                                                container = cargo;
                                                invent = cargo.GetInventory();
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.Neutral:
                                                container = cargo;
                                                invent = cargo.GetInventory();
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
                                return;
                            }
                        }
                    }

                    if (invent != null)
                    {
                        invent.AddItems(AmountFixed, new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
                        container.GetInventory().Refresh();
                        EconUtils.TakeMoney(player.IdentityId, (long)amount);

                        Context.Respond($"Added credits to {container.DisplayNameText} in grid {container.CubeGrid?.DisplayName}");
                        CrunchUtilitiesPlugin.Log.Info(player.SteamUserId + " withdrew " + string.Format("{0:n0}", amount) + " to " + container.CubeGrid?.DisplayName);
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
            if (amount > int.MaxValue)
            {
                Context.Respond("Amount is too high");
                return;
            }


            MyPlayer player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(PlayerName) as MyPlayer;
            if (player == null)
            {
                try
                {
                    player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerBySteamId(ulong.Parse(PlayerName)) as MyPlayer;
                }
                catch
                {
                    Context.Respond("Not a steam id or player name!");
                    return;
                }
                if (player == null)
                {
                    Context.Respond("Cant find that player");
                    return;
                }
            }

            if (!(player.Character.GetInventory() is MyInventory invent))
            {
                Context.Respond("Player has no character, cant give item.");
                return;
            }

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

            if (!force)
            {
                try
                {
                    MyVisualScriptLogicProvider.AddToPlayersInventory(player.Identity.IdentityId, id, amount);
                }
                catch (Exception ex)
                {
                    Context.Respond("Error, item not given");
                    CrunchUtilitiesPlugin.Log.Error(ex);
                    return;
                }

                Context.Respond("Added the items");
                SendMessage("[C]", "You were given " + amount + " " + subtypeName, Color.Green, (long)player.Id.SteamId);
                return;
            }
            else
            {
                MyObjectBuilder_PhysicalObject item;
                switch (type.ToLower())
                {
                    //Eventually add some checks to see if the item exists before adding it
                    case "object":
                        item = new MyObjectBuilder_PhysicalGunObject { SubtypeName = subtypeName };
                        break;
                    case "ammo":
                        item = new MyObjectBuilder_AmmoMagazine { SubtypeName = subtypeName };
                        break;
                    case "ore":
                        item = new MyObjectBuilder_Ore() { SubtypeName = subtypeName };
                        break;
                    case "ingot":
                        item = new MyObjectBuilder_Ingot() { SubtypeName = subtypeName };
                        break;
                    case "component":
                        item = new MyObjectBuilder_Component() { SubtypeName = subtypeName };
                        break;
                    case "physicalgunobject":
                        item = new MyObjectBuilder_PhysicalGunObject { SubtypeName = subtypeName };
                        break;
                    default:
                        Context.Respond("Error : use Object, Ammo, Ore, Ingot, Component, PhysicalGunObject");
                        return;
                }

                MethodInfo method = typeof(MyInventory).GetMethod("AddItemsInternal", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[7]
                {
                    typeof (MyFixedPoint), typeof (MyObjectBuilder_PhysicalObject), typeof (uint?), typeof (int), typeof(bool), typeof(int), typeof(int)
                },
                null);

                if (method == null)
                    throw new Exception("reflection error");

                MyFixedPoint AmountFixed = (MyFixedPoint)(double)amount;

                method.Invoke(invent, new object[7] { AmountFixed, item, new uint?(), -1, false, -1, 0 });
                //refresh this or buggy stuff happens
                invent.Refresh();
                SendMessage("[C]", "You were given " + amount + " " + subtypeName, Color.Green, (long)player.Id.SteamId);
                Context.Respond("items are added");
            }
        }

        [Command("broadcast", "Send a message in a noticable colour, false parameter will not show up in discord")]
        [Permission(MyPromoteLevel.Admin)]
        public void SendThisMessage(string message, string author = "Broadcast", int r = 50, int g = 168, int b = 168, bool global = true)
        {
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
                    Color col = new Color(r, g, b);
                    SendMessage(author, message, col, (long)player.Id.SteamId);
                }
            }
        }

        [Command("fac kick", "Admin command to take money")]
        [Permission(MyPromoteLevel.Admin)]
        public void KickMember(string tag, string target)
        {
            MyFaction faction = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (faction == null)
            {
                Context.Respond("Cant find that faction.");
                return;
            }
            MyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(target);
            if (id == null)
            {
                Context.Respond("Cant find that player.");
                return;
            }
            MyVisualScriptLogicProvider.KickPlayerFromFaction(id.IdentityId);

            Context.Respond("Kicked the player");
        }
        [Command("isnpc", "Admin command to see if an identity is an npc")]
        [Permission(MyPromoteLevel.Admin)]
        public void IsThisAnNPC(string target)
        {
            MyIdentity identity = CrunchUtilitiesPlugin.GetIdentityByNameOrId(target);
            if (identity == null)
            {
                Context.Respond("Couldnt find that identity");
                return;
            }
            Context.Respond(MySession.Static.Players.IdentityIsNpc(identity.IdentityId) + "");
        }


        [Command("eco take", "Admin command to take money")]
        [Permission(MyPromoteLevel.Admin)]
        public void TakeMoney(string type, string recipient, string inputAmount)
        {
            long amount;
            inputAmount = inputAmount.Replace(",", "");
            inputAmount = inputAmount.Replace(".", "");
            inputAmount = inputAmount.Replace(" ", "");
            try
            {
                amount = long.Parse(inputAmount);
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
                    if (EconUtils.GetBalance(id.IdentityId) >= amount)
                    {
                        Context.Respond(id.DisplayName + " Balance Before Change : " + string.Format("{0:n0}", EconUtils.GetBalance(id.IdentityId)));

                        EconUtils.TakeMoney(id.IdentityId, amount);

                        Context.Respond(id.DisplayName + " Balance After Change : " + string.Format("{0:n0}", EconUtils.GetBalance(id.IdentityId)));
                    }
                    else
                    {
                        Context.Respond("They cant afford that.");
                        Context.Respond(id.DisplayName + " Current Balance : " + string.Format("{0:n0}", EconUtils.GetBalance(id.IdentityId)));
                    }
                    break;
                case "faction":
                    IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(recipient);
                    if (fac == null)
                    {
                        Context.Respond("Cant find that faction");
                        return;
                    }
                    if (EconUtils.GetBalance(fac.FactionId) >= amount)
                    {
                        Context.Respond(fac.Name + " FACTION Balance Before Change : " + string.Format("{0:n0}", EconUtils.GetBalance(fac.FactionId)));
                        EconUtils.TakeMoney(fac.FactionId, amount);
                        Context.Respond(fac.Name + " FACTION Balance After Change : " + string.Format("{0:n0}", EconUtils.GetBalance(fac.FactionId)));
                    }
                    else
                    {
                        Context.Respond("They cant afford that.");
                        Context.Respond(fac.Name + " Current Balance : " + string.Format("{0:n0}", EconUtils.GetBalance(fac.FactionId)));
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
            long amount;
            inputAmount = inputAmount.Replace(",", "");
            inputAmount = inputAmount.Replace(".", "");
            inputAmount = inputAmount.Replace(" ", "");
            try
            {
                amount = long.Parse(inputAmount);
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
                            EconUtils.AddMoney(p.Identity.IdentityId, amount);

                        }
                        SendMessage(CrunchUtilitiesPlugin.file.EconName, "You got money : " + amount, Color.Cyan, 0);
                        return;
                    }
                    //Context.Respond("Error Player not online");
                    IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                    if (id == null)
                    {
                        Context.Respond("Cant find that player.");
                        return;
                    }
                    Context.Respond(id.DisplayName + " Balance Before Change : " + string.Format("{0:n0}", EconUtils.GetBalance(id.IdentityId)));

                    EconUtils.AddMoney(id.IdentityId, amount);

                    Context.Respond(id.DisplayName + " Balance After Change : " + string.Format("{0:n0}", EconUtils.GetBalance(id.IdentityId)));

                    break;
                case "faction":
                    IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(recipient);
                    if (fac == null)
                    {
                        Context.Respond("Cant find that faction");
                        return;
                    }
                    Context.Respond(fac.Name + " FACTION Balance Before Change : " + string.Format("{0:n0}", EconUtils.GetBalance(fac.FactionId)));
                    EconUtils.AddMoney(fac.FactionId, amount);
                    Context.Respond(fac.Name + " FACTION Balance After Change : " + string.Format("{0:n0}", EconUtils.GetBalance(fac.FactionId)));
                    break;

                default:
                    Context.Respond("Incorrect usage, example - !eco give player PlayerName amount or !eco give faction tag amount");
                    break;


            }
        }

        [Command("ez hide", "hide gps with names that contain the input")]
        [Permission(MyPromoteLevel.None)]
        public void Hidegps(string name)
        {
            List<IMyGps> gpsList = MyAPIGateway.Session?.GPS.GetGpsList(Context.Player.IdentityId);
            int count = 0;

            name = Context.RawArgs;
            if (name.Equals("all"))
            {
                foreach (IMyGps igps in gpsList)
                {
                    MyGps gps = igps as MyGps;

                    gps.ShowOnHud = false;
                    count++;
                    MyAPIGateway.Session?.GPS.RemoveGps(Context.Player.Identity.IdentityId, igps);
                    MyAPIGateway.Session?.GPS.AddGps(Context.Player.IdentityId, gps);
                }
            }
            else
            {
                foreach (IMyGps igps in gpsList)
                {
                    MyGps gps = igps as MyGps;
                    if (gps.Name.Contains(name))
                    {
                        gps.ShowOnHud = false;
                        count++;
                        MyAPIGateway.Session?.GPS.RemoveGps(Context.Player.Identity.IdentityId, igps);
                        MyAPIGateway.Session?.GPS.AddGps(Context.Player.IdentityId, gps);

                    }
                }
            }

            Context.Respond("Hiding " + count + " signals");
        }

        [Command("ez show", "show gps with names that contain the input")]
        [Permission(MyPromoteLevel.None)]
        public void Showgps(string name)
        {
            List<IMyGps> gpsList = MyAPIGateway.Session?.GPS.GetGpsList(Context.Player.IdentityId);
            int count = 0;
            name = Context.RawArgs;
            foreach (IMyGps igps in gpsList)
            {
                MyGps gps = igps as MyGps;
                if (gps.Name.Contains(name))
                {
                    gps.ShowOnHud = true;
                    count++;
                    MyAPIGateway.Session?.GPS.RemoveGps(Context.Player.Identity.IdentityId, igps);
                    MyAPIGateway.Session?.GPS.AddGps(Context.Player.IdentityId, gps);
                }
            }
            Context.Respond("Showing " + count + " signals");
        }

        [Command("ez delete", "delete gps with names that contain the input")]
        [Permission(MyPromoteLevel.None)]
        public void Deletegps(string name)
        {
            List<IMyGps> gpsList = MyAPIGateway.Session?.GPS.GetGpsList(Context.Player.IdentityId);
            int count = 0;
            name = Context.RawArgs;
            foreach (IMyGps igps in gpsList)
            {
                MyGps gps = igps as MyGps;
                if (gps.Name.Contains(name))
                {
                    gps.ShowOnHud = true;
                    count++;
                    MyAPIGateway.Session?.GPS.RemoveGps(Context.Player.Identity.IdentityId, igps);
                }
            }
            Context.Respond("Deleting " + count + " signals");
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
                long amount;
                inputAmount = inputAmount.Replace(",", "");
                inputAmount = inputAmount.Replace(".", "");
                inputAmount = inputAmount.Replace(" ", "");
                try
                {
                    amount = long.Parse(inputAmount);
                }
                catch (Exception)
                {
                    SendMessage(CrunchUtilitiesPlugin.file.EconName, "Error parsing amount", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }
                if (amount < 0 || amount == 0)
                {
                    SendMessage(CrunchUtilitiesPlugin.file.EconName, "Must be a positive number", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }
                type = type.ToLower();
                switch (type)
                {
                    case "player":
                        if (acrossInstances)
                        {
                            IMyIdentity targetid = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                            if (targetid == null)
                            {
                                SendMessage(CrunchUtilitiesPlugin.file.EconName, "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                                return;
                            }
                            if (EconUtils.GetBalance(Context.Player.IdentityId) >= amount)
                            {
                                EconUtils.TakeMoney(Context.Player.IdentityId, amount);
                                EconUtils.AddMoney(targetid.IdentityId, amount);


                                //SendMessage(CrunchUtilitiesPlugin.file.EconName, Context.Player.DisplayName + " Has sent you : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                                SendMessage(CrunchUtilitiesPlugin.file.EconName, "You sent " + targetid.DisplayName + " : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                            }
                            else
                            {
                                SendMessage(CrunchUtilitiesPlugin.file.EconName, "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                            }
                            return;
                        }

                        if (!(Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(recipient) is MyPlayer player))
                        {
                            SendMessage(CrunchUtilitiesPlugin.file.EconName, "They arent online, trying across instance", Color.Red, (long)Context.Player.SteamUserId);
                            IMyIdentity targetid = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                            if (targetid == null)
                            {
                                SendMessage(CrunchUtilitiesPlugin.file.EconName, "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                                return;
                            }
                            if (EconUtils.GetBalance(Context.Player.IdentityId) >= amount)
                            {
                                EconUtils.TakeMoney(Context.Player.IdentityId, amount);
                                EconUtils.AddMoney(targetid.IdentityId, amount);

                                //SendMessage(CrunchUtilitiesPlugin.file.EconName, Context.Player.DisplayName + " Has sent you : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                                SendMessage(CrunchUtilitiesPlugin.file.EconName, "You sent " + targetid.DisplayName + " : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                                return;
                            }
                            else
                            {
                                SendMessage(CrunchUtilitiesPlugin.file.EconName, "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                                return;
                            }
                        }

                        if (EconUtils.GetBalance(Context.Player.IdentityId) >= amount)
                        {
                            EconUtils.TakeMoney(Context.Player.IdentityId, amount);
                            EconUtils.AddMoney(player.Identity.IdentityId, amount);

                            SendMessage(CrunchUtilitiesPlugin.file.EconName, MyMultiplayer.Static.GetMemberName(Context.Player.SteamUserId) + " Has sent you : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                            SendMessage(CrunchUtilitiesPlugin.file.EconName, "You sent " + MyMultiplayer.Static.GetMemberName(player.Id.SteamId) + " : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                        }
                        else
                        {
                            SendMessage(CrunchUtilitiesPlugin.file.EconName, "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                        }
                        break;
                    case "faction":
                        IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(recipient);
                        if (fac == null)
                        {
                            SendMessage(CrunchUtilitiesPlugin.file.EconName, "Cant find that faction", Color.Red, (long)Context.Player.SteamUserId);
                            return;
                        }
                        if (EconUtils.GetBalance(Context.Player.IdentityId) >= amount)
                        {
                            //Probablty need to do some reflection/patching shit to add the transfer to the activity log
                            EconUtils.TakeMoney(Context.Player.IdentityId, amount);
                            EconUtils.AddMoney(fac.FactionId, amount);

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
                                SendMessage(CrunchUtilitiesPlugin.file.EconName, Context.Player.DisplayName + " Has sent : " + string.Format("{0:n0}", amount) + " SC to the faction bank.", Color.DarkGreen, (long)steamid);
                                temp.Add(steamid);
                            }

                            SendMessage(CrunchUtilitiesPlugin.file.EconName, "You sent " + fac.Name + " : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                        }
                        else
                        {
                            SendMessage(CrunchUtilitiesPlugin.file.EconName, "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                        }
                        break;
                    case "steam":
                        //Context.Respond("Error Player not online");
                        IMyIdentity id2 = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                        if (id2 == null)
                        {
                            SendMessage(CrunchUtilitiesPlugin.file.EconName, "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                            return;
                        }
                        if (EconUtils.GetBalance(Context.Player.IdentityId) >= amount)
                        {
                            EconUtils.TakeMoney(Context.Player.IdentityId, amount);
                            EconUtils.AddMoney(id2.IdentityId, amount);

                            //SendMessage(CrunchUtilitiesPlugin.file.EconName, Context.Player.DisplayName + " Has sent you : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                            SendMessage(CrunchUtilitiesPlugin.file.EconName, "You sent " + id2.DisplayName + " : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                        }
                        else
                        {
                            SendMessage(CrunchUtilitiesPlugin.file.EconName, "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                        }
                        break;
                    default:
                        SendMessage(CrunchUtilitiesPlugin.file.EconName, "Incorrect usage, example - !eco pay player PlayerName amount or !eco pay faction tag amount", Color.Red, (long)Context.Player.SteamUserId);
                        break;
                }
            }
            else
            {
                SendMessage(CrunchUtilitiesPlugin.file.EconName, "Player pay not enabled", Color.Red, (long)Context.Player.SteamUserId);
            }
        }

        public static void SendMessage(string author, string message, Color color, long steamID)
        {
            ScriptedChatMsg scriptedChatMsg1 = new ScriptedChatMsg
            {
                Author = author,
                Text = message,
                Font = "White",
                Color = color,
                Target = Sync.Players.TryGetIdentityId((ulong)steamID)
            };
            ScriptedChatMsg scriptedChatMsg2 = scriptedChatMsg1;
            MyMultiplayerBase.SendScriptedChatMessage(ref scriptedChatMsg2);
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
            Context.Respond(id.DisplayName + " Balance Before Change : " + EconUtils.GetBalance(id.IdentityId));

            EconUtils.TakeMoney(id.IdentityId, EconUtils.GetBalance(id.IdentityId));

            Context.Respond(id.DisplayName + " Balance After Change : " + EconUtils.GetBalance(id.IdentityId));
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
                EconUtils.TakeMoney(fac.FactionId, EconUtils.GetBalance(fac.FactionId));
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
        public void Cleargrid()
        {
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
            int inventory = 0;
            foreach (var item in gridWithSubGrids)
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                {
                    MyCubeGrid grid = groupNodes.NodeData;

                    //foreach (MySlimBlock b in grid.GetBlocks())
                    //{
                    //    if (b.FatBlock != null && b.FatBlock.HasInventory)
                    //    {
                    //        b.FatBlock.GetInventory().Clear();
                    //        inventory++;

                    //    }
                    //}
                    BoundingSphereD sphere = new BoundingSphereD(grid.PositionComp.GetPosition(), 10000);
                    foreach (MyEntity ent in MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere))
                    {
                        if (ent.Parent != null && ent.Parent.EntityId.Equals(grid.EntityId))
                        {
                            if (ent.HasInventory)
                            {
                                ent.GetInventory().Clear();
                                inventory++;
                            }
                        }
                    }
                }
            }
            Context.Respond("Cleared " + inventory + " inventories");
        }

        [Command("dirkclear", "clear a grids inventory")]
        [Permission(MyPromoteLevel.Admin)]
        public void Dorlcleargrid(string gridName)
        {
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindGridGroup(gridName);
            int inventory = 0;
            int uran = 0;
            int ice = 0;
            int ammo = 0;
            foreach (var item in gridWithSubGrids)
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                {
                    MyCubeGrid grid = groupNodes.NodeData;
                    BoundingSphereD sphere = new BoundingSphereD(grid.PositionComp.GetPosition(), 10000);

                    foreach (MyEntity ent in MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere))
                    {

                        if (ent.Parent != null && ent.Parent.EntityId.Equals(grid.EntityId))
                        {
                            List<uint> deleteThese = new List<uint>();
                            if (ent.HasInventory)
                            {
                                foreach (MyPhysicalInventoryItem invitem in ent.GetInventory().GetItems())
                                {
                                    bool delete = true;
                                    if (invitem.Content.TypeId.ToString().Contains("Ingot") && invitem.Content.SubtypeName.Equals("Uranium"))
                                    {
                                        delete = false;
                                        uran += invitem.Amount.ToIntSafe();
                                    }
                                    if (invitem.Content.TypeId.ToString().Contains("AmmoMagazine"))
                                    {
                                        delete = false;
                                        ammo += invitem.Amount.ToIntSafe();
                                    }

                                    if (invitem.Content.TypeId.ToString().Contains("Ore") && invitem.Content.SubtypeName.Equals("Ice"))
                                    {
                                        delete = false;
                                        ice += invitem.Amount.ToIntSafe();
                                    }
                                    if (delete)
                                    {
                                        deleteThese.Add(invitem.ItemId);
                                        inventory++;
                                    }
                                }
                                foreach (uint id in deleteThese)
                                {
                                    ent.GetInventory().RemoveItems(id);
                                }
                            }
                        }
                    }
                }
            }
            Context.Respond("Cleared " + inventory + " inventories");
            Context.Respond("Uran " + uran + " Ice " + ice + " Ammo " + ammo);
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

        [Command("faction rep change", "Change repuation between factions")]
        [Permission(MyPromoteLevel.Admin)]
        public void ChangeFactionRep(string tag, string tag2, long amount)
        {
            IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac == null)
            {
                Context.Respond($"{tag} faction not found");
                return;
            }
            if (tag2 == "all")
            {

                foreach (var faction in MySession.Static.Factions.Factions.Values)
                {
                    MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, faction.FactionId, int.Parse(amount.ToString()));
                    foreach (var player in MySession.Static.Players.GetAllIdentities())
                    {
                        MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(player.IdentityId, faction.FactionId, int.Parse(amount.ToString()), ReputationChangeReason.Admin);
                    }
                }
            }
            else
            {
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
            }
            Context.Respond("Done");
            return;
        }

        [Command("player rep change", "Change repuation between faction and player")]
        [Permission(MyPromoteLevel.Admin)]
        public void ChangePlayerRep(string playerNameOrId, string tag, long amount)
        {
            MyIdentity player = CrunchUtilitiesPlugin.GetIdentityByNameOrId(playerNameOrId);

            IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(tag);

            if (player != null && fac2 != null)
            {
                //  Context.Respond(player.DisplayName + " FACTION Reputation Before Change : " + MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(Context.Player.IdentityId, fac2.FactionId));
                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(player.IdentityId, fac2.FactionId, 1500, ReputationChangeReason.Admin);
                Context.Respond("Changed");
                // Context.Respond(player.DisplayName + " FACTION Reputation After Change : " + MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(Context.Player.IdentityId, fac2.FactionId));
            }
            else
            {
                Context.Respond("Error faction not found");
            }

            return;

        }
    }
}
