using Lyralei.InterestMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.Visa;
using Sims3.SimIFace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static HobbiesAndInterests.HelperClasses.EnergyManager;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class HobbyClubsManager
    {
        //Feature ideas: 

        // - Create Your own Club
        // - Add sims to said club (automatically asign depending on hobby, or the club owner is random.)
        // - Do activities with Club at hobby lot.

        [Persistable]
        public static string mSavedEnergyCompanyData;

        [Tunable]
        public static int mHowManyHobbyClubsToGrabPerDay = 3; // Just like finding a job, how many hobby clubs will show up that day to choose from.

        private static AlarmHandle sAlarmNewHobbyList = AlarmHandle.kInvalidHandle;

        public static HobbyClubsManager Instance;
        public HobbyClubsManager()
        {
            HobbyClubsManager singleton = Instance;
        }

        public static Dictionary<ulong, ClubHobby> mSimsInClubHobbies;


        private static List<ClubHobby> mClubsInTown;
        public static List<ClubHobby> ClubsInTown
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
                    mSimsInClubHobbies = new Dictionary<ulong, ClubHobby>(); // Remove this after saving code stuff.
                }

                if (ClubsInTown == null || ClubsInTown.Count <= 0)
                {
                    ClubsInTown = new List<ClubHobby>();
                    LoadClubXML();
                }

            }
            catch (Exception ex)
            {
                InterestManager.print(ex.ToString());
            }
        }

        private static void FindRandomClubForInteraction()
        {
            if(ClubsInTown != null && ClubsInTown.Count > 0)
            {
                ClubsInTown.ForEach(index => {
                    index.CanActivelyJoin = false;
                });

                Random r = new Random((int)SimClock.ElapsedTime(TimeUnit.Days) + Computer.RandomComputerSeed);

                int num = 0;
                StringBuilder stringBuilder = new StringBuilder();

                while (num < mHowManyHobbyClubsToGrabPerDay)
                {
                    HobbyClubsManager.ClubHobby randomObjectFromList = RandomUtil.GetRandomObjectFromList(ClubsInTown, r);
                    if (randomObjectFromList == null)
                    {
                        break;
                    }
                    randomObjectFromList.CanActivelyJoin = true;
                    stringBuilder.AppendLine(randomObjectFromList.Name.ToString());
                    num++;
                }

                InterestManager.print(stringBuilder.ToString());
            }
        }

        public static void LoadClubXML()
        {
            try
            {
                XmlDbData xmlSupportedMods = XmlDbData.ReadData(new ResourceKey(0x292970446D743AB9, 0x0333406C, 0x00000000), false);

                if(ClubsInTown == null || ClubsInTown.Count <0 )
                    ClubsInTown= new List<ClubHobby>();

                if (xmlSupportedMods != null)
                {
                    InterestManager.print("Loading Clubs...");
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
                                SimDescription sim = TryToGetRandomOwner(sims);
                                if (sim != null)
                                {
                                    club.ClubOwner = RandomUtil.GetRandomObjectFromList<SimDescription>(sims).SimDescriptionId;
                                    mSimsInClubHobbies.Add(club.ClubOwner, club); // Add sim to me in hobby.
                                }
                            }
                        }
                        ClubsInTown.Add(club);
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


        private static SimDescription TryToGetRandomOwner(List<SimDescription> sims)
        {
            int triesLeft = 10;

            for(int i = 0; i < triesLeft; i++)
            {
                SimDescription sim = RandomUtil.GetRandomObjectFromList<SimDescription>(sims);
                if (sim.AdultOrAbove)
                {
                    return sim;
                }
            }
            return null;
        }

        public class ClubHobby
        {
            private string mName;

            private string mDescription;

            private HobbyLot mHobbyLot;

            private InterestTypes mInterestType = InterestTypes.None;

            private bool mCanActivelyJoin = false;

            private ulong mClubOwner;

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

        public class FindClubNewspaper : HeldNewspaperInteraction
        {
            public class Definition : InteractionDefinition<Sim, Newspaper, FindClubNewspaper>
            {

                public override string GetInteractionName(Sim a, Newspaper target, InteractionObjectPair interaction)
                {
                    return "Show Hobby Clubs";
                }

                public override bool Test(Sim a, Newspaper target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (target.IsReadable && a.SimDescription.TeenOrAbove)
                    {
                        return true;
                    }
                    return false;
                }
            }

            public static InteractionDefinition Singleton = new Definition();

            public override bool Run()
            {
                return DoFindClub(false);
            }

            public override bool RunFromInventory()
            {
                return DoFindClub(true);
            }

            public bool DoFindClub(bool bFromInventory)
            {
                StateMachineClient stateMachineClient = StateMachineClient.Acquire(Actor.Proxy.ObjectId, "ReadNewspaper");
                if (!Target.StartUsingNewspaper(bFromInventory, false, Actor, stateMachineClient))
                {
                    CarrySystem.PutDownOnFloor(Actor);
                    return false;
                }
                stateMachineClient.SetActor("newspaper", Target);
                stateMachineClient.SetActor("x", Actor);
                stateMachineClient.RequestState("x", "ReadNewspaper");
                FindRandomClubForInteraction();
                return Target.StopUsingNewspaper(Actor, stateMachineClient, bFromInventory);
            }

            public override void PostureTransitionFailed(bool transitionExitReason)
            {
                Target.PutNewspaperAway(Actor, true);
            }
        }



    }
}
