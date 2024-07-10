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
using HtmlAgilityPack;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static DxWinAppDriverUtils.DxElementPrimitives;

namespace DxWinAppDriverUtils
{
    public class WindowsDriverUtils
    {
        //#TODO debug only
        static public bool DumpFlag = false;

        static public string GetURLFromHyperTextEdit(WinElement hyperTextEdit)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(hyperTextEdit.Text);
                HtmlNode htmlNode = htmlDoc.DocumentNode.Descendants("href=https:").ToList()[0];
                string url = htmlNode.Attributes[0].Name;
                //#WORKAROUND  sometimes strips the prefix
                if (!url.StartsWith("http"))
                    url = "https://" + url;
                return url;
            }
            catch
            {
                return null;
            }
        }
        static public void StartWinAppDriver(string exePath)
        {
            if (!IsRunning())
            {
                Process.Start(exePath);
            }
        }
        static private bool IsRunning()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.Contains("WinAppDriver"))
                    return true;
            }
            return false;
        }
        public const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";


        public static void MoveMouseTo(WindowsDriver<WindowsElement> session, WinElement element)
        {
            Actions a = new Actions(session);
            a.MoveToElement(element.WebElement);
            a.Perform(); Pause();
        }
        public static void DoubleClick(WindowsDriver<WindowsElement> session, WinElement element)
        {
            Actions action = null;
            action = new Actions(session);
            action.MoveToElement(element.WebElement);
            action.DoubleClick();
            action.Perform(); Pause();
        }

        public static void CreateLogFile(string withName)
        {
            Log = new StreamWriter(withName);
            Log.AutoFlush = true;
        }
        public static void CloseLogFile()
        {
            Log?.Close();
        }
        public static StreamWriter Log;

        public static void LoadFileToClipboard(string filePath)
        {
            string text = File.ReadAllText(filePath);
            Clipboard.SetText(text);
        }

        public static void DumpAll(WinElement root)
        {
            foreach (WinElement el in FindElementsByXPath(root, AllDescendants))
            {
                string name = Textify(el.GetAttribute("Name"));
                string className = Textify(el.GetAttribute("ClassName"));
                string ctype = Textify(el.GetAttribute("LocalizedControlType"));
                string text = Textify(el.GetAttribute("Text"));
                //string acc = Textify(el.GetAttribute("Text"));
                string etext = Textify(el.Text);
                Log.WriteLine(DateTime.Now.ToShortTimeString() +
                    $"Nm={name} ClNm={className} ClsTp={ctype} TxtAtr={text} TxtEl={etext}");
            }
        }

        public static void DumpAll(WindowsDriver<WindowsElement> session)
        {
            foreach (WinElement el in FindElementsByXPath(session, AllDescendants))
            {
                string name = Textify(el.GetAttribute("Name"));
                string className = Textify(el.GetAttribute("ClassName"));
                string ctype = Textify(el.GetAttribute("LocalizedControlType"));
                string text = Textify(el.GetAttribute("Text"));
                string etext = Textify(el.Text);
                Log.WriteLine(DateTime.Now.ToShortTimeString() +
                    $"Nm={name} ClNm={className} ClsTp={ctype} TxtAtr={text} TxtEl={etext}");
            }
        }

    }
}
