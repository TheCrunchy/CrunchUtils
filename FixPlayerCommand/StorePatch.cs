//using NLog;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Torch.Managers.PatchManager;
//using System.Reflection;
//using Sandbox.Game.Entities.Blocks;
//using Sandbox.Game.World;
//using Sandbox.Game.Entities;
//using Sandbox.ModAPI;
//using VRage.Game;
//using Sandbox.Game.GameSystems.BankingAndCurrency;
//using Torch.Mod.Messages;
//using VRage.Game.Entity;
//using Torch.Mod;
//using Sandbox.Game.Entities.Character;

//namespace CrunchUtilities
//{
//    [PatchShim]
//    public static class MyStorePatch
//    {

//        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

//        internal static readonly MethodInfo update =
//            typeof(MyStoreBlock).GetMethod("BuyFromPlayer", BindingFlags.Instance | BindingFlags.NonPublic) ??
//            throw new Exception("Failed to find patch method");

//        internal static readonly MethodInfo storePatch =
//            typeof(MyStorePatch).GetMethod(nameof(StorePatchMethod), BindingFlags.Static | BindingFlags.Public) ??
//            throw new Exception("Failed to find patch method");

//        public static void Patch(PatchContext ctx)
//        {

//            ctx.GetPattern(update).Prefixes.Add(storePatch);
//            Log.Info("Patching Successful HydrogenStores");
//        }

//        public static bool StorePatchMethod(MyStoreBlock __instance, long id, int amount, long targetEntityId, MyPlayer player, MyAccountInfo playerAccountInfo)
//        {

//            MyStoreItem storeItem = (MyStoreItem)null;
//            foreach (MyStoreItem playerItem in __instance.PlayerItems)
//            {

//                if (playerItem.Id == id)
//                {
//                    storeItem = playerItem;
//                    break;
//                }
//            }
//            if (storeItem == null)
//            {
//                return true;
//            }
//            Boolean isItem = false;
          
//            if (storeItem.Item.Value.SubtypeName.Contains("HydrogenCredit"))
//            {
//                isItem = true;
//            }
//            else
//            {
//                return true;
//            }
 

//            if (isItem)
//            {
//                IMyGridTerminalSystem gridTerminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(__instance.CubeGrid);
//                List<IMyGasTank> tanks = new List<IMyGasTank>();
//                gridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks);
//                List<IMyGasTank> storeTanks = new List<IMyGasTank>();
//                List<IMyGasTank> playerTanks = new List<IMyGasTank>();
//                double totalGas = 0f;
//                VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties gas = new VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties { SubtypeName = "Hydrogen" };
//                double playerCapacity = 0f;
//              //  Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyStoreBlock, MyStoreSellItemResult>(this, (Func<MyStoreBlock, Action<MyStoreSellItemResult>>)(x => new Action<MyStoreSellItemResult>(x.OnSellItemResult)), storeSellItemResult, MyEventContext.Current.Sender);
//                foreach (IMyGasTank gasTank in tanks)
//                {
//                    if (gasTank.OwnerId == __instance.OwnerId)
//                    {
//                        storeTanks.Add(gasTank);
//                        MyGasTank tankk = gasTank as MyGasTank;
//                        if (tankk.FilledRatio > 0 && tankk.BlockDefinition.StoredGasId == MyDefinitionId.FromContent(gas))
//                        {
                        
//                            totalGas += (tankk.FilledRatio) * (double)tankk.Capacity;
//                        }
//                        continue;
//                    }
//                    if (gasTank.OwnerId == player.Identity.IdentityId)
//                    {
//                        playerTanks.Add(gasTank);
//                        MyGasTank tankk = gasTank as MyGasTank;

//                        playerCapacity += (1.0 - tankk.FilledRatio) * (double)tankk.Capacity;
//                        continue;
//                    }
//                }
//                if (totalGas == 0)
//                {
//                    DialogMessage m1 = new DialogMessage("Shop Error", "Tanks have no gas to sell!");
//                    ModCommunication.SendMessageTo(m1, player.Id.SteamId);
//                    return false;
//                }
//                MyCubeGrid grid = __instance.CubeGrid;
//                MyIdentity identity = MySession.Static.Players.TryGetIdentity(playerAccountInfo.OwnerIdentifier);
//                double amountToUse = amount * 1000;
//                double gasToRemove = 0;
//                Log.Info(amountToUse);
//                long price = 0;
//                if (amountToUse >= totalGas)
//                    amountToUse = totalGas;

//                if (amountToUse >= playerCapacity)
//                    amountToUse = playerCapacity;

//                Log.Info(amountToUse);

//                foreach (IMyGasTank tank in playerTanks)
//                {
//                    if (amountToUse > 0)
//                    {
//                        MyGasTank tank2 = tank as MyGasTank;

//                        double num = (1.0 - tank2.FilledRatio) * (double)tank2.Capacity;

//                        if (amountToUse >= num)
//                        {
//                            Log.Info("Filling 1");
//                            tank2.ChangeFillRatioAmount(tank2.FilledRatio + (num / tank2.Capacity));
//                            gasToRemove += num;
//                            price += (long)(num / 1000) * storeItem.PricePerUnit;
//                            amountToUse -= num;

//                        }
//                        else
//                        {
//                            Log.Info("Filling 2");
//                            tank2.ChangeFillRatioAmount(tank2.FilledRatio + (amountToUse / tank2.Capacity));
//                            double newNum = num - amountToUse;
//                            gasToRemove += newNum;
//                            price += (long)(amountToUse / 1000) * storeItem.PricePerUnit;
//                            amountToUse -= newNum;
//                        }
//                    }

//                }
//                foreach (IMyGasTank gas2 in storeTanks)
//                {
//                    if (gasToRemove > 0)
//                    {
//                        MyGasTank tank = gas2 as MyGasTank;

//                        double num = (tank.FilledRatio) * (double)tank.Capacity;

//                        if (gasToRemove >= num)
//                        {
//                            Log.Info("Taking 1");
//                            tank.ChangeFillRatioAmount(0);
//                            gasToRemove -= num;
                            
//                        }
//                        else
//                        {
//                            Log.Info("Taking 2");
//                            double newAmount = num - gasToRemove;
//                            tank.ChangeFillRatioAmount(tank.FilledRatio - (newAmount / tank.Capacity));
//                            gasToRemove -= newAmount;
//                        }
//                    }
                    
//                }
//                EconUtils.takeMoney(player.Identity.IdentityId, price);
//                EconUtils.addMoney(__instance.OwnerId, price);
//                DialogMessage m = new DialogMessage("Shop", "Tanks filled.");
//                ModCommunication.SendMessageTo(m, player.Id.SteamId);
//            }
   

//            return false;
//        }

//    }
//}
