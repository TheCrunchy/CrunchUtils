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
using Sandbox.Game;
using Torch.Managers;
using Torch.API.Plugins;

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


            public static void Patch(PatchContext ctx)
            {

                ctx.GetPattern(update).Prefixes.Add(updatePatch);
                //  ctx.GetPattern(testUpdate).Prefixes.Add(testUpdatePatch);
                Log.Info("Patching Successful CrunchDrill!");

            }

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
                                //if (!ids.Contains(drill.OwnerId))
                                //{
                                //    ids.Add(drill.OwnerId);
                                //}

                                return false;
                            }
                        }
                        if (ids.Contains(drill.OwnerId))
                        {
                            MyObjectBuilder_Ore newObject = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(material.MinedOre);
                            if (newObject.SubtypeName.ToLower().Contains("stone"))
                            {
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

        public static void BanPlayer(ulong steamId, DateTime time)
        {
            BanManager.BanPlayer(steamId, true);
            BanList.TempBans.Add(new TempBanItem() { SteamId = steamId, UnbannedAfter = time });

            Log.Info($"Banning {steamId} untils {time}");
        }
        public static FileUtils utils = new FileUtils();
        public static void LoadTempBans()
        {
            if (File.Exists($"{path}//TempBans.xml"))
            {
                BanList = utils.ReadFromXmlFile<TempBanList>($"{path}//TempBans.xml");
            }
        }
        private static readonly Guid NexusGUID = new Guid("28a12184-0422-43ba-a6e6-2e228611cca5");
        public static bool NexusInstalled { get; private set; } = false;
        public static NexusAPI API;
        public void InitPluginDependencies(PluginManager Plugins, PatchManager Patches)
        {
            //if (Plugins.Plugins.TryGetValue(NexusGUID, out ITorchPlugin torchPlugin))
            //{
            //    Type type = torchPlugin.GetType();
            //    Type type2 = ((type != null) ? type.Assembly.GetType("Nexus.API.PluginAPISync") : null);
            //    if (type2 != null)
            //    {
            //        type2.GetMethod("ApplyPatching", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,
            //            new object[]
            //            {
            //                typeof(NexusAPI),
            //                "CrunchUtils"
            //            });
            //        API = new NexusAPI(2498);
            //        MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(2498,
            //            new Action<ushort, byte[], ulong, bool>(HandleNexusMessage));
            //        NexusInstalled = true;
            //    }
            //}

        }

        public static Stack<long> IdsToYEET = new Stack<long>();
        private static void HandleNexusMessage(ushort handlerId, byte[] data, ulong steamID, bool fromServer)
        {
            //var message = MyAPIGateway.Utilities.SerializeFromBinary<EconSync>(data);
            //foreach (var item in message.SyncThis)
            //{
            //    var player = item.Key;
            //    var balance = item.Value;

            //    if (MyBankingSystem.Static.TryGetAccountInfo(player, out var account))
            //    {
            //        MyBankingSystem.RemoveAccount_Clients(player);
            //        MyBankingSystem.CreateAccount_Clients(player, balance);
            //    }

            //}
        }

        public static void SaveTempBans()
        {
            utils.WriteToXmlFile<TempBanList>($"{path}//TempBans.xml", BanList);
        }

        public static List<long> ids = new List<long>();
        //    public static Logger EconLog = LogManager.GetLogger("Econ");
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
        public static List<MyFunctionalBlock> blocksToTurnOff = new List<MyFunctionalBlock>();

        private static DateTime UpdateTime = DateTime.Now;
        private static DateTime PlayerAlertNext = DateTime.Now;
        public static Boolean AlliancesInstalled = false;
        public static TempBanList BanList = new TempBanList() { TempBans = new List<TempBanItem>() };

        public static IMultiplayerManagerServer BanManager;
        private bool InitPlugins = false;
        public override void Update()

        {
            //if (!InitPlugins)
            //{
            //    InitPluginDependencies(Torch.Managers.GetManager<PluginManager>(), Torch.Managers.GetManager<PatchManager>());
            //    InitPlugins = true;
            //}
            ticks++;

            if (ticks % 20 == 0)
            {
                if (IdsToYEET.Any())
                {
                    var yeet = IdsToYEET.Pop();
                    var balance = EconUtils.GetBalance(yeet);
                    MyBankingSystem.Static.RemoveAccount(yeet);
                    MyBankingSystem.Static.CreateAccount(yeet, balance);
                }
            }

            if (ticks % 524 == 0)
            {


                List<long> idsToRemove = new List<long>();
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
                List<long> expiredOffers = new List<long>();
                foreach (KeyValuePair<long, ShipOffer> offers in Commands.saleOffers)
                {
                    if (DateTime.Now >= offers.Value.TimeOfOffer)
                    {
                        expiredOffers.Add(offers.Key);
                    }
                }
                foreach (long id in expiredOffers)
                {
                    Commands.saleOffers.Remove(id);
                }
            }
            if (ticks % 10000 == 0 && file != null && file.IdentityUpdate)
            {
                try
                {

                    if (derp == TorchSessionState.Loaded && MySession.Static.Players.GetOnlinePlayers().Count > 0)
                    {
                        foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                        {
                            if (player == null || player.Id == null)
                                continue;

                            string name = MyMultiplayer.Static.GetMemberName(player.Id.SteamId);
                            if (name == null || string.IsNullOrEmpty(name))
                                continue;

                            MyIdentity identity = player.Identity;
                            if (identity == null)
                                continue;

                            if (player.Character != null && player.Character.DisplayName != null && !player.DisplayName.Equals(name))
                            {

                                identity.SetDisplayName(name);

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
        }

        public static Dictionary<long, NotificationMessage> attackMessages = new Dictionary<long, NotificationMessage>();
        public static void SendAttackNotification(IMyFaction attacker, IMyFaction defender, long attackerId, ulong playerSteamid, Vector3 position)
        {
            if (defender != null)
            {
                if (attacker != null && !attacker.Equals(defender))
                {
                    if (blockCooldowns.TryGetValue(attackerId, out DateTime time))
                    {
                        if (DateTime.Now >= time)
                        {
                            if (MySession.Static.Players.TryGetPlayerBySteamId(playerSteamid) != null)
                            {
                                Vector3 playerPos = MySession.Static.Players.TryGetPlayerBySteamId(playerSteamid).GetPosition();
                                float distance = Vector3.Distance(playerPos, position);
                                if (distance > 10000)
                                {
                                    return;
                                }
                            }
                            NotificationMessage message;

                            message = new NotificationMessage("You are attacking " + defender.Tag, 5000, "Red");
                            //this is annoying, need to figure out how to check the exact world time so a duplicate message isnt possible
                            ModCommunication.SendMessageTo(message, playerSteamid);
                            blockCooldowns.Remove(attackerId);
                            blockCooldowns.Add(attackerId, DateTime.Now.AddSeconds(11));

                        }
                    }
                    else
                    {

                        if (MySession.Static.Players.TryGetPlayerBySteamId(playerSteamid) != null)
                        {
                            Vector3 playerPos = MySession.Static.Players.TryGetPlayerBySteamId(playerSteamid).GetPosition();
                            float distance = Vector3.Distance(playerPos, position);
                            if (distance > 10000)
                            {
                                return;
                            }
                        }

                        blockCooldowns.Remove(attackerId);
                        blockCooldowns.Add(attackerId, DateTime.Now.AddSeconds(11));
                        ModCommunication.SendMessageTo(new NotificationMessage("You are attacking " + defender.Tag, 5000, "Red"), playerSteamid);

                    }
                }
            }
            return;
        }
        public static Dictionary<long, long> attackers = new Dictionary<long, long>();
        private void DamageCheck(object target, ref MyDamageInformation info)
        {
            if (file != null)
            {

                if (file.LogNeutralsDamagingEachOther || file.ShowFactionTagsOnDamageGrid)
                    try
                    {
                        if (!(target is MySlimBlock block))
                            return;
                        long attackerId = GetAttacker(info.AttackerId);
                        MyIdentity id = MySession.Static.Players.TryGetIdentity(attackerId);
                        if (id == null)
                        {
                            return;
                        }
                        IMyFaction defender = FacUtils.GetPlayersFaction(FacUtils.GetOwner(block.CubeGrid));
                        IMyFaction attacker = FacUtils.GetPlayersFaction(id.IdentityId);
                        if (file.ShowFactionTagsOnDamageGrid)
                        {

                            if (Sync.Players.TryGetPlayerId(id.IdentityId, out MyPlayer.PlayerId player))
                            {
                                if (MySession.Static.Players.GetPlayerById(player) != null)
                                {
                                    SendAttackNotification(attacker, defender, attackerId, player.SteamId, block.CubeGrid.PositionComp.GetPosition());
                                }
                            }
                        }


                        MyCubeBlock cubeBlock = block.FatBlock;
                        if (cubeBlock == null)
                            return;



                        if (cubeBlock as MyTerminalBlock == null)
                            return;

                        if (cubeBlock.EntityId == 0L)
                            return;

                        if (GetAttacker(info.AttackerId) > 0L)
                        {




                            //this is so messy 
                            if (attacker != null && defender != null)
                            {
                                if (attacker.Equals(defender))
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
                                    if (file.ShowFactionTagsOnDamageGrid)
                                    {
                                        blockCooldowns.Remove(cubeBlock.EntityId);
                                        blockCooldowns.Add(cubeBlock.EntityId, DateTime.Now.AddSeconds(10));
                                        CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Attacking while not at war " + attackerId + " " + attacker.Tag + " " + attacker.FactionId + " against " + cubeBlock.CubeGrid.DisplayName + ", " + defender.Tag + " " + defender.FactionId);
                                    }
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


            return file;
        }
        public static ConfigFile SaveConfig()
        {
            FileUtils utils = new FileUtils();
            utils.WriteToXmlFile<ConfigFile>(path + "\\CrunchUtils.xml", file);

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
            else
            {
                file = new ConfigFile();
                utils.WriteToXmlFile<ConfigFile>(StoragePath + "\\CrunchUtils.xml", file, false);
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
                    if (identity.IdentityId == (long)steamId)
                        return identity;
                }

            }

            return null;
        }

        public static List<IMyIdentity> GetAllIdentitiesByNameOrId(string playerNameOrSteamId)
        {
            List<IMyIdentity> ids = new List<IMyIdentity>();
            foreach (var identity in MySession.Static.Players.GetAllIdentities())
            {
                if (identity.DisplayName == playerNameOrSteamId)
                {
                    if (!ids.Contains(identity))
                    {
                        ids.Add(identity);
                    }
                }
                if (ulong.TryParse(playerNameOrSteamId, out ulong steamId))
                {
                    ulong id = MySession.Static.Players.TryGetSteamId(identity.IdentityId);
                    if (id == steamId)
                    {
                        if (!ids.Contains(identity))
                        {
                            ids.Add(identity);
                        }

                    }
                    if (identity.IdentityId == (long)steamId)
                    {
                        if (!ids.Contains(identity))
                        {
                            ids.Add(identity);
                        }
                    }
                }

            }
            return ids;
        }


        //MyInventoryBase
        //    public abstract bool AddItems(MyFixedPoint amount, MyObjectBuilder_Base objectBuilder);
        private void SessionChanged(ITorchSession session, TorchSessionState newState)
        {
            //Do something here in the future
            Log.Info("Session-State is now " + newState);
            if (newState == TorchSessionState.Loaded)
            {

                derp = TorchSessionState.Loaded;
                MySession.Static.Factions.FactionStateChanged += FactionLogging.StateChange;
                MyBankingSystem.Static.OnAccountBalanceChanged += BankPatch.BalanceChangedMethod2;
                FactionLogging.ApplyLogging();
                if (Torch.Managers.GetManager<PluginManager>().Plugins.TryGetValue(Guid.Parse("74796707-646f-4ebd-8700-d077a5f47af3"), out ITorchPlugin Alliances))
                {
                    AlliancesInstalled = true;
                }
                //  session.Managers.GetManager<IMultiplayerManagerBase>().PlayerJoined += test;
                if (file.FactionsNeutralOnCreation)
                {
                    var FactionCollection = MySession.Static.Factions.GetType().Assembly.GetType("Sandbox.Game.Multiplayer.MyFactionCollection");
                    sendChange = FactionCollection?.GetMethod("SendFactionChange", BindingFlags.NonPublic | BindingFlags.Static);

                    MySession.Static.Factions.FactionCreated += ProcessNewFaction;

                }
                MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageCheck);
           
            }

        }

        public static MethodInfo sendChange;
        public void ProcessNewFaction(long newid)
        {
            var faction = MySession.Static.Factions.TryGetFactionById(newid);
            if (faction == null) return;
            foreach (MyFaction fac in MySession.Static.Factions.GetAllFactions())
            {

                if (fac.FactionId != newid && !file.IsExcluded(fac.Tag))
                {
                    DoNeutralUpdate(faction.FactionId, fac.FactionId);
                }
            }
        }

        public void DoNeutralUpdate(long firstId, long SecondId)
        {
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                MyFactionCollection.MyFactionPeaceRequestState state = MySession.Static.Factions.GetRequestState(firstId, SecondId);
                if (state != MyFactionCollection.MyFactionPeaceRequestState.Sent)
                {
                    Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(firstId, SecondId);
                    Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(firstId, SecondId);
                }
                MyFactionStateChange change = MyFactionStateChange.SendPeaceRequest;
                MyFactionStateChange change2 = MyFactionStateChange.AcceptPeace;
                List<object[]> Input = new List<object[]>();
                object[] MethodInput = new object[] { change, firstId, SecondId, 0L };
                sendChange?.Invoke(null, MethodInput);
                object[] MethodInput2 = new object[] { change2, SecondId, firstId, 0L };
                sendChange?.Invoke(null, MethodInput2);
                MySession.Static.Factions.SetReputationBetweenFactions(firstId, SecondId, 0);
            });
        }
    }
}
