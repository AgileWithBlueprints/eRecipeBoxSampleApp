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
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using static DataStoreUtils.DbConnection;

namespace DataStoreUtils
{
    public partial class XpoService
    {
        #region Connect to DataStore
        //#TODO IMPORTANT INSTALLATION NOTE/RESTRICTION.
        //For KIS, WE CREATE DATABASES AND USE DEFAULT sql server owner = dbo and default PostgreSQL schema public 
        static public DbProvider GetDbProvider(string connectionString)
        {
            string provider = ConnectionStrings.GetKeyValue("XpoProvider", connectionString);
            if (provider == "MSSqlServer")
                return DbConnection.DbProvider.SQLSERVER;
            else if (provider == "SQLite")
                return DbConnection.DbProvider.SQLITE;
            else if (provider == "Postgres")
                return DbConnection.DbProvider.NPGSQL;
            throw new NotImplementedException();
        }
        static public void ConnectDataStoreViaWCF(string wcfServiceURL, string user, string pw)
        {
            //ThreadSafeDataLayer
            XpoDefault.DataLayer = new ThreadSafeDataLayer(GetWSHttpDataStore(wcfServiceURL, user, pw));
            //XpoDefault.DataLayer = new SimpleDataLayer(GetWSHttpDataStore(wcfServiceURL, user, pw));
            UpdateModelVersions();
        }
        static public IDataStore ConnectDataStore(bool threadSafe, Type[] persistentTypes, string connStr)
        {
            //server mode case insensitive query devX T724378
            DevExpress.Data.Helpers.ServerModeCore.DefaultForceCaseInsensitiveForAnySource = true;

            ReflectionDictionary dictionary = new ReflectionDictionary();
            dictionary.GetDataStoreSchema(persistentTypes);   // Pass all of your persistent object types to this method.
            AutoCreateOption autoCreateOption = AutoCreateOption.DatabaseAndSchema;  // Use AutoCreateOption.DatabaseAndSchema if the database or tables do not exist. Use AutoCreateOption.SchemaAlreadyExists if the database already exists.
            IDataStore provider = XpoDefault.GetConnectionProvider(connStr, autoCreateOption);
            return provider;
        }
        static public IDataStore ConnectDefaultDataStore(Type[] persistentTypes, string appConfigConnectionStringEntry)
        {
            //server mode case insensitive query devX T724378
            DevExpress.Data.Helpers.ServerModeCore.DefaultForceCaseInsensitiveForAnySource = true;

            //https://supportcenter.devexpress.com/ticket/details/t941233/connecting-existing-sqlite-table-with-xpobject            
            //https://docs.devexpress.com/XPO/403900/best-practices/how-to-resolve-cannot-modify-dictionary-because-thread-safe-data-layer-uses-it
            //https://supportcenter.devexpress.com/ticket/details/t585951/how-to-load-data-asynchronously-using-xpo-in-a-winforms-application
            //https://docs.devexpress.com/XPO/403900/best-practices/how-to-resolve-cannot-modify-dictionary-because-thread-safe-data-layer-uses-it
            // Use AutoCreateOption.DatabaseAndSchema if the database or tables do not exist. Use AutoCreateOption.SchemaAlreadyExists if the database already exists.
            AutoCreateOption autoCreateOption = AutoCreateOption.DatabaseAndSchema;
            string connStr = ConnectionStrings.GetXPOConnectionString(appConfigConnectionStringEntry);
            IDataStore provider = XpoDefault.GetConnectionProvider(connStr, autoCreateOption);
            XpoDefault.DataLayer = (IDataLayer)new ThreadSafeDataLayer(provider);

            ReflectionDictionary dictionary = new ReflectionDictionary();
            dictionary.GetDataStoreSchema(persistentTypes);   // Pass all of your persistent object types to this method.
            IDataLayer dl = new SimpleDataLayer(dictionary, provider);
            Session session = new Session(dl);
            XpoDefault.Session = session;
            //UpdateModelVersions();
            return provider;
        }

        #endregion Connect

        #region Xpo View service

