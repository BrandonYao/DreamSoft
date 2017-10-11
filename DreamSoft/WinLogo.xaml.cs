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
using System.Threading;

namespace DreamSoft
{
    /// <summary>
    /// WinFirst.xaml 的交互逻辑
    /// </summary>
    public partial class WinLogo : Window
    {
        CSHelper.INI csIni = new CSHelper.INI();
        public WinLogo()
        {
            InitializeComponent();
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            Config.InitialConfig_Client();
            if (Config.Soft.Config == "Y")
            {
                new WinConfig().ShowDialog();
            }

            switch (Config.Soft.SoftType)
            {
                case "0":
                    new WinLogin().Show();
                    break;
                case "1":
                    new WinMain_AP().Show();
                    break;
                case "2":
                    new WinMain_SP().Show();
                    break;
                case "3":
                    new WinMain_CP().Show();
                    break;
            }
            Close();
        }
    }
}
