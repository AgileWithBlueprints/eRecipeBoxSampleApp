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
using DataStoreUtils;
using DevExpress.Xpo;
using Foundation;
using RecipeBoxSolutionModel;
using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
namespace eRecipeBox.DataStoreServiceReference
{
    internal class AutomatedTestUtils
    {
        //#AtmtTstng #TRICKY verify and calibrate the system test database.  important because the calendar starts with 'today'
        static internal void VerifySystemTestData()
        {
            bool systemTestLogging = AppSettings.GetBoolAppSetting("SystemTestLogging");
            if (!(systemTestLogging || DataStoreServiceReference.IsSingleUserDeployment))
                throw new Exception("SystemTestLogging in eRecipeBox.exe.config must be true when testing");
            try
            {
                DateTime today = DateTime.Today;
                int dayOfWeekIdx = (int)today.DayOfWeek;
                DateTime pastSunday = today.AddDays(-dayOfWeekIdx);
                DateTime nextSunday = pastSunday.AddDays(7);
                string connString = ConnectionStrings.GetConnectionString(DataStoreServiceReference.AppConfigConnectionSetting);
                DbConnection.DbProvider dbProvider = XpoService.GetDbProvider(connString);
                using (UnitOfWork uow = new UnitOfWork())
                {
                    CookedDish lastCookedDish = uow.Query<CookedDish>().Where(x => x.RecipeCard.RecipeBox.Oid == DataStoreServiceReference.MyRecipeBoxOid).OrderByDescending(o => o.CookedDate).ToList()[0];
                    if (!(lastCookedDish.RecipeCard.Title != "Sweet Potato Bowl" || //SysTest01
                        lastCookedDish.RecipeCard.Title != "Low Fat Cheesy Spinach and Eggplant Lasagna"  //recipeTest125KRB
                        ))
                        throw new Exception("System Test data doesn't appear right. Must be SysTest01 or recipeTest125KRB.");

                    int numberOfDaysBetweenActualAndExpected = (int)(nextSunday - lastCookedDish.CookedDate).TotalDays;
                    if (numberOfDaysBetweenActualAndExpected == 0)
                        return;
                    //expect it to be on a Sunday
                    if (numberOfDaysBetweenActualAndExpected % 7 != 0)
                        throw new Exception("Sys Test data doesn't appear right.");

                    //move all cooked dates up
                    if (dbProvider == DbConnection.DbProvider.SQLSERVER)
                        uow.ExecuteNonQuery(
$@"update CookedDish set CookedDate = DATEADD(day,{numberOfDaysBetweenActualAndExpected},cookeddate) 
where RecipeCard in (select Oid from RecipeCard where RecipeBox = {DataStoreServiceReference.MyRecipeBoxOid}) ");
                    else if (dbProvider == DbConnection.DbProvider.SQLITE)
                        uow.ExecuteNonQuery($"update CookedDish set CookedDate = DATE(CookedDate, '+{numberOfDaysBetweenActualAndExpected} days');");
                    else if (dbProvider == DbConnection.DbProvider.NPGSQL)
                    {
                        //#WORKAROUND REFACTOR For some reason, Postgres fails on a constraint violation.                        
                        var cmd =
@" 
DO $$ 
DECLARE
    r record;
BEGIN
    FOR r IN ( 
SELECT idx.indexrelid::regclass::text AS iname
    FROM pg_index idx
    JOIN pg_class cls ON idx.indexrelid = cls.oid
    JOIN pg_namespace ns ON cls.relnamespace = ns.oid
    WHERE ns.nspname = 'public' and idx.indexrelid::regclass::text LIKE '""CK%'
) 
LOOP			  
        EXECUTE 'DROP INDEX IF EXISTS ' || r.iname || ';';
    END LOOP;
 
END $$;
";
                        uow.ExecuteNonQuery(cmd);

                        cmd = $"update public.\"CookedDish\" set \"CookedDate\" = \"CookedDate\" + INTERVAL '+{numberOfDaysBetweenActualAndExpected} days';";
                        uow.ExecuteNonQuery(cmd);

                        //recreate indexes
                        cmd = RecipeBoxSolutionModel.SQL.RecipeCardSQL.GenerateConstraintAndIndexesSQL("NPGSQL");
                        uow.ExecuteNonQuery(cmd);
                    }
                    else
                        throw new Exception("not implemented");
                }
            }
            catch
            {
                throw new Exception("Sys Test data doesn't appear right.");
            }
        }
    }
}
