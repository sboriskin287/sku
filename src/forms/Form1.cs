using System;
using System.Windows.Forms;
using System.IO;
using sku_to_smv.Properties;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

// TODO Дописать таблицу входных сигналов

namespace sku_to_smv
{
    enum defines{NO_STATE_SELECTED = -1};

    public partial class Form1 : Form
    {
        bool AddStateButtonSelected;
        bool Saved, TextCH, Inv, Analysed;
        bool b_FileLoad;
        bool b_Parsing;
        bool b_TxtHLEnable;
        string sOpenFileName, sOpenSaveFileName, sSaveFileName, sAutoSaveFileName;
        
        public Parser parser;
        //SympleParser sParser;
        ToolStripControlHost scaleTrack;
        options optWindow;
        ContextMenuStrip TextContextMenu;

        public Form1()
        {
            InitializeComponent();

            scaleTrack = new ToolStripControlHost(new TrackBar());
            ((TrackBar)scaleTrack.Control).LargeChange = 10;
            ((TrackBar)scaleTrack.Control).Maximum = 200;
            ((TrackBar)scaleTrack.Control).Minimum = 20;
            ((TrackBar)scaleTrack.Control).Name = "trackBar1";
            ((TrackBar)scaleTrack.Control).Size = new System.Drawing.Size(300, 18);
            ((TrackBar)scaleTrack.Control).TabIndex = 5;
            ((TrackBar)scaleTrack.Control).Value = 100;
            ((TrackBar)scaleTrack.Control).Scroll += new System.EventHandler(this.trackBar1_Scroll);
            this.toolStripSplitButton1.DropDownItems.Add(scaleTrack);
            this.Size = new System.Drawing.Size(800, 600);

            parser = new Parser();
            //sParser = new SympleParser();
            optWindow = new options();
            TextContextMenu = new ContextMenuStrip();
            TextContextMenu.Items.Add("Вырезать");
            TextContextMenu.Items.Add("Копировать");
            TextContextMenu.Items.Add("Вставить");
            this.richTextBox1.ContextMenuStrip = TextContextMenu;
            TextContextMenu.ItemClicked += new ToolStripItemClickedEventHandler(TextContextMenu_Click);

            this.pictureBox1.SimulationStarted += this.SimulStarted;
            this.pictureBox1.SimulationStoped += this.SimulStoped;

            Saved = true;
            TextCH = false;
            Inv = false;
            Analysed = false;
            AddStateButtonSelected = false;
            b_Parsing = false;
            b_FileLoad = false;


            FileInfo fi = new FileInfo("simul.exe");
            if (fi.Exists)
            {
                try
                {
                    fi.Delete();
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            ParceTimer.Start();
            //this.DataBindings.Add(new Binding("Size", Settings.Default, "MainWndSize", false, DataSourceUpdateMode.OnPropertyChanged));
            //this.OnResizeEnd(null);
            this.Size = Settings.Default.MainWndSize;
            if (Settings.Default.MainWndMaximized)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;

            ApplySettings();
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
                sOpenFileName = openFileDialog1.FileName;
                sAutoSaveFileName = sOpenFileName;
                sOpenSaveFileName = openFileDialog1.SafeFileName;
                this.toolStripStatusLabel1.Text = "Открыт файл: " + sOpenFileName;
                this.toolStripProgressBar1.Value = 0;
                FileToTextbox();
                this.toolStripProgressBar1.Value = 100;
                Saved = true;
                b_FileLoad = true;
                //TextCH = false;
                sOpenSaveFileName = sOpenSaveFileName.Remove(sOpenSaveFileName.Length - 4);
                pictureBox1.ClearArea();
                pictureBox1.LogFileName = sOpenSaveFileName;
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
                sAutoSaveFileName = sSaveFileName;
                TextboxToFile(sSaveFileName);
                this.toolStripStatusLabel1.Text = "Файл " + sSaveFileName + " сохранен";
                this.toolStripProgressBar1.Value = 100;
                Saved = true;
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// Загружает открытый файл для редактирования
        /// </summary>
        private void FileToTextbox()
        {
            this.richTextBox1.Clear();
            StreamReader SR = new StreamReader(sOpenFileName, System.Text.Encoding.GetEncoding("windows-1251"));
            this.richTextBox1.AppendText(SR.ReadToEnd());
            SR.Close();
        }
        /// <summary>
        /// Сохраняет отредактированный файл
        /// </summary>
        private void TextboxToFile(string Name)
        {
            this.toolStripStatusLabel3.Image = global::sku_to_smv.Properties.Resources.save_anim;
            this.toolStripStatusLabel3.Visible = true;
            this.AnimationTimer.Start();
            String str;
            str = this.richTextBox1.Text;
            str = str.Replace("\n", "\r\n");
            StreamWriter sw = new StreamWriter(Name, false, System.Text.Encoding.GetEncoding("windows-1251"));
            sw.Write(str);
            sw.Flush();
            sw.Close();
        }
        /// <summary>
        /// Просто печатает текст в текстбокс
        /// </summary>
        /// <param name="temp"></param>
        private void PrintText(string temp)
        {
            this.toolStripStatusLabel2.Text = temp;
        }

        private void анализироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /////////////Вывод в smv//////////////////////////////////
            saveFileDialog1.Filter = "SMV files (*.smv)|*.smv";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFileDialog1.FileName.Length > 0)
            {
                this.toolStripProgressBar1.Value = 0;
                sSaveFileName = saveFileDialog1.FileName;
                if (parser.ParseStart(this.richTextBox1.Text) == parceResult.PARCE_ERROR)
                {
                    PrintText("Ошибка разбора");
                    this.toolStripProgressBar1.Value = 50;
                    return;
                }
                this.toolStripProgressBar1.Value = 50;
                PrintText("Разбор окончен");
                parser.SaveToSMV(sSaveFileName);
                this.toolStripStatusLabel1.Text = "Файл сохранен под именем: " + sSaveFileName;
                this.toolStripProgressBar1.Value = 100;
            }
            ///////////////////////////////////////////////////
        }
        private void CreateGraf(object sender, EventArgs e)
        {
            this.toolStripProgressBar1.Value = 0;
            pictureBox1.canselDrawElements();
            parser.Rules.Clear();
            parser.signals.Clear();
            if (parser.ParseStart(this.richTextBox1.Text) == parceResult.PARCE_ERROR)
            {
                PrintText("Ошибка разбора");
                this.toolStripProgressBar1.Value = 25;
                return;
            }
            this.toolStripProgressBar1.Value = 25;
            PrintText("Разбор окончен");
            this.tabControl1.SelectedIndex = 1;
            Array.Resize(ref pictureBox1.rules, parser.Rules.Count);
            pictureBox1.rules = parser.Rules.ToArray();         
            this.toolStripProgressBar1.Value = 50;       
            pictureBox1.createStates();
            pictureBox1.createLinks();
            this.toolStripProgressBar1.Value = 75;            
            pictureBox1.createSignals();
            pictureBox1.createTimeMarks();
            RefreshScreen();
            this.toolStripProgressBar1.Value = 100;
        }

        private void RefreshScreen(Rule[] rules, Graphics g)
        {
            
        }

        private void RefreshScreen()
        {
            pictureBox1.Refresh(); 
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {
            RefreshScreen();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            pictureBox1.ScaleT = (float)((TrackBar)scaleTrack.Control).Value / 100;
            this.toolStripSplitButton1.Text = "Масштаб " + ((TrackBar)scaleTrack.Control).Value.ToString() + "%";
            RefreshScreen();
        }

        private void сохранитьКакРисунокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /////////////Сохраняем картинку//////////////////////////////////
            saveFileDialog1.Filter = "BMP files (*.bmp)|*.bmp";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFileDialog1.FileName.Length > 0)
            {
                this.toolStripProgressBar1.Value = 0;
                sSaveFileName = saveFileDialog1.FileName;
                this.pictureBox1.SaveImage(sSaveFileName);
                this.toolStripStatusLabel1.Text = "Файл сохранен под именем: " + sSaveFileName;
                this.toolStripProgressBar1.Value = 100;
            }
            ///////////////////////////////////////////////////
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                ParceTimer.Start();
                if (Settings.Default.Autosave)
                {
                    this.AnimationTimer.Start();
                }
                this.CopyToolButton.Visible = true;
                this.PasteToolButton.Visible = true;
                this.CutToolButton.Visible = true;
                this.toolStripSeparator4.Visible = true;
                this.toolStripSplitButton1.Enabled = false;
            }
            else
            {
                ParceTimer.Stop();
                this.AnimationTimer.Stop();
                this.CopyToolButton.Visible = false;
                this.PasteToolButton.Visible = false;
                this.CutToolButton.Visible = false;
                this.toolStripSeparator4.Visible = false;
                this.toolStripSplitButton1.Enabled = true;
            }
            RefreshScreen();
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

