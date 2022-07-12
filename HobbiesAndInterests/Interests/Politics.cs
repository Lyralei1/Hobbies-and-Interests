using Lyralei;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Politics : Interest
    {
        public Politics( ulong simDescription)
            : base(InterestTypes.Politics, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Politics;
            this.indexInList = 10;
            this.Name = "Politics"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";

        }

        public Politics()
        {
        }

        public override void CreateInterestJournalInfo()
        {

        }
        public override Interest Clone()
        {
            Politics animals = base.Clone() as Politics;
            return animals;
        }
    }
}
