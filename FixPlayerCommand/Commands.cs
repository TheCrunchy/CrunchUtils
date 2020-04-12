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
                        Action m_convertToShipResult = null;
                        grid.RequestConversionToShip(m_convertToShipResult);
                        Context.Respond("Fixing grid / subgrid");
                    }
                }
            

        }

        [Command("makeship", "Admin command, Turn a station and connected grids into a ship")]
        [Permission(MyPromoteLevel.None)]
        public void MakeShipPlayer()
        {
        if (CrunchUtilitiesPlugin.file.PlayerMakeShip)
        {
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = GridFinder.FindLookAtGridGroup(Context.Player.Character);
            foreach (var item in gridWithSubGrids)
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                {
                    MyCubeGrid grid = groupNodes.NodeData;
                        if (!FacUtils.IsOwnerOrFactionOwned(grid, Context.Player.IdentityId, true))
                        {
                            Context.Respond("You dont own this");
                            continue;
                        }
                        else
                        {
                            Action m_convertToShipResult = null;
                            grid.RequestConversionToShip(m_convertToShipResult);
                            Context.Respond("Fixing grid / subgrid");
                        }
                }
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
