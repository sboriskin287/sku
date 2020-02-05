using sku_to_smv.src;
using System;
using System.IO;

namespace sku_to_smv
{
    // Summary:
    //     Описывает элемент списка правил автомата 
    //     создающегося при разборе его описания.
    public class Rule
    {
        public State startState;
        public State endState;
        public Signal signal;
        public Time timeMark;

        public Rule()
        {
            startState = new State();
            endState = new State();
            signal = new Signal();
            timeMark = null;
        }

        public Rule(State startState, State endState, Signal signal, Time timeMark)
        {
            this.startState = startState;
            this.endState = endState;
            this.signal = signal;
            this.timeMark = timeMark;
        }
    }
}