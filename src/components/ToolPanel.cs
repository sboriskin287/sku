﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using sku_to_smv.Properties;

namespace sku_to_smv
{
    enum MouseState { NONE, CLICK, HOVER };
    enum Alignment { TOP, BOTTOM, LEFT, RIGHT };
    class ToolPanel : Control
    {
        public Orientation PanelOrientation;
        public Alignment PanelAlignment;
        public Collection<Button> Buttons;
        //public DrawArea area;
        public static readonly Brush ToolPanelBrush = new SolidBrush(SystemColors.Control);
        private static ToolPanel _instance;

        public static ToolPanel getInstance()
        {
            if (_instance == null)
            {
                _instance = new ToolPanel();
            }
            return _instance;
        }

        public delegate void cToolButtonEventHandler(object sender, ToolButtonEventArgs a);
        public event cToolButtonEventHandler ButtonClicked;

        private ToolPanel()
        {
            Buttons = new Collection<Button>();
            Location = new Point(50, 0);
            Size = new Size(40, 500);

            Button startButtnon = new Button();
            //startButtnon.InactiveImage = Resources.create_simulation;
            startButtnon.Image = Resources.create_simulation;
            startButtnon.Name = "start";
            startButtnon.Text = "Запустить симуляцию";
            startButtnon.Location = Point.Empty;
            //CalculateLocation(ref startButtnon);
            Buttons.Add(startButtnon);

            Button runButton = new Button();
            //runButton.SetInactiveImage(Resources.play_grey);
            runButton.Image = Resources.play;
            runButton.Name = "run";
            runButton.Text = "Запуск симуляции";
            CalculateLocation(ref runButton);
            Buttons.Add(runButton);

            Button stepButton = new Button();
            //stepButton.SetInactiveImage(Resources.step_grey);
            stepButton.Image = Resources.step;
            stepButton.Name = "step";
            stepButton.Text = "Шаг с остановом";
            CalculateLocation(ref stepButton);
            Buttons.Add(stepButton);

            Button stopSimulation = new Button();
            //stopSimulation.SetInactiveImage(Resources.stop_grey);
            stopSimulation.Image = Resources.stop;
            stopSimulation.Name = "stop";
            stopSimulation.Text = "Остановить симуляцию";
            CalculateLocation(ref stopSimulation);
            Buttons.Add(stopSimulation);

            Button signalsTable = new Button();
            //signalsTable.SetInactiveImage(Resources.table_grey);
            signalsTable.Image = Resources.table;
            signalsTable.Name = "table";
            signalsTable.Text = "Таблица сигналов";
            CalculateLocation(ref signalsTable);
            Buttons.Add(signalsTable);

            Button canselSignals = new Button();
            //canselSignals.SetInactiveImage(Resources.table_grey);
            canselSignals.Image = Resources.reset_all;
            canselSignals.Name = "reset";
            canselSignals.Text = "Сбросить все сигналы";
            CalculateLocation(ref canselSignals);
            Buttons.Add(canselSignals);

            Button showLog = new Button();
            //showLog.SetInactiveImage(Resources.table_grey);
            showLog.Image = Resources.show_log;
            showLog.Name = "showlog";
            showLog.Text = "Показать лог-файл";
            CalculateLocation(ref showLog);
            Buttons.Add(showLog);

            Controls.AddRange(Buttons.ToArray());
        }
        public void CalculateLocation(ref Button button)
        {
            int x = Location.X + 3;
            int y = Location.Y + 3;
            if (PanelOrientation.Equals(Orientation.Vertical))
            {
                y += (button.Size.Height + 5) * Buttons.Count;
            } 
            else
            {
                x += (button.Size.Width + 5) * Buttons.Count;
            }
            button.Location = new Point(x, y);
        }
        /*public void UpdateControlsLocation()
        {
            switch (Settings.Default.ToolsPanelLocation)
            {
                case 0:
                    PanelOrientation = Orientation.Vertical;
                    PanelAlignment = Alignment.LEFT;
                    break;
                case 1:
                    PanelOrientation = Orientation.Horizontal;
                    PanelAlignment = Alignment.TOP;
                    break;
                case 2:
                    PanelOrientation = Orientation.Vertical;
                    PanelAlignment = Alignment.RIGHT;
                    break;
                case 3:
                    PanelOrientation = Orientation.Horizontal;
                    PanelAlignment = Alignment.BOTTOM;
                    break;
            }
        }*/       

        private void DrawButtons(Graphics g)
        {
            foreach (ToolButton b in Buttons)
            {
                b.Draw(ref g);
            }
        }
    }

    public class ToolButtonEventArgs : EventArgs
    {
        public ToolButtonEventArgs(string s)
        {
            ButtonName = s;
        }
        private string ButtonName;
        public string Name
        {
            get { return ButtonName; }
        }
    }

