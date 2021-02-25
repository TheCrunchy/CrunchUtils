using NLog;
using Sandbox.Game.World;
using System;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Torch;
using Torch.API;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using Torch.Session;
using System.Timers;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;
using System.Collections;
using Torch.API.Managers;
using Torch.API.Session;
using System.IO;
using Sandbox.Game.Entities;
using VRage.Groups;
using VRage.Game;
using System.Collections.Concurrent;
using Sandbox.Game.Entities.Cube;
using System.Collections.Generic;
using Sandbox.Engine.Multiplayer;
using Torch.Managers.PatchManager;
using System.Reflection;
using Sandbox.Game.Weapons;
using VRage.ObjectBuilders;
using Sandbox.ModAPI;
using VRageMath;
using VRage.ModAPI;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Blocks;
using VRage.Game.Entity;
using Torch.Mod.Messages;
using Torch.Mod;
using SpaceEngineers.Game.EntityComponents.GameLogic;
using Sandbox.Game.SessionComponents;
using SpaceEngineers.Game.Entities.Blocks;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using VRage.Network;
using Sandbox.ModAPI.Weapons;

namespace CrunchUtilities
{


    public class CrunchUtilitiesPlugin : TorchPluginBase
    {
        public static Dictionary<long, long> moneyToPay = new Dictionary<long, long>();



        [PatchShim]
        public static class ProjectorPatch
        {

            public static readonly Logger Log = LogManager.GetCurrentClassLogger();

            internal static readonly MethodInfo update =
                typeof(MyProjectorBase).GetMethod("InitializeClipboard", BindingFlags.Instance | BindingFlags.NonPublic) ??
                throw new Exception("Failed to find patch method");

