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

namespace CrunchUtilities
{
    public class Commands : CommandModule
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        [Command("crunch reload", "Reload the config")]
        [Permission(MyPromoteLevel.Admin)]
        public void ReloadConfig()
        {
            CrunchUtilitiesPlugin.LoadConfig();
            Context.Respond("Reloaded config");
        }


        [Command("admin makeship", "Admin command, Turn a station and connected grids into a ship")]
        [Permission(MyPromoteLevel.Admin)]
        public void MakeShip()
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

        [Command("admin makestation", "Admin command, Turn a station and connected grids into a ship")]
        [Permission(MyPromoteLevel.Admin)]
        public void MakeStation()
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
        private static CurrentCooldown CreateNewCooldown(Dictionary<long, CurrentCooldown> cooldownMap, long playerId, long cooldown)
        {

            var currentCooldown = new CurrentCooldown(cooldown);

            if (cooldownMap.ContainsKey(playerId))
                cooldownMap[playerId] = currentCooldown;
            else
                cooldownMap.Add(playerId, currentCooldown);

            return currentCooldown;
        }

        [Command("stone", "Delete all stone in a grid")]
        [Permission(MyPromoteLevel.None)]
        public void DeleteStone()
        {
            if (CrunchUtilitiesPlugin.file.DeleteStone)
            {
                CrunchUtilitiesPlugin plugin = (CrunchUtilitiesPlugin) Context.Plugin;
                CrunchUtilitiesPlugin.Log.Info("Cooldown");
               var currentCooldownMap = plugin.CurrentCooldownMap;
                CrunchUtilitiesPlugin.Log.Info("Cooldown 1");
                if (currentCooldownMap.TryGetValue(Context.Player.IdentityId, out CurrentCooldown currentCooldown))
                {
                    CrunchUtilitiesPlugin.Log.Info("Cooldown 2");
                    long remainingSeconds = currentCooldown.GetRemainingSeconds(null);
                    CrunchUtilitiesPlugin.Log.Info("Cooldown 3");
                    if (remainingSeconds > 0)
                    {
                        CrunchUtilitiesPlugin.Log.Info("Cooldown 4");
                        CrunchUtilitiesPlugin.Log.Info("Cooldown for Player " + Context.Player.DisplayName + " still running! " + remainingSeconds + " seconds remaining!");
                        Context.Respond("Command is still on cooldown for " + remainingSeconds + " seconds.");
                        return;
                    }
                    CrunchUtilitiesPlugin.Log.Info("Cooldown 5");
                    currentCooldown = CreateNewCooldown(currentCooldownMap, Context.Player.IdentityId, plugin.Cooldown);
                    currentCooldown.StartCooldown(null);
                }
                else
                {
                    currentCooldown = CreateNewCooldown(currentCooldownMap, Context.Player.IdentityId, plugin.Cooldown);
                    currentCooldown.StartCooldown(null);
                }
                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
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
                                        if (items[i].Content.SubtypeId.ToString().Contains("Stone"))
                                        {
                                            block.GetInventory().RemoveItems(items[i].ItemId);
                                        }
                                    }
                                }
                            }
                        }
                        Context.Respond("Deleted the stone?");
                    }
                }
            }
            else
            {
                Context.Respond("stone not enabled");
            }

        }
        [Command("convert", "Player command, Turn a ship and connected grids into a station")]
        [Permission(MyPromoteLevel.None)]
        public void MakeStationPlayer()
        {
            if (CrunchUtilitiesPlugin.file.PlayerMakeShip)
            {

                if (MyGravityProviderSystem.IsPositionInNaturalGravity(Context.Player.GetPosition()))
                {
                    
                    Context.Respond("You cannot use this command in natural gravity!");
                    return;
                }

                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
                if (gridWithSubGrids.Count > 0)
                {
                    foreach (var item in gridWithSubGrids)
                    {
                        foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                        {

                            MyCubeGrid grid = groupNodes.NodeData;

                            if (!FacUtils.IsOwnerOrFactionOwned(grid, Context.Player.IdentityId, true))
                            {

                                continue;
                            }
                            else
                            {
                                if (grid.IsStatic)
                                {
                                    Action m_convertToShipResult = null;
                                    grid.RequestConversionToShip(m_convertToShipResult);
                                    Context.Respond("Converting to ship " + grid.DisplayName);
                                }
                                else
                                {
                                    grid.OnConvertedToStationRequest();
                                    Context.Respond("Converting to station " + grid.DisplayName);

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
                try
                {
                    MatrixD playerPos = player.Character.WorldMatrix;
                    VRage.ModAPI.IMyEntity entity = MyEntities.GetEntityById(player.Character.EntityId) as VRage.ModAPI.IMyEntity;
                    VRage.Game.ModAPI.IMyInventory playerInv = player.Character.GetInventory();
                    Dictionary<String, int> amounts = new Dictionary<string, int>();
                    //  MyItemType itemType = new MyInventoryItemFilter("MyObjectBuilder_Component/" + pair.Key).ItemType;

                    player.SpawnAt(playerPos, new Vector3(0, 0, 0), entity, false);
                    IMyCharacter oldCharacter = player.Character;
                    IMyCharacter newCharacter = player.Character;
                    player.SpawnIntoCharacter(newCharacter);
                    oldCharacter.GetInventory().Clear();
                    oldCharacter.Kill();
                    // oldCharacter.Delete();


                    int i;
                    for (i = 0; i < playerInv.ItemCount; i++)
                    {
                        playerInv.TransferItemTo(player.Character.GetInventory(), playerInv.GetItemAt(i).Value);
                    }

                    Context.Respond("You should be fixed");
                }
                catch (Exception)
                {
                    Context.Respond("Relog and try again");
                    return;
                }
            }
            else
            {
                Context.Respond("PlayerFixMe not enabled");
            }
        }
    }
}
