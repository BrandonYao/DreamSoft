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

namespace DreamSoft
{
    /// <summary>
    /// Win_Login.xaml 的交互逻辑
    /// </summary>
    public partial class WinLogin : Window
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();
        CSHelper.TKey csKey = new CSHelper.TKey();

        public string GetUserName(string code)
        {
            string result = "无此用户";
            string v;
            string sql = "select username from sys_user where usercode='" + code + "'";
            csSql.ExecuteScalar(sql, Config.Soft.ConnString, out v);
            if (!string.IsNullOrEmpty(v))
                result = v;
            return result;
        }

        private void UserLogin()
        {
            string code = tbUserCode.Text.Trim();
            string pass = tbPassword.Password.Trim();
            string sql = "select * from sys_user where usercode='" + code + "'";
            DataTable dtUser;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtUser);
            if (dtUser != null && dtUser.Rows.Count > 0)
            {
                sql = "select * from sys_user where usercode='" + code + "' and password='" + pass + "'";
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtUser);
                if (dtUser != null && dtUser.Rows.Count > 0)
                {
                    Config.Soft.UserCode = code;
                    Config.Soft.UserName = dtUser.Rows[0]["username"].ToString().Trim();
                    new WinMain_Manage().Show();
                    this.Close();
                }
                else
                    csMsg.ShowWarning("账户或密码不正确", false);
            }
            else
                csMsg.ShowWarning("账户不存在", false);
        }

        public WinLogin()
        {
            InitializeComponent();
        }

        private void btExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btLogin_Click(object sender, RoutedEventArgs e)
        {
            UserLogin();
        }

        private void tbUserCode_LostFocus(object sender, RoutedEventArgs e)
        {
            tbUserName.Text = GetUserName(tbUserCode.Text.Trim());
        }

        private void tbUserCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                tbPassword.Focus();
        }

        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UserLogin();
        }

        private void btConfig_Click(object sender, RoutedEventArgs e)
        {
            new WinConfig().Show();
            //Close();
        }
    }
}
