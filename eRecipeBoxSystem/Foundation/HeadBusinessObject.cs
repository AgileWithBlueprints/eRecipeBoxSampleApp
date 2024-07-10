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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
namespace Foundation
{
    /// <summary>
    /// The head of a body of CRUDed business objects.
    /// CRUD transaction scope is a cohesive body of business objects.
    /// </summary>
    [NonPersistent]
    public class HeadBusinessObject : BusinessObject
    {

        #region properties

        private bool fSavingModel = false;
        [JsonIgnore]
        [NonPersistent]
        public bool SavingModel { get { return fSavingModel; } set { fSavingModel = value; } }

        private DateTime fDummy;
        /// <summary>
        /// 
        /// invoking TriggerObjectChanged would probably do the same thing.
        /// </summary>
        [JsonIgnore]
        [NonPersistent]
        public DateTime Dummy
        {
            get { return fDummy; }
            set { SetPropertyValue(nameof(Dummy), ref fDummy, value); }
        }

        private Dictionary<string, string> fDataStoreSavingErrors = new Dictionary<string, string>();
        /// <summary>
        /// Allow consumers of BusObjects to set error messages, keyed off property names.
        /// propertyName => error message
        /// </summary>
        [JsonIgnore]
        [NonPersistent]
        public Dictionary<string, string> DataStoreSavingErrors
        {
            get
            {
                return fDataStoreSavingErrors;
            }
        }

        #endregion properties

        #region ctor
        public HeadBusinessObject() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public HeadBusinessObject(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }
        #endregion ctor

        #region IDXDataErrorInfo
        public override void GetPropertyError(string propertyName, ErrorInfo info)
        {
            if (SavingModel)
            {
                //these are errors returned by the data store service when saving
                if (DataStoreSavingErrors.ContainsKey(propertyName))
                {
                    //Error from server while saving.  Fetch error message stashed by error handler.
                    info.ErrorText = DataStoreSavingErrors[propertyName]; //eg Title
                    info.ErrorType = ErrorType.Critical;
                }
            }
            else
            {
                base.GetPropertyError(propertyName, info);
            }
        }
        #endregion IDXDataErrorInfo

        #region unique constraint names
        static public string GenerateUniqueConstraintName(string className, string propertyName)
        {
            return $"CK_unique_{className}_{propertyName}";
        }

        static public string ExtractUniqueConstraintName(string errorMessage)
        {
            //https://stackoverflow.com/questions/2678666/regex-to-find-words-that-start-with-a-specific-character

            var pattern = @"(?<!\w)" + Regex.Escape("'CK_unique_") + @"\w+";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = regex.Match(errorMessage);
            if (match.Success)
            {
                if (match.Value != null)
                    return match.Value.Trim('\'');
            }
            else
            {
                //try again with starting with a quote
                pattern = @"(?<!\w)" + Regex.Escape("\"CK_unique_") + @"\w+";
                regex = new Regex(pattern, RegexOptions.IgnoreCase);
                match = regex.Match(errorMessage);
                if (match.Success && match.Value != null)
                    return match.Value.Trim('"');
            }
            return null;
        }
        static public void ExtractClassNamePropertyName(string constraintName, out string className, out string propertyName)
        {
            //CK_unique_RecipeCard_Title
            string[] parts = constraintName.Split('_');
            if (parts == null || parts.Length != 4 || parts[0] != "CK" || parts[1] != "unique")
                throw new Exception($"Invalid constraint format: {constraintName}");
            className = parts[2];
            propertyName = parts[3];
        }

        public string GenerateUniqueConstraintName(string propertyName)
        {
            string className = this.GetType().Name;
            return GenerateUniqueConstraintName(className, propertyName);
        }

        #endregion unique constraint names

        #region methods
        public virtual string SerializeMe()
        {
            using (StringWriter file = new StringWriter())
            {
                var settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                settings.ContractResolver = new DerivedClassContractResolver(typeof(BusinessObject));
                settings.Converters.Add(new DecimalConverter("00.00"));
                JsonSerializer serializer = JsonSerializer.Create(settings);
                serializer.Serialize(file, this);
                var x = file.ToString();
                string myType = this.GetType().Name;
                return $"({myType}){x}";
            }
        }
        #endregion

    }

    public class DecimalConverter : JsonConverter
    {
        private readonly string _format;

        public DecimalConverter(string format = "000.00")
        {
            _format = format;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            decimal decimalValue = (decimal)value;
            // Ensure the value is rounded to match the specified format
            decimal roundedValue = Math.Round(decimalValue, 2, MidpointRounding.AwayFromZero);
            writer.WriteValue(roundedValue.ToString(_format, CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string stringValue = (string)reader.Value;
                try
                {
                    return decimal.Parse(stringValue, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    throw new JsonSerializationException($"Error parsing '{stringValue}' as decimal.");
                }
            }
            throw new JsonSerializationException($"Unexpected token type {reader.TokenType}.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(decimal?);
        }
    }
    public class DerivedClassContractResolver : DefaultContractResolver
    {
        private Type _stopAtBaseType;

        public DerivedClassContractResolver(Type stopAtBaseType)
        {
            _stopAtBaseType = stopAtBaseType;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            Type originalType = GetOriginalType(type);
            IList<JsonProperty> defaultProperties = base.CreateProperties(type, memberSerialization);
            List<string> includedProperties = Utilities.GetPropertyNames(originalType, _stopAtBaseType);

            return defaultProperties.Where(p => includedProperties.Contains(p.PropertyName)).ToList();
        }

        private Type GetOriginalType(Type type)
        {
            Type originalType = type;

            //If the type is a dynamic proxy, get the base type
            if (typeof(Castle.DynamicProxy.IProxyTargetAccessor).IsAssignableFrom(type))
                originalType = type.BaseType ?? type;

            return originalType;
        }
    }

    public class Utilities
    {
        /// <summary>
        /// Gets a list of all public instance properties of a given class type
        /// excluding those belonging to or inherited by the given base type.
        /// </summary>
        /// <param name="type">The Type to get property names for</param>
        /// <param name="stopAtType">A base type inherited by type whose properties should not be included.</param>
        /// <returns></returns>
        public static List<string> GetPropertyNames(Type type, Type stopAtBaseType)
        {
            List<string> propertyNames = new List<string>();

            if (type == null || type == stopAtBaseType) return propertyNames;

            Type currentType = type;

            do
            {
                PropertyInfo[] properties = currentType.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);

                foreach (PropertyInfo property in properties)
                    if (!propertyNames.Contains(property.Name))
                        propertyNames.Add(property.Name);

                currentType = currentType.BaseType;
            } while (currentType != null && currentType != stopAtBaseType);

            return propertyNames;
        }
    }

}
