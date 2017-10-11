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
    /// WinPos_GT.xaml 的交互逻辑
    /// </summary>
    public partial class WinPos_GT : Window
    {
        static CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public WinPos_GT(string mac,string poscode, int num)
        {
            InitializeComponent();
            tbMac.Text = mac;
            tbPos.Text = poscode;
            DrugNum = num;
        }
        //储位内药品种类上限
        int DrugNum;

        public class Drug
        {
            public string DrugOnlyCode { get; set; }
            public string DrugName { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
        }

        private void ShowDrug_In()
        {
            List<Drug> ds = new List<Drug>();
            string sql = "select * from view_pos_drug_sp where maccode='{0}' and poscode='{1}'";
            sql = string.Format(sql, tbMac.Text, tbPos.Text);

            DataTable dtDrug = new DataTable();
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDrug);
            if (dtDrug != null && dtDrug.Rows.Count > 0)
            {
                foreach (DataRow row in dtDrug.Rows)
                {
                    ds.Add(new Drug()
                    {
                        DrugOnlyCode = row["drugonlycode"].ToString(),
                        DrugName = row["drugname"].ToString(),
                        DrugSpec = row["drugspec"].ToString(),
                        DrugFactory = row["drugfactory"].ToString()
                    });
                }
            }
            dgDrug_In.ItemsSource = ds;
        }
        private void ShowDrug_Out(string code)
        {
            List<Drug> ds = new List<Drug>();
            string sql = @"select * from drug_info where drugpycode like '%{0}%' and drugonlycode not in( select distinct drugonlycode from drug_pos )";
            sql = string.Format(sql, code);

            DataTable dtDrug = new DataTable();
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDrug);
            if (dtDrug != null && dtDrug.Rows.Count > 0)
            {
                foreach (DataRow row in dtDrug.Rows)
                {
                    ds.Add(new Drug()
                    {
                        DrugOnlyCode = row["drugonlycode"].ToString(),
                        DrugName = row["drugname"].ToString(),
                        DrugSpec = row["drugspec"].ToString(),
                        DrugFactory = row["drugfactory"].ToString()
                    });
                }
            }
            dgDrug_Out.ItemsSource = ds;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowDrug_In();
            //ShowDrug_Out("");
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDrug_In.SelectedItem != null)
            {
                if (csMsg.ShowQuestion("确定要移除此药品吗？", false))
                {
                    Drug d = dgDrug_In.SelectedItem as Drug;
                    string s = "delete from drug_pos where maccode='{0}' and poscode='{1}' and drugonlycode='{2}';";
                    string sql = string.Format(s, tbMac.Text, tbPos.Text, d.DrugOnlyCode);
                    csSql.ExecuteSql(sql, Config.Soft.ConnString);
                    ShowDrug_In();
                    ShowDrug_Out(tbPYCode.Text.Trim());
                }
            }
        }
        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgDrug_In.Items.Count >= DrugNum)
            {
                csMsg.ShowWarning("该储位存放药品数量已达到上限", false);
                return;
            }
            if (dgDrug_Out.SelectedItem != null)
            {
                Drug d = dgDrug_Out.SelectedItem as Drug;
                string s = "insert into drug_pos (maccode,poscode,drugonlycode,drugnum) values ('{0}','{1}','{2}',0);";
                string sql = string.Format(s, tbMac.Text, tbPos.Text, d.DrugOnlyCode);
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
                ShowDrug_In();
                ShowDrug_Out(tbPYCode.Text.Trim());
            }
        }


        private void tbPYCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowDrug_Out(tbPYCode.Text.Trim());
        }
    }
}