        private void button5_Click(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedTab != null)
            {
                for (int i = 0; i < this.tabControl1.TabPages[this.tabControl1.SelectedIndex].Controls.Count; i++)
                {
                    this.tabControl1.TabPages[this.tabControl1.SelectedIndex].Controls[i].Dispose();
                }
                this.tabControl1.TabPages.Remove(this.tabControl1.SelectedTab);
                GC.Collect();
            }
        }

        private void NewGraf_Click(object sender, EventArgs e)
        {
            DrawArea dwa = DrawArea.getInstance();
            dwa.Dock = System.Windows.Forms.DockStyle.Fill;
            dwa.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dwa.Location = new System.Drawing.Point(3, 3);

            TabPage tbp = new TabPage();
            tbp.Controls.Add(dwa);
            tbp.Name = "grafPage";
            tbp.Padding = new System.Windows.Forms.Padding(3);
            tbp.Text = "Граф";
            tbp.UseVisualStyleBackColor = true;

            this.tabControl1.Controls.Add(tbp);
        }

        private void NewSKU_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = new RichTextBox();
            rtb.Dock = System.Windows.Forms.DockStyle.Fill;
            rtb.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            rtb.Location = new System.Drawing.Point(3, 3);
            rtb.Text = "";

            TabPage tbp = new TabPage();
            tbp.Controls.Add(rtb);
            tbp.Name = "SKUPage";
            tbp.Padding = new System.Windows.Forms.Padding(3);
            tbp.Text = "СКУ";
            tbp.UseVisualStyleBackColor = true;

