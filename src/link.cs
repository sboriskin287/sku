using sku_to_smv.src;
using System;
using System.Collections.Generic;

namespace sku_to_smv
{
    public class Link
    {
        public Dot startDot;          //точка начала
        public Dot endDot;          //точка конца        
        public Dot timeDot;    //Точка, в которой над линией отображается время перехода
        public State startState;   //имя начального состояния
        public State endState;     //имя конечного состояния
        public int lengthLink;
        public int lengthBetweenStatesCentre;
        public string name;
        public bool Arc;            //если арка = true
        public bool Selected;
        public float timeTransfer; //Время перехода
        public Signal[] signals;
        public double cosx;
        public double sinx;
        public Link()
        {
            startDot = new Dot();
            endDot = new Dot();
            timeDot = new Dot();
            startState = null;
            endState = null;
            lengthLink = 0;
            lengthBetweenStatesCentre = 0;
            name = null;
            Arc = false;
            Selected = false;
            timeTransfer = 0;
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
            timeDot = new Dot();
            name = null;
            Arc = false;
            Selected = false;
            timeTransfer = 0;
            signals = new Signal[0];
            cosx = 1;
            sinx = 0;
        }

        private void setTimeDot()
        {
            timeDot = new Dot(
                startDot.x + (endDot.x - startDot.x) / 2 + signals.Length + 40,
                startDot.y + (endDot.y - startDot.y) / 2 + signals.Length);
        }

        public void setName()
        {
            this.name = startState.Name + endState.Name;
        }

        private void setCosX()
        {
            cosx = startState.paintDot.y == endState.paintDot.y
                ? (endState.paintDot.x - startState.paintDot.x) / Math.Abs(endState.paintDot.x - startState.paintDot.x)
                : (endState.paintDot.x - startState.paintDot.x) / lengthBetweenStatesCentre;
        }

        private void setSinX()
        {
            sinx = startState.paintDot.x == endState.paintDot.x
                ? (endState.paintDot.y - startState.paintDot.y) / Math.Abs(startState.paintDot.y - endState.paintDot.y)
                : (endState.paintDot.y - startState.paintDot.y) / lengthBetweenStatesCentre;
        }

        private void calculateLength()
        {
            int radius = Properties.Settings.Default.StateDiametr / 2;
            lengthBetweenStatesCentre = (int)Math.Sqrt(
                    Math.Pow(endState.paintDot.x - startState.paintDot.x, 2) +
                    Math.Pow(startState.paintDot.y - endState.paintDot.y, 2));
            lengthLink = -radius * 2 + lengthBetweenStatesCentre;
        }

        private void calculateStartAndEndDots()
        {

            int radius = Properties.Settings.Default.StateDiametr / 2;
            float x = radius * (float)cosx;
            float y = radius * (float)sinx;
            startDot.x = startState.paintDot.x + radius + x;
            startDot.y = startState.paintDot.y + radius + y;
            endDot.x = endState.paintDot.x + radius - x;
            endDot.y = endState.paintDot.y + radius - y;
        }

        public void calculateLocation()
        {
            calculateLength();
            setCosX();
            setSinX();
            calculateStartAndEndDots();
            setTimeDot();
        }

        public override bool Equals(object obj)
        {
            return obj is Link link &&
                   name.Equals(link.name);
        }
    }
}