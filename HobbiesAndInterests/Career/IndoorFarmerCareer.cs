using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.Gameplay.ActiveCareer;
using System;
using System.Collections.Generic;
using System.Text;
using Sims3.Gameplay.Autonomy;
using HobbiesAndInterests.Career;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Core;
using Sims3.SimIFace.CAS;

namespace Sims3.Gameplay.Lyralei.InterestMod.Careers
{
    public class IndoorFarmerCareer : SkillBasedCareer
	{
		public static InterestTypes mInterestType = InterestTypes.Environment;
		public static ulong mGUID = 0x6E2E4EC20E9DA1A1;

		public IndoorFarmerCareer()
		{
		}

		public IndoorFarmerCareer(OccupationNames guid)
			: base(guid)
		{
		}

		public override DaysOfTheWeek PensionPayDays
		{
			get
			{
				return DaysOfTheWeek.None;
			}
		}

		public override int PensionAmount()
		{
			return 0;
		}

		public override void OnStartup()
		{
			base.OnStartup();
		}
		public override void SetAttributesForNewJob(CareerLocation location, ulong lotId, bool bRequestFromCharacterImport)
		{
			base.SetAttributesForNewJob(location, lotId, bRequestFromCharacterImport);
		}

		public override void LeaveJobNow(Career.LeaveJobReason reason)
		{
			base.LeaveJobNow(reason);
			Sim ownerSim = base.OwnerSim;
			if (ownerSim != null)
			{
				Inventory inventory = ownerSim.Inventory;
			}
		}

		public static bool isCareer(Sim sim)
		{
			if (sim != null && sim.Occupation != null)
			{
				return sim.Occupation is IndoorFarmerCareer;
			}
			return false;
		}

		public override bool OnRejoiningQuitCareer()
		{
			base.OnRejoiningQuitCareer();
			Sim ownerSim = base.OwnerSim;
			return true;
		}

		public override void OnSetupWorkDayAlarms(bool removeJobCreationAlarms)
		{
			base.OnSetupWorkDayAlarms(removeJobCreationAlarms);
		}

		private void AddObjectToInventory(string instanceName, ProductVersion version, Inventory inventory)
		{
			IGameObject gameObject = GlobalFunctions.CreateObjectOutOfWorld(instanceName, version);
			if (gameObject != null && !(gameObject is FailureObject))
			{
				inventory.TryToAdd(gameObject);
			}
		}

		public static void SwitchToWorkOutfit(Sim actor)
		{
			if (actor != null)
			{
				Occupation occupation = actor.Occupation;
				if (occupation != null && occupation.Guid == (OccupationNames)mGUID)
				{
					actor.SwitchToOutfitWithSpin(Sim.ClothesChangeReason.GoingToWork, OutfitCategories.Career);
				}
			}
		}

		public override Occupation Clone()
		{
			SkillBasedCareer skillBasedCareer = (SkillBasedCareer)MemberwiseClone();
			skillBasedCareer.AvailableInFutureWorld = true;
			return skillBasedCareer;
		}
	}

	public class JoinActiveCareerFarmer : RabbitHole.ModalDialogRabbitHoleInteraction<CityHall>
	{
		private sealed class Definition : InteractionDefinition<Sim, CityHall, JoinActiveCareerFarmer>
		{
			public override bool Test(Sim a, CityHall target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
			{
				return true;
			}

			public override string[] GetPath(bool isFemale)
			{
				return new string[2]
				{
					"Interests & Hobbies...",
					"Jobs..."
				};
			}

			public override string GetInteractionName(Sim actor, CityHall target, InteractionObjectPair iop)
			{
				return "Join Planter Career";
			}
		}

		public static readonly InteractionDefinition Singleton = new Definition();

		public override bool InRabbitHole()
		{
			TryDisablingCameraFollow(Actor);
			CareerInstantiatorManager.AssignHobbyActiveCareer(Actor, IndoorFarmerCareer.mGUID);
			if (Actor.Occupation != null && Actor.Occupation is IndoorFarmerCareer)
			{
				return true;
			}
			return false;
		}
	}





}
