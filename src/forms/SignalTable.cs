using System;
using System.Windows.Forms;

namespace sku_to_smv
{
    public enum returnResults{rTRUE = 1, rFALSE = 0, rUNDEF = 2};

    public partial class SignalTable : Form
    {
        int CurrentColCount;
        int CurrentRowCount;
        int CurrentStep;
        bool LoopSteps;
        DataGridViewCellStyle selectStyle, inputStyle;
        TableElement[] Table;
        Control MyParent;

        public SignalTable(int rows, Control _Parent = null)
        {
            InitializeComponent();
            this.MyParent = _Parent;

            CurrentColCount = 1;
            CurrentRowCount = rows;
            CurrentStep = 0;
            LoopSteps = false;

            inputStyle = new System.Windows.Forms.DataGridViewCellStyle();
            inputStyle.BackColor = System.Drawing.Color.White;
            selectStyle = new System.Windows.Forms.DataGridViewCellStyle();
            selectStyle.BackColor = System.Drawing.Color.White;
            Table = new TableElement[CurrentRowCount];
            for (int i = 0; i < CurrentRowCount; i++)
            {
                Table[i] = new TableElement();
            }
            this.dataGridView1.ColumnCount = CurrentColCount;
            this.dataGridView1.RowCount = CurrentRowCount+1;
            this.dataGridView1.VirtualMode = true;
            this.dataGridView1.RowHeadersVisible = true;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.Columns[0].HeaderText = "1";
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToOrderColumns = false;
            this.toolStripLabel1.Text = "Шаг " + (CurrentStep+1);
            for (int i = 0; i < CurrentRowCount; i++)
            {
                this.dataGridView1.Rows[i].Cells[CurrentStep].Style = selectStyle;
            }
            //UpdateTable();
        }
        public void ShowT()
        {
            this.Show();
            this.dataGridView1.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }
        /// <summary>
        /// Добавляет сигналы в таблицу и заполняет первый шаг симуляции
        /// </summary>
        /// <param name="Row">Номер строки</param>
        /// <param name="Name">Имя сигнала</param>
        /// <param name="Signaled">в 1-це или нет</param>
        public void AddElement(int Row, String Name, bool Signaled, bool Input)
        {
            Table[Row].name = Name;
            //Table[Row].values[0] = Signaled ? returnResults.rTRUE : returnResults.rFALSE;
            Table[Row].b_Input = Input;
            this.dataGridView1.Rows[Row].HeaderCell.Value = Name;
            this.dataGridView1.Rows[Row].HeaderCell.Style = Input ? inputStyle : this.dataGridView1.DefaultCellStyle;
            //UpdateTable();
        }
        /// <summary>
        /// Добавляет шаг симуляции 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddColumn(object sender, EventArgs e)
        {
            CurrentColCount++; 
            for (int i = 0; i < CurrentRowCount; i++)
            {
                Table[i].AddElement();
            }
            this.dataGridView1.ColumnCount = CurrentColCount;
            //this.dataGridView1.Columns[0].HeaderText = "Сигналы";
            this.dataGridView1.Columns[CurrentColCount-1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.Columns[CurrentColCount-1].HeaderText = (CurrentColCount).ToString();
            //UpdateTable();
        }
        /// <summary>
        /// Удаляет шаг симуляции
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteColumn(object sender, EventArgs e)
        {
            if (this.dataGridView1.CurrentCell.ColumnIndex > 0)
            {
                CurrentColCount--;
                this.dataGridView1.ColumnCount = CurrentColCount;
                for (int i = 0; i < CurrentRowCount; i++)
                {
                    Table[i].DeleteElement(this.dataGridView1.CurrentCell.ColumnIndex);
                }
                //UpdateTable();
                this.dataGridView1.Refresh();
            }
        }
        /// <summary>
        /// Установка общего числа шагов симуляции
        /// </summary>
        /// <param name="Steps"></param>
        public void SetSimulationRange(int Steps)
        {
            int oldCount = CurrentColCount;
            CurrentColCount = Steps;
            for (int i = 0; i < Table.Length; i++)
            {
                Table[i].Resize(Steps);
            }
            this.dataGridView1.ColumnCount = CurrentColCount;
            //this.dataGridView1.Columns[0].HeaderText = "Сигналы";
            for (int i = oldCount; i < CurrentColCount; i++)
            {
                this.dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                this.dataGridView1.Columns[i].HeaderText = (i+1).ToString();
            }
            //UpdateTable();
        }
        /// <summary>
        /// Возвращет значение сигнала на текущем шаге симуляции по номеру
        /// </summary>
        /// <param name="num">Номер сигнала</param>
        /// <returns>Состояние сигнала типа returnResults</returns>
        public returnResults GetElementByNumber(int num)
        {
            if (num <= CurrentRowCount && (CurrentStep <= CurrentColCount - 1)/* && CurrentStep != 0*/)
            {
                return Table[num].values[CurrentStep];
            }
            return returnResults.rUNDEF;
        }
        /// <summary>
        /// Переход на следующий шаг симуляции
        /// </summary>
        public void NextStep()
        {
            if (CurrentStep < CurrentColCount)
            {
                for (int i = 0; i < CurrentRowCount; i++)
                {
                    this.dataGridView1.Rows[i].Cells[CurrentStep].Style = this.dataGridView1.DefaultCellStyle;
                }
            }
            if (CurrentStep < CurrentColCount - 1)
            {
                CurrentStep++;
            }
            else
            {
                if (LoopSteps)
                {
                    CurrentStep = 0;
                }
                else CurrentStep = CurrentColCount;
            }
            if (CurrentStep < this.dataGridView1.ColumnCount)
                this.dataGridView1.FirstDisplayedScrollingColumnIndex = CurrentStep;
            this.toolStripLabel1.Text = "Шаг " + (CurrentStep + 1);
            if (CurrentStep < CurrentColCount)
            {
                for (int i = 0; i < CurrentRowCount; i++)
                {
                    this.dataGridView1.Rows[i].Cells[CurrentStep].Style = selectStyle;
                }
            }
            UpdateTable();
            //this.dataGridView1.Refresh();
            //UpdateTable();
        }
        /// <summary>
        /// Сброс симуляции
        /// </summary>
        public void ResetSteps()
        {
            if (CurrentStep < CurrentColCount)
            {
                for (int i = 0; i < CurrentRowCount; i++)
                {
                    this.dataGridView1.Rows[i].Cells[CurrentStep].Style = this.dataGridView1.DefaultCellStyle;
                }
            }
            CurrentStep = 0;
            for (int i = 0; i < CurrentRowCount; i++)
            {
                this.dataGridView1.Rows[i].Cells[CurrentStep].Style = selectStyle;
            }
            this.toolStripLabel1.Text = "Шаг " + (CurrentStep + 1);
            this.dataGridView1.FirstDisplayedScrollingColumnIndex = CurrentStep;
            UpdateTable();
            this.dataGridView1.Refresh();
        }
        /// <summary>
        /// Обновляет содержимое таблицы
        /// </summary>
        private void UpdateTable()
        {
            for (int i = this.dataGridView1.FirstDisplayedCell.RowIndex; i < CurrentRowCount; i++ )
            {
                if (this.dataGridView1.Rows[i].Displayed)
                {
                    for (int j = this.dataGridView1.FirstDisplayedCell.ColumnIndex; j < CurrentColCount; j++ )
                    {
                        if (this.dataGridView1.Columns[j].Displayed)
                        {
                            switch (Table[i].values[j])
                            {
                                case returnResults.rFALSE: this.dataGridView1[j, i].Value = "0";
                                    break;
                                case returnResults.rTRUE: this.dataGridView1[j, i].Value = "1";
                                    break;
                                case returnResults.rUNDEF: this.dataGridView1[j, i].Value = "";
                                    break;
                            }
                        }
                        else break;
                    }
                }
                else break;
            }
            /*if (this.dataGridView1.FirstDisplayedScrollingRowIndex >= 0)
            {
                for (int i = this.dataGridView1.FirstDisplayedScrollingRowIndex; i < CurrentRowCount; i++)
                {
                    this.dataGridView1[0, i].Value = Table[i].name;
                    if (Table[i].b_Input)
                        this.dataGridView1[0, i].Style = inputStyle;
                    this.dataGridView1[0, i].ReadOnly = true;

                    if (this.dataGridView1.FirstDisplayedScrollingColumnIndex >= 0)
                    {
                        for (int j = this.dataGridView1.FirstDisplayedScrollingColumnIndex == 0 ? this.dataGridView1.FirstDisplayedScrollingColumnIndex + 1 : this.dataGridView1.FirstDisplayedScrollingColumnIndex
                            ; j < CurrentColCount; j++)
                        {
                            if (dataGridView1.GetColumnDisplayRectangle(j, true).Height != 0 || dataGridView1.GetColumnDisplayRectangle(j, true).Width != 0)
                            {
                                if (CurrentStep != 0 && j == CurrentStep)
                                {
                                    this.dataGridView1[j, i].Style = selectStyle;
                                }
                                else this.dataGridView1[j, i].Style = this.dataGridView1.DefaultCellStyle;
                                switch (Table[i].values[j - 1])
                                {
                                    case returnResults.rFALSE: this.dataGridView1[j, i].Value = "0";
                                        break;
                                    case returnResults.rTRUE: this.dataGridView1[j, i].Value = "1";
                                        break;
                                    case returnResults.rUNDEF: this.dataGridView1[j, i].Value = "";
                                        break;
                                }
                                this.dataGridView1.Columns[j].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                this.dataGridView1.Columns[j].HeaderText = j.ToString();
                            }
                            else break;
                        }
                    }
                }
            }*/
        }
        /// <summary>
        /// Обработчик события завершения правки ячейки таблицы
        /// Следит за правильностью ввода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            /*String str;
            try
            {
                str = this.dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
            }
            catch (System.NullReferenceException)
            {
                str = "";
            }
            if (str != "0" && str != "1" && str != "")
            {
                this.dataGridView1[e.ColumnIndex, e.RowIndex].Value = '0';
                Table[e.RowIndex].values[e.ColumnIndex - 1] = returnResults.rUNDEF;
            }
            else
            {
                switch (str)
                {
                    case "0": Table[e.RowIndex].values[e.ColumnIndex - 1] = returnResults.rFALSE;
                        break;
                    case "1": Table[e.RowIndex].values[e.ColumnIndex - 1] = returnResults.rTRUE;
                        break;
                    case "": Table[e.RowIndex].values[e.ColumnIndex - 1] = returnResults.rUNDEF;
                        break;
                }
            }*/
            //UpdateTable();
        }
        /// <summary>
        /// Обработчик сигнала от кнопки
        /// "Задать количество шагов симуляции"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            StepsNumberForm snf = new StepsNumberForm();
            snf.ShowDialog();
            SetSimulationRange(snf.steps);
            //UpdateTable();
        }
        /// <summary>
        /// Обработчик сигнала от кнопки
        /// "Циклическая работа"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (LoopSteps)
            {
                LoopSteps = false;
                this.toolStripButton4.Checked = false;
            }
            else
            {
                LoopSteps = true;
                this.toolStripButton4.Checked = true;
            }
        }

        private void SignalTable_Shown(object sender, EventArgs e)
        {
            this.dataGridView1.Refresh();
            //UpdateTable();
        }

        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.EndScroll) this.dataGridView1.VirtualMode = true;
            else if (e.Type == ScrollEventType.LargeDecrement || e.Type == ScrollEventType.LargeIncrement)
             this.dataGridView1.VirtualMode = false;
            //UpdateTable();
        }

        private void dataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < CurrentRowCount)
            {
//                 if (e.ColumnIndex == 0)
//                 {
//                     e.Value = Table[e.RowIndex].name;
//                     if (Table[e.RowIndex].b_Input)
//                         this.dataGridView1[0, e.RowIndex].Style = inputStyle;
//                     this.dataGridView1[0, e.RowIndex].ReadOnly = true;
//                 }
//                 else
//                 {
//                     if (/*CurrentStep != 0 && */e.ColumnIndex == CurrentStep)
//                     {
//                         this.dataGridView1[e.ColumnIndex, e.RowIndex].Style = selectStyle;
//                     }
//                     else this.dataGridView1[e.ColumnIndex, e.RowIndex].Style = this.dataGridView1.DefaultCellStyle;
                    switch (Table[e.RowIndex].values[e.ColumnIndex])
                    {
                        case returnResults.rFALSE: e.Value = "0";
                            break;
                        case returnResults.rTRUE: e.Value = "1";
                            break;
                        case returnResults.rUNDEF: e.Value = "";
                            break;
                    }
                    //this.dataGridView1.Columns[e.ColumnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                /*}*/
            }
            else this.dataGridView1[e.ColumnIndex, e.RowIndex].ReadOnly = true;
        }

        private void dataGridView1_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            String str;
            if (e.RowIndex < CurrentRowCount)
            {
                try
                {
                    str = e.Value.ToString();
                }
                catch (System.NullReferenceException)
                {
                    str = "";
                }
                if (str != "0" && str != "1" && str != "")
                {
                    //e.Value = "";
                    Table[e.RowIndex].values[e.ColumnIndex] = returnResults.rUNDEF;
                }
                else
                {
                    switch (str)
                    {
                        case "0": //e.Value = "0";
                            Table[e.RowIndex].values[e.ColumnIndex] = returnResults.rFALSE;
                            break;
                        case "1": //e.Value = "1";
                            Table[e.RowIndex].values[e.ColumnIndex] = returnResults.rTRUE;
                            break;
                        case "": //e.Value = "";
                            Table[e.RowIndex].values[e.ColumnIndex] = returnResults.rUNDEF;
                            break;
                    }
                }
                this.dataGridView1.Update();
            }
        }

        private void startStripButton_Click(object sender, EventArgs e)
        {
            (this.MyParent as drawArea).ToolPanelButtonClicked(sender, new ToolButtonEventArgs("run"));
        }

        private void stepStripButton_Click(object sender, EventArgs e)
        {
            (this.MyParent as drawArea).ToolPanelButtonClicked(sender, new ToolButtonEventArgs("step"));
        }

        private void stopStripButton_Click(object sender, EventArgs e)
        {
            (this.MyParent as drawArea).ToolPanelButtonClicked(sender, new ToolButtonEventArgs("stop"));
        }

        private void resetStripButton_Click(object sender, EventArgs e)
        {
            ResetSteps();
        }
        public void UpdateSimControls(bool[] Controls)
        {
            if (Controls.Length >= 3)
            {
                this.startStripButton.Enabled = Controls[0];
                this.stepStripButton.Enabled = Controls[1];
                this.stopStripButton.Enabled = Controls[2];
            }
        }
    }
    /// <summary>
    /// Класс описывающий набор значений сигнала
    /// </summary>
    public class TableElement
    {
        public String name;
        public bool b_Input;
        public returnResults[] values;
        public TableElement()
        {
            b_Input = false;
            name = "";
            values = new returnResults[1];
            values[0] = returnResults.rUNDEF;
        }
        /// <summary>
        /// Изменение размера набора 
        /// </summary>
        /// <param name="size">Новый размер</param>
        public void Resize(int size)
        {
            int oldLeight;
            oldLeight = values.Length;
            Array.Resize(ref values, size);
            for (int i = oldLeight; i < values.Length; i++ )
            {
                values[i] = returnResults.rUNDEF;
            }
        }
        /// <summary>
        /// Удаление произвольного элемента из набора
        /// </summary>
        /// <param name="num">Номер элемента</param>
        public void DeleteElement(int num)
        {
            returnResults[] tmp = new returnResults[values.Length - 1];
            int j = 0;
            for (int i = 0; i < values.Length; i++ )
            {
                if (i != num-1)
                {
                    tmp[j] = values[i];
                    j++;
                }
            }
            values = tmp;
        }
        /// <summary>
        /// Добавление нового элемента в конец набора
        /// </summary>
        public void AddElement()
        {
            returnResults[] tmp = new returnResults[values.Length + 1];
            for (int i = 0; i < values.Length; i++)
            {
                    tmp[i] = values[i];
            }
            tmp[values.Length] = returnResults.rUNDEF;
            values = tmp;
        }
    }
}
