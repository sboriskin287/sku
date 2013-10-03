namespace sku_to_smv
{
    public class Element
    {
        public bool Empty;
        public bool Inverted;
        public string Type;
        public string Value;
        public bool Local;
        public Element()
        {
            Empty = true;
            Inverted = false;
            Local = true;
            Type = "";
            Value = "";
        }
        ~Element() { }
    }
}