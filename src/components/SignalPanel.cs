using sku_to_smv.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace sku_to_smv.src.components
{
    class SignalPanel : Control
    {
        private static SignalPanel _instance;

        public static SignalPanel getInstance()
        {
            if (_instance == null)
            {
                _instance = new SignalPanel();
            }
            return _instance;
        }

        public static void updateInstance()
        {   
           _instance = new SignalPanel();
        }

        DrawArea da = DrawArea.getInstance();
        public Orientation PanelOrientation;
        public Alignment PanelAlignment;
        public static readonly Font signalDefaultFont = Settings.Default.SignalDefaultFont;
        public static readonly Font signalSelectedFont = Settings.Default.SignalSelectedFont;

        private SignalPanel()
        {
            Location = new Point(da.Size.Width - 41, 0);
            Size = new Size(41, 500);
            BackColor = Color.Pink;
            PanelOrientation = Orientation.Vertical;
            PanelAlignment = Alignment.RIGHT;
            updateButtons();
        }

        private void updateButtons()
        {
            for (int i = 0; i < da.Signals.Length; i++)
            {
                Signal s = da.Signals[i];
                Button b = new Button();
                b.Name = s.name;
                b.Text = s.name;
                b.Click += new EventHandler(onButtonAction);
                b.Size = new Size(35, 35);
                b.BackColor = Color.FromArgb(0, 0, 0, 0);
                b.Enabled = true;
                b.Visible = true;
                CalculateLocation(ref b, i);
                Controls.Add(b);
            }
        }

        private void onButtonAction(object sender, EventArgs e)
        {
            Button b = (Button) sender;
            Signal s = da.getSignalByName(b.Name);
            s.Active = !s.Active;
            b.ForeColor = s.Active ? Color.Red : Color.Black;
            b.Refresh();
            da.Refresh();
        }

        public void CalculateLocation(ref Button button, int count)
        {
            int x = 3;
            int y = 3;
            if (PanelOrientation.Equals(Orientation.Vertical))
            {
                y += (button.Size.Height + 5) * count;
            }
            else
            {
                x += (button.Size.Width + 5) * count;
            }
            button.Location = new Point(x, y);
        }

        public override void Refresh()
        {
            Location = new Point(da.Size.Width - 41, 0);
            base.Refresh();
        }
    }
}
