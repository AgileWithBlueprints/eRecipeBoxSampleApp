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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static DxWinAppDriverUtils.DxElementPrimitives;
using static eRecipeBoxSystemTests.eRecipeBoxSession;
using static eRecipeBoxSystemTests.eRecipeBoxTestUtils;
using static eRecipeBoxSystemTests.eRecipeBoxWindowElements;

namespace eRecipeBoxSystemTests.FunctionalTests
{
    [TestClass]
    public class SearchRecipeBox
    {
        //dependencies..SysTest01 test data
        [TestMethod]
        public void TestCase01()
        {
            WindowsDriverUtils.Log.WriteLine($"Start run {nameof(TestCase01)}");

            //#TRICKY.. Cooked dates for test use relative dates vs absolute dates
            DateTime today = DateTime.Today;
            int dayOfWeekIdx = (int)today.DayOfWeek;

            DateTime lastSunday = today.AddDays(-dayOfWeekIdx);
            if (dayOfWeekIdx == 0)
                lastSunday = today.AddDays(-7);

            //we are planning meals for next week..  Anchor off nextWeekMonday
            DateTime nextWeekMonday = lastSunday.AddDays(8);
            DateTime mondayLess1 = nextWeekMonday.AddDays(-7);
            DateTime mondayLess2 = mondayLess1.AddDays(-7);
            DateTime mondayPlus2 = nextWeekMonday.AddDays(7);
            DateTime mondayLess6 = nextWeekMonday.AddDays(-6 * 7);

            DateTime sundayLess2 = nextWeekMonday.AddDays(-1 - 7);
            DateTime sundayLess5 = nextWeekMonday.AddDays((-4 * 7) - 1);

            DateTime tuesdayPlus1 = nextWeekMonday.AddDays(1);
            DateTime tuesdayLess5 = nextWeekMonday.AddDays((-4 * 7) - 6);

            DateTime thursdayPlus1 = nextWeekMonday.AddDays(3);
            DateTime thursdayLess1 = nextWeekMonday.AddDays(-4);
            DateTime thursdayLess2 = thursdayLess1.AddDays(-7);
            DateTime thursdayLess5 = nextWeekMonday.AddDays((-4 * 7) - 4);

            DateTime fridayPlus1 = nextWeekMonday.AddDays(4);
            DateTime fridayLess2 = nextWeekMonday.AddDays(-3 - 7);
            DateTime fridayLess3 = fridayLess2.AddDays(-7);
            DateTime fridayLess4 = fridayLess3.AddDays(-7);


            DateTime saturdayPlus1 = nextWeekMonday.AddDays(5);
            DateTime saturdayLess3 = nextWeekMonday.AddDays((-2 * 7) - 2);
            DateTime saturdayLess4 = nextWeekMonday.AddDays((-3 * 7) - 2);



            //#SearchRecipeBoxTestCase01-0001 login login using cached creds (thus no Login form)
            DxFormElements<SRBs> srb = new DxFormElements<SRBs>(TestSession);

            //trigger easter egg to verify and calibrate system test data
            srb.FormElement.SendKeys(Keys.Control + Keys.Alt + Keys.Shift + "t" + Keys.Control + Keys.Alt + Keys.Shift); Pause(1);
            srb.Get(SRBs.Refresh).Click(); Pause(1);

            //trigger easter egg to send next monday as the anchor date for logging cooked dates while eRB runs the test
            srb.Get(SRBs.filterTermLookUpEdit).SendKeys(nextWeekMonday.Date.ToString()); Pause(1);
            srb.FormElement.SendKeys(Keys.Control + Keys.Alt + Keys.Shift + "a" + Keys.Control + Keys.Alt + Keys.Shift); Pause(1);
            ClearFilter(srb);

            DxGridViewElement<SRBs> searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);
            LogBreadcrumbOnErecipeBox(srb); Pause(2);// 1

            //#SearchRecipeBoxTestCase01-0008 GrdFilter filter TotalTimemin, MyRating, Flagged columns
            //#WORKAROUND For some  reason, when running SetColumnFilter later in the script, grid header elements don't appear.  Run this first.
            //This will fail to find TotalTimemin column, so continue to restart the test until it passes this point.
            //Not deterministic.
            try
            {
                searchResults.SetNumericColumnFilter(SRBs.TotalTimemin, TestSession.WinAppDrvrSession, "30", "60"); Pause();
            }
            catch
            {
                WindowsDriverUtils.DumpAll(TestSession.WinAppDrvrSession);
                throw;
            }
            searchResults.SetNumericColumnFilter(SRBs.MyRating, TestSession.WinAppDrvrSession, "5", null); Pause();
            searchResults.SetCheckBoxColumnFilter(SRBs.Flagged, TestSession.WinAppDrvrSession, true); Pause();
            ClearFilter(srb);

