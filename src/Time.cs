using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace sku_to_smv.src
{
    public class Time : Rule
    {
        public String name;
        public int value;
        public Link[] links;
        public Point[] paintDots;
        public bool selected;

        public Time(String name)
        {
            this.name = name;
            value = 0;
            paintDots = new Point[0];
            links = new Link[0];
            selected = false;
        }

        public TimeTextBox TimeTextBox
        {
            get => default;
            set
            {
            }
        }

        public DrawArea DrawArea
        {
            get => default;
            set
            {
            }
        }

        private void calculatePainDots()
        {
            paintDots = new Point[links.Length];
            for (int i = 0; i < links.Length; i++)
            {
                Link l = links[i];
                paintDots[i] = new Point(
                    (int)(l.startDot.X + (l.endDot.X - l.startDot.X) / 2 + l.signals.Length * 20),
                    (int)(l.startDot.Y + (l.endDot.Y - l.startDot.Y) / 2));
            }
        }

        public void calculateLocation()
        {
            calculatePainDots();
        }
    }
}
