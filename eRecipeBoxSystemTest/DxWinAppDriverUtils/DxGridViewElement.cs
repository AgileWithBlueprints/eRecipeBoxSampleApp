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
using System.IO;
using System.Xml.Linq;
using static DxWinAppDriverUtils.DxElementPrimitives;

namespace DxWinAppDriverUtils
{
    public partial class DxGridViewElement<T> where T : System.Enum
    {
        #region properties
        public WinElement Element { get; private set; }
        public DxFormElements<T> ParentForm { get; private set; }
        #endregion properties

        #region methods

        public WinElement GetColumnHeader(T columnHeaderName)
        {
            //col headers
            //getColumnHeader
            //#WORKAROUND searchResultsGridView or hdrPnlEl find Descendants doesn't work.  
            string name = ColumnNameMapValue(columnHeaderName.ToString());
            foreach (WinElement hdr in DxElementPrimitives.FindElementsByName(ParentForm.FormElement, $"{name}"))
            {
                string ctype = Textify(hdr.GetAttribute("LocalizedControlType"));
                //if (ctype != "header") // #WARNING #FRAGILE DevEx V22 used header
                if (ctype != "header item") // V23.x
                    throw new Exception("not expected");
                return hdr;
            }
            return null;
        }
        public WinElement GetCell(int rowNumberOnScreen, T columnHeaderName, bool doAssert = true)
        {
            return GetCell(rowNumberOnScreen, false, columnHeaderName, doAssert);
        }
        public WinElement GetCell(int rowNumber, bool absoluteRow, T columnHeaderName, bool doAssert)
        {
            //https://docs.devexpress.com/WindowsForms/404045/build-an-application/ui-tests-ui-automation-appium-coded-ui
            //https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-controlpatternsoverview
            //https://www.automatetheplanet.com/most-complete-appium-csharp-cheat-sheet/
            //https://community.devexpress.com/blogs/winforms/archive/2022/07/04/winforms-customize-accessibility-properties.aspx
            //https://en.wikipedia.org/wiki/Comparison_of_GUI_testing_tools

            try
            {
                string name = ColumnNameMapValue(columnHeaderName.ToString());
                int absoluteRowNumber = rowNumber;
                if (absoluteRow)
                    absoluteRowNumber = (int)topRowNumberInView + rowNumber - 1;
                WinElement result = FindElementByName(Element, $"{name} row {absoluteRowNumber}", doAssert);
                return result;
            }
            catch
            {
                IWebElement dataPanel = FindElementByName(Element, "Data Panel", doAssert);
                //find first absolute row number 
                foreach (IWebElement child in dataPanel.FindElements(By.XPath(AllChildren)))
                {
                    string txt = Textify(child.GetAttribute("Name"));
                    if (txt.StartsWith("Row"))
                    {
                        //eg Row 20
                        string[] words = txt.Split(' ');
                        if (words.Length != 2 || words[0] != "Row")
                            throw;
                        int topRowNum;
                        if (int.TryParse(words[1], out topRowNum))
                        {
                            topRowNumberInView = topRowNum;
                            break;
                        }
                    }
                }
                var x = columnHeaderName.ToString();
                string name = ColumnNameMapValue(x);
                int absoluteRowNumber = rowNumber;
                if (topRowNumberInView != null)
                    absoluteRowNumber = (int)topRowNumberInView + rowNumber - 1;
                var xx = $"{name} row {absoluteRowNumber}";
                //IWebElement btn = FindElementByName(Element, $"Line Down", doAssert);
                //btn.Click();Pause(1);
                //btn = FindElementByName(Element, $"Line Up", doAssert);
                //btn.Click(); Pause(1);
                //Element.SendKeys(Keys.ArrowRight);

                WinElement cell = FindElementByName(Element, $"{name} row {absoluteRowNumber}", false);
                if (cell == null)
                {
                    WindowsDriverUtils.DumpAll(Element);
                    if (doAssert)
                        Assert.IsNotNull(cell);
                }
                return cell;

            }
        }


