using Lyralei;
using Sims3.Gameplay.ActorSystems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Animals : Interest
    {
        public Animals(ulong simDescription)
            : base(InterestTypes.Animals, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Animals;
            this.indexInList = 0;

            this.Name = "Animals"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";
            this.traitBoost = new List<TraitNames>() {
                TraitNames.SupernaturalFanTrait,
                TraitNames.Inappropriate,
                TraitNames.NightOwlTrait,
                TraitNames.Daredevil
            };
            this.traitPenalty = new List<TraitNames>() {
                TraitNames.SupernaturalSkeptic,
                TraitNames.Coward
            };
            //ClimateChangeHobby climateChangeHobby = new ClimateChangeHobby(IsVegetarian);
            //NatureHobby natureHobby = new NatureHobby();
            //OffTheGrid offTheGrid = new OffTheGrid();

            //base.mHobbies = new List<Hobby>() {
            //                                                climateChangeHobby,
            //                                                natureHobby,
            //                                                offTheGrid
            //                                          };
        }

        public Animals()
        {
        }

        public override void CreateInterestJournalInfo()
        {
        }
        public override Interest Clone()
        {
            Animals animals = base.Clone() as Animals;
            return animals;
        }

    }
}
