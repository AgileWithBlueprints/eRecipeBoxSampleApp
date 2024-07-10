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
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Text.RegularExpressions;

namespace DxWinAppDriverUtils
{
    public class DxElementPrimitives
    {
        static DxElementPrimitives()
        {
            DefaultPauseDuration = double.Parse(ConfigurationManager.AppSettings["DefaultPauseDuration"]);
        }
        static private double DefaultPauseDuration;
        static public string AllDescendants = ".//*";
        static public string AllChildren = "*/*";

        static public string Textify(object str)
        {
            if (str == null)
                return "";
            else
                return str.ToString();
        }
        static public string Namify(string str)
        {            
            string p1 = Regex.Replace(str, "[ &\r\n()!']+", "", RegexOptions.Compiled);
            string p2 = Regex.Replace(p1, "[\\/%$&.]+", "_", RegexOptions.Compiled);
            if (string.IsNullOrWhiteSpace(str))
                return null;
            return p2.Replace("__", "_");
        }

        static public string EditFmt(DateTime date) {
            return date.ToString("MM/dd/yyyy");
        }

        static public WinElement FindElementByClassName(WinElement parentElement, string className, bool doAssert = true)
        {
            try
            {
                WinElement result = new WinElement(parentElement.FindElement(By.ClassName(className)));
                if (doAssert)
                    Assert.IsNotNull(result);
                return result;
            }
            catch
            {
                if (doAssert)
                    Assert.IsNotNull(null);
                return null;
            }
        }

        static public WinElement FindElementByName(WinElement parentElement, string name, bool doAssert = true)
        {
            try
            {
                if (WindowsDriverUtils.DumpFlag)
                {
                    WindowsDriverUtils.Log.WriteLine($"looking for {name}");
                    WindowsDriverUtils.DumpAll(parentElement);
                }
                WinElement result = new WinElement(parentElement.FindElement(By.Name(name)));
                if (doAssert)
                    Assert.IsNotNull(result);
                return result;
            }
            catch
            {
                if (doAssert)
                    Assert.IsNotNull(null);
                return null;
            }
        }

        static public IList<WinElement> FindElementsByXPath(WindowsDriver<WindowsElement> session, string xPath)
        {
            List<WinElement> result = new List<WinElement>();
            foreach (var el in session.FindElements(By.XPath(xPath)))
            {
                result.Add(new WinElement(el));
            }
            return result;
        }
        static public IList<WinElement> FindElementsByName(WindowsDriver<WindowsElement> session, string name)
        {
            List<WinElement> result = new List<WinElement>();
            foreach (var el in session.FindElements(By.Name(name)))
            {
                result.Add(new WinElement(el));
            }
            return result;
        }
        static public WinElement FindElementByName(WindowsDriver<WindowsElement> session, string name, bool doAssert = true)
        {
            try
            {
                WinElement result = new WinElement(session.FindElementByName(name));
                if (doAssert)
                    Assert.IsNotNull(result);
                return result;
            }
            catch
            {
                if (doAssert)
                    Assert.IsNotNull(null);
                return null;
            } 
        }
        static public IList<WinElement> FindElementsByXPath(WinElement parentElement, string xPath)
        {
            List<WinElement> result = new List<WinElement>();
            foreach (var el in parentElement.FindElements(By.XPath(xPath)))
            {
                result.Add(new WinElement(el));
            }
            return result;
        }
        static public IList<WinElement> FindElementsByName(WinElement parentElement, string name)
        {
            List<WinElement> result = new List<WinElement>();
            foreach (var el in parentElement.FindElements(By.Name(name)))
            {
                result.Add(new WinElement(el));
            }
            return result;
        }
        static public void Pause(double? duration = null) {
            double pauseDuration = DxElementPrimitives.DefaultPauseDuration;
            if (duration!=null)
                pauseDuration = duration.Value;
            if (pauseDuration == 0.0)
                return;
            else
                Thread.Sleep(TimeSpan.FromSeconds(pauseDuration)); 
        }


    }
}
