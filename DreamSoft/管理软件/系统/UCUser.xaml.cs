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
    /// UCUser.xaml 的交互逻辑
    /// </summary>
    public partial class UCUser : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public List<string> Roles
        {
            get { return roles; }
            set { roles = value; }
        }
        private List<string> roles = new List<string>();

        public class User : INotifyPropertyChanged
        {
            private string userCode;
            public string UserCode
            {
                get { return userCode; }
                set
                {
                    userCode = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("UserCode"));
                }
            }

            private string userName;
            public string UserName
            {
                get { return userName; }
                set
                {
                    userName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("UserName"));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, e);
            }
        }

        public UCUser()
        {
            InitializeComponent();

            DataTable dtName;
            string sql = "select rolename from sys_role order by rolecode";
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtName);
            if (dtName != null)
            {
                foreach (DataRow row in dtName.Rows)
                {
                    roles.Add(row[0].ToString().Trim());
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowUser();
            ShowAllRole();
        }

        private void ShowUser()
        {
            List<User> us = new List<User>();
            string sql = @"select usercode,username from sys_user order by usercode";
            DataTable dtWindow;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtWindow);
            if (dtWindow != null && dtWindow.Rows.Count > 0)
            {
                foreach (DataRow row in dtWindow.Rows)
                {
                    User u = new User()
                    {
                        UserCode = row["usercode"].ToString().Trim(),
                        UserName = row["username"].ToString().Trim()
                    };
                    us.Add(u);
                }
            }
            dgUser.ItemsSource = us;
        }
        private void dgUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUser.SelectedItem != null)
            {
                User r = dgUser.SelectedItem as User;
                if (r != null)
                    ShowRole(r.UserCode);
            }
        }
        private void ShowRole(string userCode)
        {
            IList<string> rs = new List<string>();
            string sql = @"select * from view_user_role where usercode='{0}'";
            sql = string.Format(sql, userCode);
            DataTable dt; csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    rs.Add(row["rolecode"].ToString().Trim());
                }
            }
            //遍历treeview
            IList<Model.TreeModel> treeList = tvRole.ItemsSourceData as List<Model.TreeModel>;
            foreach (Model.TreeModel tree in treeList)
            {
                tree.IsChecked = rs.Contains(tree.Id);
            }
        }

        private void ShowAllRole()
        {
            IList<Model.TreeModel> treeList = new List<Model.TreeModel>();
            string sql = "select rolecode,rolename from sys_role order by rolecode";
            DataTable dtRole; csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtRole);
            if (dtRole != null && dtRole.Rows.Count > 0)
            {
                foreach (DataRow row in dtRole.Rows)
                {
                    Model.TreeModel tree = new Model.TreeModel();
                    tree.Id = row["rolecode"].ToString().Trim();
                    tree.Name = row["rolename"].ToString().Trim();
                    tree.IsExpanded = true;
                    treeList.Add(tree);
                }
            }
            tvRole.ItemsSourceData = treeList;
        }

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            dgUser.Columns[0].IsReadOnly = false;
            dgUser.CanUserAddRows = true;
        }
        private void dgUser_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int r = e.Row.GetIndex();
            User p = e.Row.Item as User;
            string code = p.UserCode;
            if (string.IsNullOrEmpty(code))
            {
                csMsg.ShowWarning("用户账号不能为空,请先输入用户账号", false);
            }
            else
            {
                string name = "";
                if (p.UserName != null) name = p.UserName;

                string sql = @" if exists (select * from sys_user where usercode='{0}') update sys_user set username='{1}' where usercode='{0}' 
else insert into sys_user (usercode,username) values ('{0}','{1}')";
                sql = string.Format(sql, code, name);
                if (csSql.ExecuteSql(sql, Config.Soft.ConnString))
                    csMsg.ShowInfo("操作成功", false);
                else csMsg.ShowWarning("操作失败", false);

            }

            //DataGridTextColumn dgtc = e.Column as DataGridTextColumn;
            //Binding b = dgtc.Binding as Binding;
            //string col = b.Path.Path;
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string code = b.Uid;
            if (csMsg.ShowQuestion("确定要删除该用户吗？", false))
            {
                string sql = @"delete from sys_user where usercode='{0}';delete from sys_user_role where usercode='{0}'";
                sql = string.Format(sql, code);
                if (csSql.ExecuteSql(sql, Config.Soft.ConnString))
                    csMsg.ShowInfo("操作成功", false);
                else csMsg.ShowWarning("操作失败", false);
                ShowUser();
            }
        }
        private void btReg_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string code = b.Uid;
            WinZKFinger winF = new WinZKFinger();
            winF.userCode = code; winF.funType = "V";
            winF.ShowDialog();
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgUser.SelectedItem != null)
            {
                User r = dgUser.SelectedItem as User;
                SaveRole(r.UserCode);
            }
        }
        private void SaveRole(string userCode)
        {
            //遍历treeview
            IList<Model.TreeModel> treeList = tvRole.ItemsSourceData as List<Model.TreeModel>;
            string sql = "";
            foreach (Model.TreeModel tree in treeList)
            {
                sql += GetRoleSql(userCode, tree.Id, tree.IsChecked);
            }
            if (!string.IsNullOrEmpty(sql))
            {
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
            }
        }
        private string GetRoleSql(string userCode, string roleCode, bool chk)
        {
            string s = "";
            if (chk)
            {
                s = @"if not exists(select *  from sys_user_role where usercode='{0}' and rolecode='{1}') 
                      insert into sys_user_role values('{0}','{1}');";
            }
            else
            {
                s = "delete from sys_user_role where usercode='{0}' and rolecode='{1}';";
            }
            return string.Format(s, userCode, roleCode);
        }
    }
}
