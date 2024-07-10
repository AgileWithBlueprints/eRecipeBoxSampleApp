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
using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;
using DevExpress.XtraEditors;
using Foundation;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SimpleCRUDFramework
{
    public class CRUDBusinessObjectForm : MDIChildForm
    {

        #region properties
        public int? Oid { get; protected set; }

        public HeadBusinessObject FormHeadBusinessObject { get; protected set; }

        protected void NewUnitOfWork()
        {
            if (unitOfWork != null)
                unitOfWork.Dispose();
            unitOfWork = null;
        }
        UnitOfWork unitOfWork;
        public UnitOfWork UnitOfWork
        {
            get
            {
                if (unitOfWork == null)
                {
                    unitOfWork = new UnitOfWork();
                    //unitOfWork.TrackPropertiesModifications = true;  //#TODO Understand this better.  is it opt ccy at the field level?
                }
                return unitOfWork;
            }
            set
            {
                unitOfWork = value;
            }
        }

        #endregion properties

        #region ctor
        //#NOTE  this is only required so Visual Studio can create an instance of the form in the visual designer 
        public CRUDBusinessObjectForm() : base(null)
        {
        }

        public CRUDBusinessObjectForm(int? Oid, string formName, MDIParentForm parent) : base(formName, parent)
        {
            this.Oid = Oid;
        }

        #endregion ctor

        #region model view methods implemented by Edit<HeadBusinessObject> forms

        /// <summary>
        /// reload model from the data store
        /// </summary>
        /// <returns></returns>
        virtual protected bool ReloadModel() { return false; }

        //load model into view
        virtual protected void ModelToView() { }

        //load busobj into model then into view
        virtual protected bool ReloadModelAndView()
        {
            bool result;
            result = ReloadModel();
            if (!result)
                return result;
            ModelToView();
            return true;
        }

        //save view to model 
        virtual protected bool ViewToModel() { return true; }

        /// <summary>
        /// An exception occurred while saving the model.
        /// Allow the CRUD form to handle the exception and present
        /// a user friendly error message.
        /// </summary>
        /// <param name="ex">Exception returned from Data Store</param>
        /// <returns>True - expected error that user can fix.  False - Display the raw exception to the user.</returns>
        virtual protected bool ProcessDataStoreException(Exception ex)
        {
            HeadBusinessObject busObj = (HeadBusinessObject)this.FormHeadBusinessObject;
            string constraintName = HeadBusinessObject.ExtractUniqueConstraintName(ex.Message);
            //#WORKAROUND SQLite doesn't return the constraintName in the error message.  Bummer.
            if (constraintName == null)
            {
                //eg constraint failed\r\nUNIQUE constraint failed: RecipeCard.RecipeBox, RecipeCard.Title
                Regex reg = new Regex(@"UNIQUE constraint failed: RecipeCard.(?<property1>.*), RecipeCard.(?<property2>.*)");
                Match match = reg.Match(ex.InnerException.Message);
                if (match.Success)
                {
                    GroupCollection groups = match.Groups;
                    string propertyName1 = groups["property1"].Value;
                    string propertyName2 = groups["property2"].Value;
                    busObj.DataStoreSavingErrors[propertyName2] = $"An existing RecipeCard with {propertyName2} already exists. Enter a unique {propertyName2}.";
                    return true;
                }
            }

            //WCF doesnt seem to pass the inner exception for SQL Server back to the client. use #Workaround below
            ////https://supportcenter.devexpress.com/Ticket/Details/Q531802/how-to-handle-wcf-service-exceptions-on-the-client-side
            ////need to get the inner from wcf
            //if (ex.InnerException is SqlException)
            //{
            //    SqlException msSQLex = (SqlException)ex.InnerException;

            //    //#RQT unique check violation error from MSSQL
            //    //http://www.sql-server-helper.com/error-messages/msg-2627.aspx
            //    if (msSQLex.Number == 2627)
            //    {
            //    }
            //}


            //#WORKAROUND Just parse the entire message
            if (constraintName != null)
            {
                string className;
                string propertyName;
                HeadBusinessObject.ExtractClassNamePropertyName(constraintName, out className, out propertyName);
                //Is this my business object class?
                if (className.ToLower() == busObj.GetType().Name.ToLower())
                {
                    busObj.DataStoreSavingErrors[propertyName] = $"An existing {className} with {propertyName} already exists. Enter a unique {propertyName}.";
                    return true;
                }
                else
                    throw new Exception("Logic error. Unique DataStore constraints should only be on HeadBusinessObjects");
            }
            else
                return false;
        }

        public enum SaveErrorReason { UserCancelledSave, UserSaidDontSave, OptimisticConcurrencyError, BusObjectNoLongerExists, DataStoreException }

        /// <summary>
        /// Save model to persistent data store
        /// </summary>
        /// <returns>Save was successful</returns>
        virtual protected bool SaveModel(out SaveErrorReason? errorReason)
        {
            errorReason = null;
            try
            {
                ClearDataStoreErrors();
                FormHeadBusinessObject.SavingModel = true;
                XpoService.CommitBodyOfBusObjs(FormHeadBusinessObject, UnitOfWork);
                //For new BusinessObjects, save our newly assigned OID.
                Oid = FormHeadBusinessObject.Oid;
                return true;
            }
            catch (LockingException)
            {
                //determine if this is delete or update issue
                bool isUpdate = XpoService.ObjectExistsInDataStore(this.FormHeadBusinessObject);
                if (isUpdate)
                {
                    //#RQT Optimistic concurrency errror - simultaneous updates. Refresh lastest version of recipe and user re-edits. 
                    ShowMessageBox("The record was modified . Reloading current version. Apply changes to this version and Save again.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    errorReason = SaveErrorReason.OptimisticConcurrencyError;
                    ReloadModelAndView();
                    return false;
                }
                else
                {
                    //#RQT concurrency errror - object deleted. Close the form with no changes
                    ShowMessageBox("The record was deleted. Unable to save.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    errorReason = SaveErrorReason.BusObjectNoLongerExists;
                    return false;
                }

            }
            catch (Exception ex)
            {
                //Datastore threw an error while saving the body of businss objects to persistent storage.
                //Give the form a chance to process the error, notify the user, and allow
                //the user to correct the error.
                var errorWasProcessed = ProcessDataStoreException(ex);
                if (errorWasProcessed)
                {
                    //The form set errors indicating the error.  Notify the user to correct the error. 
                    ShowMessageBox("Please correct the errors indicated on the form.", "Form Errors");

                    //assigning Dummy forces the form to validate all properties again.  Errors will appear on the form.
                    //#TODO REFACTOR put this in MDIchildform?
                    if (this.FormHeadBusinessObject is HeadBusinessObject crudObj)
                        crudObj.Dummy = System.DateTime.Now;
                }
                else
                {
                    errorReason = SaveErrorReason.DataStoreException;
                    //#RQT all other errors.  report the error to the user.
                    ShowMessageBox(ex.Message, "Unhandled Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                return false;
            }
            finally
            {
                FormHeadBusinessObject.SavingModel = false;
            }
        }
        #endregion model view methods

        #region common save, delete, cancel logic

        protected void CloseActiveEditor()
        {
            //#TODO REFACTOR put this in a BASE CLASS? 
            ////Close editor in active control.
            ////https://supportcenter.devexpress.com/ticket/details/a1110/a-data-bound-editor-s-value-is-not-saved-when-data-is-posted-to-a-persistent-database
            Control activeCtrl = ActiveControl;
            while (activeCtrl is ContainerControl)
                activeCtrl = ((ContainerControl)activeCtrl).ActiveControl;
            if (activeCtrl is DevExpress.XtraEditors.TextBoxMaskBox)
                activeCtrl = activeCtrl.Parent;
            if (activeCtrl is DevExpress.XtraEditors.BaseEdit)
                ((DevExpress.XtraEditors.BaseEdit)activeCtrl).DoValidate();
        }
        protected virtual void ClearDataStoreErrors()
        {
            if (this.FormHeadBusinessObject is HeadBusinessObject crudObj)
            {
                if (crudObj.DataStoreSavingErrors.Count > 0)
                {
                    crudObj.DataStoreSavingErrors.Clear();
                    crudObj.Dummy = System.DateTime.Now;  //#TRICKY Here to potentially clear prevous errors on the form
                }
            }
        }

        protected virtual bool DoSave(bool reloadModelAndViewAfterSave = true)
        {
            SaveErrorReason? reason;
            return DoSave(out reason, reloadModelAndViewAfterSave);
        }

        protected virtual bool DoSave(out SaveErrorReason? reason, bool reloadModelAndViewAfterSave = true)
        {
            using (WaitCursor wc = new WaitCursor())
            {
                reason = null;
                if (!ViewToModel())
                    return false;
                if (!ValidateFormAndNotify())
                    return false;
                bool saveSuccess = SaveModel(out reason);
                if (reason != null && reason == SaveErrorReason.BusObjectNoLongerExists)
                    this.Close();
                else if (saveSuccess && reloadModelAndViewAfterSave)
                    ReloadModelAndView();  //someone else could have changed other parts of the body. 
                return saveSuccess;
            }
        }
        //return true - look at the results in the dialog.  false - ignore results in dialog


        protected virtual bool DoSaveAndClose(bool promptToSaveChanges = true)
        {
            SaveErrorReason? reason;
            return DoSaveAndClose(out reason, promptToSaveChanges);
        }

        protected virtual bool DoSaveAndClose(out SaveErrorReason? reason, bool promptToSaveChanges = true)
        {
            bool saveSuccess = DoSave(out reason, promptToSaveChanges);
            if (!saveSuccess)
                return false;
            this.DialogResult = DialogResult.OK;
            this.Close();
            return saveSuccess;
        }
        protected virtual bool DoCancel(bool promptToSaveChanges = true)
        {
            SaveErrorReason? reason;
            return DoCancel(out reason, promptToSaveChanges);
        }

        protected virtual bool DoCancel(out SaveErrorReason? reason, bool promptToSaveChanges = true)
        {
            reason = null;
            try
            {
                ViewToModel();
            }
            catch (Exception ex)
            {
                ShowMessageBox($"Error: {ex.Message}", "View To Model Error");
                this.DialogResult = DialogResult.Cancel;
                return false;
            }

            ICollection obsToSave = UnitOfWork.GetObjectsToSave();
            ICollection obsToDelete = UnitOfWork.GetObjectsToDelete();
            if (obsToSave.Count + obsToDelete.Count == 0)
            {
                //https://stackoverflow.com/questions/1882523/how-to-skip-validating-after-clicking-on-a-forms-cancel-button
                //disable any validation
                AutoValidate = AutoValidate.Disable;
                this.DialogResult = DialogResult.Cancel;
                Close();
                return true;  //nothing changed, so cancel was effectively "success"
            }

            //save, dont save, or cancel (go back)
            //#RQT Save, Dont Save, Cancel (i.e., Go back to the form)
            XtraMessageBoxArgs args = new XtraMessageBoxArgs();
            args.Showing += SaveDontSaveCancelText;
            args.Caption = "Save Confirmation";
            args.Text = "Do you want to save changes?";
            args.Buttons = new DialogResult[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel };
            DialogResult answer;
            if (promptToSaveChanges)
                answer = this.ShowMessageBox(args);
            else
                answer = DialogResult.Yes;
            if (answer == DialogResult.Yes)
            {
                if (DoSave(out reason, reloadModelAndViewAfterSave: false))
                {
                    this.DialogResult = DialogResult.Cancel;
                    Close();
                    return true;
                }
                else
                {
                    this.DialogResult = DialogResult.Cancel;
                    return false;  //save failed. Don't close. Cancel failed. 
                }
            }
            else if (answer == DialogResult.No)
            {
                //https://stackoverflow.com/questions/1882523/how-to-skip-validating-after-clicking-on-a-forms-cancel-button
                //disable any validation
                AutoValidate = AutoValidate.Disable;
                this.DialogResult = DialogResult.Cancel;
                reason = SaveErrorReason.UserSaidDontSave;
                Close();
                return false;  //do not want to save results
            }
            else
            {
                reason = SaveErrorReason.UserCancelledSave;
                return false;  //Go Back and let user make more changes  
            }
            throw new Exception("logic error unexpected");
        }

        protected virtual void DoHardCancel()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        public enum DeleteErrorReason { UserCancelledDelete, BusObjectWasUpdated, DataStoreException }

        protected virtual bool DoDeleteAndClose()
        {
            DeleteErrorReason? errorReason;
            return DoDeleteAndClose(out errorReason);

        }
        protected virtual bool DoDeleteAndClose(out DeleteErrorReason? errorReason)
        {
            errorReason = null;
            string question = string.Format("Confirm delete recipe {0}?", ((BusinessObject)FormHeadBusinessObject).BusinessObjectDisplayName);
            DialogResult answer = ShowMessageBox(this.MdiParent, question, "Confirmation", MessageBoxButtons.YesNo);
            if (answer == DialogResult.Yes)
            {
                if (FormHeadBusinessObject.Oid >= 0)
                {
                    try
                    {
                        using (WaitCursor wc = new WaitCursor())
                        {
                            FormHeadBusinessObject.Delete();
                            XpoService.CommitBodyOfBusObjs(FormHeadBusinessObject, UnitOfWork);
                        }
                    }
                    catch (LockingException)
                    {
                        //determine if this is delete or update issue
                        bool isUpdate = XpoService.ObjectExistsInDataStore(FormHeadBusinessObject);
                        if (isUpdate)
                        {
                            //#RQT concurrency errror - simultaneous updates. Refresh lastest version of recipe and re-delete. 
                            ShowMessageBox("The record was modified . Reloading current version. Attempt to delete again.", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            errorReason = DeleteErrorReason.BusObjectWasUpdated;
                            ModelToView();
                            return false;
                        }
                        else
                        {
                            //someone else already deleted it. so do nothing
                            ;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorReason = DeleteErrorReason.DataStoreException;
                        ShowMessageBox($"Fatal error while deleting: {ex.Message}", "Fatal Error");
                        return false;
                    }
                }
                this.Oid = null;
                this.FormHeadBusinessObject = null;
                AutoValidate = AutoValidate.Disable;
                this.Close();
                return true;
            }
            else
            {
                errorReason = DeleteErrorReason.UserCancelledDelete;
                return false;
            }
        }

        protected void SaveDontSaveCancelText(object sender, XtraMessageShowingArgs e)
        {
            e.Buttons[DialogResult.Yes].Text = "Save";
            e.Buttons[DialogResult.No].Text = "Don't Save";
            e.Buttons[DialogResult.Cancel].Text = "Cancel";
        }

        #endregion common save, delete, cancel logic
    }
}
