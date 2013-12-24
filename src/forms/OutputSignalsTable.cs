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
        public OutputSignalsTable(int count = 0)
        {
            InitializeComponent();
            if (count != 0)
            {
                DataGridViewCheckBoxColumn checkedColumn = new DataGridViewCheckBoxColumn();
                checkedColumn.HeaderText = "Задать";
                checkedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                this.sigTable.Columns.Add(checkedColumn);
                this.sigTable.Rows.Add(count);
                this.sigTable.Columns.Add("outputSig", "Выходной сигнал");
                this.sigTable.Columns.Add("states", "Состояние");
                this.sigTable.AllowUserToAddRows = false;
                this.sigTable.AllowUserToOrderColumns = false;
            }
        }
        public void AddState(int num, string StateName, string OutputName)
        {
            this.sigTable[2,num].Value = StateName;
            if (OutputName != null)
            {
                this.sigTable[0,num].Value = true;
                this.sigTable[1,num].Value = OutputName;
            }
        }
    }
}
