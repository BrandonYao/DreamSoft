using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Runtime.InteropServices;

namespace CSHelper
{
    public partial class FrmMessage : Form
    {
        string t;
        Color c;
        public FrmMessage(string text,Color color)
        {
            InitializeComponent();

            t = text;
            c = color;

            if (color == Color.Green)
                this.BackColor = this.TransparencyKey = Color.LightGreen;
            if(color ==Color.Red)
                this.BackColor = this.TransparencyKey = Color.DarkRed;
        }
        
        private void FrmMsg_Load(object sender, EventArgs e)
        {
            lbMsg.Text = t;
            lbMsg.ForeColor = c;

            this.Size = new System.Drawing.Size(lbMsg.Width, lbMsg.Height);
            this.Location = new System.Drawing.Point((Screen.PrimaryScreen.Bounds.Width - this.Width) / 2, (Screen.PrimaryScreen.Bounds.Height - this.Height) / 2);

            SetLayeredWindowAttributes(this.Handle, 0, 128, 2);  
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Opacity -= 0.02;
            this.Location = new System.Drawing.Point(this.Location.X, this.Location.Y - 1);
            if (this.Opacity == 0)
                Close();
        }

        //窗体透明
        #region
        [DllImport("user32.dll")]
        public static extern int SetLayeredWindowAttributes(IntPtr Handle, int crKey, byte bAlpha, int dwFlags); 　  
        #endregion
    }
}
