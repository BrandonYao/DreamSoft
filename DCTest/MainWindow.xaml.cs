using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DCTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DCT_AP.DCT_AP_Initial("192.168.1.101", 2000);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int code, time;
            if (int.TryParse(tbxCode.Text, out code) && int.TryParse(tbxTime.Text, out time))
            {
                DCT_AP.DCTMoveDownSingle(code, time);
            }
            else MessageBox.Show("输入编号或时长无效");
        }
    }
}
