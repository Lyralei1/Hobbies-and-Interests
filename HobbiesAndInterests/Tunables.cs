using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestsAndHobbies
{
    public class Tunables
    {
        /* 
         * ===================================================
         * ||                                               ||
         * ||            Townie Helper Tunables             ||
         * ||                                               ||
         * ===================================================
         */

        [Tunable]
        public static int[] MinMaxForTowniesInterestPenalty = new int[] { -10, -2 };

        [Tunable]
        public static int[] MinMaxForTowniesInterestPassionate = new int[] { 5, 10 };

        [Tunable]
        public static bool kCanAcceptDynamicallyAssignSkillsToTownies = true;

        [Tunable]
        public static bool kMayTowniesPerformHobbiesAutonomously = true;

        /* 
         * ===================================================
         * ||                                               ||
         * ||            Interactions Tunables              ||
         * ||                                               ||
         * ===================================================
         */

        [Tunable]
        public static float kAmountOfPagesASimReadsResearch = 30f;

        /* 
         * =======================================================
         * ||                                                   ||
         * ||            Energy Manager Tunables                ||
         * ||                                                   ||
         * =======================================================
         */


        [Tunable]
        public static int mChanceOfEnergyCrisis = 10;
        [Tunable]
        public static int mAmountIncreaseAtCrisisOffPeak = 10;
        [Tunable]
        public static int mAmountIncreaseAtCrisisPeak = 20;
        [Tunable]
        public static float mStartPeakHour = 16f;
        [Tunable]
        public static float mEndPeakHour = 3f;
        [Tunable]
        public static float mStartOffPeakHour = 4f;
        [Tunable]
        public static float mEndOffPeakHour = 15f;
        [Tunable]
        public static float mSubPointsToAddIfEcoFriendly = 1f;




    }
}
