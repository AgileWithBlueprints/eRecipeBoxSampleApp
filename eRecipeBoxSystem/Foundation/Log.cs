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
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Foundation
{
    public partial class Log
    {
        //Application log
        static public Log App;
        //DataStore log
        static public Log DataStore;

        public string LogFileFullName { get; private set; }

        //Log an Info message.  
        //#TODO add Warn and Error messages 
        public void Info(string msg)
        {
            logger.Info(msg);
        }

        static public bool SystemTestLogging { get; private set; }

        //#TODO REFACTOR This doesnt belong here
        static public void LogWindowElement(string formName, string className, string name, string[] ancestors)
        {
            if (!SystemTestLogging)
                return;
            if (string.IsNullOrEmpty(name))
                return;
            List<string> list = new List<string>(ancestors);
            list.Add(name);
            string anc = string.Join(".", list.ToArray());
            Log.App.Info($"WindowElement Form={formName} Class={className}  Name={name} Ancestors={anc}");
        }

        //#TODO REFACTOR This doesnt belong here
        //Scan logFileFullName log and report the distinct observed Forms and their child Form Elements
        static public void ReportFormsAndTheirElements(string logFileFullName, string reportFileName = null)
        {
            if (reportFileName == null)
                reportFileName = "WindowElementNames.txt";
            List<string> fileContents = new List<string>();
            StreamReader logFile = new StreamReader(logFileFullName);
            while (!logFile.EndOfStream)
                fileContents.Add(logFile.ReadLine());
            logFile.Close();
            fileContents = fileContents.Distinct().ToList();

            Dictionary<string, List<WindowElement>> result = new Dictionary<string, List<WindowElement>>();
            foreach (string logEntry in fileContents)
            {
                WindowElement windowElement = WindowElement.Parse(logEntry);
                if (windowElement == null)
                    continue;
                if (!result.ContainsKey(windowElement.FormName))
                    result[windowElement.FormName] = new List<WindowElement>();
                result[windowElement.FormName].Add(windowElement);
            }
            logFile.Close();
            string reportPath = $"{logDirectory}\\{reportFileName}";
            StreamWriter outf = new StreamWriter(reportPath);
            //sort the elements within each form, based on their ancestors 
            foreach (string key in result.Keys)
            {
                List<WindowElement> elements = result[key];
                elements.Sort(delegate (WindowElement x, WindowElement y)
                {
                    int idx = 0;
                    int maxIdx = Math.Max(x.Ancestors.Length, y.Ancestors.Length);
                    while (true)
                    {
                        //Both have values for this part
                        if (idx < x.Ancestors.Length && idx < y.Ancestors.Length)
                        {
                            if (x.Ancestors[idx] == y.Ancestors[idx])
                            {
                                //this part is the same for both, move to next part
                                idx++;
                                continue;
                            }
                            else
                                return x.Ancestors[idx].CompareTo(y.Ancestors[idx]);
                        }
                        if (idx >= x.Ancestors.Length && idx < y.Ancestors.Length)
                        {
                            //x doesn't have a value, y does, sort x first.
                            return -1;
                        }
                        if (idx < x.Ancestors.Length && idx >= y.Ancestors.Length)
                        {
                            //y doesn't have a value, x does, sort y first.
                            return 1;
                        }
                        //both are equal
                        return 0;
                    }
                });

                foreach (WindowElement element in elements)
                {
                    string fullPath = string.Join(".", element.Ancestors);
                    outf.WriteLine($"{element.FormName} {fullPath}");
                }
            }
            outf.Close();

            //read report and generate Enum for each form, containing names of each of its WindowElements
            string currentFormName = "";
            List<string> windowElementNames = new List<string>();
            StreamReader reportFile = new StreamReader(reportPath);
            string enumsPath = $"{logDirectory}\\FormEnums.txt";
            StreamWriter enumsf = new StreamWriter(enumsPath);

            while (!reportFile.EndOfStream)
            {
                string[] windowElement = reportFile.ReadLine().Split(' ');
                if (currentFormName != windowElement[0])
                {
                    if (!string.IsNullOrEmpty(currentFormName))
                        enumsf.WriteLine($"public enum {currentFormName}s {{ {string.Join(",", windowElementNames.ToArray())} }};");
                    windowElementNames.Clear();
                    currentFormName = windowElement[0];
                }
                string[] elementNames = windowElement[1].Split('.');
                windowElementNames.Add(elementNames[elementNames.Length - 1]);
            }
            if (!string.IsNullOrEmpty(currentFormName))
                enumsf.WriteLine($"public enum {currentFormName}s {{ {string.Join(",", windowElementNames.ToArray())} }};");
            reportFile.Close();
            enumsf.Close();
        }

        //Initialize all logging
        static public void Initialize()
        {
            App = new Log();
            DataStore = new Log();

            //https://www.c-sharpcorner.com/article/basic-understanding-of-nlog/        

            SystemTestLogging = AppSettings.GetBoolAppSetting("SystemTestLogging");

            //LogManager.ThrowExceptions = true;
            string appLogName = "App";
            string dataStoreLogName = "DataStore";
            string appFilePath = InitLogger(appLogName);
            string dataStoreFilePath = InitLogger(dataStoreLogName);
            LogManager.Configuration = config;
            App.logger = LogManager.GetLogger(appLogName);
            App.LogFileFullName = appFilePath;
            DataStore.logger = LogManager.GetLogger(dataStoreLogName);
            DataStore.LogFileFullName = dataStoreFilePath;
        }
        //Shutdown all logging
        static public void Shutdown()
        {
            NLog.LogManager.Shutdown();
        }
    }

}
