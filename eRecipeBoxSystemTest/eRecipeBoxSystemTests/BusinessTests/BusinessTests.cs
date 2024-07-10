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
using System.Diagnostics;
using System.IO;
using System.Linq;
using static DxWinAppDriverUtils.DxElementPrimitives;
using static eRecipeBoxSystemTests.eRecipeBoxSession;
using static eRecipeBoxSystemTests.eRecipeBoxTestUtils;
using static eRecipeBoxSystemTests.eRecipeBoxWindowElements;

namespace eRecipeBoxSystemTests.BusinessTests
{
    [TestClass]
    public class BusinessTests
    {
        //#TODO ask app for its provider, skip file check if postgres.  
        //dependencies..Data: SysTest01 
        [TestMethod]
        public void TestCase01()
        {
            DateTime today = DateTime.Today;
            int dayOfWeekIdx = (int)today.DayOfWeek;
            DateTime pastSunday = today.AddDays(-dayOfWeekIdx);
            DateTime nextMonday = pastSunday.AddDays(8);
            DateTime nextTuesday = nextMonday.AddDays(1);

            WindowsDriverUtils.Log.WriteLine($"Start run {nameof(TestCase01)}");
            WinElement.ClearTimings();
            //#BusTestCase01-0001 login using cached creds (thus no Login form)

            DxFormElements<SRBs> srb = new DxFormElements<SRBs>(TestSession);

            //trigger easter egg to verify and calibrate system test data
            srb.FormElement.SendKeys(Keys.Control + Keys.Alt + Keys.Shift + "t" + Keys.Control + Keys.Alt + Keys.Shift); Pause(1);
            srb.Get(SRBs.Refresh).Click(); Pause(1);

            //trigger easter egg to send next monday as the anchor date for logging cooked dates while eRB runs the test
            srb.Get(SRBs.filterTermLookUpEdit).SendKeys(nextMonday.Date.ToString()); Pause(1);
            srb.FormElement.SendKeys(Keys.Control + Keys.Alt + Keys.Shift + "a" + Keys.Control + Keys.Alt + Keys.Shift); Pause(1);
            srb.Get(SRBs.clearFilterSimpleButton).Click(); Pause(1);


            srb = new DxFormElements<SRBs>(TestSession);
            WinElement srbWebElement = srb.Get(SRBs.SearchRecipeBox);

            //---------------------------
            //#BusTestCase01-0002 review meals from this and last week
            srb.Get(SRBs.MonthView).Click();

            DxSchedularElement<SRBs> scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);

            //scroll up to see last week            
            scheduler.ScrollFirstDayOfCalendar(pastSunday);
            scheduler.GetCalendarLineUpButton().Click();

            //# TODO test only
            //var xxrcard = scheduler.GetAppointment("Sweet Potato Bowl").WinElement;

            //#BusTestCase01-0003 filter on keyword
            FilterRecipeBox(srb, "summer");

            DxGridViewElement<SRBs> searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);

            //sort on rating
            //#BusTestCase01-0004 sort to get the highest rated and review
            WinElement rating = searchResults.GetColumnHeader(SRBs.RatingCount);
            rating.Click(); Pause(.2);
            rating.Click(); Pause(.2);

            //copy to clipboard
            WinElement row1Title = searchResults.GetCell(1, SRBs.Title);
            row1Title.Click(); Pause(.2);
            Assert.AreEqual(row1Title.Text, "Avocado Gazpacho");

            searchResults.Element.SendKeys(Keys.Control + "c" + Keys.Control);

