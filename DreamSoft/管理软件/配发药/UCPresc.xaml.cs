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
using System.Threading;
using System.Windows.Threading;

namespace DreamSoft
{
    /// <summary>
    /// WinOut_Auto.xaml 的交互逻辑
    /// </summary>
    public partial class UCPresc : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();

        public UCPresc()
        {
            InitializeComponent();
        }

        public class Presc
        {
            public string PrescNo { get; set; }
            public int WindowNo { get; set; }
            public string PatName { get; set; }
            public string Status { get; set; }
        }
        public class PrescDetails
        {
            public string MacCode { get; set; }
            public string DrugOnlyCode { get; set; }
            public string DrugName { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
            public string DrugNum { get; set; }
            public string Status { get; set; }
        }

        private string GetNumString(int num, string unit, int pack, string packUnit, int split, string splitUnit)
        {
            string s = "";
            if (unit == packUnit)
                s = num + packUnit;
            else
            {
                int n = num / (pack / split);
                int m = num % (pack / split);
                if (n > 0)
                    s += n + packUnit;
                if (m > 0)
                    s += m + splitUnit;
            }
            return s;
        }

        //显示完成处方
        private void ShowOver()
        {
            string sel = "";
            if (lvOver.SelectedItems.Count > 0)
            {
                Presc p = lvOver.SelectedItems[0] as Presc;
                sel = p.PrescNo;
            }

            List<Presc> ps = null;
            string sql = "select distinct prescno,windowno,patname,ledflag from view_presc_window where windowno='{0}' and prescdoflag in('O','H') order by prescdotime ";
            sql = string.Format(sql, Config.Soft.WindowNo);
            DataTable dtOver;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtOver);
            if (dtOver != null && dtOver.Rows.Count > 0)
            {
                ps = new List<Presc>();
                foreach (DataRow row in dtOver.Rows)
                {
                    Presc p = new Presc
                    {
                        PrescNo = row["prescno"].ToString(),
                        WindowNo = int.Parse(row["windowno"].ToString()),
                        PatName = row["patname"].ToString(),
                        Status = row["ledflag"].ToString(),
                    };
                    switch (p.Status)
                    {
                        case "Y":
                            p.Status = "显示";
                            break;
                        case "N":
                            p.Status = "隐藏";
                            break;
                    }
                    ps.Add(p);
                }
            }
            else
            {
                tbPresc_Over.Text = tbTitle_Over.Text = "";
                lvOver_Details.ItemsSource = null;
            }
            lvOver.ItemsSource = ps;
        }
        //显示待配明细
        private void ShowDetails(string prescNo, ListView lv)
        {
            string sql = "select * from view_presc_window where prescno='{0}'";
            sql = string.Format(sql, prescNo);
            DataTable dtDetails;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDetails);

