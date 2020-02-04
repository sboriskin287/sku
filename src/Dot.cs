using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sku_to_smv.src
{
    public class Dot
    {
        public float x
        {
            get;
            set;
        }
        public float y
        {
            get;
            set;
        }

        public Dot(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Dot()
        {
            x = 0.0F;
            y = 0.0F;
        }

        public override bool Equals(object obj)
        {
            return obj is Dot dot &&
                   x == dot.x &&
                   y == dot.y;
        }
    }
}