        //#WORKAROUND for devEx V23.2
        public void SetNumericColumnFilter(T columnName, WindowsDriver<WindowsElement> session, string minValue, string maxValue = null)
        {
            WinElement colElement = GetColumnHeader(columnName);
            //hovering mouse over column header triggest the filter button to appear
            WindowsDriverUtils.MoveMouseTo(session, colElement);
            Pause(1);

            string colHeaderName = ColumnNameMapValue(columnName.ToString());
            FindElementByName(Element, $"{colHeaderName} ColumnFilterButton").Click(); Pause(1);

            IWebElement valuesTab = FindElementByName(Element, "Values");  //Values tab in the popup
            IWebElement lowRangeEdit = null;
            IWebElement highRangeEdit = null;

            //#WORKAROUND  
            foreach (IWebElement el in valuesTab.FindElements(By.XPath(AllDescendants)))
            {
                string name = Textify(el.GetAttribute("Name"));
                string className = Textify(el.GetAttribute("ClassName"));
                string ctype = Textify(el.GetAttribute("LocalizedControlType"));

                //looking for this
                //11:48 AMNm=From ClNm=WindowsForms10.EDIT.app.0.131a902_r8_ad1 ClsTp=edit TxtAtr= TxtEl=
                if (name.ToLower().Contains("from") &&  className.ToLower().Contains(".edit.") && ctype == "edit" && lowRangeEdit == null)
                {
                    lowRangeEdit = el;
                }

                //looking for this
                //11:48 AMNm=To ClNm=WindowsForms10.EDIT.app.0.131a902_r8_ad1 ClsTp=edit TxtAtr= TxtEl=
                else if (name.ToLower().Contains("to") && className.ToLower().Contains(".edit.") && ctype == "edit" && highRangeEdit == null)
                {
                    highRangeEdit = el;
                    break;
                }               
            }
            lowRangeEdit.Click(); Pause(1);
            lowRangeEdit.SendKeys(minValue); Pause();  //Note: grid filters right away
            lowRangeEdit.SendKeys(Keys.Tab); Pause();
            if (maxValue != null)
            {
                highRangeEdit.SendKeys(maxValue); Pause();
                lowRangeEdit.SendKeys(Keys.Tab); Pause();  //close the editor
            }
            Element.SendKeys(Keys.Escape); Pause(1);  //#WORKAROUND Close button seems to clear the filter for some reason
        }

        //#WORKAROUND for devEx V22
        public void SetNumericColumnFilterV22(T columnName, WindowsDriver<WindowsElement> session, string minValue, string maxValue = null)
        {            
            WinElement colElement = GetColumnHeader(columnName);
            //hovering mouse over column header triggest the filter button to appear
            WindowsDriverUtils.MoveMouseTo(session, colElement);
            Pause(1);

            string colHeaderName =  ColumnNameMapValue(columnName.ToString());
            FindElementByName(Element, $"{colHeaderName} ColumnFilterButton").Click(); Pause(1);

            IWebElement valuesTab = FindElementByName(Element, "Values");  //Values tab in the popup
            IWebElement lowRangeEdit = null;
            IWebElement highRangeEdit = null;

            //#WORKAROUND no name for input edits, so first <number> is min and next <number> is max element
            foreach (IWebElement el in valuesTab.FindElements(By.XPath(AllDescendants)))
            {
                string name = Textify(el.GetAttribute("Name"));


                int val;
                if (Int32.TryParse(name, out val))
                {
                    if (lowRangeEdit == null)
                        lowRangeEdit = el;
                    else
                    {
                        highRangeEdit = el;
                        break;
                    }
                }
            }            
            lowRangeEdit.Click(); Pause(1);
            lowRangeEdit.SendKeys(minValue); Pause();  //Note: grid filters right away
            lowRangeEdit.SendKeys(Keys.Tab); Pause();   
            if (maxValue != null)
            {                
                highRangeEdit.SendKeys(maxValue); Pause();
                lowRangeEdit.SendKeys(Keys.Tab); Pause();  //close the editor
            }
            Element.SendKeys(Keys.Escape); Pause(1);  //#WORKAROUND Close button seems to clear the filter for some reason
        }
        