            scheduler.SelectDateOnCalendar(nextMonday);
            //#BusTestCase01-0005 schedule a RC
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control);

            //#BusTestCase01-0006 clear filter
            srb.Get(SRBs.clearFilterSimpleButton).Click();

            //filter on Potato   
            //#BusTestCase01-0007 filter on item
            FilterRecipeBox(srb, "potato   ");

            //add one pan(pot) filter
            //#BusTestCase01-0008 append filter
            FilterRecipeBox(srb, "one pan" + Keys.Tab);

            //sort on my rating
            //#BusTestCase01-0009 review filter results 
            WinElement myRating = searchResults.GetColumnHeader(SRBs.MyRating);
            myRating.Click(); Pause(2);
            myRating.Click(); Pause(2);

            WinElement row6Title = searchResults.GetCell(6, SRBs.Title);
            Assert.AreEqual(row6Title.Text, "Kelly’s Butternut Squash Soup");

            row6Title.Click(); Pause(.5); //Set focus to the cell
            row6Title.Click(); Pause(.5); //Click again to force titleHyperLinkEdit child
            WinElement titleHyperlink = FindElementByName(row6Title, nameof(SRBs.titleHyperLinkEdit));
            string url = WindowsDriverUtils.GetURLFromHyperTextEdit(titleHyperlink);
            if (!string.IsNullOrEmpty(url))
            {
                //#BusTestCase01-0010 view RC in browser
                Process.Start(url); Pause(4);
            }
            srbWebElement.Click(); Pause(1);

            //#BusTestCase01-0011 find RC that is flagged
            WinElement row6Flagged = searchResults.GetCell(6, SRBs.Flagged);
            row6Flagged.Click(); Pause(); //set focus to Flagged
            //#BusTestCase01-0012 unflag it in the grid
            row6Flagged.Click(); Pause(); //toggle the flag

            //edit Kelly’s Butternut Squash Soup
            srb.Get(SRBs.EditRecipe).Click(); Pause(1.0);  //allow enough time for form to open

            //---------------------------
            DxFormElements<ERCs> erc = new DxFormElements<ERCs>(TestSession);
            DxTabControlElement<ERCs> recipeCardTabControlElement = erc.FindTabElement(ERCs.recipeCardTabControl);

            //#BusTestCase01-0013 edit RC, update fields found in browser
            erc.Get(ERCs.ratingTextEdit).SendKeys("9.5");
            erc.Get(ERCs.yieldTextEdit).SendKeys("6");

            recipeCardTabControlElement.GetTabPage(ERCs.Instructions).Click(); Pause(.8);
            erc.Get(ERCs.Cancel).Click(); Pause();

            DxFormElements<SCs> sc = new DxFormElements<SCs>(TestSession);
            sc.Get(SCs.Save).Click(); Pause(1);

            //---------------------------
            srb = new DxFormElements<SRBs>(TestSession);
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);
            //copy the RC to clipboard            
            searchResults.Element.SendKeys(Keys.Control + "c" + Keys.Control); Pause();

            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);

            scheduler.SelectDateOnCalendar(nextTuesday);
            //#BusTestCase01-0014 schedule the RC
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control); Pause(1.5);

            //---------------------------
            srb = new DxFormElements<SRBs>(TestSession);
            srbWebElement = srb.Get(SRBs.SearchRecipeBox);

            //#BusTestCase01-0015 clear filter
            srb.Get(SRBs.clearFilterSimpleButton).Click();

            //#BusTestCase01-0016 filter again on 2 terms
            FilterRecipeBox(srb, "shrimp");
            FilterRecipeBox(srb, "med" + Keys.Tab);

            //---------------------------
            srb.Get(SRBs.NewRecipe).Click(); Pause(0.5);

            erc = new DxFormElements<ERCs>(TestSession);
            WinElement ercElement = erc.Get(ERCs.EditRecipeCard);

            //---------------------------

            string shrimpRCUrl = @"https://www.themediterraneandish.com/shrimp-fra-diavolo";
            //#BusTestCase01-0017 search web and find one
            Process.Start(shrimpRCUrl); Pause(4);

            //---------------------------

            ercElement.Click(); Pause(1);

            WindowsDriverUtils.LoadFileToClipboard(TestSession.TestDataDirectory + "\\ShrimpRecipe.txt");
            erc.Get(ERCs.ImportRecipe).Click(); Pause(1);

            DxFormElements<IRCPs> ircp = new DxFormElements<IRCPs>(TestSession);
            ircp.FormElement.FindElementWithNameContaining("Import Text").Click(); Pause(1);

            DxFormElements<IRCs> irc = new DxFormElements<IRCs>(TestSession);
            irc.Get(IRCs.memoEdit1).SendKeys(Keys.Control + "a" + Keys.Control); Pause();
            irc.Get(IRCs.memoEdit1).SendKeys(Keys.Control + "v" + Keys.Control); Pause(0.5);

            irc.Get(IRCs.Parse).Click(); Pause();
            //#BusTestCase01-0018 import manually from website
            irc.Get(IRCs.Import).Click(); Pause(1.0);

            //---------------------------
            erc = new DxFormElements<ERCs>(TestSession);
            WinElement igv234 = erc.Get(ERCs.ingredientsGridView);
            DxGridViewElement<ERCs> ingredients = erc.FindGridViewElement(ERCs.ingredientsGridView);
            erc.Get(ERCs.Save).Click(); Pause();

            //#BusTestCase01-0019 complete all  Items that were not autopopulated
            ingredients.GetCell(3, ERCs.Item).Click();
            ingredients.Element.SendKeys("red " + Keys.ArrowDown + Keys.ArrowDown + Keys.ArrowDown + Keys.ArrowDown + Keys.ArrowDown + Keys.Enter + Keys.Tab); Pause();
            ingredients.GetCell(6, ERCs.Item).Click();
            ingredients.Element.SendKeys("garlic" + Keys.Tab); Pause();
            ingredients.GetCell(8, ERCs.Item).Click();
            ingredients.Element.SendKeys("roa" + Keys.ArrowDown + Keys.Enter + Keys.Tab); Pause();
            ingredients.GetCell(9, ERCs.Item).Click();
            ingredients.Element.SendKeys("Toma" + Keys.Tab + Keys.Tab); Pause();
            ingredients.GetCell(11, ERCs.Item).Click();
            ingredients.Element.SendKeys("Ore" + Keys.Tab + Keys.Tab); Pause(1.0);

            recipeCardTabControlElement = erc.FindTabElement(ERCs.recipeCardTabControl);
            recipeCardTabControlElement.GetTabPage(ERCs.Keywords).Click(); Pause();

            //#BusTestCase01-0020 add some keywords
            erc.Get(ERCs.keywordLookUpEdit).SendKeys("it" + Keys.Tab); Pause(1.0);
            erc.Get(ERCs.addKeywordSimpleButton).Click();
            erc.Get(ERCs.keywordLookUpEdit).SendKeys("me" + Keys.Tab); Pause(1.0);
            erc.Get(ERCs.addKeywordSimpleButton).Click();

            //#BusTestCase01-0021 view RC in cook mode
            erc.Get(ERCs.ViewRecipe).Click(); Pause(0.5);

            //#N
            //sc = new DxFormElements<SCs>(TestSession);
            //sc.Get(SCs.Save).Click();

            //---------------------------
            DxFormElements<VRCs> vrc = new DxFormElements<VRCs>(TestSession);
            Pause(1.0);

            //#BusTestCase01-0022 return to edit RC
            vrc.Get(VRCs.EditRecipe).Click(); Pause();

            //---------------------------
            erc = new DxFormElements<ERCs>(TestSession);
            ercElement = erc.Get(ERCs.EditRecipeCard);
            ingredients = erc.FindGridViewElement(ERCs.ingredientsGridView);

            ingredients.GetCell(1, ERCs.Qty).Click();
            erc.Get(ERCs.NewRow).Click();

            //#BusTestCase01-0023 attempt to add new section, with error
            erc.Get(ERCs.Save).Click();

            DxFormElements<FEs> fe = new DxFormElements<FEs>(TestSession);
            fe.Get(FEs.OK).Click();

            //#BusTestCase01-0024 fix error - add section
            ingredients.GetCell(1, ERCs.ItemDescription).Click();
            ingredients.Element.SendKeys("Pan Fry:"); Pause();
            erc.Get(ERCs.Save).Click();

            ingredients.GetCell(5, ERCs.ItemDescription).Click();
            erc.Get(ERCs.NewRow).Click();
            //#BusTestCase01-0025 add another section header
            ingredients.Element.SendKeys("Saute and Simmer:"); Pause();

            //#BusTestCase01-0026 attempt to cancel, but ask to save change. Cancel so I can do more editing.
            erc.Get(ERCs.Cancel).Click(); Pause();

            sc = new DxFormElements<SCs>(TestSession);
            sc.Get(SCs.Cancel).Click(); Pause(1);

            ingredients.GetCell(14, ERCs.ItemDescription).Click(); Pause();
            //#BusTestCase01-0027 append item description in the grid
            ercElement.SendKeys(", optional"); Pause();

            //#BusTestCase01-0028 save
            erc.Get(ERCs.Save).Click();

            //#BusTestCase01-0029 view RC in cook mode, notice changes
            erc.Get(ERCs.ViewRecipe).Click(); Pause(1);

            //---------------------------
            vrc = new DxFormElements<VRCs>(TestSession);
            Pause(1.0);
            //#BusTestCase01-0030 close view RC form
            vrc.Get(VRCs.ViewRecipeCard).SendKeys(Keys.Alt + "c" + Keys.Alt); Pause(1);//Note: Close button closes the app

            //---------------------------
            srb = new DxFormElements<SRBs>(TestSession);
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);

            searchResults.Element.SendKeys(Keys.Control + "c" + Keys.Control);

            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);

            DateTime nextWednesday = nextTuesday.AddDays(1);
            scheduler.SelectDateOnCalendar(nextWednesday);
            //#BusTestCase01-0031 select new RC and schedule on the next day
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control);

            //---------------------------            
            srb.Get(SRBs.clearFilterSimpleButton).Click();

            //#BusTestCase01-0032 filter again using multiple filters - keyword and ingredient
            FilterRecipeBox(srb, "Sou" + Keys.Tab);

            FilterRecipeBox(srb, "bl" + Keys.Tab);

            //#BusTestCase01-0033 results are not what  I want 
            srb.Get(SRBs.NewRecipe).Click(); Pause(0.5);

            //---------------------------
            erc = new DxFormElements<ERCs>(TestSession);
            ercElement = erc.Get(ERCs.EditRecipeCard);

            //---------------------------
            string beezieCUrl = @"https://www.allrecipes.com/recipe/25691/beezies-black-bean-soup/";
            //#BusTestCase01-0034 search allrecipes.com and find one
            Process.Start(beezieCUrl); Pause(4);

            //---------------------------

            erc.Get(ERCs.ImportRecipe).Click(); Pause(1);

            ircp = new DxFormElements<IRCPs>(TestSession);
            ircp.FormElement.FindElementWithNameContaining("Import From AllRecipes").Click(); Pause(0.5);

            DxFormElements<IARs> iar = new DxFormElements<IARs>(TestSession);
            //#BusTestCase01-0035 import from allrecipes with invalid URL
            iar.Get(IARs.textEdit).SendKeys("testInvalidURL"); Pause(0.5);
            iar.Get(IARs.OK).Click(); Pause(0.5);

            DxFormElements<IAREs> iare = new DxFormElements<IAREs>(TestSession);
            iare.Get(IAREs.OK).Click(); Pause(0.5);

            WindowsDriverUtils.LoadFileToClipboard(TestSession.TestDataDirectory + "\\BeezieURL.txt");

            //#BusTestCase01-0036 import from allrecipes 
            erc.Get(ERCs.ImportRecipe).Click(); Pause(1);

            ircp = new DxFormElements<IRCPs>(TestSession);
            ircp.FormElement.FindElementWithNameContaining("Import From AllRecipes").Click(); Pause(0.5);

            iar = new DxFormElements<IARs>(TestSession);
            iar.Get(IARs.textEdit).SendKeys(Keys.Control + "v" + Keys.Control); Pause(.3);
            iar.Get(IARs.OK).Click(); Pause(0.5);

            erc = new DxFormElements<ERCs>(TestSession);
            ingredients = erc.FindGridViewElement(ERCs.ingredientsGridView);

            //#BusTestCase01-0037 set items not autofilled
            ingredients.GetCell(10, ERCs.Item).Click();
            ingredients.Element.SendKeys("di" + Keys.Tab + Keys.Tab); Pause();

            ingredients.GetCell(13, ERCs.Item).Click();
            ingredients.Element.SendKeys("ore" + Keys.ArrowDown + Keys.ArrowDown + Keys.Enter + Keys.Tab); Pause();

            //#BusTestCase01-0038 add new item not previously in our curated list
            ingredients.GetCell(15, ERCs.Item).Click();
            ingredients.Element.SendKeys("red wine vinegar " + Keys.Enter); Pause(1);

            DxFormElements<Cnfms> cnfm = new DxFormElements<Cnfms>(TestSession);
            cnfm.Get(Cnfms.Yes).Click(); Pause(1);

            //#BusTestCase01-0039 show how item description is autofilled from Item
            WinElement Row17ItemDesc = ingredients.GetCell(17, ERCs.ItemDescription);
            Row17ItemDesc.Click(); Pause(.2);
            Row17ItemDesc.SendKeys(Keys.Shift + Keys.Home + Keys.Shift); Pause(.2);
            Row17ItemDesc.SendKeys(Keys.Delete + Keys.Enter); Pause(.2);
            Row17ItemDesc.SendKeys(Keys.Tab); Pause(.2);
            ingredients.Element.SendKeys("bas" + Keys.ArrowDown + Keys.ArrowDown + Keys.Enter); Pause(1);
            erc.Get(ERCs.Save).Click(); Pause();

            recipeCardTabControlElement = erc.FindTabElement(ERCs.recipeCardTabControl);
            recipeCardTabControlElement.GetTabPage(ERCs.Keywords).Click(); Pause();

            //#BusTestCase01-0040 add keywords
            erc.Get(ERCs.keywordLookUpEdit).SendKeys("sou" + Keys.Tab);
            erc.Get(ERCs.addKeywordSimpleButton).Click();

            erc.Get(ERCs.keywordLookUpEdit).SendKeys("ve" + Keys.Tab);
            erc.Get(ERCs.addKeywordSimpleButton).Click();

            erc.Get(ERCs.keywordLookUpEdit).SendKeys("veg" + Keys.ArrowDown + Keys.ArrowDown + Keys.ArrowDown + Keys.ArrowDown + Keys.Enter + Keys.Tab);
            erc.Get(ERCs.addKeywordSimpleButton).Click();

            var keywordElement = erc.Get(ERCs.keywordLookUpEdit);
            keywordElement.Click(); Pause(1);
            //#BusTestCase01-0041 add new keyword not previously in our curated list
            erc.Get(ERCs.keywordLookUpEdit).SendKeys("hearty " + Keys.Tab); Pause(1);
            DxFormElements<CNKs> cNKs = new DxFormElements<CNKs>(TestSession);
            cNKs.Get(CNKs.Yes).Click(); Pause(1);

            //#BusTestCase01-0042 save newly imported RC
            erc.Get(ERCs.SaveandClose).Click(); Pause(1);

            //---------------------------
            srb = new DxFormElements<SRBs>(TestSession);
            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);

            searchResults.Element.SendKeys(Keys.Control + "c" + Keys.Control);

            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);

            DateTime nextThursday = nextWednesday.AddDays(1);
            DateTime nextFriday = nextThursday.AddDays(1);
            scheduler.SelectDateOnCalendar(nextThursday);
            //#BusTestCase01-0043 schedule new RC
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control); Pause(1.5);


            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);

            //#BusTestCase01-0044 reschedule multiple RCs at same time using calendar                        
            Win32API.LockShiftKey(true);
            WinElement rcard = null;
            try
            {
                rcard = scheduler.GetAppointment("Avocado Gazpacho", nextMonday).WinElement;
                rcard.Click(); Pause(.5);
                rcard = scheduler.GetAppointment("Beezie's Black Bean Soup", nextThursday).WinElement;
                rcard.Click(); Pause(.5);
            }
            finally
            {
                Win32API.LockShiftKey(false);
            }
            Pause(1);

            scheduler.Element.SendKeys(Keys.Control + "x" + Keys.Control); Pause();
            scheduler.SelectDateOnCalendar(nextTuesday); Pause();
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control); Pause(1);

            //#BusTestCase01-0045 clear filter
            srb.Get(SRBs.clearFilterSimpleButton).Click(); Pause(1);

            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView); Pause(1);
            searchResults.SetNumericColumnFilter(SRBs.MyRating, TestSession.WinAppDrvrSession, "9");

            myRating = searchResults.GetColumnHeader(SRBs.MyRating);
            myRating.Click(); Pause(.5);


            row1Title = searchResults.GetCell(1, SRBs.Title);
            row1Title.Click(); Pause();
            Assert.AreEqual(row1Title.Text, "The Best Thai Coconut Soup");


            //#BusTestCase01-0047 hide calendar to see more rows in grid 
            srb.Get(SRBs.HideCalendar).Click(); Pause(1);

            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);

            WinElement row1Flagged = searchResults.GetCell(1, SRBs.Flagged);
            //#BusTestCase01-0048 select favorite RC that is flagged
            row1Flagged.Click();
            //#BusTestCase01-0049 clear flag in results grid
            row1Flagged.Click(); //toggle off

            //toggle calendar.. showing this time
            //#BusTestCase01-0050 show calendar again
            srb.Get(SRBs.HideCalendar).Click(); Pause(1);

            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);

            searchResults.Element.SendKeys(Keys.Control + "c" + Keys.Control);

            DateTime nextSaturday = nextFriday.AddDays(1);
            scheduler.SelectDateOnCalendar(nextSaturday); Pause();
            //#BusTestCase01-0051 schedule the favorite RC
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control); Pause(.5);

            //#BusTestCase01-0052 unschedule a RC from the calendar
            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            rcard = scheduler.GetAppointment("Sweet Potato Bowl").WinElement;
            rcard.Click();
            rcard.SendKeys(Keys.Delete); Pause(2);

            //#BusTestCase01-0053 edit GL
            srb.Get(SRBs.GroceryList).Click(); Pause(1);

            //-----------------
            DxFormElements<EGLs> egl = new DxFormElements<EGLs>(TestSession);
            WinElement gListGridViewasdfWebEl = egl.Get(EGLs.groceryListGridView);
            DxGridViewElement<EGLs> gListElement = egl.FindGridViewElement(EGLs.groceryListGridView);

            gListElement.Element.Click(); Pause();
            gListElement.Element.SendKeys(Keys.Control + "a" + Keys.Control); Pause();

            //#BusTestCase01-0054 clear all items on GL except 1
            WinElement row20Item = gListElement.GetCell(20, EGLs.Item);
            Assert.AreEqual(row20Item.Text, "Mayonnaise");
            row20Item.Click(); Pause();
            row20Item.SendKeys(Keys.Shift + Keys.Tab + Keys.Shift); Pause();
            gListElement.Element.SendKeys(Keys.Space); Pause(); //deselect
            gListElement.Element.SendKeys(Keys.Delete); Pause(); //delete all but mayo

            //#BusTestCase01-0055 close and save GL
            egl.Get(EGLs.SaveAndClose).Click(); Pause();

            //-----------------
            srb = new DxFormElements<SRBs>(TestSession);
            srb.Get(SRBs.clearFilterSimpleButton).Click(); Pause(1);
            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            scheduler.Element.Click(); Pause(1);
            scheduler.Element.SendKeys(Keys.ArrowRight); Pause(); //#TRICKY arrow right to deselect all appts

            //#TODO delete this 
            //scheduler.GetCalendarPreviousButton().Click(); Pause(1);
            //scheduler.GetCalendarNextButton().Click(); Pause(1);

            //#BusTestCase01-0056 select all newly scheduled RCs 
            Pause(1);//give time to paint            
            Win32API.LockShiftKey(true);
            try
            {
                rcard = scheduler.GetAppointment("Avocado Gazpacho", nextTuesday).WinElement;
                rcard.Click(); Pause();
                rcard = scheduler.GetAppointment("The Best Thai Coconut Soup", nextSaturday).WinElement;
                rcard.Click(); Pause();
            }
            finally
            {
                Win32API.LockShiftKey(false);
            }
            Pause(1);

            //#BusTestCase01-0057 select all their ingredients to add to GL 
            srb.Get(SRBs.AddtoGroceryList).Click();

            //-----------------
            DxFormElements<SIFGLs> sifgl = new DxFormElements<SIFGLs>(TestSession);
            WinElement sifgasdflGVWebEl = sifgl.Get(SIFGLs.selectIngrGridView);
            DxGridViewElement<SIFGLs> selectIng = sifgl.FindGridViewElement(SIFGLs.selectIngrGridView);

            //#BusTestCase01-0058 remove ingredients that we already have at home, mostly spices
            List<string> items = new List<string>();
            StreamReader inf = new StreamReader(TestSession.TestDataDirectory + "\\IngredientsToDelete01.txt");
            while (!inf.EndOfStream)
                items.Add(inf.ReadLine());
            inf.Close();
            selectIng.DeleteRows(SIFGLs.Item, items); Pause();

            //#BusTestCase01-0059 then add remaining ingredients to GL
            sifgl.Get(SIFGLs.AddItemstoGroceryList).Click(); Pause(1);

            //-----------------           
            egl = new DxFormElements<EGLs>(TestSession);
            //#BusTestCase01-0060 save and close GL
            egl.Get(EGLs.SaveAndClose).Click(); Pause(1);

            //-----------------
            srb = new DxFormElements<SRBs>(TestSession);
            //switch to week view
            srb.Get(SRBs.MonthView).Click();
            srb.Get(SRBs.clearFilterSimpleButton).Click(); Pause(1);

            //#BusTestCase01-0061 filter side dish and other terms
            FilterRecipeBox(srb, "si" + Keys.Tab);
            FilterRecipeBox(srb, "easy");
            srb.FormElement.SendKeys(Keys.Alt + "i" + Keys.Alt);
            srb.Get(SRBs.filterTermLookUpEdit).SendKeys("vegetable  " + Keys.Enter); Pause();

            searchResults = srb.FindGridViewElement(SRBs.searchResultsGridView);

            //#BusTestCase01-0062 select side dish RC and add it as a 2nd dish for a meal
            WinElement row3Title = searchResults.GetCell(3, SRBs.Title); Pause();
            Assert.IsTrue(row3Title.Text.Contains("Baked Acorn Squash"));
            row3Title.Click(); Pause();
            searchResults.Element.SendKeys(Keys.Control + "c" + Keys.Control); Pause(1);

            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator); Pause(1);
            scheduler.Element.Click(); Pause(1);

            //#WORKAROUND force devx to populate appointment window elements
            //scheduler.GetCalendarPreviousButton().Click(); Pause(1);
            //scheduler.GetCalendarNextButton().Click(); Pause(1);
            //scheduler.Element.SendKeys(Keys.ArrowRight);//#TRICKY arrow right to deselect all appts

            scheduler.ScrollFirstDayOfCalendar(pastSunday);
            scheduler.SelectDateOnCalendar(nextFriday);
            scheduler.Element.SendKeys(Keys.Control + "v" + Keys.Control);

            scheduler = srb.FindSchedularElement(SRBs.schedulerControl, SRBs.dateNavigator);
            //#BusTestCase01-0063 ..then add its ingredients to the GL
            rcard = scheduler.GetAppointment("Baked Acorn Squash", nextFriday).WinElement;
            rcard.Click(); Pause();
            srb.Get(SRBs.AddtoGroceryList).Click(); Pause(.2);

            //-----------------
            sifgl = new DxFormElements<SIFGLs>(TestSession);
            selectIng = sifgl.FindGridViewElement(SIFGLs.selectIngrGridView);

            //#BusTestCase01-0064 , removing items we already have at home
            items = new List<string>();
            inf = new StreamReader(TestSession.TestDataDirectory + "\\IngredientsToDelete02.txt");
            while (!inf.EndOfStream)
                items.Add(inf.ReadLine());
            inf.Close();
            selectIng.DeleteRows(SIFGLs.Item, items); Pause();

            sifgl.Get(SIFGLs.AddItemstoGroceryList).Click(); Pause(.5);

            //-----------------
            egl = new DxFormElements<EGLs>(TestSession);
            gListElement = egl.FindGridViewElement(EGLs.groceryListGridView);

            //#BusTestCase01-0065 add a few staples to the GL
            egl.Get(EGLs.itemLookUpEdit).SendKeys("milk" + Keys.Enter); Pause();
            egl.Get(EGLs.itemLookUpEdit).SendKeys("coffee" + Keys.Enter); Pause();
            egl.Get(EGLs.itemLookUpEdit).SendKeys("apples" + Keys.Enter); Pause();
            egl.Get(EGLs.itemLookUpEdit).SendKeys("eggs" + Keys.Enter); Pause();

            WinElement row11Item = gListElement.GetCell(11, EGLs.Item);
            row11Item.Click(); Pause();
            Assert.AreEqual(row11Item.Text, "Coffee");

            //#BusTestCase01-0066 edit qty and a few other fields in GL
            WinElement row11Qty = gListElement.GetCell(11, EGLs.Qty);
            row11Qty.Click(); Pause();
            row11Qty.SendKeys("2" + Keys.Tab); Pause();

            gListElement.Element.SendKeys("large cans" + Keys.Tab); Pause();

            WinElement row2Item = gListElement.GetCell(2, EGLs.Item);
            row2Item.Click(); Pause();
            Assert.AreEqual(row2Item.Text, "Apple");
            row2Item.SendKeys(Keys.End + " granny smith" + Keys.Tab); Pause();

            egl.Get(EGLs.Save).Click(); Pause(.1);

            //#BusTestCase01-0067 email GL
            egl.Get(EGLs.Email).Click(); Pause(4);

            //#BusTestCase01-0068 Close GL
            egl.Get(EGLs.SaveAndClose).Click(); Pause(2);

            //-----------------
            srb = new DxFormElements<SRBs>(TestSession);
            srb.Get(SRBs.clearFilterSimpleButton).Click(); Pause(.5);

            Pause(3);

            //close the session to close the app's log files
            //#BusTestCase01-0069 close app
            StopTestSession(); Pause(.5);

            //#BusTestCase01-0070 compare app DataLog with expected results
            bool success = CompareRecipeBoxDataLogWithExpectedResults();

            WindowsDriverUtils.Log.WriteLine($"Test data compare results: {success}");
            WindowsDriverUtils.Log.WriteLine($"End run {nameof(TestCase01)}");

            if (!success)
                throw new Exception("Test failed. DataLog compare against expected results failed.");
        }
        public bool CompareRecipeBoxDataLogWithExpectedResults()
        {
            string testResultsFile = TestSession.TestDataDirectory + "\\BusinessTestSuiteExpectedDataResults.txt";
            List<string> files = new List<string>(Directory.GetFiles($"{eRecipeBoxLogFolder}", "DataStore*.log"));
            files.Sort();
            string expectedTestResultsFile = files[files.Count - 1]; //most recent data log file
            WindowsDriverUtils.Log.WriteLine($"{testResultsFile} {expectedTestResultsFile}");

            bool areEqual = System.IO.File.ReadLines(testResultsFile).SequenceEqual(
                            System.IO.File.ReadLines(expectedTestResultsFile));
            return areEqual;
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
