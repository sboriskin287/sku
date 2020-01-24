using System;
using System.Text;
using System.IO;
using sku_to_smv.Properties;
using System.Text.RegularExpressions;
using sku_to_smv.src;
using System.Collections.Generic;

namespace sku_to_smv
{
    public enum parceResult { PARSE_OK, PARCE_ERROR };

    public class Parser
    {
        enum state { H, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, ERR, END, EXT, OPT, OPT_END, TIME, O1, O2, O3, O4, O5 };
        enum parseStates { TARGET_STATE, SOURCE_STATE, TIME, SIGNAL, OPERATION, INPUT_SIGNALS, SKU }

        int RCount;                    //Количество правил полученных после анализа
        bool Inv;
        bool isSKU;
        bool isOUT;

        public Rule[] Rules;           //Массив правил, строящийся при анализе
        public Rule[] OutputRules;     //Массив правил, строящийся при анализе
        public string[] LocalStates;   //Список локальных состояний
        public string[] Inputs;        //Список входных сигналов
        public string[] RecovStates;   //Список повторновходимых состояний
        public string[] Outputs;       //Список выходных сигналов
        public bool signalsParsed;
        public List<Signal> signals;
        public List<State> states;
        public Parser()
        {
            //Инициализируем массивы
            Rules = new Rule[0];
            LocalStates = new string[0];
            Inputs = new string[0];
            Outputs = new string[0];
            signals = new List<Signal>();
            states = new List<State>();
            RecovStates = new string[0];
            RCount = 0;
            signalsParsed = false;
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
        private void AddOutRule()
        {
            Array.Resize(ref OutputRules, OutputRules.Length + 1);
            OutputRules[Rules.Length - 1] = new Rule();
            //RCount++;
        }

        private bool isExistsState(String stateName)
        {
            foreach (State state in this.states)
            {
                if (state.Name.Equals(stateName)) return true;
            }
            return false;
        }

        private State getStateByName(String name)
        {
            foreach (State state in this.states)
            {
                if (state.Name.Equals(name)) return state;
            }
            throw new Exception("State with name " + name + " not found");
        }

        private Signal getSignalByName(String name)
        {
            foreach (Signal signal in this.signals)
            {
                if (signal.name.Equals(name)) return signal;
            }
            throw new Exception("State with name " + name + " not found");
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
            isSKU = true;
            isOUT = false;
            StringBuilder sb1 = new StringBuilder();
            Rules = new Rule[0];
            LocalStates = new string[0];
            Inputs = new string[0];
            Outputs = new string[0];
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
                if (parce(STR) == parceResult.PARCE_ERROR)
                {
                    return parceResult.PARCE_ERROR;
                }
            }
            //Составление списка локальных состояний//
            //LocalStates = new string[Rules.Length];
            /*for (int i = 0; i < Rules.Length; i++)
            {
                if (Rules[i].Elems[0].Output)
                {
                    Array.Resize(ref Outputs, Outputs.Length + 1);
                    Outputs[Outputs.Length - 1] = Rules[i].Elems[0].Value;
                }
                else
                {
                    Array.Resize(ref LocalStates, LocalStates.Length + 1);
                    LocalStates[LocalStates.Length - 1] = Rules[i].Elems[0].Value;
                }
            }
            SearchForInputs();//Составление списка входных сигналов//
            SearchForRecov();//Составление списка повторновходимых состояний//*/

            return parceResult.PARSE_OK;
        }
        /// <summary>
        /// Разбор очередной строки
        /// </summary>
        /// <param name="InputString">Строка для разбора</param>
        /// <param name="StartState">Начальное состояние парсера</param>
        /// <returns>Результат разбора типа parceResult</returns>
        /*private parceResult Parse(String InputString, state StartState)
        {
            state st1 = StartState;
            int k = 0;
            string tmp;
            string time = "";
            for (int i = 0; i < InputString.Length; i++)
            {
                if (!signalsParsed) parce(InputString);
                switch (st1)
                {
                    //Буква
                    case state.H:
                        if (InputString[i] == '[')
                            st1 = state.OPT;
                        else if (Regex.IsMatch(InputString[i].ToString(), "[a-z]"))
                        {
                            k = i;
                            //Rules[Rules.Length - 1].output = isOUT;
                            st1 = (isSKU) ? state.S1 : state.O1;
                        }
                        else st1 = state.ERR;
                        break;
                    //Если новая секция
                    case state.OPT:
                        if (InputString[i] == 's')
                        {
                            isSKU = true;
                            isOUT = false;
                            //Rules[Rules.Length - 1].output = false;
                            st1 = state.OPT_END;
                        }
                        else if (InputString[i] == 'o')
                        {
                            isSKU = false;
                            isOUT = true;
                            //Rules[Rules.Length - 1].output = true;
                            st1 = state.OPT_END;
                        }
                        else st1 = state.ERR;
                        break;
                    //Ожидаем конца определения секции
                    case state.OPT_END:
                        if (InputString[i] == ']')
                            st1 = state.H;
                        break;
                    case state.O1:
                        if (Regex.IsMatch(InputString[i].ToString(), "[a-z0-9]"))
                        {
                            st1 = state.O2;
                        }
                        else if (InputString[i] == '{')
                        {
                            st1 = state.TIME;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.O2:
                        if (InputString[i] == '<')
                        {
                            st1 = state.O3;
                            tmp = InputString.Substring(k, i - k);
                            if (tmp.Contains("{")) tmp = tmp.Substring(0, tmp.IndexOf("{"));
                            //Rules[Rules.Length - 1].AddData("State", tmp, false, isOUT);
                            k = i + 1;
                        }
                        else if (Regex.IsMatch(InputString[i].ToString(), "[a-z0-9]"))
                        {
                            st1 = state.O2;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.O3:
                        if (InputString[i] == '=')
                        {
                            st1 = state.O4;
                            //Rules[Rules.Length - 1].AddData("<=", "<=", false, isOUT);
                            k = i + 1;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.O4:
                        if (Regex.IsMatch(InputString[i].ToString(), "[a-z]"))
                        {
                            k = i;
                            st1 = state.O5;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.O5:
                        if (Regex.IsMatch(InputString[i].ToString(), "[a-z0-9]"))
                        {
                            st1 = state.O5;
                        }
                        else if (InputString[i] == ';')
                        {
                            tmp = InputString.Substring(k, i - k);
                            if (tmp.Contains("{")) tmp = tmp.Substring(0, tmp.IndexOf("{"));
                            Rules[Rules.Length - 1].AddData("State", tmp, false, isOUT);
                            st1 = state.END;
                        }
                        else st1 = state.ERR;
                        break;
                    //Буква или цифра или '(' или '='
                    case state.S1:
                        if (Regex.IsMatch(InputString[i].ToString(), "[a-z0-9]"))
                        {
                            st1 = state.S1;
                        }
                        else if (InputString[i] == '(')
                        {
                            st1 = state.S2;
                            tmp = InputString.Substring(k, i - k);
                            if (tmp.Contains("{")) tmp = tmp.Substring(0, tmp.IndexOf("{"));
                            Rules[Rules.Length - 1].AddData("State", tmp, false, isOUT);
                            k = i + 1;
                        }
                        else if (InputString[i] == '=')
                        {
                            st1 = state.S7;
                            tmp = InputString.Substring(k, i - k);
                            if (tmp.Contains("{")) tmp = tmp.Substring(0, tmp.IndexOf("{"));
                            Rules[Rules.Length - 1].AddData("State", tmp, false, isOUT);
                            Rules[Rules.Length - 1].AddData("=", "=", false);
                            k = i + 1;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S2:
                        if (InputString[i] == 't')
                        {
                            st1 = state.S3;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S3:
                        if (InputString[i] == '+')
                        {
                            st1 = state.S4;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S4:
                        if (InputString[i] == '1')
                        {
                            st1 = state.S5;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S5:
                        if (InputString[i] == ')')
                        {
                            Rules[Rules.Length - 1].AddData("t+1", "(t+1)", false);
                            k = i + 1;
                            st1 = state.S6;
                        }
                        else st1 = state.ERR;
                        break;
                    case state.S6:
                        if (InputString[i] == '=')
                        {
                            Rules[Rules.Length - 1].AddData("=", "=", false);
                            k = i + 1;
                            st1 = state.S7;
                        }
                        else st1 = state.ERR;
                        break;
                    //Буква или '(' или '~'
                    case state.S7:
                        if (Regex.IsMatch(InputString[i].ToString(), "[0-9]"))
                        {
                            st1 = state.S7;
                        }
                        else if (Regex.IsMatch(InputString[i].ToString(), "[a-z]"))
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
                            i = k - 1;
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
                    case state.S8:
                        if (InputString[i] == ')')
                        {
                            Rules[Rules.Length - 1].AddData(")", ")", false);
                            st1 = state.S9;
                        }
                        else st1 = state.ERR;
                        break;
                    // '&' или '|' или ';'
                    case state.S9:
                        if (InputString[i] == '&')
                        {
                            Rules[Rules.Length - 1].AddData("&", "&", false);
                            st1 = state.S7;
                        }
                        else if (InputString[i] == '|')
                        {
                            Rules[Rules.Length - 1].AddData("|", "|", false);
                            st1 = state.S7;
                        }
                        else if (InputString[i] == ';')
                        {
                            st1 = state.END;
                        }
                        break;
                    // Буква или '(' или
                    case state.S10:
                        if (Regex.IsMatch(InputString[i].ToString(), "[a-z]"))
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
                            i = k - 1;
                            st1 = state.S8;
                        }
                        break;
                    case state.S11:
                        if (Regex.IsMatch(InputString[i].ToString(), "[a-z0-9]"))
                        {
                            st1 = state.S11;
                        }
                        else if (InputString[i] == '{')
                        {
                            st1 = state.TIME;
                        }
                        else if (InputString[i] == '&')
                        {
                            st1 = state.S7;
                            string stateName = InputString.Substring(k, i - k);
                            if (stateName.Contains("{")) stateName = stateName.Substring(0, stateName.IndexOf("{"));
                            Rules[Rules.Length - 1].AddData("State", stateName, Inv);
                            Rules[Rules.Length - 1].AddData("&", "&", false);
                        }
                        else if (InputString[i] == '|')
                        {
                            st1 = state.S7;
                            string stateName = InputString.Substring(k, i - k);
                            if (stateName.Contains("{")) stateName = stateName.Substring(0, stateName.IndexOf("{"));
                            Rules[Rules.Length - 1].AddData("State", stateName, Inv);
                            Rules[Rules.Length - 1].AddData("|", "|", false);
                        }
                        else if (InputString[i] == ';')
                        {
                            st1 = state.END;
                            string stateName = InputString.Substring(k, i - k);
                            if (stateName.Contains("{")) stateName = stateName.Substring(0, stateName.IndexOf("{"));
                            Rules[Rules.Length - 1].AddData("State", stateName, Inv);
                        }
                        else st1 = state.ERR;
                        break;
                    case state.TIME:
                        if (InputString[i] == '}')
                        {
                            Rules[Rules.Length - 1].AddData("TimeTransfer", time, Inv);
                            st1 = state.S11;
                            time = "";
                            break;
                        }
                        time += InputString[i].ToString();
                        st1 = state.TIME;
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
        }*/

