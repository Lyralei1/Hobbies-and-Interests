using Lyralei;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.Hud;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Environment : Interest
    {
        public int indexInList = 0;

        public bool isAVegetarian = false;

        public bool mHasInstalledSolarPanels = false;

        public Environment(ulong simDescription, bool vegetarian)
            : base(InterestTypes.Environment, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Environment;
            this.sHobbySkillListener = EventTracker.AddListener(EventTypeId.kSkillLevelUp, new ProcessEventDelegate(OnSkilledUpHobbyHelper));
            //this.sHobbyInteractionListener = EventTracker.AddListener(EventTypeId.kUserDirectedInteraction, new ProcessEventDelegate(OnInteractionHelper));
            this.sHobbyInteractionListener = EventTracker.AddListener(EventTypeId.kInteractionSuccess, new ProcessEventDelegate(OnInteractionHelper));

            this.indexInList = 4;
            this.mHasInstalledSolarPanels = false;

            this.Name = "Environment"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";
            this.traitBoost = new List<TraitNames>() {
                                    TraitNames.GreenThumb,
                                    TraitNames.SuperGreenThumb,
                                    TraitNames.Handy,
                                    TraitNames.GathererTrait,
                                    TraitNames.Angler,
                                    TraitNames.EnvironmentallyConscious,
                                    TraitNames.LovesTheOutdoors,
                                    TraitNames.Vegetarian
                                };

            this.traitPenalty = new List<TraitNames>() {
                                    TraitNames.HatesOutdoors,
                                    TraitNames.CouchPotato
                                };

            this.mNecessarySkillsForInterest = new List<SkillNames>()
            {
                SkillNames.Gardening,
                SkillNames.Handiness,
                SkillNames.Fishing
            };
            isAVegetarian = vegetarian;

            ClimateChangeHobby climateChangeHobby = new ClimateChangeHobby(vegetarian, this);
            NatureHobby natureHobby = new NatureHobby(this);
            OffTheGrid offTheGrid = new OffTheGrid(this);

            this.mHobbies = new List<Hobby>() {
                climateChangeHobby,
                natureHobby,
                offTheGrid
            };
        }

        public Environment()
        {
        }

        public ListenerAction OnSkilledUpHobbyHelper(Event e)
        {
            SimDescription sim = SimDescription.Find(this.InterestOwner);
            if (sim != null && sim.CreatedSim != null)
            {
                for(int i = 0; i < this.mHobbies.Count; i++)
                {
                    for(int h = 0; h < this.mHobbies[i].mRequiredSkillsForHobby.Count; h++)
                    {
                        if (sim.SkillManager.HasElement(this.mHobbies[i].mRequiredSkillsForHobby[h]))
                        {
                            hobbies[i].IsCompleted(sim.CreatedSim, this.mHobbies[i].mRequiredSkillsForHobby, this.mHobbies[i].mRequiredSkillPoints);
                        }
                    }
                }
            }
            return ListenerAction.Keep;
        }
        public ListenerAction OnInteractionHelper(Event e)
        {
            InteractionSuccessEvent interactionSuccessEvent = e as InteractionSuccessEvent;
            StringEvent stringEvent = e as StringEvent;
            if (interactionSuccessEvent != null)
            {
                //InterestManager.print(interactionSuccessEvent.IOP.InteractionDefinition.GetType().FullName);
            }

            if(interactionSuccessEvent.IOP.InteractionDefinition.GetType().FullName.Contains(typeof(Objects.RabbitHoles.ScienceLab.SellSamples).FullName))
            {
                CheckInteractionForHobbies(interactionSuccessEvent);
            }

            return ListenerAction.Keep;
        }

        public void CheckInteractionForHobbies(InteractionSuccessEvent interactionSuccessEvent)
        {
            for(int i = 0; i < this.mHobbies.Count; i++)
            {
                if (this.mHobbies[i] is ClimateChangeHobby)
                {
                    ClimateChangeHobby hobbyClimate = this.mHobbies[i] as ClimateChangeHobby;
                    hobbyClimate.PlantSamplesDonated++;

                    for (int c = 0; c < this.mHobbies[i].mHobbyChallenges.Count; c++)
                    {
                        if (this.mHobbies[i].mHobbyChallenges[c] is SellPlantSamples)
                        {
                            SellPlantSamples hobbyPlants = this.mHobbies[i].mHobbyChallenges[c] as SellPlantSamples;
                            hobbyPlants.PlantSamples++;
                        }
                    }
                    return;
                }
            }
        }

        [Persistable(false)]
        public List<Hobby> mTrackedStats;

        public List<Hobby> TrackedStats
        {
            get
            {
                return mTrackedStats;
            }
        }

        //public override void CreateInterestJournalInfo()
        //{
        //    NatureHobby mNatureHobby = new NatureHobby(this);
        //    ClimateChangeHobby mClimateChangeHobby = new ClimateChangeHobby(isAVegetarian, this);
        //    OffTheGrid offTheGrid = new OffTheGrid(this);

        //    mTrackedStats = new List<Hobby>();
        //    mTrackedStats.Add(mNatureHobby);
        //    mTrackedStats.Add(mClimateChangeHobby);
        //    mTrackedStats.Add(offTheGrid);
        //}


        public class ClimateChangeHobby : Hobby
        {
            public int mSimsConverted = 0;
            public int SimsConverted
            {
                get
                {
                    return mSimsConverted;
                }
                set
                {
                    mSimsConverted = value;
                }
            }

            public int mDonatedToCauses = 0;
            public int DonatedToCauses
            {
                get
                {
                    return mDonatedToCauses;
                }
                set
                {
                    mDonatedToCauses = value;
                }
            }

            public int mPlantSamplesDonated = 0;
            public int PlantSamplesDonated
            {
                get
                {
                    return mPlantSamplesDonated;
                }
                set
                {
                    mPlantSamplesDonated = value;
                }
            }

            public int mTreesPlantedAndGrown = 0;
            public int TreesPlantedAndGrown
            {
                get
                {
                    return mTreesPlantedAndGrown;
                }
                set
                {
                    mTreesPlantedAndGrown = value;
                }
            }

            public bool mIsVegetarian = false;
            public bool IsVegetarian
            {
                get
                {
                    return mIsVegetarian;
                }
                set
                {
                    mIsVegetarian = value;
                }
            }

            private bool mIsMasterInHobby = false;
            public bool IsMasterInHobby
            {
                get
                {
                    SimDescription sim = SimDescription.Find(mLinkedInterestInstance.InterestOwner);
                    return this.IsCompleted(sim.CreatedSim, this.mRequiredSkillsForHobby, this.mRequiredSkillPoints);
                }
                set
                {
                    mIsMasterInHobby = value;
                }
            }

            public AlarmHandle mDoClimateChangeWorry = AlarmHandle.kInvalidHandle;

            public ClimateChangeHobby(bool isVegetarian, Interest interest)
            {
                this.mName                                  = "Climate change";

                if(interest != null)
                {
                    this.mLinkedInterestInstance            = interest;
                }

                this.mDescription                           = "Sims who love the environment, can also decide to help the climate! Everything that helps the planet a little is a very interesting hobby for them.";
                this.IsMasterInHobby                        = false;
                this.mRequiredSkillsForHobby                = new List<SkillNames>() { SkillNames.Charisma };
                this.mSkillsOptional                        = new List<SkillNames>() { SkillNames.Science };
                this.mRequiredSkillPoints                   = new List<int>();
                this.mRequiredSkillPoints                   = InterestManager.RequiredSkillsLevelForAgeSettings(this.mRequiredSkillsForHobby, this.mLinkedInterestInstance.InterestOwner);
                this.mOptionalSkillPoints                   = InterestManager.RequiredSkillsLevelForAgeSettings(this.mSkillsOptional, this.mLinkedInterestInstance.InterestOwner);


                ConvinceSimsToInterest convinceSims         = new ConvinceSimsToInterest(interest);
                DonateToGoodCauses donateToGoodCauses       = new DonateToGoodCauses(interest);
                PlantTrees plantTrees                       = new PlantTrees(interest);

                if (GameUtils.IsInstalled(ProductVersion.EP9))
                {
                    SellPlantSamples sellPlantSamples = new SellPlantSamples(interest);
                    this.mHobbyChallenges.Add(sellPlantSamples);
                }

                this.mHobbyChallenges.Add(convinceSims);
                this.mHobbyChallenges.Add(donateToGoodCauses);
                this.mHobbyChallenges.Add(plantTrees);

                IsVegetarian = isVegetarian;

                int dayToWorry = RandomUtil.GetInt(4, 10);

                mDoClimateChangeWorry = AlarmManager.Global.AddAlarmRepeating(dayToWorry, TimeUnit.Days, DoClimateChangeWorry, dayToWorry, TimeUnit.Days, "Environment_worryAbout", AlarmType.AlwaysPersisted, null);
            }

            public void DoClimateChangeWorry()
            {
                SimDescription owner = SimDescription.Find(mLinkedInterestInstance.InterestOwner);

                if (owner == null)
                    return;
                if (owner.CreatedSim.BuffManager.HasElement(0xA74C038A60FBF398))
                {
                    return;
                }

                bool morelikely = owner.TraitManager.HasAnyElement(new TraitNames[] { TraitNames.Neurotic, TraitNames.GreenThumb, TraitNames.Unstable, TraitNames.BroodingTrait, TraitNames.Vegetarian, TraitNames.EnvironmentallyConscious, TraitNames.LovesTheOutdoors, TraitNames.OverEmotional });
                
                if(morelikely && RandomUtil.CoinFlip())
                {
                    owner.CreatedSim.BuffManager.AddElement(0xA74C038A60FBF398, Origin.FromBeingScared);
                }
                if(!morelikely && RandomUtil.RandomChance(45f))
                {
                    owner.CreatedSim.BuffManager.AddElement(0xA74C038A60FBF398, Origin.FromBeingScared);
                }
            }

            public override string ToString()
            {
                SimDescription simDescription = SimDescription.Find(this.mLinkedInterestInstance.InterestOwner);

                StringBuilder sb = new StringBuilder();
                if(simDescription != null && simDescription.CreatedSim != null)
                {
                    sb.AppendLine(this.mName);
                    sb.AppendLine(this.mId.ToString());
                    sb.AppendLine(this.mDescription);
                    sb.AppendLine("");
                    sb.AppendLine("Master in Hobby: " + this.IsMasterInHobby.ToString());
                    sb.AppendLine("Is Vegetarian: " + this.IsVegetarian.ToString());
                    sb.AppendLine("");
                    sb.AppendLine("Required Skill(s):");

                    int index = 0;
                    foreach (SkillNames skill in this.mRequiredSkillsForHobby)
                    {
                        sb.AppendLine("*" + skill.ToString() + " points: " + simDescription.SkillManager.GetSkillLevel(skill) + "(Needs: " + this.mRequiredSkillPoints[index] + ")");
                        index++;
                    }

                    sb.AppendLine("");
                    sb.AppendLine("Optional Skill(s):");

                    index = 0;
                    foreach (SkillNames skill in this.mSkillsOptional)
                    {
                        sb.AppendLine(skill.ToString() + " points: " + simDescription.SkillManager.GetSkillLevel(skill) + "(Needs: " + this.mOptionalSkillPoints[index] + ")");
                        index++;
                    }
                    sb.AppendLine("");

                    sb.AppendLine("Donated to causes: " + this.DonatedToCauses.ToString());
                    sb.AppendLine("Times Samples Donated: " + this.PlantSamplesDonated.ToString());
                    sb.AppendLine("Sims Influenced: " + this.SimsConverted.ToString());
                    sb.AppendLine("Trees planted & Grown: " + this.TreesPlantedAndGrown.ToString());
                }
                return sb.ToString();
            }

            public bool ImportContent(IPropertyStreamReader reader)
            {
                reader.ReadBool(2424195054u,  out mIsVegetarian, false);
                reader.ReadInt32(4107471323u, out mTreesPlantedAndGrown, 0);
                reader.ReadInt32(2554671113u, out mPlantSamplesDonated, 0);
                reader.ReadInt32(2069381774u, out mDonatedToCauses, 0);
                reader.ReadInt32(1533178144u, out mSimsConverted, 0);
                return true;
            }

            public void ExportContent(IPropertyStreamWriter writer)
            {
                writer.WriteBool(2424195054u, mIsVegetarian);
                writer.WriteInt32(4107471323u, mTreesPlantedAndGrown);
                writer.WriteInt32(2554671113u, mPlantSamplesDonated);
                writer.WriteInt32(2069381774u, mDonatedToCauses);
                writer.WriteInt32(1533178144u, mSimsConverted);
            }

            public void MergeTravelData(Hobby hobby)
            {
                ClimateChangeHobby climateChangeHobby = hobby as ClimateChangeHobby;
                mIsVegetarian = climateChangeHobby.mIsVegetarian;
                mTreesPlantedAndGrown = climateChangeHobby.mTreesPlantedAndGrown;
                mPlantSamplesDonated = climateChangeHobby.mPlantSamplesDonated;
                mDonatedToCauses = climateChangeHobby.mDonatedToCauses;
                mSimsConverted = climateChangeHobby.mSimsConverted;
            }
        }

        public class ConvinceSimsToInterest : HobbyChallenge
        {
            public int mSimsConverted = 0;
            public int SimsConverted
            {
                get
                {
                    return mSimsConverted;
                }
                set
                {
                    mSimsConverted = value;
                }
            }

            public int mAmountToFinish = 5;

            public bool isComplete
            {
                get
                {
                    if (SimsConverted >= mAmountToFinish)
                    {
                        return this.isComplete = true;
                    }
                    return this.mIsCompleted;
                }
                set
                {
                    this.mIsCompleted = value;
                }
            }

            public ConvinceSimsToInterest(Interest interest)
            {
                this.mName = "Convince 5 Sims to Pursue the Environment Interest";
                this.mDescription = "Whether it might help the world a little or not, your sim would love to convince at least 5 sims to pursue the Environment Interest";
                this.mLinkedInterestInstance = interest;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(this.mName);
                sb.AppendLine(this.mDescription);
                sb.AppendLine();
                sb.AppendLine("Sims influenced: " + this.SimsConverted.ToString() + "( " + this.mAmountToFinish.ToString() + ")");
                return sb.ToString();
            }
        }

        public class DonateToGoodCauses : HobbyChallenge
        {
            public int mDonatedToGoodCauses = 0;
            public int DonatedToGoodCauses
            {
                get
                {
                    return mDonatedToGoodCauses;
                }
                set
                {
                    mDonatedToGoodCauses = value;
                }
            }

            public int mAmountToFinish = 20;

            public bool isComplete
            {
                get
                {
                    if(DonatedToGoodCauses >= mAmountToFinish)
                    {
                        return this.isComplete = true;
                    }
                    return this.mIsCompleted;
                }
                set
                {
                    this.mIsCompleted = value;
                }
            }

            public DonateToGoodCauses(Interest interest)
            {
                this.mName = "Donated to at least 20 good causes";
                this.mDescription = "Your sim is looking for ways to donate to plenty of Climate Chance-related causes, to help the organisations continue their progress.";
                this.mLinkedInterestInstance = interest;
            }

            public override string ToString()
            {

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(this.mName);
                sb.AppendLine(this.mDescription);
                sb.AppendLine();
                sb.AppendLine("Donated to good cause: " + this.DonatedToGoodCauses.ToString() + "( " + this.mAmountToFinish.ToString() + ")");
                return sb.ToString();
            }
        }

        public class SellPlantSamples : HobbyChallenge
        {
            public int mSellPlantSamples = 0;

            public int PlantSamples
            {
                get
                {
                    return mSellPlantSamples;
                }
                set
                {
                    mSellPlantSamples = value;
                }
            }
            public int mAmountToFinish = 25;

            public bool isComplete
            {
                get
                {
                    if (PlantSamples >= mAmountToFinish)
                    {
                        return this.isComplete = true;
                    }
                    return this.mIsCompleted;
                }
                set
                {
                    this.mIsCompleted = value;
                }
            }

            public SellPlantSamples(Interest interest)
            {
                this.mName = "Sell 25 Plant Samples to Science";
                this.mDescription = "The Science lab is always asking for more Plant Samples, so that they can archive plant breeds that are dying out or are no longer around to grow them back. By Selling 25 Plant Samples, you'll help this archive grow.";
                this.mLinkedInterestInstance = interest;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(this.mName);
                sb.AppendLine(this.mDescription);
                sb.AppendLine();
                sb.AppendLine("Plant Samples Donated: " + this.PlantSamples.ToString() + "( " + this.mAmountToFinish.ToString() + ")");
                return sb.ToString();
            }
        }

        public class PlantTrees : HobbyChallenge
        {
            public int mPlantedTrees = 0;

            public int PlantedTrees
            {
                get
                {
                    return mPlantedTrees;
                }
                set
                {
                    mPlantedTrees = value;
                }
            }
            public int mAmountToFinish = 5;

            public bool isComplete
            {
                get
                {
                    if (PlantedTrees >= mAmountToFinish)
                    {
                        return this.isComplete = true;
                    }
                    return this.mIsCompleted;
                }
                set
                {
                    this.mIsCompleted = value;
                }
            }

            public PlantTrees(Interest interest)
            {
                this.mName = "Plant 5 Trees and Watch Them Age";
                this.mDescription = "In average, a tree takes 30 years to mature. Your sim would love to see at least 5 trees maturing in their lifetime.";
                this.mLinkedInterestInstance = interest;
            }

            public override string ToString()
            {

                StringBuilder sb = new StringBuilder();
                
                sb.AppendLine(this.mName);
                sb.AppendLine(this.mDescription);
                sb.AppendLine();
                sb.AppendLine("Planted Trees: " + this.PlantedTrees.ToString() + "( " + this.mAmountToFinish.ToString() + ")");
                return sb.ToString();
            }
        }

        public class NatureHobby : Hobby
        {
            public int FishCaughtCount = 0; // 

            public NatureHobby(Interest interest)
            {
                this.mName                          = "Part of Nature";
                this.mDescription                   = "Nature has some really cool and entertaining things to it, and these sims want to experience that to the fullest!";
                this.mIsMasterInHobby               = false;
                this.mLinkedInterestInstance        = interest;
                this.mRequiredSkillsForHobby        = new List<SkillNames>() { SkillNames.Gardening, SkillNames.Collecting };
                this.mSkillsOptional                = new List<SkillNames>() { SkillNames.Fishing };

                this.mRequiredSkillPoints           = new List<int>();
                this.mRequiredSkillPoints           = InterestManager.RequiredSkillsLevelForAgeSettings(this.mRequiredSkillsForHobby, this.mLinkedInterestInstance.InterestOwner);
                this.mOptionalSkillPoints           = InterestManager.RequiredSkillsLevelForAgeSettings(this.mSkillsOptional, this.mLinkedInterestInstance.InterestOwner);
            }

            public override string ToString()
            {
                SimDescription simDescription = SimDescription.Find(this.mLinkedInterestInstance.InterestOwner);

                StringBuilder sb = new StringBuilder();
                if (simDescription != null && simDescription.CreatedSim != null)
                {
                    sb.AppendLine(this.mName);
                    sb.AppendLine(this.mId.ToString());
                    sb.AppendLine(this.mDescription);
                    sb.AppendLine();
                    sb.AppendLine("Required Skill(s):");

                    int index = 0;
                    foreach (SkillNames skill in this.mRequiredSkillsForHobby)
                    {
                        sb.AppendLine("*" + skill.ToString() + " points: " + simDescription.SkillManager.GetSkillLevel(skill) + "(Needs: " + this.mRequiredSkillPoints[index] + ")");
                        index++;
                    }

                    sb.AppendLine();
                    sb.AppendLine("Optional Skill(s):");

                    index = 0;
                    foreach (SkillNames skill in this.mSkillsOptional)
                    {
                        sb.AppendLine("*" + skill.ToString() + " points: " + simDescription.SkillManager.GetSkillLevel(skill) + "(Needs: " + this.mOptionalSkillPoints[index] + ")");
                        index++;
                    }

                    sb.AppendLine("Fish Caught: " + this.FishCaughtCount.ToString());
                }
                return sb.ToString();
            }
        }

        public class OffTheGrid : Hobby
        {
            public OffTheGrid(Interest interest)
            {
                this.mName = "Off-The-Grid";
                this.mDescription = "Some sims just absolutely love the aspects of taking advantage of nature's resources! We've done it in the olden days, why not as a hobby now?";
                this.mIsMasterInHobby = false;
                this.mLinkedInterestInstance = interest;
                this.mRequiredSkillsForHobby = new List<SkillNames>() { SkillNames.Handiness, SkillNames.Gardening, SkillNames.Inventing };
                this.mSkillsOptional = new List<SkillNames>() { SkillNames.Collecting };
                this.mRequiredSkillPoints = new List<int>();
                this.mRequiredSkillPoints = InterestManager.RequiredSkillsLevelForAgeSettings(this.mRequiredSkillsForHobby, this.mLinkedInterestInstance.InterestOwner);
                this.mOptionalSkillPoints = InterestManager.RequiredSkillsLevelForAgeSettings(this.mSkillsOptional, this.mLinkedInterestInstance.InterestOwner);

            }

            public override string ToString()
            {
                SimDescription simDescription = SimDescription.Find(this.mLinkedInterestInstance.InterestOwner);

                StringBuilder sb = new StringBuilder();
                if (simDescription != null && simDescription.CreatedSim != null)
                {
                    sb.AppendLine(this.mName);
                    sb.AppendLine(this.mId.ToString());
                    sb.AppendLine(this.mDescription);
                    sb.AppendLine();
                    sb.AppendLine("Required Skill(s):");

                    int index = 0;
                    foreach (SkillNames skill in this.mRequiredSkillsForHobby)
                    {
                        sb.AppendLine("*" + skill.ToString() + " points: " + simDescription.SkillManager.GetSkillLevel(skill) + "(Needs: " + this.mRequiredSkillPoints[index] + ")");
                        index++;
                    }

                    sb.AppendLine();
                    sb.AppendLine("Optional Skill(s):");

                    index = 0;
                    foreach (SkillNames skill in this.mSkillsOptional)
                    {
                        sb.AppendLine("*" + skill.ToString() + " points: " + simDescription.SkillManager.GetSkillLevel(skill) + "(Needs: " + this.mOptionalSkillPoints[index] + ")");
                        index++;
                    }

                    //sb.AppendLine("Fish Caught: " + this.FishCaughtCount.ToString());
                }
                return sb.ToString();
            }
        }
    }
}