            internal static readonly MethodInfo updatePatch =
                typeof(ProjectorPatch).GetMethod(nameof(TestPatchMethod), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");


            internal static readonly MethodInfo remove =
             typeof(MyProjectorBase).GetMethod("RemoveProjection", BindingFlags.Instance | BindingFlags.NonPublic) ??
             throw new Exception("Failed to find patch method");

            internal static readonly MethodInfo removePatch =
                typeof(ProjectorPatch).GetMethod(nameof(removeM), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");
            public static void Patch(PatchContext ctx)
            {

                ctx.GetPattern(update).Suffixes.Add(updatePatch);
                ctx.GetPattern(remove).Prefixes.Add(removePatch);
                Log.Info("Patching Successful Crunch Projector!");
            }

            public static void TestPatchMethod(MyProjectorBase __instance)
            {
                if (file == null)
                {

                    return;
                }
                if (!file.projectorPatch)
                {
                    return;
                }


                if (__instance != null)
                {
                    List<MyObjectBuilder_CubeGrid> grids = __instance.Clipboard.CopiedGrids;
                    int count = 0;
                    if (grids == null)
                    {
                        return;
                    }
                    foreach (MyObjectBuilder_CubeGrid objectBuilderCubeGrid in grids)
                    {
                        count += objectBuilderCubeGrid.CubeBlocks.Count;
                    }
                    MyCubeGrid grid = __instance.CubeGrid;
                    //Log.Info("Removing? " + grid.BlocksPCU);
                    grid.BlocksPCU -= count;
                    // Log.Info("Removing? " + grid.BlocksPCU);
                }


            }
           
            public static void removeM(MyProjectorBase __instance)
            {
                if (file == null)
                {

                    return;
                }
                if (!file.projectorPatch)
                {
                    return;
                }
                if (__instance != null)
                {
                    List<MyCubeGrid> grids2 = __instance?.Clipboard?.PreviewGrids;
                    int count = 0;
                    if (grids2 == null)
                    {
                        return;
                    }
                    foreach (MyCubeGrid griid in grids2)
                    {

                        count += griid.CubeBlocks.Count;
                        //  Log.Info("count " + count);
                    }
                    // Log.Info("count " + count);
                    MyCubeGrid grid = __instance.CubeGrid;
                    //  Log.Info("Adding? " + grid.BlocksPCU);
                    grid.BlocksPCU += count;
                    //   Log.Info("Adding? " + grid.BlocksPCU);

                }


            }
        }

        [PatchShim]
        public static class MyPatch
        {

            public static readonly Logger Log = LogManager.GetCurrentClassLogger();

            internal static readonly MethodInfo update =
                typeof(MyDrillBase).GetMethod("TryHarvestOreMaterial", BindingFlags.Instance | BindingFlags.NonPublic) ??
                throw new Exception("Failed to find patch method");

            internal static readonly MethodInfo updatePatch =
                typeof(MyPatch).GetMethod(nameof(TestPatchMethod), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");

            public static void Patch(PatchContext ctx)
            {

                ctx.GetPattern(update).Prefixes.Add(updatePatch);
                Log.Info("Patching Successful CrunchDrill!");
            }

            public static bool TestPatchMethod(MyDrillBase __instance, MyVoxelMaterialDefinition material, Vector3 hitPosition)
            {

                // var playerId = __instance.Owner.GetPlayerIdentityId();

                //     Log.Info(__instance.OutputInventory.Owner.GetBaseEntity().EntityId);
                //     Log.Info(MyAPIGateway.Entities.GetEntityById(__instance.OutputInventory.Owner.GetBaseEntity().EntityId).GetType());
                if (file != null && !file.DeleteStoneAuto)
                {
                    return true;
                }
                if (string.IsNullOrEmpty(material.MinedOre))
                    return false;
                List<IMyEntity> l = new List<IMyEntity>();
                if (__instance.OutputInventory == null || __instance.OutputInventory.Owner == null || __instance.OutputInventory.Owner.GetBaseEntity() == null)
                {
                    return true;
                }
                if (__instance.OutputInventory != null && __instance.OutputInventory.Owner != null)
                {
                    if (__instance.OutputInventory.Owner.GetBaseEntity() is MyShipDrill)
                    {
                        MyShipDrill drill = __instance.OutputInventory.Owner.GetBaseEntity() as MyShipDrill;

                        if (drill == null)
                        {
                            return true;
                        }

                        //     BoundingSphereD sphere = new BoundingSphereD(hitPosition, 400);
                        //    l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
                        //  foreach (IMyEntity e in l)
                        //  {
                        //    if (e is MyCharacter)
                        //   {

                        if (drill.DisplayNameText != null && containsName(drill.DisplayNameText))
                        {
                            MyObjectBuilder_Ore newObject = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(material.MinedOre);
                            if (newObject.SubtypeName.ToLower().Contains("stone"))
                            {
                                if (file.UsingDraconisEliteDrills)
                                {
                                    if (!drill.BlockDefinition.BlockPairName.Equals("Drill8x"))
                                    {
                                        return true;
                                    }

                                }
                                return false;
                            }
                        }
                        if (ids.Contains(drill.OwnerId))
                        {
                            MyObjectBuilder_Ore newObject = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(material.MinedOre);
                            if (newObject.SubtypeName.ToLower().Contains("stone"))
                            {
                                if (file.UsingDraconisEliteDrills)
                                {
                                    if (!drill.BlockDefinition.BlockPairName.Equals("Drill8x"))
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                return true;

            }

            // }


            //return true;

            //  }
        }
        public static bool containsName(String name)
        {
            if (name.ToLower().Contains("no stone") || name.ToLower().Contains("!stone"))
            {
                return true;
            }
            return false;
        }

        //[PatchShim]
        //public static class MyJumpPatch
        //{

        //    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //    internal static readonly MethodInfo update =
        //        typeof(MyGpsCollection).GetMethod("GetGpsList", BindingFlags.Instance | BindingFlags.Public) ??
        //        throw new Exception("Failed to find patch method");

        //    internal static readonly MethodInfo storePatch =
        //        typeof(MyJumpPatch).GetMethod(nameof(GasPatchMethod), BindingFlags.Static | BindingFlags.Public) ??
        //        throw new Exception("Failed to find patch method");

        //    public static void Patch(PatchContext ctx)
        //    {

        //        ctx.GetPattern(update).Suffixes.Add(storePatch);
        //        Log.Info("Patching Successful CrunchDrill!");
        //    }

        //    public static void GasPatchMethod(MyGpsCollection __instance, long identityId, List<IMyGps> list)
        //    {
        //        Log.Info("JUMP DRIVE?");

        //                Dictionary<int, IMyGps> someOrganisation = new Dictionary<int, IMyGps>();
        //    List<IMyGps> unsorted = new List<IMyGps>();
        //    int highest = 0;
        //                foreach (IMyGps gps in list)
        //                {
        //                    if (gps != null && gps.Name != null && gps.Name.StartsWith("#"))
        //                    {

        //                        String part1 = gps.Name.Split(' ')[0].Replace("#", "");
        //                        if (int.TryParse(part1, out int result))
        //                        {
        //                            if (!someOrganisation.ContainsKey(result))
        //                            {
        //                                if (result > highest)
        //                                {
        //                                    highest = result;
        //                                }
        //                                if (result <= 100)
        //                                {
        //                                    someOrganisation.Add(result, gps);
        //                                }
        //                                else
        //                                {
        //                                    unsorted.Add(gps);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                unsorted.Add(gps);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            unsorted.Add(gps);
        //                        }

        //                    }
        //                    else
        //                    {
        //                        unsorted.Add(gps);
        //                    }


        //                }

        //                if (highest > 100)
        //                {
        //                    highest = 100;
        //                }
        //                list.Clear();


        //                if (someOrganisation.Count > 0)
        //                {
        //                    for (int i = 0; i <= highest; i++)
        //                    {
        //                        if (someOrganisation.ContainsKey(i))
        //                        {
        //                            someOrganisation.TryGetValue(i, out IMyGps gps);
        //                            list.Add(gps);

        //                        }
        //                    }
        //                }

        //                if (unsorted.Count > 0)
        //                {

        //                    list.AddList(unsorted);

        //                }

        //                return;
        //            }
        //        }

        //[PatchShim]
        //public static class MyStorePatch
        //{

        //    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //    internal static readonly MethodInfo update =
        //        typeof(MyStoreBlock).GetMethod("BuyFromPlayer", BindingFlags.Instance | BindingFlags.NonPublic) ??
        //        throw new Exception("Failed to find patch method");

        //    internal static readonly MethodInfo storePatch =
        //        typeof(MyStorePatch).GetMethod(nameof(StorePatchMethod), BindingFlags.Static | BindingFlags.Public) ??
        //        throw new Exception("Failed to find patch method");

        //    public static void Patch(PatchContext ctx)
        //    {

        //        ctx.GetPattern(update).Prefixes.Add(storePatch);
        //        Log.Info("Patching Successful CrunchDrill!");
        //    }

        //    public static void yeetHydrogen(List<Sandbox.ModAPI.IMyGasTank> tanks, double amountToTake, long OwnerId)
        //    {
        //        double amountTaken = 0;
        //        double toTake = amountToTake;
        //        foreach (Sandbox.ModAPI.IMyGasTank tank in tanks)
        //        {
        //            MyGasTank tankk = tank as MyGasTank;

        //            if (tankk.FilledRatio > 0 && tankk.OwnerId == OwnerId)
        //            {
        //                double gasintank = (tankk.FilledRatio / tankk.Capacity) * 100;
        //                if (toTake > 0)
        //                {
        //                    if (gasintank >= toTake)
        //                    {
        //                        double gasTaken = gasintank - (gasintank -= toTake);
        //                        double gasLeft = gasintank - toTake;
        //                        if (gasLeft == 0)
        //                        {
        //                            tankk.ChangeFillRatioAmount(0);
        //                        }
        //                        else
        //                        {
        //                            tankk.ChangeFillRatioAmount((gasLeft / tankk.GasCapacity) * 100);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        toTake -= gasintank;
        //                        tankk.ChangeFillRatioAmount(0);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    public static bool StorePatchMethod(MyStoreBlock __instance, long id, int amount, long targetEntityId, MyPlayer player, MyAccountInfo playerAccountInfo)
        //    {

        //        MyStoreItem storeItem = (MyStoreItem)null;
        //        foreach (MyStoreItem playerItem in __instance.PlayerItems)
        //        {
        //            //add the hydrogen thing
        //            Boolean isItem = false;
        //          //  if (playerItem.Item.Value.SubtypeName.Contains("GSI Premium Hydrogen") || playerItem.Item.Value.SubtypeName.Contains("Premium Hydrogen"))
        //           // {
        //                isItem = true;
        //          //  }
        //            if (playerItem.Id == id && isItem)
        //            {
        //                storeItem = playerItem;
        //                Log.Info(storeItem.Item.Value.SubtypeName);
        //                MyCubeGrid grid = __instance.CubeGrid;
        //                Log.Info(grid.DisplayName);
        //                var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
        //                VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties gas = new VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties { SubtypeName = "Hydrogen" };
        //                var blockList = new List<Sandbox.ModAPI.IMyGasTank>();
        //                gts.GetBlocksOfType<Sandbox.ModAPI.IMyGasTank>(blockList);
        //                double totalGas = 0f;
        //                foreach (Sandbox.ModAPI.IMyGasTank tank in blockList)
        //                {
        //                    MyGasTank tankk = tank as MyGasTank;

        //                    if (tankk.FilledRatio > 0 && tankk.OwnerId == __instance.OwnerId && tankk.BlockDefinition.StoredGasId == MyDefinitionId.FromContent(gas))
        //                    {
        //                        double gasintank = (tankk.FilledRatio / tankk.Capacity) * 100;
        //                        totalGas += gasintank;
        //                    }
        //                }
        //                MyIdentity identity = MySession.Static.Players.TryGetIdentity(playerAccountInfo.OwnerIdentifier);
        //                double amountToUse = amount;
        //                if (amount > totalGas)
        //                {
        //                    amountToUse = totalGas;
        //                  //  DialogMessage m = new DialogMessage("Shop Error", "Selected amount exceeds tank capacity");
        //                   // ModCommunication.SendMessageTo(m, identity.Character.ControlSteamId);
        //                }

        //                Log.Info(totalGas);
        //                if (totalGas == 0)
        //                {
        //                     DialogMessage m = new DialogMessage("Shop Error", "Tanks have no gas to sell!");
        //                     ModCommunication.SendMessageTo(m, identity.Character.ControlSteamId);
        //                    return false;
        //                }
        //                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(targetEntityId, out MyEntity entity, false))
        //                {
        //                    if (entity is MyCubeBlock myCubeBlock)
        //                    {
        //                        Log.Info(myCubeBlock.CubeGrid.DisplayName);
        //                        var gts2 = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(myCubeBlock.CubeGrid);
        //                        var blockList2 = new List<Sandbox.ModAPI.IMyGasTank>();
        //                        gts.GetBlocksOfType<Sandbox.ModAPI.IMyGasTank>(blockList2);

        //                       double cost = 0;
        //                      double filledAmount = 0f;



        //                        foreach (Sandbox.ModAPI.IMyGasTank tank in blockList2)
        //                        {

        //                            if (filledAmount <amountToUse)
        //                            {

        //                                MyGasTank tank2 = tank as MyGasTank;



        //                                if (tank2.BlockDefinition.StoredGasId == MyDefinitionId.FromContent(gas))
        //                                {

        //                                    Log.Info("hydrogen Tank");
        //                                    double newamount;
        //                                    if (filledAmount > 0)
        //                                    {
        //                                        newamount = (amountToUse - filledAmount) * 100;
        //                                    }
        //                                    else
        //                                    {
        //                                        newamount = amountToUse * 100;
        //                                    }

        //                                    double num = (1.0 - tank2.FilledRatio) * (double)tank2.Capacity;
        //                                    //num = whats in tank

        //                                    //  Log.Info(num);

        //                                    if ((double)newamount > num)
        //                                    {

        //                                        DialogMessage m = new DialogMessage("Shop Error", "Selected amount exceeds tank capacity");
        //                                        ModCommunication.SendMessageTo(m, identity.Character.ControlSteamId);
        //                                        return false;
        //                                    }
        //                                    double newpercent = (newamount / tank2.Capacity) * 100;
        //                                    Log.Info((tank2.FilledRatio * (double)tank2.Capacity).ToString());
        //                                    double test = (tank2.FilledRatio / (double)tank2.Capacity) * 100;
        //                                    test += newamount;
        //                                    if (tank2.FilledRatio + (test / tank2.Capacity) * 100 > tank2.Capacity)
        //                                    {
        //                                        double canFill = (tank2.FilledRatio / tank2.Capacity) * 100;
        //                                        filledAmount += newamount / 100;
        //                                        double localCost = storeItem.PricePerUnit * (canFill / 100);
        //                                        tank2.ChangeFillRatioAmount(tank2.FilledRatio + (canFill / tank2.Capacity) * 100);
        //                                        EconUtils.takeMoney(playerAccountInfo.OwnerIdentifier, (long)localCost);
        //                                        EconUtils.addMoney(__instance.OwnerId, (long)localCost);
        //                                        playerItem.Amount -= (int)canFill / 100;
        //                                        //  MyIdentity identity = MySession.Static.Players.TryGetIdentity(tank2.OwnerId);
        //                                        //   DialogMessage m = new DialogMessage("Shop Error", "Selected amount exceeds tank capacity");
        //                                        //  ModCommunication.SendMessageTo(m, identity.Character.ControlSteamId);
        //                                        //  return false;
        //                                    }
        //                                    else
        //                                    {
        //                                        if (totalGas >= test)
        //                                        {
        //                                            cost += storeItem.PricePerUnit * (newamount / 100);
        //                                            totalGas -= newamount;
        //                                           double localCost = storeItem.PricePerUnit * (newamount / 100);
        //                                            if (playerAccountInfo.Balance >= localCost)
        //                                            {
        //                                                filledAmount += newamount / 100;
        //                                                tank2.ChangeFillRatioAmount(tank2.FilledRatio + (test / tank2.Capacity) * 100);
        //                                                EconUtils.takeMoney(playerAccountInfo.OwnerIdentifier, (long) localCost);
        //                                                playerItem.Amount -= (int) filledAmount / 100;
        //                                                EconUtils.addMoney(__instance.OwnerId, (long)localCost);
        //                                            }
        //                                        }
        //                                    }




        //                                    //loop through tanks, check if each tanks can take the full amount, if it can fill it and charge the user
        //                                    //if it cant, fill what it can and charge the user then move onto the next tank



        //                                    //  object[] MethodInput = new object[] { newamount };
        //                                    //  fillTank.Invoke(tank2, MethodInput);
        //                                }
        //                            }
        //                        }
        //                        if (cost > 0)
        //                        {
        //                            yeetHydrogen(blockList, filledAmount, __instance.OwnerId);
        //                            return false;
        //                        }
        //                        else
        //                        {
        //                            return true;
        //                        }
        //                    }

        //                    else
        //                    {
        //                        if (entity is MyCharacter character)
        //                        {

        //                            DialogMessage m = new DialogMessage("Shop Error", "Hydrogen credits must be bought to a cargo container of the target ship");
        //                            ModCommunication.SendMessageTo(m, identity.Character.ControlSteamId);
        //                        }
        //                        return false;
        //                    }
        //                }



        //                //   var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
        //                //   var blockList = new List<Sandbox.ModAPI.IMyStoreBlock>();
        //                //    gts.GetBlocksOfType<Sandbox.ModAPI.IMyStoreBlock>(blockList);
        //                // foreach (Sandbox.ModAPI.IMyStoreBlock store in blockList)

        //            }
        //        }

        //        return true;
        //    }
        //}

        public static List<long> ids = new List<long>();
        public static Logger Log = LogManager.GetCurrentClassLogger();
        public static ConfigFile file;
        public static string path;
        public Dictionary<long, CurrentCooldown> CurrentCooldownMap { get; } = new Dictionary<long, CurrentCooldown>();
        public Dictionary<long, CurrentCooldown> CurrentCooldownMap2 { get; } = new Dictionary<long, CurrentCooldown>();
        public long Cooldown { get { return file.CooldownInSeconds * 1000; } }
        public long CooldownRespawn { get { return file.RespawnCooldownInSeconds * 1000; } }
        private static TorchSessionState derp;
        public Dictionary<long, CurrentCooldown> CurrentCooldownMapFix { get; } = new Dictionary<long, CurrentCooldown>();

        private static Timer aTimer = new Timer();

        //public void test(IPlayer p)
        //{
        //    if (file != null && file.SortGPSOnJoin)
        //    {

        //        if (p == null)
        //        {
        //            return;
        //        }
        //        MyIdentity id = GetIdentityByNameOrId(p.SteamId.ToString());
        //        if (id == null)
        //        {
        //            return;
        //        }

        //        List<IMyGps> playergpsList = MyAPIGateway.Session?.GPS.GetGpsList(id.IdentityId);
        //        bool hasSorting = false;
        //        if (playergpsList == null)
        //        {
        //            return;
        //        }
        //        foreach (IMyGps gps in playergpsList)
        //        {
        //            if (gps != null && gps.Name != null && gps.Name.StartsWith("#"))
        //            {
        //                hasSorting = true;
        //            }
        //        }
        //        if (hasSorting)
        //        {

        //            Dictionary<int, IMyGps> someOrganisation = new Dictionary<int, IMyGps>();
        //            List<IMyGps> unsorted = new List<IMyGps>();
        //            int highest = 0;

        //            foreach (IMyGps gps in playergpsList)
        //            {
        //                if (gps != null && gps.Name != null && gps.Name.StartsWith("#"))
        //                {

        //                    String part1 = gps.Name.Split(' ')[0].Replace("#", "");
        //                    if (int.TryParse(part1, out int result))
        //                    {
        //                        if (!someOrganisation.ContainsKey(result))
        //                        {
        //                            if (result > highest)
        //                            {
        //                                highest = result;
        //                            }
        //                            if (result <= 100)
        //                            {
        //                                someOrganisation.Add(result, gps);
        //                            }
        //                            else
        //                            {
        //                                unsorted.Add(gps);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            unsorted.Add(gps);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        unsorted.Add(gps);
        //                    }

        //                }
        //                else
        //                {
        //                    unsorted.Add(gps);
        //                }


        //            }

        //            foreach (IMyGps g in playergpsList)
        //            {
        //                MyAPIGateway.Session?.GPS.RemoveGps(id.IdentityId, g);
        //            }
        //            MyGpsCollection gpsCollection = (MyGpsCollection)MyAPIGateway.Session?.GPS;

        //            if (highest > 100)
        //            {
        //                highest = 100;
        //            }
        //            if (unsorted.Count > 0)
        //            {
        //                foreach (IMyGps gps in unsorted)
        //                {
        //                    MyGps gpsRef = gps as MyGps;
        //                    long entityId = 0L;
        //                    entityId = gpsRef.EntityId;
        //                    gpsCollection.SendAddGps(id.IdentityId, ref gpsRef, entityId, false);
        //                }
        //            }
        //            if (someOrganisation.Count > 0)
        //            {
        //                for (int i = highest; i > 0; i--)
        //                {
        //                    if (someOrganisation.ContainsKey(i))
        //                    {
        //                        someOrganisation.TryGetValue(i, out IMyGps gps);
        //                        MyGps gpsRef = gps as MyGps;
        //                        long entityId = 0L;
        //                        entityId = gpsRef.EntityId;
        //                        gpsCollection.SendAddGps(id.IdentityId, ref gpsRef, entityId, false);

        //                    }
        //                }
        //            }


        //        }
        //    }
        // }

        public static Dictionary<long, long> attackers = new Dictionary<long, long>();
        private void DamageCheck(object target, ref MyDamageInformation info)
        {
            
            try
            {

                if (!(target is MySlimBlock block))
                    return;

                MyCubeBlock cubeBlock = block.FatBlock;

                if (cubeBlock == null)
                    return;

                if (cubeBlock as MyTerminalBlock == null)
                    return;

                if (cubeBlock.EntityId == 0L)
                    return;

                if (GetAttacker(info.AttackerId) > 0L)
                {
                    attackers.Remove(cubeBlock.EntityId);
                   attackers.Add(cubeBlock.EntityId, GetAttacker(info.AttackerId));
                }
                else
                {
                    return;
                }
              


                } catch (Exception e) {
                Log.Error(e, "Error on Checking Damage!");
            }
        }
        public long GetAttacker(long attackerId)
        {

            var entity = MyAPIGateway.Entities.GetEntityById(attackerId);

            if (entity == null)
                return 0L;

            if (entity is MyPlanet)
            {

                return 0L;
            }

            if (entity is MyCharacter character)
            {

                  return character.GetPlayerIdentityId();
            }

            if (entity is IMyEngineerToolBase toolbase)
            {

                      return toolbase.OwnerIdentityId;
              
            }

            if (entity is MyLargeTurretBase turret)
            {

                return turret.OwnerId;

            }

            if (entity is MyShipToolBase shipTool)
            {

                return shipTool.OwnerId;
            }

            if (entity is IMyGunBaseUser gunUser)
            {

                return gunUser.OwnerId;

            }

       

            if (entity is MyCubeGrid grid)
            {

                var gridOwnerList = grid.BigOwners;
                var ownerCnt = gridOwnerList.Count;
                var gridOwner = 0L;

                if (ownerCnt > 0 && gridOwnerList[0] != 0)
                    gridOwner = gridOwnerList[0];
                else if (ownerCnt > 1)
                    gridOwner = gridOwnerList[1];

                return gridOwner;
     
            }

            return 0L;
        }

 
        public static ConfigFile LoadConfig()
        {
            FileUtils utils = new FileUtils();
            file = utils.ReadFromXmlFile<ConfigFile>(path + "\\CrunchUtils.xml");
            if (file.IdentityUpdate)
            {
                aTimer.Enabled = false;
                aTimer.Interval = 150000;
                aTimer.Elapsed += OnTimedEventA;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
 
            return file;
        }
        public static ConfigFile SaveConfig()
        {
            FileUtils utils = new FileUtils();
            utils.WriteToXmlFile<ConfigFile>(path + "\\CrunchUtils.xml", file);
            if (file.IdentityUpdate)
            {
                aTimer.Enabled = false;
                aTimer.Interval = 150000;
                aTimer.Elapsed += OnTimedEventA;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
            return file;
        }
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            Log.Info("Loading Crunch Utils");
            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            SetupConfig();
            if (sessionManager != null)
            {
                sessionManager.SessionStateChanged += SessionChanged;
            }
            else
            {
                Log.Warn("No session manager loaded!");
            }

        }
        private void SetupConfig()
        {
            FileUtils utils = new FileUtils();
            path = StoragePath;
            if (File.Exists(StoragePath + "\\CrunchUtils.xml"))
            {
                file = utils.ReadFromXmlFile<ConfigFile>(StoragePath + "\\CrunchUtils.xml");
                utils.WriteToXmlFile<ConfigFile>(StoragePath + "\\CrunchUtils.xml", file, false);
            }
            else { 
                file = new ConfigFile();
                utils.WriteToXmlFile<ConfigFile>(StoragePath + "\\CrunchUtils.xml", file, false);
            }
            if (file.IdentityUpdate)
            {
                aTimer.Enabled = false;
                aTimer.Interval = 150000;
                aTimer.Elapsed += OnTimedEventA;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
        }

        //method from lord tylus
        public static MyIdentity GetIdentityByNameOrId(string playerNameOrSteamId)
        {
            foreach (var identity in MySession.Static.Players.GetAllIdentities())
            {
                if (identity.DisplayName == playerNameOrSteamId)
                    return identity;
                if (ulong.TryParse(playerNameOrSteamId, out ulong steamId))
                {
                    ulong id = MySession.Static.Players.TryGetSteamId(identity.IdentityId);
                    if (id == steamId)
                        return identity;
                }
            }
            return null;
        }

        private static void OnTimedEventA(Object source, System.Timers.ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                UpdateNamesTask();
            });
        }
        private static void OnTimedEventB(Object source, System.Timers.ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                DoJumpDriveShit();
            });
        }
        public static void DoJumpDriveShit()
        {
            foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
            {
                
            }
        }
        public static void UpdateNamesTask()
        {
          
            
        //    if (derp == TorchSessionState.Loaded) {
                Log.Info("Updating names");
                foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                {
                    string name = MyMultiplayer.Static.GetMemberName(player.Id.SteamId);
                    MyIdentity identity = GetIdentityByNameOrId(player.Id.SteamId.ToString());

                    if (!player.DisplayName.Equals(name))
                    {
                      
                      
                        identity.SetDisplayName(MyMultiplayer.Static.GetMemberName(player.Id.SteamId));

                    }
               // }
            }
        }
        private void SessionChanged(ITorchSession session, TorchSessionState newState)
        {
            //Do something here in the future
            Log.Info("Session-State is now " + newState);
            if (newState == TorchSessionState.Loaded)
            {
                derp = TorchSessionState.Loaded;
                //  session.Managers.GetManager<IMultiplayerManagerBase>().PlayerJoined += test;
               // MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageCheck);
            }

        }
    }
}
