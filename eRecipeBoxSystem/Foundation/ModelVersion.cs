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
using System;
using System.Collections.Generic;
using System.Linq;
namespace Foundation
{
    //#TODO REFACTOR This was developed quick and dirty. Encapsulate RegisteredDataStoreComponentVersions
    public class ModelVersion : XPObject
    {
        public ModelVersion() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public ModelVersion(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        private string fDataStoreComponent;
        [Nullable(false)]
        [Size(256)]
        [Indexed("Version", Unique = true)]
        public string DataStoreComponent
        {
            get { return fDataStoreComponent; }
            set
            {
                SetPropertyValue(nameof(DataStoreComponent), ref fDataStoreComponent, PrimitiveUtils.Clean(value));
            }
        }

        public const string SchemaDefinition = "SchemaDefinition";
        public const string ReferenceData = "ReferenceData";

        private DateTime fVersion;
        [Nullable(false)]
        public DateTime Version
        {
            get { return fVersion; }
            set { SetPropertyValue(nameof(Version), ref fVersion, value); }
        }

        private bool fIsMajorVersion;
        [Nullable(false)]
        public bool IsMajorVersion
        {
            get { return fIsMajorVersion; }
            set { SetPropertyValue(nameof(IsMajorVersion), ref fIsMajorVersion, value); }
        }

        private string fComment;
        [Size(4000)]
        public string Comment
        {
            get { return fComment; }
            set
            {
                SetPropertyValue(nameof(Comment), ref fComment, PrimitiveUtils.Clean(value));
            }
        }

        public void Set(string dataStoreComponent, DateTime version, bool isMajorVersion, string comment)
        {
            DataStoreComponent = dataStoreComponent;
            Version = version;
            IsMajorVersion = isMajorVersion;
            Comment = comment;
        }

        static public List<ModelVersion> RegisteredDataStoreComponentVersions = new List<ModelVersion>();

        static public void RegisterNewVersion(string dataStoreComponent, DateTime version, bool isMajorVersion, string comment)
        {
            ModelVersion newOne = new ModelVersion();
            newOne.Set(dataStoreComponent, version, isMajorVersion, comment);
            RegisteredDataStoreComponentVersions.Add(newOne);
        }

        static private ModelVersion FindVersion(DevExpress.Xpo.XPQuery<ModelVersion> versions, DateTime version)
        {
            foreach (var x in versions)
            {
                if (version == x.Version)
                    return x;
            }
            return null;
        }

        static public bool PersistDataStoreComponentVersion(ModelVersion version, UnitOfWork uow)
        {
            XPQuery<ModelVersion> modelVersionQuery = new XPQuery<ModelVersion>(uow);

            var versions = from v in modelVersionQuery
                               //#WORKAROUND publish #DEVEXPRESS " && v.Version == version.Version " doesn't seem to work with SQLite, so perform the query ourselves on the client.
                               //Report to DexExpress
                           where (v.DataStoreComponent == version.DataStoreComponent)
                           select v;


            ModelVersion foundVersion = FindVersion((XPQuery<ModelVersion>)versions, version.Version);
            if (foundVersion == null)
            {
                ModelVersion newOne = new ModelVersion(uow);
                newOne.Set(version.DataStoreComponent, version.Version, version.IsMajorVersion, version.Comment);
                uow.CommitChanges();
                return true;
            }
            else
            {
                if (foundVersion.Comment == version.Comment && foundVersion.IsMajorVersion == version.IsMajorVersion)
                    return false;
                foundVersion.IsMajorVersion = version.IsMajorVersion;
                foundVersion.Comment = version.Comment;
                uow.CommitChanges();
                return true;
            }
        }
    }
}
