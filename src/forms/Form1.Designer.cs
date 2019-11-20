using sku_to_smv;
using System.Drawing;

namespace sku_to_smv
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.открытьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.правкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.построитьГрафToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.анализироватьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.генерироватьVHDLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьКакРисунокToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.помощьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.CutToolButton = new System.Windows.Forms.ToolStripButton();
            this.CopyToolButton = new System.Windows.Forms.ToolStripButton();
            this.PasteToolButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton8 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton9 = new System.Windows.Forms.ToolStripButton();
            this.NewGraf = new System.Windows.Forms.ToolStripButton();
            this.NewSKU = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton10 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton7 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.grafPage = new System.Windows.Forms.TabPage();
            this.pictureBox1 = new sku_to_smv.drawArea();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.button5 = new System.Windows.Forms.Button();
            this.ParceTimer = new System.Windows.Forms.Timer(this.components);
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.AutosaveTimer = new System.Windows.Forms.Timer(this.components);
            this.AnimationTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.grafPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.правкаToolStripMenuItem,
            this.настройкиToolStripMenuItem,
            this.помощьToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(816, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.открытьToolStripMenuItem,
            this.сохранитьToolStripMenuItem,
            this.выходToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.файлToolStripMenuItem.Text = "File";
            // 
            // открытьToolStripMenuItem
            // 
            this.открытьToolStripMenuItem.Name = "открытьToolStripMenuItem";
            this.открытьToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.открытьToolStripMenuItem.Text = "Open...";
            this.открытьToolStripMenuItem.Click += new System.EventHandler(this.открытьToolStripMenuItem_Click);
            // 
            // сохранитьToolStripMenuItem
            // 
            this.сохранитьToolStripMenuItem.Name = "сохранитьToolStripMenuItem";
            this.сохранитьToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.сохранитьToolStripMenuItem.Text = "Save...";
            this.сохранитьToolStripMenuItem.Click += new System.EventHandler(this.сохранитьToolStripMenuItem_Click);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.выходToolStripMenuItem.Text = "Exit";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // правкаToolStripMenuItem
            // 
            this.правкаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.построитьГрафToolStripMenuItem,
            this.анализироватьToolStripMenuItem,
            this.генерироватьVHDLToolStripMenuItem,
            this.сохранитьКакРисунокToolStripMenuItem});
            this.правкаToolStripMenuItem.Name = "правкаToolStripMenuItem";
            this.правкаToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.правкаToolStripMenuItem.Text = "Edit";
            // 
            // построитьГрафToolStripMenuItem
            // 
            this.построитьГрафToolStripMenuItem.Name = "построитьГрафToolStripMenuItem";
            this.построитьГрафToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.построитьГрафToolStripMenuItem.Text = "Build graph";
            this.построитьГрафToolStripMenuItem.Click += new System.EventHandler(this.CreateGraf);
            // 
            // анализироватьToolStripMenuItem
            // 
            this.анализироватьToolStripMenuItem.Name = "анализироватьToolStripMenuItem";
            this.анализироватьToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.анализироватьToolStripMenuItem.Text = "Generate SMV...";
            this.анализироватьToolStripMenuItem.Click += new System.EventHandler(this.анализироватьToolStripMenuItem_Click);
            // 
            // генерироватьVHDLToolStripMenuItem
            // 
            this.генерироватьVHDLToolStripMenuItem.Name = "генерироватьVHDLToolStripMenuItem";
            this.генерироватьVHDLToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.генерироватьVHDLToolStripMenuItem.Text = "Generate VHDL...";
            this.генерироватьVHDLToolStripMenuItem.Click += new System.EventHandler(this.генерироватьVHDLToolStripMenuItem_Click);
            // 
            // сохранитьКакРисунокToolStripMenuItem
            // 
            this.сохранитьКакРисунокToolStripMenuItem.Name = "сохранитьКакРисунокToolStripMenuItem";
            this.сохранитьКакРисунокToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.сохранитьКакРисунокToolStripMenuItem.Text = "Save as picture...";
            this.сохранитьКакРисунокToolStripMenuItem.Click += new System.EventHandler(this.сохранитьКакРисунокToolStripMenuItem_Click);
            // 
            // настройкиToolStripMenuItem
            // 
            this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
            this.настройкиToolStripMenuItem.Size = new System.Drawing.Size(79, 20);
            this.настройкиToolStripMenuItem.Text = "Settings";
            this.настройкиToolStripMenuItem.Click += new System.EventHandler(this.настройкиToolStripMenuItem_Click);
            // 
            // помощьToolStripMenuItem
            // 
            this.помощьToolStripMenuItem.Name = "помощьToolStripMenuItem";
            this.помощьToolStripMenuItem.Size = new System.Drawing.Size(94, 20);
            this.помощьToolStripMenuItem.Text = "About";
            this.помощьToolStripMenuItem.Click += new System.EventHandler(this.помощьToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3,
            this.toolStripProgressBar1,
            this.toolStripSplitButton1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 508);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip1.Size = new System.Drawing.Size(816, 26);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 21);
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.AutoSize = false;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 21);
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(0, 21);
            this.toolStripStatusLabel3.Visible = false;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 20);
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripSplitButton1.Enabled = false;
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(106, 24);
            this.toolStripSplitButton1.Text = "Scale 100%";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripSeparator2,
            this.CutToolButton,
            this.CopyToolButton,
            this.PasteToolButton,
            this.toolStripSeparator4,
            this.toolStripButton8,
            this.toolStripButton9,
            this.NewGraf,
            this.NewSKU,
            this.toolStripButton3,
            this.toolStripSeparator1,
            this.toolStripButton10,
            this.toolStripButton4,
            this.toolStripButton5,
            this.toolStripButton6,
            this.toolStripButton7});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(816, 27);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::sku_to_smv.Properties.Resources.openHS;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton1.Text = "Open document...";
            this.toolStripButton1.Click += new System.EventHandler(this.открытьToolStripMenuItem_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::sku_to_smv.Properties.Resources.saveHS;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton2.Text = "Save document...";
            this.toolStripButton2.Click += new System.EventHandler(this.сохранитьToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // CutToolButton
            // 
            this.CutToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.CutToolButton.Image = global::sku_to_smv.Properties.Resources.CutHS;
            this.CutToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CutToolButton.Name = "CutToolButton";
            this.CutToolButton.Size = new System.Drawing.Size(24, 24);
            this.CutToolButton.Text = "Cut";
            this.CutToolButton.Click += new System.EventHandler(this.CutToolButton_Click);
            // 
            // CopyToolButton
            // 
            this.CopyToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.CopyToolButton.Image = global::sku_to_smv.Properties.Resources.CopyHS;
            this.CopyToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CopyToolButton.Name = "CopyToolButton";
            this.CopyToolButton.Size = new System.Drawing.Size(24, 24);
            this.CopyToolButton.Text = "Copy";
            this.CopyToolButton.Click += new System.EventHandler(this.CopyToolButton_Click);
            // 
            // PasteToolButton
            // 
            this.PasteToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PasteToolButton.Image = global::sku_to_smv.Properties.Resources.PasteHS;
            this.PasteToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PasteToolButton.Name = "PasteToolButton";
            this.PasteToolButton.Size = new System.Drawing.Size(24, 24);
            this.PasteToolButton.Text = "Paste";
            this.PasteToolButton.Click += new System.EventHandler(this.PasteToolButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripButton8
            // 
            this.toolStripButton8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton8.Image = global::sku_to_smv.Properties.Resources.create_smv;
            this.toolStripButton8.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton8.Name = "toolStripButton8";
            this.toolStripButton8.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton8.Text = "Generate SMV...";
            this.toolStripButton8.Click += new System.EventHandler(this.анализироватьToolStripMenuItem_Click);
            // 
            // toolStripButton9
            // 
            this.toolStripButton9.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton9.Image = global::sku_to_smv.Properties.Resources.create_vhdl;
            this.toolStripButton9.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton9.Name = "toolStripButton9";
            this.toolStripButton9.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton9.Text = "Generate VHDL...";
            this.toolStripButton9.Click += new System.EventHandler(this.генерироватьVHDLToolStripMenuItem_Click);
            // 
            // NewGraf
            // 
            this.NewGraf.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.NewGraf.Image = global::sku_to_smv.Properties.Resources.new_graf;
            this.NewGraf.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NewGraf.Name = "NewGraf";
            this.NewGraf.Size = new System.Drawing.Size(24, 24);
            this.NewGraf.Text = "Build graph";
            this.NewGraf.Click += new System.EventHandler(this.CreateGraf);
            // 
            // NewSKU
            // 
            this.NewSKU.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.NewSKU.Enabled = false;
            this.NewSKU.Image = global::sku_to_smv.Properties.Resources.new_sku;
            this.NewSKU.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NewSKU.Name = "NewSKU";
            this.NewSKU.Size = new System.Drawing.Size(24, 24);
            this.NewSKU.Text = "toolStripButton5";
            this.NewSKU.Visible = false;
            this.NewSKU.Click += new System.EventHandler(this.NewSKU_Click);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Enabled = false;
            this.toolStripButton3.Image = global::sku_to_smv.Properties.Resources.state1;
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton3.Text = "toolStripButton3";
            this.toolStripButton3.Visible = false;
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            this.toolStripSeparator1.Visible = false;
            // 
            // toolStripButton10
            // 
            this.toolStripButton10.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton10.Image = global::sku_to_smv.Properties.Resources.create_simulation;
            this.toolStripButton10.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton10.Name = "toolStripButton10";
            this.toolStripButton10.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton10.Text = "Run simulation";
            this.toolStripButton10.Visible = false;
            this.toolStripButton10.Click += new System.EventHandler(this.toolStripButton10_Click);
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Enabled = false;
            this.toolStripButton4.Image = global::sku_to_smv.Properties.Resources.play;
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton4.Text = "Start simulation";
            this.toolStripButton4.Visible = false;
            this.toolStripButton4.Click += new System.EventHandler(this.toolStripButton4_Click);
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5.Enabled = false;
            this.toolStripButton5.Image = global::sku_to_smv.Properties.Resources.step;
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton5.Text = "Stap with wait";
            this.toolStripButton5.Visible = false;
            this.toolStripButton5.Click += new System.EventHandler(this.toolStripButton5_Click);
            // 
            // toolStripButton6
            // 
            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton6.Enabled = false;
            this.toolStripButton6.Image = global::sku_to_smv.Properties.Resources.stop;
            this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton6.Text = "Stop simulation";
            this.toolStripButton6.Visible = false;
            this.toolStripButton6.Click += new System.EventHandler(this.toolStripButton6_Click);
            // 
            // toolStripButton7
            // 
            this.toolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton7.Enabled = false;
            this.toolStripButton7.Image = global::sku_to_smv.Properties.Resources.table;
            this.toolStripButton7.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton7.Name = "toolStripButton7";
            this.toolStripButton7.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton7.Text = "Table of signals";
            this.toolStripButton7.Visible = false;
            this.toolStripButton7.Click += new System.EventHandler(this.toolStripButton7_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.grafPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 51);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(816, 457);
            this.tabControl1.TabIndex = 3;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.richTextBox1);         
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(808, 431);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "SKU";           
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBox1.Location = new System.Drawing.Point(3, 3);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedBoth;
            this.richTextBox1.Size = new System.Drawing.Size(802, 425);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;            
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // grafPage
            // 
            this.grafPage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("grafPage.BackgroundImage")));
            this.grafPage.Controls.Add(this.pictureBox1);
            this.grafPage.Location = new System.Drawing.Point(4, 22);
            this.grafPage.Name = "grafPage";
            this.grafPage.Padding = new System.Windows.Forms.Padding(3);
            this.grafPage.Size = new System.Drawing.Size(808, 431);
            this.grafPage.TabIndex = 1;
            this.grafPage.Text = "Graph";
            this.grafPage.UseVisualStyleBackColor = true;
            this.grafPage.Click += new System.EventHandler(this.tabPage2_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.b_SimulStarted = false;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.LogFileName = "";
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.ScaleT = 1F;
            this.pictureBox1.Size = new System.Drawing.Size(802, 425);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.tabPage2_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // button5
            // 
            this.button5.Enabled = false;
            this.button5.FlatAppearance.BorderSize = 0;
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button5.Location = new System.Drawing.Point(1046, 48);
            this.button5.Margin = new System.Windows.Forms.Padding(0);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(20, 20);
            this.button5.TabIndex = 1;
            this.button5.Text = "X";
            this.button5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Visible = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // ParceTimer
            // 
            this.ParceTimer.Interval = 1500;
            this.ParceTimer.Tick += new System.EventHandler(this.ParceTimer_Tick);
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(121, 23);
            // 
            // AutosaveTimer
            // 
            this.AutosaveTimer.Tick += new System.EventHandler(this.AutosaveTimer_Tick);
            // 
            // AnimationTimer
            // 
            this.AnimationTimer.Interval = 2000;
            this.AnimationTimer.Tick += new System.EventHandler(this.AnimationTimer_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(816, 534);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = new Icon("../../resources/Icon.ico");
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Environment of modelling computing systems";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.grafPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem открытьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сохранитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem правкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem помощьToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TabPage grafPage;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem анализироватьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem построитьГрафToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сохранитьКакРисунокToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.ToolStripButton NewSKU;
        private System.Windows.Forms.ToolStripButton NewGraf;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private System.Windows.Forms.ToolStripButton toolStripButton7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton8;
        private System.Windows.Forms.ToolStripButton toolStripButton9;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton10;
        private System.Windows.Forms.ToolStripMenuItem генерироватьVHDLToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.Timer ParceTimer;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;
        private System.Windows.Forms.Timer AutosaveTimer;
        private System.Windows.Forms.ToolStripButton CutToolButton;
        private System.Windows.Forms.ToolStripButton CopyToolButton;
        private System.Windows.Forms.ToolStripButton PasteToolButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.Timer AnimationTimer;
        private drawArea pictureBox1;
    }
}

