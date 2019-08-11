using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DreamSoft
{
    /// <summary>
    /// WinPassword.xaml 的交互逻辑
    /// </summary>
    public partial class WinPass : Window
    {
        public WinPass()
        {
            InitializeComponent();
        }

        private CSHelper.Msg csMsg = new CSHelper.Msg();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Pass();
        }

        private void tbxPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Pass();
        }
        private void Pass()
        {
            string pass = DateTime.Now.ToString("yyyyMMddHH");
            if (tbxPass.Password.Trim() == pass)
                this.DialogResult = true;
            else
            {
                csMsg.ShowWarning("密码错误！\r\n想一想，现在几点了？", false);
                this.DialogResult = false;
            }
        }
    }
}
