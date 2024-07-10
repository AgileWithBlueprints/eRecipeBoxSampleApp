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
using Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eRecipeBox.ImportUtils
{
    internal partial class GPTRecipeCardImporter
    {
        #region static methods         
        //#WORKAROUND openAI occasionally returns invalid json.. so fix it.  even though i ask it for decimals, it returns (invalid) fractions.
        //bad  "quantity": 3/4,
        //fixed  "quantity": "3/4",
        //sometimes it adds just one " others there are no quotes for fractions
        static internal string FixOpenAIBadJSON(string recipeCardJSON)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            try
            {
                writer.Write(recipeCardJSON);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                List<string> outLines = new List<string>();
                string pattern = @"(""quantity"":)(?<qty>.*)(,)";
                // convert stream to string
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        Regex regEx = new Regex(pattern);
                        MatchCollection mc = regEx.Matches(line);
                        if (mc.Count == 1)
                        {
                            //sometimes it adds just one " others there are no quotes for fractions
                            string qty = mc[0].Groups["qty"].Value.Trim().Replace('"' + "", "");
                            outLines.Add(@" ""quantity"": """ + qty + '"' + ",");
                        }
                        else
                            outLines.Add(line);
                    }
                    foreach (var l in outLines)
                        Log.App.Info(l);
                }
                return string.Join("\n", outLines);
            }
            finally
            {
                stream.Dispose();
                writer.Dispose();
            }
        }

        static public async Task<string> GetRecipeAsync(string userRecipePrompt)
        {
            string gptPrompt = userRecipePrompt +
@". Return the recipe in valid JSON format with title, sourceWebsiteURL, description, totalCookTime in minutes, yield, userRating, ingredient and steps. Each ingredient should have separate fields for quantity, unitOfMeasure, item, and itemDescription. Return ingredient quantities that are fractions as JSON decimal and unitOfMeasure using Imperial system of measurement.";
            try
            {

                string API_KEY = Encryption.DecryptHexString(AppSettings.GetAppSetting("EncryptedGptApiKey"));
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
                    client.DefaultRequestHeaders.Add("User-Agent", "OpenAI-C#-Client");
                    OpenAIPayload payload = new OpenAIPayload
                    {
                        model = "gpt-3.5-turbo",
                        //json mode is relatively new https://platform.openai.com/docs/guides/text-generation/json-mode
                        response_format = new ResponseFormat { type = "json_object" },
                        messages = new List<OpenAIMessage>
                        {
                            new OpenAIMessage { role = "system", content = "You are a helpful assistant." },
                            new OpenAIMessage { role = "user", content = gptPrompt}
                        }
                    };

                    Log.App.Info($"{gptPrompt}");
                    string jsonString = JsonConvert.SerializeObject(payload);
                    StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    string ENDPOINT_URL = "https://api.openai.com/v1/chat/completions";
                    // Send the POST request to OpenAI API
                    HttpResponseMessage response = await client.PostAsync(ENDPOINT_URL, content);

                    // Parse the response and extract the desired information
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JObject jsonResponse = JObject.Parse(responseBody);
                        //var x = jsonResponse.ToString();
                        string choices = jsonResponse["choices"].ToString();
                        Log.App.Info($"{choices}");

                        JToken json = JArray.Parse(choices);
                        string responseContent = WebUtility.HtmlDecode(json[0]["message"]["content"].ToString());
                        Log.App.Info($"{responseContent}");

                        //#WORKAROUND GPT sometimes returns in a list and sometimes not
                        if (!responseContent.StartsWith("["))
                            responseContent = '[' + responseContent + ']';

                        return responseContent;
                    }
                    else
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        string logMsg = $"API call failed with status code {response.StatusCode}: {response.ReasonPhrase} {responseBody}";
                        Log.App.Info(logMsg);
                        throw new Exception(logMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.App.Info($"Error calling GPT: {ex.Message}");
                throw new Exception($"GPT unable to find recipe: {ex.Message}");
            }
        }
        #endregion static methods 
    }

    #region helper classes
    internal class OpenAIMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    internal class ResponseFormat
    {
        public string type { get; set; }
    }
    internal class OpenAIPayload
    {
        public string model { get; set; }
        public ResponseFormat response_format { get; set; }
        public List<OpenAIMessage> messages { get; set; }
    }
    #endregion helper classes
}
