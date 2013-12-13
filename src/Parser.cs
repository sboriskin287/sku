using System;
using System.Text;
using System.IO;
using sku_to_smv.Properties;

namespace sku_to_smv
{
    public enum parceResult{PARSE_OK, PARCE_ERROR};

    public class Parser
    {
        enum state { H, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, ERR, END, EXT };

        int RCount;             //Количество правил полученных после анализа
        bool Inv;

        public Rule[] Rules;           //Массив правил, строящийся при анализе
        public string[] LocalStates;   //Список локальных состояний
        public string[] Inputs;        //Список входных сигналов
        public string[] RecovStates;   //Список повторновходимых состояний
        public Parser()
        {
            //Инициализируем массивы
            Rules = new Rule[0];
            LocalStates = new string[0];
            Inputs = new string[0];
            RecovStates = new string[0];
            RCount = 0;
        }
        /// <summary>
        /// Добавляет новое правило в массив
        /// </summary>
        private void AddRule()
        {
            Array.Resize(ref Rules, Rules.Length + 1);
            Rules[Rules.Length - 1] = new Rule();
            RCount++;
        }
        /// <summary>
        /// Запуск парсера
        /// </summary>
        /// <param name="InputString">Текст для разбора</param>
        /// <returns>Результат разбора типа parceResult</returns>
        public parceResult ParseStart(String InputString)
        {
            int k = 0; 
            string STR;
            string inputSTR = InputString;
            int[] EndOfRules = new int[0];
            bool b_Comment = false;
            StringBuilder sb1 = new StringBuilder();
            Rules = new Rule[0];
            LocalStates = new string[0];
            Inputs = new string[0];
            RecovStates = new string[0];
            GC.Collect();

            //Убираем пробелы, знаки табуляции, перехода на новую строку и комментарии
            for (int i = 0; i < inputSTR.Length; i++)
            {
                if (inputSTR[i] == '#' && !b_Comment) b_Comment = true;
                else if (inputSTR[i] == '#' && b_Comment) b_Comment = false;
                if (inputSTR[i] != 32 && inputSTR[i] != '\t' && inputSTR[i] != '\n' && !b_Comment && inputSTR[i] != '#')
                {
                    sb1.Append(inputSTR[i]);
                }
            }

            inputSTR = sb1.ToString();
            //Поиск всех знаков ';'
            for (int i = 0; i < inputSTR.Length; i++)
            {
                if (inputSTR[i] == ';')
                {
                    Array.Resize(ref EndOfRules, EndOfRules.Length + 1);
                    EndOfRules[EndOfRules.Length - 1] = i;
                }
            }
            //Переводим в нижний регистр
            inputSTR = inputSTR.ToLower();

            sb1.Clear();
            k = 0;
            for (int i = 0; i < EndOfRules.Length; i++)
            {
                //Выделение одного правила
                STR = inputSTR.Substring(k, EndOfRules[i] - k + 1);
                k = EndOfRules[i] + 1;
                //Разбор правила
                AddRule();
                if (Parse(STR, state.H) == parceResult.PARCE_ERROR)
                {
                    return parceResult.PARCE_ERROR;
                }
            }
            //Составление списка локальных состояний//
            LocalStates = new string[Rules.Length];
            for (int i = 0; i < Rules.Length; i++)
            {
                LocalStates[i] = Rules[i].Elems[0].Value;
            }
            SearchForInputs();//Составление списка входных сигналов//
            SearchForRecov();//Составление списка повторновходимых состояний//

            return parceResult.PARSE_OK;
        }
        /// <summary>
        /// Разбор очередной строки
        /// </summary>
        /// <param name="InputString">Строка для разбора</param>
        /// <param name="StartState">Начальное состояние парсера</param>
        /// <returns>Результат разбора типа parceResult</returns>
        private parceResult Parse(String InputString, state StartState)
        {
            state st1 = StartState;
            int k = 0;
            string tmp;
            for (int i = 0; i < InputString.Length; i++ )
            {
                switch (st1)
                {
                    //Буква
                    case state.H: if (InputString[i] == 'a' || InputString[i] == 'b' || InputString[i] == 'c' || InputString[i] == 'd' || InputString[i] == 'e' || InputString[i] == 'f' || InputString[i] == 'g' || InputString[i] == 'h' || InputString[i] == 'i' || InputString[i] == 'j' || InputString[i] == 'k' || InputString[i] == 'l' || InputString[i] == 'm' || InputString[i] == 'n' || InputString[i] == 'o' || InputString[i] == 'p' || InputString[i] == 'q' || InputString[i] == 'r' || InputString[i] == 's' || InputString[i] == 't' || InputString[i] == 'u' || InputString[i] == 'v' || InputString[i] == 'w' || InputString[i] == 'x' || InputString[i] == 'y' || InputString[i] == 'z')
                        {
                            k = i;
                            st1 = state.S1;
                        }
                        else
                        {
                            st1 = state.ERR;
                        }
                        break;
                    //Буква или цифра или '(' или '='
                    case state.S1: if (InputString[i] == 'a' || InputString[i] == 'b' || InputString[i] == 'c' || InputString[i] == 'd' || InputString[i] == 'e' || InputString[i] == 'f' || InputString[i] == 'g' || InputString[i] == 'h' || InputString[i] == 'i' || InputString[i] == 'j' || InputString[i] == 'k' || InputString[i] == 'l' || InputString[i] == 'm' || InputString[i] == 'n' || InputString[i] == 'o' || InputString[i] == 'p' || InputString[i] == 'q' || InputString[i] == 'r' || InputString[i] == 's' || InputString[i] == 't' || InputString[i] == 'u' || InputString[i] == 'v' || InputString[i] == 'w' || InputString[i] == 'x' || InputString[i] == 'y' || InputString[i] == 'z' || InputString[i] == '0' || InputString[i] == '1' || InputString[i] == '2' || InputString[i] == '3' || InputString[i] == '4' || InputString[i] == '5' || InputString[i] == '6' || InputString[i] == '7' || InputString[i] == '8' || InputString[i] == '9')
                        {
                            st1 = state.S1;
                        }
                        else if (InputString[i] == '(')
                        {
                            st1 = state.S2;
                            tmp = InputString.Substring(k, i - k);
                            Rules[Rules.Length - 1].AddData("State", tmp, false);
                            k = i + 1;
                        }
                        else if (InputString[i] == '=')
                        {
                            st1 = state.S7;
                            tmp = InputString.Substring(k, i - k);
                            Rules[Rules.Length - 1].AddData("State", tmp, false);
                            Rules[Rules.Length - 1].AddData("=", "=", false);
                            k = i + 1;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S2: if (InputString[i] == 't')
                        {
                            st1 = state.S3;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S3: if (InputString[i] == '+')
                        {
                            st1 = state.S4;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S4: if (InputString[i] == '1')
                        {
                            st1 = state.S5;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S5: if (InputString[i] == ')')
                        {
                            Rules[Rules.Length - 1].AddData("t+1", "(t+1)", false);
                            k = i + 1;
                            st1 = state.S6;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S6: if (InputString[i] == '=')
                        {
                            Rules[Rules.Length - 1].AddData("=", "=", false);
                            k = i + 1;
                            st1 = state.S7;
                        }
                        else st1 = state.ERR;
                        break;
                    //Буква или '(' или '~'
                    case state.S7: if (InputString[i] == 'a' || InputString[i] == 'b' || InputString[i] == 'c' || InputString[i] == 'd' || InputString[i] == 'e' || InputString[i] == 'f' || InputString[i] == 'g' || InputString[i] == 'h' || InputString[i] == 'i' || InputString[i] == 'j' || InputString[i] == 'k' || InputString[i] == 'l' || InputString[i] == 'm' || InputString[i] == 'n' || InputString[i] == 'o' || InputString[i] == 'p' || InputString[i] == 'q' || InputString[i] == 'r' || InputString[i] == 's' || InputString[i] == 't' || InputString[i] == 'u' || InputString[i] == 'v' || InputString[i] == 'w' || InputString[i] == 'x' || InputString[i] == 'y' || InputString[i] == 'z')
                        {
                            k = i;
                            Inv = false;
                            st1 = state.S11;
                        }
                        else if (InputString[i] == '(')
                        {
                            Inv = false;
                            Rules[Rules.Length - 1].AddData("(", "(", Inv);
                            int n = 0;
                            for (int j = i; j < InputString.Length; j++ )
                            {
                                if (InputString[j] == '(')
                                {
                                    n++;
                                }
                                if (InputString[j] == ')')
                                {
                                    n--;
                                }
                                if (n==0)
                                {
                                    k = j;
                                    break;
                                }
                            }
                            if (Parse(InputString.Substring(i + 1, k - i - 1) + ";", state.S7) == parceResult.PARCE_ERROR)
                            {
                                st1 = state.ERR;
                            }
                            i = k-1;
                            st1 = state.S8;
                        }
                        else if (InputString[i] == '~')
                        {
                            st1 = state.S10;
                            Inv = true;
                        }
                        else st1 = state.ERR;
                        break;
                    // ')'
                    case state.S8: if (InputString[i] == ')')
                        {
                            Rules[Rules.Length - 1].AddData(")", ")", false);
                            st1 = state.S9;
                        }
                        else st1 = state.ERR;
                        break;
                    // '&' или '|' или ';'
                    case state.S9: if (InputString[i] == '&')
                        {
                            st1 = state.S7;
                            Rules[Rules.Length - 1].AddData("&", "&", false);
                        }
                        else if (InputString[i] == '|')
                        {
                            st1 = state.S7;
                            Rules[Rules.Length - 1].AddData("|", "|", false);
                        }
                        else if (InputString[i] == ';')
                        {
                            st1 = state.END;
                        }
                        break;
                    // Буква или '(' или
                    case state.S10: if (InputString[i] == 'a' || InputString[i] == 'b' || InputString[i] == 'c' || InputString[i] == 'd' || InputString[i] == 'e' || InputString[i] == 'f' || InputString[i] == 'g' || InputString[i] == 'h' || InputString[i] == 'i' || InputString[i] == 'j' || InputString[i] == 'k' || InputString[i] == 'l' || InputString[i] == 'm' || InputString[i] == 'n' || InputString[i] == 'o' || InputString[i] == 'p' || InputString[i] == 'q' || InputString[i] == 'r' || InputString[i] == 's' || InputString[i] == 't' || InputString[i] == 'u' || InputString[i] == 'v' || InputString[i] == 'w' || InputString[i] == 'x' || InputString[i] == 'y' || InputString[i] == 'z')
                        {
                            k = i;
                            st1 = state.S11;
                        }
                        else if (InputString[i] == '(')
                        {
                            Inv = false;
                            Rules[Rules.Length - 1].AddData("(", "(", Inv);
                            int n = 0;
                            for (int j = i; j < InputString.Length; j++)
                            {
                                if (InputString[j] == '(')
                                {
                                    n++;
                                }
                                if (InputString[j] == ')')
                                {
                                    n--;
                                }
                                if (n == 0)
                                {
                                    k = j;
                                    break;
                                }
                            }
                            if (Parse(InputString.Substring(i + 1, k - i - 1) + ";", state.S7) == parceResult.PARCE_ERROR)
                            {
                                st1 = state.ERR;
                            }
                            i = k-1;
                            st1 = state.S8;
                        }
                        break;
                    case state.S11: if (InputString[i] == 'a' || InputString[i] == 'b' || InputString[i] == 'c' || InputString[i] == 'd' || InputString[i] == 'e' || InputString[i] == 'f' || InputString[i] == 'g' || InputString[i] == 'h' || InputString[i] == 'i' || InputString[i] == 'j' || InputString[i] == 'k' || InputString[i] == 'l' || InputString[i] == 'm' || InputString[i] == 'n' || InputString[i] == 'o' || InputString[i] == 'p' || InputString[i] == 'q' || InputString[i] == 'r' || InputString[i] == 's' || InputString[i] == 't' || InputString[i] == 'u' || InputString[i] == 'v' || InputString[i] == 'w' || InputString[i] == 'x' || InputString[i] == 'y' || InputString[i] == 'z' || InputString[i] == '0' || InputString[i] == '1' || InputString[i] == '2' || InputString[i] == '3' || InputString[i] == '4' || InputString[i] == '5' || InputString[i] == '6' || InputString[i] == '7' || InputString[i] == '8' || InputString[i] == '9')
                        {
                            st1 = state.S11;
                        }
                        else if (InputString[i] == '&')
                        {

                            st1 = state.S7;
                            //---//

                            Rules[Rules.Length - 1].AddData("State", InputString.Substring(k, i - k), Inv);
                            Rules[Rules.Length - 1].AddData("&", "&", false);

                        }
                        else if (InputString[i] == '|')
                        {

                            st1 = state.S7;
                            //---//

                            Rules[Rules.Length - 1].AddData("State", InputString.Substring(k, i - k), Inv);
                            Rules[Rules.Length - 1].AddData("|", "|", false);

                        }
                        else if (InputString[i] == ';')
                        {
                            st1 = state.END;
                            Rules[Rules.Length - 1].AddData("State", InputString.Substring(k, i - k), Inv);
                        }
                        else st1 = state.ERR;
                        break;
                    case state.ERR: //PrintText("Ошибка разбора");
                        //st1 = state.EXT;
                        break;
                    case state.END: //j++;
                        //AddRule();
                        //st1 = state.H;
                        break;
                    case state.EXT:
                        break;
                }
            }
            if (st1 == state.ERR)
            {
                return parceResult.PARCE_ERROR;
            }
            return parceResult.PARSE_OK;
        }
        /// <summary>
        /// Функция поиска входных сигналов
        /// </summary>
        private void SearchForInputs()
        {
            bool NotInput = false;
            bool Exists = false;
            for (int i = 0; i < Rules.Length; i++)
            {
                for (int j = 0; j < Rules[i].Elems.Length; j++)
                {
                    if (Rules[i].Elems[j].Type == "State")
                    {
                        for (int k = 0; k < LocalStates.Length; k++)
                        {
                            if (Rules[i].Elems[j].Value == LocalStates[k])
                            {
                                NotInput = true;
                                break;
                            }
                        }
                        for (int k = 0; k < Inputs.Length; k++)
                        {
                            if (Rules[i].Elems[j].Value == Inputs[k])
                            {
                                Exists = true;
                                Rules[i].Elems[j].Local = false;
                                break;
                            }
                        }
                        if (!NotInput && !Exists)
                        {
                            Array.Resize(ref Inputs, Inputs.Length + 1);
                            Inputs[Inputs.Length - 1] = Rules[i].Elems[j].Value;
                            Rules[i].Elems[j].Local = false;
                        }
                        NotInput = false;
                        Exists = false;
                    }
                }
            }
        }
        /// <summary>
        /// Функция поиска повторновходимых состояний
        /// </summary>
        private void SearchForRecov()
        {
            for (int i = 0; i < Rules.Length; i++)
            {
                for (int j = 1; j < Rules[i].Elems.Length; j++)
                {
                    if (Rules[i].Elems[0].Value == Rules[i].Elems[j].Value)
                    {
                        Array.Resize(ref RecovStates, RecovStates.Length + 1);
                        RecovStates[RecovStates.Length - 1] = Rules[i].Elems[j].Value;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Функция сохранения результата в SMV
        /// </summary>
        /// <param name="Path">Путь для сохранения</param>
        public void SaveToSMV(string Path)
        {
            StreamWriter sw = new StreamWriter(Path, false);
            sw.Write("module main(");
            sw.Flush();
            for (int i = 0; i < Inputs.Length; i++)
            {
                sw.Write(Inputs[i]);
                if (i != Inputs.Length - 1)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(")\r\n{\r\n");
            sw.Write("\tinput ");
            for (int i = 0; i < Inputs.Length; i++)
            {
                sw.Write(Inputs[i]);
                if (i != Inputs.Length - 1)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(": boolean;\r\n");
            //sw.Write("\toutput ");
            sw.Write("\tVAR ");
            for (int i = 0; i < LocalStates.Length; i++)
            {
                sw.Write(LocalStates[i]);
                if (i != LocalStates.Length - 1)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(": boolean;\r\n");
            sw.Write("ASSIGN\r\n");
            for (int i = 0; i < Inputs.Length; i++)
            {
                sw.Write("\tinit(" + Inputs[i] + ") :=0;\r\n");
                sw.Flush();
            }
            for (int i = 0; i < LocalStates.Length; i++)
            {
                sw.Write("\tinit(" + LocalStates[i] + ") :=0;\r\n");
                sw.Flush();
            }
            sw.Write("default\r\n{");
            for (int i = 0; i < LocalStates.Length; i++)
            {
                sw.Write("\tnext(" + LocalStates[i] + ") :=0;\r\n");
                sw.Flush();
            }
            sw.Write("}\r\n");
            sw.Write("in\r\n{\r\n");
            for (int i = 0; i < Rules.Length; i++)
            {
                Rules[i].PrintRule(sw);
                sw.Write("\r\n");
                sw.Flush();
            }
            sw.Write("}\r\n\r\n");
            sw.Write("--Reachability of states\r\n");
            for (int i = 0; i < LocalStates.Length; i++)
            {
                sw.Write("SPEC EF " + LocalStates[i] + ";\r\n");
                sw.Flush();
            }
            sw.Write("--Recoverability of states\r\n");
            for (int i = 0; i < RecovStates.Length; i++)
            {
                sw.Write("SPEC AG ( " + RecovStates[i] + " -> AF " + RecovStates[i] + " );\r\n");
                sw.Flush();
            }
            sw.Write("}");
            sw.Flush();
            sw.Close();
        }
        public void SaveToVHDL(string Path, bool CreateBus)
        {
            String resultCode;
            int index;
            StringBuilder sb = new StringBuilder();
            StreamWriter sw = new StreamWriter(Path, false);
            StreamReader fr = new StreamReader("template\\tmpl.vhd");
            FileInfo fi = new FileInfo(Path);
            resultCode = fr.ReadToEnd();
            //////////////////////////////////////////////////////////////////////////
            while ((index = resultCode.IndexOf("$name")) != -1)
            {
                resultCode = resultCode.Remove(index, "$name".Length);
                resultCode = resultCode.Insert(index, fi.Name.Replace(fi.Extension, ""));
            }
            //////////////////////////////////////////////////////////////////////////
            index = resultCode.IndexOf("$ports");
            resultCode = resultCode.Remove(index, "$ports".Length);
            resultCode = resultCode.Insert(index, (Inputs.Length-1).ToString());
            //////////////////////////////////////////////////////////////////////////
            while ((index = resultCode.IndexOf("$width")) != -1)
            {
                resultCode = resultCode.Remove(index, "$width".Length);
                resultCode = resultCode.Insert(index, (Rules.Length-1).ToString());
            }
            //////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < Rules.Length; i++)
            {
                sb.Append("0");
            }
            while ((index = resultCode.IndexOf("$zero")) != -1)
            {
                resultCode = resultCode.Remove(index, "$zero".Length);
                resultCode = resultCode.Insert(index, sb.ToString());
            }
            sb.Clear();
            //////////////////////////////////////////////////////////////////////////
            index = resultCode.IndexOf("$rules");
            resultCode = resultCode.Remove(index, "$rules".Length);
            for (int i = 0; i < Rules.Length; i++)
            {
                sb.Append("\t\tif (");
                for (int j = 1; j < Rules[i].Elems.Length; j++)
                {
                    if ((Rules[i].Elems[j].Type != "=") && (Rules[i].Elems[j].Type != "t+1") && (!Rules[i].Elems[j].Empty))
                    {
                        if (Rules[i].Elems[j].Type == "State")
                        {
                            if (Rules[i].Elems[j].Inverted)
                            {
                                if (Rules[i].Elems[j].Local)
                                {
                                    for (int n = 0; n < Rules.Length; n++)
                                    {
                                        if (Rules[i].Elems[j].Value == Rules[n].Elems[0].Value)
                                        {
                                            sb.Append("curState(" + n.ToString() + ")  = '0'");
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int n = 0; n < Inputs.Length; n++)
                                    {
                                        if (Inputs[n] == Rules[i].Elems[j].Value)
                                        {
                                            sb.Append("inputs(" + n.ToString() + ")  = '0'");
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Rules[i].Elems[j].Local)
                                {
                                    for (int n = 0; n < Rules.Length; n++)
                                    {
                                        if (Rules[i].Elems[j].Value == Rules[n].Elems[0].Value)
                                        {
                                            sb.Append("curState(" + n.ToString() + ")  = '1'");
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int n = 0; n < Inputs.Length; n++)
                                    {
                                        if (Inputs[n] == Rules[i].Elems[j].Value)
                                        {
                                            sb.Append("inputs(" + n.ToString() + ")  = '1'");
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Rules[i].Elems[j].Type == "|") sb.Append(" or ");
                            if (Rules[i].Elems[j].Type == "&") sb.Append(" and ");
                            if (Rules[i].Elems[j].Type == "(")
                            {
                                if (Rules[i].Elems[j].Inverted)
                                {
                                    sb.Append(" not ");
                                }
                                sb.Append("(");
                            }
                            if (Rules[i].Elems[j].Type == ")")
                            {
                                sb.Append(")");
                            }
                        }
                    }
                }
                sb.Append(") then " + "newState(" + i.ToString() + ") := '1';");
                sb.Append("\n\t\telse " + "newState(" + i.ToString() + ") := '0';");
                sb.AppendLine("\n\t\tend if;");
            }
            resultCode = resultCode.Insert(index, sb.ToString());
            sw.Write(resultCode);
            sw.Flush();
            sw.Close();
            fr.Close();
        }
    }

    public class SympleParser
    {
        enum state { START, COMMENT, SIGNAL };

        public SympleElement[] elements;
        public SympleElement brackets;
        public bool b_Brackets;
        state CurrentState;
        int currentElement;
        String str;

        public SympleParser()
        {
            b_Brackets = false;
            elements = new SympleElement[0];
            CurrentState = state.START;
            brackets = new SympleElement();
            brackets.StartIndex = -1;
            brackets.EndIndex = -1;
        }
        static public int FindBracket(String Input, int StartIndex, bool b_UpDown)
        {
            int counter = 0;
            if (b_UpDown)
            {
                for (int i = StartIndex; i < Input.Length; i++)
                {
                    if (Input[i] == '(')
                    {
                        counter++;
                    }
                    if (Input[i] == ')')
                    {
                        counter--;
                    }
                    if (counter == 0)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int i = StartIndex; i > 0; i--)
                {
                    if (Input[i] == ')')
                    {
                        counter++;
                    }
                    if (Input[i] == '(')
                    {
                        counter--;
                    }
                    if (counter == 0)
                    {
                        return i;
                    }
                }
            }
            return StartIndex;
        }
        public parceResult Start(String Input)
        {
            elements = new SympleElement[0];
            CurrentState = state.START;
            currentElement = -1;
            str = Input.ToLower();
            
            for (int i = 0; i < str.Length; i++)
            {
                switch (CurrentState)
                {
                    case state.START: if (str[i] == '#')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement();
                            elements[currentElement].StartIndex = i;
                            CurrentState = state.COMMENT;
                        }
                        else if (str[i] == 'a' || str[i] == 'b' || str[i] == 'c' || str[i] == 'd' || str[i] == 'e' || str[i] == 'f' || str[i] == 'g' || str[i] == 'h' || str[i] == 'i' || str[i] == 'j' || str[i] == 'k' || str[i] == 'l' || str[i] == 'm' || str[i] == 'n' || str[i] == 'o' || str[i] == 'p' || str[i] == 'q' || str[i] == 'r' || str[i] == 's' || str[i] == 't' || str[i] == 'u' || str[i] == 'v' || str[i] == 'w' || str[i] == 'x' || str[i] == 'y' || str[i] == 'z')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement();
                            elements[currentElement].StartIndex = i;
                            CurrentState = state.SIGNAL;
                        }
                        /*
                        else if (str[i] == '(')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement();
                            elements[currentElement].StartIndex = i;
                            elements[currentElement].EndIndex = i;
                            elements[currentElement].Style = System.Drawing.FontStyle.Bold;
                            CurrentState = state.START;
                        }
                        else if (str[i] == ')')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement();
                            elements[currentElement].StartIndex = i;
                            elements[currentElement].EndIndex = i;
                            elements[currentElement].Style = System.Drawing.FontStyle.Bold;
                            CurrentState = state.START;
                        }
                        */
                        break;
                    case state.COMMENT: if (str[i] == '#')
                        {
                            elements[currentElement].EndIndex = i;
                            elements[currentElement].TextColor = Settings.Default.TextFieldCommentColor;
                            elements[currentElement].Style = 0;
                            CurrentState = state.START;
                        }
                        break;
                    case state.SIGNAL: if (str[i] != ';' && str[i] != '(' && str[i] != ')' && str[i] != '&' && str[i] != '|' && str[i] != '~' && str[i] != '#')
                        {
                            break;
                        }
                        else
                        {
                            elements[currentElement].EndIndex = i - 1;
                            if (str.Substring(elements[currentElement].StartIndex, elements[currentElement].EndIndex - elements[currentElement].StartIndex + 1) == "t+1")
                            {
                                elements[currentElement].TextColor = System.Drawing.Color.Red;
                                elements[currentElement].Style = 0;
                            }
                            else
                            {
                                elements[currentElement].TextColor = Settings.Default.TextFieldSignalColor;
                                elements[currentElement].Style = 0;
                            }
                            if (str[i] == '#')
                            {
                                Array.Resize(ref elements, elements.Length + 1);
                                currentElement++;
                                elements[currentElement] = new SympleElement();
                                elements[currentElement].StartIndex = i;
                                CurrentState = state.COMMENT;
                            }
                        /*
                            if (str[i] == '(')
                            {
                                Array.Resize(ref elements, elements.Length + 1);
                                currentElement++;
                                elements[currentElement] = new SympleElement();
                                elements[currentElement].StartIndex = i;
                                elements[currentElement].EndIndex = i;
                                elements[currentElement].Style = System.Drawing.FontStyle.Bold;
                                CurrentState = state.START;
                            }
                            if (str[i] == ')')
                            {
                                Array.Resize(ref elements, elements.Length + 1);
                                currentElement++;
                                elements[currentElement] = new SympleElement();
                                elements[currentElement].StartIndex = i;
                                elements[currentElement].EndIndex = i;
                                elements[currentElement].Style = System.Drawing.FontStyle.Bold;
                                CurrentState = state.START;
                            }
                        */
                            CurrentState = state.START;
                        }
                        break;
                }
            }

            return parceResult.PARSE_OK;
        }
    }
}
