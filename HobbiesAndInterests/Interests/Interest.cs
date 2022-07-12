using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Utilities;
using Sims3.UI;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Skills;
using System.Collections.Generic;
using Lyralei;
using Lyralei.InterestMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    [Persistable]
    public class Interest : IHasGuid<InterestTypes>
    {

        //[Persistable(false)]
        //public class NonPersistableInterestData
        //{
        //[Persistable(false)]
        public string Name = "";

        //[Persistable(false)]
        public string Description = "";

        //public string ThoughtBalloonTopicString;

        //public bool AlwaysDisplayLevelUpTns;

        //public CASAGSAvailabilityFlags AvailableAgeSpecies = CASAGSAvailabilityFlags.None;

        //public string DreamsAndPromisesIcon;

        //public ResourceKey DreamsAndPromisesIconKey;

        //public ResourceKey IconKey;

        //public ResourceKey SkillUIIconKey;

        [TunableComment("Description: List of traits that will boost points from this interest")]
        [Tunable]
        [Persistable(false)]
        public List<TraitNames> traitBoost = new List<TraitNames>();

        [TunableComment("Description: List of traits that will substract points from this interest")]
        [Tunable]
        [Persistable(false)]
        public List<TraitNames> traitPenalty = new List<TraitNames>();

        public List<Hobby> mHobbies;
        //}

        //[Persistable(false)]
        //public NonPersistableInterestData mNonPersistableData;

        //[Persistable(false)]
        //public int listIndex = 0;

        //public NonPersistableInterestData NonPersistableData
        //{
        //    get
        //    {
        //        return mNonPersistableData;
        //    }
        //    set
        //    {
        //        mNonPersistableData = value;
        //    }
        //}

        [Persistable(false)]
        public Hobby mHobby;

        public Hobby hobby
        {
            get
            {
                return mHobby;
            }
            set
            {
                mHobby = value;
            }
        }

        public List<Hobby> hobbies
        {
            get
            {
                if (mHobbies != null)
                {
                    return mHobbies;
                }
                return null;
            }
            set
            {
                mHobbies = value;
            }
        }

        // Stores information if owner of interest has any sims compatible with interest.
        [Persistable(true)]
        public Dictionary<ulong, InterestTypes> mSimKnownWithPassionateInterests = new Dictionary<ulong, InterestTypes>();

        public Dictionary<ulong, InterestTypes> SimKnownWithPassionateInterests
        {
            get
            {
                if (mSimKnownWithPassionateInterests != null)
                {
                    return mSimKnownWithPassionateInterests;
                }
                return null;
            }
            set
            {
                mSimKnownWithPassionateInterests = value;
            }
        }
        // Stores information if owner of interest has any sims hating this interest.
        [Persistable(true)]
        public Dictionary<ulong, InterestTypes> mSimKnownWithHateInterests = new Dictionary<ulong, InterestTypes>();

        public Dictionary<ulong, InterestTypes> SimKnownWithHateInterests
        {
            get
            {
                if (mSimKnownWithHateInterests != null)
                {
                    return SimKnownWithHateInterests;
                }
                return null;
            }
            set
            {
                SimKnownWithHateInterests = value;
            }
        }

        public InterestTypes mInterestsGuid;

        public InterestTypes Guid
        {
            get
            {
                return mInterestsGuid;
            }
            set
            {
                mInterestsGuid = value;
            }
        }

        public ulong mInterestOwner = 0;

        public ulong InterestOwner
        {
            get
            {
                return mInterestOwner;
            }
            set
            {
                mInterestOwner = value;
            }
        }

        public Interest(InterestTypes guid, ulong InterestOwner)
        {
            mInterestsGuid = guid;
            mInterestOwner = InterestOwner;
        }

        public Interest()
        {
            CreateInterestJournalInfo();
        }

        public virtual void CreateInterestJournalInfo()
        {
        }

        // In case we need to check whether a sim has a certain skill to participate in a interest.
        public List<SkillNames> mNecessarySkillsForInterest = new List<SkillNames>();

        public static int maxInterestPoints = 20;

        public static int minInterestPoints = 0;

        public int mMaxForSocialLevel = 13;

        public int currInterestPoints = 10;

        public bool mHasMagazineSubscription = false;

        [Persistable(true)]
        public float PercentageResearchDone = 0f;

        // Var below shares in decimals how long till an actual point can be added.
        [Persistable(true)]
        public float mPointsBeforeAddingInterestPoint = 0;

        public bool CanAddInterestPoint
        {
            get
            {
                return mPointsBeforeAddingInterestPoint >= 1f;
            }
        }

        // buff modifier will have to get modified to +AmountOfBuff and used for calculating how quick an interest should grow.
        public float mBuffModifier = 0f;

        public virtual Interest Clone()
        {
            Interest interest = base.MemberwiseClone() as Interest;
            //interest.InterestOwner = owner;
            interest.CreateInterestJournalInfo();
            return interest;
        }

        public int SetInterestPoint()
        {
            int num = 0;
            string input = StringInputDialog.Show("Set Interest Point", "Please type in a number from 0 to 10 for the interest points", 1.ToString());

            if (string.IsNullOrEmpty(input))
            {
                SimpleMessageDialog.Show("Set Interest Point", "No input found!");
                return 0;
            }
            else if (!int.TryParse(input, out num))
            {
                SimpleMessageDialog.Show("Set Interest Point", "Value is either not a int value or clearly not a number to begin with");
                return 0;
            }
            else
            {
                return int.Parse(input);
            }
        }

        public int indexInList = 0;

        // for adding full Levels!!!
        public int modifyInterestLevel(int num, ulong desc, InterestTypes type)
        {
            //InterestManager.mSavedSimInterests
            if (currInterestPoints + num <= maxInterestPoints && currInterestPoints + num >= minInterestPoints)
            {
                for (int i = 0; i < InterestManager.mSavedSimInterests[desc].Count; i++)
                {
                    if (InterestManager.mSavedSimInterests[desc][i].mInterestsGuid == type)
                    {
                        InterestManager.mSavedSimInterests[desc][i].currInterestPoints += num;
                        //InterestManager.mSavedSimInterests[desc][i] = this;
                        SimDescription sim = SimDescription.Find(desc);

                        if (currInterestPoints == 11 && sim.mSim.IsActiveSim)
                        {
                            sim.mSim.ShowTNSIfSelectable("I'm feeling pretty passionate about the " + InterestManager.mSavedSimInterests[desc][i].mInterestsGuid.ToString() + " interest! I really should try out the different hobbies it comes with... (Newly unlocked hobbies can be seen under Sim > Interests & hobbies > See interests of sim...)", StyledNotification.NotificationStyle.kSimTalking);
                        }
                    }
                }
            }
            return num;
        }

        public virtual bool ImportContent(IPropertyStreamReader reader)
        {
            reader.ReadInt32(0x151F28DA, out currInterestPoints, 0);
            reader.ReadFloat(0x151F28DA, out mPointsBeforeAddingInterestPoint, 0);
            //reader.ReadUint64(0x1A0285E0, out SavedHobbyLotID, 0);
            //reader.ReadFloat(2627064505u, out SkillPoints, 0f);
            //reader.ReadFloat(3502711471u, out SkillGainRate, 0f);
            //bool[] array;
            //reader.ReadBool(4108196078u, out array);
            //if (array.Length > 0)
            //{
            //    mLifetimeOppsShown = new List<bool>(array);
            //}
            return true;
        }

        public void ExportContent(IPropertyStreamWriter writer)
        {
            writer.WriteInt32(0x151F28DA, currInterestPoints);
            writer.WriteFloat(0x151F28DA, mPointsBeforeAddingInterestPoint);
            //writer.WriteUint64(0x1A0285E0, SavedHobbyLotID);
        }

        public class Hobby
        {
            public int mId = 0;

            public string mName = "";

            public string mDescription = "";

            public Interest mLinkedInterestInstance = null;

            public List<SkillNames> mRequiredSkillsForHobby = new List<SkillNames>();

            public List<int> mRequiredSkillPoints = new List<int>();

            public List<SkillNames> mSkillsOptional = new List<SkillNames>();

            public bool mIsMasterInHobby = false;

            public List<HobbyChallenge> mHobbyChallenges = new List<HobbyChallenge>();

            public bool isCompleted(Sim sim, List<SkillNames> requiredSkills)
            {
                int amountMasteredSkills = 0;
                if (requiredSkills.Count != 0)
                {
                    foreach (SkillNames skill in requiredSkills)
                    {
                        int skillLevel = sim.SkillManager.GetSkillLevel(skill);

                        if (skillLevel == 10)
                        {
                            amountMasteredSkills++;
                        }
                    }
                    if (requiredSkills.Count == amountMasteredSkills)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public class HobbyChallenge
        {
            public string mName = "";
            public string mDescription = "";
            public Interest mLinkedInterestInstance = null;
            public bool mIsCompleted = false;
        }
    }
}
