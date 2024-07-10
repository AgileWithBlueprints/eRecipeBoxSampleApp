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
    [MapInheritance(MapInheritanceType.OwnTable)]
    /// <summary>
    /// Represents a User who has access to eRecipeBox app.
    /// UserProfile identity is driven off PersonalEmail.
    /// </summary>
    public class UserProfile : HeadBusinessObject
    {
        public UserProfile() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public UserProfile(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        override public string BusinessObjectDisplayName
        {
            get { return $"{this.PersonalEmail}"; }
        }

        [JsonIgnore]
        [Association("UserProfileUPRB")]
        public XPCollection<UserProfileRecipeBox> UPRBs
        {
            get { return GetCollection<UserProfileRecipeBox>(nameof(UPRBs)); }
        }

        [Nullable(false)]
        [Size(500)]
        public string FirstName
        {
            get { return fFirstName; }
            set { SetPropertyValue(nameof(FirstName), ref fFirstName, PrimitiveUtils.Clean(value)); }
        }
        string fFirstName;

        [Nullable(false)]
        [Size(500)]
        public string LastName
        {
            get { return fLastName; }
            set { SetPropertyValue(nameof(LastName), ref fLastName, PrimitiveUtils.Clean(value)); }
        }
        string fLastName;

        //#TODO Encrypt or even hash this in DataStore so it can't be hacked 
        [Nullable(false)]
        [Indexed(Unique = true)]
        [Size(500)]
        [BusinessKey]
        public string PersonalEmail
        {
            get { return fPersonalEmail; }
            set { SetPropertyValue(nameof(PersonalEmail), ref fPersonalEmail, PrimitiveUtils.Clean(value)); }
        }
        string fPersonalEmail;

        [Nullable(false)]
        [Size(50)]
        public string PersonalCellNumber
        {
            get { return fPersonalCellNumber; }
            set { SetPropertyValue(nameof(PersonalCellNumber), ref fPersonalCellNumber, PrimitiveUtils.JustDigits(value)); }
        }
        string fPersonalCellNumber;

        [Nullable(false)]
        [Size(10)]
        public string HomeZipCode
        {
            get { return fHomeZipCode; }
            set { SetPropertyValue(nameof(HomeZipCode), ref fHomeZipCode, PrimitiveUtils.Clean(value)); }
        }
        string fHomeZipCode;

        /// <summary>
        /// Set of client PCs where the user has run eRecipebox
        /// </summary>
        [JsonIgnore]
        [Association("UserProfileClientComputer"), Aggregated]
        public XPCollection<UserProfileClientComputer> InstalledClients
        {
            get
            {
                XPCollection<UserProfileClientComputer> result = GetCollection<UserProfileClientComputer>(nameof(InstalledClients));
                result.DisplayableProperties = "ClientDeviceMAC,ClientSystemName,InstalledDate";
                return result;
            }
        }

        public void AddClientComputer(UserProfileClientComputer newCC)
        {
            InstalledClients.Add(newCC);
        }

        public UserProfileClientComputer FindMACAddress(string deviceMAC)
        {
            foreach (UserProfileClientComputer cc in this.InstalledClients)
            {
                if (cc.ClientDeviceMAC == deviceMAC)
                    return cc;
            }
            return null;
        }

        public UserProfileClientComputer FindSystemName(string systemName)
        {
            foreach (UserProfileClientComputer cc in this.InstalledClients)
            {
                if (cc.ClientSystemName.ToLower() == systemName.ToLower())
                    return cc;
            }
            return null;
        }


    }
}
