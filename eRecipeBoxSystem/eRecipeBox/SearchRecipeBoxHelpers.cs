/*
* MIT License
* 
* Copyright (C) 2024 SoftArc, LLC
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using DataStoreUtils;
using DevExpress.Data.Filtering;
using DevExpress.Utils;
using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraScheduler;
using DevExpress.XtraScheduler.Commands;
using DevExpress.XtraScheduler.Services;
using eRecipeBox.HelperForms;
using Foundation;
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Forms = System.Windows.Forms;

namespace eRecipeBox
{
    //#RQT 1SrchRcBxFrm SearchRecipeBox behavior -
    //#RQT 1SrchRcBxFrm Enter FilterTerms, one at a time, and click Filter.
    //#RQT 1SrchRcBxFrm Each additional FilterTerm is AND-ed with the previous FilterTerms.
    //#RQT 1SrchRcBxFrm System displays results in the grid and row count in status bar.
    //#RQT 1SrchRcBxFrm User can update MyRating and Flagged directly in the grid without opening the RecipeCard.
    //#RQT 1SrchRcBxFrm User can also edit/delete the last CookedDate or add a new CookedDate directly in the grid without opening the RecipeCard.
    //#RQT 1SrchRcBxFrm Drag and drop filter results grid RecipeCard onto Calendar to schedule RecipeCard.
    //#RQT 1SrchRcBxFrm Also, can Copy grid RecipeCard onto Calendar to schedule RecipeCard.
    //#RQT 1SrchRcBxFrm Drag and drop events within Calendar to reschedule RecipeCard(s). 
    //#RQT 1SrchRcBxFrm Also, Cut/Paste events within Calendar to reschedule RecipeCard(s). 
    //#RQT 1SrchRcBxFrm Also, Copy/Paste events within Calendar to create additional cookedDishes for these RecipeCard(s). 
    //#RQT 1SrchRcBxFrm Calendar: Hold CNTL key to multi-select Calendar events.
    //#RQT 1SrchRcBxFrm Calendar: Hold SHFT key to select all events between two days (all events on all these days)
    //#RQT 1SrchRcBxFrm User can also dragNdrop RecipeCard(s) events from Calendar onto the RHS (months) navigation calendar.
    partial class SearchRecipeBox
    {
        #region form properties
        private RepositoryItemHypertextLabel titleHyperLinkEdit = new RepositoryItemHypertextLabel();

        //These are used to support entering a new FilterTerm
        //Used by search parameter lookupedit to autofilter and search on term not in the list 
        Keys LastLookupEditKey { get; set; }
        private string NewFilterTerm { get; set; }

        //Used by the schedular control 
        private GridHitInfo DownHitInfo { get; set; }
        private bool IsRefreshingAppointments { get; set; }
        private bool RefreshIsRequired { get; set; }
        private int NumberOfSelectedAppointments { get; set; }
        private List<CookedDishKey> cookedDishKeysToDelete = new List<CookedDishKey>();

        private bool SchedulerHasFocus = false;

        SearchFormClipboard searchFormClipboard { get; set; }

        private int? WeekViewPosition { get; set; }
        private int? MonthViewPosition { get; set; }

        private bool ConnectedWithWCF = false;

        const string shrinkCaption = "Shrink\nCalendar";
        const string growCaption = "Grow\nCalendar";
        const string hideCaption = "Hide\nCalendar";
        const string showCaption = "Show\nCalendar";
        const string monthViewCaption = "Month\nView";
        const string weekViewCaption = "Week\nView";
        #endregion

        #region form methods

        private void InitializeComponent2()
        {
            ConnectedWithWCF = AppSettings.GetBoolAppSetting("UseWCF");

            this.growCalendarBarButtonItem.Caption = growCaption;
            bool systemTestLogging = AppSettings.GetBoolAppSetting("SystemTestLogging");

            searchFormClipboard = new SearchFormClipboard();

            //#RQT ShCut Shortcut keys: Alt-c (Close app), Alt-v (viewRecipeCard), Alt-e (editRecipeCard), Alt-n (createNewRecipeCard), Alt-g (editGroceryList),  Alt-a (addToGroceryList), Alt-f (Filter), Alt-L (clear input), Alt-i (move cursor to the FilterTerm (input)), Alt-t (Today),
            //#RQT ShCut Shft-e (Edit MyRating) 
            //#RQT ShCut Shft-e (Edit LastCookedDate), Shft-a (Add New CookedDate), Shft-d (Delete LastCookedDate)
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(closeBarButtonItem, Keys.Alt, Keys.C); //Ctl-C is copy to clipboard.  Use alt
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(viewRecBarButtonItem, Keys.Alt, Keys.V);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(openRecipeBarButtonItem, Keys.Alt, Keys.E);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(newRecipeBarButtonItem, Keys.Alt, Keys.N);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(groceryListBarButtonItem, Keys.Alt, Keys.G);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(addToGLBarButtonItem, Keys.Alt, Keys.A);
            SimpleCRUDFramework.DevExpressUxUtils.AssignShortcutToButton(tdyDtBrBtnItm, Keys.Alt, Keys.T);

            filterSimpleButton.ToolTip = "Alt+F";
            clearFilterSimpleButton.ToolTip = "Alt+L";  //Alt-C is used for close, so we use Alt-L for clear search

            searchResultsGridControl.RepositoryItems.Add(titleHyperLinkEdit);

            //schedular
            //Week view is being phased out
            //https://supportcenter.devexpress.com/ticket/details/t545133/wrong-weekview-when-work-stars-on-sunday/
            dateNavigator.ShowTodayButton = true;
            dateNavigator.ShowWeekNumbers = false;
            schedulerControl.ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Month;
            schedulerControl.MonthView.CompressWeekend = false;
            schedulerControl.MonthView.WeekCount = 1;
            schedulerControl.MonthView.CompressWeekend = false;
            schedulerControl.OptionsCustomization.AllowAppointmentResize = UsedAppointmentType.None;
            schedulerControl.OptionsCustomization.AllowDisplayAppointmentForm = AllowDisplayAppointmentForm.Never;
            schedulerControl.OptionsCustomization.AllowInplaceEditor = UsedAppointmentType.None;
            schedulerControl.OptionsCustomization.AllowAppointmentCreate = UsedAppointmentType.None;
            showCalendarBarButtonItem.ButtonStyle = BarButtonStyle.Check;
            monthWeekBarButtonItem.ButtonStyle = BarButtonStyle.Check;

            //#TODO student exercise Get this to work... currently, the event is fired separately
            //borrowed from here 
            ////https://supportcenter.devexpress.com/ticket/details/t374228/schedulerstorage-appointmentsdeleted-event-fired-separately-for-single-deletion-of
            //ISchedulerCommandFactoryService service = schedulerControl.GetService<ISchedulerCommandFactoryService>();
            //schedulerControl.RemoveService(typeof(ISchedulerCommandFactoryService));
            //schedulerControl.AddService(typeof(ISchedulerCommandFactoryService), new CustomSchedulerCommandFactoryServiceWrapper(service, schedulerControl));

            ShowCalendar();

            monthWeekBarButtonItem.Enabled = true;
            monthWeekBarButtonItem.ImageOptions.SvgImage = this.svgImageCollection1[1];
            monthWeekBarButtonItem.Caption = monthViewCaption; //press to switch to month

            //filter results
            //https://supportcenter.devexpress.com/ticket/details/t600628/the-hyperlinkedit-does-not-open-links
            searchResultsGridView.OptionsSelection.MultiSelect = false;
            searchResultsGridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
            searchResultsGridView.OptionsView.ShowIndicator = false; //carrot
            searchResultsGridView.OptionsBehavior.EditorShowMode = EditorShowMode.MouseDownFocused;  //best approach.  first click to get cell focused, then can hyperclick
            searchResultsGridView.OptionsBehavior.ReadOnly = false;
            searchResultsGridView.OptionsView.ShowAutoFilterRow = false; // filter at top of each row
            searchResultsGridView.OptionsView.ShowGroupPanel = false; //no grouping panel
            searchResultsGridView.OptionsCustomization.AllowGroup = false; //no grouping  
            searchResultsGridView.OptionsFind.AlwaysVisible = false; //no find panel.  we do our own search
            searchResultsGridView.OptionsFind.AllowFindPanel = false; //no find panel.  we do our own search
            searchResultsGridView.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Default;  //filter panel at bottom

            this.LastLookupEditKey = Keys.None;

            //#RQT FltrTrmInptCntns FilterTerm LookupEdit shows matches that contain the entered text.
            SimpleCRUDFramework.DevExpressUxUtils.InitLookupEditorBusObj(filterTermLookUpEdit, searchArgumentXPBindingSource, "Oid", "Name", DefaultBoolean.False);
            filterTermLookUpEdit.Properties.SortColumnIndex = 1;
            filterTermLookUpEdit.Properties.Columns[filterTermLookUpEdit.Properties.SortColumnIndex].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;

            SetControlConstraints();
        }

        private void ConfigureSearchInput()
        {
            Dictionary<string, FilterTerm> allTerms = new Dictionary<string, FilterTerm>();
            int counter = 1;
            //#RQT FltrTrmInptLkUps List of suggested FilterTerms in the LookupEdit contains all Keywords, Items and RecipeCard.Titles - no duplicates.
            using (var uow = new UnitOfWork())
            {
                //add item names
                var iList = uow.Query<Item>();
                foreach (Item item in iList)
                {
                    FilterTerm newS = new FilterTerm(counter++, item.Name);
                    allTerms[item.Name] = newS;
                }

                //add keyword names
                var kList = uow.Query<Keyword>();
                foreach (Keyword key in kList)
                {
                    FilterTerm newS = new FilterTerm(counter++, key.Name);
                    allTerms[key.Name] = newS;
                }

                //add recipe titles
                int recipeBoxOid = DataStoreServiceReference.DataStoreServiceReference.MyRecipeBoxOid;
                var rcList = uow.Query<RecipeCard>().Where(x => x.RecipeBox.Oid == recipeBoxOid);
                foreach (RecipeCard rc in rcList)
                {
                    FilterTerm newS = new FilterTerm(counter++, rc.Title);
                    allTerms[rc.Title] = newS;
                }
            }
            var sortedSearchFilterTerms = allTerms.Values.OrderBy(x => x.Name).ToList();
            searchArgumentXPBindingSource.DataSource = sortedSearchFilterTerms;
            filterTermLookUpEdit.EditValue = null;
            filterTermLookUpEdit.Text = null;
            NewFilterTerm = null;
        }

        private void RefreshSearchResults()
        {
            ConfigureSearchInput();

            //the link below shows how to use events to update view status.  KISS: We just Refresh the entire view when underlying data has changed.
            //https://supportcenter.devexpress.com/ticket/details/e4505/how-to-implement-crud-operations-using-xtragrid-and-xpinstantfeedbacksource
            xpInstantFeedbackView1.Refresh();
        }

        private void RefreshAppointments()
        {
            schedulerControl.BeginUpdate();
            try
            {
                schedulerControl.DataStorage.Appointments.Clear();
                using (UnitOfWork uow = new UnitOfWork())
                {
                    //#TIP.  this flag tells the Inserted event handler that this is the original bind to view and not a d&drop insert that needs to be persisted
                    IsRefreshingAppointments = true;

                    XPView xpView = new XPView(uow, typeof(CookedDish));
                    xpView.Properties.AddRange(new ViewProperty[] {
new ViewProperty("Oid", SortDirection.None, "[Oid]",false, true),
new ViewProperty("CookedDate", SortDirection.None, "[CookedDate]",false, true),
new ViewProperty("RCOid", SortDirection.None, "[RecipeCard.Oid]",false, true),
new ViewProperty("RCTitle", SortDirection.None, "[RecipeCard.Title]",false, true)
            });
                    int back12months = -1 * (365);
                    //#TODO ENHANCEMENT Bind DataStorage.Appointments to a server mode scrolling view for best performance.
                    //For now, a minor performance enhancement.  Only load last 12 months into the calendar
                    string twelveMonthsAgo = DateTime.Now.AddDays(back12months).ToString("yyyy-MM-dd");
                    int? myRecipeBoxOid = DataStoreServiceReference.DataStoreServiceReference.MyRecipeBoxOid;
                    xpView.Criteria = CriteriaOperator.Parse($"[RecipeCard.RecipeBox] = {myRecipeBoxOid} AND [CookedDate] >= '{twelveMonthsAgo}'", null);
                    foreach (ViewRecord cookDish in xpView)
                    {
                        // Create a new appointment.  
                        Appointment apt = schedulerControl.DataStorage.CreateAppointment(AppointmentType.Normal);
                        apt.Subject = (string)cookDish["RCTitle"]; //Title to Subject
                        apt.Start = (DateTime)cookDish["CookedDate"]; //CookedDate to Start
                        apt.AllDay = true;
                        CookedDishKey key = new CookedDishKey();
                        key.CookedDishOid = PrimitiveUtils.ObjectToInt(cookDish["Oid"]);
                        key.RecipeCardOid = PrimitiveUtils.ObjectToInt(cookDish["RCOid"]);
                        key.CookedDishDate = apt.Start;

                        //#TIP Store persistent key IDs in the appointment LabelKey since we are using unbound mode
                        apt.LabelKey = key;

                        //Add the new appointment to the appointment collection.  
                        schedulerControl.DataStorage.Appointments.Add(apt);
                    }
                }
            }
            finally
            {
                IsRefreshingAppointments = false;
                schedulerControl.EndUpdate();
            }
        }

        protected override void RefreshForm()
        {
            RefreshSearchResults();
            RefreshAppointments();
        }

        protected override void UpdateViewState()
        {
            if (ConnectedWithWCF)
                this.adminRibbonPage.Visible = false;
            else
                this.adminRibbonPage.Visible = true;

            //#RQT BtnVwEdRecCrdEnbl Only enable View/Edit/DeleteRecipeCard if we have focus on a valid RecipeCard.
            if ((this.searchResultsGridView.IsFocusedView && searchResultsGridView.FocusedRowHandle >= 0)
                || (SchedulerHasFocus && schedulerControl.SelectedAppointments.Count > 0))
            {
                this.viewRecBarButtonItem.Enabled = true;
                this.openRecipeBarButtonItem.Enabled = true;
                this.addToGLBarButtonItem.Enabled = true;
                this.scrollResultsToTopBarButtonItem.Enabled = true;
                //#RQT BtnDelRecCrdEnbl Enable delete RecipeCard button only when grid is focused on a RecipeCard; never when Calendar has focus.
                if (this.searchResultsGridView.IsFocusedView)
                    this.deleteRecipeBarButtonItem.Enabled = true;
                else
                    this.deleteRecipeBarButtonItem.Enabled = false;

            }
            else
            {
                this.viewRecBarButtonItem.Enabled = false;
                this.openRecipeBarButtonItem.Enabled = false;
                this.deleteRecipeBarButtonItem.Enabled = false;
                this.addToGLBarButtonItem.Enabled = false;
                this.scrollResultsToTopBarButtonItem.Enabled = false;
            }

            //#WORKAROUND  #TRICKY  #TODO This not a pretty workaround, but 
            //searchParmLookUpEdit_ProcessNewValue_1 is fired twice, before and after filtering.
            //ConfigureCommandButtons is called after the 2nd time searchParmLookUpEdit_ProcessNewValue_1 is fired, so we clear NewSearchFilterTerm here
            NewFilterTerm = null;
        }

        private void EditLastCookedDate(ButtonEdit parentBtnEdit, EditorButton clickedButton, GridView recipeListGridView)
        {
            int focusedRowHandle = recipeListGridView.FocusedRowHandle;
            if (focusedRowHandle < 0)
                return;

            bool isEditDate = false;
            bool isAddDate = false;
            bool isDeleteDate = false;

            if (clickedButton == parentBtnEdit.Properties.Buttons[0])
                isEditDate = true;
            if (clickedButton == parentBtnEdit.Properties.Buttons[1])
                isAddDate = true;
            if (clickedButton == parentBtnEdit.Properties.Buttons[2])
                isDeleteDate = true;

            //Get the recipe PK for the focused row
            int oid = (int)recipeListGridView.GetRowCellValue(focusedRowHandle, recipeListGridView.Columns[nameof(RecipeCard.Oid)]);

            //load recipe from database
            UnitOfWork uow = new UnitOfWork();
            RecipeCard recipeCard = (RecipeCard)XpoService.LoadHeadBusObjByKey(typeof(RecipeCard), oid, uow);
            //assert not null
            if ((isEditDate || isDeleteDate) && recipeCard.CookedInstances.Count == 0)
                return;

            //first, handle delete case
            if (isDeleteDate)
            {
                DateTime date = (DateTime)recipeListGridView.GetRowCellValue(focusedRowHandle, recipeListGridView.Columns["Last"]);
                string question = string.Format("Confirm delete last cooked date {0}?", date.ToString("d"));
                //#AtmtdTsting  session.Confirmation Yes No
                Forms.DialogResult answer = ShowMessageBox(owner: MdiParent, question, "Confirmation", MessageBoxButtons.YesNo);
                if (answer == Forms.DialogResult.Yes)
                {
                    var lastCooked = recipeCard.FindCookedDish(date);
                    recipeCard.DeleteCookedDish(lastCooked);
                    XpoService.CommitBodyOfBusObjs(recipeCard, uow);
                    RefreshForm();
                    //#RQT GrdLstCkdDtDelScr Scroll to the top of the grid after deleting a last cooked date from the search results grid.
                    SimpleCRUDFramework.DevExpressUxUtils.ScrollToTop(recipeListGridView);
                }
                return;
            }

            //next, handle the [edit, add] last cooked date cases...           
            DateTime? dateToEdit = (DateTime?)recipeListGridView.GetRowCellValue(focusedRowHandle, recipeListGridView.Columns["Last"]);

            if (isEditDate && dateToEdit == null)
                return;

            string caption = null;
            //if editing the last date, need to pass it into EditDateDialog
            if (isEditDate)
            {
                //#AtmtdTstng
                caption = "Edit Last Cooked Date";
            }
            else
            {
                //#AtmtdTstng
                caption = "Add New Cooked Date";
                dateToEdit = DateTime.Today; //default is to add a new cooked date of Today
            }

            SimpleCRUDFramework.HelperForms.EditDateDialog edd = new SimpleCRUDFramework.HelperForms.EditDateDialog(caption, "Date", dateToEdit);
            Forms.DialogResult dr = edd.ShowDialog(); // owner: this.MdiParent
            if (dr == Forms.DialogResult.OK)
            {
                //Allow user to edit/change a new cooked date to the selected recipe in the search grid
                if (isEditDate)
                {
                    CookedDish crToEdit = recipeCard.FindCookedDish((DateTime)dateToEdit);
                    if (crToEdit == null)
                        throw new Exception("logic error.  this should not be null.  perhaps optimistic concurrency issue.");
                    crToEdit.EditCookedDishDate((DateTime)edd.Date);
                }
                //Allow user to add a new cooked date to the selected recipe in the search grid
                else if (isAddDate)
                {
                    CookedDish newCooked = new CookedDish(uow);
                    newCooked.CookedDate = (DateTime)edd.Date;
                    recipeCard.AddCookedDish(newCooked);
                }
                XpoService.CommitBodyOfBusObjs(recipeCard, uow);
                RefreshForm();
                //#RQT GrdCkdDtEdtStFcs Set focus to the edited RecipeCard after adding/editing LastCookedDate in grid.
                SimpleCRUDFramework.DevExpressUxUtils.FocusTo(recipeListGridView, oid);
            }
        }
        private bool ContainsSearchTerm(string searchTerm, BindingList<FilterTerm> inList)
        {
            foreach (FilterTerm searchArg in inList)
            {
                if (searchArg.Name == searchTerm)
                    return true;
            }
            return false;
        }

        private void ShowCalendar()
        {
            splitContainerControl1.PanelVisibility = SplitPanelVisibility.Both;
            showCalendarBarButtonItem.Caption = hideCaption;
            shrinkCalendarBarButtonItem.Enabled = true;
            growCalendarBarButtonItem.Enabled = true;
            monthWeekBarButtonItem.Enabled = true;
            tdyDtBrBtnItm.Enabled = true;
        }
        private void HideCalendar()
        {
            splitContainerControl1.PanelVisibility = SplitPanelVisibility.Panel1;
            showCalendarBarButtonItem.Caption = showCaption;
            shrinkCalendarBarButtonItem.Enabled = false;
            growCalendarBarButtonItem.Enabled = false;
            monthWeekBarButtonItem.Enabled = false;
            tdyDtBrBtnItm.Enabled = false;
        }

        private void SelectAllApptsBetween()
        {
            //#RQT ClndrClckShft Consistent with Exel's grid, Shift+Click selects all recipes between two dates and Cntl+Click union selects clicked events on the calendar  
            if (schedulerControl.SelectedAppointments.Count == 2)
            {
                DateTime? beginDate = null;
                DateTime? endDate = null;
                foreach (Appointment appt in schedulerControl.SelectedAppointments)
                {
                    if (beginDate == null)
                        beginDate = appt.Start;
                    else
                        endDate = appt.Start;
                }
                //first selected appt to 2nd selected appt 
                if (((DateTime)endDate - (DateTime)beginDate).TotalDays < 0)
                    (beginDate, endDate) = (endDate, beginDate); //swap                    
                                                                 //select all appointmenets between the 2 ends
                for (int i = 0; i < schedulerDataStorage1.Appointments.Count; i++)
                {
                    var appt = schedulerDataStorage1.Appointments[i];
                    if (appt.Start >= beginDate && appt.Start <= endDate)
                    {
                        schedulerControl.SelectedAppointments.Add(appt);
                    }
                }
            }
        }

        private void SchedulerCopyCut(SearchFormClipboard.CopyActions copyAction)
        {
            if (schedulerControl.SelectedAppointments.Count == 0)
                return;
            searchFormClipboard.Clipboard.Clear();
            searchFormClipboard.ClipboardActor = SearchFormClipboard.ClipboardActors.CALENDAR;
            searchFormClipboard.CopyAction = copyAction;
            foreach (var appointment in schedulerControl.SelectedAppointments)
            {
                searchFormClipboard.Clipboard.Add(((CookedDishKey)appointment.LabelKey).Clone());
            }
        }
        private void SchedulerPaste()
        {
            if (!(searchFormClipboard.Clipboard.Count > 0 && schedulerControl.SelectedInterval.Duration != TimeSpan.Zero
                && schedulerControl.SelectedInterval.Duration.Days <= 2))
                return;

            if (searchFormClipboard.Clipboard.Count == 0)
                return;

            List<CookedDishKey> cookedDishKeys = new List<CookedDishKey>();
            foreach (CookedDishKey key in searchFormClipboard.Clipboard)
                cookedDishKeys.Add(key);
            //#AtmtTstng #TRICKY Force list of cookedDishKey order to be deterministic to support automated testing.
            cookedDishKeys = cookedDishKeys.OrderBy(cdk => cdk.RecipeCardOid).ToList();

            int copyMoveDayOffset = int.MinValue;
            //number of days to move each cookedDish
            if (searchFormClipboard.ClipboardActor == SearchFormClipboard.ClipboardActors.CALENDAR)
                copyMoveDayOffset = (int)(schedulerControl.SelectedInterval.Start - (DateTime)searchFormClipboard.Clipboard[0].CookedDishDate).TotalDays;

            schedulerControl.BeginUpdate();
            try
            {
                foreach (CookedDishKey cdKey in cookedDishKeys)
                {
                    //although least performing, the safest way to mass reschedule is to do one at a time, each in its own transaction
                    using (UnitOfWork uow = new UnitOfWork())
                    {
                        RecipeCard rec = (RecipeCard)XpoService.LoadHeadBusObjByKey(typeof(RecipeCard), cdKey.RecipeCardOid, uow);
                        if (rec == null)
                            return;
                        CookedDish cookedDish = null;

                        //if we are copying/moving the cookedDish, need to fetch it
                        if (cdKey.CookedDishOid != null)
                        {
                            cookedDish = rec.FindCookedDish((int)cdKey.CookedDishOid);
                            if (cookedDish == null)
                                return;
                            //#TRICKY if the cut to clipboard date is different, someone else modified the cookedDish
                            if (cdKey.CookedDishDate != cookedDish.CookedDate)
                            {
                                ShowMessageBox("Error: Recipe schedule has since been modified.  Refreshing calendar.", "Concurrency Error");
                                return;
                            }
                        }
                        DateTime newDate;
                        if (searchFormClipboard.ClipboardActor == SearchFormClipboard.ClipboardActors.CALENDAR)
                            newDate = cookedDish.CookedDate.AddDays((double)copyMoveDayOffset);
                        else
                            newDate = schedulerControl.SelectedInterval.Start; //special case where copy 1 recipe from grid

                        if (searchFormClipboard.CopyAction == SearchFormClipboard.CopyActions.COPY)
                        {
                            CookedDish newCookedDish = new CookedDish(uow);
                            newCookedDish.CookedDate = newDate;
                            rec.AddCookedDish(newCookedDish);
                        }
                        else
                            cookedDish.EditCookedDishDate(newDate);

                        XpoService.CommitBodyOfBusObjs(rec, uow);
                    }
                }
            }
            finally
            {
                schedulerControl.EndUpdate();
            }
            RefreshForm();
        }

        private void DoFilter(LookUpEdit searchParmLookUpEdit, GridView gridView, Image editImage, Image addImage, Image deleteImage)
        {
            //--------------------------------------------
            //Configure the search results grid
            //--------------------------------------------
            //edit cooked instances
            RepositoryItemButtonEdit edtLstCkdDateBtn = new RepositoryItemButtonEdit();
            edtLstCkdDateBtn.DisplayFormat.FormatType = FormatType.DateTime;
            edtLstCkdDateBtn.DisplayFormat.FormatString = "d";
            edtLstCkdDateBtn.EditFormat.FormatType = FormatType.DateTime;
            edtLstCkdDateBtn.EditFormat.FormatString = "d";
            //edit last cooked date

            edtLstCkdDateBtn.Buttons[0].Kind = ButtonPredefines.Ellipsis;
            edtLstCkdDateBtn.Buttons[0].Caption = "Edit Last Cooked Date";

            //#WORKAROUND.  opening buttonedit doesnt create the 3 buttons, just imgs.  only after clicking on a glyph image are the buttons created.  Too late
            //So AtmtdTstng needs to send shortcuts to clicke the buttons
            //https://supportcenter.devexpress.com/ticket/details/t525424/how-do-i-activate-click-a-button-in-a-grid-cell-using-the-keyboard

            //#AtmtdTstng
            edtLstCkdDateBtn.Buttons[0].Shortcut = new DevExpress.Utils.KeyShortcut(Keys.Shift | Keys.E);

            EditorButton x = new EditorButton();
            edtLstCkdDateBtn.Buttons.Add(x);
            edtLstCkdDateBtn.Buttons[1].Kind = ButtonPredefines.Plus;
            edtLstCkdDateBtn.Buttons[1].Caption = "Add New Cooked Date";
            //#AtmtdTstng
            edtLstCkdDateBtn.Buttons[1].Shortcut = new DevExpress.Utils.KeyShortcut(Keys.Shift | Keys.A);

            x = new EditorButton();
            edtLstCkdDateBtn.Buttons.Add(x);
            edtLstCkdDateBtn.Buttons[2].Kind = ButtonPredefines.Delete;
            edtLstCkdDateBtn.Buttons[2].Caption = "Delete Last Cooked Date";
            //#AtmtdTstng
            edtLstCkdDateBtn.Buttons[2].Shortcut = new DevExpress.Utils.KeyShortcut(Keys.Shift | Keys.D);

            //#RQT GrdEdtLstCkdDt edit, add, delete last cooked date in the grid 
            edtLstCkdDateBtn.ButtonClick += repositoryItemButtonEdit1_ButtonClick;

            //edit flagged
            RepositoryItemCheckEdit flaggedEdit = new RepositoryItemCheckEdit();
            //#RQT GrdEdtFlg Toggle flag in the grid 
            flaggedEdit.Click += FlaggedEdit_Click;
            //edit my score
            RepositoryItemButtonEdit myScoreEdit = new RepositoryItemButtonEdit();
            myScoreEdit.DisplayFormat.FormatType = FormatType.Numeric;
            myScoreEdit.DisplayFormat.FormatString = "G";
            myScoreEdit.EditFormat.FormatType = FormatType.Numeric;
            myScoreEdit.EditFormat.FormatString = "G";
            myScoreEdit.Buttons[0].Kind = DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph;
            myScoreEdit.Buttons[0].Image = editImage;

            //#AtmtdTstng            
            myScoreEdit.Buttons[0].Shortcut = new DevExpress.Utils.KeyShortcut(Keys.Shift | Keys.E);
            //#RQT GrdEdtMyRtng Edit myRating in the grid 
            myScoreEdit.ButtonClick += myScoreEdit_ButtonClick;

            //Note: i copied this from the code generated by the designer.  Need a new XPInstantFeedbackView each time we search.
            //#TODO #TRICKY if i pass this as a parameter it doesnt refresh..understand why
            this.xpInstantFeedbackView1 = new DevExpress.Xpo.XPInstantFeedbackView(this.components);
            XPInstantFeedbackView xpInstantFeedbackView = this.xpInstantFeedbackView1;
            xpInstantFeedbackView.ObjectType = typeof(RecipeBoxSolutionModel.RecipeCard);
            xpInstantFeedbackView.Properties.AddRange(new DevExpress.Xpo.ServerViewProperty[] {
                new DevExpress.Xpo.ServerViewProperty("Oid", DevExpress.Xpo.SortDirection.None, "[Oid]"),
                new DevExpress.Xpo.ServerViewProperty("HyperLinkTitle", DevExpress.Xpo.SortDirection.None, "[HyperLinkTitle]"),
                new DevExpress.Xpo.ServerViewProperty("TotalTime (min)", DevExpress.Xpo.SortDirection.None, "[TotalTime]"),
                new DevExpress.Xpo.ServerViewProperty("Rating (Count)", DevExpress.Xpo.SortDirection.None, "[RatingRatingCount]"),
                new DevExpress.Xpo.ServerViewProperty("MyRating", DevExpress.Xpo.SortDirection.None, "[MyRating]"),
                new DevExpress.Xpo.ServerViewProperty("Flagged", DevExpress.Xpo.SortDirection.None, "[WouldLikeToTryFlag]"),
                new DevExpress.Xpo.ServerViewProperty("Last", DevExpress.Xpo.SortDirection.Descending, "[CookedInstances][].Max([CookedDate])"),
                new DevExpress.Xpo.ServerViewProperty("SourceURL", DevExpress.Xpo.SortDirection.None, "[SourceURL]"),
                new DevExpress.Xpo.ServerViewProperty("Source", DevExpress.Xpo.SortDirection.None, "[SourceIndividualFullName]"),
                new DevExpress.Xpo.ServerViewProperty("Title", DevExpress.Xpo.SortDirection.None, "[Title]"),  //need this to use for drag & drop and sort
                //#WORKAROUND devExpress ticket T1176435 sorted in order fetched.  Oid needs to be the first col by our design, and yet we want to order by it last to force deterministic 
                //order of results
                //#TRICKY this is required to make server mode order deterministic 
                new DevExpress.Xpo.ServerViewProperty("OidSorted", DevExpress.Xpo.SortDirection.Descending, "[Oid]")
            });

            //--------------------------------------------
            //Perform the search 
            //--------------------------------------------
            string searchArg = (string.IsNullOrWhiteSpace(NewFilterTerm)) ? this.filterTermLookUpEdit.Text : this.NewFilterTerm;
            searchArg = PrimitiveUtils.Clean(searchArg);

            //#RQT BtnFltr Filter my RecipeBox by OR-ing these fields with each FilterTerm - Title, Notes, Description, Instructions, SourceIndividualFirstName, SourceIndividualLastName, Ingredient.ItemDescription, Ingredient.Item.Name, Keyword.Name
            //#RQT BtnFltr Each FilterTerm is AND-ed with the previous FilterTerm(s).
            //#RQT BtnFltr eg, Easy AND Mexican. Find all RecipeCards that have 'Easy' in one of the fields AND 'Mexican' in one of the fields.
            string searchOneTermSQL =
@"(Title like '%{0}%' 
or Notes like '%{0}%' 
or Description like '%{0}%' 
or Instructions like '%{0}%' 
or SourceIndividualFirstName like '%{0}%' 
or SourceIndividualLastName like '%{0}%' 
or Ingredients[ItemDescription like '%{0}%'] 
or Ingredients[Item.Name like '%{0}%']
or Keywords[Name like '%{0}%'])";

            int recipeBoxOid = DataStoreServiceReference.DataStoreServiceReference.MyRecipeBoxOid;

            if (searchArg != null)
                FilterTerm.AppendSearchFilter(searchArg);

            CriteriaOperator criteria = null;
            if (FilterTerm.SearchFilter.Count == 0)
            {
                string criteriaText = $"RecipeBox.Oid = {recipeBoxOid}";
                criteria = CriteriaOperator.Parse(criteriaText);

            }
            else
            {
                StringBuilder sql = new StringBuilder();
                bool isFirst = true;
                foreach (string searchTerm in FilterTerm.SearchFilter)
                {
                    string searchArgSQLStringLiteral = PrimitiveUtils.SQLStringLiteral(searchTerm);
                    if (isFirst)
                        isFirst = false;
                    else
                        sql.Append(" and ");
                    sql.Append(string.Format(searchOneTermSQL, searchArgSQLStringLiteral));
                }
                sql.Append(" and ");
                sql.Append($"RecipeBox.Oid = {recipeBoxOid}");

                criteria = CriteriaOperator.Parse(sql.ToString());
            }
            XpoService.InitiateQuery(xpInstantFeedbackView, criteria);

            //want sql and XPDataView and server mode.  need to use XPServerModeView.  chose to use xpInstantFeedbackView1
            //https://supportcenter.devexpress.com/ticket/details/t535156/use-the-server-mode-when-session-executequery-session-executequery-or-session
            ((System.ComponentModel.ISupportInitialize)(gridView)).BeginInit();
            gridView.GridControl.DataSource = xpInstantFeedbackView;

            gridView.PopulateColumns();

            if (gridView.Columns.Count == 0)
                return;

            //cols
            gridView.Columns["Oid"].Caption = "Oid";
            gridView.Columns["TotalTime (min)"].Caption = "TotalTime (min)";
            gridView.Columns["Rating (Count)"].Caption = "Rating (Count)";
            gridView.Columns["MyRating"].Caption = "My Rating";
            gridView.Columns["Flagged"].Caption = "Flagged";
            gridView.Columns["Last"].Caption = "Last";
            gridView.Columns["SourceURL"].Caption = "Source URL";
            gridView.Columns["Source"].Caption = "Source";
            gridView.Columns["Title"].Caption = "Title";


            gridView.Columns["Oid"].Visible = false;
            gridView.Columns["OidSorted"].Visible = false;
            gridView.Columns["Title"].Visible = false;
            //#RQT GrdTtlHyprLnk Click on title to open the source URL in a browser.
            DevExpressUxUtils.ConfigureHypertextColumn(searchResultsGridView.Columns["HyperLinkTitle"], "Title", "Title");
            gridView.Columns["SourceURL"].Visible = false;
            gridView.Columns["Last"].ColumnEdit = edtLstCkdDateBtn;
            gridView.Columns["Flagged"].ColumnEdit = flaggedEdit;
            gridView.Columns["MyRating"].ColumnEdit = myScoreEdit;
            ((System.ComponentModel.ISupportInitialize)(gridView)).EndInit();

            searchParmLookUpEdit.EditValue = null;
            searchParmLookUpEdit.Text = null;
            LastLookupEditKey = Keys.None;

            //set focus to search results 
            gridView.Focus();

            //#TRICKY unable to clear the filter input here directly.  Instead, let set the focus to the search results.
            //Click a button to clear the input in a separate event
            //need this to clear input after selecting a choice and hitting Enter
            Thread.Sleep(5);
            clrInptBrBttnItm.PerformClick();
        }

        private void ClearSearchFilters()
        {
            FilterTerm.ClearSearchFilter();
            ConfigureSearchInput();
            DoFilter(this.filterTermLookUpEdit, this.searchResultsGridView, imageCollection1.Images[0], imageCollection1.Images[1], imageCollection1.Images[2]);
            this.filterTermLookUpEdit.Select();
        }

        //Display the number of rows in the grid in the status bar
        private void DisplaySearchResultsStatus()
        {
            StringBuilder status = new StringBuilder();
            status.Append($"Rows: {searchResultsGridView.RowCount.ToString()}" + '\t' + $"Filter: ");
            if (FilterTerm.SearchFilter.Count > 0)
            {
                bool isFirst = true;
                foreach (string searchTerm in FilterTerm.SearchFilter)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        status.Append(" AND ");
                    status.Append(searchTerm);
                }
            }
            else
            {
                status.Append("<none>");
            }

            this.barStaticItem.Caption = status.ToString();
        }

        IDataObject GetDragData(GridView view)
        {
            int[] selection = view.GetSelectedRows();
            if (selection == null)
                return null;

            if (selection.Length != 1)
                return null;

            List<AppointmentExchangeData> exchangeList = new List<AppointmentExchangeData>();
            int count = selection.Length;
            for (int i = 0; i < count; i++)
            {
                int rowIndex = selection[i];
                exchangeList.Add(new AppointmentExchangeData()
                {
                    Title = (string)view.GetRowCellValue(rowIndex, "Title"),
                    Start = DateTime.MinValue,
                    Duration = new TimeSpan(0, 0, 0),
                    RecipeOid = (int)view.GetRowCellValue(rowIndex, "Oid")
                }); ;
            }

            return new DataObject(DataFormats.Serializable, exchangeList);
        }

        private void AddSelectedToGroceryList()
        {
            List<RecipeCard> recs = new List<RecipeCard>();
            using (UnitOfWork uow = new UnitOfWork())
            {
                if (this.searchResultsGridView.IsFocusedView)
                {
                    RecipeCard rc = LoadFocusedRecipeCard(uow);
                    if (rc == null)
                        return;
                    recs.Add(rc);
                }
                else if (this.schedulerControl.Focused)
                {
                    List<CookedDishKey> cookedDishKeys = new List<CookedDishKey>();
                    foreach (Appointment appt in schedulerControl.SelectedAppointments)
                    {
                        CookedDishKey key = (CookedDishKey)appt.LabelKey;
                        cookedDishKeys.Add(key);
                    }
                    //#AtmtTstng #TRICKY Force list of cookedDishKey order to be deterministic to support automated testing.
                    cookedDishKeys = cookedDishKeys.OrderBy(cdk => cdk.RecipeCardOid).ToList();
                    foreach (CookedDishKey cdk in cookedDishKeys)
                    {
                        RecipeCard rc = (RecipeCard)XpoService.LoadHeadBusObjByKey(typeof(RecipeCard), cdk.RecipeCardOid, uow);
                        if (rc == null)
                            return;
                        recs.Add(rc);
                    }
                }
                if (recs.Count == 0)
                    return;
                int? glOid = DataStoreServiceReference.DataStoreServiceReference.MyGroceryListOid;
                SelectIngredientsForGroceryList form = new SelectIngredientsForGroceryList(glOid, recs.ToArray(), this.MDIParentForm);
                this.MDIParentForm.FormStack.PushStackAndShowDialogForm(form, () =>
                {
                    if (form.DialogResult == Forms.DialogResult.OK)
                    {
                        form.Dispose();
                        form = null;
                        EditGroceryList eglForm = new EditGroceryList(glOid, this.MDIParentForm);
                        this.MDIParentForm.FormStack.PushStackAndShowDialogForm(eglForm);
                    }
                });
            }
        }
        private void DeleteFocusedRecipeCard()
        {
            //#TODO REFACTOR put this in base class passing Oid
            using (UnitOfWork uow = new UnitOfWork())
            {
                RecipeCard recipeToDelete = LoadFocusedRecipeCard(uow);
                if (recipeToDelete == null)
                {
                    //It is already deleted, so do nothing.
                    RefreshForm();
                    return;
                }

                //#RQT BtnDelRecCrdCnfm Prompt user for confirmation before deleting RecipeCard.
                string question = string.Format("Confirm delete recipe {0}?", recipeToDelete.BusinessObjectDisplayName);
                //#AtmtdTstng session.Confirmation Yes No
                Forms.DialogResult answer = ShowMessageBox(this.MdiParent, question, "Confirmation", MessageBoxButtons.YesNo);
                if (answer == Forms.DialogResult.Yes)
                {
                    try
                    {
                        using (WaitCursor wc = new WaitCursor())
                        {
                            recipeToDelete.Delete();
                            XpoService.CommitBodyOfBusObjs(recipeToDelete, uow);
                        }
                    }
                    catch (LockingException)
                    {
                        //determine if this is delete or update issue
                        bool isUpdate = XpoService.ObjectExistsInDataStore(recipeToDelete);
                        if (recipeToDelete != null)
                        {
                            //Concurrency errror, it was updated before we deleted 
                            ShowMessageBox("The record was modified. Attempt to delete again.", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        }
                        else
                        {
                            //Someone else already deleted it. so do nothing
                            ;
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessageBox($"Fatal error while deleting: {ex.Message}", "Fatal Error");
                    }
                }
                RefreshForm();
            }
        }

        public RecipeCard LoadFocusedRecipeCard(UnitOfWork uow)
        {
            int focusedRowHandle = searchResultsGridView.FocusedRowHandle;
            if (focusedRowHandle < 0)
                return null;

            //Get the recipe PK for the focused row
            int oid = (int)searchResultsGridView.GetRowCellValue(focusedRowHandle, searchResultsGridView.Columns["Oid"]);
            //load recipe from database
            RecipeCard rec = (RecipeCard)XpoService.LoadHeadBusObjByKey(typeof(RecipeCard), oid, uow);
            return rec;
        }

        public void BackupDatabase()
        {
            try
            {

                //Backup the DB from the client
                string rootdir = AppSettings.GetAppSetting("RootDirectoryDataStoreBackups");
                string outfile = null;
                if (rootdir == null)
                {
                    ShowMessageBox("RootDirectoryDataStoreBackups is missing in configuration AppSettings", "App.Config Error");
                    return;
                }
                string connStr = ConnectionStrings.GetConnectionString(DataStoreServiceReference.DataStoreServiceReference.AppConfigConnectionSetting);
                if (connStr == null)
                {
                    ShowMessageBox("Recipes is missing in configuration ConnectionStrings section.", "App.Config Error");
                    return;
                }
                //#TODO ENHANCEMENT Add Administrator role to UserProfile.  only admins can see the admin tab
                BackupDataStore backupSchema = new BackupDataStore();
                if (backupSchema.ShowDialog() == Forms.DialogResult.OK)
                {
                    using (WaitCursor wc = new WaitCursor())
                    {
                        DataStoreArchiver dataStoreArchiver = new DataStoreArchiver();
                        if (backupSchema.ReplicateBusObjBodies)
                        {
                            Type headBusObj = null;
                            foreach (Type t in ModelInfo.PersistentTypes)
                                if (t.Name == backupSchema.HeadBusinessObject)
                                {
                                    headBusObj = t;
                                    break;
                                }
                            outfile = dataStoreArchiver.ReplicateBusObjBodies(
                                headBusObj, backupSchema.NumberToReplicate,
                                backupSchema.ForwardFKsToReplicate,
                                backupSchema.DontReplicateTheseBodyParts,
                                ModelInfo.PersistentTypes,
                                BusinessObject.GetAllBusinessKeys(ModelInfo.PersistentTypes),
                                DataStoreServiceReference.DataStoreServiceReference.AppConfigConnectionSetting,
                                rootdir, PrimitiveUtils.Namify(backupSchema.BackupName));
                        }//if
                        else
                            outfile = dataStoreArchiver.BackupDataStore(
                                ModelInfo.PersistentTypes,
                                DataStoreServiceReference.DataStoreServiceReference.AppConfigConnectionSetting,
                                rootdir, PrimitiveUtils.Namify(backupSchema.BackupName), backupSchema.ResetOids);
                    }//using
                    FileInfo fi = new FileInfo(outfile);
                    ShowMessageBox($"Backup complete: {fi.FullName}", "Success");
                }//if
            }
            catch (Exception ex)
            {
                ShowMessageBox($"Backup failed: {ex.Message}", "Backup Database Error");
            }
        }
        public void RestoreDatabase()
        {
            try
            {
                string databaseName = null;
                string rootdir = AppSettings.GetAppSetting("RootDirectoryDataStoreBackups");
                if (rootdir == null)
                {
                    ShowMessageBox("RootDirectoryDataStoreBackups is missing in configuration AppSettings", "App.Config Error");
                    return;
                }
                RestoreDataStore dialog = new RestoreDataStore();
                if (dialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    using (new WaitCursor())
                    {
                        DataStoreArchiver dataStoreArchiver = new DataStoreArchiver();
                        databaseName = dataStoreArchiver.RestoreDataStore(
                            dialog.RestoreSchemaPrompts.BackupFolder,
                            dialog.RestoreSchemaPrompts.ConnectionString,
                            ModelInfo.CreateOneOfEach, ModelInfo.PersistentTypes);
                    }
                }
                ShowMessageBox($"Restore complete to: {databaseName}", "Success");
            }
            catch (Exception ex)
            {
                ShowMessageBox($"Restore failed: {ex.Message}", "Restore Database Error");
            }
        }

        #endregion

        #region window navigation helpers
        //window navigation helpers

        private void ShowViewRecipeForm(int? recipeIDToOpen)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                var form = new ViewRecipeCard(recipeIDToOpen, this.MDIParentForm);
                this.MDIParentForm.FormStack.PushStackAndShowDialogForm(form, () =>
                {
                    ShowNextForm(form);
                });
            }
        }

        private void ShowEditRecipeCardForm(int? recipeIDToOpen)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                var form = new EditRecipeCard(recipeIDToOpen, this.MDIParentForm);
                this.MDIParentForm.FormStack.PushStackAndShowDialogForm(form, () =>
                {
                    ShowNextForm(form);
                });
            }
        }

        private void ShowEditGroceryListForm()
        {
            using (WaitCursor wc = new WaitCursor())
            {
                int? glOid = DataStoreServiceReference.DataStoreServiceReference.MyGroceryListOid;
                var form = new EditGroceryList(glOid, this.MDIParentForm);
                this.MDIParentForm.FormStack.PushStackAndShowDialogForm(form);
            }
        }

        private void ShowNextForm(MDIChildForm previousForm)
        {
            int? ViewedOid = null;
            if (previousForm is CRUDBusinessObjectForm)
                ViewedOid = ((CRUDBusinessObjectForm)previousForm).Oid;
            string nextFormToOpen = previousForm.NextFormToOpen;
            bool refreshSearchResults = previousForm is EditRecipeCard || previousForm is ViewRecipeCard;
            //all done with previousForm, release resources
            previousForm.Dispose();
            previousForm = null;
            if (nextFormToOpen is null)
            {
                if (refreshSearchResults)
                {
                    this.xpInstantFeedbackView1.Refresh();
                    RefreshAppointments();
                }
                if (ViewedOid != null)
                {
                    //#RQT GrdClsRCFrmFcs Set focus to the previously Viewed/Edited RecipeCard in the grid after closing the form.
                    SimpleCRUDFramework.DevExpressUxUtils.FocusTo(this.searchResultsGridView, ViewedOid);
                }
            }
            else if (nextFormToOpen == nameof(EditRecipeCard))
                ShowEditRecipeCardForm(ViewedOid);
            else if (nextFormToOpen == nameof(ViewRecipeCard))
                ShowViewRecipeForm(ViewedOid);
            else
                throw new Exception(string.Format("logic error unexpected nextFormToOpen {0}", previousForm.NextFormToOpen));
        }

        #endregion

    }

    #region helper classes
    //Attach this to each appointment in the schedular so we can load its corresponding CookedDish
    //These are the keys used to fetch the cooked recipe
    public class CookedDishKey
    {
        public CookedDishKey() { }
        public CookedDishKey(int? oid, DateTime? cookedDishDate, int recipeCardOid)
        {
            this.CookedDishOid = oid;
            this.RecipeCardOid = recipeCardOid;
        }
        public CookedDishKey Clone()
        {
            CookedDishKey clone = new CookedDishKey();
            clone.CookedDishOid = this.CookedDishOid;
            clone.CookedDishDate = this.CookedDishDate;
            clone.RecipeCardOid = this.RecipeCardOid;
            return clone;
        }
        public int? CookedDishOid { get; set; }
        public DateTime? CookedDishDate { get; set; }
        public int RecipeCardOid { get; set; }
    }

    [Serializable]
    [ComVisible(true)]
    [SecurityCritical]
    //This supports drag and drop. 
    internal class AppointmentExchangeData
    {
        internal string Title { get; set; }
        internal DateTime Start { get; set; }
        internal TimeSpan Duration { get; set; }
        internal int RecipeOid { get; set; }
        [SecurityCritical]
        internal void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Title), Title, typeof(string));
            info.AddValue(nameof(Start), Start, typeof(DateTime));
            info.AddValue(nameof(Duration), Duration, typeof(TimeSpan));
            info.AddValue(nameof(RecipeOid), Duration, typeof(int));
        }
    }

    public class FilterTerm : XPObject
    {
        public FilterTerm() { }
        public FilterTerm(int Oid, string name)
        {
            this.Oid = Oid;
            this.Name = name;
        }
        private string fName;
        public string Name
        {
            get { return fName; }
            set
            {
                SetPropertyValue(nameof(Name), ref fName, PrimitiveUtils.Clean(value));
            }
        }

        #region statics
        static private List<string> fSearchFilter = new List<string>();
        static internal List<string> SearchFilter
        {
            get { return fSearchFilter; }
            set { fSearchFilter = value; }
        }

        static internal void AppendSearchFilter(string searchArg)
        {
            fSearchFilter.Add(searchArg);
        }
        static internal void ClearSearchFilter()
        {
            fSearchFilter.Clear();
        }

        #endregion
    }
    public class SearchFormClipboard
    {
        public enum ClipboardActors { GRID, CALENDAR }
        public ClipboardActors? ClipboardActor = null;

        public enum CopyActions { CUT, COPY }
        public CopyActions? CopyAction = null;

        public List<CookedDishKey> Clipboard = new List<CookedDishKey>();
    }

    //borrowed from here 
    ////https://supportcenter.devexpress.com/ticket/details/t374228/schedulerstorage-appointmentsdeleted-event-fired-separately-for-single-deletion-of
    public class CustomSchedulerCommandFactoryServiceWrapper : SchedulerCommandFactoryServiceWrapper
    {
        SchedulerControl control;
        public CustomSchedulerCommandFactoryServiceWrapper(ISchedulerCommandFactoryService service, SchedulerControl control)
            : base(service)
        {
            this.control = control;
        }

        public override SchedulerCommand CreateCommand(SchedulerCommandId id)
        {
            if (id == SchedulerCommandId.DeleteAppointmentsQueryOrDependencies)
                return new CustomDeleteAppointmentsQueryOrDependenciesCommand(this.control);
            return base.CreateCommand(id);
        }
    }

    public class CustomDeleteAppointmentsQueryCommand : DeleteAppointmentsQueryCommand
    {
        public CustomDeleteAppointmentsQueryCommand(ISchedulerCommandTarget target)
            : base(target)
        {
        }

        public CustomDeleteAppointmentsQueryCommand(ISchedulerCommandTarget target, AppointmentBaseCollection appointments)
            : base(target, appointments)
        {
        }

        protected override void ExecuteCore()
        {
            Control.Storage.BeginUpdate();
            base.ExecuteCore();
            Control.Storage.EndUpdate();
        }
    }

    public class CustomDeleteAppointmentsQueryOrDependenciesCommand : DeleteAppointmentsQueryOrDependenciesCommand
    {
        public CustomDeleteAppointmentsQueryOrDependenciesCommand(ISchedulerCommandTarget target)
            : base(target)
        {
        }

        public override void Execute()
        {
            Control.Storage.BeginUpdate();
            base.Execute();
            Control.Storage.EndUpdate();
        }
    }
    #endregion helper classes
}