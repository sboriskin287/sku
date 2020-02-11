using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace sku_to_smv.src
{
    public class Time
    {
        public String name;
        public float value;
        public Link[] links;
        public Dot[] paintDots;       
        public Dot textBoxDot;
        public bool selected;
      
        public Time(String name)
        { 
            this.name = name;
            value = 0;
            paintDots = new Dot[0];
            links = new Link[0];          
            textBoxDot = null;
            selected = false;
        }

        private void calculatePainDots()
        {
            paintDots = new Dot[links.Length];
            for (int i = 0; i < links.Length; i++)
            {
                Link l = links[i];
                paintDots[i] = new Dot(
                    l.startDot.x + (l.endDot.x - l.startDot.x) / 2 + l.signals.Length * 20,
                    l.startDot.y + (l.endDot.y - l.startDot.y) / 2);
            }
        }

        public void calculateLocation()
        {
            calculatePainDots();
        }
    }
}
