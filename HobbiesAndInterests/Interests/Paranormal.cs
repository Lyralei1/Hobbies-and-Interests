using Lyralei;
using Sims3.Gameplay.ActorSystems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Lyralei.InterestMod
{
    public class Paranormal : Interest
    {
        public Paranormal(ulong simDescription)
            : base(InterestTypes.Paranormal, simDescription)
        {
            this.mInterestsGuid = InterestTypes.Paranormal;
            this.indexInList = 9;
            this.Name = "Paranormal"; // Change to localisation
            this.Description = "The dead, the space invaders, or anything that’s considered ‘spooky and out of the ordinary’ is what these sims love the most! You usually find them loving to be among occults or learning more about them. If not, magic is something they’re quite into!";
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

            DIYChef hobby1 = new DIYChef();

            this.mHobbies = new List<Hobby>() {
                                                                hobby1,
                                                          };

        }

        public Paranormal()
        {
        }

        public override void CreateInterestJournalInfo()
        {
        }

        public class DIYChef : Hobby
        {
        }

        public override Interest Clone()
        {
            Paranormal animals = base.Clone() as Paranormal;
            return animals;
        }
    }
}
