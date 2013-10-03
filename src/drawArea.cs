using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace sku_to_smv
{
    class drawArea : PictureBox
    {
       public drawArea()
        {

        }
        ~drawArea() { }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
        public void Refresh(Bitmap bm)
        {
            this.Image = bm;
            base.Refresh();
        }
    }
}
