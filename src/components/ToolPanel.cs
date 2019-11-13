using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using SCUConverterDrawArea.Properties;

namespace SCUConverterDrawArea
{
    enum MouseState { NONE, CLICK, HOVER};
    enum Alignment{TOP, BOTTOM, LEFT, RIGHT};
    class ToolPanel : Control
    {
        private int xLoc, yLoc;
        public new Point Location { set; get; }
        public Size size { set; get; }
        public new Color BackColor { set; get; }
        public Orientation PanelOrientation { set; get; }
        public Alignment PanelAlignment{ set; get; }
        public readonly Collection<ToolButton> Buttons;

        public delegate void CToolButtonEventHandler(object sender, ToolButtonEventArgs a);
        public event CToolButtonEventHandler ButtonClicked;

        public ToolPanel()
        {
            xLoc = 5;
            yLoc = 5;
            PanelOrientation = Orientation.Vertical;
            PanelAlignment = Alignment.LEFT;
            Buttons = new Collection<ToolButton>();
            Location = new Point(0, 0);
            size = new Size(40, 40);
            BackColor = SystemColors.Control;
        }
        
        private void OnButtonClicked(string name)
        {
            var handler = ButtonClicked;
            if (handler != null)
                handler(this, new ToolButtonEventArgs(name));
        }
        
        public void AddControl(ref ToolButton button)
        {
            if (PanelOrientation == Orientation.Vertical)
            {
                button.Location = new Point(Location.X + 5, Location.Y + yLoc);
                yLoc = yLoc + button.size.Height + 5;
            }
            else
            {
                button.Location = new Point(Location.X + xLoc, Location.Y + 5);
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
            
            foreach (var button in Buttons)
            {
                if (PanelOrientation == Orientation.Vertical)
                {
                    button.Location = new Point(Location.X + 5, Location.Y + yLoc);
                    yLoc = yLoc + button.size.Height + 5;
                }
                else
                {
                    button.Location = new Point(Location.X + xLoc, Location.Y + 5);
                    xLoc = xLoc + button.size.Width + 5;
                }
            }
        }
        
        public void Draw(ref Graphics g)
        {
            g.FillRectangle(new SolidBrush(BackColor), new Rectangle(Location, size));
            foreach (var button in Buttons)
            {
                button.Draw(ref g);
            }
        }

        public string CheckMouseState(MouseEventArgs e, bool eventEnable)
        {
            var index = -1;
            for (var i = 0; i < Buttons.Count; i++)
            {
                switch (Buttons[i].CheckMouseState(e))
                {
                    case MouseState.NONE: break;
                    case MouseState.HOVER:
                        index = i;
                        break;
                    case MouseState.CLICK:
                        index = i;
                        if (eventEnable) OnButtonClicked(Buttons[i].Name);
                        break;
                }
            }

            return index != -1 ? Buttons[index].Text : null;
        }
    }

    public class ToolButtonEventArgs : EventArgs
    {
        public ToolButtonEventArgs(string s)
        {
            ButtonName = s;
        }
        private readonly string ButtonName;
        public string Name
        {
            get { return ButtonName; }
        } 
    }

    internal class ToolButton
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
            BackColor = SystemColors.Control;
            Name = "Button";
            Text = "toolButton";
            Enabled = true;
            Visible = true;
            ms = MouseState.NONE;
            BackColor = Color.FromArgb(0, 0, 0, 0);
        }

        public void Draw(ref Graphics g)
        {
            if (!Visible)
            {
                return;
            }
            
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

        public void SetImage(Bitmap im)
        {
            image = im;
            image.MakeTransparent(Color.Magenta);
        }

        public void SetInactiveImage(Bitmap im)
        {
            imageInactive = im;
            imageInactive.MakeTransparent(Color.Magenta);
        }

        public void SetFrame(Bitmap im)
        {
            frame = im;
            frame.MakeTransparent(Color.Magenta);
        }

        public void SetFrameHover(Bitmap im)
        {
            frame_hover = im;
            frame_hover.MakeTransparent(Color.Magenta);
        }

        public void SetFrameClick(Bitmap im)
        {
            frame_click = im;
            frame_click.MakeTransparent(Color.Magenta);
        }
        
        public MouseState CheckMouseState(MouseEventArgs e)
        {
            if (e == null)
            {
                return ms = MouseState.NONE;
            }

            if (e.X <= Location.X || e.X >= Location.X + size.Width || e.Y <= Location.Y ||
                e.Y >= Location.Y + size.Height || !Visible)
            {
                return ms = MouseState.NONE;
            }
            
            if (e.Button == MouseButtons.Left && Enabled)
            {
                return ms = MouseState.CLICK;
            }
            
            return ms = MouseState.HOVER;
        }
    }
}
