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
using System.Windows.Media.Effects;
using System.Windows.Threading;
using System.Threading;
using System.Data;

namespace DreamSoft
{
    /// <summary>
    /// UCDebug.xaml 的交互逻辑
    /// </summary>
    public partial class UCDebug : UserControl
    {
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCDebug()
        {
            InitializeComponent();
        }

        private void ShowLayerSet()
        {
            for (int i = 0; i < 2; i++)
            {
                gridLayerSet.RowDefinitions.Add(new RowDefinition());
                for (int j = 0; j < 5; j++)
                {
                    if (i == 0)
                        gridLayerSet.ColumnDefinitions.Add(new ColumnDefinition());
                    Button b = new Button();
                    b.Content = "设定" + (i * 10 + (j * 2 + 1)).ToString().PadLeft(2, '0') + "层";
                    b.Tag = i * 10 + (j * 2 + 1);
                    b.Margin = new Thickness(5);
                    b.Click += new RoutedEventHandler(Set_Click);
                    gridLayerSet.Children.Add(b);
                    Grid.SetRow(b, i);
                    Grid.SetColumn(b, j);
                }
            }
        }
        void Set_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            string lay = bt.Tag.ToString();
            if (csMsg.ShowQuestion("确定要设置该层脉冲吗？", false))
            {
                //读取当前脉冲
                int pulse = PLC_SP.ReadNowPulse();
                PLC_SP.SavePulse(int.Parse(lay), pulse);
            }
        }

