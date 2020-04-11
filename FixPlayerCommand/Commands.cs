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

namespace FixPlayerPlugin
{
    public class Commands : CommandModule
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        [Command("fixme", "Murder a player then respawn them at their current location")]
        [Permission(MyPromoteLevel.None)]
        public void CalcShipPrice()
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
    }
}
