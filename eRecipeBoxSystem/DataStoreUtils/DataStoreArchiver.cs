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
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using static DataStoreUtils.DbConnection;

namespace DataStoreUtils
{
    public partial class DataStoreArchiver
    {
        public string BackupDataStore(Type[] persistentTypes, string appConfigConnectionString, string backupRootDirectory, string backupName, bool resetOids)
        {
            string xpoConnectionString = ConnectionStrings.GetXPOConnectionString(appConfigConnectionString);
            DbProvider provider = XpoService.GetDbProvider(xpoConnectionString);
            string providerConnectionString = XpoDefault.Session.ConnectionString;

            string backupDirectory = BackupSchemaDefinition(persistentTypes, appConfigConnectionString, backupRootDirectory, backupName);

            //Save all table data in binary flat files.
            if (resetOids)
                ResetOidsInXpoTables(persistentTypes, providerConnectionString, provider, backupDirectory);
            else
                BackupXpoTablesToFiles(persistentTypes, providerConnectionString, provider, backupDirectory);
            return backupDirectory;
        }
        public string ReplicateBusObjBodies(
            Type headBusinessObject, int numberToReplicate, IList<string> forwardFKsToReplicate,
            IList<string> dontReplicateTheseBodyParts,
            Type[] persistentTypes, IList<string> businessKeys,
            string appConfigConnectionString, string backupRootDirectory, string backupName)
        {
            string xpoConnectionString = ConnectionStrings.GetXPOConnectionString(appConfigConnectionString);
            DbProvider provider = XpoService.GetDbProvider(xpoConnectionString);
            string providerConnectionString = XpoDefault.Session.ConnectionString;

            string backupDirectory = BackupSchemaDefinition(persistentTypes, appConfigConnectionString, backupRootDirectory, backupName);

            ReplicateBusObjBodies(headBusinessObject, numberToReplicate, forwardFKsToReplicate,
                dontReplicateTheseBodyParts,
                persistentTypes, businessKeys, providerConnectionString,
                provider, backupDirectory);
            return backupDirectory;
        }

        public delegate void CreateOneOfEach(UnitOfWork unitOfWork);
        public string RestoreDataStore(string fromBackupFolder, string targetXpoConnectionString, CreateOneOfEach createOneOfEach, Type[] persistentTypes)
        {
            //#NOTE this fails for postgres npgsql v8, so back reved to V702 DevX T1217611
            IDataLayer DL = XpoDefault.GetDataLayer(targetXpoConnectionString, AutoCreateOption.DatabaseAndSchema);
            DbProvider provider = XpoService.GetDbProvider(targetXpoConnectionString);

            //create the schema 
            if (provider == DbConnection.DbProvider.SQLITE)
            {
                using (UnitOfWork uow = new UnitOfWork(DL))
                {
                    //#WORKAROUND XPO doesn't generate schema DDL for SQLITE, so this the best hack I could think of
                    createOneOfEach(uow);

                    ReflectionDictionary dictionary = new ReflectionDictionary();
                    DBTable[] dBTables = dictionary.GetDataStoreSchema(persistentTypes);

                    foreach (var table in dBTables)
                    {
                        uow.ExecuteNonQuery($"delete from {table.Name};");
                    }
                    uow.ExecuteNonQuery("VACUUM;");
                    uow.CommitChanges();
                }
            }
            else if (provider == DbProvider.SQLSERVER || provider == DbProvider.NPGSQL)
            {
                if (!targetXpoConnectionString.ToLower().Contains("supabase"))
                {
                    string scriptPath = $"{fromBackupFolder}\\{SchemaDdlFileName}";
                    string sqlScript = File.ReadAllText(scriptPath);
                    using (UnitOfWork uow = new UnitOfWork(DL))
                    {
                        uow.ExecuteNonQuery(sqlScript);
                        uow.CommitChanges();
                    }
                }
            }
            else
                throw new NotImplementedException($"Restore for {provider} not supported.");

            bool disableAllIndexes = true;
            if (targetXpoConnectionString.ToLower().Contains("supabase"))
                disableAllIndexes = false;
            //Load data into the tables
            LoadDataFromBackup(DL, provider, fromBackupFolder, disableAllIndexes);

            if (targetXpoConnectionString.ToLower().Contains("supabase"))
            {
                string setSequences = @"
--#FRAGILE  this assumes we are using public schema!!  need to set all PK sequences to max(largest PK)
--#WORKAROUND seems like the generated backup ddl should do this for us
DO
$$
DECLARE
    rec RECORD;
BEGIN
    FOR rec IN
with mytables as (
SELECT c.relname, CONCAT('public.""',c.relname,'""')   AS fulltablename        
FROM pg_class c
JOIN pg_namespace n ON c.relnamespace = n.oid
WHERE n.nspname = 'public' and  c.relkind = 'r')
        SELECT
            c.relname AS table_name,
            a.attname AS column_name,
            pg_get_serial_sequence(mt.fulltablename, a.attname) AS seq_name
        FROM
            pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            JOIN pg_attribute a ON a.attrelid = c.oid
            JOIN pg_constraint ct ON ct.conrelid = c.oid AND a.attnum = ANY(ct.conkey)
            JOIN pg_attrdef ad ON ad.adrelid = c.oid AND ad.adnum = a.attnum
	    JOIN mytables mt on mt.relname=c.relname
        WHERE
            c.relkind = 'r' -- only real tables
            AND n.nspname = 'public' -- assuming public schema; adjust as necessary
            AND ct.contype = 'p' -- primary key constraint
            AND pg_get_serial_sequence(mt.fulltablename, a.attname) IS NOT NULL
    LOOP
        EXECUTE format('SELECT setval(''%s'', COALESCE(MAX(%I), 0) + 1) FROM %I', rec.seq_name, rec.column_name, rec.table_name) INTO rec;
    END LOOP;
END
$$;
";
                using (UnitOfWork uow = new UnitOfWork(DL))
                {
                    uow.ExecuteNonQuery(setSequences);
                    uow.CommitChanges();
                }

            }



            return DL.Connection.Database;
        }
    }
}
