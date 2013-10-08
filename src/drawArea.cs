using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using sku_to_smv.src;

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

        int xT, yT, dx, dy;

      public float ScaleT;

       public drawArea()
        {
            InitializeArea();
        }
        ~drawArea() { }
        private void InitializeArea()
        {
            ScaleT = 1.0f;
            xT = 0;
            yT = 0;
            dx = 0;
            dy = 0;

            hScroll = new HScrollBar();
            vScroll = new VScrollBar();

            this.Controls.Add(hScroll);
            this.Controls.Add(vScroll);

            hScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            hScroll.LargeChange = 50;
            hScroll.Location = new System.Drawing.Point(0, this.Height - 20);
            hScroll.Maximum = 1000;
            hScroll.Name = "hScroll";
            hScroll.Size = new System.Drawing.Size(this.Width - 20, 20);
            hScroll.TabIndex = 2;
            hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScroll_Scroll);

            vScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            vScroll.LargeChange = 50;
            vScroll.Location = new System.Drawing.Point(this.Width - 20, 0);
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
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
        public void Refresh(/*Bitmap bm*/ref Link[] Links, ref State[] States)
        {
            float gip, DeltaX, DeltaY, cosa, sina, xn, yn;
            g.Clear(System.Drawing.Color.White);            //Отчищаем буфер заливая его фоном
            TextFont = new System.Drawing.Font("Courier New", (14 * ScaleT));
            //Mu.WaitOne();                                   //ждем освобождения мьютекса

            if (Links != null)
            {
                for (int i = 0; i < Links.Length - 1; i++)
                {
                    if (Links[i].FromInput == true)         //Связи от входных сигналов
                    {
                        //рисуем связь(темно-синяя линия)
                        g.DrawLine(penDarkBlue, (Links[i].x1 + 30 + xT) * ScaleT, (Links[i].y1 + 30 + yT) * ScaleT, (Links[i].x2 + 30 + xT) * ScaleT, (Links[i].y2 + 30 + yT) * ScaleT);
                        //вычисляем гипотенузу
                        gip = (float)System.Math.Sqrt(Math.Pow((Links[i].y1 + 30 + yT) * ScaleT - (Links[i].y2 + 30 + yT) * ScaleT, 2) + Math.Pow((Links[i].x1 + 30 + xT) * ScaleT - (Links[i].x2 + 30 + xT) * ScaleT, 2));

                        if (Links[i].x2 > Links[i].x1)
                        {
                            DeltaX = (Links[i].x2 - Links[i].x1) * ScaleT;
                            sina = DeltaX / gip;
                            if (Links[i].y2 < Links[i].y1)
                            {//1
                                DeltaY = (Links[i].y1 - Links[i].y2) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(penDarkGreen, (Links[i].x2 + 30 + xT) * ScaleT, (Links[i].y2 + 30 + yT) * ScaleT, ((Links[i].x2 + 30 + xT) * ScaleT) - xn, ((Links[i].y2 + 30 + yT) * ScaleT) + yn);
                            }
                            if (Links[i].y2 > Links[i].y1)
                            {//2
                                DeltaY = (Links[i].y2 - Links[i].y1) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(penDarkGreen, (Links[i].x2 + 30 + xT) * ScaleT, (Links[i].y2 + 30 + yT) * ScaleT, ((Links[i].x2 + 30 + xT) * ScaleT) - xn, ((Links[i].y2 + 30 + yT) * ScaleT) - yn);
                            }
                        }
                        if (Links[i].x2 < Links[i].x1)
                        {
                            DeltaX = (Links[i].x2 - Links[i].x1) * ScaleT;
                            sina = DeltaX / gip;
                            if (Links[i].y2 < Links[i].y1)
                            {//4
                                DeltaY = (Links[i].y1 - Links[i].y2) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(penDarkGreen, (Links[i].x2 + 30 + xT) * ScaleT, (Links[i].y2 + 30 + yT) * ScaleT, ((Links[i].x2 + 30 + xT) * ScaleT) - xn, ((Links[i].y2 + 30 + yT) * ScaleT) + yn);
                            }
                            if (Links[i].y2 > Links[i].y1)
                            {//3
                                DeltaY = (Links[i].y2 - Links[i].y1) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(penDarkGreen, (Links[i].x2 + 30 + xT) * ScaleT, (Links[i].y2 + 30 + yT) * ScaleT, ((Links[i].x2 + 30 + xT) * ScaleT) - xn, ((Links[i].y2 + 30 + yT) * ScaleT) - yn);
                            }
                        }
                    }
                    else if (Links[i].Arc == true)
                    {
                        g.DrawArc(penBlack, (Links[i].x1 - 20 + xT) * ScaleT, (Links[i].y1 - 20 + yT) * ScaleT, 50 * ScaleT, 50 * ScaleT, 0, 360);
                        g.DrawArc(penRed, (Links[i].x1 - 20 + xT) * ScaleT, (Links[i].y1 - 20 + yT) * ScaleT, 50 * ScaleT, 50 * ScaleT, 300, 60);
                    }
                    else
                    {
                        //PointF[] curvePoints = {new PointF((Links[i].x2 + 30 + xT) * Scale, (Links[i].y2 + 30 + yT) * Scale), new PointF(100.0f,100.0f), new PointF(((Links[i].x2 + 30 + xT) * Scale) - xn, ((Links[i].y2 + 30 + yT) * Scale) + yn)};
                        //g.DrawCurve(penRed, curvePoints, 1.0f);
                        g.DrawLine(penBlack, (Links[i].x1 + 30 + xT) * ScaleT, (Links[i].y1 + 30 + yT) * ScaleT, (Links[i].x2 + 30 + xT) * ScaleT, (Links[i].y2 + 30 + yT) * ScaleT);
                        gip = (float)System.Math.Sqrt(Math.Pow((Links[i].y1 + 30 + yT) * ScaleT - (Links[i].y2 + 30 + yT) * ScaleT, 2) + Math.Pow((Links[i].x1 + 30 + xT) * ScaleT - (Links[i].x2 + 30 + xT) * ScaleT, 2));
                        //xn = Math.Abs((Links[i].y1 + 30 + yT) * Scale-(Links[i].y2 + 30 + yT) * Scale)/gip*(gip-20);
                        //yn = Math.Abs((Links[i].x1 + 30 + xT) * Scale-(Links[i].x2 + 30 + xT) * Scale)/gip*(gip-20);
                        //g.DrawLine(p4,(Links[i].x1 + 30 + xT) * Scale,(Links[i].y1 + 30 + yT) * Scale,xn,yn);
                        //g.DrawLine(p3, (Links[i].x1 + 25)*4, (Links[i].y1 + 25)*4, Links[i].x2 + 25, Links[i].y2 + 25);
                        if (Links[i].x2 > Links[i].x1)
                        {
                            DeltaX = (Links[i].x2 - Links[i].x1) * ScaleT;
                            sina = DeltaX / gip;
                            if (Links[i].y2 < Links[i].y1)
                            {//1
                                DeltaY = (Links[i].y1 - Links[i].y2) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;

                                g.DrawLine(penRed, (Links[i].x2 + 30 + xT) * ScaleT, (Links[i].y2 + 30 + yT) * ScaleT, ((Links[i].x2 + 30 + xT) * ScaleT) - xn, ((Links[i].y2 + 30 + yT) * ScaleT) + yn);

                            }
                            if (Links[i].y2 > Links[i].y1)
                            {//2
                                DeltaY = (Links[i].y2 - Links[i].y1) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(penRed, (Links[i].x2 + 30 + xT) * ScaleT, (Links[i].y2 + 30 + yT) * ScaleT, ((Links[i].x2 + 30 + xT) * ScaleT) - xn, ((Links[i].y2 + 30 + yT) * ScaleT) - yn);
                            }
                        }
                        if (Links[i].x2 < Links[i].x1)
                        {
                            DeltaX = (Links[i].x2 - Links[i].x1) * ScaleT;
                            sina = DeltaX / gip;
                            if (Links[i].y2 < Links[i].y1)
                            {//4
                                DeltaY = (Links[i].y1 - Links[i].y2) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(penRed, (Links[i].x2 + 30 + xT) * ScaleT, (Links[i].y2 + 30 + yT) * ScaleT, ((Links[i].x2 + 30 + xT) * ScaleT) - xn, ((Links[i].y2 + 30 + yT) * ScaleT) + yn);
                            }
                            if (Links[i].y2 > Links[i].y1)
                            {//3
                                DeltaY = (Links[i].y2 - Links[i].y1) * ScaleT;
                                cosa = DeltaY / gip;
                                xn = 50 * sina * ScaleT;
                                yn = 50 * cosa * ScaleT;
                                g.DrawLine(penRed, (Links[i].x2 + 30 + xT) * ScaleT, (Links[i].y2 + 30 + yT) * ScaleT, ((Links[i].x2 + 30 + xT) * ScaleT) - xn, ((Links[i].y2 + 30 + yT) * ScaleT) - yn);
                            }
                        }
                    }
                }
            }
            if (States != null)
            {

                for (int i = 0; i < States.Length; i++)
                {
                    if (States[i].InputSignal == true)
                    {
                        g.FillRectangle(System.Drawing.Brushes.White, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        if (States[i].Selected) g.DrawRectangle(penOrange, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        else g.DrawRectangle(penDarkRed, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        g.DrawString(States[i].Value, TextFont, System.Drawing.Brushes.Black, (States[i].x + 10 + xT) * ScaleT, (States[i].y + 10 + yT) * ScaleT);
                    }
                    else
                    {
                        g.FillEllipse(System.Drawing.Brushes.White, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        if (States[i].Selected) g.DrawEllipse(penOrange, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        else g.DrawEllipse(penDarkRed, (States[i].x + xT) * ScaleT, (States[i].y + yT) * ScaleT, 60 * ScaleT, 60 * ScaleT);
                        g.DrawString(States[i].Value, TextFont, System.Drawing.Brushes.Black, (States[i].x + 10 + xT) * ScaleT, (States[i].y + 15 + yT) * ScaleT);
                    }
                }
            }
           // Mu.ReleaseMutex();
            g.Flush();
            this.Image = bm;
            base.Refresh();
        }

        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {
            xT = -hScroll.Value;
            //g.TranslateTransform(dx - hScroll.Value, dy);
        }

        private void vScroll_Scroll(object sender, ScrollEventArgs e)
        {
            yT = -vScroll.Value;
            //g.TranslateTransform(dx, dy - vScroll.Value);
        }
    }
}
