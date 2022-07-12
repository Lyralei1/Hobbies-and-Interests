using Lyralei;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Culture : Interest
    {
        public Culture( ulong simDescription)
            : base(InterestTypes.Culture, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Culture;
            this.indexInList = 2;
            this.Name = "Culture"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";
        }

        public Culture()
        {
        }

        public override void CreateInterestJournalInfo()
        {
        }
        public override Interest Clone()
        {
            Culture animals = base.Clone() as Culture;
            return animals;
        }
    }
}
