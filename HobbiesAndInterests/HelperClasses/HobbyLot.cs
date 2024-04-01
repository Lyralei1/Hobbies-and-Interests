using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Fishing;
using Sims3.Gameplay.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using static Sims3.Gameplay.Actors.Sim.ZombifiedSelfInteractions.Definition;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class HobbyLot : Lot
    {
        public ulong lotID = 0u;

        public Lot lot;

        public InterestTypes typeHobbyLot = InterestTypes.None;

        public Dictionary<string, InteractionDefinition> mBlacklistedInteractions = new Dictionary<string, InteractionDefinition>();

        public Dictionary<string, InteractionDefinition> mWhitelistedInteractions = new Dictionary<string, InteractionDefinition>();

        public Dictionary<string, InteractionDefinition> mAvailableInteractions = new Dictionary<string, InteractionDefinition>();

        public List<InteractionDefinition> mBlacklistSims = new List<InteractionDefinition>();

        public void InitLot()
        {
            mAvailableInteractions = InterestManager.GetDefaultLotInteractions(typeHobbyLot);
            mWhitelistedInteractions = InterestManager.GetDefaultLotInteractions(typeHobbyLot);
        }

        // flesh this out to mention what type of interaction it got

        public InteractionDefinition GetRandomInteractionFromWhitelist(InterestTypes interestType, SimDescription simDescription)
        {
            InteractionTypesHobbies interactionTypes = InteractionTypesHobbies.None;
            List<InteractionTypesHobbies> items = null;
            switch (interestType)
            {
                case InterestTypes.Politics:
                    break;
                case InterestTypes.Crime:
                    break;
                case InterestTypes.Food:
                    break;
                case InterestTypes.Sports:
                    break;
                case InterestTypes.Money:
                    break;
                case InterestTypes.Entertainment:
                    break;
                case InterestTypes.Health:
                    break;
                case InterestTypes.Paranormal:
                    break;
                case InterestTypes.Toys:
                    break;
                case InterestTypes.Environment:
                    items = new List<InteractionTypesHobbies>() { InteractionTypesHobbies.Gardening, InteractionTypesHobbies.Fishing, };

                    if (simDescription.SkillManager.HasElement(SkillNames.Gardening) && simDescription.SkillManager.GetSkillLevel(SkillNames.Gardening) > 3) items.Add(InteractionTypesHobbies.Gardening);
                    if (simDescription.SkillManager.HasElement(SkillNames.Fishing) && simDescription.SkillManager.GetSkillLevel(SkillNames.Fishing) > 3) items.Add(InteractionTypesHobbies.Fishing);
                    items.Add(InteractionTypesHobbies.BeeKeeping);

                    break;
                case InterestTypes.Culture:
                    break;
                case InterestTypes.Fashion:
                    break;
                case InterestTypes.Animals:
                    break;
                case InterestTypes.Scifi:
                    break;
                case InterestTypes.None:
                    break;
                default: break;
            }

            if(items != null && items.Count > 0)
            {
                interactionTypes = RandomUtil.GetRandomObjectFromList<InteractionTypesHobbies>(items);
                switch (interactionTypes)
                {
                    case InteractionTypesHobbies.Gardening:
                        InteractionDefinition interactionChosenGarden = RandomUtil.GetRandomObjectFromDictionary(InterestManager.DefaultGardeningInteractions);

                        if (mWhitelistedInteractions.ContainsValue(interactionChosenGarden))
                        {
                            return interactionChosenGarden;
                        }
                        break;
                    case InteractionTypesHobbies.BeeKeeping:

                        InteractionDefinition interactionChosenBees = RandomUtil.GetRandomObjectFromDictionary(InterestManager.DefaultBeeKeepingInteractions);

                        if (mWhitelistedInteractions.ContainsValue(interactionChosenBees))
                        {
                            return interactionChosenBees;
                        }
                        break;
                    case InteractionTypesHobbies.Fishing:
                        InteractionDefinition interactionChosenFish = FishAutonomously.Singleton;

                        if (mWhitelistedInteractions.ContainsValue(interactionChosenFish))
                        {
                            return interactionChosenFish;
                        }
                        break;
                    default:
                        return null;
                }
            }



           
            return null;
        }
    }
}
