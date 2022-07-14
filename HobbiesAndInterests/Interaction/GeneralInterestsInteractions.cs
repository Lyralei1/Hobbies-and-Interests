using Lyralei;
using Lyralei.UI;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Lyralei.InterestsAndHobbies;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.ThoughtBalloons;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.Enums;
using Sims3.UI;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lyralei.InterestMod
{
    public class ResearchInterest : Computer.ComputerInteraction
    {
        public class Definition : InteractionDefinition<Sim, Computer, ResearchInterest>
        {
            public Interest CurrInterestType = null;

            public override string[] GetPath(bool isFemale)
            {
                return new string[2]
                {
                    "Interests & Hobbies...",
                    "Research..."
                };
            }

            public Definition()
            {
            }

            public Definition(Interest currentInterest)
            {
                CurrInterestType = currentInterest;
            }

            public override string GetInteractionName(Sim actor, Computer target, InteractionObjectPair iop)
            {
                if (CurrInterestType.PercentageResearchDone > 0f)
                {
                    return "(Continue Researching) " + CurrInterestType.Name + " (" + String.Format("{0:0\\%}", CurrInterestType.PercentageResearchDone) + " )";
                }
                return CurrInterestType.Name;
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, Computer target, List<InteractionObjectPair> results)
            {
                List<Interest> interests = InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId];

                for (int i = 0; i < interests.Count; i++)
                {
                    if (interests[i].currInterestPoints == 10)
                    {
                        if (!target.IsActorUsingMe(actor) && interests[i] != null)
                        {
                            // Loop through all interests that are at 10, so sims can start reseaching them to start with.
                            results.Add(new InteractionObjectPair(new Definition(interests[i]), iop.Target));
                        }
                    }
                }
            }

            public override bool Test(Sim a, Computer target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (target.InUse || target.IsActorUsingMe(a))
                {
                    return false;
                }
                return true;
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public Interest interestChosen = null;

        public override bool Run()
        {
            Definition definition = base.InteractionDefinition as Definition;

            interestChosen = definition.CurrInterestType;

            base.StandardEntry(true);

            if (!base.Target.StartComputing(this, Sims3.SimIFace.Enums.SurfaceHeight.Table, true))
            {
                GlobalOptionsHobbiesAndInterests.print("False");

                base.StandardExit();
                return false;
            }

            base.Target.StartVideo(Computer.VideoType.Browse);
            base.BeginCommodityUpdates();

            if (base.Actor.TraitManager.HasElement(TraitNames.NightOwlTrait) && base.Actor.BuffManager.HasElement(BuffNames.PastBedTime))
            {
                base.AlterMotiveMultiplier(CommodityKind.Fun, TraitTuning.NightOwlFunModifier);
            }
            if (interestChosen == null)
            {
                base.AnimateSim("WorkTyping");
                base.EndCommodityUpdates(false);
                base.Target.StopComputing(this, Computer.StopComputingAction.TurnOff, false);
                base.StandardExit();
                return false;
            }
            base.AnimateSim("WorkTyping");

            ProgressMeter.ShowProgressMeter(base.Actor, 0f, ProgressMeter.GlowType.Weak);

            bool flag = DoLoop(ExitReason.Default, LoopDel, null);
            ProgressMeter.HideProgressMeter(base.Actor, flag);
            if (interestChosen.PercentageResearchDone >= 100f)
            {
                definition.CurrInterestType.currInterestPoints++;
                if (base.Actor.IsSelectable)
                {
                    Audio.StartObjectSound(base.Target.ObjectId, "sting_repair_success", false);
                }
            }
            else
            {
                // Copy back to the actual interest.
                definition.CurrInterestType.PercentageResearchDone = interestChosen.PercentageResearchDone;
            }

            base.EndCommodityUpdates(flag);
            base.Target.StopComputing(this, Computer.StopComputingAction.TurnOff, false);
            base.StandardExit(true);

            return flag;
        }

        public void LoopDel(StateMachineClient smc, LoopData loopData)
        {
            if (RandomUtil.RandomChance(5f))
            {
                ShowThoughtBalloonForInterest();
            }
            if (UpdateResearchProgress(loopData.mDeltaTime))
            {
                base.Actor.AddExitReason(ExitReason.Finished);
            }
        }

        public void ShowThoughtBalloonForInterest()
        {

            if (base.Actor.ThoughtBalloonManager == null)
            {
                return;
            }

            string randomStringFromList = "";
            switch (interestChosen.mInterestsGuid)
            {
                case InterestTypes.Animals:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestAnimalsBalloons);
                    break;
                case InterestTypes.Crime:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestCrimeBalloons);
                    break;
                case InterestTypes.Culture:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestCultureBalloons);
                    break;
                case InterestTypes.Entertainment:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestEntertainmentBalloons);
                    break;
                case InterestTypes.Environment:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestEnvironmentBalloons);
                    break;
                case InterestTypes.Fashion:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestFashionBalloons);
                    break;
                case InterestTypes.Food:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestFoodBalloons);
                    break;
                case InterestTypes.Health:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestHealthBalloons);
                    break;
                case InterestTypes.Money:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestMoneyBalloons);
                    break;
                case InterestTypes.Paranormal:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestParanormalBalloons);
                    break;
                case InterestTypes.Politics:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestPoliticsBalloons);
                    break;
                case InterestTypes.Scifi:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestScifiBalloons);
                    break;
                case InterestTypes.Sports:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestSportsBalloons);
                    break;
                case InterestTypes.Toys:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestToysBalloons);
                    break;
                default:
                    return;
            }

            if (!base.Actor.ThoughtBalloonManager.HasBallon)
            {
                ThoughtBalloonManager.BalloonData balloondata = new ThoughtBalloonManager.BalloonData(randomStringFromList)
                {
                    BalloonType = ThoughtBalloonTypes.kThoughtBalloon,
                    Duration = ThoughtBalloonDuration.Medium
                };
                if (balloondata == null)
                {
                    return;
                }

                base.Actor.ThoughtBalloonManager.ShowBalloon(balloondata);
            }
        }

        public bool UpdateResearchProgress(float deltaTime)
        {
            if (interestChosen == null)
            {
                return false;
            }
            float researchRate = GetFinishedResearchRate();
            ProgressMeter.GlowType glowType = ProgressMeter.GlowType.Strong;
            if (researchRate < 1f)
            {
                glowType = ProgressMeter.GlowType.None;
            }
            else if (researchRate < 2f)
            {
                glowType = ProgressMeter.GlowType.Weak;
            }
            researchRate *= deltaTime;
            interestChosen.PercentageResearchDone += researchRate * 100f / Tunables.kAmountOfPagesASimReadsResearch; // Amount of pages the sim will read (30f pages currently)
            if (interestChosen.PercentageResearchDone > 100f)
            {
                interestChosen.PercentageResearchDone = 100f;
            }
            ProgressMeter.UpdateProgressMeter(base.Actor, interestChosen.PercentageResearchDone * 0.01f, glowType);
            return interestChosen.PercentageResearchDone == 100f; // if percentage is 100, then consider it done!
        }

        public float GetFinishedResearchRate()
        {
            float num = 0.05f; // basevalue
            if (base.Actor.TraitManager.HasElement(TraitNames.BookWorm))
            {
                num += 0.05f; // If they're a bookwork, add this.
            }
            // Get the amount of books the sim has read, as this will speed up their reading through research
            float num2 = (float)base.Actor.ReadBookDataList.Count;
            if (num2 >= 50f)
            {
                num2 = 50f;
            }
            num += num2 / 50f * 0.1f;
            if (base.Actor.TraitManager.HasElement(TraitNames.Neurotic))
            {
                num *= 0.08f; // Neurotic sims will read quicker since they 
            }
            return num;
        }
    }

    public class ResearchInterestPhone : Interaction<Sim, PhoneSmart>
    {
        public class Definition : InteractionDefinition<Sim, PhoneSmart, ResearchInterestPhone>
        {
            public Interest CurrInterestType = null;

            public override string[] GetPath(bool isFemale)
            {
                return new string[1]
                {
                    "Interests & Hobbies..."
                };
            }

            public Definition()
            {
            }

            public Definition(Interest currentInterest)
            {
                CurrInterestType = currentInterest;
            }

            public override string GetInteractionName(Sim actor, PhoneSmart target, InteractionObjectPair iop)
            {
                if (CurrInterestType.PercentageResearchDone > 0f)
                {
                    return "Continue Researching " + CurrInterestType.Name + " (" + String.Format("{0:0\\%}", CurrInterestType.PercentageResearchDone) + " )";
                }
                return "Research " + CurrInterestType.Name;
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, PhoneSmart target, List<InteractionObjectPair> results)
            {
                List<Interest> interests = InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId];

                for (int i = 0; i < interests.Count; i++)
                {
                    if (interests[i].currInterestPoints == 10)
                    {
                        if (!target.IsActorUsingMe(actor) && interests[i] != null)
                        {
                            // Loop through all interests that are at 10, so sims can start reseaching them to start with.
                            results.Add(new InteractionObjectPair(new Definition(interests[i]), iop.Target));
                        }
                    }
                }
            }

            public override bool Test(Sim a, PhoneSmart target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!target.IsUsableBy(a))
                {
                    return false;
                }
                if (a.Inventory.Find<IPhoneSmart>() == null)
                {
                    return false;
                }
                if (!target.IsUsableBy(a) && a.Inventory.Find<PhoneSmart>() == null)
                {
                    return false;
                }
                return true;
            }
        }

        public override ThumbnailKey GetIconKey()
        {
            ThumbnailKey kInvalidThumbnailKey = ThumbnailKey.kInvalidThumbnailKey;
            string name = "w_smart_phone_browse_web";
            ProductVersion version = (ProductVersion)131072u;
            IPhoneFuture phoneFuture = base.Actor.Inventory.Find<IPhoneFuture>();
            if (GameUtils.IsInstalled((ProductVersion)1048576u) && phoneFuture != null)
            {
                name = "w_future_phone_browse_web";
                version = (ProductVersion)1048576u;
            }
            return new ThumbnailKey(ResourceKey.CreatePNGKey(name, ResourceUtils.ProductVersionToGroupId(version)), ThumbnailSize.Large);
        }

        public static InteractionDefinition Singleton = new Definition();

        public override bool RunFromInventory()
        {
            return Run();
        }

        [TunableComment("Chance to drop the smart phone")]
        [Tunable]
        public static float kChanceToDrop = 0.1f;

        public Jig mJig;

        public Interest interestChosen = null;

        public override bool Run()
        {
            Definition definition = base.InteractionDefinition as Definition;
            interestChosen = definition.CurrInterestType;

            IGameObject gameObject = null;
            base.StandardEntry();
            SocialNetworkingSkill socialNetworkingSkill = base.Actor.SkillManager.GetElement(SkillNames.SocialNetworking) as SocialNetworkingSkill;

            mJig = (GlobalFunctions.CreateObject("SocialJigOnePerson", Vector3.OutOfWorld, 0, Vector3.OutOfWorld) as SocialJigOnePerson);
            mJig.SetOpacity(0f, 0f);
            Vector3 position = base.Actor.Position;
            Vector3 forwardVector = base.Actor.ForwardVector;
            mJig.SetPosition(position);
            mJig.SetForward(forwardVector);
            mJig.AddToWorld();

            base.Target.StartUsingSmartPhone(base.Actor, ref gameObject, this);
            base.BeginCommodityUpdates();

            if (base.Actor.TraitManager.HasElement(TraitNames.NightOwlTrait) && base.Actor.BuffManager.HasElement(BuffNames.PastBedTime))
            {
                base.AlterMotiveMultiplier(CommodityKind.Fun, TraitTuning.NightOwlFunModifier);
            }
            base.AnimateSim("BrowseTheWeb");

            base.Actor.SkillManager.AddElement(SkillNames.SocialNetworking);
            if (base.Actor.IsSelectable)
            {
                Tutorialette.TriggerLesson(Lessons.SocialNetworking, base.Actor);
            }
            base.BeginCommodityUpdates();
            ProgressMeter.ShowProgressMeter(base.Actor, 0f, ProgressMeter.GlowType.Weak);

            bool flag = DoLoop(ExitReason.Default, LoopDel, base.mCurrentStateMachine);

            ProgressMeter.HideProgressMeter(base.Actor, flag);
            if (interestChosen.PercentageResearchDone >= 100f)
            {
                definition.CurrInterestType.currInterestPoints++;
            }
            else
            {
                // Copy back to the actual interest.
                definition.CurrInterestType.PercentageResearchDone = interestChosen.PercentageResearchDone;
            }


            base.EndCommodityUpdates(flag);
            if (RandomUtil.RandomChance01(kChanceToDrop))
            {
                base.AnimateSim("PhoneDropped");
                base.Target.IsBroken = true;
            }
            else
            {
                base.AnimateSim("NormalExit");
            }
            base.Actor.BuffManager.AddElement(BuffNames.PhoneJunkie, Origin.FromUsingSmartPhone);
            base.Target.RemoveHandObject();
            if (mJig != null)
            {
                mJig.Destroy();
                mJig = null;
            }
            base.StandardExit();
            return true;
        }

        public void LoopDel(StateMachineClient smc, LoopData loopData)
        {
            if (RandomUtil.RandomChance(5f))
            {
                ShowThoughtBalloonForInterest();
            }
            if (UpdateResearchProgress(loopData.mDeltaTime))
            {
                base.Actor.AddExitReason(ExitReason.Finished);
            }
        }

        public override void Cleanup()
        {
            if (mJig != null)
            {
                mJig.Destroy();
                mJig = null;
            }
            base.Cleanup();
        }

        public void ShowThoughtBalloonForInterest()
        {
            if (base.Actor.ThoughtBalloonManager == null)
            {
                return;
            }

            string randomStringFromList = "";
            switch (interestChosen.mInterestsGuid)
            {
                case InterestTypes.Animals:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestAnimalsBalloons);
                    break;
                case InterestTypes.Crime:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestCrimeBalloons);
                    break;
                case InterestTypes.Culture:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestCultureBalloons);
                    break;
                case InterestTypes.Entertainment:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestEntertainmentBalloons);
                    break;
                case InterestTypes.Environment:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestEnvironmentBalloons);
                    break;
                case InterestTypes.Fashion:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestFashionBalloons);
                    break;
                case InterestTypes.Food:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestFoodBalloons);
                    break;
                case InterestTypes.Health:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestHealthBalloons);
                    break;
                case InterestTypes.Money:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestMoneyBalloons);
                    break;
                case InterestTypes.Paranormal:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestParanormalBalloons);
                    break;
                case InterestTypes.Politics:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestPoliticsBalloons);
                    break;
                case InterestTypes.Scifi:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestScifiBalloons);
                    break;
                case InterestTypes.Sports:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestSportsBalloons);
                    break;
                case InterestTypes.Toys:
                    randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestToysBalloons);
                    break;
                default:
                    return;
            }

            if (!base.Actor.ThoughtBalloonManager.HasBallon)
            {
                ThoughtBalloonManager.BalloonData balloondata = new ThoughtBalloonManager.BalloonData(randomStringFromList)
                {
                    BalloonType = ThoughtBalloonTypes.kThoughtBalloon,
                    Duration = ThoughtBalloonDuration.Medium
                };
                if (balloondata == null)
                {
                    return;
                }

                base.Actor.ThoughtBalloonManager.ShowBalloon(balloondata);
            }
        }

        public bool UpdateResearchProgress(float deltaTime)
        {
            if (interestChosen == null)
            {
                return false;
            }
            float researchRate = GetFinishedResearchRate();
            ProgressMeter.GlowType glowType = ProgressMeter.GlowType.Strong;
            if (researchRate < 1f)
            {
                glowType = ProgressMeter.GlowType.None;
            }
            else if (researchRate < 2f)
            {
                glowType = ProgressMeter.GlowType.Weak;
            }
            researchRate *= deltaTime;
            interestChosen.PercentageResearchDone += researchRate * 100f / (float)30f; //Turn into a tunable! Amount of pages the sim will read (30f pages currently)
            if (interestChosen.PercentageResearchDone >= 100f)
            {
                interestChosen.currInterestPoints++;
            }
            ProgressMeter.UpdateProgressMeter(base.Actor, interestChosen.PercentageResearchDone * 0.01f, glowType);
            return interestChosen.PercentageResearchDone == 100f;
        }

        public float GetFinishedResearchRate()
        {
            float num = 0.05f;// basevalue
            if (base.Actor.TraitManager.HasElement(TraitNames.BookWorm))
            {
                num += 0.05f; // If they're a bookwork, add this.
            }
            // Get the amount of books the sim has read, as this will speed up their reading through research
            float num2 = (float)base.Actor.ReadBookDataList.Count;
            if (num2 >= 50f)
            {
                num2 = 50f;
            }
            num += num2 / 50f * 0.1f;
            if (base.Actor.TraitManager.HasElement(TraitNames.Neurotic))
            {
                num *= 0.08f; // Neurotic sims will read quicker since they 
            }
            return num;
        }
    }

    public class SubscribeToInterestMagazine : Computer.ComputerInteraction
    {
        public class Definition : InteractionDefinition<Sim, Computer, SubscribeToInterestMagazine>
        {
            public Interest CurrInterestType = null;

            public Definition()
            {
            }

            public Definition(Interest currentInterest)
            {
                CurrInterestType = currentInterest;
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, Computer target, List<InteractionObjectPair> results)
            {
                List<Interest> interests = InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId];

                for (int i = 0; i < interests.Count; i++)
                {
                    if (interests[i].currInterestPoints >= 11)
                    {
                        if (!target.IsActorUsingMe(actor) && interests[i] != null)
                        {
                            // Loop through all interests that are at 10, so sims can start reseaching them to start with.
                            results.Add(new InteractionObjectPair(new Definition(interests[i]), iop.Target));
                        }
                    }
                }
            }

            public override string[] GetPath(bool isFemale)
            {
                return new string[2]
                {
                    "Interests & Hobbies...",
                    "Purchase Interest Magazine Subscription..."
                };
            }

            public override string GetInteractionName(Sim actor, Computer target, InteractionObjectPair iop)
            {
                return CurrInterestType.Name + " $20";
            }

            public override bool Test(Sim a, Computer target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                // && !CurrInterestType.mHasMagazineSubscription

                if (target.IsComputerUsable(a, true, false, isAutonomous) && a.FamilyFunds >= kCost)
                {
                    if (CurrInterestType.mHasMagazineSubscription)
                    {
                        greyedOutTooltipCallback = () => "Your sim already has a subscription for this particular interest!";
                        return false;
                    }
                    return true;
                }
                else
                {
                    greyedOutTooltipCallback = () => "The household either lacks the money to pay for the $20 subscription fee, or the computer is unusable!";
                }
                return false;
            }
        }

        public static InteractionDefinition Singleton = new Definition();

        [TunableComment("Range:  Simoleons.  Description:  Lifetime membership cost of joining the pattern club.")]
        [Tunable]
        public static int kCost = 20;

        public override bool Run()
        {
            Definition definition = base.InteractionDefinition as Definition;
            base.StandardEntry();
            if (!base.Target.StartComputing(this, SurfaceHeight.Table, true))
            {
                base.StandardExit();
                return false;
            }
            //mActor = Actor;
            base.Target.StartVideo(Computer.VideoType.Browse);
            base.BeginCommodityUpdates();
            base.AnimateSim("WorkTyping");
            bool flag = TwoButtonDialog.Show("Would you like to subscribe to our weekly magazine about " + definition.CurrInterestType.mInterestsGuid.ToString() + " , we'll charge you $20 per week and you'll get the magazine every Tuesday!", "Sign Me Up", "I Don't Want A Magazine");

            if (flag)
            {
                Mailbox mailbox = Mailbox.GetMailboxOnHomeLot(base.Actor);
                if (mailbox != null)
                {
                    //mailbox.AddAlarmDay(1f, DaysOfTheWeek.Tuesday, () => { InterestManager.SendTheMagazines(definition.CurrInterestType, base.Actor.SimDescription.SimDescriptionId); }, "Mailbox:_Interest_Magazine_" + base.Actor.mSimDescription.mSimDescriptionId.ToString() + "_" + definition.CurrInterestType.ToString(), AlarmType.AlwaysPersisted);
                    InterestManager.mMagazineSubscriptionAlarms.Add(definition.CurrInterestType.mInterestsGuid.ToString() + "_" + base.Actor.SimDescription.SimDescriptionId.ToString() , mailbox.AddAlarmDay(1f, DaysOfTheWeek.All, () => { InterestManager.SendTheMagazines(definition.CurrInterestType, base.Actor.SimDescription.SimDescriptionId); }, "Mailbox:_Interest_Magazine_" + base.Actor.mSimDescription.mSimDescriptionId.ToString() + "_" + definition.CurrInterestType.ToString(), AlarmType.AlwaysPersisted));

                    List<Interest> interests = InterestManager.mSavedSimInterests[base.Actor.mSimDescription.SimDescriptionId];
                    for (int i = 0; i < interests.Count; i++)
                    {
                        if(!interests[i].mHasMagazineSubscription && definition.CurrInterestType.mInterestsGuid == interests[i].mInterestsGuid)
                        {
                            interests[i].mHasMagazineSubscription = true;
                        }
                    }
                    StyledNotification.Format error = new StyledNotification.Format("'Thanks for subscribing with us! If you ever get into a different hobby, you may always have multiple subscriptions with us! Keep in mind that this will add another 20 simoleans onto your subscription bill. If you ever can't pay it directly, we'll put the money onto your general bill. You can always cancel the subsciption for certain (or all) subscription if you ever need to!'", base.Actor.ObjectId, base.Target.ObjectId, StyledNotification.NotificationStyle.kGameMessagePositive);
                    StyledNotification.Show(error);
                }
                else
                {
                    StyledNotification.Format error = new StyledNotification.Format("ERROR: Your sim is, according to the game, homeless! Maybe resetting them might help?", base.Actor.ObjectId, base.Target.ObjectId, StyledNotification.NotificationStyle.kGameMessageNegative);
                    StyledNotification.Show(error);
                }
            }
            base.Target.StopComputing(this, Computer.StopComputingAction.TurnOff, false);
            base.EndCommodityUpdates(flag);
            base.StandardExit();
            return true;
        }
    }

    public class UnSubscribeToInterestMagazine : Computer.ComputerInteraction
    {
        public class Definition : InteractionDefinition<Sim, Computer, UnSubscribeToInterestMagazine>
        {
            public Interest CurrInterestType = null;

            public Definition()
            {
            }

            public Definition(Interest currentInterest)
            {
                CurrInterestType = currentInterest;
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, Computer target, List<InteractionObjectPair> results)
            {
                List<Interest> interests = InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId];

                for (int i = 0; i < interests.Count; i++)
                {
                    if (interests[i] != null && !target.IsActorUsingMe(actor))
                    {
                        // Loop through all interests that are at 10, so sims can start reseaching them to start with.
                        results.Add(new InteractionObjectPair(new Definition(interests[i]), iop.Target));
                    }
                }
            }

            public override string[] GetPath(bool isFemale)
            {
                return new string[2]
                {
                    "Interests & Hobbies...",
                    "Unsubscribe Interest Magazine Subscription..."
                };
            }

            public override string GetInteractionName(Sim actor, Computer target, InteractionObjectPair iop)
            {
                return CurrInterestType.Name + " $20";
            }

            public override bool Test(Sim a, Computer target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                // && !CurrInterestType.mHasMagazineSubscription

                if (target.IsComputerUsable(a, true, false, isAutonomous))
                {
                    if (CurrInterestType.mHasMagazineSubscription)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }

        public static InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            Definition definition = base.InteractionDefinition as Definition;
            base.StandardEntry();
            if (!base.Target.StartComputing(this, SurfaceHeight.Table, true))
            {
                base.StandardExit();
                return false;
            }
            //mActor = Actor;
            base.Target.StartVideo(Computer.VideoType.Browse);
            base.BeginCommodityUpdates();
            base.AnimateSim("WorkTyping");
            bool flag = TwoButtonDialog.Show("Are you sure you'd like to unsubscribe the " + definition.CurrInterestType.mInterestsGuid.ToString() + " interest magazine? We'll make sure to not charge you $20 anymore for this magazine and will stop sending it. If you happen to have multiple subscriptions though and want to cancel those as well, you'll need to do that inidividually.", "Cancel " + definition.CurrInterestType.ToString() + " Magazine", "I don't want to cancel!");

            if (flag)
            {
                Mailbox mailbox = Mailbox.GetMailboxOnHomeLot(base.Actor);
                if (mailbox != null)
                {
                    Dictionary<string, AlarmHandle> toRemoveFromDictionary = new Dictionary<string, AlarmHandle>();

                    if(InterestManager.mMagazineSubscriptionAlarms.ContainsKey(definition.CurrInterestType.mInterestsGuid.ToString() + "_" + base.Actor.SimDescription.SimDescriptionId.ToString()))
                    {
                        mailbox.RemoveAlarm(InterestManager.mMagazineSubscriptionAlarms[definition.CurrInterestType.mInterestsGuid.ToString() + "_" + base.Actor.SimDescription.SimDescriptionId.ToString()]);
                        InterestManager.mMagazineSubscriptionAlarms[definition.CurrInterestType.mInterestsGuid.ToString() + "_" + base.Actor.SimDescription.SimDescriptionId.ToString()] = AlarmHandle.kInvalidHandle;
                        InterestManager.mMagazineSubscriptionAlarms.Remove(definition.CurrInterestType.mInterestsGuid.ToString() + "_" + base.Actor.SimDescription.SimDescriptionId.ToString());
                    }

                    // Set subscription back to false since we unsubscribed
                    List<Interest> interests = InterestManager.mSavedSimInterests[base.Actor.mSimDescription.SimDescriptionId];
                    for (int i = 0; i < interests.Count; i++)
                    {
                        if (interests[i].mHasMagazineSubscription && definition.CurrInterestType.mInterestsGuid == interests[i].mInterestsGuid)
                        {
                            interests[i].mHasMagazineSubscription = false;
                        }
                    }
                    StyledNotification.Format error = new StyledNotification.Format("'We've successfully removed the magazine you canceled off of our delivery system. You will no longer get this magazine nor will you be paying for this magazine anymore, but if you ever feel like re-subscribing then always feel free to do so!'", base.Actor.ObjectId, base.Target.ObjectId, StyledNotification.NotificationStyle.kGameMessagePositive);
                    StyledNotification.Show(error);
                }
                else
                {
                    StyledNotification.Format error = new StyledNotification.Format("ERROR: Your sim is, according to the game, homeless! Maybe resetting them might help?", base.Actor.ObjectId, base.Target.ObjectId, StyledNotification.NotificationStyle.kGameMessageNegative);
                    StyledNotification.Show(error);
                }
            }
            base.Target.StopComputing(this, Computer.StopComputingAction.TurnOff, false);
            base.EndCommodityUpdates(flag);
            base.StandardExit();
            return true;
        }
    }

    public class AssignInterestLot : Interaction<Sim, Lot>
    {
        public class Definition : InteractionDefinition<Sim, Lot, AssignInterestLot>
        {
            public override string[] GetPath(bool isFemale)
            {
                return new string[1]
                {
                    "interests & hobbies..."
                };
            }

            public Definition()
            {
            }

            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                //List<Interest> interests = InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId];
                //if(interests != null || interests.Count != 0)
                //{
                //    for (int i = 0; i < interests.Count; i++)
                //    {
                //        for(int h = 0; h < interests[i].mHobbyLot.Count; h++)
                //        {
                //            if (interests[i].mHobbyLot[h].lotID != 0 && interests[i].mHobbyLot[h].lotID == target.LotId)
                //            {
                //                return "Change Hobby Lot Type or options " + "( curr: " + interests[i].mInterestsGuid.ToString() + ")";
                //            }
                //        }
                //    }
                //}

                if(InterestManager.mHobbyLot != null)
                {
                    foreach(HobbyLot lot in InterestManager.mHobbyLot)
                    {
                        if(lot.lotID == target.LotId)
                        {
                            return "Change Hobby Lot Type or Options " + "(" + lot.typeHobbyLot.ToString() + ")";
                        }
                    }
                }


                return "Add Hobby Type To Lot";
            }

            public override bool Test(Sim a, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public Interest interestChosen = null;

        public override bool Run()
        {
            try
            {
                List<InterestTypes> types = new List<InterestTypes>();
                List<Interest> interests = InterestManager.mSavedSimInterests[base.Actor.mSimDescription.SimDescriptionId];

                for (int i = 0; i < interests.Count; i++)
                {
                    types.Add(interests[i].mInterestsGuid);
                }

                List<ObjectListPickerInfo> typeInfo = new List<ObjectListPickerInfo>();

                foreach (InterestTypes type in types)
                {
                    ObjectListPickerInfo o = new ObjectListPickerInfo(type.ToString(), type);
                    typeInfo.Add(o);
                }
                string typeReturned = ObjectListPickerDialog.Show(typeInfo).ToString();
                InterestTypes typeChosen = ConvertStringToInterestType(typeReturned);

                HobbyLot mHobbylot = null;

                if (typeChosen != InterestTypes.None)
                {
                    if (InterestManager.mHobbyLot != null && InterestManager.mHobbyLot.Count > 0)
                    {
                        foreach (HobbyLot lot in InterestManager.mHobbyLot)
                        {
                            if (lot.typeHobbyLot == typeChosen && lot.lotID == base.Target.LotId)
                            {
                                mHobbylot = lot;
                            }
                        }
                    }
                    if(mHobbylot == null)
                    {
                        mHobbylot = new HobbyLot();

                        mHobbylot.lotID = base.Target.LotId;
                        mHobbylot.lot = base.Target;

                        mHobbylot.typeHobbyLot = typeChosen;
                        mHobbylot.InitLot();
                    }
                    

                    if (mHobbylot != null)
                    {
                        InterestManager.print(mHobbylot.lot.Address.ToString());
                        InterestManager.mHobbyLot.Add(mHobbylot);
                        DisallowAllowInteractionsDialogue(mHobbylot);

                        GlobalOptionsHobbiesAndInterests.print("Successfully assigned this lot as a hobby lot for the interest: " + mHobbylot.typeHobbyLot.ToString() + "!");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                InterestManager.print(ex.ToString());
                return false;
            }
        }

        public static void DisallowAllowInteractionsDialogue(HobbyLot hobbylot)
        {
            List<ObjectPicker.HeaderInfo> header = new List<ObjectPicker.HeaderInfo>();
            List<ObjectPicker.TabInfo> tab = new List<ObjectPicker.TabInfo>();
            List<ObjectPicker.RowInfo> populatedRows = new List<ObjectPicker.RowInfo>();

            int totalInteractions = 0;
            totalInteractions = hobbylot.mAvailableInteractions.Count;

            header.Add(new ObjectPicker.HeaderInfo("Header test", "HeaderTestTooltip", 500));
            header.Add(new ObjectPicker.HeaderInfo("Header test", "HeaderTestTooltip", 100));

            foreach (KeyValuePair<string, InteractionDefinition> interaction in hobbylot.mAvailableInteractions)
            {
                List<ObjectPicker.ColumnInfo> columnItems = new List<ObjectPicker.ColumnInfo>();
                columnItems.Add(new ObjectPicker.TextColumn(interaction.Key));
                columnItems.Add(new ObjectPicker.TextColumn(ReturnWhetherAllowedOrNot(interaction.Key, hobbylot))); // Edit this to be proper
                ObjectPicker.RowInfo item = new ObjectPicker.RowInfo(interaction.Key, columnItems);
                populatedRows.Add(item);
            }

            //tab.Add(tabinfo);
            ObjectPicker.TabInfo tabinfo = new ObjectPicker.TabInfo(string.Empty, "Select Interactions you DON'T want preformed on this lot.", populatedRows);
            tab.Add(tabinfo);

            List<ObjectPicker.RowInfo> rowInfo1 = ObjectPickerDialog.Show(
                true,
                ModalDialog.PauseMode.PauseSimulator,
                "Interaction (Dis)allowance",
                Localization.LocalizeString("Ui/Caption/Global:Ok"),
                Localization.LocalizeString("Ui/Caption/Global:Cancel"),
                tab,
                header,
                totalInteractions
            );

            if (rowInfo1 != null && rowInfo1.Count > 0)
            {
                PopulateDisallowAllowList(hobbylot, rowInfo1);
            }
        }

        public static string ReturnWhetherAllowedOrNot(string key, HobbyLot lot)
        {
            if(lot.mWhitelistedInteractions.ContainsKey(key))
            {
                return "Allowed";
            }
            else if(lot.mBlacklistedInteractions.ContainsKey(key))
            {
                return "Disallowed";
            }
            else
            {
                return "Allowed";
            }
        }

        public static void PopulateDisallowAllowList(HobbyLot lot, List<ObjectPicker.RowInfo> rowInfo1)
        {
            foreach(ObjectPicker.RowInfo row in rowInfo1)
            {
                if (lot.mBlacklistedInteractions.ContainsKey(row.Item.ToString()))
                {
                    lot.mWhitelistedInteractions.Add(row.Item.ToString(), lot.mBlacklistedInteractions[row.Item.ToString()]);
                    lot.mBlacklistedInteractions.Remove(row.Item.ToString());
                    continue;
                }

                if (lot.mWhitelistedInteractions.ContainsKey(row.Item.ToString()))
                {
                    lot.mBlacklistedInteractions.Add(row.Item.ToString(), lot.mWhitelistedInteractions[row.Item.ToString()]);
                    lot.mWhitelistedInteractions.Remove(row.Item.ToString());
                    continue;
                }
            }
        }

        // Find the type that fits the string and return it.
        public InterestTypes ConvertStringToInterestType(string interestName)
        {
            if(string.IsNullOrEmpty(interestName))
            {
                return InterestTypes.None;
            }
            var allInterestTypes = Enum.GetValues(typeof(InterestTypes));

            foreach(InterestTypes type in allInterestTypes)
            {
                if (interestName == type.ToString())
                {
                    return type;
                }
            }
            return InterestTypes.None;
        }

        //public void SetHobbyLotForAllSims(InterestTypes type, HobbyLot hobbylot)
        //{
        //    Sim[] objects = Sims3.Gameplay.Queries.GetObjects<Sim>();

        //    foreach (Sim sim in objects)
        //    {
        //        //print("e.actor: " + e.Actor.SimDescription.SimDescriptionId.ToString());
        //        if (sim != null && InterestManager.mSavedSimInterests.ContainsKey(sim.SimDescription.SimDescriptionId))
        //        {
        //            List<Interest> interests = InterestManager.mSavedSimInterests[sim.mSimDescription.SimDescriptionId];

        //            for(int s = 0; s < interests.Count; s++)
        //            {
        //                if (interests[s].mInterestsGuid == type)
        //                {
        //                    interests[s].mHobbyLot.Add(hobbylot);
        //                }
        //            }
        //        }
        //    }

        //    //foreach (KeyValuePair<ulong, List<Interest>> entry in InterestManager.mSavedSimInterests)
        //    //{
        //    //    for (int i = 0; i < entry.Value.Count; i++)
        //    //    {
        //    //        if(entry.Value[i].mInterestsGuid == type)
        //    //        {
        //    //            entry.Value[i].mHobbyLot.Add(hobbylot);
        //    //        }
        //    //    }
        //    //}
        //}
    }

    public class ShowKnownInterest : ImmediateInteraction<Sim, Sim>
    {
        public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, ShowKnownInterest>
        {
            public override string[] GetPath(bool isFemale)
            {
                return new string[1]
                {
                    "Interests & Hobbies..."
                };
            }
            public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
            {
                return "Show " + target.FirstName + "'s known interests...";
            }
            public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if(InterestManager.mSavedSimInterests.ContainsKey(target.SimDescription.SimDescriptionId))
                {
                    return true;
                }
                return false;
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            List<Interest> interests = InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId];

            InterestsOnSimDialog.Show(InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId], base.Target);

            return true;
        }
    }

    


    public class DebateEnvironment : SocialInteractionA
    {
        public class DefinitionDebateEnvironment : Definition, IHasTraitIcon
        {
            public InterestTypes chosenType = InterestTypes.None;
            public Interest mInterestForIcon = null;

            public DefinitionDebateEnvironment()
                : base("DebateEnvironment", new string[0], null, false)
            {
            }

            public DefinitionDebateEnvironment(InterestTypes chosen, Interest interest)
                : base("DebateEnvironment", new string[0], null, false)
            {
                chosenType = chosen;
                mInterestForIcon = interest;
            }

            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                DebateEnvironment debateEnvironment = new DebateEnvironment();
                debateEnvironment.Init(ref parameters);
                return debateEnvironment;
            }

            public override ResourceKey GetTraitIcon(Sim actor, GameObject target)
            {
                return InterestManager.GetCorrectTraitIcon(actor.mSimDescription, mInterestForIcon);
            }

            public override string[] GetPath(bool isFemale)
            {
                string text = Localization.LocalizeString("Gameplay/Socializing:Special");
                //string text = ""
                return new string[3]
                {
                    text,
                    "Interests & Hobbies Mod...",
                    "Debate About..."
                };
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, Sim target, List<InteractionObjectPair> results)
            {

                if (actor.NeedsToBeGreeted(target))
                {
                    return;
                }
                List<Interest> interests = InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId];
                for (int i = 0; i < interests.Count; i++)
                {
                    if (interests[i] != null)
                    {
                        // Loop through all interests that are at 10, so sims can start reseaching them to start with.
                        results.Add(new InteractionObjectPair(new DefinitionDebateEnvironment(interests[i].mInterestsGuid, interests[i]), iop.Target));
                    }
                }
            }

            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                //return Localization.LocalizeString("Lyralei/Localized/socialsAskAboutEntry:InteractionName", new object[0]);
                return chosenType.ToString();
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (actor.NeedsToBeGreeted(target))
                {
                    return false;
                }
                if (!InterestManager.HasTheNecessaryInterest(actor.mSimDescription, chosenType, false))
                {
                    return false;
                }
                if (InterestManager.DoesSimHatesLovesInterest(actor.mSimDescription, chosenType) == 2 || InterestManager.DoesSimHatesLovesInterest(actor.mSimDescription, chosenType) == 3)
                {
                    return false;
                }
                if (target.Posture.Container is IBedDouble)
                {
                    return false;
                }
                if (actor == target)
                {
                    return false;
                }
                if (!actor.Posture.AllowsNormalSocials())
                {
                    return false;
                }
                if (!target.Posture.AllowsNormalSocials())
                {
                    return false;
                }
                return true;
            }
        }
        public static InteractionDefinition Singleton = new DefinitionDebateEnvironment();

        public static void DecideWhatToDoAfterDebate(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {
            DebateEnvironment debateEnvironment = i as DebateEnvironment;

            DefinitionDebateEnvironment definition = i.InteractionDefinition as DefinitionDebateEnvironment;
            if (debateEnvironment != null)
            {
                string Interactionname = "";

                // If target hates Environment...
                if (InterestManager.DoesSimHatesLovesInterest(target.mSimDescription, definition.chosenType) == 2)
                {
                    LongTermRelationship.UpdateLiking(actor.SimDescription, target.SimDescription, InterestManager.mSubstractLikingWhenHatingInterest);
                    //relationship.LTR.UpdateLiking(5f, base.Actor, base.Target);
                    InterestManager.LearnInterest(actor.mSimDescription, target.mSimDescription, definition.chosenType, true);
                    Interactionname = "Trait Incompatibility";
                }
                // Else if Target loves environment
                else if (InterestManager.DoesSimHatesLovesInterest(target.mSimDescription, definition.chosenType) == 1)
                {
                    LongTermRelationship.UpdateLiking(actor.SimDescription, target.SimDescription, InterestManager.mAddLikingWhenLovingInterest);
                    InterestManager.LearnInterest(actor.mSimDescription, target.mSimDescription, definition.chosenType, false);

                    if(InterestManager.CanAddSubPointsForSocial(actor.SimDescription, definition.chosenType))
                    {
                        InterestManager.AddSubPoints(0.2f, actor.SimDescription, definition.chosenType);
                    }
                    if(InterestManager.CanAddSubPointsForSocial(target.SimDescription, definition.chosenType))
                    {
                        InterestManager.AddSubPoints(0.2f, target.SimDescription, definition.chosenType);
                    }
                    Interactionname = "Trait Bonding";
                }

                if (InterestManager.CanAddSubPointsForSocial(actor.SimDescription, definition.chosenType))
                {
                    InterestManager.AddSubPoints(0.2f, actor.SimDescription, definition.chosenType);
                }

                if (InterestManager.CanAddSubPointsForSocial(target.SimDescription, definition.chosenType))
                {
                    if(RandomUtil.RandomChance(25f))
                    {
                        if (InterestManager.mSavedSimInterests.ContainsKey(target.mSimDescription.SimDescriptionId))
                        {
                            for (int j = 0; j < InterestManager.mSavedSimInterests[target.mSimDescription.SimDescriptionId].Count; j++)
                            {
                                if (InterestManager.mSavedSimInterests[target.mSimDescription.SimDescriptionId][j].Guid == definition.chosenType)
                                {
                                    InterestManager.mSavedSimInterests[target.mSimDescription.SimDescriptionId][j].modifyInterestLevel(1, target.mSimDescription.SimDescriptionId, InterestManager.mSavedSimInterests[target.mSimDescription.SimDescriptionId][j].mInterestsGuid);
                                    actor.ShowTNSIfSelectable("Success! I've convinced " + target.FirstName + " that the " + definition.chosenType.ToString() + " interest is really cool! Hopefully that introduces them to a great hobby to get into!", StyledNotification.NotificationStyle.kSimTalking);
                                    break;
                                }
                            }
                        }
                    }
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
                            actor.ShowTNSIfSelectable("I just learnt that " + target.FirstName + " absolutely LOVES the " + definition.chosenType.ToString() + " interest! We should hang out more and share more on our mutual interest!", StyledNotification.NotificationStyle.kSimTalking);
                        }
                        else
                        {
                            actor.ShowTNSIfSelectable("I just learnt that " + target.FirstName + " absolutely Hates the " + definition.chosenType.ToString() + " interest! Probably for the best if I stop talking about it...", StyledNotification.NotificationStyle.kSimTalking);
                        }
                    }
                }
            }
        }
    }

    public class RantAboutInterest : SocialInteractionA
    {
        public class DefinitionRantAboutInterest : Definition, IHasTraitIcon
        {

            public InterestTypes chosenType = InterestTypes.None;
            public Interest mInterestForIcon = null;
            public DefinitionRantAboutInterest()
                : base("RantAboutInterest", new string[0], null, false)
            {
            }

            public DefinitionRantAboutInterest(InterestTypes chosen, Interest interest)
                : base("RantAboutInterest", new string[0], null, false)
            {
                chosenType = chosen;
                mInterestForIcon = interest;
            }

            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                RantAboutInterest rantAboutInterest = new RantAboutInterest();
                rantAboutInterest.Init(ref parameters);
                return rantAboutInterest;
            }

            public override ResourceKey GetTraitIcon(Sim actor, GameObject target)
            {
                return InterestManager.GetCorrectTraitIcon(actor.mSimDescription, mInterestForIcon);
            }

            public override string[] GetPath(bool isFemale)
            {
                string text = Localization.LocalizeString("Gameplay/Socializing:Special");
                //string text = ""
                return new string[3]
                {
                    text,
                    "Interests & Hobbies Mod...",
                    "Rant About..."
                };
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, Sim target, List<InteractionObjectPair> results)
            {
                if (actor.NeedsToBeGreeted(target))
                {
                    return;
                }

                List<Interest> interests = InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId];
                for (int i = 0; i < interests.Count; i++)
                {
                    if (interests[i] != null)
                    {
                        // Loop through all interests that are at 10, so sims can start reseaching them to start with.
                        results.Add(new InteractionObjectPair(new DefinitionRantAboutInterest(interests[i].mInterestsGuid, interests[i]), iop.Target));
                    }
                }
            }

            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                //return Localization.LocalizeString("Lyralei/Localized/socialsAskAboutEntry:InteractionName", new object[0]);
                return chosenType.ToString();
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (actor.NeedsToBeGreeted(target))
                {
                    return false;
                }
                if (!InterestManager.HasTheNecessaryInterest(actor.mSimDescription, chosenType, false))
                {
                    return false;
                }
                if (InterestManager.DoesSimHatesLovesInterest(actor.mSimDescription, chosenType) == 1 || InterestManager.DoesSimHatesLovesInterest(actor.mSimDescription, chosenType) == 3 || InterestManager.DoesSimHatesLovesInterest(actor.mSimDescription, chosenType) == 0)
                {
                    return false;
                }
                if (target.Posture.Container is IBedDouble)
                {
                    return false;
                }
                if (actor == target)
                {
                    return false;
                }
                if (!actor.Posture.AllowsNormalSocials())
                {
                    return false;
                }
                if (!target.Posture.AllowsNormalSocials())
                {
                    return false;
                }

                return true;
            }
        }
        public static InteractionDefinition Singleton = new DefinitionRantAboutInterest();

        public static void DecideWhatToDoAfterRant(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {
            RantAboutInterest rantAboutInterest = i as RantAboutInterest;

            DefinitionRantAboutInterest definition = i.InteractionDefinition as DefinitionRantAboutInterest;
            if (rantAboutInterest != null)
            {
                string Interactionname = "";

                // If target hates Environment...
                if (InterestManager.DoesSimHatesLovesInterest(target.mSimDescription, definition.chosenType) == 2)
                {
                    LongTermRelationship.UpdateLiking(actor.SimDescription, target.SimDescription, InterestManager.mAddLikingWhenLovingInterest);
                    //relationship.LTR.UpdateLiking(5f, base.Actor, base.Target);
                    if (InterestManager.CanAddSubPointsForSocial(actor.SimDescription, definition.chosenType))
                    {
                        InterestManager.AddSubPoints(0.2f, actor.SimDescription, definition.chosenType);
                    }
                    if (InterestManager.CanAddSubPointsForSocial(target.SimDescription, definition.chosenType))
                    {
                        InterestManager.AddSubPoints(0.2f, target.SimDescription, definition.chosenType);
                    }
                    InterestManager.LearnInterest(actor.mSimDescription, target.mSimDescription, definition.chosenType, false);
                    Interactionname = "Trait Bonding";
                }
                // Else if Target loves environment
                else if (InterestManager.DoesSimHatesLovesInterest(target.mSimDescription, definition.chosenType) == 1)
                {
                    LongTermRelationship.UpdateLiking(actor.SimDescription, target.SimDescription, InterestManager.mSubstractLikingWhenHatingInterest);
                    InterestManager.LearnInterest(actor.mSimDescription, target.mSimDescription, definition.chosenType, true);
                    Interactionname = "Trait Incompatibility";
                }

                if (InterestManager.CanAddSubPointsForSocial(actor.SimDescription, definition.chosenType))
                {
                    InterestManager.AddSubPoints(0.2f, actor.SimDescription, definition.chosenType);
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
                            actor.ShowTNSIfSelectable("I just learnt that " + target.FirstName + " absolutely HATES the " + definition.chosenType.ToString() + " interest, just like me! We really should rant about this more!", StyledNotification.NotificationStyle.kSimTalking);
                        }
                        else
                        {
                            actor.ShowTNSIfSelectable("I just learnt that " + target.FirstName + " absolutely LOVES the " + definition.chosenType.ToString() + " interest... you do you... I'll keep on hating this interest!", StyledNotification.NotificationStyle.kSimTalking);
                        }
                    }
                }
            }
        }
    }

    public class ConvinceToPursueInterest : SocialInteractionA
    {
        public class DefinitionConvinceToPursueInterest : Definition, IHasTraitIcon
        {
            public InterestTypes chosenType = InterestTypes.None;
            public Interest mInterest = null;
            public int TotalScore = 0;

            public DefinitionConvinceToPursueInterest()
                : base("ConvinceToPursueInterest", new string[0], null, false)
            {
            }

            public DefinitionConvinceToPursueInterest(InterestTypes chosen, Interest interest, int totalScore)
                : base("ConvinceToPursueInterest", new string[0], null, false)
            {
                chosenType = chosen;
                mInterest = interest;
                TotalScore = totalScore;
            }

            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                ConvinceToPursueInterest rantAboutInterest = new ConvinceToPursueInterest();
                rantAboutInterest.Init(ref parameters);
                return rantAboutInterest;
            }

            public override ResourceKey GetTraitIcon(Sim actor, GameObject target)
            {
                return InterestManager.GetCorrectTraitIcon(actor.mSimDescription, mInterest);
            }

            public override string[] GetPath(bool isFemale)
            {
                string text = Localization.LocalizeString("Gameplay/Socializing:Special");
                //string text = ""
                return new string[3]
                {
                    text,
                    "Interests & Hobbies Mod...",
                    "Convince To Pursue..."
                };
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, Sim target, List<InteractionObjectPair> results)
            {
                if (actor.NeedsToBeGreeted(target))
                {
                    return;
                }
                List<Interest> interests = InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId];
                for (int i = 0; i < interests.Count; i++)
                {
                    if (interests[i] != null)
                    {
                        // Loop through all interests that are at 10, so sims can start reseaching them to start with.
                        results.Add(new InteractionObjectPair(new DefinitionConvinceToPursueInterest(interests[i].mInterestsGuid, interests[i], InterestManager.GetPercentage(actor, target, interests[i])), iop.Target));
                    }
                }
            }

            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                //+ GetPercentage(actor, target).ToString()
                return chosenType.ToString() + " (Success Rate: " + TotalScore.ToString() + "%)";
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {

                if (target.Posture.Container is IBedDouble)
                {
                    return false;
                }
                if (actor.SimDescription.SimDescriptionId == target.SimDescription.SimDescriptionId)
                {
                    return false;
                }
                if (!actor.Posture.AllowsNormalSocials())
                {
                    return false;
                }
                if (!target.Posture.AllowsNormalSocials())
                {
                    return false;
                }
                if (!InterestManager.HasTheNecessaryInterest(actor.mSimDescription, chosenType, true) && InterestManager.GetLevelForInterest(actor.mSimDescription, chosenType) <= 17)
                {
                    return false;
                }
                if (InterestManager.HasTheNecessaryInterest(target.mSimDescription, chosenType, true) && InterestManager.GetLevelForInterest(actor.mSimDescription, chosenType) <= 12)
                {
                    return false;
                }
                if (InterestManager.DoesSimHatesLovesInterest(actor.mSimDescription, chosenType) == 2 || InterestManager.DoesSimHatesLovesInterest(actor.mSimDescription, chosenType) == 3 || InterestManager.DoesSimHatesLovesInterest(actor.mSimDescription, chosenType) == 0)
                {
                    return false;
                }
                return true;
            }
        }
        
        public static InteractionDefinition Singleton = new DefinitionConvinceToPursueInterest();

        public static void DecideWhatToDoAfterConvicing(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {
            if(actor.SimDescription.SimDescriptionId == target.SimDescription.SimDescriptionId)
            {
                return;
            }
            
            ConvinceToPursueInterest convinceToPursue = i as ConvinceToPursueInterest;

            DefinitionConvinceToPursueInterest definition = i.InteractionDefinition as DefinitionConvinceToPursueInterest;
            if (convinceToPursue != null)
            {
                InterestManager.LearnInterest(actor.mSimDescription, target.mSimDescription, definition.chosenType, false);

                // To convince a sim they:
                // ** need the right sets of traits
                // ** A good frienship score
                // ** optional, but adds a bonus, Skills.

                if (RandomUtil.RandomChance(definition.TotalScore))
                {
                    if (InterestManager.mSavedSimInterests.ContainsKey(target.mSimDescription.SimDescriptionId))
                    {
                        for (int j = 0; j < InterestManager.mSavedSimInterests[target.mSimDescription.SimDescriptionId].Count; j++)
                        {
                            if (InterestManager.mSavedSimInterests[target.mSimDescription.SimDescriptionId][j].Guid == definition.chosenType)
                            {
                                InterestManager.mSavedSimInterests[target.mSimDescription.SimDescriptionId][j].modifyInterestLevel(3, target.mSimDescription.SimDescriptionId, InterestManager.mSavedSimInterests[target.mSimDescription.SimDescriptionId][j].mInterestsGuid);
                                actor.ShowTNSIfSelectable("I've conviced " + target.FirstName + " to seriously pursue the " + definition.chosenType.ToString() + " interest! This means I might actually see them at the hobby lot more often!", StyledNotification.NotificationStyle.kSimTalking);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}

