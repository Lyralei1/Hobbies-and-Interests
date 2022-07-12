using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Fishing;
using System;
using System.Collections.Generic;
using System.Text;

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
        public InteractionDefinition GetRandomInteractionFromWhitelist(InteractionTypesHobbies interactionTypes)
        {
            switch(interactionTypes)
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
            return null;
        }
    }
}
