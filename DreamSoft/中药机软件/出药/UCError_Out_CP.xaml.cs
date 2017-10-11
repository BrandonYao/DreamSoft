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
using System.Threading;

namespace DreamSoft
{
    /// <summary>
    /// WinError_Out.xaml 的交互逻辑
    /// </summary>
    public partial class UCError_Out_CP : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCError_Out_CP()
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
            string sql = @"select se.*,drugname,drugspec,drugfactory,dp.drugnum 
from sys_error se,drug_info di,drug_pos dp 
where se.maccode=dp.maccode and se.poscode=dp.poscode and se.drugonlycode=di.drugonlycode 
and se.maccode='{0}' and errortype='OE'";
            sql = string.Format(sql, Config.Soft.MacCode);
            DataTable dtError; csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtError);
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
        //测试
        private void btTest_Click(object sender, RoutedEventArgs e)
        {
            if (lvPos.SelectedItems.Count > 0)
            {
                ErrorPos p = lvPos.SelectedItems[0] as ErrorPos;
                string poscode = p.PosCode;
                PLC_Tcp_CP.ClearRecordSingle(poscode);
                Thread.Sleep(100);
                PLC_Tcp_CP.DCTMoveDownSingle(poscode);
                Thread.Sleep(1000);
                int n = PLC_Tcp_CP.ReadRecordSingle(poscode);
                if (n == 1)
                {
                    csMsg.ShowInfo("出药成功", true);
                }
                else if (n > 1)
                {
                    csMsg.ShowWarning("多出药", true);
                }
                else
                {
                    csMsg.ShowWarning("未出药", true);
                }
            }
        }
        //启用
        private void btOpen_Click(object sender, RoutedEventArgs e)
        {
            if (lvPos.SelectedItems.Count > 0)
            {
                ErrorPos p = lvPos.SelectedItems[0] as ErrorPos;
                string poscode = p.PosCode;
                string sql = "update drug_pos set errornum=0 where maccode='{0}' and poscode='{1}';";
                sql += "delete from sys_error where maccode='{0}' and poscode='{1}' and errortype='1'";
                sql = string.Format(sql, Config.Soft.MacCode, poscode);
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
                ShowRecord();
            }
        }
        //全部启用
        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            string sql = "update drug_pos set errornum=0 where maccode='{0}';";
            sql += "delete from sys_error where maccode='{0}';";
            sql = string.Format(sql, Config.Soft.MacCode);
            csSql.ExecuteSql(sql, Config.Soft.ConnString);
            ShowRecord();
        }
    }
}
