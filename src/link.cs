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
        public Dot[] arrowDots;
        public State startState;   //имя начального состояния
        public State endState;     //имя конечного состояния
        public int lengthLink;
        public int lengthBetweenStatesCentre;
        public string name;
        public bool Arc;            //если арка = true      
        public bool Selected;
        public float timeTransfer; //Время перехода
        public Signal[] signals;
        public float cosx;
        public float sinx;
        public Link()
        {
            startDot = new Dot();
            endDot = new Dot();
            arcDot = new Dot();
            timeDot = new Dot();
            arrowDots = new Dot[2];
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

        public Link(State startState, State endState) : this()
        {          
            this.startState = startState;
            this.endState = endState;     
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
            int arrowLength = 20; //Длина наконечника срелки        
            float cos30 = (float)Math.Sqrt(3) / 2; //Косинус 30 т.к. угол между линией и наконечником = 30
            float sin30 = 0.5F; //Синус 30 т.к. угол между линией и наконечником = 30
            float cos1 = cosx * cos30 - sinx * sin30; //cos(a+b) = cos(a)*cos(b) – sin(a)*sin(b);
            float sin1 = sinx * cos30 + cosx * sin30; //sin(a+b) = sin(a)*cos(b) + cos(a)*sin(b);
            float cos2 = cosx * cos30 + sinx * sin30; //cos(a-b) = cos(a)*cos(b) + sin(a)*sin(b);
            float sin2 = sinx * cos30 - cosx * sin30; //sin(a-b) = sin(a)*cos(b) – cos(a)*sin(b);          
            arrowDots[0] = new Dot(endDot.x - cos1 * arrowLength, endDot.y - sin1 * arrowLength);
            arrowDots[1] = new Dot(endDot.x - cos2 * arrowLength, endDot.y - sin2 * arrowLength);
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
                float x = radius * cosx;
                float y = radius * sinx;
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