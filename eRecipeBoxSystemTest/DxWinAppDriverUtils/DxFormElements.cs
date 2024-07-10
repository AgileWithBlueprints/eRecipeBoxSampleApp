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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using static DxWinAppDriverUtils.DxElementPrimitives;
namespace DxWinAppDriverUtils
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">list of all element names on a form, including elements in ribbon</typeparam>
    public class DxFormElements<T> where T : System.Enum
    {
        public DxWinAppDriverSession ParentSession { get; private set; }
        public WinElement FormElement { get; private set; }
        public WinElement RibbonElement { get; private set; }
        static protected string ribbonName = "The Ribbon";
        public DxFormElements(DxWinAppDriverSession session)
        {
            this.ParentSession = session;
            this.FormElement = null;
            string formName = Enum.GetName(typeof(T), 0);
            try
            {
                this.FormElement = new WinElement(ParentSession.FindElementByName(formName));
            }
            catch (Exception ex)
            {
                //WindowsDriverUtils.Log.WriteLine($"DxFormElements ctor catching for messagebox special case {ex.Message}");
                //#WORKAROUND MessageBoxes names are their Captions, which need to be Namified.
                foreach (WinElement child in FindElementsByXPath(this.ParentSession.WinAppDrvrSession,AllDescendants))
                {
                    string childName = Namify(Textify(child.GetAttribute("Name")));
                    if (formName == childName)
                    {
                        this.FormElement = child;
                        break;
                    }
                }
            }
            if (this.FormElement == null)
                throw new Exception("Form not found");

            try
            {
                RibbonElement = new WinElement(ParentSession.FindElementByName(ribbonName));
            }
            catch
            {
                //not every form has a Ribbon.
                RibbonElement = null;
            }
        }
        public DxGridViewElement<T> FindGridViewElement(T gridViewName)
        {
            return new DxGridViewElement<T>(this, Get(gridViewName));
        }
        public DxSchedularElement<T> FindSchedularElement(T schedulerControlName, T dateNavigatorName)
        {
            return new DxSchedularElement<T>(Get(schedulerControlName), Get(dateNavigatorName), this);
        }
        public DxTabControlElement<T> FindTabElement(T tabControlName)
        {            
            return new DxTabControlElement<T>(Get(tabControlName), this);
        }
        private WinElement FindElementByName(string name, bool doAssert = true)
        {
            try
            {
                WinElement result = DxElementPrimitives.FindElementByName(FormElement, name);
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

        public WinElement Get(T element)
        {
            WinElement result;
            string elementName = Enum.GetName(typeof(T), element);
            try
            {
                //try the form
                result = FindElementByName(elementName, true);
                return result;
            }
            catch
            {
            }
            try
            {
                //try the ribbon
                result = DxElementPrimitives.FindElementByName(RibbonElement, elementName);
                return result;
            }
            catch
            {
            }

            //#WORKAROUND MessageBox button names need to be Namified.
            foreach (WinElement child in FindElementsByXPath( this.ParentSession.WinAppDrvrSession, AllDescendants))
            {
                string childName = Namify(Textify(child.GetAttribute("Name")));
                WindowsDriverUtils.Log.WriteLine(childName);
                if (elementName == childName)
                {
                    result = child;
                    return result;
                }
            }            
            throw new Exception($"{elementName} not found");
        }
    }

}
