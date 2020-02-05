using sku_to_smv.src;
using System;
using System.Collections.Generic;

namespace sku_to_smv
{
    public class Link
    {
        public Dot startDot;          //точка начала
        public Dot endDot;          //точка конца   
        public Dot arcDot;      //Точка отрисовки арки, если это арка
        public Dot timeDot;    //Точка, в которой над линией отображается время перехода
        public Dot[] arrowDots = { new Dot(), new Dot() };
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
            arcDot = new Dot();
            timeDot = new Dot();
            //arrowDots = { new Dot(), new Dot() };
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

        private void caluclateArrowDots()
        {
            if (Arc) return;
            int l = 20;
            double angle = sinx != 0
                ? Math.Asin(Math.Abs(sinx))
                : Math.Acos(Math.Abs(cosx));
            if (cosx < 0 && sinx > 0) angle = Math.PI - angle;
            else if (cosx < 0 && sinx < 0) angle = Math.PI + angle;
            else if (cosx > 0 && sinx < 0) angle = Math.PI * 2 - angle;
            double angle1 = Math.PI / 3 - angle;
            double angle2 = angle - Math.PI / 6;
            double cos1 = Math.Cos(angle1);
            double sin1 = Math.Sin(angle1);
            double cos2 = Math.Cos(angle2);
            double sin2 = Math.Sin(angle2);
            arrowDots[0] = new Dot(endDot.x - (float)(sin1 * l), endDot.y - (float)(cos1 * l));
            arrowDots[1] = new Dot(endDot.x - (float)(cos2 * l), endDot.y - (float)(sin2 * l));
        }

        private void calculateStartAndEndDots()
        {         
            if (Arc)
            {
                int arcOffset = 18;
                arcDot = new Dot(
                    startState.paintDot.x - arcOffset,
                    startState.paintDot.y - arcOffset);
            } else
            {
                int radius = Properties.Settings.Default.StateDiametr / 2;
                float x = radius * (float)cosx;
                float y = radius * (float)sinx;
                startDot.x = startState.paintDot.x + radius + x;
                startDot.y = startState.paintDot.y + radius + y;
                endDot.x = endState.paintDot.x + radius - x;
                endDot.y = endState.paintDot.y + radius - y;
            }       
        }

        public void calculateLocation()
        {
            calculateLength();
            setCosX();
            setSinX();
            calculateStartAndEndDots();
            caluclateArrowDots();
            setTimeDot();
        }

        public override bool Equals(object obj)
        {
            return obj is Link link &&
                   name.Equals(link.name);
        }
    }
}