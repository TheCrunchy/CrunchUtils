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

        [Command("enablestone", "Reload the config")]
        [Permission(MyPromoteLevel.Admin)]
        public void Enableconfig()
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
                    CrunchUtilitiesPlugin.file.PlayerMakeShip = bool.TryParse(value, out bool _);
                    break;
                case "playerfixme":
                    CrunchUtilitiesPlugin.file.PlayerFixMe = bool.TryParse(value, out bool _);
                    break;
                case "deletestone":
                    CrunchUtilitiesPlugin.file.DeleteStone = bool.TryParse(value, out bool _);
                    break;
                case "withdraw":
                    CrunchUtilitiesPlugin.file.Withdraw = bool.TryParse(value, out bool _);
                    break;
                case "deposit":
                    CrunchUtilitiesPlugin.file.Deposit = bool.TryParse(value, out bool _);
                    break;
                case "factionsharedeposit":
                    CrunchUtilitiesPlugin.file.FactionShareDeposit = bool.TryParse(value, out bool _);
                    break;
                case "identityupdate":
                    CrunchUtilitiesPlugin.file.IdentityUpdate = bool.TryParse(value, out bool _);
                    break;
                case "cooldowninseconds":
                    CrunchUtilitiesPlugin.file.CooldownInSeconds = int.Parse(value);
                    break;
            }
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

        public bool IsBlockedFaction(IMyPlayer player)
        {
            //IMyFaction fac = FacUtils.GetPlayersFaction(player.IdentityId);
            //if (fac != null && fac.Name.ToLower().Contains("henchsel"))
            //{
            //    return true;
            //}
            // if (fac != null && fac.Name.ToLower().Contains("innovation jump"))
            // {
            //     return true;
            // }
            // if (fac != null && fac.Name.ToLower().Contains("allied humans"))
            // {
            //     return true;
            // }
            // if (fac != null && fac.Name.ToLower().Contains("red matter"))
            // {
            //     return true;
            // }
            // if (fac != null && fac.Name.ToLower().Contains("extraplanetary alliance"))
            // {
            //     return true;
            // }
            return false;
        }

        public int DeleteStone(List<MyCubeGrid> gridsToSearch)
        {
            int count = 0;
            foreach (MyCubeGrid grid in gridsToSearch)
            {
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
                        if (block != null && block.HasInventory)
                        {
                            MyInventory inventory = (MyInventory)block.GetInventory();
                            var items = inventory.GetItems();

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
            return count;
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
                    tempList.Add(controller.CubeGrid);
                }
                else
                {
                    ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
                    foreach (var item in gridWithSubGrids)
                    {
                        foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                        {
                            //     MyObjectBuilder_PhysicalObject stone = new MyObjectBuilder_PhysicalObject("MyObjectBuilder_Ore/Stone");
                            MyCubeGrid grid = groupNodes.NodeData;
                            tempList.Add(grid);
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

        [Command("claim", "Player command, claim a shared grid")]
        [Permission(MyPromoteLevel.None)]
        public void ClaimCommand()
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
                                        if (!block.IsFunctional)
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
                                                factionShared += 1;
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                                                //sharedWithAll += 1;
                                                break;
                                            case MyRelationsBetweenPlayerAndBlock.Neutral:
                                            case MyRelationsBetweenPlayerAndBlock.Enemies:
                                            case MyRelationsBetweenPlayerAndBlock.Friends:
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
                                grid.ChangeGridOwner(Context.Player.IdentityId, MyOwnershipShareModeEnum.None);

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

        [Command("prediction", "remove the prediction shit")]
        [Permission(MyPromoteLevel.Admin)]
        public void Noprediction()
        {
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
            if (gridWithSubGrids.Count > 0)
            {

                foreach (var item in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;
                        grid.AllowPrediction = !grid.AllowPrediction;
                        //   grid.ForceDisablePrediction = !grid.ForceDisablePrediction;
                        Context.Respond("Prediction set to " + grid.AllowPrediction);
                    }
                }
            }
        }

        //[Command("paint", "recolorblocks")]
        //[Permission(MyPromoteLevel.None)]
        //public void RepaintTargetGridColor(string oldColourHex, string newColourHex)
        //{


        //    if (MyGravityProviderSystem.IsPositionInNaturalGravity(Context.Player.GetPosition()) && !CrunchUtilitiesPlugin.file.convertInGravity)
        //    {

        //        Context.Respond("You cannot use this command in natural gravity!");
        //        return;
        //    }

        //    var col = ColorExtensions.HexToColor(oldColourHex);
        //    var newcol = ColorExtensions.HexToColor(newColourHex);


        //    ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroupMechanical(Context.Player.Character);
        //    if (gridWithSubGrids.Count > 0)
        //    {

        //        foreach (var item in gridWithSubGrids)
        //        {
        //            foreach (MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node groupNodes in item.Nodes)
        //            {

        //                MyCubeGrid grid = groupNodes.NodeData;
        //                if (FacUtils.IsOwnerOrFactionOwned(grid, Context.Player.IdentityId, true))
        //                {
        //                    foreach (MySlimBlock block in grid.GetBlocks())
        //                    {

        //                        CrunchUtilitiesPlugin.Log.Info(block.ColorMaskHSV + " " + col + " " + ColorExtensions.ColorToHSVDX11(col));
        //                        if (block.ColorMaskHSV.X == ColorExtensions.ColorToHSV(col).X)
        //                        {

        //                            block.CubeGrid.ColorBlocks(block.Min, block.Max, newcol, true, false);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}



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
                        MyGasTank tankk = tank as MyGasTank;

                        tankk.ChangeFillRatioAmount(fill);

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
                foreach (MyStation myStation in keyValuePair.Value.Stations)
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

        /*
        private MyGps CreateGps(Vector3D Position, Color gpsColor, int seconds, string Nation, string Reason)
        {
            MyGps gps = new MyGps
            {
                Coords = Position,
                Name = Nation + " - ",
                DisplayName = Nation + " - Position",
                GPSColor = gpsColor,
                IsContainerGPS = true,
                ShowOnHud = true,
                DiscardAt = new TimeSpan(0, 0, seconds, 0),
                Description = "Nation Distress Signal \n" + Reason,
            };
            gps.UpdateHash();

            return gps;
        }
        */

        //[Command("isthisasteroid", "fuck fuck fuck")]
        //[Permission(MyPromoteLevel.None)]
        //public void DoAsteroidStuff()
        //{

        //    List<VRage.ModAPI.IMyEntity> l = new List<VRage.ModAPI.IMyEntity>();

        //    BoundingSphereD sphere = new BoundingSphereD(Context.Player.Character.PositionComp.GetPosition(), 500);
        //    l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);


        //    foreach (IMyEntity e in l)
        //    {
        //        if (e is MyCubeGrid grid)
        //        {

        //            MyGps gps = CreateGps(grid.PositionComp.GetPosition(), Color.Green, 300, "", "");


        //            MyGpsCollection gpsCollection = (MyGpsCollection)MyAPIGateway.Session?.GPS;
        //            gps.SetEntityId(grid.EntityId);
        //            gpsCollection.AddPlayerGps(Context.Player.IdentityId, ref gps);

        //        }
        //        CrunchUtilitiesPlugin.Log.Info(e.GetType().ToString());
        //    }

        //}
        [Command("ishydrogen", "testing store stuff")]
        [Permission(MyPromoteLevel.Admin)]
        public void TEST()
        {
            MyStoreBlock store = null;

            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);

            if (gridWithSubGrids.Count > 0)
            {
                Context.Respond("3");
                foreach (var itemd in gridWithSubGrids)
                {
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in itemd.Nodes)
                    {
                        MyCubeGrid grid = groupNodes.NodeData;
                        var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                        var blockList = new List<Sandbox.ModAPI.IMyStoreBlock>();
                        gts.GetBlocksOfType(blockList);

                        foreach (Sandbox.ModAPI.IMyStoreBlock s in blockList)
                        {
                            store = s as MyStoreBlock;
                            foreach (MyStoreItem item in store.PlayerItems)
                            {
                                Context.Respond(item.StoreItemType.ToString());
                            }
                        }
                    }
                }
            }
        }

        /*
        private static long PriceWorth(MyDefinitionId id)
        {
            MyBlueprintDefinitionBase bpDef = MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(id);
            float price = 0;
            MyDefinitionManager.Static.TryGetComponentDefinition(id, out MyComponentDefinition component);
            //calculate by the minimal price per unit for modded components, vanilla is aids

            if (component != null && component.MinimalPricePerUnit > 1)
            {
                long amn = Math.Abs(component.MinimalPricePerUnit);
                price += amn;
            }
            //if keen didnt give the fucker a minimal price calculate by the ores that make up the ingots, because fuck having an integer for an economy right?
            else
            {
                for (var p = 0; p < bpDef.Prerequisites.Length; p++)
                {
                    if (bpDef.Prerequisites[p].Id != null)
                    {
                        MyDefinitionBase oreDef = MyDefinitionManager.Static.GetDefinition(bpDef.Prerequisites[p].Id);
                        if (oreDef != null)
                        {
                            MyPhysicalItemDefinition ore = oreDef as MyPhysicalItemDefinition;
                            float amn = Math.Abs(ore.MinimalPricePerUnit);
                            float count = (float)bpDef.Prerequisites[p].Amount;
                            amn = (float)Math.Round(amn * count * 3);
                            price += amn;
                        }
                    }
                }
            }
            return Convert.ToInt64(price);
        }
        */

        [Command("zone", "edit safezone whitelist or blacklist")]
        [Permission(MyPromoteLevel.Admin)]
        public void EditZone(string addOrRemove, string playerOrFac, string nameOrTag)
        {

            //   if (!whiteOrBlack.ToLower().Contains("white") && !whiteOrBlack.ToLower().Contains("black"))
            //  {
            //    Context.Respond("Couldnt read input! Use whitelist or blacklist");
            //     return;
            //  }
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

        [Command("isecon", "output if the target station is an economy")]
        [Permission(MyPromoteLevel.None)]
        public void FindNearbyStations()
        {
            if (CrunchUtilitiesPlugin.file.FixTradeStation)
            {
                BoundingSphereD sphere = new BoundingSphereD(Context.Player.Character.PositionComp.GetPosition(), 500);
                var l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
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

                foreach (VRage.Game.ModAPI.IMyCubeGrid grid in NPCGrids)
                {

                    if (MySession.Static.Factions.GetStationByGridId(grid.EntityId) != null)
                    {
                        Context.Respond("Yes this is an economy station");
                    }
                    else
                    {
                        Context.Respond("No this isnt an economy station");
                    }
                }
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



        //[Command("clustercheck", "check for grid clusters in input range")]
        //[Permission(MyPromoteLevel.Admin)]
        //public void basicClusterCheck(int distance)
        //{
        //    StringBuilder clusters = new StringBuilder();
        //    clusters.AppendLine("Clustered grids");
        //    try
        //    {
        //        foreach (var group in MyCubeGridGroups.Static.Mechanical.Groups)
        //        {
        //            foreach (var item in group.Nodes)
        //            {

        //                int count = 0;
        //                MyCubeGrid grid = item.NodeData;
        //                BoundingSphereD sphere = new BoundingSphereD(grid.PositionComp.GetPosition(), distance);
        //                foreach (MyCubeGrid grid2 in MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere).OfType<MyCubeGrid>())
        //                {
        //                    count++;
        //                }
        //                if (count > 0)
        //                {
        //                    clusters.AppendLine(grid.DisplayName + " grids within distance = " + count);
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CrunchUtilitiesPlugin.Log.Error(ex);
        //        return;
        //    }
        //    Context.Respond(clusters.ToString());
        //}

        [Command("warstatus", "check war status")]
        [Permission(MyPromoteLevel.None)]
        public void DeclareWar(string tag1, string tag2)
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
        public void DeclareWar(string tag)
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

                            MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, fac.FactionId, -3000);

                            MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, fac.FactionId, 0);

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

                                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, fac.FactionId, 0);

                                MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, fac.FactionId, 0);

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

        [Command("ac", "display a factions description")]
        [Permission(MyPromoteLevel.None)]
        public void ShowAdminTools()
        {
            if (MySession.Static.Players.GetOnlinePlayers().Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
                {
                    int count = 0;
                    if (MySession.Static.RemoteAdminSettings.ContainsKey(p.Id.SteamId))
                    {
                        stringBuilder.AppendLine("");

                        stringBuilder.AppendLine(p.DisplayName + " Steam ID: " + p.Id.SteamId);
                        AdminSettingsEnum adminSetting = MySession.Static.RemoteAdminSettings[p.Id.SteamId];

                        if (MySession.Static.CreativeToolsEnabled(p.Id.SteamId))
                        {
                            stringBuilder.AppendLine("CreativeTools: enabled");
                            count++;
                        }
                        if (adminSetting.HasFlag(AdminSettingsEnum.IgnorePcu))
                        {
                            stringBuilder.AppendLine("IgnorePCU: enabled");
                            count++;
                        }

                        if (adminSetting.HasFlag(AdminSettingsEnum.IgnoreSafeZones))
                        {
                            stringBuilder.AppendLine("IgnoreSafeZones: enabled");
                            count++;
                        }

                        if (adminSetting.HasFlag(AdminSettingsEnum.Invulnerable))
                        {
                            stringBuilder.AppendLine("Invulnerable: enabled");
                            count++;
                        }

                        if (adminSetting.HasFlag(AdminSettingsEnum.KeepOriginalOwnershipOnPaste))
                        {
                            stringBuilder.AppendLine("KeepOriginalOwnershipOnPaste: enabled");
                            count++;
                        }

                        if (adminSetting.HasFlag(AdminSettingsEnum.ShowPlayers))
                        {
                            stringBuilder.AppendLine("ShowPlayers: enabled");
                            count++;
                        }

                        if (adminSetting.HasFlag(AdminSettingsEnum.Untargetable))
                        {
                            stringBuilder.AppendLine("Untargetable: enabled");
                            count++;
                        }

                        if (adminSetting.HasFlag(AdminSettingsEnum.UseTerminals))
                        {
                            stringBuilder.AppendLine("UseTerminals: enabled");
                            count++;
                        }

                        if (count > 0)
                            if (Context.Player == null)
                            {
                                Context.Respond(stringBuilder.ToString(), "Big Brother");
                            }
                            else
                            {
                                DialogMessage m = new DialogMessage("Admin tools enabled", "hi", stringBuilder.ToString());
                                ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);
                            }


                    }
                }
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
        public void PlaceStation(string npctag, string type, int x, int y, int z)
        {
            if (!(MySession.Static.Factions.TryGetFactionByTag(npctag) is MyFaction npcfac))
            {
                Context.Respond("Cant find faction.");
                return;
            }

            if (MyGravityProviderSystem.IsPositionInNaturalGravity(new Vector3D(x, y, z)))
            {

                Context.Respond("This doesnt work properly in gravity yet, avoid planets");
                return;
            }

            //MyStationsListDefinition stationTypeDefinition = MyStationGenerator.GetStationTypeDefinition(MyStationTypeEnum.SpaceStation);
            //Type generator = MySession.Static.GetType().Assembly.GetType("Sandbox.Game.World.Generator.MyStationGenerator");
            //MethodInfo getStationTypeDefinition = generator?.GetMethod("SendFactionChange", BindingFlags.NonPublic | BindingFlags.Static);
            //List<object[]> ReturnPlayers = new List<object[]>();
            //object[] MethodInput = new object[] { change, target.FactionId, playerfac.FactionId, 0L };

            //  sendChange?.Invoke(null, MethodInput);
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
            Vector3 position = new Vector3D(x, y, z);
            MyStation station = new MyStation(MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.STATION, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM), position, stationType, npcfac, GetRandomStationName(stationDefinition), stationDefinition.GeneratedItemsContainerType);

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
                    MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, npcfac.FactionId, 0);

                    MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, npcfac.FactionId, 0);
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
                    MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id2.IdentityId, npcfac.FactionId, 0);
                    MySession.Static.Factions.AddFactionPlayerReputation(id2.IdentityId, npcfac.FactionId, 0);
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
                    return;
                }
                else
                {
                    Context.Respond("\nName: " + fac.Name + " " + fac.FactionId + "\nTag: " + fac.Tag + "\n" + warstatus + "\nDescription: " + fac.Description + "\nMembers: " + fac.Members.Count);

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
            Context.Respond("Command Disabled, widraw with amount");
            return;

            /*
            long balance;
            long withdrew = 0;
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
                                long min = long.Parse(block.FatBlock.GetInventory().CurrentVolume.RawValue.ToString());
                                long max = long.Parse(block.FatBlock.GetInventory().MaxVolume.RawValue.ToString());
                                long difference = (max - min) * 1000;
                                if (balance >= int.MaxValue)
                                {
                                    long newBalance = balance;
                                    bool canAdd = true;
                                    while (canAdd)
                                    {
                                        if (newBalance >= int.MaxValue)
                                        {
                                            newBalance -= int.MaxValue;
                                            if ((block.FatBlock.GetInventory().CanItemsBeAdded(MyFixedPoint.DeserializeStringSafe(int.MaxValue.ToString()), itemType)))
                                            {
                                                container = block.FatBlock as MyCubeBlock;
                                                invent = container.GetInventory();
                                                invent.AddItems(MyFixedPoint.DeserializeStringSafe(int.MaxValue.ToString()), new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
                                                EconUtils.takeMoney(player.IdentityId, int.MaxValue);
                                                withdrew += int.MaxValue;
                                                Context.Respond("Added the credits to " + container.DisplayNameText);
                                            }
                                            else
                                            {
                                                canAdd = false;
                                            }
                                        }
                                        else
                                        {
                                            if ((block.FatBlock.GetInventory().CanItemsBeAdded(MyFixedPoint.DeserializeStringSafe(newBalance.ToString()), itemType)))
                                            {
                                                container = block.FatBlock as MyCubeBlock;
                                                invent = container.GetInventory();
                                                invent.AddItems(MyFixedPoint.DeserializeStringSafe(newBalance.ToString()), new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
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
                                    if (block.FatBlock.GetInventory().CanItemsBeAdded(MyFixedPoint.DeserializeStringSafe(balance.ToString()), itemType))
                                    {

                                        container = block.FatBlock as MyCubeBlock;
                                        invent = container.GetInventory();

                                        invent.AddItems(MyFixedPoint.DeserializeStringSafe(balance.ToString()), new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
                                        EconUtils.takeMoney(player.IdentityId, balance);

                                        withdrew += balance;
                                        balance = 0;
                                        Context.Respond("Added the credits to " + container.DisplayNameText);

                                        Context.Respond("Withdrew : " + string.Format("{0:n0}", withdrew));
                                        return;
                                    }
                                }
                            }
                        }
                    }

                }
            }
            Context.Respond("Withdrew : " + string.Format("{0:n0}", withdrew));
            */
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

                                                EconUtils.AddMoney(player.IdentityId, (long)amount);
                                            }
                                            else
                                            {
                                                deposited += Item.Amount;
                                                EconUtils.AddMoney(player.IdentityId, (long)amount);
                                                block.FatBlock.GetInventory().RemoveItemAmount(Item, amount);
                                            }
                                        }
                                        gridname += grid.DisplayName + ", ";
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

                MethodInfo method = typeof(MyInventory).GetMethod("AddItemsInternal", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[4]
                {
                    typeof (MyFixedPoint), typeof (MyObjectBuilder_PhysicalObject), typeof (uint?), typeof (int)
                },
                null);

                if (method == null)
                    throw new Exception("reflection error");

                MyFixedPoint AmountFixed = (MyFixedPoint)(double)amount;

                method.Invoke(invent, new object[4] { AmountFixed, item, new uint?(), -1 });
                //refresh this or buggy stuff happens
                invent.Refresh();
                SendMessage("[C]", "You were given " + amount + " " + subtypeName, Color.Green, (long)player.Id.SteamId);
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

            /*
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
                if (fac.IsFounder(Context.Player.IdentityId))
                {
                    foreach (VRage.Game.MyFactionMember key in members.Values)
                    {
                        if (id.IdentityId.Equals(key.PlayerId))
                            newFounder = key;
                        if (id.IdentityId.Equals(Context.Player.IdentityId))
                            currentFounder = key;
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
            */
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
                if (IsBlockedFaction(Context.Player))
                {
                    Context.Respond("This faction lost the ability to use this command.");
                    return;
                }
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
                    SendMessage("CrunchEcon", "Error parsing amount", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }
                if (amount < 0 || amount == 0)
                {
                    SendMessage("CrunchEcon", "Must be a positive number", Color.Red, (long)Context.Player.SteamUserId);
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
                                SendMessage("CrunchEcon", "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                                return;
                            }
                            if (EconUtils.GetBalance(Context.Player.IdentityId) >= amount)
                            {
                                EconUtils.TakeMoney(Context.Player.IdentityId, amount);
                                EconUtils.AddMoney(targetid.IdentityId, amount);


                                //SendMessage("CrunchEcon", Context.Player.DisplayName + " Has sent you : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                                SendMessage("CrunchEcon", "You sent " + targetid.DisplayName + " : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                            }
                            else
                            {
                                SendMessage("CrunchEcon", "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                            }
                            return;
                        }

                        if (!(Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(recipient) is MyPlayer player))
                        {
                            SendMessage("CrunchEcon", "They arent online, trying across instance", Color.Red, (long)Context.Player.SteamUserId);
                            IMyIdentity targetid = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                            if (targetid == null)
                            {
                                SendMessage("CrunchEcon", "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                                return;
                            }
                            if (EconUtils.GetBalance(Context.Player.IdentityId) >= amount)
                            {
                                EconUtils.TakeMoney(Context.Player.IdentityId, amount);
                                EconUtils.AddMoney(targetid.IdentityId, amount);

                                //SendMessage("CrunchEcon", Context.Player.DisplayName + " Has sent you : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                                SendMessage("CrunchEcon", "You sent " + targetid.DisplayName + " : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                                return;
                            }
                            else
                            {
                                SendMessage("CrunchEcon", "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                                return;
                            }
                        }

                        if (EconUtils.GetBalance(Context.Player.IdentityId) >= amount)
                        {
                            EconUtils.TakeMoney(Context.Player.IdentityId, amount);
                            EconUtils.AddMoney(player.Identity.IdentityId, amount);

                            SendMessage("CrunchEcon", MyMultiplayer.Static.GetMemberName(Context.Player.SteamUserId) + " Has sent you : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                            SendMessage("CrunchEcon", "You sent " + MyMultiplayer.Static.GetMemberName(player.Id.SteamId) + " : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                        }
                        else
                        {
                            SendMessage("CrunchEcon", "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                        }
                        break;
                    case "faction":
                        IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(recipient);
                        if (fac == null)
                        {
                            SendMessage("CrunchEcon", "Cant find that faction", Color.Red, (long)Context.Player.SteamUserId);
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
                                SendMessage("CrunchEcon", Context.Player.DisplayName + " Has sent : " + string.Format("{0:n0}", amount) + " SC to the faction bank.", Color.DarkGreen, (long)steamid);
                                temp.Add(steamid);
                            }

                            SendMessage("CrunchEcon", "You sent " + fac.Name + " : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                        }
                        else
                        {
                            SendMessage("CrunchEcon", "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                        }
                        break;
                    case "steam":
                        //Context.Respond("Error Player not online");
                        IMyIdentity id2 = CrunchUtilitiesPlugin.GetIdentityByNameOrId(recipient);
                        if (id2 == null)
                        {
                            SendMessage("CrunchEcon", "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                            return;
                        }
                        if (EconUtils.GetBalance(Context.Player.IdentityId) >= amount)
                        {
                            EconUtils.TakeMoney(Context.Player.IdentityId, amount);
                            EconUtils.AddMoney(id2.IdentityId, amount);

                            //SendMessage("CrunchEcon", Context.Player.DisplayName + " Has sent you : " + String.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)player.Id.SteamId);
                            SendMessage("CrunchEcon", "You sent " + id2.DisplayName + " : " + string.Format("{0:n0}", amount) + " SC", Color.Cyan, (long)Context.Player.SteamUserId);
                        }
                        else
                        {
                            SendMessage("CrunchEcon", "You too poor", Color.Red, (long)Context.Player.SteamUserId);
                        }
                        break;
                    default:
                        SendMessage("CrunchEcon", "Incorrect usage, example - !eco pay player PlayerName amount or !eco pay faction tag amount", Color.Red, (long)Context.Player.SteamUserId);
                        break;
                }
            }
            else
            {
                SendMessage("CrunchEcon", "Player pay not enabled", Color.Red, (long)Context.Player.SteamUserId);
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

            //why is this even necessary
            var msgIndex = Context.RawArgs.IndexOf(" ", message.Length);
            if (msgIndex == -1 || msgIndex > Context.RawArgs.Length - 1)
                return;

            message = Context.RawArgs.Substring(msgIndex);
            if (!(Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(name) is MyPlayer player))
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

            //why is this even necessary
            var msgIndex = Context.RawArgs.IndexOf(" ", message.Length);
            if (msgIndex == -1 || msgIndex > Context.RawArgs.Length - 1)
                return;

            message = Context.RawArgs.Substring(msgIndex);
            if (!(Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(name) is MyPlayer player))
            {
                SendMessage("[]", "Cant find that player", Color.Red, (long)Context.Player.SteamUserId);
                return;
            }

            SendMessage("From " + MyMultiplayer.Static.GetMemberName(Context.Player.SteamUserId) + " >> " + message.ToString(), "", Color.MediumPurple, (long)player.Id.SteamId);
            SendMessage("To " + MyMultiplayer.Static.GetMemberName(player.Id.SteamId) + " >> " + message.ToString(), "", Color.MediumPurple, (long)Context.Player.SteamUserId);
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

        [Command("eco giveplayer", "gives money to a player")]
        [Permission(MyPromoteLevel.Admin)]
        public void AddMoneysPlayer(string playerNameOrId, long amount)
        {
            Context.Respond("Legacy command. Use !eco give player PlayerName amount or !eco give faction tag amount");
            return;

            /*
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
            */
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

        //[Command("deletenoworkingbeacon", "delete beacons if they arent working")]
        //[Permission(MyPromoteLevel.Admin)]
        //public void deleteTheseGrids()
        //{

        //    foreach (var group in MyCubeGridGroups.Static.Logical.Groups)
        //    {
        //        bool NPC = false;
        //        foreach (var item in group.Nodes)
        //        {
        //            MyCubeGrid grid = item.NodeData;
        //            if (((int)grid.Flags & 4) != 0)
        //            {
        //                //concealed
        //                break;
        //            }
        //            if (item.NodeData.Projector != null)
        //            {
        //                //projection
        //                break;
        //            }
        //            IEnumerable<MyBeacon> beacons = grid.GetFatBlocks().OfType<MyBeacon>();

        //            bool delete = true;
        //            foreach (long l in grid.BigOwners)
        //            {
        //                if (FacUtils.GetFactionTag(l) != null && FacUtils.GetFactionTag(l).Length > 3)
        //                {
        //                    NPC = true;
        //                }
        //            }

        //            if (NPC)
        //            {
        //                break;
        //            }


        //            foreach (MyBeacon b in beacons)
        //            {

        //                List<Sandbox.ModAPI.Ingame.IMyPowerProducer> PowerProducers = new List<Sandbox.ModAPI.Ingame.IMyPowerProducer>();
        //                var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
        //                float power = 0f;
        //                gts.GetBlocksOfType(PowerProducers);
        //                if (PowerProducers.Count != 0)
        //                {
        //                    foreach (var powerProducer in PowerProducers)
        //                    {
        //                        power += powerProducer.CurrentOutput;

        //                    }
        //                }
        //                if (b.IsFunctional && power > 0f)
        //                {
        //                    delete = false;
        //                }

        //            }

        //            if (delete)
        //            {
        //                if (warnedGrids.ContainsKey(grid.EntityId))
        //                {
        //                    warnedGrids.TryGetValue(grid.EntityId, out int temp);
        //                    if (temp > 4)
        //                    {
        //                        var b = grid.GetFatBlocks<MyCockpit>();
        //                        foreach (var c in b)
        //                        {
        //                            c.RemovePilot();
        //                        }
        //                        grid.Close();
        //                        foreach (long l in grid.BigOwners)
        //                        {
        //                            SendMessage("Delete Notification", "The grid " + grid.DisplayName + " Was deleted for lacking a functional beacon and power", Color.DarkRed, (long)MySession.Static.Players.TryGetSteamId(l));

        //                            CrunchUtilitiesPlugin.Log.Info("Deleting " + grid.DisplayName);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        warnedGrids.Remove(grid.EntityId);
        //                        foreach (long l in grid.BigOwners)
        //                        {
        //                            CrunchUtilitiesPlugin.Log.Info("Warning " + temp + " " + grid.DisplayName);
        //                            SendMessage("Delete Warning", "The grid " + grid.DisplayName + " Will be deleted in " + (5 - temp) + " minutes if a beacon is not functional and powered", Color.DarkRed, (long)MySession.Static.Players.TryGetSteamId(l));

        //                        }

        //                        warnedGrids.Add(grid.EntityId, temp += 1);

        //                    }

        //                }
        //                else
        //                {
        //                    foreach (long l in grid.BigOwners)
        //                    {
        //                        CrunchUtilitiesPlugin.Log.Info("Warning " + grid.DisplayName);
        //                        SendMessage("Delete Warning", "The grid " + grid.DisplayName + " Will be deleted in 5 minutes if a beacon is not functional and powered", Color.DarkRed, (long)MySession.Static.Players.TryGetSteamId(l));

        //                    }

        //                    warnedGrids.Add(grid.EntityId, 1);
        //                }

        //                //check if its had 5 warnings
        //                //grid.Close();
        //                //log it
        //            }
        //        }
        //    }
        //}
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
        public void RemoveMoneysPlayer(string playerNameOrId, long amount)
        {
            Context.Respond("Legacy command. Use !eco take player PlayerName amount or !eco take faction tag amount");
            return;
            /*
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
            */
        }

        [Command("eco givefac", "gibs money to a faction")]
        [Permission(MyPromoteLevel.Admin)]
        public void AddMoneysFaction(string tag, long amount)
        {
            Context.Respond("Legacy command. Use !eco give player PlayerName amount or !eco give faction tag amount");
            return;

            /*
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
            */
        }

        [Command("faction rep change", "Change repuation between factions")]
        [Permission(MyPromoteLevel.Admin)]
        public void ChangeFactionRep(string tag, string tag2, long amount)
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
        public void ChangePlayerRep(string playerNameOrId, string tag, long amount)
        {
            MyIdentity player = CrunchUtilitiesPlugin.GetIdentityByNameOrId(playerNameOrId);
            IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(tag);

            if (player != null && fac2 != null)
            {
                //  Context.Respond(player.DisplayName + " FACTION Reputation Before Change : " + MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(Context.Player.IdentityId, fac2.FactionId));
                MySession.Static.Factions.AddFactionPlayerReputation(player.IdentityId, fac2.FactionId, 1500, true, true);
                Context.Respond("Did it work?");
                // Context.Respond(player.DisplayName + " FACTION Reputation After Change : " + MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(Context.Player.IdentityId, fac2.FactionId));
            }
            else
            {
                Context.Respond("Error faction not found");
            }
            return;

        }

        [Command("eco takefac", "remove money from a faction")]
        [Permission(MyPromoteLevel.Admin)]
        public void RemoveMoneysFaction(string tag, long amount)
        {
            Context.Respond("Legacy command. Use !eco take player PlayerName amount or !eco take faction tag amount");
            return;

            /*
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
            */
        }
    }
}
