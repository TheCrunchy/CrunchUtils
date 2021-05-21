using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers.PatchManager;
using System.Reflection;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.World;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Torch.Mod.Messages;
using VRage.Game.Entity;
using Torch.Mod;
using Sandbox.Game.Entities.Character;

namespace CrunchUtilities
{
    [PatchShim]
    public static class MyStorePatch
    {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal static readonly MethodInfo update =
            typeof(MyStoreBlock).GetMethod("BuyFromPlayer", BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo storePatch =
            typeof(MyStorePatch).GetMethod(nameof(StorePatchMethod), BindingFlags.Static | BindingFlags.Public) ??
            throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {

            ctx.GetPattern(update).Prefixes.Add(storePatch);
            Log.Info("Patching Successful CrunchDrill!");
        }

        //        public static void yeetHydrogen(List<Sandbox.ModAPI.IMyGasTank> tanks, double amountToTake, long OwnerId)
        //        {
        //            double amountTaken = 0;
        //            double toTake = amountToTake;
        //            foreach (Sandbox.ModAPI.IMyGasTank tank in tanks)
        //            {
        //                MyGasTank tankk = tank as MyGasTank;

        //                if (tankk.FilledRatio > 0 && tankk.OwnerId == OwnerId)
        //                {
        //                    double gasintank = (tankk.FilledRatio / tankk.Capacity) * 100;
        //                    if (toTake > 0)
        //                    {
        //                        if (gasintank >= toTake)
        //                        {
        //                            double gasTaken = gasintank - (gasintank -= toTake);
        //                            double gasLeft = gasintank - toTake;
        //                            if (gasLeft == 0)
        //                            {
        //                                tankk.ChangeFillRatioAmount(0);
        //                            }
        //                            else
        //                            {
        //                                tankk.ChangeFillRatioAmount((gasLeft / tankk.GasCapacity) * 100);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            toTake -= gasintank;
        //                            tankk.ChangeFillRatioAmount(0);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        public static bool StorePatchMethod(MyStoreBlock __instance, long id, int amount, long targetEntityId, MyPlayer player, MyAccountInfo playerAccountInfo)
        //        {

        //            MyStoreItem storeItem = (MyStoreItem)null;
        //            foreach (MyStoreItem playerItem in __instance.PlayerItems)
        //            {
        //                //add the hydrogen thing
        //                Boolean isItem = false;
        //                //  if (playerItem.Item.Value.SubtypeName.Contains("GSI Premium Hydrogen") || playerItem.Item.Value.SubtypeName.Contains("Premium Hydrogen"))
        //                // {
        //                isItem = true;
        //                //  }
        //                if (playerItem.Id == id && isItem)
        //                {
        //                    storeItem = playerItem;
        //                    Log.Info(storeItem.Item.Value.SubtypeName);
        //                    MyCubeGrid grid = __instance.CubeGrid;
        //                    Log.Info(grid.DisplayName);
        //                    var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
        //                    VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties gas = new VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties { SubtypeName = "Hydrogen" };
        //                    var blockList = new List<Sandbox.ModAPI.IMyGasTank>();
        //                    gts.GetBlocksOfType<Sandbox.ModAPI.IMyGasTank>(blockList);
        //                    double totalGas = 0f;
        //                    foreach (Sandbox.ModAPI.IMyGasTank tank in blockList)
        //                    {
        //                        MyGasTank tankk = tank as MyGasTank;

        //                        if (tankk.FilledRatio > 0 && tankk.OwnerId == __instance.OwnerId && tankk.BlockDefinition.StoredGasId == MyDefinitionId.FromContent(gas))
        //                        {
        //                            double gasintank = (tankk.FilledRatio / tankk.Capacity) * 100;
        //                            totalGas += gasintank;
        //                        }
        //                    }
        //                    MyIdentity identity = MySession.Static.Players.TryGetIdentity(playerAccountInfo.OwnerIdentifier);
        //                    double amountToUse = amount;
        //                    if (amount > totalGas)
        //                    {
        //                        amountToUse = totalGas;
        //                        //  DialogMessage m = new DialogMessage("Shop Error", "Selected amount exceeds tank capacity");
        //                        // ModCommunication.SendMessageTo(m, identity.Character.ControlSteamId);
        //                    }

        //                    Log.Info(totalGas);
        //                    if (totalGas == 0)
        //                    {
        //                        DialogMessage m = new DialogMessage("Shop Error", "Tanks have no gas to sell!");
        //                        ModCommunication.SendMessageTo(m, identity.Character.ControlSteamId);
        //                        return false;
        //                    }
        //                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(targetEntityId, out MyEntity entity, false))
        //                    {
        //                        if (entity is MyCubeBlock myCubeBlock)
        //                        {
        //                            Log.Info(myCubeBlock.CubeGrid.DisplayName);
        //                            var gts2 = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(myCubeBlock.CubeGrid);
        //                            var blockList2 = new List<Sandbox.ModAPI.IMyGasTank>();
        //                            gts.GetBlocksOfType<Sandbox.ModAPI.IMyGasTank>(blockList2);

        //                            double cost = 0;
        //                            double filledAmount = 0f;



        //                            foreach (Sandbox.ModAPI.IMyGasTank tank in blockList2)
        //                            {

        //                                if (filledAmount < amountToUse)
        //                                {

        //                                    MyGasTank tank2 = tank as MyGasTank;



        //                                    if (tank2.BlockDefinition.StoredGasId == MyDefinitionId.FromContent(gas))
        //                                    {

        //                                        Log.Info("hydrogen Tank");
        //                                        double newamount;
        //                                        if (filledAmount > 0)
        //                                        {
        //                                            newamount = (amountToUse - filledAmount) * 100;
        //                                        }
        //                                        else
        //                                        {
        //                                            newamount = amountToUse * 100;
        //                                        }

        //                                        double num = (1.0 - tank2.FilledRatio) * (double)tank2.Capacity;
        //                                        //num = whats in tank

        //                                        //  Log.Info(num);

        //                                        if ((double)newamount > num)
        //                                        {

        //                                            DialogMessage m = new DialogMessage("Shop Error", "Selected amount exceeds tank capacity");
        //                                            ModCommunication.SendMessageTo(m, identity.Character.ControlSteamId);
        //                                            return false;
        //                                        }
        //                                        double newpercent = (newamount / tank2.Capacity) * 100;
        //                                        Log.Info((tank2.FilledRatio * (double)tank2.Capacity).ToString());
        //                                        double test = (tank2.FilledRatio / (double)tank2.Capacity) * 100;
        //                                        test += newamount;
        //                                        if (tank2.FilledRatio + (test / tank2.Capacity) * 100 > tank2.Capacity)
        //                                        {
        //                                            double canFill = (tank2.FilledRatio / tank2.Capacity) * 100;
        //                                            filledAmount += newamount / 100;
        //                                            double localCost = storeItem.PricePerUnit * (canFill / 100);
        //                                            tank2.ChangeFillRatioAmount(tank2.FilledRatio + (canFill / tank2.Capacity) * 100);
        //                                            EconUtils.takeMoney(playerAccountInfo.OwnerIdentifier, (long)localCost);
        //                                            EconUtils.addMoney(__instance.OwnerId, (long)localCost);
        //                                            playerItem.Amount -= (int)canFill / 100;
        //                                            //  MyIdentity identity = MySession.Static.Players.TryGetIdentity(tank2.OwnerId);
        //                                            //   DialogMessage m = new DialogMessage("Shop Error", "Selected amount exceeds tank capacity");
        //                                            //  ModCommunication.SendMessageTo(m, identity.Character.ControlSteamId);
        //                                            //  return false;
        //                                        }
        //                                        else
        //                                        {
        //                                            if (totalGas >= test)
        //                                            {
        //                                                cost += storeItem.PricePerUnit * (newamount / 100);
        //                                                totalGas -= newamount;
        //                                                double localCost = storeItem.PricePerUnit * (newamount / 100);
        //                                                if (playerAccountInfo.Balance >= localCost)
        //                                                {
        //                                                    filledAmount += newamount / 100;
        //                                                    tank2.ChangeFillRatioAmount(tank2.FilledRatio + (test / tank2.Capacity) * 100);
        //                                                    EconUtils.takeMoney(playerAccountInfo.OwnerIdentifier, (long)localCost);
        //                                                    playerItem.Amount -= (int)filledAmount / 100;
        //                                                    EconUtils.addMoney(__instance.OwnerId, (long)localCost);
        //                                                }
        //                                            }
        //                                        }




        //                                        //loop through tanks, check if each tanks can take the full amount, if it can fill it and charge the user
        //                                        //if it cant, fill what it can and charge the user then move onto the next tank



        //                                        //  object[] MethodInput = new object[] { newamount };
        //                                        //  fillTank.Invoke(tank2, MethodInput);
        //                                    }
        //                                }
        //                            }
        //                            if (cost > 0)
        //                            {
        //                                yeetHydrogen(blockList, filledAmount, __instance.OwnerId);
        //                                return false;
        //                            }
        //                            else
        //                            {
        //                                return true;
        //                            }
        //                        }

        //                        else
        //                        {
        //                            if (entity is MyCharacter character)
        //                            {

        //                                DialogMessage m = new DialogMessage("Shop Error", "Hydrogen credits must be bought to a cargo container of the target ship");
        //                                ModCommunication.SendMessageTo(m, identity.Character.ControlSteamId);
        //                            }
        //                            return false;
        //                        }
        //                    }



        //                    //   var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
        //                    //   var blockList = new List<Sandbox.ModAPI.IMyStoreBlock>();
        //                    //    gts.GetBlocksOfType<Sandbox.ModAPI.IMyStoreBlock>(blockList);
        //                    // foreach (Sandbox.ModAPI.IMyStoreBlock store in blockList)

        //                }
        //            }

        //            return true;
        //        }
        //    }
        //}
        //    [PatchShim]
        //    public static class StorePatch
        //    {

        //        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //        internal static readonly MethodInfo update =
        //            typeof(MyStoreBlock).GetMethod("BuyFromPlayer", BindingFlags.Instance | BindingFlags.NonPublic) ??
        //            throw new Exception("Failed to find patch method");

        //        internal static readonly MethodInfo storePatch =
        //            typeof(StorePatch).GetMethod(nameof(StorePatchMethod), BindingFlags.Static | BindingFlags.Public) ??
        //            throw new Exception("Failed to find patch method");
        //        public static MethodInfo gasTransfer;
        //        public static void Patch(PatchContext ctx)
        //        {

        //            ctx.GetPattern(update).Prefixes.Add(storePatch);
        //            Log.Info("Patching Successful CrunchDrill!");
        //        }

        //        public static void yeetHydrogen(List<Sandbox.ModAPI.IMyGasTank> tanks, double amountToTake, long OwnerId)
        //        {
        //            double amountTaken = 0;
        //            double toTake = amountToTake;
        //            foreach (Sandbox.ModAPI.IMyGasTank tank in tanks)
        //            {
        //                MyGasTank tankk = tank as MyGasTank;

        //                if (tankk.FilledRatio > 0 && tankk.OwnerId == OwnerId)
        //                {
        //                    double gasintank = (tankk.FilledRatio / tankk.Capacity) * 100;
        //                    if (toTake > 0)
        //                    {
        //                        if (gasintank >= toTake)
        //                        {
        //                            double gasTaken = gasintank - (gasintank -= toTake);
        //                            double gasLeft = gasintank - toTake;
        //                            if (gasLeft == 0)
        //                            {
        //                                tankk.ChangeFillRatioAmount(0);
        //                            }
        //                            else
        //                            {
        //                                tankk.ChangeFillRatioAmount((gasLeft / tankk.GasCapacity) * 100);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            toTake -= gasintank;
        //                            tankk.ChangeFillRatioAmount(0);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        public static bool StorePatchMethod(MyStoreBlock __instance, long id, int amount, long targetEntityId, MyPlayer player, MyAccountInfo playerAccountInfo)
        {

            MyStoreItem storeItem = (MyStoreItem)null;
            foreach (MyStoreItem playerItem in __instance.PlayerItems)
            {

                if (playerItem.Id == id)
                {
                    storeItem = playerItem;
                    break;
                }
            }
            if (storeItem == null)
            {
                return true;
            }
            Boolean isItem = false;
          
            if (storeItem.Item.Value.SubtypeName.Contains("HydrogenCredit"))
            {
                isItem = true;
            }
            else
            {
                return true;
            }
 

            if (isItem)
            {
                IMyGridTerminalSystem gridTerminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(__instance.CubeGrid);
                List<IMyGasTank> tanks = new List<IMyGasTank>();
                gridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks);
                List<IMyGasTank> storeTanks = new List<IMyGasTank>();
                List<IMyGasTank> playerTanks = new List<IMyGasTank>();
                double totalGas = 0f;
                VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties gas = new VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties { SubtypeName = "Hydrogen" };
                double playerCapacity = 0f;
                foreach (IMyGasTank gasTank in tanks)
                {
                    if (gasTank.OwnerId == __instance.OwnerId)
                    {
                        storeTanks.Add(gasTank);
                        MyGasTank tankk = gasTank as MyGasTank;
                        if (tankk.FilledRatio > 0 && tankk.BlockDefinition.StoredGasId == MyDefinitionId.FromContent(gas))
                        {
                        
                            totalGas += (tankk.FilledRatio) * (double)tankk.Capacity;
                        }
                        continue;
                    }
                    if (gasTank.OwnerId == player.Identity.IdentityId)
                    {
                        playerTanks.Add(gasTank);
                        MyGasTank tankk = gasTank as MyGasTank;

                        playerCapacity += (1.0 - tankk.FilledRatio) * (double)tankk.Capacity;
                        continue;
                    }
                }
                if (totalGas == 0)
                {
                    DialogMessage m1 = new DialogMessage("Shop Error", "Tanks have no gas to sell!");
                    ModCommunication.SendMessageTo(m1, player.Id.SteamId);
                    return false;
                }
                MyCubeGrid grid = __instance.CubeGrid;
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(playerAccountInfo.OwnerIdentifier);
                double amountToUse = amount * 1000;
                double gasToRemove = 0;
                Log.Info(amountToUse);
                long price = 0;
                if (amountToUse >= totalGas)
                    amountToUse = totalGas;

                if (amountToUse >= playerCapacity)
                    amountToUse = playerCapacity;

                Log.Info(amountToUse);

                foreach (IMyGasTank tank in playerTanks)
                {
                    if (amountToUse > 0)
                    {
                        MyGasTank tank2 = tank as MyGasTank;

                        double num = (1.0 - tank2.FilledRatio) * (double)tank2.Capacity;

                        if (amountToUse >= num)
                        {
                            Log.Info("Filling 1");
                            tank2.ChangeFillRatioAmount(tank2.FilledRatio + (num / tank2.Capacity));
                            gasToRemove += num;
                            price += (long)(num / 1000) * storeItem.PricePerUnit;
                            amountToUse -= num;

                        }
                        else
                        {
                            Log.Info("Filling 2");
                            tank2.ChangeFillRatioAmount(tank2.FilledRatio + (amountToUse / tank2.Capacity));
                            gasToRemove += num;
                            price += (long)(amountToUse / 1000) * storeItem.PricePerUnit;
                            amountToUse -= num;
                        }
                    }

                }
                foreach (IMyGasTank gas2 in storeTanks)
                {
                    if (gasToRemove > 0)
                    {
                        MyGasTank tank = gas2 as MyGasTank;

                        double num = (1.0 - tank.FilledRatio) * (double)tank.Capacity;

                        if (gasToRemove >= num)
                        {
                            Log.Info("Taking 1");
                            tank.ChangeFillRatioAmount(tank.FilledRatio - (num / tank.Capacity));
                            gasToRemove -= num;
                        }
                        else
                        {
                            Log.Info("Taking 2");
                            tank.ChangeFillRatioAmount(tank.FilledRatio - (gasToRemove / tank.Capacity));
                            gasToRemove -= num;
                        }
                    }
                    
                }
                EconUtils.takeMoney(player.Identity.IdentityId, price);
                EconUtils.addMoney(__instance.OwnerId, price);
                DialogMessage m = new DialogMessage("Shop", "Tanks filled.");
                ModCommunication.SendMessageTo(m, player.Id.SteamId);
            }
   

            return false;
        }

    }
}
