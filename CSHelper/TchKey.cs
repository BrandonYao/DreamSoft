using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSHelper
{
    public class TKey
    {
        static frmKey wk = new frmKey();
        public void Show(int x, int y)
        {
            wk.Left = x;
            wk.Top = y;
            wk.TopMost = true;
            wk.Show();
        }

        public void Close()
        {
            wk.Hide();
        }
    }
}
