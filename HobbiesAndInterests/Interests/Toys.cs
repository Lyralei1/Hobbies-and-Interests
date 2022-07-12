using Lyralei;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Toys : Interest
    {
        public Toys(ulong simDescription)
            : base(InterestTypes.Toys, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Toys;
            this.indexInList = 13;
            this.Name = "Toys"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";
        }

        public Toys()
        {
        }

        public override void CreateInterestJournalInfo()
        {
        }
        public override Interest Clone()
        {
            Toys animals = base.Clone() as Toys;
            return animals;
        }
    }
}
