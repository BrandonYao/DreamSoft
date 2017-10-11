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
using DreamSoft.Class;

namespace DreamSoft
{
    /// <summary>
    /// WinError_PD.xaml 的交互逻辑
    /// </summary>
    public partial class UCError_PD : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCError_PD()
        {
            InitializeComponent();
        }
        public class ErrorPos
        {
            public string PosCode { get; set; }
            public string DrugOnlyCode { get; set; }
            public string DrugName { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
            public string OkNum { get; set; }
            public string ErrorNum { get; set; }
            public string DrugNum { get; set; }
            public string ErrorTime { get; set; }
        }
        //显示记录
        private void ShowRecord()
        {
            string sql = @"select * from view_error where maccode='{0}' and errortype='PE'";
            sql = string.Format(sql, Config.Soft.MacCode);
            DataTable dtError;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtError);
            List<ErrorPos> eps = new List<ErrorPos>();
            if (dtError != null && dtError.Rows.Count > 0)
            {
                foreach (DataRow row in dtError.Rows)
                {
                    eps.Add(new ErrorPos
                    {
                        PosCode = row["poscode"].ToString(),
                        DrugOnlyCode = row["drugonlycode"].ToString(),
                        DrugName = row["drugname"].ToString(),
                        DrugSpec = row["drugspec"].ToString(),
                        DrugFactory = row["drugfactory"].ToString(),
                        OkNum = row["oknum"].ToString(),
                        ErrorNum = row["errornum"].ToString(),
                        DrugNum = row["drugnum"].ToString(),
                        ErrorTime = row["errortime"].ToString()
                    });
                }
            }
            lvPos.ItemsSource = eps;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowRecord();
        }
        //重新盘点
        private void btPD_Click(object sender, RoutedEventArgs e)
        {
            if (lvPos.SelectedItems.Count > 0)
            {
                ErrorPos p = lvPos.SelectedItems[0] as ErrorPos;
                string poscode = p.PosCode;

                int n = PLC_Tcp_AP.PDNum(poscode, string.IsNullOrEmpty(p.DrugOnlyCode));
                tbNum.Text = n.ToString();
            }
        }
        //更新库存
        private void btUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (lvPos.SelectedItems.Count > 0)
            {
                ErrorPos p = lvPos.SelectedItems[0] as ErrorPos;
                if (!string.IsNullOrEmpty(p.DrugOnlyCode))
                {
                    string poscode = p.PosCode;
                    string sql = "update drug_pos set drugnum={0} where maccode='{1}' and poscode='{2}';";
                    sql = string.Format(sql, tbNum.Text, Config.Soft.MacCode, poscode);
                    sql += "delete from sys_error where errortype='PE' and maccode='{0}' and poscode='{1}'";
                    sql = string.Format(sql, Config.Soft.MacCode, poscode);
                    csSql.ExecuteSql(sql, Config.Soft.ConnString);
                    ShowRecord();
                }
            }
        }
        //全部忽略
        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            string sql = "delete from sys_error where errortype='PE' and maccode='{0}'";
            sql = string.Format(sql, Config.Soft.MacCode);
            csSql.ExecuteSql(sql, Config.Soft.ConnString);
            ShowRecord();
        }
    }
}
