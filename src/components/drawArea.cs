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
    public class DrawArea : PictureBox
    {

        //States
        public static readonly Pen stateDefaultPen = new Pen(Settings.Default.ColorDefaultState, 2);
        public static readonly Pen stateSelectedPen = new Pen(Settings.Default.ColorSelectedState, 3);
        public static readonly Pen stateActivePen = new Pen(Settings.Default.ColorActiveState, 4);
        public static readonly Pen stateActiveAndSelectedPen = new Pen(Settings.Default.ColorActiveState, 6);
        public static readonly Brush stateActiveBrush = new SolidBrush(Settings.Default.ColorFillActiveState);
        public static readonly Font defaultTextFont = Settings.Default.DefaultText;
        public static readonly Brush defaultTextBrush = new SolidBrush(Settings.Default.ColorDefaultTextBrush);

        public static readonly int StateCentre = Settings.Default.StateCentre;
        public static readonly int OffsetX = Settings.Default.OffsetStateX;
        public static readonly int OffsetY = Settings.Default.OffsetStateY;
        public static readonly int ArcCyrcleRadius = Settings.Default.ArcCyrcleRadius;
        public static readonly int ArcStartAngle = Settings.Default.ArcStartAngle;
        public static readonly int ArcSweepAngle = Settings.Default.ArcSweepAngle;
        public static readonly int StateDiametr = Settings.Default.StateDiametr;
        //Links
        public static readonly Pen linkDefaultPen = new Pen(Settings.Default.ColorDefaultLink, 2);
        public static readonly Pen linkSelectedPen = new Pen(Settings.Default.ColorSelectedLink, 4);
        //Signals
        public static readonly Font signalDefaultFont = Settings.Default.SignalDefaultFont;
        public static readonly Font signalSelectedFont = Settings.Default.SignalSelectedFont;
        public static readonly Brush signalDefaultBrush = new SolidBrush(Settings.Default.SignalDefaultColor);
        public static readonly Brush signalActiveBrush = new SolidBrush(Settings.Default.SignalActiveColor);
        //TimeMarks
        public TimeTextBox timeTb;
        private static DrawArea _instnce;


        public static DrawArea getInstance()
        {
            if (_instnce == null)
            {
                _instnce = new DrawArea();
            }
            return _instnce;
        }

        public State[] States;
        public Link[] Links;
        public Signal[] Signals;
        public Time[] TimeMarks;
        public Rule[] Rules;
        private float DragOffsetX, DragOffsetY;
        private Point PaintDotSelectedState;
        public bool SimulStarted;

        private DrawArea()
        {
            InitializeArea();
        }
        ~DrawArea()
        {
            Array.Clear(States, 0, States.Length);
            Array.Clear(Links, 0, Links.Length);
            Array.Clear(Signals, 0, Links.Length);

        }
        private void InitializeArea()
        {
            timeTb = TimeTextBox.getInstance(this);
            Controls.Add(timeTb);
            DragOffsetX = 0;
            DragOffsetY = 0;
            PaintDotSelectedState = Point.Empty;
            SimulStarted = false;
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            Links = new Link[0];
            States = new State[0];
            Signals = new Signal[0];
            TimeMarks = new Time[0];
            Rules = new Rule[0];
            this.DoubleBuffered = true;
            this.MouseClick += new MouseEventHandler(this.mouseClick);
            this.MouseDown += new MouseEventHandler(this.mouseDown);
            this.MouseMove += new MouseEventHandler(this.mouseMove);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
        }

        private void RefreshArea(Graphics g)
        {
            g.Clear(Color.White);
            drawStates(g);
            drawLinks(g);
            drawSignals(g);
            drawTimeMarks(g);
        }

        public void createStates()
        {
            int stateDefaultCentreX = StateCentre;
            int stateDefaultCentreY = StateCentre;
            int offsetStateX = OffsetX;
            int offsetStateY = OffsetY;
            foreach (Rule rule in this.Rules)
            {
                State startState = rule.startState;
                State endState = rule.endState;
                State[] startAndEndStates = { startState, endState };
                foreach (State state in startAndEndStates)
                {
                    State s = getStateByName(state.Name);
                    if (s == null)
                    {
                        state.paintDot.X = States.Length % 2 == 0 ? stateDefaultCentreX : stateDefaultCentreX + offsetStateX;
                        state.paintDot.Y = stateDefaultCentreY + States.Length / 2 * offsetStateY;
                        addState(state);
                    }
                }
            }
        }

        private void drawStates(Graphics g)
        {
            int stateDiametr = StateDiametr;
            foreach (State state in States)
            {
                state.calculateLocation();
                Pen statePen;
                if (!state.Active && !state.Selected) statePen = stateDefaultPen;
                else if (!state.Active && state.Selected) statePen = stateSelectedPen;
                else if (state.Active && !state.Selected) statePen = stateActivePen;
                else statePen = stateActiveAndSelectedPen;
                g.DrawEllipse(statePen, state.paintDot.X, state.paintDot.Y, stateDiametr, stateDiametr);
                if (state.Active) g.FillEllipse(stateActiveBrush, state.paintDot.X, state.paintDot.Y, stateDiametr, stateDiametr);
                g.DrawString(state.Name, defaultTextFont, defaultTextBrush, state.nameDot.X, state.nameDot.Y);
            }
        }

        private void canselStates()
        {
            States = new State[0];
        }

        public void createLinks()
        {
            foreach (Rule rule in Rules)
            {
                Link link = getLinkByName(rule.startState.Name + rule.endState.Name);
                if (link == null)
                {
                    Array.Resize(ref Links, Links.Length + 1);
                    link = new Link(rule.startState, rule.endState);
                    link.setName();
                    link.Arc = link.startState.Equals(link.endState);
                    link.startState.links.Add(link);
                    link.endState.links.Add(link);
                    Links[Links.Length - 1] = link;
                }
                if (rule.SignalInventered)
                {
                    Array.Resize(ref link.inventeredSignals, link.inventeredSignals.Length + 1);
                    link.inventeredSignals[link.inventeredSignals.Length - 1] = rule.signal;
                }
                if (rule.signal != null)
                {
                    Array.Resize(ref rule.signal.links, rule.signal.links.Length + 1);
                    rule.signal.links[rule.signal.links.Length - 1] = link;
                    Array.Resize(ref link.signals, link.signals.Length + 1);
                    link.signals[link.signals.Length - 1] = rule.signal;
                }
                link.timeMark = rule.timeMark;
            }
        }

        private void drawLinks(Graphics g)
        {
            int arcRadius = ArcCyrcleRadius;
            int arcStartAngle = ArcStartAngle;
            int arcSweepAngle = ArcSweepAngle;
            foreach (Link link in Links)
            {
                Pen linkPen = link.Selected ? linkSelectedPen : linkDefaultPen;
                link.calculateLocation();
                if (link.Arc)
                {
                    g.DrawArc(linkPen, new RectangleF(link.arcDot.X, link.arcDot.Y, arcRadius * 2, arcRadius * 2), arcStartAngle, arcSweepAngle);
                }
                else
                {
                    g.DrawLine(linkPen, link.startDot.X, link.startDot.Y, link.endDot.X, link.endDot.Y);
                    g.DrawLine(linkPen, link.endDot.X, link.endDot.Y, link.arrowDots[0].X, link.arrowDots[0].Y);
                    g.DrawLine(linkPen, link.endDot.X, link.endDot.Y, link.arrowDots[1].X, link.arrowDots[1].Y);
                }
            }
        }
        private void canselLinks()
        {
            Links = new Link[0];
        }

        public void createSignals()
        {
            foreach (Rule rule in Rules)
            {
                Signal s = getSignalByName(rule.signal.name);
                if (s == null)
                {
                    s = rule.signal;
                    Array.Resize(ref Signals, Signals.Length + 1);
                    Signals[Signals.Length - 1] = s;
                }
            }
        }

        public void drawSignals(Graphics g)
        {
            foreach (Signal s in Signals)
            {
                s.calculateLocation();
                foreach (Point d in s.paintDots)
                {
                    Font font = (s.Selected) ? signalSelectedFont : signalDefaultFont;
                    bool isActive = s.isInvert(d) ? !s.Active : s.Active;
                    Brush brush = (isActive) ? signalActiveBrush : signalDefaultBrush;
                    String printedName = (s.inventeredPoint.Contains(d)) ? "!" + s.name : s.name;
                    g.DrawString(printedName, font, brush, d.X, d.Y);
                }
            }
        }

        private void canselSignals()
        {
            Signals = new Signal[0];
        }

        public void createTimeMarks()
        {
            foreach (Rule r in Rules)
            {
                if (r.timeMark == null) continue;
                Time timeMark = getTimeMarkByName(r.timeMark.name);
                if (timeMark == null)
                {
                    timeMark = r.timeMark;
                    Array.Resize(ref TimeMarks, TimeMarks.Length + 1);
                    TimeMarks[TimeMarks.Length - 1] = timeMark;
                }
                Link l = getLinkByName(r.startState.Name + r.endState.Name);
                if (l == null) continue;
                Array.Resize(ref timeMark.links, timeMark.links.Length + 1);
                timeMark.links[timeMark.links.Length - 1] = l;
            }
        }

        public void drawTimeMarks(Graphics g)
        {
            foreach (Time t in TimeMarks)
            {
                t.calculateLocation();
                foreach (Point d in t.paintDots)
                {
                    Font font = (t.selected) ? signalSelectedFont : signalDefaultFont;
                    Brush brush = signalDefaultBrush;
                    g.DrawString(t.name + "<" + t.value + ">", font, brush, d.X, d.Y);
                }
            }
        }
        private void canselTimeMarks()
        {
            TimeMarks = new Time[0];
        }

        public void canselDrawElements()
        {
            canselStates();
            canselLinks();
            canselSignals();
            canselTimeMarks();
        }

        public Link getLinkByName(String name)
        {
            foreach (Link link in this.Links)
            {
                if (link.name.Equals(name)) return link;
            }
            return null;
        }

        private Signal getSignalByName(String name)
        {
            foreach (Signal signal in Signals)
            {
                if (signal.name.Equals(name)) return signal;
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

        private Time getTimeMarkByName(String name)
        {
            foreach (Time t in TimeMarks)
            {
                if (t.name.Equals(name)) return t;
            }
            return null;
        }

        public State getActiveState()
        {
            foreach (State s in States)
            {
                if (s.Active) return s;
            }
            return null;
        }

        public List<Rule> getRulesWithActiveState()
        {
            List<Rule> activeRules = new List<Rule>();
            State activeState = getActiveState();
            foreach (Rule r in Rules)
            {
                if (r.startState.Equals(activeState))
                    activeRules.Add(r);
            }
            return activeRules;
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

        private bool isDotOnLink(Point dot, Link link)
        {
            float x = dot.X;
            float y = dot.Y;
            if (link.Arc)
            {
                int arcRadius = ArcCyrcleRadius;
                Point centreArc = new Point(link.arcDot.X + arcRadius, link.arcDot.Y + arcRadius);
                //Уравнение окружности вида (x-x.centre)^2 + (y-y.centre)^2 = r^2, представляющей часть арки
                int result = (int)(Math.Pow(x - centreArc.X, 2) + Math.Pow(y - centreArc.Y, 2));
                int squareRadius = (int)Math.Pow(arcRadius, 2);
                bool dotOnPaintedArc = !(x >= centreArc.X && y >= centreArc.Y); //Переменная отвечающая, что точка лежит на отрисованной части окружности - арке
                return result >= (squareRadius - 100) && result <= (squareRadius + 100) && dotOnPaintedArc;
            }
            else
            {
                //Каноническое уравнение прямой на плоскости типа (x-x1)/(x2-x1) = (y-y1)/(y2-y1)
                float result = (link.startDot.Y - link.endDot.Y) * x + (link.endDot.X - link.startDot.X) * y + (link.startDot.X * link.endDot.Y - link.endDot.X * link.startDot.Y);
                int offset = 10;
                float maxX = Math.Max(link.startDot.X, link.endDot.X) + offset;
                float minX = Math.Min(link.startDot.X, link.endDot.X) - offset;
                float maxY = Math.Max(link.startDot.Y, link.endDot.Y) + offset;
                float minY = Math.Min(link.startDot.Y, link.endDot.Y) - offset;
                return (result >= -2500
                    && result <= 2500
                    && x >= minX && x <= maxX
                    && y >= minY && y <= maxY);
            }
        }

        public bool isDotOnState(Point dot, State state)
        {
            int diametr = StateDiametr;
            return dot.X >= state.paintDot.X
                && dot.X <= state.paintDot.X + diametr
                && dot.Y >= state.paintDot.Y
                && dot.Y <= state.paintDot.Y + diametr;
        }

        public bool isDotOnSignal(Point dot, Signal signal)
        {
            float offset = 20;
            bool isOnSignal = false;
            foreach (Point pDot in signal.paintDots)
            {
                isOnSignal = isOnSignal || (dot.X >= pDot.X
                     && dot.X <= pDot.X + offset
                     && dot.Y >= pDot.Y
                     && dot.Y <= pDot.Y + offset);
            }
            return isOnSignal;
        }

        private bool isDotOnTimeMark(Point dot, Time tm)
        {
            int offset = 10 * tm.name.Length;
            bool isOnMark = false;
            foreach (Point pd in tm.paintDots)
            {
                isOnMark = isOnMark
                    || dot.X >= pd.X
                    && dot.X <= pd.X + offset
                    && dot.Y >= pd.Y
                    && dot.Y <= pd.Y + offset;
            }
            return isOnMark;
        }

        private bool setSelectedLink(Point dot)
        {
            bool isChanged = false;
            foreach (Link link in Links)
            {
                bool isLink = isDotOnLink(dot, link);
                isChanged = isChanged || link.Selected ^ isLink;
                link.Selected = isLink;
            }
            return isChanged;
        }

        private bool setSelectedState(Point dot)
        {
            bool isChanged = false;
            foreach (State state in States)
            {
                bool isState = isDotOnState(dot, state);
                isChanged = isChanged || state.Selected ^ isState;
                state.Selected = isState;
            }
            return isChanged;
        }

        private bool setActiveState()
        {
            bool isChanged = false;
            foreach (State s in States)
            {
                bool isNeedChangeActivity = s.Selected && PaintDotSelectedState.Equals(s.paintDot);
                isChanged = isChanged || isNeedChangeActivity;
                if (isNeedChangeActivity)
                {
                    s.Active = !s.Active;
                    canselStates(s);
                }
            }
            return isChanged;
        }

        private void canselStates(State state)
        {
            foreach (State s in States)
            {
                s.Active = s.Equals(state) && s.Active;
            }
        }

        private bool setSelectedTimeMarks(Point dot)
        {
            bool isChanged = false;
            foreach (Time tm in TimeMarks)
            {
                bool isOnMark = isDotOnTimeMark(dot, tm);
                isChanged = isChanged || tm.selected ^ isOnMark;
                tm.selected = isOnMark;
            }
            return isChanged;
        }

        private bool setActiveSignal()
        {
            bool isChanged = false;
            foreach (Signal s in Signals)
            {
                bool newActivityStatus = s.Selected && !s.Active || !s.Selected && s.Active;
                isChanged = isChanged || s.Active ^ newActivityStatus;
                s.Active = newActivityStatus;
            }
            return isChanged;
        }

        private bool setSelectedSignal(Point dot)
        {
            bool isSignalsChanged = false;
            foreach (Signal signal in Signals)
            {
                bool isOnSignal = isDotOnSignal(dot, signal);
                isSignalsChanged = isSignalsChanged || signal.Selected ^ isOnSignal;
                signal.Selected = isOnSignal;
            }
            return isSignalsChanged;
        }

        private bool setValueTimeMark(Point dot)
        {
            bool tbVisible = false;
            Point tbLocation = Point.Empty;
            Time selectedTm = null;
            foreach (Time tm in TimeMarks)
            {
                tbVisible = tbVisible || tm.selected;
                if (tm.selected)
                {
                    tbLocation = dot;
                    selectedTm = tm;
                }
            }
            timeTb.Visible = tbVisible;
            timeTb.Location = tbLocation;
            timeTb.timeMark = selectedTm;
            return true;
        }

        private bool isElementsChanged(Point dot)
        {
            bool isLinkChanged = setSelectedLink(dot);
            bool isStateChanged = setSelectedState(dot);
            bool isSignalChanged = setSelectedSignal(dot);
            bool isTimeMarkChanged = setSelectedTimeMarks(dot);
            return isLinkChanged
                || isStateChanged
                || isSignalChanged
                || isTimeMarkChanged;
        }

        private void relocationStates(Point dot)
        {
            foreach (State state in States)
            {
                if (state.Selected)
                {
                    state.paintDot.X = dot.X - (int)DragOffsetX;
                    state.paintDot.Y = dot.Y - (int)DragOffsetY;
                    foreach (Link l in state.links)
                    {
                        l.calculateLocation();
                        if (l.lengthLink < 0 && !l.Arc)
                        {
                            int direction = state.Equals(l.startState) ? -1 : 1;
                            state.paintDot.X += (int)(l.cosx * Math.Abs(l.lengthLink) * direction);
                            state.paintDot.Y += (int)(l.sinx * Math.Abs(l.lengthLink) * direction);
                        }
                    }
                }
            }
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            Point dot = e.Location;
            if (e.Button.Equals(MouseButtons.Left))
            {
                relocationStates(dot);
            }
            if (isElementsChanged(dot) || e.Button.Equals(MouseButtons.Left))
            {
                Refresh();
            }
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            foreach (State state in States)
            {
                if (state.Selected)
                {
                    DragOffsetX = e.X - state.paintDot.X;
                    DragOffsetY = e.Y - state.paintDot.Y;
                    PaintDotSelectedState = new Point(state.paintDot.X, state.paintDot.Y);
                }
            }
        }

        private void mouseClick(object sender, MouseEventArgs e)
        {
            DragOffsetX = 0;
            DragOffsetY = 0;
            bool isStateChangeActivity = !SimulStarted ? setActiveState() : false;
            bool isSignalChangeActivity = setActiveSignal();
            bool s = setValueTimeMark(e.Location);
            if (isStateChangeActivity || isSignalChangeActivity || s) Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//Включаем сглаживание графических объектов
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;//Включаем сглаживание шрифтов
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;//Включаем интерполяцию
            RefreshArea(e.Graphics);
        }


        public void re()
        {
            Refresh();
        }

        public Parser Parser
        {
            get => default;
            set
            {
            }
        }

        public Form1 Form1
        {
            get => default;
            set
            {
            }
        }
    } 
}
