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
using Pluralize.NET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
namespace Foundation
{

    #region EnvironmentUtils
    public class EnvironmentUtils
    {
        static public void GetCredentials(string email, out string userID, out string pw)
        {
            //#RQT how wcf user and pw is comprised and encrypted
            string clientSystemName = Environment.MachineName;
            string macAddress = EnvironmentUtils.GetDefaultMacAddress();
            string now = DateTime.Now.ToLongTimeString();
            if (email == null)
                throw new Exception("logic error, null passed when getting creds");
            //adding now causes a unique string each time
            userID = Encryption.EncryptToHex($"{email.ToLower()}\t{now}");
            pw = Encryption.EncryptToHex($"{macAddress}\t{clientSystemName.ToLower()}\t{email}\t{now}");
        }
        static public string GetWindowsUserName()
        {
            string username = Environment.UserName.ToLower();
            System.Security.Principal.WindowsIdentity me = System.Security.Principal.WindowsIdentity.GetCurrent();
            string myuser = me.Name.ToLower();  //includes domain system. dont want this
            return username;
        }
        static public string GetDefaultMacAddress()
        {
            Dictionary<string, long> macAddresses = new Dictionary<string, long>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                    macAddresses[nic.GetPhysicalAddress().ToString()] = nic.GetIPStatistics().BytesSent + nic.GetIPStatistics().BytesReceived;
            }
            long maxValue = 0;
            string mac = "";
            foreach (KeyValuePair<string, long> pair in macAddresses)
            {
                if (pair.Value > maxValue)
                {
                    mac = pair.Key;
                    maxValue = pair.Value;
                }
            }
            return mac;
        }
    }
    #endregion


    #region PrimitiveUtils
    public class PrimitiveUtils
    {

        private class SimplifiedFraction
        {
            //borrowed from https://stackoverflow.com/questions/5287514/how-to-simplify-fractions
            public SimplifiedFraction(int wholePart, int num, int denom)
            {
                WholePart = wholePart;
                Numerator = num;
                Denominator = denom;
                Normalize();
            }
            public int WholePart { get; private set; }
            public int Numerator { get; private set; }
            public int Denominator { get; private set; }
            private int GCD(int a, int b)
            {
                while (b != 0)
                {
                    int t = b;
                    b = a % b;
                    a = t;
                }
                return a;
            }
            private void Reduce(int x)
            {
                Numerator /= x;
                Denominator /= x;
            }
            private void Normalize()
            {
                // Add the whole part to the fraction so that we don't have to check its sign later
                Numerator += WholePart * Denominator;

                // Reduce the fraction to be in lowest terms
                Reduce(GCD(Numerator, Denominator));

                // Make it so that the denominator is always positive
                Reduce(Math.Sign(Denominator));

                // Turn num/denom into a proper fraction and add to wholePart appropriately
                WholePart = Numerator / Denominator;
                Numerator %= Denominator;
            }

            override public String ToString()
            {
                if (WholePart == 0)
                    return $"{Numerator}/{Denominator}";
                else
                    return $"{WholePart} {Numerator}/{Denominator}";
            }
        }

        static public int ObjectToInt(object oid)
        {
            if (oid is int)
                return (int)oid;
            else
            {
                //some are long, some are int
                long ol = (long)oid;
                return (int)ol;
            }
        }

        static public string ConvertToFraction(decimal dec)
        {

            decimal integerPart;
            decimal fractionPart;
            decimal absDec = Math.Abs(dec);
            string sign = (dec < 0.0m) ? "-" : "";

            integerPart = Math.Floor(absDec);
            fractionPart = absDec - integerPart;

            if (fractionPart == 0.0m)
                return dec.ToString();

            int count = 0;
            //thousandths are plenty enough precision for ingredient quantities
            while (fractionPart % 1 != 0 && count <= 3)
            {
                fractionPart = fractionPart * 10;
                count++;
            }
            int denominator = Convert.ToInt32(Math.Pow(10, count));
            int numerator = Convert.ToInt32(fractionPart);

            List<string> results = new List<string>();
            results.Add(sign + new SimplifiedFraction((int)integerPart, numerator, denominator).ToString());
            // solves the .333333 issue.  1/3 is better  than 333/1000
            results.Add(sign + new SimplifiedFraction((int)integerPart, numerator, denominator - 1).ToString());
            //.667
            results.Add(sign + new SimplifiedFraction((int)integerPart, numerator - 1, denominator - 1).ToString());

            int minLength = results.Min(y => y.Length);
            return results.FirstOrDefault(x => x.Length == minLength);
        }
        public static TimeSpan? MinutesToTimeSpan(int? min)
        {
            TimeSpan? result = null;
            if (min == null)
                return null;
            else
            {
                result = new TimeSpan(0, (int)min, 0);
                return result;
            }
        }
        static public string newl = Environment.NewLine;
        static public string newl2 = newl + newl;
        static public string Captialize(string str)
        {
            if (str == null) return null;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(str);
        }
        static public string Clean(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;
            else
                return str.Trim();
        }

        static public string Namify(string str)
        {
            //#TODO REFACTOR regex this
            if (string.IsNullOrWhiteSpace(str))
                return null;
            return str.Replace(" ", "").Replace("\\", "_").Replace("/", "_").Replace("%", "_").Replace("$", "_").Replace("&", "").Replace(".", "_")
                .Replace("$", "_").Replace("\r", "").Replace("\n", "").Replace("__", "_").Replace("(", "").Replace(")", "").Replace("!", "").Replace("'", "");

        }

        static public string ObjectToString(object val)
        {
            if (val == null)
                return "";
            else
                return val.ToString().Trim();
        }
        static public string SQLStringLiteral(string input)
        {
            if (input == null)
                return null;
            else
                return input.Replace("'", "''");
        }
        static public TimeSpan? TotalMinToTimeSpan(string text)
        {
            if (text == null || text.Trim().Length == 0)
                return null;
            else
                return new TimeSpan(0, Int32.Parse(text.Trim()), 0);
        }
        static public decimal? TextEditToDecimal(string text)
        {
            if (text == null || text.Trim().Length == 0)
                return null;
            else
                return decimal.Parse(text);
        }

        static public string RemoveVowels(string str, bool lcOnly = true)
        {
            Regex r;
            if (lcOnly)
                r = new Regex("[aeiou]");
            else
                r = new Regex("[aAeEiIoOuU]");
            return r.Replace(str, "");
        }

        static public string Abbreviate(string str)
        {
            return RemoveVowels(str);
        }


        public static string InsertLineBreakEachSentence(string str)
        {
            string result = Regex.Replace(str, @"[!?.]+(?=$|\s)", @"." + Environment.NewLine, RegexOptions.Compiled);
            result = Regex.Replace(result, Environment.NewLine + " ", Environment.NewLine, RegexOptions.Compiled);
            return result;
        }

        public static string ReplaceSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", " ", RegexOptions.Compiled);
        }

        static public int? TextEditToInt32(string text)
        {
            if (text == null || text.Trim().Length == 0)
                return null;
            else
                return Int32.Parse(text);
        }
        /// <summary>
        /// Standardize an item to its singular.  eg, apples returns apple
        /// </summary>
        /// <param name="str">item</param>
        /// <returns>singlular version of item</returns>
        static public string ToSingularLower(string str)
        {
            if (str == null || str.Trim().Length == 0)
                return str;

            IPluralize pluralizer = new Pluralizer();

            string[] words = str.Split(' ');
            string[] lcWords = new string[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                lcWords[i] = pluralizer.Singularize(words[i]).ToLower();
            }
            return string.Join(" ", lcWords);
        }
        static public string JustDigits(string str)
        {
            if (str == null)
                return null;
            StringBuilder result = new StringBuilder();
            foreach (char c in str)
            {
                if (Char.IsDigit(c))
                    result.Append(c);
            }
            return result.ToString();
        }
        public const string USPhoneNumberMask = @"\d{10}";
        public const string USZipCodeMask = @"\d{5}";
        static public bool IsValidUSZipcode(string zipCode)
        {
            Regex regex = new Regex(USZipCodeMask);
            Match match = regex.Match(zipCode);
            return match.Success;
        }
        static public bool IsValidUSPhoneNumber(string phoneNumber)
        {
            Regex regex = new Regex(USPhoneNumberMask);
            Match match = regex.Match(phoneNumber);
            return match.Success;
        }

        //#RQT Email addresses must be RFC 5322 compliant.
        //sources: http://emailregex.com/
        //https://stackoverflow.com/questions/13992403/regex-validation-of-email-addresses-according-to-rfc5321-rfc5322
        // @"([!#-'*+/-9=?A-Z^-~-]+(\.[!#-'*+/-9=?A-Z^-~-]+)*|""([]!#-[^-~ \t]|(\\[\t -~]))+"")@([!#-'*+/-9=?A-Z^-~-]+(\.[!#-'*+/-9=?A-Z^-~-]+)*|\[[\t -Z^-~]*])"
        // works @"((((\w+-*)|(-*\w+))+\.*((\w+-*)|(-*\w+))+)|(((\w+-*)|(-*\w+))+))+@((((\w+-*)|(-*\w+))+\.*((\w+-*)|(-*\w+))+)|(((\w+-*)|(-*\w+))+))+\.[A-Za-z]+";
        public const string EmailMask = @"((((\w+-*)|(-*\w+))+\.*((\w+-*)|(-*\w+))+)|(((\w+-*)|(-*\w+))+))+@((((\w+-*)|(-*\w+))+\.*((\w+-*)|(-*\w+))+)|(((\w+-*)|(-*\w+))+))+\.[A-Za-z]+";
        static public bool IsValidEmailAddress(string emailAddress)
        {
            Regex regex = new Regex(EmailMask);  //@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"
            Match match = regex.Match(emailAddress);
            return match.Success;
        }

    }

    //Borrowed from here
    //https://stackoverflow.com/questions/13903621/convert-a-text-fraction-to-a-decimal
    public class FractionalNumber
    {
        public Double Result
        {
            get { return this.result; }
            private set { this.result = value; }
        }
        private Double result;

        public FractionalNumber(String input)
        {
            this.Result = this.Parse(input);
        }

        private Double Parse(String input)
        {
            input = (input ?? String.Empty).Trim();
            if (String.IsNullOrEmpty(input))
                return Double.NaN;

            // standard decimal number (e.g. 1.125)
            if (input.IndexOf('.') != -1 || (input.IndexOf(' ') == -1 && input.IndexOf('/') == -1 && input.IndexOf('\\') == -1))
            {
                Double result;
                if (Double.TryParse(input, out result))
                {
                    return result;
                }
            }

            String[] parts = input.Split(new[] { ' ', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            // stand-off fractional (e.g. 7/8)
            if (input.IndexOf(' ') == -1 && parts.Length == 2)
            {
                Double num, den;
                if (Double.TryParse(parts[0], out num) && Double.TryParse(parts[1], out den))
                {
                    return num / den;
                }
            }

            // Number and fraction (e.g. 2 1/2)
            if (parts.Length == 3)
            {
                Double whole, num, den;
                if (Double.TryParse(parts[0], out whole) && Double.TryParse(parts[1], out num) && Double.TryParse(parts[2], out den))
                {
                    return whole + (num / den);
                }
            }

            // Bogus / unable to parse
            return Double.NaN;
        }

        public override string ToString()
        {
            return this.Result.ToString();
        }

        public static implicit operator Double(FractionalNumber number)
        {
            return number.Result;
        }

        //https://www.compart.com/en/unicode/decomposition/%3Cfraction%3E
        static public readonly Dictionary<string, string> UnicodeFractionsTranslation = new Dictionary<string, string>()
                    {
                        { "¼", "1/4" },
                        { "½", "1/2" },
                        { "¾", "3/4" },
                        { "⅐", "1/7" },
                        { "⅑", "1/9" },
                        { "⅒", "1/10" },
                        { "⅓", "1/3" },
                        { "⅔", "2/3" },
                        { "⅕", "1/5" },
                        { "⅖", "2/5" },
                        { "⅗", "3/5" },
                        { "⅘", "4/5" },
                        { "⅙", "1/6" },
                        { "⅚", "5/6" },
                        { "⅛", "1/8" },
                        { "⅜", "3/8" },
                        { "⅝", "5/8" },
                        { "⅞", "7/8" },
                        { "↉", "0/3" }
                    };
    }
    #endregion
}
