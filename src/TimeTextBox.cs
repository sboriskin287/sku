using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace sku_to_smv.src
{
    public class TimeTextBox : TextBox
    {
        public Time timeMark;
        public DrawArea area;

        public TimeTextBox(DrawArea area)
        {
            this.area = area;
        }

        protected override void OnLostFocus(EventArgs e)
        {               
            base.OnLostFocus(e);
            String value = Text;
            if (!string.IsNullOrEmpty(value))
            {
                timeMark.value = Convert.ToInt32(value);
                area.Refresh();
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar);
            base.OnKeyPress(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            Text = string.Empty;
        }
    }
}
