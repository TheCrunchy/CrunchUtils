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
using Sandbox.Game.GameSystems;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Weapons.Guns;
using VRage.Utils;
using Sandbox.Engine.Physics;
using Sandbox.Definitions;
using System.Linq;

namespace CrunchUtilities
{


    public class CrunchUtilitiesPlugin : TorchPluginBase
    {
    
        public static Dictionary<long, long> moneyToPay = new Dictionary<long, long>();



    
        [PatchShim]
        public static class MyPatch
        {
          
            public static readonly Logger Log = LogManager.GetCurrentClassLogger();
            private static List<MyPhysics.HitInfo> m_castList = new List<MyPhysics.HitInfo>();
            internal static readonly MethodInfo update =
                typeof(MyDrillBase).GetMethod("TryHarvestOreMaterial", BindingFlags.Instance | BindingFlags.NonPublic) ??
                throw new Exception("Failed to find patch method");

            internal static readonly MethodInfo updatePatch =
                typeof(MyPatch).GetMethod(nameof(TestPatchMethod), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");


    //        internal static readonly MethodInfo testUpdate =
    //typeof(MyDrillBase).GetMethod("DrillVoxel", BindingFlags.Instance | BindingFlags.NonPublic) ??
    //throw new Exception("Failed to find patch method");

    //        internal static readonly MethodInfo testUpdatePatch =
    //            typeof(MyPatch).GetMethod(nameof(TestPatchMethod2), BindingFlags.Static | BindingFlags.Public) ??
    //            throw new Exception("Failed to find patch method");

            public static void Patch(PatchContext ctx)
            {

                ctx.GetPattern(update).Prefixes.Add(updatePatch);
              //  ctx.GetPattern(testUpdate).Prefixes.Add(testUpdatePatch);
                Log.Info("Patching Successful CrunchDrill!");
            }

      //      public static bool TestPatchMethod2(MyDrillBase __instance, MyDrillSensorBase.DetectionInfo entry,
      //bool collectOre,
      //bool performCutout,
      //bool assignDamagedMaterial,
      //ref MyStringHash targetMaterial)
      //      {
      //        //  return false;
      //          if (__instance.OutputInventory.Owner.GetBaseEntity() is MyShipDrill drill)
      //          {
      //              MyVoxelBase entity = entry.Entity as MyVoxelBase;
      //              Vector3D worldPosition = entry.DetectionPoint;
      //              bool flag1 = false;
      //              int count = 0;
      //              CrunchUtilitiesPlugin.Log.Info(count + "");
      //              count++;
                 

      //                  CrunchUtilitiesPlugin.Log.Info(count + "");
      //                  count++;
      //                  MyVoxelMaterialDefinition material = (MyVoxelMaterialDefinition)null;
      //                  Vector3D center = __instance.CutOut.Sphere.Center;
      //                  MatrixD worldMatrix = drill.WorldMatrix;
      //                  Vector3D forward = worldMatrix.Forward;
      //                  Vector3D from = center - forward;
      //                  worldMatrix = drill.WorldMatrix;
      //                  CrunchUtilitiesPlugin.Log.Info(count + "");
      //                  count++;
      //                  MyPhysics.CastRay(from, from + worldMatrix.Forward * (__instance.CutOut.Sphere.Center + 1.0), m_castList, 28);
      //                  CrunchUtilitiesPlugin.Log.Info(count + "");
      //                  count++;
      //                  bool flag2 = false;
      //                  CrunchUtilitiesPlugin.Log.Info(count + "");
      //                  count++;
      //                  foreach (MyPhysics.HitInfo cast in m_castList)
      //                  {
      //                      CrunchUtilitiesPlugin.Log.Info(count + "");
      //                      count++;
      //                      if (cast.HkHitInfo.GetHitEntity() is MyVoxelBase)
      //                      {
      //                          worldPosition = cast.Position;
      //                      MyVoxelMaterialDefinition material2;
                 
      //                          material2 = entity.GetMaterialAt(ref worldPosition);
      //                      if (material2 == null)
      //                      {
                                
      //                      }
      //                      MyObjectBuilder_Ore newObject = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(material2.MinedOre);
      //                      if (!newObject.SubtypeName.ToLower().Contains("stone"))
      //                      {
      //                          material = material2;
      //                          flag2 = true;
      //                          break;
      //                      }
      //                      }
      //                  }
      //                  if (material == null)
      //              {
      //                  return false;
      //              }
      //                  CrunchUtilitiesPlugin.Log.Info(count + "");
      //                  count++;
      //                  MyObjectBuilder_Ore newObject2 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(material.MinedOre);
      //                  if (newObject2.SubtypeName.ToLower().Contains("stone"))
      //                  {
      //                      CrunchUtilitiesPlugin.Log.Info("It be stone");
      //                      return false;
      //                  }
      //                  return true;
                    
      //          }
      //          return true;
      //      }
                public static bool TestPatchMethod(MyDrillBase __instance, MyVoxelMaterialDefinition material, Vector3 hitPosition)
            {
             
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
                    if (__instance.OutputInventory.Owner.GetBaseEntity() is MyShipDrill drill)
                    {
     
                        if (drill == null)
                        {
                            return true;
                        }

   
                        if (drill.DisplayNameText != null && containsName(drill.DisplayNameText))
                        {
                            MyObjectBuilder_Ore newObject = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(material.MinedOre);
                            if (newObject.SubtypeName.ToLower().Contains("stone"))
                            {
                                //if (file.UsingDraconisEliteDrills)
                                //{
                                //    if (!drill.BlockDefinition.BlockPairName.Equals("Drill8x"))
                                //    {
                                //        return true;
                                //    }

                                //}
                                return false;
                            }
                        }
                        if (ids.Contains(drill.OwnerId))
                        {
                            MyObjectBuilder_Ore newObject = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(material.MinedOre);
                            if (newObject.SubtypeName.ToLower().Contains("stone"))
                            {
                                //if (file.UsingDraconisEliteDrills)
                                //{
                                //    if (!drill.BlockDefinition.BlockPairName.Equals("Drill8x"))
                                //    {
                                //        return true;
                                //    }
                                //}
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
        }
        public static bool containsName(String name)
        {
            if (name.ToLower().Contains("no stone") || name.ToLower().Contains("!stone"))
            {
                return true;
            }
            return false;
        }

 

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
        private static Dictionary<long, DateTime> blockCooldowns = new Dictionary<long, DateTime>();
        private static int ticks = 0;
        public static void Update()
        {
            ticks++;
            List<long> idsToRemove = new List<long>();
            if (ticks % 524 == 0)
            {
                foreach (KeyValuePair<long, DateTime> pair in blockCooldowns)
                {
                    if (DateTime.Now >= pair.Value)
                    {
                        idsToRemove.Add(pair.Key);
                    }
                }
                foreach (long id in idsToRemove)
                {
                    blockCooldowns.Remove(id);
                }
            }
        }
        public static Dictionary<long, long> attackers = new Dictionary<long, long>();
        private void DamageCheck(object target, ref MyDamageInformation info)
        {
            if (file != null && file.LogNeutralsDamagingEachOther)
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
                        long attackerId = GetAttacker(info.AttackerId);
                        MyIdentity id = MySession.Static.Players.TryGetIdentity(attackerId);
                        if (FacUtils.GetPlayersFaction(id.IdentityId) != null)
                        {

                            IMyFaction attacker = FacUtils.GetPlayersFaction(id.IdentityId);
                            IMyFaction defender = FacUtils.GetPlayersFaction(cubeBlock.OwnerId);
                            if (attacker == null || defender == null)
                            {
                                return;
                            }

                            if (attacker == defender)
                            {
                                return;
                            }

                            if (MySession.Static.Factions.AreFactionsFriends(attacker.FactionId, defender.FactionId) || MySession.Static.Factions.AreFactionsNeutrals(attacker.FactionId, defender.FactionId))
                            {
                                if (blockCooldowns.TryGetValue(cubeBlock.EntityId, out DateTime time))
                                {
                                    if (DateTime.Now < time)
                                    {
                                        return;
                                    }
                                }

                                blockCooldowns.Remove(cubeBlock.EntityId);
                                blockCooldowns.Add(cubeBlock.EntityId, DateTime.Now.AddSeconds(10));
                                CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Attacking while not at war " + attackerId + " " + attacker.Tag + " " + attacker.FactionId + " against " + cubeBlock.CubeGrid.DisplayName + ", " + defender.Tag + " " + defender.FactionId);
                            }



                        }
                        else
                        {
                            return;
                        }


                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error on Checking Damage!");
                }
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
                    if (identity.IdentityId == (long) steamId)
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
            try
            {
                Log.Info("Updating names");
                if (derp == TorchSessionState.Loaded && MySession.Static.Players.GetOnlinePlayers().Count > 0)
                {
                    foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                    {
                        if (player == null || player.Id == null)
                        {
                            break;
                        }
             
                        string name = MyMultiplayer.Static.GetMemberName(player.Id.SteamId);
                        if (name == null)
                        {
                            break;
                        }
                        MyIdentity identity = GetIdentityByNameOrId(player.Id.SteamId.ToString());
                        if (identity == null)
                        {
                            break;
                        }
                        if (player.Character != null && player.Character.DisplayName != null && !player.DisplayName.Equals(name))
                        {


                            identity.SetDisplayName(MyMultiplayer.Static.GetMemberName(player.Id.SteamId));

                        }
                        // }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info("Error on updating names");
                Log.Error(ex.ToString());
                return;
            }
        }
        private void SessionChanged(ITorchSession session, TorchSessionState newState)
        {
            //Do something here in the future
            Log.Info("Session-State is now " + newState);
            if (newState == TorchSessionState.Loaded)
            {
                derp = TorchSessionState.Loaded;
                MySession.Static.Factions.FactionStateChanged += FactionLogging.StateChange;
                MyBankingSystem.Static.OnAccountBalanceChanged += BankPatch.BalanceChangedMethod2;
                
                //  session.Managers.GetManager<IMultiplayerManagerBase>().PlayerJoined += test;
                MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageCheck);
            }

        }
    }
}
