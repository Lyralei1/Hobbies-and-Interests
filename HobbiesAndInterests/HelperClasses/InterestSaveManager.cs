using Battery;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CustomContent;
using Sims3.UI;
using Sims3.UI.GameEntry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Lyralei.InterestMod
{
    //[System.Serializable]
    //public class InterestSaveData
    //{
    //    //numSims - number of sims in this save file
    //    //  simId - sim id
    //    //  numHobbies - number of hobbes that this sim has
    //    //    hobbyGuid
    //    //  numInterests - number of interests that this sim has
    //    //    interestGuid
    //    //    interestLevel
    //    //    interestXP
    //    //    simId
    //    //knownSimsInterests

    //    //sim A - lists all sims in neighbourhood that sim A has met
    //    //  knownSimsInterests
    //    //    simId: List<interestGuid>();

    //    //sim B

    //    public string saveName;

    //    // Used for making a forloop
    //    public int amountOfSimOwners;
    //    public ulong[] simId;

    //    // Used to make a forloop for hobbies
    //    public int mNumInterests;

    //    public struct InterestData
    //    {
    //        public ulong[] interestGuid;
    //        public int interestLevel;
    //        public int interestXP;
    //    }

    //    // Used to make a forloop for hobbies
    //    public int numHobbies;
    //    public int HobbyID;

    //    // Used for known sim interests
    //    public int KnownSimsInterest;

    //}


    public class InterestSaveManager
    {

        public static void WriteInterestData()
        {
            if (InterestManager.mSavedSimInterests == null || InterestManager.mSavedSimInterests.Count == 0)
            {
                return;
            }

            try
            {
                //ProgressDialog.Show("Saving Interests...");

                MemoryStream memorystream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(memorystream);

                int numSims = InterestManager.mSavedSimInterests.Count;
                bw.Write(numSims);

                foreach (KeyValuePair<ulong, List<Interest>> kpv in InterestManager.mSavedSimInterests)
                {
                    if (kpv.value == null || kpv.key == 0) { continue; }

                    bw.Write(kpv.Key); // simID
                    bw.Write(kpv.Value.Count); // Amount of interests

                    foreach (Interest interest in kpv.Value)
                    {
                        if (interest == null)
                        {
                            continue;
                        }

                        bw.Write((ulong)interest.Guid);

                        bw.Write(interest.currInterestPoints);
                        bw.Write(interest.mPointsBeforeAddingInterestPoint);

                        if(interest.mSimKnownWithPassionateInterests == null)
                        {
                            bw.Write(0);
                        }
                        else
                        {
                            bw.Write(interest.mSimKnownWithPassionateInterests.Count);
                            if (interest.mSimKnownWithPassionateInterests.Count > 0)
                            {
                                //InterestManager.print("Found passionate sims " + interest.mSimKnownWithPassionateInterests.Count);
                                foreach (KeyValuePair<ulong, InterestTypes> knownPassionate in interest.mSimKnownWithPassionateInterests)
                                {
                                    // Sim ID of known sim
                                    bw.Write(knownPassionate.Key);

                                    // InterestType of passionate.
                                    bw.Write((ulong)knownPassionate.Value);
                                }
                            }
                        }

                        if (interest.mSimKnownWithHateInterests == null)
                        {
                            bw.Write(0);
                        }
                        else
                        {
                            bw.Write(interest.mSimKnownWithHateInterests.Count);
                            if (interest.mSimKnownWithHateInterests.Count > 0)
                            {
                                //InterestManager.print("Found hate sims " + interest.mSimKnownWithHateInterests.Count);
                                foreach (KeyValuePair<ulong, InterestTypes> knownHate in interest.mSimKnownWithHateInterests)
                                {
                                    // Sim ID of known sim
                                    bw.Write(knownHate.Key);

                                    // InterestType of Hate.
                                    bw.Write((ulong)knownHate.Value);
                                }
                            }
                        }
                    }
                }

                bw.Close();

                byte[] bytes = memorystream.ToArray();

                InterestManager.print("MemorySteam bytes: " + bytes.Length.ToString());
                var str = BitConverter.ToString(bytes);
                var mSaveFileName = "";

                if (GameStates.SaveGameMetadata == null)
                {
                     mSaveFileName = "LyraleiInterestsAndHobbiesPlaceholder_" + GameStates.GetCurrentWorldName(false);
                }
                else
                {
                     mSaveFileName = GameStates.SaveGameMetadata.mSaveFile;

                }

                if (GlobalOptionsHobbiesAndInterests.retrieveData == null)
                {
                    return;
                }

                // If there is already a save, override the old data with the new data.
                if (GlobalOptionsHobbiesAndInterests.retrieveData.mInterestSaveData.ContainsKey(mSaveFileName))
                {
                    GlobalOptionsHobbiesAndInterests.retrieveData.mInterestSaveData[mSaveFileName] = str;
                }
                else
                {
                    GlobalOptionsHobbiesAndInterests.retrieveData.mInterestSaveData.Add(mSaveFileName, str);
                }
                //ProgressDialog.Close();
                InterestManager.print("Done!");
            }
            catch (Exception ex)
            {
                InterestManager.print(ex.ToString());
            }
        }

        public static void SaveInterestData()
        {
            // If it's null, it's a new save and we should trigger the setup code instead
            if(InterestManager.mSavedSimInterests == null || InterestManager.mSavedSimInterests.Count == 0)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            try
            {
                uint num = 0u;
                string s = Simulator.CreateExportFile(ref num, "InterestAndHobbies_savedData_BACKUP" + GameStates.SaveGameMetadata.mSaveFile);

                if (num != 0)
                {
                    
                    //ProgressDialog.Show("Saving Interest & Hobbies Data... One Sec!", false);
                    //FastStreamWriter writer = new FastStreamWriter(num);
                    CustomXmlWriter customXmlWriter = new CustomXmlWriter(num);

                    MemoryStream memorystream = new MemoryStream();
                    BinaryWriter bw = new BinaryWriter(memorystream);

                    int numSims = InterestManager.mSavedSimInterests.Count;
                    //InterestManager.print(numSims.ToString());
                    
                    bw.Write(numSims);


                    foreach (KeyValuePair<ulong, List<Interest>> kpv in InterestManager.mSavedSimInterests)
                    {
                        bw.Write(kpv.Key); // simID
                        bw.Write(kpv.Value.Count); // Amount of interests

                        //if (count == 0)
                        //{
                        //    InterestManager.print("simID: " + kpv.Key.ToString());
                        //}

                        foreach (Interest interest in kpv.Value)
                        {
                            if(interest == null)
                            {
                                continue;
                            }

                            bw.Write((ulong)interest.Guid);

                            bw.Write(interest.currInterestPoints);
                            bw.Write(interest.mPointsBeforeAddingInterestPoint);

                            //if (interest.hobbies == null)
                            //{
                            //    continue;
                            //}
                            //else
                            //{
                            //    // Seems to be null all the time?
                            //    bw.Write(interest.hobbies.Count);

                            //    foreach (Interest.Hobby hobby in interest.hobbies)
                            //    {
                            //        if (hobby == null)
                            //        {
                            //            continue;
                            //        }
                            //        bw.Write(hobby.mId);
                            //        bw.Write(hobby.mIsMasterInHobby);

                            //        // TODO: Store saved count points
                            //    }
                            //}

                            bw.Write(interest.mSimKnownWithPassionateInterests.Count);

                            if (interest.mSimKnownWithPassionateInterests.Count > 0)
                            {
                                //InterestManager.print("Found passionate sims " + interest.mSimKnownWithPassionateInterests.Count);
                                foreach (KeyValuePair<ulong, InterestTypes> knownPassionate in interest.mSimKnownWithPassionateInterests)
                                {
                                    // Sim ID of known sim
                                    bw.Write(knownPassionate.Key);

                                    // InterestType of passionate.
                                    bw.Write((ulong)knownPassionate.Value);
                                }
                            }
                            bw.Write(interest.mSimKnownWithHateInterests.Count);

                            if (interest.mSimKnownWithHateInterests.Count > 0)
                            {
                                InterestManager.print("Found hate sims " + interest.mSimKnownWithHateInterests.Count);
                                foreach (KeyValuePair<ulong, InterestTypes> knownHate in interest.mSimKnownWithHateInterests)
                                {
                                    // Sim ID of known sim
                                    bw.Write(knownHate.Key);

                                    // InterestType of Hate.
                                    bw.Write((ulong)knownHate.Value);
                                }
                            }
                        }
                    }
                    //InterestManager.print("Wrote " + count.ToString() + " savedSimInterests");

                    bw.Close();

                    byte[] bytes = memorystream.ToArray();

                    InterestManager.print("MemorySteam bytes: " + bytes.Length.ToString());
                    //InterestManager.print("Expected " + ((count * 4) + 4).ToString() + " bytes");

                    //var str = System.Text.Encoding.UTF8.GetString(bytes);
                    var str = BitConverter.ToString(bytes);
                    var mWorldName = GameStates.SaveGameMetadata.mSaveFile;



                    // If there is already a save, override the old data with the new data.
                    if (GlobalOptionsHobbiesAndInterests.retrieveData != null && GlobalOptionsHobbiesAndInterests.retrieveData.mInterestSaveData.ContainsKey(mWorldName))
                    {
                        GlobalOptionsHobbiesAndInterests.retrieveData.mInterestSaveData[mWorldName] = str;
                    }
                    else
                    {
                        GlobalOptionsHobbiesAndInterests.retrieveData.mInterestSaveData.Add(mWorldName, str);
                    }

                    //ProgressDialog.Close();

                    //InterestManager.print(str);

                    //InterestManager.print(str.Length.ToString());

                    //customXmlWriter.WriteStartDocument();
                    //customXmlWriter.WriteElementString("saveData", str);
                    customXmlWriter.WriteToBuffer(str);
                    customXmlWriter.WriteEndDocument();

                    InterestManager.print("Done!");

                    //foreach(ulong id in InterestManager.mSavedSimInterests.Keys)
                    //{
                    //    bw.Write(id);
                    //    bw.Write(InterestManager.mSavedSimInterests[id].Count);

                    //    foreach (Interest interest in InterestManager.mSavedSimInterests[id])
                    //    {

                    //        bw.Write((ulong)interest.Guid);

                    //        bw.Write(interest.currInterestPoints);
                    //        bw.Write(interest.mPointsBeforeAddingInterestPoint);


                    //        //bw.Write(interest.hobbies.Count);


                    //        //    foreach(Interest.Hobby hobby in interest.hobbies)
                    //        //    {
                    //        //        bw.Write(hobby.mId);
                    //        //        bw.Write(hobby.mIsMasterInHobby);

                    //        //        // TODO: Store saved count points

                    //        //    }

                    //        foreach (KeyValuePair<ulong, InterestTypes> kpv in interest.mSimKnownWithPassionateInterests)
                    //        {
                    //            // Sim ID of known sim
                    //            bw.Write(kpv.Key);

                    //            // InterestType of passionate.
                    //            bw.Write((ulong)kpv.Value);
                    //        }

                    //        foreach (KeyValuePair<ulong, InterestTypes> kpv in interest.mSimKnownWithHateInterests)
                    //        {
                    //            // Sim ID of known sim
                    //            bw.Write(kpv.Key);

                    //            // InterestType of Hate.
                    //            bw.Write((ulong)kpv.Value);
                    //        }
                    //    }
                    //}
                    //bw.Close();
                }


            }
            catch (Exception ex)
            {
                ProgressDialog.Close();
                WriteErrorXMLFile("Save data error", ex, null);
            }


            //GetFirstPackageId(ref PackageId packageId)
        }

        public static void ExtractInterestData()
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                string currentWorld = "";
                if (GameStates.SaveGameMetadata == null)
                {
                    currentWorld = "LyraleiInterestsAndHobbiesPlaceholder_" + GameStates.GetCurrentWorldName(false);
                }
                else
                {
                    currentWorld = GameStates.SaveGameMetadata.mSaveFile;

                }

                if (GlobalOptionsHobbiesAndInterests.retrieveData.mInterestSaveData != null && GlobalOptionsHobbiesAndInterests.retrieveData.mInterestSaveData.ContainsKey(currentWorld))
                {

                    // Convert string to a usable byte to read.
                    string str = GlobalOptionsHobbiesAndInterests.retrieveData.mInterestSaveData[currentWorld];

                    InterestManager.print(str.Length.ToString());

                    byte[] data = FromHex(str);

                    InterestManager.print("data len: " + data.Length);

                    MemoryStream input = new MemoryStream(data);
                    BinaryReader reader = new BinaryReader(input);


                    int numSim = reader.ReadInt32();
                    InterestManager.print("NumSim: " + numSim.ToString());


                    for (int i = 0; i < numSim; i++)
                    {
                        sb.AppendLine("#" + i.ToString());
                        ulong simId = reader.ReadUInt64();

                        sb.AppendLine("simID: " + simId.ToString());
                        SimDescription findOwner = SimDescription.Find(simId);

                        if(findOwner == null)
                        {
                            sb.AppendLine("simID didn't exist... skipping it now ");
                        }

                        int amountInterests = reader.ReadInt32();
                        sb.AppendLine("  amountInterests: " + amountInterests.ToString());

                        List<Interest> interests = GlobalOptionsHobbiesAndInterests.PrepareInterestListForSim(simId);


                        for (int a = 0; a < amountInterests; a++)
                        {
                            ulong interestGUID = reader.ReadUInt64();
                            InterestTypes typeVer = (InterestTypes)interestGUID;

                            sb.AppendLine("    interestGUID: " + interestGUID.ToString() + " - " + typeVer.ToString());

                            interests[a].mInterestsGuid = (InterestTypes)interestGUID;

                            int level = reader.ReadInt32();
                            sb.AppendLine("    level: " + level.ToString());

                            interests[a].currInterestPoints = level;

                            float xp = reader.ReadSingle();
                            sb.AppendLine("    xp: " + xp.ToString());

                            interests[a].mPointsBeforeAddingInterestPoint = xp;

                            int passionateSims = reader.ReadInt32();
                            sb.AppendLine("    passionateSims: " + passionateSims.ToString());


                            if (passionateSims > 0)
                            {
                                for (int p = 0; p < passionateSims; p++)
                                {
                                    ulong simIDLoveNeighbor = reader.ReadUInt64();
                                    sb.AppendLine("      simIDLoveNeighbor: " + simIDLoveNeighbor.ToString());

                                    ulong interestLoveGUIDNeighbor = reader.ReadUInt64();
                                    sb.AppendLine("      interestLoveGUIDNeighbor: " + interestLoveGUIDNeighbor.ToString());

                                    interests[a].mSimKnownWithPassionateInterests.Add(simIDLoveNeighbor, (InterestTypes)interestLoveGUIDNeighbor);
                                }
                            }
                            else
                            {
                                interests[a].mSimKnownWithPassionateInterests = new Dictionary<ulong, InterestTypes>();
                            }

                            int hateSims = reader.ReadInt32();
                            sb.AppendLine("    hateSims: " + hateSims.ToString());

                            if (hateSims > 0)
                            {
                                for (int h = 0; h < hateSims; h++)
                                {
                                    ulong simIDHateNeighbor = reader.ReadUInt64();
                                    sb.AppendLine("      simIDHateNeighbor: " + simIDHateNeighbor.ToString());

                                    ulong interestHateGUIDNeighbor = reader.ReadUInt64();
                                    sb.AppendLine("      interestHateGUIDNeighbor: " + interestHateGUIDNeighbor.ToString());

                                    interests[a].mSimKnownWithHateInterests.Add(simIDHateNeighbor, (InterestTypes)interestHateGUIDNeighbor);
                                }
                            }
                            else
                            {
                                interests[a].mSimKnownWithHateInterests = new Dictionary<ulong, InterestTypes>();
                            }
                        }
                        sb.AppendLine("--------------");

                        InterestManager.mSavedSimInterests.Add(simId, interests);
                    }
                    WriteErrorXMLFile("Extracted", null, sb.ToString());

                    InterestManager.print(numSim.ToString());
                }
            }
            catch (Exception ex)
            {
                WriteErrorXMLFile("Extract data error", ex, sb.ToString());
            }
        }

        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        //public class container
        //{
        //    public Dictionary<ulong, List<Interest>> data = new Dictionary<ulong, List<Interest>>();
        //}

        //public static string savedData;

        //// We return a bool to check if the saving was successful.
        //public static bool SaveUserData()
        //{
        //    try
        //    {
        //        container ct = new container();
        //        ct.data = InterestManager.mSavedSimInterests;

        //        savedData = ct.ToExportString();

        //        uint num = 0u;
        //        string s = Simulator.CreateExportFile(ref num, "Save data Serialised");

        //        if (num != 0)
        //        {
        //            CustomXmlWriter customXmlWriter = new CustomXmlWriter(num);
        //            customXmlWriter.WriteToBuffer(savedData.ToString());
        //            customXmlWriter.WriteEndDocument();
        //        }
        //        GlobalOptionsHobbiesAndInterests.print("Save data was successfully written.");
        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        WriteErrorXMLFile("Save Data - ERROR", ex);
        //        return false;
        //    }
        //}

        //public static bool ExtractSavedData()
        //{
        //    try
        //    {
        //        uint num = 0u;
        //        string s = Simulator.CreateExportFile(ref num, "Saved User Data - InterestMod");

        //        if (num != 0)
        //        {
        //            CustomXmlWriter customXmlWriter = new CustomXmlWriter(num);

        //            customXmlWriter.WriteToBuffer("--- " + GameUtils.GetCurrentWorld().ToString() + " ---");
        //            customXmlWriter.WriteToBuffer(System.Environment.NewLine);

        //            container mReserialisedSavedData = Export_Import.Deserialize<container>(savedData);

        //            customXmlWriter.WriteToBuffer(savedData);

        //            //Dictionary<ulong, List<Interest>> mReserialisedSavedData = Export_Import.Deserialize<Dictionary<ulong, List<Interest>>>(savedData);

        //            //foreach (KeyValuePair<ulong, List<Interest>> kvp in mReserialisedSavedData.data)
        //            //{
        //            //    customXmlWriter.WriteToBuffer(System.Environment.NewLine);
        //            //    customXmlWriter.WriteToBuffer("ID: " + kvp.Key.ToString());
        //            //    customXmlWriter.WriteToBuffer(System.Environment.NewLine);

        //            //    foreach (Interest interest in kvp.Value)
        //            //    {
        //            //        customXmlWriter.WriteToBuffer("Name: " + interest.Name);
        //            //        customXmlWriter.WriteToBuffer(System.Environment.NewLine);
        //            //        customXmlWriter.WriteToBuffer("Description: " + interest.Description);
        //            //        customXmlWriter.WriteToBuffer(System.Environment.NewLine);
        //            //        customXmlWriter.WriteToBuffer("CurrInterestPoints: " + interest.currInterestPoints.ToString());
        //            //        customXmlWriter.WriteToBuffer(System.Environment.NewLine);
        //            //        customXmlWriter.WriteToBuffer("GUID: " + interest.Guid.ToString());
        //            //        customXmlWriter.WriteToBuffer(System.Environment.NewLine);
        //            //        customXmlWriter.WriteToBuffer("Hobby count: " + interest.hobbies.Count.ToString());
        //            //        customXmlWriter.WriteToBuffer(System.Environment.NewLine);
        //            //        customXmlWriter.WriteToBuffer("Interest Owner: " + interest.InterestOwner.ToString());
        //            //        customXmlWriter.WriteToBuffer(System.Environment.NewLine);
        //            //        customXmlWriter.WriteToBuffer("mPointsBeforeAddingInterestPoint: " + interest.mPointsBeforeAddingInterestPoint.ToString());
        //            //        customXmlWriter.WriteToBuffer(System.Environment.NewLine);
        //            //        customXmlWriter.WriteToBuffer("traitBoost Count: " + interest.traitBoost.Count.ToString());
        //            //        customXmlWriter.WriteToBuffer(System.Environment.NewLine);
        //            //        customXmlWriter.WriteToBuffer("traitPenaltiy Count: " + interest.traitPenalty.Count.ToString());
        //            //        customXmlWriter.WriteToBuffer(System.Environment.NewLine);

        //            //    }
        //            //    customXmlWriter.WriteToBuffer(System.Environment.NewLine);
        //            //}
        //            customXmlWriter.WriteEndDocument();

        //            StyledNotification.Format format = new StyledNotification.Format("Visible sim export written to '" + s + "'.", StyledNotification.NotificationStyle.kGameMessageNegative);
        //            StyledNotification.Show(format);
        //            return true;
        //        }
        //        else
        //        {
        //            StyledNotification.Format format = new StyledNotification.Format("Num ref was 0", StyledNotification.NotificationStyle.kGameMessageNegative);
        //            StyledNotification.Show(format);
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteErrorXMLFile("Extract Save Data - ERROR", ex);
        //        return false;
        //    }
        //}

        public static void WriteErrorXMLFile(string fileName, Exception errorToPrint, string additionalinfo)
        {
            uint num = 0u;
            string s = Simulator.CreateExportFile(ref num, fileName);

            if (num != 0)
            {
                CustomXmlWriter customXmlWriter = new CustomXmlWriter(num);

                if(!String.IsNullOrEmpty(additionalinfo))
                {
                    customXmlWriter.WriteToBuffer(additionalinfo);
                }
                if(errorToPrint != null)
                {
                    customXmlWriter.WriteToBuffer(errorToPrint.ToString());
                }
                customXmlWriter.WriteEndDocument();
            }
        }
    }
}
