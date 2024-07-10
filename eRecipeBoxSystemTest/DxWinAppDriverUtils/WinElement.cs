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
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DxWinAppDriverUtils
{
    public class WinElement : IWebElement
    {
        static public void ClearTimings()
        {
            clickTimes.Clear();
        }
        static public TimingStats GetClickTimingStats()
        {
            TimingStats result = new TimingStats();
            result.Count = clickTimes.Count;
            long min = long.MaxValue;
            long max = 0;
            long sum = 0;
            foreach (long miliseconds in clickTimes)
            {
                sum += miliseconds;
                min = Math.Min(min, miliseconds);
                max = Math.Max(max, miliseconds);
            }
            result.Min = TimeSpan.FromMilliseconds(min);
            result.Max = TimeSpan.FromMilliseconds(max);
            result.Mean = TimeSpan.FromMilliseconds((sum + 0.0) / result.Count);
            return result;
        }
        static public TimingStats GetSendKeysTimingStats()
        {
            TimingStats result = new TimingStats();
            result.Count = sendKeysTimes.Count;
            long min = long.MaxValue;
            long max = 0;
            long sum = 0;
            foreach (long miliseconds in sendKeysTimes)
            {
                sum += miliseconds;
                min = Math.Min(min, miliseconds);
                max = Math.Max(max, miliseconds);
            }
            result.Min = TimeSpan.FromMilliseconds(min);
            result.Max = TimeSpan.FromMilliseconds(max);
            result.Mean = TimeSpan.FromMilliseconds((sum + 0.0) / result.Count);
            return result;
        }
        public IWebElement WebElement { get; private set; }
        public WinElement(IWebElement webElement)
        {
            WebElement = webElement;
        }
        public string TagName { get { return WebElement.TagName; } }

        public string Text { get { return WebElement.Text; } }

        public bool Enabled { get { return WebElement.Enabled; } }

        public bool Selected { get { return WebElement.Selected; } }

        public Point Location { get { return WebElement.Location; } }

        public Size Size { get { return WebElement.Size; } }

        public bool Displayed { get { return WebElement.Displayed; } }

        public void Clear()
        {
            WebElement.Clear();
        }

        static private List<long> clickTimes = new List<long>();
        static private List<long> sendKeysTimes = new List<long>();

        public void Click()
        {
            Stopwatch sw = Stopwatch.StartNew();
            WebElement.Click();
            clickTimes.Add(sw.ElapsedMilliseconds);
        }

        public WinElement FindElementWithNameContaining(string partialName)
        {
            foreach (WinElement el in DxElementPrimitives.FindElementsByXPath(this, DxElementPrimitives.AllDescendants))
            {                
                string etext = DxElementPrimitives.Textify(el.Text);
                if (etext.Contains(partialName))
                    return el;
            }
            return null;
        }

        public IWebElement FindElement(By by)
        {
            return WebElement.FindElement(by);
        }

        public ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            return WebElement.FindElements(by);
        }

        public string GetAttribute(string attributeName)
        {
            return WebElement.GetAttribute(attributeName);
        }

        public string GetCssValue(string propertyName)
        {
            throw new NotImplementedException();
        }

        public string GetProperty(string propertyName)
        {
            return WebElement.GetProperty(propertyName);
        }

        public void SendKeys(string text)
        {
            Stopwatch sw = Stopwatch.StartNew();
            WebElement.SendKeys(text);
            sendKeysTimes.Add(sw.ElapsedMilliseconds);
        }

        public void Submit()
        {
            WebElement.Submit();
        }
    }
    public class TimingStats
    {
        public TimeSpan Mean { get; set; }
        public TimeSpan Max { get; set; }
        public TimeSpan Min { get; set; }
        public int Count { get; set; }
    }

}