        private void ShowLayerTurn()
        {
            for (int i = 0; i < 2; i++)
            {
                gridLayerTurn.RowDefinitions.Add(new RowDefinition());
                for (int j = 0; j < 5; j++)
                {
                    if (i == 0)
                        gridLayerTurn.ColumnDefinitions.Add(new ColumnDefinition());
                    Button b = new Button();
                    b.Content = "转到" + (i * 10 + (j * 2 + 1)).ToString().PadLeft(2, '0') + "层";
                    b.Tag = i * 10 + (j * 2 + 1);
                    b.Margin = new Thickness(5);
                    b.Click += new RoutedEventHandler(Turn_Click);
                    gridLayerTurn.Children.Add(b);
                    Grid.SetRow(b, i);
                    Grid.SetColumn(b, j);
                }
            }
            //for (int i = 0; i < 10; i++)
            //{
            //    gridLayerTurn.ColumnDefinitions.Add(new ColumnDefinition());
            //    Button b = new Button();
            //    b.Content = "转到" + (i * 2 + 1).ToString().PadLeft(2, '0') + "层";
            //    b.Tag = i * 2 + 1;
            //    b.Margin = new Thickness(5);
            //    b.Click += new RoutedEventHandler(Turn_Click);
            //    gridLayerTurn.Children.Add(b);
            //    Grid.SetColumn(b, i);
            //}
        }
        void Turn_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            string lay = bt.Tag.ToString();
            PLC_SP.TurnTo(int.Parse(lay));
        }

        private void ShowLasers()
        {
            for (int i = 0; i < 3; i++)
            {
                gridLaser.RowDefinitions.Add(new RowDefinition());
                for (int j = 0; j < 12; j++)
                {
                    if (i == 0)
                        gridLaser.ColumnDefinitions.Add(new ColumnDefinition());

                    TextBlock tb = new TextBlock();
                    tb.Text = (i + 1).ToString() + "-" + (j + 1).ToString();
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.Foreground = new SolidColorBrush(Colors.Black);
                    gridLaser.Children.Add(tb);
                    Grid.SetRow(tb, 2 - i);
                    Grid.SetColumn(tb, j);

                    Ellipse b = new Ellipse();
                    b.Fill = new SolidColorBrush(Color.FromArgb(150, 128, 128, 128));
                    double width = gridLaser.ActualWidth / 12 - 10;
                    double height = gridLaser.ActualHeight / 3 - 10;
                    b.Width = b.Height = width < height ? width : height;
                    b.Margin = new Thickness(5);
                    b.Stroke = new SolidColorBrush(Colors.Black);
                    DropShadowEffect dse = new DropShadowEffect();
                    dse.ShadowDepth = 0;
                    b.Effect = dse;
                    b.Tag = (i + 1).ToString() + "-" + (j + 1).ToString() + "-0";
                    b.MouseDown += new MouseButtonEventHandler(b_MouseDown);
                    gridLaser.Children.Add(b);
                    Grid.SetRow(b, 2 - i);
                    Grid.SetColumn(b, j);

                }
            }
        }
        void b_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse b = sender as Ellipse;
            string[] ss = b.Tag.ToString().Split(new char[] { '-' });
            int i = Math.Abs(int.Parse(ss[2]) - 1);
            b.Tag = ss[0] + "-" + ss[1] + "-" + i.ToString();
            switch (i)
            {
                case 0:
                    b.Fill = new SolidColorBrush(Color.FromArgb(150, 128, 128, 128));
                    PLC_SP.LightSingleNum(int.Parse(ss[0]), int.Parse(ss[1]), PLC_SP.LightType.Close);
                    break;
                case 1:
                    b.Fill = new SolidColorBrush(Color.FromArgb(150, 220, 20, 60));
                    PLC_SP.LightSingleNum(int.Parse(ss[0]), int.Parse(ss[1]), PLC_SP.LightType.Open);
                    break;
            }
        }

        DispatcherTimer timer_Pulse = new DispatcherTimer();
        DispatcherTimer timer_Test = new DispatcherTimer();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer_Pulse.Interval = TimeSpan.FromSeconds(2);
            timer_Pulse.Tick += new EventHandler(timer_Pulse_Tick);

            timer_Test.Interval = TimeSpan.FromSeconds(3);
            timer_Test.Tick += new EventHandler(timer_Test_Tick);

            if (Config.Mac_S.PLCIsEnable == "Y")
            {
                tbPulse.Text = PLC_SP.ReadNowPulse().ToString();
                tbLayer.Text = PLC_SP.ReadNowLay().ToString();
                timer_Pulse.Start();
            }

            if (Config.Mac_S.ShowTest == "N")
            {
                btStart_Test.Visibility = btStop_Test.Visibility = Visibility.Hidden;
            }

            ShowLayerSet();
            ShowLayerTurn();
            ShowLasers();
        }

        void timer_Pulse_Tick(object sender, EventArgs e)
        {
            tbPulse.Text = PLC_SP.ReadNowPulse().ToString();
            tbLayer.Text = PLC_SP.ReadNowLay().ToString();
        }

        private void btZero_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            PLC_SP.BackZero();
            DateTime timeBegin = DateTime.Now;
            Thread.Sleep(1000);

            while (!PLC_SP.BackZeroIsFinished())
            {
                if (DateTime.Now > timeBegin.AddSeconds(60))
                {
                    csMsg.ShowWarning("原点返回超时", false);
                    break;
                }
                Thread.Sleep(1000);
            }

            if (PLC_SP.BackZeroIsFinished())
            {
                csMsg.ShowInfo("原点返回完成", false);
            }
            Cursor = null;
        }

        private void btPerimeter_Click(object sender, RoutedEventArgs e)
        {
            PLC_SP.TestZC();
        }

        private void btUp_Click(object sender, RoutedEventArgs e)
        {
            btUp.IsEnabled = false;
            btStop.IsEnabled = true;
            btDown.IsEnabled = false;
            PLC_SP.Turn(PLC_SP.TurnType.Up);
        }
        private void btDown_Click(object sender, RoutedEventArgs e)
        {
            btUp.IsEnabled = false;
            btStop.IsEnabled = true;
            btDown.IsEnabled = false;
            PLC_SP.Turn(PLC_SP.TurnType.Down);
        }
        private void btStop_Click(object sender, RoutedEventArgs e)
        {
            btUp.IsEnabled = true;
            btStop.IsEnabled = false;
            btDown.IsEnabled = true;
            PLC_SP.Turn(PLC_SP.TurnType.Stop);
        }

        private void btAllOn_Click(object sender, RoutedEventArgs e)
        {
            PLC_SP.LightAllNum(PLC_SP.LightType.Open);
        }
        private void btAllOff_Click(object sender, RoutedEventArgs e)
        {
            PLC_SP.LightAllNum(PLC_SP.LightType.Close);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PLC_SP.LightAllNum(PLC_SP.LightType.Close);
        }

        int ToLay = 0;
        private void Test()
        {
            int nowLay = PLC_SP.ReadNowLay();
            if (ToLay == 0 || nowLay == ToLay)
            {
                Random r = new Random();
                int i = r.Next(11);
                i = (i * 2) - 1;
                PLC_SP.TurnTo(i);
            }
        }
        void timer_Test_Tick(object sender, EventArgs e)
        {
            Test();
        }
        private void btStart_Test_Click(object sender, RoutedEventArgs e)
        {
            Test();
            timer_Test.Start();
            btStart_Test.IsEnabled = false;
            btStop_Test.IsEnabled = true;
        }
        private void btStop_Test_Click(object sender, RoutedEventArgs e)
        {
            timer_Test.Stop(); 
            btStart_Test.IsEnabled = true;
            btStop_Test.IsEnabled = false;
        }
    }
}
