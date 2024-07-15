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
using DxWinAppDriverUtils;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace eRecipeBoxSystemTests
{
    public class eRecipeBoxSession : DxWinAppDriverSession
    {

        static public void StartTestSession()
        {
            if (TestSession == null)
            {
                TestSession = new eRecipeBoxSession();
                string winAppDriverExePath = ConfigurationManager.AppSettings["WinAppDriverExePath"];

                TestSession.StartSession(winAppDriverExePath, eRecipeBoxExePath);
            }
        }
        static public void StopTestSession()
        {
            if (TestSession.WinAppDrvrSession != null)
            {
                TestSession.CloseSession();
            }
        }

        static private string eRecipeBoxExePath;
        static public string eRecipeBoxLogFolder { get; set; }
        static public eRecipeBoxSession TestSession { get; set; }
        private eRecipeBoxSession() : base()
        {
            string eRecipeBoxBinFolder = ConfigurationManager.AppSettings["eRecipeBoxBinFolder"];
            eRecipeBoxLogFolder = eRecipeBoxBinFolder + @"\LogFiles";

            //#TODO we shouldnt need to copy these.  remove them and test
            eRecipeBoxExePath = eRecipeBoxBinFolder + @"\eRecipeBox.exe";
            string gmailOAuthCredentialsJsonPath = eRecipeBoxBinFolder + @"\gCredentials.json";
            string gmailOAuthtokenjsonFolder = eRecipeBoxBinFolder + @"\token.json";


            //#IMPORTANT gmail's credentials file needs to be in the assembly folder in order to us its oauth service to email
            //the grocery list
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            FileInfo credsFile = new FileInfo(gmailOAuthCredentialsJsonPath);
            if (!credsFile.Exists)
            {
                throw new Exception($"{gmailOAuthCredentialsJsonPath} file not found.  Required to run test.  Run eRecipeBox in dev and email a grocery list to create a creds file.");
            }
            File.Copy(gmailOAuthCredentialsJsonPath, assemblyFolder + @"\gCredentials.json", true);
            DirectoryInfo di = new DirectoryInfo(assemblyFolder + @"\token.json");
            if (!di.Exists)
                Directory.CreateDirectory(assemblyFolder + @"\token.json");
            foreach (string filePath in Directory.GetFiles(gmailOAuthtokenjsonFolder, "*.*"))
            {
                File.Copy(filePath, filePath.Replace(gmailOAuthtokenjsonFolder, di.FullName), true);
            }

        }




        //helpful
        //https://plainswheeler.com/2019/08/23/writing-a-slightly-more-complicated-test-using-winappdriver/

    }
}
