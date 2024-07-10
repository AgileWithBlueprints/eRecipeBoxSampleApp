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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Appium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace DxWinAppDriverUtils
{

    //helpful documentation
    //https://plainswheeler.com/2019/08/23/writing-a-slightly-more-complicated-test-using-winappdriver/

    public partial class DxWinAppDriverSession : IDisposable
    {
        #region properties
        public WindowsDriver<WindowsElement> WinAppDrvrSession { get; protected set; }
        public string TestDataDirectory { get; private set; }
        #endregion properties

        #region methods
        virtual public void StartSession(string winAppDriverExePath,string devExWinformAppExePath)
        {
            if (WinAppDrvrSession == null)
            {
                AssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                TestDataDirectory = AssemblyFolder + "\\TestData";
                string logFilesDir = AssemblyFolder + "\\LogFiles";
                DirectoryInfo di = new DirectoryInfo(logFilesDir);
                if (!di.Exists)
                    Directory.CreateDirectory(logFilesDir);
                string nowStr = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                //https://gist.github.com/rstackhouse/1162357/930f9c124b9ba05c819474da26551a5aadf35aec

                string logPath = $"{logFilesDir}\\TestRunLog{nowStr}.log";
                WindowsDriverUtils.CreateLogFile(logPath);

                if (winAppDriverExePath == null)
                    throw new Exception("WinAppDriverExePath required");
                if (devExWinformAppExePath == null)
                    throw new Exception("DevExWinformAppExePath required");

                WinAppDriverExePath = winAppDriverExePath;
                DevExWinformAppExePath = devExWinformAppExePath;


                //lazy start WindowsApplicationDriver
                WindowsDriverUtils.StartWinAppDriver(WinAppDriverExePath);

                FileInfo fi = new FileInfo(DevExWinformAppExePath);
                string path = fi.FullName;

                var appCapabilities = new AppiumOptions();
                appCapabilities.AddAdditionalCapability("app", path);
                appCapabilities.AddAdditionalCapability("deviceName", "WindowsPC");
                appCapabilities.AddAdditionalCapability("platformName", "Windows");
                WinAppDrvrSession = new WindowsDriver<WindowsElement>(new Uri(WindowsDriverUtils.WindowsApplicationDriverUrl), appCapabilities, TimeSpan.FromSeconds(180));
                Assert.IsNotNull(WinAppDrvrSession);
                Assert.IsNotNull(WinAppDrvrSession.SessionId);
                //double check
                //var handles = WinAppDrvrSession.WindowHandles;
                //This is not true in Windows 11
                //Assert.AreEqual(WinAppDrvrSession.CurrentWindowHandle, handles[0]);
                //#IMPORTANT guaranteeing the screen is alway the same size (on same monitor)
                WinAppDrvrSession.Manage().Window.Maximize();
            }
        }
        virtual public void CloseSession()
        {
            // Close the application and delete the session
            if (WinAppDrvrSession != null)
            {
                WinAppDrvrSession.Close();
                WinAppDrvrSession.Quit();
                WinAppDrvrSession = null;
            }

            TimingStats stats = WinElement.GetClickTimingStats();
            WindowsDriverUtils.Log.WriteLine("Response Time Stats (secs)");
            WindowsDriverUtils.Log.WriteLine($"Min: {stats.Min.ToString()} Max: {stats.Max.ToString()} Mean: {stats.Mean.ToString()}");
        }        
        public void Dispose()
        {
            WindowsDriverUtils.CloseLogFile();
        }
        #endregion  methods
    }
}
