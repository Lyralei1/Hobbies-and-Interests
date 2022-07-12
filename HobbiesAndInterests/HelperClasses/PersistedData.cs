using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Objects.Lyralei;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lyralei.InterestMod
{
    [Persistable]
    public class PersistedDataInterests
    {

        [Persistable] // string = version+savename. String 2 = Binary data in string format. (See InterestSaveManager.cs)
        public Dictionary<string, string> mInterestSaveData;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazinePolitics;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineCrime;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineFood;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineSports;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineMoney;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineEntertainment;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineHealth;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineParanormal;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineToys;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineEnvironment;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineCulture;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineFashion;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineAnimals;

        [Persistable]
        public Dictionary<string, BookMagazineInterestsData> mMagazineScifi;



        public PersistedDataInterests()
        {

            if (mInterestSaveData == null)
            {
                mInterestSaveData = new Dictionary<string, string>();
            }

            if (mMagazinePolitics == null)
            {
                mMagazinePolitics = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineCrime == null)
            {
                mMagazineCrime = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineFood == null)
            {
                mMagazineFood = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineSports == null)
            {
                mMagazineSports = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineMoney == null)
            {
                mMagazineMoney = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineEntertainment == null)
            {
                mMagazineEntertainment = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineHealth == null)
            {
                mMagazineHealth = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineParanormal == null)
            {
                mMagazineParanormal = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineToys == null)
            {
                mMagazineToys = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineEnvironment == null)
            {
                mMagazineEnvironment = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineCulture == null)
            {
                mMagazineCulture = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineFashion == null)
            {
                mMagazineFashion = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineAnimals == null)
            {
                mMagazineAnimals = new Dictionary<string, BookMagazineInterestsData>();
            }
            if (mMagazineScifi == null)
            {
                mMagazineScifi = new Dictionary<string, BookMagazineInterestsData>();
            }

        }

        public void Cleanup()
        {
            mInterestSaveData = null;
            mMagazinePolitics = null;
            mMagazineCrime = null;
            mMagazineFood = null;
            mMagazineSports = null;
            mMagazineMoney = null;
            mMagazineEntertainment = null;
            mMagazineHealth = null;
            mMagazineParanormal = null;
            mMagazineToys = null;
            mMagazineEnvironment = null;
            mMagazineCulture = null;
            mMagazineFashion = null;
            mMagazineAnimals = null;
            mMagazineScifi = null;

        }
    }
}
