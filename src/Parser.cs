using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SCUConverterDrawArea.Properties;

namespace SCUConverterDrawArea
{
    public enum parceResult{PARSE_OK, PARCE_ERROR};

    public class Parser
    {
        enum state { H, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, ERR, END, EXT, OPT, OPT_END, TIME, O1, O2, O3, O4, O5 };

        bool Inv;
        bool isSKU;
        bool isOUT;

        public Rule[] Rules;           //Массив правил, строящийся при анализе
        public Rule[] OutputRules;     //Массив правил, строящийся при анализе
        public string[] LocalStates;   //Список локальных состояний
        public string[] Inputs;        //Список входных сигналов
        public string[] RecovStates;   //Список повторновходимых состояний
        public string[] Outputs;       //Список выходных сигналов
        public Parser()
        {
            //Инициализируем массивы
            Rules = new Rule[0];
            LocalStates = new string[0];
            Inputs = new string[0];
            Outputs = new string[0];
            RecovStates = new string[0];
        }
        /// <summary>
        /// Добавляет новое правило в массив
        /// </summary>
        private void AddRule()
        {
            Array.Resize(ref Rules, Rules.Length + 1);
            Rules[Rules.Length - 1] = new Rule();
        }

        /// <summary>
        /// Запуск парсера
        /// </summary>
        /// <param name="inputString">Текст для разбора</param>
        /// <returns>Результат разбора типа parceResult</returns>
        public parceResult ParseStart(string inputString)
        {
            var inputStr = inputString;
            var endOfRules = new int[0];
            var bComment = false;
            isSKU = true;
            isOUT = false;
            var sb1 = new StringBuilder();
            Rules = new Rule[0];
            LocalStates = new string[0];
            Inputs = new string[0];
            Outputs = new string[0];
            RecovStates = new string[0];
            GC.Collect();

            //Убираем пробелы, знаки табуляции, перехода на новую строку и комментарии
            foreach (var str in inputStr)
            {
                if (str == '#' && !bComment) bComment = true;
                else if (str == '#' && bComment) bComment = false;
                if (str != 32 && str != '\t' && str != '\n' && !bComment && str != '#')
                {
                    sb1.Append(str);
                }
            }
            inputStr = sb1.ToString();
            //Поиск всех знаков ';'
            for (var i = 0; i < inputStr.Length; i++)
            {
                if (inputStr[i] != ';')
                {
                    continue;
                }
                Array.Resize(ref endOfRules, endOfRules.Length + 1);
                endOfRules[endOfRules.Length - 1] = i;
            }
            //Переводим в нижний регистр
            inputStr = inputStr.ToLower();

            sb1.Clear();
            var k = 0;
            foreach (var endRule in endOfRules)
            {
                //Выделение одного правила
                var str = inputStr.Substring(k, endRule - k + 1);
                k = endRule + 1;
                //Разбор правила
                AddRule();
                if (Parse(str, state.H) == parceResult.PARCE_ERROR)
                {
                    return parceResult.PARCE_ERROR;
                }
            }
            //Составление списка локальных состояний//
            foreach (var rule in Rules)
            {
                if (rule.Elems[0].Output)
                {
                    Array.Resize(ref Outputs, Outputs.Length + 1);
                    Outputs[Outputs.Length-1] = rule.Elems[0].Value;
                }
                else
                {
                    Array.Resize(ref LocalStates, LocalStates.Length + 1);
                    LocalStates[LocalStates.Length - 1] = rule.Elems[0].Value;
                }
            }
            SearchForInputs();//Составление списка входных сигналов//
            SearchForRecov();//Составление списка повторновходимых состояний//

            return parceResult.PARSE_OK;
        }
        
