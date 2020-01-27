using sku_to_smv.src;
using System.Collections.Generic;
using System.Drawing;

namespace sku_to_smv
{
    public enum STATE_TYPE { NONE, INPUT, OUTPUT};
    public class State
    {
        public string Name;        //Имя состояния
        public Dot paintDot;
        public Dot nameDot;
        public bool Selected;       //если выбрано = true
        public List<Signal> inputs;
        public List<Signal> outputs;
        public State()
        {
            Name = null;
            paintDot = new Dot();
            nameDot = new Dot();
            Selected = false;
            inputs = new List<Signal>();
            outputs = new List<Signal>();
        }

        public override bool Equals(object obj)
        {
            return obj is State state &&
                   Name.Equals(state.Name);
        }

        public void setNameDot()
        {
            int radius = Properties.Settings.Default.StateDiametr / 2;
            Font style = Properties.Settings.Default.StateNameText;
            nameDot = new Dot(
                paintDot.x + radius - style.Size,
                paintDot.y + radius - style.Size);
        }


    }
}