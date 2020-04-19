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
                        bool isStatic = false;
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
                                        Action m_convertToShipResult = null;
                                        grid.RequestConversionToShip(m_convertToShipResult);
                                        Context.Respond("Converting to ship " + grid.DisplayName);
                                        isStatic = true;
                                }
                                else
                                {
                                    if (isStatic)
                                    {
                                        return;
                                    }
                                    try
                                    {
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
                    Context.Respond("You should be fixed after respawning");
                    player.Character.Kill();
                    player.Character.Delete();
                    MyMultiplayer.Static.DisconnectClient(player.SteamUserId);
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
                Context.Respond("PlayerFixMe not enabled");
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
        [Permission(MyPromoteLevel.Admin)]
        public void EcoHelp()
        {
            Context.Respond("\n"
            + "Players \n"
            + "!eco balanceplayer player \n"
            + "!eco giveplayer player amount \n"
            + "!eco takeplayer player amount \n"

            + "Factions \n"
            + "!eco balancefaction tag \n"
            + "!eco givefac tag amount \n"
            + "!eco takefac tag amount \n");
        }

        [Command("eco balanceplayer", "See a players balance")]
        [Permission(MyPromoteLevel.Admin)]
        public void CheckMoneysPlayer(string playerNameOrId)
        {
            //Context.Respond("Error Player not online");
            IMyIdentity id = CrunchUtilitiesPlugin.GetIdentityByNameOrId(playerNameOrId);
            if (id == null)
            {
                Context.Respond("Error cant find that guy");
                return;
            }
            else
            {
                Context.Respond(id.DisplayName + " Balance : " + EconUtils.getBalance(id.IdentityId));
                return;
            }
        }

        [Command("eco withdraw", "Withdraw moneys")]
        [Permission(MyPromoteLevel.Admin)]
        public void PlayerWithdraw(Int64 amount)
        {
            IMyPlayer player = Context.Player;
            if (player == null)
            {
                Context.Respond("Console cant withdraw money.....");
            }

        }


        [Command("giveitem", "Give target player an item")]
        [Permission(MyPromoteLevel.Admin)]
        public void PlayerWithdraw(string PlayerName, string type, string subtypeName, Int64 amount)
        {
            IMyPlayer player = MySession.Static.Players.GetPlayerByName(PlayerName);
            if (player == null)
            {
                Context.Respond("Cant find that player");
            }
            VRage.Game.ModAPI.IMyInventory invent = player.Character.GetInventory();
            switch (type)
            {
                //Eventually add some checks to see if the item exists before adding it
                case "Ore":
                    MyObjectBuilder_PhysicalObject item = new MyObjectBuilder_Ore() { SubtypeName = subtypeName };
                        invent.AddItems(VRage.MyFixedPoint.DeserializeStringSafe(amount.ToString()), item);
                        Context.Respond("Giving " + player.DisplayName + " " + amount + " " + item.SubtypeName);
                    break;
                case "Ingot":
                        MyObjectBuilder_PhysicalObject item2 = new MyObjectBuilder_Ingot() { SubtypeName = subtypeName };
                        invent.AddItems(VRage.MyFixedPoint.DeserializeStringSafe(amount.ToString()), item2);
                        Context.Respond("Giving " + player.DisplayName + " " + amount + " " + item2.SubtypeName);
                    break;
                case "Component":
                    MyObjectBuilder_PhysicalObject item3 = new MyObjectBuilder_Component() { SubtypeName = subtypeName };
                        invent.AddItems(VRage.MyFixedPoint.DeserializeStringSafe(amount.ToString()), item3);
                        Context.Respond("Giving " + player.DisplayName + " " + amount + " " + item3.SubtypeName);
                    break;
            }
          
        }

        [Command("eco balancefaction", "See a factions balance")]
        [Permission(MyPromoteLevel.Admin)]
        public void CheckMoneysFaction(string tag)
        {
            IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac != null)
            {
                Context.Respond(fac.Name + "FACTION Balance : " + fac.GetBalanceShortString());
                return;
            }
            else
            {
                Context.Respond("Error, no faction");
            }
        }

        [Command("eco giveplayer", "gibs money to a player")]
        [Permission(MyPromoteLevel.Admin)]
        public void AddMoneysPlayer(string playerNameOrId, Int64 amount)
        {
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



        [Command("eco takeplayer", "removes money from a player")]
        [Permission(MyPromoteLevel.Admin)]
        public void RemoveMoneysPlayer(string playerNameOrId, Int64 amount)
        {
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

            return;

        }

        [Command("eco givefac", "gibs money to a faction")]
        [Permission(MyPromoteLevel.Admin)]
        public void AddMoneysFaction(string tag, Int64 amount)
        {
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
                    Context.Respond(fac.Name + " FACTION Reputation After Change : "+ MySession.Static.Factions.GetRelationBetweenFactions(fac.FactionId, fac2.FactionId));  
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

        [Command("faction rep change", "gibs money to a faction")]
        [Permission(MyPromoteLevel.Admin)]
        public void AddMoneysFaction(string tag, string tag2, Int64 amount)
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
        [Command("eco takefac", "remove money from a faction")]
        [Permission(MyPromoteLevel.Admin)]
        public void removeMoneysFaction(string tag, Int64 amount)
        {
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
