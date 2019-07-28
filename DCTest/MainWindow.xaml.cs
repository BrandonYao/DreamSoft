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
using System.Windows.Threading;

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

        private IniDAL fIni;
        private DispatcherTimer tmr_state;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            fIni = new IniDAL(Environment.CurrentDirectory + "/config.ini");
            string ip = fIni.ReadIni("TCP", "IP", "192.168.1.101");
            int port = int.Parse(fIni.ReadIni("TCP", "Port", "2000"));
            DCT_AP.DCT_AP_Initial(ip, port);
            tmr_state = new DispatcherTimer();
            tmr_state.Interval = TimeSpan.FromSeconds(1);
            tmr_state.Tick += Tmr_state_Tick;
            tmr_state.Start();
            tbkVersion.Text = "Version: " + Application.ResourceAssembly.GetName().Version.ToString();
        }

        private void Tmr_state_Tick(object sender, EventArgs e)
        {
            elpState.Fill = DCT_AP.IsConnected ? Brushes.Green : Brushes.Red;
            int code, num;
            if (int.TryParse(tbxCode.Text, out code))
            {
                DCT_AP.ReadRecordSingle(code, out num);
                tbkNum.Text = string.Format("({0})", num);
            }
            else tbkNum.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int code, time;
            if (int.TryParse(tbxCode.Text, out code) && int.TryParse(tbxTime.Text, out time))
            {
                bool res = DCT_AP.DCTMoveDownSingle(code, time);
                if(!res) MessageBox.Show("数据发送失败");
            }
            else MessageBox.Show("输入编号或时长无效");
        }
    }
}
