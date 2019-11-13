using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Windows.Forms;
using Microsoft.CSharp;
using SCUConverterDrawArea.Properties;

namespace SCUConverterDrawArea
{
    class DrawArea : PictureBox
    {
        HScrollBar hScroll;
        VScrollBar vScroll;
        ContextMenuStrip contextMenu;
        ToolTip toolTip;
        Point cursorLastPosition;
        Timer clickToolPanelTimer;

        ToolPanel toolPanel;
        Graphics graphics;
        Pen highlightPen;
        Pen signalPen;
        Pen inputSignalPen;
        Pen outputSignalPen;
        Pen localLinePen;
        Pen localLineEndPen;
        Pen inputLinePen;
        Pen inputLineEndPen;
        Brush textColorBrush;
        Brush signalActiveBrush;
        Font textFont;

        State[] states;
        Link[] links;
        NamedPipeServerStream pipe;
        StreamWriter sw;
        SignalTable table;
        BufferedGraphicsContext drawContext;
        BufferedGraphics drawBuffer;
        LogWriter log;

        int xT, yT;
        int xs, ys, xM, yM;
        int curX, curY;
        int inputsLeight;
        bool selectState;
        bool selectLink;
        bool createdTable;
        bool showDebugInfo;
        bool savingImage;
        int selectedState;
        int stepNumber;
        bool inputsDraw;
        bool enableLogs;

        public bool isStartSimul { get; set; }
        public string pathToLogFile { get; set; }
        private Timer timer;
        private IContainer components;
        FormClosedEventHandler handler;

        public delegate void drawAreaEventHandler(object sender, EventArgs a);
        public event drawAreaEventHandler SimulationStarted;
        public event drawAreaEventHandler SimulationEnded;

        private float scaleT; 
        
        public float ScaleT
        {
            get { return scaleT; }
            set
            {
                scaleT = value;
                textFont = new Font(Settings.Default.GrafFieldTextFont.Name, 
                    Settings.Default.GrafFieldTextFont.Size * value, 
                    Settings.Default.GrafFieldTextFont.Style);
            }
        }

       public DrawArea()
        {
            InitializeArea();
        }
       
        ~DrawArea() 
        {
            hScroll.Dispose();
            vScroll.Dispose();
            graphics.Dispose();
            Array.Clear(states, 0, states.Length);
            Array.Clear(links, 0, links.Length);
        }
        
