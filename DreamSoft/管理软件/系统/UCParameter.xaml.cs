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
    /// UCParameter.xaml 的交互逻辑
    /// </summary>
    public partial class UCParameter : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCParameter()
        {
            InitializeComponent();
        }

        private void ShowParaGroup()
        {
            spGroup.Children.Clear();

            //RadioButton rb_Sys = new RadioButton();
            //rb_Sys.Margin = new Thickness(20, 5, 20, 5); rb_Sys.FontSize = 16; rb_Sys.FontWeight = FontWeights.Bold;
            //rb_Sys.Content = "系统"; rb_Sys.Uid = "Sys";
            //rb_Sys.Foreground = new SolidColorBrush(Colors.Green);
            //rb_Sys.Checked += new RoutedEventHandler(rb_Checked);
            //spGroup.Children.Add(rb_Sys);

            //string sql = "select maccode,(maccode+'-'+macname) as name,mactype from sys_macinfo";
            string sql = "select distinct paragroup from sys_parameter order by paragroup";
            DataTable dt;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
            foreach (DataRow row in dt.Rows)
            {
                RadioButton rb = new RadioButton();
                rb.Margin=new Thickness(20,5,20,5); rb.FontSize=16; rb.FontWeight=FontWeights.Bold;
                rb.Content = rb.Uid = row["paragroup"].ToString().Trim();
                rb.Foreground = new SolidColorBrush(Colors.Black);
                //switch(row["mactype"].ToString().Trim())
                //{
                //    case "A":
                //        rb.Foreground = new SolidColorBrush(Colors.Crimson);
                //        break;
                //    case "G":
                //        rb.Foreground = new SolidColorBrush(Colors.RoyalBlue);
                //        break;
                //}
                rb.Checked += new RoutedEventHandler(rb_Checked);
                spGroup.Children.Add(rb);
            }
        }

        void rb_Checked(object sender, RoutedEventArgs e)
        {
            foreach (RadioButton rb in spGroup.Children)
            {
                if ((bool)rb.IsChecked)
                {
                    ShowParameter(rb.Uid);
                    break;
                }
            }
        }

        public class Parameter
        {
            public string ParaCode { get; set; }
            public string ParaName { get; set; }
            public string ParaValue { get; set; }
        }
        private void ShowParameter(string group)
        {
            Cursor = Cursors.Wait;

            List<Parameter> ps = new List<Parameter>();
            string sql = "select paracode,paraname,paravalue from sys_parameter where paragroup='{0}'";
            sql = string.Format(sql, group);
            DataTable dtSetting;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtSetting);
            if (dtSetting != null && dtSetting.Rows.Count > 0)
            {
                foreach (DataRow row in dtSetting.Rows)
                {
                    ps.Add(new Parameter()
                    {
                        ParaCode = row["paracode"].ToString().Trim(),
                        ParaName = row["paraname"].ToString().Trim(),
                        ParaValue = row["paravalue"].ToString().Trim()
                    });
                }
            }
            dgParameter.ItemsSource = ps;

            Cursor = null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowParaGroup();
        }

        private void dgParameter_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int i = e.Row.GetIndex();
            Parameter p = dgParameter.Items[i] as Parameter;
            string code = p.ParaCode;
            TextBox tb = e.EditingElement as TextBox;
            string value = tb.Text.Trim();

            string group = "Sys";
            foreach (RadioButton rb in spGroup.Children)
            {
                if ((bool)rb.IsChecked)
                {
                    group = rb.Uid;
                    break;
                }
            }
            string sql = "update sys_parameter set paravalue='{0}' where paragroup='{1}' and paracode='{2}'";
            sql = string.Format(sql, value, group, code);
            if (csSql.ExecuteSql(sql, Config.Soft.ConnString))
                csMsg.ShowInfo("操作成功", false);
            else csMsg.ShowWarning("操作失败", false);
        }

        private void dgParameter_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!csMsg.ShowQuestion("确定要修改此配置项吗？", false))
            {
                e.Cancel = true;
            }
        }
    }
}
