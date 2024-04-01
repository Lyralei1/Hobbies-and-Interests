using Lyralei.InterestMod;
using Lyralei.UI;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Lyralei.InterestsAndHobbies;
using Sims3.Gameplay.Objects.Appliances;
using Sims3.Gameplay.Objects.Decorations;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Environment;
using Sims3.Gameplay.Objects.Lighting;
using Sims3.Gameplay.Objects.PerformanceObjects;
using Sims3.Gameplay.Objects.Register;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HobbiesAndInterests.HelperClasses
{
    [Persistable(false)]
    public class EnergyManager
    {
        public static Dictionary<ulong, EnergyCompany> mAllContractsPerHousehold = new Dictionary<ulong, EnergyCompany>();

        public AlarmHandle mShouldInflateDeflatePricesAlarm = AlarmHandle.kInvalidHandle;


        [Persistable]
        public static string mSavedEnergyCompanyData;

        //private static EnergyManager singleton;
        public static EnergyManager Instance;
        public EnergyManager()
        {
            EnergyManager singleton = Instance;
        }

        public static void Startup()
        {
            try
            {
                if (Instance == null)
                {
                    Instance = new EnergyManager();
                }
                GetCompanyDetailsFromXML();

                // Load after XML, so it doesn't override our saved data with any original data.
                LoadCompanyData();

                Instance.mShouldInflateDeflatePricesAlarm = AlarmManager.Global.AddAlarmRepeating(5f, TimeUnit.Days, Instance.InflateDeflatePrices, 5f, TimeUnit.Days, "EnergyManager_Inflate_Deflate_Alarm", AlarmType.NeverPersisted, null);

                InterestManager.print("Companies: " + AllCompanies.Count.ToString());
            }
            catch (Exception ex)
            {
                InterestManager.print(ex.ToString());
            }
        }

        public static void Shutdown()
        {
            SaveCompanyData();

            if (Instance != null)
            {
                Instance.Dispose();
            }
            Instance = null;
        }
        public void Dispose()
        {
            AlarmManager.Global.RemoveAlarm(mShouldInflateDeflatePricesAlarm);
        }

        public static void GetCompanyDetailsFromXML()
        {
            try
            {
                XmlDbData xmlSupportedMods = XmlDbData.ReadData(new ResourceKey(0x3343E53EAA368AC6, 0x0333406C, 0x00000000), false);
                if (xmlSupportedMods != null)
                {
                    InterestManager.print("Loading items...");
                    if (!ReadCompanyDetails(xmlSupportedMods))
                    {
                        InterestManager.print("An error occured while loading Energy company details. Error file has been made in Documents/The Sims 3. Make sure to give this to Lyralei. Interest & Hobbies mod will work fine though!");
                    }
                }
            }
            catch(Exception ex)
            {
                InterestManager.print(ex.ToString());
            }
        }

        private static bool ReadCompanyDetails(XmlDbData xmlDocument)
        {
            try
            {
                XmlDbTable xmlDbTable;
                AllCompanies.Clear();

                if (xmlDocument != null && xmlDocument.Tables != null && xmlDocument.Tables.TryGetValue("EnergyCompany", out xmlDbTable))
                {
                    foreach (XmlDbRow row in xmlDbTable.Rows)
                    {
                        EnergyCompany mCompany = new EnergyCompany();

                        mCompany.NameCompany = row.GetString("NameCompany");

                        mCompany.DescCompany = row.GetString("DescCompany");
                        mCompany.IsFixedContract = row.GetBool("IsFixedContract");

                        mCompany.PeakTarif = row.GetFloat("PeakTarif");
                        mCompany.OffPeakTarif = row.GetFloat("OffPeakTarif");

                        mCompany.NewPeakTarif = mCompany.PeakTarif;
                        mCompany.NewOffPeakTarif = mCompany.OffPeakTarif;

                        mCompany.DaysToExpire = row.GetInt("DaysToExpire");
                        mCompany.IsEcoFriendly = row.GetBool("IsEcoFriendly");
                        mCompany.OnceExpiredShouldRenew = row.GetBool("OnceExpiredShouldRenew");
                        mCompany.DiscountsWhenRenewableEnergy = row.GetBool("DiscountsWhenRenewableEnergy");

                        mCompany.Identifier = mCompany.NameCompany + "_" + mCompany.IsFixedContract.ToString();


                        AllCompanies.Add(mCompany);

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

        private static void SaveCompanyData()
        {
            MemoryStream memorystream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(memorystream);

            int householdCount = mAllContractsPerHousehold.Count;

            bw.Write(householdCount);

            foreach (KeyValuePair<ulong, EnergyCompany> kpv in mAllContractsPerHousehold)
            {
                if (kpv.Value == null)
                    return;

                ulong householdId = kpv.Key;
                bw.Write(householdId);

                kpv.Value.SaveData(bw, memorystream);
            }

            // Closing Binary writer.
            bw.Close();

            byte[] bytes = memorystream.ToArray();
            var str = BitConverter.ToString(bytes);

            // Now we store it as a string, so we can restore the data later on.
            mSavedEnergyCompanyData = str;
        }

        private static void LoadCompanyData()
        {
            if (String.IsNullOrEmpty(mSavedEnergyCompanyData))
                return;

            mAllContractsPerHousehold.Clear();

            string str = mSavedEnergyCompanyData;

            byte[] data = InterestSaveManager.FromHex(str);

            MemoryStream input = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(input);

            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                ulong householdId = reader.ReadUInt64();

                EnergyCompany company = new EnergyCompany();
                company.LoadData(reader, input);
                company.InitImportantStuff();
                mAllContractsPerHousehold.Add(householdId, company);
            }

            reader.Close();
            input.Close();
        }

        private static EnergyCompany GetCompanyDataForSavedData(string identifier)
        {
            foreach(EnergyCompany company in AllCompanies)
            {
                if (company.Identifier == identifier)
                    return company;
            }
            return null;
        }

        public void InflateDeflatePrices()
        {
            if (AllCompanies == null || AllCompanies.Count == 0)
                return;

            bool IsEnergyCrisis = RandomUtil.RandomChance(Tunables.mChanceOfEnergyCrisis);

            foreach(KeyValuePair<ulong, EnergyCompany> kpv in mAllContractsPerHousehold)
            {
                if (kpv.Value == null)
                    continue;

                if (!kpv.Value.IsFixedContract)
                {
                    if(IsEnergyCrisis)
                    {
                        kpv.Value.NewOffPeakTarif = kpv.Value.OffPeakTarif + Tunables.mAmountIncreaseAtCrisisOffPeak;
                        kpv.Value.NewPeakTarif = kpv.Value.PeakTarif + Tunables.mAmountIncreaseAtCrisisPeak;
                    }

                    kpv.Value.EnergyTotalPrice = RandomUtil.GetFloat(kpv.Value.NewOffPeakTarif, kpv.Value.NewPeakTarif);

                    // Update new Peak values on variable contracts.
                    AllCompanies.ForEach(
                        (EnergyCompany obj) => { 
                            if (obj != null && obj.Identifier == kpv.Value.Identifier) 
                                    obj.NewOffPeakTarif = kpv.Value.NewOffPeakTarif; 
                                    obj.NewPeakTarif = kpv.Value.NewPeakTarif; 
                        }
                    );
                }
            }


            foreach(EnergyCompany company in AllCompanies)
            {
                // We need to update the fixed rates on regular contracts, but not the household ones.
                if (IsEnergyCrisis)
                {
                    company.NewOffPeakTarif = company.OffPeakTarif + Tunables.mAmountIncreaseAtCrisisOffPeak;
                    company.NewPeakTarif = company.PeakTarif + Tunables.mAmountIncreaseAtCrisisPeak;
                }
                else
                {
                    // Should change peak/offpeak tarifs?
                    if(company.IsFixedContract && RandomUtil.CoinFlip())
                    {
                        company.NewOffPeakTarif = RandomUtil.GetFloat(0, company.OffPeakTarif + 5);
                        company.NewPeakTarif = RandomUtil.GetFloat(0, company.PeakTarif + 5);
                    }
                    else if(!company.IsFixedContract)
                    {
                        company.NewOffPeakTarif = RandomUtil.GetFloat(0, company.OffPeakTarif + 5);
                        company.NewPeakTarif = RandomUtil.GetFloat(0, company.PeakTarif + 5);
                    }
                }
            }

        }

        public static EnergyCompany GetSimEnergyCompanyIsWith(Sim sim)
        {
            if (sim == null || mAllContractsPerHousehold.Count == 0)
                return null;

            if (mAllContractsPerHousehold.TryGetValue(sim.Household.HouseholdId, out EnergyCompany energyCompany))
                return energyCompany;
            
            return null;
        }

        public static void RegisterSimWithEnergyCompany(Sim sim, EnergyCompany company)
        {
            if (sim == null || mAllContractsPerHousehold == null || company == null)
                return;

            if (mAllContractsPerHousehold.ContainsKey(sim.Household.HouseholdId))
                mAllContractsPerHousehold[sim.Household.HouseholdId] = company;
            else
                mAllContractsPerHousehold.Add(sim.Household.HouseholdId, company);
            // Set to registered
            mAllContractsPerHousehold[sim.Household.HouseholdId].isExpiredContract = false;
            mAllContractsPerHousehold[sim.Household.HouseholdId].EnergyCurrentPrice = 0;
            mAllContractsPerHousehold[sim.Household.HouseholdId].kWhCurrentlyProduced = 0;
            mAllContractsPerHousehold[sim.Household.HouseholdId].HouseholdId = sim.Household.HouseholdId;
            mAllContractsPerHousehold[sim.Household.HouseholdId].InitImportantStuff();

            if(mAllContractsPerHousehold[sim.Household.HouseholdId].IsEcoFriendly)
            {
                InterestManager.AddSubPoints(Tunables.mSubPointsToAddIfEcoFriendly, sim.SimDescription, InterestTypes.Environment);
            }
        }

        public static void CleanNullDataFromDictionary(ulong householdId)
        {
            mAllContractsPerHousehold.Remove(householdId);
        }

        public static void PickEnergyCompanyForFamily()
        {
            if (Sim.ActiveActor != null)
            {
                if (EnergyManager.GetSimEnergyCompanyIsWith(Sim.ActiveActor) == null)
                {
                    bool energyCompanyChoice = TwoButtonDialog.Show("Your household isn't with any energy company yet! Do you want to assign this yourself, or should this be random?", "I'll choose", "Randomize");

                    bool userDeclinedDialog = false;
                    if (energyCompanyChoice)
                    {
                        EnergyManager.EnergyCompany company = EnergyCompanySelectionDialog.Show();

                        if (company == null)
                        {
                            userDeclinedDialog = true;
                            SimpleMessageDialog.Show("Interests & Hobbies - Energy Manager", "It seems that you've clicked away the dialogue, Energy manager will pick a company for you.");
                        }
                        else
                        {
                            EnergyManager.RegisterSimWithEnergyCompany(Sim.ActiveActor, company);
                        }
                    }
                    if (!energyCompanyChoice || userDeclinedDialog)
                    {
                        EnergyManager.EnergyCompany company = RandomUtil.GetRandomObjectFromList<EnergyManager.EnergyCompany>(EnergyManager.AllCompanies);
                        EnergyManager.RegisterSimWithEnergyCompany(Sim.ActiveActor, company);
                    }
                }
            }
        }

        public static void PickEnergyCompanyForFamily(object sender, LotManager.ActiveLotChangedArgs e)
        {
            PickEnergyCompanyForFamily();
        }

        public static List<EnergyCompany> AllCompanies = new List<EnergyCompany>();

        public class EnergyCompany : IAlarmOwner
        {

            // These are all extracted info from the xml, do NOT change any details! Use newPeakTarifs instead!
            public string Identifier;
            public string NameCompany;
            public string DescCompany;
            public bool IsFixedContract = true; // False means dynamic.
            public float EnergyTotalPrice = 100; // Total price they will have to pay per 5 days.
            public float EnergyCurrentPrice = 0; // How much the family has already made cost wise.
            public float PeakTarif = 1;
            public float OffPeakTarif = 1;
            public int DaysToExpire = 5;
            public bool IsEcoFriendly = false;
            public bool OnceExpiredShouldRenew = false;
            public bool DiscountsWhenRenewableEnergy = false;

            // Any variable here is meant for changing and keeping track of things.
            public bool isExpiredContract = true;
            public int RemainingDays = 5;
            public float NewPeakTarif = 1;
            public float NewOffPeakTarif = 1;

            public float kWhCurrentlyProduced = 0;
            public ulong HouseholdId = 0;

            public AlarmHandle mCheckkWhWithdrawn = AlarmHandle.kInvalidHandle;
            public AlarmHandle mContractAge = AlarmHandle.kInvalidHandle;
            public AlarmHandle mWithdrawMoney = AlarmHandle.kInvalidHandle;

            public EnergyCompany()
            {}

            //public EnergyCompany(string nameCompany, string descCompany, bool isfixedContract, int peakTarif, int offPeakTarif, int daysToExpire)
            //{
            //    NameCompany = nameCompany;
            //    DescCompany = descCompany;
            //    IsFixedContract = isfixedContract;
            //    PeakTarif = peakTarif;
            //    OffPeakTarif = offPeakTarif;
            //    DaysToExpire = daysToExpire;
            //}

            // Run this after load!
            public void InitImportantStuff()
            {
                mContractAge            = AlarmManager.Global.AddAlarmRepeating(DaysToExpire, TimeUnit.Days, ContractHasExpired, RemainingDays, TimeUnit.Days, "ContractAge_" + NameCompany + "_" + Identifier, AlarmType.NeverPersisted, this);
                mCheckkWhWithdrawn      = AlarmManager.Global.AddAlarmRepeating(1f, TimeUnit.Hours, CheckkWhProduced, 1f, TimeUnit.Hours, "kWhChecker" + NameCompany + "_" + Identifier, AlarmType.NeverPersisted, this);
                mWithdrawMoney          = AlarmManager.Global.AddAlarmDay(true, 13f, DaysOfTheWeek.Monday, WithdrawMoney, "WithdrawEnergyCost", AlarmType.NeverPersisted, this);
            }

            public void ContractHasExpired()
            {
                RemainingDays--;

                if (RemainingDays == 0)
                {
                    isExpiredContract = true;
                    Household household = Household.Find(HouseholdId);
                    if (household == null)
                    {
                        CleanNullDataFromDictionary(HouseholdId);
                        return;
                    }
                    StyledNotification.Show(new StyledNotification.Format("Your contract with " + NameCompany + " has expired! If you don't change it today, then the contract will be renewed tomorrow!", household.LotHome.ObjectId, household.Sims[0].ObjectId, StyledNotification.NotificationStyle.kGameMessageNegative));
                }

                if(RemainingDays == -1)
                {
                    isExpiredContract = false;
                    RemainingDays = DaysToExpire;

                    Household household = Household.Find(HouseholdId);
                    if (household == null)
                    {
                        CleanNullDataFromDictionary(HouseholdId);
                        return;
                    }

                    EnergyCurrentPrice = 0;
                    kWhCurrentlyProduced = 0;

                    StyledNotification.Show(new StyledNotification.Format("Your contract with " + NameCompany + " has been renewed! You'll have to wait " + RemainingDays.ToString() + " before being able to switch energy companies.", household.LotHome.ObjectId, household.Sims[0].ObjectId, StyledNotification.NotificationStyle.kGameMessagePositive));
                }
            }

            public void CheckkWhProduced()
            {
                Household household = Household.Find(HouseholdId);
                if (household == null)
                {
                    CleanNullDataFromDictionary(HouseholdId);
                    return;
                }

                Lot lot = household.LotHome;

                float kWh = 0;

                if(lot.AreHolidayHouseLightsEnabled)
                    kWh += 29; // this is a combined calculation of, say, 70 > 100 lamps

                LightGameObject[] lights = lot.GetObjects<LightGameObject>();
                if(lights != null)
                {
                    foreach (LightGameObject light in lights)
                    {
                        // Since EA has no consistancy, both check for black lights and if it's even on.
                        if (light.IsBlackLight || !light.IsLightOn())
                            continue;

                        kWh += 0.0043f;
                    }
                }

                List<GameObject> electronics = GetAllElectronicObjectsOnLot(household.LotHome);

                FishTank[] fishTank = lot.GetObjects<FishTank>();
                if (fishTank != null || fishTank.Length > 0)
                    electronics.AddRange(fishTank);

                if (electronics != null)
                {
                    foreach (GameObject electronic in electronics)
                    {

                        // Because the TV could be on when not in use...
                        if(electronic.GetType() == typeof(TV))
                        {
                            TV tv = electronic as TV;
                            if(tv.IsTurnedOn() && !tv.InUse)
                                kWh += 0.06f;
                        }
                        if (electronic.GetType() == typeof(Stereo))
                        {
                            Stereo stereo = electronic as Stereo;
                            if (stereo.IsTurnedOn() && !stereo.InUse)
                                kWh += 0.1f;
                        }
                        if(electronic.GetType() == typeof(FishTank))
                           kWh += 0.4f;

                        if (electronic.GetType() == typeof(Dryer))
                        {
                            Dryer dryer = electronic as Dryer;
                            if (dryer.CurDryerState == Dryer.DryerState.Running)
                                kWh += WattageTokWh(3000);
                        }
                        if (electronic.GetType() == typeof(WashingMachine))
                        {
                            WashingMachine washingMachine = electronic as WashingMachine;
                            if(washingMachine.mWashState == WashingMachine.WashState.Running || washingMachine.mWashState == WashingMachine.WashState.RunningViolently)
                                kWh += WattageTokWh(850);
                        }
                        if (electronic.GetType() == typeof(Microwave))
                        {
                            Microwave microwave = electronic as Microwave;
                            if (microwave.On)
                                kWh += WattageTokWh(1200);
                        }
                        if (electronic.GetType() == typeof(Fridge))
                            kWh += WattageTokWh(120);

                        if (electronic.GetType() == typeof(Dishwasher))
                        {
                            Dishwasher dishwasher = electronic as Dishwasher;
                            if (dishwasher.mWasherRunSound != null)
                                kWh += WattageTokWh(1800);
                        }

                        // Generic average.
                        kWh += 0.05f;
                    }
                }

                SolarPanel[] solarPanel = lot.GetObjects<SolarPanel>();
                Windmill[] windMill = lot.GetObjects<Windmill>();

                if (DiscountsWhenRenewableEnergy)
                {
                    if(windMill != null && windMill.Length > 0)
                    {
                        // Average watt a windMill produces is this amount.
                        float discount = WattageTokWh(893) * solarPanel.Length;
                        kWh -= discount;
                    }
                    else if (solarPanel != null && solarPanel.Length > 0)
                    {
                        // Average kWh a solar panel produces is this amount. This way if a player has a shit tonne of solar panels, they can practically make their bill 0
                        float discount = 0.35f * solarPanel.Length;
                        kWh -= discount;
                    }
                }

                kWhCurrentlyProduced += kWh;

                float currentCost = CalculateCostFromkWh(kWh);
                EnergyCurrentPrice += currentCost;

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Currently kWh: " + kWh.ToString());
                sb.AppendLine("Total: " + kWhCurrentlyProduced.ToString());
                sb.AppendLine("Cost this hour: " + currentCost.ToString());
                sb.AppendLine("Total Cost: " + EnergyCurrentPrice.ToString());
                

                StyledNotification.Show(new StyledNotification.Format(sb.ToString(), household.LotHome.ObjectId, household.Sims[0].ObjectId, StyledNotification.NotificationStyle.kGameMessagePositive));
            }

            public void WithdrawMoney()
            {

                if(!isExpiredContract && (int)EnergyCurrentPrice != 0)
                {
                    Household household = Household.Find(HouseholdId);
                    if (household == null)
                    {
                        CleanNullDataFromDictionary(HouseholdId);
                        return;
                    }

                    if (Household.ActiveHousehold.HouseholdId != household.HouseholdId)
                        return;

                    int finalBill = (int)EnergyCurrentPrice;

                    if (household.FamilyFunds <= finalBill)
                    {
                        // 200
                        // tax: 4000
                        int leftToPay = finalBill - household.FamilyFunds;
                        int amountToSubtract = household.FamilyFunds;

                        household.UnpaidBills += leftToPay;

                        household.Sims[0].ShowTNSIfSelectable(NameCompany + " tried to withdraw a total of $" + finalBill.ToString() + ", but couldn't fully make the transaction. The remaining " + leftToPay.ToString() + " was put on the bills instead.", Sims3.UI.StyledNotification.NotificationStyle.kSimTalking);

                    }
                    else
                    {
                        household.Sims[0].ModifyFunds(-finalBill);

                        household.Sims[0].ShowTNSIfSelectable(NameCompany + " has withdrawn a total of $" + finalBill.ToString() + "! The next paytime will be on Monday, 1pm again.", Sims3.UI.StyledNotification.NotificationStyle.kSimTalking);
                    }

                }
            }


            private float WattageTokWh(int watt)
            {
                float kwh = watt * 1;
                kwh = kwh / 1000;
                return kwh;
            }

            public float CalculateCostFromkWh(float kWh)
            {
                // kWh reference: 0.310733
                // PeakTarif reference: 0.41325

                float hoursPassedOfDay = SimClock.HoursPassedOfDay;
                float currentCost = 0;

                // If peak hour...
                if(hoursPassedOfDay >= Tunables.mStartOffPeakHour && hoursPassedOfDay < Tunables.mEndPeakHour)
                    currentCost = kWh * NewPeakTarif;
                else
                    currentCost = kWh * NewOffPeakTarif;
                
                return currentCost;
            }

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
                    "Sims3.Gameplay.Objects.PerformanceObjects"
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
                        obj.GetType() == typeof(AthleticGameObject)
                        )
                    {
                        mElectronics.Add(obj);
                    }
                }
                return mElectronics;

            }

            public void SaveData(BinaryWriter bw, MemoryStream memorystream)
            {
                //MemoryStream memorystream = new MemoryStream();
                //BinaryWriter bw = new BinaryWriter(memorystream);

                // Save important data that doesn't come with the XML
                string id = Identifier;
                bw.Write(id);

                ulong householdId = HouseholdId;
                bw.Write(householdId);

                float newpeak = NewPeakTarif;
                bw.Write(newpeak);

                float newOffpeak = NewOffPeakTarif;
                bw.Write(newOffpeak);

                float remainingDays = RemainingDays;
                bw.Write(remainingDays);

                float currentCosts = EnergyCurrentPrice;
                bw.Write(currentCosts);

                float currentkWh = kWhCurrentlyProduced;
                bw.Write(currentkWh);

                bool expired = isExpiredContract;
                bw.Write(expired);


                //// Closing Binary writer.
                //bw.Close();

                //byte[] bytes = memorystream.ToArray();
                //var str = BitConverter.ToString(bytes);

                //// Now we store it as a string, so we can restore the data later on.
                //mSavedEnergyCompanyData = str;
            }

            public void LoadData(BinaryReader reader, MemoryStream input)
            {
                //if (String.IsNullOrEmpty(mSavedEnergyCompanyData))
                //    return;

                //string str = mSavedEnergyCompanyData;

                //byte[] data = InterestSaveManager.FromHex(str);

                //MemoryStream input = new MemoryStream(data);
                //BinaryReader reader = new BinaryReader(input);
                this.Identifier = reader.ReadString();

                EnergyCompany refCompany = GetCompanyDataForSavedData(this.Identifier);

                this.HouseholdId = reader.ReadUInt64();

                this.NewPeakTarif = reader.ReadSingle();
                this.NewOffPeakTarif = reader.ReadSingle();
                this.RemainingDays = reader.ReadInt32();
                this.EnergyCurrentPrice = reader.ReadSingle();
                this.kWhCurrentlyProduced = reader.ReadSingle();
                this.IsFixedContract = reader.ReadBoolean();

                // Copy XML data to this instance.
                this.DaysToExpire = refCompany.DaysToExpire;
                this.DescCompany = refCompany.DescCompany;
                this.DiscountsWhenRenewableEnergy = refCompany.DiscountsWhenRenewableEnergy;
                this.IsEcoFriendly = refCompany.IsEcoFriendly;
                this.isExpiredContract = refCompany.isExpiredContract;
                this.NameCompany = refCompany.NameCompany;
                this.OffPeakTarif = refCompany.OffPeakTarif;
                this.PeakTarif = refCompany.PeakTarif;

            }

        }

    }
}
