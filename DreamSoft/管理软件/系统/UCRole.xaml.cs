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
    /// UCRole.xaml 的交互逻辑
    /// </summary>
    public partial class UCRole : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCRole()
        {
            InitializeComponent();
        }
        public class Role : INotifyPropertyChanged
        {
            private string roleCode;
            public string RoleCode
            {
                get { return roleCode; }
                set
                {
                    roleCode = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("RoleCode"));
                }
            }

            private string roleName;
            public string RoleName
            {
                get { return roleName; }
                set
                {
                    roleName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("RoleName"));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, e);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowRole();
            ShowAllRight();
        }

        private void ShowRole()
        {
            IList<Role> rs = new List<Role>();
            string sql = @"select rolecode,rolename from sys_role";
            DataTable dt;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Role r = new Role()
                    {
                        RoleCode = row["RoleCode"].ToString().Trim(),
                        RoleName = row["RoleName"].ToString().Trim()
                    };
                    rs.Add(r);
                }
            }
            dgRole.ItemsSource = rs;
        }
        private void dgRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRole.SelectedItem != null)
            {
                Role r = dgRole.SelectedItem as Role;
                if (r != null)
                    ShowRight(r.RoleCode);
            }
        }
        private void ShowRight(string roleCode)
        {
            IList<string> rs = new List<string>();
            string sql = @"select * from view_role_right where rolecode='{0}'";
            sql = string.Format(sql, roleCode);
            DataTable dt; csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    rs.Add(row["menucode"].ToString().Trim());
                }
            }
            //遍历treeview
            IList<Model.TreeModel> treeList = tvRight.ItemsSourceData as List<Model.TreeModel>;
            foreach (Model.TreeModel tree in treeList)
            {
                tree.IsChecked = rs.Contains(tree.Id);
                ShowChildRight(tree, rs);
            }
        }
        private void ShowChildRight(Model.TreeModel tree, IList<string> rs)
        {
            foreach(Model.TreeModel child in tree.Children)
            {
                child.IsChecked = rs.Contains(child.Id);
                ShowChildRight(child, rs);
            }
        }

        private void ShowAllRight()
        {
            IList<Model.TreeModel> treeList = new List<Model.TreeModel>();
            string sql = "select menucode,menuname from sys_menu where parentcode='0'";
            DataTable dtRight; csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtRight);
            if (dtRight != null && dtRight.Rows.Count > 0)
            {
                foreach (DataRow row in dtRight.Rows)
                {
                    Model.TreeModel tree = new Model.TreeModel();
                    tree.Id = row["menucode"].ToString().Trim();
                    tree.Name = row["menuname"].ToString().Trim();
                    tree.IsExpanded = true;
                    treeList.Add(tree);
                    AddTreeNode(tree, tree.Id);
                }
            }
            tvRight.ItemsSourceData = treeList;
        }
        private void AddTreeNode(Model.TreeModel tree, string parCode)
        {
            string sql = "select menucode,menuname from sys_menu where parentcode='{0}'";
            sql = string.Format(sql, parCode);
            DataTable dtRight; csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtRight);
            if (dtRight != null && dtRight.Rows.Count > 0)
            {
                foreach (DataRow row in dtRight.Rows)
                {
                    Model.TreeModel child = new Model.TreeModel();
                    child.Id = row["menucode"].ToString().Trim();
                    child.Name = row["menuname"].ToString().Trim();
                    child.IsExpanded = true;
                    child.Parent = tree;
                    tree.Children.Add(child);
                    AddTreeNode(child, child.Id);
                }
            }
        }

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            dgRole.Columns[0].IsReadOnly = false;
            dgRole.CanUserAddRows = true;
        }
        private void dgRole_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int r = e.Row.GetIndex();
            Role p = e.Row.Item as Role;
            string code = p.RoleCode;
            if (string.IsNullOrEmpty(code))
            {
                csMsg.ShowWarning("角色编号不能为空,请先输入编号", false);
            }
            else
            {
                string name = "";
                if (p.RoleName != null) name = p.RoleName;

                string sql = @" if exists (select * from sys_role where rolecode='{0}') update sys_role set rolename='{1}' where rolecode='{0}' 
else insert into sys_role (rolecode,rolename) values ('{0}','{1}')";
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
            string code = b.Tag.ToString();
            if (csMsg.ShowQuestion("确定要删除该角色吗？", false))
            {
                string sql = @"delete from sys_role where rolecode='{0}';delete from sys_role_menu where rolecode='{0}'";
                sql = string.Format(sql, code);
                if (csSql.ExecuteSql(sql, Config.Soft.ConnString))
                    csMsg.ShowInfo("操作成功", false);
                else csMsg.ShowWarning("操作失败", false);
                ShowRole();
            }
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgRole.SelectedItem != null)
            {
                Role r = dgRole.SelectedItem as Role;
                SaveRight(r.RoleCode);
            }
        }
        private void SaveRight(string roleCode)
        {
            //遍历treeview
            IList<Model.TreeModel> treeList = tvRight.ItemsSourceData as List<Model.TreeModel>;
            string sql = "";
            foreach (Model.TreeModel tree in treeList)
            {
                sql += GetRightSql(roleCode, tree.Id, tree.IsChecked);
                SaveChildRight(roleCode,tree, ref sql);
            }
            if (!string.IsNullOrEmpty(sql))
            {
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
            }
        }
        private void SaveChildRight(string roleCode,Model.TreeModel tree, ref string sql)
        {
            foreach (Model.TreeModel child in tree.Children)
            {
                sql += GetRightSql(roleCode, child.Id, child.IsChecked);
                SaveChildRight(roleCode, child, ref sql);
            }
        }
        private string GetRightSql(string roleCode, string menuCode, bool chk)
        {
            string s = "";
            if (chk)
            {
                s = @"if not exists(select *  from sys_role_menu where rolecode='{0}' and menucode='{1}') 
                      insert into sys_role_menu values('{0}','{1}');";
            }
            else
            {
                s = "delete from sys_role_menu where rolecode='{0}' and menucode='{1}';";
            }
            return string.Format(s, roleCode, menuCode);
        }

    }
}
