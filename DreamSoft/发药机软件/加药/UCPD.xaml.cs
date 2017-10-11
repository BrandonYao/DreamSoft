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
using System.Windows.Threading;
using DreamSoft.Class;
using System.ComponentModel;

namespace DreamSoft
{
    /// <summary>
    /// WinPD.xaml 的交互逻辑
    /// </summary>
    public partial class UCPD : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCPD()
        {
            InitializeComponent();
        }

        string color_Ok = Colors.LimeGreen.ToString();
        string color_Error = Colors.Gray.ToString();
        string color_Ying = Colors.LightSalmon.ToString();
        string color_Kui = Colors.Crimson.ToString();

        public class Pos : INotifyPropertyChanged
        {
            public string PosCode { get; set; }
            public string DrugOnlyCode { get; set; }
            public string DrugName { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
            public string DrugNum { get; set; }

            private string pdNum;
            public string PDNum
            {
                get { return pdNum; }
                set
                {
                    pdNum = value; 
                    OnPropertyChanged(new PropertyChangedEventArgs("PDNum"));
                }
            }

            private string backColor;
            public string BackColor
            {
                get { return backColor; }
                set
                {
                    backColor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BackColor"));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, e);
            }
        }
        //显示储位
        private void ShowPos(string beginPos, bool showNull)
        {
            List<Pos> list_Pos = new List<Pos>();
            int beginLayer = int.Parse(beginPos.Substring(1, 2));
            for (int i = 0; beginLayer + i <= Config.Mac_A.Count_Lay; i++)
            {
                string sql = "";
                if (i == 0)
                {
                    sql = @"select * from view_pos_drug_ap where maccode='{0}' and poscode>='{1}' and substring(poscode,2,2)='{2}' ";
                    sql = string.Format(sql, Config.Soft.MacCode, beginPos, beginPos.Substring(1, 2));
                }
                else
                {
                    sql = @"select * from view_pos_drug_ap where maccode='{0}' and substring(poscode,2,2)='{1}' ";
                    sql = string.Format(sql, Config.Soft.MacCode, (beginLayer + i).ToString().PadLeft(2, '0'));
                } 
                if (!showNull)
                    sql += " and dp.drugonlycode is not null ";
                sql += " order by p.poscode ";
                if (i % 2 != 0)
                {
                    sql += " desc";
                }
                DataTable dtPos = new DataTable();
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPos);
                if (dtPos != null && dtPos.Rows.Count > 0)
                {
                    for (int j = 0; j < dtPos.Rows.Count; j++)
                    {
                       list_Pos.Add( new Pos
                        {
                            PosCode = dtPos.Rows[j]["poscode"].ToString(),
                            DrugOnlyCode = dtPos.Rows[j]["drugonlycode"].ToString(),
                            DrugName = dtPos.Rows[j]["drugname"].ToString(),
                            DrugSpec = dtPos.Rows[j]["drugspec"].ToString(),
                            DrugFactory = dtPos.Rows[j]["drugfactory"].ToString(),
                            DrugNum = dtPos.Rows[j]["drugnum"].ToString(),
                            PDNum = ""
                        });
                    }
                }
            }
            lvPos.ItemsSource = list_Pos.ToArray();
        }

        DispatcherTimer timer_PD = new DispatcherTimer();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer_PD.Interval = TimeSpan.FromSeconds(1);
            timer_PD.Tick += new EventHandler(timerPD_Tick);

