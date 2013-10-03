namespace sku_to_smv
{
    public class Link
    {
        public int x1, y1;          //точка начала
        public int x2, y2;          //точка конца
        public string StartState;   //имя начального состояния
        public string EndState;     //имя конечного состояния
        public bool Arc;            //если арка = true
        public bool FromInput;      //если от входного сигнала = true
        public Link()
        {
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 0;
            StartState = null;
            EndState = null;
            Arc = false;
            FromInput = false;
        }
        ~Link() { }
    }
}