            //#SearchRecipeBoxTestCase01-0019 1SrchRcBxFrm copy from grid and paste to calendar
            //#WORKAROUND..  For some  reason, when running SetColumnFilter later in the script, grid header elements don't appear.  Run this first.            
            FilterRecipeBox(srb, "Caramel-Pecan Dream Bars"); Pause(.5);
            LogBreadcrumbOnErecipeBox(srb); // 2
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);
            WinElement row1Title = searchResults.GetCell(1, SRBs.Title);
            row1Title.Click(); Pause(); //focus
            row1Title.Click(); Pause();//hyperlink.
            ClearFilter(srb);

            //#SearchRecipeBoxTestCase01-0002 BtnFltr create unique value in each possible search RC property.  Then filter on each individually.
            FilterRecipeBox(srb, "Test Title 01"); ClearFilter(srb);
            FilterRecipeBox(srb, "Test Notes 01"); ClearFilter(srb);
            FilterRecipeBox(srb, "Test Description 01"); ClearFilter(srb);
            FilterRecipeBox(srb, "Test Instructions 01"); ClearFilter(srb);
            FilterRecipeBox(srb, "Test First Name 01"); ClearFilter(srb);
            FilterRecipeBox(srb, "Test Last Name 01"); ClearFilter(srb);
            FilterRecipeBox(srb, "Test item description 01"); ClearFilter(srb);
            FilterRecipeBox(srb, "Test Item 01"); ClearFilter(srb);
            FilterRecipeBox(srb, "Test Keyword 01"); ClearFilter(srb);

            //#SearchRecipeBoxTestCase01-0003 BtnFltr verify not filtering on Yield, prep time, cook time, total time, rating, rating count, ingredient.Quantity and ingredient.UoM. Create unique value in each field.  Then filter on each individually.
            FilterRecipeBox(srb, "Test Yield 01"); ClearFilter(srb);
            FilterRecipeBox(srb, "9999"); ClearFilter(srb);
            FilterRecipeBox(srb, "9998"); ClearFilter(srb);
            FilterRecipeBox(srb, "9997"); ClearFilter(srb);
            FilterRecipeBox(srb, "88.88"); ClearFilter(srb);
            FilterRecipeBox(srb, "9996"); ClearFilter(srb);
            FilterRecipeBox(srb, "test qty 01"); ClearFilter(srb);
            FilterRecipeBox(srb, "test uom 01"); ClearFilter(srb);

            //#SearchRecipeBoxTestCase01-0004 BtnFltr verify OR across fields by querying the database with the equivalent SQL (next tab)
            FilterRecipeBox(srb, "potato"); ClearFilter(srb);
            FilterRecipeBox(srb, "potatoes"); ClearFilter(srb);
            FilterRecipeBox(srb, "easy"); ClearFilter(srb);
            FilterRecipeBox(srb, "summer"); ClearFilter(srb);
            FilterRecipeBox(srb, "mix"); ClearFilter(srb);

            //#SearchRecipeBoxTestCase01-0005 BtnFltr  AND Filter on multiple terms:  2 items, 3 items, 4 items, 2 items+2 keywords, 1 itemDesc+1 keywords+notes, 1 item+1 keywords+sourceFName.  Try some with same combination but in different order.  Should yield same results 
            FilterRecipeBox(srb, "Tomatoes");
            FilterRecipeBox(srb, "Salt And Pepper");
            FilterRecipeBox(srb, "Cucumber");
            ClearFilter(srb); Pause(1.5);

            FilterRecipeBox(srb, "Cucumber");
            FilterRecipeBox(srb, "Tomatoes");
            FilterRecipeBox(srb, "Salt And Pepper"); Pause(.5);//should get same result            
            FilterRecipeBox(srb, "Avocado");
            ClearFilter(srb);

            FilterRecipeBox(srb, "garlic   ");
            FilterRecipeBox(srb, "olive oil");
            FilterRecipeBox(srb, "easy");
            FilterRecipeBox(srb, "vegetarian");
            ClearFilter(srb);

            FilterRecipeBox(srb, "garlic, minced");
            FilterRecipeBox(srb, "easy");
            FilterRecipeBox(srb, "does not freeze well");
            ClearFilter(srb);

            FilterRecipeBox(srb, "does not freeze well");
            FilterRecipeBox(srb, "easy");
            FilterRecipeBox(srb, "garlic, minced");
            ClearFilter(srb);

            FilterRecipeBox(srb, "test first name");
            FilterRecipeBox(srb, "elbow macaroni");
            FilterRecipeBox(srb, "lunch");
            ClearFilter(srb);

