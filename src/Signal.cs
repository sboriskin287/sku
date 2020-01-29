using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sku_to_smv.src
{
    public class Signal
    {
        public String name;
        public Dot[] paintDots;
        public bool Selected;
        public Link[] links;

        public Signal()
        {
            name = null;
            paintDots = new Dot[0];
            Selected = false;
            links = new Link[0];
        }

        public Signal(string name)
        {
            this.name = name;
        }

        private void calculatePaintDots()
        {
            paintDots = new Dot[links.Length];
            for (int i = 0; i < links.Length; i++)
            {
                for (int j = 0; j < links[i].signals.Length; j++)
                {
                    if (links[i].signals[j].name.Equals(name)) paintDots[i] = links[i].signalDots[j];
                }               
            }
        }

        public void calculateLocation()
        {
            calculatePaintDots();
        }
    }
}
