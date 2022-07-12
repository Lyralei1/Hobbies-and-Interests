using Lyralei;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Sports : Interest
    {
        public Sports(ulong simDescription)
            : base(InterestTypes.Sports, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Sports;
            this.indexInList = 12;
            this.Name = "Sports"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";
        }

        public Sports()
        {
        }

        public override void CreateInterestJournalInfo()
        {
        }
        public override Interest Clone()
        {
            Sports animals = base.Clone() as Sports;
            return animals;
        }
    }
}
