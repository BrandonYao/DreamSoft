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
using System.IO.Ports;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Data;
using System.Configuration;

namespace DreamSoft
{
    /// <summary>
    /// WinConfig.xaml 的交互逻辑
    /// </summary>
    public partial class WinConfig : Window
    {
        CSHelper.Msg csMsg = new CSHelper.Msg();
        CSHelper.XML csXml = new CSHelper.XML();
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.INI csIni = new CSHelper.INI();

        public WinConfig()
        {
            InitializeComponent();
        }

        private void ShowWindows()
        {
            cbWin.Items.Clear();
            if (string.IsNullOrEmpty(cbServer.Text))
            {
                csMsg.ShowWarning("服务器不能为空", false);
                return;
            }
            if (string.IsNullOrEmpty(tbUserID.Text))
            {
                csMsg.ShowWarning("用户名不能为空", false);
                return;
            }
            if (string.IsNullOrEmpty(cbDatabase.Text))
            {
                csMsg.ShowWarning("数据库不能为空", false);
                return;
            }
            string connStr = "Data Source=" + cbServer.Text.Trim() + ";User ID=" + tbUserID.Text.Trim() + ";Password=" + tbPassword.Text.Trim() + ";Initial Catalog=" + cbDatabase.Text.Trim();
            string sql = "select windowno from sys_window";
            DataTable dt; csSql.ExecuteSelect(sql, connStr, out dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    cbWin.Items.Add(row["windowno"].ToString());
                }
            }
            cbWin.Text = Config.Soft.WindowNo;
        }

