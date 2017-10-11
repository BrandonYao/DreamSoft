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
    /// UCDrug.xaml 的交互逻辑
    /// </summary>
    public partial class UCDrug : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCDrug()
        {
            InitializeComponent();
        }

        public class Drug
        {
            public string DrugOnlyCode { get; set; }
            public string DrugName { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
            public string DrugBarCode { get; set; }
            public string DrugSupCode { get; set; }
            public string Length { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
            public string OutTime { get; set; }
        }
        //添加扫描功能
        private void ShowDrug(string code, bool isBarcode)
        {
            Cursor = Cursors.Wait;

            List<Drug> ds = new List<Drug>();
            string sql = "select * from view_drug ";
            if (isBarcode)
                sql += " where drugbarcode='{0}' or drugsupcode='{0}'";
            else
                sql += " where drugpycode like '%{0}%' or drugaliaspycode like '%{0}%'";
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
                        DrugFactory = row["drugfactory"].ToString(),
                        DrugBarCode = row["drugbarcode"].ToString(),
                        DrugSupCode = row["drugsupcode"].ToString(),
                        Length = row["length"].ToString(),
                        Width = row["width"].ToString(),
                        Height = row["height"].ToString(),
                        OutTime = row["outtime"].ToString()
                    });
                }
            }
            dgDrug.ItemsSource = ds;

            Cursor = null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //ShowDrug("", false);
        }

        private void tbPYCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowDrug(tbPYCode.Text.Trim(), false);
        }

        private void dgDrug_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int r = e.Row.GetIndex();
            Drug p = dgDrug.Items[r] as Drug;
            string code = p.DrugOnlyCode;

            TextBox tb = e.EditingElement as TextBox;
            string value = tb.Text.Trim();

            DataGridTextColumn dgtc = e.Column as DataGridTextColumn;
            Binding b = dgtc.Binding as Binding;
            string col = b.Path.Path;

            string sql = @"if exists(select * from drug_infoannex where drugonlycode='{2}') update drug_infoannex set {0}='{1}' where drugonlycode='{2}' 
else insert into drug_infoannex (drugonlycode,{0}) values ('{2}','{1}')";
            sql = string.Format(sql, col, value, code);
            if (csSql.ExecuteSql(sql, Config.Soft.ConnString))
                csMsg.ShowInfo("操作成功", false);
            else csMsg.ShowWarning("操作失败", false);
        }

        private void dgDrug_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!csMsg.ShowQuestion("确定要修改此配置项吗？", false))
            {
                e.Cancel = true;
            }
        }
    }
}
