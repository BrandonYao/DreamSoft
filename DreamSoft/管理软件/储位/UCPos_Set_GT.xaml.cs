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
    /// UCPos_Set_GT.xaml 的交互逻辑
    /// </summary>
    public partial class UCPos_Set_GT : UserControl
    {
        static CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCPos_Set_GT()
        {
            InitializeComponent();
        }

        private void InitialGrid(int row, int col)
        {
            grid_Pos.RowDefinitions.Clear();
            grid_Pos.ColumnDefinitions.Clear();
            for (int i = 0; i <= row; i++)
            {
                RowDefinition rd = new RowDefinition();
                if(i==0)
                    rd.Height = new GridLength(30);
                else
                    rd.MinHeight = 40;
                if (grid_Pos.RowDefinitions.Count < row + 1)
                    grid_Pos.RowDefinitions.Add(rd);
            } 
            
            for (int i = 0; i <= col; i++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                if (i == 0)
                    cd.Width = new GridLength(30);
                else
                    cd.MinWidth = 150;
                if (grid_Pos.ColumnDefinitions.Count < col + 1)
                    grid_Pos.ColumnDefinitions.Add(cd);
            }
            InitialHeader();
        }

        private void InitialHeader()
        {
            int r = grid_Pos.RowDefinitions.Count;
            for (int i = 1; i < r; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = "第" + i.ToString().PadLeft(2, '0') + "行";
                tb.Style = (Style)TryFindResource("RowHeaderStyle");
                grid_Pos.Children.Add(tb);
                Grid.SetRow(tb, r - i);
                Grid.SetColumn(tb, 0);
            }

            int c = grid_Pos.ColumnDefinitions.Count;
            for (int i = 1; i < c; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = "第" + i.ToString().PadLeft(2, '0') + "列";
                tb.Style = (Style)TryFindResource("ColumnHeaderStyle");
                grid_Pos.Children.Add(tb);
                Grid.SetRow(tb, 0);
                Grid.SetColumn(tb, i);
            }
        }

        private void ShowMac()
        {
            cbMac.Items.Clear();
            string sql = "select MacCode,(maccode+'-'+macname) as Name from sys_macinfo where mactype='G'";
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

        private void InitialPos()
        {
            grid_Pos.Children.Clear();
            //InitialHeader();
            if (cbMac.SelectedItem != null && cbUnit.SelectedItem != null)
            {
                string mac = cbMac.SelectedValue.ToString().Trim();
                string unit = cbUnit.SelectedValue.ToString().Trim();

                Dictionary<string, string> dics = Config.ReadConfig(mac);
                if (dics.Count == 0) return;
                int count_Unit = int.Parse(dics["Count_Unit"]);
                int count_Lay = int.Parse(dics["Count_Lay"]);
                int count_Col = int.Parse(dics["Count_Col"]);
                InitialGrid(count_Lay, count_Col);

                string sql = "select maccode,poscode,drugonlycode,drugname from view_pos_drug_sp where maccode='{0}' and substring(poscode,1,1)='{1}'";
                sql = string.Format(sql, mac, unit);
                DataTable dtPos;
                csSql.ExecuteSelect(sql,Config.Soft.ConnString,out dtPos);
                if (dtPos != null)
                {
                    for (int r = count_Lay; r > 0; r--)
                    {
                        for (int c = 1; c <= count_Col; c++)
                        {
                            string poscode = "1" + r.ToString().PadLeft(2, '0') + c.ToString().PadLeft(2, '0');
                            DataRow[] rows = dtPos.Select("poscode='" + poscode + "'");

                            Border b = new Border();
                            b.Uid = poscode;
                            b.BorderThickness = new Thickness(1);
                            b.BorderBrush = new SolidColorBrush(Colors.Black);
                            b.CornerRadius = new CornerRadius(5);
                            b.PreviewMouseDown += new MouseButtonEventHandler(b_PreviewMouseDown);

                            ShowBorder(b, rows);

                            grid_Pos.Children.Add(b);
                            Grid.SetRow(b, count_Lay + 1 - r);
                            Grid.SetColumn(b, c);
                        }
                    }
                }
            }
        }

        void b_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Border b = sender as Border;
            new WinPos_GT(cbMac.SelectedValue.ToString(), b.Uid, 2).ShowDialog();
            //InitialPos();
            b.Child = null;
            string sql = "select maccode,poscode,drugonlycode,drugname from view_pos_drug_sp where maccode='{0}' and poscode='{1}'";
            sql = string.Format(sql, cbMac.SelectedValue.ToString(), b.Uid);
            DataTable dtPos;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPos);
            if (dtPos != null && dtPos.Rows.Count > 0)
            {
                DataRow[] rows = dtPos.Select();
                ShowBorder(b, rows);
            }
            else
            {
                b.Background = new SolidColorBrush(Colors.LightGray);
            }
        }
        private void ShowBorder(Border b, DataRow[] rows)
        {
            if (rows.Length > 0)
            {
                b.Background = new SolidColorBrush(Colors.LightCoral);
                Grid gd = new Grid();
                for (int i = 0; i < rows.Length; i++)
                {
                    gd.RowDefinitions.Add(new RowDefinition());
                    TextBlock tb = new TextBlock();
                    tb.FontSize = 12; tb.FontWeight = FontWeights.Normal;
                    tb.HorizontalAlignment = HorizontalAlignment.Center; tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.Text = rows[i]["drugname"].ToString();
                    gd.Children.Add(tb);
                    Grid.SetRow(tb, i);
                }
                b.Child = gd;
            }
            else
            {
                b.Background = new SolidColorBrush(Colors.LightGray);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //InitialGrid(20, 6);
            //InitialHeader();
            //InitialPos();
            ShowMac();
        }

        private void btBig_Click(object sender, RoutedEventArgs e)
        {
            Zomm(1.2);
        }
        private void btSmall_Click(object sender, RoutedEventArgs e)
        {
            Zomm(0.9);
        }
        private void Zomm(double f)
        {
            vb.Width = vb.ActualWidth * f;
            vb.Height = vb.ActualHeight * f;
        }

        private void cbMac_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbUnit.Items.Clear();
            if (cbMac.SelectedValue != null)
            {
                string mac = cbMac.SelectedValue.ToString().Trim();
                Dictionary<string, string> dics = Config.ReadConfig(mac);
                if (dics.Count == 0) return;
                int count_Unit = int.Parse(dics["Count_Unit"]);
                for (int i = 1; i <= count_Unit; i++)
                {
                    cbUnit.Items.Add(i.ToString());
                }
                if (cbUnit.HasItems)
                    cbUnit.SelectedIndex = 0;
            }
            InitialPos();
        }
        private void cbUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitialPos();
        }

        private void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            InitialPos();
        }
    }
}
