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
        public State startState
        {
            get;
            set;
        }
      
        public State endState
        {
            get;
            set;
        }

        public Signal signal
        {
            get;
            set;
        }

        public double timeTransfer
        {
            get;
            set;
        }

        public Rule()
        {
            startState = new State();
            endState = new State();
            signal = new Signal();
            timeTransfer = 0;
        }

        public Rule(State startState, State endState, Signal signal, double timeTransfer)
        {
            this.startState = startState;
            this.endState = endState;
            this.signal = signal;
            this.timeTransfer = timeTransfer;
        }
    }
}