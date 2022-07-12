using Lyralei;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Scifi : Interest
    {
        public Scifi(ulong simDescription)
            : base(InterestTypes.Scifi, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Scifi;
            this.indexInList = 11;
            this.Name = "Scifi"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";
        }

        public Scifi()
        {
        }

        public override void CreateInterestJournalInfo()
        {
        }
        public override Interest Clone()
        {
            Scifi animals = base.Clone() as Scifi;
            return animals;
        }
    }
}
