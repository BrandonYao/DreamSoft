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
using System.Collections.ObjectModel;

namespace DreamSoft
{
    /// <summary>
    /// UCPos_Set.xaml 的交互逻辑
    /// </summary>
    public partial class UCPos_Set_KF : UserControl
    {
        static CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCPos_Set_KF()
        {
            InitializeComponent();
        }

        public class Mac
        {
            public string MacCode { get; set; }
            public string Name { get; set; }
        }
        private void ShowMac()
        {
            List<Mac> ms = new List<Mac>();
            string sql = "select MacCode,(maccode+'-'+macname) as Name from sys_macinfo where mactype='A'";
            DataTable dtMac;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtMac);
            foreach (DataRow row in dtMac.Rows)
            {
                cbMac.ItemsSource = dtMac.AsDataView();
                cbMac.SelectedValuePath = "MacCode";
                cbMac.DisplayMemberPath = "Name";
            }
            if (cbMac.HasItems)
                cbMac.SelectedIndex = 0;
        }

        public class Drug
        {
            public string DrugOnlyCode { get; set; }
            public string DrugName { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
            public string Length { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
            public string Unit { get; set; }
        }
        private void ShowDrug(string code, bool Barcode)
        {
            Cursor = Cursors.Wait;

            List<Drug> ds = new List<Drug>();
            string sql = "select * from view_drug ";
            if (Barcode)
                sql += " where drugbarcode='{0}' or drugsupcode='{0}'";
            else
                sql += "where drugpycode like '%{0}%' or drugaliaspycode like '%{0}%'";
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
                        Length = row["length"].ToString(),
                        Width = row["width"].ToString(),
                        Height = row["height"].ToString(),
                        Unit = row["drugpackunit"].ToString(),
                    });
                }
            }
            dgDrug.ItemsSource = ds;

            Cursor = null;
        }

        public class Pos:INotifyPropertyChanged
        {
            private bool isChecked;
            public bool IsChecked
            {
                get { return isChecked; }
                set
                {
                    isChecked = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsChecked"));
                }
            }

            public string MacCode { get; set; }
            public string PosCode { get; set; }
            public string Length { get; set; }
            public string MaxWidth { get; set; }
            public string MinWidth { get; set; }
            public string Height { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            public void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, e);
            }
        }

        private void ShowPos(string code)
        {
            Cursor = Cursors.Wait;

            List<Pos> ps = new List<Pos>();
            string sql = "select maccode,poscode from drug_pos where drugonlycode='{0}' order by maccode,poscode";
            sql = string.Format(sql, code);
            DataTable dtPos;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPos);
            if (dtPos != null && dtPos.Rows.Count > 0)
            {
                foreach (DataRow row in dtPos.Rows)
                {
                    ps.Add(new Pos()
                    {
                        IsChecked = false,
                        MacCode = row["maccode"].ToString().Trim(),
                        PosCode = row["poscode"].ToString().Trim()
                    });
                }
            }
            dgPos.ItemsSource = ps;

            Cursor = null;
        }
        private void ShowPosEmpty(string maccode, float width, float height)
        {
            Cursor = Cursors.Wait;

            List<Pos> ps = new List<Pos>();
            string sql = @"select poscode,posmaxwidth,posminwidth,posheight,poslength from view_pos_drug_ap
            where maccode='{0}' and posminwidth<={1} and posmaxwidth>=({1}+{2}) and posheight>=({3}+{4}) and useflag=1 and drugonlycode is null order by posmaxwidth,posheight,poscode";
            sql = string.Format(sql, maccode, width, Config.Sys.WidthSpan, height, Config.Sys.HeightSpan);
            DataTable dtPos;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPos);
            if (dtPos != null && dtPos.Rows.Count > 0)
            {
                foreach (DataRow row in dtPos.Rows)
                {
                    ps.Add(new Pos()
                    {
                        IsChecked = false,
                        PosCode = row["poscode"].ToString().Trim(),
                        MaxWidth = row["posmaxwidth"].ToString().Trim(),
                        MinWidth = row["posminwidth"].ToString().Trim(),
                        Height = row["posheight"].ToString().Trim(),
                        Length = row["poslength"].ToString()
                    });
                }
            }
            dgPos_Empty.ItemsSource = ps;

            Cursor = null;
        }

        private void RefreshPosEmpty()
        {
            if (dgDrug.SelectedItem != null)
            {
                Drug d = dgDrug.SelectedItem as Drug;
                if (cbMac.SelectedValue != null)
                {
                    string mac = cbMac.SelectedValue.ToString().Trim();
                    float width = 0f;
                    float height = 0f;
                    if (!string.IsNullOrEmpty(d.Width))
                        width = float.Parse(d.Width);
                    if (!string.IsNullOrEmpty(d.Height))
                        height = float.Parse(d.Height);
                    if (width > 0 && height > 0)
                    {
                        ShowPosEmpty(mac, width, height);
                    }
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowMac();
            //ShowDrug("", false);
        }

        private void tbPYCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowDrug(tbPYCode.Text.Trim(), false);
        }

        private void dgDrug_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgDrug.SelectedItem != null)
            {
                Drug d = dgDrug.SelectedItem as Drug;
                ShowPos(d.DrugOnlyCode);

                RefreshPosEmpty();
            }
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDrug.SelectedItem != null)
            {
                Drug d = dgDrug.SelectedItem as Drug;
                bool fresh_Pos = false; bool fresh_Empty = false; string mac = cbMac.Text.Trim(); 
                List<Pos> ps = dgPos.ItemsSource as List<Pos>;
                foreach (Pos p in ps)
                {
                    if (p.IsChecked)
                    {
                        fresh_Pos = true;
                        if (p.MacCode == mac) fresh_Empty = true;
                        string s = "delete from drug_pos where maccode='{0}' and poscode='{1}' and drugonlycode='{2}';";
                        string sql = string.Format(s, p.MacCode, p.PosCode, d.DrugOnlyCode);
                        csSql.ExecuteSql(sql, Config.Soft.ConnString);
                    }
                } if (fresh_Pos) ShowPos(d.DrugOnlyCode);
                if (fresh_Empty)
                {
                    RefreshPosEmpty();
                }
            }
        }

        private void cbMac_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshPosEmpty();
        }

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgDrug.SelectedItem != null)
            {
                Drug d = dgDrug.SelectedItem as Drug;
                if (!string.IsNullOrEmpty(d.Length))
                {
                    string sql = "";
                    List<Pos> ps = dgPos_Empty.ItemsSource as List<Pos>;
                    foreach (Pos p in ps)
                    {
                        if (p.IsChecked)
                        {
                            int posLength = int.Parse(p.Length);
                            double maxNum = Math.Floor(posLength / float.Parse(d.Length));
                            string s = "insert into drug_pos (maccode,poscode,drugonlycode,drugnum,drugunit,drugnummax,drugnummin) values ('{0}','{1}','{2}',0,'{3}',{4},1);";
                            sql += string.Format(s, cbMac.SelectedValue.ToString().Trim(), p.PosCode, d.DrugOnlyCode, d.Unit, maxNum);
                        }
                    }
                    if (!string.IsNullOrEmpty(sql))
                    {
                        csSql.ExecuteSql(sql, Config.Soft.ConnString);
                        ShowPos(d.DrugOnlyCode);
                        RefreshPosEmpty();
                    }
                }
            }
        }
    }
}
