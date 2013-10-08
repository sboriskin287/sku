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
        HScrollBar hScroll;
        VScrollBar vScroll;
        Graphics g;
        Bitmap bm;
        Pen penDarkRed, penBlack, penRed, penOrange, penDarkBlue, penDarkGreen;
        Font TextFont;

        int xT, yT;

      public float ScaleT;

       public drawArea()
        {
            InitializeArea();
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
        private void InitializeArea()
        {
            ScaleT = 1.0f;
            xT = 0;
            yT = 0;

            hScroll = new HScrollBar();
            vScroll = new VScrollBar();

            this.Controls.Add(hScroll);
            this.Controls.Add(vScroll);

            hScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            hScroll.LargeChange = 50;
            hScroll.Location = new System.Drawing.Point(0, this.Height-20);
            hScroll.Maximum = 1000;
            hScroll.Name = "hScroll";
            hScroll.Size = new System.Drawing.Size(this.Width-20, 20);
            hScroll.TabIndex = 2;
            hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScroll_Scroll);

            vScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            vScroll.LargeChange = 50;
            vScroll.Location = new System.Drawing.Point(this.Width-20, 0);
            vScroll.Maximum = 1000;
            vScroll.Name = "vScroll";
            vScroll.Size = new System.Drawing.Size(20, this.Height);
            vScroll.TabIndex = 1;
            vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScroll_Scroll);

            bm = new Bitmap(3000, 2000);            //графический буфер
            g = Graphics.FromImage(bm);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//Включаем сглаживание графических объектов
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;//Включаем сглаживание шрифтов
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;//Включаем интерполяцию
            //Определяются цвета
            penDarkRed = new System.Drawing.Pen(System.Drawing.Brushes.DarkRed, 2);
            penBlack = new System.Drawing.Pen(System.Drawing.Brushes.Black, 1);
            penRed = new System.Drawing.Pen(System.Drawing.Brushes.Red, 3);
            penOrange = new System.Drawing.Pen(System.Drawing.Brushes.Orange, 3);
            penDarkBlue = new System.Drawing.Pen(System.Drawing.Brushes.DarkBlue, 1);
            penDarkGreen = new System.Drawing.Pen(System.Drawing.Brushes.DarkGreen, 3);
            //И шрифт
            TextFont = new System.Drawing.Font("Courier New", (14 * ScaleT));
        }
        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void vScroll_Scroll(object sender, ScrollEventArgs e)
        {

        }
    }
}
