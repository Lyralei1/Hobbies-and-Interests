using Lyralei.InterestMod;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Objects.Gardening;
using Sims3.Gameplay.Objects.Lyralei;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.UI;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;
using Environment = Sims3.Gameplay.Lyralei.InterestMod.Environment;

namespace Lyralei
{
    public class SimDebugInteractions
    {
        //public class AddInterestPoint : ImmediateInteraction<Sim, Sim>
        //{
        //    public static readonly InteractionDefinition Singleton = new Definition();

        //    public override bool Run()
        //    {
        //        //InterestManager.ManualInterestIncrease(base.Actor);
        //        return true;
        //    }

        //    [DoesntRequireTuning]
        //    public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, AddInterestPoint>
        //    {
        //        public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
        //        {
        //            return "Edit sim's Interests points";
        //        }
        //        public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        //        {
        //            return true;
        //        }
        //    }
        //}

        //public class AddInterestToSim : ImmediateInteraction<Sim, Sim>
        //{
        //    public static readonly InteractionDefinition Singleton = new Definition();

        //    public override bool Run()
        //    {
        //        return true;
        //    }

        //    [DoesntRequireTuning]
        //    public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, AddInterestToSim>
        //    {
        //        public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
        //        {
        //            return "Register sim to have interests";
        //        }
        //        public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        //        {
        //            return true;
        //        }
        //    }
        //}

        //public class CheckIfSimHasInterests : ImmediateInteraction<Sim, Sim>
        //{
        //    public static readonly InteractionDefinition Singleton = new Definition();

        //    public override bool Run()
        //    {
        //        InterestManager.Instance.hasAnyElementsCached(InterestManager.mSavedSimInterests[base.Actor.SimDescription.SimDescriptionId]);
        //        return true;
        //    }

        //    [DoesntRequireTuning]
        //    public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, CheckIfSimHasInterests>
        //    {
        //        public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
        //        {
        //            return "Check if sim has interests";
        //        }
        //        public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        //        {
        //            return true;
        //        }
        //    }
        //}

        public class ListInterestData : ImmediateInteraction<Sim, Sim>
        {
            public static readonly InteractionDefinition Singleton = new Definition();

            public override bool Run()
            {
                List<Interest> interests = InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId];
                
                StringBuilder str = new StringBuilder();
                str.Append("ID: " + base.Target.SimDescription.SimDescriptionId.ToString() + '\n');
                str.Append("Owner: " + base.Target.SimDescription.FullName.ToString() + '\n');
                str.Append('\n');

                //foreach (Interest interest in interests)
                for (int i = 0; i < interests.Count; i++)
                {
                    //Interest interest = InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId][c];

                    str.Append("GUID: " + interests[i].Guid.ToString() + '\n');
                    str.Append("Point(s): " + interests[i].currInterestPoints.ToString() + '\n');

                    str.Append('\n');
                }
                InterestManager.print(str.ToString());
                return true;
            }

