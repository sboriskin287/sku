using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace sku_to_smv
{
    class LogWriter
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

        public void StartLog(bool isStart, bool isEnd, string StateName)
        {
            switch (LogFormat)
            {
                case 0: StartLogTxt(isStart, isEnd, StateName);
                    break;
                case 1: StartLogHTML(isStart, isEnd, StateName);
                    break;
                case 2: StartLogCSV(isStart, isEnd, StateName);
                    break;
                default: StartLogTxt(isStart, isEnd, StateName);
                    break;
           }
        }
        private void StartLogTxt(bool isStart, bool isEnd, string StateName)
        {
            Step = 0;
            if (isStart)
            {
                result += "Начало лог-файла " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
                result += "\r\nШаг симуляции";
            }
            else if (isEnd)
            {
                ;
            }
            else
            {
                result += "\t" + StateName;
            }
        }
        private void StartLogHTML(bool isStart, bool isEnd, string StateName)
        {
            string tmp;
            Step = 0;
            if (isStart)
            {
                result = global::sku_to_smv.Properties.Resources.html;
                _index = result.IndexOf("$data");
                result = result.Remove(_index, "$data".Length);
                result = result.Insert(_index, "<tr>\n<th>Шаг симуляции</th>");
                _index += "<tr>\n<th>Шаг симуляции</th>".Length;
            }
            else if (isEnd)
            {
                result = result.Insert(_index, "\n");
                _index += "\n".Length;
            }
            else
            {
                tmp = "<th>" + StateName + "</th>";
                result = result.Insert(_index, tmp);
                _index += tmp.Length;
            }
        }
        private void StartLogCSV(bool isStart, bool isEnd, string StateName)
        {
            Step = 0;
            if (isStart)
            {
                result += "Шаг симуляции";
            }
            else if (isEnd)
            {
                ;
            }
            else
            {
                result += ";" + StateName;
            }
        }
        public void AddToLog(String Name, bool Value, bool Input, bool Output, bool StepStart)
        {
            switch (LogFormat)
            {
                case 0: AddToLogTxt(Name, Value, Input, Output, StepStart);
                    break;
                case 1: AddToLogHTML(Name, Value, Input, Output, StepStart);
                    break;
                case 2: AddToLogCSV(Name, Value, Input, Output, StepStart);
                    break;
                default: AddToLogTxt(Name, Value, Input, Output, StepStart);
                    break;
            }
        }
        private void AddToLogTxt(String Name, bool Value, bool Input, bool Output, bool StepStart)
        {
            if (StepStart)
            {
                result += "\r\nШаг " + Step;
                Step++;
            }
            result += (Value ? "\t1" : "\t0");
        }
        private void AddToLogHTML(String Name, bool Value, bool Input, bool Output, bool StepStart)
        {
            string tmp;
            if (StepStart)
            {
                tmp = "\n</tr>\n<tr><td>Шаг " + Step + "</td>";
                result = result.Insert(_index, tmp);
                _index += tmp.Length;
                Step++;
            }
            if (Input)
                tmp = "<td class=\"input\"" + (Value ? "style=\"font-weight: bold; color: red;\"" : "") + ">" + (Value ? "1" : "0") + "</td>";
            else if (Output)
                tmp = "<td class=\"output\"" + (Value ? "style=\"font-weight: bold; color: red;\"" : "") + ">" + (Value ? "1" : "0") + "</td>";
            else
                tmp = "<td class=\"local\"" + (Value ? "style=\"font-weight: bold; color: red;\"" : "") + ">" + (Value ? "1" : "0") + "</td>";
            result = result.Insert(_index, tmp);
            _index += tmp.Length;
        }
        private void AddToLogCSV(String Name, bool Value, bool Input, bool Output, bool StepStart)
        {
            if (StepStart)
            {
                result += "\r\nШаг " + Step;
                Step++;
            }
            result += (Value ? ";1" : ";0");
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
        public void EndLogTxt()
        {
            SavedFileName = FileName + "_" + DateTime.Now.ToString("dMyhms", CultureInfo.InvariantCulture) + GetFormat();
            sw = new StreamWriter(SavedFileName, false, System.Text.Encoding.GetEncoding("windows-1251"));
            if (sw != null)
            {
                result += "\r\nКонец лог-файла " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
                sw.Write(result);
                sw.Flush();
                sw.Close();
                sw = null;
            }
        }
        public void EndLogHTML()
        {
            SavedFileName = FileName + "_" + DateTime.Now.ToString("dMyhms", CultureInfo.InvariantCulture) + GetFormat();
            sw = new StreamWriter(SavedFileName, false);
            if (sw != null)
            {
                result = result.Insert(_index, "</tr>");
                sw.Write(result);
                sw.Flush();
                sw.Close();
                sw = null;
            }
        }
        public void EndLogCSV()
        {
            SavedFileName = FileName + "_" + DateTime.Now.ToString("dMyhms", CultureInfo.InvariantCulture) + GetFormat();
            sw = new StreamWriter(SavedFileName, false, System.Text.Encoding.GetEncoding("windows-1251"));
            if (sw != null)
            {
                sw.Write(result);
                sw.Flush();
                sw.Close();
                sw = null;
            }
        }
    }
}
