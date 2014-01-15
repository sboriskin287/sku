using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace sku_to_smv
{
    class LogWriter
    {
        public String FileName { get; set; }
        StreamWriter sw;
        int Step;
        public int LogFormat;
        int _index;
        String result;

        public void StartLog(bool isStart, bool isEnd, string StateName)
        {
            switch (LogFormat)
            {
                case 0: StartLogTxt(isStart, isEnd, StateName);
                    break;
                case 1: StartLogHTML(isStart, isEnd, StateName);
                    break;
                default: StartLogTxt(isStart, isEnd, StateName);
                    break;
           }
        }
        private void StartLogTxt(bool isStart, bool isEnd, string StateName)
        {
            if (isStart)
            {
                sw = new StreamWriter(FileName, true);
                if (sw != null)
                {
                    sw.WriteLine("Начало лог-файла " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
                    sw.Flush();
                    Step = 0;
                }
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
        public void AddToLog(String Name, bool Value, bool Input, bool Output, bool StepStart)
        {
            switch (LogFormat)
            {
                case 0: AddToLogTxt(Name, Value, Input, Output, StepStart);
                    break;
                case 1: AddToLogHTML(Name, Value, Input, Output, StepStart);
                    break;
                default: AddToLogTxt(Name, Value, Input, Output, StepStart);
                    break;
            }
        }
        private void AddToLogTxt(String Name, bool Value, bool Input, bool Output, bool StepStart)
        {
            if (sw != null)
            {
                if (Input)
                {
                    if (StepStart)
                    {
                        sw.WriteLine();
                        sw.WriteLine("---------------------------------------------------------------------");
                        sw.WriteLine("Такт " + Step.ToString() + " время " + DateTime.Now.ToLongTimeString());
                        sw.WriteLine("Входные сигналы");
                        Step++;
                    }
                }
                else
                {
                    if (StepStart)
                    {
                        sw.WriteLine();
                        sw.WriteLine("Локальные сигналы");
                    }
                }
                sw.WriteLine(Name + "\t\t\t" + (Value ? "1" : "0"));
                sw.Flush();
            }
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
                tmp = "<td class=\"input\"" + (Value ? " font-weight: bold" : "") + ">" + (Value ? "1" : "0") + "</td>";
            else if (Output)
                tmp = "<td class=\"output\"" + (Value ? " font-weight: bold" : "") + ">" + (Value ? "1" : "0") + "</td>";
            else
                tmp = "<td class=\"local\"" + (Value ? " font-weight: bold" : "") + ">" + (Value ? "1" : "0") + "</td>";
            result = result.Insert(_index, tmp);
            _index += tmp.Length;
        }
        public void EndLog()
        {
            switch (LogFormat)
            {
                case 0: EndLogTxt();
                    break;
                case 1: EndLogHTML();
                    break;
                default: EndLogTxt();
                    break;
            }
        }
        public void EndLogTxt()
        {
            if (sw != null)
            {
                sw.WriteLine("---------------------------------------------------------------------");
                sw.WriteLine("Конец лог-файла " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
                sw.WriteLine();
                sw.Flush();
                sw.Close();
                sw = null;
            }
        }
        public void EndLogHTML()
        {
            sw = new StreamWriter(FileName, false);
            if (sw != null)
            {
                result = result.Insert(_index, "</tr>");
                sw.Write(result);
                sw.Flush();
                sw.Close();
                sw = null;
            }
        }
    }
}