            [DoesntRequireTuning]
            public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, ListInterestData>
            {
                public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
                {
                    return "List Interest Data";
                }
                public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return true;
                }
            }
        }


        //public class CheckInterestDetails : ImmediateInteraction<Sim, Sim>
        //{
        //    public static readonly InteractionDefinition Singleton = new Definition();

        //    public override bool Run()
        //    {
        //        List<Interest> interests = InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId];

        //        StringBuilder str = new StringBuilder();

        //        str.Append("ID: " + base.Target.SimDescription.SimDescriptionId.ToString() + '\n');
        //        str.Append("Owner: " + base.Target.SimDescription.FullName.ToString() + '\n');
        //        str.Append('\n');

        //        //foreach (Interest interest in interests)
        //        for (int i = 0; i < interests.Count; i++)
        //        {
        //            if(interests[i].Guid == InterestTypes.Environment)
        //            {
        //                foreach(Interest.Hobby hobby in interests[i].hobbies)
        //                {
        //                    if(hobby is Sims3.Gameplay.Lyralei.InterestMod.Environment.ClimateChangeHobby)
        //                    {
        //                        Sims3.Gameplay.Lyralei.InterestMod.Environment.ClimateChangeHobby climateChange = hobby as Sims3.Gameplay.Lyralei.InterestMod.Environment.ClimateChangeHobby;
        //                        str.Append(climateChange.mName + '\n');
        //                        str.Append(climateChange.mDescription + '\n');
        //                        str.Append("Is vegan? " + climateChange.IsVegetarian.ToString() + '\n');
        //                        str.Append("Plants donated " + climateChange.PlantSamplesDonated + '\n');
        //                        str.Append("Sims converted " + climateChange.SimsConverted + '\n');
        //                        str.Append("Tree planted " + climateChange.TreesPlantedAndGrown + '\n');
        //                        str.Append("Is master: " + climateChange.mIsMasterInHobby + '\n');
        //                        str.Append('\n');
        //                    }
        //                }
        //            }
        //            //Interest interest = InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId][c];
        //        }

        //        InterestManager.print(str.ToString());

        //        return true;
        //    }

        //    [DoesntRequireTuning]
        //    public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, CheckInterestDetails>
        //    {
        //        public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
        //        {
        //            return "Check interest Details";
        //        }
        //        public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        //        {
        //            return true;
        //        }
        //    }
        //}

        //public class ShowHobbiesUI : ImmediateInteraction<Sim, Sim>
        //{
        //    public static readonly InteractionDefinition Singleton = new Definition();

        //    public override bool Run()
        //    {
        //        try
        //        {
        //            if(!InterestManager.mSavedSimInterests.ContainsKey(base.Target.SimDescription.SimDescriptionId))
        //            {
        //                InterestManager.print("SimDescription doesn't exist in key");
        //                return true;
        //            }

        //            List<Interest> interests = InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId];

        //            StringBuilder str = new StringBuilder();

        //            //foreach (Interest interest in interests)
        //            for (int i = 0; i < interests.Count; i++)
        //            {
        //                if (interests[i].Guid == InterestTypes.Environment)
        //                {
        //                    foreach (Interest.Hobby hobby in interests[i].hobbies)
        //                    {
        //                        if (hobby is Sims3.Gameplay.Lyralei.InterestMod.Environment.ClimateChangeHobby)
        //                        {
        //                            Sims3.Gameplay.Lyralei.InterestMod.Environment.ClimateChangeHobby climateChange = hobby as Sims3.Gameplay.Lyralei.InterestMod.Environment.ClimateChangeHobby;
        //                            str.Append(climateChange.mName + '\n');
        //                            str.Append(climateChange.mDescription + '\n');
        //                            str.Append("Is vegan? " + climateChange.IsVegetarian.ToString() + '\n');
        //                            str.Append("Plants donated " + climateChange.PlantSamplesDonated + '\n');
        //                            str.Append("Sims converted " + climateChange.SimsConverted + '\n');
        //                            str.Append("Tree planted " + climateChange.TreesPlantedAndGrown + '\n');
        //                            str.Append("Is master: " + climateChange.mIsMasterInHobby + '\n');

        //                            str.Append("Hobbies required:" + '\n');

        //                            //climateChange.mRequiredSkillPoints = InterestManager.RequiredSkillsLevelForAgeSettings(climateChange.mRequiredSkillsForHobby, base.Target.SimDescription.SimDescriptionId);
        //                            for (int h = 0; h < climateChange.mRequiredSkillsForHobby.Count; h++)
        //                            {
        //                                str.Append(climateChange.mRequiredSkillsForHobby[h].ToString() + " - " + "Points in: " + base.Actor.SkillManager.GetSkillLevel(climateChange.mRequiredSkillsForHobby[h]).ToString() + " (mastered at level: " + climateChange.mRequiredSkillPoints[h].ToString() + '\n');
        //                                str.Append('\n');
        //                            }
        //                            str.Append('\n');
        //                        }
        //                        if (hobby is Sims3.Gameplay.Lyralei.InterestMod.Environment.NatureHobby)
        //                        {
        //                            Sims3.Gameplay.Lyralei.InterestMod.Environment.NatureHobby natureHobby = hobby as Sims3.Gameplay.Lyralei.InterestMod.Environment.NatureHobby;
        //                            str.Append(natureHobby.mName + '\n');
        //                            str.Append(natureHobby.mDescription + '\n');
        //                            str.Append("Fish caught " + natureHobby.FishCaughtCount.ToString() + '\n');
        //                            str.Append("Hobbies required:" + '\n');

        //                            //natureHobby.mRequiredSkillPoints = InterestManager.RequiredSkillsLevelForAgeSettings(natureHobby.mRequiredSkillsForHobby, base.Target.SimDescription.SimDescriptionId);
        //                            for (int h = 0; h < natureHobby.mRequiredSkillsForHobby.Count; h++)
        //                            {
        //                                str.Append(natureHobby.mRequiredSkillsForHobby[h].ToString() + " - " + "Points in: " + base.Actor.SkillManager.GetSkillLevel(natureHobby.mRequiredSkillsForHobby[h]).ToString() + " (mastered at level: " + natureHobby.mRequiredSkillPoints[h].ToString() + '\n');
        //                            }
        //                            str.Append('\n');
        //                        }
        //                        if (hobby is Sims3.Gameplay.Lyralei.InterestMod.Environment.OffTheGrid)
        //                        {
        //                            Sims3.Gameplay.Lyralei.InterestMod.Environment.OffTheGrid offTheGrid = hobby as Sims3.Gameplay.Lyralei.InterestMod.Environment.OffTheGrid;
        //                            str.Append(offTheGrid.mName + '\n');
        //                            str.Append(offTheGrid.mDescription + '\n');
        //                            str.Append("Hobbies required:" + '\n');

        //                            //offTheGrid.mRequiredSkillPoints = InterestManager.RequiredSkillsLevelForAgeSettings(offTheGrid.mRequiredSkillsForHobby, base.Target.SimDescription.SimDescriptionId);
        //                            for (int h = 0; h < offTheGrid.mRequiredSkillsForHobby.Count; h++)
        //                            {
        //                                str.Append(offTheGrid.mRequiredSkillsForHobby[h].ToString() + " - " + "Points in: " + base.Actor.SkillManager.GetSkillLevel(offTheGrid.mRequiredSkillsForHobby[h]).ToString() + " (mastered at level: " + offTheGrid.mRequiredSkillPoints[h].ToString() + '\n');
        //                            }
        //                            str.Append('\n');
        //                        }
        //                    }
        //                }
        //                //Interest interest = InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId][c];
        //            }
        //            InterestManager.print(str.ToString());
        //            return true;
        //        }
        //        catch(Exception ex)
        //        {
        //            InterestManager.print(ex.ToString());
        //            return true;
        //        }
        //    }

        //    [DoesntRequireTuning]
        //    public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, ShowHobbiesUI>
        //    {
        //        public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
        //        {
        //            return "Show Hobbies UI";
        //        }
        //        public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        //        {
        //            return true;
        //        }
        //    }
        //}

        public class ManipulateHobbyPoints : ImmediateInteraction<Sim, Sim>
        {
            public static readonly InteractionDefinition Singleton = new Definition();

            public override bool Run()
            {
                List<Interest> interests = InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId];

                StringBuilder str = new StringBuilder();
                //foreach (Interest interest in interests)
                for (int i = 0; i < interests.Count; i++)
                {
                    if (interests[i].Guid == InterestTypes.Environment)
                    {

                        for (int h = 0; h < interests[i].hobbies.Count; i++)
                        {
                            if (interests[i].hobbies[h] is Sims3.Gameplay.Lyralei.InterestMod.Environment.ClimateChangeHobby)
                            {
                                Sims3.Gameplay.Lyralei.InterestMod.Environment.ClimateChangeHobby climateChange = interests[i].hobbies[h] as Sims3.Gameplay.Lyralei.InterestMod.Environment.ClimateChangeHobby;

                                climateChange.TreesPlantedAndGrown = RandomUtil.GetInt(1, 10);
                                climateChange.SimsConverted = RandomUtil.GetInt(1, 10);
                                climateChange.PlantSamplesDonated = RandomUtil.GetInt(1, 10);
                            }
                        }
                    }
                    //Interest interest = InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId][c];
                }

                //base.Target.LotHome.MakeSimDoSomethingAfterArrivingAtVenue(MetaAutonomyVenueType.Garden, base.Target);

                return true;
            }

            [DoesntRequireTuning]
            public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, ManipulateHobbyPoints>
            {
                public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
                {
                    return "Manipulate Hobby points";
                }
                public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return true;
                }
            }
        }

        //public class HasBuggedInterests : ImmediateInteraction<Sim, Sim>
        //{
        //    public static readonly InteractionDefinition Singleton = new Definition();

        //    public override bool Run()
        //    {
        //        InterestManager.FixWeirdInterestData(base.Target.SimDescription);
        //        return true;
        //    }

        //    [DoesntRequireTuning]
        //    public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, HasBuggedInterests>
        //    {
        //        public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
        //        {
        //            return "Check for Bugged Interests";
        //        }
        //        public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        //        {
        //            return true;
        //        }
        //    }
        //}

        //public class ExtractSaveData : ImmediateInteraction<Sim, Sim>
        //{
        //    public static readonly InteractionDefinition Singleton = new Definition();

        //    public override bool Run()
        //    {
        //        InterestSaveManager.ImportInterestData();
        //        return true;
        //    }

        //    [DoesntRequireTuning]
        //    public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, ExtractSaveData>
        //    {
        //        public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
        //        {
        //            return "Extract Save Data";
        //        }
        //        public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        //        {
        //            return true;
        //        }
        //    }
        //}

        //public class SaveTheData : ImmediateInteraction<Sim, Sim>
        //{
        //    public static readonly InteractionDefinition Singleton = new Definition();

        //    public override bool Run()
        //    {
        //        InterestSaveManager.ExportInterestData();
        //        return true;
        //    }

        //    [DoesntRequireTuning]
        //    public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, SaveTheData>
        //    {
        //        public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
        //        {
        //            return "Save The Data";
        //        }
        //        public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        //        {
        //            return true;
        //        }
        //    }
        //}

        public class ChooseAHobbyDEBUG : ImmediateInteraction<Sim, Sim>
        {
            public static readonly InteractionDefinition Singleton = new Definition();

            public override bool Run()
            {
                List<Interest> interests = InterestManager.mSavedSimInterests[base.Target.SimDescription.SimDescriptionId];
                for (int i = 0; i < interests.Count; i++)
                {
                    if (interests[i].Guid == InterestTypes.Environment)
                    {
                        if (interests[i].currInterestPoints == 0) interests[i].modifyInterestLevel(10, base.Target.SimDescription.SimDescriptionId, InterestTypes.Environment);

                        List<ObjectListPickerInfo> breedInfo = new List<ObjectListPickerInfo>();

                        foreach (Interest.Hobby hobby in interests[i].hobbies)
                        {
                            ObjectListPickerInfo o = new ObjectListPickerInfo(hobby.mName, hobby);
                            breedInfo.Add(o);
                        }
                        Interest.Hobby chosenItem = ObjectListPickerDialog.Show(breedInfo) as Interest.Hobby;

                        SimpleMessageDialog.Show("Interests & hobbies: ", chosenItem.ToString());

                        foreach (Interest.HobbyChallenge challenge in chosenItem.mHobbyChallenges)
                        {
                            SimpleMessageDialog.Show("Interests & hobbies: ", challenge.ToString());
                        }
                    }
                }
                return true;
            }

            [DoesntRequireTuning]
            public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, ChooseAHobbyDEBUG>
            {
                public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
                {
                    return "[DEBUG] Check a Hobby...";
                }
                public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return true;
                }
            }
        }

        //public class CheckTheSavedDataList : ImmediateInteraction<Sim, Sim>
        //{
        //    public static readonly InteractionDefinition Singleton = new Definition();

        //    public override bool Run()
        //    {
        //        Sim[] objects = Sims3.Gameplay.Queries.GetObjects<Sim>();
        //        InterestManager.print("Sim data available: " + InterestManager.mSavedSimInterests.Count.ToString() + "   (Current population: " + objects.Length.ToString());

        //        return true;
        //    }

        //    [DoesntRequireTuning]
        //    public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, CheckTheSavedDataList>
        //    {
        //        public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
        //        {
        //            return "Check The Saved Data lists";
        //        }
        //        public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        //        {
        //            return true;
        //        }
        //    }
        //}


        public class GenerateInterestBook : ImmediateInteraction<Sim, Sim>
        {
            public static readonly InteractionDefinition Singleton = new Definition();

            public override bool Run()
            {
                try
                {
                    if (GlobalOptionsHobbiesAndInterests.BookMagazineInterestsDataList == null)
                    {
                        GlobalOptionsHobbiesAndInterests.print("Interests Magazine List is null");
                        return true;
                    }
                    if (GlobalOptionsHobbiesAndInterests.BookMagazineInterestsDataList.Values == null)
                    {
                        return true;
                    }
                    List<BookMagazineInterestsData> list = new List<BookMagazineInterestsData>(GlobalOptionsHobbiesAndInterests.BookMagazineInterestsDataList.Values);


                    if (list.Count > 0)
                    {
                        int @int = RandomUtil.GetInt(list.Count - 1);
                        GlobalOptionsHobbiesAndInterests.print("List is not null");
                        if (list[@int] == null)
                        {
                            GlobalOptionsHobbiesAndInterests.print("list[@int] was null");
                            return true;
                        }
                        else
                        {
                            GlobalOptionsHobbiesAndInterests.print("list[@int] was NOT null");

                            BookMagazineInterests bookGeneral = BookMagazineInterests.CreateOutOfWorld(list[@int]);
                            if (bookGeneral == null)
                            {
                                GlobalOptionsHobbiesAndInterests.print("Magazine is null");
                            }
                            else
                            {
                                GlobalOptionsHobbiesAndInterests.print("Magazine is NOT null");
                            }

                            if (!base.Actor.Inventory.TryToAdd(bookGeneral))
                            {
                                GlobalOptionsHobbiesAndInterests.print("Couldn't add to Mailbox :(");
                            }
                            return true;
                        }
                    }
                    else
                    {
                        GlobalOptionsHobbiesAndInterests.print("List was null");
                    }
                }
                catch(Exception ex)
                {
                    GlobalOptionsHobbiesAndInterests.print(ex.ToString());
                }
                return true;
            }

            [DoesntRequireTuning]
            public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, GenerateInterestBook>
            {
                public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
                {
                    return "DEBUG create random interest book";
                }
                public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return true;
                }
            }
        }

        public class TrySettingUpHobbyClubsAgain : ImmediateInteraction<Sim, Sim>
        {
            public static readonly InteractionDefinition Singleton = new Definition();

            public override bool Run()
            {
                if (HobbyClubsManager.ClubsInTown == null)
                    HobbyClubsManager.ClubsInTown = new List<HobbyClubsManager.ClubHobby>();
                else
                    HobbyClubsManager.ClubsInTown.Clear();
                HobbyClubsManager.Startup();

                HobbyClubsManager.LoadClubXML();

                StringBuilder sb = new StringBuilder();
                foreach (HobbyClubsManager.ClubHobby club in HobbyClubsManager.ClubsInTown)
                {
                    string line = "";
                    if (club.HobbyLot != null)
                    {
                        line += " At: " + club.HobbyLot.Address;
                    }
                    if (club.ClubOwner != 0)
                    {
                        SimDescription owner = SimDescription.Find(club.ClubOwner);

                        if (owner != null)
                        {
                            line += " owner: " + owner.FullName;
                        }
                    }
                    sb.AppendLine(club.Name + line);
                }

                InterestManager.print(sb.ToString());
                return true;
            }

            [DoesntRequireTuning]
            public sealed class Definition : ImmediateInteractionDefinition<Sim, Sim, TrySettingUpHobbyClubsAgain>
            {
                public override string GetInteractionName(Sim a, Sim target, InteractionObjectPair interaction)
                {
                    return "DEBUG Redo Hobby Lot loading.";
                }
                public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {

                    return true;
                }
            }
        }


    }
}
