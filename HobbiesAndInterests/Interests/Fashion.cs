using Lyralei;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Fashion : Interest
    {
        public Fashion(ulong simDescription)
            : base(InterestTypes.Fashion, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Fashion;
            this.indexInList = 5;
            this.Name = "Fashion"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";

        }

        public Fashion()
        {
        }
        public override void CreateInterestJournalInfo()
        {
        }
        public override Interest Clone()
        {
            Fashion animals = base.Clone() as Fashion;
            return animals;
        }
    }
}
