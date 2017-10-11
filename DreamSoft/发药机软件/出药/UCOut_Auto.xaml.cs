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
using DreamSoft.Class;
using System.Threading;
using System.Windows.Threading;

namespace DreamSoft
{
    /// <summary>
    /// WinOut_Auto.xaml 的交互逻辑
    /// </summary>
    public partial class UCOut_Auto : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();

        public UCOut_Auto()
        {
            InitializeComponent();
        }

        public class Presc
        {
            public string PrescNo { get; set; }
            public int WindowNo { get; set; }
            public string PatName { get; set; }
        }

        string color_No = Colors.Khaki.ToString();
        string color_Doing = Colors.LightSalmon.ToString();
        string color_Yes = Colors.LimeGreen.ToString();
        public class PC
        {
            public string OutPC { get; set; }
            public string Flag { get; set; }
            public string BackColor { get; set; }
        }

        string color_In = Colors.LimeGreen.ToString();
        string color_Out = Colors.Gray.ToString();
        public class PrescDetails
        {
            public string MacCode { get; set; }
            public string DrugOnlyCode { get; set; }
            public string DrugName { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
            public string DrugNum { get; set; }
            public string Status { get; set; }
            public string BackColor { get; set; }
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

        private int GetNumInt(int num, string unit, int split, string packUnit)
        {
            int s = 0;

            if (unit == packUnit)
            {
                s = num;
            }
            else
            {
                int n = num / split;
                int m = num % split;
                if (m == 0)
                {
                    s = n;
                }
                else
                {
                    if (Config.Mac_A.SplitModel == "N")
                        s = 0;
                    else if (Config.Mac_A.SplitModel == "E")
                    {
                        s = n;
                    }
                    else if (Config.Mac_A.SplitModel == "M")
                    {
                        s = n + 1;
                    }
                }
            }
            return s;
        }

        //显示正在配药
        private void ShowDoing()
        {
            string wins = GetWins(Config.Soft.MacCode);
            string sql = "select prescno,windowno,patname from pat_prescinfo where windowno in ({0}) and doflag in('D','O') and paytime>=convert(varchar(100),getdate(),23) order by dotime desc";
            sql = string.Format(sql, wins);
            DataTable dtPresc;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPresc);

            List<Presc> ps = null;
            List<PC> cs = null;
            List<PrescDetails> ds = null;
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

                sql = @"select pd.drugonlycode,di.drugname,di.drugaliasname,di.drugspec,di.drugfactory,di.drugpacknum,di.drugpackunit,di.drugsplitnum,di.drugsplitunit,pd.drugnum,pd.drugunit,pd.doflag,
(select top 1 maccode from drug_pos dp where dp.drugonlycode=pd.drugonlycode) as maccode 
from pat_druginfo pd
left join drug_info di on di.drugonlycode=pd.drugonlycode
where prescno='{0}'";
                sql = string.Format(sql, prescNo);
                DataTable dtDetails;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDetails);
                if (dtDetails != null && dtDetails.Rows.Count > 0)
                {
                    ds = new List<PrescDetails>();
                    foreach (DataRow row in dtDetails.Rows)
                    {
                        PrescDetails pd = new PrescDetails
                        {
                            MacCode = row["maccode"].ToString(),
                            DrugOnlyCode = row["drugonlycode"].ToString(),
                            DrugName = row["drugname"].ToString(),
                            DrugSpec = row["drugspec"].ToString(),
                            DrugFactory = row["drugfactory"].ToString(),
                            DrugNum = GetNumString(int.Parse(row["drugnum"].ToString()), row["drugunit"].ToString(), int.Parse(row["drugpacknum"].ToString()), row["drugpackunit"].ToString(), int.Parse(row["drugsplitnum"].ToString()), row["drugsplitunit"].ToString()),
                            Status = row["doflag"].ToString()
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
                        pd.BackColor = pd.MacCode == Config.Soft.MacCode ? color_In : color_Out;
                        ds.Add(pd);
                    }
                }

                sql = @"select distinct outpc,pcflag from pat_druginfo where prescno='{0}' and outpc is not null order by outpc";
                sql = string.Format(sql, prescNo, Config.Soft.MacCode);
                DataTable dtPC;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPC);
                if (dtPC.Rows.Count > 0)
                {
                    cs = new List<PC>();
                    foreach (DataRow row in dtPC.Rows)
                    {
                        PC c = new PC { OutPC = row["outpc"].ToString(), Flag = row["pcflag"].ToString()};
                        switch (c.Flag)
                        {
                            case "N":
                                c.BackColor = color_No;
                                break;
                            case "D":
                                c.BackColor = color_Doing;
                                break;
                            case "Y":
                                c.BackColor = color_Yes;
                                break;
                        }
                        cs.Add(c);
                    }                                 
                }
            }
            lvDoing.ItemsSource = ps;
            lvPC.ItemsSource = cs;
            lvDoing_Details.ItemsSource = ds;
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
            string wins = GetWins(Config.Soft.MacCode);
            string sql = "select prescno,windowno,patname from pat_prescinfo where windowno in ({0}) and doflag in('N','Y','W') and paytime>=convert(varchar(100),getdate(),23) order by toptime desc,paytime ";
            sql = string.Format(sql, wins);
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
            lvWait.ItemsSource = ps;
        }
        //显示待配明细
        private void ShowWaitDetails(string prescNo)
        {
            string sql = @"select pd.drugonlycode,di.drugname,di.drugaliasname,di.drugspec,di.drugfactory,di.drugpacknum,di.drugpackunit,di.drugsplitnum,di.drugsplitunit,pd.drugnum,drugunit from pat_druginfo pd
left join drug_info di on di.drugonlycode=pd.drugonlycode
where prescno='{0}'";
            sql = string.Format(sql, prescNo);
            DataTable dtWait_Details;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtWait_Details);

            List<PrescDetails> ds = null;
            if (dtWait_Details != null && dtWait_Details.Rows.Count > 0)
            {
                ds = new List<PrescDetails>();
                foreach (DataRow row in dtWait_Details.Rows)
                {
                    ds.Add(new PrescDetails
                    {
                        DrugOnlyCode = row["drugonlycode"].ToString(),
                        DrugName = row["drugname"].ToString(),
                        DrugSpec = row["drugspec"].ToString(),
                        DrugFactory = row["drugfactory"].ToString(),
                        DrugNum = GetNumString(int.Parse(row["drugnum"].ToString()), row["drugunit"].ToString(), int.Parse(row["drugpacknum"].ToString()), row["drugpackunit"].ToString(), int.Parse(row["drugsplitnum"].ToString()), row["drugsplitunit"].ToString())
                    });
                }
            }
            lvWait_Details.ItemsSource = ds;
        }

        //获取设备对应窗口组合
        private string GetWins(string maccode)
        {
            string result = "";
            string sql = "select windowno from sys_window_mac where maccode='{0}'";
            sql = string.Format(sql, Config.Soft.MacCode);
            DataTable dt;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
            foreach (DataRow row in dt.Rows)
            {
                result += row["windowno"].ToString().Trim() + ",";
            }
            if (result.Length > 0)
                result = result.Substring(0, result.Length - 1);
            else result = "0";
            return result;
        }

        bool ToOut = false;
        bool IsOut = false;
        //出药
        private void GoOut()
        {
            if (!IsOut)
            {
                IsOut = true;

                string wins = GetWins(Config.Soft.MacCode);
                string sql = "select top 1 prescno,windowno,patname from pat_prescinfo where windowno in ({0}) and doflag in('W','D') and paytime>=convert(varchar(100),getdate(),23) order by toptime desc,paytime";
                sql = string.Format(sql, wins);
                DataTable dtDoing;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDoing);
                if (dtDoing.Rows.Count > 0)
                {
                    string prescNo = dtDoing.Rows[0]["prescno"].ToString();
                    string windowNo = dtDoing.Rows[0]["windowno"].ToString();
                    string patname = dtDoing.Rows[0]["patname"].ToString();
                    //正在出药（D）
                    UpdatePrescStatus(prescNo, "D");
                    //快发内药品明细
                    sql = @"select pd.drugonlycode,sum(drugnum) as num,drugunit,drugpacknum,drugpackunit,drugsplitnum,drugsplitunit from pat_druginfo pd
left join drug_info di on di.drugonlycode=pd.drugonlycode
where prescno='{0}' and  pd.drugonlycode in
(
select drugonlycode from drug_pos where maccode='{1}' 
)
group by pd.drugonlycode,drugunit,drugpacknum,drugpackunit,drugsplitnum,drugsplitunit";
                    sql = string.Format(sql, prescNo, Config.Soft.MacCode);
                    DataTable dtDrug;
                    csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDrug);
                    if (dtDrug != null && dtDrug.Rows.Count > 0)//分配批次
                    {
                        int pc = 1;
                        int pcNum = Config.Mac_A.PCNum;
                        foreach (DataRow dr_Drug in dtDrug.Rows)
                        {
                            string drugonlycode = dr_Drug["drugonlycode"].ToString().Trim();
                            int drugnum = GetNumInt(int.Parse(dr_Drug["num"].ToString().Trim()), dr_Drug["drugunit"].ToString().Trim(), int.Parse(dr_Drug["drugpacknum"].ToString().Trim()), dr_Drug["drugpackunit"].ToString().Trim());
                            if (drugnum > 0)
                            {
                                if (drugnum > Config.Mac_A.MaxNum)//超出出药上限(B)
                                {
                                    UpdateDrugStatus(prescNo, drugonlycode, "B");
                                }
                                else
                                {
                                    sql = @"select sum(isnull(drugnum,0)-isnull(drugnummin,0)) num from drug_pos where maccode='{0}' and drugonlycode='{1}' and errorNum<3 group by drugonlycode";
                                    sql = string.Format(sql, Config.Soft.MacCode, drugonlycode);
                                    int numSum = 0;
                                    string value;
                                    csSql.ExecuteScalar(sql, Config.Soft.ConnString, out value);
                                    if (!string.IsNullOrEmpty(value))
                                        numSum = int.Parse(value);
                                    if (numSum < drugnum)//缺药(S)
                                    {
                                        UpdateDrugStatus(prescNo, drugonlycode, "S");
                                    }
                                    else
                                    {
                                        if (drugnum > pcNum)
                                        {
                                            pc++;
                                            pcNum = Config.Mac_A.PCNum - drugnum;
                                        }
                                        else
                                            pcNum -= drugnum;
                                        //分配批次
                                        sql = "update pat_druginfo set outpc={0},pcflag='N' where prescno='{1}' and drugonlycode='{2}'";
                                        sql = string.Format(sql, pc, prescNo, drugonlycode);
                                        csSql.ExecuteSql(sql, Config.Soft.ConnString);
                                    }
                                }
                            }
                            else
                            {
                                //机外
                                UpdateDrugStatus(prescNo, drugonlycode, "E");
                            }
                        }
                        //判断出药口位置
                        PLC_Tcp_AP.MovePos type = PLC_Tcp_AP.MovePos.Down;
                        sql = "select outposition from sys_window_mac where windowno={0} and maccode='{1}'";
                        sql = string.Format(sql, windowNo, Config.Soft.MacCode);
                        string v;
                        csSql.ExecuteScalar(sql, Config.Soft.ConnString, out v);
                        switch (v)
                        {
                            case "T":
                                type = PLC_Tcp_AP.MovePos.Top;
                                break;
                            case "U":
                                type = PLC_Tcp_AP.MovePos.Up;
                                break;
                            case "D":
                                type = PLC_Tcp_AP.MovePos.Down;
                                break;
                        }
                        //逐个批次出药
                        for (int p = 1; p <= pc; p++)
                        {
                            while (OutDrug_AP.IsOut)
                                Thread.Sleep(500);
                            sql = @"select drugonlycode,sum(drugnum) as num from pat_druginfo
where prescno='{0}' and outpc='{1}' and drugonlycode in
(
select drugonlycode from drug_pos  
where maccode='{2}' 
)
group by drugonlycode";
                            sql = string.Format(sql, prescNo, p, Config.Soft.MacCode);
                            DataTable dtPC;
                            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPC);
                            if (dtPC != null && dtPC.Rows.Count > 0)
                            {
                                DataTable dtOut = GetOutPos(dtPC);

                                OutDrug_AP od = new OutDrug_AP(type, dtOut, true, prescNo, p, dtPC);
                                od.GoOut();

                                //批次出药开始
                                sql = "update pat_druginfo set pcflag='D',dotime=getdate() where prescno='{0}' and outpc='{1}'";
                                sql = string.Format(sql, prescNo, p);
                                csSql.ExecuteSql(sql, Config.Soft.ConnString);
                                Thread.Sleep(1000);
                            }

                            Thread.Sleep(1000);
                            while (OutDrug_AP.IsOut)
                            {
                                Thread.Sleep(500);
                            }
                            //批次出药完成
                            sql = "update pat_druginfo set pcflag='Y',dotime=getdate() where prescno='{0}' and outpc='{1}'";
                            sql = string.Format(sql, prescNo, p);
                            csSql.ExecuteSql(sql, Config.Soft.ConnString);
                            Thread.Sleep(1000);
                        }
                    }
                    //结束出药（4）
                    UpdatePrescStatus(prescNo, "O");
                    isWait = true;
                    wait = Config.Mac_A.PrescWait;
                    timer_Wait.Start();
                    IsOut = false;
                }
                IsOut = false;
            }
        }

        //更新状态
        private void UpdatePrescStatus(string prescNo, string flag)
        {
            string sql = "update pat_prescinfo set doflag='{0}',dotime=getdate() where prescno='{1}'";
            sql = string.Format(sql, flag, prescNo);
            csSql.ExecuteSql(sql, Config.Soft.ConnString);
            if (flag == "O")
            {
                sql = "update pat_prescinfo set ledflag='Y' where prescno='{0}'";
                sql = string.Format(sql, prescNo);
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
            }
        }

        private void UpdateDrugStatus(string prescNo, string drugonlycode, string flag)
        {
            string sql = "update pat_druginfo set doflag='{0}' where prescno='{1}' and drugonlycode='{2}'";
            sql = string.Format(sql, flag, prescNo, drugonlycode);
            csSql.ExecuteSql(sql, Config.Soft.ConnString);
        }

        public DataTable GetOutPos(DataTable dtDrug)
        {
            DataTable dtPos = new DataTable("Drug_Out");
            dtPos.Columns.Add("PosCode");
            dtPos.Columns.Add("Num");
            foreach (DataRow row in dtDrug.Rows)
            {
                string drugonlycode = row["drugonlycode"].ToString().Trim();
                int num = int.Parse(row["num"].ToString());
                string sql = "select poscode,(ISNULL(drugnum,0)-ISNULL(drugnummin,1)) as drugnum from drug_pos where maccode='{0}' and DrugonlyCode='{1}' and errorNum<3 and (ISNULL(drugnum,0)-ISNULL(drugnummin,1))>0 order by drugbatch,drugnum desc";
                sql = string.Format(sql, Config.Soft.MacCode, drugonlycode);
                DataTable dtNum;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtNum);
                //根据批号、库存排序，逐个储位循环分配
                while (num > 0)
                {
                    foreach (DataRow dr_Num in dtNum.Rows)
                    {
                        if (num > 0)
                        {
                            int num_Pos = int.Parse(dr_Num["drugnum"].ToString());
                            if (num_Pos > 0)
                            {
                                string posCode = dr_Num["poscode"].ToString().Trim();
                                //分配一个
                                dr_Num["drugnum"] = int.Parse(dr_Num["drugnum"].ToString()) - 1;
                                num -= 1;
                                AddOut(ref dtPos, posCode, 1);
                            }
                        }
                        else
                            break;
                    }
                }
                num = 0;
            }
            return dtPos;
        }
        //添加出药数据表
        void AddOut(ref DataTable dt, string posCode, int num)
        {
            bool add = true;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["poscode"].ToString().Trim() == posCode)
                {
                    add = false;
                    dr["num"] = int.Parse(dr["num"].ToString()) + num;
                    break;
                }
            }
            if (add)
            {
                DataRow dr = dt.NewRow();
                dr["poscode"] = posCode;
                dr["num"] = num;
                dt.Rows.Add(dr);
            }
        }

        DispatcherTimer timer_Show = new DispatcherTimer();
        DispatcherTimer timer_Wait = new DispatcherTimer();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer_Show.Interval = TimeSpan.FromSeconds(1);
            timer_Show.Tick += new EventHandler(timer_Show_Tick);
            timer_Show.Start();

            timer_Wait.Interval = TimeSpan.FromSeconds(1);
            timer_Wait.Tick += new EventHandler(timer_Wait_Tick);

            thOut = new Thread(Out);
            thOut.IsBackground = true;
            thOut.Start();
        }

        private void timer_Show_Tick(object sender, EventArgs e)
        {
            ShowDoing();
            ShowWait();
        }

        Thread thOut;
        private void Out()
        {
            while (true)
            {
                if (ToOut && !isWait)
                {
                    GoOut();
                    Thread.Sleep(1000);
                }
            }
        }

        bool isWait = false;
        int wait = Config.Mac_A.PrescWait;
        private void timer_Wait_Tick(object sender, EventArgs e)
        {
            if (wait > 0)
            {
                tbWait.Text = wait.ToString();
                tbWait.Visibility = Visibility.Visible;
                wait--;
            }
            else if (wait <= 0)
            {
                isWait = false;
                tbWait.Visibility = Visibility.Hidden;
                timer_Wait.Stop();
            }
        }

        private void lvWait_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvWait.SelectedItems.Count > 0)
            {
                Presc p = lvWait.SelectedItems[0] as Presc;
                tbPresc.Text = p.PrescNo;
                tbTitle.Text = p.WindowNo + "  " + p.PatName;
                ShowWaitDetails(p.PrescNo);
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
            }
        }
        //取消
        private void btCancle_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPresc.Text.Trim()))
            {
                UpdatePrescStatus(tbPresc.Text.Trim(), "C");
            }
        }

        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            if (btStart.Content.ToString() == "开始")
            {
                ToOut = true;
                btStart.Content = "暂停";
                btStart.Template = (ControlTemplate)FindResource("pauseButton");
            }
            else
            {
                ToOut = false;
                btStart.Content = "开始";
                btStart.Template = (ControlTemplate)FindResource("startButton");
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ToOut = false; isWait = false;
            while (IsOut)
            {
                Thread.Sleep(1000);
            }
            thOut.Abort();
            timer_Show.Stop(); timer_Wait.Stop();
        }
    }
}
