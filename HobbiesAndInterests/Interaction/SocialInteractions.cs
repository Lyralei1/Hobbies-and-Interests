using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Socializing;
using Sims3.SimIFace;
using Sims3.UI;
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

        public static void DoSkillingAfterwards(Sim actor, Sim target, bool doSkillingForTarget)
        {
            // Check for actor first.
            bool canAdd  = InterestManager.CanAddSubPointsForSocial(actor.SimDescription, InterestTypes.Environment);
            int loveHate = InterestManager.DoesSimHatesLovesInterest(actor.SimDescription, InterestTypes.Environment);

            if (canAdd && loveHate == 0 || loveHate == 1)
            {
                InterestManager.AddSubPoints(0.3f, actor.SimDescription, InterestTypes.Environment);
            }

            if (doSkillingForTarget)
            {
                // Check for Target now.
                bool canAddTarget  = InterestManager.CanAddSubPointsForSocial(target.SimDescription, InterestTypes.Environment);
                int loveHateTarget = InterestManager.DoesSimHatesLovesInterest(target.SimDescription, InterestTypes.Environment);

                if (canAddTarget && loveHateTarget == 0 || loveHateTarget == 1)
                {
                    InterestManager.AddSubPoints(0.3f, actor.SimDescription, InterestTypes.Environment);
                }
            }
        }

        public static void CallbackGeneralInterestSharing(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {
            if (InterestManager.mSavedSimInterests.ContainsKey(actor.SimDescription.SimDescriptionId))
            {
                HandleInterestDiscovered(actor, target, RandomUtil.GetRandomObjectFromList(InterestManager.mSavedSimInterests[actor.SimDescription.SimDescriptionId]).Guid);
            }
        }


        //Primarily focuses on overriding existing socials.
        public static void CallbackTalkAboutGoingGreen(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {
            DoSkillingAfterwards(actor, target, true);
            HandleInterestDiscovered(actor, target, InterestTypes.Environment);
        }

        public static void CallbackTalkingAboutEnvironment(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {            
            if (InterestManager.CanAddSubPointsForSocial(actor.SimDescription, InterestTypes.Environment))
            {
                DoSkillingAfterwards(actor, target, true);
            }
            HandleInterestDiscovered(actor, target, InterestTypes.Environment);
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

        public static void HandleInterestDiscovered(Sim actor, Sim target, InterestTypes type)
        {
            string Interactionname = "";

            int loveHate = InterestManager.DoesSimHatesLovesInterest(target.SimDescription, InterestTypes.Environment);

            // If target hates Environment...
            if (InterestManager.DoesSimHatesLovesInterest(target.mSimDescription, type) == 2)
            {
                LongTermRelationship.UpdateLiking(actor.SimDescription, target.SimDescription, InterestManager.mSubstractLikingWhenHatingInterest);
                //relationship.LTR.UpdateLiking(5f, base.Actor, base.Target);
                InterestManager.LearnInterest(actor.mSimDescription, target.mSimDescription, type, true);
                Interactionname = "Trait Incompatibility";
            }
            // Else if Target loves environment
            else if (InterestManager.DoesSimHatesLovesInterest(target.mSimDescription, type) == 1)
            {
                LongTermRelationship.UpdateLiking(actor.SimDescription, target.SimDescription, InterestManager.mAddLikingWhenLovingInterest);
                InterestManager.LearnInterest(actor.mSimDescription, target.mSimDescription, type, false);

                if (InterestManager.CanAddSubPointsForSocial(actor.SimDescription, type))
                {
                    InterestManager.AddSubPoints(0.2f, actor.SimDescription, type);
                }
                if (InterestManager.CanAddSubPointsForSocial(target.SimDescription, type))
                {
                    InterestManager.AddSubPoints(0.2f, target.SimDescription, type);
                }
                Interactionname = "Trait Bonding";
            }

            if (Interactionname != "")
            {
                actor.InteractionQueue.PushAsContinuation(new SocialInteractionA.Definition(Interactionname, null, null, false).CreateInstance(target, actor, new InteractionPriority(InteractionPriorityLevel.High), true, false), true);
                Sim.ForceSocial(actor, target, Interactionname, InteractionPriorityLevel.High, false);

                if (actor.IsActiveSim)
                {
                    if (Interactionname == "Trait Bonding")
                    {
                        // gets here....
                        actor.ShowTNSIfSelectable("I just learnt that " + target.FirstName + " absolutely LOVES the " + type.ToString() + " interest! We should hang out more and share more on our mutual interest!", StyledNotification.NotificationStyle.kSimTalking);
                    }
                    else
                    {
                        actor.ShowTNSIfSelectable("I just learnt that " + target.FirstName + " absolutely HATES the " + type.ToString() + " interest! Probably for the best if I stop talking about it...", StyledNotification.NotificationStyle.kSimTalking);
                    }
                }
            }
        }
    }
}
