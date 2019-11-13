using System;
using System.IO;

namespace SCUConverterDrawArea
{
    // Summary:
    //     Описывает элемент списка правил автомата 
    //     создающегося при разборе его описания.
    public class Rule
    {
        public Element[] Elems;     //набор элементов правила.
        public int count;                  //счетчик элементов в правиле.
        public bool output;         //определяет евляется ли правило 
                                    // описанием выходного сигнала.
        public Rule()
        {
            Elems = new Element[1];
            Elems[0] = new Element();
            count = 0;
            output = false;
        }
        
        // Summary:
        //     Добавляет пустой элемен в правило.
        private void AddElement()
        {
            Array.Resize(ref Elems, Elems.Length + 1);
            Elems[Elems.Length - 1] = new Element();
            count++;
        }
        
        // Summary:
        //     Добавляет элемент в правило и заполняет его данными.
        public void AddData(string type, string value, bool inv, bool output = false)
        {
            Elems[count].Empty = false;
            Elems[count].Type = type;
            Elems[count].Value = value;
            Elems[count].Inverted = inv;
            Elems[count].Output = output;
            AddElement();
        }
        
        // Summary:
        //     компонует правило в строку и выводит на печать.
        public void PrintRule(StreamWriter sw)
        {
            for (var j = 0; j < Elems.Length - 1; j++)
            {
                if (j == 0)
                {
                    sw.Write("\tnext({0})", Elems[j].Value);
                }
                else switch (Elems[j].Type)
                {
                    case "=":
                    case "<=":
                        sw.Write(":=");
                        break;
                    case "t+1":
                        break;
                    default:
                    {
                        if (!Elems[j].Inverted) sw.Write(Elems[j].Value);
                        else sw.Write("~" + Elems[j].Value);
                        break;
                    }
                }

                sw.Flush();
            }
            sw.Write(";");
        }
    }
}