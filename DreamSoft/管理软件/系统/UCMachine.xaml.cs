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
using System.ComponentModel;

namespace DreamSoft
{
    /// <summary>
    /// UCMachine.xaml 的交互逻辑
    /// </summary>
    public partial class UCMachine : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();

        public UCMachine()
        {
            InitializeComponent();
        }

        string color_Yes = Colors.LimeGreen.ToString();
        string color_No = Colors.Gray.ToString();
        public class Mac : INotifyPropertyChanged
        {
            public string MacCode { get; set; }
            public string MacName { get; set; }
            public string MacType { get; set; }

            private string userFlag;
            public string UseFlag
            {
                get { return userFlag; }
                set
                {
                    userFlag = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("UseFlag"));
                }
            }

            private string backColor;
            public string BackColor
            {
                get { return backColor; }
                set
                {
                    backColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BackColor"));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, e);
            }
        }

        private void ShowMac()
        {
            Cursor = Cursors.Wait;

            List<Mac> ms = new List<Mac>();
            string sql = "select * from view_mac";
            DataTable dtMac;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtMac);
            if (dtMac != null && dtMac.Rows.Count > 0)
            {
                foreach (DataRow row in dtMac.Rows)
                {
                    Mac m = new Mac()
                    {
                        MacCode = row["maccode"].ToString().Trim(),
                        MacName = row["macname"].ToString().Trim(),
                        MacType = row["dictname"].ToString().Trim(),
                        UseFlag = row["useflag"].ToString().Trim()
                    };
                    m.BackColor = bool.Parse(m.UseFlag) ? color_Yes : color_No;
                    ms.Add(m);
                }
            }
            lvMac.ItemsSource = ms;

            Cursor = null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowMac();
        }

        private void btUse_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string maccode = b.Tag.ToString();
            List<Mac> ms = lvMac.ItemsSource as List<Mac>;
            foreach (Mac m in ms)
            {
                if (m.MacCode == maccode)
                {
                    bool u = !bool.Parse(m.UseFlag);
                    string s = u ? "1" : "0";
                    string sql = "update sys_Macinfo set useflag={0} where maccode='{1}'";
                    sql = string.Format(sql, s, maccode);
                    if (csSql.ExecuteSql(sql, Config.Soft.ConnString))
                    {
                        ShowMac();
                    }
                    break;
                }
            }
        }
    }
}
