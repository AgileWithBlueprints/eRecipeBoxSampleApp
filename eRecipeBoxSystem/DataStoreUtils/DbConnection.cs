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
using DataStoreUtils.DbConnectionAdapters;
using Foundation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using static DataStoreUtils.BinaryFileConstants;
namespace DataStoreUtils
{
    public abstract partial class DbConnection : IDbConnection
    {
        #region enums
        public enum DbProvider { SQLSERVER, SQLITE, NPGSQL }
        #endregion

        #region ctor & statics

        //#TRICKY Since we are using dbProvider as a parm to decide which subclass, this needs to be static.
        //Trying to avoid having the API client write this conditional logic each time.
        static public DbConnection NewDbConnection(string connString, DbProvider dbProvider)
        {
            DbConnection result = null;
            if (dbProvider == DbProvider.SQLSERVER)
                result = new SqlServerConnection(connString, dbProvider);
            else if (dbProvider == DbProvider.NPGSQL)
                result = new NpgSqlConnection(connString, dbProvider);
            else if (dbProvider == DbProvider.SQLITE)
                result = new SqLiteConnection(connString, dbProvider);
            else
                throw new NotImplementedException();
            return result;
        }

        //Creates its own DbConnection
        static public void BackupTableToFile(string tableName, string connString, DbProvider provider, string directory)
        {
            using (DbConnection DBConnection = NewDbConnection(connString, provider))
            {
                ((IDbConnection)DBConnection).Open();
                DBConnection.BackupTableToBinaryFile(tableName, directory);
                ((IDbConnection)DBConnection).Close();
            }
        }

        //Creates its own DbConnection
        static public void LoadDataFromBackup(string targetConnectionString, DbProvider targetDbProvider, string backupDirectoryPath, bool enableAndDisableConstraints = true)
        {
            DirectoryInfo di = new DirectoryInfo(backupDirectoryPath);
            if (di == null)
                throw new Exception("Invalid directory");
            string connStr = targetConnectionString;
            using (DbConnection Conn = NewDbConnection(connStr, targetDbProvider))
            {
                ((IDbConnection)Conn).Open();

                if (enableAndDisableConstraints)
                    Conn.DisableAllFKandIndexes();

                Log.App.Info("Before load files");
                Conn.LoadBinaryFilesToTables(di.FullName);
                Log.App.Info("After load files");

                if (enableAndDisableConstraints)
                    Conn.EnableAllFKsAndRebuildIndexes();
                Log.App.Info("After enable FKs");
                ((IDbConnection)Conn).Close();
            }
        }

        #endregion

        #region write/read tables to binary files

        //outfile path = directory\<tablename>.data
        public string BackupTableToBinaryFile(string tableName, string directory)
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
            BinaryWriter writer = new BinaryWriter(bs);
            string cmd = $"select * from {SQLTableName(tableName)};";
            //Write table info header
            writer.Write(tableName);
            int count = RunSqlSelect(cmd, WriteColumnInfos01, WriteCellToFile, null, writer);
            writer.Close();
            bs.Close();
            fs.Close();
            Log.App.Info($"Backup table {tableName} Total Rows: {count}");
            return targetFilePath;
        }

        //loading binary data (high performance) is provider specific, so this is abstract
        public abstract void LoadBinaryFileToTable(string filePath, int batchSize = 100000);

        public void LoadBinaryFilesToTables(string backupDirectory)
        {
            string[] dfiles = Directory.GetFiles(backupDirectory, "*.data");
            foreach (string dfile in dfiles)
            {
                FileInfo fi = new FileInfo(dfile);
                string tableName = fi.Name.Replace(".data", "");
                Log.App.Info($"Before load file {fi.Name}");
                LoadBinaryFileToTable(fi.FullName, 15000);
                Log.App.Info($"After load file {fi.Name}");
            }
        }

        #endregion

        #region Execute SQL 

