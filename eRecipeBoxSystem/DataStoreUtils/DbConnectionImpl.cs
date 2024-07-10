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
using System.IO;

namespace DataStoreUtils
{
    partial class DbConnection
    {

        #region ctor        
        internal DbConnection(string connString, DbProvider dbProvider)
        {
            this.myProvider = (DbProvider)dbProvider;
        }
        #endregion ctor

        #region fields
        protected IDbConnection dbConnection { get; set; }
        protected DbProvider myProvider;
        protected bool disposedValue;
        protected const string reportSchemaQuerySQLserver = @"
SELECT t.name AS table_name,
--SCHEMA_NAME(schema_id) AS schema_name,
c.column_id as colSeq,
c.name AS column_name,
--c.system_type_id,
typ.name as cType,
c.max_length,
c.is_nullable,
RefTableName,
RefTableColumn
FROM sys.tables AS t
INNER JOIN sys.columns c ON t.OBJECT_ID = c.OBJECT_ID
join sys.types typ on typ.user_type_id=c.user_type_id
 
left outer join
(
 select ForeignKeys.*,refTable.name as RefTableName,refCol.name  as RefTableColumn from (
    select  sys.foreign_keys.[name] as ForeignKeyName
     ,schema_name(sys.objects.schema_id) as ForeignTableSchema
     ,sys.objects.[name] as ForeignTableName
     ,sys.columns.[name]  as ForeignTableColumn
     ,sys.foreign_keys.referenced_object_id as referenced_object_id
     ,sys.foreign_key_columns.referenced_column_id as referenced_column_id
     from sys.foreign_keys
      inner join sys.foreign_key_columns
        on (sys.foreign_key_columns.constraint_object_id
          = sys.foreign_keys.[object_id])
      inner join sys.objects
        on (sys.objects.[object_id]
          = sys.foreign_keys.parent_object_id)
        inner join sys.columns
          on (sys.columns.[object_id]
            = sys.objects.[object_id])
           and (sys.columns.column_id
            = sys.foreign_key_columns.parent_column_id)
    ) ForeignKeys
    join sys.tables refTable on refTable.object_id=ForeignKeys.referenced_object_id
    join sys.columns refCol on refCol.object_id=ForeignKeys.referenced_object_id and refCol.column_id=ForeignKeys.referenced_column_id
    --order by ForeignKeys.ForeignTableName,ForeignKeys.ForeignTableColumn
    )FKeys on t.name=ForeignTableName and c.name=ForeignTableColumn
    order by table_name,colSeq
";
        #endregion

        #region methods

        abstract public string SQLTableName(string tableName);
        abstract public string SQLColumnName(string colName);


        abstract public void EnableAllFKsAndRebuildIndexes();

        abstract public void DisableAllFKandIndexes();
        #endregion

        #region write/read tables to binary files
        private void WriteColumnInfos01(IList<DbColumnInfo> columnInfos, object runQueryState)
        {
            BinaryWriter writer = (BinaryWriter)runQueryState;
            writer.Write((int)columnInfos.Count);
            foreach (DbColumnInfo dbColumnInfo in columnInfos)
            {
                writer.Write(dbColumnInfo.Name);
                writer.Write(dbColumnInfo.DatatypeID);
                writer.Write(dbColumnInfo.DatatypeName);
                writer.Write(dbColumnInfo.MaxLength);
                writer.Write(dbColumnInfo.IsNullable);
            }
        }
        private void WriteCellToFile(long resultNumber, DbColumnInfo columnInfo, object colValue, object runQueryState)
        {
            BinaryWriter writer = (BinaryWriter)runQueryState;
            try
            {
                if (colValue is string)
                    writer.Write((string)colValue);
                else if (colValue is DateTime)
                    writer.Write((Int64)((DateTime)colValue).Ticks);
                else if (colValue is Int64)
                    writer.Write((Int64)colValue);
                else if (colValue is Int32)
                    writer.Write((Int32)colValue);
                else if (colValue is Int16)
                    writer.Write((Int16)colValue);
                else if (colValue is double)
                    writer.Write((double)colValue);
                else if (colValue is float)
                    writer.Write((float)colValue);
                else if (colValue is decimal)
                    writer.Write((decimal)colValue);
                else if (colValue is Guid)
                    writer.Write(((Guid)colValue).ToByteArray());
                else if (colValue is Byte)
                    writer.Write((Byte)colValue);
                else
                    throw new Exception($"backup of type {colValue.GetType().Name} not supported");
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