        static public void InitiateQuery(XPInstantFeedbackView xpInstantFeedbackView, CriteriaOperator withCriteria)
        {
            xpInstantFeedbackView.FixedFilterCriteria = withCriteria;
            bool systemTestLogging = AppSettings.GetBoolAppSetting("SystemTestLogging");
            if (systemTestLogging)
            {
                OperandValue[] criteriaParametersList;
                string criteriaString = CriteriaOperator.ToString(withCriteria, out criteriaParametersList);
                string crParms = DisplayString(criteriaParametersList);
                //https://supportcenter.devexpress.com/ticket/details/t755340/xpinstantfeedbacksource-and-total-row-count
                Type t = xpInstantFeedbackView.ObjectType;
                //#TODO BUG DevEx T1243794 Count is not the same count as grid results when using postgres.
                var count = XpoDefault.Session.Evaluate(t, new AggregateOperand("", Aggregate.Count), xpInstantFeedbackView.FixedFilterCriteria);
                Log.DataStore.Info($"QueryView|{t.Name}|{criteriaString}|{crParms}|ResultCount={count}");
            }
        }

        #endregion

        #region DataStore Business Object services 

        static public HeadBusinessObject LoadHeadBusObjByKey(Type headBusObjectType, int? Oid, UnitOfWork unitOfWork)
        {
            if (Oid == null)
                return null;
            HeadBusinessObject result = (HeadBusinessObject)unitOfWork.GetObjectByKey(headBusObjectType, Oid);
            bool systemTestLogging = AppSettings.GetBoolAppSetting("SystemTestLogging");
            if (systemTestLogging)
                LogHeadBOAction("LoadFromDataStore", result);
            if (result == null || result.IsDeleted)
                return null;
            else
                return result;
        }
        static public bool ObjectExistsInDataStore(BusinessObject businessObject)
        {
            XPClassInfo info = businessObject.ClassInfo;
            using (UnitOfWork uow = new UnitOfWork())
            {
                XPView xpView = new XPView(uow, info);
                xpView.Properties.AddRange(new ViewProperty[] {
                    new ViewProperty("Oid", SortDirection.None, "[Oid]",false, true),
                    });
                xpView.Criteria = CriteriaOperator.Parse($"Oid = {businessObject.Oid}", null);
                return xpView.Count > 0;
            }
        }
        static public void CommitBodyOfBusObjs(HeadBusinessObject headBusObject, UnitOfWork unitOfWork)
        {
            bool systemTestLogging = AppSettings.GetBoolAppSetting("SystemTestLogging");
            if (systemTestLogging)
                LogHeadBOAction("BeforeCommit", headBusObject);

            unitOfWork.CommitChanges();

            if (systemTestLogging)
                LogHeadBOAction("AfterCommit", headBusObject);
        }

        static public void CommitBodiesOfBusinessObjects(IEnumerable<HeadBusinessObject> headBusObjects, UnitOfWork unitOfWork)
        {
            //For those new to OO coding.
            //This explains why we use IEnumerable instead of IList.  There are no Add methods on IEnumerable.
            //https://stackoverflow.com/questions/16966961/cannot-convert-from-listderivedclass-to-listbaseclass

            unitOfWork.CommitChanges();

        }

        #endregion

        #region DataStore maintenance
        static public void PurgeDeletedObjects()
        {
            try
            {

                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    unitOfWork.PurgeDeletedObjects();
                    unitOfWork.CommitChanges();
                }
            }
            catch { }  //#TODO get error sometimes.. unable to recreate
        }


        #endregion DataStore maintenance        

        #region DataStore schema utilities

        //https://github.com/DevExpress-Examples/XPO_how-to-generate-sql-script-for-schema-migration
        static public void GenerateSchemaDDL(Type[] persistentTypes, string outFilePath, string emptySchemaXpoConnectionString)
        {
            IDataStore provider = XpoDefault.GetConnectionProvider(emptySchemaXpoConnectionString, AutoCreateOption.DatabaseAndSchema);

            var migrationProvider = (IDataStoreSchemaMigrationProvider)provider;
            var migrationScriptFormatter = (IUpdateSchemaSqlFormatter)provider;

            var dictionary = new ReflectionDictionary();
            DBTable[] targetSchema = dictionary.GetDataStoreSchema(persistentTypes);

            var migrationOptions = new SchemaMigrationOptions();
            var updateSchemaStatements = migrationProvider.CompareSchema(targetSchema, migrationOptions);
            string sql = null;
            try
            {
                sql = migrationScriptFormatter.FormatUpdateSchemaScript(updateSchemaStatements);
            }
            catch (Exception ex)
            {
                Log.App.Info($"Exception during GenerateSchemaDDL: {ex.Message}");
                throw;
            }
            //#TODO Test To be safe, run the script on empty, then compare the tables and col order datatype with target to ensure they are the same
            if (sql != null)
                File.WriteAllText(outFilePath, sql);
        }
        #endregion

    }
}