        public int ExecuteNonQuery(string executeStatement, bool ignoreError, StreamWriter log)
        {
            int numRowsAffected = 0;
            try
            {
                IDbCommand command = this.dbConnection.CreateCommand();
                command.CommandText = executeStatement;
                command.CommandTimeout = 99999;
                numRowsAffected = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.WriteLine(ex.Message);
                numRowsAffected = -1;
                if (!ignoreError)
                    throw ex;
            }
            return numRowsAffected;
        }
        public long ExecuteQueryReturnLong(string countQuery)
        {
            try
            {
                IDbCommand command = this.dbConnection.CreateCommand();
                command.CommandText = countQuery;
                command.CommandTimeout = 99999;
                object o = command.ExecuteScalar();
                if (o is Int64)
                    return (long)o;
                else if (o is Int32)
                    return (long)(int)o;
                else
                    throw new Exception("expected scalar from command.ExecuteScalar();");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IDataReader ExecuteQueryReturnReader(string SQLStatement)
        {
            IDbCommand command = dbConnection.CreateCommand();
            command.CommandText = SQLStatement;
            command.CommandTimeout = 6000;
            IDataReader resultReader = command.ExecuteReader();
            return resultReader;
        }
        public virtual void GenerateSchemaReport(string reportPath) { }

        public virtual string DropAllTablesDDL() { return null; }
        #endregion

        #region Simple API to run any query and retrieve the info using delegates

        public delegate void ReportColumnInfos(IList<DbColumnInfo> columnInfos, object runSelectState);
        public delegate void ProcessFetchedCell(long resultNumber, DbColumnInfo columnInfo, object colValue, object runSelectState);
        public delegate void ProcessFetchedRow(long resultNumber, object[] colValues, object runSelectState);

        //Run the select statement with high performance binary query. runSelectState is passed to the callbacks.
        //1. reportColumnInfos callback at the beginning
        //2. processFetchedCell called with each value that is loaded (including nulls)
        //C# object is returned for the value. DBNull is always seen as C# null.
        public int RunSqlSelect(string sqlStatement, ReportColumnInfos reportColumnInfos, ProcessFetchedCell processFetchedCell,
            ProcessFetchedRow processFetchedRow, object runSelectState)
        {
            int resultNumber = 0;

            string sv = BinaryFileConstants.nullString;
            long i64v = BinaryFileConstants.nullInt64;
            Byte tiv = BinaryFileConstants.nullByte;
            Int32 i32v = BinaryFileConstants.nullInt32;
            Int16 i16v = BinaryFileConstants.nullInt16;
            DateTime dtv = BinaryFileConstants.nullDT;
            Byte bv = BinaryFileConstants.nullBool;
            Guid gv = BinaryFileConstants.nullGUID;
            double fv = BinaryFileConstants.nullDouble;
            float floatv = BinaryFileConstants.nullFloat;
            decimal dv = BinaryFileConstants.nullDecimal;

            IList<DbColumnInfo> cols = null;
            object[] rowValues = null;
            try
            {
                using (IDataReader rdr = ExecuteQueryReturnReader(sqlStatement))
                {
                    while (rdr.Read())
                    {
                        if (cols == null)
                        {
                            cols = DbColumnInfo.GetColInfos(rdr);
                            if (reportColumnInfos != null)
                                reportColumnInfos(cols, runSelectState);
                            rowValues = new object[cols.Count];
                        }
                        resultNumber++;
                        for (int j = 0; j < cols.Count; j++)
                        {
                            int ft = cols[j].DatatypeID;
                            switch (ft)
                            {
                                case nvarcharID:
                                case ncharID:
                                case ccharID:
                                case ntextID:
                                case textID:
                                case varcharID:
                                    sv = (rdr[j] == DBNull.Value) ? BinaryFileConstants.nullString : ((string)rdr[j]);    //DisplayCharsOnly  TODO ?
                                    rowValues[j] = sv;
                                    break;
                                case datetimeID:
                                case datetime2ID:
                                case dateID:
                                    dtv = (rdr[j] == DBNull.Value) ? BinaryFileConstants.nullDT : (DateTime)rdr[j];
                                    rowValues[j] = dtv;
                                    break;
                                case bigintID:
                                    i64v = (rdr[j] == DBNull.Value) ? BinaryFileConstants.nullInt64 : ((Int64)rdr[j]);
                                    rowValues[j] = i64v;
                                    break;
                                case floatID:
                                    fv = (rdr[j] == DBNull.Value) ? BinaryFileConstants.nullDouble : ((double)rdr[j]);
                                    rowValues[j] = fv;
                                    break;
                                case realID:
                                    floatv = (rdr[j] == DBNull.Value) ? BinaryFileConstants.nullFloat : ((float)rdr[j]);
                                    rowValues[j] = floatv;
                                    break;
                                case numericID:
                                case decimalID:
                                case moneyID:
                                    dv = (rdr[j] == DBNull.Value) ? BinaryFileConstants.nullDecimal : ((decimal)rdr[j]);
                                    rowValues[j] = dv;
                                    break;
                                case intID:
                                    i32v = (rdr[j] == DBNull.Value) ? BinaryFileConstants.nullInt32 : ((Int32)rdr[j]);
                                    rowValues[j] = i32v;
                                    break;
                                case smallintID:
                                    i16v = (rdr[j] == DBNull.Value) ? BinaryFileConstants.nullInt16 : ((Int16)rdr[j]);
                                    rowValues[j] = i16v;
                                    break;
                                case bitID:
                                    if (rdr[j] == DBNull.Value) bv = BinaryFileConstants.nullBool;
                                    else if ((bool)rdr[j]) bv = BinaryFileConstants.trueBool;
                                    else bv = BinaryFileConstants.falseBool;
                                    rowValues[j] = bv;
                                    break;
                                case uniqueidentifierID:
                                    gv = (rdr[j] == DBNull.Value) ? new Guid(BinaryFileConstants.nullBA) : (Guid)(rdr[j]);
                                    rowValues[j] = gv;
                                    break;
                                case tinyintID:
                                    tiv = (rdr[j] == DBNull.Value) ? BinaryFileConstants.nullByte : (Byte)(rdr[j]);
                                    rowValues[j] = tiv;
                                    break;
                                //case varbinary:
                                //case image:
                                //    break;
                                default:  //money bit/char? 
                                    throw new Exception("Unsupported DataType " + ft);
                            }//switch
                            if (processFetchedCell != null)
                                processFetchedCell(resultNumber, cols[j], rowValues[j], runSelectState);
                        }//for
                        if (processFetchedRow != null)
                            processFetchedRow(resultNumber, rowValues, runSelectState);
                    }//while

                    return resultNumber;
                }//rdr            
            }//try
            catch (Exception ex)
            {
                Log.App.Info($"Exception during RunSqlSelect: {ex.Message}");
                throw ex;
            }
        }
        #endregion

        #region IDbConnection
        string IDbConnection.ConnectionString
        {
            get => dbConnection.ConnectionString;
            set => dbConnection.ConnectionString = value;
        }
        int IDbConnection.ConnectionTimeout => dbConnection.ConnectionTimeout;
        string IDbConnection.Database => dbConnection.Database;
        ConnectionState IDbConnection.State => dbConnection.State;
        IDbTransaction IDbConnection.BeginTransaction() => dbConnection.BeginTransaction();
        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il) => dbConnection.BeginTransaction(il);
        void IDbConnection.ChangeDatabase(string databaseName) => dbConnection.ChangeDatabase(databaseName);
        IDbCommand IDbConnection.CreateCommand() => dbConnection.CreateCommand();
        void IDbConnection.Open() => dbConnection.Open();
        void IDbConnection.Close() => dbConnection.Close();
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.dbConnection.Dispose();
                }
                disposedValue = true;
            }
        }
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion IDbConnection
    }
}
