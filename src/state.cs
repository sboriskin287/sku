namespace SCUConverterDrawArea
{
    public enum STATE_TYPE { NONE, INPUT, OUTPUT};
    public class State
    {
        public string Name;        //Имя состояния
        public int x;               //координата x
        public int y;               //координата y
        public bool Selected;       //если выбрано = true
        public bool InputSignal;    //если входной сигнал = true
        public bool Signaled;
        public bool AlSignaled;
        public STATE_TYPE Type;
        public State()
        {
            Name = null;
            x = 0;
            y = 0;
            Selected = false;
            InputSignal = false;
            Signaled = false;
            AlSignaled = false;
            Type = STATE_TYPE.NONE;
        }
    }
}