using System.Drawing;
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
    }
    public class SympleElement
    {
        public int StartIndex;
        public int EndIndex;
        public Color TextColor;
        public FontStyle Style;
    }
}