        private parceResult parce(String inputStr)
        {
            if (inputStr.Contains("input="))
            {
                int firstBreaket = inputStr.IndexOf("[") + 1;
                int lastBreaket = inputStr.LastIndexOf("]");
                String signalStr = inputStr.Substring(firstBreaket, lastBreaket - firstBreaket);
                String[] parsedSignals = signalStr.Split(',');
                foreach (String s in parsedSignals)
                {
                    Signal signal = new Signal();
                    signal.name = s;
                    this.signals.Add(signal);
                }
                return parceResult.PARSE_OK;
            }
            String pattern = "(\\w+)=(\\(?[\\w\\|]+\\)?)((&\\w+)+)";
            String statePattern = "\\(?(\\w+)\\|?\\)?";
            String signalPattern = "&(\\w+)";
            String str = inputStr;
            Regex reg = new Regex(pattern, RegexOptions.Compiled);
            Regex stateReg = new Regex(statePattern, RegexOptions.Compiled);
            Regex signalReg = new Regex(signalPattern, RegexOptions.Compiled);
            Match match = reg.Match(str);

            State endState = null;
            List<State> beginStates = new List<State>();
            List<Signal> signals = new List<Signal>();
            List<Rule> rules = new List<Rule>();
            for (int i = 1; i < match.Groups.Count; i++)
            {
                switch (i)
                {
                    case 1:
                        {
                            String stateName = match.Groups[i].Value;
                            if (isExistsState(stateName))
                            {
                                endState = getStateByName(stateName);
                            }
                            else
                            {
                                endState = new State();
                                endState.Name = stateName;
                            }
                            break;
                        }
                    case 2:
                        {
                            MatchCollection matches = stateReg.Matches(match.Groups[i].Value);
                            foreach (Match m in matches)
                            {
                                for (int j = 1; j < m.Groups.Count; j++)
                                {
                                    String stateName = m.Groups[j].Value;
                                    State beginState;
                                    if (isExistsState(stateName))
                                    {
                                        beginState = getStateByName(stateName);
                                    }
                                    else
                                    {
                                        beginState = new State();
                                        beginState.Name = stateName;
                                    }
                                    beginStates.Add(beginState);
                                }
                            }
                            break;
                        }
                    case 3:
                        {
                            MatchCollection matches = signalReg.Matches(match.Groups[i].Value);
                            foreach (Match m in matches)
                            {
                                for (int j = 1; j < m.Groups.Count; j++)
                                {
                                    String signalName = m.Groups[j].Value;
                                    try
                                    {
                                        signals.Add(getSignalByName(signalName));
                                    }
                                    catch (Exception)
                                    {
                                        return parceResult.PARCE_ERROR;
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            if (endState == null)
            {
                return parceResult.PARCE_ERROR;
            }
            foreach (Signal signal in signals)
            {
                foreach (State state in beginStates)
                {
                    Rule rule = new Rule(state, endState, signal, 0);
                    rules.Add(rule);
                }
            }
            Array.Resize(ref Rules, rules.Count);
            this.Rules = rules.ToArray();
            return parceResult.PARSE_OK;
        }

        /// <summary>
        /// Функция поиска входных сигналов
        /// </summary>
        /*private void SearchForInputs()
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
                        if (!NotInput && !Exists && !Rules[i].Elems[j].Output)
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
        }*/
        /// <summary>
        /// Функция поиска повторновходимых состояний
        /// </summary>
        /*private void SearchForRecov()
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
        }*/
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
                if ((i != Inputs.Length - 1) || Outputs.Length > 0)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            for (int i = 0; i < Outputs.Length; i++)
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
            if (Outputs.Length > 0)
            {
                sw.Write("\toutput ");
                for (int i = 0; i < Outputs.Length; i++)
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
            for (int i = 0; i < Outputs.Length; i++)
            {
                sw.Write("\tnext(" + Outputs[i] + ") :=0;\r\n");
                sw.Flush();
            }
            sw.Write("}\r\n");
            sw.Write("in\r\n{\r\n");
            for (int i = 0; i < Rules.Length; i++)
            {
                //Rules[i].PrintRule(sw);
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
            for (int i = 0; i < Outputs.Length; i++)
            {
                sw.Write("SPEC EF " + Outputs[i] + ";\r\n");
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
        /*public void SaveToVHDL(string Path, bool CreateBus, int OuputSigCount, OutputTableElement[] OutTable)
        {
            String resultCode, tmp;
            int index;
            StringBuilder sb = new StringBuilder();
            StreamWriter sw = new StreamWriter(Path, false);
            FileInfo fi = new FileInfo(Path);
            resultCode = global::sku_to_smv.Properties.Resources.vhd_tmpl;
            //////////////////////////////////////////////////////////////////////////
            index = resultCode.IndexOf("$inputDescription");
            resultCode = resultCode.Remove(index, "$inputDescription".Length);
            if (CreateBus)
            {
                tmp = "-- Входные сигналы\n";
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
                for (int i = 0; i < Inputs.Length; i++)
                {
                    tmp = "-- " + Inputs[i] + "\t->\tinputs[" + i.ToString() + "]\n";
                    resultCode = resultCode.Insert(index, tmp);
                    index += tmp.Length;
                }
                int j = 0;
                tmp = "-- Выходные сигналы\n";
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
                for (int i = 0; i < OutTable.Length; i++)
                {
                    if (OutTable[i].HasOutput)
                    {
                        tmp = "-- " + OutTable[i].OutputName + "\t->\toutputs[" + j.ToString() + "]\n";
                        resultCode = resultCode.Insert(index, tmp);
                        index += tmp.Length;
                        ++j;
                    }
                }
            }
            //////////////////////////////////////////////////////////////////////////
            while ((index = resultCode.IndexOf("$name")) != -1)
            {
                resultCode = resultCode.Remove(index, "$name".Length);
                resultCode = resultCode.Insert(index, fi.Name.Replace(fi.Extension, ""));
            }
            //////////////////////////////////////////////////////////////////////////
            index = resultCode.IndexOf("$ports");
            resultCode = resultCode.Remove(index, "$ports".Length);
            if (CreateBus)
            {
                if (OuputSigCount > 0) tmp = "inputs				:	in		STD_LOGIC_VECTOR(" + (Inputs.Length - 1).ToString() + " downto 0);\n";
                else tmp = "inputs				:	in		STD_LOGIC_VECTOR(" + (Inputs.Length - 1).ToString() + " downto 0)";
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
            }
            else
            {
                for (int i = 0; i < Inputs.Length - 1; i++)
                {
                    tmp = "\t\t\t" + Inputs[i] + "				:	in		STD_LOGIC;\n";
                    resultCode = resultCode.Insert(index, tmp);
                    index += tmp.Length;
                }
                if (OuputSigCount > 0) tmp = "\t\t\t" + Inputs[Inputs.Length - 1] + "				:	in		STD_LOGIC;\n";
                else tmp = "\t\t\t" + Inputs[Inputs.Length - 1] + "				:	in		STD_LOGIC\n";
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
            }

            if (OuputSigCount > 0)
            {
                if (CreateBus)
                {
                    tmp = "\t\t\toutputs				:	out		STD_LOGIC_VECTOR(" + (OuputSigCount - 1).ToString() + " downto 0)";
                    resultCode = resultCode.Insert(index, tmp);
                }
                else
                {
                    for (int i = 0; i < OutTable.Length - 1; i++)
                    {
                        if (OutTable[i].HasOutput)
                        {
                            tmp = "\t\t\t" + OutTable[i].OutputName + "				:	out		STD_LOGIC;\n";
                            resultCode = resultCode.Insert(index, tmp);
                            index += tmp.Length;
                        }
                    }
                    if (OutTable[OutTable.Length - 1].HasOutput)
                    {
                        tmp = "\t\t\t" + OutTable[OutTable.Length - 1].OutputName + "				:	out		STD_LOGIC\n";
                        resultCode = resultCode.Insert(index, tmp);
                    }
                    else resultCode = resultCode.Remove(index - 2, 1);
                }
            }
            //////////////////////////////////////////////////////////////////////////
            while ((index = resultCode.IndexOf("$width")) != -1)
            {
                resultCode = resultCode.Remove(index, "$width".Length);
                resultCode = resultCode.Insert(index, (Rules.Length - 1).ToString());
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
            index = resultCode.IndexOf("$localDescription");
            resultCode = resultCode.Remove(index, "$localDescription".Length);

            tmp = "-- Состояния автомата\n";
            resultCode = resultCode.Insert(index, tmp);
            index += tmp.Length;
            for (int i = 0; i < Rules.Length; i++)
            {
                if (Rules[i].output)
                {
                    tmp = "-- not used\t->\tcurState[" + i.ToString() + "]\n";
                }
                else tmp = "-- " + Rules[i].Elems[0].Value + "\t->\tcurState[" + i.ToString() + "]\n";
                resultCode = resultCode.Insert(index, tmp);
                index += tmp.Length;
            }
            //////////////////////////////////////////////////////////////////////////
            index = resultCode.IndexOf("$rules");
            resultCode = resultCode.Remove(index, "$rules".Length);
            for (int i = 0; i < Rules.Length; i++)
            {
                if (!Rules[i].output)
                {
                    //sb.Append("\t\tif (");
                    sb.Append("\n\t\tnewState(" + i.ToString() + ") := ");
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
                                                //sb.Append("curState(" + n.ToString() + ")  = '0'");
                                                sb.Append("(not curState(" + n.ToString() + "))");
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
                                                //                                                 if (CreateBus)
                                                //                                                     sb.Append("inputs(" + n.ToString() + ")  = '0'");
                                                //                                                 else sb.Append(Inputs[n] + "  = '0'");
                                                if (CreateBus)
                                                    sb.Append("(not inputs(" + n.ToString() + "))");
                                                else sb.Append("(not " + Inputs[n] + ")");
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
                                                //sb.Append("curState(" + n.ToString() + ")  = '1'");
                                                sb.Append("curState(" + n.ToString() + ")");
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
                                                //                                                 if (CreateBus)
                                                //                                                     sb.Append("inputs(" + n.ToString() + ")  = '1'");
                                                //                                                 else sb.Append(Inputs[n] + "  = '1'");
                                                if (CreateBus)
                                                    sb.Append("inputs(" + n.ToString() + ")");
                                                else sb.Append(Inputs[n]);
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
                    int co = 0;
                    int k = 0;
                    for (k = 0; k < OutTable.Length; k++)
                    {
                        if (Rules[i].Elems[0].Value == OutTable[k].StateName && OutTable[k].HasOutput)
                        {
                            tmp = OutTable[k].OutputName;
                            break;
                        }
                        else if (OutTable[k].HasOutput)
                            co++;
                    }
                    if (CreateBus)
                    {
                        if (k < OutTable.Length && OutTable[k].HasOutput)
                        {
                            //sb.Append(") then " + "newState(" + i.ToString() + ") := '1';\n\t\t\toutputs(" + co.ToString() + ") <= '1';");
                            //sb.Append("\n\t\telse " + "newState(" + i.ToString() + ") := '0';\n\t\t\toutputs(" + co.ToString() + ") <= '0';");
                            sb.Append(";\n\t\t\toutputs(" + co.ToString() + ") <= " + "newState(" + i.ToString() + ")");
                        }
                        else
                        {
                            //sb.Append(") then " + "newState(" + i.ToString() + ") := '1';");
                            //sb.Append("\n\t\telse " + "newState(" + i.ToString() + ") := '0';");
                        }
                    }
                    else
                    {
                        if (k < OutTable.Length && OutTable[k].HasOutput)
                        {
                            //sb.Append(") then " + "newState(" + i.ToString() + ") := '1';\n\t\t\t" + tmp.ToString() + " <= '1';");
                            //sb.Append("\n\t\telse " + "newState(" + i.ToString() + ") := '0';\n\t\t\t" + tmp.ToString() + " <= '0';");
                            sb.Append(";\n\t\t\t" + tmp.ToString() + " <= " + "newState(" + i.ToString() + ")");
                        }
                        else
                        {
                            //sb.Append(") then " + "newState(" + i.ToString() + ") := '1';");
                            //sb.Append("\n\t\telse " + "newState(" + i.ToString() + ") := '0';");
                        }
                    }
                    //sb.AppendLine("\n\t\tend if;");
                    sb.Append(";");
                }
            }
            resultCode = resultCode.Insert(index, sb.ToString());
            sw.Write(resultCode);
            sw.Flush();
            sw.Close();
        }
    }*/

        public class SympleParser
        {
            enum state { START, COMMENT, SIGNAL, OPT };

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
                        case state.START:
                            if (str[i] == '#')
                            {
                                Array.Resize(ref elements, elements.Length + 1);
                                currentElement++;
                                elements[currentElement] = new SympleElement();
                                elements[currentElement].StartIndex = i;
                                CurrentState = state.COMMENT;
                            }
                            else if (Regex.IsMatch(str[i].ToString(), "[a-z]"))
                            {
                                Array.Resize(ref elements, elements.Length + 1);
                                currentElement++;
                                elements[currentElement] = new SympleElement();
                                elements[currentElement].StartIndex = i;
                                CurrentState = state.SIGNAL;
                            }
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
                            else if (str[i] == '[')
                            {
                                Array.Resize(ref elements, elements.Length + 1);
                                currentElement++;
                                elements[currentElement] = new SympleElement();
                                elements[currentElement].StartIndex = i;
                                CurrentState = state.OPT;
                            }
                            break;
                        case state.COMMENT:
                            if (str[i] == '#')
                            {
                                elements[currentElement].EndIndex = i;
                                elements[currentElement].TextColor = Settings.Default.TextFieldCommentColor;
                                elements[currentElement].Style = 0;
                                CurrentState = state.START;
                            }
                            break;
                        case state.OPT:
                            if (str[i] == ']')
                            {
                                elements[currentElement].EndIndex = i;
                                elements[currentElement].TextColor = Settings.Default.TextFieldOptionColor;
                                elements[currentElement].Style = 0;
                                CurrentState = state.START;
                            }
                            break;
                        case state.SIGNAL:
                            if (str[i] != ';' && str[i] != '(' && str[i] != ')' && str[i] != '&' && str[i] != '|' && str[i] != '~' && str[i] != '#' && str[i] != '=')
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

                                CurrentState = state.START;
                            }
                            break;
                    }
                }

                return parceResult.PARSE_OK;
            }
        }
    }
}
