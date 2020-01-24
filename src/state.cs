using sku_to_smv.src;
using System.Collections.Generic;

namespace sku_to_smv
{
    public enum STATE_TYPE { NONE, INPUT, OUTPUT};
    public class State
    {
        public string Name;        //Имя состояния
        public int x;               //координата x
        public int y;               //координата y
        public bool Selected;       //если выбрано = true
        public bool alreadyPaint;
        public List<Signal> inputs;
        public List<Signal> outputs;
        public State()
        {
            Name = null;
            x = 0;
            y = 0;
            Selected = false;
            alreadyPaint = false;
            inputs = new List<Signal>();
            outputs = new List<Signal>();
        }
    }
}