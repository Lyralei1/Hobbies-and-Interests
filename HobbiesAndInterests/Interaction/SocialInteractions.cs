using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Socializing;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lyralei.InterestMod
{
    public class SocialTestsInterest
    {
        public static bool TestCanDebateEnvironmentThings(Sim actor, Sim target, ActiveTopic topic, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        {
            int loveHate = InterestManager.DoesSimHatesLovesInterest(actor.SimDescription, InterestTypes.Environment);

            if (loveHate == 2 || loveHate == 3)
            {
                return false;
            }
            return true;
        }

        public static bool TestShouldResultIntoBored(Sim actor, Sim target, ActiveTopic topic, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        {
            int loveHate = InterestManager.DoesSimHatesLovesInterest(target.SimDescription, InterestTypes.Environment);

            if (loveHate == 2 || loveHate == 3)
            {
                return true;
            }
            return false;
        }
    }


    public class InterestSocialCallback
    {
        public static Dictionary<string, string> mOverridableInteraction = new Dictionary<string, string>()
        {
            { "Talk About Going Green",  "CallbackTalkAboutGoingGreen" },
        };

        public static void DoSkillingAfterwards(Sim actor, Sim target, bool doSkillingForTarget)
        {
            // Check for actor first.
            bool canAdd = InterestManager.CanAddSubPointsForSocial(actor.SimDescription, InterestTypes.Environment);
            int loveHate = InterestManager.DoesSimHatesLovesInterest(actor.SimDescription, InterestTypes.Environment);

            if (canAdd && loveHate == 0 || loveHate == 1)
            {
                InterestManager.AddSubPoints(0.3f, actor.SimDescription, InterestTypes.Environment);
            }

            if (doSkillingForTarget)
            {
                // Check for Target now.
                bool canAddTarget = InterestManager.CanAddSubPointsForSocial(target.SimDescription, InterestTypes.Environment);
                int loveHateTarget = InterestManager.DoesSimHatesLovesInterest(target.SimDescription, InterestTypes.Environment);

                if (canAddTarget && loveHateTarget == 0 || loveHateTarget == 1)
                {
                    InterestManager.AddSubPoints(0.3f, actor.SimDescription, InterestTypes.Environment);
                }
            }
        }

        //Primarily focuses on overriding existing socials.
        public static void CallbackTalkAboutGoingGreen(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {
            DoSkillingAfterwards(actor, target, true);
        }

        public static void CallbackDebateClimateChange(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {
            DoSkillingAfterwards(actor, target, true);

            bool isEasilyImpressed = target.HasTrait(Sims3.Gameplay.ActorSystems.TraitNames.EasilyImpressed);
            int loveHate = InterestManager.DoesSimHatesLovesInterest(target.SimDescription, InterestTypes.Environment);

            if (isEasilyImpressed && RandomUtil.RandomChance(40f) && loveHate == 0)
            {
                Interest interest = InterestManager.GetInterestFromInterestType(InterestTypes.Environment, target);
                interest.modifyInterestLevel(3, target.SimDescription.SimDescriptionId, InterestTypes.Environment);
            }
            if (RandomUtil.RandomChance(15f) && loveHate == 0)
            {
                Interest interest = InterestManager.GetInterestFromInterestType(InterestTypes.Environment, target);
                interest.modifyInterestLevel(3, target.SimDescription.SimDescriptionId, InterestTypes.Environment);
            }
        }

    }
}
