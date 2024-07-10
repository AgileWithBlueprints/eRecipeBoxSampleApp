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
using DevExpress.Xpo.Metadata.Helpers;
using Foundation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using static DataStoreUtils.DbConnection;

namespace DataStoreUtils
{
    public partial class DataStoreArchiver
    {
        #region statics to support backup threads
        private static int nextThreadParameterIndex = -1;
        private static int GetNextThreadParm()
        {
            return Interlocked.Increment(ref nextThreadParameterIndex);
        }
        private static List<BackupTableToFileParms> lbfThreadParms;
        #endregion

        #region constants
        private const string SchemaDdlFileName = "SchemaDDL.txt";
        private const string SchemaReportFileName = "SchemaReport.txt";
        #endregion

        //#TODO use folder directory consistently throughout 

        #region Backup tables, schema def and schema def report to a folder
        private string BackupSchemaDefinition(Type[] persistentTypes, string appConfigConnectionString, string backupRootDirectory, string backupName)
        {
            string xpoConnectionString = ConnectionStrings.GetXPOConnectionString(appConfigConnectionString);
            DbProvider provider = XpoService.GetDbProvider(xpoConnectionString);

            //#RQT Create parent folder for each database name.  Backup versions are inside it.
            string dbName = XpoDefault.Session.Connection.Database;
            string buDbFolderName = dbName;
            string sqLiteDataSource;
            if (provider == DbProvider.SQLITE)
            {
                sqLiteDataSource = ConnectionStrings.GetKeyValue("Data Source", xpoConnectionString);
                FileInfo fi = new FileInfo(sqLiteDataSource);
                if (!fi.Exists)
                    throw new Exception($"Unable to find SQLite file {sqLiteDataSource}");
                //#RQT Exception for SQLite, parent folder name is the same as the SQLite db file name prefix.  Backup versions are inside it.
                buDbFolderName = fi.Name.Replace(".sqllitedb", "");
            }

            //Folder naming pattern is root\dbname\datetimenow\backup files
            Directory.CreateDirectory(backupRootDirectory);
            string dbdir = backupRootDirectory + '\\' + buDbFolderName;
            Directory.CreateDirectory(dbdir);
            string suffix = backupName;
            if (string.IsNullOrEmpty(backupName))
                suffix = "";
            string backupDirectory = $"{dbdir}\\{DateTime.Now.ToString("yyyy-MM-dd_HHmmss")}_{backupName}".TrimEnd('_');
            Directory.CreateDirectory(backupDirectory);
            string ddlPath = $"{backupDirectory}\\{SchemaDdlFileName}";
            string reportPath = $"{backupDirectory}\\{SchemaReportFileName}";

            BackupSchemaDefinitionVersion(backupDirectory);

            //Create a DDL script to restore the schema 
            if (provider == DbProvider.SQLSERVER || provider == DbProvider.NPGSQL)
            {
                //#WORKAROUND DevExpress ticket T1189443 this isn't pretty or robust, but will work for development and test.
                //Using the original XPO connection string as a template, hardcode the connection string to reference EmptySchema
                string emptySchemaXpoConnectionString = xpoConnectionString.Replace("Database=" + dbName, "Database=EmptySchemaZZuniqueName");

                //#WORKAROUND only get 1 database per supabase project.  so skip attempting to generate the DDL
                if (!emptySchemaXpoConnectionString.ToLower().Contains("supabase"))
                    XpoService.GenerateSchemaDDL(persistentTypes, ddlPath, emptySchemaXpoConnectionString);
            }
            else if (provider == DbProvider.SQLITE)
            {
                //#WORKAROUND DevExpress (ticket T1111909) doesn't support creating a script for SQLite, so just copy entire DB file.
                //<add name="Recipes" connectionString="XpoProvider=SQLite;Data Source=C:\temp\recipesSQLLight\test.sqllitedb" />
                string dataSource = ConnectionStrings.GetKeyValue("Data Source", xpoConnectionString);
                FileInfo fi = new FileInfo(dataSource);
                if (!fi.Exists)
                    throw new Exception($"Unable to find SQLite file {dataSource}");
                string destFilename = $"{backupDirectory}\\{fi.Name}";
                File.Copy(fi.FullName, destFilename);
            }

            string providerConnectionString = XpoDefault.Session.ConnectionString;
            GenerateSchemaReport(providerConnectionString, provider, reportPath);
            return backupDirectory;
        }
        private void BackupSchemaDefinitionVersion(string toFolder)
        {
            //#RQT Save the latest/current SchemaDefinition Version in a file.
            using (UnitOfWork uow = new UnitOfWork())
            {
                //Query the latest version of SchemaDefinition
                XPQuery<ModelVersion> metaDataQuery = new XPQuery<ModelVersion>(uow);
                var versions = (from v in metaDataQuery
                                orderby v.Version descending
                                where v.DataStoreComponent == ModelVersion.SchemaDefinition
                                select v).Take(1);
                if (versions.Count() > 0)
                {
                    ModelVersion latest = versions.First();
                    string latestVersion = latest.Version.ToString("yyyy-MM-dd HH:mm:ss");
                    StreamWriter verFile = new StreamWriter($"{toFolder}\\RecipeBoxVersions.txt");
                    verFile.WriteLine($"RecipeBoxSchemaDefinitionVersion={latestVersion}    MajorVersion={latest.IsMajorVersion}    Comment={latest.Comment}");
                    verFile.Close();
                }
            }
        }
        private void GenerateSchemaReport(string connString, DbProvider provider, string directory)
        {
            using (DbConnection DBConnection = NewDbConnection(connString, provider))
            {
                ((IDbConnection)DBConnection).Open();
                DBConnection.GenerateSchemaReport(directory);
                ((IDbConnection)DBConnection).Close();
            }
        }
        private void BackupTableToFileThread(object threadIdx)
        {
            try
            {
                BackupTableToFileParms p = lbfThreadParms[(int)threadIdx];
                BackupTableToFile(p.TableName, p.ConnectionString, p.Provider, p.Directory);

                //process next one
                int nextTParm = GetNextThreadParm();
                while (nextTParm < lbfThreadParms.Count)
                {
                    p = lbfThreadParms[nextTParm];
                    BackupTableToFile(p.TableName, p.ConnectionString, p.Provider, p.Directory);
                    nextTParm = GetNextThreadParm();
                }
            }
            catch
            {
                //#TODO log
                throw;
            }
        }
        private bool TableIsReferenced(string tableName, DBTable[] schema)
        {
            foreach (DBTable t in schema)
            {
                foreach (DBForeignKey foreignKey in t.ForeignKeys)
                {
                    if (foreignKey.PrimaryKeyTable == tableName)
                        return true;
                }
            }
            return false;
        }
        private void BackupXpoTablesToFiles(Type[] persistentTypes, string connectionString, DbProvider provider,
        string rootDirectory, int numberOfThreads = 3)
        {
            lbfThreadParms = new List<BackupTableToFileParms>();
            ReflectionDictionary dictionary = new ReflectionDictionary();
            DBTable[] targetSchema = dictionary.GetDataStoreSchema(persistentTypes);
            foreach (DBTable t in targetSchema)
            {
                BackupTableToFileParms newp = new BackupTableToFileParms
                {
                    ConnectionString = connectionString,
                    Provider = provider,
                    Directory = rootDirectory,
                    TableName = t.Name
                };
                lbfThreadParms.Add(newp);
            }
            List<Thread> threads = new List<Thread>();
            nextThreadParameterIndex = numberOfThreads - 1;
            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread t = new Thread(BackupTableToFileThread);
                threads.Add(t);
            }
            for (int i = 0; i < threads.Count; i++)
            {
                threads[i].Start(i);
            }
            for (int i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }
        }
        #endregion

