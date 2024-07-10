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
using DevExpress.XtraEditors;
using Foundation;
using RecipeBoxSolutionModel;
using System;
using System.Windows.Forms;
using static eRecipeBox.DataStoreServiceReference.DataStoreServiceReference;

namespace eRecipeBox.Authentication
{
    public partial class Authenticator
    {
        #region authenticate user
        static public bool DoLogin()
        {
            bool usedCachedEmail = true;
            string user;
            string pw;
            string email;
            email = GetCachedEmail();
            LoginForm cr = new LoginForm();
            if (email == null)
            {
                usedCachedEmail = false;
                cr.ShowDialog();
                if (cr.DialogResult == DialogResult.OK)
                {
                    email = cr.UserProfile.PersonalEmail;
                    //#RQT LginCachEmlId Cache the user's email in a file (encrypted) on the client PC so eRecipeBox doesn't need to prompt the user for it on subsequent logins.
                    SaveEmailToCache(email);
                }
                else
                    return false;
            }
            bool useWCF = AppSettings.GetBoolAppSetting("UseWCF");

            //3-tier
            if (useWCF)
            {
                bool credentialsSuccess = Authenticator.GetUserCredentials(email, out user, out pw);
                if (!credentialsSuccess)
                {
                    XtraMessageBox.Show("Login failed: GetUserCredentials failed.  Perhaps out of sync with the server.");
                    return false;
                }
                string svcURL = ConnectionStrings.GetConnectionString(AppConfigDataStoreServiceSetting);
                ConnectDataStoreViaWCF(svcURL, user, pw);
            }
            else //2-tier
                ConnectDataStoreViaRDBConnection(ModelInfo.PersistentTypes, AppConfigConnectionSetting);

            string errMsg = UpdateUserProfileUponLogin(email, cr.UserProfile);
            //#TRICKY If usedCachedEmail and email not in DB, prompt user again for their email
            if (usedCachedEmail && errMsg == UserUnauthorizedMessage)
            {
                cr = new LoginForm();
                cr.ShowDialog();
                if (cr.DialogResult == DialogResult.OK)
                {
                    email = cr.UserProfile.PersonalEmail;
                    SaveEmailToCache(email);
                    string errMsg2 = UpdateUserProfileUponLogin(email, cr.UserProfile);
                    if (errMsg2 != null)
                        throw new Exception(errMsg2);
                }
                else
                    return false;
            }
            else if (errMsg != null)
                throw new Exception(errMsg);

            return true;
        }
        #endregion 
    }
}
