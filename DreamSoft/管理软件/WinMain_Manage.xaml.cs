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
using System.Reflection;
using System.Data;

namespace DreamSoft
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WinMain_Manage : Window
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public WinMain_Manage()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowMenu();
        }
        private void ShowMenu()
        {
            string sql = "select * from view_user_menu where usercode='{0}' order by sort";
            sql = string.Format(sql, Config.Soft.UserCode);
            DataTable dt; csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
            if (dt != null)
            {
                DataRow[] rows = dt.Select("parentcode='0'");
                if (rows.Length > 0)
                {
                    foreach (DataRow row in rows)
                    {
                        MenuItem item = new MenuItem();
                        item.Uid = row["menucode"].ToString().Trim();
                        item.Header = row["menuname"].ToString().Trim();
                        switch (row["isvisible"].ToString().Trim())
                        {
                            case "Y":
                                item.Visibility = Visibility.Visible;
                                break;
                            default :
                                item.Visibility = Visibility.Collapsed;
                                break;
                        }
                        item.VerticalAlignment = VerticalAlignment.Center; 
                        item.Padding = new Thickness(10);
                        menuMain.Items.Add(item);
                        ShowChildMenu(dt, item);
                    }
                }
            }
        }
        private void ShowChildMenu(DataTable dtMenu, MenuItem parentItem)
        {
            DataRow[] rows = dtMenu.Select(string.Format("parentcode='{0}'", parentItem.Uid));
            if (rows.Length > 0)
            {
                foreach (DataRow row in rows)
                {
                    MenuItem item = new MenuItem();
                    item.Uid = row["menucode"].ToString().Trim();
                    item.Header = row["menuname"].ToString().Trim();
                    item.Tag = row["url"].ToString().Trim(); 
                    switch (row["isvisible"].ToString().Trim())
                    {
                        case "Y":
                            item.Visibility = Visibility.Visible;
                            break;
                        default:
                            item.Visibility = Visibility.Collapsed;
                            break;
                    }
                    switch (row["event"].ToString().Trim())
                    {
                        case "MenuItem_Click":
                            item.Click += new RoutedEventHandler(MenuItem_Click);
                            break;
                        case "menuPassword_Click":
                            item.Click += new RoutedEventHandler(menuPassword_Click);
                            break;
                        case "menuLed_Click":
                            item.Click += new RoutedEventHandler(menuLed_Click);
                            break;
                        case "menuAbout_Click":
                            item.Click += new RoutedEventHandler(menuAbout_Click);
                            break;
                    }
                    item.FontWeight = FontWeights.Normal;
                    item.Padding = new Thickness(10);
                    string iconUrl = row["iconurl"].ToString().Trim();
                    if (!string.IsNullOrEmpty(iconUrl))
                    {
                        item.Icon = new Image() { Source = new BitmapImage(new Uri(iconUrl, UriKind.RelativeOrAbsolute)), Width=30, Height = 30 };
                    }
                    parentItem.Items.Add(item);
                    ShowChildMenu(dtMenu, item);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            string tag = mi.Tag.ToString();

            Assembly ass = Assembly.GetExecutingAssembly();
            UserControl uc = ass.CreateInstance("DreamSoft." + tag) as UserControl;
            if (uc != null)
                ShowUC(uc, mi.Header.ToString().Trim());
        }
        private void ShowUC(UserControl uc, string header)
        {
            bool add = true;
            /*
            if (tc_Child.Items.Count > 0)
            {
                foreach (TabItem item in tc_Child.Items)
                {
                    if (item.Header.ToString().Trim() == header)
                    {
                        add = false;
                        tc_Child.SelectedItem = item;
                        break;
                    }
                }
            }
            if (add)
            {
                TabItem ti = new TabItem();
                ti.Header = "  " + header + "  ";
                ti.FontSize = 16;
                ti.FontWeight = FontWeights.Bold;
                uc.Height = tc_Child.ActualHeight < uc.Height ? tc_Child.ActualHeight : uc.Height;
                uc.Width = tc_Child.ActualWidth < uc.Width ? tc_Child.ActualWidth : uc.Width;
                ti.Content = uc;
                tc_Child.Items.Add(ti);
                tc_Child.SelectedItem = ti;
            }*/
            //带关闭功能的TabControl
            if (tc_Child2.Items.Count > 0)
            {
                foreach (Wpf.Controls.TabItem item in tc_Child2.Items)
                {
                    if (item.Header.ToString().Trim() == header)
                    {
                        add = false;
                        tc_Child2.SelectedItem = item;
                        break;
                    }
                }
            } 
            if (add)
            {
                Wpf.Controls.TabItem ti = new Wpf.Controls.TabItem();
                ti.Header = "  " + header + "  ";
                ti.FontSize = 16;
                ti.FontWeight = FontWeights.Bold;
                uc.Height = tc_Child2.ActualHeight < uc.Height ? tc_Child2.ActualHeight : uc.Height;
                uc.Width = tc_Child2.ActualWidth < uc.Width ? tc_Child2.ActualWidth : uc.Width;
                ti.Content = uc;
                tc_Child2.Items.Add(ti);
                tc_Child2.SelectedItem = ti;
            }
        }

        private void menuPassword_Click(object sender, RoutedEventArgs e)
        {
            new WinPassword().ShowDialog();
        }
        private void menuLed_Click(object sender, RoutedEventArgs e)
        {
            new WinLED().Show();
        }
        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            csMsg.ShowInfo("关于", false);
        }
    }
}
