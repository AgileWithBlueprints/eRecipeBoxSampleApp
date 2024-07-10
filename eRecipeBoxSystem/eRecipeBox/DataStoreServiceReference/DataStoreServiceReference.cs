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
using DevExpress.Xpo.DB;
using Foundation;
using RecipeBoxSolutionModel;
using System;
using static DataStoreUtils.DbConnection;

namespace eRecipeBox.DataStoreServiceReference
{
    public class DataStoreServiceReference
    {
        public const string AppConfigDataStoreServiceSetting = "RecipesService";
        public const string AppConfigConnectionSetting = "Recipes";

        public const string UserUnauthorizedMessage = "User access denied.  User email must be in the database for authorization.  Verify proper database connection and email is in the UserProfile table.";

        //TODO REFACTOR protect these as R/O
        //My user profile for this winform session
        static public UserProfile MyUserProfile;
        static public int MyRecipeBoxOid;
        static public string MyRecipeBoxName;
        static public int MyGroceryListOid;
        static public bool IsSingleUserDeployment = false;



        //3-tier connection for a client 
        public static void ConnectDataStoreViaWCF(string wcfServiceURL, string user, string pw)
        {
            DataStoreUtils.XpoService.ConnectDataStoreViaWCF(wcfServiceURL, user, pw);
        }

        //2-tier connection for a client... a user on a local LAN or Dev, Test. administrator must be local
        public static DbProvider GetDbProvider()
        {
            string connStr = ConnectionStrings.GetXPOConnectionString(AppConfigConnectionSetting);
            DbProvider provider = DataStoreUtils.XpoService.GetDbProvider(connStr);
            return provider;
        }
        public static IDataStore ConnectDataStoreViaRDBConnection(Type[] persistentTypes, string appConfigConnectionStringEntry)
        {
            IDataStore defaultDS = DataStoreUtils.XpoService.ConnectDefaultDataStore(persistentTypes, appConfigConnectionStringEntry);
            string connStr = ConnectionStrings.GetXPOConnectionString(appConfigConnectionStringEntry);
            DbProvider provider = DataStoreUtils.XpoService.GetDbProvider(connStr);
            using (UnitOfWork uow = new UnitOfWork())
            {
                string cmd = RecipeBoxSolutionModel.SQL.RecipeCardSQL.GenerateConstraintAndIndexesSQL(provider.ToString());
                if (cmd != null)
                    uow.ExecuteNonQuery(cmd);
            }
            return defaultDS;
        }

