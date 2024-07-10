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
using DevExpress.Xpo;
using Foundation;
using System;
using static Foundation.ModelVersion;
namespace RecipeBoxSolutionModel
{
    public class ModelInfo
    {
        //How to design bus objs with XPO
        //https://docs.devexpress.com/eXpressAppFramework/112600/business-model-design-orm/business-model-design-with-xpo 

        static public readonly Type[] PersistentTypes = new Type[]{
            typeof(CookedDish),
            typeof(GroceryList),
            typeof(GroceryListItem),
            typeof(Ingredient),
            typeof(Item),
            typeof(Keyword),
            typeof(ModelVersion),
            typeof(RecipeBox),
            typeof(RecipeCard),
            typeof(UserProfile),
            typeof(UserProfileClientComputer),
            typeof(UserProfileRecipeBox)
        };

        static public void RegisterModelVersions()
        {

            RegisterNewVersion(SchemaDefinition, DateTime.Parse("2023-03-23 08:15:00"), true, "Add ModelVersion to schema.");
            RegisterNewVersion(SchemaDefinition, DateTime.Parse("2023-03-23 10:01:00"), true, "Add ModelVersion to PersistentTypes.");
            RegisterNewVersion(SchemaDefinition, DateTime.Parse("2023-03-24 13:24:00"), false, "Added table/col header info to binary backup files. Prior backup files will no longer load with this logic.");
            RegisterNewVersion(SchemaDefinition, DateTime.Parse("2023-03-27 17:05:00"), false, "Ingredient.ItemDescription now nullable while refactoring adding a new Item while editRecipeCard");
            RegisterNewVersion(SchemaDefinition, DateTime.Parse("2023-03-28 14:15:00"), false, "Ingredient.ItemDescription back to not null.  Better solution.");
            RegisterNewVersion(SchemaDefinition, DateTime.Parse("2023-04-11 08:26:00"), false, "Changed RecipeBox to GL as an aggregate collection to match solution model.");
            RegisterNewVersion(SchemaDefinition, DateTime.Parse("2023-04-21 12:45:00"), false, "ClientDeviceMAC no longer unique.  Too restrictive.");
            RegisterNewVersion(SchemaDefinition, DateTime.Parse("2023-06-22 12:41:00"), false, "Remove index on RC.Description, too long.");

        }

        static public void CreateOneOfEach(UnitOfWork uow)
        {
            //#TODO #WORKAROUND #FRAGILE XPO doesnt generate DDL for SQLite. (Ticket T1111909)  Persisting an object does.  This is a workaround.
            //brute force quick and dirty
            //Create 1 of each to force DataStore tables to be created upon commit.
            UserProfile up = new UserProfile(uow); up.PersonalEmail = @"x@gmail.com"; up.FirstName = "x"; up.LastName = "x"; up.HomeZipCode = "12345"; up.PersonalCellNumber = "1234567890";
            UserProfileClientComputer upcc = new UserProfileClientComputer(uow); upcc.ClientDeviceMAC = "x"; upcc.InstalledDate = DateTime.Now; upcc.LastLoginDate = DateTime.Now; upcc.NumberOfLogins = 0; upcc.ClientSystemName = "x"; upcc.WindowsUserName = "x";
            RecipeBox rb = new RecipeBox(uow); rb.Name = "x";
            UserProfileRecipeBox uprb = new UserProfileRecipeBox(uow);
            RecipeCard rc = new RecipeCard(uow); rc.CreatedDate = DateTime.Now; rc.Title = "x"; rc.WouldLikeToTryFlag = false;
            CookedDish cd = new CookedDish(uow); cd.CookedDate = DateTime.Now;
            rc.AddCookedDish(cd);
            Keyword kw = new Keyword(uow); kw.Name = "x";
            rc.AddKeyword(kw);
            Ingredient ing = new Ingredient(uow); ing.SortOrder = 1; ing.ItemDescription = "x";
            rc.AddIngredient(ing);
            Item item = new Item(uow); item.Name = "x";
            GroceryList gl = new GroceryList(uow);
            GroceryListItem gli = new GroceryListItem(uow); gli.Item = "x";
            gl.AddGroceryListItem(gli);
            ModelVersion meta = new ModelVersion(uow); meta.DataStoreComponent = "x"; meta.Version = DateTime.Now;
            uow.CommitChanges();
        }
    }
}