        #region ResetOids in all tables during backup
        private void ResetOidsInXpoTables(Type[] persistentTypes, string connectionString, DbProvider provider,
            string rootDirectory)
        {
            if (provider == DbProvider.SQLITE)
                throw new Exception("Reseting OIDs is not supported with SQLite.");

            //purge soft deletes
            XpoService.PurgeDeletedObjects();

            //Step 1: Create a one up map for each referenced table. old oid to new oid
            IDictionary<string, IDictionary<int, int>> tableToOidMaps = new Dictionary<string, IDictionary<int, int>>();
            ReflectionDictionary dictionary = new ReflectionDictionary();
            DBTable[] targetSchema = dictionary.GetDataStoreSchema(persistentTypes);
            foreach (DBTable t in targetSchema)
            {
                //Optimization.  Only need maps for tables that have references
                if (!TableIsReferenced(t.Name, targetSchema))
                    continue;
                using (DbConnection DBConnection = NewDbConnection(connectionString, provider))
                {
                    ((IDbConnection)DBConnection).Open();
                    tableToOidMaps[t.Name] = CreateOidMap(DBConnection, t.Name);
                    ((IDbConnection)DBConnection).Close();
                }
            }

            //Step 2: Create a map of all foreign key cols - and who they reference
            IDictionary<string, string> columnNameToReferencedTable = new Dictionary<string, string>();
            foreach (DBTable t in targetSchema)
            {
                foreach (DBForeignKey foreignKey in t.ForeignKeys)
                {
                    List<string> colNames = new List<string>();
                    foreach (string col in foreignKey.Columns) { colNames.Add(col); }
                    string colName = string.Join(".", colNames);
                    string fullColName = $"{t.Name}.{colName}";
                    columnNameToReferencedTable[fullColName] = foreignKey.PrimaryKeyTable;
                }
            }

            //Step 3: Write each table to binary file, assign new oid for all PKs and FKs
            using (DbConnection DBConnection = NewDbConnection(connectionString, provider))
            {
                foreach (DBTable t in targetSchema)
                {
                    ((IDbConnection)DBConnection).Open();
                    ResetOidsToBinaryFile(t.Name, rootDirectory, tableToOidMaps, columnNameToReferencedTable, DBConnection);
                    ((IDbConnection)DBConnection).Close();
                }
            }
        }
        public string ResetOidsToBinaryFile(string tableName, string directory, IDictionary<string, IDictionary<int, int>> tableToOidMaps,
            IDictionary<string, string> columnNameToReferencedTable, DbConnection DBConnection)
        {
            string divider = '\\' + "";
            if (directory.EndsWith(@"\"))
                divider = "";
            string targetFilePath = directory + divider + tableName + ".data";

            // open first out file          
            if (File.Exists(targetFilePath))
                File.Delete(targetFilePath);

            FileStream fs = new FileStream(targetFilePath, FileMode.Create);
            BufferedStream bs = new BufferedStream(fs, 8192000);

            OidMapperState tableMapperState = new OidMapperState
            {
                Writer = new BinaryWriter(bs),
                TableToOidMaps = tableToOidMaps,
                ColumnNameToReferencedTable = columnNameToReferencedTable,
                CurrentTableName = tableName
            };

            string OIDname = DBConnection.SQLColumnName("OID");
            string cmd = $"select * from {DBConnection.SQLTableName(tableName)} order by {OIDname};";
            //Write table info header
            tableMapperState.Writer.Write(tableName);
            DBConnection.RunSqlSelect(cmd, WriteColumnInfos02, WriteMappedCellToFile, null, tableMapperState);
            tableMapperState.Writer.Close();
            bs.Close();
            fs.Close();
            return targetFilePath;
        }
        private void WriteColumnInfos02(IList<DbColumnInfo> columnInfos, object runSelectState)
        {
            OidMapperState tableMapperState = (OidMapperState)runSelectState;
            tableMapperState.Writer.Write((int)columnInfos.Count);
            foreach (DbColumnInfo dbColumnInfo in columnInfos)
            {
                dbColumnInfo.TableName = tableMapperState.CurrentTableName;
                tableMapperState.Writer.Write(dbColumnInfo.Name);
                tableMapperState.Writer.Write(dbColumnInfo.DatatypeID);
                tableMapperState.Writer.Write(dbColumnInfo.DatatypeName);
                tableMapperState.Writer.Write(dbColumnInfo.MaxLength);
                tableMapperState.Writer.Write(dbColumnInfo.IsNullable);
            }
        }
        private void WriteMappedCellToFile(long resultNumber, DbColumnInfo columnInfo, object colValue, object runQueryState)
        {
            try
            {
                OidMapperState tableMapperState = (OidMapperState)runQueryState;
                string fullColName = $"{columnInfo.TableName}.{columnInfo.Name}";
                if (tableMapperState.ColumnNameToReferencedTable.ContainsKey(fullColName)
                    && columnInfo.DatatypeID == BinaryFileConstants.intID)
                {
                    int origOid = (int)colValue;
                    if (origOid == BinaryFileConstants.nullInt32)
                        tableMapperState.Writer.Write(origOid);
                    else
                    {
                        //FK column, so write the new, mapped reference int
                        var x = tableMapperState.ColumnNameToReferencedTable[fullColName];
                        IDictionary<int, int> oidMap = tableMapperState.TableToOidMaps[x];
                        tableMapperState.Writer.Write(oidMap[(int)colValue]);
                    }
                }
                else if (columnInfo.Name.ToUpper() == "OID")
                {
                    //PK Oid column that is not referenced, so set the new PK as the one up (row) number 
                    tableMapperState.Writer.Write((int)resultNumber);
                }
                else
                {
                    if (colValue is string)
                        tableMapperState.Writer.Write((string)colValue);
                    else if (colValue is DateTime)
                        tableMapperState.Writer.Write((Int64)((DateTime)colValue).Ticks);
                    else if (colValue is Int64)
                        tableMapperState.Writer.Write((Int64)colValue);
                    else if (colValue is Int32)
                        tableMapperState.Writer.Write((Int32)colValue);
                    else if (colValue is Int16)
                        tableMapperState.Writer.Write((Int16)colValue);
                    else if (colValue is double)
                        tableMapperState.Writer.Write((double)colValue);
                    else if (colValue is float)
                        tableMapperState.Writer.Write((float)colValue);
                    else if (colValue is decimal)
                        tableMapperState.Writer.Write((decimal)colValue);
                    else if (colValue is Guid)
                        tableMapperState.Writer.Write(((Guid)colValue).ToByteArray());
                    else if (colValue is Byte)
                        tableMapperState.Writer.Write((Byte)colValue);
                    else
                        throw new Exception($"backup of type {colValue.GetType().Name} not supported");
                }
            }
            catch
            {
                throw;
            }
        }
        public Dictionary<int, int> CreateOidMap(DbConnection DBConnection, string tableName)
        {
            //generate one up oids for the pk col
            Dictionary<int, int> oldNewOidMap = new Dictionary<int, int>();
            string OIDname = DBConnection.SQLColumnName("OID");
            string cmd = $"select {OIDname} from {DBConnection.SQLTableName(tableName)} order by {OIDname};";
            DBConnection.RunSqlSelect(cmd, null, AddOidMapEntry, null, oldNewOidMap);
            return oldNewOidMap;
        }
        private void AddOidMapEntry(long resultNumber, DbColumnInfo columnInfo, object colValue, object runQueryState)
        {
            Dictionary<int, int> oldNewOidMap = (Dictionary<int, int>)runQueryState;
            oldNewOidMap[(int)colValue] = (int)resultNumber;
        }

        #endregion

        #region Replicate Bus Obj Bodies to generate large test data

        private IDictionary<string, DBTable> FindAllTablesThatReferenceThisHead(Type[] persistentTypes, Type headBusinessObject,
            Dictionary<string, DBForeignKey> forwardFKsToReplicate)
        {
            Dictionary<DBTable, Type> allTablesThatReferenceHead = new Dictionary<DBTable, Type>();
            XPClassInfo headClassInfo = Session.DefaultSession.GetClassInfo(headBusinessObject);

            //Add Head of the body to the set
            allTablesThatReferenceHead[headClassInfo.Table] = headBusinessObject;
            int beforeTableCount = 0;
            int afterTableCount = 1;
            while (beforeTableCount < afterTableCount)
            {
                Dictionary<Type, int> currentTypesInSet = new Dictionary<Type, int>();
                foreach (Type t in allTablesThatReferenceHead.Values)
                    currentTypesInSet[t] = 1;

                beforeTableCount = allTablesThatReferenceHead.Count;
                foreach (Type tableType in currentTypesInSet.Keys)
                {
                    XPClassInfo classInfo = Session.DefaultSession.GetClassInfo(tableType);
                    AddTablesThatReferenceThisType(persistentTypes, classInfo, allTablesThatReferenceHead, forwardFKsToReplicate);
                }
                afterTableCount = allTablesThatReferenceHead.Count;
            }
            Dictionary<string, DBTable> result = new Dictionary<string, DBTable>();
            foreach (DBTable t in allTablesThatReferenceHead.Keys)
                result[t.Name] = t;

            return result;
        }
        private void WriteColumnInfos03(IList<DbColumnInfo> columnInfos, object runQueryState)
        {
            ReplicateState replicateState = (ReplicateState)runQueryState;
            replicateState.ColumnInfos = columnInfos;
            replicateState.Writer.Write((int)columnInfos.Count);
            foreach (DbColumnInfo dbColumnInfo in columnInfos)
            {
                replicateState.Writer.Write(dbColumnInfo.Name);
                replicateState.Writer.Write(dbColumnInfo.DatatypeID);
                replicateState.Writer.Write(dbColumnInfo.DatatypeName);
                replicateState.Writer.Write(dbColumnInfo.MaxLength);
                replicateState.Writer.Write(dbColumnInfo.IsNullable);
            }
        }
        public string ReplicateTableToBinaryFile(string tableName, int numberToReplicate, string directory,
            IDictionary<string, int> foreignKeyColNamesToReplicate,
            IList<string> businessKeys, DbConnection DBConnection)
        {
            string divider = '\\' + "";
            if (directory.EndsWith(@"\"))
                divider = "";
            string targetFilePath = directory + divider + tableName + ".data";

            // open first out file          
            if (File.Exists(targetFilePath))
                File.Delete(targetFilePath);

            FileStream fs = new FileStream(targetFilePath, FileMode.Create);
            BufferedStream bs = new BufferedStream(fs, 8192000);

            Dictionary<string, int> businessKeysDict = new Dictionary<string, int>();
            foreach (string busKey in businessKeys)
                businessKeysDict[busKey] = 1;
            ReplicateState replicateState = new ReplicateState
            {
                Writer = new BinaryWriter(bs),
                NumberOfReplications = numberToReplicate,
                TableName = tableName,
                BusinessKeys = businessKeysDict,
                ForeignKeyColNamesToReplicate = foreignKeyColNamesToReplicate
            };

            string cmd = $"select * from {DBConnection.SQLTableName(tableName)};";
            //Write table info header
            replicateState.Writer.Write(tableName);
            DBConnection.RunSqlSelect(cmd, WriteColumnInfos03, null, ReplicateRow, replicateState);
            replicateState.Writer.Close();
            bs.Close();
            fs.Close();
            return targetFilePath;
        }

        private void ReplicateRow(long resultNumber, object[] colValues, object runSelectState)
        {
            try
            {
                ReplicateState replicateState = (ReplicateState)runSelectState;
                for (int writeCount = 1; writeCount <= replicateState.NumberOfReplications + 1; writeCount++)
                {
                    //write a row
                    for (int colIdx = 0; colIdx < colValues.Length; colIdx++)
                    {

                        DbColumnInfo colInfo = replicateState.ColumnInfos[colIdx];
                        string colName = $"{replicateState.TableName}.{colInfo.Name}";
                        object colValue = colValues[colIdx];

                        if (colValue is string)
                        {
                            string val = (string)colValue;
                            //since this is a business key, we need to append copy number to make it unique
                            if (replicateState.BusinessKeys.ContainsKey(colName) && writeCount > 1)
                            {
                                //#FRAGILE Assume this is an email address
                                if (Regex.Matches(val, "@").Count == 1)
                                    val = val.Replace("@", $"{writeCount.ToString("D4")}@");
                                else
                                    val = val + writeCount.ToString("D4");
                            }
                            replicateState.Writer.Write(val);
                        }
                        else if (colValue is DateTime)
                            replicateState.Writer.Write((Int64)((DateTime)colValue).Ticks);
                        else if (colValue is Int64)
                            replicateState.Writer.Write((Int64)colValue);
                        else if (colValue is Int32)
                        {
                            //#TODO PERFORMANCE toupper is slow. DBColumn.Key is much faster.  do once add to ForeignKeyColNamesToReplicate
                            if (colInfo.Name.ToUpper() == "OID" ||
                                replicateState.ForeignKeyColNamesToReplicate.ContainsKey($"{replicateState.TableName}.{colInfo.Name}"))
                            {
                                int originalKey = (Int32)colValue;
                                int replicationKey = ((originalKey - 1) * (replicateState.NumberOfReplications + 1)) + writeCount;
                                replicateState.Writer.Write(replicationKey);
                            }
                            else
                                replicateState.Writer.Write((Int32)colValue);
                        }
                        else if (colValue is Int16)
                            replicateState.Writer.Write((Int16)colValue);
                        else if (colValue is double)
                            replicateState.Writer.Write((double)colValue);
                        else if (colValue is float)
                            replicateState.Writer.Write((float)colValue);
                        else if (colValue is decimal)
                            replicateState.Writer.Write((decimal)colValue);
                        else if (colValue is Guid)
                            replicateState.Writer.Write(((Guid)colValue).ToByteArray());
                        else if (colValue is Byte)
                            replicateState.Writer.Write((Byte)colValue);
                        else
                            throw new Exception($"backup of type {colValue.GetType().Name} not supported");
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        private void AddTablesThatReferenceThisType(Type[] persistentTypes, XPClassInfo headClassInfo, Dictionary<DBTable, Type> tableSet,
            Dictionary<string, DBForeignKey> forwardFKsToReplicate)
        {
            foreach (Type classType in persistentTypes)
            {
                XPClassInfo classInfo = Session.DefaultSession.GetClassInfo(classType);
                foreach (ReflectionPropertyInfo member in classInfo.CollectionProperties)
                {
                    if (member.IntermediateClass != null)
                    {
                        //many to many
                        foreach (var intClassMember in member.IntermediateClass.Members)
                        {
                            //intermediate class points to me
                            if (intClassMember is IntermediateObjectFieldInfo && intClassMember.MemberType == headClassInfo.ClassType)
                            {
                                tableSet[member.IntermediateClass.Table] = classType;
                            }
                        }
                    }
                }
                foreach (var member in classInfo.OwnMembers)
                {
                    string memberName = $"{classType.Name}.{member.Name}";
                    //one to many and one to one
                    if (member is ReflectionPropertyInfo && member.ReferenceType != null
                        && member.ReferenceType.ClassType == headClassInfo.ClassType)
                    {
                        tableSet[classInfo.Table] = classType;
                    }
                    if (forwardFKsToReplicate.ContainsKey(memberName))
                    {
                        //Add the Head Bus Obj table that the caller explicitly wants us to include in replication
                        DBForeignKey fk = forwardFKsToReplicate[memberName];
                        Type referencedClassType = null;
                        foreach (Type t in persistentTypes)
                        {
                            if (t.Name == fk.PrimaryKeyTable)
                            {
                                referencedClassType = t;
                                break;
                            }
                        }

                        XPClassInfo referencedClassInfo = Session.DefaultSession.GetClassInfo(referencedClassType);
                        tableSet[referencedClassInfo.Table] = referencedClassType;
                    }
                }
            }
        }


        //Prerequisite: All Oids must be reset, starting at 1,2,3, etc
        //
        //Description: Replicate bodies of Business Objects as a way to create large test datastores.
        //
        //Input: Head Business Object and 'n' number of copies and its body contents to replicate.
        //
        //Processing: 
        //Creates 'n' copies of the Head and for each table refering to the Head(transitively) creates 'n' copies of the table.
        //Assigns new Oids to the replicates of the Head and body part tables.
        //Ensures FKs within the replicated Business Object Bodies refer correctly within the new bodies.
        //
        //Extra parameters for flexibility:
        //ExcludeBodyParts - List of body part tables to not replicate(transitively). Instead, all replications point to the same table as original table.
        //ReplicateForwardReferences - List of columns (<table>.<column> format) within the body who reference another table (presumably another Head Bus Obj).  
        //  Replicate the referenced table (and its children as well).
        //
        //Parameters needed for implementation:
        //PersistentTypes - All classes for consideration
        //ConnectionString & provider - Data store connection info
        //RootDirectory - Pareent for backup directory 
        private void ReplicateBusObjBodies(
                        Type headBusinessObject, int numberToReplicate,
                        IList<string> forwardFKsToReplicate,
                        IList<string> dontReplicateTheseBodyParts,
                        Type[] persistentTypes,
                        IList<string> businessKeys,
                        string connectionString, DbProvider provider,
                        string rootDirectory)
        {
            XPClassInfo classInfo = Session.DefaultSession.GetClassInfo(headBusinessObject);
            ReflectionDictionary dictionary = new ReflectionDictionary();
            DBTable[] targetSchema = dictionary.GetDataStoreSchema(persistentTypes);


            //starting with the head, identify all child tables that refer to the head (indirectly) that need to be replicated
            Dictionary<string, int> forwardFKsToReplicateDir = new Dictionary<string, int>();
            Dictionary<string, DBForeignKey> forwardFKsToReplicateDir2 = new Dictionary<string, DBForeignKey>();
            foreach (string s in forwardFKsToReplicate) { forwardFKsToReplicateDir[s] = 1; }

            foreach (DBTable t in targetSchema)
            {
                foreach (DBForeignKey foreignKey in t.ForeignKeys)
                {
                    List<string> colNames = new List<string>();
                    foreach (string col in foreignKey.Columns) { colNames.Add(col); }
                    string colName = string.Join(".", colNames);
                    string fullColName = $"{t.Name}.{colName}";
                    if (forwardFKsToReplicateDir.ContainsKey(fullColName))
                    {
                        forwardFKsToReplicateDir2[fullColName] = foreignKey;
                    }
                }
            }
            IDictionary<string, DBTable> bodyToReplicate = FindAllTablesThatReferenceThisHead(persistentTypes, headBusinessObject, forwardFKsToReplicateDir2);

            //Now, remove the body parts that user explicitly doesn't want us to replicate
            foreach (string tableName in dontReplicateTheseBodyParts)
            {
                if (bodyToReplicate.ContainsKey(tableName))
                    bodyToReplicate.Remove(tableName);
            }

            //create a dictionary of all foreign key cols that need to be replicated
            IDictionary<string, int> foreignKeyColNamesToReplicate = new Dictionary<string, int>();
            foreach (DBTable t in targetSchema)
            {
                foreach (DBForeignKey foreignKey in t.ForeignKeys)
                {
                    List<string> colNames = new List<string>();
                    foreach (string col in foreignKey.Columns) { colNames.Add(col); }
                    string colName = string.Join(".", colNames);
                    if (bodyToReplicate.ContainsKey(foreignKey.PrimaryKeyTable))
                    {
                        foreignKeyColNamesToReplicate[$"{t.Name}.{colName}"] = 1;
                    }
                }
            }

            //Write each table to a binary file, for tables in the bodyToReplicate - replicate their rows and their FK references to their parent tables
            using (DbConnection DBConnection = NewDbConnection(connectionString, provider))
            {
                foreach (DBTable t in targetSchema)
                {
                    ((IDbConnection)DBConnection).Open();
                    if (bodyToReplicate.ContainsKey(t.Name))
                        ReplicateTableToBinaryFile(t.Name, numberToReplicate, rootDirectory,
                            foreignKeyColNamesToReplicate,
                            businessKeys, DBConnection);
                    else
                        DBConnection.BackupTableToBinaryFile(t.Name, rootDirectory);
                    ((IDbConnection)DBConnection).Close();
                }
            }
        }


        #endregion

        #region Load all tables from binary files
        private void LoadDataFromBackup(IDataLayer DL, DbProvider provider, string backupDirectoryPath, bool enableAndDisableConstraints = true)
        {
            DbConnection.LoadDataFromBackup(DL.Connection.ConnectionString, provider, backupDirectoryPath, enableAndDisableConstraints);
        }
        #endregion



    }
    #region parameter classes
    internal class ReplicateState
    {
        internal BinaryWriter Writer;
        internal string TableName { get; set; }
        internal IList<DbColumnInfo> ColumnInfos;
        internal IDictionary<string, int> BusinessKeys;
        internal int NumberOfReplications;
        internal IDictionary<string, int> ForeignKeyColNamesToReplicate;
    }
    internal class OidMapperState
    {
        internal string CurrentTableName;
        internal BinaryWriter Writer;
        internal IDictionary<string, IDictionary<int, int>> TableToOidMaps;
        internal IDictionary<string, string> ColumnNameToReferencedTable;
    }
    internal class BackupTableToFileParms
    {
        internal string TableName { get; set; }
        internal string ConnectionString { get; set; }
        internal DbProvider Provider { get; set; }
        internal string Directory { get; set; }
    }
    #endregion 
}
