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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CSHelper
{
    /// <summary>
    /// WinMsg.xaml 的交互逻辑
    /// </summary>
    public partial class WinMsg : UserControl
    {
        string text;
        Color color;
        public WinMsg(string t, Color c)
        {
            InitializeComponent();

            text = t;
            color = c;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtInfo.Text = text;
            txtInfo.Foreground = new SolidColorBrush(color);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(20);
            timer.Tick += new EventHandler(timer_Tick);
        }

        DispatcherTimer timer;
        void timer_Tick(object sender, EventArgs e)
        {
            this.Opacity -= 0.02;
            if (this.Opacity == 0)
                this.Visibility=Visibility.Collapsed;
        }
    }
}
