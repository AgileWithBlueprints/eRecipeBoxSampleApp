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
using System;
using Foundation;
namespace RecipeBoxSolutionModel.SQL
{
    public class RecipeCardSQL
    {
        static public string GenerateConstraintAndIndexesSQL(string dataStoreProvider = "SQLSERVER")
        {            
            string uniqueTitleConstraintName = HeadBusinessObject.GenerateUniqueConstraintName("RecipeCard", "Title");
            string uniqueCookedDateConstraintName = HeadBusinessObject.GenerateUniqueConstraintName("RecipeCard", "CookedDishDate");
            string cmd;

            //#TODO this pukes due to soft deletes.  ask devexpress
            //            string todo = @"
            //ALTER TABLE dbo.Ingredient DROP CONSTRAINT 
            //if exists[CK_unique_Ingredient_RecipeCard_SortOrder];

            //#TODO ENHANCEMENT  this belongs on the server for 3-tier. 

            //ALTER TABLE dbo.Ingredient
            //ADD CONSTRAINT [CK_unique_Ingredient_RecipeCard_SortOrder]
            //UNIQUE([RecipeCard],[SortOrder], [GCRecord]);
            //";
            switch (dataStoreProvider)
            {
                case "SQLSERVER":
                    //Unique title within each recipebox 
                    //Index on ingredient RecipeCard,sortOrder for fast retrieval            
                    cmd = $@"
                    
IF NOT EXISTS (SELECT * FROM sys.objects 
               WHERE object_id = OBJECT_ID(N'[dbo].[{uniqueTitleConstraintName}]') 
               AND type in (N'UQ', N'PK'))
BEGIN
    ALTER TABLE dbo.RecipeCard
    ADD CONSTRAINT {uniqueTitleConstraintName} UNIQUE (recipebox, title, gcrecord)
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='iRecipeCardIngredientSortOrder' AND object_id = OBJECT_ID('ingredient'))
BEGIN
    CREATE NONCLUSTERED INDEX [iRecipeCardIngredientSortOrder] ON [ingredient] ([RecipeCard],[SortOrder])
END

IF NOT EXISTS (SELECT * FROM sys.objects 
               WHERE object_id = OBJECT_ID(N'[dbo].[{uniqueCookedDateConstraintName}]') 
               AND type in (N'UQ', N'PK'))
BEGIN
    ALTER TABLE dbo.CookedDish
    ADD CONSTRAINT {uniqueCookedDateConstraintName} UNIQUE (recipecard, CookedDate, gcrecord)
END

                    ";
                    return cmd;

                case "SQLITE":
                    cmd = $@"
PRAGMA foreign_keys=off;

BEGIN TRANSACTION;

DROP TABLE IF EXISTS RecipeCardBeforeConstraints;
ALTER TABLE RecipeCard RENAME TO RecipeCardBeforeConstraints;

CREATE TABLE [RecipeCard] ([OID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, [RecipeBox] int, [CreatedDate] datetime NOT NULL, [Title] text NOT NULL, [Description] text, [SourceURL] text, [SourceIndividualFirstName] nvarchar(250), [SourceIndividualLastName] nvarchar(250), [PrepTime] int, [CookTime] int, [TotalTime] int, [Yield] nvarchar(200), [Rating] money, [RatingCount] int, [MyRating] int, [Instructions] text, [Notes] text, [WouldLikeToTryFlag] bit NOT NULL DEFAULT (0), [OptimisticLockField] int, [GCRecord] int,
CONSTRAINT {uniqueTitleConstraintName} UNIQUE (recipebox, title)
);
INSERT INTO RecipeCard SELECT * FROM RecipeCardBeforeConstraints;
DROP TABLE IF EXISTS RecipeCardBeforeConstraints;
COMMIT;

PRAGMA foreign_keys=on;

                    ";
                    return cmd;
                case "NPGSQL":                    
                    //#TRICKY ANSI SQL (PostgreSql) treats null as unknowns so using SQL Server's nullable GCRecord constraint doesn't work.
                    cmd = $@"
DROP INDEX IF EXISTS ""{uniqueTitleConstraintName}"";
CREATE UNIQUE INDEX IF NOT EXISTS ""{uniqueTitleConstraintName}""
ON public.""RecipeCard"" (""RecipeBox"", ""Title"")
WHERE ""GCRecord"" IS  NULL;


DROP INDEX IF EXISTS ""{uniqueCookedDateConstraintName}"";
CREATE UNIQUE INDEX IF NOT EXISTS ""{uniqueCookedDateConstraintName}""
ON public.""CookedDish"" (""RecipeCard"", ""CookedDate"")
WHERE ""GCRecord"" IS  NULL;

CREATE INDEX IF NOT EXISTS ""iRecipeCardIngredientSortOrder"" ON public.""Ingredient"" (""RecipeCard"",""SortOrder"");

                    ";
                    return cmd; 
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
