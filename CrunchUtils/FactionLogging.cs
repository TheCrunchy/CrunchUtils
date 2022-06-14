using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using Sandbox.Game.World;
using VRage.Game.ModAPI;

namespace CrunchUtilities
{
    public static class FactionLogging
    {


        //public static void MemberJoined(MyFaction fac, long id)
        //{
        //    log.Info("Faction join : " + fac.Tag + " " + fac.Name + " " + fac.FactionId + " PLAYER ID " + id);
        //}
        //public static void MemberLeft(MyFaction fac, long id)
        //{
        //    log.Info("Faction leaving : " + fac.Tag + " " + fac.Name + " " + fac.FactionId + " PLAYER ID " + id);
        //}
        //public static void Created(long id)
        //{
        //    if (MySession.Static.Factions.TryGetFactionById(id) != null)
        //    {
        //        IMyFaction fac = MySession.Static.Factions.TryGetFactionById(id);
        //        log.Info("Faction created : " + fac.Tag + " " + fac.Name + " " + fac.FactionId);
        //    }
        //}


        public static Logger log = LogManager.GetLogger("FactionLog");
        public static void ApplyLogging()
        {

            var rules = LogManager.Configuration.LoggingRules;

            for (int i = rules.Count - 1; i >= 0; i--)
            {

                var rule = rules[i];

                if (rule.LoggerNamePattern == "FactionLog")
                    rules.RemoveAt(i);
            }



            var logTarget = new FileTarget
            {
                FileName = "Logs/FactionLog-" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + ".txt",
                Layout = "${var:logStamp} ${var:logContent}"
            };

            var logRule = new LoggingRule("FactionLog", LogLevel.Debug, logTarget)
            {
                Final = true
            };

            rules.Insert(0, logRule);

            LogManager.Configuration.Reload();
        }


        public static void StateChange(MyFactionStateChange change, long fromFacId, long toFacId, long playerId, long senderId)
        {
            if (CrunchUtilitiesPlugin.file != null && CrunchUtilitiesPlugin.file.LogFactionStuff)
            {
                IMyFaction fac1;
                IMyFaction fac2;
                switch (change)
                {
                    case MyFactionStateChange.AcceptPeace:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        fac2 = MySession.Static.Factions.TryGetFactionById(toFacId);
                        if (fac1 != null && fac2 != null)
                        {
                            log.Info("FACTIONLOG Accepting peace between " + fac1.Tag + " " + fac1.FactionId + " and " + fac2.Tag + " " + fac2.FactionId + " Requested by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.DeclareWar:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        fac2 = MySession.Static.Factions.TryGetFactionById(toFacId);
                        if (fac1 != null && fac2 != null)
                        {
                            log.Info("FACTIONLOG Declaring war between " + fac1.Tag + " " + fac1.FactionId + " and " + fac2.Tag + " " + fac2.FactionId + " Requested by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberPromote:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            log.Info("FACTIONLOG Promotion " + fac1.Tag + " " + fac1.FactionId + " target " + playerId + " by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberDemote:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            log.Info("FACTIONLOG Demotion " + fac1.Tag + " " + fac1.FactionId + " target " + playerId + " by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberKick:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            log.Info("FACTIONLOG Kicked member " + fac1.Tag + " " + fac1.FactionId + " target " + playerId + " by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.RemoveFaction:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            log.Info("FACTIONLOG Deleted faction, heres some IDs, " + fromFacId + " " + playerId + " " + senderId);
                        }
                        break;
                    case MyFactionStateChange.SendFriendRequest:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        fac2 = MySession.Static.Factions.TryGetFactionById(toFacId);
                        if (fac1 != null && fac2 != null)
                        {
                            log.Info("FACTIONLOG Sending friend request " + fac1.Tag + " " + fac1.FactionId + " and " + fac2.Tag + " " + fac2.FactionId + " Requested by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.AcceptFriendRequest:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        fac2 = MySession.Static.Factions.TryGetFactionById(toFacId);
                        if (fac1 != null && fac2 != null)
                        {
                            log.Info("FACTIONLOG Accepting friend request " + fac1.Tag + " " + fac1.FactionId + " and " + fac2.Tag + " " + fac2.FactionId + " Requested by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberAcceptJoin:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            log.Info("FACTIONLOG Accepted join " + fac1.Tag + " " + fac1.FactionId + " target " + playerId + " by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberCancelJoin:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            log.Info("FACTIONLOG Cancel join " + fac1.Tag + " " + fac1.FactionId + " by " + playerId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberSendJoin:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            log.Info("FACTIONLOG Send join request " + fac1.Tag + " " + fac1.FactionId + " by " + playerId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberLeave:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            log.Info("FACTIONLOG Member leaving " + fac1.Tag + " " + fac1.FactionId + " target " + playerId);
                        }
                        break;
                }
            }
        }
    }
}