        public void SetCheckBoxColumnFilter(T columnName, WindowsDriver<WindowsElement> session, bool check)
        {
            WinElement colElement = GetColumnHeader(columnName);
            //hovering mouse over column header triggest the filter button to appear
            WindowsDriverUtils.MoveMouseTo(session, colElement);
            Pause(.5);

            //IWebElement colElement = GetColumnHeader(columnName);
            //colElement.Click(); Pause();
            //Element.SendKeys(Keys.Control + Keys.Home + Keys.Control); Pause(1.5);
            string colHeaderName = ColumnNameMapValue(columnName.ToString());
            FindElementByName(Element, $"{colHeaderName} ColumnFilterButton").Click(); Pause(1);

            IWebElement valuesTab = FindElementByName(Element, "Values");  //Values tab in the popup
            IWebElement checkBoxElement = null;
            //#WORKAROUND no name for input edits, so find first checkbox
            foreach (IWebElement el in valuesTab.FindElements(By.XPath(AllDescendants)))
            {
                string ctype = Textify(el.GetAttribute("LocalizedControlType"));
                if (ctype=="check box")
                {
                    checkBoxElement = el;
                    break;
                }
            }
            checkBoxElement.Click(); Pause(.3);
            checkBoxElement.Click(); Pause(.3);
            Element.SendKeys(Keys.Escape); Pause(.3);  //#WORKAROUND Close button seems to clear the filter for some reason
        }

        public void DeleteRows(T withColumnName, List<string> containingAnyOf)
        {

            SelectRows(withColumnName, containingAnyOf); Pause();
            Element.SendKeys(Keys.Delete); Pause(1);

            //#TRICKY #WORKAROUND if a row is partically displayed, we dont see it when paging down (dont know why).
            //so just try again after deleting the first set. #TODO report to DevExpress
            SelectRows(withColumnName, containingAnyOf); Pause();
            Element.SendKeys(Keys.Delete); Pause(1);

        }

        public void SelectRows(T columnName, List<string> containingAnyOf)
        {
            Element.SendKeys(Keys.Control + Keys.Home + Keys.Control);
            Win32API.LockControlKey(true);
            try
            {
                Dictionary<int, int> selectedRows = new Dictionary<int, int>();
                Dictionary<string, int> values = new Dictionary<string, int>();
                foreach (string val in containingAnyOf)
                    values[val] = 1;

                IWebElement pageDown = FindElementByName(Element, $"Page Down", false);

                int rowNum = 0;
                IWebElement cell;
                do
                {
                    rowNum++;
                    cell = FindElementByName(Element, $"{columnName} row {rowNum}", false);
                    if (cell == null && pageDown != null)
                    {
                        //page down and try again
                        pageDown.Click(); Pause(1);
                        pageDown = FindElementByName(Element, $"Page Down", false);
                        //we've just paged down, find the first child of the data panel, that's the top row number to continue with
                        WinElement dataPanel = FindElementByName(Element, "Data Panel", true);
                        foreach (WinElement row in FindElementsByXPath(dataPanel, AllDescendants))
                        {                            
                            string name = Textify(row.GetAttribute("Name"));
                            if (name == "Data Panel")
                                continue;
                            if (name.StartsWith("Row "))
                            {
                                rowNum = Int32.Parse(name.Replace("Row ", ""));
                                break;
                            }
                            throw new Exception("Error: 1st child of header panel expected Row <nn>");
                        }
                        cell = FindElementByName(Element, $"{columnName} row {rowNum}", false);
                        WindowsDriverUtils.Log.WriteLine($"after page down {cell.Text} {rowNum}");
                    }
                    if (cell != null)
                    {
                        string etext = Textify(cell.Text);
                        //click to select, if not already selected
                        if (values.ContainsKey(etext) && !selectedRows.ContainsKey(rowNum))
                        {
                            WindowsDriverUtils.Log.WriteLine($"select {cell.Text} {rowNum}");
                            cell.Click();
                            selectedRows[rowNum] = 1;
                        }
                    }
                } while (cell != null);
            }
            finally
            {
                Win32API.LockControlKey(false);
            } Pause(1);
        }
        #endregion methods

    }
}