    class ToolButton : Button
    {
        Bitmap image;
        Bitmap imageInactive;
        Bitmap frame;
        Bitmap frame_hover;
        Bitmap frame_click;
        MouseState ms;
        public bool Enabled { set; get; }
        public bool Visible { set; get; }
        public Point Location { set; get; }
        public Size Size { set; get; }
        public Color BackColor { set; get; }
        public String Name { set; get; }
        public String Text { set; get; }

        public ToolButton()
        {
            image = null;
            imageInactive = null;
            frame = null;
            frame_hover = null;
            frame_click = null;
            Location = new Point(0, 0);
            Size = new Size(30, 30);
            BackColor = System.Drawing.SystemColors.Control;
            Name = "Button";
            Text = "toolButton";
            Enabled = true;
            Visible = true;
            ms = MouseState.NONE;
            BackColor = Color.FromArgb(0, 0, 0, 0);
            SetFrame(Resources.frame);
            SetFrameHover(Resources.frame_hover);
            SetFrameClick(Resources.frame_click);
        }

        public void Draw(ref Graphics g)
        {
            if (Visible)
            {
                g.FillRectangle(new SolidBrush(BackColor), new Rectangle(Location, Size));
                if (Enabled)
                {
                    if (image != null)
                        g.DrawImage(image, new Rectangle(new Point(Location.X + 4, Location.Y + 4), new Size(22, 22)));
                    switch (ms)
                    {
                        case MouseState.NONE:
                            if (frame != null)
                                g.DrawImage(frame, new Rectangle(Location, Size));
                            break;
                        case MouseState.HOVER:
                            if (frame_hover != null)
                                g.DrawImage(frame_hover, new Rectangle(Location, Size));
                            break;
                        case MouseState.CLICK:
                            if (frame_click != null)
                                g.DrawImage(frame_click, new Rectangle(Location, Size));
                            break;
                    }
                }
                else
                {
                    if (imageInactive != null)
                        g.DrawImage(imageInactive, new Rectangle(new Point(Location.X + 4, Location.Y + 4), new Size(22, 22)));
                    if (frame != null)
                        g.DrawImage(frame, new Rectangle(Location, Size));
                }
            }
        }
        public void SetImage(String path)
        {
            try
            {
                image = new Bitmap(path);
                image.MakeTransparent(System.Drawing.Color.Magenta);
            }
            catch (System.IO.FileNotFoundException)
            {
                image = null;
            }
        }
        public void SetImage(Bitmap im)
        {
            image = im;
            image.MakeTransparent(System.Drawing.Color.Magenta);
        }
        public void SetInactiveImage(String path)
        {
            try
            {
                imageInactive = new Bitmap(path);
                imageInactive.MakeTransparent(System.Drawing.Color.Magenta);
            }
            catch (System.IO.FileNotFoundException)
            {
                imageInactive = null;
            }
        }
        public void SetInactiveImage(Bitmap im)
        {
            imageInactive = im;
            imageInactive.MakeTransparent(System.Drawing.Color.Magenta);
        }
        public void SetFrame(String path)
        {
            try
            {
                frame = new Bitmap(path);
                frame.MakeTransparent(System.Drawing.Color.Magenta);
            }
            catch (System.IO.FileNotFoundException)
            {
                frame = null;
            }
        }
        public void SetFrame(Bitmap im)
        {
            frame = im;
            frame.MakeTransparent(System.Drawing.Color.Magenta);
        }
        public void SetFrameHover(String path)
        {
            try
            {
                frame_hover = new Bitmap(path);
                frame_hover.MakeTransparent(System.Drawing.Color.Magenta);
            }
            catch (System.IO.FileNotFoundException)
            {
                frame_hover = null;
            }
        }
        public void SetFrameHover(Bitmap im)
        {
            frame_hover = im;
            frame_hover.MakeTransparent(System.Drawing.Color.Magenta);
        }
        public void SetFrameClick(String path)
        {
            try
            {
                frame_click = new Bitmap(path);
                frame_click.MakeTransparent(System.Drawing.Color.Magenta);
            }
            catch (System.IO.FileNotFoundException)
            {
                frame_click = null;
            }
        }
        public void SetFrameClick(Bitmap im)
        {
            frame_click = im;
            frame_click.MakeTransparent(System.Drawing.Color.Magenta);
        }
        public MouseState CheckMouseState(MouseEventArgs e)
        {
            if (e != null)
                if ((e.X > Location.X && e.X < Location.X + Size.Width && e.Y > Location.Y && e.Y < Location.Y + Size.Height) && Visible)
                {
                    if (e.Button == MouseButtons.Left && Enabled)
                    {
                        return ms = MouseState.CLICK;
                    }
                    return ms = MouseState.HOVER;
                }
            return ms = MouseState.NONE;
        }
    }
}
