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
using System.Data;
using System.Windows.Threading;

namespace DreamSoft
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WinLED : Window
    {
        CSHelper.SQL csSql = new CSHelper.SQL();

        public WinLED()
        {
            InitializeComponent();

            Config.InitialConfig_LED();
        }

        public void ShowWindows()
        {
            grid.Children.Clear(); grid.RowDefinitions.Clear(); grid.ColumnDefinitions.Clear();
            string sql = "select * from sys_window order by windowno {0}";
            sql = string.Format(sql, Config.Led.OrderType);
            DataTable dt;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string win = dt.Rows[i]["windowno"].ToString();
                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(double.Parse(dt.Rows[i]["width"].ToString()), GridUnitType.Star);
                    grid.ColumnDefinitions.Add(cd);

                    Border b=new Border();
                    b.BorderThickness = new Thickness(2); b.BorderBrush = new SolidColorBrush(Colors.Red);
                    b.CornerRadius = new CornerRadius(5); b.Margin = new Thickness(1);
                    grid.Children.Add(b); Grid.SetColumn(b, i);

                    Grid g = new Grid();
                    RowDefinition rd1 = new RowDefinition(); rd1.Height = new GridLength(1, GridUnitType.Star);
                    RowDefinition rd2 = new RowDefinition(); rd2.Height = new GridLength(1, GridUnitType.Star);
                    g.RowDefinitions.Add(rd1); g.RowDefinitions.Add(rd2);
                    ColumnDefinition cd1 = new ColumnDefinition(); cd1.Width = new GridLength(1, GridUnitType.Auto);
                    ColumnDefinition cd2 = new ColumnDefinition(); cd2.Width = new GridLength(1, GridUnitType.Auto);
                    ColumnDefinition cd3 = new ColumnDefinition(); cd3.Width = new GridLength(1, GridUnitType.Star);
                    g.ColumnDefinitions.Add(cd1); g.ColumnDefinitions.Add(cd2); g.ColumnDefinitions.Add(cd3);
                    b.Child = g;

                    TextBlock tbWin =new TextBlock();
                    tbWin.Text = win; tbWin.Foreground = new SolidColorBrush(Colors.Orange); tbWin.FontSize = 2 * Config.Led.FontSize;
                    tbWin.Style = (Style)TryFindResource("TextBlockStyle");
                    g.Children.Add(tbWin);
                    if (!bool.Parse(dt.Rows[i]["openflag"].ToString()))
                    {
                        TextBlock tbStatus = new TextBlock();
                        tbStatus.Text = "窗口关闭"; tbStatus.Foreground = new SolidColorBrush(Colors.Red); tbStatus.FontSize = 12;
                        tbStatus.Style = (Style)TryFindResource("TextBlockStyle");
                        g.Children.Add(tbStatus); Grid.SetRow(tbStatus, 1);
                    } 
                    TextBlock tb1 = new TextBlock();
                    tb1.Text = "已配:"; tb1.Foreground = new SolidColorBrush(Colors.Green); tb1.FontSize = Config.Led.FontSize;
                    tb1.Style = (Style)TryFindResource("TextBlockStyle");
                    g.Children.Add(tb1); Grid.SetColumn(tb1, 1); 
                    TextBlock tb2 = new TextBlock();
                    tb2.Text = "待配:"; tb2.Foreground = new SolidColorBrush(Colors.Red); tb2.FontSize = Config.Led.FontSize;
                    tb2.Style = (Style)TryFindResource("TextBlockStyle");
                    g.Children.Add(tb2); Grid.SetRow(tb2, 1); Grid.SetColumn(tb2, 1);

                    //已配
                    WrapPanel wp1 = new WrapPanel(); g.Children.Add(wp1); Grid.SetColumn(wp1, 2);
                    ShowName(wp1, win, "'O','H'", Colors.Green);
                    //待配
                    WrapPanel wp2 = new WrapPanel(); g.Children.Add(wp2); Grid.SetRow(wp2, 1); Grid.SetColumn(wp2, 2);
                    ShowName(wp2, win, "'W','D'", Colors.Red);
                }
            }
        }

        private void ShowName(WrapPanel wp, string win, string status, Color c)
        {
            wp.Children.Clear();
            string sql = "select patname from view_led where doflag in({1}) and windowno='{0}'";
            sql = string.Format(sql, win, status);
            DataTable dtName = new DataTable();
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtName);
            if (dtName != null && dtName.Rows.Count > 0)
            {
                foreach (DataRow row in dtName.Rows)
                {
                    TextBlock tb = new TextBlock();
                    tb.Text = " " + row["patname"].ToString() + " ";
                    tb.Foreground = new SolidColorBrush(c);
                    tb.FontSize = Config.Led.FontSize;
                    wp.Children.Add(tb);
                }
            }
        }

        DispatcherTimer timer_Refresh = new DispatcherTimer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grid.Margin = new Thickness(Config.Led.Margin_Left, Config.Led.Margin_Top, Config.Led.Margin_Left, Config.Led.Top);
            this.Width = Config.Led.Width;
            this.Height = Config.Led.Height;
            this.Top = Config.Led.Top;
            this.Left = Config.Led.Left;

            ShowWindows();

            timer_Refresh.Interval = TimeSpan.FromSeconds(Config.Led.RefreshTime);
            timer_Refresh.Tick += new EventHandler(timer_Refresh_Tick);
            timer_Refresh.Start();
        }

        void timer_Refresh_Tick(object sender, EventArgs e)
        {
            ShowWindows();
        }
    }
}
