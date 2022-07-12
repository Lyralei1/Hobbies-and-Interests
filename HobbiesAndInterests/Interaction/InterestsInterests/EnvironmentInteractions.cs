using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
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
    public class InstallSolarPanels : Interaction<Sim, Lot>
    {
        public class Definition : InteractionDefinition<Sim, Lot, ResearchInterest>
        {
            public override string[] GetPath(bool isFemale)
            {
                return new string[1]
                {
                    "Interests & Hobbies...",
                };
            }

            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return "Install Solar Panels on Roof";
            }

            public override bool Test(Sim actor, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if(Sims3.SimIFace.World.HasAnyRoof(target.LotId))
                {
                    greyedOutTooltipCallback = (() => "These solar panels need to be installed on a roof! Either build a roof over the house or install the solar panels on the ground.");
                    return false;
                }

                List<Interest> interests = InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId];

                for (int i = 0; i < interests.Count; i++)
                {
                    if (interests[i] != null && interests[i].currInterestPoints >= 13)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            return true;
        }
    }
}

