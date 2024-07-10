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
using System.Collections.Generic;
using System.Data;

namespace DataStoreUtils
{
    public class DbColumnInfo
    {
        #region statics
        public static IList<DbColumnInfo> GetColInfos(IDataReader rdr)
        {
            List<DbColumnInfo> results = new List<DbColumnInfo>();
            for (int i = 0; i < rdr.FieldCount; i++)
            {
                DbColumnInfo fieldMetaData = new DbColumnInfo();
                Type dotNetType = rdr.GetFieldType(i);
                fieldMetaData.TableName = null;
                fieldMetaData.Name = rdr.GetName(i);
                fieldMetaData.DatatypeName = dotNetType.Name;
                results.Add(fieldMetaData);
            }
            return results;

            //alternative way to get this info
            ////https://learn.microsoft.com/en-us/dotnet/api/system.data.odbc.odbcdatareader.getschematable?view=dotnet-plat-ext-8.0
            ////http://aspalliance.com/631_Retrieving_Database_Schema_Information_using_the_OleDbSchemaGuid_Class.3
            ////https://csharp.hotexamples.com/examples/Npgsql/NpgsqlDataReader/GetSchemaTable/php-npgsqldatareader-getschematable-method-examples.html
            //var command = DBConnection.CreateCommand()
            //string queryTable = $"select * from {DBConnection.GetTableName(TableName)}";
            //command.CommandText = queryTable;
            //SqlDataReader reader = command.ExecuteReader();
            //using (var schemaTable = reader.GetSchemaTable())
            //{
            //    foreach (DataRow row in schemaTable.Rows)
            //    {
            //        string ColumnName = row.Field<string>("ColumnName");
            //        string DataTypeName = row.Field<string>("DataTypeName");
            //        short NumericPrecision = row.Field<short>("NumericPrecision");
            //        short NumericScale = row.Field<short>("NumericScale");
            //        int ColumnSize = row.Field<int>("ColumnSize");
            //        Console.WriteLine("Column: {0} Type: {1} Precision: {2} Scale: {3} ColumnSize {4}",
            //        ColumnName, DataTypeName, NumericPrecision, NumericScale, ColumnSize);
            //    }
            //}

        }
        #endregion


        #region ctor
        public DbColumnInfo()
        {
            DatatypeID = -1;
            MaxLength = -1;
        }
        #endregion
        #region properties

        public int DatatypeID { get; set; }
        public string _datatypeName;
        public string DatatypeName
        {
            get { return _datatypeName; }
            set { _datatypeName = value; DatatypeID = BinaryFileConstants.ParseDataType(_datatypeName); }
        }

        public string TableName { get; set; }

        public string Name { get; set; }
        public int MaxLength { get; set; }
        public bool IsNullable { get; set; }
        #endregion

    }
}
