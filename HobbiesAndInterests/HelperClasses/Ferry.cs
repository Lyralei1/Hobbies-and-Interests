using Sims3.Gameplay;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Lyralei.InterestMod
{
    // CODE BY GAMEFREAK130: https://modthesims.info/showthread.php?p=5679760#post5679760

    public static class Ferry<T>
    {
        private static readonly Dictionary<FieldInfo, object> mCargo;

        static Ferry()
        {
            FieldInfo[] fields = FindPersistableStatics();
            if (fields.Length == 0)
            {
                throw new NotSupportedException($"There are no PersistableStatic fields declared in {typeof(T)}.");
            }
            mCargo = new Dictionary<FieldInfo, object>(fields.Length);
            foreach (FieldInfo current in fields)
            {
                mCargo[current] = null;
            }
        }

        private static FieldInfo[] FindPersistableStatics()
        {
            MemberInfo[] fieldMembers = typeof(T).FindMembers(MemberTypes.Field,
                BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic,
                (info, criteria) => info.GetCustomAttributes(typeof(PersistableStaticAttribute), false).Length > 0, null);

            return Array.ConvertAll(fieldMembers, (x) => (FieldInfo)x);
        }

        // Transfer loaded Cargo to respective members of T
        public static void UnloadCargo()
        {
            if (GameStates.IsTravelling)
            {
                foreach (FieldInfo current in new List<FieldInfo>(mCargo.Keys))
                {
                    current.SetValue(null, mCargo[current]);
                    mCargo[current] = null;
                }
            }
        }

        // Load all Cargo onto T's Ferry for transfer
        public static void LoadCargo()
        {
            if (GameStates.IsTravelling)
            {
                foreach (FieldInfo current in new List<FieldInfo>(mCargo.Keys))
                {
                    mCargo[current] = current.GetValue(null);
                }
            }
        }
    }
}
