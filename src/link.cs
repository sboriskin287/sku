using sku_to_smv.src;
using System.Collections.Generic;

namespace sku_to_smv
{
    public class Link
    {
        public int x1, y1;          //точка начала
        public int x2, y2;          //точка конца
        public int signalX, signalY;//Точка отображения сигнала перехода
        public int timeX, timeY;    //Точка, в которой над линией отображается время перехода
        public int transferX, transferY; //Точка отображения сигнала перехода
        public string StartState;   //имя начального состояния
        public string EndState;     //имя конечного состояния
        public string Name;
        public bool Arc;            //если арка = true
        public bool FromInput;      //если от входного сигнала = true
        public bool Selected;
        public bool Moved;
        public double leight;
        public float timeTransfer; //Время перехода
        public List<Signal> signals;
        public double rst;
        public Link()
        {
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 0;
            signalX = 0;
            signalY = 0;
            timeX = 0;
            timeY = 0;
            StartState = null;
            EndState = null;
            Name = null;
            Arc = false;
            FromInput = false;
            Selected = false;
            Moved = false;
            leight = 0.0;
            timeTransfer = 0;
            rst = 0.0;
            signals = new List<Signal>();
        }

        public void setTimeDot()
        {
            timeX = x1 + (x2 - x1) / 2 + 20;
            timeY = y1 + (y2 - y1) / 2 + 20;
        }

        public void setTransferDot()
        {
            timeX = x1 + (x2 - x1) / 2 - 20;
            timeY = y1 + (y2 - y1) / 2 - 20;
        }

        public void setName()
        {
            this.Name = StartState + EndState;
        }
    }
}