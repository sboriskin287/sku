using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace sku_to_smv.src
{
	public class Signal
	{
		public String name;
		public Point[] paintDots;
		public bool Selected;
		public bool Active;
		public Link[] links;

		public Signal()
		{
			name = null;
			paintDots = new Point[0];
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
			paintDots = new Point[links.Length];
			for (int i = 0; i < links.Length; i++)
			{
				Link l = links[i];
				for (int j = 0; j < links[i].signals.Length; j++)
				{
					Signal s = l.signals[j];
                    if (s.name.Equals(name)) paintDots[i] = (l.Arc)
                            ? l.arcDot
                            : new Point(
						l.startDot.X + (l.endDot.X - l.startDot.X) / 2 + j * 20,
						l.startDot.Y + (l.endDot.Y - l.startDot.Y) / 2);
                }
            }
		}

		public void calculateLocation()
		{
			calculatePaintDots();
		}
	}
}
