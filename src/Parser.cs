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

        public Rule[] rules;           //Массив правил, строящийся при анализе
        public Rule[] rulesOutput;     //Массив правил, строящийся при анализе
        public string[] localStates;   //Список локальных состояний
        public string[] inputs;        //Список входных сигналов
        public string[] statesRecov;   //Список повторновходимых состояний
        public string[] outputs;       //Список выходных сигналов
        public Parser()
        {
            //Инициализируем массивы
            rules = new Rule[0];
            localStates = new string[0];
            inputs = new string[0];
            outputs = new string[0];
            statesRecov = new string[0];
        }
        /// <summary>
        /// Добавляет новое правило в массив
        /// </summary>
        private void addRule()
        {
            Array.Resize(ref rules, rules.Length + 1);
            rules[rules.Length - 1] = new Rule();
        }

        /// <summary>
        /// Запуск парсера
        /// </summary>
        /// <param name="inputString">Текст для разбора</param>
        /// <returns>Результат разбора типа parceResult</returns>
        public parceResult parseStart(string inputString)
        {
            var inputStr = inputString;
            var endOfRules = new int[0];
            var bComment = false;
            isSKU = true;
            isOUT = false;
            var sb1 = new StringBuilder();
            rules = new Rule[0];
            localStates = new string[0];
            inputs = new string[0];
            outputs = new string[0];
            statesRecov = new string[0];
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
                addRule();
                if (parse(str, state.H) == parceResult.PARCE_ERROR)
                {
                    return parceResult.PARCE_ERROR;
                }
            }
            //Составление списка локальных состояний//
            foreach (var rule in rules)
            {
                if (rule.Elems[0].Output)
                {
                    Array.Resize(ref outputs, outputs.Length + 1);
                    outputs[outputs.Length-1] = rule.Elems[0].Value;
                }
                else
                {
                    Array.Resize(ref localStates, localStates.Length + 1);
                    localStates[localStates.Length - 1] = rule.Elems[0].Value;
                }
            }
            searchForInputs();//Составление списка входных сигналов//
            searchForRecov();//Составление списка повторновходимых состояний//

            return parceResult.PARSE_OK;
        }
        
        /// <summary>
        /// Разбор очередной строки
        /// </summary>
        /// <param name="inputString">Строка для разбора</param>
        /// <param name="startState">Начальное состояние парсера</param>
        /// <returns>Результат разбора типа parceResult</returns>
        private parceResult parse(string inputString, state startState)
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
                                rules[rules.Length - 1].output = isOUT;
                                st1 = state.S1;
                            }
                            else
                            {
                                k = i;
                                rules[rules.Length - 1].output = isOUT;
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
                            rules[rules.Length - 1].output = false;
                            st1 = state.OPT_END;
                        }
                        else if (inputString[i] == 'o')
                        {
                            isSKU = false;
                            isOUT = true;
                            rules[rules.Length - 1].output = true;
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
                            rules[rules.Length - 1].AddData("State", tmp, false, isOUT);
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
                            rules[rules.Length - 1].AddData("<=", "<=", false, isOUT);
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
                            rules[rules.Length - 1].AddData("State", tmp, false, isOUT);
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
                            rules[rules.Length - 1].AddData("State", tmp, false, isOUT);
                            k = i + 1;
                        }
                        else if (inputString[i] == '=')
                        {
                            st1 = state.S7;
                            tmp = inputString.Substring(k, i - k);
                            if (tmp.Contains("{")) tmp = tmp.Substring(0, tmp.IndexOf("{", StringComparison.Ordinal));
                            rules[rules.Length - 1].AddData("State", tmp, false, isOUT);
                            rules[rules.Length - 1].AddData("=", "=", false);
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
                            rules[rules.Length - 1].AddData("t+1", "(t+1)", false);
                            k = i + 1;
                            st1 = state.S6;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S6: if (inputString[i] == '=')
                        {
                            rules[rules.Length - 1].AddData("=", "=", false);
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
                            rules[rules.Length - 1].AddData("(", "(", Inv);
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
                            if (parse(string.Format("{0};", inputString.Substring(i + 1, k - i - 1)), state.S7) == parceResult.PARCE_ERROR)
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
                            rules[rules.Length - 1].AddData(")", ")", false);
                            st1 = state.S9;
                        }
                        else st1 = state.ERR;
                        break;
                    // '&' или '|' или ';'
                    case state.S9: if (inputString[i] == '&')
                        {                                                                              
                            rules[rules.Length - 1].AddData("&", "&", false);                           
                            st1 = state.S7;
                        }
                        else if (inputString[i] == '|')
                        {                                                     
                            rules[rules.Length - 1].AddData("|", "|", false);                                                          
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
                            rules[rules.Length - 1].AddData("(", "(", Inv);
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
                            if (parse(inputString.Substring(i + 1, k - i - 1) + ";", state.S7) == parceResult.PARCE_ERROR)
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
                            rules[rules.Length - 1].AddData("State", stateName, Inv);
                            rules[rules.Length - 1].AddData("&", "&", false);
                        }
                        else if (inputString[i] == '|')
                        {
                            st1 = state.S7;
                            var stateName = inputString.Substring(k, i - k);
                            if (stateName.Contains("{")) stateName = stateName.Substring(0, stateName.IndexOf("{", StringComparison.Ordinal));
                            rules[rules.Length - 1].AddData("State", stateName, Inv);
                            rules[rules.Length - 1].AddData("|", "|", false);
                        }
                        else if (inputString[i] == ';')
                        {
                            st1 = state.END;
                            var stateName = inputString.Substring(k, i - k);
                            if (stateName.Contains("{")) stateName = stateName.Substring(0, stateName.IndexOf("{", StringComparison.Ordinal));
                            rules[rules.Length - 1].AddData("State", stateName, Inv);
                        }
                        else st1 = state.ERR;
                        break;
                    case state.TIME:
                        if (inputString[i] == '}')
                        {
                            rules[rules.Length - 1].AddData("TimeTransfer", time, Inv);
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
        private void searchForInputs()
        {
            var notInput = false;
            var exists = false;
            foreach (var rules in rules)
            {
                foreach (var element in rules.Elems)
                {
                    if (element.Type != "State") continue;
                    if (localStates.Any(localState => element.Value == localState))
                    {
                        notInput = true;
                    }
                    if (inputs.Any(input => element.Value == input))
                    {
                        exists = true;
                        element.Local = false;
                    }
                    if (!notInput && !exists && !element.Output)
                    {
                        Array.Resize(ref inputs, inputs.Length + 1);
                        inputs[inputs.Length - 1] = element.Value;
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
        private void searchForRecov()
        {
            foreach (var rule in rules)
            {
                for (var j = 1; j < rule.Elems.Length; j++)
                {
                    if (rule.Elems[0].Value != rule.Elems[j].Value) continue;
                    Array.Resize(ref statesRecov, statesRecov.Length + 1);
                    statesRecov[statesRecov.Length - 1] = rule.Elems[j].Value;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Функция сохранения результата в SMV
        /// </summary>
        /// <param name="path">Путь для сохранения</param>
        public void saveToSMV(string path)
        {
            var sw = new StreamWriter(path, false);
            sw.Write("module main(");
            sw.Flush();
            for (var i = 0; i < inputs.Length; i++)
            {
                sw.Write(inputs[i]);
                if (i != inputs.Length - 1 || outputs.Length > 0)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            for (var i = 0; i < outputs.Length; i++)
            {
                sw.Write(outputs[i]);
                if (i != outputs.Length - 1)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(")\r\n{\r\n");
            sw.Write("\tinput ");
            for (var i = 0; i < inputs.Length; i++)
            {
                sw.Write(inputs[i]);
                if (i != inputs.Length - 1)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(": boolean;\r\n");
            if (outputs.Length > 0)
            {
                sw.Write("\toutput ");
                for (var i = 0; i < outputs.Length; i++)
                {
                    sw.Write(outputs[i]);
                    if (i != outputs.Length - 1)
                    {
                        sw.Write(",");
                    }
                    sw.Flush();
                }
                sw.Write(": boolean;\r\n");
            }
            sw.Write("\tVAR ");
            for (var i = 0; i < localStates.Length; i++)
            {
                sw.Write(localStates[i]);
                if (i != localStates.Length - 1)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(": boolean;\r\n");
            sw.Write("ASSIGN\r\n");
            foreach (var input in inputs)
            {
                sw.Write("\tinit({0}) :=0;\r\n", input);
                sw.Flush();
            }
            foreach (var localState in localStates)
            {
                sw.Write("\tinit({0}) :=0;\r\n", localState);
                sw.Flush();
            }
            sw.Write("default\r\n{");
            foreach (var localState in localStates)
            {
                sw.Write("\tnext({0}) :=0;\r\n", localState);
                sw.Flush();
            }
            foreach (var output in outputs)
            {
                sw.Write("\tnext({0}) :=0;\r\n", output);
                sw.Flush();
            }
            sw.Write("}\r\n");
            sw.Write("in\r\n{\r\n");
            foreach (var rule in rules)
            {
                rule.PrintRule(sw);
                sw.Write("\r\n");
                sw.Flush();
            }
            sw.Write("}\r\n\r\n");
            sw.Write("--Reachability of states\r\n");
            foreach (var localState in localStates)
            {
                sw.Write("SPEC EF {0};\r\n", localState);
                sw.Flush();
            }
            foreach (var output in outputs)
            {
                sw.Write("SPEC EF {0};\r\n", output);
                sw.Flush();
            }
            sw.Write("--Recoverability of states\r\n");
            foreach (var recovState in statesRecov)
            {
                sw.Write("SPEC AG ( {0} -> AF {0} );\r\n", recovState);
                sw.Flush();
            }
            sw.Write("}");
            sw.Flush();
            sw.Close();
        }
        
        public void saveToVHDL(string path, bool createBus, int ouputSigCount, OutputTableElement[] outTable)
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
                for (var i = 0; i < inputs.Length; i++)
                {
                    tmp = string.Format("-- {0}\t->\tinputs[{1}]\n", inputs[i], i);
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
                    : "inputs				:	in		STD_LOGIC_VECTOR({0} downto 0)", inputs.Length - 1);
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
            }
            else
            {
                for (var i = 0; i < inputs.Length-1; i++ )
                {
                    tmp = string.Format("\t\t\t{0}				:	in		STD_LOGIC;\n", inputs[i]);
                    resultCode = resultCode.Insert(index, tmp);
                    index += tmp.Length;
                }

                tmp = string.Format(ouputSigCount > 0 ?
                    "\t\t\t{0}				:	in		STD_LOGIC;\n"
                    : "\t\t\t{0}				:	in		STD_LOGIC\n", inputs[inputs.Length - 1]);
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
                resultCode = resultCode.Insert(index, (rules.Length-1).ToString());
            }

            for (var i = 0; i < rules.Length; i++)
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
            for (int i = 0; i < rules.Length; i++)
            {
                if (rules[i].output)
                {
                    tmp = string.Format("-- not used\t->\tcurState[{0}]\n", i);
                }
                else tmp = string.Format("-- {0}\t->\tcurState[{1}]\n", rules[i].Elems[0].Value, i);
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
            }

            index = resultCode.IndexOf("$rules", StringComparison.Ordinal);
            resultCode = resultCode.Remove(index, "$rules".Length);
            for (var i = 0; i < rules.Length; i++)
            {
                if (rules[i].output)
                {
                    continue;
                }
                sb.AppendFormat("\n\t\tnewState({0}) := ", i);
                for (var j = 1; j < rules[i].Elems.Length; j++)
                {
                    if (rules[i].Elems[j].Type == "=" || rules[i].Elems[j].Type == "t+1" ||
                        rules[i].Elems[j].Empty) continue;
                    if (rules[i].Elems[j].Type == "State")
                    {
                        if (rules[i].Elems[j].Inverted)
                        {
                            if (rules[i].Elems[j].Local)
                            {
                                for (var n = 0; n < rules.Length; n++)
                                {
                                    if (rules[i].Elems[j].Value != rules[n].Elems[0].Value) continue;
                                    sb.AppendFormat("(not curState({0}))", n);
                                    break;
                                }
                            }
                            else
                            {
                                for (var n = 0; n < inputs.Length; n++)
                                {
                                    if (inputs[n] != rules[i].Elems[j].Value) continue;
                                    if (createBus)
                                        sb.AppendFormat("(not inputs({0}))", n);
                                    else sb.AppendFormat("(not {0})", inputs[n]);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (rules[i].Elems[j].Local)
                            {
                                for (var n = 0; n < rules.Length; n++)
                                {
                                    if (rules[i].Elems[j].Value != rules[n].Elems[0].Value) continue;
                                    sb.AppendFormat("curState({0})", n);
                                    break;
                                }
                            }
                            else
                            {
                                for (var n = 0; n < inputs.Length; n++)
                                {
                                    if (inputs[n] != rules[i].Elems[j].Value) continue;
                                    if (createBus)
                                        sb.AppendFormat("inputs({0})", n);
                                    else sb.Append(inputs[n]);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (rules[i].Elems[j].Type == "|") sb.Append(" or ");
                        if (rules[i].Elems[j].Type == "&") sb.Append(" and ");
                        if (rules[i].Elems[j].Type == "(")
                        {
                            if (rules[i].Elems[j].Inverted)
                            {
                                sb.Append(" not ");
                            }
                            sb.Append("(");
                        }
                        if (rules[i].Elems[j].Type == ")")
                        {
                            sb.Append(")");
                        }
                    }
                }
                var co = 0;
                int k;
                for (k = 0; k < outTable.Length; k++)
                {
                    if (rules[i].Elems[0].Value == outTable[k].StateName && outTable[k].HasOutput)
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
        state stateCurrent;
        int currentElement;
        string str;

        public SympleParser()
        {
            b_Brackets = false;
            elements = new SympleElement[0];
            stateCurrent = state.START;
            brackets = new SympleElement();
            brackets.StartIndex = -1;
            brackets.EndIndex = -1;
        }

        public void start(string input)
        {
            elements = new SympleElement[0];
            stateCurrent = state.START;
            currentElement = -1;
            str = input.ToLower();
            
            for (var i = 0; i < str.Length; i++)
            {
                switch (stateCurrent)
                {
                    case state.START: if (str[i] == '#')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement {StartIndex = i};
                            stateCurrent = state.COMMENT;
                        }
                        else if (Regex.IsMatch(str[i].ToString(), "[a-z]"))
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement {StartIndex = i};
                            stateCurrent = state.SIGNAL;
                        }
                        else if (str[i] == '(')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement
                            {
                                StartIndex = i, EndIndex = i, Style = System.Drawing.FontStyle.Bold
                            };
                            stateCurrent = state.START;
                        }
                        else if (str[i] == ')')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement
                            {
                                StartIndex = i, EndIndex = i, Style = System.Drawing.FontStyle.Bold
                            };
                            stateCurrent = state.START;
                        }
                        else if (str[i] == '[')
                        {
                            Array.Resize(ref elements, elements.Length + 1);
                            currentElement++;
                            elements[currentElement] = new SympleElement {StartIndex = i};
                            stateCurrent = state.OPT;
                        }
                        break;
                    case state.COMMENT: if (str[i] == '#')
                        {
                            elements[currentElement].EndIndex = i;
                            elements[currentElement].TextColor = Settings.Default.TextFieldCommentColor;
                            elements[currentElement].Style = 0;
                            stateCurrent = state.START;
                        }
                        break;
                    case state.OPT: if (str[i] == ']')
                        {
                            elements[currentElement].EndIndex = i;
                            elements[currentElement].TextColor = Settings.Default.TextFieldOptionColor;
                            elements[currentElement].Style = 0;
                            stateCurrent = state.START;
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
                                stateCurrent = state.COMMENT;
                            }
                        
                            if (str[i] == '(')
                            {
                                Array.Resize(ref elements, elements.Length + 1);
                                currentElement++;
                                elements[currentElement] = new SympleElement
                                {
                                    StartIndex = i, EndIndex = i, Style = System.Drawing.FontStyle.Bold
                                };
                                stateCurrent = state.START;
                            }
                            if (str[i] == ')')
                            {
                                Array.Resize(ref elements, elements.Length + 1);
                                currentElement++;
                                elements[currentElement] = new SympleElement
                                {
                                    StartIndex = i, EndIndex = i, Style = System.Drawing.FontStyle.Bold
                                };
                                stateCurrent = state.START;
                            }
                        
                            stateCurrent = state.START;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
