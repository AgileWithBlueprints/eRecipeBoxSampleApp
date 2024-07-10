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
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Text.RegularExpressions;
using static DxWinAppDriverUtils.DxElementPrimitives;
using static eRecipeBoxSystemTests.eRecipeBoxWindowElements;
namespace eRecipeBoxSystemTests
{
    internal class eRecipeBoxTestUtils
    {
        static internal void FilterRecipeBox(DxFormElements<SRBs> srb, string filterTerm)
        {
            srb.FormElement.SendKeys(Keys.Alt + "i" + Keys.Alt);
            if (filterTerm != null)
                srb.Get(SRBs.filterTermLookUpEdit).SendKeys(filterTerm);
            srb.Get(SRBs.filterSimpleButton).Click(); Pause(.5);
        }
        static internal void ClearFilter(DxFormElements<SRBs> srb)
        {
            srb.Get(SRBs.clearFilterSimpleButton).Click(); Pause(.8);
        }

        static internal string GetRecCardTitleFromERCTitleBar(WindowsDriver<WindowsElement> session)
        {
            string pattern = @"(?<=\[EditRecipeCard: )(.*)(?=])";
            return GetRecCardTitleFromTitleBar(session, pattern);
        }
        static internal string GetRecCardTitleFromVRCTitleBar(WindowsDriver<WindowsElement> session)
        {
            string pattern = @"(?<=\[ViewRecipeCard: )(.*)(?=])";
            return GetRecCardTitleFromTitleBar(session, pattern);
        }
        static private string GetRecCardTitleFromTitleBar(WindowsDriver<WindowsElement> session, string regexPattern)
        {
            string rcTitle = null;
            foreach (WinElement el in FindElementsByXPath(session, AllChildren))
            {
                string ctype = Textify(el.GetAttribute("LocalizedControlType"));
                if (ctype == "title bar")
                {
                    string etext = Textify(el.Text);
                    WindowsDriverUtils.Log.WriteLine(etext);
                    //es XXX Household RecipeBox - [EditRecipeCard: Creamy Spinach and Shrimp Enchiladas]
                    Regex regEx = new Regex(regexPattern);
                    MatchCollection mc = regEx.Matches(etext);
                    rcTitle = mc[0].Value.Trim();
                }
            }
            if (rcTitle == null)
                throw new Exception("cant find RC title in title bar");
            return rcTitle;
        }

    }
}
