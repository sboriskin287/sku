namespace sku_to_smv
{
    public class Link
    {
        public int x1, y1;          //точка начала
        public int x2, y2;          //точка конца
        public int timeX, timeY;    //Точка, в которой над линией отображается время перехода
        public string StartState;   //имя начального состояния
        public string EndState;     //имя конечного состояния
        public bool Arc;            //если арка = true
        public bool FromInput;      //если от входного сигнала = true
        public bool Selected;
        public bool Moved;
        public double leight;
        public float timeTransfer; //Время перехода
        public double rst;
        public Link()
        {
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 0;
            timeX = 0;
            timeY = 0;
            StartState = null;
            EndState = null;
            Arc = false;
            FromInput = false;
            Selected = false;
            Moved = false;
            leight = 0.0;
            timeTransfer = 0;
            rst = 0.0;
        }

        public void setTimeDot()
        {
            timeX = x1 + (x2 - x1) / 2 + 20;
            timeY = y1 + (y2 - y1) / 2 + 20;
        }
    }
}