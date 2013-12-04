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
        public SignalTable(int rows)
        {
            InitializeComponent();

            CurrentColCount = 2;
            CurrentRowCount = rows;
            CurrentStep = 0;
            LoopSteps = false;

            inputStyle = new System.Windows.Forms.DataGridViewCellStyle();
            inputStyle.BackColor = System.Drawing.Color.LightSalmon;
            selectStyle = new System.Windows.Forms.DataGridViewCellStyle();
            selectStyle.BackColor = System.Drawing.Color.LightGreen;
            Table = new TableElement[CurrentRowCount];
            for (int i = 0; i < CurrentRowCount; i++)
            {
                Table[i] = new TableElement();
            }
            this.dataGridView1.ColumnCount = CurrentColCount;
            this.dataGridView1.RowCount = CurrentRowCount;
            UpdateTable();
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
            Table[Row].values[0] = Signaled ? returnResults.rTRUE : returnResults.rFALSE;
            Table[Row].b_Input = Input;
            UpdateTable();
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
            UpdateTable();
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
                for (int i = 0; i < CurrentRowCount; i++)
                {
                    Table[i].DeleteElement(this.dataGridView1.CurrentCell.ColumnIndex);
                }
                UpdateTable();
            }
        }
        /// <summary>
        /// Установка общего числа шагов симуляции
        /// </summary>
        /// <param name="Steps"></param>
        public void SetSimulationRange(int Steps)
        {
            CurrentColCount = Steps+1;
            for (int i = 0; i < Table.Length; i++)
            {
                Table[i].Resize(Steps);
            }
            UpdateTable();
        }
        /// <summary>
        /// Возвращет значение сигнала на текущем шаге симуляции по номеру
        /// </summary>
        /// <param name="num">Номер сигнала</param>
        /// <returns>Состояние сигнала типа returnResults</returns>
        public returnResults GetElementByNumber(int num)
        {
            if (num <= CurrentRowCount && (CurrentStep <= CurrentColCount - 1) && CurrentStep != 0)
            {
                return Table[num].values[CurrentStep-1];
            }
            return returnResults.rUNDEF;
        }
        /// <summary>
        /// Переход на следующий шаг симуляции
        /// </summary>
        public void NextStep()
        {
            if (CurrentStep <= CurrentColCount - 2)
            {
                CurrentStep++;
            }
            else
            {
                if (LoopSteps)
                {
                    CurrentStep = 1;
                }
                else CurrentStep = CurrentColCount + 1;
            }
            UpdateTable();
        }
        /// <summary>
        /// Сброс симуляции
        /// </summary>
        public void ResetSteps()
        {
            CurrentStep = 0;
            UpdateTable();
        }
        /// <summary>
        /// Обновляет содержимое таблицы
        /// </summary>
        private void UpdateTable()
        {
            this.dataGridView1.ColumnCount = CurrentColCount;
            this.dataGridView1.Columns[0].HeaderText = "Сигналы";
            for (int i = 0; i < CurrentRowCount; i++ )
            {
                this.dataGridView1[0, i].Value = Table[i].name;
                if (Table[i].b_Input)
                    this.dataGridView1[0, i].Style = inputStyle;
                this.dataGridView1[0, i].ReadOnly = true;
                for (int j = 1; j < CurrentColCount; j++)
                {
                    if (CurrentStep != 0 && j == CurrentStep)
                    {
                        this.dataGridView1[j, i].Style = selectStyle;
                    }
                    else this.dataGridView1[j, i].Style = this.dataGridView1[0, 0].Style;
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
            }
        }
        /// <summary>
        /// Обработчик события завершения правки ячейки таблицы
        /// Следит за правильностью ввода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            String str;
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
            }
            UpdateTable();
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
            UpdateTable();
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