        /// <summary>
        /// Разбор очередной строки
        /// </summary>
        /// <param name="inputString">Строка для разбора</param>
        /// <param name="startState">Начальное состояние парсера</param>
        /// <returns>Результат разбора типа parceResult</returns>
        private parceResult Parse(string inputString, state startState)
        {
            var st1 = startState;
            var k = 0;
            var time = "";
            for (var i = 0; i < inputString.Length; i++ )
            {
                string tmp;
                switch (st1)
                {
                    //Буква
                    case state.H: if (inputString[i] == '[')
                            st1 = state.OPT;
                        else if (Regex.IsMatch(inputString[i].ToString(), "[a-z]"))
                        {
                            if (isSKU)
                            {
                                k = i;
                                Rules[Rules.Length - 1].output = isOUT;
                                st1 = state.S1;
                            }
                            else
                            {
                                k = i;
                                Rules[Rules.Length - 1].output = isOUT;
                                st1 = state.O1;
                            }
                        }
                        else st1 = state.ERR;
                        break;
                    //Если новая секция
                    case state.OPT: if (inputString[i] == 's')
                        {
                            isSKU = true;
                            isOUT = false;
                            Rules[Rules.Length - 1].output = false;
                            st1 = state.OPT_END;
                        }
                        else if (inputString[i] == 'o')
                        {
                            isSKU = false;
                            isOUT = true;
                            Rules[Rules.Length - 1].output = true;
                            st1 = state.OPT_END;
                        }
                        else st1 = state.ERR;
                        break;
                    //Ожидаем конца определения секции
                    case state.OPT_END: if (inputString[i] == ']')
                            st1 = state.H;
                        break;
                    case state.O1:
                        if (Regex.IsMatch(inputString[i].ToString(), "[a-z0-9]"))
                        {
                            st1 = state.O2;
                        }
                        else if (inputString[i] == '{')
                        {
                            st1 = state.TIME;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.O2: if (inputString[i] == '<')
                        {
                            st1 = state.O3;
                            tmp = inputString.Substring(k, i - k);
                            if (tmp.Contains("{")) tmp = tmp.Substring(0, tmp.IndexOf("{"));
                            Rules[Rules.Length - 1].AddData("State", tmp, false, isOUT);
                            k = i + 1;
                        }
                        else if (Regex.IsMatch(inputString[i].ToString(), "[a-z0-9]"))
                        {
                            st1 = state.O2;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.O3: if (inputString[i] == '=')
                        {
                            st1 = state.O4;
                            Rules[Rules.Length - 1].AddData("<=", "<=", false, isOUT);
                            k = i + 1;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.O4: if (Regex.IsMatch(inputString[i].ToString(), "[a-z]"))
                        {
                            k = i;
                            st1 = state.O5;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.O5: if (Regex.IsMatch(inputString[i].ToString(), "[a-z0-9]"))
                        {
                            st1 = state.O5;
                        }
                        else if (inputString[i] == ';')
                        {
                            tmp = inputString.Substring(k, i - k);
                            if (tmp.Contains("{")) tmp = tmp.Substring(0, tmp.IndexOf("{", StringComparison.Ordinal));
                            Rules[Rules.Length - 1].AddData("State", tmp, false, isOUT);
                            st1 = state.END;
                        }
                        else st1 = state.ERR;
                        break;
                    //Буква или цифра или '(' или '='
                    case state.S1:
                        if (Regex.IsMatch(inputString[i].ToString(),"[a-z0-9]"))
                        {
                            st1 = state.S1;
                        }
                        else if (inputString[i] == '(')
                        {
                            st1 = state.S2;
                            tmp = inputString.Substring(k, i - k);
                            if (tmp.Contains("{")) tmp = tmp.Substring(0, tmp.IndexOf("{", StringComparison.Ordinal));
                            Rules[Rules.Length - 1].AddData("State", tmp, false, isOUT);
                            k = i + 1;
                        }
                        else if (inputString[i] == '=')
                        {
                            st1 = state.S7;
                            tmp = inputString.Substring(k, i - k);
                            if (tmp.Contains("{")) tmp = tmp.Substring(0, tmp.IndexOf("{", StringComparison.Ordinal));
                            Rules[Rules.Length - 1].AddData("State", tmp, false, isOUT);
                            Rules[Rules.Length - 1].AddData("=", "=", false);
                            k = i + 1;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S2: st1 = inputString[i] == 't' ? state.S3 : state.ERR;
                        break;
                    case state.S3: st1 = inputString[i] == '+' ? state.S4 : state.ERR;
                        break;
                    case state.S4: st1 = inputString[i] == '1' ? state.S5 : state.ERR;
                        break;
                    case state.S5: if (inputString[i] == ')')
                        {
                            Rules[Rules.Length - 1].AddData("t+1", "(t+1)", false);
                            k = i + 1;
                            st1 = state.S6;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S6: if (inputString[i] == '=')
                        {
                            Rules[Rules.Length - 1].AddData("=", "=", false);
                            k = i + 1;
                            st1 = state.S7;
                        }
                        else st1 = state.ERR;
                        break;
                    //Буква или '(' или '~'
                    case state.S7:
                        if (Regex.IsMatch(inputString[i].ToString(), "[0-9]"))
                        {
                            st1 = state.S7;
                        }
                        else if (Regex.IsMatch(inputString[i].ToString(),"[a-z]"))
                        {
                            k = i;
                            Inv = false;
                            st1 = state.S11;
                        }
                        else if (inputString[i] == '(')
                        {
                            Inv = false;
                            Rules[Rules.Length - 1].AddData("(", "(", Inv);
                            var n = 0;
                            for (var j = i; j < inputString.Length; j++ )
                            {
                                if (inputString[j] == '(')
                                {
                                    n++;
                                }
                                if (inputString[j] == ')')
                                {
                                    n--;
                                }

                                if (n != 0)
                                {
                                    continue;
                                }
                                k = j;
                                break;
                            }
                            if (Parse(string.Format("{0};", inputString.Substring(i + 1, k - i - 1)), state.S7) == parceResult.PARCE_ERROR)
                            {
                                st1 = state.ERR;
                            }
                            else
                            {
                                i = k-1;
                                st1 = state.S8;
                            }
                        }
                        else if (inputString[i] == '~')
                        {
                            st1 = state.S10;
                            Inv = true;
                        }
                        else st1 = state.ERR;
                        break;
                    // ')'
                    case state.S8: if (inputString[i] == ')')
                        {
                            Rules[Rules.Length - 1].AddData(")", ")", false);
                            st1 = state.S9;
                        }
                        else st1 = state.ERR;
                        break;
                    // '&' или '|' или ';'
                    case state.S9: if (inputString[i] == '&')
                        {                                                                              
                            Rules[Rules.Length - 1].AddData("&", "&", false);                           
                            st1 = state.S7;
                        }
                        else if (inputString[i] == '|')
                        {                                                     
                            Rules[Rules.Length - 1].AddData("|", "|", false);                                                          
                            st1 = state.S7;
                        }
                        else if (inputString[i] == ';')
                        {
                            st1 = state.END;
                        }
                        break;
                    // Буква или '(' или
                    case state.S10: if (Regex.IsMatch(inputString[i].ToString(),"[a-z]"))
                        {
                            k = i;
                            st1 = state.S11;
                        }
                        else if (inputString[i] == '(')
                        {
                            Inv = false;
                            Rules[Rules.Length - 1].AddData("(", "(", Inv);
                            var n = 0;
                            for (var j = i; j < inputString.Length; j++)
                            {
                                if (inputString[j] == '(')
                                {
                                    n++;
                                }
                                if (inputString[j] == ')')
                                {
                                    n--;
                                }
                                if (n == 0)
                                {
                                    k = j;
                                    break;
                                }
                            }
                            if (Parse(inputString.Substring(i + 1, k - i - 1) + ";", state.S7) == parceResult.PARCE_ERROR)
                            {
                                st1 = state.ERR;
                            }
                            else
                            {
                                i = k - 1;
                                st1 = state.S8;
                            }
                        }
                        break;
                    case state.S11:
                        if (Regex.IsMatch(inputString[i].ToString(), "[a-z0-9]"))
                        {
                            st1 = state.S11;
                        }
                        else if (inputString[i] == '{')
                        {
                            st1 = state.TIME;
                        }
                        else if (inputString[i] == '&')
                        {
                            st1 = state.S7;
                            var stateName = inputString.Substring(k, i - k);
                            if (stateName.Contains("{")) stateName = stateName.Substring(0, stateName.IndexOf("{", StringComparison.Ordinal));
                            Rules[Rules.Length - 1].AddData("State", stateName, Inv);
                            Rules[Rules.Length - 1].AddData("&", "&", false);
                        }
                        else if (inputString[i] == '|')
                        {
                            st1 = state.S7;
                            var stateName = inputString.Substring(k, i - k);
                            if (stateName.Contains("{")) stateName = stateName.Substring(0, stateName.IndexOf("{", StringComparison.Ordinal));
                            Rules[Rules.Length - 1].AddData("State", stateName, Inv);
                            Rules[Rules.Length - 1].AddData("|", "|", false);
                        }
                        else if (inputString[i] == ';')
                        {
                            st1 = state.END;
                            var stateName = inputString.Substring(k, i - k);
                            if (stateName.Contains("{")) stateName = stateName.Substring(0, stateName.IndexOf("{", StringComparison.Ordinal));
                            Rules[Rules.Length - 1].AddData("State", stateName, Inv);
                        }
                        else st1 = state.ERR;
                        break;
                    case state.TIME:
                        if (inputString[i] == '}')
                        {
                            Rules[Rules.Length - 1].AddData("TimeTransfer", time, Inv);
                            st1 = state.S11;
                            time = "";
                            break;
                        }
                        time += inputString[i].ToString();
                        st1 = state.TIME;                      
                        break;
                    case state.ERR:
                        break;
                    case state.END:
                        break;
                    case state.EXT:
                        break;
                    case state.S12:
                        break;
                    case state.S13:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return st1 == state.ERR ? parceResult.PARCE_ERROR : parceResult.PARSE_OK;
        }
        
        /// <summary>
        /// Функция поиска входных сигналов
        /// </summary>
        private void SearchForInputs()
        {
            var notInput = false;
            var exists = false;
            foreach (var rules in Rules)
            {
                foreach (var element in rules.Elems)
                {
                    if (element.Type != "State") continue;
                    if (LocalStates.Any(localState => element.Value == localState))
                    {
                        notInput = true;
                    }
                    if (Inputs.Any(input => element.Value == input))
                    {
                        exists = true;
                        element.Local = false;
                    }
                    if (!notInput && !exists && !element.Output)
                    {
                        Array.Resize(ref Inputs, Inputs.Length + 1);
                        Inputs[Inputs.Length - 1] = element.Value;
                        element.Local = false;
                    }
                    notInput = false;
                    exists = false;
                }
            }
        }
        
        /// <summary>
        /// Функция поиска повторновходимых состояний
        /// </summary>
        private void SearchForRecov()
        {
            foreach (var rule in Rules)
            {
                for (var j = 1; j < rule.Elems.Length; j++)
                {
                    if (rule.Elems[0].Value != rule.Elems[j].Value) continue;
                    Array.Resize(ref RecovStates, RecovStates.Length + 1);
                    RecovStates[RecovStates.Length - 1] = rule.Elems[j].Value;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Функция сохранения результата в SMV
        /// </summary>
        /// <param name="path">Путь для сохранения</param>
        public void SaveToSMV(string path)
        {
            var sw = new StreamWriter(path, false);
            sw.Write("module main(");
            sw.Flush();
            for (var i = 0; i < Inputs.Length; i++)
            {
                sw.Write(Inputs[i]);
                if (i != Inputs.Length - 1 || Outputs.Length > 0)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            for (var i = 0; i < Outputs.Length; i++)
            {
                sw.Write(Outputs[i]);
                if (i != Outputs.Length - 1)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(")\r\n{\r\n");
            sw.Write("\tinput ");
            for (var i = 0; i < Inputs.Length; i++)
            {
                sw.Write(Inputs[i]);
                if (i != Inputs.Length - 1)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(": boolean;\r\n");
            if (Outputs.Length > 0)
            {
                sw.Write("\toutput ");
                for (var i = 0; i < Outputs.Length; i++)
                {
                    sw.Write(Outputs[i]);
                    if (i != Outputs.Length - 1)
                    {
                        sw.Write(",");
                    }
                    sw.Flush();
                }
                sw.Write(": boolean;\r\n");
            }
            sw.Write("\tVAR ");
            for (var i = 0; i < LocalStates.Length; i++)
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
            foreach (var input in Inputs)
            {
                sw.Write("\tinit({0}) :=0;\r\n", input);
                sw.Flush();
            }
            foreach (var localState in LocalStates)
            {
                sw.Write("\tinit({0}) :=0;\r\n", localState);
                sw.Flush();
            }
            sw.Write("default\r\n{");
            foreach (var localState in LocalStates)
            {
                sw.Write("\tnext({0}) :=0;\r\n", localState);
                sw.Flush();
            }
            foreach (var output in Outputs)
            {
                sw.Write("\tnext({0}) :=0;\r\n", output);
                sw.Flush();
            }
            sw.Write("}\r\n");
            sw.Write("in\r\n{\r\n");
            foreach (var rule in Rules)
            {
                rule.PrintRule(sw);
                sw.Write("\r\n");
                sw.Flush();
            }
            sw.Write("}\r\n\r\n");
            sw.Write("--Reachability of states\r\n");
            foreach (var localState in LocalStates)
            {
                sw.Write("SPEC EF {0};\r\n", localState);
                sw.Flush();
            }
            foreach (var output in Outputs)
            {
                sw.Write("SPEC EF {0};\r\n", output);
                sw.Flush();
            }
            sw.Write("--Recoverability of states\r\n");
            foreach (var recovState in RecovStates)
            {
                sw.Write("SPEC AG ( {0} -> AF {0} );\r\n", recovState);
                sw.Flush();
            }
            sw.Write("}");
            sw.Flush();
            sw.Close();
        }
        
        public void SaveToVHDL(string path, bool createBus, int ouputSigCount, OutputTableElement[] outTable)
        {
            string tmp;
            var sb = new StringBuilder();
            var sw = new StreamWriter(path, false);
            var fi = new FileInfo(path);
            var resultCode = Resources.vhd_tmpl;

            var index = resultCode.IndexOf("$inputDescription", StringComparison.Ordinal);
            resultCode = resultCode.Remove(index, "$inputDescription".Length);
            if (createBus)
            {
                tmp = "-- Входные сигналы\n";
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
                for (var i = 0; i < Inputs.Length; i++)
                {
                    tmp = string.Format("-- {0}\t->\tinputs[{1}]\n", Inputs[i], i);
                    resultCode = resultCode.Insert(index, tmp);
                    index += tmp.Length;
                }
                var j = 0;
                tmp = "-- Выходные сигналы\n";
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
                foreach (var ot in outTable)
                {
                    if (!ot.HasOutput)
                    {
                        continue;
                    }
                    tmp = string.Format("-- {0}\t->\toutputs[{1}]\n", ot.OutputName, j);
                    resultCode = resultCode.Insert(index, tmp);
                    index += tmp.Length;
                    ++j;
                }
            }

            while ((index = resultCode.IndexOf("$name", StringComparison.Ordinal)) != -1)
            {
                resultCode = resultCode.Remove(index, "$name".Length);
                resultCode = resultCode.Insert(index, fi.Name.Replace(fi.Extension, ""));
            }

            index = resultCode.IndexOf("$ports", StringComparison.Ordinal);
            resultCode = resultCode.Remove(index, "$ports".Length);
            if (createBus)
            {
                tmp = string.Format(ouputSigCount > 0 ?
                    "inputs				:	in		STD_LOGIC_VECTOR({0} downto 0);\n" 
                    : "inputs				:	in		STD_LOGIC_VECTOR({0} downto 0)", Inputs.Length - 1);
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
            }
            else
            {
                for (var i = 0; i < Inputs.Length-1; i++ )
                {
                    tmp = string.Format("\t\t\t{0}				:	in		STD_LOGIC;\n", Inputs[i]);
                    resultCode = resultCode.Insert(index, tmp);
                    index += tmp.Length;
                }

                tmp = string.Format(ouputSigCount > 0 ?
                    "\t\t\t{0}				:	in		STD_LOGIC;\n"
                    : "\t\t\t{0}				:	in		STD_LOGIC\n", Inputs[Inputs.Length - 1]);
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
            }

            if (ouputSigCount > 0)
            {
                if (createBus)
                {
                    tmp = string.Format("\t\t\toutputs				:	out		STD_LOGIC_VECTOR({0} downTo 0)", (ouputSigCount - 1));
                    resultCode = resultCode.Insert(index, tmp);
                }
                else
                {
                    for (var i = 0; i < outTable.Length-1; i++)
                    {
                        if (!outTable[i].HasOutput) continue;
                        tmp = string.Format("\t\t\t{0}				:	out		STD_LOGIC;\n", outTable[i].OutputName);
                        resultCode = resultCode.Insert(index, tmp);
                        index += tmp.Length;
                    }
                    if (outTable[outTable.Length - 1].HasOutput)
                    {
                        tmp = string.Format("\t\t\t{0}				:	out		STD_LOGIC\n", outTable[outTable.Length - 1].OutputName);
                        resultCode = resultCode.Insert(index, tmp);
                    }
                    else resultCode = resultCode.Remove(index-2, 1);
                }
            }

            while ((index = resultCode.IndexOf("$width", StringComparison.Ordinal)) != -1)
            {
                resultCode = resultCode.Remove(index, "$width".Length);
                resultCode = resultCode.Insert(index, (Rules.Length-1).ToString());
            }

            for (var i = 0; i < Rules.Length; i++)
            {
                sb.Append("0");
            }
            while ((index = resultCode.IndexOf("$zero", StringComparison.Ordinal)) != -1)
            {
                resultCode = resultCode.Remove(index, "$zero".Length);
                resultCode = resultCode.Insert(index, sb.ToString());
            }
            sb.Clear();

            index = resultCode.IndexOf("$localDescription", StringComparison.Ordinal);
            resultCode = resultCode.Remove(index, "$localDescription".Length);

            tmp = "-- Состояния автомата\n";
            resultCode = resultCode.Insert(index, tmp);
            index += tmp.Length;
            for (int i = 0; i < Rules.Length; i++)
            {
                if (Rules[i].output)
                {
                    tmp = string.Format("-- not used\t->\tcurState[{0}]\n", i);
                }
                else tmp = string.Format("-- {0}\t->\tcurState[{1}]\n", Rules[i].Elems[0].Value, i);
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
            }

            index = resultCode.IndexOf("$rules", StringComparison.Ordinal);
            resultCode = resultCode.Remove(index, "$rules".Length);
            for (var i = 0; i < Rules.Length; i++)
            {
                if (Rules[i].output)
                {
                    continue;
                }
                sb.AppendFormat("\n\t\tnewState({0}) := ", i);
                for (var j = 1; j < Rules[i].Elems.Length; j++)
                {
                    if (Rules[i].Elems[j].Type == "=" || Rules[i].Elems[j].Type == "t+1" ||
                        Rules[i].Elems[j].Empty) continue;
                    if (Rules[i].Elems[j].Type == "State")
                    {
                        if (Rules[i].Elems[j].Inverted)
                        {
                            if (Rules[i].Elems[j].Local)
                            {
                                for (var n = 0; n < Rules.Length; n++)
                                {
                                    if (Rules[i].Elems[j].Value != Rules[n].Elems[0].Value) continue;
                                    sb.AppendFormat("(not curState({0}))", n);
                                    break;
                                }
                            }
                            else
                            {
                                for (var n = 0; n < Inputs.Length; n++)
                                {
                                    if (Inputs[n] != Rules[i].Elems[j].Value) continue;
                                    if (createBus)
                                        sb.AppendFormat("(not inputs({0}))", n);
                                    else sb.AppendFormat("(not {0})", Inputs[n]);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (Rules[i].Elems[j].Local)
                            {
                                for (var n = 0; n < Rules.Length; n++)
                                {
                                    if (Rules[i].Elems[j].Value != Rules[n].Elems[0].Value) continue;
                                    sb.AppendFormat("curState({0})", n);
                                    break;
                                }
                            }
                            else
                            {
                                for (var n = 0; n < Inputs.Length; n++)
                                {
                                    if (Inputs[n] != Rules[i].Elems[j].Value) continue;
                                    if (createBus)
                                        sb.AppendFormat("inputs({0})", n);
                                    else sb.Append(Inputs[n]);
                                    break;
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
                var co = 0;
                int k;
                for (k = 0; k < outTable.Length; k++)
                {
                    if (Rules[i].Elems[0].Value == outTable[k].StateName && outTable[k].HasOutput)
                    {
                        tmp = outTable[k].OutputName;
                        break;
                    }
                    else if (outTable[k].HasOutput)
                        co++;
                }
                if (createBus)
                {
                    if (k < outTable.Length && outTable[k].HasOutput)
                    {
                        sb.AppendFormat(";\n\t\t\toutputs({0}) <= newState({1})", co, i);
                    }
                }
                else
                {
                    if (k < outTable.Length && outTable[k].HasOutput)
                    {
                        sb.AppendFormat(";\n\t\t\t{0} <= newState({1})", tmp, i);
                    }
                }
                sb.Append(";");
            }
            resultCode = resultCode.Insert(index, sb.ToString());
            sw.Write(resultCode);
            sw.Flush();
            sw.Close();
        }
    }

    public class SympleParser
    {
        enum state { START, COMMENT, SIGNAL, OPT };

        public SympleElement[] elements;
        public SympleElement brackets;
        public bool b_Brackets;
        state CurrentState;
        int currentElement;
        string str;

        public SympleParser()
        {
            b_Brackets = false;
            elements = new SympleElement[0];
            CurrentState = state.START;
            brackets = new SympleElement();
            brackets.StartIndex = -1;
            brackets.EndIndex = -1;
        }

        public void Start(string input)
        {
            elements = new SympleElement[0];
            CurrentState = state.START;
            currentElement = -1;
            str = input.ToLower();
            
            for (var i = 0; i < str.Length; i++)
            {
                switch (CurrentState)
                {
                    case state.START: if (str[i] == '#')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement {StartIndex = i};
                            CurrentState = state.COMMENT;
                        }
                        else if (Regex.IsMatch(str[i].ToString(), "[a-z]"))
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement {StartIndex = i};
                            CurrentState = state.SIGNAL;
                        }
                        else if (str[i] == '(')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement
                            {
                                StartIndex = i, EndIndex = i, Style = System.Drawing.FontStyle.Bold
                            };
                            CurrentState = state.START;
                        }
                        else if (str[i] == ')')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement
                            {
                                StartIndex = i, EndIndex = i, Style = System.Drawing.FontStyle.Bold
                            };
                            CurrentState = state.START;
                        }
                        else if (str[i] == '[')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement {StartIndex = i};
                            CurrentState = state.OPT;
                        }
                        break;
                    case state.COMMENT: if (str[i] == '#')
                        {
                            elements[currentElement].EndIndex = i;
                            elements[currentElement].TextColor = Settings.Default.TextFieldCommentColor;
                            elements[currentElement].Style = 0;
                            CurrentState = state.START;
                        }
                        break;
                    case state.OPT: if (str[i] == ']')
                        {
                            elements[currentElement].EndIndex = i;
                            elements[currentElement].TextColor = Settings.Default.TextFieldOptionColor;
                            elements[currentElement].Style = 0;
                            CurrentState = state.START;
                        }
                        break;
                    case state.SIGNAL: if (str[i] != ';' && str[i] != '(' && str[i] != ')' && str[i] != '&' && str[i] != '|' && str[i] != '~' && str[i] != '#' && str[i] != '=')
                        {
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
                                elements[currentElement] = new SympleElement {StartIndex = i};
                                CurrentState = state.COMMENT;
                            }
                        
                            if (str[i] == '(')
                            {
                                Array.Resize(ref elements, elements.Length + 1);
                                currentElement++;
                                elements[currentElement] = new SympleElement
                                {
                                    StartIndex = i, EndIndex = i, Style = System.Drawing.FontStyle.Bold
                                };
                                CurrentState = state.START;
                            }
                            if (str[i] == ')')
                            {
                                Array.Resize(ref elements, elements.Length + 1);
                                currentElement++;
                                elements[currentElement] = new SympleElement
                                {
                                    StartIndex = i, EndIndex = i, Style = System.Drawing.FontStyle.Bold
                                };
                                CurrentState = state.START;
                            }
                        
                            CurrentState = state.START;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
