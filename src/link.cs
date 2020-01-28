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
        public double cosx;
        public double sinx;
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
            cosx = 1;
            sinx = 0;
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
            cosx = 1;
        }

        private void setTimeDot()
        {
            int radius = Properties.Settings.Default.StateDiametr / 2;
            timeDot = new Dot(
                startDot.x + (endDot.x - startDot.x) / 2 + signals.Length + 20,
                startDot.y + (endDot.y - startDot.y) / 2 + signals.Length + 20);
        }

        private void setTransferDots()
        {
            int radius = Properties.Settings.Default.StateDiametr / 2;
            for (int i = 0; i < signals.Length; i++)
            {
                signalDots[i] = new Dot(
                    startDot.x + (endDot.x - startDot.x) / 2 + i * 20, 
                    startDot.y + (endDot.y - startDot.y) / 2);              
            }
        }

        public void setName()
        {
            this.name = startState.Name + endState.Name;
        }

        private void setCosX()
        {
            cosx = startState.paintDot.y == endState.paintDot.y
                ? (endState.paintDot.x - startState.paintDot.x) / Math.Abs(endState.paintDot.x - startState.paintDot.x)
                : (endState.paintDot.x - startState.paintDot.x) /
                Math.Sqrt(
                    Math.Pow(endState.paintDot.x - startState.paintDot.x, 2) +
                    Math.Pow(startState.paintDot.y - endState.paintDot.y, 2));
        }

        private void setSinX()
        {
            sinx = startState.paintDot.x == endState.paintDot.x
                ? (startState.paintDot.y - endState.paintDot.y) / Math.Abs(startState.paintDot.y - endState.paintDot.y)
                : (startState.paintDot.y - endState.paintDot.y) /
                Math.Sqrt(
                    Math.Pow(endState.paintDot.x - startState.paintDot.x, 2) +
                    Math.Pow(startState.paintDot.y - endState.paintDot.y, 2));
        }

        private void calculateStartAndEndDots()
        {            

            int radius = Properties.Settings.Default.StateDiametr / 2;
            float x = radius * (float) cosx;
            float y = radius * (float) sinx;
            startDot.x = startState.paintDot.x + radius + x;
            startDot.y = startState.paintDot.y + radius - y;
            endDot.x = endState.paintDot.x + radius - x;
            endDot.y = endState.paintDot.y + radius + y;
        }

        public void initializeLocation()
        {
            signalDots = new Dot[signals.Length];
            setCosX();
            setSinX();
            calculateStartAndEndDots();
            setTransferDots();
            setTimeDot();
        }
    }
}