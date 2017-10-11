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
using System.Globalization;

namespace DreamSoft
{
    /// <summary>
    /// WinOut_Manual.xaml 的交互逻辑
    /// </summary>
    public partial class UCOut_Manual_CP : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();
        //CSHelper.TKey csKey = new CSHelper.TKey();

        public delegate void SetKey(bool show);
        public static SetKey ShowKey;

        public UCOut_Manual_CP()
        {
            InitializeComponent();
        }

        string color_Ok = Colors.LimeGreen.ToString();
        string color_Error = Colors.LightSalmon.ToString();
        string color_Stop = Colors.Crimson.ToString();

        public class Drug
        {
            public string DrugOnlyCode { get; set; }
            public string DrugName { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
            public string PosCode { get; set; }
            public string DrugNum { get; set; }
            public int ErrorNum { get; set; }
            public string BackColor { get; set; }
        }
        private void ShowDrug(string code)
        {
            string sql = @"select dp.drugonlycode,drugname,drugaliasname,drugspec,drugfactory,poscode,drugnum,errornum 
from drug_info di, drug_pos dp 
where di.drugonlycode=dp.drugonlycode and maccode='{0}' and (drugpycode like '%{1}%' or drugaliaspycode like '%{1}%') order by di.drugonlycode";
            sql = string.Format(sql, Config.Soft.MacCode, code);
            DataTable dtDrug;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDrug);

            //DataTable dt = dtDrug.DefaultView.ToTable();
            //dt.Clear();

            List<Drug> ds = null; 
            if (dtDrug != null && dtDrug.Rows.Count > 0)
            {
                ds = new List<Drug>();
                foreach (DataRow row in dtDrug.Rows)
                {
                    Drug d = new Drug
                    {
                        DrugOnlyCode = row["drugonlycode"].ToString(),
                        DrugName = row["drugname"].ToString(),
                        DrugSpec = row["drugspec"].ToString(),
                        DrugFactory = row["drugfactory"].ToString(),
                        PosCode = row["poscode"].ToString(),
                        DrugNum = row["drugnum"].ToString(),
                        ErrorNum = int.Parse(row["errornum"].ToString())
                    };
                    if (d.ErrorNum == 0)
                        d.BackColor = color_Ok;
                    else if (d.ErrorNum < 3)
                        d.BackColor = color_Error;
                    else
                        d.BackColor = color_Stop;
                    ds.Add(d);
                }
            }
            lvDrug.ItemsSource = ds;
        }
        public class OutPos
        {
            public string PosCode { get; set; }
            public int Num { get; set; }
        }

        private void AddOut(string drugPos, int num)
        {
            OutPos[] os = lvOut.ItemsSource as OutPos[];
            OutPos[] ops = null;
            if (os != null)
            {
                foreach (OutPos op in os)
                {
                    if (op.PosCode == drugPos)
                    {
                        op.Num += num;
                        ops = new OutPos[os.Length];
                        os.CopyTo(ops, 0);
                        lvOut.ItemsSource = ops;
                        return;
                    }
                }
            }

            if (os == null)
            {
                ops = new OutPos[1];
                ops[0] = new OutPos { PosCode = drugPos, Num = num };
            }
            else
            {
                ops = new OutPos[os.Length + 1];
                os.CopyTo(ops, 0);
                ops[os.Length] = new OutPos { PosCode = drugPos, Num = num };
            }
            lvOut.ItemsSource = ops;
        }

        private void CancleOut(string drugPos, int num)
        {
            OutPos[] os = lvOut.ItemsSource as OutPos[];
            OutPos[] ops = null;
            foreach (OutPos op in os)
            {
                if (op.PosCode == drugPos)
                {
                    op.Num -= num;
                    if (op.Num <= 0)
                    {
                        List<OutPos> lop = os.ToList();
                        lop.Remove(op);
                        ops = lop.ToArray();
                    }
                    else
                    {
                        ops = new OutPos[os.Length];
                        os.CopyTo(ops, 0);
                    }
                    lvOut.ItemsSource = ops;
                    return;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowDrug("");
        }

        private void tbCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowDrug(tbCode.Text.Trim());
        }

        private void lvDrug_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ShowKey(false);
            //csKey.Close();

            if (lvDrug.SelectedItem != null)
            {
                Drug d = lvDrug.SelectedItem as Drug;
                AddOut(d.PosCode, 1);
            }
        }

        private void lvOut_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lvOut.SelectedItem != null)
            {
                OutPos d = lvOut.SelectedItem as OutPos;
                CancleOut(d.PosCode, 1);
            }
        }
        //出药
        private void btOut_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            if (lvOut.Items.Count > 0)
            {
                PLC_Tcp_AP.MovePos type = PLC_Tcp_AP.MovePos.Top;
                switch (cbOutPosition.SelectedIndex)
                {
                    case 1:
                        type = PLC_Tcp_AP.MovePos.Up;
                        break;
                    case 2:
                        type = PLC_Tcp_AP.MovePos.Down;
                        break;
                }
                DataTable dtTemp = new DataTable();
                dtTemp.Columns.Add("PosCode");
                dtTemp.Columns.Add("Num");
                OutPos[] ops = lvOut.ItemsSource as OutPos[];
                foreach (OutPos op in ops)
                {
                    DataRow row = dtTemp.NewRow();
                    row[0] = op.PosCode;
                    row[1] = op.Num;
                    dtTemp.Rows.Add(row);
                }
                dtTemp.DefaultView.Sort = "PosCode";
                DataTable dtOut = dtTemp.DefaultView.ToTable();

                OutDrug_CP od = new OutDrug_CP(type, dtOut, false, "", 0, null);
                od.GoOut();

                Thread.Sleep(3000);
                while (OutDrug_CP.IsOut)
                {
                    Thread.Sleep(500);
                }
                //lvOut.ItemsSource = null;
                ShowDrug(tbCode.Text.Trim());
                Cursor = null;
            }
        }

        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            lvOut.ItemsSource = null;
        }

        private void tbCode_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ShowKey(true);
            //csKey.Show(400, 200);
        }
    }
}
