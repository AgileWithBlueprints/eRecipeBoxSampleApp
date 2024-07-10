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
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Foundation
{
    /// <summary>
    /// A business or solution class (aka model class) that is CRUDed in the data store.  
    /// CRUD transaction scope is a cohesive body of business objects.
    /// </summary>
    [NonPersistent]
    public class BusinessObject : XPObject, IDXDataErrorInfo
    {

        #region ctor

        public BusinessObject() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public BusinessObject(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        virtual public string BusinessObjectDisplayName
        {
            get { return null; }
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        private void GetPropertyRequiredErrors(string propertyName, ErrorInfo info)
        {
            Type boType = this.GetType();
            PropertyInfo pi = boType.GetProperty(propertyName);
            if (pi == null)
                return;
            var attrs = pi.GetCustomAttributes(false);
            object propertyValue = pi.GetValue(this);
            object propertyValueStringCleaned = propertyValue;
            bool isStringProperty = pi.PropertyType == typeof(string);
            if (isStringProperty)
                propertyValueStringCleaned = PrimitiveUtils.Clean((string)propertyValue);

            foreach (var attribute in attrs)
            {
                if (attribute is NullableAttribute)
                {
                    bool isNullable = ((NullableAttribute)attribute).IsNullable;
                    if (!isNullable && propertyValueStringCleaned == null)
                    {
                        info.ErrorText = $"'{propertyName}' is required";
                        info.ErrorType = ErrorType.Critical;
                    }
                }
            }
        }

        private void GetPropertyMaskErrors(string propertyName, ErrorInfo info)
        {
            string propertyNameLower = propertyName.ToLower();
            Type boType = this.GetType();
            PropertyInfo pi = boType.GetProperty(propertyName);
            if (pi == null)
                return;
            var attrs = pi.GetCustomAttributes(false);
            object propertyValue = pi.GetValue(this);
            bool isStringProperty = pi.PropertyType == typeof(string);
            foreach (var attribute in attrs)
            {
                if (propertyValue == null || !isStringProperty)
                    return;
                string stringPropertyValue = (string)propertyValue;
                if (propertyNameLower.EndsWith("email") && !PrimitiveUtils.IsValidEmailAddress(stringPropertyValue))
                {
                    info.ErrorText = $"{propertyName}: Invalid email address";
                    info.ErrorType = ErrorType.Critical;
                }
                else if ((propertyNameLower.EndsWith("cellnumber") || propertyNameLower.EndsWith("mobilenumber")
                    || propertyNameLower.EndsWith("homenumber")) && !PrimitiveUtils.IsValidUSPhoneNumber(stringPropertyValue))
                {
                    info.ErrorText = $"{propertyName}: Invalid phone number";
                    info.ErrorType = ErrorType.Critical;
                }
                else if (propertyNameLower.EndsWith("zipcode") && !PrimitiveUtils.IsValidUSZipcode(stringPropertyValue))
                {
                    info.ErrorText = $"{propertyName}: Invalid zipcode";
                    info.ErrorType = ErrorType.Critical;
                }
            }
        }

        #endregion

        #region IDXDataErrorInfo
        virtual public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            GetPropertyRequiredErrors(propertyName, info);
            GetPropertyMaskErrors(propertyName, info);
        }

        virtual public void GetError(ErrorInfo info)
        {
        }
        #endregion

        #region statics
        static public IList<string> GetAllBusinessKeys(Type[] classTypes)
        {
            List<string> result = new List<string>();
            foreach (Type classType in classTypes)
            {
                PropertyInfo[] properties = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo property in properties)
                {
                    object[] attrs = property.GetCustomAttributes(true);
                    foreach (var attribute in attrs)
                    {
                        if (attribute is BusinessKeyAttribute)
                        {
                            result.Add($"{classType.Name}.{property.Name}");
                            break;
                        }
                    }
                }
            }
            return result;
        }
        #endregion
    }
    public class BusinessKeyAttribute : Attribute
    {
    }

}
