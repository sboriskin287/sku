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
		public bool Active;
		public Link[] links;

		public Signal()
		{
			name = null;
			paintDots = new Dot[0];
			Selected = false;
			Active = false;
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
				Link l = links[i];
				for (int j = 0; j < links[i].signals.Length; j++)
				{
					Signal s = l.signals[j];
                    if (s.name.Equals(name)) paintDots[i] = (l.Arc)
                            ? l.arcDot
                            : new Dot(
                        l.startDot.x + (l.endDot.x - l.startDot.x) / 2 + j * 20,
                        l.startDot.y + (l.endDot.y - l.startDot.y) / 2);
                }
            }
		}

		public void calculateLocation()
		{
			calculatePaintDots();
		}
	}
}
