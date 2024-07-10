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
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static DxWinAppDriverUtils.DxElementPrimitives;
namespace DxWinAppDriverUtils
{
    public partial class DxSchedularElement<T> where T : System.Enum
    {
        #region properties
        public WinElement Element { get; private set; }
        public DxFormElements<T> ParentForm { get; private set; }
        public WinElement DateNavigator { get; private set; }
        #endregion properties

        #region methods
        public CalendarState SetFocusToCalendar()
        {
            WinElement calendarElement = GetCalendarElement();
            //#TRICKY set focus to the entire calendar
            calendarElement.Click();
            //#TRICKY one more click to select the current day in the calendar
            calendarElement.Click();
            return GetCalendarState(calendarElement);
        }
        public void ScrollFirstDayOfCalendar(DateTime toStartDate, bool dumpAll = false)
        {
            DateTime s = DateTime.MinValue;
            DateTime e = DateTime.MinValue;

            if (dumpAll)
                WindowsDriverUtils.DumpAll(Element);

            if (toStartDate.DayOfWeek != DayOfWeek.Sunday)
                throw new Exception("Start Data must be Sunday");

            WinElement calendarElement = GetCalendarElement();
            CalendarState currentCalendar = GetCalendarState(calendarElement);
            var x = currentCalendar.SelectedDate;

            WinElement btnPrev = null;
            WinElement btnNext = null;
            try
            {
                btnPrev = GetCalendarPreviousButton();
                btnNext = GetCalendarNextButton();
            }
            catch
            {
                //#WORKAROUND for some reason buttons arent there when they should be 
                SetFocusToCalendar();
                btnPrev = FindElementByName(this.ParentForm.FormElement, nameof(btnPrev));
                btnNext = FindElementByName(this.ParentForm.FormElement, nameof(btnNext));
            }


            currentCalendar = GetCalendarState(calendarElement);
            int direction = 1;
            if ((toStartDate - currentCalendar.Start).TotalDays < 0)
                direction = -1;
            while (!currentCalendar.Contains(toStartDate))
            {
                if (direction < 0)
                    btnPrev.Click();
                else
                    btnNext.Click();

                currentCalendar = SetFocusToCalendar();
            }
        }
        public void SelectDateOnCalendar(DateTime date)
        {
            //need to click on it to get the focus to the calendar
            CalendarState currentCalendar = SetFocusToCalendar();
            DateTime position = currentCalendar.SelectedDate;

            int direction = 1;
            if ((date - position).TotalDays < 0)
                direction = -1;


            while (position != date)
            {
                if (direction == 1)
                    Element.SendKeys(Keys.ArrowRight);
                else
                    Element.SendKeys(Keys.Left);
                position = position.AddDays(direction);
            }
        }
        //#WARNING  this only scrolls that week to the top of the calender. doesn't select the date in the calender.
        //Use SelectDateOnCalendar to actually select the date in the calender
        public void ClickDateOnDateNavigator(DateTime date)
        {
            if (dateElements == null)
                LoadDateElements();

            string dateMonthName = date.Month.ToString();
            foreach (List<NavDateElement> month in dateElements)
            {
                for (int i = 0; i < month.Count; i++)
                {
                    NavDateElement d = month[i];                    
                    if (d.panelMonthName == dateMonthName && d.dateInt == date.Day)
                    {
                        d.el.Click();
                        break;
                    }
                }
            }
        }
        public IList<Appointment> GetAllVisibleAppointments()
        {
            List<Appointment> result = new List<Appointment>();
            if (appointmentsByFullName == null)
                LoadAppointmentCaches();
            foreach(var x in appointmentsByFullName)
            {
                result.Add(x.Value);
            }
            return result;
        }
        public IList<Appointment> GetAllAppointmentsOnDate(DateTime date)
        {
            if (appointmentsByDateThenSubjectName == null)
                LoadAppointmentCaches();
            if (!appointmentsByDateThenSubjectName.ContainsKey(date))
                return null;
            Dictionary<string, Appointment> dateAppts = appointmentsByDateThenSubjectName[date];
            List<Appointment> result = new List<Appointment>();
            foreach (Appointment app in dateAppts.Values)
                result.Add(app);
            return result;
        }
        public Appointment GetAppointment(string subjectName, DateTime? onDate = null, bool doAssert = true)
        {
            Appointment result;
            if (appointmentsByFullName == null)
                LoadAppointmentCaches();

            if (WindowsDriverUtils.DumpFlag)
            {
                WindowsDriverUtils.Log.WriteLine("Begin dump");
                foreach (var x in appointmentsByFullName.Keys)
                {
                    WindowsDriverUtils.Log.WriteLine(x);
                }
                WindowsDriverUtils.Log.WriteLine("end dump");
            }

            if (onDate != null)
            {
                if (appointmentsByFullName.ContainsKey(Appointment.GetFullName((DateTime)onDate, subjectName)))
                    result = appointmentsByFullName[Appointment.GetFullName((DateTime)onDate, subjectName)];
                else
                    result = null;
            }
            else
            {
                if (!appointmentsBySubjectNameThenDate.ContainsKey(subjectName))
                    result = null;
                else
                {
                    List<Appointment> subjectDates = appointmentsBySubjectNameThenDate[subjectName];
                    result = subjectDates[0];
                }
            }
            if (doAssert && result == null)
            {
                throw new Exception("AppointmentNotFound");
            }
            else
                return result;
        }
        public WinElement GetCalendarPreviousButton()
        {

            WinElement btnPrev = FindElementByName(this.Element, nameof(btnPrev));
            return btnPrev;
        }
        public WinElement GetCalendarNextButton()
        {
            WinElement btnNext = FindElementByName(this.Element, nameof(btnNext));
            return btnNext;
        }
        public WinElement GetCalendarLineUpButton()
        {             
            WinElement LineUp = FindElementByName(Element, "Line Up");
            return LineUp;
        }
        public WinElement GetCalendarLineDownButton()
        {
            WinElement LineDown = FindElementByName(this.Element, "Line Down");
            return LineDown;
        }
        public WinElement GetCalendarPageUpButton()
        {
            WinElement PageUp = FindElementByName(this.Element, "Page Up");
            return PageUp;
        }
        public WinElement GetCalendarPageDownButton()
        {
            WinElement PageDown = FindElementByName(this.Element, "Page Down");
            return PageDown;
        }
        #endregion methods
    }

    #region helper classes
    public class Appointment
    {
        public DateTime DateTime { get; set; }
        public string Subject{ get; set; }     
        public WinElement WinElement { get; set; }

        internal string FullName { get { return Appointment.GetFullName(DateTime, Subject); } }
        static internal string GetFullName(DateTime dateTime, string subject)
        {
            return dateTime.ToShortDateString() + subject;
        }
    }

    public class CalendarState
    {
        public DateTime Start { get { return start; } }
        public DateTime End { get { return end; } }
        public DateTime SelectedDate { get { return selectedDate;} set { selectedDate = value; } }
        public CalendarState() { }
        public CalendarState(DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;
        }
        public bool Contains(DateTime moment)
        {
            return moment >= this.start && moment <= this.end;
        }

        private DateTime start = DateTime.MinValue;
        private DateTime end = DateTime.MinValue;
        private DateTime selectedDate = DateTime.MinValue;
    }
    #endregion helper classes
}
