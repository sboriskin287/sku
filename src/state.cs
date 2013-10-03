namespace sku_to_smv
{
    public class State
    {
        public string Value;        //Имя состояния
        public int x;               //координата x
        public int y;               //координата y
        public bool Selected;       //если выбрано = true
        public bool InputSignal;    //если входной сигнал = true
        public State()
        {
            Value = null;
            x = 0;
            y = 0;
            Selected = false;
            InputSignal = false;
        }
        ~State() { }
    }
}