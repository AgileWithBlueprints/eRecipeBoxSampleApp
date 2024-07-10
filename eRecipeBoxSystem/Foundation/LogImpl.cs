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
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Text;
using System.Text.RegularExpressions;
namespace Foundation
{
    public partial class Log
    {
        private Logger logger;

        static readonly string logDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\LogFiles";

        static private string InitLogger(string loggerName)
        {
            string nowStr = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            //https://gist.github.com/rstackhouse/1162357/930f9c124b9ba05c819474da26551a5aadf35aec

            string logPath = $"{logDirectory}\\{loggerName}{nowStr}.log";
            var fileTarget = new FileTarget
            {
                CreateDirs = true,
                FileName = logPath,
                Encoding = Encoding.Unicode,
                //#TODO BUG  Even though AutoFlush = true, don't see log until we shutdown.
                AutoFlush = true,
                ConcurrentWrites = true,
                //other possible settings
                //ArchiveFileName = String.Format("{0}\\{1}.{2}.log", logPath, logFileName, "{#####}"),
                //ArchiveNumbering = ArchiveNumberingMode.Sequence,
                //ArchiveEvery = FileArchivePeriod.None,
                //ArchiveAboveSize = 10 * 1024 * 1024,
                //BufferSize = 10240
            };
            bool systemTestLogging = AppSettings.GetBoolAppSetting("SystemTestLogging");
            if (systemTestLogging)
                fileTarget.Layout = Layout.FromString("${message}\t${exception}");
            else
                fileTarget.Layout = Layout.FromString("${date:format=yyyy-MM-dd HH\\:mm\\:ss.fffffff} ${level}\t${message}\t${exception}");
            var asyncFileTarget = new AsyncTargetWrapper(fileTarget);
            config.AddTarget("file", asyncFileTarget);
            var fileLoggingRule = new LoggingRule(loggerName, LogLevel.Info, asyncFileTarget);
            config.LoggingRules.Add(fileLoggingRule);
            return logPath;
        }
        static private LoggingConfiguration config = new LoggingConfiguration();
    }

    internal class WindowElement
    {
        public string FormName { get; private set; } = null;
        public string ClassName { get; private set; } = null;
        public string Name { get; private set; } = null;
        public string[] Ancestors { get; private set; } = null;

        static internal WindowElement Parse(string logEntry)
        {
            if (!logEntry.StartsWith("WindowElement"))
                return null;
            WindowElement result = new WindowElement();
            string formPattern = @"(?<=Form=)\w+";
            var m = Regex.Match(logEntry, formPattern);
            if (!m.Success)
                throw new Exception("log entry exception");
            result.FormName = m.Value;

            string classPattern = @"(?<=Class=)\w+";
            m = Regex.Match(logEntry, classPattern);
            if (m.Success)
                result.ClassName = m.Value;

            string namePattern = @"(?<=Name=)\w+";
            m = Regex.Match(logEntry, namePattern);
            if (m.Success)
                result.Name = m.Value;

            string ancestorsPattern = @"(?<=Ancestors=)\S+";
            m = Regex.Match(logEntry, ancestorsPattern);
            if (m.Success)
                result.Ancestors = m.Value.Split('.');
            return result;
        }
    }
}