            ShowPos("10111", false);
        }
        //刷新
        private void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            string u = ((ComboBoxItem)cbUnitCode.SelectedItem).Content.ToString();
            string l = ((ComboBoxItem)cbLayerCode.SelectedItem).Content.ToString().PadLeft(2, '0');
            string c = ((ComboBoxItem)cbColumnCode.SelectedItem).Content.ToString().PadLeft(2, '0');
            string beginPos = u + l + c;
            ShowPos(beginPos, (bool)chkNull.IsChecked);
            timer_PD.Stop();
        }


        bool toPD = false;
        //开始盘点
        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            btStart.IsEnabled = false;
            btStop.IsEnabled = true;

            PLC_Tcp_AP.LaserOn();
            PDItem();

            toPD = true;
            timer_PD.Start();
        }
        //暂停盘点
        private void btStop_Click(object sender, RoutedEventArgs e)
        {
            btStart.IsEnabled = true;
            btStop.IsEnabled = false;

            toPD = false;
            timer_PD.Stop();
        }

        private void timerPD_Tick(object sender, EventArgs e)
        {
            if (toPD)
            {
                PDItem();
            }
        }
        private void PDItem()
        {
            toPD = false;
            Pos[] ps = lvPos.ItemsSource as Pos[];
            Pos p = null;
            int index = 0;
            for (int i = 0; i < ps.Length; i++)
            {
                if (string.IsNullOrEmpty(ps[i].PDNum))
                {
                    p = ps[i];
                    index = i;
                    break;
                }
            }
            if (p != null)
            {
                int scrIndex = index + 5;
                if (scrIndex >= lvPos.Items.Count)
                    scrIndex = lvPos.Items.Count - 1;
                lvPos.ScrollIntoView(lvPos.Items[scrIndex]);

                if (!string.IsNullOrEmpty(p.DrugOnlyCode))
                {
                    int n = PLC_Tcp_AP.PDNum(p.PosCode, false);
                    if (n < 0)
                    {
                        p.PDNum = "-1";
                        p.BackColor = color_Error;
                    }
                    else
                    {
                        p.PDNum = n.ToString();

                        int num = int.Parse(p.DrugNum);
                        if (n == num)
                        {
                            p.BackColor = color_Ok;
                        }
                        else
                        {
                            if (n > num)
                            {
                                p.BackColor = color_Ying;
                            }
                            else if (n < num)
                            {
                                p.BackColor = color_Kui;
                            }
                            string sql = "insert into sys_error (maccode,poscode,drugonlycode,errortype,oknum,errornum,errortime) values('{0}','{1}','{2}','PE',{3},{4},getdate())";
                            sql = string.Format(sql, Config.Soft.MacCode, p.PosCode, p.DrugOnlyCode, num, n);
                            csSql.ExecuteSql(sql, Config.Soft.ConnString);
                        }
                    }
                }
                else
                {
                    int n = PLC_Tcp_AP.PDNum(p.PosCode, true);
                    if (n == -1)
                    {
                        p.PDNum = "错误";
                        p.BackColor = color_Error;
                    }
                    else if (n == 1)
                    {
                        p.PDNum = "非空";
                        p.BackColor = color_Kui;

                        string sql = "insert into sys_error (maccode,poscode,drugonlycode,errortype,oknum,errornum,errortime) values('{0}','{1}','{2}','PE',{3},{4},getdate())";
                        sql = string.Format(sql, Config.Soft.MacCode, p.PosCode, p.DrugOnlyCode, 0, 1);
                        csSql.ExecuteSql(sql, Config.Soft.ConnString);
                    }
                    else if (n == 0)
                    {
                        p.PDNum = "空";
                        p.BackColor = color_Ok;
                    }
                }

                lvPos.ItemsSource = ps.ToList().ToArray();

                toPD = true;
            }
            else
            {
                toPD = false;
                timer_PD.Stop(); 
                
                csMsg.ShowInfo("盘点完成", false);
                //运行到接药口
                PLC_Tcp_AP.ExtramanAutoMoveToPulse(float.Parse(Config.Mac_A.Pulse_Meet_X), float.Parse(Config.Mac_A.Pulse_Meet_Z));
            }
        }

        private void btUpdate_Click(object sender, RoutedEventArgs e)
        {
            Pos[] ps = lvPos.ItemsSource as Pos[];
            string sql = "";
            foreach (Pos p in ps)
            {
                if (!string.IsNullOrEmpty(p.DrugOnlyCode) && !string.IsNullOrEmpty(p.PDNum))
                {
                        int m = int.Parse(p.DrugNum);
                        int n = int.Parse(p.PDNum);
                    if (n >= 0)
                    {
                        if (m != n)
                        {
                            string s = "update drug_pos set drugnum={2} where maccode='{0}' and poscode='{1}';";
                            sql += string.Format(s, Config.Soft.MacCode, p.PosCode, p.PDNum);
                            //盘盈亏明细
                            string type = "Y";
                            if (m > n)
                            {
                                type = "K";
                            }
                            s = @"insert into drug_import (maccode,drugonlycode,drugbatch,type,drugnum,drugunit,doperson,dotime,prescno) 
select '{0}','{1}','','{2}','{3}',drugunit,'{4}',getdate(),'' from drug_pos where maccode='{0}' and poscode='{5}';";
                            sql += string.Format(s, Config.Soft.MacCode, p.DrugOnlyCode, type, Math.Abs(m - n), Config.Soft.UserCode, p.PosCode);
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(sql))
                csSql.ExecuteSql(sql, Config.Soft.ConnString);

            csMsg.ShowInfo("库存更新成功", false);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer_PD.Stop();
        }
    }
}
