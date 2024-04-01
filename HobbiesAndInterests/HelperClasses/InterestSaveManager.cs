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

        // because we can't always guarantee that the sims that in the save that once were in the world, are still there when we save, we want to remove them.
        private static void CleanSavedSimsList()
        {
            List<ulong> mKeysToDelete = new List<ulong>();
            foreach (KeyValuePair<ulong, List<Interest>> kpv in InterestManager.mSavedSimInterests)
            {
                SimDescription foundSim = SimDescription.Find(kpv.Key);

                if(foundSim == null)
                {
                    mKeysToDelete.Add(kpv.Key);
                }

                // This usually means it's a townie that has left the world for the time being. We don't store this type of data because it's useless for service sims and homeless NPCs to have hobbies...
                if(foundSim.CreatedSim == null)
                {
                    mKeysToDelete.Add(kpv.Key);
                }

                if(!foundSim.CreatedSim.InWorld) { mKeysToDelete.Add(kpv.Key); }
            }
        }

        public static void ExportInterestData()
        {
            if (InterestManager.mSavedSimInterests == null || InterestManager.mSavedSimInterests.Count == 0 )
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
                    if (kpv.Value == null || kpv.Key == 0) { continue; }

                    bw.Write(kpv.Key); // simID
                    bw.Write(kpv.Value.Count); // Amount of interests

                    foreach (Interest interest in kpv.Value)
                    {
                        if (interest == null)
                        {
                            continue;
                        }

                        bw.Write((ulong)interest.Guid);
                        bw.Write(interest.mHasNotifiedPlayerAboutSocialSkilling);

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

                //InterestManager.print("MemorySteam bytes: " + bytes.Length.ToString());
                var str = BitConverter.ToString(bytes);

                string saveName = (string.IsNullOrEmpty(GameStates.sLoadFileName)) ? GameUtils.GetCurrentWorld().ToString() + "_PLACEHOLDER_INTERESTS" : GameUtils.GetCurrentWorld().ToString() + "_" + GameStates.sLoadFileName;//error

                if (PersistedDataInterests.mInterestSaveData.ContainsKey(saveName))
                {
                    PersistedDataInterests.mInterestSaveData[saveName] = str;
                }
                else
                {
                    PersistedDataInterests.mInterestSaveData.Add(saveName, str);
                }

                //ProgressDialog.Close();
                InterestManager.print("Done!");
            }
            catch (Exception ex)
            {
                InterestManager.print(ex.ToString());
            }
        }

        public static void ImportInterestData()
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                string saveName = (string.IsNullOrEmpty(GameStates.sLoadFileName)) ? GameUtils.GetCurrentWorld().ToString() + "_PLACEHOLDER_INTERESTS" : GameUtils.GetCurrentWorld().ToString() + "_" + GameStates.sLoadFileName;
                string dataStr = String.Empty;

                if (PersistedDataInterests.mInterestSaveData.ContainsKey(saveName))
                {
                    dataStr = PersistedDataInterests.mInterestSaveData[saveName];
                    SimpleMessageDialog.Show("", "has Data Str with world/save. Loading now...");
                }
                else
                {
                    return; // Since there isn't any data, we'll do this
                }

                if (String.IsNullOrEmpty(dataStr))
                    return;


                string str = dataStr;

                byte[] data = FromHex(dataStr);

                if (data.Length == 0)
                {
                    return;
                }


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

                    if (findOwner == null)
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

                        bool playerHasBeenNotifiedSocial = reader.ReadBoolean();
                        sb.AppendLine("    playerHasBeenNotifiedSocial: " + playerHasBeenNotifiedSocial.ToString());

                        interests[a].mHasNotifiedPlayerAboutSocialSkilling = playerHasBeenNotifiedSocial;

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

                // We have to manually close this if we want to write an XML file
                reader.Close();
                input.Close();

                WriteErrorXMLFile("Extracted", null, sb.ToString());

                InterestManager.print(numSim.ToString());
                
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
