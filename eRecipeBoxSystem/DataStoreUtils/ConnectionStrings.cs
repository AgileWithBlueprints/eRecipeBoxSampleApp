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
using DevExpress.Xpo.DB;
using Foundation;
using System;
using System.Configuration;
using System.Text.RegularExpressions;

namespace DataStoreUtils
{
    public class ConnectionStrings
    {
        static public string GetConnectionString(string connectionName)
        {
            if (connectionName == null)
                throw new ArgumentException($"Connection not found: {connectionName}");
            var connectionStringSetting = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionStringSetting == null)
                throw new ArgumentException($"Connection not found: {connectionName}");
            return connectionStringSetting.ConnectionString;
        }

        public static string GetKeyValue(string key, string fromConnectionString)
        {
            string keypart = key + "=";
            string pattern = key + "\\=.*" + "(;|$)";
            Match match = Regex.Match(fromConnectionString, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;
            string keyval = fromConnectionString.Substring(match.Index + keypart.Length);
            int semi = keyval.IndexOf(";");
            if (semi >= 0)
                keyval = keyval.Substring(0, semi);
            return keyval;
        }
        public static string GetSQLServerConnectionString(string connectionStringsConfigEntry)
        {
            string connStr = GetConnectionString(connectionStringsConfigEntry);
            string ds = GetKeyValue("Data Source", connStr);
            if (ds == null)
                ds = GetKeyValue("Server", connStr);
            string user = GetKeyValue("User Id", connStr);
            string db = GetKeyValue("initial catalog", connStr);
            string password = Encryption.DecryptHexString(GetKeyValue("password", connStr));
            return $"Server={ds};initial catalog={db};user={user};password={password}";
        }
        public static string GetXPOConnectionString(string connectionStringsConfigEntry)
        {
            string connStr = GetConnectionString(connectionStringsConfigEntry);
            string provider = GetKeyValue("XpoProvider", connStr);
            if (provider == "MSSqlServer")
            {
                string ds = GetKeyValue("Data Source", connStr);
                if (ds == null)
                    ds = GetKeyValue("Server", connStr);
                string user = GetKeyValue("User Id", connStr);
                string db = GetKeyValue("initial catalog", connStr);
                var pw = GetKeyValue("password", connStr);
                if (pw == null)
                {
                    return MSSqlConnectionProvider.GetConnectionString(ds, db);
                }
                else
                {
                    string password = Encryption.DecryptHexString(pw);
                    return MSSqlConnectionProvider.GetConnectionString(ds, user, password, db);
                }
            }
            else if (provider.ToLower() == "sqlite")
                return connStr;
            else if (provider.ToLower() == "postgres")
            {
                string encrPassword = GetKeyValue("Password", connStr);
                string password = Encryption.DecryptHexString(encrPassword);
                return connStr.Replace(encrPassword, password);
            }
            else
                throw new NotImplementedException($"{provider} not yet implemented");
        }

    }
}
