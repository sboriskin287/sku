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

        public void StartLog()
        {
            sw = new StreamWriter(FileName, true);
            if (sw != null)
            {
                sw.WriteLine("Начало лог-файла " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
                sw.Flush();
                Step = 0;
            }
        }
        public void AddToLog(String Name, bool Value, bool Input, bool StepStart)
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
                sw.WriteLine(Name + "\t\t\t" + (Value? "1":"0"));
                sw.Flush();
            }
        }
        public void EndLog()
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
    }
}
