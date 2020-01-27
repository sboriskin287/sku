using sku_to_smv.src;
using System;
using System.Collections.Generic;

namespace sku_to_smv
{
    public class Link
    {
        public Dot startDot;          //точка начала
        public Dot endDot;          //точка конца
        public Dot[] signalDots;//Точка отображения сигнала перехода
        public Dot timeDot;    //Точка, в которой над линией отображается время перехода
        public State startState;   //имя начального состояния
        public State endState;     //имя конечного состояния
        public string name;
        public bool Arc;            //если арка = true
        public bool FromInput;      //если от входного сигнала = true
        public bool Selected;
        public bool Moved;
        public double leight;
        public float timeTransfer; //Время перехода
        public Signal[] signals;
        public double rst;
        public Link()
        {
            startDot = new Dot();
            endDot = new Dot();
            signalDots = new Dot[0];
            timeDot = new Dot();
            startState = null;
            endState = null;
            name = null;
            Arc = false;
            FromInput = false;
            Selected = false;
            Moved = false;
            leight = 0.0;
            timeTransfer = 0;
            rst = 0.0;
            signals = new Signal[0];
        }

        public Link(State startState, State endState)
        {
            this.startState = startState;
            this.endState = endState;
            startDot = new Dot();
            endDot = new Dot();
            signalDots = new Dot[0];
            timeDot = new Dot();
            name = null;
            Arc = false;
            FromInput = false;
            Selected = false;
            Moved = false;
            leight = 0.0;
            timeTransfer = 0;
            rst = 0.0;
            signals = new Signal[0];
        }

        public void setTimeDot()
        {
            int radius = Properties.Settings.Default.StateDiametr / 2;
            timeDot = new Dot(
                startDot.x + (endDot.x - startDot.x) / 2 + signals.Length + 20 + radius,
                startDot.y + (endDot.y - startDot.y) / 2 + signals.Length + 20 + radius);
        }

        public void setTransferDot()
        {
            int radius = Properties.Settings.Default.StateDiametr / 2;
            for (int i = 0; i < signals.Length; i++)
            {
                signalDots[i] = new Dot(
                    startDot.x + (endDot.x - startDot.x) / 2 + i * 20 + radius, 
                    startDot.y + (endDot.y - startDot.y) / 2 + i * 20 + radius);              
            }
        }

        public void setName()
        {
            this.name = startState.Name + endState.Name;
        }

        public void calculateStartAndEndDots()
        {
            int radius = Properties.Settings.Default.StateDiametr / 2;
            double cos = Math.Abs(endState.x - startState.x) /
                Math.Sqrt(
                    Math.Pow(endState.x - startState.x, 2) +
                    Math.Pow(endState.y - startState.y, 2));
            double sin = Math.Sqrt(1 - Math.Pow(cos, 2));
            float x = radius * (float) cos;
            float y = radius * (float) sin;
            startDot.x = startState.x + x + radius;
            startDot.y = startState.y - y + radius;
            endDot.x = endState.x - x + radius;
            endDot.y = endState.y + y + radius;
        }
    }
}