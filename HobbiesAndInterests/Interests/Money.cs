using Lyralei;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Money : Interest
    {
        public Money(ulong simDescription)
            : base(InterestTypes.Money, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Money;
            this.indexInList = 8;
            this.Name = "Money"; // Change to localisation
            this.Description = "Your sim has a big interest in helping the environment! They’ll want to try different approaches within the environment interest to help or enjoy the planet a little :) Whether that’s eating less/no meat, experience all aspect of nature, or living off-the-grid, that’s all up to your sim!";
        }

        public Money()
        {
        }

        public override void CreateInterestJournalInfo()
        {
        }
        public override Interest Clone()
        {
            Money animals = base.Clone() as Money;
            return animals;
        }
    }
}
