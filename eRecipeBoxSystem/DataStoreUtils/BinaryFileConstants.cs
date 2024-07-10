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
namespace DataStoreUtils
{
    public class BinaryFileConstants
    {
        public const int nvarcharID = 0;
        public const int varcharID = 1;
        public const int textID = 2;
        public const int ccharID = 3;
        public const int ncharID = 4;
        public const int ntextID = 5;
        public const int xmlID = 6;
        public const int datetimeID = 7;
        public const int datetime2ID = 8;
        public const int dateID = 9;
        public const int smalldatetimeID = 10;
        public const int timeID = 11;
        public const int timestampID = 12;
        public const int bigintID = 13;
        public const int floatID = 14;
        public const int intID = 15;
        public const int bitID = 16;
        public const int uniqueidentifierID = 17;
        public const int tinyintID = 18;
        public const int smallintID = 19;
        public const int realID = 20;
        public const int moneyID = 21;
        public const int decimalID = 22;
        public const int numericID = 23;
        public const int varbinaryID = 24;
        public const int imageID = 25;

        public static int ParseDataType(string dtname)
        {
            switch (dtname.ToLower())
            {
                case "nvarchar":
                case "string":
                    return nvarcharID;
                case "varchar":
                    return varcharID;
                case "text":
                    return textID;
                case "char":
                    return ccharID;
                case "nchar":
                    return ncharID;
                case "ntext":
                    return ntextID;
                case "xml":
                    return xmlID;
                case "datetime":
                    return datetimeID;
                case "datetime2":
                    return datetime2ID;
                case "date":
                    return dateID;
                case "smalldatetime":
                    return smalldatetimeID;
                case "time":
                    return timeID;
                case "timestamp":
                    return timestampID;
                case "bigint":
                case "int8":
                case "int64":
                    return bigintID;
                case "float":
                case "float8":
                    return floatID;
                case "int":
                case "int32":
                case "int4":
                    return intID;
                case "bit":
                case "boolean":
                    return bitID;
                case "uniqueidentifier":
                    return uniqueidentifierID;
                case "tinyint":
                    return tinyintID;
                case "smallint":
                    return smallintID;
                case "real":
                case "float4":
                    return realID;
                case "money":
                    return moneyID;
                case "decimal":
                    return decimalID;
                case "numeric":
                    return numericID;
                case "varbinary":
                    return varbinaryID;
                case "image":
                    return imageID;
                default:  //         
                    throw new Exception("unsupported dt " + dtname);
            }
        }

        //write binary so need null values for each datatype
        static public System.Data.SqlTypes.SqlDateTime minSQLDT = System.Data.SqlTypes.SqlDateTime.MinValue;
        static public DateTime nullDT = DateTime.Parse("1800-01-03");
        static public string nullString = "";
        static public Int64 nullInt64 = Int64.MinValue;
        static public Int32 nullInt32 = Int32.MinValue;
        static public Int16 nullInt16 = Int16.MinValue;
        static public Byte nullByte = Byte.MaxValue;
        static public Byte nullBool = Byte.MaxValue;  //store bools as a byte so we can support a NULL value
        static public Byte falseBool = 0;
        static public Byte trueBool = 1;
        static public decimal nullDecimal2 = Decimal.MinValue;
        static public decimal nullDecimal = Decimal.MinValue;
        static public double nullDouble = Double.MinValue;
        static public float nullFloat = Single.MinValue;
        static public byte[] nullBA = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        static public byte[] maxBA = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        static public Guid nullGUID = Guid.Empty;
        static public string nullGUIDString = nullGUID.ToString();
    }
}
