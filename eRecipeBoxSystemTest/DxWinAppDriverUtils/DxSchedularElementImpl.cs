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
        private Dictionary<string, Appointment> appointmentsByFullName = null;
        private Dictionary<DateTime, Dictionary<string, Appointment>> appointmentsByDateThenSubjectName = null;
        private Dictionary<string, List<Appointment>> appointmentsBySubjectNameThenDate = null;
        List<List<NavDateElement>> dateElements = null;
      

        #region ctor
        internal DxSchedularElement(WinElement schedulerControl, WinElement dateNavigator, DxFormElements<T> parentForm)
        {
            this.Element = schedulerControl;
            this.DateNavigator = dateNavigator;
            this.ParentForm = parentForm;
        }
        #endregion ctor

        private void LoadDateElements()
        {
            dateElements = new List<List<NavDateElement>>();
            DateTime calStart = DateTime.MinValue;
            string currentMonth = null;
            List<NavDateElement> datesForOneMonthPanel = null;
            foreach (WinElement element in FindElementsByXPath(DateNavigator, AllDescendants))
            {
                string etext = Textify(element.Text);
                string name = Textify(element.GetAttribute("Name"));
                int nameAsInt;
                if (!Int32.TryParse(name, out nameAsInt))
                    nameAsInt = int.MinValue;
                if (NavDateElement.monthsInfo.ContainsKey(name))
                {
                    bool isFirst = (currentMonth == null);
                    currentMonth = name;
                    //moved on to next month... set the dates for the prev month
                    if (isFirst)
                    {
                        calStart = DateTime.Parse(etext);
                        datesForOneMonthPanel = new List<NavDateElement>();
                    }
                    else
                    {
                        NavDateElement.SetDates(datesForOneMonthPanel);
                        dateElements.Add(datesForOneMonthPanel);
                        datesForOneMonthPanel = new List<NavDateElement>();
                    }
                }
                else if (nameAsInt != int.MinValue)
                {
                    NavDateElement dtEl = new NavDateElement();
                    dtEl.dateInt = nameAsInt;
                    dtEl.panelMonthName = currentMonth;
                    dtEl.el = element;
                    dtEl.calStart = calStart;
                    datesForOneMonthPanel.Add(dtEl);
                }
            }
            NavDateElement.SetDates(datesForOneMonthPanel);
            dateElements.Add(datesForOneMonthPanel);
        }

        private CalendarState GetCalendarState(WinElement monthCalenderElement)
        {
            CalendarState result = new CalendarState();
            string name = Textify(monthCalenderElement.GetAttribute("Name"));
            //#WARNING #FRAGILE
            //ex... this is DevEx V22  Month Calendar, 8 total events, 12:00 AM  Sunday, June 11, 2023 to 12:00 AM  Sunday, June 18, 2023  
            //ex... this is DevEx V23.x Month Calendar, 12:00 AM  Sunday, July 30, 2023 to 12:00 AM  Sunday, August 27, 2023
            Regex monthIntervalPattern = new Regex(@"(?<=Month Calendar, )(?<start>.*)(?: to )(?<end>.*)");
            //Regex monthIntervalPatternV22 = new Regex(@"(?<=Month Calendar, \d* total events, )(?<start>.*)(?: to )(?<end>.*)");
            MatchCollection mc = monthIntervalPattern.Matches(name);
            foreach (Match m in mc)
            {
                result = new CalendarState(DateTime.Parse(m.Groups["start"].Value.Trim()),
                    DateTime.Parse(m.Groups["end"].Value.Trim()));
            }
            if (result == null)
                throw new Exception("Logic Error");


            //#WARNING #FRAGILE
            //this is DevExpress V23.x how to get the selected date
            //eg, August 2
            string selectedDate = Textify(monthCalenderElement.Text);
            string currYr = result.Start.Year.ToString();
            DateTime selDate = DateTime.Parse($"{selectedDate}, {currYr}");
            //unfortunately no year, so we have to search on the cal for month and day
            DateTime currDate = result.Start;
            while (true)
            {
                if (currDate.Month == selDate.Month && currDate.Day == selDate.Day)
                {
                    result.SelectedDate = currDate;
                    break;
                }
                currDate = currDate.AddDays(1);
            }
            if (result.SelectedDate == DateTime.MinValue)
                throw new Exception("Logic Error");
            return result;

            //#WARNING #FRAGILE this is how we got the selected day in DevExpress V22  
            //foreach (WinElement el in FindElementsByXPath(monthCalenderElement, AllDescendants))
            //{
            //    string ctype = Textify(el.GetAttribute("LocalizedControlType"));
            //    //conditions for the interval, list of events
            //    if (ctype == "list item")
            //    {
            //        name = Textify(el.GetAttribute("Name"));
            //        //ex... Month Calendar, 1 total events 12:00 AM  Monday, June 19, 2023 to 12:00 AM  Tuesday, June 20, 2023, 0 Events 
            //        monthIntervalPattern = new Regex(@"(?<=Month Calendar, \d* total events )(?<start>.*)(?: to )(?<end>.*)(?:, .*)");
            //        mc = monthIntervalPattern.Matches(name);
            //        if (mc.Count == 0)
            //        {
            //            //#WORKAROUND
            //            //or this format... 12:00 AM  Wednesday, June 21, 2023 to 12:00 AM  Thursday, June 22, 2023, 0 Events 
            //            monthIntervalPattern = new Regex(@"(?<start>.*)(?: to )(?<end>.*)(?:, .*)");
            //            mc = monthIntervalPattern.Matches(name);
            //        }

            //        foreach (Match m in mc)
            //        {
            //            string startDate = m.Groups["start"].Value.Trim();
            //            result.SelectedDate = DateTime.Parse(startDate);
            //            return result;
            //        }
            //    }
            //}
            //throw new Exception("Logic Error");
        }

        private void LoadAppointmentCaches()
        {
            LoadAppointmentCachesOneAttempt();
            
            //#TODO debug only
            //WindowsDriverUtils.Log.WriteLine($"appointmentsByFullName count {appointmentsByFullName.Count}");
            //WindowsDriverUtils.Log.WriteLine($"appointmentsByDateThenSubjectName count {appointmentsByDateThenSubjectName.Count}");
            //WindowsDriverUtils.Log.WriteLine($"appointmentsBySubjectNameThenDate count {appointmentsBySubjectNameThenDate.Count}");

            if (appointmentsByFullName.Count == 0)
            {

                //#WORKAROUND force devX to populate calendar events elements
                this.Element.Click(); Pause(1);
                this.GetCalendarPreviousButton().Click(); Pause(1);
                this.GetCalendarNextButton().Click(); Pause(1);
                this.Element.SendKeys(Keys.ArrowRight);//#TRICKY arrow right to deselect all appts
                //try again
                LoadAppointmentCachesOneAttempt();
            }

        }
        private void LoadAppointmentCachesOneAttempt()
        {
            appointmentsByFullName = new Dictionary<string, Appointment>();
            appointmentsByDateThenSubjectName = new Dictionary<DateTime, Dictionary<string, Appointment>>();
            appointmentsBySubjectNameThenDate = new Dictionary<string, List<Appointment>>();
            foreach (WinElement el in FindElementsByXPath(Element, AllDescendants))
            {                
                string elText = el.Text;
                //conditions for an appointment                
                if (elText == "Appointment")                 
                {
                    string name = Textify(el.GetAttribute("Name"));
                    //V23.2 ex Coconut Red Curry with Tofu, 12:00 AM  Sunday, July 23, 2023 to 12:00 AM  Monday, July 24, 2023, Time Free. 1 of 20 
                    Regex appointmentPattern = new Regex(@"(?<subject>.*), 12:00 AM (?<start>.*)(?: to )(?<end>.*), Time Free.(?:.*)");

                    //V22 ... Month Calendar, 8 total events 12:00 AM  Sunday, June 11, 2023 to 12:00 AM  Monday, June 12, 2023, Subject Naked Spinach Lasagna Quinoa Casserole, Time Free, 1 of 8
                    //Regex appointmentPattern = new Regex(@"(?<=Month Calendar, \d* total events)(?<start>.*)(?: to )(?<end>.*)(?:, Subject )(?<subject>.*)(?:, Time*)");

                    MatchCollection mc = appointmentPattern.Matches(name);

                    //V22 1 //#WORKAROUND try another format
                    //if (mc.Count == 0)
                    //{                        
                    //    //or this format... 12:00 AM  Monday, June 19, 2023 to 12:00 AM  Tuesday, June 20, 2023, Subject Avocado Gazpacho, Time Free, 10 of 13 
                    //    appointmentPattern = new Regex(@"(?<start>.*)(?: to )(?<end>.*)(?:, Subject )(?<subject>.*)(?:, Time*)");
                    //    mc = appointmentPattern.Matches(name);
                    //}
                    foreach (Match m in mc)
                    {
                        DateTime appointmentDate = DateTime.Parse(m.Groups["start"].Value.Trim());
                        DateTime appointmentDateEnd = DateTime.Parse(m.Groups["end"].Value.Trim());
                        string appointmentSubject = m.Groups["subject"].Value.Trim();
                        Appointment newAppt = new Appointment();
                        newAppt.Subject = appointmentSubject;
                        newAppt.DateTime = appointmentDate;
                        newAppt.WinElement = el;
                        appointmentsByFullName[newAppt.FullName] = newAppt;

                        if (!appointmentsByDateThenSubjectName.ContainsKey(newAppt.DateTime))
                            appointmentsByDateThenSubjectName[newAppt.DateTime] = new Dictionary<string, Appointment>();
                        Dictionary<string, Appointment> dayAppts = appointmentsByDateThenSubjectName[newAppt.DateTime];
                        dayAppts[newAppt.Subject] = newAppt;

                        if (!appointmentsBySubjectNameThenDate.ContainsKey(newAppt.Subject))
                            appointmentsBySubjectNameThenDate[newAppt.Subject] = new List<Appointment>();
                        List<Appointment> subjectDates = appointmentsBySubjectNameThenDate[newAppt.Subject];
                        subjectDates.Add(newAppt);
                    }
                }
            }
        }

        private WinElement GetCalendarElement()
        {
            //#WARNING only works on month calendar.
            foreach (WinElement el in FindElementsByXPath(Element, AllDescendants))
            {
                string ctype = Textify(el.GetAttribute("LocalizedControlType"));
                string name = Textify(el.GetAttribute("Name"));
                //#WARNING #FRAGILE DevEx changed this from list to pane from V22   then back to ctype=list for V23.2
                //Just assume the first "Month Calendar" element 
                if (ctype == "pane" && name.StartsWith("Month Calendar"))  //V22 ctype == "list" &&
                {
                    return el;
                }
            }
            throw new Exception("Unable to find calendar element");
        }

    }

    internal class NavDateElement
    {
        internal int dateInt;
        internal DateTime date = DateTime.MinValue;
        internal WinElement el;
        internal string panelMonthName;
        internal DateTime calStart;

        static private int FindIndex(IList<NavDateElement> list, int dayNumber)
        {
            int index = 0;
            foreach (NavDateElement el in list)
            {
                if (el.dateInt == dayNumber)
                    return index;
            }
            throw new Exception("logic error");
        }
        static internal void SetDates(IList<NavDateElement> list)
        {
            DateTime claStart = list[0].calStart;
            Tuple<int, int> monthInfo = monthsInfo[list[0].panelMonthName];
            int firstOfMonthIndex = FindIndex(list, 1);
            int lastMonthIndex = FindIndex(list, monthInfo.Item2);
            //set the dates for that month
            for (int i = firstOfMonthIndex; i <= lastMonthIndex; i++)
            {
                NavDateElement el = list[i];
                el.date = new DateTime(claStart.Year, monthInfo.Item1, el.dateInt);
            }
            //Set remaining forward
            DateTime currDate = list[firstOfMonthIndex].date;
            for (int j = firstOfMonthIndex + 1; j < list.Count; j++)
            {
                NavDateElement navDateElement = list[j];
                navDateElement.date = currDate.AddDays(1);
                currDate = navDateElement.date;
            }
            //Set remaining forward
            currDate = list[list.Count - 1].date;
            for (int j = list.Count - 2; j >= 0; j--)
            {
                NavDateElement navDateElement = list[j];
                navDateElement.date = currDate.AddDays(-1);
                currDate = navDateElement.date;
            }
        }

        static internal Dictionary<string, Tuple<int, int>> monthsInfo = new Dictionary<string, Tuple<int, int>>
            {
{"January",Tuple.Create(1,31)},
{"February",Tuple.Create(2,28)},
{"March",Tuple.Create(3,31)},
{"April",Tuple.Create(4,30)},
{"May",Tuple.Create(5,31)},
{"June",Tuple.Create(6, 30)},
{"July",Tuple.Create(7, 31)},
{"August",Tuple.Create(8, 31)},
{"September",Tuple.Create(9, 30)},
{"October",Tuple.Create(10, 31)},
{"November",Tuple.Create(11, 30)},
{"December",Tuple.Create(12, 31)}
            };
    }
}
