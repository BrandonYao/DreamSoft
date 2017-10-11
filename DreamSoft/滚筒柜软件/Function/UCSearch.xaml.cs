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
    /// UCSearch.xaml 的交互逻辑
    /// </summary>
    public partial class UCSearch : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Oracle csOracle = new CSHelper.Oracle();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCSearch()
        {
            InitializeComponent();
        }

        public delegate void SetKey(bool show);
        public static SetKey ShowKey;

        public class Drug
        {
            public string PosCode { get; set; }
            public string DrugOnlyCode { get; set; }
            public string DrugName { get; set; }
            public string DrugPYCode { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
            public string DrugNum { get; set; }
        }

        public void ShowDrug(string code, bool all)
        {
            Cursor = Cursors.Wait;

            List<Drug> drugs = new List<Drug>();
            string sql = @"select * from view_drug_pos where maccode='{0}' and drugpycode like '%{1}%'";
            sql = string.Format(sql, Config.Soft.MacCode, code);
            if (!all)
                sql += " and poscode is not null";

            DataTable dtDrug;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDrug);
            if (dtDrug != null && dtDrug.Rows.Count > 0)
            {
                int c = dtDrug.Rows.Count;
                foreach (DataRow row in dtDrug.Rows)
                {
                    string poscode = row["poscode"].ToString().Trim();
                    drugs.Add(new Drug()
                    {
                        PosCode = poscode,
                        DrugOnlyCode = row["drugonlycode"].ToString().Trim(),
                        DrugName = row["drugname"].ToString().Trim(),
                        DrugPYCode= row["DrugPYCode"].ToString().Trim(),
                        DrugSpec = row["drugspec"].ToString().Trim(),
                        DrugFactory = row["drugfactory"].ToString().Trim(),
                        DrugNum = row["drugnum"].ToString().Trim()
                    });
                }
            }
            lvDrug.ItemsSource = drugs;

            Cursor = null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbNum.Visibility = btConfirm.Visibility = btCancle.Visibility = Visibility.Hidden;
            btAdd.Visibility = btOut.Visibility = Visibility.Visible;

            ShowDrug("", false);
        }

        private void tbCode_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            tbCode.SelectAll();
            ShowKey(true);
            //csKey.Show(300, 300);
        }

        private void tbCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowDrug(tbCode.Text.Trim(), (bool)cbAll.IsChecked);
        }

        private void tbCode_LostFocus(object sender, RoutedEventArgs e)
        {
            ShowKey(false);
            //csKey.Close();
        }

        private void btTurn_Click(object sender, RoutedEventArgs e)
        {
            if (lvDrug.SelectedItem != null)
            {
                Drug d = lvDrug.SelectedItem as Drug;
                //string pos = d.PosCode;
                if (d.PosCode == null || d.PosCode == "")
                {
                    csMsg.ShowWarning("该药品未分配储位", false);
                    return;
                }
                UCAuto.ShowPos(d.PosCode);
                //int unit = int.Parse(pos.Substring(0, 1));
                //int lay = int.Parse(pos.Substring(1, 2));

                //string strcol = "01";
                //if (lay % 2 == 0)
                //{
                //    lay -= 1;
                //    strcol = "02";
                //}
                //strcol += pos.Substring(3, 2);

                //PLC.TurnTo(lay);
                //PLC.LightMutilNum(strcol);
            }
            else
                csMsg.ShowWarning("请选择一个药品", true);

            tbCode.Focus();
            tbCode.SelectAll();
        }

        string Type = "+";
        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            Type = "+";
            btAdd.Visibility = btOut.Visibility = Visibility.Hidden;
            tbNum.Visibility = btConfirm.Visibility = btCancle.Visibility = Visibility.Visible;
        }
        private void btOut_Click(object sender, RoutedEventArgs e)
        {
            Type = "-";
            btAdd.Visibility = btOut.Visibility = Visibility.Hidden;
            tbNum.Visibility = btConfirm.Visibility = btCancle.Visibility = Visibility.Visible;
        }

        private void tbNum_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            tbNum.SelectAll();
            ShowKey(true);
            //csKey.Show(300, 300);
        }

        private void btConfirm_Click(object sender, RoutedEventArgs e)
        {
            string num = tbNum.Text.Trim();
            if (!string.IsNullOrEmpty(num))
            {
                int i;
                if (int.TryParse(num, out i))
                {
                    if (lvDrug.SelectedItem != null)
                    {
                        Drug d = lvDrug.SelectedItem as Drug;

                        string sql = "update drug_pos set drugnum=(drugnum{0}{1}) where maccode='{2}' and poscode='{3}' and drugonlycode='{4}'";
                        sql = string.Format(sql, Type, i, Config.Soft.MacCode, d.PosCode, d.DrugOnlyCode);
                        csSql.ExecuteSql(sql, Config.Soft.ConnString);
                        csMsg.ShowInfo("操作成功", false);
                        tbNum.Visibility = btConfirm.Visibility = btCancle.Visibility = Visibility.Hidden;
                        btAdd.Visibility = btOut.Visibility = Visibility.Visible;
                    }
                    else
                        csMsg.ShowWarning("请选择一个药品", false);
                }
                else
                    csMsg.ShowWarning("请输入正确的数量", false);
            }
            else
                csMsg.ShowWarning("请输入加药或取药数量", false);
        }
        private void btCancle_Click(object sender, RoutedEventArgs e)
        {
            tbNum.Visibility = btConfirm.Visibility = btCancle.Visibility = Visibility.Hidden;
            btAdd.Visibility = btOut.Visibility = Visibility.Visible;
        }

        private void UpdateDrug(string code)
        {
            string sql_His = "select * from view_interface_druginfojz where isenable=1 and drugonlycode like '%{0}%'";
            sql_His = string.Format(sql_His, code);
            DataTable dt_Drug;
            csOracle.ExecuteSelect(sql_His, Config.Soft.ConnString_His, out dt_Drug);
            if (dt_Drug != null)
            {
                if (dt_Drug.Rows.Count > 0)
                {
                    string sql_Dream = "";
                    foreach (DataRow row in dt_Drug.Rows)
                    {
                        string str_Update = ""; string str_Insert_Column = ""; string str_Insert_Value = "";
                        #region "拼接语句"
                        string s = "drugcode";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugname";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugaliasname";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugpycode";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugaliaspycode";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugspec";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugtype";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugpackunit";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugpacknum";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugsplitunit";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugsplitnum";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugfacid";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugfactory";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "remark";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "base_dose";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "dose_unit";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        #endregion
                        string p = @"if exists(select * from drug_info where drugonlycode='{0}') 
update drug_info set drugonlycode='{0}' {1} where drugonlycode='{0}'
else insert into drug_info (drugonlycode{2}) values ('{0}'{3});";
                        sql_Dream += string.Format(p, row["drugonlycode"].ToString().Trim(), str_Update, str_Insert_Column, str_Insert_Value);
                    }
                    if (!string.IsNullOrEmpty(sql_Dream))
                        csSql.ExecuteSql(sql_Dream, Config.Soft.ConnString);
                }
                else
                    csMsg.ShowInfo("无更新数据", false);
            }
            else
                csMsg.ShowWarning("查询出错", false);
        }
        private void btDrug_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            UpdateDrug("");
            Cursor = null;
            //csOracle.OracleIsConnected(Config.Soft.ConnString_HIS);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PLC_SP.LightAllNum(PLC_SP.LightType.Close);
        }
    }
}
