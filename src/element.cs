using System.Drawing;
namespace SCUConverterDrawArea
{
    // Summary:
    //     Описывает элемент списка создающегося при 
    //     разборе описания автомата.
    public class Element
    {
        public bool Empty;          //Определяет что элемент пуст.
        
        public bool Inverted;       //Определяет что элемент инвертирован.
        
        public string Type;         //Строка определяющая тип элемента.
        
        public string Value;        //Строка определяющая значение(имя) элемента.
        
        public bool Local;          //Определяет что элемент локальный.
        
        public bool Output;         //Определяет что элемент выходной.

        public Element()
        {
            Empty = true;
            Inverted = false;
            Local = true;
            Output = false;
            Type = "";
            Value = "";
        }
    }
    // Summary:
    //     Описывает элемент списка создающегося при разборе 
    //     описания автомата испльзующийся в дальнейшем для 
    //     подсветки синтаксиса.
    public class SympleElement
    {
        public int StartIndex;      //Индекс начала выделения.
        
        public int EndIndex;        //Индекс конца выделения.
        
        public Color TextColor;     //Цвет выделения.
        
        public FontStyle Style;     //Стиль выделения.
    }
}