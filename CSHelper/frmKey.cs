using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace CSHelper
{
    public partial class frmKey : Form
    {

        public frmKey()
        {
            InitializeComponent();
        }
        //按钮事件
        private void button_Click(object sender, EventArgs e)
        {
            foreach (Button b in panel1.Controls)
            {
                b.BackColor = Color.Black;
            }
            Button bt = (Button)sender;
            bt.BackColor = Color.Brown;

            string k = bt.Text.Trim();
            System.Windows.Input.Key key = System.Windows.Input.Key.None;
            switch (k)
            {
                case "0":
                    key = System.Windows.Input.Key.D0;
                    break;
                case "1":
                    key = System.Windows.Input.Key.D1;
                    break;
                case "2":
                    key = System.Windows.Input.Key.D2;
                    break;
                case "3":
                    key = System.Windows.Input.Key.D3;
                    break;
                case "4":
                    key = System.Windows.Input.Key.D4;
                    break;
                case "5":
                    key = System.Windows.Input.Key.D5;
                    break;
                case "6":
                    key = System.Windows.Input.Key.D6;
                    break;
                case "7":
                    key = System.Windows.Input.Key.D7;
                    break;
                case "8":
                    key = System.Windows.Input.Key.D8;
                    break;
                case "9":
                    key = System.Windows.Input.Key.D9;
                    break;
                case "A":
                    key = System.Windows.Input.Key.A;
                    break;
                case "B":
                    key = System.Windows.Input.Key.B;
                    break;
                case "C":
                    key = System.Windows.Input.Key.C;
                    break;
                case "D":
                    key = System.Windows.Input.Key.D;
                    break;
                case "E":
                    key = System.Windows.Input.Key.E;
                    break;
                case "F":
                    key = System.Windows.Input.Key.F;
                    break;
                case "G":
                    key = System.Windows.Input.Key.G;
                    break;
                case "H":
                    key = System.Windows.Input.Key.H;
                    break;
                case "I":
                    key = System.Windows.Input.Key.I;
                    break;
                case "J":
                    key = System.Windows.Input.Key.J;
                    break;
                case "K":
                    key = System.Windows.Input.Key.K;
                    break;
                case "L":
                    key = System.Windows.Input.Key.L;
                    break;
                case "M":
                    key = System.Windows.Input.Key.M;
                    break;
                case "N":
                    key = System.Windows.Input.Key.N;
                    break;
                case "O":
                    key = System.Windows.Input.Key.O;
                    break;
                case "P":
                    key = System.Windows.Input.Key.P;
                    break;
                case "Q":
                    key = System.Windows.Input.Key.Q;
                    break;
                case "R":
                    key = System.Windows.Input.Key.R;
                    break;
                case "S":
                    key = System.Windows.Input.Key.S;
                    break;
                case "T":
                    key = System.Windows.Input.Key.T;
                    break;
                case "U":
                    key = System.Windows.Input.Key.U;
                    break;
                case "V":
                    key = System.Windows.Input.Key.V;
                    break;
                case "W":
                    key = System.Windows.Input.Key.W;
                    break;
                case "X":
                    key = System.Windows.Input.Key.X;
                    break;
                case "Y":
                    key = System.Windows.Input.Key.Y;
                    break;
                case "Z":
                    key = System.Windows.Input.Key.Z;
                    break;
            }
            Keyboard.Press(key);
        }
        //删除键BACKSPACE
        private void button48_Click(object sender, EventArgs e)
        {
            Keyboard.Press(System.Windows.Input.Key.Back);
        }
        //SHIFT
        private void button42_Click(object sender, EventArgs e)
        {
            foreach (Control c in panel1.Controls)
            {
                string t = c.GetType().ToString();
                if (t == "System.Windows.Forms.Button")
                {
                    string s = c.Text;
                    c.Text = c.Tag.ToString();
                    c.Tag = s;
                }
            }
        }
        //关闭
        private void button49_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        #region 将当前窗体指定为浮动工具条窗体
        public enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00000000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,

            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,

            WS_CAPTION = WS_BORDER | WS_DLGFRAME,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW = WS_CHILD,

            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_TRANSPARENT = 0x00000020,

            WS_EX_MDICHILD = 0x00000040,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_WINDOWEDGE = 0x00000100,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_CONTEXTHELP = 0x00000400,

            WS_EX_RIGHT = 0x00001000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,

            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_APPWINDOW = 0x00040000,

            WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
            WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),

            WS_EX_LAYERED = 0x00080000,

            WS_EX_NOINHERITLAYOUT = 0x00100000,
            WS_EX_LAYOUTRTL = 0x00400000,


            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_NOACTIVATE = 0x08000000
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams ret = base.CreateParams;
                ret.Style = (int)WindowStyles.WS_THICKFRAME | (int)WindowStyles.WS_CHILD;
                ret.ExStyle |= (int)WindowStyles.WS_EX_NOACTIVATE | (int)WindowStyles.WS_EX_TOOLWINDOW;
                ret.X = this.Location.X;
                ret.Y = this.Location.Y;
                return ret;
            }
        }

        #endregion
    }
}
