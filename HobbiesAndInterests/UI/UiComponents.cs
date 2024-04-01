using HobbiesAndInterests.HelperClasses;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.CAS;
using Sims3.UI.Hud;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Lyralei.UI
{
    public class InterestsOnSimDialog : ModalDialog
    {
        public Text mNameText;
        public Text mFamilyTreeText;
        public ScrollWindow mScrollWindow;
        List<Interest> mCurrentInterests = null;
        public IHudModel mHudModel;
        public Sim mCurrSim = null;
        public static InterestsOnSimDialog sDialog = null;
        public ItemGrid mFillBarGrid = null;
        public FillBarController mFillBar = null;

        public InterestsOnSimDialog(List<Interest> interests, Sim sim)
            : base("HUDLyraleiInterestsModal", 1, true, PauseMode.PauseSimulator, "OKCancelDialog") 
        {
            try
            {
                if ((WindowBase)base.mModalDialogWindow != (WindowBase)null)
                {
                    base.mModalDialogWindow.CenterInParent();
                    mCurrSim = sim;
                    mHudModel = HudController.Instance.Model;
                    mCurrentInterests = interests;
                    mNameText = (base.mModalDialogWindow.GetChildByID(110311168u, true) as Text);
                    mFamilyTreeText = (base.mModalDialogWindow.GetChildByID(110311172u, true) as Text);

                    //mScrollWindow = (base.mModalDialogWindow.GetChildByID(0x06933705, true) as ScrollWindow); //0x06933705
                    //mScrollWindow.Update();

                    mFillBarGrid = (base.mModalDialogWindow.GetChildByID(0x0df95003, true) as ItemGrid);
                    if (mFillBarGrid == null)
                    {
                        StyledNotification.Show(new StyledNotification.Format("mFillBarGrid is null ", StyledNotification.NotificationStyle.kDebugAlert));
                    }
                    // When grid has been selected, initiate the function.
                    //mFillBarGrid.ItemClicked += OnGridItemClick;
                    CreateInterestBar(interests);

                    Button button = base.mModalDialogWindow.GetChildByID(110311170u, true) as Button;
                    button.Click += OnOKButtonClick;
                    base.OkayID = 110311170u;
                }
            }
            catch(Exception ex)
            {
                StyledNotification.Show(new StyledNotification.Format("Main init: " + ex.Message + "Source: " + ex.Source, StyledNotification.NotificationStyle.kDebugAlert));
            }

        }

        public static void Show(List<Interest> interests, Sim sim)
        {
            if (sDialog == null)
            {
                sDialog = new InterestsOnSimDialog(interests, sim);
                sDialog.StartModal();
                sDialog = null;
            }
        }
        public void OnGridItemClick(WindowBase sender, ItemGridCellClickEvent eventArgs)
        {
        }

        public void OnOKButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
        {
            eventArgs.Handled = true;
            mCurrentInterests = null;
            EndDialog(eventArgs.ButtonID);
        }


        public ResourceKey mGridEntryLayoutKey = ResourceKey.kInvalidResourceKey;

        public void CreateInterestBar(List<Interest> interests)
        {
            try
            {
                foreach (Interest interest in interests)
                {
                    if (interest == null)
                    {
                        StyledNotification.Show(new StyledNotification.Format("interest is null ", StyledNotification.NotificationStyle.kDebugAlert));
                        continue;
                    }
                    mGridEntryLayoutKey = ResourceKey.CreateUILayoutKey("HUDLyraleiInterestsDialogEntry", 0u);
                    Window window = Sims3.UI.UIManager.LoadLayout(mGridEntryLayoutKey).GetWindowByExportID(1) as Window;

                    Text text = window.GetChildByID(0x00000241, true) as Text;
                    text.Caption = interest.Name;
                    text.AutoSize(true);

                    if (window == null)
                    {
                        StyledNotification.Show(new StyledNotification.Format("mGridEntryLayoutKey is null ", StyledNotification.NotificationStyle.kDebugAlert));
                    }
                    FillBarController mFillBar = window.GetChildByIndex(1u) as FillBarController;

                    if (mFillBar == null)
                    {
                        StyledNotification.Show(new StyledNotification.Format("Fillbar is null ", StyledNotification.NotificationStyle.kDebugAlert));
                    }

                    // Because EA's code is stupid, instantiate the components manually.
                    mFillBar.mClipWindow = mFillBar.GetChildByID(0x000fbc02, true) as Window;
                    mFillBar.mMinValue = 0f;
                    mFillBar.mMinValue = 20f;
                    mFillBar.mCurValue = 10f;
                    mFillBar.mFillBarDirection = 1u;
                    mFillBar.mMainColor = 0xff8cff5c;
                    mFillBar.mSecondaryColor = 0xffe8548f;
                    mFillBar.mFillBarWindow = mFillBar.GetChildByID(0x000fbc01, true) as Window;
                    mFillBar.mHudModel = this.mHudModel;
                    mFillBar.mbVertical = false;
                    mFillBar.mCheatWindow = mFillBar.GetChildByID(0x000fbc05, true) as Window;
                    mFillBar.mbCheatDragging = false;

                    CenterFillbar(true, false, mFillBar, mFillBarGrid);
                    //CenterFillbar(true, false, text, mFillBarGrid);
                    //mFillBar.CenterInParent();

                    if ((WindowBase)mFillBar != (WindowBase)null)
                    {
                        mFillBar.Initialize(0f, 20f, interest.currInterestPoints);
                        if (mHudModel.CheatsEnabled)
                        {
                            mFillBar.EnableCheatWindow(interest);
                            //mFillBar.CheatBarDragged += OnInterestDrag;
                        }
                    }
                    mFillBar.Invalidate();
                    mFillBarGrid.AddItem(new ItemGridCellItem(window, mFillBar));
                    //mScrollWindow.Update();
                }
            }
            catch(Exception ex)
            {
                StyledNotification.Show(new StyledNotification.Format("Fillbar init: " + ex.Message + "Source: " + ex.StackTrace, StyledNotification.NotificationStyle.kDebugAlert));
            }
        }

        public void CenterFillbar(bool horizontal, bool IsText, WindowBase fillbar, Window grid)
        {
            bool vertical = false;
            Rect area = fillbar.Area;
            Rect physicalArea = grid.PhysicalArea;
            float width = area.Width;
            float height = area.Height;
            float width2 = physicalArea.Width;
            float height2 = physicalArea.Height;
            float num = 0f;
            if (IsText && horizontal)
            {
                 num = ((float)Math.Round((double)((width2 - width) * 0.40f)));
            }
            else if (horizontal)
            {
                 num = horizontal ? ((float)Math.Round((double)((width2 - width) * 0.5f))) : area.TopLeft.x;
            }
            float num2 = vertical ? ((float)Math.Round((double)((height2 - height) * 0.5f))) : area.TopLeft.y;
            area.Set(num, num2, num + width, num2 + height);
            fillbar.Area = area;
        }

        public void OnInterestDrag(WindowBase sender, float value)
        {
            if (mHudModel.CheatsEnabled)
            {
                Interest interest = sender.Tag as Interest;
                if (interest != null)
                {
                    ChangeInterestPoint(interest, (int)value);
                }
            }
        }

        public void ChangeInterestPoint(Interest interest, int value)
        {
            // Update interests we see in the UI first.
            interest.currInterestPoints = value;

            //Now update the interest point for the sim.
            int indexToEdit = 0;
            for (int i = 0; i < InterestManager.mSavedSimInterests[mCurrSim.SimDescription.SimDescriptionId].Count; i++)
            {
                if(interest.mInterestsGuid == InterestManager.mSavedSimInterests[mCurrSim.SimDescription.SimDescriptionId][i].mInterestsGuid)
                {
                    indexToEdit = i;
                    break;
                }
            }
            InterestManager.mSavedSimInterests[mCurrSim.SimDescription.SimDescriptionId][indexToEdit].currInterestPoints = value;
        }

        public void UpdateInterestBar(Window parentWindow, float value)
        {
            if ((WindowBase)parentWindow != (WindowBase)null)
            {
                FillBarController fillBarController = parentWindow.GetChildByID(5u, true) as FillBarController;
                if ((WindowBase)fillBarController != (WindowBase)null)
                {
                    fillBarController.Value = value;
                }
            }
        }
	}


	public class EnergyCompanySelectionDialog : ModalDialog
	{
		public enum ControlIDs : uint
		{
			ButtonOK = 65333248u,
			WindowObjectImage = 107605763u,
			WindowSimImage = 107605764u,
			TextCareerName = 107605765u,
			TextCareerTitle = 107605766u,
			TextPayPerHour = 107605767u,
			TextHours = 107605768u,
			ScrollDescription = 107605769u,
			ScrollWindow = 107605770u,
			TextDaySunday = 107605792u,
			TextDayMonday = 107605793u,
			TextDayTuesday = 107605794u,
			TextDayWednesday = 107605795u,
			TextDayThursday = 107605796u,
			TextDayFriday = 107605797u,
			TextDaySaturday = 107605798u,
			WindowCurrentDayArrowStart = 107605808u,
			CareerCycleLeftButton = 114633984u,
			CareerCycleRightButton = 114633985u,
			JobIconWin = 114633986u,
			SeeLocationButton = 114633987u,
			CancelButton = 114633988u
		}

		public IHudModel mHudModel;

		public EnergyManager.EnergyCompany mSelectedEnergyCompany;

		public List<EnergyManager.EnergyCompany> mCareerEntries;

		public bool mbResult;

		public bool mIsFemale;

		public Color kDayTextNotWorkingColor = new Color(2155905152u);

		public Color kDayTextWorkingColor = new Color(4278198336u);

		public bool mWasMapview;

		public static string kLayoutName = "EnergyCompanySelectorUI";

		public static int kExportID = 1;

		public static EnergyManager.EnergyCompany Show()
		{
			if (ScreenGrabController.InProgress)
			{
				return null;
			}
			Responder.Instance.HudModel.RestoreUIVisibility();
			using (EnergyCompanySelectionDialog careerSelectionDialog = new EnergyCompanySelectionDialog())
			{
				careerSelectionDialog.StartModal();
				return careerSelectionDialog.mbResult ? careerSelectionDialog.mSelectedEnergyCompany : null;
			}
		}

		public override bool OnEnd(uint endID)
		{
			mbResult = endID == base.OkayID && mSelectedEnergyCompany != null;
			Responder.Instance.OptionsModel.UIDisableSave = false;
			int currentIndex = mCareerEntries.IndexOf(mSelectedEnergyCompany);
			UIManager.DarkenBackground(false);
			SetGameUIVisibility(true);
			BorderTreatmentsController.SetButtonEnabled(true);
			UIManager.GetSceneWindow().MapViewModeEnabled = mWasMapview;
			WindowBase modalWindow = UIManager.GetModalWindow();
			if (modalWindow == mModalDialogWindow)
			{
				UIManager.EndModal(mModalDialogWindow);
			}
			PieMenu.Hide();
			UIManager.SetOverrideCursor(0u);
			return base.OnEnd(endID);
		}

		public void SetGameUIVisibility(bool visible)
		{
			UIManager.GetUITopWindow().GetChildByID(57857282u, true).Visible = visible;
			UIManager.GetUITopWindow().GetChildByID(57857283u, true).Visible = visible;
			UIManager.GetUITopWindow().GetChildByID(57857291u, true).Visible = visible;
			UIManager.GetUITopWindow().GetChildByID(57857293u, true).Visible = visible;
			UIManager.GetUITopWindow().GetChildByID(57857296u, true).Visible = visible;
			UIManager.GetUITopWindow().GetChildByID(57857295u, true).Visible = visible;
		}

		public EnergyCompanySelectionDialog()
			: base(kLayoutName, kExportID, false, PauseMode.PauseSimulator, null)
		{
			mEnableBackgroundDarkening = false;
			mHudModel = HudController.Instance.Model;

			if (EnergyManager.AllCompanies.Count == 0)
				return;
			mCareerEntries = EnergyManager.AllCompanies;

			Window window = mModalDialogWindow.GetChildByID(107605763u, true) as Window;
			//ImageDrawable imageDrawable = window.Drawable as ImageDrawable;
			//imageDrawable.Image = UIManager.GetThumbnailImage(Responder.Instance.HudModel.GetThumbnailForGameObject(mCareerSelectionModel.InteractionObjectGuid));
			//window.Invalidate();

			window = mModalDialogWindow.GetChildByID(107605764u, true) as Window;
			//imageDrawable = window.Drawable as ImageDrawable;
			//imageDrawable.Image = UIManager.GetThumbnailImage(Responder.Instance.HudModel.GetThumbnailForGameObject(mCareerSelectionModel.SimGuid));
			//window.Invalidate();

			Button button = mModalDialogWindow.GetChildByID(65333248u, true) as Button;
			button.Click += OnAcceptEnergyCompany;
			button = mModalDialogWindow.GetChildByID(114633988u, true) as Button;
			button.Click += OnCancelButtonClick;
			button = mModalDialogWindow.GetChildByID(114633984u, true) as Button;
			if (mCareerEntries.Count > 1)
			{
				button.Click += OnCareerSelectionChanged;
			}
			else
			{
				button.Visible = false;
			}
			button = mModalDialogWindow.GetChildByID(114633985u, true) as Button;
			if (mCareerEntries.Count > 1)
			{
				button.Click += OnCareerSelectionChanged;
			}
			else
			{
				button.Visible = false;
			}
			//button = mModalDialogWindow.GetChildByID(114633987u, true) as Button;
			//button.Click += OnSeeLocationButtonClick;
			FillEnergyCompanyInfo(mCareerEntries[0]); // Null reference?
			//EnableDisableAcceptCareerButton(mCareerSelectionModel, mCareerEntries[0]);

			uint id = (uint)(107605808 + mHudModel.CurrentDay);
			window = mModalDialogWindow.GetChildByID(id, true) as Window;
			window.Visible = true;
			mSelectedEnergyCompany = mCareerEntries[0];

			//mCareerSelectionModel.CareerSelected(mSelectedCareer);

			base.OkayID = 65333248u;
			base.CancelID = 114633988u;
			mWasMapview = UIManager.GetSceneWindow().MapViewModeEnabled;
			UIManager.DarkenBackground(true);
			UIManager.BeginModal(mModalDialogWindow);
			UIManager.SetOverrideCursor(95342848u);
			Responder.Instance.OptionsModel.UIDisableSave = true;
		}

		public bool OnPreSaveGame()
		{
			return false;
		}

		public void OnCareerSelectionChanged(WindowBase sender, UIButtonClickEventArgs eventArgs)
		{
			int num = mCareerEntries.IndexOf(mSelectedEnergyCompany);
			int count = mCareerEntries.Count;
			num = ((sender.ID == 114633984) ? (num + 1) : (num + (count - 1))) % count;
			mSelectedEnergyCompany = mCareerEntries[num];
			FillEnergyCompanyInfo(mSelectedEnergyCompany);
			//EnableDisableAcceptCareerButton(mCareerSelectionModel, mSelectedCareer);
			if (mSelectedEnergyCompany != null)
			{
				//mCareerSelectionModel.CareerSelected(mSelectedCareer);
				UIManager.GetSceneWindow().MapViewModeEnabled = false;
			}
			eventArgs.Handled = true;
		}

		public void OnAcceptEnergyCompany(WindowBase sender, UIButtonClickEventArgs eventArgs)
		{
			EndDialog(base.OkayID);
			eventArgs.Handled = true;
		}

		public void OnCancelButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
		{
			EndDialog(base.CancelID);
			eventArgs.Handled = true;
		}

		public void FillEnergyCompanyInfo(EnergyManager.EnergyCompany entry)
		{

			if (entry != null)
			{
				Text text = mModalDialogWindow.GetChildByID(107605765u, true) as Text;
				text.Caption = entry.NameCompany;
				text = mModalDialogWindow.GetChildByID(107605766u, true) as Text;

				float getPrice = 300 * entry.NewPeakTarif;

				text.Caption = "Estimated cost p/w: $" + getPrice.ToString();
				text = mModalDialogWindow.GetChildByID(107605769u, true) as Text;
				text.Caption = entry.DescCompany;
				text.AutoSize(true);
				text.Position = new Vector2(0f, 0f);
				ScrollWindow scrollWindow = mModalDialogWindow.GetChildByID(107605770u, true) as ScrollWindow;
				scrollWindow.Update();

				text = mModalDialogWindow.GetChildByID(107605767u, true) as Text;
				text.Caption = "Off peak: " + entry.NewOffPeakTarif.ToString() + " (was: " + entry.OffPeakTarif.ToString() + "), Peak: " + entry.NewPeakTarif.ToString() + " (was: " + entry.PeakTarif.ToString() + ")";
				// It's working till here...

				text = mModalDialogWindow.GetChildByID(107605768u, true) as Text;
				//text.Caption = (entry.HasOpenHours ? Responder.Instance.LocalizationModel.LocalizeString("UI/Caption/CareerSelection:OpenHours") : entry.CareerOfferWorkHours);
				HideWorkDays();

				//if (entry.IsActive && entry.HasOpenHours)
				//{
				//	HideWorkDays();
				//}
				//else
				//{
				//	UpdateDaysofWeek(entry.CareerOfferWorkDays);
				//}

				//Button button = mModalDialogWindow.GetChildByID(114633987u, true) as Button;
				//button.Visible = true;

				//if (entry.IsActive && entry.ActiveCareerLotID == 0)
				//{
				//	button.Visible = false;
				//}
				//else
				//{
				//	button.Visible = true;
				//}

				Window window = mModalDialogWindow.GetChildByID(114633986u, true) as Window;
				//(window.Drawable as ImageDrawable).Image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(entry.CareerIconColored, 0u));
				//window.Invalidate();
			}
		}

		//public void EnableDisableAcceptCareerButton(ICareerSelectionModel model, IOccupationEntry entry)
		//{
		//	if (entry == null)
		//	{
		//		return;
		//	}
		//	GreyedOutTooltipCallback greyedOutTooltipCallback = null;
		//	Button button = mModalDialogWindow.GetChildByID(65333248u, true) as Button;
		//	if (entry.CanAcceptCareer(model.SimGuid, ref greyedOutTooltipCallback))
		//	{
		//		button.Enabled = true;
		//		button.TooltipText = string.Empty;
		//		return;
		//	}
		//	button.Enabled = false;
		//	if (greyedOutTooltipCallback != null)
		//	{
		//		button.TooltipText = greyedOutTooltipCallback();
		//	}
		//	else
		//	{
		//		button.TooltipText = string.Empty;
		//	}
		//}

		//public void UpdateDaysofWeek(string workDays)
		//{
		//	for (uint num = 107605792u; num <= 107605798; num++)
		//	{
		//		Text text = mModalDialogWindow.GetChildByID(num, true) as Text;
		//		text.Visible = true;
		//		text.TextColor = kDayTextNotWorkingColor;
		//		text.AutoSize(false);
		//	}
		//	char[] array = workDays.ToCharArray();
		//	foreach (char c in array)
		//	{
		//		uint num2 = 0u;
		//		switch (c)
		//		{
		//			case 'U':
		//				num2 = 107605792u;
		//				break;
		//			case 'M':
		//				num2 = 107605793u;
		//				break;
		//			case 'T':
		//				num2 = 107605794u;
		//				break;
		//			case 'W':
		//				num2 = 107605795u;
		//				break;
		//			case 'R':
		//				num2 = 107605796u;
		//				break;
		//			case 'F':
		//				num2 = 107605797u;
		//				break;
		//			case 'S':
		//				num2 = 107605798u;
		//				break;
		//		}
		//		if (num2 != 0)
		//		{
		//			Text text2 = mModalDialogWindow.GetChildByID(num2, true) as Text;
		//			if (text2 != null)
		//			{
		//				text2.TextColor = kDayTextWorkingColor;
		//			}
		//		}
		//	}
		//}

		public void HideWorkDays()
		{
			for (uint num = 107605792u; num <= 107605798; num++)
			{
				Text text = mModalDialogWindow.GetChildByID(num, true) as Text;
				text.Visible = false;
			}
		}
	}


}