        private void ShowMacs()
        {
            cbMac.Items.Clear();
            if (string.IsNullOrEmpty(cbServer.Text))
            {
                csMsg.ShowWarning("服务器不能为空", false);
                return;
            }
            if (string.IsNullOrEmpty(tbUserID.Text))
            {
                csMsg.ShowWarning("用户名不能为空", false);
                return;
            }
            if (string.IsNullOrEmpty(cbDatabase.Text))
            {
                csMsg.ShowWarning("数据库不能为空", false);
                return;
            }
            string connStr = "Data Source=" + cbServer.Text.Trim() + ";User ID=" + tbUserID.Text.Trim() + ";Password=" + tbPassword.Text.Trim() + ";Initial Catalog=" + cbDatabase.Text.Trim();

            string s = "";
            switch (cbSoft.SelectedIndex)
            {
                case 1:
                    s = "A";
                    break;
                case 2:
                    s = "G";
                    break;
                case 3:
                    s = "C";
                    break;
            }
            string sql = "select maccode from sys_macinfo where mactype='{0}'";
            sql = string.Format(sql, s);
            DataTable dt; csSql.ExecuteSelect(sql, connStr, out dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    cbMac.Items.Add(row["maccode"].ToString());
                }
            }
            cbMac.Text = Config.Soft.MacCode;
        }

        private void ShowConfig()
        {
            chkShow.IsChecked = Config.Soft.Config == "N" ? true : false;
            cbServer.Text = Config.Soft.Server;
            tbUserID.Text = Config.Soft.UserID;
            tbPassword.Text = Config.Soft.Password;
            cbDatabase.Text = Config.Soft.Database;
            tbHis.Text = Config.Soft.ConnString_His;
            cbSoft.SelectedIndex = int.Parse(Config.Soft.SoftType);
            if (Config.Soft.SoftType == "0")
            {
                tbWin.Visibility = cbWin.Visibility = Visibility.Visible;
                tbMac.Visibility = cbMac.Visibility = Visibility.Hidden;
                tbFunction.Visibility = cbFunction.Visibility = Visibility.Hidden;
                //cbWin.Text = Config.Soft.WindowNo;
            }
            else
            {
                tbWin.Visibility = cbWin.Visibility = Visibility.Hidden;
                tbMac.Visibility = cbMac.Visibility = Visibility.Visible;
                //cbMac.Text = Config.Soft.MacCode;
                if (Config.Soft.SoftType == "2")
                {
                    tbFunction.Visibility = cbFunction.Visibility = Visibility.Hidden;
                }
                else
                {
                    tbFunction.Visibility = cbFunction.Visibility = Visibility.Visible;
                    //cbFunction.Text = Config.Soft.Function;
                }
            }
        }

        private void bt_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
            //Application.Current.Shutdown();
        }

        private void bt_Sure_Click(object sender, RoutedEventArgs e)
        {
            //Dictionary<string, string> dics = new Dictionary<string, string>();

            string s = "Y";
            if ((bool)chkShow.IsChecked)
                s = "N";
            csIni.WriteIni("SYS", "Config", s, Config.file);

            if (string.IsNullOrEmpty(cbServer.Text))
            {
                tcConfig.SelectedIndex = 0;
                csMsg.ShowWarning("服务器不能为空", false);
                return;
            }
            s = cbServer.Text.Trim();
            csIni.WriteIni("SYS", "Server", s, Config.file);

            if (string.IsNullOrEmpty(tbUserID.Text))
            {
                tcConfig.SelectedIndex = 0;
                csMsg.ShowWarning("用户名不能为空", false);
                return;
            }
            s = tbUserID.Text.Trim();
            csIni.WriteIni("SYS", "UserID", s, Config.file);

            s = tbPassword.Text.Trim();
            csIni.WriteIni("SYS", "Password", s, Config.file);

            if (string.IsNullOrEmpty(cbDatabase.Text))
            {
                tcConfig.SelectedIndex = 0;
                csMsg.ShowWarning("数据库不能为空", false);
                return;
            }
            s = cbDatabase.Text.Trim();
            csIni.WriteIni("SYS", "Database", s, Config.file);

            s = cbSoft.SelectedIndex.ToString();
            csIni.WriteIni("SOFT", "SoftType", s, Config.file);
            s = cbWin.Text.Trim();
            csIni.WriteIni("SOFT", "WindowNo", s, Config.file);
            s = cbMac.Text.Trim();
            csIni.WriteIni("SOFT", "MacCode", s, Config.file);
            s = cbFunction.Text.Trim();
            csIni.WriteIni("SOFT", "Function", s, Config.file);
            s = tbHis.Text.Trim();
            csIni.WriteIni("HIS", "ConnString", s, Config.file);

            Config.InitialConfig_Client();
            this.Close();
        }

        private void bt_Test_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbServer.Text))
            {
                csMsg.ShowWarning("服务器不能为空", false);
                return;
            }
            if (string.IsNullOrEmpty(tbUserID.Text))
            {
                csMsg.ShowWarning("用户名不能为空", false);
                return;
            }
            if (string.IsNullOrEmpty(cbDatabase.Text))
            {
                csMsg.ShowWarning("数据库不能为空", false);
                return;
            }
            string connStr = "Data Source=" + cbServer.Text.Trim() + ";User ID=" + tbUserID.Text.Trim() + ";Password=" + tbPassword.Text.Trim() + ";Initial Catalog=" + cbDatabase.Text.Trim();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    csMsg.ShowInfo("测试成功", false);
                }
                catch (Exception ex)
                {
                    csMsg.ShowWarning(ex.Message, false);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowConfig();
        }

        private void cbServer_DropDownOpened(object sender, EventArgs e)
        {
            Cursor = Cursors.Wait;
            SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
            DataTable dtServer = instance.GetDataSources();
            cbServer.Items.Clear();
            foreach (DataRow row in dtServer.Rows)
            {
                cbServer.Items.Add(row["servername"].ToString());
            }
            Cursor = null;
        }

        private void cbDatabase_DropDownOpened(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbServer.Text))
            {
                csMsg.ShowWarning("服务器不能为空", false);
                return;
            }
            if (string.IsNullOrEmpty(tbUserID.Text))
            {
                csMsg.ShowWarning("用户名不能为空", false);
                return;
            }

            Cursor = Cursors.Wait;
            string connStr = "Data Source=" + cbServer.Text.Trim() + ";User ID=" + tbUserID.Text.Trim() + ";Password=" + tbPassword.Text.Trim() + ";Initial Catalog=master";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "select name from sys.databases where name like 'pas%'";
                SqlDataAdapter sda = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                try
                {
                    sda.Fill(dt);
                }
                catch (Exception ex)
                {
                    csMsg.ShowWarning(ex.Message, false);
                }

                cbDatabase.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    cbDatabase.Items.Add(row["name"].ToString());
                }
            }
            Cursor = null;
        }

        private void cbSoft_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string s = cbSoft.SelectedIndex.ToString();
            if (s == "0")
            {
                tbWin.Visibility = cbWin.Visibility = Visibility.Visible;
                tbMac.Visibility = cbMac.Visibility = Visibility.Hidden;
                tbFunction.Visibility = cbFunction.Visibility = Visibility.Hidden; 
                ShowWindows();
            }
            else
            {
                tbWin.Visibility = cbWin.Visibility = Visibility.Hidden;
                tbMac.Visibility = cbMac.Visibility = Visibility.Visible; 
                ShowMacs();
                
                if (s == "2")
                {
                    tbFunction.Visibility = cbFunction.Visibility = Visibility.Hidden;
                }
                else
                {
                    tbFunction.Visibility = cbFunction.Visibility = Visibility.Visible;
                    cbFunction.Text = Config.Soft.Function;
                }
            }
        }

        private void cbWin_DropDownOpened(object sender, EventArgs e)
        {
            //ShowWindows();
        }

        private void cbMac_DropDownOpened(object sender, EventArgs e)
        {
            //ShowMacs();
        }
    }
}
