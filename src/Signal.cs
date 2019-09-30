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
        public List<KeyValuePair<State, State>> states
        {
            get;
            set;
        }
      

        public Signal(String name, List<KeyValuePair<State, State>> states)
        {
            this.name = name;
            this.states = states;
        }

        public Signal(string name)
        {
            this.name = name;
            states = new List<KeyValuePair<State, State>>();
        }

        public Signal()
        {
        }

        public void addPair(State begin, State end)
        {
            states.Add(new KeyValuePair<State, State>(begin, end));
        }
    }
}
