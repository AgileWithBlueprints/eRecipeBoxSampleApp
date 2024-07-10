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
using DevExpress.Xpo;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraScheduler;
using eRecipeBox.DataStoreServiceReference;
using Foundation;
using RecipeBoxSolutionModel;
using SimpleCRUDFramework;
using SimpleCRUDFramework.HelperForms;
using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace eRecipeBox
{
    public partial class SearchRecipeBox : MDIChildForm
    {
        #region statics
        private int logBreadcrumbID = 1;
        #endregion statics

        #region ctor

        public SearchRecipeBox(MDIParentForm parent) : base(nameof(SearchRecipeBox), parent)
        {
            base.InitializeBaseComponent();
            InitializeComponent();
            InitializeComponent2();
        }

        #endregion

        #region form methods and event handlers

        //https://supportcenter.devexpress.com/ticket/details/t275978/how-to-hide-bar-items-pages-and-groups
        private void SearchRecipeBox_Load(object sender, EventArgs e)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                WeekViewPosition = splitContainerControl1.SplitterPosition;
                if (DataStoreServiceReference.DataStoreServiceReference.IsSingleUserDeployment)
                    AutomatedTestUtils.VerifySystemTestData(); //move the cooked dates to this week so user sees them
                ClearSearchFilters();
                RefreshAppointments();
                SetAllControlNames();
                EndFormLoad();
            }
        }

        //https://stackoverflow.com/questions/2499095/how-to-assign-a-shortcut-key-something-like-ctrlf-to-a-text-box-in-windows-fo
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //SHCUTai Shortcut Key: Alt-i moves the cursor to the search filter term. 
            if (keyData == (Keys.Alt | Keys.I))
            {
                filterTermLookUpEdit.Select();
                return true;
            }
            //#AtmtTstng #TRICKY verify and calibrate the system test database.  important because the calendar starts with 'today'
            if (keyData == (Keys.Control | Keys.Alt | Keys.Shift | Keys.T))
            {
                AutomatedTestUtils.VerifySystemTestData();
                return true;
            }
            //#AtmtTstng #TRICKY eRB needs to log cooked dates relative to an Anchor.  Pickup the anchor date with this key combo.
            if (keyData == (Keys.Control | Keys.Alt | Keys.Shift | Keys.A))
            {
                DateTime anchorDate = DateTime.Parse(filterTermLookUpEdit.Text);
                CookedDish.SetAnchorDate(anchorDate);
                return true;
            }
            //#AtmtTstng #TRICKY Help auto testing..create a log entry in DataStore Log to help indicate where we are in the test script
            if (keyData == (Keys.Control | Keys.Alt | Keys.Shift | Keys.L))
            {
                Log.DataStore.Info($"Breadcrumb: {logBreadcrumbID}");
                logBreadcrumbID++;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region InstantFeedbackView1 event handlers
        private void xpInstantFeedbackView1_ResolveSession_1(object sender, DevExpress.Xpo.ResolveSessionEventArgs e)
        {
            //Each new filter uses a new session
            e.Session = new Session();
        }

        private void xpInstantFeedbackView1_DismissSession_1(object sender, ResolveSessionEventArgs e)
        {
            e.Session.Session.Dispose();
        }

        #endregion

        #region filter results Grid event handlers
        private void searchResultsGridControl_ProcessGridKey(object sender, KeyEventArgs e)
        {
            try
            {
                int rowHandle = searchResultsGridView.FocusedRowHandle;
                if (rowHandle < 0)
                    return;

                int recipeIDToOpen = (int)searchResultsGridView.GetRowCellValue(rowHandle, this.searchResultsGridView.Columns["Oid"]);
                if (e.KeyData == (Keys.Control | Keys.C))
                {
                    searchFormClipboard.Clipboard.Clear();
                    //#FRAGILE ASSUMPTION Single select for grid
                    searchFormClipboard.Clipboard.Add(new CookedDishKey(null, null, recipeIDToOpen));
                    searchFormClipboard.ClipboardActor = SearchFormClipboard.ClipboardActors.GRID;
                    searchFormClipboard.CopyAction = SearchFormClipboard.CopyActions.COPY;
                    //#IMPORTANT We also want the row to be copied to the Windows clipboard
                    e.Handled = false;
                }
                //#RQT GrdCtlEntr Ctl+Enter key in grid launches EditRecipeCard.  
                else if (e.KeyData == (Keys.Control | Keys.Enter))
                {
                    e.Handled = true;
                    ShowEditRecipeCardForm(recipeIDToOpen);
                }
                //#RQT GrdEntr Enter key in grid launches ViewRecipeCard.  
                else if (e.KeyData == Keys.Enter)
                {
                    e.Handled = true;
                    ShowViewRecipeForm(recipeIDToOpen);
                }
                else
                    e.Handled = false;
            }
            catch { }  //#WORKAROUND occasionally throws when holding arrow key down and scrolling
        }
        #endregion

        #region filter results GridView event handlers     
        private void searchResultsGridView_DataSourceChanged(object sender, EventArgs e)
        {
            DisplaySearchResultsStatus();
        }
        //Filter results grid row count is displayed on the status bar below the grid
        //https://supportcenter.devexpress.com/ticket/details/q521694/getting-count-of-full-resultset-in-server-mode
        //wire event handlers that fire when the row count changes
        private void searchResultsGridView_RowCountChanged(object sender, EventArgs e)
        {
            DisplaySearchResultsStatus();
        }
        private void searchResultsGridView_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            //https://supportcenter.devexpress.com/ticket/details/t447467/repositoryitemhyperlinkedit-how-to-make-a-link-non-underlined
            if (e.Column.FieldName == nameof(RecipeCard.HyperLinkTitle))
            {
                //Support hyperlinkedit on Title
                e.RepositoryItem = titleHyperLinkEdit;
            }
        }

        private void searchResultsGridView_RowClick(object sender, RowClickEventArgs e)
        {
            //#RQT GrdDblClk Double click opens the EditRecipeCard form on clicked RecipeCard in the grid.
            if (e.Clicks == 2)
            {
                e.Handled = true;
                int recipeIDToOpen = (int)this.searchResultsGridView.GetRowCellValue(e.RowHandle, this.searchResultsGridView.Columns["Oid"]);
                ShowEditRecipeCardForm(recipeIDToOpen);
            }
        }
        //Scroll to top after sort
        //#TODO get this to work
        private void searchResultsGridView_EndSorting(object sender, EventArgs e)
        {
            //https://supportcenter.devexpress.com/ticket/details/q345436/scroll-to-top-after-sort
            //Unfortunately, this doesn't work, so we added an explicit button for the user
            scrollResultsToTopBarButtonItem.PerformClick();
        }
        //Dragging a RecipeCard from the grid and dropping it on a date schedules the RecipeCard for that date.
        //https://docs.devexpress.com/WindowsForms/114668/controls-and-libraries/data-grid/getting-started/walkthroughs/hit-information/tutorial-hit-information
        private void searchResultsGridView_MouseDown(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            this.DownHitInfo = null;

            GridHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));
            if (Control.ModifierKeys != Keys.None)
                return;
            if (e.Button == MouseButtons.Left && hitInfo.InRow && hitInfo.HitTest != GridHitTest.RowIndicator)
                this.DownHitInfo = hitInfo;

        }

        private void searchResultsGridView_MouseMove(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Button == MouseButtons.Left && this.DownHitInfo != null)
            {
                Size dragSize = SystemInformation.DragSize;
                Rectangle dragRect = new Rectangle(new Point(this.DownHitInfo.HitPoint.X - dragSize.Width / 2,
                    this.DownHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

                if (!dragRect.Contains(new Point(e.X, e.Y)))
                {
                    view.GridControl.DoDragDrop(GetDragData(view), DragDropEffects.All);
                    this.DownHitInfo = null;
                }
            }
        }
        private void searchResultsGridView_GotFocus(object sender, EventArgs e)
        {
            DoUpdateViewState();
        }

        private void searchResultsGridView_LostFocus(object sender, EventArgs e)
        {
            DoUpdateViewState();
        }

        private void searchResultsGridView_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DoUpdateViewState();
        }
        #endregion

        #region filter parameter lookupedit event handlers
        /*
#TODOREFACTOR encapsulate this same behavior in a LookUpEdit subclass.  SearchFilter, GLItems, ingredient.Items

LastLookupEditKey - init, set in keydown, addSimpleButton_Click if tab, send tab
NewItemToAdd - set in process new

lookuped editclosed - click add  

in addSimpleButton_Click
string itemToAdd = (string.IsNullOrWhiteSpace(NewItemToAdd)) ? this.itemLookUpEdit.Text : this.NewItemToAdd;
            if (string.IsNullOrWhiteSpace(itemToAdd))
                return;
            AddItemToGL(itemToAdd);
then clear
            this.SearchFilter = new SearchFilter();
            searchArgumentXPBindingSource.DataSource = this.SearchFilter;         
         */

        //tricky to get this to work the way we need.
        //#RQT FltrTrmInpt LookupEdit behavior - 
        //#RQT FltrTrmInpt a) Enter key applies the selected drop down text (or new text) as a new filter term and runs the filter.
        //#RQT FltrTrmInpt b) Start to type (shows autocompletion), then tab.  Accepts the autocomplete.        
        private void filterTermLookUpEdit_ProcessNewValue(object sender, ProcessNewValueEventArgs e)
        {
            string cleanedVal = PrimitiveUtils.Clean((string)e.DisplayValue);
            if (cleanedVal == null)
                return;

            NewFilterTerm = cleanedVal;
            e.DisplayValue = null;

            e.Handled = true; //need this to be true
        }

        //When Filter term LookupEdit closes, automatically initiate Filter so the user doesn't have to click the Filter button.
        //note that this also is fired when we add a new search to the list and set it  
        private void filterTermLookUpEdit_Closed(object sender, ClosedEventArgs e)
        {
            string searchFilterTerm = (string.IsNullOrWhiteSpace(NewFilterTerm)) ? this.filterTermLookUpEdit.Text : this.NewFilterTerm;
            if (e.CloseMode == PopupCloseMode.Normal && !string.IsNullOrWhiteSpace(searchFilterTerm) && LastLookupEditKey == Keys.Enter)
            {
                //this adds and item when hitting enter or tabbing off a suggestion or selecting a suggestion
                this.filterSimpleButton.PerformClick();
            }

        }
        private void filterTermLookUpEdit_KeyDown(object sender, KeyEventArgs e)
        {
            LastLookupEditKey = e.KeyCode;
        }
        #endregion

        #region scheduler event handlers

        private void schedulerControl_AppointmentDrag(object sender, AppointmentDragEventArgs e)
        {
            Appointment apt = e.EditedAppointment;
            if (apt.Start == DateTime.MinValue)
                apt.Start = e.HitInterval.Start;
        }

        private void schedulerControl_AppointmentDrop(object sender, AppointmentDragEventArgs e)
        {
            e.Allow = true;
        }

        //this supports insert and changed.  refresh the views to reflect changes in the model
        private void schedulerControl_AppointmentDropComplete(object sender, AppointmentDropCompleteEventArgs e)
        {
            if (RefreshIsRequired)
                RefreshAppointments();
            this.xpInstantFeedbackView1.Refresh();

            //#RQT ClndrFcusGrd Set focus to the scheduled RecipeCard in the grid after drag and drop to the Calendar.
            foreach (Appointment o in e.Appointments)
            {
                CookedDishKey key = (CookedDishKey)o.LabelKey;
                SimpleCRUDFramework.DevExpressUxUtils.FocusTo(searchResultsGridView, key.RecipeCardOid);
                break;
            }
        }

        private void schedulerControl_PrepareDragData(object sender, PrepareDragDataEventArgs e)
        {
            object data = e.DataObject.GetData(DataFormats.Serializable);
            AppointmentBaseCollection appointments = new AppointmentBaseCollection();
            foreach (AppointmentExchangeData item in (IList)data)
            {
                Appointment apt = this.schedulerDataStorage1.CreateAppointment(AppointmentType.Normal);

                apt.Subject = item.Title;
                apt.Start = item.Start;
                apt.Duration = item.Duration;
                CookedDishKey key = new CookedDishKey();
                key.RecipeCardOid = item.RecipeOid;
                apt.LabelKey = key;
                apt.AllDay = true;
                appointments.Add(apt);
            }
            SchedulerDragData schedulerDragData = new SchedulerDragData(appointments);
            e.DragData = schedulerDragData;

        }
        //prevent selecting a date on the navigator from switching to DateView
        private void schedulerControl_DateNavigatorQueryActiveViewType(object sender, DateNavigatorQueryActiveViewTypeEventArgs e)
        {
            //https://supportcenter.devexpress.com/ticket/details/q461871/selecting-a-day-in-the-datenavigator-switches-month-view-to-day-view
            //Default behavior: Clicking on the date navigator increases the interval.
            //Disable this and for 1 week or 4 week views only.
            if (e.OldViewType == SchedulerViewType.Month && this.monthWeekBarButtonItem.Caption == monthViewCaption)
            {
                e.NewViewType = SchedulerViewType.Month;
                this.schedulerControl.MonthView.WeekCount = 1;
                DateTime selectedDate = e.SelectedDays[0].Start;
                DateTime sunday = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
                e.SelectedDays.Add(new TimeInterval(sunday, TimeSpan.FromDays(7)));
            }
            else
            {
                e.NewViewType = SchedulerViewType.Month;
                this.schedulerControl.MonthView.WeekCount = 4;
                DateTime selectedDate = e.SelectedDays[0].Start;
                DateTime sunday = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
                e.SelectedDays.Add(new TimeInterval(sunday, TimeSpan.FromDays(28)));
            }

        }

        private void schedulerControl_DoubleClick(object sender, EventArgs e)
        {
            if (schedulerControl.SelectedAppointments.Count != 1)
                return;
            foreach (Appointment appt in schedulerControl.SelectedAppointments)
            {
                //#RQT ClndrDblClk Double click opens EditRecipeCard iif one RecipeCard is selected in the Calendar.
                CookedDishKey key = (CookedDishKey)appt.LabelKey;
                ShowEditRecipeCardForm(key.RecipeCardOid);
            }

        }

        private void schedulerControl_KeyDown(object sender, KeyEventArgs e)
        {
            //https://supportcenter.devexpress.com/ticket/details/q495520/implement-cut-copy-paste-appointments
            //#RQT ClndrClpbrd Ctl-c copies (Ctl-x cuts) to Windows clipboard and to app clipboard.
            //#RQT ClndrClpbrd Ctl-v, if copied, creates new CookedDishes at selected day
            //#RQT ClndrClpbrd Ctl-v, if cut, moves CookedDishes to the selected day
            if (e.Control & (e.KeyCode == Keys.C || e.KeyCode == Keys.X))
            {
                SearchFormClipboard.CopyActions copyAction;
                if (e.KeyCode == Keys.C)
                    copyAction = SearchFormClipboard.CopyActions.COPY;
                else
                    copyAction = SearchFormClipboard.CopyActions.CUT;
                SchedulerCopyCut(copyAction);
                return;
            }
            //Paste from localClipBoard to target day in calendar - creating a new CookedDish date
            if (e.Control & e.KeyCode == Keys.V)
            {
                SchedulerPaste();
                return;
            }

            if (schedulerControl.SelectedAppointments.Count != 1)
                return;
            //#RQT ClndrCtlEntr Ctl-Enter opens EditRecipeCard iif one RecipeCard is selected in the Calendar.
            if (e.KeyCode == Keys.Enter && e.Control)
            {
                foreach (Appointment appt in schedulerControl.SelectedAppointments)
                {
                    CookedDishKey key = (CookedDishKey)appt.LabelKey;
                    ShowEditRecipeCardForm(key.RecipeCardOid);
                }
            }
            //#RQT ClndrEntr Enter opens ViewRecipeCard iif one RecipeCard is selected in the Calendar.
            else if (e.KeyCode == Keys.Enter)
            {
                foreach (Appointment appt in schedulerControl.SelectedAppointments)
                {
                    //#RQT ClndrEntr Enter opens ViewRecipeCard iif one RecipeCard is selected in the Calendar.
                    CookedDishKey key = (CookedDishKey)appt.LabelKey;
                    ShowViewRecipeForm(key.RecipeCardOid);
                }
            }

        }

        private void schedulerControl_SelectionChanged(object sender, EventArgs e)
        {
            //https://supportcenter.devexpress.com/ticket/details/t960736/popup-menu-mouse-click-detect-shift-key-held-down
            if (ModifierKeys == Keys.Shift)
                SelectAllApptsBetween();

            DoUpdateViewState();
        }

        private void schedulerControl_Enter(object sender, EventArgs e)
        {
            this.SchedulerHasFocus = true;
            DoUpdateViewState();
        }

        private void schedulerControl_Leave(object sender, EventArgs e)
        {
            this.SchedulerHasFocus = false;
            DoUpdateViewState();
        }

        private void schedulerControl_ActiveViewChanging(object sender, ActiveViewChangingEventArgs e)
        {
            if (e.NewView.Type != SchedulerViewType.Month)
                e.Cancel = true;

            //https://supportcenter.devexpress.com/ticket/details/q38707/how-to-prevent-switching-to-day-view
            //Prevent schedular from changing to DayView
            //this.schedulerControl1.Views.DayView.Enabled = false;
            //Note: this doesn't seem to work, so just prevent it from changing using this event handler
            //this.schedulerControl1.Views.DayView.Enabled = false;
        }

        private void schedulerControl_PopupMenuShowing(object sender, DevExpress.XtraScheduler.PopupMenuShowingEventArgs e)
        {
            //https://supportcenter.devexpress.com/ticket/details/q93386/how-to-disable-the-schedulercontrol-context-menu
            e.Menu.Items.Clear();
        }
        #endregion

        #region schedulerDataStorage event handlers
        private void schedulerDataStorage1_AppointmentsChanged(object sender, PersistentObjectsEventArgs e)
        {
            //Dragging and dropping selected RecipeCard(s) within the Calendar effectively reschedules the CookedDish.CookDate(s) to the new (dropped) dates.
            //User can select one or multiple to reschedule.
            this.schedulerControl.BeginUpdate();
            try
            {
                foreach (Appointment appointment in e.Objects)
                {
                    //#TRICKY When deleting an appt, AppointmentsDeleted is fired, then AppointmentsChanged is fired.  Handle this corner case
                    if (appointment.LabelKey == null)
                        continue;
                    CookedDishKey key = (CookedDishKey)appointment.LabelKey;
                    using (UnitOfWork uow = new UnitOfWork())
                    {
                        RecipeCard recipeCard = (RecipeCard)XpoService.LoadHeadBusObjByKey(typeof(RecipeCard), key.RecipeCardOid, uow);
                        if (recipeCard == null)
                            return;
                        CookedDish cookedDish = recipeCard.FindCookedDish((int)key.CookedDishOid);
                        if (cookedDish == null)
                            return;
                        //reschedule is implemented as a delete and insert.  This handles the moving to double booked case
                        cookedDish.EditCookedDishDate(appointment.Start);
                        this.RefreshIsRequired = true;
                        XpoService.CommitBodyOfBusObjs(recipeCard, uow);
                        key.CookedDishOid = cookedDish.Oid;
                        key.CookedDishDate = cookedDish.CookedDate;
                    }
                }
            }
            finally
            {
                this.schedulerControl.EndUpdate();
            }
        }

        private void schedulerDataStorage1_AppointmentsDeleted(object sender, PersistentObjectsEventArgs e)
        {
            //#RQT ClndrDel Deleting (Delete key) selected RecipeCard(s) in the calendar deletes the corresponding CookedDish(es) for the selected RecipeCards.
            //#RQT ClndrDel Note: To prevent accidental RecipeCard deletion, user cannot delete a RecipeCard from the Calendar.

            //https://supportcenter.devexpress.com/ticket/details/t1068527/schedulercontrol-appointmentschanged-fires-for-each-changed-appointment-when-moving

            //#TRICKY. Scheduler default behavior.  Select multiple, hit Delete.  Deletes are not wrapped in a BeginUpdate\EndUpdate, so this 
            //event is fired once for each delete.  This KIS implementation assumes that and deletes one cooked dish each time it is fired.
            //Once event has been fired the number of times == NumberOfSelectedAppointments, it refreshes the search results grid.

            //Alternative solution #1: Process the delete key event myself and wrap in a BeginUpdate\EndUpdate
            //Alternative solution #2: Create a CustomSchedulerCommandFactoryService as suggested here.  It doesn't seem to 
            //fire schedulerDataStorage1_AppointmentsDeleted at all.   
            ////https://supportcenter.devexpress.com/ticket/details/t374228/schedulerstorage-appointmentsdeleted-event-fired-separately-for-single-deletion-of

            //if my counter is cleared, this is the first call to delete.  Start with the number of selected appointments and count down.
            if (NumberOfSelectedAppointments == 0)
                NumberOfSelectedAppointments = schedulerControl.SelectedAppointments.Count;
            NumberOfSelectedAppointments--;
            //add the key, to be deleted in batch
            foreach (Appointment o in e.Objects)
            {
                CookedDishKey key = (CookedDishKey)o.LabelKey;
                cookedDishKeysToDelete.Add(key);
                o.LabelKey = null;  //just to be safe
                break;
            }

            //last time this event is fired, perform the delete
            if (NumberOfSelectedAppointments == 0)
            {
                //since we couldnt get them in a single command begin/end, this is our temporary workaround
                schedulerControl.BeginUpdate();
                try
                {
                    //#AtmtTstng #TRICKY Force list of cookedDishKey order to be deterministic to support automated testing.
                    cookedDishKeysToDelete = cookedDishKeysToDelete.OrderBy(cdk => cdk.RecipeCardOid).ToList();
                    foreach (CookedDishKey key in cookedDishKeysToDelete)
                    {
                        using (UnitOfWork uow = new UnitOfWork())
                        {
                            RecipeCard rec = (RecipeCard)XpoService.LoadHeadBusObjByKey(typeof(RecipeCard), key.RecipeCardOid, uow);
                            if (rec == null)
                                return;
                            CookedDish cr = rec.FindCookedDish((int)key.CookedDishOid);
                            if (cr == null)
                                return;
                            rec.DeleteCookedDish(cr);
                            XpoService.CommitBodyOfBusObjs(rec, uow);
                        }
                    }
                }
                finally
                {
                    schedulerControl.EndUpdate();
                }
                cookedDishKeysToDelete.Clear();
                NumberOfSelectedAppointments = 0;   //reset my counter
                RefreshSearchResults(); //refresh the grid after all have been removed
            }
        }

        private void schedulerDataStorage1_AppointmentsInserted(object sender, PersistentObjectsEventArgs e)
        {
            if (IsRefreshingAppointments)
                return;
            schedulerControl.BeginUpdate();
            try
            {
                foreach (Appointment appointment in e.Objects)
                {
                    string debugstr = appointment.Subject;
                    CookedDishKey key = (CookedDishKey)appointment.LabelKey;
                    using (UnitOfWork uow = new UnitOfWork())
                    {
                        RecipeCard rec = (RecipeCard)XpoService.LoadHeadBusObjByKey(typeof(RecipeCard), key.RecipeCardOid, uow);
                        if (rec == null)
                            return;
                        CookedDish cookedDish = new CookedDish(uow);
                        cookedDish.CookedDate = appointment.Start;
                        rec.AddCookedDish(cookedDish);
                        XpoService.CommitBodyOfBusObjs(rec, uow);
                        key.CookedDishOid = cookedDish.Oid;  //set the newly assigned Oid in the Model
                        key.CookedDishDate = cookedDish.CookedDate;
                    }
                    RefreshIsRequired = true;
                }
            }
            finally
            {
                schedulerControl.EndUpdate();
            }
        }





        #endregion

        #region button event handlers

        //User can edit MyRating for a RecipeCard in the filter results grid.
        private void myScoreEdit_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            //load recipe from database
            using (UnitOfWork uow = new UnitOfWork())
            {
                RecipeCard rec = LoadFocusedRecipeCard(uow);
                if (rec == null)
                    return;
                int? myRating = (int?)this.searchResultsGridView.GetRowCellValue(this.searchResultsGridView.FocusedRowHandle, this.searchResultsGridView.Columns["MyRating"]);

                EditIntegerDialog id = new EditIntegerDialog("Edit MyRating", "MyRating",
                    RecipeCard.MyRatingMinValue, RecipeCard.MyRatingMaxValue, myRating);
                DialogResult dr = id.ShowDialog();
                if (dr == DialogResult.Cancel)
                    return;
                rec.MyRating = id.IntegerValue;
                XpoService.CommitBodyOfBusObjs(rec, uow);
                xpInstantFeedbackView1.Refresh();
                //Scroll to the edited Recipe after editing the recipe's my score
                SimpleCRUDFramework.DevExpressUxUtils.FocusTo(searchResultsGridView, rec.Oid);
            }
        }

        //Clicking on the RecipeCard Flag in the filter results grid toggles its value.
        private void FlaggedEdit_Click(object sender, EventArgs e)
        {
            using (UnitOfWork uow = new UnitOfWork())
            {
                RecipeCard rec = LoadFocusedRecipeCard(uow);
                if (rec == null)
                    return;
                bool currFlagValue = (bool)this.searchResultsGridView.GetRowCellValue(this.searchResultsGridView.FocusedRowHandle, this.searchResultsGridView.Columns["Flagged"]);
                rec.WouldLikeToTryFlag = !currFlagValue;  //toggle the flag
                XpoService.CommitBodyOfBusObjs(rec, uow);
                xpInstantFeedbackView1.Refresh();
                SimpleCRUDFramework.DevExpressUxUtils.FocusTo(searchResultsGridView, rec.Oid);
            }
        }

        //User can edit the last CookedDate for a RecipeCard in the filter results grid.
        private void repositoryItemButtonEdit1_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            EditLastCookedDate((ButtonEdit)sender, e.Button, this.searchResultsGridView);
        }
        //private void addDateEdit_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        //{
        //    EditLastCookedDate((ButtonEdit)sender, e.Button, this.ingrGrdVw);
        //}
        private void filterSimpleButton_Click(object sender, EventArgs e)
        {
            DoFilter(this.filterTermLookUpEdit, this.searchResultsGridView, imageCollection1.Images[0], imageCollection1.Images[1], imageCollection1.Images[2]);
        }
        private void clearFilterSimpleButton_Click(object sender, EventArgs e)
        {
            ClearSearchFilters();
        }
        private void newRecipeBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.ShowEditRecipeCardForm(null);
        }
        private void barButtonItem3_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DeleteFocusedRecipeCard();
        }
        private void groceryListBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ShowEditGroceryListForm();
        }
        private void editRecipeCardBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (searchResultsGridView.IsFocusedView)
            {
                int focusedRow = searchResultsGridView.FocusedRowHandle;
                if (focusedRow < 0)
                    return;
                int recipeIDToOpen = (int)searchResultsGridView.GetRowCellValue(focusedRow, this.searchResultsGridView.Columns["Oid"]);
                ShowEditRecipeCardForm(recipeIDToOpen);
            }
            else if (this.schedulerControl.Focused)
            {
                //edit an appointment if 1 and only 1 appointment is selected           
                if (schedulerControl.SelectedAppointments.Count != 1)
                    return;
                foreach (Appointment appt in schedulerControl.SelectedAppointments)
                {
                    CookedDishKey key = (CookedDishKey)appt.LabelKey;
                    ShowEditRecipeCardForm(key.RecipeCardOid);
                }
            }
        }
        private void barButtonItem4_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                RefreshForm();
            }
        }
        private void closeBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var mdi = this.MdiParent;
            this.Close();  //close me
            mdi.Close(); //close mdi         
        }
        private void viewRecBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (this.searchResultsGridView.IsFocusedView)
            {
                int focusedRow = searchResultsGridView.FocusedRowHandle;
                if (focusedRow < 0)
                    return;
                int recipeIDToOpen = (int)searchResultsGridView.GetRowCellValue(focusedRow, this.searchResultsGridView.Columns["Oid"]);
                ShowViewRecipeForm(recipeIDToOpen);
            }
            else if (this.schedulerControl.Focused)
            {
                if (schedulerControl.SelectedAppointments.Count != 1)
                    return;
                foreach (Appointment appt in schedulerControl.SelectedAppointments)
                {
                    CookedDishKey key = (CookedDishKey)appt.LabelKey;
                    ShowViewRecipeForm(key.RecipeCardOid);
                }
            }
        }
        private void showCalendarBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (showCalendarBarButtonItem.Caption == showCaption)
                ShowCalendar();
            else
                HideCalendar();
        }

        //Toggle month view, week view
        private void monthWeekBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (this.monthWeekBarButtonItem.Caption == monthViewCaption)
            {
                monthWeekBarButtonItem.ImageOptions.SvgImage = this.svgImageCollection1[0];
                schedulerControl.MonthView.WeekCount = 4;
                monthWeekBarButtonItem.Caption = weekViewCaption;
                if (MonthViewPosition == null)
                {
                    //default month height is 40%
                    int h = SystemInformation.VirtualScreen.Height;
                    MonthViewPosition = (int)((h - 100.0) * 0.4);
                }
                int currentPosition = splitContainerControl1.SplitterPosition;
                WeekViewPosition = currentPosition;
                splitContainerControl1.SplitterPosition = (int)MonthViewPosition;
            }
            else
            {
                //swap the image too
                //load image into imagecollection from project resource svg
                //https://supportcenter.devexpress.com/ticket/details/t700440/svg-image-collection-load-from-resources
                //https://supportcenter.devexpress.com/ticket/details/t646775/getting-image-form-barbuttonitem-in-code-no-longer-works-in-ver-18-1
                schedulerControl.MonthView.WeekCount = 1;
                monthWeekBarButtonItem.ImageOptions.SvgImage = this.svgImageCollection1[1];
                monthWeekBarButtonItem.Caption = monthViewCaption;
                int currentPosition = splitContainerControl1.SplitterPosition;
                MonthViewPosition = currentPosition;
                splitContainerControl1.SplitterPosition = (int)WeekViewPosition;
            }
        }
        private void scrollResultsToTopBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            searchResultsGridView.ClearSelection();
            searchResultsGridView.FocusedRowHandle = 0;
            searchResultsGridView.TopRowIndex = 0;
        }
        private void addToGLBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            //#RQT BtnAd2GL AddToGroceryList button:
            //#RQT BtnAd2GL 1. Populate SelectIngredientsForGroceryList form with all Ingredients of all selected RecipeCards (grid or calendar, whichever has focus).  
            //#RQT BtnAd2GL 2. User deletes the Items not needed, then adds the remaining to the GroceryList.
            AddSelectedToGroceryList();
        }
        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            //#RQT BtnAdBkUpDB Admin.BackupDB button: Backup DataStore schema definition and all data to a set of files in a folder on the PC.
            BackupDatabase();
        }
        private void barButtonItem5_ItemClick(object sender, ItemClickEventArgs e)
        {
            //Admin.RestoreDB button: Restore a previous Backup (folder of files) to a new DataStore.
            //#RQT BtnAdRstrDB Admin.RestoreDB button: Restore DataStore schema definition and all data from a backup folder.  Prompt user for the folder. 
            RestoreDatabase();
        }
        private void clearInputBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.filterTermLookUpEdit.EditValue = null;
            this.filterTermLookUpEdit.Text = null;
        }
        private void tdyDtNvgtrBrBtnItm_ItemClick(object sender, ItemClickEventArgs e)
        {
            //set visible intervals in sch
            //https://supportcenter.devexpress.com/ticket/details/t573315/how-to-change-navigation-button-programmatically-when-implementing-weekly-view
            this.schedulerControl.Start = DateTime.Today;
        }

        private void growCalendarBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.splitContainerControl1.SplitterPosition -= 25;
        }

        private void shrinkCalendarBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.splitContainerControl1.SplitterPosition += 25;
        }

        private void encryptSettingBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            EditTextDialog etd = new EditTextDialog(
                "Encrypt App Setting", "Setting to Encrypt");
            DialogResult dr = etd.ShowDialog();
            if (dr == DialogResult.OK && !string.IsNullOrEmpty(etd.Text))
            {
                EditTextDialog response = new EditTextDialog("Done", "Encrypted Setting");
                response.Text = Encryption.EncryptToHex(etd.Text);
                response.ShowDialog();
            }
        }

        private void helpBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            DevExpressUxUtils.OpenURLinBrowser(AppSettings.GetAppSetting("UserEdHelpVideoURL"));
        }
        private void addUserBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            //#TODO REFACTOR this code is quick and dirty for MVP
            string newEmail = null;
            try
            {
                EditTextDialog newUPdialog = new EditTextDialog(
                    "Add UserProfile", "User's Personal Email", defaultResponse: null, editMask: PrimitiveUtils.EmailMask);
                DialogResult dr = newUPdialog.ShowDialog();
                if (dr == DialogResult.OK && !string.IsNullOrEmpty(newUPdialog.Text))
                {
                    newEmail = newUPdialog.Text;
                    UnitOfWork u = new UnitOfWork();
                    UserProfile p = new UserProfile(u);
                    p.PersonalEmail = newEmail;
                    //these are required
                    p.FirstName = "x";
                    p.LastName = "x";
                    p.HomeZipCode = "12354";
                    p.PersonalCellNumber = "7777777777";
                    u.CommitChanges();
                    ShowMessageBox($"New user with email = {newEmail} created.", "Success");
                }
            }
            catch (Exception ex)
            {
                ShowMessageBox($"Error creating user {newEmail}: {ex.Message}", "Error");
            }
        }

        private void deleteUserBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            string emailToDelete = null;
            try
            {
                //#TODO REFACTOR this code is quick and dirty for MVP
                EditTextDialog newUPdialog = new EditTextDialog(
                "Delete UserProfile", "UserProfile Personal Email", defaultResponse: null, editMask: PrimitiveUtils.EmailMask);
                DialogResult dr = newUPdialog.ShowDialog();
                if (dr == DialogResult.OK && !string.IsNullOrEmpty(newUPdialog.Text))
                {
                    emailToDelete = newUPdialog.Text;
                    UnitOfWork uow = new UnitOfWork();
                    XPView xpView = new XPView(uow, typeof(UserProfile));
                    xpView.Properties.AddRange(new ViewProperty[] {
                    new ViewProperty("Oid", SortDirection.None, "[Oid]",false, true)
                    });
                    xpView.Criteria = CriteriaOperator.Parse($"PersonalEmail = '{PrimitiveUtils.SQLStringLiteral(emailToDelete)}'");
                    if (xpView.Count != 1)
                    {
                        ShowMessageBox($"Error deleting user {emailToDelete}: Email not found", "Error");
                        return;
                    }
                    ViewRecord rec = xpView[0];
                    UserProfile up = (UserProfile)XpoService.LoadHeadBusObjByKey(typeof(UserProfile), (int)rec["Oid"], uow);
                    up.Delete();
                    foreach (var uprb in up.UPRBs)
                    {
                        uprb.Delete();
                    }
                    uow.CommitChanges();
                    XpoService.PurgeDeletedObjects();
                    ShowMessageBox($"User with email = {emailToDelete} deleted. Their RecipeBox and GroceryList remains.", "Success");
                }

            }
            catch (Exception ex)
            {
                ShowMessageBox($"Error deleting user {emailToDelete}: {ex.Message}", "Error");
            }
        }

        #endregion
    }
}