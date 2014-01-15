using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace sku_to_smv
{
    public partial class OutputSignalsTable : Form
    {
        OutputTableElement[] TableData;
        public int OutputSignalsCount { get; set; }
        public OutputSignalsTable(int count = 0)
        {
            InitializeComponent();
            TableData = new OutputTableElement[0];
            if (count != 0)
            {
                DataGridViewCheckBoxColumn checkedColumn = new DataGridViewCheckBoxColumn();
                checkedColumn.HeaderText = "Задать";
                checkedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                this.sigTable.Columns.Add(checkedColumn);
                this.sigTable.Rows.Add(count);
                this.sigTable.Columns.Add("outputSig", "Выходной сигнал");
                this.sigTable.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.sigTable.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.sigTable.Columns.Add("states", "Состояние");
                this.sigTable.Columns[2].ReadOnly = true;
                this.sigTable.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.sigTable.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.sigTable.AllowUserToAddRows = false;
                this.sigTable.AllowUserToOrderColumns = false;
            }
            this.sigTable.VirtualMode = true;
            this.okButton.Select();
            OutputSignalsCount = 0;
        }

        public void AddState(int num, string StateName, string OutputName)
        {
            int n = TableData.Length;
            Array.Resize(ref TableData, n + 1);
            TableData[n] = new OutputTableElement(StateName, OutputName);
            if (TableData[n].HasOutput) OutputSignalsCount++;
//             this.sigTable[2,num].Value = StateName;
//             if (OutputName != null)
//             {
//                 this.sigTable[0,num].Value = true;
//                 this.sigTable[1,num].Value = OutputName;
//             }
        }

        private void sigTable_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 0: e.Value = TableData[e.RowIndex].HasOutput;
                    break;
                case 1: e.Value = TableData[e.RowIndex].OutputName;
                    break;
                case 2: e.Value = TableData[e.RowIndex].StateName;
                    break;
            }
        }

        private void sigTable_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 0: if (TableData[e.RowIndex].HasOutput && !((bool)e.Value)) OutputSignalsCount--;
                    else if (!TableData[e.RowIndex].HasOutput && ((bool)e.Value)) OutputSignalsCount++;
                    TableData[e.RowIndex].HasOutput = (bool)e.Value;
                    if (TableData[e.RowIndex].OutputName == null || TableData[e.RowIndex].OutputName.Length == 0)
                    {
                        TableData[e.RowIndex].OutputName = TableData[e.RowIndex].StateName + "_out";
                    }
                    break;
                case 1: if (!isNameExists((string)e.Value, e.RowIndex)) TableData[e.RowIndex].OutputName = (String)e.Value;
                    else MessageBox.Show("Имена выходных сигналов не могут совпадать", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 2: break;
            }
        }
        private bool isNameExists(String _Name, int _index)
        {
            for (int i = 0; i < TableData.Length; i++ )
            {
                if (TableData[i].OutputName == _Name && i != _index)
                    return true;
            }
            return false;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        public OutputTableElement[] GetTable()
        {
            return TableData;
        }
    }

    public class OutputTableElement
    {
        public string StateName;
        public string OutputName;
        public bool HasOutput;

        public OutputTableElement(string _sName, string _oName)
        {
            StateName = _sName;
            OutputName = _oName;
            if (_oName != null && _oName.Length != 0) HasOutput = true;
            else HasOutput = false;
        }
    }
}
