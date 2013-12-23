using System;
using System.IO;

namespace sku_to_smv
{
    public class Rule
    {
        public Element[] Elems;
        int count;
        public Rule()
        {
            Elems = new Element[1];
            Elems[0] = new Element();
            count = 0;
        }
        public bool AddElement()
        {
            Array.Resize(ref Elems, Elems.Length + 1);
            Elems[Elems.Length - 1] = new Element();
            count++;
            return true;
        }
        public bool AddData(string Type, string Value, bool Inv, bool Output = false)
        {
            Elems[count].Empty = false;
            Elems[count].Type = Type;
            Elems[count].Value = Value;
            Elems[count].Inverted = Inv;
            Elems[count].Output = Output;
            AddElement();
            return true;
        }
        public void PrintRule(StreamWriter sw)
        {
            for (int j = 0; j < Elems.Length - 1; j++)
            {
                if (j == 0)
                {
                    sw.Write("\tnext(" + Elems[j].Value + ")");
                }
                else if (Elems[j].Type == "=")
                {
                    sw.Write(":=");
                }
                else if (Elems[j].Type == "t+1")
                {
                }
                else if (!Elems[j].Inverted) sw.Write(Elems[j].Value);
                else sw.Write("~" + Elems[j].Value);

                sw.Flush();
            }
            sw.Write(";");
        }
        public void Clear()
        {
            Array.Resize(ref Elems, 1);
            Elems[0].Empty = true;
            Elems[0].Inverted = false;
            Elems[0].Type = "";
            Elems[0].Value = "";
            Elems[0].Local = false;
            count = 0;
        }
    }
}