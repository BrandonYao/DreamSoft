using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSHelper
{
    public partial class FrmQuestion : Form
    { 
        string t;
        public FrmQuestion(string text)
        {
            InitializeComponent();

            t = text;
        }

        private void FrmQst_Load(object sender, EventArgs e)
        {
            lbMsg.Text = t;

            this.Size = new Size(lbMsg.Width + 30, lbMsg.Height + 100);
            this.Location = new Point((Screen.PrimaryScreen.Bounds.Width - this.Width) / 2, (Screen.PrimaryScreen.Bounds.Height - this.Height) / 2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            Close();
        }
    }
}
