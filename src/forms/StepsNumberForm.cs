using System;
using System.Windows.Forms;

namespace sku_to_smv
{
    public partial class StepsNumberForm : Form
    {
        public int steps;
        int pxStep;
        int time;
        public StepsNumberForm()
        {
            steps = 1;
            pxStep = 1;
            time = 0;
            InitializeComponent();
            this.button2.Select();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            steps = Int32.Parse(this.textBox1.Text);
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text.Length > 0)
                {
                    if (Int32.Parse(this.textBox1.Text) < 0)
                    {
                        this.label2.Text = "Значение не может быть отрицательным";
                        this.label2.Visible = true;
                        this.button2.Enabled = false;
                        Animate(false);
                        Animate(true);
                    }
                    else
                    {
                        Animate(false);
                        this.label2.Visible = false;
                        this.button2.Enabled = true;
                    }
                }
            }
            catch (FormatException)
            {
                this.label2.Text = "Введен недопустимый символ";
                this.label2.Visible = true;
                this.button2.Enabled = false;
                Animate(false);
                Animate(true);
            }
            catch (OverflowException)
            {
                this.label2.Text = "Введено слишком большое число";
                this.label2.Visible = true;
                this.button2.Enabled = false;
                Animate(false);
                Animate(true);
            }
        }
        private void Animate(bool show)
        {
            if (show)
            {
                pxStep = 1;
                time = 0;
                this.timer1.Start();
            }
            else this.label2.Location = new System.Drawing.Point(this.label2.Location.X, 50);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (time < 20)
            {
                this.label2.Location = new System.Drawing.Point(this.label2.Location.X, this.label2.Location.Y + pxStep);
                time++;
            }
            else this.timer1.Stop();
        }
    }
}
