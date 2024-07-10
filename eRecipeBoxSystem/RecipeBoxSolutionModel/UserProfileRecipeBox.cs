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

namespace RecipeBoxSolutionModel
{
    [MapInheritance(MapInheritanceType.OwnTable)]
    /// <summary>
    /// Association class between UserProfile and RecipeBox.  
    /// UPRB grants access permission for the user to the RecipeBox and also its GroceryList
    /// In the future, we can add properties such as permissions, access history, etc.
    /// </summary>
    public class UserProfileRecipeBox : HeadBusinessObject
    {
        public UserProfileRecipeBox() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }
        public UserProfileRecipeBox(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }
        public UserProfileRecipeBox(Session session, RecipeBox recipeBox, UserProfile userProfile) : base(session)
        {
            if (recipeBox == null || userProfile == null)
                throw new Exception("LogicError:  recipeBox or userProfile cannot be null.");
            this.RecipeBox = recipeBox;
            this.UserProfile = userProfile;
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        private RecipeBox fRecipeBox;
        [Association("RecipeBoxUPRB"), Aggregated]
        public RecipeBox RecipeBox
        {
            get { return fRecipeBox; }
            set { SetPropertyValue(nameof(RecipeBox), ref fRecipeBox, value); }
        }

        private UserProfile fUserProfile;
        [Association("UserProfileUPRB"), Aggregated]
        public UserProfile UserProfile
        {
            get { return fUserProfile; }
            set { SetPropertyValue(nameof(UserProfile), ref fUserProfile, value); }
        }

    }


}
