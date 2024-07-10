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
using static DxWinAppDriverUtils.DxElementPrimitives;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DxWinAppDriverUtils
{
    public class DxTabControlElement<T> where T : System.Enum
    {
        public DxFormElements<T> ParentForm { get; private set; }
        public WinElement GetTabPage(T tabPageName)
        {
            foreach (WinElement el in FindElementsByXPath(tabControl, AllDescendants))
            {
                string ctype = Textify(el.GetAttribute("LocalizedControlType"));
                if (ctype != "tab item")
                    continue;
                if (Namify(el.Text) == $"{tabPageName}")
                    return el;
            }
            throw new Exception("Not found");
        }

        internal DxTabControlElement(WinElement tabControl, DxFormElements<T> parentForm)
        {
            this.tabControl = tabControl;
            this.ParentForm = parentForm;
        }
        private WinElement tabControl;
    }
}
