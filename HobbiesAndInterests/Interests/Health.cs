using Lyralei;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Health : Interest
    {
        public Health(ulong simDescription)
            : base(InterestTypes.Health, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Health;
            this.indexInList = 7;
            this.Name = "Health"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";

        }

        public Health()
        {
        }

        public override void CreateInterestJournalInfo()
        {
        }
        public override Interest Clone()
        {
            Health animals = base.Clone() as Health;
            return animals;
        }
    }
}
