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
using DevExpress.Xpo;
using Foundation;
using Newtonsoft.Json;
using System;

namespace RecipeBoxSolutionModel
{

    /// <summary>
    /// Represents a client PC where eRecipeBox has been installed. Track the MAC address where installed, number of logins, etc. for a single UserProfile.
    /// </summary>    
    public class UserProfileClientComputer : BusinessObject
    {
        public UserProfileClientComputer() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public UserProfileClientComputer(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        private UserProfile fUserProfile;
        [JsonIgnore]
        [Association("UserProfileClientComputer"), Aggregated]
        public UserProfile UserProfile
        {
            get { return fUserProfile; }
            set { SetPropertyValue(nameof(UserProfile), ref fUserProfile, value); }
        }

        [Size(100)]
        public string ClientDeviceMAC
        {
            get { return fClientDeviceMAC; }
            set { SetPropertyValue(nameof(ClientDeviceMAC), ref fClientDeviceMAC, PrimitiveUtils.Clean(value)); }
        }
        string fClientDeviceMAC;

        [Size(256)]
        [Nullable(false)]
        public string ClientSystemName
        {
            get { return fClientSystemName; }
            set { SetPropertyValue(nameof(ClientSystemName), ref fClientSystemName, PrimitiveUtils.Clean(value)); }
        }
        string fClientSystemName;

        [Size(256)]
        public string WindowsUserName
        {
            get { return fWindowsUserName; }
            set { SetPropertyValue(nameof(WindowsUserName), ref fWindowsUserName, PrimitiveUtils.Clean(value)); }
        }
        string fWindowsUserName;

        //#RQT InstlDtRboxApp Store date and time that eRecipeBox was installed on the client computer
        [JsonIgnore]
        public DateTime InstalledDate
        {
            get { return fInstalledDate; }
            set { SetPropertyValue(nameof(InstalledDate), ref fInstalledDate, value); }
        }
        DateTime fInstalledDate;

        //#RQT NmbrLgnsCC Log number of times the user has logged in from each client computer
        /// <summary>
        /// Increment each time recipie client starts up.  Most count will probably be Home network.  
        /// </summary>
        [JsonIgnore]
        public int NumberOfLogins
        {
            get { return fNumberOfLogins; }
            set { SetPropertyValue(nameof(NumberOfLogins), ref fNumberOfLogins, value); }
        }
        int fNumberOfLogins = 0;

        //#RQT LstLgnDtCC Store the most recent date and time that user logged in from each client computer
        [JsonIgnore]
        public DateTime LastLoginDate
        {
            get { return fLastLoginDate; }
            set { SetPropertyValue(nameof(LastLoginDate), ref fLastLoginDate, value); }
        }
        DateTime fLastLoginDate;

        //#TODO postMVP consider saving the client's external IP address to track where client is connecting to the server
    }
}
