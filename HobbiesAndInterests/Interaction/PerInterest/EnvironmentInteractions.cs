using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Socializing;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;
using static Sims3.Gameplay.Objects.Electronics.Phone;

namespace Lyralei.InterestMod
{

	public class CancelCarpoolService : Call
	{
		public class Definition : CallDefinition<CancelCarpoolService>
		{
			public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
			{

				if (!target.IsUsableBy(a))
					return false;
				if (a.SimDescription == null)
					return false;
				if (a.SimDescription.TeenOrBelow)
					return false;
				if (a.Household == Household.NpcHousehold)
					return false;
				if (a.Service != null)
					return false;
				if (a.CareerManager == null && a.CareerManager.Occupation == null && a.OccupationAsCareer == null)
					return false;
				return base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback);
			}

			public override string[] GetPath(bool isFemale)
			{
				return new string[] { "Interests & Hobbies...", "Services..." } ;
			}

			public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop)
			{
				if (actor.SimDescription == null)
					return "";
				if (actor.Occupation == null)
					return "";
				if (actor == null)
					return "[Interest & Hobbies]: SIM IS NULL? (original interaction: Cancel carpool service";

				if (actor.Occupation.mbCarpoolEnabled)
					return "Cancel Carpool Service";

				return "Continue Carpool Service";
			}
		}

		public static InteractionDefinition Singleton = new Definition();


		public override ConversationBehavior OnCallConnected()
		{
			Actor.Occupation.mbCarpoolEnabled = !Actor.Occupation.mbCarpoolEnabled;

			if (InterestManager.CanAddSubPointsForGeneric(Actor.SimDescription, InterestTypes.Environment))
			{
				InterestManager.AddSubPoints(1f, Actor.SimDescription, InterestTypes.Environment);
			}
			Actor.ShowTNSIfSelectable("From now on the carpool won't pick me up anymore. If I ever change my mind, they said I can always call them up again.", StyledNotification.NotificationStyle.kSimTalking);

			return ConversationBehavior.TalkBriefly;
		}
	}

	public class CancelSchoolBusService : Call
	{
		public class Definition : CallDefinition<CancelSchoolBusService>
		{
			public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
			{

				if (!target.IsUsableBy(a))
				{
					return false;
				}
				if (a.Household == Household.NpcHousehold)
				{
					return false;
				}
				if (a.Service != null)
				{
					return false;
				}
                if (a.SimDescription == null)
                    return false;

				if (a.School == null)
					return false;

				//Making this false for now, because I have nooooo idea why it makes children unselectable...
				if (a.SimDescription.Child || a.SimDescription.Teen)
					return false;



				if (a.SimDescription.Genealogy != null)
                {
                    if (a.SimDescription.Genealogy.Children != null && a.SimDescription.Genealogy.Children.Count > 0)
                    {
                        // Check if the parent at least has one child.
                        foreach (Genealogy gen in a.SimDescription.Genealogy.Children)
                        {
                            if (gen == null)
                                continue;
                            if ((gen.SimDescription.Teen || gen.SimDescription.Child) && gen.SimDescription.LotHomeId == a.LotHome.LotId)
                                return base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback);
                        }
                    }
                }

				return false;
			}

			public override string[] GetPath(bool isFemale)
			{
				return new string[] { "Interests & Hobbies...", "Services..." };
			}

			public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop)
			{
				return GetBusString(actor);

			}
		}

		public static string GetBusString(Sim actor)
		{
			if (actor.SimDescription.Genealogy != null)
			{
				if (actor.SimDescription.Genealogy.Children != null && actor.SimDescription.Genealogy.Children.Count > 0)
				{
					if (actor.SimDescription.Genealogy.Children.Count > 0 && actor.SimDescription.Genealogy.Children[0].SimDescription.CareerManager != null && actor.SimDescription.Genealogy.Children[0].SimDescription.CareerManager.School != null && actor.SimDescription.Genealogy.Children[0].SimDescription.CareerManager.School.mbCarpoolEnabled)
					{
						return "Cancel School Bus Service for all Children";
					}
					else if (actor.SimDescription.Genealogy.Children.Count > 0 && actor.SimDescription.Genealogy.Children[0].SimDescription.CareerManager != null && actor.SimDescription.Genealogy.Children[0].SimDescription.CareerManager.School != null&& !actor.SimDescription.Genealogy.Children[0].SimDescription.CareerManager.School.mbCarpoolEnabled)
					{
						return "Continue School Bus Service for all Children";
					}
				}

				if (actor.School != null && actor.School.mbCarpoolEnabled)
				{
					return "Cancel School Bus Service for all children";
				}
				if (actor.School != null && !actor.School.mbCarpoolEnabled)
				{
					return "Continue School Bus Service";
				}

			}


			return "";
		}

		public static InteractionDefinition Singleton = new Definition();


		public override ConversationBehavior OnCallConnected()
		{

			if (Actor.SimDescription.Teen || Actor.SimDescription.Child)
            {
				Actor.School.mbCarpoolEnabled = !Actor.School.mbCarpoolEnabled;
				Actor.ShowTNSIfSelectable("From now on the School bus won't pick me up anymore. If I ever change my mind, they said I can always call them up again.", StyledNotification.NotificationStyle.kSimTalking);
			}
			else
            {
				foreach (Sim children in Actor.Household.Sims)
				{
					if (children.SimDescription.Teen || children.SimDescription.Child)
					{
						children.School.mbCarpoolEnabled = !children.School.mbCarpoolEnabled;
					}
				}
				Actor.ShowTNSIfSelectable("From now on the School bus won't pick up the kids anymore. If I ever change my mind, they said I can always call them up again.", StyledNotification.NotificationStyle.kSimTalking);
			}

			if (InterestManager.CanAddSubPointsForGeneric(Actor.SimDescription, InterestTypes.Environment))
			{
				InterestManager.AddSubPoints(1f, Actor.SimDescription, InterestTypes.Environment);
			}

			return ConversationBehavior.TalkBriefly;
		}
	}

}

