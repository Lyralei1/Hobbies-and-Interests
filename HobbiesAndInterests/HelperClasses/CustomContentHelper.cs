using Lyralei.InterestMod;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Sims3.SimIFace.World;

namespace HobbiesAndInterests.HelperClasses
{
    public class CustomContentHelper
    {
        private static List<CustomContentItem> mItems = new List<CustomContentItem>();

        public static void LoadAllItemsFromXMLStartup()
        {
            XmlDbData xmlSupportedMods = XmlDbData.ReadData(new ResourceKey(0xFB8395AB64929453, 0x0333406C, 0x00000000), false);
            if (xmlSupportedMods != null)
            {
                InterestManager.print("Loading items...");
                if (!ReadContentData(xmlSupportedMods))
                {
                    InterestManager.print("An error occured while loading compatible CC. Error file has been made in Documents/The Sims 3. Make sure to give this to Lyralei. Interest & Hobbies mod will work fine though!");
                }
            }
        }



        private static bool ReadContentData(XmlDbData xmlDocument)
        {
            try
            {
                XmlDbTable xmlDbTable;

                if (xmlDocument != null && xmlDocument.Tables != null && xmlDocument.Tables.TryGetValue("ModOnBought", out xmlDbTable))
                {
                    InterestManager.print("Scanning...");

                    foreach (XmlDbRow row in xmlDbTable.Rows)
                    {
                        CustomContentItem mCustomContent = null;
                        InterestManager.print("Started on item...");

                        InterestTypes key;
                        if (!row.TryGetEnum("InterestType", out key, InterestTypes.None))
                            continue;

                        if (key == InterestTypes.None)
                            continue;

                        mCustomContent = new CustomContentItem(key);
                        mCustomContent.type = key;

                        mCustomContent.pointToAdd = row.GetFloat("Rarity");
                        mCustomContent.id = ResourceKey.FromString(row.GetString("OBJK"));
                        mCustomContent.isSubPoint = row.GetBool("CountPointsTowardsXP");

                        //if(!mItems.Contains(mCustomContent)) 
                        mItems.Add(mCustomContent);
                        InterestManager.print("Finished on item...");

                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                CommonHelpers.printException(ex);
                return false;
            }
        }


        public static void OnBuildBuy(object sender, EventArgs args)
        {
            World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs = args as World.OnObjectPlacedInLotEventArgs;

            if (onObjectPlacedInLotEventArgs == null)
                return;

            GameObject @object = GameObject.GetObject(onObjectPlacedInLotEventArgs.ObjectId);

            if (@object == null)
                return;

            if (mItems != null || mItems.Count != 0)
            {
                foreach (CustomContentItem cc in mItems)
                {
                    // If the CC item and our just bought item is the same...
                    if (@object.GetResourceKey().InstanceId == cc.id.InstanceId)
                    {
                        int items = (int)Sims3.SimIFace.Queries.CountObjects(@object.GetType(), Sim.ActiveActor.LotHome.LotId);

                        // Check if this is the very first item bought onto the lot...
                        if (items == 1)
                        {
                            cc.name = @object.CatalogName;
                            mCachedCCItems.Add(cc);

                            OneShotFunction mShowInvitationFunction = new OneShotFunction(HandleUIYielding);
                            Simulator.AddObject(mShowInvitationFunction);
                        }

                        break;
                    }
                }
            }
        }

        private static List<CustomContentItem> mCachedCCItems = new List<CustomContentItem>();

        public static void HandleUIYielding()
        {

            for(int i = 0; i < mCachedCCItems.Count; i++)
            {
                bool continueOn = TwoButtonDialog.Show("You've bought " + mCachedCCItems[i].name + " Would you like to spend a point into " + mCachedCCItems[i].type.ToString() + " for some of your sims?", "Yes", "No");

                if (continueOn)
                {

                    List<Sim> UserChosenSims = GlobalOptionsHobbiesAndInterests.SimPicker(Sim.ActiveActor.SimDescription, 1);

                    if (UserChosenSims != null && UserChosenSims.Count > 0)
                    {
                        foreach (Sim sim in UserChosenSims)
                        {
                            if (mCachedCCItems[i].isSubPoint)
                            {
                                InterestManager.AddSubPoints(mCachedCCItems[i].pointToAdd, sim.SimDescription, mCachedCCItems[i].type);
                            }
                            else
                            {
                                InterestManager.AddInterestPoints(mCachedCCItems[i].pointToAdd, sim.SimDescription, mCachedCCItems[i].type);
                            }
                        }
                    }
                }

            }

            // Clear cache as we've handled it all...
            mCachedCCItems.Clear();
        }

    }

    public class CustomContentItem 
    {
        public InterestTypes type = InterestTypes.None;
        public float pointToAdd = 0;
        public ResourceKey id;
        public bool isSubPoint = false;
        public Object ClassName = null;
        public string name;

        public CustomContentItem() { }
        public CustomContentItem(InterestTypes XMLtype)
        {
            type = XMLtype;
        }
    }
}