        /// <summary>
        /// Функция инициализации области рисования
        /// </summary>
        private void InitializeArea()
        {
            showDebugInfo = false;
            ScaleT = 1F;
            xT = 0;
            yT = 0;
            stepNumber = 1;
            inputsLeight = 0;

            inputsDraw = true;
            isStartSimul = false;
            selectLink = false;
            createdTable = false;
            savingImage = false;
            enableLogs = true;
            pathToLogFile = String.Empty;

            log = new LogWriter();
            drawContext = new BufferedGraphicsContext();
            handler = TableClosed;
            components = new Container();
            toolTip = new ToolTip(components);
            timer = new Timer(components);
            ((ISupportInitialize)this).BeginInit();
            SuspendLayout();

            links = new Link[1];
            links[0] = new Link();

            states = new State[0];

            DoubleBuffered = true;

            toolPanel = new ToolPanel();
            toolPanel.BackColor = Color.FromArgb(120, 145, 217, 255);
           
            toolPanel.ButtonClicked += ToolPanelButtonClicked;
            toolPanel.PanelOrientation = Orientation.Vertical;
            toolPanel.PanelAlignment = Alignment.RIGHT;

            var toolButton = new ToolButton();
            toolButton.SetFrame(Resources.frame);
            toolButton.SetFrameHover(Resources.frame_hover);
            toolButton.SetFrameClick(Resources.frame_click);
            toolButton.SetInactiveImage(Resources.create_simulation);
            toolButton.SetImage(Resources.create_simulation);
            toolButton.Name = "start";
            toolButton.Text = "Запустить Моделирование системы";
            toolPanel.AddControl(ref toolButton);

            toolButton = new ToolButton();
            toolButton.SetFrame(Resources.frame);
            toolButton.SetFrameHover(Resources.frame_hover);
            toolButton.SetFrameClick(Resources.frame_click);
            toolButton.SetInactiveImage(Resources.play_grey);
            toolButton.SetImage(Resources.play);
            toolButton.Name = "run";
            toolButton.Text = "Запуск моделирования";
            toolPanel.AddControl(ref toolButton);

            toolButton = new ToolButton();
            toolButton.SetFrame(Resources.frame);
            toolButton.SetFrameHover(Resources.frame_hover);
            toolButton.SetFrameClick(Resources.frame_click);
            toolButton.SetInactiveImage(Resources.step_grey);
            toolButton.SetImage(Resources.step);
            toolButton.Name = "step";
            toolButton.Text = "Шаг с остановом";
            toolPanel.AddControl(ref toolButton);

            toolButton = new ToolButton();
            toolButton.SetFrame(Resources.frame);
            toolButton.SetFrameHover(Resources.frame_hover);
            toolButton.SetFrameClick(Resources.frame_click);
            toolButton.SetInactiveImage(Resources.stop_grey);
            toolButton.SetImage(Resources.stop);
            toolButton.Name = "stop";
            toolButton.Text = "Остановить моделирование";
            toolPanel.AddControl(ref toolButton);

            toolButton = new ToolButton();
            toolButton.SetFrame(Resources.frame);
            toolButton.SetFrameHover(Resources.frame_hover);
            toolButton.SetFrameClick(Resources.frame_click);
            toolButton.SetInactiveImage(Resources.table_grey);
            toolButton.SetImage(Resources.table);
            toolButton.Name = "table";
            toolButton.Text = "Таблица сигналов";
            toolPanel.AddControl(ref toolButton);

            toolButton = new ToolButton();
            toolButton.SetFrame(Resources.frame);
            toolButton.SetFrameHover(Resources.frame_hover);
            toolButton.SetFrameClick(Resources.frame_click);
            toolButton.SetImage(Resources.reset_all);
            toolButton.Name = "reset";
            toolButton.Text = "Сбросить все сигналы";
            toolPanel.AddControl(ref toolButton);

            toolButton = new ToolButton();
            toolButton.SetFrame(Resources.frame);
            toolButton.SetFrameHover(Resources.frame_hover);
            toolButton.SetFrameClick(Resources.frame_click);
            toolButton.SetImage(Resources.show_log);
            toolButton.Name = "showlog";
            toolButton.Text = "Показать лог-файл";
            toolPanel.AddControl(ref toolButton);

            toolPanel.Buttons[1].Enabled = false;
            toolPanel.Buttons[2].Enabled = false;
            toolPanel.Buttons[3].Enabled = false;
            toolPanel.Buttons[4].Enabled = false;

            hScroll = new HScrollBar();
            vScroll = new VScrollBar();
            contextMenu = new ContextMenuStrip(components) {Visible = false};
            contextMenu.Items.Add("Установить 1");
            contextMenu.Items.Add("Всегда 1");
            contextMenu.Items.Add("Установить 0");
            contextMenu.ItemClicked += clickContextMenu;

            Controls.Add(hScroll);
            Controls.Add(vScroll);

            hScroll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            hScroll.LargeChange = 50;
            hScroll.Location = new Point(0, Height - 20);
            hScroll.Maximum = 1000;
            hScroll.Name = "hScroll";
            hScroll.Size = new Size(Width - 20, 20);
            hScroll.TabIndex = 2;
            hScroll.Scroll += hScroll_Scroll;

            vScroll.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            vScroll.LargeChange = 50;
            vScroll.Location = new Point(this.Width - 20, 0);
            vScroll.Maximum = 1000;
            vScroll.Name = "vScroll";
            vScroll.Size = new Size(20, Height);
            vScroll.TabIndex = 1;
            vScroll.Scroll += vScroll_Scroll;
            
            drawBuffer = drawContext.Allocate(CreateGraphics(), ClientRectangle);
            graphics = drawBuffer.Graphics;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            ApplySettings();

            MouseClick += drawArea_MouseClick;
            MouseDown += drawArea_MouseDown;
            MouseMove += drawArea_MouseMove;
            SizeChanged += AreaResized;

            clickToolPanelTimer = new Timer {Interval = 200};
            clickToolPanelTimer.Tick += ClickToolPanelTimer_Tick;

            timer.Tick += timer1_Tick;
            ((ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }
        
        public void ApplySettings()
        {
            signalPen = new Pen(Settings.Default.GrafFieldLocalSignalColor, 2);
            inputSignalPen = new Pen(Color.Aqua, 2);
            outputSignalPen = new Pen(Settings.Default.GrafFieldOutputSignalColor, 2);
            localLinePen = new Pen(Brushes.DarkBlue, 3);
            localLineEndPen = new Pen(Brushes.Orange, 3);
            highlightPen = new Pen(Settings.Default.GrafFieldSygnalSelectionColor, 3);
            inputLinePen = new Pen(Brushes.DarkRed, 3);
            inputLineEndPen = new Pen(Brushes.LightGreen, 3);
            signalActiveBrush = new SolidBrush(Color.LightPink);
            textColorBrush = new SolidBrush(Settings.Default.GrafFieldTextColor);
            textFont = new Font("Consols", 20);

            enableLogs = Settings.Default.LogSimulation;
            toolPanel.Buttons[6].Visible = enableLogs;

            if (timer.Enabled)
            {
                timer.Stop();
                timer.Interval = int.Parse(Settings.Default.SimulationPeriod);
                timer.Start();
            }
            else
            {
                timer.Interval = int.Parse(Settings.Default.SimulationPeriod);
            }

            Refresh();
        }
        
        /// <summary>
        /// Закрывает именованный канал
        /// </summary>
        public void ClosePipe()
        {
            if (pipe == null)
            {
                return;
            }
            
            if (pipe.IsConnected)
            {
                WritePipe(0, 0, 'e');
                if (pipe.IsConnected)
                {
                    pipe.Disconnect();
                }
            }
            sw = null;
            pipe = null;
        }
        
        public override void Refresh()
        {
            GC.Collect();
            base.Refresh();
            Refresh(graphics);
            drawBuffer.Render();
            vScroll.Maximum = (int)(1000.0 * ScaleT);
            hScroll.Maximum = (int)(1000.0 * ScaleT);
        }
        
        /// <summary>
        /// Функция обновления(рисования) объекта
        /// </summary>
        private void Refresh(Graphics g)
        {
            textFont = new Font("Consols", 15, FontStyle.Italic);

            g.Clear(Color.White);
            g.DrawImage(new Bitmap("../../resources/backgroung_graph.jpg"), 1, 1);
            if (links != null)
            {
                foreach (var link in links)
                {
                    float deltaX;
                    float gip;
                    float deltaY;
                    float cosa;
                    float sina;
                    float xn;
                    float yn;
                    if (link.FromInput)
                    {
                        if (!inputsDraw)
                        {
                            continue;
                        }
                        
                        if (link.Selected)
                        {
                            g.DrawLine(highlightPen,
                                (link.x1 + xT) * ScaleT, (link.y1 + yT) * ScaleT,
                                (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT);
                        }

                        g.DrawLine(inputLinePen,
                            (link.x1 + xT) * ScaleT, (link.y1 + yT) * ScaleT,
                            (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT);
                        link.setTimeDot();
                        DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                        gip = (float) System.Math.Sqrt(
                            Math.Pow((link.y1 + yT) * ScaleT - (link.y2 + yT) * ScaleT, 2) +
                            Math.Pow((link.x1 + xT) * ScaleT - (link.x2 + xT) * ScaleT, 2));

                        if (link.x2 > link.x1)
                        {
                            deltaX = (link.x2 - link.x1) * ScaleT;
                            sina = deltaX / gip;
                            if (link.y2 < link.y1)
                            {
                                deltaY = (link.y1 - link.y2) * ScaleT;
                                cosa = deltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(inputLineEndPen,
                                    (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT,
                                    (link.x2 + xT) * ScaleT - xn, (link.y2 + yT) * ScaleT + yn);
                                link.setTimeDot();
                                DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                            }

                            if (link.y2 > link.y1)
                            {
                                deltaY = (link.y2 - link.y1) * ScaleT;
                                cosa = deltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(inputLineEndPen,
                                    (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT,
                                    (link.x2 + xT) * ScaleT - xn, (link.y2 + yT) * ScaleT - yn);
                                link.setTimeDot();
                                DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                            }
                        }

                        if (link.x2 >= link.x1)
                        {
                            continue;
                        }
                            
                        deltaX = (link.x2 - link.x1) * ScaleT;
                        sina = deltaX / gip;
                        if (link.y2 < link.y1)
                        {
                            deltaY = (link.y1 - link.y2) * ScaleT;
                            cosa = deltaY / gip;
                            xn = 50 * sina * ScaleT;
                            yn = 50 * cosa * ScaleT;
                            g.DrawLine(inputLineEndPen,
                                (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT,
                                (link.x2 + xT) * ScaleT - xn, (link.y2 + yT) * ScaleT + yn);
                            link.setTimeDot();
                            DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                        }

                        if (link.y2 <= link.y1)
                        {
                            continue;
                        }
                                
                        deltaY = (link.y2 - link.y1) * ScaleT;
                        cosa = deltaY / gip;
                        xn = 50 * sina * ScaleT;
                        yn = 50 * cosa * ScaleT;
                        g.DrawLine(inputLineEndPen,
                            (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT,
                            (link.x2 + xT) * ScaleT - xn, (link.y2 + yT) * ScaleT - yn);
                        link.setTimeDot();
                        DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                    }
                    else if (link.Arc)
                    {
                        g.DrawArc(localLinePen, (link.x1 - 50 + xT) * ScaleT,
                            (link.y1 - 50 + yT) * ScaleT, 50 * ScaleT, 50 * ScaleT, 0, 360);
                        g.DrawArc(localLineEndPen, (link.x1 - 50 + xT) * ScaleT,
                            (link.y1 - 50 + yT) * ScaleT, 50 * ScaleT, 50 * ScaleT, 300, 60);
                    }
                    else
                    {
                        if (link.Selected)
                        {
                            g.DrawLine(highlightPen, (link.x1 + xT) * ScaleT, (link.y1 + yT) * ScaleT,
                                (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT);
                            link.setTimeDot();
                            DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                        }

                        g.DrawLine(localLinePen,
                            (link.x1 + xT) * ScaleT, (link.y1 + yT) * ScaleT, (link.x2 + xT) * ScaleT,
                            (link.y2 + yT) * ScaleT);
                        link.setTimeDot();
                        DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                        gip = (float) Math.Sqrt(Math.Pow((link.y1 + yT) * ScaleT - (link.y2 + yT) * ScaleT, 2)
                                                + Math.Pow((link.x1 + xT) * ScaleT - (link.x2 + xT) * ScaleT, 2));
                        if (link.x2 > link.x1)
                        {
                            deltaX = (link.x2 - link.x1) * ScaleT;
                            sina = deltaX / gip;
                            if (link.y2 < link.y1)
                            {
                                deltaY = (link.y1 - link.y2) * ScaleT;
                                cosa = deltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;

                                g.DrawLine(localLineEndPen, (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT,
                                    (link.x2 + xT) * ScaleT - xn, (link.y2 + yT) * ScaleT + yn);
                                link.setTimeDot();
                                DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                            }

                            if (link.y2 > link.y1)
                            {
                                deltaY = (link.y2 - link.y1) * ScaleT;
                                cosa = deltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(localLineEndPen,
                                    (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT,
                                    (link.x2 + xT) * ScaleT - xn, ((link.y2 + yT) * ScaleT) - yn);
                                link.setTimeDot();
                                DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                            }
                        }

                        if (link.x2 >= link.x1)
                        {
                            continue;
                        }
                        
                        deltaX = (link.x2 - link.x1) * ScaleT;
                        sina = deltaX / gip;
                        if (link.y2 < link.y1)
                        {
                            deltaY = (link.y1 - link.y2) * ScaleT;
                            cosa = deltaY / gip;
                            xn = 50 * sina * ScaleT;
                            yn = 50 * cosa * ScaleT;
                            g.DrawLine(localLineEndPen, (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT,
                                ((link.x2 + xT) * ScaleT) - xn, ((link.y2 + yT) * ScaleT) + yn);
                            link.setTimeDot();
                            DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                        }

                        if (link.y2 <= link.y1)
                        {
                            continue;
                        }
                        
                        deltaY = (link.y2 - link.y1) * ScaleT;
                        cosa = deltaY / gip;
                        xn = 50 * sina * ScaleT;
                        yn = 50 * cosa * ScaleT;
                        g.DrawLine(localLineEndPen, (link.x2 + xT) * ScaleT, (link.y2 + yT) * ScaleT,
                            ((link.x2 + xT) * ScaleT) - xn, ((link.y2 + yT) * ScaleT) - yn);
                        link.setTimeDot();
                        DrawTime(g, link.timeTransfer, link.timeX, link.timeY);
                    }
                }
            }

            if (states != null)
            {
                foreach (var state in states)
                {
                    if (state.InputSignal)
                    {
                        if (!inputsDraw)
                        {
                            continue;
                        }
                        
                        if (state.Signaled || state.AlSignaled)
                        {
                            g.FillRectangle(signalActiveBrush, (state.x + xT) * ScaleT, (state.y + yT) * ScaleT,
                                60 * ScaleT, 60 * ScaleT);
                        }
                        else
                        {
                            g.FillRectangle(Brushes.White, x: (state.x + xT) * ScaleT, y: (state.y + yT) * ScaleT,
                                width: 60 * ScaleT, height: 60 * ScaleT);
                        }

                        g.DrawRectangle(state.Selected ? highlightPen : inputSignalPen, (state.x + xT) * ScaleT,
                            (state.y + yT) * ScaleT,
                            60 * ScaleT, 60 * ScaleT);

                        g.DrawString(state.Name, textFont, textColorBrush,
                            (state.x + 10 + xT) * ScaleT, (state.y + 10 + yT) * ScaleT);
                    }
                    else if (state.Type == STATE_TYPE.OUTPUT)
                    {
                        if (state.Signaled || state.AlSignaled)
                        {
                            g.FillRectangle(signalActiveBrush, (state.x + xT) * ScaleT, (state.y + yT) * ScaleT,
                                60 * ScaleT, 60 * ScaleT);
                        }
                        else
                        {
                            g.FillRectangle(Brushes.White, (state.x + xT) * ScaleT, (state.y + yT) * ScaleT,
                                60 * ScaleT, 60 * ScaleT);
                        }

                        g.DrawRectangle(state.Selected ? highlightPen : outputSignalPen, (state.x + xT) * ScaleT,
                            (state.y + yT) * ScaleT,
                            60 * ScaleT, 60 * ScaleT);

                        g.DrawString(state.Name, textFont, textColorBrush, (state.x + 10 + xT) * ScaleT,
                            (state.y + 10 + yT) * ScaleT);
                    }
                    else
                    {
                        if (state.Signaled || state.AlSignaled)
                        {
                            g.FillEllipse(signalActiveBrush, (state.x + xT) * ScaleT, (state.y + yT) * ScaleT,
                                60 * ScaleT, 60 * ScaleT);
                        }
                        else
                        {
                            g.FillEllipse(Brushes.White, (state.x + xT) * ScaleT, (state.y + yT) * ScaleT,
                                60 * ScaleT, 60 * ScaleT);
                        }

                        g.DrawEllipse(state.Selected ? highlightPen : signalPen, (state.x + xT) * ScaleT,
                            (state.y + yT) * ScaleT,
                            60 * ScaleT, 60 * ScaleT);

                        g.DrawString(state.Name, textFont, textColorBrush,
                            (state.x + 10 + xT) * ScaleT, (state.y + 15 + yT) * ScaleT);
                    }
                }
            }

            if (savingImage)
            {
                return;
            }
            
            if (toolPanel.PanelOrientation == Orientation.Vertical)
                if (toolPanel.PanelAlignment == Alignment.LEFT)
                {
                    toolPanel.size = new Size(40, this.Size.Height);
                    toolPanel.Location = new Point(0, 0);
                }
                else
                {
                    toolPanel.size = new Size(40, this.Size.Height);
                    toolPanel.Location = new Point(this.Size.Width - 40 - vScroll.Size.Width, 0);
                }
            else
            {
                if (toolPanel.PanelAlignment == Alignment.TOP)
                {
                    toolPanel.size = new Size(this.Size.Width, 40);
                    toolPanel.Location = new Point(0, 0);
                }
                else
                {
                    toolPanel.size = new Size(this.Size.Width, 40);
                    toolPanel.Location = new Point(0, this.Size.Height - 40 - hScroll.Size.Height);
                }
            }

            toolPanel.UpdateControlsLocation();
            toolPanel.Draw(ref g);
        }

        private void AreaResized(object sender, EventArgs e)
        {
            graphics.Dispose();
            drawBuffer.Dispose();
            drawBuffer = drawContext.Allocate(CreateGraphics(), ClientRectangle);
            graphics = drawBuffer.Graphics;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
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
            float cursorPositionX = e.X;
            float cursorPositionY = e.Y;
            var r = 30 * ScaleT;
            xM = e.X;
            yM = e.Y;
            
            if (e.Button == MouseButtons.Right && isStartSimul)
            {
                if (CheckSelectedState(cursorPositionX, cursorPositionY, r, true) >= 0)
                {
                    contextMenu.Visible = true;
                    contextMenu.Show(this, e.Location);
                }
            }
            Refresh();
        }
        
        private void drawArea_MouseClick(object sender, MouseEventArgs e)
        {
            toolPanel.CheckMouseState(e, true);
            Refresh();
        }

        private int CheckSelectedState(float cursorPositionX, float cursorPositionY, float r, bool right)
        {
            if (states == null)
            {
                return (int) defines.NO_STATE_SELECTED;
            }
            
            for (var i = states.Length - 1; i >= 0; i--)
            {
                var x0 = (states[i].x + 30 + xT) * ScaleT;
                var y0 = (states[i].y + 30 + yT) * ScaleT;
                var f = (float)Math.Pow(x0 - cursorPositionX, 2) + (float)Math.Pow(y0 - cursorPositionY, 2);
                if (f <= (float)Math.Pow(r, 2))
                {
                    if (!right)
                    {
                        states[selectedState].Selected = false;
                        selectState = true;
                        selectedState = i;
                        states[i].Selected = true;
                        return i;
                    }

                    selectedState = i;
                    return i;
                }

                if (right)
                {
                    continue;
                }
                selectState = false;
                states[i].Selected = false;
            }
            return (int)defines.NO_STATE_SELECTED;
        }
        
        /// <summary>
        /// Функция проверки наведения мыши на линию
        /// и отображения информации о линии
        /// </summary>
        /// <param name="cursorPositionX">Координата x мыши</param>
        /// <param name="cursorPositionY">Координата y мыши</param>
        private void CheckSelectedLink(float cursorPositionX, float cursorPositionY)
        {
            var dx = (cursorPositionX - xT) / ScaleT;
            var dy = (cursorPositionY - yT) / ScaleT;
            selectLink = false;
            foreach (var link in links)
            {
                if (!selectLink)
                {
                    if (link.Arc)
                    {
                        continue;
                    }
                    
                    var sqrl = Math.Sqrt(Math.Pow(link.x2 - link.x1, 2.0) + Math.Pow(link.y2 - link.y1, 2.0));
                    var hlfl = Math.Sqrt(Math.Pow(dx - link.x1, 2.0) + Math.Pow(dy - link.y1, 2.0)) +
                                  Math.Sqrt(Math.Pow(dx - link.x2, 2.0) + Math.Pow(dy - link.y2, 2.0));
                    if (hlfl - sqrl < 1)
                    {
                        link.Selected = true;
                        selectLink = true;
                    }
                    else link.Selected = false;
                }
                else link.Selected = false;
            }
        }
        
        /// <summary>
        /// Обработчик движения мыши по области рисования
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawArea_MouseMove(object sender, MouseEventArgs e)
        {
            string str;
            curX = e.X;
            curY = e.Y;
            if (cursorLastPosition != e.Location && (str = toolPanel.CheckMouseState(e, false)) != null)
            {
                toolTip.Show(str, this, curX + 10, curY - 15, 500);
            }
            CheckSelectedLink(e.X, e.Y);
            if (e.Button == MouseButtons.Left && selectState)
            {
                if (xM != e.X)
                {
                    var dx = (int)(((float)e.X - xM) / ScaleT);
                    states[selectedState].x = states[selectedState].x + dx;
                    UpdateLinks();

                }
                if (yM != e.Y)
                {
                    var dy = (int)(((float)e.Y - yM) / ScaleT);
                    states[selectedState].y = states[selectedState].y + dy;
                    UpdateLinks();

                }
                xM = e.X;
                yM = e.Y;
                
            }
            cursorLastPosition = e.Location;
            Refresh();
        }
        
        /// <summary>
        /// Обновляет связи между состояниями графа
        /// </summary>
        private void UpdateLinks()
        {
            foreach (var link in links)
            {
                if (link.StartState == states[selectedState].Name)
                {
                    link.x1 = states[selectedState].x + 30;
                    link.y1 = states[selectedState].y + 30;
                    if (link.Moved)
                    {
                        link.x1 += 10;
                        link.y1 += 10;
                    }
                }

                if (link.EndState != states[selectedState].Name)
                {
                    continue;
                }
                link.x2 = states[selectedState].x + 30;
                link.y2 = states[selectedState].y + 30;
                
                if (!link.Moved)
                {
                    continue;
                }
                link.x2 += 10;
                link.y2 += 10;
            }
        }
        
        /// <summary>
        /// Создает состояния для графа
        /// </summary>
        public void CreateStates(ref string[] localStates, ref string[] inputs, ref string[] outputs)
        {
            var counter = 0;
            xs = 50;
            ys = 40;
            Array.Resize(ref states, 0);
            states = new State[inputs.Length];
            for (var j = 0; j < states.Length; j++)
            {
                states[j] = new State();
            }

            xs = 50;
            for (var i = 0; i < inputs.Length; i++)
            {
                states[i].Name = inputs[i];
                states[i].x = xs;
                states[i].y = ys;
                states[i].InputSignal = true;
                states[i].Type = STATE_TYPE.INPUT;
                ys += 62;
            }
            
            inputsLeight = inputs.Length;
            Array.Resize(ref states, states.Length + localStates.Length);
            
            for (var i = inputs.Length; i < states.Length; i++)
            {
                states[i] = new State();
            }
            
            var n = (int)Math.Truncate(Math.Sqrt(states.Length));
            xs = 120;
            ys = 62;
            
            for (var i = inputs.Length; i < states.Length; i++)
            {
                states[i].Name = localStates[i - inputs.Length];
                states[i].x = xs;
                states[i].y = ys;
                states[i].Type = STATE_TYPE.NONE;

                ys += 70;
                counter++;
                if (counter != n)
                {
                    continue;
                }
                counter = 0;
                xs += 70;
                ys = 62;
            }

            if (outputs == null || outputs.Length <= 0)
            {
                return;
            }


            Array.Resize(ref states, states.Length + outputs.Length);
            for (var i = inputs.Length + localStates.Length; i < states.Length; i++)
            {
                states[i] = new State();
            }

            xs = 112;
            ys = 10;
            for (var i = inputs.Length + localStates.Length; i < states.Length; i++)
            {
                states[i].Name = outputs[i - (inputs.Length + localStates.Length)];
                states[i].x = xs;
                xs += 62;
                states[i].y = ys;
                states[i].Type = STATE_TYPE.OUTPUT;
            }
            
        }
        
        /// <summary>
        /// Создает связи для графа
        /// </summary>
        /// <param name="rules">Массив разобранных правил</param>
        public void CreateLinks(ref Rule[] rules)
        {
            Array.Resize(ref links, 0);
            foreach (var rule in rules)
            {
                var doubleLink = true;
                for (var j = 1; j < rule.Elems.Length; j++)
                {
                    if (rule.Elems[j].Type != "State")
                    {
                        continue;
                    }

                    foreach (var link in links)
                    {
                        if (link.EndState == rule.Elems[0].Value && link.StartState == rule.Elems[j].Value)
                        {
                            doubleLink = false;
                        }
                    }

                    if (!doubleLink)
                    {
                        continue;
                    }
                    Array.Resize(ref links, links.Length + 1);
                    links[links.Length - 1] = new Link
                    {
                        EndState = rule.Elems[0].Value, StartState = rule.Elems[j].Value
                    };
                    
                    if (links[links.Length - 1].EndState == links[links.Length - 1].StartState)
                    {
                        links[links.Length - 1].Arc = true;
                    }

                    foreach (var state in states)
                    {
                        if (state.Name == rule.Elems[0].Value)
                        {
                            links[links.Length - 1].x2 = state.x + 30;
                            links[links.Length - 1].y2 = state.y + 30;
                        }
                        if (state.Name == rule.Elems[j].Value)
                        {
                            links[links.Length - 1].x1 = state.x + 30;
                            links[links.Length - 1].y1 = state.y + 30;
                        }
                        links[links.Length - 1].setTimeDot();
                        if (rule.Elems[j - 1].Type.Equals("TimeTransfer"))
                        {
                            links[links.Length - 1].timeTransfer = float.Parse(rule.Elems[j - 1].Value);
                        }
                    }
                    links[links.Length - 1].FromInput = rule.Elems[j].Local != true;
                }
            }
        }
        
        public void ToolPanelButtonClicked(object sender, ToolButtonEventArgs e)
        {
            clickToolPanelTimer.Start();
            switch (e.Name)
            {
                case "start":
                    CreateSimulation(((Form1) Parent.Parent.Parent).parser.rules,
                        ((Form1) Parent.Parent.Parent).parser.outputs);
                    break;
                case "run":
                    SimulationStart();
                    break;
                case "step":
                    SimulationStep(true);
                    break;
                case "stop":
                    SimulationStop();
                    break;
                case "table":
                    CreateTable();
                    break;
                case "reset":
                    ResetAllSignals();
                    break;
                case "showlog":
                    ShowLog();
                    break;
            }
        }
        private void ClickToolPanelTimer_Tick(object sender, EventArgs e)
        {
            OnMouseClick(null);
            clickToolPanelTimer.Stop();
            Refresh();
        }

        private void OnSimulationStarted()
        {
            var handler = SimulationStarted;
            if (handler != null)
                handler(this, new EventArgs());
        }

        private void OnSimiulationEnded()
        {
            var handler = SimulationEnded;
            if (handler != null)
                handler(this, new EventArgs());
        }
        
        /// <summary>
        /// Создает исходники программы симуляции
        /// компилирует их, запускает программу и 
        /// ожидает подключения к именованному каналу
        /// </summary>
        /// <param name="rules">Массив разобранных правил</param>
        public void CreateSimulation(Rule[] rules, string[] outputs) 
        {
            var sb = new StringBuilder();
            var providerOptions = new Dictionary<string, string>
            {
                {"CompilerVersion", "v3.5"}
            };
            var provider = new CSharpCodeProvider(providerOptions);
            const string outputAssembly = "simul.exe";
            var compilerParams = new CompilerParameters { OutputAssembly = outputAssembly, GenerateExecutable = true };
            var fi = new FileInfo(outputAssembly);
            if (!isStartSimul)
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
                var resultCode = Resources.tmpl;
                var index = resultCode.IndexOf('$');
                
                resultCode = resultCode.Remove(index, 1);
                for (var i = 0; i < states.Length; i++)
                {
                    sb.AppendLine(string.Format("{0} = {1},", states[i].Name.ToUpper(), i));
                }
                resultCode = resultCode.Insert(index, sb.ToString());
                sb.Clear();
                index = resultCode.IndexOf('$');
                resultCode = resultCode.Remove(index, 1);
                resultCode = resultCode.Insert(index, states.Length.ToString());

                index = resultCode.IndexOf('$');
                resultCode = resultCode.Remove(index, 1);
                resultCode = resultCode.Insert(index, states.Length.ToString());

                index = resultCode.IndexOf('$');
                resultCode = resultCode.Remove(index, 1);

                foreach (var rule in rules)
                {
                    if (rule.output)
                    {
                        continue;
                    }
                    
                    sb.Append("if(");
                    for (var j = 1; j < rule.Elems.Length; j++)
                    {
                        if (rule.Elems[j].Type == "=" || rule.Elems[j].Type == "t+1" || rule.Elems[j].Empty)
                        {
                            continue;
                        }
                            
                        if (rule.Elems[j].Type == "State")
                        {
                            if (rule.Elems[j].Inverted)
                            {
                                sb.Append(" !");
                            }
                            sb.AppendFormat("curState[(int)simulationDefines.{0}] ", rule.Elems[j].Value.ToUpper());
                        }
                        else
                        {
                            if (rule.Elems[j].Type == "|") sb.Append(" || ");
                            if (rule.Elems[j].Type == "&") sb.Append(" && ");
                            if (rule.Elems[j].Type == "(")
                            {
                                if (rule.Elems[j].Inverted)
                                {
                                    sb.Append(" !");
                                }
                                sb.Append("(");
                            }
                            if (rule.Elems[j].Type == ")")
                            {
                                sb.Append(")");
                            }
                        }
                    }
                    var tmp = -1;
                    var br = false;
                    foreach (var output in outputs)
                    {
                        for (var k = 0; k < rules.Length; k++ )
                        {
                            if (rules[k].Elems[0].Value == output && rule.Elems[0].Value == rules[k].Elems[2].Value)
                            {
                                tmp = k;
                                br = true;
                                break;
                            }
                            else
                            {
                                tmp = -1;
                            }
                        }
                        if (br) break;
                    }
                    sb.AppendFormat(") {{" + "newState[(int)simulationDefines.{0}] = true;", rule.Elems[0].Value.ToUpper());
                    if (tmp != -1)
                    {
                        sb.AppendFormat("\nnewState[(int)simulationDefines.{0}] = true;}}", rules[tmp].Elems[0].Value.ToUpper());
                    }
                    else sb.Append("}");
                    sb.AppendFormat("\nelse {{" + "newState[(int)simulationDefines.{0}] = false;", rule.Elems[0].Value.ToUpper());
                    if (tmp != -1)
                    {
                        sb.AppendFormat("\nnewState[(int)simulationDefines.{0}] = false;}}", rules[tmp].Elems[0].Value.ToUpper());
                    }
                    else sb.Append("}");
                    sb.AppendLine();
                }
                resultCode = resultCode.Insert(index, sb.ToString());

                compilerParams.ReferencedAssemblies.Add("System.Core.dll");
                
                var results = provider.CompileAssemblyFromSource(compilerParams, resultCode);

                if (results.Errors.Count != 0 || !fi.Exists)
                {
                    return;
                }

                var psi = new ProcessStartInfo(outputAssembly) {WindowStyle = ProcessWindowStyle.Minimized};
                Process.Start(psi);
                pipe = new NamedPipeServerStream("{E8B5BDF5-725C-4BF4-BCA4-2427875DF2E0}", PipeDirection.InOut);
                pipe.WaitForConnection();
                sw = new StreamWriter(pipe) {AutoFlush = true};
                isStartSimul = true;
                toolPanel.Buttons[0].SetImage(Resources.stop_simulation);
                toolPanel.Buttons[0].Text = "Остановить моделирование";
                toolPanel.Buttons[1].Enabled = true;
                toolPanel.Buttons[2].Enabled = true;
                toolPanel.Buttons[4].Enabled = true;
            }
            else
            {
                if (createdTable)
                {
                    table.Close();
                }
                SimulationStop();
                isStartSimul = false;
                if (pipe.IsConnected)
                {
                    WritePipe(0, 0, 'e');
                    if (pipe.IsConnected) pipe.Disconnect();
                }
                sw = null;
                pipe = null;
                toolPanel.Buttons[0].SetImage(Resources.create_simulation);
                toolPanel.Buttons[0].Text = "Запустить моделирование";
                toolPanel.Buttons[1].Enabled = false;
                toolPanel.Buttons[2].Enabled = false;
                toolPanel.Buttons[3].Enabled = false;
                toolPanel.Buttons[4].Enabled = false;
            }
        }
        /// <summary
        /// >
        /// Запись в именованный канал
        /// </summary>
        /// <param name="num">Номер сигнала</param>
        /// <param name="b">Значение сигнала</param>
        /// <param name="ch">Тип сообщения</param>
        private void WritePipe(int num, int b, char ch, int step = 0)
        {
            try
            {
                if (pipe == null || !pipe.IsConnected)
                {
                    return;
                }
             
                if (ch == 's')
                {
                    sw.WriteLine("set {0} {1}", num, b);
                }
                if (ch == 't')
                {
                    sw.WriteLine("step {0}", step);
                }
                if (ch == 'e')
                {
                    sw.WriteLine("exit");
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
            if (pipe == null || !pipe.IsConnected) return false;
            sw.WriteLine("get {0} ", num);
            pipe.WaitForPipeDrain();
            return pipe.ReadByte() > 0;
        }
        
        /// <summary>
        /// Записк автоматической симуляции
        /// </summary>
        public void SimulationStart()
        {
            OnSimulationStarted();
            if (enableLogs)
            {
                log.LogFormat = Settings.Default.LogFormat;
                log.FileName = pathToLogFile.Length > 0 ? pathToLogFile : string.Format("log_{0}", states.GetHashCode());
                log.StartLog(true, false, null);
            }

            toolPanel.Buttons[1].Enabled = false;
            toolPanel.Buttons[2].Enabled = false;
            toolPanel.Buttons[3].Enabled = true;

            if (table != null)
            {
                table.UpdateSimControls(new[] {true, false, true});
                table.ResetSteps();
                if (createdTable)
                {
                    for (var i = 0; i < inputsLeight; i++)
                    {
                        switch (table.GetElementByNumber(i))
                        {
                            case returnResults.rFALSE: states[i].Signaled = states[i].Signaled;
                                break;
                            case returnResults.rTRUE: states[i].Signaled = true;
                                break;
                            case returnResults.rUNDEF: break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            foreach (var state in states)
            {
                log.StartLog(false, false, state.Name);
            }
            log.StartLog(false, true, null);
            this.timer.Start();
        }
        
        /// <summary>
        /// Останов автоматической симуляции
        /// </summary>
        public void SimulationStop()
        {
            if (enableLogs && timer.Enabled)
                log.EndLog();
            if (table != null)
                table.UpdateSimControls(new[] { true, true, false });
            timer.Stop();
            OnSimiulationEnded();
            toolPanel.Buttons[1].Enabled = true;
            toolPanel.Buttons[2].Enabled = true;
            toolPanel.Buttons[3].Enabled = false;
        }
        
        /// <summary>
        /// Шаг симуляции
        /// </summary>
        public void SimulationStep(bool bManual = false)
        {
            if (bManual)
            {
                if (createdTable)
                {
                    for (var i = 0; i < inputsLeight; i++)
                    {
                        switch (table.GetElementByNumber(i))
                        {
                            case returnResults.rFALSE: states[i].Signaled = states[i].Signaled;
                                break;
                            case returnResults.rTRUE: states[i].Signaled = true;
                                break;
                            case returnResults.rUNDEF: break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                Refresh();
                System.Threading.Thread.Sleep(500);
            }
            
            var stepStart = true;

            for (var i = 0; i < states.Length; i++)
            {
                if (enableLogs)
                {
                    log.AddToLog(states[i].Signaled || states[i].AlSignaled, 
                        states[i].Type == STATE_TYPE.INPUT, 
                        states[i].Type == STATE_TYPE.OUTPUT, stepStart);
                    stepStart = false;
                }
                WritePipe(i, Convert.ToInt16((states[i].Signaled || states[i].AlSignaled)), 's');
            }
            WritePipe(0, 0, 't', stepNumber);
            stepNumber++;
            for (var i = 0; i < states.Length; i++)
            {
                states[i].Signaled = ReadPipe(i);
            }
            if (createdTable)
            {
                table.NextStep();
            }
            if (createdTable)
            {
                for (var i = 0; i < inputsLeight; i++)
                {
                    switch (table.GetElementByNumber(i))
                    {
                        case returnResults.rFALSE: states[i].Signaled = states[i].Signaled;
                            break;
                        case returnResults.rTRUE: states[i].Signaled = true;
                            break;
                        case returnResults.rUNDEF: break;
                        default:
                            throw new ArgumentOutOfRangeException();
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
            SimulationStep();
        }
        
        /// <summary>
        /// Обработчик клика по выпадающему меню
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clickContextMenu(object sender, ToolStripItemClickedEventArgs  e)
        {
            switch (e.ClickedItem.Text)
            {
                case "Установить 1":
                    states[selectedState].Signaled = true;
                    break;
                case "Всегда 1":
                    states[selectedState].AlSignaled = true;
                    break;
                case "Установить 0":
                    states[selectedState].Signaled = false;
                    states[selectedState].AlSignaled = false;
                    break;
            }

            Refresh();
        }
        
        /// <summary>
        /// Создает таблицу сигналов и отображает ее
        /// </summary>
        public void CreateTable()
        {
            if (inputsLeight == 0 || createdTable)
            {
                return;
            }
            
            if (table != null)
            {
                table.FormClosed -= handler;
            }
            table = null;
            table = new SignalTable(inputsLeight, this);
            table.FormClosed += handler;
            table.Font = new Font("Consols", 10, FontStyle.Italic);
            table.Icon = new Icon("../../resources/Icon.ico");
               
            for (var i = 0; i < inputsLeight; i++)
            {
                table.AddElement(i, states[i].Name, states[i].Signaled, states[i].InputSignal);
            }
            table.ShowT();
            createdTable = true;
        }
        
        /// <summary>
        /// Обработчик закрытия таблицы сигналов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableClosed(object sender, FormClosedEventArgs e)
        {
            createdTable = false;
        }
        
        public void SaveImage(String path)
        {
            var maxX = 0;
            var maxY = 0;
            foreach (var state in states)
            {
                if (state.x > maxX) maxX = state.x;
                if (state.y > maxY) maxY = state.y;
            }
            
            var imageB = new Bitmap(maxX + 70, maxY + 70);
            var graf = Graphics.FromImage(imageB);
            graf.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//Включаем сглаживание графических объектов
            graf.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;//Включаем сглаживание шрифтов
            graf.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;//Включаем интерполяцию
            var tempX = xT;
            var tempY = yT;
            var tempScale = ScaleT;
            xT = 0;
            yT = 0;
            ScaleT = 1F;
            savingImage = true;
            Refresh(graf);
            savingImage = false;
            xT = tempX;
            yT = tempY;
            ScaleT = tempScale;
            imageB.Save(path);
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//Включаем сглаживание графических объектов
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;//Включаем сглаживание шрифтов
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;//Включаем интерполяцию
            Refresh(e.Graphics);
        }
        
        private void ResetAllSignals()
        {
            foreach (var state in states)
            {
                state.Signaled = false;
                state.AlSignaled = false;
            }
        }

        private void ShowLog()
        {
            var prInf = new ProcessStartInfo {FileName = log.SavedFileName};
            if(!string.IsNullOrEmpty(log.FileName))
                Process.Start(prInf);
        }
        
        public void ClearArea()
        {
            if (isStartSimul)
            {
                CreateSimulation(null, null);
            }
            Array.Resize(ref links, 0);
            Array.Resize(ref states, 0);
        }

        private static void DrawTime(Graphics g, float time, float x, float y)
        {
            if (time != 0) g.DrawString(time.ToString(), new Font("TimesNewRoman", 20), Brushes.Black, x, y);
        }
    } 
}
