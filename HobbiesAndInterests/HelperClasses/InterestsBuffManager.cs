using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;

namespace HobbiesAndInterests.Buffmanager
{
    public class InterestsBuffManager
    {
        public void LoadBuffData()
        {
            AddBuffs(null);
            UIManager.NewHotInstallStoreBuffData += new UIManager.NewHotInstallStoreBuffCallback(AddBuffs);
            //UIManager.NewHotInstallStoreBuffData = (UIManager.NewHotInstallStoreBuffCallback)Delegate.Combine(UIManager.NewHotInstallStoreBuffData, new UIManager.NewHotInstallStoreBuffCallback(AddBuffs));
        }


        public void AddBuffs(ResourceKey[] resourceKeys)
        {
            ResourceKey key = new ResourceKey(ResourceUtils.HashString64("buffs_Hobbies_and_interests"), 0x0333406C, 0x0);
            XmlDbData data = XmlDbData.ReadData(key, false);
            if (data != null)
            {
                BuffManager.ParseBuffData(data, true);
            }
        }
    }

    public class WorryAboutClimateChange : Buff
    {
        private const ulong kBuffworryAboutClimateChangeGuid = 0xA74C038A60FBF398;

        public static ulong StaticGuid
        {
            get
            {
                return 0xA74C038A60FBF398;
            }
        }

        public class BuffWorryAboutClimateChange : BuffInstance
        {
            //public AlarmHandle PheumoniaAlarm = AlarmHandle.kInvalidHandle;
            //public AlarmHandle PheumoniaAlarmKill = AlarmHandle.kInvalidHandle;
            //public AlarmHandle PheumoniaAnimationAlarm = AlarmHandle.kInvalidHandle;

            public SimDescription mOwningSim;

            public BuffWorryAboutClimateChange()
            {
            }

            public BuffWorryAboutClimateChange(Buff buff, BuffNames buffGuid, int effectValue, float timeoutCount)
                : base(buff, buffGuid, effectValue, timeoutCount)
            {
            }

            public override BuffInstance Clone()
            {
                return new BuffWorryAboutClimateChange(base.mBuff, base.mBuffGuid, base.mEffectValue, base.mTimeoutCount);
            }
        }

        public WorryAboutClimateChange(BuffData data) : base(data)
        {
        }

        public override BuffInstance CreateBuffInstance()
        {
            return new BuffWorryAboutClimateChange(this, base.BuffGuid, base.EffectValue, base.TimeoutSimMinutes);
        }

        public override bool ShouldAdd(BuffManager bm, MoodAxis axisEffected, int moodValue)
        {       
            if (InterestManager.HasTheNecessaryInterest(bm.Actor.SimDescription, InterestTypes.Environment, true) && !bm.Actor.SimDescription.ChildOrBelow)
            {
                return base.ShouldAdd(bm, axisEffected, moodValue);
            }
            return false;
        }

        //public override void OnAddition(BuffManager bm, BuffInstance bi, bool travelReaddition)
        //{
        //    Sim actor = bm.Actor;
        //    if (actor != null)
        //    {
        //        BuffWorryAboutClimateChange buffInstancePheumonia = bi as BuffWorryAboutClimateChange;
        //        buffInstancePheumonia.mOwningSim = actor.SimDescription;
        //        //bm.Actor.BuffManager.ForceAddBuff(BuffNames.Germy, Origin.FromUncapturedMinorPet);

        //        //if (RandomizerDiseaseFunctions.kShouldAddGermyForCallingWork)
        //        //{
        //        //    bm.Actor.BuffManager.ForceAddBuff(BuffNames.Germy, Origin.FromUnknown);
        //        //}

        //        buffInstancePheumonia.PheumoniaAlarm = AlarmHandle.kInvalidHandle;
        //        buffInstancePheumonia.PheumoniaAlarm = AlarmManager.Global.AddAlarm(3f, TimeUnit.Hours, buffInstancePheumonia.PheumoniaCallback, "Pheumonia Callback Alarm", AlarmType.AlwaysPersisted, actor);

        //        buffInstancePheumonia.PheumoniaAnimationAlarm = AlarmHandle.kInvalidHandle;
        //        buffInstancePheumonia.PheumoniaAnimationAlarm = AlarmManager.Global.AddAlarm(1f, TimeUnit.Hours, buffInstancePheumonia.PheumoniaAnimationCallback, "Pheumonia Anim Callback Alarm", AlarmType.AlwaysPersisted, actor);

        //    }
        //}

        //public override void OnRemoval(BuffManager bm, BuffInstance bi)
        //{
        //    Sim actor = bm.Actor;
        //    if (actor != null)
        //    {
        //        BuffWorryAboutClimateChange buffInstancePheumonia = bi as BuffWorryAboutClimateChange;
        //        buffInstancePheumonia.mOwningSim = null;

        //        AlarmManager.Global.RemoveAlarm(buffInstancePheumonia.PheumoniaAlarm);
        //        buffInstancePheumonia.PheumoniaAlarm = AlarmHandle.kInvalidHandle;
        //        AlarmManager.Global.RemoveAlarm(buffInstancePheumonia.PheumoniaAnimationAlarm);
        //        buffInstancePheumonia.PheumoniaAnimationAlarm = AlarmHandle.kInvalidHandle;
        //    }
        //}

        public override void OnTimeout(BuffManager bm, BuffInstance bi, OnTimeoutReasons reason)
        {
            OnRemoval(bm, bi);
        }
    }
}
