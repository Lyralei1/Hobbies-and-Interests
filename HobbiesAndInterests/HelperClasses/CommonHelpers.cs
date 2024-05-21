using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Objects.Appliances;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.HobbiesSkills;
using Sims3.Gameplay.Objects.Lighting;
using Sims3.Gameplay.Objects.PerformanceObjects;
using Sims3.Gameplay.Objects.Register;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Lyralei.InterestMod
{
    public static class CommonHelpers
    {

        public static List<GameObject> GetAllElectronicObjectsOnLot(Lot lot)
        {
            GameObject[] mGameObjects = lot.GetObjects<GameObject>();
            List<GameObject> mElectronics = new List<GameObject>();

            string[] validNamespaces = new string[]
            {
                    "Sims3.Gameplay.Objects.Entertainment",
                    "Sims3.Gameplay.Objects.Electronics",
                    "Sims3.Gameplay.Objects.Appliances",
                    "Sims3.Gameplay.Objects.Lighting",
                    "Sims3.Gameplay.Objects.PerformanceObjects",
            };

            foreach (GameObject obj in mGameObjects)
            {
                if (obj.GetType() == typeof(PhoneHome) ||
                    obj.GetType() == typeof(DaylightLight) ||
                    obj.GetType() == typeof(MoodLamp) ||
                    obj.GetType() == typeof(LightInvisible) ||
                    obj.GetType() == typeof(Grill) ||
                    obj.GetType() == typeof(Clothesline) ||
                    obj.GetType() == typeof(FutureBar) ||
                    obj.GetType() == typeof(FutureFoodSynthesizer) ||
                    obj.GetType() == typeof(AlarmClockCheap) ||
                    obj.GetType() == typeof(VideoCamera) ||
                    obj.GetType() == typeof(VRGoggles) ||
                    obj.GetType() == typeof(CrowdMonster) ||
                    obj.GetType() == typeof(PerformanceTips) ||
                    obj.GetType() == typeof(ProprietorWaitingArea) ||
                    obj.GetType() == typeof(ShowFloor) ||
                    obj.GetType() == typeof(ShowStage)
                    )
                {
                    continue;
                }

                string mNamespace = obj.GetType().Namespace;

                if ((mNamespace == validNamespaces[0]) ||
                    (mNamespace == validNamespaces[1]) ||
                    (mNamespace == validNamespaces[2]) ||
                    (mNamespace == validNamespaces[3]) ||
                    (mNamespace == validNamespaces[4]) ||
                    obj.GetType() == typeof(ShoppingRegister) ||
                    obj.GetType() == typeof(AthleticGameObject) ||
                    obj.GetType() == typeof(NectarMaker)
                    )
                {
                    mElectronics.Add(obj);
                }
            }
            return mElectronics;

        }

        public static MethodInfo FindMethod(string methodName)
        {
            if (methodName.Contains(","))
            {
                string[] array = methodName.Split(',');
                string typeName = array[0] + "," + array[1];
                Type type = Type.GetType(typeName, true);
                string text = array[2];
                text = text.Replace(" ", "");

                if (type.GetMethod(text) == null)
                {
                    return null;
                }

                return type.GetMethod(text);
            }
            return null;
            //Type typeFromHandle = typeof(RandomizerFunctions);
            //return typeFromHandle.GetMethod(methodName);
        }

        // Gotten from https://stackoverflow.com/questions/1028136/random-entry-from-dictionary
        public static TValue GetRandomDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            Random rand = new Random();
            List<TKey> keyList = new List<TKey>(dict.Keys);

            TKey randomKey = keyList[RandomUtil.GetInt(keyList.Count + 1)];
            return dict[randomKey];
        }


        public static Dictionary<Key, Value> MergeInPlace<Key, Value>(Dictionary<Key, Value> left, Dictionary<Key, Value> right)
        {
            if (left == null)
            {
                InterestManager.print("Can't merge into a null dictionary");
            }
            else if (right == null)
            {
                return left;
            }

            foreach (var kvp in right)
            {
                if (!left.ContainsKey(kvp.Key))
                {
                    left.Add(kvp.Key, kvp.Value);
                }
            }

            return left;
        }

        public static bool ReplaceRHSThatHasNoProcedularMethod<CLASSNAME>(string action, string newFunction, bool isAfterUpdate)
        {
            MethodInfo method2 = typeof(InterestSocialCallback).GetMethod(newFunction);
            if (method2 == null)
            {
                InterestManager.print(newFunction + ": New Not Found");
            }
            List<SocialRuleRHS> list2 = SocialRuleRHS.Get(action);
            if (list2 == null)
            {
                InterestManager.print(action + ": Action Not Found");
                return false;
            }
            bool flag = false;
            foreach (SocialRuleRHS item in list2)
            {
                if (item.ProceduralEffectBeforeUpdate == null && !isAfterUpdate)
                {
                    item.mProceduralEffectBeforeUpdate = method2;
                    flag = true;
                }
                if (item.ProceduralEffectAfterUpdate == null && isAfterUpdate)
                {
                    item.mProceduralEffectAfterUpdate = method2;
                    flag = true;
                }
            }
            return flag;
        }

        public static float DotProduct(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static float DotProduct(Vector4 a, Vector4 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        public static float ComputeAngle(Vector3 vec1, Vector3 vec2)
        {
            vec1.Normalize();
            vec2.Normalize();
            float num = DotProduct(vec1, vec2);
            if (num > 1f)
            {
                return 0f;
            }
            if (num < -1f)
            {
                return 3.14159274f;
            }
            return (float)System.Math.Acos((double)num);
        }
        public static float SqrMagnitude(Vector3 vector) { return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z; }
        public static float Dot(Vector3 lhs, Vector3 rhs) { return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z; }


        //public Vector3 forward { get { return rotation * Vector3.forward; } set { rotation = Quaternion.LookRotation(value); } }

        public static Vector3 GetTransformRotation(Vector3 forward, Vector3 up)
        {
            Quaternion rotation = QuaternionLookRotation(forward, up);
            return doTimes(rotation, Vector3.UnitZ); 

        }

        // Rotates the point /point/ with /rotation/.
        public static Vector3 doTimes(Quaternion rotation, Vector3 point)
        {
            Vector3 axis = Quaternion.GetAxis(rotation);
            
            float x = axis.x * 2f;
            float y = axis.y * 2f;
            float z = axis.z * 2f;
            float xx = axis.x * x;
            float yy = axis.y * y;
            float zz = axis.z * z;
            float xy = axis.x * y;
            float xz = axis.x * z;
            float yz = axis.y * z;
            float wx = rotation.n * x;
            float wy = rotation.n * y;
            float wz = rotation.n * z;

            Vector3 res;
            res.x = (1F - (yy + zz)) * point.x + (xy - wz) * point.y + (xz + wy) * point.z;
            res.y = (xy + wz) * point.x + (1F - (xx + zz)) * point.y + (yz - wx) * point.z;
            res.z = (xz - wy) * point.x + (yz + wx) * point.y + (1F - (xx + yy)) * point.z;
            return res;
        }

        private static Quaternion QuaternionLookRotation(Vector3 forward, Vector3 up)
        {
            forward.Normalize();

            Vector3 vector = forward.Normalize();

            Vector3 vector2 = Vector3.CrossProduct(up, vector).Normalize();
            Vector3 vector3 = Vector3.CrossProduct(vector, vector2);
            var m00 = vector2.x;
            var m01 = vector2.y;
            var m02 = vector2.z;
            var m10 = vector3.x;
            var m11 = vector3.y;
            var m12 = vector3.z;
            var m20 = vector.x;
            var m21 = vector.y;
            var m22 = vector.z;

            float x;
            float y;
            float z;
            float n;


            float num8 = (m00 + m11) + m22;
            
            if (num8 > 0f)
            {
                
                var num = (float)Math.Sqrt(num8 + 1f);
                 n = num * 0.5f;
                num = 0.5f / num;
                 x = (m12 - m21) * num;
                 y = (m20 - m02) * num;
                 z = (m01 - m10) * num;

                var quaternion0 = new Quaternion(x, y, z, n);

                return quaternion0;
            }
            if ((m00 >= m11) && (m00 >= m22))
            {
                var num7 = (float)Math.Sqrt(((1f + m00) - m11) - m22);
                var num4 = 0.5f / num7;
                 x = 0.5f * num7;
                 y = (m01 + m10) * num4;
                 z = (m02 + m20) * num4;
                 n = (m12 - m21) * num4;

                var quaternion1 = new Quaternion(x, y, z, n);

                return quaternion1;
            }
            if (m11 > m22)
            {
                var num6 = (float)Math.Sqrt(((1f + m11) - m00) - m22);
                var num3 = 0.5f / num6;
                 x = (m10 + m01) * num3;
                 y = 0.5f * num6;
                 z = (m21 + m12) * num3;
                 n = (m20 - m02) * num3;

                var quaternion2 = new Quaternion(x, y, z, n);


                return quaternion2;
            }
            var num5 = (float)Math.Sqrt(((1f + m22) - m00) - m11);
            var num2 = 0.5f / num5;
             x = (m20 + m02) * num2;
             y = (m21 + m12) * num2;
             z = 0.5f * num5;
             n = (m01 - m10) * num2;

            var quaternion3 = new Quaternion(x, y, z, n);

            return quaternion3;
        }

        public static float Angle(Vector3 from, Vector3 to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = (float)Math.Sqrt(SqrMagnitude(from) * SqrMagnitude(to));
            if (denominator < MathUtils.kEpsilon)
                return 0F;

            float dot = MathUtils.Clamp(Dot(from, to) / denominator, -1F, 1F);
            return ((float)Math.Acos(dot)) * 360 / ((float)Math.PI * 2);
        }

        public static void printException(Exception e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("An exception (" + e.GetType().Name + ") occurred.");
            sb.AppendLine("   Message:\n" + e.Message);
            sb.AppendLine("   Stack Trace:\n   " + e.StackTrace);
            Exception ie = e.InnerException;
            if (ie != null)
            {
                sb.AppendLine("   The Inner Exception:");
                sb.AppendLine("      Exception Name: " + ie.GetType().Name);
                sb.AppendLine("      Message: " + ie.Message + "\n");
                sb.AppendLine("      Stack Trace:\n   " + ie.StackTrace + "\n");
            }
            SimpleMessageDialog.Show("Lyralei's Interest & Hobbies [ERROR]:", sb.ToString());
        }
    }
}
