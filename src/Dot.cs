using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sku_to_smv.src
{
     public class Dot
    {
        public float X
        {
            get;
            set;
        }
        public float Y
        {
            get;
            set;
        }

        public Dot(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public Dot()
        {
            X = 0.0F;
            Y = 0.0F;
        }

        public override bool Equals(object obj)
        {
            return obj is Dot dot &&
                   X == dot.X &&
                   Y == dot.Y;
        }
    }
}
