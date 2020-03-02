using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace sku_to_smv.src
{
    public class MessageDialog
    {
        private static readonly String ERROR = "Ошибка";
        public static void activeStateIsNull()
        {
            MessageBox.Show("Выберите начальное состояние!", ERROR, MessageBoxButtons.OK);
        }
    }
}
