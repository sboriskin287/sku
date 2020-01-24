using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sku_to_smv.src
{
    public class Signal
    {
        public String name
        {
            get;
            set;
        }

        public Signal()
        {
            name = null;
        }

        public Signal(string name)
        {
            this.name = name;
        }
    }
}
