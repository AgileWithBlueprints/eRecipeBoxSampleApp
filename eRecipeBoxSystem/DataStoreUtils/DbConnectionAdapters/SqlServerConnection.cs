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
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace DataStoreUtils.DbConnectionAdapters
{
    internal class SqlServerConnection : DbConnection
    {
        internal SqlServerConnection(string connString, DbProvider dbProvider) : base(connString, dbProvider)
        {
            dbConnection = new SqlConnection(connString);
        }
        public override string SQLTableName(string tableName)
        {
            return $"[{tableName}]";
        }
        public override string SQLColumnName(string colName)
        {
            return $"[{colName}]";
        }



        public override void LoadBinaryFileToTable(string filePath, int batchSize = 100000)
        {
            try
            {
                BinaryTableFileReader infile = new BinaryTableFileReader(filePath, null);
                //no data, nothing to load
                if (infile.ColumnInfos.Count == 0)
                    return;
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(this.dbConnection.ConnectionString,
                    SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.KeepIdentity))
                {
                    bulkCopy.DestinationTableName = SQLTableName(infile.TableName);
                    bulkCopy.BatchSize = batchSize;
                    bulkCopy.BulkCopyTimeout = 9999;
                    bulkCopy.WriteToServer(infile);
                    bulkCopy.Close();
                }
                infile.Close();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                throw;
            }
        }

        public override void EnableAllFKsAndRebuildIndexes()
        {
            ExecuteNonQuery(@"EXEC sp_MSforeachtable 'SET QUOTED_IDENTIFIER ON; ALTER TABLE ? CHECK CONSTRAINT all'", false, null);
            ExecuteNonQuery(@"EXEC sp_msforeachtable 'SET QUOTED_IDENTIFIER ON; ALTER INDEX ALL ON ? REBUILD'", false, null);


            string enableAllIndexes =
@"DECLARE @sql AS VARCHAR(MAX)= '';

SELECT @sql = @sql +
'ALTER INDEX ' + sys.indexes.name + ' ON  [' + sys.objects.name + '] REBUILD;' + CHAR(13) + CHAR(10)
FROM
sys.indexes
JOIN
sys.objects
ON sys.indexes.object_id = sys.objects.object_id
WHERE sys.indexes.type_desc = 'NONCLUSTERED'
AND sys.objects.type_desc = 'USER_TABLE';

EXEC(@sql);";
            ExecuteNonQuery(enableAllIndexes, false, null);

        }
        public override void DisableAllFKandIndexes()
        {
            //sql server
            //https://stackoverflow.com/questions/50811835/how-to-disable-all-fk-in-sqlserver
            ExecuteNonQuery(@"EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all'", false, null);

            string disableAllIndexes =
@"DECLARE @sql AS VARCHAR(MAX)= '';

SELECT @sql = @sql +
'ALTER INDEX ' + sys.indexes.name + ' ON  [' + sys.objects.name + '] DISABLE;' + CHAR(13) + CHAR(10)
FROM
sys.indexes
JOIN
sys.objects
ON sys.indexes.object_id = sys.objects.object_id
WHERE sys.indexes.type_desc = 'NONCLUSTERED'
AND sys.objects.type_desc = 'USER_TABLE';

EXEC(@sql);";
            ExecuteNonQuery(disableAllIndexes, false, null);

        }

        public override void GenerateSchemaReport(string reportPath)
        {
            int recordCount = 0;
            StreamWriter report = new StreamWriter(reportPath);
            using (IDataReader rdr = ExecuteQueryReturnReader(reportSchemaQuerySQLserver))
            {
                while (rdr.Read())
                {
                    if (recordCount == 0)
                    {
                        //column header
                        bool isFirstH = true;
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            if (isFirstH)
                                isFirstH = false;
                            else
                                report.Write('\t');
                            report.Write(rdr.GetName(i));
                        }
                        report.WriteLine();
                    }

                    bool isFirst = true;
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        if (isFirst)
                            isFirst = false;
                        else
                            report.Write('\t');
                        report.Write(rdr[i].ToString());
                    }
                    report.WriteLine();
                    recordCount++;
                }
            }
            report.Close();
        }
    }
}
