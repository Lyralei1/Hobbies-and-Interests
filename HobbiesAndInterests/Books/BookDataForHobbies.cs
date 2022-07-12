using Lyralei;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.Objects.Lyralei
{
    public class BookGeneralInterestsData : BookData
    {
        public InterestTypes interestType = InterestTypes.None;


        public BookGeneralInterestsData()
        {
        }

        //(string title, string author, int length, int value, string geoState, string materialState)
        public BookGeneralInterestsData(InterestTypes ParsedinterestType, string title)
            : base(title, "Lyralei", 500, 50, "BookLargeThick", "bookGeneric1")
        {
            base.ID = title + "_Lyralei";
            base.mTitle = "Gameplay/Excel/Books/LyraleiBooks:" + title;
            base.mAuthor = "LyraleiTheAuthor";
            base.Length = 500;
            base.Value = 50;
            base.PagesMinNorm = 3f;
            base.PagesMinBW = 5f;
            base.GeometryState = "BookLargeThick";
            base.MaterialState = "bookGeneric" + RandomUtil.GetInt(1, 3);
            base.RowIndex = -1;
            base.MyType = BookType.General;
            base.NotInBookStore = true;
            base.AllowedWorlds = new List<WorldName>();
            base.AllowedWorldTypes = new List<WorldType>()
            {
                WorldType.Base, WorldType.Downtown, WorldType.University, WorldType.Future
            };
            interestType = ParsedinterestType;
            //GlobalOptionsHobbiesAndInterests.BookGeneralInterestsDataList.Add(base.ID, this);
        }

        public override string GenerateUIStoreItemID()
        {
            return "BookGeneral_" + base.ID;
        }
    }

    //public class BookInterestGeneralStoreItem : BookBaseStoreItem
    //{
    //    public InterestTypes interestType = InterestTypes.None;

    //    public BookInterestGeneralStoreItem(string name, float price, object customData, ThumbnailKey thumb, string storeUIItemID, CreateObjectCallback callback, ProcessObjectCallback process, List<WorldName> allowWorlds, List<WorldType> worldTypes, string author, string title, int pageCount, InterestTypes mInterestType)
    //        : base(name, price, customData, thumb, storeUIItemID, callback, process, allowWorlds, worldTypes, author, title, pageCount)
    //    {
    //        interestType = mInterestType;
    //    }

    //    public BookInterestGeneralStoreItem()
    //    {
    //    }

    //    public override ShoppingUIItem CreateUIItem(float percentPriceModifier, float salePercentage, int markupPercentage, Dictionary<string, ShoppingCoupon> couponMap)
    //    {
    //        ShoppingCoupon coupon;
    //        int actualPrice;
    //        base.CreateSharedUIItemData(out coupon, out actualPrice, percentPriceModifier, salePercentage, markupPercentage, couponMap);
    //        return new ShoppingInterestGeneralBookUIItem(this, actualPrice, coupon);
    //    }
    //}

    //public class ShoppingInterestGeneralBookUIItem : ShoppingUIItemBookBase, IBookUIItem, IShoppingUIItem, IShopItem
    //{
    //    public InterestTypes InterestType
    //    {
    //        get
    //        {
    //            return ((BookInterestGeneralStoreItem)base.mStoreItem).interestType;
    //        }
    //    }

    //    public ShoppingInterestGeneralBookUIItem(BookInterestGeneralStoreItem bookItem, int actualPrice, ShoppingCoupon coupon)
    //        : base(bookItem, actualPrice, coupon)
    //    {
    //    }
    //}

    public class BookGeneralInterests : Book
    {
        public override void OnLoad()
        {
            BookGeneralInterestsData data;
            if (GlobalOptionsHobbiesAndInterests.BookGeneralInterestsDataList.TryGetValue(base.mBookId, out data))
            {
                base.Data = data;
            }
            base.OnLoad();
        }

        public static BookGeneralInterests CreateOutOfWorld(BookGeneralInterestsData data)
        {
            try
            {
                BookGeneralInterests bookRecipe = GlobalFunctions.CreateObjectOutOfWorld(new ResourceKey(0x5B85FC105055568A, 0x319E4F1D, 0x00000000)) as BookGeneralInterests;
                bookRecipe.InitBookCommon(data);
                return bookRecipe;
            }
            catch (Exception ex)
            {
                InterestManager.print("Create Out of world for book found an error: " + '\n' + ex.ToString());
                return null;
            }
        }

        public static ObjectGuid CreateOutOfWorldID(BookGeneralInterestsData data, ref Simulator.ObjectInitParameters initData)
        {
            IGameObject gameObject = GlobalFunctions.CreateObjectOutOfWorld(new ResourceKey(0x5B85FC105055568A, 0x319E4F1D, 0x00000000), null, initData);
            if (gameObject == null)
            {
                return ObjectGuid.InvalidObjectGuid;
            }
            return gameObject.ObjectId;
        }

        public static void ProcessCallback(BookGeneralInterestsData data, BookGeneralInterests book)
        {
            book.InitBookCommon(data);
            book.RemoveFromWorld();
        }

        //public override IGameObject Copy(bool movingOut)
        //{
        //    BookGeneralInterests result = null;
        //    BookGeneralInterestsData bookRecipeData = base.Data as BookGeneralInterestsData;
        //    if (bookRecipeData != null)
        //    {
        //        result = CreateOutOfWorld(bookRecipeData);
        //    }
        //    return result;
        //}

        public override bool StopUsingBook(Sim Actor, StateMachineClient smcRead, bool bPutInInventory)
        {
            BookGeneralInterestsData bookRecipeData = base.Data as BookGeneralInterestsData;
            InterestManager.AddSubPoints(0.10f, Actor.SimDescription, bookRecipeData.interestType);
            return base.StopUsingBook(Actor, smcRead, bPutInInventory);
        }

        public override void FirstTimeFinished(Sim actor)
        {
            BookGeneralInterestsData bookRecipeData = base.Data as BookGeneralInterestsData;
            if (bookRecipeData != null)
            {
                if (InterestManager.mSavedSimInterests.ContainsKey(actor.mSimDescription.SimDescriptionId))
                {
                    for (int j = 0; j < InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId].Count; j++)
                    {
                        if (InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId][j].Guid == bookRecipeData.interestType)
                        {
                            InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId][j].modifyInterestLevel(1, actor.mSimDescription.SimDescriptionId, InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId][j].Guid);
                            break;
                        }
                    }
                }
                base.mOneShotDelete = false;
            }
        }

        public override bool TestReadBook(Sim Actor, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        {
            //if (isAutonomous)
            //{
            //    return false;
            //}

            BookGeneralInterestsData bookRecipeData = base.Data as BookGeneralInterestsData;
            if (bookRecipeData == null)
            {
                return false;
            }

            if(InterestManager.GetLevelForInterest(Actor.mSimDescription, bookRecipeData.interestType) >= 18)
            {
                greyedOutTooltipCallback = (() => "Your sim isn't really interested in reading up about the " + bookRecipeData.interestType.ToString() + " interest. They already know everything about this interest!");
                return false;
            }

            if (!InterestManager.HasTheNecessaryInterest(Actor.mSimDescription, bookRecipeData.interestType, false))
            {
                greyedOutTooltipCallback = (() => "Your sim isn't really interested in reading up about the " + bookRecipeData.interestType.ToString() + " interest. Make sure to research an interest first before reading this book!");
                return false;
            }

            // If sim hates interest...
            if (InterestManager.DoesSimHatesLovesInterest(Actor.mSimDescription, bookRecipeData.interestType) == 2 || InterestManager.DoesSimHatesLovesInterest(Actor.mSimDescription, bookRecipeData.interestType) == 3)
            {
                //greyedOutTooltipCallback = (() => Localization.LocalizeString(Actor.IsFemale, "Gameplay/Objects/BookRecipe:NoCookingSkill", Actor));
                greyedOutTooltipCallback = (() => "Your sim isn't really interested in reading up about the " + bookRecipeData.interestType.ToString() + " interest. Check if they hate this interest or do some more research on the interest.");
                return false;
            }
            //greyedOutTooltipCallback = (() => Localization.LocalizeString(Actor.IsFemale, "Gameplay/Objects/BookRecipe:NoCookingSkill", Actor));
            return true;
        }
    }



    public class BookMagazineInterestsData : BookData
    {
        public InterestTypes interestType = InterestTypes.None;

        public int issueNum = 0;

        public BookMagazineInterestsData()
        {
        }

        public static string[] randomSimsArray = new string[6]
        {
            "Nancy Landgraab",
            "Malcolm Langraab CLXXXV",
            "Dina Caliente",
            "Mortimer Goth",
            "Bella",
            "Nick Alto",
        };

        public override string Title
        {
            get
            {
                string randomPerson = RandomUtil.GetRandomStringFromList(randomSimsArray);
                return "#" + RandomUtil.GetInt(1, 63) + " - " + Localization.LocalizeString(base.mTitle , new object[2] { interestType.ToString(), randomPerson });
            }
            set
            {
                base.mTitle = value;
            }
        }

        //(string title, string author, int length, int value, string geoState, string materialState)
        public BookMagazineInterestsData(InterestTypes ParsedinterestType, string title, int number)
            : base(title, "Lyralei", 500, 50, "BookSmallThin", "bookGeneric1")
        {
            interestType = ParsedinterestType;
            base.ID = title + "_Lyralei";
            //base.mTitle = "Lyralei/InterestsHobbies/MagazineSentence" + number;
            base.mTitle = "Gameplay/Excel/Books/LyraleiMagazines:" + number;
            base.mAuthor = "LyraleiTheAuthor";
            base.Length = 500;
            base.Value = 50;
            base.PagesMinNorm = 3f;
            base.PagesMinBW = 5f;
            base.GeometryState = "BookSmallThin";
            base.MaterialState = "EP9bookComic" + RandomUtil.GetInt(1, 15);
            base.RowIndex = -1;
            base.MyType = BookType.General;
            base.NotInBookStore = false;
            base.AllowedWorlds = new List<WorldName>();
            base.AllowedWorldTypes = new List<WorldType>()
            {
                WorldType.Base, WorldType.Downtown, WorldType.University, WorldType.Future
            };
            //GlobalOptionsHobbiesAndInterests.BookGeneralInterestsDataList.Add(base.ID, this);
        }

        public override string GenerateUIStoreItemID()
        {
            return "BookGeneral_" + base.ID;
        }
    }

    public class BookMagazineInterests : Book
    {
        public override void OnLoad()
        {
            BookMagazineInterestsData data;
            if (GlobalOptionsHobbiesAndInterests.BookMagazineInterestsDataList.TryGetValue(base.mBookId, out data))
            {
                base.Data = data;
            }
            base.OnLoad();
        }

        public static BookMagazineInterests CreateOutOfWorld(BookMagazineInterestsData data)
        {
            try
            {
                BookMagazineInterests bookRecipe = GlobalFunctions.CreateObjectOutOfWorld(new ResourceKey(0x5A0008C9EB7DA05E, 0x319E4F1D, 0x00000000)) as BookMagazineInterests;
                bookRecipe.InitBookCommon(data);
                return bookRecipe;
            }
            catch (Exception ex)
            {
                InterestManager.print("Create Out of world for book found an error: " + '\n' + ex.ToString());
                return null;
            }
        }

        public static ObjectGuid CreateOutOfWorldID(BookMagazineInterestsData data, ref Simulator.ObjectInitParameters initData)
        {
            IGameObject gameObject = GlobalFunctions.CreateObjectOutOfWorld(new ResourceKey(0x5A0008C9EB7DA05E, 0x319E4F1D, 0x00000000), null, initData);
            if (gameObject == null)
            {
                return ObjectGuid.InvalidObjectGuid;
            }
            return gameObject.ObjectId;
        }

        public static void ProcessCallback(BookMagazineInterestsData data, BookMagazineInterests book)
        {
            book.InitBookCommon(data);
            book.RemoveFromWorld();
        }

        public override bool StopUsingBook(Sim Actor, StateMachineClient smcRead, bool bPutInInventory)
        {
            BookMagazineInterestsData bookRecipeData = base.Data as BookMagazineInterestsData;
            InterestManager.AddSubPoints(0.30f, Actor.SimDescription, bookRecipeData.interestType);
            return base.StopUsingBook(Actor, smcRead, bPutInInventory);
        }

        public override void FirstTimeFinished(Sim actor)
        {
            BookMagazineInterestsData bookRecipeData = base.Data as BookMagazineInterestsData;
            if (bookRecipeData != null)
            {
                if (InterestManager.mSavedSimInterests.ContainsKey(actor.mSimDescription.SimDescriptionId))
                {
                    for (int j = 0; j < InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId].Count; j++)
                    {
                        if (InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId][j].Guid == bookRecipeData.interestType)
                        {
                            InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId][j].modifyInterestLevel(1, actor.mSimDescription.SimDescriptionId, InterestManager.mSavedSimInterests[actor.mSimDescription.SimDescriptionId][j].Guid);
                            break;
                        }
                    }
                }
                base.mOneShotDelete = false;
            }
        }

        public override bool TestReadBook(Sim Actor, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        {
            //if (isAutonomous)
            //{
            //    return false;
            //}

            BookMagazineInterestsData bookRecipeData = base.Data as BookMagazineInterestsData;
            if (bookRecipeData == null)
            {
                return false;
            }

            //if (InterestManager.GetLevelForInterest(Actor.mSimDescription, bookRecipeData.interestType) >= 18)
            //{
            //    greyedOutTooltipCallback = (() => "Your sim isn't really interested in reading up about the " + bookRecipeData.interestType.ToString() + " interest. They already know everything about this interest!");
            //    return false;
            //}

            //if (!InterestManager.HasTheNecessaryInterest(Actor.mSimDescription, bookRecipeData.interestType, false))
            //{
            //    greyedOutTooltipCallback = (() => "Your sim isn't really interested in reading up about the " + bookRecipeData.interestType.ToString() + " interest. Make sure to research an interest first before reading this book!");
            //    return false;
            //}

            // If sim hates interest...
            if (InterestManager.DoesSimHatesLovesInterest(Actor.mSimDescription, bookRecipeData.interestType) == 2 || InterestManager.DoesSimHatesLovesInterest(Actor.mSimDescription, bookRecipeData.interestType) == 3)
            {
                //greyedOutTooltipCallback = (() => Localization.LocalizeString(Actor.IsFemale, "Gameplay/Objects/BookRecipe:NoCookingSkill", Actor));
                greyedOutTooltipCallback = (() => "Your sim isn't really interested in reading up about the " + bookRecipeData.interestType.ToString() + " interest. Check if they hate this interest or do some more research on the interest.");
                return false;
            }
            //greyedOutTooltipCallback = (() => Localization.LocalizeString(Actor.IsFemale, "Gameplay/Objects/BookRecipe:NoCookingSkill", Actor));
            return true;
        }
    }
}
