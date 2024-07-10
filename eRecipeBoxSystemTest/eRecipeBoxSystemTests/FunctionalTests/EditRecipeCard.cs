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
using static eRecipeBoxSystemTests.eRecipeBoxSession;
namespace eRecipeBoxSystemTests.FunctionalTests
{
    public class EditRecipeCard
    {
        //dependencies..SysTest01 test data
        [TestMethod]
        public void TestCase01()
        {
            //This automated test script is left to be implemented as an exercise for the reader.
            //Follow the same pattern as SearchRecipeBoxTestCase01 -
            //Find actual instances in the SysTest01 dataset to implement the test design conditions
            //specified in the "Comprehensive EditRecipeCard Test Case Design"

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
