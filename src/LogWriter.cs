using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace SCUConverterDrawArea
{
    internal class LogWriter
    {
        public int LogFormat;
        public String FileName { get; set; }
        public String SavedFileName { get; set; }

        StreamWriter sw;
        int Step;
        int _index;
        String result;

        public string GetFormat()
        {
            switch (LogFormat)
            {
                case 0: return ".txt";
                case 1: return ".htm";
                case 2: return ".csv";
                default: return ".txt";
            }
        }

        public void StartLog(bool isStart, bool isEnd, string stateName)
        {
            switch (LogFormat)
            {
                case 0: StartLogTxt(isStart, isEnd, stateName);
                    break;
                case 1: StartLogHTML(isStart, isEnd, stateName);
                    break;
                case 2: StartLogCSV(isStart, isEnd, stateName);
                    break;
                default: StartLogTxt(isStart, isEnd, stateName);
                    break;
           }
        }
        
        private void StartLogTxt(bool isStart, bool isEnd, string stateName)
        {
            Step = 0;
            if (isStart)
            {
                result += string.Format("Начало лог-файла {0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
                result += "\r\nШаг моделирования";
            }
            else if (isEnd)
            {
                ;
            }
            else
            {
                result += string.Format("\t{0}", stateName);
            }
        }
        
        private void StartLogHTML(bool isStart, bool isEnd, string stateName)
        {
            Step = 0;
            if (isStart)
            {
                result = Properties.Resources.html;
                _index = result.IndexOf("$data", StringComparison.Ordinal);
                result = result.Remove(_index, "$data".Length);
                result = result.Insert(_index, "<tr>\n<th>Шаг моделирования</th>");
                _index += "<tr>\n<th>Шаг моделирования</th>".Length;
            }
            else if (isEnd)
            {
                result = result.Insert(_index, "\n");
                _index += "\n".Length;
            }
            else
            {
                var tmp = string.Format("<th>{0}</th>", stateName);
                result = result.Insert(_index, tmp);
                _index += tmp.Length;
            }
        }
        
        private void StartLogCSV(bool isStart, bool isEnd, string stateName)
        {
            Step = 0;
            if (isStart)
            {
                result += "Шаг моделирования";
            }
            else if (isEnd)
            {
                ;
            }
            else
            {
                result += string.Format(";{0}", stateName);
            }
        }
        public void AddToLog(bool value, bool input, bool output, bool stepStart)
        {
            switch (LogFormat)
            {
                case 0: AddToLogTxt(value, stepStart);
                    break;
                case 1: AddToLogHTML(value, input, output, stepStart);
                    break;
                case 2: AddToLogCSV(value, stepStart);
                    break;
                default: AddToLogTxt(value, stepStart);
                    break;
            }
        }
        private void AddToLogTxt(bool value, bool stepStart)
        {
            if (stepStart)
            {
                result += string.Format("\r\nШаг {0}", Step);
                Step++;
            }
            result += (value ? "\t1" : "\t0");
        }
        
        private void AddToLogHTML(bool value, bool input, bool output, bool stepStart)
        {
            string tmp;
            if (stepStart)
            {
                tmp = string.Format("\n</tr>\n<tr><td>Шаг {0}</td>", Step);
                result = result.Insert(_index, tmp);
                _index += tmp.Length;
                Step++;
            }
            if (input)
                tmp = string.Format("<td class=\"input\"{0}>{1}</td>", (value ? "style=\"font-weight: bold; color: red;\"" : ""), (value ? "1" : "0"));
            else if (output)
                tmp = string.Format("<td class=\"output\"{0}>{1}</td>", (value ? "style=\"font-weight: bold; color: red;\"" : ""), (value ? "1" : "0"));
            else
                tmp = string.Format("<td class=\"local\"{0}>{1}</td>", (value ? "style=\"font-weight: bold; color: red;\"" : ""), (value ? "1" : "0"));
            result = result.Insert(_index, tmp);
            _index += tmp.Length;
        }
        private void AddToLogCSV(bool value, bool stepStart)
        {
            if (stepStart)
            {
                result += string.Format("\r\nШаг {0}", Step);
                Step++;
            }
            result += (value ? ";1" : ";0");
        }
        public void EndLog()
        {
            switch (LogFormat)
            {
                case 0: EndLogTxt();
                    break;
                case 1: EndLogHTML();
                    break;
                case 2: EndLogCSV();
                    break;
                default: EndLogTxt();
                    break;
            }
        }

        private void EndLogTxt()
        {
            SavedFileName = string.Format("{0}_{1}{2}", FileName, DateTime.Now.ToString("dMyhms", CultureInfo.InvariantCulture), GetFormat());
            sw = new StreamWriter(SavedFileName, false, Encoding.GetEncoding("windows-1251"));
            if (sw == null)
            {
                return;
            }
            result += string.Format("\r\nКонец лог-файла {0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
            sw.Write(result);
            sw.Flush();
            sw.Close();
            sw = null;
        }

        private void EndLogHTML()
        {
            SavedFileName = string.Format("{0}_{1}{2}", FileName, DateTime.Now.ToString("dMyhms", CultureInfo.InvariantCulture), GetFormat());
            sw = new StreamWriter(SavedFileName, false);
            if (sw == null)
            {
                return;
            }
            result = result.Insert(_index, "</tr>");
            sw.Write(result);
            sw.Flush();
            sw.Close();
            sw = null;
        }

        private void EndLogCSV()
        {
            SavedFileName = string.Format("{0}_{1}{2}", FileName, DateTime.Now.ToString("dMyhms", CultureInfo.InvariantCulture), GetFormat());
            sw = new StreamWriter(SavedFileName, false, Encoding.GetEncoding("windows-1251"));
            if (sw == null)
            {
                return;
            }
            sw.Write(result);
            sw.Flush();
            sw.Close();
            sw = null;
        }
    }
}
