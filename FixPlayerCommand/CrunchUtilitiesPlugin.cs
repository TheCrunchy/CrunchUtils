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

namespace CrunchUtilities
{
    public class CrunchUtilitiesPlugin : TorchPluginBase
    {

        public static Logger Log = LogManager.GetCurrentClassLogger();
        public static ConfigFile file;
        private static string path;
        public Dictionary<long, CurrentCooldown> CurrentCooldownMap { get; } = new Dictionary<long, CurrentCooldown>();
         public long Cooldown { get { return file.CooldownInSeconds * 1000; } }
        public static ConfigFile LoadConfig()
        {
            FileUtils utils = new FileUtils();
            file = utils.ReadFromXmlFile<ConfigFile>(path + "\\CrunchUtils.xml");
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
            }
            else { 
                file = new ConfigFile();
                utils.WriteToXmlFile<ConfigFile>(StoragePath + "\\CrunchUtils.xml", file, false);
            }

        }

        private void SessionChanged(ITorchSession session, TorchSessionState newState)
        {
            //Do something here in the future
            Log.Info("Session-State is now " + newState);
            if (newState == TorchSessionState.Loaded)
            {
                
            }

        }
    }
}