        //Every time we connect, verify user is authorized to connect, then 
        //update our UserProfile with clientComputer and login info 
        //returns null if success, errorMessage if failed
        static public string UpdateUserProfileUponLogin(string email, UserProfile newProfile)
        {
            const string factoryUserID = "recipes1961@gmail.com";
            //update our user profile info
            using (UnitOfWork uow = new UnitOfWork())
            {
                //Count total UserProfiles
                XPView xpView = new XPView(uow, typeof(UserProfile));
                xpView.Properties.AddRange(new ViewProperty[] {
                new ViewProperty("Oid", SortDirection.None, "[Oid]",false, true)
                });
                xpView.Criteria = null;
                int userProfilesCount = xpView.Count;

                //look for factory userID
                xpView = new XPView(uow, typeof(UserProfile));
                xpView.Properties.AddRange(new ViewProperty[] {
                new ViewProperty("Oid", SortDirection.None, "[Oid]",false, true)
                });

                bool onlyUserIsFactoryUser = false;
                int factoryUserOID = -1;
                xpView.Criteria = CriteriaOperator.Parse($"PersonalEmail = '{factoryUserID}'");
                if (xpView.Count == 1)
                {
                    ViewRecord recipesRec = xpView[0];
                    factoryUserOID = (int)recipesRec["Oid"];
                    onlyUserIsFactoryUser = true;
                }

                if (factoryUserOID < 0)
                    factoryUserOID = 1;

                //#RQT #TRICKY #FRAGILE Special case for delivering to open source users.
                //if logging in for first time or runnning against stress test DB, overwrite factoryEmail with email  
                bool useWCF = AppSettings.GetBoolAppSetting("UseWCF");
                if ((userProfilesCount == 1 && !useWCF && onlyUserIsFactoryUser && email.ToLower() != factoryUserID) || userProfilesCount == 25000)
                {
                    MyUserProfile = (UserProfile)uow.GetObjectByKey(typeof(UserProfile), factoryUserOID);
                    if (MyUserProfile.InstalledClients.Count == 0 || userProfilesCount == 25000)
                    {
                        MyUserProfile.PersonalEmail = email;
                    }
                    else
                        return UserUnauthorizedMessage;
                    uow.CommitChanges();
                }

                bool GptApiKeyIsMissing = false;
                //#FRAGILE but adding this late in the game. if EncryptedGptApiKey isnt there we can assume IsSingleUserDeployment
                try
                {
                    string API_KEY = Encryption.DecryptHexString(AppSettings.GetAppSetting("EncryptedGptApiKey"));
                }
                catch
                {
                    GptApiKeyIsMissing = true;
                }
                if (GetDbProvider() == DbProvider.SQLITE && GptApiKeyIsMissing)
                    IsSingleUserDeployment = true;

                //#RQT 6.1.3.3.1.5.1 Authenticate User
                xpView.Criteria = CriteriaOperator.Parse($"PersonalEmail = '{PrimitiveUtils.SQLStringLiteral(email)}'");

                if (xpView.Count != 1)
                    return UserUnauthorizedMessage;
                ViewRecord rec = xpView[0];
                //#TRICKY.  do not use LoadHeadBusinessObjectByKey so automated test log is not dependent on UserProfile. 
                MyUserProfile = (UserProfile)uow.GetObjectByKey(typeof(UserProfile), (int)rec["Oid"]);

                //save new user profile attributes              
                if (MyUserProfile.UPRBs.Count == 0)
                {
                    //First login, so create a RB and GL for the user.
                    RecipeBox rb = new RecipeBox(uow);
                    rb.Name = MyUserProfile.PersonalEmail;
                    UserProfileRecipeBox myUPRB = new UserProfileRecipeBox(uow, rb, MyUserProfile);
                    GroceryList myGL = new GroceryList(uow);
                    rb.AddGroceryList(myGL);
                }
                else if (MyUserProfile.UPRBs[0].RecipeBox.GroceryLists.Count == 0)
                {
                    GroceryList myGL = new GroceryList(uow);
                    MyUserProfile.UPRBs[0].RecipeBox.AddGroceryList(myGL);
                }
                //update the profile every time prompted for authentication info
                if (newProfile != null && newProfile.FirstName != null)
                {
                    MyUserProfile.FirstName = newProfile.FirstName;
                    MyUserProfile.LastName = newProfile.LastName;
                    MyUserProfile.PersonalCellNumber = newProfile.PersonalCellNumber;
                    MyUserProfile.HomeZipCode = newProfile.HomeZipCode;
                }

                string myID = EnvironmentUtils.GetWindowsUserName();
                string macAddress = EnvironmentUtils.GetDefaultMacAddress();
                string systemName = Environment.MachineName;
                string myWindowsUserName = EnvironmentUtils.GetWindowsUserName();

                UserProfileClientComputer cc = MyUserProfile.FindSystemName(systemName);
                if (cc == null)
                {
                    cc = new UserProfileClientComputer(uow);
                    cc.ClientSystemName = systemName;
                    MyUserProfile.AddClientComputer(cc);
                }
                cc.WindowsUserName = myWindowsUserName;
                cc.ClientDeviceMAC = macAddress;
                if (cc.InstalledDate == null)
                    cc.InstalledDate = DateTime.Now;
                cc.NumberOfLogins += 1;
                cc.LastLoginDate = DateTime.Now;

                //#TRICKY. Do not use (XpoUtils.CommitBodyOfBusinessObjects(MyUserProfile, uow);) so automated test log is no dependent on UserProfile. 
                uow.CommitChanges();

                //#FRAGILE ASSUMPTION.  For MVP 1.0, assume each user has access to 1 RecipeBox.  And the RB has 1 GL.
                //Set Oids for my RB and GL
                MyRecipeBoxOid = MyUserProfile.UPRBs[0].RecipeBox.Oid;
                MyRecipeBoxName = MyUserProfile.UPRBs[0].RecipeBox.BusinessObjectDisplayName;
                MyGroceryListOid = MyUserProfile.UPRBs[0].RecipeBox.GroceryLists[0].Oid;
            }

            return null;
        }
    }
}
