using HobbiesAndInterests.HelperClasses;
using Lyralei.InterestMod;
using Lyralei.UI;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Lyralei.InterestsAndHobbies;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.Visa;
using Sims3.SimIFace;
using Sims3.SimIFace.Enums;
using Sims3.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static HobbiesAndInterests.HelperClasses.EnergyManager;
using static Sims3.Gameplay.Objects.Entertainment.DunkTank;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class HobbyClubsManager
    {
        //Feature ideas: 

        // - Create Your own Club
        // - Add sims to said club (automatically asign depending on hobby, or the club owner is random.)
        // - Do activities with Club at hobby lot.

        [Persistable]
        public static string mSavedSimsClubHobbiesData;

        private static AlarmHandle sAlarmNewHobbyList = AlarmHandle.kInvalidHandle;

        public static HobbyClubsManager Instance;
        public HobbyClubsManager()
        {
            HobbyClubsManager singleton = Instance;
        }
        //public static Dictionary<ulong, List<ClubHobby>> mSimsInClubHobbies;

        private static Dictionary<string, ClubHobby> mClubsInTown;
        public static Dictionary<string, ClubHobby> ClubsInTown
        {
            get => mClubsInTown;
            set => mClubsInTown = value;
        }

        public static void Startup()
        {
            try
            {
                if (Instance == null)
                {
                    Instance = new HobbyClubsManager();
                    //mSimsInClubHobbies = new Dictionary<ulong, List<ClubHobby>>(); // Remove this after saving code stuff.
                }

                if (ClubsInTown == null || ClubsInTown.Count <= 0)
                {
                    ClubsInTown = new Dictionary<string, ClubHobby>();
                    LoadClubXML();
                }
            }
            catch (Exception ex)
            {
                InterestManager.print(ex.ToString());
            }
        }


        //private static void SaveSimsHobbyClubData()
        //{
        //    try
        //    {
        //        MemoryStream memorystream = new MemoryStream();
        //        BinaryWriter bw = new BinaryWriter(memorystream);

        //        int allEntries = mSimsInClubHobbies.Count;

        //        bw.Write(allEntries);

        //        foreach (KeyValuePair <ulong, List<ClubHobby>> kpv in mSimsInClubHobbies)
        //        {
        //            // Get sim ID first...
        //            bw.Write((ulong)kpv.Key);

        //            // Then get the list count...
        //            bw.Write(kpv.Value.Count);

        //            foreach(ClubHobby clubHobby in kpv.Value)
        //            {
        //                bw.Write(clubHobby.Name);
        //                bw.Write(clubHobby.Description);
        //                bw.Write(clubHobby.InterestType.ToString());
        //                bw.Write(clubHobby.StartTime);
        //                bw.Write(clubHobby.Duration);
        //                bw.Write(clubHobby.CanActivelyJoin);
        //                bw.Write(clubHobby.ClubOwner);
        //                bw.Write(ParserFunctions.ParseDaysFlagFieldToString(clubHobby.DaysToWork));
        //                bw.Write(clubHobby.HobbyLot.lotID); // here we store the LotId, since the rest we can repopulate...
        //                bw.Write(clubHobby.)
        //            }
        //        }

        //    }
        //    catch
        //    {

        //    }
        //}




        public static List<ClubHobby> GetListsForHobbyUI()
        {
            try
            {
                if (ClubsInTown != null && ClubsInTown.Count > 0)
                {
                    List<ClubHobby> clubs = new List<ClubHobby>();
                    List<ClubHobby> ClubsForSeed = new List<ClubHobby>();

                    foreach (KeyValuePair<string, ClubHobby> kvp in ClubsInTown) { 
                        kvp.Value.CanActivelyJoin = false;
                        ClubsForSeed.Add(kvp.Value);
                    }

                    Random r = new Random((int)SimClock.ElapsedTime(TimeUnit.Days) + Computer.RandomComputerSeed);

                    int num = 0;

                    while (num < Tunables.mHowManyHobbyClubsToGrabPerDay)
                    {
                        HobbyClubsManager.ClubHobby randomObjectFromList = RandomUtil.GetRandomObjectFromList(ClubsForSeed, r);
                        if (randomObjectFromList == null)
                        {
                            break;
                        }

                        if (randomObjectFromList.ClubOwner == 0)
                        {
                            break;
                        }
                        randomObjectFromList.CanActivelyJoin = true;
                        clubs.Add(randomObjectFromList);
                        num++;
                    }
                    return clubs;
                }
                return null;
            }
            catch (Exception ex)
            {
                InterestManager.print(ex.ToString());
                return null;

            }
        }

        public static void LoadClubXML()
        {
            try
            {
                XmlDbData xmlSupportedMods = XmlDbData.ReadData(new ResourceKey(0x292970446D743AB9, 0x0333406C, 0x00000000), false);

                if(ClubsInTown == null || ClubsInTown.Count <0)
                    ClubsInTown= new Dictionary<string, ClubHobby>();

                if (xmlSupportedMods != null)
                {
                    if (!ReadClubDetails(xmlSupportedMods))
                    {
                        InterestManager.print("An error occured while loading Club Hobbies XML file. Error file has been made in Documents/The Sims 3. Make sure to give this to Lyralei. Interest & Hobbies mod will work fine, but Hobby Clubs will not!");
                    }
                }
            }
            catch (Exception ex)
            {
                InterestManager.print(ex.ToString());
            }
        }

        private static bool ReadClubDetails(XmlDbData xmlDocument)
        {
            try
            {
                XmlDbTable xmlDbTable;

                if (xmlDocument != null && xmlDocument.Tables != null && xmlDocument.Tables.TryGetValue("Club", out xmlDbTable))
                {
                    foreach (XmlDbRow row in xmlDbTable.Rows)
                    {
                        ClubHobby club = new ClubHobby();

                        club.Name = row.GetString("Name");
                        club.Description = row.GetString("Description");
                        club.InterestType = row.GetEnum("InterestType", InterestTypes.None);
                        club.DaysToWork = ParserFunctions.ParseDayListToEnum(row.GetString("DaysToWork"));
                        club.StartTime = row.GetTime("StartTime");
                        club.Duration = row.GetFloat("Duration");

                        if (club.InterestType == InterestTypes.None) { 
                            club = null;
                            continue;
                        }

                        // Set Hobby Lot if there is one already.
                        if(club.InterestType != InterestTypes.None && InterestManager.mHobbyLot != null && InterestManager.mHobbyLot.Count > 0)
                        {
                            foreach(HobbyLot lot in InterestManager.mHobbyLot)
                            {
                                if(club.InterestType == lot.typeHobbyLot)
                                {
                                    club.HobbyLot = lot;
                                    break;
                                }
                            }
                        }

                        // Set Hobby owner.
                        if(club.InterestType != InterestTypes.None && InterestManager.mSavedSimInterests != null && InterestManager.mSavedSimInterests.Count > 0)
                        {
                            List<SimDescription> sims = InterestManager.GetSimsWithInterestTypesFromTown(club.InterestType, true);

                            if(sims != null || sims.Count > 0)
                            {
                                SimDescription sim = TryToGetRandomOwner(sims, club.Name);
                                if (sim != null)
                                {

                                    club.ClubOwner = sim.SimDescriptionId;
                                    AddSimToClubsList(club.ClubOwner, club);
                                    

                                }
                            }
                        }
                        if(!ClubsInTown.ContainsKey(club.Name))
                        {
                            ClubsInTown.Add(club.Name, club);
                        }
                        else
                        {
                            club = null;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                CommonHelpers.printException(ex);
                return false;
            }
        }

        //private static ulong GetRandomClubOwner(List<SimDescription> sims, int tries)
        //{
        //    for (int i = 0; i < tries; i++)
        //    {
        //        ulong simId = RandomUtil.GetRandomObjectFromList<SimDescription>(sims).SimDescriptionId;
        //        if (!ClubsInTown.ContainsKey(simId))
        //        {
        //            return simId;
        //        }
        //    }
        //    return 0;
        //}

        private static SimDescription TryToGetRandomOwner(List<SimDescription> sims, string clubName)
        {
            int triesLeft = 10;

            for(int i = 0; i < triesLeft; i++)
            {
                SimDescription sim = RandomUtil.GetRandomObjectFromList<SimDescription>(sims);
                if (sim.AdultOrAbove && !IsSimAlreadyInClub(sim.SimDescriptionId, clubName));
                {
                    return sim;
                }
            }
            return null;
        }

        public static void AddSimToClubsList(ulong simdesc, ClubHobby club)
        {
            if(ClubsInTown != null)
            {
                foreach(KeyValuePair<string, ClubHobby> kpv in ClubsInTown)
                {
                    if(kpv.Value.Name == club.Name && !IsSimAlreadyInClub(simdesc, kpv.Value.Name))
                    {
                        kpv.Value.mMembers.Add(simdesc);
                    }
                }
            }
        }

        public static bool IsSimAlreadyInClub(ulong simdesc, string nameClub)
        {
            if(ClubsInTown != null)
            {
                if (ClubsInTown.ContainsKey(nameClub))
                {
                    return ClubsInTown[nameClub].mMembers.Contains(simdesc);
                }
            }
            return false;
        }

        public class ClubHobby
        {
            private string mName;

            private string mDescription;

            private HobbyLot mHobbyLot;

            private InterestTypes mInterestType = InterestTypes.None;

            private bool mCanActivelyJoin = false;

            private ulong mClubOwner = 0;

            public DaysOfTheWeek DaysToWork;

            public float StartTime;

            public float Duration;

            public string Name
            {
                get => mName;
                set => mName = value;
            }

            public string Description
            {
                get => mDescription;
                set => mDescription = value;
            }

            public ulong ClubOwner
            {
                get => mClubOwner;
                set => mClubOwner = value;
            }

            public bool CanActivelyJoin
            {
                get => mCanActivelyJoin;
                set => mCanActivelyJoin = value;
            }

            public InterestTypes InterestType
            {
                get => mInterestType;
                set => mInterestType = value;
            }

            public HobbyLot HobbyLot
            {
                get => mHobbyLot;
                set => mHobbyLot = value;
            }

            public List<ulong> mMembers = new List<ulong>(); // All members INCLUDING leader. SimIDs here


            public ClubHobby()            
            {
            }

            public ClubHobby(string name, string description, InterestTypes type)
            {
                Name = name;
                Description = description;
                InterestType = type;
            }

            public ClubHobby(string name, string description, InterestTypes type, HobbyLot lot)
            {
                Name = name;
                Description = description;
                InterestType = type;
                HobbyLot = lot;
            }
        }

        public class FindHobbyClub : Computer.ComputerInteraction
        {
            public class Definition : InteractionDefinition<Sim, Computer, FindHobbyClub>
            {
                public override string[] GetPath(bool isFemale)
                {
                    return new string[2]
                    {
                        "Interests & Hobbies...",
                        "Hobbies..."
                    };
                }


                public override string GetInteractionName(Sim actor, Computer target, InteractionObjectPair iop)
                {
                    return "Find Hobby Club";
                }

                public override bool Test(Sim a, Computer target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (target.InUse || target.IsActorUsingMe(a))
                        return false;

                    return true;
                }
            }

            public static readonly InteractionDefinition Singleton = new Definition();

            public override bool Run()
            {
                try
                {
                    base.StandardEntry(true);

                    if (!base.Target.StartComputing(this, SurfaceHeight.Table, true))
                    {
                        base.StandardExit();
                        return false;
                    }
                    base.Target.StartVideo(Computer.VideoType.Browse);
                    base.BeginCommodityUpdates();
                    base.AnimateSim("WorkTyping");

                    ClubHobby club = HobbyClubSelectionDialog.Show();

                    if (club != null)
                    {
                        AddSimToClubsList(base.Actor.SimDescription.SimDescriptionId, club);
                    }

                    base.Target.StopComputing(this, Computer.StopComputingAction.TurnOff, false);
                    base.EndCommodityUpdates(true);
                    base.StandardExit();
                    return true;
                }
                catch (Exception ex)
                {
                    InterestManager.print(ex.ToString());
                }
                return false;
            }
        }
    }
}
