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
using static DataStoreUtils.BinaryFileConstants;

namespace DataStoreUtils
{
    /// <summary>
    /// Simple FileReader that loads binary values from a flat file. Minimally implemented to support DataStoreArchiver 
    /// </summary>
    public partial class BinaryTableFileReader : IDataReader
    {
        #region Properties

        public int RecordsRead { get; set; }
        public string TableName { get; set; }

        protected List<DbColumnInfo> fColumnInfos;
        public List<DbColumnInfo> ColumnInfos => fColumnInfos;

        #endregion

        #region ctor
        public BinaryTableFileReader(string filePath, StreamWriter log)
        {
            fColumnInfos = new List<DbColumnInfo>();
            RecordsRead = 0;
            this.log = log;
            fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            bs = new BufferedStream(fs, 16777215);
            reader = new BinaryReader(bs);
            TableName = reader.ReadString();
            if (reader.BaseStream.Position >= reader.BaseStream.Length)
            {
                vals = new object[0];
                return;
            }

            int numberOfColumns = reader.ReadInt32();
            for (int i = 0; i < numberOfColumns; i++)
            {
                DbColumnInfo col = new DbColumnInfo();
                col.TableName = TableName;
                col.Name = reader.ReadString();
                col.DatatypeID = reader.ReadInt32();
                col.DatatypeName = reader.ReadString();
                col.MaxLength = reader.ReadInt32();
                col.IsNullable = reader.ReadBoolean();
                fColumnInfos.Add(col);
            }
            vals = new object[this.ColumnInfos.Count];
        }
        #endregion

        #region IDataReader
        public int Depth { get { return 1; } }
        public bool IsClosed { get { return isClosed; } }
        public int RecordsAffected { get { return RecordsRead; } }
        public void Close()
        {
            reader.Close();
            bs.Close();
            fs.Close();
            isClosed = true;
        }
        public DataTable GetSchemaTable() { throw new NotImplementedException("GetSchemaTable"); }
        public bool NextResult() { throw new NotImplementedException("NextResult"); }
        public bool Read()
        {
            try
            {
                if (reader.BaseStream.Position >= reader.BaseStream.Length)
                    return false;
                for (int i = 0; i < ColumnInfos.Count; i++)
                {
                    switch (ColumnInfos[i].DatatypeID)
                    {
                        case nvarcharID:
                        case ncharID:
                        case ccharID:
                        case ntextID:
                        case textID:
                        case varcharID:
                            vals[i] = reader.ReadString(); if ((string)vals[i] == nullString) vals[i] = null;
                            if (vals[i] != null && ColumnInfos[i].DatatypeName != "text" && ColumnInfos[i].MaxLength > 0 && ((string)vals[i]).Length > ColumnInfos[i].MaxLength)
                                throw new Exception("string too long " + i + " " + ((string)vals[i]).Length.ToString());
                            break;
                        case datetimeID:
                        case datetime2ID:
                        case dateID:
                            vals[i] = new DateTime(reader.ReadInt64()); if ((DateTime)vals[i] == nullDT) vals[i] = null;
                            break;
                        case bigintID:
                            vals[i] = reader.ReadInt64(); if ((Int64)vals[i] == nullInt64) vals[i] = null;
                            break;
                        case floatID:
                            vals[i] = reader.ReadDouble(); if ((double)vals[i] == nullDouble) vals[i] = null;
                            break;
                        case realID:
                            vals[i] = reader.ReadSingle(); if ((float)vals[i] == nullFloat) vals[i] = null;
                            break;
                        case numericID:
                        case decimalID:
                        case moneyID:
                            vals[i] = reader.ReadDecimal(); if ((decimal)vals[i] == nullDecimal) vals[i] = null;
                            break;
                        case intID:
                            vals[i] = reader.ReadInt32(); if ((Int32)vals[i] == nullInt32) vals[i] = null;
                            break;
                        case bitID:
                            vals[i] = reader.ReadByte();
                            if ((Byte)vals[i] == nullBool) vals[i] = null; else if ((Byte)vals[i] == falseBool) vals[i] = false; else vals[i] = true;
                            break;
                        case uniqueidentifierID:
                            vals[i] = new Guid(reader.ReadBytes(16)); if (vals[i] == nullBA) vals[i] = null;
                            break;
                        case tinyintID:
                            vals[i] = reader.ReadByte(); if ((Byte)vals[i] == nullByte) vals[i] = null;
                            break;
                        case smallintID:
                            vals[i] = reader.ReadInt16(); if ((Int16)vals[i] == nullInt16) vals[i] = null;
                            break;
                        //case "varbinary":
                        //case "image":
                        //    //log.WriteLine("skipping varbinary column");
                        //    break;
                        default:  //money bit/char? int16  decimal double/float
                            throw new Exception("unsupported dt " + ColumnInfos[i].DatatypeName.ToLower());
                    }
                }
                RecordsRead++;
                if (log != null && RecordsRead % 100000 == 0)
                    log.WriteLine(DateTime.Now.ToLongTimeString() + " " + RecordsRead);
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.WriteLine("filereader read " + ex.Message);
            }
            return true;
        }
        //IDataRecord
        public int FieldCount { get { return vals.Length; } }
        public object this[int i] { get { return vals[i]; } }
        public object this[string name] { get { throw new NotImplementedException("name indexer"); } }
        public bool GetBoolean(int i) { return (bool)vals[i]; }
        public byte GetByte(int i) { return (byte)vals[i]; }
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) { throw new NotImplementedException("GetBytes"); }
        public char GetChar(int i) { return (char)vals[i]; }
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) { throw new NotImplementedException("GetChars"); }
        public IDataReader GetData(int i) { throw new NotImplementedException("GetData"); }
        public string GetDataTypeName(int i) { throw new NotImplementedException("GetDataTypeName"); }
        public DateTime GetDateTime(int i) { return (DateTime)vals[i]; }
        public decimal GetDecimal(int i) { return (decimal)vals[i]; }
        public double GetDouble(int i) { return (double)vals[i]; }
        public Type GetFieldType(int i) { return null; }
        public float GetFloat(int i) { return (float)vals[i]; }
        public Guid GetGuid(int i) { return (Guid)vals[i]; }
        public short GetInt16(int i) { return (Int16)vals[i]; }
        public int GetInt32(int i) { return (Int32)vals[i]; }
        public long GetInt64(int i) { return (Int64)vals[i]; }
        public string GetName(int i)
        {
            throw new Exception("GetName not implemented");
        }
        public int GetOrdinal(string name)
        {
            throw new Exception("GetOrdinal not implemented");
        }
        public string GetString(int i) { return (string)vals[i]; }
        public object GetValue(int i)
        {
            return vals[i];
        }
        public int GetValues(object[] values) { return -1; }
        public bool IsDBNull(int i)
        {
            return vals[i] == null;
        }
        //disposable
        public void Dispose()
        {
            reader.Dispose();
            bs.Dispose();
            fs.Dispose();
        }

        #endregion
    }
}
