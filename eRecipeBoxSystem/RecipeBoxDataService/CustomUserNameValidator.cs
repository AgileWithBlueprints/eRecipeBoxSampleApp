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
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Foundation;
using RecipeBoxSolutionModel;
using System;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace RecipeBoxDataService
{
    public class CustomUserNameValidator : UserNamePasswordValidator
    {

        /// <summary>
        ///userName = Crypt.encryptToHex($"{email}\t{current datetime}");
        ///password = Crypt.encryptToHex($"{macAddress}\t{email.ToLower()}\t{clientSystemName}\t{email}");
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public override void Validate(string userName, string password)
        {
            if (Service.DataStore == null)
                Service.CreateDataStore();

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("User and Password can not be empty.");

            string[] parts = Encryption.DecryptHexString(userName).Split('\t');
            if (parts.Length != 2)
                throw new FaultException("Unknown Username or Incorrect Password");
            string email = parts[0];
            string datetime = parts[1];
            DateTime validateDT;

            //#RQT server requirement validating user creds to enable wcf communication            
            if (!DateTime.TryParse(datetime, out validateDT))
                throw new FaultException("Unknown Username or Incorrect Password");

            //last - Validate the pw
            parts = Encryption.DecryptHexString(password).Split('\t');
            if (parts.Length != 4)
                throw new FaultException("Unknown Username or Incorrect Password");
            string macAddress = parts[0];
            string clientSystemName = parts[1];
            string emailAgain = parts[2];
            datetime = parts[3];
            //sanity check
            if (!DateTime.TryParse(datetime, out validateDT))
                throw new FaultException("Unknown Username or Incorrect Password");

            string email2 = PrimitiveUtils.SQLStringLiteral(email);
            using (UnitOfWork uow = new UnitOfWork())
            {
                //#RQT 6.1.3.3.1.5.1 Authenticate User
                //#IMPORTANT!! this logic must match UpdateUserProfileUponLogin
                //#TODO REFACTOR Put in one spot and share with client logic!!! 
                XPView xpView = new XPView(uow, typeof(UserProfile));
                xpView.Properties.AddRange(new ViewProperty[] {
new ViewProperty("Oid", SortDirection.None, "[Oid]",false, true),
                    });
                var yy = $"PersonalEmail = '{email2}'";
                xpView.Criteria = CriteriaOperator.Parse($"PersonalEmail = '{email2}'");
                int numberFoundRecords = xpView.Count;

                //#TODO This error message is not displayed to the client. Triage, fix so user
                //gets approprate error message
                if (numberFoundRecords != 1)
                    throw new FaultException("Email must be registered. See your Administrator");
            }
        }
    }
}
