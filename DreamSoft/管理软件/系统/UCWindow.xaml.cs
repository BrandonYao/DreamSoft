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
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace DreamSoft
{
    /// <summary>
    /// UCWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UCWindow : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public List<string> PRS
        {
            get { return prs; }
            set { prs = value; }
        }
        private List<string> prs = new List<string>();

        public List<string> OMS
        {
            get { return oms; }
            set { oms = value; }
        }
        private List<string> oms = new List<string>();

        public List<string> PTS
        {
            get { return pts; }
            set { pts = value; }
        }
        private List<string> pts = new List<string>();

        string color_Default = Colors.Khaki.ToString();
        string color_Yes = Colors.LimeGreen.ToString();
        string color_No = Colors.Gray.ToString();
        public class Window : INotifyPropertyChanged
        {
            public string WindowNo { get; set; }
            public string WindowName { get; set; }
            private string prescType;
            public string PrescType
            {
                get { return prescType; }
                set
                {
                    prescType = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("PrescType"));
                }
            }
            private string outModel;
            public string OutModel
            {
                get { return outModel; }
                set
                {
                    outModel = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("OutModel"));
                }
            }
            private string defaultFlag;
            public string DefaultFlag
            {
                get { return defaultFlag; }
                set
                {
                    defaultFlag = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("DefaultFlag"));
                }
            }
            private string openFlag;
            public string OpenFlag
            {
                get { return openFlag; }
                set
                {
                    openFlag = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("OpenFlag"));
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
                PropertyChanged?.Invoke(this, e);
            }
        }

        public class Mac : INotifyPropertyChanged
        {
            private bool isChecked;
            public bool IsChecked
            {
                get { return isChecked; }
                set
                {
                    isChecked = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsChecked"));
                }
            }
            public string MacCode { get; set; }
            public string MacName { get; set; }
            private string outPos;
            public string OutPos
            {
                get { return outPos; }
                set
                {
                    outPos = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("OutPos"));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, e);
            }
        }

        public UCWindow()
        {
            InitializeComponent();
            string type = "PrescType";
            DataTable dtName;
            string s = "select dictname from sys_dict where dicttypecode='{0}' order by ordernum";

            string sql = string.Format(s, type);
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtName);
            if (dtName != null)
            {
                foreach (DataRow row in dtName.Rows)
                {
                    prs.Add(row[0].ToString().Trim());
                }
            }

            type = "OutModel"; 
            sql = string.Format(s, type);
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtName);
            if (dtName != null)
            {
                foreach (DataRow row in dtName.Rows)
                {
                    oms.Add(row[0].ToString().Trim());
                }
            } 
            
            type = "OutPosition";
            sql = string.Format(s, type);
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtName);
            if (dtName != null)
            {
                foreach (DataRow row in dtName.Rows)
                {
                    pts.Add(row[0].ToString().Trim());
                }
            }
        }

        private void ShowWindow()
        {
            List<Window> ws = new List<Window>();
            string sql = @"select windowno,windowname,
(select dictname from sys_dict sd where sd.dicttypecode='PrescType' and sd.dictcode=sw.presctype) as presctype, 
(select dictname from sys_dict sd where sd.dicttypecode='OutModel' and sd.dictcode=sw.outmodel) as outmodel,
defaultflag,openflag from sys_window sw";
            DataTable dtWindow;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtWindow);
            if (dtWindow != null && dtWindow.Rows.Count > 0)
            {
                foreach (DataRow row in dtWindow.Rows)
                {
                    Window w = new Window()
                    {
                        WindowNo = row["windowno"].ToString().Trim(),
                        WindowName = row["windowname"].ToString().Trim(),
                        PrescType = row["presctype"].ToString().Trim(),
                        OutModel = row["outmodel"].ToString().Trim(),
                        DefaultFlag = row["defaultflag"].ToString().Trim(),
                        OpenFlag = row["openflag"].ToString().Trim()
                    };
                    if (bool.Parse(w.DefaultFlag))
                        w.BackColor = color_Default;
                    else
                        w.BackColor = bool.Parse(w.OpenFlag) ? color_Yes : color_No;
                    ws.Add(w);
                }
            }
            dgWindow.ItemsSource = ws;
        }
        private void ShowKF()
        {
            List<Mac> ms = new List<Mac>();
            string sql = "select * from sys_macinfo sm where mactype='A'";
            DataTable dtMac;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtMac);
            if (dtMac != null && dtMac.Rows.Count > 0)
            {
                foreach (DataRow row in dtMac.Rows)
                {
                    Mac m = new Mac()
                    {
                        IsChecked = false,
                        MacCode = row["maccode"].ToString().Trim(),
                        MacName = row["macname"].ToString().Trim(),
                        //OutPos = "下出药口"
                    };
                    ms.Add(m);
                }
            }
            dgKF.ItemsSource = ms;
        }
        private void ShowMac()
        {
            List<Mac> ms = new List<Mac>();
            string sql = "select * from sys_macinfo sm where mactype <>'A'";
            DataTable dtMac;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtMac);
            if (dtMac != null && dtMac.Rows.Count > 0)
            {
                foreach (DataRow row in dtMac.Rows)
                {
                    Mac m = new Mac()
                    {
                        IsChecked = false,
                        MacCode = row["maccode"].ToString().Trim(),
                        MacName = row["macname"].ToString().Trim()
                    };
                    ms.Add(m);
                }
            }
            dgMac.ItemsSource = ms;
        }
        private void ShowPosition(string windowNo)
        {
            Dictionary<string, string> wms = new Dictionary<string, string>();
            string sql = @"select maccode,sd.dictname from sys_window_mac swm
left join sys_dict sd on sd.dicttypecode='outposition' and swm.outposition=sd.dictcode where windowno='{0}'";
            sql = string.Format(sql, windowNo);
            DataTable dtPosi;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPosi);
            if (dtPosi != null && dtPosi.Rows.Count > 0)
            {
                foreach (DataRow row in dtPosi.Rows)
                {
                    if (!wms.Keys.Contains(row["maccode"].ToString().Trim()))
                        wms.Add(row["maccode"].ToString().Trim(), row["dictname"].ToString().Trim());
                }
            }
            List<Mac> ms_KF = dgKF.ItemsSource as List<Mac>;
            foreach (Mac m in ms_KF)
            {
                if (wms.Keys.Contains(m.MacCode))
                {
                    m.IsChecked = true;
                    m.OutPos = wms[m.MacCode];
                }
                else
                {
                    m.IsChecked = false;
                    m.OutPos = null;
                }
            } 
            List<Mac> ms_Mac = dgMac.ItemsSource as List<Mac>;
            foreach (Mac m in ms_Mac)
            {
                if (wms.Keys.Contains(m.MacCode))
                    m.IsChecked = true;
                else
                    m.IsChecked = false;
            }
        }

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            ShowWindow();
            ShowKF();
            ShowMac();
        }

        private void btDefault_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string windowNo = b.Tag.ToString();
            List<Window> ws = dgWindow.ItemsSource as List<Window>;
            foreach (Window w in ws)
            {
                if (w.WindowNo == windowNo)
                {
                    if (!bool.Parse(w.DefaultFlag))
                    {
                        string sql = "update sys_window set defaultflag=0;update sys_Window set defaultflag=1 where windowno='{0}'";
                        sql = string.Format(sql, windowNo);
                        if (csSql.ExecuteSql(sql, Config.Soft.ConnString))
                        {
                            ShowWindow();
                        }
                    }
                    else
                        csMsg.ShowWarning("此窗口已为默认", false);
                    break;
                }
            }
        }
        private void btOpen_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string windowNo = b.Tag.ToString();
            List<Window> ws = dgWindow.ItemsSource as List<Window>;
            foreach (Window w in ws)
            {
                if (w.WindowNo == windowNo)
                {
                    if (bool.Parse(w.DefaultFlag))
                    {
                        csMsg.ShowWarning("默认窗口禁止操作", false);
                    }
                    else
                    {
                        bool u = !bool.Parse(w.OpenFlag);
                        string s = u ? "1" : "0";
                        string sql = "update sys_Window set openflag={0} where windowno='{1}'";
                        sql = string.Format(sql, s, windowNo);
                        if (csSql.ExecuteSql(sql, Config.Soft.ConnString))
                        {
                            ShowWindow();
                        }
                    }
                    break;
                }
            }
        }

        private void dgWindow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgWindow.SelectedItem != null)
            {
                Window w = dgWindow.SelectedItem as Window;
                ShowPosition(w.WindowNo);
            }
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            //保存窗口
            List<Window> ws = dgWindow.ItemsSource as List<Window>;
            string sql = "";
            foreach (Window w in ws)
            {
                string s = @"update sys_window set presctype=(select dictcode from sys_dict where dicttypecode='PrescType' and dictname='{0}'),
outmodel=(select dictcode from sys_dict where dicttypecode='OutModel' and dictname='{1}') where windowno='{2}';";
                sql += string.Format(s, w.PrescType, w.OutModel, w.WindowNo);
            }
            if (!string.IsNullOrEmpty(sql))
                csSql.ExecuteSql(sql, Config.Soft.ConnString);

            //保存设备
            if (dgWindow.SelectedItem != null)
            {
                Window w = dgWindow.SelectedItem as Window;

                sql = "delete from sys_window_mac where windowno='{0}';";
                sql = string.Format(sql, w.WindowNo);
                Dictionary<string, string> ms = new Dictionary<string, string>();
                List<Mac> ms_KF = dgKF.ItemsSource as List<Mac>;
                foreach (Mac m in ms_KF)
                {
                    if (m.IsChecked)
                        ms.Add(m.MacCode, m.OutPos);
                }
                foreach (string mac in ms.Keys)
                {
                    string s = "insert into sys_window_mac (windowno,maccode,outposition) select '{0}','{1}',dictcode from sys_dict where dicttypecode='OutPosition' and dictname='{2}';";
                    sql += string.Format(s, w.WindowNo, mac, ms[mac]);
                }

                ms.Clear();
                List<Mac> ms_Mac = dgMac.ItemsSource as List<Mac>;
                foreach (Mac m in ms_Mac)
                {
                    if (m.IsChecked)
                        ms.Add(m.MacCode, m.OutPos);
                }
                foreach (string mac in ms.Keys)
                {
                    string s = "insert into sys_window_mac (windowno,maccode) values ('{0}','{1}');";
                    sql += string.Format(s, w.WindowNo, mac);
                }
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
            }
            csMsg.ShowInfo("操作成功", false);
        }
    }
}
