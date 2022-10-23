using Lyralei.InterestMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Objects.Fishing;
using Sims3.Gameplay.Objects.Gardening;
using Sims3.Gameplay.Objects.Insect;
using Sims3.Gameplay.Objects.Lyralei;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.UI;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using Event = Sims3.Gameplay.EventSystem.Event;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public enum InterestTypes : ulong
    {
        None = 0,
        Politics = 0x1BFBA9F4,
        Crime = 0x9FC1CE21,
        Food = 0xB4B1178D,
        Sports = 0x54A577BC,
        Money = 0x2D3B55C7,
        Entertainment = 0x36F60725,
        Health = 0xDB2D51A3,
        Paranormal = 0xAC19ACD2,
        Toys = 0xAE458D0A,
        Environment = 0x494F8678,
        Culture = 0x94E412ED,
        Fashion = 0x6DA7ACA7,
        Animals = 0x389683F8,
        Scifi = 0x6AF7F30F,
    }

    public enum InteractionTypesHobbies
    {
        None,
        Gardening,
        BeeKeeping,
        Fishing
    }



    /// <summary>
    /// Interest manager is the main 'root' of the way the interests system works. The manager takes care that the sim get the interests 
    /// or gets them removed (or a point getting substracted/added).
    /// </summary>

    public class InterestManager
    {

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static InterestManager()
        {
        }
        private InterestManager()
        {
        }


        // Ulong == SimDescription ID
        // Interest is a collection of interests of the sim.
        public static Dictionary<ulong, List<Interest>> mSavedSimInterests = new Dictionary<ulong, List<Interest>>();

        private static readonly InterestManager instance = new InterestManager();

        [Tunable]
        public static int perTraitPenaltyPointLost = -3;

        [Tunable]
        public static int perTraitPassionatePointGained = 3;

        public List<InterestTypes> mGetInterestTypesList = new List<InterestTypes>();

        // Get the single instance version of this class. Since we only need one ;)
        public static InterestManager Instance
        {
            get
            {
                return instance;
            }
        }

        [Tunable]
        public static float mSubstractLikingWhenHatingInterest = -10f;

        [Tunable]
        public static float mAddLikingWhenLovingInterest = 10f;

        public string[] kInterestAnimalsBalloons = new string[11]
        {
            "balloon_butterflies",
            "balloon_fish",
            "ep5_balloon_cats",
            "ep5_balloon_dog",
            "ep5_balloon_horsie",
            "ep5_balloon_bird",
            "ep5_balloon_lizard",
            "ep5_balloon_rodent",
            "ep5_balloon_snake",
            "ep5_balloon_turtle",
            "balloon_llama"
        };

        public string[] kInterestCrimeBalloons = new string[4]
        {
            "balloon_policecar",
            "balloon_siren",
            "balloon_handcuffs",
            "balloon_burglar",
        };

        public string[] kInterestCultureBalloons = new string[8]
        {
            "balloon_paintbrush",
            "balloon_painting",
            "balloon_sheetmusic",
            "balloon_conductor",
            "balloon_inkwell",
            "balloon_musicnotes",
            "ep2_balloon_sculptingskill",
            "ep2_balloon_statue"
        };
        public string[] kInterestEntertainmentBalloons = new string[5]
        {
            "balloon_film",
            "balloon_famestar",
            "balloon_partyballoons",
            "balloon_stage",
            "balloon_moodlet_awesomeParty"
        };
        public string[] kInterestEnvironmentBalloons = new string[6]
        {
            "ep5_balloon_leaf",
            "balloon_moodlet_beautifulView",
            "balloon_flowerpot",
            "balloon_seeds",
            "balloon_tree",
            "balloon_sunrise"
        };

        public string[] kInterestFashionBalloons = new string[6]
        {
            "ep2_balloon_handheldmirror",
            "ep2_balloon_comb",
            "ep2_balloon_dressmannequin",
            "ep2_balloon_shoes",
            "ep2_balloon_freshclothes",
            "ep2_balloon_dressmannequin"
        };

        public string[] kInterestFoodBalloons = new string[9]
        {
            "balloon_food",
            "balloon_napkin",
            "balloon_servingplatter",
            "balloon_utensils",
            "balloon_moodlet_hungry",
            "balloon_cookbook",
            "balloon_frypan",
            "balloon_ingredients",
            "balloon_recipe"
        };
        public string[] kInterestHealthBalloons = new string[5]
        {
            "balloon_bandaid",
            "balloon_caduceus",
            "balloon_medbottle",
            "balloon_pill",
            "balloon_stethoscope"
        };
        public string[] kInterestMoneyBalloons = new string[5]
        {
            "balloon_cash",
            "balloon_simoleon",
            "balloon_career_simoleon",
            "balloon_diamond",
            "balloon_political_simoleon"
        };
        public string[] kInterestParanormalBalloons = new string[11]
        {
            "balloon_moodlet_creepyGraveyard",
            "balloon_tombstone",
            "balloon_death",
            "balloon_ghost",
            "ep7_balloon_ghost",
            "ep7_balloon_unicorn",
            "ep7_balloon_werewolf",
            "ep3_balloon_fangs",
            "ep7_balloon_complainwitches",
            "ep1_balloon_mummy",
            "ep7_balloon_burningstake"
        };
        public string[] kInterestPoliticsBalloons = new string[6]
        {
            "balloon_intFlags",
            "balloon_podium",
            "balloon_political_simoleon",
            "balloon_vote",
            "balloon_globe",
            "balloon_house"
        };
        public string[] kInterestSchoolBalloons = new string[4]
        {
            "chalkboard",
            "balloon_globe",
            "balloon_schoolbus",
            "balloon_schooldesk",
        };
        public string[] kInterestScifiBalloons = new string[7]
        {
            "ep11_balloon_alien",
            "ep11_balloon_galaxy",
            "ep11_balloon_robots_strong",
            "balloon_planet",
            "balloon_astronaut",
            "balloon_atoms",
            "balloon_moon"
        };

        public string[] kInterestSportsBalloons = new string[8]
        {
            "balloon_baseball",
            "balloon_finger",
            "balloon_football",
            "balloon_pennant",
            "balloon_jogger",
            "balloon_pumped",
            "balloon_weight",
            "balloon_treadmill"
        };

        public string[] kInterestToysBalloons = new string[5]
        {
            "ep7_balloon_toy_blocks",
            "ep7_balloon_teddy_bear",
            "ep7_balloon_doll_house",
            "balloon_toyblocks",
            "balloon_toybear"
        };
        public string[] kInterestTravelBalloons = new string[5]
        {
            "balloon_yeti",
            "balloon_beetles",
            "balloon_flies",
            "balloon_pond",
            "balloon_flowerpot"
        };
        public string[] kInterestWorkBalloons = new string[5]
        {
            "balloon_yeti",
            "balloon_beetles",
            "balloon_flies",
            "balloon_pond",
            "balloon_flowerpot"
        };
        public static List<HobbyLot> mHobbyLot = new List<HobbyLot>();

        public static int TotalScore = 0;

        public static int GetPercentage(Sim actor, Sim target, Interest mInterest)
        {
            if (actor.SimDescription.SimDescriptionId == target.SimDescription.SimDescriptionId)
            {
                return 0;
            }

            //return Localization.LocalizeString("Lyralei/Localized/socialsAskAboutEntry:InteractionName", new object[0]);

            // To convince a sim they:
            // ** need the right sets of traits
            // ** A good frienship score
            // ** optional, but adds a bonus, Skills.

            // Clear previous total score...
            TotalScore = 0;

            int relationshipScore = 0;
            Relationship rel = Relationship.Get(actor, target, false);

            if (rel.AreFriendsOrRomantic())
            {
                relationshipScore = 30;
            }
            TotalScore += relationshipScore;

            // Get score for Traits
            int TraitsScore = 0;

            int AmountOfPositiveTraits = InterestManager.CheckTraitForBoosts(mInterest, target.mSimDescription, false);
            int AmountOfNegTraits = InterestManager.CheckTraitForBoosts(mInterest, target.mSimDescription, true);

            if (AmountOfPositiveTraits > AmountOfNegTraits)
            {
                TraitsScore = 30;
            }
            else if (AmountOfNegTraits == 0 && AmountOfPositiveTraits == 0)
            {
                TraitsScore = 10;
            }
            else if (AmountOfPositiveTraits <= AmountOfNegTraits)
            {
                TraitsScore = -20;
            }
            TotalScore += TraitsScore;

            int SkillScore = 0;

            int AmountOfCompatibleSkills = InterestManager.HowManyCompatibleSkillsForInterest(mInterest, target.mSimDescription);
            //int AmountOfCompatibleSkills = 0;

            if (AmountOfCompatibleSkills == 0)
            {
                SkillScore = 5;
            }
            else if (AmountOfCompatibleSkills > 1 && AmountOfCompatibleSkills <= 3)
            {
                SkillScore = 30;
            }
            else if (AmountOfCompatibleSkills > 5)
            {
                SkillScore = 40;
            }
            TotalScore += SkillScore;

            // Fix for when total score goes over 100 or below 0
            if (TotalScore <= 0)
            {
                TotalScore = 0;
            }
            else if (TotalScore >= 100)
            {
                TotalScore = 100;
            }
            return TotalScore;
        }

        // checkForBeyondNeutral = Is it over 13 points (therefore inside the 'passionate' range)
        public static bool HasTheNecessaryInterest(SimDescription desc, InterestTypes typeWanted, bool checkForBeyondNeutral)
        {
            try
            {
                if (mSavedSimInterests.ContainsKey(desc.SimDescriptionId))
                {
                    foreach (Interest interest in mSavedSimInterests[desc.SimDescriptionId])
                    {
                        if(checkForBeyondNeutral)
                        {
                            if (interest.Guid == typeWanted && interest.currInterestPoints >= 14)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        if (interest.Guid == typeWanted && interest.currInterestPoints >= 11 || interest.currInterestPoints <= 9)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                return false;
            }
            catch(Exception ex)
            {
                print("HasTheNecessaryInterest: " + ex.ToString());
                return false;
            }
        }

        public static void LearnInterest(SimDescription actor, SimDescription target, InterestTypes type, bool isHate)
        {
            try
            {
                if (mSavedSimInterests.ContainsKey(actor.SimDescriptionId) && mSavedSimInterests.ContainsKey(target.SimDescriptionId))
                {
                    for (int i = 0; i < mSavedSimInterests[actor.SimDescriptionId].Count; i++)
                    {
                        if(mSavedSimInterests[actor.SimDescriptionId][i].mInterestsGuid == type)
                        {
                            if(isHate)
                            {
                                if (!mSavedSimInterests[actor.SimDescriptionId][i].mSimKnownWithHateInterests.ContainsKey(target.mSimDescriptionId))
                                {
                                    mSavedSimInterests[actor.SimDescriptionId][i].mSimKnownWithHateInterests.Add(target.mSimDescriptionId, type);
                                    mSavedSimInterests[target.SimDescriptionId][i].mSimKnownWithHateInterests.Add(actor.mSimDescriptionId, type);
                                }
                            }
                            else
                            {
                                if (!mSavedSimInterests[actor.SimDescriptionId][i].mSimKnownWithPassionateInterests.ContainsKey(target.mSimDescriptionId))
                                {
                                    mSavedSimInterests[actor.SimDescriptionId][i].mSimKnownWithPassionateInterests.Add(target.mSimDescriptionId, type);
                                    mSavedSimInterests[target.SimDescriptionId][i].mSimKnownWithPassionateInterests.Add(actor.mSimDescriptionId, type);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                InterestSaveManager.WriteErrorXMLFile("Learn Interest ERROR", ex, null);
                print("LearnInterest: " + ex.ToString());
            }
        }


        /// <summary>
        /// Returns an Int based on whether they love it or hate it (or neutral)
        /// 
        /// 0 - neutral
        /// 1 - loves
        /// 2 - hates
        /// 3 - doesn't have an opinion.
        /// </summary>
        /// <returns>int</returns>
        public static int DoesSimHatesLovesInterest(SimDescription desc, InterestTypes typeWanted)
        {
            try
            {
                if (mSavedSimInterests.ContainsKey(desc.SimDescriptionId))
                {
                    foreach (Interest interest in mSavedSimInterests[desc.SimDescriptionId])
                    {
                        if (interest.Guid == typeWanted)
                        {
                            // return Neutral
                            if (interest.currInterestPoints >= 9 && interest.currInterestPoints <= 13)
                            {
                                return 0;
                            }
                            // return Loves
                            else if (interest.currInterestPoints > 13)
                            {
                                return 1;
                            }
                            // return Hates
                            else if (interest.currInterestPoints < 8)
                            {
                                return 2;
                            }
                        }
                    }
                    // return Doesn't have an option
                    return 3;
                }
                return 3;

            }
            catch(Exception ex)
            {
                InterestSaveManager.WriteErrorXMLFile("DoesSimHateLoveError1", ex, null);
                print("DoesSimHatesLovesInterest: " + ex.ToString());
                return 3;
            }
        }

        public static int DoesSimHatesLovesInterest(Interest interest)
        {

            try
            {
                if(interest == null) { return 3; }

                // return Neutral
                if (interest.currInterestPoints >= 9 && interest.currInterestPoints <= 13)
                {
                    return 0;
                }
                // return Loves
                else if (interest.currInterestPoints > 13)
                {
                    return 1;
                }
                // return Hates
                else if (interest.currInterestPoints < 8)
                {
                    return 2;
                }
                return 3;
            }
            catch (Exception ex)
            {
                InterestSaveManager.WriteErrorXMLFile("DoesSimHateLoveError2", ex, null);
                print("DoesSimHatesLovesInterest: " + ex.ToString());
                return 3;
            }
        }

        public static void SetUpRandomBonusPoints(List<Interest> interests, SimDescription simDescription, bool isTownie)
        {
            if (interests.Count != 0)
            {
                for (int i = 0; i < interests.Count; i++)
                {
                    ApplyTraitMaths(interests[i], simDescription, isTownie);
                }
            }
        }

        public static void ApplyTraitMaths(Interest interest, SimDescription simDescription, bool isTownie)
        {
            try
            {
                int amountOfPenaltyTraits = CheckTraitForBoosts(interest, simDescription, true);
                int amountOfPassionateTraits = CheckTraitForBoosts(interest, simDescription, false);

                if (isTownie)
                {
                    TownieInterestHelper.DoTraitTownieCalculation(interest, amountOfPenaltyTraits, simDescription, true);
                    TownieInterestHelper.DoTraitTownieCalculation(interest, amountOfPassionateTraits, simDescription, false);
                    //TownieInterestHelper.premadeSims.Add(simDescription.FullName);
                }
                else
                {
                    DoTraitCalculation(interest, amountOfPenaltyTraits, simDescription, true);
                    DoTraitCalculation(interest, amountOfPassionateTraits, simDescription, false);
                }
            }
            catch(Exception ex)
            {
                print(ex.ToString());
            }
        }

        public static int CheckTraitForBoosts(Interest interest, SimDescription simDescription, bool checkingForPenalty)
        {
            int amountOfTraits = 0;

            if (interest != null)
            {
                if (checkingForPenalty)
                {
                    if (interest.traitPenalty.Count > 0)
                    {
                        TraitNames[] traitnames = interest.traitPenalty.ToArray();
                        amountOfTraits = simDescription.TraitManager.HasHowManyElements(traitnames);
                        return amountOfTraits;
                    }
                }
                else
                {
                    if (interest.traitBoost.Count > 0)
                    {
                        TraitNames[] traitnames = interest.traitBoost.ToArray();
                        amountOfTraits = simDescription.TraitManager.HasHowManyElements(traitnames);
                        return amountOfTraits;
                    }
                }
            }
            else
            {
                //print("Interest or Nonpersisted was empty :(");
                return 0;
            }
            return 0;
        }

        public static int HowManyCompatibleSkillsForInterest(Interest interest, SimDescription simDescription)
        {
            int AmountOfSkills = 0;

            if(interest != null && interest.mNecessarySkillsForInterest != null && interest.mNecessarySkillsForInterest.Count != 0 )
            {
                foreach (SkillNames name in interest.mNecessarySkillsForInterest)
                {
                    if(simDescription.SkillManager.HasElement(name))
                    {
                        AmountOfSkills++;
                    }
                }
                return AmountOfSkills;
            }
            else
            {
                return 0;
            }
        }

        public static List<TraitNames> GetCompatiblityTraitsForIcons(Interest interest, SimDescription simDescription, bool checkingForPenalty)
        {
            try
            {
                List<TraitNames> traitsReturned = new List<TraitNames>();

                if (interest != null && interest.traitPenalty.Count > 0 && interest.traitBoost.Count > 0)
                {
                    if (checkingForPenalty)
                    {
                        if (interest.traitPenalty.Count > 0)
                        {
                            foreach (TraitNames name in interest.traitPenalty)
                            {
                                if (simDescription.TraitManager.HasElement(name))
                                {
                                    traitsReturned.Add(name);
                                }
                            }
                            return traitsReturned;
                        }
                    }
                    else
                    {
                        if (interest.traitBoost.Count > 0)
                        {
                            foreach (TraitNames name in interest.traitBoost)
                            {
                                if (simDescription.TraitManager.HasElement(name))
                                {
                                    traitsReturned.Add(name);
                                }
                            }
                            return traitsReturned;
                        }
                    }
                }
                else
                {
                    //print("Interest or Nonpersisted was empty :(");
                    return null;
                }
                return null;
            }
            catch(Exception ex)
            {
                print(ex.Message.ToString());
                return null;
            }
        }

        public static void DoTraitCalculation(Interest interests, int amountOfTraits, SimDescription simDescription, bool isForPenalty)
        {
            for (int i = 0; i < amountOfTraits; i++)
            {
                if (isForPenalty)
                {
                    interests.modifyInterestLevel(InterestManager.perTraitPenaltyPointLost, simDescription.SimDescriptionId, interests.mInterestsGuid);
                }
                else
                {
                    interests.modifyInterestLevel(InterestManager.perTraitPassionatePointGained, simDescription.SimDescriptionId, interests.mInterestsGuid);
                }
            }
        }

        public static void AddSubPoints(float toAdd, SimDescription description, InterestTypes interestToAdd)
        {
            if (mSavedSimInterests.ContainsKey(description.SimDescriptionId))
            {
                for (int i = 0; i < mSavedSimInterests[description.SimDescriptionId].Count; i++)
                {
                    if(mSavedSimInterests[description.SimDescriptionId][i].Guid == interestToAdd)
                    {
                        mSavedSimInterests[description.SimDescriptionId][i].mPointsBeforeAddingInterestPoint += toAdd;

                        if(mSavedSimInterests[description.SimDescriptionId][i].CanAddInterestPoint)
                        {
                            mSavedSimInterests[description.SimDescriptionId][i].modifyInterestLevel(1, description.SimDescriptionId, mSavedSimInterests[description.SimDescriptionId][i].mInterestsGuid);
                        }
                    }
                }
            }
        }

        public static bool CanAddSubPointsForSocial(SimDescription description, InterestTypes interestToAdd)
        {
            if (mSavedSimInterests.ContainsKey(description.SimDescriptionId))
            {
                for (int i = 0; i < mSavedSimInterests[description.SimDescriptionId].Count; i++)
                {
                    if (mSavedSimInterests[description.SimDescriptionId][i].Guid == interestToAdd)
                    {
                        if(mSavedSimInterests[description.SimDescriptionId][i].currInterestPoints >= mSavedSimInterests[description.SimDescriptionId][i].mMaxForSocialLevel)
                        {
                            if (!mSavedSimInterests[description.SimDescriptionId][i].mHasNotifiedPlayerAboutSocialSkilling) { description.CreatedSim.ShowTNSIfSelectable("I can no longer gain influence from talking about my interest... maybe I should spend more time with my hobbies instead...", StyledNotification.NotificationStyle.kSimTalking); }
                            mSavedSimInterests[description.SimDescriptionId][i].mHasNotifiedPlayerAboutSocialSkilling = true;

                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static int GetLevelForInterest(SimDescription description, InterestTypes interestToCheck)
        {
            if (mSavedSimInterests.ContainsKey(description.SimDescriptionId))
            {
                for (int i = 0; i < mSavedSimInterests[description.SimDescriptionId].Count; i++)
                {
                    if (mSavedSimInterests[description.SimDescriptionId][i] != null && mSavedSimInterests[description.SimDescriptionId][i].Guid == interestToCheck)
                    {
                        return mSavedSimInterests[description.SimDescriptionId][i].currInterestPoints;
                    }
                }
            }
            return 0; 
        }

        public static ResourceKey GetCorrectTraitIcon(SimDescription desc, Interest interest)
        {
            try
            {
                bool wasDislike = false;
                List<TraitNames> traitNames = new List<TraitNames>();

                if(GetCompatiblityTraitsForIcons(interest, desc, false) == null)
                {
                    return ResourceKey.kInvalidResourceKey;
                }
                else
                {
                    traitNames.AddRange(GetCompatiblityTraitsForIcons(interest, desc, false));
                }
                
                if (traitNames.Count < 1)
                {
                    if (GetCompatiblityTraitsForIcons(interest, desc, true) == null)
                    {
                        return ResourceKey.kInvalidResourceKey;
                    }
                    else
                    {
                        traitNames.AddRange(GetCompatiblityTraitsForIcons(interest, desc, true));
                        wasDislike = true;
                    }
                    // If still null, return nothing....
                    if (traitNames.Count == 0)
                    {
                        return ResourceKey.kInvalidResourceKey;
                    }
                }

                TraitNames randomName = RandomUtil.GetRandomObjectFromList(traitNames);
                Trait trait = TraitManager.GetTraitFromDictionary(TraitNames.AbsentMinded);

                if (wasDislike)
                {
                    return trait.DislikePieMenuKey;
                }
                return trait.PieMenuKey;
            }
            catch(Exception ex)
            {
                print(ex.Message.ToString());
                return ResourceKey.kInvalidResourceKey;
            }
        }

        public static void FixWeirdInterestData(SimDescription simDescription)
        {
            // Fix for if sim isn't in the list.
            if (!mSavedSimInterests.ContainsKey(simDescription.SimDescriptionId))
            {
                print("Target isn't in the town's known interest list... adding them now...");

                List<Interest> interests = GlobalOptionsHobbiesAndInterests.PrepareInterestListForSim(simDescription.SimDescriptionId);
                mSavedSimInterests.Add(simDescription.SimDescriptionId, interests);

                print("Target is now in the town's known interest list!");
            }

            // Fix for if some or any interests are somehow 'null'.

            bool needsFixing = false;

            for (int i = 0; i < mSavedSimInterests[simDescription.SimDescriptionId].Count; i++)
            {
                if(mSavedSimInterests[simDescription.SimDescriptionId][i] == null)
                {
                    print("Target's interest is non-existent! Target's interest data will be reset, but you can always redo this! (Add explanation later...) Fixing this now...");
                    needsFixing = true;
                    break;
                }
            }

            if(needsFixing)
            {
                List<Interest> interests = GlobalOptionsHobbiesAndInterests.PrepareInterestListForSim(simDescription.SimDescriptionId);
                mSavedSimInterests[simDescription.SimDescriptionId].Clear();
                mSavedSimInterests[simDescription.SimDescriptionId] = null;
                mSavedSimInterests[simDescription.SimDescriptionId] = interests;

                SetUpRandomBonusPoints(mSavedSimInterests[simDescription.SimDescriptionId], simDescription, TownieInterestHelper.premadeSims.ContainsKey(simDescription.SimDescriptionId));

                print("Fixed target non-existent interest data!");
            }
        }

        public static int CalculateLifespan()
        {
            uint lifespan = 0u;
            OptionsModel.GetOptionSetting("AgingInterval", out lifespan);
            AgingManager.Singleton.SimDaysPerAgingYear = AgingManager.GetSimDaysPerAgingYear((int)lifespan);

            //AgingManager.Singleton.GetAverageSimLifespanInDaysForUI((int)lifespan);

            return AgingManager.Singleton.GetAverageSimLifespanInDaysForUI((int)lifespan);
        }

        public static List<int> RequiredSkillsLevelForAgeSettings(List<SkillNames> skills, ulong skillOwner)
        {
            try
            {
                if(skills == null || skills.Count <= 0 || skillOwner == 0)
                {
                    return null;
                }

                List<int> amountOfPoints = new List<int>();
                if (AgingManager.Singleton != null && AgingManager.Singleton.Enabled)
                {
                    Sim sim = SimDescription.GetCreatedSim(skillOwner);

                    if (sim != null)
                    {
                        int lifespan = InterestManager.CalculateLifespan();

                        if(lifespan <= 50)
                        {
                            for (int i = 0; i < skills.Count; i++)
                            {
                                amountOfPoints.Add(RandomUtil.GetInt(3, 5));
                            }
                        }
                        else if(lifespan >= 51 && lifespan <= 100)
                        {
                            for (int i = 0; i < skills.Count; i++)
                            {
                                amountOfPoints.Add(RandomUtil.GetInt(5, 7));
                            }
                        }
                        else if(lifespan >= 101 && lifespan <= 180)
                        {
                            for (int i = 0; i < skills.Count; i++)
                            {
                                amountOfPoints.Add(RandomUtil.GetInt(6, 9));
                            }
                        }
                        else if(lifespan >= 181 && lifespan <= 380)
                        {
                            for (int i = 0; i < skills.Count; i++)
                            {
                                amountOfPoints.Add(RandomUtil.GetInt(7, 10));
                            }
                        }
                        else if(lifespan >= 381 && lifespan <= 2000)
                        {
                            for (int i = 0; i < skills.Count; i++)
                            {
                                amountOfPoints.Add(10);
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                    return amountOfPoints;
                }
                else
                {
                    // Since aging is disabled, we just put all the required skills to 10.
                    for (int i = 0; i < skills.Count; i++)
                    {
                        amountOfPoints.Add(10);
                    }
                    return amountOfPoints;
                }
            }
            catch(Exception ex)
            {
                print("Inside Required skills: " + ex.ToString());
                return null;
            }
        }

        public static ListenerAction RerollRequiredSkillsLevelForAgeSettings(Event e)
        {
            Sim[] objects = Sims3.Gameplay.Queries.GetObjects<Sim>();

            foreach(Sim sim in objects)
            {
                //print("e.actor: " + e.Actor.SimDescription.SimDescriptionId.ToString());
                if (sim != null && mSavedSimInterests.ContainsKey(sim.SimDescription.SimDescriptionId))
                {
                    for (int i = 0; i < mSavedSimInterests[sim.SimDescription.SimDescriptionId].Count; i++)
                    {
                        for (int h = 0; h < mSavedSimInterests[sim.SimDescription.SimDescriptionId][i].hobbies.Count; h++)
                        {
                            if (mSavedSimInterests[sim.SimDescription.SimDescriptionId][i].hobbies[h] == null)
                            {
                                continue;
                            }
                            List<int> requiredSkillPointsReroll = new List<int>();
                            requiredSkillPointsReroll = RequiredSkillsLevelForAgeSettings(mSavedSimInterests[sim.SimDescription.SimDescriptionId][i].hobbies[h].mRequiredSkillsForHobby, sim.SimDescription.SimDescriptionId);
                            mSavedSimInterests[sim.SimDescription.SimDescriptionId][i].hobbies[h].mRequiredSkillPoints = requiredSkillPointsReroll;
                        }
                    }
                }
                return ListenerAction.Keep;
            }
            return ListenerAction.Remove;
        }

        public static Dictionary<string, AlarmHandle> mMagazineSubscriptionAlarms = new Dictionary<string, AlarmHandle>();

        public static void SendTheMagazines(Interest interest, ulong simDesc)
        {
            Sim sim = SimDescription.GetCreatedSim(simDesc);

            if (!Sims3.SimIFace.Environment.HasEditInGameModeSwitch && !SimIFace.GameUtils.IsOnVacation() && sim.IsInActiveHousehold)
            {
                if (mSavedSimInterests.ContainsKey(simDesc))
                {
                    if (interest.mHasMagazineSubscription)
                    {
                        Mailbox mailboxOnLot = Mailbox.GetMailboxOnHomeLot(sim);
                        if (mailboxOnLot != null)
                        {
                            Dictionary<string, BookMagazineInterestsData> dictionary = GetRightMagazineDictionary(interest.mInterestsGuid);

                            if (dictionary == null || dictionary.Values == null)
                            {
                                GlobalOptionsHobbiesAndInterests.print("Interests Book List is null");
                                return;
                            }

                            if (sim.FamilyFunds >= 20)
                            {
                                sim.ModifyFunds(-20);
                            }
                            else if (!SimIFace.GameUtils.IsFutureWorld() || sim.FamilyFunds < 20)
                            {
                                StyledNotification.Format format = new StyledNotification.Format("Your magazine subscription here! Unfortunately, we were unable to substract 20 dollars from your funds this week, so you'll be charged a bit extra in your general bills. ", mailboxOnLot.ObjectId, sim.ObjectId, StyledNotification.NotificationStyle.kGameMessageNegative);
                                StyledNotification.Show(format);
                                sim.UnpaidBills += 20;
                            }
                            List<BookMagazineInterestsData> list = new List<BookMagazineInterestsData>(dictionary.Values);

                            if (list.Count > 0)
                            {
                                int @int = RandomUtil.GetInt(list.Count - 1);

                                if (list[@int] == null)
                                {
                                    GlobalOptionsHobbiesAndInterests.print("list[@int] was null, adding 20 simoleans back");
                                    sim.ModifyFunds(20);
                                    return;
                                }
                                else
                                {
                                    BookMagazineInterests bookGeneral = BookMagazineInterests.CreateOutOfWorld(list[@int]);
                                    if (bookGeneral == null)
                                    {
                                        GlobalOptionsHobbiesAndInterests.print("Book is null, adding 20 simoleans back");
                                        sim.ModifyFunds(20);
                                        return;
                                    }

                                    if (!mailboxOnLot.AddMail(bookGeneral, false))
                                    {
                                        GlobalOptionsHobbiesAndInterests.print("Couldn't add to inventory :( adding 20 simoleans back");
                                        sim.ModifyFunds(20);
                                    }
                                }
                            }
                        }
                        
                    }
                }
            }
        }


        public static Dictionary<string, BookMagazineInterestsData> GetRightMagazineDictionary(InterestTypes type)
        {
            switch (type)
            {
                case InterestTypes.Animals:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineAnimals;
                case InterestTypes.Crime:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineCrime;
                case InterestTypes.Culture:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineCulture;
                case InterestTypes.Entertainment:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineEntertainment;
                case InterestTypes.Environment:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineEnvironment;
                case InterestTypes.Fashion:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineFashion;
                case InterestTypes.Food:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineFood;
                case InterestTypes.Health:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineHealth;
                case InterestTypes.Money:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineMoney;
                case InterestTypes.Paranormal:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineParanormal;
                case InterestTypes.Politics:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazinePolitics;
                case InterestTypes.Scifi:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineScifi;
                case InterestTypes.Sports:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineSports;
                case InterestTypes.Toys:
                    return GlobalOptionsHobbiesAndInterests.retrieveData.mMagazineToys;
                case InterestTypes.None:
                    return null;
            }
            return null;
        }

        public static Dictionary<string, InteractionDefinition> DefaultGardeningInteractions = new Dictionary<string, InteractionDefinition>()
        {
            { "Harvest Plant", HarvestPlant.Harvest.Singleton },
            { Plant.TendGarden<Plant>.LocalizeString("TendGarden"), Plant.TendGarden<Plant>.Singleton },
            { Localization.LocalizeString("Gameplay/Objects/Gardening/Plant/DisposeAllDeadPlants:DisposeAllDeadPlants"), Plant.DisposeDeadPlant.Singleton },
            { "Revive Plant", Plant.RevivePlant.Singleton },
            { "Talk To Plant", Plant.TalkToPlant.Singleton },
            { "View Plant", Plant.ViewPlant.Singleton },
            { "Plant New Plants", PlantObject.Singleton }
        };

        public static Dictionary<string, InteractionDefinition> DefaultBeeKeepingInteractions = new Dictionary<string, InteractionDefinition>()
        {
            { BeekeepingBox.LocalizeString("FeedBees"), BeekeepingBox.Feed.Singleton },
            { BeekeepingBox.LocalizeString("CleanBox"), BeekeepingBox.Clean.Singleton },
            { BeekeepingBox.LocalizeString("HarvestHoney"), BeekeepingBox.Harvest.Singleton },
            { BeekeepingBox.LocalizeString("SmokeOut"), BeekeepingBox.SmokeOut.Singleton },
        };


        public static Dictionary<string, InteractionDefinition> GetDefaultLotInteractions(InterestTypes type)
        {
            Dictionary<string, InteractionDefinition> mDefaultInteractions = new Dictionary<string, InteractionDefinition>();

            switch(type)
            {
                case InterestTypes.Animals:
                    //mDefaultInteractions.Add();
                    if (SimIFace.GameUtils.IsInstalled(ProductVersion.EP7))
                    {
                        mDefaultInteractions = CommonHelpers.MergeInPlace(mDefaultInteractions, DefaultBeeKeepingInteractions);
                    }
                    break;
                case InterestTypes.Crime:
                    break;
                case InterestTypes.Culture:
                    break;
                case InterestTypes.Entertainment:
                    break;
                case InterestTypes.Environment:

                    try
                    {
                        mDefaultInteractions.Add(Localization.LocalizeString("Gameplay/Objects/Fishing:Fish"), FishAutonomously.Singleton);
                        mDefaultInteractions = CommonHelpers.MergeInPlace(mDefaultInteractions, DefaultGardeningInteractions);

                        if (SimIFace.GameUtils.IsInstalled(ProductVersion.EP7))
                        {
                            mDefaultInteractions = CommonHelpers.MergeInPlace(mDefaultInteractions, DefaultBeeKeepingInteractions);
                        }
                        return mDefaultInteractions;
                    }
                    catch(Exception ex)
                    {
                        print(ex.ToString());
                        return null;
                    }
                case InterestTypes.Fashion:
                    break;
                case InterestTypes.Food:
                    break;
                case InterestTypes.Health:
                    break;
                case InterestTypes.Money:
                    break;
                case InterestTypes.Paranormal:
                    break;
                case InterestTypes.Politics:
                    break;
                case InterestTypes.Scifi:
                    break;
                case InterestTypes.Sports:
                    break;
                case InterestTypes.Toys:
                    break;
                default:
                    return new Dictionary<string, InteractionDefinition>();
            }

            return new Dictionary<string, InteractionDefinition>();
        }

        public static Interest GetInterestFromInterestType(InterestTypes type, Sim sim)
        {
            if(sim != null && mSavedSimInterests.ContainsKey(sim.SimDescription.SimDescriptionId))
            {
                for (int i = 0; i < mSavedSimInterests[sim.SimDescription.SimDescriptionId].Count; i++)
                {
                    if(mSavedSimInterests[sim.SimDescription.SimDescriptionId][i].mInterestsGuid == type)
                    {
                        return mSavedSimInterests[sim.SimDescription.SimDescriptionId][i];
                    }
                }
            }
            return null; 
        }



        public static void print(string text)
        {
            SimpleMessageDialog.Show("Lyralei's Interests & Hobbies", text.ToString());
        }
    }
}

