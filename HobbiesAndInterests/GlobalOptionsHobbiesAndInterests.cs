using HobbiesAndInterests.Career;
using HobbiesAndInterests.HelperClasses;
using Lyralei;
using Lyralei.InterestMod;
using Lyralei.UI;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Environment;
using Sims3.Gameplay.Objects.Lyralei;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using static Sims3.Gameplay.Lyralei.InterestMod.HobbyClubsManager;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    /// <summary>
    /// Global options hobbies and interests is the helper class that sets up anything that needs setting up, upon worldload, or any gameload.
    /// </summary>
    public class GlobalOptionsHobbiesAndInterests
	{
		[Tunable]
        protected static bool kInstantiator = false;

        private static EventListener sBoughtObjectLister = null;
        private static EventListener sSimInstantiatedListener = null;
        public static EventListener sAgeSettingsHasChanged = null;

        public static AlarmHandle sAlarmTownieEAFixes = AlarmHandle.kInvalidHandle;

        static GlobalOptionsHobbiesAndInterests()
        {
            World.sOnWorldLoadFinishedEventHandler      += new EventHandler(OnWorldLoadFinished);
            World.sOnWorldQuitEventHandler              += new EventHandler(OnWorldQuit);
            LoadSaveManager.ObjectGroupsPreLoad         += new ObjectGroupsPreLoadHandler(OnPreload);
            Simulator.mPostInitHandler                  += CareerInstantiatorManager.PostInitCareer;

            LotManager.ActiveLotChanged                 += EnergyManager.PickEnergyCompanyForFamily;

            // Used for handling CC content OnBought
            World.OnObjectPlacedInLotEventHandler       += new EventHandler(CustomContentHelper.OnBuildBuy);

             
            //LoadSaveManager.ObjectGroupsPreLoad += new ObjectGroupsPreLoadHandler(ParseBooks);
            World.sOnStartupAppEventHandler             += new EventHandler(OnStartupApp);
        }

        [PersistableStatic] static bool didAllmagazines = false;

        [PersistableStatic]
        protected static PersistedDataInterests sSettings;

        public static PersistedDataInterests retrieveData
        {
            get
            {
                if (sSettings == null)
                {
                    sSettings = new PersistedDataInterests();
                }
                return sSettings;
            }
        }
        public static bool alreadyParsed = false;

        public static void OnStartupApp(object sender, EventArgs e)
        {
            CustomContentHelper.LoadAllItemsFromXMLStartup();
        }

        public static void OnPreload()
        {
            //try
            //{
            //    LoadSocialData("LyraleiInterestsSocialData");
            //    LoadSocializingActionAvailability();
            //}
            //catch (Exception ex)
            //{
            //    print("Social " + ex.ToString());
            //}
        }

        // Loads on WorldLoadFinished. may want to move this to OnStartup
        public static void ParseSocialInteractions()
        {
            try
            {
                LoadSocialData("LyraleiInterestsSocialData");
                LoadSocializingActionAvailability("LoadSocializingActionAvailability");
            }
            catch (Exception ex)
            {
                print("Social " + ex.ToString());
            }
        }

        public static void OnWorldLoadFinished(object sender, EventArgs e)
        {
            try
            {
                // Populate interestType helper list if necessary
                InitInterestTypesList();

                ParseBooks();
                ParseSocialInteractions();

                // REMOVE WHEN DONE TESTING!!!
                //InterestManager.mSavedSimInterests.Clear();
                //InterestManager.mSavedSimInterests = null;
                //InterestManager.mSavedSimInterests = new Dictionary<ulong, List<Interest>>();

                // Get all sims in town
                Sim[] objects = Sims3.Gameplay.Queries.GetObjects<Sim>();

                // GetSimIdAndSimNames(objects);

                // Import the save data first before assigning new interests to any sims that aren't in the list.
                InterestSaveManager.ImportInterestData();

                for (int i = 0; i < objects.Length; i++)
                {
                    Sim sim = objects[i];
                    if (sim != null || sim.SimDescription != null)
                    {
                        // Check if sim is in the world, if it isn't a pet or if it isn't a service in the world
                        if (sim.InWorld && !sim.IsPet && !sim.IsPerformingAService && sim.Household.LotHome != null)
                        {
                            ulong id = sim.SimDescription.SimDescriptionId;

                            // Check if the sim already has interest data, if not, then continue...
                            if (!InterestManager.mSavedSimInterests.ContainsKey(id))
                            {
                                // Init and Set up all interests.
                                try
                                {
                                    List<Interest> preparedList = PrepareInterestListForSim(id);
                                    InterestManager.mSavedSimInterests.Add(id, preparedList);
                                }
                                catch (Exception ex)
                                {
                                    print(ex.ToString());
                                }
                                try
                                {
                                    // Set up the random bonus points and parse in if it's a townie or not (IsSelectable = The player's active sim(s), so sims with a plumbbob on their heads)
                                    InterestManager.SetUpRandomBonusPoints(InterestManager.mSavedSimInterests[id], sim.SimDescription, TownieInterestHelper.premadeSims.ContainsKey(id));
                                    if (TownieInterestHelper.premadeSims.ContainsKey(id))
                                    {
                                        TownieInterestHelper.SetTownieAlarms(sim);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    print(ex.ToString());
                                }
                            }
                            AddInteractionsSims(sim);
                            sSimInstantiatedListener = EventTracker.AddListener(EventTypeId.kSimInstantiated, new ProcessEventDelegate(OnSimInstantiated));
                            sAgeSettingsHasChanged = EventTracker.AddListener(EventTypeId.kAgeOptionChange, new ProcessEventDelegate(InterestManager.RerollRequiredSkillsLevelForAgeSettings));
                            //CreateBookAndPlaceInInventory(sim);

                            //EventTracker.AddListener(EventTypeId.kSkillLevelUp, new ProcessEventDelegate());
                        }
                    }
                }

                EnergyManager.Startup();
                HobbyClubsManager.Startup();

                OneShotFunction mShowInvitationFunction = new OneShotFunction(EnergyManager.PickEnergyCompanyForFamily);
                Simulator.AddObject(mShowInvitationFunction);

                //sAlarmTownieEAFixes = AlarmManager.Global.AddAlarm(5f, TimeUnit.Minutes, TownieInterestHelper.TownieHobbyFixer, "TownieFixes", AlarmType.NeverPersisted, null);


                //print("Count sims at End" + InterestManager.mSavedSimInterests.Count.ToString()); //84


                // Add Interactions to solarpanels
                foreach (SolarPanel solar in Sims3.Gameplay.Queries.GetObjects<SolarPanel>())
                {
                    if (solar != null)
                    {
                        AddInteractionsObjects(solar);
                    }
                }


                foreach (Computer computer in Sims3.Gameplay.Queries.GetObjects<Computer>())
                {
                    if (computer != null)
                    {
                        AddInteractionsObjects(computer);
                    }
                }

                Phone[] phones = Sims3.Gameplay.Queries.GetObjects<Phone>();

                if (phones != null || phones.Length > 0)
                {
                    foreach (Phone phone in phones)
                    {
                        if (phone != null)
                        {
                            AddInteractionsObjects(phone);

                        }

                    }

                }


                foreach (PhoneSmart phoneSmart in Sims3.Gameplay.Queries.GetObjects<PhoneSmart>())
                {
                    if (phoneSmart != null)
                    {
                        AddInteractionsObjects(phoneSmart);

                    }

                }

                foreach (Lot lot in Sims3.Gameplay.Queries.GetObjects<Lot>())
                {
                    if (lot != null)
                    {
                        AddInteractionsObjects(lot);

                    }

                }

                //foreach (CityHall rabbithole in Sims3.Gameplay.Queries.GetObjects<CityHall>())
                //{
                //    if (rabbithole != null)
                //    {
                //        CareerInstantiatorManager.AddInteractionsCareers(rabbithole);

                //    }
                //}

                EventTracker.AddListener(EventTypeId.kBoughtObject, new ProcessEventDelegate(OnObjectBought));
                EventTracker.AddListener(EventTypeId.kObjectStateChanged, new ProcessEventDelegate(OnObjectBought));

            }
            catch (Exception ex)
            {
                print(ex.ToString());
            }
        }

        public static void OnWorldQuit(object sender, EventArgs e)
        {
            // InterestSaveManager.SaveUserData();

            foreach (Sim sim in Sims3.Gameplay.Queries.GetObjects<Sim>())
            {
                if (sim != null)
                {
                    sim.RemoveInteraction(new InteractionObjectPair(ShowKnownInterest.Singleton, sim));
                    sim.RemoveInteraction(new InteractionObjectPair(DebateEnvironment.Singleton, sim));
                    sim.RemoveInteraction(new InteractionObjectPair(RantAboutInterest.Singleton, sim));
                    sim.RemoveInteraction(new InteractionObjectPair(ConvinceToPursueInterest.Singleton, sim));
                    //sim.RemoveInteraction(new InteractionObjectPair(SimDebugInteractions.HasBuggedInterests.Singleton, sim));
                    //sim.RemoveInteraction(new InteractionObjectPair(SimDebugInteractions.ShowHobbiesUI.Singleton, sim));
                    sim.RemoveInteraction(new InteractionObjectPair(SimDebugInteractions.GenerateInterestBook.Singleton, sim));
                }
                EventTracker.RemoveListener(sAgeSettingsHasChanged);
                EventTracker.RemoveListener(sSimInstantiatedListener);
            }

            foreach (SolarPanel solar in Sims3.Gameplay.Queries.GetObjects<SolarPanel>())
            {
                if (solar != null)
                {
                    RemoveInteractionsFromObject(solar);
                }
                EventTracker.RemoveListener(sBoughtObjectLister);
            }

            foreach (Computer computer in Sims3.Gameplay.Queries.GetObjects<Computer>())
            {
                if (computer != null)
                {
                    RemoveInteractionsFromObject(computer);
                }
                EventTracker.RemoveListener(sBoughtObjectLister);
            }

            foreach (Phone phoneSmart in Sims3.Gameplay.Queries.GetObjects<Phone>())
            {
                if (phoneSmart != null)
                {
                    RemoveInteractionsFromObject(phoneSmart);
                }
                EventTracker.RemoveListener(sBoughtObjectLister);
            }

            foreach (PhoneSmart phoneSmart in Sims3.Gameplay.Queries.GetObjects<PhoneSmart>())
            {
                if (phoneSmart != null)
                {
                    RemoveInteractionsFromObject(phoneSmart);
                }
                EventTracker.RemoveListener(sBoughtObjectLister);
            }

            foreach (Lot lot in Sims3.Gameplay.Queries.GetObjects<Lot>())
            {
                if (lot != null)
                {
                    RemoveInteractionsFromObject(lot);
                }
                EventTracker.RemoveListener(sBoughtObjectLister);
            }
            InterestSaveManager.ExportInterestData();

            InterestManager.mSavedSimInterests.Clear();

        }

        public static void LoadSocializingActionAvailability(string spreadsheet)
        {
            XmlDbData xdb = XmlDbData.ReadData(spreadsheet);
            if (xdb != null)
            {
                try
                {
                    XmlDbTable xmlDbTable;
                    if (xdb.Tables.TryGetValue("SAA", out xmlDbTable))
                    {
                        StringBuilder sb = new StringBuilder();
                        ActionAvailabilityData.Initialize(false);
                        foreach (XmlDbRow row in xmlDbTable.Rows)
                        {
                            //print(row.GetString("Cat"));
                            //sb.AppendLine(row.GetString("Cat"));


                            ActionAvailabilityData.ProcessStcRow(row);
                        }
                        //print(sb.ToString());
                        ActionAvailabilityData.FreeMemory();
                    }
                    //if (xdb.Tables.ContainsKey("SAA"))
                    //{
                    //    print("Contained SAA");
                    //    SocialManager.ParseStcActionAvailability(xdb);
                    //}
                }
                catch (Exception ex)
                {
                    print("SAA: " + ex.Message.ToString() + "\n" + ex.Source.ToString());
                }
            }
        }

        private static Dictionary<string, string> SocialDataAfterUpdateOverrides = new Dictionary<string, string>()
        {
            {"Share Interests" , "CallbackGeneralInterestSharing"},
            {"Talk About Going Green" , "CallbackTalkingAboutEnvironment"},
            {"Talk About Recycling" , "CallbackTalkingAboutEnvironment"},
            {"Talk About Renewable Energy" , "CallbackTalkingAboutEnvironment"},
            {"Talk About Composting" , "CallbackTalkingAboutEnvironment"},
            {"AskAboutFish" , "CallbackTalkingAboutEnvironment"},
            {"Applaud Vegetarianism" , "CallbackTalkingAboutEnvironment"},
            {"Boast About Gardening Glory" , "CallbackTalkingAboutEnvironment"},
            {"Boast About Fishing Feats" , "CallbackTalkingAboutEnvironment"},
            {"Boast About Reviving Plant" , "CallbackTalkingAboutEnvironment"},
            {"Compliment Garden" , "CallbackTalkingAboutEnvironment"},
            {"Enthuse About Outdoors" , "CallbackTalkingAboutEnvironment"},
            {"Mock Vegetarianism" , "CallbackTalkingAboutEnvironment"},
            {"Talk About Great Outdoors" , "CallbackTalkingAboutEnvironment"},
            {"Enthuse About Garlic" , "CallbackTalkingAboutEnvironment"},
            {"Enthuse About Rainbows" , "CallbackTalkingAboutEnvironment"},
            {"Talk About Flowers" , "CallbackTalkingAboutEnvironment"},
            //{"Talk About Rain" , "CallbackTalkingAboutEnvironment"},
            //{"Talk About Snow" , "CallbackTalkingAboutEnvironment"},
            //{"Talk About Hail" , "CallbackTalkingAboutEnvironment"},
            //{"Talk About Fog" , "CallbackTalkingAboutEnvironment"},
            {"Talk About Cold" , "CallbackTalkingAboutEnvironment"},
            {"Talk About Heat" , "CallbackTalkingAboutEnvironment"},
        };

        public static void LoadSocialData(string spreadsheet)
        {
            try
            {
                XmlDocument root = Simulator.LoadXML(spreadsheet);
                bool isEp5Installed = GameUtils.IsInstalled(ProductVersion.EP5);
                if (spreadsheet != null)
                {
                    XmlElementLookup lookup = new XmlElementLookup(root);
                    List<XmlElement> list = lookup["Action"];
                    foreach (XmlElement element in list)
                    {
                        XmlElementLookup table = new XmlElementLookup(element);
                        CommodityTypes intendedCom;
                        ParserFunctions.TryParseEnum(element.GetAttribute("com"), out intendedCom, CommodityTypes.Undefined);
                        ActionData data = new ActionData(element.GetAttribute("key"), intendedCom, ProductVersion.BaseGame, table, isEp5Installed);
                        ActionData.Add(data);
                    }
                }

                foreach(KeyValuePair<string, string> kpv in SocialDataAfterUpdateOverrides)
                {
                    LoadOverrideInfo(kpv.Key, kpv.Value, true);
                }


            }
            catch (Exception ex)
            {
                print(ex.Message.ToString() + "\n" + ex.Source.ToString());
            }
        }

        public static void LoadOverrideInfo(string key, string newFunctional, bool isAfterUpdate)
        {
            ActionData dataAction = ActionData.Get(key);

            if (dataAction != null)
            {
                CommonHelpers.ReplaceRHSThatHasNoProcedularMethod<InterestSocialCallback>(dataAction.Key, newFunctional, true);
            }
        }

        private static List<XmlDbData> sDelayedSkillBooks = new List<XmlDbData>();

        public static Dictionary<string, BookGeneralInterestsData> BookGeneralInterestsDataList = new Dictionary<string, BookGeneralInterestsData>();

        // Expand to Different issues per different magazines
        public static Dictionary<string, BookMagazineInterestsData> BookMagazineInterestsDataList = new Dictionary<string, BookMagazineInterestsData>();

        /*  HOW THE DICTIONARIES ARE SET UP:
         * String: ID of the magazine. Always: "Magazine_INTERESTTYPE_CREATOR_TITLEOFMAGAZINE" (No spaces or dashes allowed. Fill up with underscore if necessary)
         * New instance of the data. Parse in the Interest Type AND the title of the magazine.
         */


        public static void ParseBooks()
        {
            BookGeneralInterestsDataList.Clear();

            Array types = Enum.GetValues(typeof(InterestTypes));

            foreach (InterestTypes interest in (InterestTypes[])types)
            {
                if(interest == InterestTypes.None) { continue; }
                BookGeneralInterestsDataList.Add(interest.ToString() + "_Lyralei", new BookGeneralInterestsData(interest, interest.ToString()));

                if(!didAllmagazines)
                {
                    CreateAndSetupInterestMagazines(interest);
                }
                //BookMagazineInterestsDataList.Add("Magazine_" + interest.ToString() + "_Lyralei", new BookMagazineInterestsData(interest, interest.ToString()));
            }
            didAllmagazines = true;

            // Add general books in library bookcase:
            foreach (Bookshelf bookshelf in Sims3.Gameplay.Queries.GetObjects<Bookshelf>())
            {
                if (bookshelf != null && bookshelf.LotCurrent.CommercialLotSubType == CommercialLotSubType.kLibrary)
                {
                    if(RandomUtil.RandomChance(50f))
                    {
                        AddBooksInLibraryBookcases(bookshelf);
                    }
                }
            }

        }

        public static string[] randomSimsArray = new string[7]
        {
            "Nancy Landgraab",
            "Malcolm Langraab CLXXXV",
            "Dina Caliente",
            "Mortimer Goth",
            "Bella",
            "Nick Alto",
            "Don Lothario",
        };

        public static void CreateAndSetupInterestMagazines(InterestTypes type)
        {
            string randomPerson = RandomUtil.GetRandomStringFromList(randomSimsArray);
            Dictionary<string, BookMagazineInterestsData> dictionary = InterestManager.GetRightMagazineDictionary(type);

            // The reason we start with 1 is because of the Localisation. Else one magazine might end up with no title!
            for (int i = 1; i < 63; i++)
            {
                string randomTitle = LocalizeMagazineTitle(i, type, randomPerson);

                if(dictionary != null && !dictionary.ContainsKey("Magazine_" + type.ToString() + "_Lyralei_" + "#" + i.ToString() + RemoveWhitespace(randomTitle)))
                {
                    dictionary.Add("Magazine_" + type.ToString() + "_Lyralei_" + "#" + i.ToString() + RemoveWhitespace(randomTitle), new BookMagazineInterestsData(type, randomTitle, i));
                }
            }
        }

        public static string LocalizeMagazineTitle(int number, InterestTypes type, string randomPerson)
        {
            return Localization.LocalizeString("Gameplay/Excel/Books/LyraleiMagazines:" + number.ToString(), new object[2] { type.ToString(), randomPerson });
        }

        public static string RemoveWhitespace(string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        public static void AddBooksInLibraryBookcases(Bookshelf bookshelf)
        {
            if (bookshelf != null && bookshelf.LotCurrent.CommercialLotSubType == CommercialLotSubType.kLibrary && !Sims3.SimIFace.Environment.HasEditInGameModeSwitch)
            {
                List<BookGeneralInterests> amountOfInterestBooks = bookshelf.Inventory.FindAll<BookGeneralInterests>(false);

                if(amountOfInterestBooks.Count >= 3)
                {
                    // Don't add more books....
                    return;
                }

                foreach (BookGeneralInterestsData value in BookGeneralInterestsDataList.Values)
                {
                    if(RandomUtil.RandomChance(50f))
                    {
                        BookGeneralInterests bookSkill = BookGeneralInterests.CreateOutOfWorld(value);
                        bookSkill.BookValue = 0;
                        bookSkill.MyShelf = bookshelf;
                        bookshelf.Inventory.TryToAdd(bookSkill);
                    }
                }
            }
        }


        public static List<Interest> PrepareInterestListForSim(ulong simDesc)
        {
            if(simDesc == 0)
            {
                print("simDesc was empty!");
                return null; 
            }

            List<Interest> interestList = new List<Interest>();

            // numbers are index lists.
            Animals animals = new Animals(simDesc);//0
            interestList.Add(animals);

            Crime crime = new Crime(simDesc);//1
            interestList.Add(crime);

            Culture culture = new Culture(simDesc);//2
            interestList.Add(culture);

            Entertainment entertainment = new Entertainment(simDesc);//3
            interestList.Add(entertainment);

            SimDescription desc = SimDescription.Find(simDesc);

            if(desc != null)
            {
                Sims3.Gameplay.Lyralei.InterestMod.Environment environment = new Sims3.Gameplay.Lyralei.InterestMod.Environment(simDesc, desc.HasTrait(TraitNames.Vegetarian));
                interestList.Add(environment);//4
            }
            else
            {
                Sims3.Gameplay.Lyralei.InterestMod.Environment environment = new Sims3.Gameplay.Lyralei.InterestMod.Environment(simDesc, false);
                interestList.Add(environment);//4
            }

            Fashion fashion = new Fashion(simDesc);//5
            interestList.Add(fashion);

            Food food = new Food(simDesc);//6
            interestList.Add(food);

            Health health = new Health(simDesc);//7
            interestList.Add(health);

            Money money = new Money(simDesc);//8
            interestList.Add(money);

            Paranormal paranormal = new Paranormal(simDesc);//9
            interestList.Add(paranormal);

            Politics politics = new Politics(simDesc);//10
            interestList.Add(politics);

            Scifi scifi = new Scifi(simDesc);//11
            interestList.Add(scifi);

            Sports sports = new Sports(simDesc);//12
            interestList.Add(sports);

            Toys toys = new Toys(simDesc);//13
            interestList.Add(toys);

            return interestList;
        }

        protected static ListenerAction OnSimInstantiated(Event e)
        {
            try
            {
                Sim sim = e.TargetObject as Sim;
                if (sim != null)
                {
                    AddInteractionsSims(sim);
                    if (sim.InWorld || !sim.IsPet || !sim.IsPerformingAService)
                    {
                        ulong id = sim.SimDescription.SimDescriptionId;

                        // Check if the sim already has interest data, if not, then continue...
                        if (!InterestManager.mSavedSimInterests.ContainsKey(id))
                        {
                            // Init and Set up all interests.
                            try
                            {
                                List<Interest> preparedList = PrepareInterestListForSim(id);
                                InterestManager.mSavedSimInterests.Add(id, preparedList);
                            }
                            catch (Exception ex)
                            {
                                print(ex.ToString());
                            }

                            try
                            {
                                // Set up the random bonus points and parse in if it's a townie or not (IsSelectable = The player's active sim(s), so sims with a plumbbob on their heads)
                                InterestManager.SetUpRandomBonusPoints(InterestManager.mSavedSimInterests[id], sim.SimDescription, TownieInterestHelper.premadeSims.ContainsKey(id));
                                if (TownieInterestHelper.premadeSims.ContainsKey(id))
                                {
                                    TownieInterestHelper.SetTownieAlarms(sim);
                                }
                            }
                            catch (Exception ex)
                            {
                                print(ex.ToString());
                            }
                        }
                        //else
                        //{
                        //    ReAttachInterestDataToSim(sim);
                        //}
                    }
                }
            }
            catch (Exception exception)
            {
                print(exception.ToString());
            }
            return ListenerAction.Keep;
        }

        //public static void ReAttachInterestDataToSim(Sim sim)
        //{
        //    List<Interest> newList = PrepareInterestListForSim(sim.SimDescription.SimDescriptionId);
        //    for (int i = 0; i < InterestManager.mSavedSimInterests[sim.SimDescription.SimDescriptionId].Count; i++)
        //    {
        //        newList[i] = InterestManager.mSavedSimInterests[sim.SimDescription.SimDescriptionId][i];
        //    }
        //    InterestManager.mSavedSimInterests[sim.SimDescription.SimDescriptionId] = newList;
        //}

        protected static ListenerAction OnObjectBought(Event e)
        {
            try
            {
                GameObject obj = e.TargetObject as GameObject;

                if (obj == null)
                   return ListenerAction.Keep;
                //if (solar != null)
                //{
                //    if (solar.IsOnRoof)
                //    {
                //        print("The solar is on the roof!");
                //    }
                AddInteractionsObjects(obj);
            }
            catch (Exception ex)
            {
                print(ex.ToString());
            }
            return ListenerAction.Keep;
        }


        public static void InitInterestTypesList()
        {
            // Add All interest Type to list.
            if (InterestManager.Instance.mGetInterestTypesList.Count <= 0)
            {
                foreach (InterestTypes interest in Enum.GetValues(typeof(InterestTypes)))
                {
                    InterestManager.Instance.mGetInterestTypesList.Add(interest);
                }
            }
        }

        public static void AddInteractionsSims(Sim sim)
        {
            foreach (InteractionObjectPair pair in sim.Interactions)
            {
                if (
                    pair.InteractionDefinition.GetType() == ShowKnownInterest.Singleton.GetType() ||
                    pair.InteractionDefinition.GetType() == DebateEnvironment.Singleton.GetType() ||
                    pair.InteractionDefinition.GetType() == RantAboutInterest.Singleton.GetType() ||
                    pair.InteractionDefinition.GetType() == ConvinceToPursueInterest.Singleton.GetType() ||
                    //pair.InteractionDefinition.GetType() == SimDebugInteractions.HasBuggedInterests.Singleton.GetType() ||
                    //pair.InteractionDefinition.GetType() == SimDebugInteractions.ShowHobbiesUI.Singleton.GetType() ||
                    pair.InteractionDefinition.GetType() == SimDebugInteractions.GenerateInterestBook.Singleton.GetType() ||
                    //pair.InteractionDefinition.GetType() == SimDebugInteractions.ExtractSaveData.Singleton.GetType() ||
                    //pair.InteractionDefinition.GetType() == SimDebugInteractions.SaveTheData.Singleton.GetType() ||
                    //pair.InteractionDefinition.GetType() == SimDebugInteractions.CheckTheSavedDataList.Singleton.GetType() ||
                    pair.InteractionDefinition.GetType() == SimDebugInteractions.ChooseAHobbyDEBUG.Singleton.GetType() ||
                    pair.InteractionDefinition.GetType() == ShowHobbies.Singleton.GetType() ||
                    pair.InteractionDefinition.GetType() == SimDebugInteractions.TrySettingUpHobbyClubsAgain.Singleton.GetType()
                    )
                {
                    return;
                }
            }


            //sim.AddInteraction(SimDebugInteractions.ListInterestData.Singleton);
            //sim.AddInteraction(SimDebugInteractions.CheckInterestDetails.Singleton);

            sim.AddInteraction(ShowKnownInterest.Singleton);
            sim.AddInteraction(DebateEnvironment.Singleton);
            sim.AddInteraction(RantAboutInterest.Singleton);
            sim.AddInteraction(ConvinceToPursueInterest.Singleton);

            sim.AddInteraction(ShowHobbies.Singleton);

            //sim.AddInteraction(SimDebugInteractions.HasBuggedInterests.Singleton);
            //sim.AddInteraction(SimDebugInteractions.ShowHobbiesUI.Singleton);

            sim.AddInteraction(SimDebugInteractions.GenerateInterestBook.Singleton);

            //sim.AddInteraction(SimDebugInteractions.ExtractSaveData.Singleton);
            //sim.AddInteraction(SimDebugInteractions.SaveTheData.Singleton);
            //sim.AddInteraction(SimDebugInteractions.CheckTheSavedDataList.Singleton);

            sim.AddInteraction(SimDebugInteractions.ChooseAHobbyDEBUG.Singleton);
            sim.AddInteraction(SimDebugInteractions.TrySettingUpHobbyClubsAgain.Singleton);

        }

        public static void AddInteractionsObjects(GameObject gameObj)
        {
            
            if (gameObj == null || gameObj.Interactions == null)
                return;


            if (gameObj.GetType() == typeof(Lot))
            {
                foreach (InteractionObjectPair interaction in gameObj.Interactions)
                {
                    if (interaction.InteractionDefinition.GetType() == AssignInterestLot.Singleton.GetType())
                    {
                        return;
                    }
                }
                gameObj.AddInteraction(AssignInterestLot.Singleton);
            }

            if (gameObj.GetType().IsSubclassOf(typeof(Computer)))
            {
                foreach (InteractionObjectPair interaction in gameObj.Interactions)
                {
                    if (interaction == null || interaction.InteractionDefinition == null || interaction.InteractionDefinition.GetType() == ResearchInterest.Singleton.GetType() || interaction.InteractionDefinition.GetType() == SubscribeToInterestMagazine.Singleton.GetType() || interaction.InteractionDefinition.GetType() == UnSubscribeToInterestMagazine.Singleton.GetType() || interaction.InteractionDefinition.GetType() == CheckEnergyInfoComputer.Singleton.GetType())
                    {
                        return;
                    }
                }
                if (gameObj.ItemComp != null && gameObj.ItemComp.InteractionsInventory != null)
                {
                    foreach (InteractionObjectPair iop in gameObj.ItemComp.InteractionsInventory)
                    {
                        if (iop == null || iop.InteractionDefinition == null || iop.InteractionDefinition.GetType() == ResearchInterest.Singleton.GetType() || iop.InteractionDefinition.GetType() == SubscribeToInterestMagazine.Singleton.GetType() || iop.InteractionDefinition.GetType() == UnSubscribeToInterestMagazine.Singleton.GetType() || iop.InteractionDefinition.GetType() == CheckEnergyInfoComputer.Singleton.GetType())
                        {
                            return;
                        }
                    }
                }

                gameObj.AddInteraction(ResearchInterest.Singleton);
                gameObj.AddInteraction(SubscribeToInterestMagazine.Singleton);
                gameObj.AddInteraction(UnSubscribeToInterestMagazine.Singleton);
                gameObj.AddInteraction(CheckEnergyInfoComputer.Singleton);

            }

            if (gameObj.GetType() == typeof(PhoneSmart) )
            {
                foreach (InteractionObjectPair interaction in gameObj.Interactions)
                {
                    if (interaction.InteractionDefinition.GetType() == ResearchInterestPhone.Singleton.GetType() || interaction.InteractionDefinition.GetType() == CheckEnergyCompany.Singleton.GetType() || interaction.InteractionDefinition.GetType() == CancelCarpoolService.Singleton.GetType() || interaction.InteractionDefinition.GetType() == CancelSchoolBusService.Singleton.GetType() || interaction.InteractionDefinition.GetType() == CheckEnergyInfo.Singleton.GetType())
                    {
                        return;
                    }
                }
                if (gameObj.ItemComp.InteractionsInventory != null)
                {
                    foreach (InteractionObjectPair iop in gameObj.ItemComp.InteractionsInventory)
                    {
                        if (iop.InteractionDefinition.GetType() == ResearchInterestPhone.Singleton.GetType() || iop.InteractionDefinition.GetType() == CheckEnergyCompany.Singleton.GetType() || iop.InteractionDefinition.GetType() == CancelCarpoolService.Singleton.GetType() || iop.InteractionDefinition.GetType() == CancelSchoolBusService.Singleton.GetType() || iop.InteractionDefinition.GetType() == CheckEnergyInfo.Singleton.GetType())
                        {
                            return;
                        }
                    }
                }
                gameObj.AddInteraction(ResearchInterestPhone.Singleton);
                gameObj.AddInventoryInteraction(ResearchInterestPhone.Singleton);
                gameObj.AddInteraction(CheckEnergyCompany.Singleton);
                gameObj.AddInventoryInteraction(CheckEnergyCompany.Singleton);

                gameObj.AddInteraction(CheckEnergyInfo.Singleton);
                gameObj.AddInventoryInteraction(CheckEnergyInfo.Singleton);

                gameObj.AddInteraction(CancelCarpoolService.Singleton);
                gameObj.AddInventoryInteraction(CancelCarpoolService.Singleton);
                gameObj.AddInteraction(CancelSchoolBusService.Singleton);
                gameObj.AddInventoryInteraction(CancelSchoolBusService.Singleton);
            }

            if (gameObj.GetType().IsSubclassOf(typeof(PhoneHome)))
            {
                ////print("Found type.");
                foreach (InteractionObjectPair interaction in gameObj.Interactions)
                {
                    if (interaction.InteractionDefinition.GetType() == CancelCarpoolService.Singleton.GetType() || interaction.InteractionDefinition.GetType() == CancelSchoolBusService.Singleton.GetType())
                    //if (interaction.InteractionDefinition.GetType() == CancelCarpoolService.Singleton.GetType())
                    {
                        return;
                    }
                }
                //if (gameObj.ItemComp != null && gameObj.ItemComp.InteractionsInventory != null)
                //{
                //    foreach (InteractionObjectPair iop in gameObj.ItemComp.InteractionsInventory)
                //    {
                //        if (iop.InteractionDefinition.GetType() == CancelCarpoolService.Singleton.GetType() || iop.InteractionDefinition.GetType() == CancelSchoolBusService.Singleton.GetType())
                //        {
                //            return;
                //        }
                //    }
                //}

                gameObj.AddInteraction(CancelCarpoolService.Singleton);
                gameObj.AddInteraction(CancelSchoolBusService.Singleton);


                gameObj.AddInventoryInteraction(CancelCarpoolService.Singleton);
                gameObj.AddInventoryInteraction(CancelSchoolBusService.Singleton);
            }
            if (gameObj.GetType() == typeof(PhoneCell) || gameObj.GetType().IsSubclassOf(typeof(PhoneCell)))
            {
                foreach (InteractionObjectPair interaction in gameObj.Interactions)
                {
                    if (interaction.InteractionDefinition.GetType() == CancelCarpoolService.Singleton.GetType() || interaction.InteractionDefinition.GetType() == CancelSchoolBusService.Singleton.GetType())
                    {
                            return;
                    }
                }
                //if (gameObj.ItemComp != null && gameObj.ItemComp.InteractionsInventory != null)
                //{
                //    foreach (InteractionObjectPair iop in gameObj.ItemComp.InteractionsInventory)
                //    {
                //        if (iop.InteractionDefinition.GetType() == CancelCarpoolService.Singleton.GetType() || iop.InteractionDefinition.GetType() == CancelSchoolBusService.Singleton.GetType())
                //        {
                //            return;
                //        }
                //    }
                //}

                gameObj.AddInteraction(CancelCarpoolService.Singleton);
                gameObj.AddInteraction(CancelSchoolBusService.Singleton);

                gameObj.AddInventoryInteraction(CancelCarpoolService.Singleton);
                gameObj.AddInventoryInteraction(CancelSchoolBusService.Singleton);

            }
            if(gameObj.GetType() == typeof(Newspaper))
            {
                gameObj.AddInteraction(FindClubNewspaper.Singleton);
                gameObj.AddInventoryInteraction(FindClubNewspaper.Singleton);
            }


            if (gameObj.GetType() == typeof(SolarPanel))
            {
                SolarPanel[] solar = null;
                if (Sim.ActiveActor != null)
                {
                    solar = Sim.ActiveActor.LotHome.GetObjects<SolarPanel>();
                }

                if (solar != null && solar.Length == 1)
                {
                    print("You've bought a solar panel! Time to choose which sim(s) should get a point into Environment");

                    List<Sim> UserChosenSims = SimPicker(Sim.ActiveActor.SimDescription, 1);

                    if (UserChosenSims != null && UserChosenSims.Count > 0)
                    {
                        foreach(Sim sim in UserChosenSims)
                        {
                            InterestManager.AddInterestPoints(1, sim.SimDescription, InterestTypes.Environment);
                        }
                    }
                    else
                    {
                        print("It seems that you've canceled the dialog. If this was by accident, you can always get rid of all solar panels, and place them back again.");
                    }
                }
            }
        }

        public static List<Sim> SimPicker(SimDescription sim, int amountOfAccepted)
        {
            List<ObjectPicker.HeaderInfo> list = new List<ObjectPicker.HeaderInfo>();
            list.Add(new ObjectPicker.HeaderInfo("Pick any sims to add Interest point to", null, 256));
            List<ObjectPicker.TabInfo> list2 = new List<ObjectPicker.TabInfo>();
            list2.Add(new ObjectPicker.TabInfo(string.Empty, string.Empty, new List<ObjectPicker.RowInfo>()));
            List<ObjectPicker.RowInfo> list3 = new List<ObjectPicker.RowInfo>();


            foreach (Sim household in sim.Household.Sims)
            {
                if (household != null && household.SimDescription != null && household.SimDescription.CreatedSim != null)
                {
                    if (household.InWorld && !household.IsPet && !household.IsPerformingAService && household.Household.LotHome != null)
                    {
                        List<ObjectPicker.ColumnInfo> list4 = new List<ObjectPicker.ColumnInfo>();

                        list4.Add(new ObjectPicker.ThumbAndTextColumn(household.GetThumbnailKey(), household.FullName));
                        ObjectPicker.RowInfo item = new ObjectPicker.RowInfo(household, list4);
                        list2[0].RowInfo.Add(item);
                    }
                }
            }


            if (list2.Count > 0)
            {
                List<ObjectPicker.RowInfo> list5 = ObjectPickerDialog.Show("Sim picker", Localization.LocalizeString("Ui/Caption/ObjectPicker:OK"), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel"), list2, list, amountOfAccepted, list3, false);
                if (list5 != null)
                {
                    if (list5.Count > 0)
                    {
                        List<Sim> chosenSims = new List<Sim>();
                        foreach (ObjectPicker.RowInfo item2 in list5)
                        {
                            Sim friendChosen = item2.Item as Sim;
                            chosenSims.Add(friendChosen);
                        }
                        return chosenSims;
                    }
                }
            }
            return new List<Sim>();
        }

        public static void RemoveInteractionsFromObject(GameObject gameObj)
        {
            if (gameObj == null)
                return;

            if (gameObj.GetType().IsSubclassOf(typeof(Computer)))
            {
                gameObj.RemoveInteraction(new InteractionObjectPair(ResearchInterest.Singleton, gameObj));
                gameObj.RemoveInteraction(new InteractionObjectPair(SubscribeToInterestMagazine.Singleton, gameObj));
                gameObj.RemoveInteraction(new InteractionObjectPair(UnSubscribeToInterestMagazine.Singleton, gameObj));
                gameObj.RemoveInteraction(new InteractionObjectPair(CheckEnergyInfo.Singleton, gameObj));

            }

            if (gameObj.GetType() == typeof(PhoneHome) || gameObj.GetType().IsSubclassOf(typeof(PhoneHome))
                || gameObj.GetType() == typeof(PhoneCell) || gameObj.GetType().IsSubclassOf(typeof(PhoneCell)))
            {
                gameObj.RemoveInteraction(new InteractionObjectPair(CancelCarpoolService.Singleton, gameObj));
                gameObj.RemoveInteraction(new InteractionObjectPair(CancelSchoolBusService.Singleton, gameObj));
            }

            if (gameObj.GetType() == typeof(PhoneSmart))
            {
                gameObj.RemoveInteraction(new InteractionObjectPair(ResearchInterestPhone.Singleton, gameObj));
                gameObj.RemoveInteraction(new InteractionObjectPair(CheckEnergyCompany.Singleton, gameObj));
                gameObj.RemoveInteraction(new InteractionObjectPair(CheckEnergyInfo.Singleton, gameObj));

                gameObj.RemoveInteraction(new InteractionObjectPair(CancelCarpoolService.Singleton, gameObj));
                gameObj.RemoveInteraction(new InteractionObjectPair(CancelSchoolBusService.Singleton, gameObj));
            }

            if (gameObj.GetType() == typeof(Lot))
                gameObj.RemoveInteraction(new InteractionObjectPair(AssignInterestLot.Singleton, gameObj));
            
        }

        public static void print(string text)
        {
            SimpleMessageDialog.Show("Lyralei's Interests & Hobbies", text);
        }


        public static void GetSimIdAndSimNames(Sim[] sims)
        {
            uint num = 0u;
            string s = Simulator.CreateExportFile(ref num, "SimDescriptionIDs_" + GameUtils.GetCurrentWorld().ToString());

            if (num != 0)
            {
                CustomXmlWriter customXmlWriter = new CustomXmlWriter(num);

                customXmlWriter.WriteToBuffer("--- " + GameUtils.GetCurrentWorld().ToString() + " ---");
                customXmlWriter.WriteToBuffer(System.Environment.NewLine);

                for (int i = 0; i < sims.Length; i++)
                {
                    customXmlWriter.WriteToBuffer(sims[i].mSimDescription.SimDescriptionId.ToString() + ", //" + sims[i].FullName);
                    customXmlWriter.WriteToBuffer(System.Environment.NewLine);
                }
                customXmlWriter.WriteEndDocument();

                StyledNotification.Format format = new StyledNotification.Format("Visible sim export written to '" + s + "'.", StyledNotification.NotificationStyle.kGameMessageNegative);
                StyledNotification.Show(format);
            }
        }
    }
}