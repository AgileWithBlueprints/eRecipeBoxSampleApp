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
using Foundation;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using static DataStoreUtils.BinaryFileConstants;

namespace DataStoreUtils.DbConnectionAdapters
{
    internal class SqLiteConnection : DbConnection
    {
        internal SqLiteConnection(string connString, DbProvider dbProvider) : base(connString, dbProvider)
        {
            dbConnection = new SQLiteConnection(connString);
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
                int numCols = infile.ColumnInfos.Count;
                List<string> colVals = new List<string>(numCols);
                while (infile.Read())
                {
                    colVals.Clear();
                    for (int i = 0; i < numCols; i++)
                    {
                        if (infile.GetValue(i) == null)
                        {
                            colVals.Add("NULL");
                            continue;
                        }
                        DbColumnInfo colInfo = infile.ColumnInfos[i];
                        switch (colInfo.DatatypeID)
                        {
                            case nvarcharID:
                            case varcharID:
                            case textID:
                            case ccharID:
                            case ncharID:
                            case ntextID:
                            case xmlID:
                                colVals.Add("'" + PrimitiveUtils.SQLStringLiteral((string)infile.GetValue(i)) + "'");
                                break;
                            case datetime2ID:
                            case datetimeID:
                            case dateID:
                            case smalldatetimeID:
                            case timeID:
                            case timestampID:
                                DateTime dt = (DateTime)infile.GetValue(i);
                                colVals.Add("'" + dt.ToString("s") + "'");
                                break;
                            default:
                                {
                                    object o = infile.GetValue(i);
                                    colVals.Add(o.ToString());
                                }
                                break;
                        }
                    }
                    string insertCmd = $"insert into {SQLTableName(infile.TableName)} values (" + string.Join(",", colVals) + ");";
                    this.ExecuteNonQuery(insertCmd, false, null);
                }
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }
        }

        public override void EnableAllFKsAndRebuildIndexes()
        {
            ExecuteNonQuery(@"PRAGMA ignore_check_constraints = 0;", false, null);
        }
        public override void DisableAllFKandIndexes()
        {
            ExecuteNonQuery(@"PRAGMA ignore_check_constraints = 1;", false, null);
        }
    }
}
