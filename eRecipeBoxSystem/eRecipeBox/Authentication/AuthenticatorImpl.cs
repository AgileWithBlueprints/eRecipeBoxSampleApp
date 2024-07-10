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
using System.IO;
using System.Reflection;

namespace eRecipeBox.Authentication
{
    public partial class Authenticator
    {
        static private bool GetUserCredentials(string email, out string userID, out string pw)
        {
            EnvironmentUtils.GetCredentials(email, out userID, out pw);
            return true;
        }


        #region Cache user email on client PC
        static private void SaveEmailToCache(string email)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string licFolder = assemblyFolder + '\\' + "LicenceFiles";
            DirectoryInfo di = new DirectoryInfo(licFolder);
            if (!di.Exists)
                Directory.CreateDirectory(licFolder);
            string myID = EnvironmentUtils.GetWindowsUserName();
            string licFilePath = $"{licFolder}\\{myID}.lic";
            //store email locally in the license file
            StreamWriter licF = new StreamWriter(licFilePath);
            licF.WriteLine(Encryption.EncryptToHex(email));
            licF.Close();
        }

        static private string GetCachedEmail()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string licFolder = assemblyFolder + '\\' + "LicenceFiles";
            DirectoryInfo di = new DirectoryInfo(licFolder);
            if (!di.Exists)
                return null;
            string myID = EnvironmentUtils.GetWindowsUserName();
            string licFilePath = $"{licFolder}\\{myID}.lic";
            FileInfo fi = new FileInfo(licFilePath);
            if (!fi.Exists)
                return null;
            StreamReader licF = new StreamReader(licFilePath);
            string email = Encryption.DecryptHexString(licF.ReadLine());
            licF.Close();
            return email;
        }
        #endregion
    }
}