            FilterRecipeBox(srb, "elbow macaroni");
            FilterRecipeBox(srb, "lunch");
            FilterRecipeBox(srb, "test first name");
            ClearFilter(srb);

            LogBreadcrumbOnErecipeBox(srb); // 3

            //#SearchRecipeBoxTestCase01-0006 FltrTrmInpt test
            srb.Get(SRBs.filterTermLookUpEdit).SendKeys("vegetable  " + Keys.Enter); Pause(); ClearFilter(srb);
            FilterRecipeBox(srb, "med" + Keys.Tab); ClearFilter(srb);
            LogBreadcrumbOnErecipeBox(srb); // 4
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);

            //#SearchRecipeBoxTestCase01-0007 GrdSort sort asc/desc on all columns
            FilterRecipeBox(srb, "salt");
            LogBreadcrumbOnErecipeBox(srb); // 5
            WinElement col = searchResults.GetColumnHeader(SRBs.Title);
            col.Click(); Pause();
            col.Click(); Pause();

            col = searchResults.GetColumnHeader(SRBs.TotalTimemin);
            col.Click(); Pause();
            col.Click(); Pause();

            col = searchResults.GetColumnHeader(SRBs.RatingCount);
            col.Click(); Pause();
            col.Click(); Pause();

            col = searchResults.GetColumnHeader(SRBs.MyRating);
            col.Click(); Pause();
            col.Click(); Pause();

            col = searchResults.GetColumnHeader(SRBs.Flagged);
            col.Click(); Pause();
            col.Click(); Pause();

            col = searchResults.GetColumnHeader(SRBs.Last);
            col.Click(); Pause();
            col.Click(); Pause();

            col = searchResults.GetColumnHeader(SRBs.Source);
            col.Click(); Pause();
            col.Click(); Pause();
            ClearFilter(srb); Pause();

            //#WORKAROUND For some reason, grid cell elements aren't available after Clear.. Filtering forces them to appear.
            FilterRecipeBox(srb, "summer");
            LogBreadcrumbOnErecipeBox(srb); // 6

            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);
            searchResults.Element.SendKeys(Keys.ArrowDown); Pause(.5);
            searchResults.Element.SendKeys(Keys.ArrowDown); Pause(.5);
            string row3TitleText = searchResults.GetCell(3, SRBs.Title).Text;

            //#SearchRecipeBoxTestCase01-0009 GrdCtlEntr  test            
            searchResults.Element.SendKeys(Keys.Control + Keys.Enter + Keys.Control); Pause(1);
            DxFormElements<ERCs> erc = new DxFormElements<ERCs>(TestSession);
            string rcTitle = GetRecCardTitleFromERCTitleBar(TestSession.WinAppDrvrSession);  //#REFACTOR
            //We opened the selected RC
            Assert.AreEqual(row3TitleText, rcTitle);
            erc.Get(ERCs.SaveandClose).Click(); Pause(.3);

            srb = new DxFormElements<SRBs>(TestSession); Pause(1);
            LogBreadcrumbOnErecipeBox(srb); // 7
            //#SearchRecipeBoxTestCase01-0010 GrdClsRCFrmFcs  test
            Assert.IsTrue(GetTitleOfFocusedRow(searchResults).Contains(rcTitle));

            //#SearchRecipeBoxTestCase01-0011 GrdDblClk  test
            srb = new DxFormElements<SRBs>(TestSession);
            ClearFilter(srb); Pause(.3);

            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 8
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);
            WinElement row3Title = searchResults.GetCell(3, SRBs.Title);
            row3TitleText = row3Title.Text;
            //double click on the same row
            WindowsDriverUtils.DoubleClick(TestSession.WinAppDrvrSession, row3Title); Pause(1);

            erc = new DxFormElements<ERCs>(TestSession);
            string rcTitle2 = GetRecCardTitleFromERCTitleBar(TestSession.WinAppDrvrSession);
            Assert.AreEqual(row3TitleText, rcTitle2);
            erc.Get(ERCs.SaveandClose).Click(); Pause(.3);


            //#SearchRecipeBoxTestCase01-0012 GrdEntr  test
            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 9
            FilterRecipeBox(srb, "red onion"); Pause();
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);
            WinElement row5Title = searchResults.GetCell(5, SRBs.Title);
            string row5TitleText = row5Title.Text;
            row5Title.Click();
            searchResults.Element.SendKeys(Keys.Enter);
            DxFormElements<VRCs> vrc = new DxFormElements<VRCs>(TestSession);
            WinElement vrcEl = vrc.FormElement;
            rcTitle2 = GetRecCardTitleFromVRCTitleBar(TestSession.WinAppDrvrSession);
            Assert.AreEqual(row5TitleText, rcTitle2);
            vrcEl.SendKeys(Keys.Alt + "c"); Pause(1);  //Close button closes the entire app

            //#SearchRecipeBoxTestCase01-0013 GrdClsRCFrmFcs  test
            Assert.IsTrue(GetTitleOfFocusedRow(searchResults).Contains(row5TitleText));

            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 10
            ClearFilter(srb);
            //#WORKAROUND For some reason, grid cell elements aren't available after Clear.. Filtering forces them to appear.
            FilterRecipeBox(srb, "easy"); Pause();
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);

            LogBreadcrumbOnErecipeBox(srb); // 11



            //#SearchRecipeBoxTestCase01-0014 GrdEdtLstCkdDt test
            string row13TitleName = searchResults.GetCell(13, SRBs.Title).Text;
            WinElement row13Last = searchResults.GetCell(13, SRBs.Last);
            row13Last.Click(); Pause();
            //edit last date
            row13Last.SendKeys(Keys.Shift + "e"); Pause(.2);

            DxFormElements<ELCDs> elcd = new DxFormElements<ELCDs>(TestSession);
            DateTime testDate = fridayPlus1;
            //elcd.Get(ELCDs.dateEdit).Click(); Pause(); DevExpress V22.  remove for V23.2
            srb.FormElement.SendKeys(Keys.Control + "a" + Keys.Control); Pause(1); //select all
            elcd.Get(ELCDs.dateEdit).SendKeys(EditFmt(testDate) + Keys.Tab); Pause();
            elcd.Get(ELCDs.Cancel).Click(); Pause();

            row13Last.SendKeys(Keys.Shift + "e"); Pause(.2);
            testDate = testDate.AddDays(-1);
            //elcd.Get(ELCDs.dateEdit).Click(); Pause(); DevExpress V22.  remove for V23.2
            srb.FormElement.SendKeys(Keys.Control + "a" + Keys.Control); Pause(1); //select all
            elcd.Get(ELCDs.dateEdit).SendKeys(EditFmt(testDate) + Keys.Tab); Pause();
            elcd.Get(ELCDs.OK).Click(); Pause(2);

            //#SearchRecipeBoxTestCase01-0015 GrdCkdDtEdtStFcs test
            Assert.IsTrue(GetTitleOfFocusedRow(searchResults).Contains(row13TitleName));

            //#SearchRecipeBoxTestCase01-0016 GrdEdtFlg test
            WinElement row9Flagged = searchResults.GetCell(9, SRBs.Flagged);
            row9Flagged.Click(); Pause(.3);  //set focus
            row9Flagged.Click(); Pause(.3); //2nd click, toggles... 

            //#SearchRecipeBoxTestCase01-0017 GrdEdtMyRtng test
            ClearFilter(srb);
            LogBreadcrumbOnErecipeBox(srb); // 12
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);
            col = searchResults.GetColumnHeader(SRBs.MyRating);
            col.Click(); Pause();
            col.Click(); Pause(.1);
            searchResults.Element.SendKeys(Keys.Control + Keys.Home + Keys.Control);
            WinElement row11MyRating = searchResults.GetCell(11, SRBs.MyRating);
            string row11MyRatingText = row11MyRating.Text;
            row11MyRating.Click(); Pause();
            //edit myrating
            row11MyRating.SendKeys(Keys.Shift + "e"); Pause(.2);


            DxFormElements<EMRs> emrs = new DxFormElements<EMRs>(TestSession);
            int? testValue = null;

            //decrement existing rating
            testValue = Int32.Parse(row11MyRatingText);
            if (testValue > 1)
                testValue--;
            else
                testValue++;

            srb.FormElement.SendKeys(Keys.Control + "a" + Keys.Control); Pause(); //select all
            emrs.Get(EMRs.spinEdit).SendKeys(testValue.ToString()); Pause();
            emrs.Get(EMRs.Cancel).Click(); Pause(1);

            row11MyRating.SendKeys(Keys.Shift + "e"); Pause(.2);

            emrs = new DxFormElements<EMRs>(TestSession);
            srb.FormElement.SendKeys(Keys.Control + "a" + Keys.Control); Pause(); //select all
            emrs.Get(EMRs.spinEdit).SendKeys(testValue.ToString()); Pause();
            emrs.Get(EMRs.OK).Click(); Pause(.1);

            //#SearchRecipeBoxTestCase01-0018 GrdTtlHyprLnk both with valid URL and no URL
            ClearFilter(srb); Pause();
            LogBreadcrumbOnErecipeBox(srb); // 13
            FilterRecipeBox(srb, "Tuna and Avocado Salad"); Pause();
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);
            row1Title = searchResults.GetCell(1, SRBs.Title);
            row1Title.Click(); Pause(); //focus
            row1Title.Click(); Pause();//hyperlink.
                                       //
                                       //#SearchRecipeBoxTestCase01-0019 1SrchRcBxFrm copy from grid and paste to calendar
            srb.Get(SRBs.MonthView).Click();
            ClearFilter(srb); Pause();
            LogBreadcrumbOnErecipeBox(srb); // 14
            FilterRecipeBox(srb, "Cheesy Rice and Zucchini Frittata"); Pause();
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);
            searchResults.Element.SendKeys(Keys.ArrowDown + Keys.ArrowDown + Keys.ArrowDown);
            searchResults.Element.SendKeys(Keys.Control + "c" + Keys.Control);
            DxSchedularElement<SRBs> scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            scheduler.SelectDateOnCalendar(tuesdayPlus1);
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control);
            ClearFilter(srb); Pause();
            Appointment appt = scheduler.GetAppointment("Cheesy Rice and Zucchini Frittata", tuesdayPlus1);
            Assert.IsNotNull(appt);

            LogBreadcrumbOnErecipeBox(srb); // 15

            //#SearchRecipeBoxTestCase01-0021 ClndrClpbrd with single, CTL and SHFIT.  Each with copy and cut/paste
            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            scheduler.Element.Click(); Pause(1);
            scheduler.Element.SendKeys(Keys.ArrowRight); Pause(); //#TRICKY arrow right to deselect all appts
            LogBreadcrumbOnErecipeBox(srb); // 16
            scheduler.ScrollFirstDayOfCalendar(sundayLess2);
            DateTime moveDate;
            Win32API.LockShiftKey(true);
            try
            {
                Appointment rcard = scheduler.GetAppointment("Spinach Lasagna Quinoa Casserole");
                moveDate = rcard.DateTime.AddDays(-2);
                rcard.WinElement.Click(); Pause();
                rcard = scheduler.GetAppointment("Coconut Red Curry with Tofu");
                rcard.WinElement.Click(); Pause();
            }
            finally
            {
                Win32API.LockShiftKey(false);
            }
            scheduler.Element.SendKeys(Keys.Control + "x" + Keys.Control);
            scheduler.SelectDateOnCalendar(moveDate); Pause();
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control); Pause(1);
            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            scheduler.Element.Click(); Pause(1);
            scheduler.Element.SendKeys(Keys.ArrowRight); Pause(); //#TRICKY arrow right to deselect all appts

            LogBreadcrumbOnErecipeBox(srb); // 17
            DateTime start = mondayLess6;
            moveDate = start.AddDays(-4);
            scheduler.SelectDateOnCalendar(start);

            Win32API.LockControlKey(true);
            try
            {
                scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click(); start = start.AddDays(2); Pause();
                scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click(); start = start.AddDays(1); Pause();
                scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click(); start = start.AddDays(3); Pause();
                scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click();
            }
            finally
            {
                Win32API.LockControlKey(false);
            }
            Pause(1);
            scheduler.Element.SendKeys(Keys.Control + "c" + Keys.Control);
            scheduler.SelectDateOnCalendar(moveDate);
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control); Pause(.3);

            LogBreadcrumbOnErecipeBox(srb); // 18

            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            scheduler.SelectDateOnCalendar(thursdayLess5);
            //#SearchRecipeBoxTestCase01-0024 ClndrDel test single and mult selected
            appt = scheduler.GetAllAppointmentsOnDate(thursdayLess5)[0]; // "Lots-of-Vegetables fried rice"
            appt.WinElement.Click(); Pause(); //TODO tue-5
            scheduler.Element.SendKeys(Keys.Delete); Pause(1);

            LogBreadcrumbOnErecipeBox(srb); // 19
            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            scheduler.SelectDateOnCalendar(fridayLess4);
            //#SearchRecipeBoxTestCase01-0021 ClndrClpbrd with single, CTL and SHFIT.  Each with copy and cut/paste
            appt = scheduler.GetAllAppointmentsOnDate(fridayLess4)[0];
            appt.WinElement.Click(); Pause();   //"Test Title 01"            
            scheduler.Element.SendKeys(Keys.Control + "x" + Keys.Control);
            moveDate = moveDate.AddDays(8);
            scheduler.SelectDateOnCalendar(moveDate);
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control); Pause(.3);

            LogBreadcrumbOnErecipeBox(srb); // 20

            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            //#SearchRecipeBoxTestCase01-0020 ClndrClckShft  verify this works by deleting what's selected
            //#SearchRecipeBoxTestCase01-0024 ClndrDel test single and mult selected
            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            scheduler.Element.Click(); Pause(1);
            scheduler.Element.SendKeys(Keys.ArrowRight); Pause(); //#TRICKY arrow right to deselect all appts

            LogBreadcrumbOnErecipeBox(srb); // 21
            start = sundayLess5;
            scheduler.SelectDateOnCalendar(start);

            Win32API.LockControlKey(true);
            try
            {
                scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click(); start = start.AddDays(3); Pause();  //sweet pot bowl
                scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click();   //air fryer til
            }
            finally
            {
                Win32API.LockControlKey(false);
            }
            Pause(1);

            scheduler.Element.SendKeys(Keys.Delete);

            LogBreadcrumbOnErecipeBox(srb); // 22

            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            scheduler.Element.Click(); Pause(1);
            scheduler.Element.SendKeys(Keys.ArrowRight); Pause(); //#TRICKY arrow right to deselect all appts

            LogBreadcrumbOnErecipeBox(srb); // 23
            start = saturdayLess4;
            scheduler.SelectDateOnCalendar(start.AddDays(3));//#TRICKY... make sure end date is in the view

            Win32API.LockShiftKey(true);
            try
            {
                scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click(); start = start.AddDays(3); Pause(); //garlic spin 16th
                scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click();
            }
            finally
            {
                Win32API.LockShiftKey(false);
            }
            Pause(1);
            scheduler.Element.SendKeys(Keys.Delete);

            LogBreadcrumbOnErecipeBox(srb); // 24

            //#SearchRecipeBoxTestCase01-0023 ClndrDblClk  test single and mult selected
            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);

            start = fridayLess3;
            scheduler.SelectDateOnCalendar(start);
            appt = scheduler.GetAllAppointmentsOnDate(start)[1];
            appt.WinElement.Click(); Pause(1);
            WindowsDriverUtils.DoubleClick(TestSession.WinAppDrvrSession, appt.WinElement); Pause(1); //moms caramel f

            erc = new DxFormElements<ERCs>(TestSession);
            rcTitle = GetRecCardTitleFromERCTitleBar(TestSession.WinAppDrvrSession);
            //We opened the selected RC
            Assert.AreEqual(appt.Subject, rcTitle);
            erc.Get(ERCs.SaveandClose).Click(); Pause(1);

            //#SearchRecipeBoxTestCase01-0025 ClndrEntr test single and mult selected
            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 25
            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);


            start = thursdayLess2;
            scheduler.SelectDateOnCalendar(start);
            appt = scheduler.GetAllAppointmentsOnDate(start)[0];
            appt.WinElement.Click(); //classic pad th
            LogBreadcrumbOnErecipeBox(srb); // 26

            scheduler.Element.SendKeys(Keys.Control + Keys.Enter + Keys.Control);

            erc = new DxFormElements<ERCs>(TestSession);
            rcTitle = GetRecCardTitleFromERCTitleBar(TestSession.WinAppDrvrSession);
            //We opened the selected RC
            Assert.AreEqual(appt.Subject, rcTitle);
            erc.Get(ERCs.SaveandClose).Click(); Pause(1);

            //#SearchRecipeBoxTestCase01-0027 BtnAd2GL test in grid.  Test with 1, 2,10 selected RCs in calendar.  Remove items in SIFGL only 1/2 the time.  Clear GL after each step.
            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 27
            ClearFilter(srb); Pause();
            FilterRecipeBox(srb, "summer");
            LogBreadcrumbOnErecipeBox(srb); // 28
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);
            searchResults.Element.SendKeys(Keys.ArrowDown); Pause(.5);
            searchResults.Element.SendKeys(Keys.ArrowDown); Pause(.5);
            row3TitleText = searchResults.GetCell(3, SRBs.Title).Text;
            LogBreadcrumbOnErecipeBox(srb); // 29
            srb.Get(SRBs.AddtoGroceryList).Click(); Pause(0.5);
            DxFormElements<SIFGLs> sifgl = new DxFormElements<SIFGLs>(TestSession);
            sifgl.Get(SIFGLs.AddItemstoGroceryList).Click(); Pause(1);
            DxFormElements<EGLs> egl = new DxFormElements<EGLs>(TestSession);
            egl.Get(EGLs.ClearAll).Click(); Pause();
            egl.Get(EGLs.SaveAndClose).Click(); Pause();

            //Add several from calendar and delete items i already have
            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 30
            ClearFilter(srb); Pause();
            LogBreadcrumbOnErecipeBox(srb); // 31
            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            scheduler.Element.Click(); Pause(1);
            scheduler.Element.SendKeys(Keys.ArrowRight); Pause(); //#TRICKY arrow right to deselect all appts

            start = fridayLess2;
            scheduler.SelectDateOnCalendar(start);
            scheduler.SelectDateOnCalendar(start.AddDays(-9));//force both on view

            Win32API.LockShiftKey(true);
            try
            {
                scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click(); start = start.AddDays(-9); Pause();  //22nd
                scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click();   //11th
            }
            finally
            {
                Win32API.LockShiftKey(false);
            }
            Pause(1);
            LogBreadcrumbOnErecipeBox(srb); // 32
            srb.Get(SRBs.AddtoGroceryList).Click(); Pause(2);
            sifgl = new DxFormElements<SIFGLs>(TestSession);
            DxGridViewElement<SIFGLs> selectIng = sifgl.FindGridViewElement(SIFGLs.selectIngrGridView);
            List<string> items = new List<string>();
            StreamReader inf = new StreamReader(TestSession.TestDataDirectory + "\\IngredientsToDelete03.txt");
            while (!inf.EndOfStream)
                items.Add(inf.ReadLine());
            inf.Close();
            selectIng.DeleteRows(SIFGLs.Item, items); Pause(0.5);

            sifgl.Get(SIFGLs.AddItemstoGroceryList).Click(); Pause(1);
            egl = new DxFormElements<EGLs>(TestSession);
            egl.Get(EGLs.ClearAll).Click(); Pause();
            egl.Get(EGLs.SaveAndClose).Click(); Pause();

            //#SearchRecipeBoxTestCase01-0028 BtnDelRecCrdCnfm  test
            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 33
            FilterRecipeBox(srb, null); Pause(.5);
            searchResults.Element.SendKeys(Keys.ArrowDown); Pause(.5);
            searchResults.Element.SendKeys(Keys.ArrowDown); Pause(.5);
            srb.Get(SRBs.DeleteRecipe).Click(); Pause(0.5);

            DxFormElements<Cnfmtns> cnfmtns = new DxFormElements<Cnfmtns>(TestSession);
            cnfmtns.Get(Cnfmtns.Yes).Click(); Pause(1);

            //#SearchRecipeBoxTestCase01-0029 BtnDelRecCrdEnbl  very button is disabled
            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 34
            FilterRecipeBox(srb, null); Pause(.5);
            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);

            start = mondayLess1;
            scheduler.SelectDateOnCalendar(start);
            scheduler.GetAllAppointmentsOnDate(start)[0].WinElement.Click();
            Assert.IsFalse(srb.Get(SRBs.DeleteRecipe).Enabled);

            //#SearchRecipeBoxTestCase01-0030 BtnVwEdRecCrdEnbl  very button is disabled
            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 35
            FilterRecipeBox(srb, "zzzzzzzzzzzzzzzzzzzz "); Pause(.5);
            Assert.IsFalse(srb.Get(SRBs.ViewRecipe).Enabled);
            Assert.IsFalse(srb.Get(SRBs.EditRecipe).Enabled);
            Assert.IsFalse(srb.Get(SRBs.DeleteRecipe).Enabled);
            Assert.IsFalse(srb.Get(SRBs.AddtoGroceryList).Enabled);
            Assert.IsTrue(srb.Get(SRBs.NewRecipe).Enabled);

            //#SearchRecipeBoxTestCase01-0031 ShCut test all shortcuts
            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 36
            ClearFilter(srb); Pause();
            LogBreadcrumbOnErecipeBox(srb); // 37
            FilterRecipeBox(srb, null); Pause(.5);
            srb.FormElement.SendKeys(Keys.Alt + "v" + Keys.Alt); Pause(0.3);

            vrc = new DxFormElements<VRCs>(TestSession);
            vrc.Get(VRCs.ViewRecipeCard).SendKeys(Keys.Alt + "c" + Keys.Alt); Pause(0.5);//Note: Close button closes the app
            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 38

            srb.FormElement.SendKeys(Keys.Alt + "e" + Keys.Alt); Pause(0.3);
            erc = new DxFormElements<ERCs>(TestSession);
            erc.Get(ERCs.SaveandClose).Click(); Pause(0.5);

            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 39

            srb.FormElement.SendKeys(Keys.Alt + "n" + Keys.Alt); Pause(0.3);
            erc = new DxFormElements<ERCs>(TestSession);
            erc.Get(ERCs.Cancel).Click(); Pause(1.5);

            DxFormElements<SCs> sc = new DxFormElements<SCs>(TestSession);
            sc.Get(SCs.DontSave).Click(); Pause(1);

            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 40

            srb.FormElement.SendKeys(Keys.Alt + "g" + Keys.Alt); Pause(0.3);
            egl = new DxFormElements<EGLs>(TestSession);
            egl.Get(EGLs.SaveAndClose).Click(); Pause(0.5);

            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 41            
            srb.FormElement.SendKeys(Keys.Alt + "a" + Keys.Alt); Pause(0.3);
            sifgl = new DxFormElements<SIFGLs>(TestSession);
            sifgl.Get(SIFGLs.Cancel).Click(); Pause(0.5);

            srb = new DxFormElements<SRBs>(TestSession);
            LogBreadcrumbOnErecipeBox(srb); // 42
            srb.FormElement.SendKeys(Keys.Alt + "f" + Keys.Alt); Pause(0.3);
            srb.FormElement.SendKeys(Keys.Alt + "l" + Keys.Alt); Pause(0.3);
            srb.FormElement.SendKeys(Keys.Alt + "i" + Keys.Alt); Pause(0.3);
            srb.FormElement.SendKeys(Keys.Alt + "t" + Keys.Alt); Pause(0.3);
            LogBreadcrumbOnErecipeBox(srb); // 43
            //close the session to close the app's log files
            StopTestSession(); Pause(.5);
            string errorMsg;
            bool success = CompareRecipeBoxDataLogWithExpectedResults(out errorMsg);

            WindowsDriverUtils.Log.WriteLine($"Test data compare results: {success}");
            WindowsDriverUtils.Log.WriteLine($"End run {nameof(TestCase01)}");

            if (!success)
                throw new Exception($"Test failed. DataLog compare against expected results failed. {errorMsg}");
        }

        //#TODO refactor
        public static void LogBreadcrumbOnErecipeBox(DxFormElements<SRBs> srb)
        {
            srb.FormElement.SendKeys(Keys.Control + Keys.Alt + Keys.Shift + "l" + Keys.Control + Keys.Alt + Keys.Shift);
        }

        //#TODO refactor
        public bool CompareRecipeBoxDataLogWithExpectedResults(out string errorMsg)
        {
            errorMsg = null;
            string testResultsFile = TestSession.TestDataDirectory + "\\FunctionalTestSearchRecipeBoxExpectedDataResults.txt";
            List<string> files = new List<string>(Directory.GetFiles($"{eRecipeBoxLogFolder}", "DataStore*.log"));
            files.Sort();
            string expectedTestResultsFile = files[files.Count - 1]; //most recent data log file
            WindowsDriverUtils.Log.WriteLine($"{testResultsFile} {expectedTestResultsFile}");

            bool areEqual = System.IO.File.ReadLines(testResultsFile).SequenceEqual(
                            System.IO.File.ReadLines(expectedTestResultsFile));

            if (areEqual)
                return true;

            //find first mismatch
            StreamReader testRunLogReader = new StreamReader(testResultsFile);
            StreamReader expectedLogReader = new StreamReader(expectedTestResultsFile);
            int lineNumber = 1;
            while (true)
            {
                if (testRunLogReader.EndOfStream && expectedLogReader.EndOfStream)
                    break;
                if (testRunLogReader.EndOfStream)
                {
                    errorMsg = $"{testResultsFile} end of file";
                    break;
                }
                if (expectedLogReader.EndOfStream)
                {
                    errorMsg = $"{expectedTestResultsFile} end of file";
                    break;
                }
                string logFileLine = testRunLogReader.ReadLine();
                string expectedLogLine = expectedLogReader.ReadLine();
                lineNumber++;
                int diffIndex = FindFirstDifferenceIndex(logFileLine, expectedLogLine);
                if (diffIndex >= 0)
                {
                    errorMsg = $"Lines {lineNumber} differ at index: {diffIndex}";
                    break;
                }
            }
            return areEqual;
        }
        public static int FindFirstDifferenceIndex(string str1, string str2)
        {
            int minLength = Math.Min(str1.Length, str2.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (str1[i] != str2[i])
                {
                    return i;
                }
            }

            // If one string is a prefix of the other
            if (str1.Length != str2.Length)
            {
                return minLength;
            }

            // If strings are identical
            return -1;
        }

        private string GetTitleOfFocusedRow(DxGridViewElement<SRBs> searchResults)
        {
            try
            {
                searchResults.Element.SendKeys(Keys.Home + " " + Keys.Control + "a" + Keys.Control);
                searchResults.Element.SendKeys(Keys.Control + "c" + Keys.Control); Pause(1);
                TextReader reader = new StringReader(System.Windows.Forms.Clipboard.GetText());
                string[] cols = reader.ReadLine().Split('\t');
                //2 conditions... 
                if (cols.Length == 1 && cols[0] != "Title")
                    //#1 just the Title cell text is copied 
                    return cols[0];
                else if (cols.Length > 0 && cols[0] == "Title")
                {
                    //#2 col hdrs and the entire row
                    //Title TotalTime(min) Rating(Count)  My Rating   Flagged Last    Source
                    //Naked Spinach Lasagna Quinoa Casserole  70  4.80(5)        Checked 7 / 28 / 2023
                    string[] vals = reader.ReadLine().Split('\t');
                    if (vals.Length > 0)
                        return vals[0];
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            StartTestSession();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            StopTestSession();
        }

    }
}