            this.tabControl1.Controls.Add(tbp);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            this.pictureBox1.SimulStart();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            this.pictureBox1.SimulStep();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            this.pictureBox1.SimulStop();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!Saved)
            {
                switch (MessageBox.Show("Сохранить документ перед выходом?", this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.OK: сохранитьToolStripMenuItem_Click(this, e);
                        break;
                    case DialogResult.Cancel: e.Cancel = true;
                        break;
                    case DialogResult.No: Application.Exit();
                        break;
                }
            }
            else Application.Exit();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < this.tabControl1.TabPages.Count; i++)
            {
                if (this.tabControl1.TabPages[i].Name == "grafPage")
                {
                    (this.tabControl1.TabPages[i].Controls[0] as DrawArea).ClosePipe();
                }
            }
            if (this.WindowState != FormWindowState.Maximized)
                Settings.Default.MainWndSize = this.Size;
            else Settings.Default.MainWndMaximized = true;

            Settings.Default.Save();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            this.pictureBox1.CreateTable();
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            if (!pictureBox1.SimulStarted)
            {
                this.toolStripButton10.Image = global::sku_to_smv.Properties.Resources.stop_simulation;
                this.toolStripButton10.Text = "Остановить симуляцию";
                this.toolStripButton4.Enabled = true;
                this.toolStripButton5.Enabled = true;
                this.toolStripButton6.Enabled = true;
                this.toolStripButton7.Enabled = true;
            }
            else
            {
                this.toolStripButton10.Image = global::sku_to_smv.Properties.Resources.create_simulation;
                this.toolStripButton10.Text = "Запустить симуляцию";
                this.toolStripButton4.Enabled = false;
                this.toolStripButton5.Enabled = false;
                this.toolStripButton6.Enabled = false;
                this.toolStripButton7.Enabled = false;
            }
            //pictureBox1.CreateSimul(parser.Rules, parser.Outputs);
        }

        private void генерироватьVHDLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isBreak = false;
            /////////////Вывод в VHDL//////////////////////////////////

            saveFileDialog1.Filter = "VHDL files (*.vdh)|*.vhd";
            saveFileDialog1.RestoreDirectory = true;
            bool b_CreateBus = false;
            switch (MessageBox.Show("Объединять входные сигналы в шину?", "Генерация VHDL", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
            {
                case DialogResult.Yes: b_CreateBus = true;
                    break;
                case DialogResult.No: b_CreateBus = false;
                    break;
                case DialogResult.Cancel: return;
            }
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFileDialog1.FileName.Length > 0)
            {
                this.toolStripProgressBar1.Value = 0;
                sSaveFileName = saveFileDialog1.FileName;
                if (parser.ParseStart(this.richTextBox1.Text) == parceResult.PARCE_ERROR)
                {
                    PrintText("Ошибка разбора");
                    this.toolStripProgressBar1.Value = 50;
                    return;
                }
                OutputSignalsTable form3 = new OutputSignalsTable(parser.LocalStates.Length);
                for (int i = 0; i < parser.LocalStates.Length; i++)
                {
                    isBreak = false;
                    for (int j = 0; j < parser.Rules.Count; j++)
                    {
                        /*if (parser.Rules[j].output && parser.Rules[j].Elems[2].Value == parser.LocalStates[i])
                        {
                            form3.AddState(i, parser.LocalStates[i], parser.Rules[j].Elems[0].Value);
                            isBreak = true;
                            break;
                        }*/
                    }
                    if (!isBreak) form3.AddState(i, parser.LocalStates[i], null);
                }
                if (form3.ShowDialog() == DialogResult.OK)
                {
                    this.toolStripProgressBar1.Value = 50;
                    PrintText("Разбор окончен");
                    //parser.SaveToVHDL(sSaveFileName, b_CreateBus, form3.OutputSignalsCount, form3.GetTable());
                    this.toolStripStatusLabel1.Text = "Файл сохранен под именем: " + sSaveFileName;
                    this.toolStripProgressBar1.Value = 100;
                }
            }
            ///////////////////////////////////////////////////
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!b_Parsing)
            {
                TextCH = true;
                Analysed = false;
                if (b_FileLoad)
                {
                    Saved = true;
                    b_FileLoad = false;
                }
                else Saved = false;
            }
        }

        private void ParceTimer_Tick(object sender, EventArgs e)
        {
            if (TextCH && b_TxtHLEnable)
            {
                this.richTextBox1.ShowSelectionMargin = false;
                TextCH = false;
                b_Parsing = true;
                int Index = this.richTextBox1.SelectionStart;
                /*if (!sParser.b_Brackets)
                    sParser.Start(this.richTextBox1.Text);*/
                this.richTextBox1.SelectAll();
                this.richTextBox1.ForeColor = Settings.Default.TextFieldTextColor;
                this.richTextBox1.Font = Settings.Default.TextFieldTextFont;

                /*for (int i = 0; i < sParser.elements.Length; i++)
                {
                    if (sParser.elements[i].EndIndex != 0)
                    {
                        this.richTextBox1.Select(sParser.elements[i].StartIndex, sParser.elements[i].EndIndex - sParser.elements[i].StartIndex + 1);
                        this.richTextBox1.SelectionColor = sParser.elements[i].TextColor;
                        this.richTextBox1.SelectionFont = new System.Drawing.Font(Settings.Default.TextFieldTextFont, Settings.Default.TextFieldTextFont.Style | sParser.elements[i].Style);
                        this.richTextBox1.Select(sParser.elements[i].EndIndex + 1, 0);
                        this.richTextBox1.SelectionColor = Settings.Default.TextFieldTextColor;
                    }
                    if (sParser.brackets.StartIndex != -1 && sParser.brackets.EndIndex != -1)
                    {
                        this.richTextBox1.Select(sParser.brackets.StartIndex, 1);
                        this.richTextBox1.SelectionFont = new System.Drawing.Font(Settings.Default.TextFieldTextFont, Settings.Default.TextFieldTextFont.Style | FontStyle.Bold);
                        this.richTextBox1.Select(sParser.brackets.StartIndex + 1, 0);
                        this.richTextBox1.SelectionFont = new System.Drawing.Font(Settings.Default.TextFieldTextFont, Settings.Default.TextFieldTextFont.Style);

                        this.richTextBox1.Select(sParser.brackets.EndIndex, 1);
                        this.richTextBox1.SelectionFont = new System.Drawing.Font(Settings.Default.TextFieldTextFont, Settings.Default.TextFieldTextFont.Style | FontStyle.Bold);
                        this.richTextBox1.Select(sParser.brackets.EndIndex + 1, 0);
                        this.richTextBox1.SelectionFont = new System.Drawing.Font(Settings.Default.TextFieldTextFont, Settings.Default.TextFieldTextFont.Style);
                        sParser.b_Brackets = false;
                    }
                }*/
                this.richTextBox1.Select(Index, 0);
                this.richTextBox1.ShowSelectionMargin = true;
                b_Parsing = false;
                //this.richTextBox1.SelectionStart = Index;
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
//             if (this.Size.Width < 800)
//                 this.Size = new System.Drawing.Size(800, this.Size.Height);
//             if (this.Size.Height < 600)
//                 this.Size = new System.Drawing.Size(this.Size.Width, 600);
//             this.toolStripStatusLabel1.Size = new System.Drawing.Size((this.Size.Width - this.toolStripProgressBar1.Size.Width
//                 - this.toolStripSplitButton1.Size.Width - 50) / 2, this.toolStripStatusLabel1.Size.Height);
//             this.toolStripStatusLabel2.Size = new System.Drawing.Size((this.Size.Width - this.toolStripProgressBar1.Size.Width
//                 - this.toolStripSplitButton1.Size.Width - 50) / 2, this.toolStripStatusLabel2.Size.Height);
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (optWindow.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.Save();
                ApplySettings();
            }
            else
            {
                Settings.Default.Reload();
                ApplySettings();
            }
        }
        private void ApplySettings()
        {
            if (Settings.Default.Autosave)
            {
                this.AutosaveTimer.Interval = int.Parse(Settings.Default.AutosavePeriod) * 60000;
                this.AutosaveTimer.Start();
            }
            else this.AutosaveTimer.Stop();
            b_TxtHLEnable = Settings.Default.TextFieldEnableHighLight;
            if (!b_TxtHLEnable)
            {
                int Index = this.richTextBox1.SelectionStart;
                this.richTextBox1.SelectAll();
                this.richTextBox1.ForeColor = Settings.Default.TextFieldTextColor;
                this.richTextBox1.Font = Settings.Default.TextFieldTextFont;
                this.richTextBox1.SelectionColor = Settings.Default.TextFieldTextColor;
                this.richTextBox1.Select(Index, 0);
            }
            pictureBox1.ApplySettings();
            //int Index = this.richTextBox1.SelectionStart;
            this.richTextBox1.Font = Settings.Default.TextFieldTextFont;
//             this.richTextBox1.SelectAll();
//             this.richTextBox1.SelectionFont = Settings.Default.TextFieldTextFont;
//             this.richTextBox1.Select(Index, 0);
            TextCH = true;
        }
        private void TextContextMenu_Click(object sender, ToolStripItemClickedEventArgs  e)
        {
            switch (e.ClickedItem.Text)
            {
                case "Вырезать":
                    if (richTextBox1.SelectedText != "")
                        richTextBox1.Cut();
                    break;
                case "Копировать":
                    if (richTextBox1.SelectionLength > 0)
                        richTextBox1.Copy();
                    break;
                case "Вставить":
                    if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true)
                    {
                        if (richTextBox1.SelectionLength > 0)
                        {
                                richTextBox1.SelectionStart = richTextBox1.SelectionStart + richTextBox1.SelectionLength;
                        }
                        richTextBox1.Paste();
                    }

                    break;
            }
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                Settings.Default.MainWndMaximized = true;
            }
            else
            {
                if (this.Size.Width < 800)
                    this.Size = new System.Drawing.Size(800, this.Size.Height);
                if (this.Size.Height < 600)
                    this.Size = new System.Drawing.Size(this.Size.Width, 600);
                this.toolStripStatusLabel1.Size = new System.Drawing.Size((this.Size.Width - this.toolStripProgressBar1.Size.Width
                    - this.toolStripSplitButton1.Size.Width - 50) / 2, this.toolStripStatusLabel1.Size.Height);
                this.toolStripStatusLabel2.Size = new System.Drawing.Size((this.Size.Width - this.toolStripProgressBar1.Size.Width
                    - this.toolStripSplitButton1.Size.Width - 50) / 2, this.toolStripStatusLabel2.Size.Height);
                Settings.Default.MainWndSize = this.Size;
            }
            Settings.Default.Save();
        }

        private void SimulStarted(object sender, EventArgs e)
        {
            this.toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
        }
        private void SimulStoped(object sender, EventArgs e)
        {
            this.toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
        }

        private void CutToolButton_Click(object sender, EventArgs e)
        {
            if (richTextBox1.SelectedText != "")
                richTextBox1.Cut();
        }

        private void CopyToolButton_Click(object sender, EventArgs e)
        {
            if (richTextBox1.SelectionLength > 0)
                richTextBox1.Copy();
        }

        private void PasteToolButton_Click(object sender, EventArgs e)
        {
            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true)
            {
                if (richTextBox1.SelectionLength > 0)
                {
                    richTextBox1.SelectionStart = richTextBox1.SelectionStart + richTextBox1.SelectionLength;
                }
                richTextBox1.Paste();
            }
        }

        private void AutosaveTimer_Tick(object sender, EventArgs e)
        {
            if (sAutoSaveFileName != null && sAutoSaveFileName.Length != 0)
            {
                TextboxToFile(sAutoSaveFileName);
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            this.toolStripStatusLabel3.Image = global::sku_to_smv.Properties.Resources.saveHS;
            this.toolStripStatusLabel3.Visible = false;
            this.AnimationTimer.Stop();
        }

        private void помощьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Разработал Кутузов Владимир\nПГУ 2013", Assembly.GetExecutingAssembly().GetName().Name + " " + Assembly.GetExecutingAssembly().GetName().Version.ToString()
                , MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
        }
        //Поиск парных скобок
        /*private void richTextBox1_Click(object sender, EventArgs e)
        {
            int result = this.richTextBox1.SelectionStart;
            if (this.richTextBox1.Text.Length != 0 
                && this.richTextBox1.SelectionStart != this.richTextBox1.Text.Length)
            {
                if (this.richTextBox1.Text[this.richTextBox1.SelectionStart] == '(')
                {
                    result = SympleParser.FindBracket(this.richTextBox1.Text, this.richTextBox1.SelectionStart, true);
                }
                else if (this.richTextBox1.Text[this.richTextBox1.SelectionStart] == ')')
                {
                    result = SympleParser.FindBracket(this.richTextBox1.Text, this.richTextBox1.SelectionStart, false);
                }
                if (result != this.richTextBox1.SelectionStart)
                {
                    sParser.b_Brackets = true;
                    sParser.brackets.StartIndex = this.richTextBox1.SelectionStart;
                    sParser.brackets.EndIndex = result;
                    TextCH = true;
                    return;
                }

                if (sParser.brackets.StartIndex != -1 && sParser.brackets.EndIndex != -1)
                {
                    sParser.brackets.StartIndex = -1;
                    sParser.brackets.EndIndex = -1;
                    TextCH = true;
                }
                sParser.brackets.StartIndex = -1;
                sParser.brackets.EndIndex = -1;
            }
        }*/
    }
}


