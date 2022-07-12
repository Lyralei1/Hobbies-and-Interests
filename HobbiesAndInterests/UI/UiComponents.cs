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
}
