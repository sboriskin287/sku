using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using sku_to_smv.Properties;

namespace sku_to_smv
{
    enum MouseState { NONE, CLICK, HOVER};
    enum Alignment{TOP, BOTTOM, LEFT, RIGHT};
    class ToolPanel: Control
    {
        private int xLoc, yLoc;
        new public Point Location { set; get; }
        public Size size { set; get; }
        new public Color BackColor { set; get; }
        public Orientation PanelOrientation { set; get; }
        public Alignment PanelAlignment{ set; get; }
        public Collection<ToolButton> Buttons;

        public delegate void cToolButtonEventHandler(object sender, ToolButtonEventArgs a);
        public event cToolButtonEventHandler ButtonClicked;

        public ToolPanel()
        {
            xLoc = 5;
            yLoc = 5;
            PanelOrientation = Orientation.Vertical;
            PanelAlignment = Alignment.LEFT;
            Buttons = new Collection<ToolButton>();
            Location = new Point(0, 0);
            size = new Size(40, 40);
            BackColor = System.Drawing.SystemColors.Control;
        }
        private void OnButtonClicked(String Name)
        {
            cToolButtonEventHandler handler = ButtonClicked;
            if (handler != null)
                handler(this, new ToolButtonEventArgs(Name));
        }
        public void AddControl(ref ToolButton button)
        {
            if (PanelOrientation == Orientation.Vertical)
            {
                button.Location = new Point(this.Location.X + 5, this.Location.Y + yLoc);
                yLoc = yLoc + button.size.Height + 5;
            }
            else
            {
                button.Location = new Point(this.Location.X + xLoc, this.Location.Y + 5);
                xLoc = xLoc + button.size.Width + 5;
            }
            Buttons.Add(button);
        }
        public void UpdateControlsLocation()
        {
            xLoc = yLoc = 5;
            switch (Settings.Default.ToolsPanelLocation)
            {
                case 0: PanelOrientation = Orientation.Vertical;
                    PanelAlignment = Alignment.LEFT;
                    break;
                case 1: PanelOrientation = Orientation.Horizontal;
                    PanelAlignment = Alignment.TOP;
                    break;
                case 2: PanelOrientation = Orientation.Vertical;
                    PanelAlignment = Alignment.RIGHT;
                    break;
                case 3: PanelOrientation = Orientation.Horizontal;
                    PanelAlignment = Alignment.BOTTOM;
                    break;
            }
            for (int i = 0; i < Buttons.Count; i++ )
            {
                if (PanelOrientation == Orientation.Vertical)
                {
                    Buttons[i].Location = new Point(this.Location.X + 5, this.Location.Y + yLoc);
                    yLoc = yLoc + Buttons[i].size.Height + 5;
                }
                else
                {
                    Buttons[i].Location = new Point(this.Location.X + xLoc, this.Location.Y + 5);
                    xLoc = xLoc + Buttons[i].size.Width + 5;
                }
            }
        }
        public void Draw(ref Graphics g)
        {
            g.FillRectangle(new SolidBrush(BackColor), new Rectangle(Location, size));
            for (int i = 0; i < Buttons.Count; i++ )
            {
                Buttons[i].Draw(ref g);
            }
        }
        public String CheckMouseState(MouseEventArgs e, bool EventEnable)
        {
            int index = -1;
//             if (e.X > Location.X && e.X < Location.X + size.Width && e.Y > Location.Y && e.Y < Location.Y + size.Height)
//             {
                for (int i = 0; i < Buttons.Count; i++)
                {
                    switch (Buttons[i].CheckMouseState(e))
                    {
                        case MouseState.NONE: break;
                        case MouseState.HOVER: index = i; 
                            break;
                        case MouseState.CLICK: index = i; 
                            if (EventEnable) OnButtonClicked(Buttons[i].Name);
                            break;
                    }
                }
           /* }*/
                if (index != -1) return Buttons[index].Text;
                else return null;
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
    
    class ToolButton
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
        public Size size { set; get; }
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
            size = new Size(30, 30);
            BackColor = System.Drawing.SystemColors.Control;
            Name = "Button";
            Text = "toolButton";
            Enabled = true;
            Visible = true;
            ms = MouseState.NONE;
            BackColor = Color.FromArgb(0, 0, 0, 0);
        }

        public void Draw(ref Graphics g)
        {
            if (Visible)
            {
                g.FillRectangle(new SolidBrush(BackColor), new Rectangle(Location, size));
                if (Enabled)
                {
                    if (image != null)
                        g.DrawImage(image, new Rectangle(new Point(Location.X + 4, Location.Y + 4), new Size(22, 22)));
                    switch (ms)
                    {
                        case MouseState.NONE:
                            if (frame != null)
                                g.DrawImage(frame, new Rectangle(Location, size));
                            break;
                        case MouseState.HOVER:
                            if (frame_hover != null)
                                g.DrawImage(frame_hover, new Rectangle(Location, size));
                            break;
                        case MouseState.CLICK:
                            if (frame_click != null)
                                g.DrawImage(frame_click, new Rectangle(Location, size));
                            break;
                    }
                }
                else
                {
                    if (imageInactive != null)
                        g.DrawImage(imageInactive, new Rectangle(new Point(Location.X + 4, Location.Y + 4), new Size(22, 22)));
                    if (frame != null)
                        g.DrawImage(frame, new Rectangle(Location, size));
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
            if ((e.X > Location.X && e.X < Location.X + size.Width && e.Y > Location.Y && e.Y < Location.Y + size.Height) && Visible)
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