            List<PrescDetails> ds = null;
            if (dtDetails != null && dtDetails.Rows.Count > 0)
            {
                ds = new List<PrescDetails>();
                foreach (DataRow row in dtDetails.Rows)
                {
                    PrescDetails pd = new PrescDetails
                    {
                        DrugOnlyCode = row["drugonlycode"].ToString(),
                        DrugName = row["drugname"].ToString(),
                        DrugSpec = row["drugspec"].ToString(),
                        DrugFactory = row["drugfactory"].ToString(),
                        DrugNum = GetNumString(int.Parse(row["drugnum"].ToString()), row["drugunit"].ToString(), int.Parse(row["drugpacknum"].ToString()), row["drugpackunit"].ToString(), int.Parse(row["drugsplitnum"].ToString()), row["drugsplitunit"].ToString()),
                        Status = row["drugdoflag"].ToString()
                    }; 
                    switch (pd.Status)
                    {
                        case "N":
                            pd.Status = "未出";
                            break;
                        case "Y":
                            pd.Status = "已出";
                            break;
                        case "E":
                            pd.Status = "机外";
                            break;
                        case "S":
                            pd.Status = "缺药";
                            break;
                        case "B":
                            pd.Status = "超限";
                            break;
                        case "M":
                            pd.Status = "多出";
                            break;
                        case "Q":
                            pd.Status = "少出";
                            break;
                    }
                    ds.Add(pd);
                }
            }
            lv.ItemsSource = ds;
        }

        //显示正在配药
        private void ShowDoing()
        {
            string sql = "select top 1 prescno,windowno,patname from view_presc_window where windowno='{0}' and prescdoflag in('D','O') order by prescdotime desc";
            sql = string.Format(sql, Config.Soft.WindowNo);
            DataTable dtPresc;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPresc);

            List<Presc> ps = null;
            if (dtPresc != null && dtPresc.Rows.Count > 0)
            {
                string prescNo = dtPresc.Rows[0]["prescno"].ToString().Trim();
                ps = new List<Presc>();
                ps.Add(new Presc 
                {
                    PrescNo = prescNo, 
                    WindowNo = int.Parse(dtPresc.Rows[0]["windowno"].ToString()), 
                    PatName = dtPresc.Rows[0]["patname"].ToString() 
                });

                ShowDetails(prescNo, lvDoing_Details);
            }
            lvDoing.ItemsSource = ps;
        }

        //显示待配处方
        private void ShowWait()
        {
            string sel = "";
            if (lvWait.SelectedItems.Count > 0)
            {
                Presc p = lvWait.SelectedItems[0] as Presc;
                sel = p.PrescNo;
            }

            List<Presc> ps = null;
            string sql = "select distinct prescno,windowno,patname from view_presc_window where windowno='{0}' and doflag in('N','Y','W') order by toptime desc,paytime ";
            sql = string.Format(sql, Config.Soft.WindowNo);
            DataTable dtWait;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtWait);
            if (dtWait != null && dtWait.Rows.Count > 0)
            {
                ps = new List<Presc>();
                foreach (DataRow row in dtWait.Rows)
                {
                    ps.Add(new Presc
                    {
                        PrescNo = row["prescno"].ToString(),
                        WindowNo = int.Parse(row["windowno"].ToString()),
                        PatName = row["patname"].ToString()
                    });
                }
            }
            else
            {
                tbPresc.Text = tbTitle.Text = "";
                lvWait_Details.Items.Clear();
            }
            lvWait.ItemsSource = ps;
        }

        //更新处方状态
        private void UpdatePrescStatus(string prescNo, string flag)
        {
            string sql = "update pat_prescinfo set doflag='{0}',dotime=getdate() where prescno='{1}'";
            sql = string.Format(sql, flag, prescNo);
            csSql.ExecuteSql(sql, Config.Soft.ConnString);
        }
        //更新LED显示状态
        private void UpdateLEDStatus(string prescNo, string flag)
        {
            string sql = "update pat_prescinfo set ledflag='{0}' where prescno='{1}'";
            sql = string.Format(sql, flag, prescNo);
            csSql.ExecuteSql(sql, Config.Soft.ConnString);
        }

        DispatcherTimer timer_Doing = new DispatcherTimer();
        DispatcherTimer timer_Show = new DispatcherTimer();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowOver();
            ShowDoing();
            ShowWait();

            timer_Doing.Interval = TimeSpan.FromSeconds(1);
            timer_Doing.Tick += new EventHandler(timer_Doing_Tick);
            timer_Doing.Start();

            timer_Show.Interval = TimeSpan.FromSeconds(5);
            timer_Show.Tick += new EventHandler(timer_Show_Tick);
            timer_Show.Start();
        }

        private void timer_Doing_Tick(object sender, EventArgs e)
        {
            ShowDoing();
        }
        private void timer_Show_Tick(object sender, EventArgs e)
        {
            ShowOver();
            ShowWait();
        }

        private void lvWait_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvWait.SelectedItems.Count > 0)
            {
                Presc p = lvWait.SelectedItems[0] as Presc;
                tbPresc.Text = p.PrescNo;
                tbTitle.Text = p.WindowNo + "号窗口  " + p.PatName;
                ShowDetails(p.PrescNo, lvWait_Details);
            }
        }
        private void lvOver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvOver.SelectedItems.Count > 0)
            {
                Presc p = lvOver.SelectedItems[0] as Presc;
                tbPresc_Over.Text = p.PrescNo;
                tbTitle_Over.Text = p.WindowNo + "号窗口  " + p.PatName;
                ShowDetails(p.PrescNo, lvOver_Details);
            }
        } 
        //置顶
        private void btTop_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPresc.Text.Trim()))
            {
                string sql = "update pat_prescinfo set toptime=getdate() where prescno='{0}'";
                sql = string.Format(sql, tbPresc.Text.Trim());
                csSql.ExecuteSql(sql, Config.Soft.ConnString);

                ShowWait();
            }
        }
        //取消
        private void btCancle_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPresc.Text.Trim()))
            {
                UpdatePrescStatus(tbPresc.Text.Trim(), "C"); 
                ShowWait();
            }
        }
        
        //发出
        private void btFa_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPresc_Over.Text.Trim()))
            {
                UpdatePrescStatus(tbPresc_Over.Text.Trim(), "S");
                ShowOver();
            }
        }
        //显示
        private void btShow_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPresc_Over.Text.Trim()))
            {
                UpdateLEDStatus(tbPresc_Over.Text.Trim(), "Y"); 
                ShowOver();
            }
        }
        //隐藏
        private void btHide_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPresc_Over.Text.Trim()))
            {
                UpdateLEDStatus(tbPresc_Over.Text.Trim(), "N");
                ShowOver();
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer_Doing.Stop();
            timer_Show.Stop();
        }
    }
}
