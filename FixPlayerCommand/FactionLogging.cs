using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.World;
using VRage.Game.ModAPI;

namespace CrunchUtilities
{
    public static class FactionLogging
    {


        //public static void MemberJoined(MyFaction fac, long id)
        //{
        //    CrunchUtilitiesPlugin.Log.Info("Faction join : " + fac.Tag + " " + fac.Name + " " + fac.FactionId + " PLAYER ID " + id);
        //}
        //public static void MemberLeft(MyFaction fac, long id)
        //{
        //    CrunchUtilitiesPlugin.Log.Info("Faction leaving : " + fac.Tag + " " + fac.Name + " " + fac.FactionId + " PLAYER ID " + id);
        //}
        //public static void Created(long id)
        //{
        //    if (MySession.Static.Factions.TryGetFactionById(id) != null)
        //    {
        //        IMyFaction fac = MySession.Static.Factions.TryGetFactionById(id);
        //        CrunchUtilitiesPlugin.Log.Info("Faction created : " + fac.Tag + " " + fac.Name + " " + fac.FactionId);
        //    }
        //}
 
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
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Accepting peace between " + fac1.Tag + " " + fac1.FactionId + " and " + fac2.Tag + " " + fac2.FactionId + " Requested by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.DeclareWar:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        fac2 = MySession.Static.Factions.TryGetFactionById(toFacId);
                        if (fac1 != null && fac2 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Declaring war between " + fac1.Tag + " " + fac1.FactionId + " and " + fac2.Tag + " " + fac2.FactionId + " Requested by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberPromote:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Promotion " + fac1.Tag + " " + fac1.FactionId + " target " + playerId + " by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberDemote:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Demotion " + fac1.Tag + " " + fac1.FactionId + " target " + playerId + " by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberKick:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Kicked member " + fac1.Tag + " " + fac1.FactionId + " target " + playerId + " by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.RemoveFaction:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Deleted faction, heres some IDs, " + fromFacId + " " + playerId + " " + senderId);
                        }
                        break;
                    case MyFactionStateChange.SendFriendRequest:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        fac2 = MySession.Static.Factions.TryGetFactionById(toFacId);
                        if (fac1 != null && fac2 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Sending friend request " + fac1.Tag + " " + fac1.FactionId + " and " + fac2.Tag + " " + fac2.FactionId + " Requested by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.AcceptFriendRequest:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        fac2 = MySession.Static.Factions.TryGetFactionById(toFacId);
                        if (fac1 != null && fac2 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Accepting friend request " + fac1.Tag + " " + fac1.FactionId + " and " + fac2.Tag + " " + fac2.FactionId + " Requested by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberAcceptJoin:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Accepted join " + fac1.Tag + " " + fac1.FactionId + " target " + playerId + " by " + senderId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberCancelJoin:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Cancel join " + fac1.Tag + " " + fac1.FactionId + " by " + playerId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberSendJoin:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Send join request " + fac1.Tag + " " + fac1.FactionId + " by " + playerId);
                        }
                        break;
                    case MyFactionStateChange.FactionMemberLeave:
                        fac1 = MySession.Static.Factions.TryGetFactionById(fromFacId);
                        if (fac1 != null)
                        {
                            CrunchUtilitiesPlugin.Log.Info("FACTIONLOG Member leaving " + fac1.Tag + " " + fac1.FactionId + " target " + playerId);
                        }
                        break;
                }
            }
        }
    }
}
