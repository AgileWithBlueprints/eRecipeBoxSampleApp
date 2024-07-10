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
using Newtonsoft.Json;
using System;

namespace RecipeBoxSolutionModel
{

    [MapInheritance(MapInheritanceType.OwnTable)]
    /// <summary>
    /// Enumerate and standardize Items contained in recipes.
    /// </summary>
    public class Item : HeadBusinessObject
    {
        public Item() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public Item(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        private string fName;

        [Nullable(false)]
        [Indexed(Unique = true)]
        [Size(4000)]
        [BusinessKey]
        public string Name
        {
            get { return fName; }
            set
            {
                //#RQT ItmClnCapNm Auto-clean and capitalize all Item Name words. eg, Red Onion.  
                cachedNameSingularLower = PrimitiveUtils.ToSingularLower(value);
                SetPropertyValue(nameof(Name), ref fName, PrimitiveUtils.Clean(PrimitiveUtils.Captialize(value)));
            }
        }

        private string cachedNameSingularLower = null;

        [JsonIgnore]
        [NonPersistent]
        public string NameSingularLower
        {
            get
            {
                //if (cachedNameSingularLower == null)
                //    cachedNameSingularLower = PrimitiveUtils.ToSingularLower(Name);
                return cachedNameSingularLower;
            }
        }
    }

}
