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
using Castle.DynamicProxy.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using static DxWinAppDriverUtils.DxElementPrimitives;

namespace DxWinAppDriverUtils
{
    public partial class DxGridViewElement<T> where T : System.Enum
    {
        //#TRICKY #WORKAROUND  the header panel only has columns the first time it is painted, so store for all instances/grids
        static private Dictionary<string, string> columnNameMap = null;
        private string ColumnNameMapValue(string key)
        {
            if (!columnNameMap.ContainsKey(key))
                LoadColumnNameMap(this);
            if (!columnNameMap.ContainsKey(key))
                WindowsDriverUtils.DumpAll(this.Element);
            return columnNameMap[key];
        }
        static private void LoadColumnNameMap(DxGridViewElement<T> gridViewElement)
        {
            if (columnNameMap == null)
                columnNameMap = new Dictionary<string, string>();

            //#TRICKY #WORKAROUND  the header panel only has columns the first time it is painted, so store for all instances/grids
            IWebElement hdrPanel = gridViewElement.Element.FindElement(By.Name("Header Panel")); 
            foreach (IWebElement hdr in hdrPanel.FindElements(By.XPath(AllDescendants)))
            {
                string ctype = Textify(hdr.GetAttribute("LocalizedControlType"));

                //if (ctype != "header")  //#WARNING #FRAGILE DevEx V22 used header for column headers
                if (ctype != "header item") //#WARNING #FRAGILE V23.x used header item for column headers
                    continue;
                string elementName = Textify(hdr.GetAttribute("Name"));
                columnNameMap[Namify(elementName)] = elementName;
            }
        }
        private int? topRowNumberInView = null;
        internal DxGridViewElement(DxFormElements<T> parentForm, WinElement gridView)
        {
            this.ParentForm = parentForm;
            this.Element = gridView;
            IWebElement dataPanel = FindElementByName(Element, "Data Panel");
            //find first absolute row number 
            foreach (IWebElement child in dataPanel.FindElements(By.XPath(AllChildren)))
            {
                string txt = Textify(child.GetAttribute("Name"));
                if (txt.StartsWith("Row"))
                {
                    //eg Row 20
                    string[] words = txt.Split(' ');
                    if (words.Length != 2 || words[0] != "Row")
                        return;
                    int topRowNum;
                    if (int.TryParse(words[1], out topRowNum))
                        topRowNumberInView = topRowNum;
                }
            }
            LoadColumnNameMap(this);
        }
    }
}
