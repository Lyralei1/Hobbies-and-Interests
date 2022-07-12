using Lyralei;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Crime : Interest
    {
        public Crime( ulong simDescription)
            : base(InterestTypes.Crime, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Crime;
            this.indexInList = 1;
            this.Name = "Criminal"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";

        }

        public Crime()
        {
        }

        public override void CreateInterestJournalInfo()
        {
        }
        public override Interest Clone()
        {
            Crime animals = base.Clone() as Crime;
            return animals;
        }
    }
}
