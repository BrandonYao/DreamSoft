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
using System.Data;

namespace DreamSoft
{
    /// <summary>
    /// WinPassword.xaml 的交互逻辑
    /// </summary>
    public partial class WinPassword : Window
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public WinPassword()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbCode.Text = Config.Soft.UserCode;
            tbName.Text = Config.Soft.UserName;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string code = tbCode.Text.Trim();
            string pass = tbNow.Password.Trim();
            if (string.IsNullOrEmpty(pass))
            {
                csMsg.ShowWarning("请输入当前密码", false);
                return;
            }
            DataTable dtUser;
            string sql = "select * from sys_user where usercode='" + code + "' and password='" + pass + "'";
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtUser);
            if (dtUser != null && dtUser.Rows.Count > 0)
            {
                string pass1 = tbNew.Password.Trim();
                string pass2 = tbReNew.Password.Trim();
                if (string.IsNullOrEmpty(pass1))
                {
                    csMsg.ShowWarning("新密码不能为空", false);
                    return;
                } 
                if (string.IsNullOrEmpty(pass2))
                {
                    csMsg.ShowWarning("确认新密码不能为空", false);
                    return;
                }
                if (pass1 != pass2)
                {
                    csMsg.ShowWarning("两次输入的新密码不同", false);
                    return;
                }
                sql = "update sys_user set password='{0}' where usercode='{1}'";
                sql = string.Format(sql, pass1, code);
                if(csSql.ExecuteSql(sql, Config.Soft.ConnString))
                    csMsg.ShowInfo("操作成功", false);
            }
            else
                csMsg.ShowWarning("当前密码不正确", false);
        }
    }
}
