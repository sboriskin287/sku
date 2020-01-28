using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.CSharp;
using sku_to_smv.Properties;
using sku_to_smv.src;

namespace sku_to_smv
{
    class drawArea : PictureBox
    {
        HScrollBar hScroll;
        VScrollBar vScroll;
        ContextMenuStrip contextMenu;
        ToolTip toolTip;
        Point MouseLastPosition;
        System.Windows.Forms.Timer ClickToolPanelTimer;
        
        ToolPanel tools;
        Graphics g;
        Pen penHighlight;//цвет выделения
        Pen penSignal;//цвет сигналов
        Pen penInputSignal;//цвет сигналов
        Pen penOutputSignal;//цвет сигналов
        Pen penLocalLine;//цвет линии между локальными сигналами
        Pen penLocalLineEnd;//цвет стрелки
        Pen penInputLine;//цвет линии от входных сигналов
        Pen penInputLineEnd;//цвет стрелки
        Brush brushTextColor;
        Brush brushSignalActive;//кисть для активных сигналов
        Font TextFont;//Шрифт

        public State[] States;
        public Link[] Links;
        public Rule[] rules;
        NamedPipeServerStream pipe;
        StreamWriter sw;
        SignalTable table;
        BufferedGraphicsContext drawContext;
        BufferedGraphics drawBuffer;
        LogWriter log;

        int xT, yT/*, dx, dy*/;
        int xs, ys, xM, yM;
        int curX, curY;
        int InputsLeight;
        int OutputsLeight;
        bool StateSelected;
        bool LinkSelected;
        //bool AddStateButtonSelected;
        bool TableCreated;
        bool b_ShowDebugInfo;
        bool b_SavingImage;
        int SelectedState;
        int stepNumber;
        bool DrawInputs;
        bool b_EnableLogging;

        public bool b_SimulStarted { get; set; }
        public String LogFileName { get; set; }
        private System.Windows.Forms.Timer timer1;
        private System.ComponentModel.IContainer components;
        FormClosedEventHandler handler;

        public delegate void drawAreaEventHandler(object sender, EventArgs a);
        public event drawAreaEventHandler SimulationStarted;
        public event drawAreaEventHandler SimulationStoped;

        private float scaleT; 
        
        public float ScaleT
        {
            get { return scaleT; }
            set
            {
                scaleT = value;
                TextFont = new System.Drawing.Font(Settings.Default.GrafFieldTextFont.Name, (Settings.Default.GrafFieldTextFont.Size * value), Settings.Default.GrafFieldTextFont.Style);
            }
        }

       public drawArea()
        {
            InitializeArea();
        }
        ~drawArea() 
        {
            hScroll.Dispose();
            vScroll.Dispose();
            g.Dispose();
            Array.Clear(States, 0, States.Length);
            Array.Clear(Links, 0, Links.Length);
        }
        /// <summary>
        /// Функция инициализации области рисования
        /// </summary>
        private void InitializeArea()
        {
            //Отображение отладочной информации на графе
            b_ShowDebugInfo = false;
            ScaleT = 1F;
            xT = 0;
            yT = 0;
            stepNumber = 1;
            InputsLeight = 0;
            OutputsLeight = 0;

            //AddStateButtonSelected = false;
            DrawInputs = true;
            b_SimulStarted = false;
            LinkSelected = false;
            TableCreated = false;
            b_SavingImage = false;
            b_EnableLogging = true;
            LogFileName = "";

            log = new LogWriter();
            drawContext = new BufferedGraphicsContext();
            handler = new System.Windows.Forms.FormClosedEventHandler(this.TableClosed);
            this.components = new System.ComponentModel.Container();
            this.toolTip = new ToolTip(components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();

            Links = new Link[1];
            Links[0] = new Link();

            States = new State[0];
            rules = new Rule[0];

            this.DoubleBuffered = true;

            tools = new ToolPanel();
            tools.BackColor = Color.FromArgb(120, 145, 217, 255);
            tools.ButtonClicked += this.ToolPanelButtonClicked;
            tools.PanelOrientation = Orientation.Vertical;
            tools.PanelAlignment = Alignment.RIGHT;

            ToolButton tButton = new ToolButton();
            tButton.SetFrame(global::sku_to_smv.Properties.Resources.frame);
            tButton.SetFrameHover(global::sku_to_smv.Properties.Resources.frame_hover);
            tButton.SetFrameClick(global::sku_to_smv.Properties.Resources.frame_click);
            tButton.SetInactiveImage(global::sku_to_smv.Properties.Resources.create_simulation);
            tButton.SetImage(global::sku_to_smv.Properties.Resources.create_simulation);
            tButton.Name = "start";
            tButton.Text = "Запустить симуляцию";
            tools.AddControl(ref tButton);

            tButton = new ToolButton();
            tButton.SetFrame(global::sku_to_smv.Properties.Resources.frame);
            tButton.SetFrameHover(global::sku_to_smv.Properties.Resources.frame_hover);
            tButton.SetFrameClick(global::sku_to_smv.Properties.Resources.frame_click);
            tButton.SetInactiveImage(global::sku_to_smv.Properties.Resources.play_grey);
            tButton.SetImage(global::sku_to_smv.Properties.Resources.play);
            tButton.Name = "run";
            tButton.Text = "Запуск симуляции";
            tools.AddControl(ref tButton);

            tButton = new ToolButton();
            tButton.SetFrame(global::sku_to_smv.Properties.Resources.frame);
            tButton.SetFrameHover(global::sku_to_smv.Properties.Resources.frame_hover);
            tButton.SetFrameClick(global::sku_to_smv.Properties.Resources.frame_click);
            tButton.SetInactiveImage(global::sku_to_smv.Properties.Resources.step_grey);
            tButton.SetImage(global::sku_to_smv.Properties.Resources.step);
            tButton.Name = "step";
            tButton.Text = "Шаг с остановом";
            tools.AddControl(ref tButton);

            tButton = new ToolButton();
            tButton.SetFrame(global::sku_to_smv.Properties.Resources.frame);
            tButton.SetFrameHover(global::sku_to_smv.Properties.Resources.frame_hover);
            tButton.SetFrameClick(global::sku_to_smv.Properties.Resources.frame_click);
            tButton.SetInactiveImage(global::sku_to_smv.Properties.Resources.stop_grey);
            tButton.SetImage(global::sku_to_smv.Properties.Resources.stop);
            tButton.Name = "stop";
            tButton.Text = "Остановить симуляцию";
            tools.AddControl(ref tButton);

            tButton = new ToolButton();
            tButton.SetFrame(global::sku_to_smv.Properties.Resources.frame);
            tButton.SetFrameHover(global::sku_to_smv.Properties.Resources.frame_hover);
            tButton.SetFrameClick(global::sku_to_smv.Properties.Resources.frame_click);
            tButton.SetInactiveImage(global::sku_to_smv.Properties.Resources.table_grey);
            tButton.SetImage(global::sku_to_smv.Properties.Resources.table);
            tButton.Name = "table";
            tButton.Text = "Таблица сигналов";
            tools.AddControl(ref tButton);

            tButton = new ToolButton();
            tButton.SetFrame(global::sku_to_smv.Properties.Resources.frame);
            tButton.SetFrameHover(global::sku_to_smv.Properties.Resources.frame_hover);
            tButton.SetFrameClick(global::sku_to_smv.Properties.Resources.frame_click);
            //tButton.SetInactiveImage(global::sku_to_smv.Properties.Resources.table_grey);
            tButton.SetImage(global::sku_to_smv.Properties.Resources.reset_all);
            tButton.Name = "reset";
            tButton.Text = "Сбросить все сигналы";
            tools.AddControl(ref tButton);

            tButton = new ToolButton();
            tButton.SetFrame(global::sku_to_smv.Properties.Resources.frame);
            tButton.SetFrameHover(global::sku_to_smv.Properties.Resources.frame_hover);
            tButton.SetFrameClick(global::sku_to_smv.Properties.Resources.frame_click);
            //tButton.SetInactiveImage(global::sku_to_smv.Properties.Resources.table_grey);
            tButton.SetImage(global::sku_to_smv.Properties.Resources.show_log);
            tButton.Name = "showlog";
            tButton.Text = "Показать лог-файл";
            tools.AddControl(ref tButton);

            tools.Buttons[1].Enabled = false;
            tools.Buttons[2].Enabled = false;
            tools.Buttons[3].Enabled = false;
            tools.Buttons[4].Enabled = false;

            hScroll = new HScrollBar();
            vScroll = new VScrollBar();
            contextMenu = new ContextMenuStrip(components);
            contextMenu.Visible = false;
            contextMenu.Items.Add("Установить 1");
            contextMenu.Items.Add("Всегда 1");
            contextMenu.Items.Add("Установить 0");
            contextMenu.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenuClick);

            this.Controls.Add(hScroll);
            this.Controls.Add(vScroll);

            hScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            hScroll.LargeChange = 50;
            hScroll.Location = new System.Drawing.Point(0, this.Height - 20);
            hScroll.Maximum = 1000;
            hScroll.Name = "hScroll";
            hScroll.Size = new System.Drawing.Size(this.Width - 20, 20);
            hScroll.TabIndex = 2;
            hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScroll_Scroll);

            vScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            vScroll.LargeChange = 50;
            vScroll.Location = new System.Drawing.Point(this.Width - 20, 0);
            vScroll.Maximum = 1000;
            vScroll.Name = "vScroll";
            vScroll.Size = new System.Drawing.Size(20, this.Height);
            vScroll.TabIndex = 1;
            vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScroll_Scroll);



            //графический буфер
            drawBuffer = drawContext.Allocate(this.CreateGraphics(), this.ClientRectangle);
            g = drawBuffer.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//Включаем сглаживание графических объектов
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;//Включаем сглаживание шрифтов
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;//Включаем интерполяцию

            ApplySettings();

            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.drawArea_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.drawArea_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.drawArea_MouseMove);
            this.SizeChanged += new EventHandler(this.AreaResized);
            
            // 
            // Таймер, отсчитывает шаги симуляции
            // 
            //this.timer1.Interval = 2000;
            this.ClickToolPanelTimer = new System.Windows.Forms.Timer();
            this.ClickToolPanelTimer.Interval = 200;
            this.ClickToolPanelTimer.Tick += new System.EventHandler(this.ClickToolPanelTimer_Tick);

            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
        }
        public void ApplySettings()
        {
            //Определяются цвета
            penSignal = new System.Drawing.Pen(Settings.Default.GrafFieldLocalSignalColor, 2);
            penInputSignal = new System.Drawing.Pen(Settings.Default.GrafFieldInputSignalColor, 2);
            penOutputSignal = new System.Drawing.Pen(Settings.Default.GrafFieldOutputSignalColor, 2);
            penLocalLine = new System.Drawing.Pen(System.Drawing.Brushes.Black, 1);
            penLocalLineEnd = new System.Drawing.Pen(System.Drawing.Brushes.Red, 3);
            penHighlight = new System.Drawing.Pen(Settings.Default.GrafFieldSygnalSelectionColor, 3);
            penInputLine = new System.Drawing.Pen(System.Drawing.Brushes.DarkBlue, 1);
            penInputLineEnd = new System.Drawing.Pen(System.Drawing.Brushes.DarkGreen, 3);
            brushSignalActive = new System.Drawing.SolidBrush(Settings.Default.GrafFieldSignalActiveColor);
            brushTextColor = new System.Drawing.SolidBrush(Settings.Default.GrafFieldTextColor);
            //И шрифт
            TextFont = new System.Drawing.Font(Settings.Default.GrafFieldTextFont.Name, (Settings.Default.GrafFieldTextFont.Size * ScaleT), Settings.Default.GrafFieldTextFont.Style);

            b_EnableLogging = Settings.Default.LogSimulation;
            if (b_EnableLogging)
                tools.Buttons[6].Visible = true;
            else tools.Buttons[6].Visible = false;

            if (timer1.Enabled)
            {
                timer1.Stop();
                timer1.Interval = int.Parse(Settings.Default.SimulationPeriod);
                timer1.Start();
            }
            else timer1.Interval = int.Parse(Settings.Default.SimulationPeriod);

            Refresh();
        }
        /// <summary>
        /// Закрывает именованный канал
        /// </summary>
        public void ClosePipe()
        {
            if (pipe != null)
            {
                if (pipe.IsConnected)
                {
                    WritePipe(0, 0, 'e');
                    if (pipe.IsConnected) pipe.Disconnect();
                }
                sw = null;
                pipe = null;
                GC.Collect();
            }
        }
        public override void Refresh()
        {
            base.Refresh();         
            RefreshArea(g);
            drawBuffer.Render();
            this.vScroll.Maximum = (int)(1000.0 * ScaleT);
            this.hScroll.Maximum = (int)(1000.0 * ScaleT);
        }

        private void RefreshArea(Graphics g)
        {
            g.Clear(System.Drawing.Color.White);          
            drawStates(g);
            drawLinks(g);
        }

        public void createStates()
        {
            int stateDefaultCentreX = Settings.Default.StateCentre;
            int stateDefaultCentreY = Settings.Default.StateCentre;
            int offsetStateX = Settings.Default.OffsetStateX;
            int offsetStateY = Settings.Default.OffsetStateY;
            foreach (Rule rule in this.rules)
            {
                State startState = rule.startState;
                State endState = rule.endState;
                State[] startAndEndStates = { startState, endState };
                foreach (State state in startAndEndStates)
                {
                    State s = getStateByName(state.Name);
                    if (s == null)
                    {
                        state.paintDot.x = States.Length % 2 == 0 ? stateDefaultCentreX : stateDefaultCentreX + offsetStateX;
                        state.paintDot.y = stateDefaultCentreY + States.Length / 2 * offsetStateY;
                        state.setNameDot();
                        addState(state);
                    }    
                }
            }
        }

        private void drawStates(Graphics g)
        {
            int stateDiametr = Settings.Default.StateDiametr;
            foreach (State state in States)
            {
                Pen statePen = state.Selected ? penOutputSignal : penInputSignal;
                g.DrawEllipse(statePen, state.paintDot.x, state.paintDot.y, stateDiametr, stateDiametr);             
                g.DrawString(state.Name, Settings.Default.StateNameText, brushTextColor, state.nameDot.x, state.nameDot.y);             
            }
        }

        public void createLinks()
        {
            foreach (Rule rule in rules)
            {
                Link link = getLinkByName(rule.startState.Name + rule.endState.Name);
                if (link == null)
                {
                    Array.Resize(ref Links, Links.Length + 1);
                    link = new Link(rule.startState, rule.endState);
                    link.setName();
                    Links[Links.Length - 1] = link;
                }
                Array.Resize(ref link.signals, link.signals.Length + 1);
                link.signals[link.signals.Length - 1] = rule.signal;
                link.initializeLocation();
            }
        }

        private void drawLinks(Graphics g)
        {          
            foreach (Link link in Links)
            {       
                Pen linkPen = link.Selected ? penHighlight : penInputLine;
                g.DrawLine(linkPen, link.startDot.x, link.startDot.y, link.endDot.x, link.endDot.y);               
                for (int i = 0; i < link.signals.Length; i++)
                {
                    g.DrawString(link.signals[i].name, new Font("Consolas", 12), brushTextColor, link.signalDots[i].x, link.signalDots[i].y);
                }
            }
        }

        private Link getLinkByName(String name)
        {
            foreach (Link link in this.Links)
            {
                if (link.name.Equals(name)) return link;
            }
            return null;
        }

        private State getStateByName(String name)
        {
            foreach (State state in States)
            {
                if (state.Name.Equals(name)) return state;
            }
            return null;
        }

        private void addState(State state)
        {
            State currentState = getStateByName(state.Name);
            if (currentState == null)
            {
                Array.Resize(ref States, States.Length + 1);
                States[States.Length - 1] = state;
            }
        }

        private bool isDotOnLink(Dot dot, Link link)
        {
            float x = dot.x;;
            float y = dot.y;
            //Каноническое уравнение прямой на плоскости типа (x-x1)/(x2-x1) = (y-y1)/(y2-y1)
            float result = (link.startDot.y - link.endDot.y) * x + (link.endDot.x - link.startDot.x) * y + (link.startDot.x * link.endDot.y - link.endDot.x * link.startDot.y);
            int offset = 10;
            float maxX = Math.Max(link.startDot.x, link.endDot.x) + offset;
            float minX = Math.Min(link.startDot.x, link.endDot.x) - offset;
            float maxY = Math.Max(link.startDot.y, link.endDot.y) + offset;
            float minY = Math.Min(link.startDot.y, link.endDot.y) - offset;
            return (result >= -2500
                && result <= 2500
                && x >= minX && x <= maxX
                && y >= minY && y <= maxY);
        }

        public bool isDotOnState(Dot dot, State state)
        {
            int diametr = Settings.Default.StateDiametr;
            return dot.x >= state.paintDot.x
                && dot.x <= state.paintDot.x + diametr
                && dot.y >= state.paintDot.y
                && dot.y <= state.paintDot.y + diametr;
        }

        private void setSelectedLink(Dot dot)
        {
            foreach (Link link in Links)
            {
                link.Selected = isDotOnLink(dot, link);
            }
        }

        private void setSelectedState(Dot dot)
        {
            foreach (State state in States)
            {
                state.Selected = isDotOnState(dot, state);
            }
        }
        /// <summary>
        /// Функция обновления(рисования) объекта
        /// </summary>
        private void Refresh(Graphics g)
        {
            float gip, DeltaX, DeltaY, cosa, sina, xn, yn;

            TextFont = new System.Drawing.Font(Settings.Default.GrafFieldTextFont.Name, (Settings.Default.GrafFieldTextFont.Size * ScaleT), Settings.Default.GrafFieldTextFont.Style);
            
            g.Clear(System.Drawing.Color.White);            //Отчищаем буфер заливая его фоном

            if (Links != null)
            {
                /*for (int i = 0; i < Links.Length; i++)
                {
                    if (Links[i].FromInput == true)         //Связи от входных сигналов
                    {
                        if (DrawInputs)
                        {
                            //рисуем связь(темно-синяя линия)
                            if (Links[i].Selected)
                            {
                                g.DrawLine(penHighlight, (Links[i].x1 + xT) * ScaleT, (Links[i].y1 + yT) * ScaleT, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT);
                            }
                            g.DrawLine(penInputLine, (Links[i].x1 + xT) * ScaleT, (Links[i].y1 + yT) * ScaleT, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT);
                            Links[i].setTimeDot();
                            drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY);
                            //вычисляем гипотенузу
                            gip = (float)System.Math.Sqrt(Math.Pow((Links[i].y1 + yT) * ScaleT - (Links[i].y2 + yT) * ScaleT, 2) + Math.Pow((Links[i].x1 + xT) * ScaleT - (Links[i].x2 + xT) * ScaleT, 2));

                            if (Links[i].x2 > Links[i].x1)
                            {
                                DeltaX = (Links[i].x2 - Links[i].x1) * ScaleT;
                                sina = DeltaX / gip;
                                if (Links[i].y2 < Links[i].y1)
                                {//1
                                    DeltaY = (Links[i].y1 - Links[i].y2) * ScaleT;
                                    cosa = DeltaY / gip;
                                    xn = 50 * sina * ScaleT;
                                    yn = 50 * cosa * ScaleT;
                                    g.DrawLine(penInputLineEnd, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT, ((Links[i].x2 + xT) * ScaleT) - xn, ((Links[i].y2 + yT) * ScaleT) + yn);
                                    Links[i].setTimeDot();
                                    drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY);
                                }
                                if (Links[i].y2 > Links[i].y1)
                                {//2
                                    DeltaY = (Links[i].y2 - Links[i].y1) * ScaleT;
                                    cosa = DeltaY / gip;
                                    xn = 50 * sina * ScaleT;
                                    yn = 50 * cosa * ScaleT;
                                    g.DrawLine(penInputLineEnd, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT, ((Links[i].x2 + xT) * ScaleT) - xn, ((Links[i].y2 + yT) * ScaleT) - yn);
                                    Links[i].setTimeDot();
                                    drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY);
                                }
                            }
                            if (Links[i].x2 < Links[i].x1)
                            {
                                DeltaX = (Links[i].x2 - Links[i].x1) * ScaleT;
                                sina = DeltaX / gip;
                                if (Links[i].y2 < Links[i].y1)
                                {//4
                                    DeltaY = (Links[i].y1 - Links[i].y2) * ScaleT;
                                    cosa = DeltaY / gip;
                                    xn = 50 * sina * ScaleT;
                                    yn = 50 * cosa * ScaleT;
                                    g.DrawLine(penInputLineEnd, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT, ((Links[i].x2 + xT) * ScaleT) - xn, ((Links[i].y2 + yT) * ScaleT) + yn);
                                    Links[i].setTimeDot();
                                    drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY);
                                }
                                if (Links[i].y2 > Links[i].y1)
                                {//3
                                    DeltaY = (Links[i].y2 - Links[i].y1) * ScaleT;
                                    cosa = DeltaY / gip;
                                    xn = 50 * sina * ScaleT;
                                    yn = 50 * cosa * ScaleT;
                                    g.DrawLine(penInputLineEnd, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT, ((Links[i].x2 + xT) * ScaleT) - xn, ((Links[i].y2 + yT) * ScaleT) - yn);
                                    Links[i].setTimeDot();
                                    drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY);
                                }
                            }
                        }
                    }
                    else if (Links[i].Arc == true)
                    {
                        g.DrawArc(penLocalLine, (Links[i].x1 - 50 + xT) * ScaleT, (Links[i].y1 - 50 + yT) * ScaleT, 50 * ScaleT, 50 * ScaleT, 0, 360);
                        g.DrawArc(penLocalLineEnd, (Links[i].x1 - 50 + xT) * ScaleT, (Links[i].y1 - 50 + yT) * ScaleT, 50 * ScaleT, 50 * ScaleT, 300, 60);
                    }
                    else
                    {
                        //PointF[] curvePoints = {new PointF((Links[i].x2 + xT) * Scale, (Links[i].y2 + yT) * Scale), new PointF(100.0f,100.0f), new PointF(((Links[i].x2 + xT) * Scale) - xn, ((Links[i].y2 + yT) * Scale) + yn)};
                        //g.DrawCurve(penRed, curvePoints, 1.0f);
                        if (Links[i].Selected)
                        {
                            g.DrawLine(penHighlight, (Links[i].x1 + xT) * ScaleT, (Links[i].y1 + yT) * ScaleT, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT);
                            Links[i].setTimeDot();
                            drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY);
                        }
                        g.DrawLine(penLocalLine, (Links[i].x1 + xT) * ScaleT, (Links[i].y1 + yT) * ScaleT, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT);
                        Links[i].setTimeDot();
                        drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY); gip = (float)System.Math.Sqrt(Math.Pow((Links[i].y1 + yT) * ScaleT - (Links[i].y2 + yT) * ScaleT, 2) + Math.Pow((Links[i].x1 + xT) * ScaleT - (Links[i].x2 + xT) * ScaleT, 2));
                        //xn = Math.Abs((Links[i].y1 + yT) * Scale-(Links[i].y2 + yT) * Scale)/gip*(gip-20);
                        //yn = Math.Abs((Links[i].x1 + xT) * Scale-(Links[i].x2 + xT) * Scale)/gip*(gip-20);
                        //g.DrawLine(p4,(Links[i].x1 + xT) * Scale,(Links[i].y1 + yT) * Scale,xn,yn);
                        //g.DrawLine(p3, (Links[i].x1 + 25)*4, (Links[i].y1 + 25)*4, Links[i].x2 + 25, Links[i].y2 + 25);
                        if (Links[i].x2 > Links[i].x1)
                        {
                            DeltaX = (Links[i].x2 - Links[i].x1) * ScaleT;
                            sina = DeltaX / gip;
                            if (Links[i].y2 < Links[i].y1)
                            {//1
                                DeltaY = (Links[i].y1 - Links[i].y2) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;

                                g.DrawLine(penLocalLineEnd, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT, ((Links[i].x2 + xT) * ScaleT) - xn, ((Links[i].y2 + yT) * ScaleT) + yn);
                                Links[i].setTimeDot();
                                drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY);
                            }
                            if (Links[i].y2 > Links[i].y1)
                            {//2
                                DeltaY = (Links[i].y2 - Links[i].y1) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(penLocalLineEnd, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT, ((Links[i].x2 + xT) * ScaleT) - xn, ((Links[i].y2 + yT) * ScaleT) - yn);
                                Links[i].setTimeDot();
                                drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY);
                            }
                        }
                        if (Links[i].x2 < Links[i].x1)
                        {
                            DeltaX = (Links[i].x2 - Links[i].x1) * ScaleT;
                            sina = DeltaX / gip;
                            if (Links[i].y2 < Links[i].y1)
                            {//4
                                DeltaY = (Links[i].y1 - Links[i].y2) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(penLocalLineEnd, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT, ((Links[i].x2 + xT) * ScaleT) - xn, ((Links[i].y2 + yT) * ScaleT) + yn);
                                Links[i].setTimeDot();
                                drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY);
                            }
                            if (Links[i].y2 > Links[i].y1)
                            {//3
                                DeltaY = (Links[i].y2 - Links[i].y1) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(penLocalLineEnd, (Links[i].x2 + xT) * ScaleT, (Links[i].y2 + yT) * ScaleT, ((Links[i].x2 + xT) * ScaleT) - xn, ((Links[i].y2 + yT) * ScaleT) - yn);
                                Links[i].setTimeDot();
                                drawTime(g, Links[i].timeTransfer, Links[i].timeX, Links[i].timeY);
                            }
                        }
                    }
                }*/
            }
            if (States != null)
            {

               /* for (int i = 0; i < States.Length; i++)
                {
                    if (States[i].InputSignal == true)
                    {
                        if (DrawInputs)
                        {
                            if (States[i].Signaled || States[i].AlSignaled)
                            {
                                g.FillRectangle(brushSignalActive, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                            }
                            else
                            {
                                g.FillRectangle(System.Drawing.Brushes.White, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                            }
                            if (States[i].Selected) g.DrawRectangle(penHighlight, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                            else g.DrawRectangle(penInputSignal, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                            g.DrawString(States[i].Name, TextFont, brushTextColor, (States[i].x + 10 + xT) * ScaleT, (States[i].y + 10 + yT) * ScaleT);
                        }
                    }
                    else if (States[i].Type == STATE_TYPE.OUTPUT)
                    {
                        if (States[i].Signaled || States[i].AlSignaled)
                        {
                            g.FillRectangle(brushSignalActive, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        }
                        else
                        {
                            g.FillRectangle(System.Drawing.Brushes.White, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        }
                        if (States[i].Selected) g.DrawRectangle(penHighlight, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        else g.DrawRectangle(penOutputSignal, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        g.DrawString(States[i].Name, TextFont, brushTextColor, (States[i].x + 10 + xT) * ScaleT, (States[i].y + 10 + yT) * ScaleT);
                    }
                    else
                    {
                        if (States[i].Signaled || States[i].AlSignaled)
                        {
                            g.FillEllipse(brushSignalActive, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        }
                        else
                        {
                            g.FillEllipse(System.Drawing.Brushes.White, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        }
                        if (States[i].Selected) g.DrawEllipse(penHighlight, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        else g.DrawEllipse(penSignal, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        g.DrawString(States[i].Name, TextFont, brushTextColor, (States[i].x + 10 + xT) * ScaleT, (States[i].y + 15 + yT) * ScaleT);
                    }
                }*/
            }
            //Отрисовка панели инструментов
            if (!b_SavingImage)
            {
                if (tools.PanelOrientation == Orientation.Vertical)
                    if (tools.PanelAlignment == Alignment.LEFT)
                    {
                        tools.size = new Size(40, this.Size.Height);
                        tools.Location = new Point(0, 0);
                    }
                    else
                    {
                        tools.size = new Size(40, this.Size.Height);
                        tools.Location = new Point(this.Size.Width - 40 - vScroll.Size.Width, 0);
                    }
                else
                {
                    if (tools.PanelAlignment == Alignment.TOP)
                    {
                        tools.size = new Size(this.Size.Width, 40);
                        tools.Location = new Point(0, 0);
                    }
                    else
                    {
                        tools.size = new Size(this.Size.Width, 40);
                        tools.Location = new Point(0, this.Size.Height - 40 - hScroll.Size.Height);
                    }
                }
                tools.UpdateControlsLocation();
                tools.Draw(ref g); 
            }
            //////////////////////////////////////////////////
            ///////////////////Для отладки//////////////////// 
            ////////////////////////////////////////////////// 
            if (b_ShowDebugInfo)
            {
                int xSS = 700, ySS = 0;
                for (int i = 0; i < Links.Length - 1; i++)
                {
                    if (!Links[i].Arc)
                    {
                        /*if (Links[i].Selected)
                        {
                            g.DrawString("Line" + i.ToString() + " = " + Links[i].leight.ToString() + " : to mouse = " + Links[i].rst.ToString() +
                            "\t\tx1=" + Links[i].x1.ToString() + " y1=" + Links[i].y1.ToString()
                             + " x2=" + Links[i].x2.ToString() + " y2=" + Links[i].y2.ToString(), TextFont, System.Drawing.Brushes.Red, xSS, ySS);
                        }
                        else g.DrawString("Line" + i.ToString() + " = " + Links[i].leight.ToString() + " : to mouse = " + Links[i].rst.ToString() +
                            "\t\tx1=" + Links[i].x1.ToString() + " y1=" + Links[i].y1.ToString()
                             + " x2=" + Links[i].x2.ToString() + " y2=" + Links[i].y2.ToString(), TextFont, System.Drawing.Brushes.Black, xSS, ySS);
                        ySS += 20;*/
                    }
                    else
                    {
                       /* g.DrawString("Arc" + i.ToString() + " = " + Links[i].leight.ToString() + " : to mouse = " + Links[i].rst.ToString() +
                            "\t\tx=" + Links[i].x1.ToString() + " y=" + Links[i].y1.ToString(), TextFont, System.Drawing.Brushes.Black, xSS, ySS);
                        ySS += 20;*/
                    }
                }

                g.DrawString("mouse x = " + curX.ToString() + "\ty = " + curY.ToString(), TextFont, System.Drawing.Brushes.Black, 0, 0);
            }
        }

        private void AreaResized(object sender, EventArgs e)
        {
            g.Dispose();
            drawBuffer.Dispose();
            drawBuffer = drawContext.Allocate(this.CreateGraphics(), this.ClientRectangle);
            g = drawBuffer.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//Включаем сглаживание графических объектов
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;//Включаем сглаживание шрифтов
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;//Включаем интерполяцию
            Refresh();
        }
        /// <summary>
        /// Обработчик горизонтального скролинга
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {
            xT = -hScroll.Value;
            Refresh();
        }
        /// <summary>
        /// Обработчик вертикального скролинга
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vScroll_Scroll(object sender, ScrollEventArgs e)
        {
            yT = -vScroll.Value;
            Refresh();
        }
        /// <summary>
        /// Обработчик клика мышкой по области рисования
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawArea_MouseDown(object sender, MouseEventArgs e)
        {
            float r;
            float xkur;
            float ykur;
            int result;

            xkur = e.X;
            ykur = e.Y;
            r = 30 * ScaleT;
            xM = e.X;
            yM = e.Y;

            if (e.Button == MouseButtons.Left)
            {
//                 if (AddStateButtonSelected)
//                 {
//                     //Mu.WaitOne();
//                     //States[i].Value = LocalStates[i];
//                     Array.Resize(ref States, States.Length + 1);
//                     States[States.Length - 1] = new State();
//                     States[States.Length - 1].x = (int)(((float)e.X - 30 - xT) / ScaleT);
//                     States[States.Length - 1].y = (int)(((float)e.Y - 30 - yT) / ScaleT);
//                     //Mu.ReleaseMutex();
//                 }

                result = CheckSelectedState(xkur, ykur, r, false);
                //CheckSelectedLink(xkur, ykur);
            }
            if (e.Button == MouseButtons.Right && b_SimulStarted)
            {
                if (CheckSelectedState(xkur, ykur, r, true) > -1)
                {
                    contextMenu.Visible = true;
                    contextMenu.Show(this, e.Location);
                }
            }
            Refresh();
        }
        private void drawArea_MouseClick(object sender, MouseEventArgs e)
        {
            tools.CheckMouseState(e, true);
            Refresh();
        }
        /// <summary>
        /// Функция проверки наведения мыши на состояние
        /// </summary>
        /// <param name="xkur">Координата x мыши</param>
        /// <param name="ykur">Координата y мыши</param>
        /// <param name="r">Радиус состояния</param>
        /// <param name="right">Нажата ли правая кнопка мыши</param>
        /// <returns>Номер найденого состояния</returns>
        private int CheckSelectedState(float xkur, float ykur, float r, bool right)
        {
            float x0, y0;
            float f;
            if (States != null)
            {
                for (int i = States.Length - 1; i >= 0; i--)
                {
                    x0 = (States[i].paintDot.x + 30 + xT) * ScaleT;
                    y0 = (States[i].paintDot.y + 30 + yT) * ScaleT;
                    f = (float)System.Math.Pow(x0 - xkur, 2) + (float)System.Math.Pow(y0 - ykur, 2);
                    if (f <= (float)System.Math.Pow(r, 2))
                    {
                        if (!right)
                        {
                            States[SelectedState].Selected = false;
                            StateSelected = true;
                            SelectedState = i;
                            States[i].Selected = true;
                            return i;
                        }
                        else
                        {
                            SelectedState = i;
                            return i;
                        }
                    }
                    else
                    {
                        if (!right)
                        {
                            StateSelected = false;
                            States[i].Selected = false;
                        }
                    }
                }
            }
            return (int)defines.NO_STATE_SELECTED;
        }
        /// <summary>
        /// Функция проверки наведения мыши на линию
        /// и отображения информации о линии
        /// </summary>
        /// <param name="xkur">Координата x мыши</param>
        /// <param name="ykur">Координата y мыши</param>
        private void CheckSelectedLink(float xkur, float ykur)
        {
            bool dl = false;
            double sqrl = 0.0, hlfl = 0.0;
            float dx, dy;
            dx = (xkur - xT) / ScaleT;
            dy = (ykur - yT) / ScaleT;
            LinkSelected = false;
            for (int i = 0; i < Links.Length; i++ )
            {
                if (!LinkSelected)
                {
                    if (!Links[i].Arc)
                    {
                        /*sqrl = Math.Sqrt(Math.Pow((double)(Links[i].x2 - Links[i].x1), 2.0) + Math.Pow((double)(Links[i].y2 - Links[i].y1), 2.0));
                        hlfl = Math.Sqrt(Math.Pow((double)(dx - Links[i].x1), 2.0) + Math.Pow((double)(dy - Links[i].y1), 2.0)) +
                            Math.Sqrt(Math.Pow((double)(dx - Links[i].x2), 2.0) + Math.Pow((double)(dy - Links[i].y2), 2.0));
                        dl = hlfl == sqrl;*/
                        Links[i].leight = sqrl;
                        Links[i].rst = hlfl;
                        if (hlfl - sqrl < 1)
                        {
                            Links[i].Selected = true;
                            //this.toolTip.Show(Links[i].StartState + "->" + Links[i].EndState, this, (int)xkur, (int)ykur - 10, 3000);
                            LinkSelected = true;
                        }
                        else Links[i].Selected = false;
                    }
                }
                else Links[i].Selected = false;
            }
        }
        /// <summary>
        /// Обработчик движения мыши по области рисования
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawArea_MouseMove(object sender, MouseEventArgs e)
        {
            setSelectedLink(new Dot(e.X, e.Y));
            setSelectedState(new Dot(e.X, e.Y));
            Refresh();
        }
        /// <summary>
        /// Обновляет связи между состояниями графа
        /// </summary>
        private void UpdateLinks()
        {
            /*for (int i = 0; i < Links.Length; i++ )
            {
                if (Links[i].startState == States[SelectedState].Name)
                {
                    Links[i].x1 = States[SelectedState].x + 30;
                    Links[i].y1 = States[SelectedState].y + 30;
                    if (Links[i].Moved)
                    {
                        Links[i].x1 += 10;
                        Links[i].y1 += 10;
                    }
                }
                if (Links[i].endState == States[SelectedState].Name)
                {
                    Links[i].x2 = States[SelectedState].x + 30;
                    Links[i].y2 = States[SelectedState].y + 30;
                    if (Links[i].Moved)
                    {
                        Links[i].x2 += 10;
                        Links[i].y2 += 10;
                    }
                }
            }*/
        }
        /// <summary>
        /// Создает состояния для графа
        /// </summary>
        public void CreateStates(ref string[] LocalStates, ref string[] Inputs, ref string[] Outputs)
        {
            int n = 0;
            int counter = 0;
            xs = 50;
            ys = 40;
            Array.Resize(ref States, 0);
            States = new State[Inputs.Length];
            for (int j = 0; j < States.Length; j++)
            {
                States[j] = new State();
            }
            Random rnd = new Random();

            xs = 50;
            for (int i = 0; i < Inputs.Length; i++)
            {
                States[i].Name = Inputs[i];
                States[i].paintDot.x = xs;
                //xs += 70;
                States[i].paintDot.y = ys;
                //States[i].InputSignal = true;
                //States[i].Type = STATE_TYPE.INPUT;
                ys += 62;
            }
            InputsLeight = Inputs.Length;
            Array.Resize(ref States, States.Length + LocalStates.Length);
            for (int i = Inputs.Length; i < States.Length; i++)
            {
                States[i] = new State();
            }
            n = (int)Math.Truncate(Math.Sqrt(States.Length));
            xs = 120;
            ys = 62;
            for (int i = Inputs.Length; i < States.Length; i++)
            {
                States[i].Name = LocalStates[i - Inputs.Length];
                States[i].paintDot.x = /*rnd.Next(0, States.Length / 2) * 62 + xs*/xs;
                States[i].paintDot.y = /*rnd.Next(0, Inputs.Length) * 62 + */ys/*rnd.Next(70, 300)*/;
                //States[i].Type = STATE_TYPE.NONE;

                ys += 70;
                counter++;
                if (counter == n)
                {
                    counter = 0;
                    xs += 70;
                    ys = 62;
                }
            }
            if (Outputs != null && Outputs.Length > 0)
            {
                OutputsLeight = Outputs.Length;
                Array.Resize(ref States, States.Length + Outputs.Length);
                for (int i = (Inputs.Length + LocalStates.Length); i < States.Length; i++)
                {
                    States[i] = new State();
                }
                xs = 112;
                ys = 10;
                for (int i = (Inputs.Length + LocalStates.Length); i < States.Length; i++)
                {
                    States[i].Name = Outputs[i - (Inputs.Length + LocalStates.Length)];
                    States[i].paintDot.x = xs;
                    xs += 62;
                    States[i].paintDot.y = ys;
                   // States[i].Type = STATE_TYPE.OUTPUT;
                }
            }
        }
        /// <summary>
        /// Создает связи для графа
        /// </summary>
        /// <param name="Rules">Массив разобранных правил</param>
        /*public void CreateLinks(ref Rule[] Rules)
        {
            bool DoubleLink = true;
            Array.Resize(ref Links, 0);
            //Обходи по всем правилам
            for (int i = 0; i < Rules.Length; i++)
            {
                //Обход по всем элементам правил не считая 0-го
                for (int j = 1; j < Rules[i].Elems.Length; j++)
                {
                    //Если элемент состояние
                    if (Rules[i].Elems[j].Type == "State")
                    {
                        //Проверка на повторы связей
                        DoubleLink = true;
                        for (int m = 0; m < Links.Length; m++ )
                        {
                            if (Links[m].EndState == Rules[i].Elems[0].Value && Links[m].StartState == Rules[i].Elems[j].Value)
                            {
                                DoubleLink = false;
                            }
                        }
                        //Если не было повтора, то добавляем новую связь
                        if (DoubleLink)
                        {
                            Array.Resize(ref Links, Links.Length + 1);
                            Links[Links.Length - 1] = new Link();

                            Links[Links.Length - 1].EndState = Rules[i].Elems[0].Value;
                            Links[Links.Length - 1].StartState = Rules[i].Elems[j].Value;
                            //Если переход сам в себя, то арка
                            if (Links[Links.Length - 1].EndState == Links[Links.Length - 1].StartState)
                            {
                                Links[Links.Length - 1].Arc = true;
                            }
                            //Задаем координаты начала и конца линий
                            for (int k = 0; k < States.Length; k++)
                            {
                                if (States[k].Name == Rules[i].Elems[0].Value)
                                {
                                    Links[Links.Length - 1].x2 = States[k].x + 30;
                                    Links[Links.Length - 1].y2 = States[k].y + 30;
                                }
                                if (States[k].Name == Rules[i].Elems[j].Value)
                                {
                                    Links[Links.Length - 1].x1 = States[k].x + 30;
                                    Links[Links.Length - 1].y1 = States[k].y + 30;
                                }
                                Links[Links.Length - 1].setTimeDot();
                                if (Rules[i].Elems[j - 1].Type.Equals("TimeTransfer"))
                                {
                                    Links[Links.Length - 1].timeTransfer = float.Parse(Rules[i].Elems[j - 1].Value);
                                }
                            }
                            //Если связь с локальным состоянием то черные линии
                            //иначе синие
                            if (Rules[i].Elems[j].Local == true)
                                Links[Links.Length - 1].FromInput = false;
                            else Links[Links.Length - 1].FromInput = true;
//                             for (int m = 0; m < Links.Length-2; m++)
//                             {
//                                 if ((Links[m].EndState == Links[Links.Length - 2].StartState) && (Links[m].StartState == Links[Links.Length - 2].EndState))
//                                 {
//                                     Links[m].x1 += 10;
//                                     Links[m].x2 += 10;
//                                     Links[m].y1 += 10;
//                                     Links[m].y2 += 10;
//                                     Links[m].Moved = true;
//                                 }
//                             }
                        }
                    }
                }
            }
        }*/

        private Link getLink(State start, State end)
        {
            foreach (Link link in Links)
            {
                if (link.startState == null || link.endState == null) return null;
                if (link.startState.Equals(start) && link.endState.Equals(end))
                {
                    return link;
                }
            }
            return null;
        }

        /*public void createLinks(List<Signal> signals, List<State> states)
        {
            foreach (State state in states)
            {
                foreach (Signal signal in state.outputs)
                {
                    Link link = new Link();
                    link.StartState = state.Name;
                    //link.EndState = signal.
                    //Array.Resize(ref Links, Links.Length + 1);
                    //Links[Links.Length - 1] = link;
                }
            }


            foreach (Signal signal in signals)
            {
                foreach (KeyValuePair<State, State> pair in signal.states)
                {
                    Link link = getLink(pair.Key, pair.Value);
                    if (link == null)
                    {
                        link = new Link();
                        link.signals.Add(signal);
                        link.StartState = pair.Key.Name;
                        link.EndState = pair.Value.Name;
                        if (pair.Key.Equals(pair.Value))
                        {
                            link.Arc = true;
                        }
                        Array.Resize(ref Links, Links.Length + 1);
                        Links[Links.Length - 1] = link;
                    }
                    else
                    {
                        link.signals.Add(signal);
                    }                 
                }               
            }
            
        }*/
        public void ToolPanelButtonClicked(object sender, ToolButtonEventArgs e)
        {
            this.ClickToolPanelTimer.Start();
            switch (e.Name)
            {
                case "start":
                    //CreateSimul((this.Parent.Parent.Parent as Form1).parser.Rules, (this.Parent.Parent.Parent as Form1).parser.Outputs);
                    break;
                case "run":
                    SimulStart();
                    break;
                case "step":
                    SimulStep(true);
                    break;
                case "stop":
                    SimulStop();
                    break;
                case "table":
                    CreateTable();
                    break;
                case "reset":
                    ResetAllsignals();
                    break;
                case "showlog":
                    ShowLog();
                    break;
            }

        }
        private void ClickToolPanelTimer_Tick(object sender, EventArgs e)
        {
            this.OnMouseClick(null);
            this.ClickToolPanelTimer.Stop();
            this.Refresh();
        }

        private void OnSimulStarted()
        {
            drawAreaEventHandler handler = SimulationStarted;
            if (handler != null)
                handler(this, new EventArgs());
        }

        private void OnSimiulStoped()
        {
            drawAreaEventHandler handler = SimulationStoped;
            if (handler != null)
                handler(this, new EventArgs());
        }
        /// <summary>
        /// Создает исходники программы симуляции
        /// компилирует их, запускает программу и 
        /// ожидает подключения к именованному каналу
        /// </summary>
        /// <param name="Rules">Массив разобранных правил</param>
        /*public void CreateSimul(Rule[] Rules, string[] Outputs) 
        {
            int tmp = -1;
            if (!b_SimulStarted)
            {
                String resultCode;
                int index;
                if (pipe != null)
                {
                    if (pipe.IsConnected)
                    {
                        WritePipe(0, 0, 'e');
                        if (pipe.IsConnected) pipe.Disconnect();
                    }
                    sw = null;
                    pipe = null;
                    GC.Collect();
                }
                resultCode = global::sku_to_smv.Properties.Resources.tmpl;
                index = resultCode.IndexOf('$');


                StringBuilder sb = new StringBuilder();

                //Определяем номера состояний
                resultCode = resultCode.Remove(index, 1);
                for (int i = 0; i < States.Length; i++)
                {
                    sb.AppendLine(States[i].Name.ToUpper() + " = " + i.ToString() + ",");
                }
                resultCode = resultCode.Insert(index, sb.ToString());
                sb.Clear();
                index = resultCode.IndexOf('$');
                resultCode = resultCode.Remove(index, 1);
                resultCode = resultCode.Insert(index, States.Length.ToString());

                index = resultCode.IndexOf('$');
                resultCode = resultCode.Remove(index, 1);
                resultCode = resultCode.Insert(index, States.Length.ToString());


                index = resultCode.IndexOf('$');
                resultCode = resultCode.Remove(index, 1);

                for (int i = 0; i < Rules.Length; i++)
                {
                    if (!Rules[i].output)
                    {
                        sb.Append("if(");
                        for (int j = 1; j < Rules[i].Elems.Length; j++)
                        {

                            if ((Rules[i].Elems[j].Type != "=") && (Rules[i].Elems[j].Type != "t+1") && (!Rules[i].Elems[j].Empty))
                            {
                                if (Rules[i].Elems[j].Type == "State")
                                {
                                    if (Rules[i].Elems[j].Inverted)
                                    {
                                        sb.Append(" !");
                                    }
                                    sb.Append("curState[(int)simulDefines." + Rules[i].Elems[j].Value.ToUpper() + "] ");
                                }
                                else
                                {
                                    if (Rules[i].Elems[j].Type == "|") sb.Append(" || ");
                                    if (Rules[i].Elems[j].Type == "&") sb.Append(" && ");
                                    if (Rules[i].Elems[j].Type == "(")
                                    {
                                        if (Rules[i].Elems[j].Inverted)
                                        {
                                            sb.Append(" !");
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
                        tmp = -1;
                        bool br = false;
                        for (int j = 0; j < Outputs.Length; j++ )
                        {
                            for (int k = 0; k < Rules.Length; k++ )
                            {
                                if (Rules[k].Elems[0].Value == Outputs[j] && Rules[i].Elems[0].Value == Rules[k].Elems[2].Value)
                                {
                                    tmp = k;
                                    br = true;
                                    break;
                                }
                                else tmp = -1;
                            }
                            if (br) break;
                        }
                        sb.Append(") {" + "newState[(int)simulDefines." + Rules[i].Elems[0].Value.ToUpper() + "] = true;");
                        if (tmp != -1)
                        {
                            sb.Append("\nnewState[(int)simulDefines." + Rules[tmp].Elems[0].Value.ToUpper() + "] = true;}");
                        }
                        else sb.Append("}");
                        sb.Append("\nelse {" + "newState[(int)simulDefines." + Rules[i].Elems[0].Value.ToUpper() + "] = false;");
                        if (tmp != -1)
                        {
                            sb.Append("\nnewState[(int)simulDefines." + Rules[tmp].Elems[0].Value.ToUpper() + "] = false;}");
                        }
                        else sb.Append("}");
                        sb.AppendLine();
                    }
                }
                resultCode = resultCode.Insert(index, sb.ToString());

                // Настройки компиляции
                Dictionary<string, string> providerOptions = new Dictionary<string, string>
         {
           {"CompilerVersion", "v3.5"}
         };
                CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

                string outputAssembly = "simul.exe";
                CompilerParameters compilerParams = new CompilerParameters { OutputAssembly = outputAssembly, GenerateExecutable = true };
                compilerParams.ReferencedAssemblies.Add("System.Core.dll");

                // Компиляция
                CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, resultCode);

                FileInfo fi = new FileInfo(outputAssembly);
                if (results.Errors.Count == 0 && fi.Exists)
                {
                    ProcessStartInfo psi= new ProcessStartInfo(outputAssembly);
                    psi.WindowStyle = ProcessWindowStyle.Minimized;
                    Process pr = Process.Start(psi);
                    pipe = new NamedPipeServerStream("{E8B5BDF5-725C-4BF4-BCA4-2427875DF2E0}", PipeDirection.InOut);
                    pipe.WaitForConnection();
                    sw = new StreamWriter(pipe);
                    sw.AutoFlush = true;
                    b_SimulStarted = true;
                    tools.Buttons[0].SetImage(global::sku_to_smv.Properties.Resources.stop_simulation);
                    tools.Buttons[0].Text = "Остановить симуляцию";
                    tools.Buttons[1].Enabled = true;
                    tools.Buttons[2].Enabled = true;
                    //tools.Buttons[3].Enabled = true;
                    tools.Buttons[4].Enabled = true;
                }
            }
            else
            {
                if (TableCreated)
                {
                    table.Close();
                }
                SimulStop();
                b_SimulStarted = false;
                if (pipe.IsConnected)
                {
                    WritePipe(0, 0, 'e');
                    if (pipe.IsConnected) pipe.Disconnect();
                }
                sw = null;
                pipe = null;
                GC.Collect();
                tools.Buttons[0].SetImage(global::sku_to_smv.Properties.Resources.create_simulation);
                tools.Buttons[0].Text = "Запустить симуляцию";
                tools.Buttons[1].Enabled = false;
                tools.Buttons[2].Enabled = false;
                tools.Buttons[3].Enabled = false;
                tools.Buttons[4].Enabled = false;
            }
        }*/
        /// <summary>
        /// Запись в именованный канал
        /// </summary>
        /// <param name="num">Номер сигнала</param>
        /// <param name="b">Значение сигнала</param>
        /// <param name="ch">Тип сообщения</param>
        private void WritePipe(int num, int b, char ch, int step = 0)
        {
            try
            {
                if (pipe != null && pipe.IsConnected)
                {
                    if (ch == 's')
                    {
                        sw.WriteLine("set " + num.ToString() + " " + b.ToString());
                    }
                    if (ch == 't')
                    {
                        sw.WriteLine("step " + step);
                    }
                    if (ch == 'e')
                    {
                        sw.WriteLine("exit");
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
            }

        }
        /// <summary>
        /// Чтение из именованного канала
        /// </summary>
        /// <param name="num">Номер сигнала</param>
        /// <returns>Значение сигнала</returns>
        private bool ReadPipe(int num)
        {
            if (pipe != null && pipe.IsConnected)
            {
                sw.WriteLine("get " + num.ToString() + " ");
                pipe.WaitForPipeDrain();
                return pipe.ReadByte() > 0? true : false;
            }
            return false;
        }
        /// <summary>
        /// Записк автоматической симуляции
        /// </summary>
        public void SimulStart()
        {
            OnSimulStarted();
            if (b_EnableLogging)
            {
                log.LogFormat = Settings.Default.LogFormat;
                if (LogFileName.Length > 0)
                {
                    log.FileName = LogFileName;
                }
                else log.FileName = "log_" + States.GetHashCode().ToString();
                log.StartLog(true, false, null);
            }

            tools.Buttons[1].Enabled = false;
            tools.Buttons[2].Enabled = false;
            tools.Buttons[3].Enabled = true;

            if (table != null)
            {
                table.UpdateSimControls(new bool[] {true, false, true});
                table.ResetSteps();
                if (TableCreated)
                {
                    for (int i = 0; i < InputsLeight; i++)
                    {
                        switch (table.GetElementByNumber(i))
                        {
                           /* case returnResults.rFALSE: States[i].Signaled = States[i].Signaled || false;
                                break;
                            case returnResults.rTRUE: States[i].Signaled = States[i].Signaled || true;
                                break;*/
                            case returnResults.rUNDEF: break;
                        }
                    }
                }
                //Refresh();
            }
            for (int i = 0; i < States.Length; i++ )
            {
                log.StartLog(false, false, States[i].Name);
            }
            log.StartLog(false, true, null);
            this.timer1.Start();
        }
        /// <summary>
        /// Останов автоматической симуляции
        /// </summary>
        public void SimulStop()
        {
            if (b_EnableLogging && this.timer1.Enabled)
                log.EndLog();
            if (table != null)
                table.UpdateSimControls(new bool[] { true, true, false });
            this.timer1.Stop();
            OnSimiulStoped();
            tools.Buttons[1].Enabled = true;
            tools.Buttons[2].Enabled = true;
            tools.Buttons[3].Enabled = false;
        }
        /// <summary>
        /// Шаг симуляции
        /// </summary>
        public void SimulStep(bool b_Manual = false)
        {
            if (b_Manual)
            {
                if (TableCreated)
                {
                    for (int i = 0; i < InputsLeight; i++)
                    {
                        switch (table.GetElementByNumber(i))
                        {
                            /*case returnResults.rFALSE: States[i].Signaled = States[i].Signaled || false;
                                break;
                            case returnResults.rTRUE: States[i].Signaled = States[i].Signaled || true;
                                break;*/
                            case returnResults.rUNDEF: break;
                        }
                    }
                }
                Refresh();
                System.Threading.Thread.Sleep(500);
            }
            bool StepStart = true;

            for (int i = 0; i < States.Length; i++)
            {
                if (b_EnableLogging /*&& States[i].InputSignal == true*/)
                {
                    //log.AddToLog(States[i].Name, States[i].Signaled || States[i].AlSignaled, States[i].Type == STATE_TYPE.INPUT, States[i].Type == STATE_TYPE.OUTPUT, StepStart);
                    StepStart = false;
                }
               // WritePipe(i, Convert.ToInt16((States[i].Signaled || States[i].AlSignaled)), 's');
            }
            WritePipe(0, 0, 't', stepNumber);
            stepNumber++;
            //StepStart = true;
            for (int i = 0; i < States.Length; i++)
            {
                    //States[i].Signaled = ReadPipe(i);
//                 if (b_EnableLogging && States[i].InputSignal != true)
//                 {
//                     log.AddToLog(States[i].Name, States[i].Signaled || States[i].AlSignaled, false, StepStart);
//                     StepStart = false;
//                 }
            }
            if (TableCreated)
            {
                table.NextStep();
            }
            if (TableCreated)
            {
                for (int i = 0; i < InputsLeight; i++)
                {
                    switch (table.GetElementByNumber(i))
                    {
                       /* case returnResults.rFALSE: States[i].Signaled = States[i].Signaled || false;
                            break;
                        case returnResults.rTRUE: States[i].Signaled = States[i].Signaled || true;
                            break;*/
                        case returnResults.rUNDEF: break;
                    }
                }
            }
            Refresh();
        }
        /// <summary>
        /// Обработчик такта таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            SimulStep();
        }
        /// <summary>
        /// Обработчик клика по выпадающему меню
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuClick(object sender, ToolStripItemClickedEventArgs  e)
        {
            switch (e.ClickedItem.Text)
            {
                /*case "Установить 1": States[SelectedState].Signaled = true; 
                    break;
                case "Всегда 1": States[SelectedState].AlSignaled = true;
                    break;
                case "Установить 0": States[SelectedState].Signaled = false;
                    States[SelectedState].AlSignaled = false; 
                    break;*/
            }
            Refresh();
        }
        /// <summary>
        /// Создает таблицу сигналов и отображает ее
        /// </summary>
        public void CreateTable()
        {
            if (InputsLeight != 0 && !TableCreated)
            {
                if (table != null)
                {
                    this.table.FormClosed -= handler;
                }
                table = null;
                GC.Collect();
                table = new SignalTable(InputsLeight, this);
                this.table.FormClosed += handler;
                for (int i = 0; i < InputsLeight; i++)
                {
                   // table.AddElement(i, States[i].Name, States[i].Signaled, States[i].InputSignal);
                }
                table.ShowT();
                TableCreated = true;
            }
        }
        /// <summary>
        /// Обработчик закрытия таблицы сигналов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableClosed(object sender, FormClosedEventArgs e)
        {
            TableCreated = false;
        }
        public void SaveImage(String Path)
        {
            int maxX = 0;
            int maxY = 0;
            int tempX, tempY;
            float tempScale;
            for (int i = 0; i < States.Length; i++ )
            {
                if (States[i].paintDot.x > maxX) maxX = (int) States[i].paintDot.x;
                if (States[i].paintDot.y > maxY) maxY = (int) States[i].paintDot.y;
            }
            Bitmap imageB = new Bitmap(maxX + 70, maxY + 70);
            Graphics graf = Graphics.FromImage(imageB);
            graf.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//Включаем сглаживание графических объектов
            graf.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;//Включаем сглаживание шрифтов
            graf.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;//Включаем интерполяцию
            tempX = xT;
            tempY = yT;
            tempScale = ScaleT;
            xT = 0;
            yT = 0;
            ScaleT = 1F;
            b_SavingImage = true;
            Refresh(graf);
            b_SavingImage = false;
            xT = tempX;
            yT = tempY;
            ScaleT = tempScale;
            imageB.Save(Path);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//Включаем сглаживание графических объектов
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;//Включаем сглаживание шрифтов
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;//Включаем интерполяцию
            Refresh(e.Graphics);
        }
        private void ResetAllsignals()
        {
            for (int i = 0; i < States.Length; i++ )
            {
              //  States[i].Signaled = false;
                //States[i].AlSignaled = false;
            }
        }

        private void ShowLog()
        {
            Process pr;
            ProcessStartInfo prInf = new ProcessStartInfo();
            prInf.FileName = log.SavedFileName;
            if(log.FileName != null && log.FileName.Length > 0)
                pr = Process.Start(prInf);
            //prInf.Arguments = log.FileName;
            /*if ((prInf.Arguments = log.FileName) == null)
            {
                if (LogFileName.Length > 0)
                {
                    prInf.Arguments = LogFileName + ".log";
                }
                else prInf.Arguments = "log" + States.GetHashCode().ToString() + ".log";
            }
            prInf.FileName = "notepad.exe";
            Process pr = Process.Start(prInf);*/
        }
        public void ClearArea()
        {
            if (b_SimulStarted)
            {
                //CreateSimul(null, null);
            }
            Array.Resize(ref Links, 0);
            Array.Resize(ref States, 0);
            GC.Collect();
        }

        private void drawTime(Graphics g, float time, float x, float y)
        {
            if (time != 0) g.DrawString(time.ToString(), new Font("TimesNewRoman", 20), Brushes.Black, x, y);
        }
    } 
}
