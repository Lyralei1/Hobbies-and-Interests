using Lyralei;
using Lyralei.InterestMod;
using ScriptCore;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Objects.Environment;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Objects.Lyralei
{

    public class LyraleiSolarPanel : SolarPanel
    {
        public override void OnStartup()
        {
            //sBoughtObjectLister = EventTracker.AddListener(EventTypeId.kBoughtObject, new ProcessEventDelegate(OnObjectBought));
            base.OnStartup();
        }

        public override void OnHandToolPlacement()
        {
            if (base.IsOnRoof)
            {
                lot = base.LotCurrent;
                if (Sims3.SimIFace.World.HasAnyRoof(lot.LotId))
                {
                    uint result = Sims3.SimIFace.World.GetRoofSlopeAngle();

                    double radians = (double)result * Math.PI / 180;

                    float cos = (float)Math.Cos(radians);
                    float sin = (float)Math.Sin(radians);

                    Matrix44 transform = new Matrix44(
                                            base.Transform.right,
                                            new Vector4(0f, cos, -sin, 0f),
                                            new Vector4(0f, sin, cos, 0f),
                                            new Vector4(0f, 0f, 0f, 0f)
                                            );

                    //InterestManager.print("Base " + base.Transform.right.ToString() + System.Environment.NewLine + base.Transform.up.ToString() + System.Environment.NewLine + base.Transform.at.ToString() + System.Environment.NewLine + base.Transform.pos.ToString());
                    //InterestManager.print("Custom " + transform.right.ToString() + System.Environment.NewLine + transform.up.ToString() + System.Environment.NewLine + transform.at.ToString() + System.Environment.NewLine + transform.pos.ToString());

                    //base.Transform = transform * base.Transform;

                    //InterestManager.print("Base: " + base.ForwardVector.ToString());

                    //transform.rotation * Vector3.forward


                    // only use the Y component of the objects orientation
                    // always returns a value between 0 and 360
                    float myHeading = CommonHelpers.GetTransformRotation(base.Position, Vector3.UnitY).y;
                    // also this is always a value between 0 and 360
                    float northHeading = CommonHelpers.GetTransformRotation(Terrain.sTerrain.Position, Vector3.UnitY).y;

                    float dif = myHeading - northHeading;
                    // wrap the value so it is always between 0 and 360
                    if (dif < 0) dif += 360f;

                    InterestManager.print(dif.ToString());

                    if (dif > 45 && dif <= 134)
                    {
                        InterestManager.print("East");
                    }
                    else if (dif > 135 && dif <= 225)
                    {
                        InterestManager.print("South");
                    }
                    else if (dif > 225 && dif <= 315)
                    {
                        InterestManager.print("West");
                    }
                    else
                    {
                        InterestManager.print("North");
                    }

                    Vector3 forward = CommonHelpers.GetTransformRotation(base.ForwardVector, Vector3.UnitY);


                    Vector3 v = forward;
                    v.y = 0;
                    v.Normalize();

                    //StringBuilder stb = new StringBuilder();

                    //stb.AppendLine("unitZ " + CommonHelpers.Angle(v, Vector3.UnitZ).ToString());
                    //stb.AppendLine("unitx " + CommonHelpers.Angle(v, Vector3.UnitX).ToString());
                    //stb.AppendLine("unitY " + CommonHelpers.Angle(v, new Vector3(0, 0, -1)).ToString());

                    //InterestManager.print(stb.ToString());

                    //if (CommonHelpers.Angle(v, Vector3.UnitZ) <= 45.0)
                    //{
                    //    InterestManager.print("North");
                    //}
                    //else if (CommonHelpers.Angle(v, Vector3.UnitX) <= 45.0)
                    //{
                    //    InterestManager.print("East");
                    //}
                    //else if (CommonHelpers.Angle(v, new Vector3(0, 0, -1)) <= 45.0)
                    //{
                    //    InterestManager.print("South");
                    //}
                    //else
                    //{
                    //    InterestManager.print("West");
                    //}

                    Vector3 roofRotationValue = Quaternion.VRotate(Quaternion.MakeFromEulerAngles((float)result * (float)Math.PI / 180, 0f, 0f), base.ForwardVector);
                    base.SetForward(roofRotationValue);

                    //InterestManager.print("Base: " + base.ForwardVector.ToString());


                    //base.SetRotation(0.90f);

                    //if (result >= 45)
                    //{
                    //    float maths = result / 100f;

                    //    InterestManager.print(maths.ToString());

                    //    Vector3 roofRotationValue = Quaternion.VRotate(Quaternion.MakeFromEulerAngles((result / 100f) * 3.14159274f, 0f, 0f), base.ForwardVector);
                    //    base.SetForward(roofRotationValue);
                    //}
                    //else if (result <= 44)
                    //{
                    //    float maths = (float)result / 100;
                    //    InterestManager.print(maths.ToString());

                    //    Vector3 roofRotationValue = Quaternion.VRotate(Quaternion.MakeFromEulerAngles((result / 100f) * 3.14159274f, 0f, 0f), base.ForwardVector);
                    //    base.SetForward(roofRotationValue);
                    //}
                }
            }

            foreach (Sim sim in this.LotCurrent.Household.Sims)
            {
                Sims3.Gameplay.Lyralei.InterestMod.Environment env = InterestManager.GetInterestFromInterestType(InterestTypes.Environment, sim) as Sims3.Gameplay.Lyralei.InterestMod.Environment;
                if (env != null && !env.mHasInstalledSolarPanels)
                {
                    env.modifyInterestLevel(2, sim.SimDescription.SimDescriptionId, env.mInterestsGuid);
                    int updatedLevel = InterestManager.GetLevelForInterest(sim.SimDescription, InterestTypes.Environment);
                    env.mHasInstalledSolarPanels = true;
                }
                
            }
            
            base.OnHandToolPlacement();
        }


        public static Lot lot = null;


        public static LyraleiSolarPanel rotateWhenPlacedOnRoof(LyraleiSolarPanel obj)
        {
            //if (obj.IsOnRoof && obj != null)
            if (obj != null)
            {
                // lot = obj.LotCurrent;
                //if (Sims3.SimIFace.World.HasAnyRoof(lot.LotId))
                //{
                uint result = Sims3.SimIFace.World.GetRoofSlopeAngle();
                float comingResult = 0.05f * (float)result;
                float resAsFloat = (float)result;
                if (result >= 45)
                {
                    Vector3 roofRotationValue = Quaternion.VRotate(Quaternion.MakeFromEulerAngles((float)result / 100 + 0.5f, 0f, 0f), obj.ForwardVector);
                    obj.SetForward(roofRotationValue);
                }
                else if (result <= 44)
                {
                    Vector3 roofRotationValue = Quaternion.VRotate(Quaternion.MakeFromEulerAngles((float)result / 100, 0f, 0f), obj.ForwardVector);
                    obj.SetForward(roofRotationValue);
                }
                return obj;
                    // Vector3 roofRotationValue = Quaternion.VRotate(Quaternion.MakeFromEulerAngles((float)result / 100 + 0.5f, 0f, 0f), this.ForwardVector);
                    // base.SetForward(roofRotationValue);
                //}
            }
            return null;
        }
    }
}
