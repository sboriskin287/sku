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
        public Dot[] arrowDots;
        public State startState;   //имя начального состояния
        public State endState;     //имя конечного состояния
        public int lengthLink;
        public int lengthBetweenStatesCentre;
        public string name;
        public bool Arc;            //если арка = true      
        public bool Selected;
        public Time timeMark; //Время перехода
        public Signal[] signals;
        public float cosx;
        public float sinx;
        public Link()
        {
            startDot = new Dot();
            endDot = new Dot();
            arcDot = new Dot();
            arrowDots = new Dot[2];
            startState = null;
            endState = null;
            lengthLink = 0;
            lengthBetweenStatesCentre = 0;
            name = null;
            Arc = false;
            Selected = false;
            timeMark = null;
            signals = new Signal[0];
            cosx = 1;
            sinx = 0;
        }

        public Link(State startState, State endState) : this()
        {          
            this.startState = startState;
            this.endState = endState;     
        } 

        public void setName()
        {
            this.name = startState.Name + endState.Name;
        }

        private void setCosX()
        {
            cosx = startState.paintDot.Y == endState.paintDot.Y
                ? (endState.paintDot.X - startState.paintDot.X) / Math.Abs(endState.paintDot.X - startState.paintDot.X)
                : (endState.paintDot.X - startState.paintDot.X) / lengthBetweenStatesCentre;
        }

        private void setSinX()
        {
            sinx = startState.paintDot.X == endState.paintDot.X
                ? (endState.paintDot.Y - startState.paintDot.Y) / Math.Abs(startState.paintDot.Y - endState.paintDot.Y)
                : (endState.paintDot.Y - startState.paintDot.Y) / lengthBetweenStatesCentre;
        }

        private void calculateLength()
        {
            int radius = Properties.Settings.Default.StateDiametr / 2;
            lengthBetweenStatesCentre = (int)Math.Sqrt(
                    Math.Pow(endState.paintDot.X - startState.paintDot.X, 2) +
                    Math.Pow(startState.paintDot.Y - endState.paintDot.Y, 2));
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
            arrowDots[0] = new Dot(endDot.X - cos1 * arrowLength, endDot.Y - sin1 * arrowLength);
            arrowDots[1] = new Dot(endDot.X - cos2 * arrowLength, endDot.Y - sin2 * arrowLength);
        }

        private void calculateStartAndEndDots()
        {         
            if (Arc)
            {
                int arcOffset = 18;
                arcDot = new Dot(
                    startState.paintDot.X - arcOffset,
                    startState.paintDot.Y - arcOffset);
            } else
            {
                int radius = Properties.Settings.Default.StateDiametr / 2;
                float x = radius * cosx;
                float y = radius * sinx;
                startDot.X = startState.paintDot.X + radius + x;
                startDot.Y = startState.paintDot.Y + radius + y;
                endDot.X = endState.paintDot.X + radius - x;
                endDot.Y = endState.paintDot.Y + radius - y;
            }       
        }

        public void calculateLocation()
        {
            calculateLength();
            setCosX();
            setSinX();
            calculateStartAndEndDots();
            caluclateArrowDots();           
        }

        public override bool Equals(object obj)
        {
            return obj is Link link &&
                   name.Equals(link.name);
        }
    }
}