using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;


namespace sku_to_smv
{
    enum defines{NO_STATE_SELECTED = -1};

    enum state { H, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, ERR, END, EXT };

    public partial class Form1 : Form
    {
        bool StateSelected;
        bool AddStateButtonSelected;
        bool Saved, TextCH, Inv, Analysed;
        int xs, ys, xM, yM;
        int xT, yT;
        int SelectedState;
        int RCount;             //Количество правил полученных после анализа
        string sOpenFileName, sOpenSaveFileName, sSaveFileName;
        
        new float Scale;

        Rule[] Rules;           //Массив правил, строящийся при анализе
        State[] States;
        Link[] Links;
        Graphics g;
        Bitmap bm;
        Mutex Mu;

        string[] LocalStates;   //Список локальных состояний
        string[] Inputs;        //Список входных сигналов
        string[] RecovStates;   //Список повторновходимых состояний

        System.Drawing.Pen penDarkRed, penBlack, penRed, penOrange, penDarkBlue, penDarkGreen;
        System.Drawing.Font TextFont;
        
        public Form1()
        {
            InitializeComponent();
            //Инициализируем массивы
            Rules     = new Rule[1];
            Rules[0]  = new Rule();

            Inputs    = new string[1];
            Inputs[0] = null; 

            Links     = new Link[1];
            Links[0]  = new Link();

            RecovStates    = new string[1];
            RecovStates[0] = null;
            

            Saved = true;
            TextCH = false;
            Inv = false;
            Analysed = false;
            StateSelected = false;
            AddStateButtonSelected = false;

            RCount = 0;
            Scale  = 1.0f;
            xs = 50;
            ys = 50;
            xT = 0;
            yT = 0;

            Mu = new Mutex(false, "MyMutex");
            bm = new Bitmap(3000, 2000);            //графический буфер
            g  = Graphics.FromImage(bm);
            g.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//Включаем сглаживание графических объектов
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;//Включаем сглаживание шрифтов
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;//Включаем интерполяцию
            //Определяются цвета
            penDarkRed   =  new System.Drawing.Pen(System.Drawing.Brushes.DarkRed, 2);
            penBlack     =  new System.Drawing.Pen(System.Drawing.Brushes.Black, 1);
            penRed       =  new System.Drawing.Pen(System.Drawing.Brushes.Red, 3);
            penOrange    =  new System.Drawing.Pen(System.Drawing.Brushes.Orange, 3);
            penDarkBlue  =  new System.Drawing.Pen(System.Drawing.Brushes.DarkBlue, 1);
            penDarkGreen = new System.Drawing.Pen(System.Drawing.Brushes.DarkGreen, 3);
            //И шрифт
            TextFont     =  new System.Drawing.Font(richTextBox1.Font.Name, (14 * Scale));

            //Разрешаем отмену асинхронных операций
            this.backgroundWorker1.WorkerSupportsCancellation = true;
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "TXT files (*.txt)|*.txt";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Multiselect = false;
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.tabControl1.SelectedIndex = 0;
                Array.Resize(ref States, 0);
                Array.Resize(ref Links, 1);
                sOpenFileName = openFileDialog1.FileName;
                sOpenSaveFileName = openFileDialog1.SafeFileName;
                this.toolStripStatusLabel1.Text = "Открыт файл: " + sOpenFileName;
                this.toolStripProgressBar1.Value = 0;
                FileToTextbox();
                this.toolStripProgressBar1.Value = 100;
                Saved = false;
                TextCH = false;
                sOpenSaveFileName = sOpenSaveFileName.Remove(sOpenSaveFileName.Length - 4);
                Analysed = false;
            }
            
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "TXT files (*.txt)|*.txt";
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = sOpenSaveFileName;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFileDialog1.FileName.Length > 0)
            {
                this.toolStripProgressBar1.Value = 0;
                sSaveFileName = saveFileDialog1.FileName;
                TextboxToFile();
                this.toolStripStatusLabel1.Text = "Файл сохранен под именем: " + sSaveFileName;
                this.toolStripProgressBar1.Value = 100;
                Saved = true;
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TextCH)
            {
                сохранитьToolStripMenuItem_Click(sender, e);
            }
            this.Close();
        }

        private void FileToTextbox()//Загружает открытый файл для редактирования//
        {
            this.richTextBox1.Clear();
            StreamReader SR = new StreamReader(sOpenFileName);
            this.richTextBox1.AppendText(SR.ReadToEnd());
        }
        private void TextboxToFile()//Сохраняет отредактированный файл//
        {
            StreamWriter sw = new StreamWriter(sSaveFileName, false);
            sw.Write(this.richTextBox1.Text);
            sw.Flush();
            sw.Close();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            TextCH = true;
            Analysed = false;
        }
        private void PrintText(string temp)//Просто печатает текст в текстбокс
        {
            textBox1.Text = textBox1.Text + " " + temp;
        }
        private void AnalyseInput()//Функция разбора входного документа//
        {
            state st1 = state.H;
            string inputSTR = this.richTextBox1.Text;
            string STR, tmp;
            Array.Resize(ref LocalStates, 0);
            Array.Resize(ref Rules, 1);
            Array.Resize(ref Inputs, 1);
            Array.Resize(ref RecovStates, 1);
            Rules[0].Clear();
            RCount = 0;
            this.textBox1.Clear();
            StringBuilder sb1 = new StringBuilder();
            for (int i = 0; i < inputSTR.Length; i++)
            {
                if (inputSTR[i] != 32 && inputSTR[i] != '\t')
                {
                    sb1.Append(inputSTR[i]);
                }
            }
            STR = sb1.ToString();
            STR = STR.ToLower();
            //добавить перевод в нижний регистр
            sb1.Remove(0, sb1.Length);
            int j = 0;

            do
            {
                try
                {
                    switch (st1)
                    {
                        case state.H: if (STR[j] == 'a' || STR[j] == 'b' || STR[j] == 'c' || STR[j] == 'd' || STR[j] == 'e' || STR[j] == 'f' || STR[j] == 'g' || STR[j] == 'h' || STR[j] == 'i' || STR[j] == 'j' || STR[j] == 'k' || STR[j] == 'l' || STR[j] == 'm' || STR[j] == 'n' || STR[j] == 'o' || STR[j] == 'p' || STR[j] == 'q' || STR[j] == 'r' || STR[j] == 's' || STR[j] == 't' || STR[j] == 'u' || STR[j] == 'v' || STR[j] == 'w' || STR[j] == 'x' || STR[j] == 'y' || STR[j] == 'z')
                            {
                                sb1.Append(STR[j]);
                                j++;
                                st1 = state.S1;
                            }
                            else if (STR[j] == ';' || STR[j] == '\n')
                            {
                                j++;
                            }
                            else
                            {
                                st1 = state.ERR;
                            }
                            break;
                        case state.S1: if (STR[j] == 'a' || STR[j] == 'b' || STR[j] == 'c' || STR[j] == 'd' || STR[j] == 'e' || STR[j] == 'f' || STR[j] == 'g' || STR[j] == 'h' || STR[j] == 'i' || STR[j] == 'j' || STR[j] == 'k' || STR[j] == 'l' || STR[j] == 'm' || STR[j] == 'n' || STR[j] == 'o' || STR[j] == 'p' || STR[j] == 'q' || STR[j] == 'r' || STR[j] == 's' || STR[j] == 't' || STR[j] == 'u' || STR[j] == 'v' || STR[j] == 'w' || STR[j] == 'x' || STR[j] == 'y' || STR[j] == 'z' || STR[j] == '0' || STR[j] == '1' || STR[j] == '2' || STR[j] == '3' || STR[j] == '4' || STR[j] == '5' || STR[j] == '6' || STR[j] == '7' || STR[j] == '8' || STR[j] == '9')
                            {
                                sb1.Append(STR[j]);
                                j++;
                                st1 = state.S1;
                            }
                            else if (STR[j] == '(')
                            {
                                j++;
                                st1 = state.S2;

                                //---//
                                tmp = sb1.ToString();
                                //---//
                                Rules[RCount].AddData("State", tmp, false);
                                sb1.Remove(0, sb1.Length);
                            }
                            else if (STR[j] == '=')
                            {
                                j++;
                                st1 = state.S7;
                                //---//
                                tmp = sb1.ToString();
                                //---//
                                Rules[RCount].AddData("State", tmp, false);
                                Rules[RCount].AddData("=", "=", false);
                                sb1.Remove(0, sb1.Length);
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S2: if (STR[j] == 't')
                            {
                                j++;
                                st1 = state.S3;
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S3: if (STR[j] == '+')
                            {
                                j++;
                                st1 = state.S4;
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S4: if (STR[j] == '1')
                            {
                                j++;
                                st1 = state.S5;
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S5: if (STR[j] == ')')
                            {
                                j++;
                                Rules[RCount].AddData("t+1", "(t+1)", false);
                                st1 = state.S6;
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S6: if (STR[j] == '=')
                            {
                                j++;
                                Rules[RCount].AddData("=", "=", false);
                                st1 = state.S7;
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S7: if (STR[j] == 'a' || STR[j] == 'b' || STR[j] == 'c' || STR[j] == 'd' || STR[j] == 'e' || STR[j] == 'f' || STR[j] == 'g' || STR[j] == 'h' || STR[j] == 'i' || STR[j] == 'j' || STR[j] == 'k' || STR[j] == 'l' || STR[j] == 'm' || STR[j] == 'n' || STR[j] == 'o' || STR[j] == 'p' || STR[j] == 'q' || STR[j] == 'r' || STR[j] == 's' || STR[j] == 't' || STR[j] == 'u' || STR[j] == 'v' || STR[j] == 'w' || STR[j] == 'x' || STR[j] == 'y' || STR[j] == 'z')
                            {
                                sb1.Append(STR[j]);
                                j++;
                                Inv = false;
                                st1 = state.S13;
                            }
                            else if (STR[j] == '(')
                            {
                                st1 = state.S8;
                                Inv = false;
                                Rules[RCount].AddData("(", "(", false);
                                j++;
                            }
                            else if (STR[j] == '~')
                            {
                                st1 = state.S12;
                                Inv = true;
                                j++;
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S8: if (STR[j] == 'a' || STR[j] == 'b' || STR[j] == 'c' || STR[j] == 'd' || STR[j] == 'e' || STR[j] == 'f' || STR[j] == 'g' || STR[j] == 'h' || STR[j] == 'i' || STR[j] == 'j' || STR[j] == 'k' || STR[j] == 'l' || STR[j] == 'm' || STR[j] == 'n' || STR[j] == 'o' || STR[j] == 'p' || STR[j] == 'q' || STR[j] == 'r' || STR[j] == 's' || STR[j] == 't' || STR[j] == 'u' || STR[j] == 'v' || STR[j] == 'w' || STR[j] == 'x' || STR[j] == 'y' || STR[j] == 'z')
                            {
                                sb1.Append(STR[j]);
                                j++;
                                Inv = false;
                                st1 = state.S10;
                            }
                            else if (STR[j] == '~')
                            {
                                j++;
                                Inv = true;
                                st1 = state.S9;
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S9: if (STR[j] == 'a' || STR[j] == 'b' || STR[j] == 'c' || STR[j] == 'd' || STR[j] == 'e' || STR[j] == 'f' || STR[j] == 'g' || STR[j] == 'h' || STR[j] == 'i' || STR[j] == 'j' || STR[j] == 'k' || STR[j] == 'l' || STR[j] == 'm' || STR[j] == 'n' || STR[j] == 'o' || STR[j] == 'p' || STR[j] == 'q' || STR[j] == 'r' || STR[j] == 's' || STR[j] == 't' || STR[j] == 'u' || STR[j] == 'v' || STR[j] == 'w' || STR[j] == 'x' || STR[j] == 'y' || STR[j] == 'z')
                            {
                                sb1.Append(STR[j]);
                                j++;
                                st1 = state.S10;
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S10: if (STR[j] == 'a' || STR[j] == 'b' || STR[j] == 'c' || STR[j] == 'd' || STR[j] == 'e' || STR[j] == 'f' || STR[j] == 'g' || STR[j] == 'h' || STR[j] == 'i' || STR[j] == 'j' || STR[j] == 'k' || STR[j] == 'l' || STR[j] == 'm' || STR[j] == 'n' || STR[j] == 'o' || STR[j] == 'p' || STR[j] == 'q' || STR[j] == 'r' || STR[j] == 's' || STR[j] == 't' || STR[j] == 'u' || STR[j] == 'v' || STR[j] == 'w' || STR[j] == 'x' || STR[j] == 'y' || STR[j] == 'z' || STR[j] == '0' || STR[j] == '1' || STR[j] == '2' || STR[j] == '3' || STR[j] == '4' || STR[j] == '5' || STR[j] == '6' || STR[j] == '7' || STR[j] == '8' || STR[j] == '9')
                            {
                                sb1.Append(STR[j]);
                                j++;
                                st1 = state.S10;
                            }
                            else if (STR[j] == '&')
                            {
                                j++;
                                st1 = state.S8;
                                //---//
                                tmp = sb1.ToString();
                                Rules[RCount].AddData("State", tmp, Inv);
                                Rules[RCount].AddData("&", "&", false);
                                sb1.Remove(0, sb1.Length);
                            }
                            else if (STR[j] == '|')
                            {
                                j++;
                                st1 = state.S8;
                                //---//
                                tmp = sb1.ToString();
                                Rules[RCount].AddData("State", tmp, Inv);
                                Rules[RCount].AddData("|", "|", false);
                                sb1.Remove(0, sb1.Length);
                            }
                            else if (STR[j] == ')')
                            {
                                j++;
                                st1 = state.S11;
                                //---//
                                tmp = sb1.ToString();
                                Rules[RCount].AddData("State", tmp, Inv);
                                Rules[RCount].AddData(")", ")", false);
                                sb1.Remove(0, sb1.Length);
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S11: if (STR[j] == '&')
                            {
                                j++;
                                Rules[RCount].AddData("&", "&", false);
                                st1 = state.S7;
                            }
                            else if (STR[j] == '|')
                            {
                                j++;
                                Rules[RCount].AddData("|", "|", false);
                                st1 = state.S7;
                            }
                            else if (STR[j] == ';')
                            {
                                j++;
                                st1 = state.END;
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S12: if (STR[j] == 'a' || STR[j] == 'b' || STR[j] == 'c' || STR[j] == 'd' || STR[j] == 'e' || STR[j] == 'f' || STR[j] == 'g' || STR[j] == 'h' || STR[j] == 'i' || STR[j] == 'j' || STR[j] == 'k' || STR[j] == 'l' || STR[j] == 'm' || STR[j] == 'n' || STR[j] == 'o' || STR[j] == 'p' || STR[j] == 'q' || STR[j] == 'r' || STR[j] == 's' || STR[j] == 't' || STR[j] == 'u' || STR[j] == 'v' || STR[j] == 'w' || STR[j] == 'x' || STR[j] == 'y' || STR[j] == 'z')
                            {
                                sb1.Append(STR[j]);
                                j++;
                                st1 = state.S13;
                            }
                            else st1 = state.ERR;
                            break;
                        case state.S13: if (STR[j] == 'a' || STR[j] == 'b' || STR[j] == 'c' || STR[j] == 'd' || STR[j] == 'e' || STR[j] == 'f' || STR[j] == 'g' || STR[j] == 'h' || STR[j] == 'i' || STR[j] == 'j' || STR[j] == 'k' || STR[j] == 'l' || STR[j] == 'm' || STR[j] == 'n' || STR[j] == 'o' || STR[j] == 'p' || STR[j] == 'q' || STR[j] == 'r' || STR[j] == 's' || STR[j] == 't' || STR[j] == 'u' || STR[j] == 'v' || STR[j] == 'w' || STR[j] == 'x' || STR[j] == 'y' || STR[j] == 'z' || STR[j] == '0' || STR[j] == '1' || STR[j] == '2' || STR[j] == '3' || STR[j] == '4' || STR[j] == '5' || STR[j] == '6' || STR[j] == '7' || STR[j] == '8' || STR[j] == '9')
                            {
                                sb1.Append(STR[j]);
                                j++;
                                st1 = state.S13;
                            }
                            else if (STR[j] == '&')
                            {
                                j++;
                                st1 = state.S7;
                                //---//
                                tmp = sb1.ToString();
                                Rules[RCount].AddData("State", tmp, Inv);
                                Rules[RCount].AddData("&", "&", false);
                                sb1.Remove(0, sb1.Length);
                            }
                            else if (STR[j] == '|')
                            {
                                j++;
                                st1 = state.S7;
                                //---//
                                tmp = sb1.ToString();
                                Rules[RCount].AddData("State", tmp, Inv);
                                Rules[RCount].AddData("|", "|", false);
                                sb1.Remove(0, sb1.Length);
                            }
                            else if (STR[j] == ';')
                            {
                                st1 = state.END;
                                tmp = sb1.ToString();
                                Rules[RCount].AddData("State", tmp, Inv);
                                sb1.Remove(0, sb1.Length);
                            }
                            else st1 = state.ERR;
                            break;
                        case state.ERR: PrintText("Ошибка разбора");
                            st1 = state.EXT;
                            break;
                        case state.END: j++;
                            AddRule();
                            st1 = state.H;
                            break;
                        case state.EXT:
                            break;
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    st1 = state.EXT;
                    PrintText("Разбор окончен");
                }

            } while (st1 != state.EXT);
            //Составление списка локальных состояний//
            LocalStates = new string[Rules.Length];
            for (int i = 0; i < Rules.Length - 1; i++ )
            {
                LocalStates[i] = Rules[i].Elems[0].Value;
            }
            //
            SearchForInputs();//Составление списка входных сигналов//
            SearchForRecov();//Составление списка повторновходимых состояний//
            Analysed = true;
        }

        private void анализироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            AnalyseInput();

            /////////////Вывод в smv//////////////////////////////////
            saveFileDialog1.Filter = "SMV files (*.smv)|*.smv";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFileDialog1.FileName.Length > 0)
            {
                this.toolStripProgressBar1.Value = 0;
                sSaveFileName = saveFileDialog1.FileName;
                //TextboxToFile();
                SaveToSMV(sSaveFileName);
                this.toolStripStatusLabel1.Text = "Файл сохранен под именем: " + sSaveFileName;
                this.toolStripProgressBar1.Value = 100;
                //Saved = true;
            }
            
            ///////////////////////////////////////////////////
        }
        private void CreateGraf(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 1;
            //if (Analysed)
            //{
            //CreateStates();
            //CreateLinks();
            ////RefreshScreen();
            //}
            //else 
            //{
                AnalyseInput();
                CreateStates();
                CreateLinks();
                RefreshScreen();
            //}
        }
        private void AddRule()//Функция добавления нового правила//
        {
            Array.Resize(ref Rules, Rules.Length + 1);
            Rules[Rules.Length - 1] = new Rule();
            RCount++;
        }
        private void SaveToSMV(string Path)//Функция сохранения результата в SMV//
        {
            StreamWriter sw = new StreamWriter(Path, false);
            sw.Write("module main(");
            sw.Flush();
            for (int i = 0; i < Inputs.Length - 1; i++)
            {
                sw.Write(Inputs[i]);
                if (i != Inputs.Length - 2)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(")\r\n{\r\n");
            sw.Write("\tinput ");
            for (int i = 0; i < Inputs.Length - 1; i++)
            {
                sw.Write(Inputs[i]);
                if (i != Inputs.Length - 2)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(": boolean;\r\n");
            //sw.Write("\toutput ");
            sw.Write("\tVAR ");
            for (int i = 0; i < LocalStates.Length - 1; i++)
            {
                sw.Write(LocalStates[i]);
                if (i != LocalStates.Length - 2)
                {
                    sw.Write(",");
                }
                sw.Flush();
            }
            sw.Write(": boolean;\r\n");
            sw.Write("ASSIGN\r\n");
            for (int i = 0; i < Inputs.Length - 1; i++)
            {
                sw.Write("\tinit(" + Inputs[i] + ") :=0;\r\n");
                sw.Flush();
            }
            for (int i = 0; i < LocalStates.Length - 1; i++)
            {
                sw.Write("\tinit(" + LocalStates[i] + ") :=0;\r\n");
                sw.Flush();
            }
            sw.Write("default\r\n{");
            for (int i = 0; i < LocalStates.Length - 1; i++)
            {
                sw.Write("\tnext(" + LocalStates[i] + ") :=0;\r\n");
                sw.Flush();
            }
            sw.Write("}\r\n");
            sw.Write("in\r\n{\r\n");
            for (int i = 0; i < Rules.Length - 1; i++)
            {
                Rules[i].PrintRule(sw);
                sw.Write("\r\n");
                sw.Flush();
            }
            sw.Write("}\r\n\r\n");
            sw.Write("--Reachability of states\r\n");
            for (int i = 0; i < LocalStates.Length - 1; i++)
            {
                sw.Write("SPEC EF " + LocalStates[i] + ";\r\n");
                sw.Flush();
            }
            sw.Write("--Recoverability of states\r\n");
            for (int i = 0; i < RecovStates.Length - 1; i++)
            {
                sw.Write("SPEC AG ( " + RecovStates[i] + " -> AF " + RecovStates[i] + " );\r\n");
                sw.Flush();
            }
            sw.Write("}");
            sw.Flush();
            sw.Close();
        }
        private void SearchForInputs()//Функция поиска входных сигналов//
        {
            bool NotInput = false;
            bool Exists = false;
            for (int i = 0; i < Rules.Length - 1; i++ )
            {
                for (int j = 0; j < Rules[i].Elems.Length - 1; j++ )
                {
                    if (Rules[i].Elems[j].Type == "State")
                    {
                        for (int k = 0; k < LocalStates.Length - 1; k++ )
                        {
                            if (Rules[i].Elems[j].Value == LocalStates[k])
                            {
                                NotInput = true;
                                break;
                            }
                        }
                        for (int k = 0; k < Inputs.Length - 1; k++)
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
                            Inputs[Inputs.Length - 1] = Rules[i].Elems[j].Value;
                            Array.Resize(ref Inputs, Inputs.Length + 1);
                            Inputs[Inputs.Length - 1] = null;
                            Rules[i].Elems[j].Local = false;
                        }
                        NotInput = false;
                        Exists = false;
                    } 
                }
            }
        }
        private void SearchForRecov()//Функция поиска повторновходимых состояний//
        {
            for (int i = 0; i < Rules.Length - 1; i++)
            {
                for (int j = 1; j < Rules[i].Elems.Length - 1; j++)
                {
                    if (Rules[i].Elems[0].Value == Rules[i].Elems[j].Value)
                    {
                        RecovStates[RecovStates.Length - 1] = Rules[i].Elems[j].Value;
                        Array.Resize(ref RecovStates, RecovStates.Length + 1);
                        RecovStates[RecovStates.Length - 1] = null;
                        break;
                    }
                }
            }
        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }
        private void RefreshScreen()
        {
            float gip,DeltaX, DeltaY, cosa, sina, xn, yn;
            g.Clear(System.Drawing.Color.White);            //Отчищаем буфер заливая его фоном
            TextFont = new System.Drawing.Font(richTextBox1.Font.Name, (14 * Scale));
            Mu.WaitOne();                                   //ждем освобождения мьютекса
            
            if (Links != null)
            {
                for (int i = 0; i < Links.Length - 1; i++)
                {
                    if (Links[i].FromInput == true)         //Связи от входных сигналов
                    {
                        //рисуем связь(темно-синяя линия)
                        g.DrawLine(penDarkBlue, (Links[i].x1 + 30 + xT) * Scale, (Links[i].y1 + 30 + yT) * Scale, (Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale);
                        //вычисляем гипотенузу
                        gip = (float)System.Math.Sqrt(Math.Pow((Links[i].y1 + 30 + yT) * Scale - (Links[i].y2 + 30 + yT) * Scale, 2) + Math.Pow((Links[i].x1 + 30 + xT) * Scale - (Links[i].x2 + 30 + xT) * Scale, 2));

                        if (Links[i].x2 > Links[i].x1)
                        {
                            DeltaX = (Links[i].x2 - Links[i].x1) * Scale;
                            sina = DeltaX / gip;
                            if (Links[i].y2 < Links[i].y1)
                            {//1
                                DeltaY = (Links[i].y1 - Links[i].y2) * Scale;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * Scale;
                                yn = 50 * cosa * Scale;
                                g.DrawLine(penDarkGreen, (Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale, ((Links[i].x2 + 30 + xT) * Scale) - xn, ((Links[i].y2 + 30 + yT) * Scale) + yn);
                            }
                            if (Links[i].y2 > Links[i].y1)
                            {//2
                                DeltaY = (Links[i].y2 - Links[i].y1) * Scale;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * Scale;
                                yn = 50 * cosa * Scale;
                                g.DrawLine(penDarkGreen, (Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale, ((Links[i].x2 + 30 + xT) * Scale) - xn, ((Links[i].y2 + 30 + yT) * Scale) - yn);
                            }
                        }
                        if (Links[i].x2 < Links[i].x1)
                        {
                            DeltaX = (Links[i].x2 - Links[i].x1) * Scale;
                            sina = DeltaX / gip;
                            if (Links[i].y2 < Links[i].y1)
                            {//4
                                DeltaY = (Links[i].y1 - Links[i].y2) * Scale;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * Scale;
                                yn = 50 * cosa * Scale;
                                g.DrawLine(penDarkGreen, (Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale, ((Links[i].x2 + 30 + xT) * Scale) - xn, ((Links[i].y2 + 30 + yT) * Scale) + yn);
                            }
                            if (Links[i].y2 > Links[i].y1)
                            {//3
                                DeltaY = (Links[i].y2 - Links[i].y1) * Scale;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * Scale;
                                yn = 50 * cosa * Scale;
                                g.DrawLine(penDarkGreen, (Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale, ((Links[i].x2 + 30 + xT) * Scale) - xn, ((Links[i].y2 + 30 + yT) * Scale) - yn);
                            }
                        }
                    }
                    else if (Links[i].Arc == true)
                    {
                        g.DrawArc(penBlack, (Links[i].x1 - 20 + xT) * Scale, (Links[i].y1 - 20 + yT) * Scale, 50 * Scale, 50 * Scale, 0, 360);
                        g.DrawArc(penRed, (Links[i].x1 - 20 + xT) * Scale, (Links[i].y1 - 20 + yT) * Scale, 50 * Scale, 50 * Scale, 300, 60);
                    }
                    else
                    {
                        g.DrawLine(penBlack, (Links[i].x1 + 30 + xT) * Scale, (Links[i].y1 + 30 + yT) * Scale, (Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale);
                        gip = (float)System.Math.Sqrt(Math.Pow((Links[i].y1 + 30 + yT) * Scale-(Links[i].y2 + 30 + yT) * Scale,2) + Math.Pow((Links[i].x1 + 30 + xT) * Scale-(Links[i].x2 + 30 + xT) * Scale,2));
                        //xn = Math.Abs((Links[i].y1 + 30 + yT) * Scale-(Links[i].y2 + 30 + yT) * Scale)/gip*(gip-20);
                        //yn = Math.Abs((Links[i].x1 + 30 + xT) * Scale-(Links[i].x2 + 30 + xT) * Scale)/gip*(gip-20);
                        //g.DrawLine(p4,(Links[i].x1 + 30 + xT) * Scale,(Links[i].y1 + 30 + yT) * Scale,xn,yn);
                        //g.DrawLine(p3, (Links[i].x1 + 25)*4, (Links[i].y1 + 25)*4, Links[i].x2 + 25, Links[i].y2 + 25);
                        if(Links[i].x2 > Links[i].x1)
                        {
                            DeltaX = (Links[i].x2 - Links[i].x1) * Scale;
                            sina = DeltaX / gip;
                            if (Links[i].y2 < Links[i].y1)
                            {//1
                                DeltaY = (Links[i].y1 - Links[i].y2) * Scale;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * Scale;
                                yn = 50 * cosa * Scale;
                                g.DrawLine(penRed, (Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale, ((Links[i].x2 + 30 + xT) * Scale) - xn, ((Links[i].y2 + 30 + yT) * Scale) + yn);
                            }
                            if (Links[i].y2 > Links[i].y1)
                            {//2
                                DeltaY = (Links[i].y2 - Links[i].y1) * Scale;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * Scale;
                                yn = 50 * cosa * Scale;
                                g.DrawLine(penRed, (Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale, ((Links[i].x2 + 30 + xT) * Scale) - xn, ((Links[i].y2 + 30 + yT) * Scale) - yn);
                            }
                        }
                        if(Links[i].x2 < Links[i].x1)
                        {
                            DeltaX = (Links[i].x2 - Links[i].x1) * Scale;
                            sina = DeltaX / gip;
                            if (Links[i].y2 < Links[i].y1)
                            {//4
                                DeltaY = (Links[i].y1 - Links[i].y2) * Scale;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * Scale;
                                yn = 50 * cosa * Scale;
                                g.DrawLine(penRed, (Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale, ((Links[i].x2 + 30 + xT) * Scale) - xn, ((Links[i].y2 + 30 + yT) * Scale) + yn);
                            }
                            if (Links[i].y2 > Links[i].y1)
                            {//3
                                DeltaY = (Links[i].y2 - Links[i].y1) * Scale;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * Scale;
                                yn = 50 * cosa * Scale;
                                g.DrawLine(penRed, (Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale, ((Links[i].x2 + 30 + xT) * Scale) - xn, ((Links[i].y2 + 30 + yT) * Scale) - yn);
                            }
                        }
                    }
                }
            }
            if (States != null)
            {
                
                for (int i = 0; i < States.Length; i++)
                {
                    if (States[i].InputSignal == true)
                    {
                        g.FillRectangle(System.Drawing.Brushes.White, (States[i].x + xT) * Scale, (States[i].y + yT) * Scale, 60 * Scale, 60 * Scale);
                        if (States[i].Selected) g.DrawRectangle(penOrange, (States[i].x + xT) * Scale, (States[i].y + yT) * Scale, 60 * Scale, 60 * Scale);
                        else g.DrawRectangle(penDarkRed, (States[i].x + xT) * Scale, (States[i].y + yT) * Scale, 60 * Scale, 60 * Scale);
                        g.DrawString(States[i].Value, TextFont, System.Drawing.Brushes.Black, (States[i].x + 10 + xT) * Scale, (States[i].y + 10 + yT) * Scale);
                    }
                    else
                    {
                        g.FillEllipse(System.Drawing.Brushes.White, (States[i].x + xT) * Scale, (States[i].y + yT) * Scale, 60 * Scale, 60 * Scale);
                        if (States[i].Selected) g.DrawEllipse(penOrange, (States[i].x + xT) * Scale, (States[i].y + yT) * Scale, 60 * Scale, 60 * Scale);
                        else g.DrawEllipse(penDarkRed, (States[i].x + xT) * Scale, (States[i].y + yT) * Scale, 60 * Scale, 60 * Scale);
                        g.DrawString(States[i].Value, TextFont, System.Drawing.Brushes.Black, (States[i].x + 10 + xT) * Scale, (States[i].y + 15 + yT) * Scale);
                    }
                }
            }
            Mu.ReleaseMutex();
            g.Flush();
            pictureBox1.Refresh(bm); 
        }
        private void tabPage2_Click(object sender, EventArgs e)
        {
            RefreshScreen();
        }
        private void CreateStates()
        {
            Mu.WaitOne();
            xs = 50;
            ys = 10;
            Array.Resize(ref States, 0);
            States = new State[LocalStates.Length - 1];
            for (int j = 0; j < States.Length; j++)
            {
                States[j] = new State();
            }
            Random rnd = new Random();

            for (int i = 0; i < LocalStates.Length - 1; i++)
            {
                States[i].Value = LocalStates[i];
                States[i].x = xs;
                xs += 80;
                States[i].y = rnd.Next(70, 300);
            }
            Array.Resize(ref States, States.Length + Inputs.Length - 1);
            for (int i = LocalStates.Length - 1; i < States.Length; i++ )
            {
                States[i] = new State();
            }
            xs = 50;
            for (int i = LocalStates.Length - 1; i < States.Length; i++)
            {
                States[i].Value = Inputs[i - LocalStates.Length + 1];
                States[i].x = xs;
                xs += 70;
                States[i].y = ys;
                States[i].InputSignal = true;
            }
            Mu.ReleaseMutex();
        }
        private void CreateLinks()
        {
            Mu.WaitOne();
            Array.Resize(ref Links, 1);
            for (int i = 0; i < Rules.Length - 1; i++ )
            {
                for (int j = 1; j < Rules[i].Elems.Length - 1; j++ )
                {
                    if (Rules[i].Elems[j].Type == "State")
                    {
                        if (Rules[i].Elems[j].Local == true)
                        {
                            Array.Resize(ref Links, Links.Length + 1);
                            Links[Links.Length - 2].EndState = Rules[i].Elems[0].Value;
                            Links[Links.Length - 2].StartState = Rules[i].Elems[j].Value;
                            if (Links[Links.Length - 2].EndState == Links[Links.Length - 2].StartState)
                            {
                                Links[Links.Length - 2].Arc = true;
                            }
                            for (int k = 0; k < States.Length; k++ )
                            {
                                if(States[k].Value == Rules[i].Elems[0].Value)
                                {
                                    Links[Links.Length - 2].x2 = States[k].x;
                                    Links[Links.Length - 2].y2 = States[k].y;
                                }
                                if (States[k].Value == Rules[i].Elems[j].Value)
                                {
                                    Links[Links.Length - 2].x1= States[k].x;
                                    Links[Links.Length - 2].y1 = States[k].y;
                                }
                            }
                            Links[Links.Length - 1] = new Link();
                        }
                        else
                        {
                            Array.Resize(ref Links, Links.Length + 1);
                            Links[Links.Length - 2].EndState = Rules[i].Elems[0].Value;
                            Links[Links.Length - 2].StartState = Rules[i].Elems[j].Value;
                            if (Links[Links.Length - 2].EndState == Links[Links.Length - 2].StartState)
                            {
                                Links[Links.Length - 2].Arc = true;
                            }
                            Links[Links.Length - 2].FromInput = true;
                            for (int k = 0; k < States.Length; k++)
                            {
                                if (States[k].Value == Rules[i].Elems[0].Value)
                                {
                                    Links[Links.Length - 2].x2 = States[k].x;
                                    Links[Links.Length - 2].y2 = States[k].y;
                                }
                                if (States[k].Value == Rules[i].Elems[j].Value)
                                {
                                    Links[Links.Length - 2].x1 = States[k].x;
                                    Links[Links.Length - 2].y1 = States[k].y;
                                }
                            }
                            Links[Links.Length - 1] = new Link();
                        }
                    }
                }
            }
            Mu.ReleaseMutex();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel2.Text = "X:" + e.X + "\tY:" + e.Y;
            int dx, dy;
            float r;
            r = 30 * ((float)trackBar1.Value / 100);
            if(e.Button == MouseButtons.Left && StateSelected)
            {
                if (xM != e.X)
                {
                    dx = (int)(((float)e.X - xM)/Scale);
                    States[SelectedState].x = States[SelectedState].x + dx;
                    CreateLinks();
                    
                }
                if(yM != e.Y)
                {
                    dy = (int)(((float)e.Y - yM) / Scale);
                    States[SelectedState].y = States[SelectedState].y + dy;
                    CreateLinks();
                    
                }
                xM = e.X;
                yM = e.Y;
                RefreshScreen();
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Scale = (float)this.trackBar1.Value / 100;
            this.toolStripStatusLabel4.Text = Scale.ToString();
            RefreshScreen();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            //yT = trackBar2.Value;
            //tabPage2_Click(sender, e);
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            xT = -hScrollBar1.Value;
            RefreshScreen();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            yT = -vScrollBar1.Value;
            RefreshScreen();
        }

        private void сохранитьКакРисунокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //System.Drawing.Bitmap bm = new System.Drawing.Bitmap(2000, 2000);
            //System.Drawing.Drawing2D.GraphicsState gs;
            //gs = g.Save();
            this.pictureBox1.Image.Save("tmp.bmp");
        }

//         private void pictureBox1_Paint(object sender, PaintEventArgs e)
//         {
//             //RefreshScreen();
//         }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            float r;
            float xkur;
            float ykur;
            int result;
            if(e.Button == MouseButtons.Left)
            {
                if (AddStateButtonSelected)
                {
                    Mu.WaitOne();
                    //States[i].Value = LocalStates[i];
                    Array.Resize(ref States, States.Length + 1);
                    States[States.Length - 1] = new State();
                    States[States.Length - 1].x = (int)(((float)e.X - 30 - xT) / Scale);
                    States[States.Length - 1].y = (int)(((float)e.Y - 30 - yT) / Scale);
                    Mu.ReleaseMutex();
                }
//                 else
//                 {
                xkur = e.X/* * Scale*/;
                ykur = e.Y/* * Scale*/;
                r = 30 * ((float)trackBar1.Value / 100);
                toolStripStatusLabel3.Text = "X:" + xkur + "\tY:" + ykur;
                result = CheckSelectedState(xkur, ykur, r);
                if (result == (int)defines.NO_STATE_SELECTED)
                {
                    this.textBox1.Clear();
                    this.textBox1.Text = "no";
                }
                else
                {
                    this.textBox1.Clear();
                    this.textBox1.Text = States[result].Value;
                }
                xM = e.X;
                yM = e.Y;
            //}
            }
            RefreshScreen();
        }

        private int CheckSelectedState(float xkur, float ykur, float r)
        {
            float x0, y0;
            float f;
            if (States != null)
            {
                for (int i = States.Length-1; i >= 0; i--)
                {
                    x0 = (States[i].x + 30 + xT) * Scale;
                    y0 = (States[i].y + 30 + yT) * Scale;
                    f = (float)System.Math.Pow(x0 - xkur, 2) + (float)System.Math.Pow(y0 - ykur, 2);
                    if (f <= (float)System.Math.Pow(r, 2))
                    {
                        States[SelectedState].Selected = false;
                        StateSelected = true;
                        SelectedState = i;
                        States[i].Selected = true;
                        return i;
                    }
                    else
                    {
                        StateSelected = false;
                        States[i].Selected = false;
                    }
                }
            }
            return (int)defines.NO_STATE_SELECTED;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (StateSelected)
            {
                States[SelectedState].y = States[SelectedState].y - 5;
                CreateLinks();
                RefreshScreen();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (StateSelected)
            {
                States[SelectedState].y = States[SelectedState].y + 5;
                CreateLinks();
                RefreshScreen();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (StateSelected)
            {
                States[SelectedState].x = States[SelectedState].x - 5;
                CreateLinks();
                RefreshScreen();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (StateSelected)
            {
                States[SelectedState].x = States[SelectedState].x + 5;
                CreateLinks();
                RefreshScreen();
            }
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {
            this.backgroundWorker1.CancelAsync();
        }

        private void tabControl1_TabIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedTab == tabPage1 && this.backgroundWorker1.IsBusy)
            {
                this.backgroundWorker1.CancelAsync();
            }
            else if (this.tabControl1.SelectedTab == tabPage2 && !this.backgroundWorker1.IsBusy)
            {
                RefreshScreen();
                this.backgroundWorker1.RunWorkerAsync();
                this.backgroundWorker1.WorkerSupportsCancellation = true;
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (!AddStateButtonSelected)
            {
                this.toolStripButton3.Image = global::sku_to_smv.Properties.Resources.state2;
                AddStateButtonSelected = true;
            }
            else
            {
                this.toolStripButton3.Image = global::sku_to_smv.Properties.Resources.state1;
                AddStateButtonSelected = false;
            }
        }
         ////if (StateSelected)
         //   //{
         //       this.textBox1.Clear();
         //       this.textBox1.Text = e.KeyValue.ToString();
         //   //}

    }
}


