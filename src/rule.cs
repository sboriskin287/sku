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
        public bool SignalInventered;

        public Rule()
        {
            startState = new State();
            endState = new State();
            signal = new Signal();
            timeMark = null;
            SignalInventered = false;
        }

        public Rule(State startState, State endState, Signal signal, Time timeMark, bool InventeredSignal)
        {
            this.startState = startState;
            this.endState = endState;
            this.signal = signal;
            this.timeMark = timeMark;
            this.SignalInventered = InventeredSignal;
        }
    }
}