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
using System.Windows.Media.Animation;
using System.Configuration;
using System.Windows.Threading;
using System.Threading;
using System.Data;

namespace DreamSoft
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WinMain_SP : Window
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Oracle csOracle = new CSHelper.Oracle();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public WinMain_SP()
        {
            InitializeComponent();

            if (!csSql.SQLIsConnected(Config.Soft.ConnString))
            {
                csMsg.ShowWarning("服务器未连接", true);

                Application.Current.Shutdown();
            }
            Config.InitialConfig_Mac_S();

            if (Config.Mac_S.PLCIsEnable == "Y")
            {
                PLC_SP.Initial();
            }
        }

        public delegate void DelMsgChanged(string msg);
        public void MsgChanged(string msg)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                Msg.ShowMsg(msg, Colors.Red);
            });
        }

        public delegate void DelKeyShow(bool show);
        public void KeyShow(bool show)
        {
            if (show)
                grid_Key.Visibility = Visibility.Visible;
            else grid_Key.Visibility = Visibility.Collapsed;
        }

        DispatcherTimer timer_Monitor = new DispatcherTimer();
        DispatcherTimer timer_Presc = new DispatcherTimer();
        DispatcherTimer timer_Clear = new DispatcherTimer();
        //Thread thPresc;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            KeyShow(false);
            PLC_SP.ThrowMsg += new PLC_SP.ShowMsg(MsgChanged);
            CSHelper.SQL.ThrowMsg += new CSHelper.SQL.ShowMsg(MsgChanged);
            CSHelper.Oracle.ThrowMsg += new CSHelper.Oracle.ShowMsg(MsgChanged);
            UCAuto.ThrowMsg += new UCAuto.ShowMsg(MsgChanged);

            UCAuto.ShowKey += new UCAuto.SetKey(KeyShow);
            UCSearch.ShowKey += new UCSearch.SetKey(KeyShow);

            ShowMonitor();
            timer_Monitor.Interval = TimeSpan.FromSeconds(3);
            timer_Monitor.Tick += new EventHandler(timer_Monitor_Tick);
            if (Config.Mac_S.PLCIsEnable == "Y")
                timer_Monitor.Start();

            //ReceivePresc();
            //timer_Presc.Interval = TimeSpan.FromSeconds(Config.Mac.RefreshSpan);
            //timer_Presc.Tick += new EventHandler(timer_Presc_Tick);
            //timer_Presc.Start();

            //if (csOracle.OracleIsConnected(Config.Soft.ConnString_HIS))
            //{
            //    thPresc = new Thread(FreshPresc);
            //    thPresc.IsBackground = true;
            //    thPresc.Start();
            //}
            //else
            //    MsgChanged("HIS数据库未连接");

            ClearOld();
            timer_Clear.Interval = TimeSpan.FromMinutes(10);
            timer_Clear.Tick += new EventHandler(timer_Clear_Tick);
        }
        private void ClearOld()
        {
            string sql = "delete from pat_prescinfo where paytime<convert(varchar(100),DateAdd(Day,-2,paytime),23)";
            csSql.ExecuteSql(sql, Config.Soft.ConnString);
        }
        void timer_Clear_Tick(object sender, EventArgs e)
        {
            ClearOld();
        }

        bool gm_old = false;
        bool gm_new = false;
        bool stop_old = false;
        bool stop_new = false;
        private void ShowMonitor()
        {
            if (Config.Mac_S.PLCIsEnable == "Y")
            {
                //光幕监控
                gm_new = PLC_SP.GMIsProtected();
                if (gm_new != gm_old)
                {
                    gm_old = gm_new;
                    ShowGM(gm_new);
                }
                //急停监控
                stop_new = PLC_SP.MacIsStopped();
                if (stop_new != stop_old)
                {
                    stop_old = stop_new;
                    ShowStop(stop_new);
                }
            }
        }
        void timer_Monitor_Tick(object sender, EventArgs e)
        {
            ShowMonitor();
        }

        private void ReceivePresc()
        {
            string sql_His = @"select * from view_interface_recipejz
where  to_char(sysdate,'yyyy-mm-dd')=to_char(paytime,'yyyy-mm-dd') 
and to_char(sysdate,'yyyy-mm-dd HH24:MI:SS')<=to_char(paytime+(1/24/60)*{0},'yyyy-mm-dd HH24:MI:SS') order by paytime";
            sql_His = string.Format(sql_His, Config.Sys.EnableTime);
            DataTable dt_Presc;
            csOracle.ExecuteSelect(sql_His, Config.Soft.ConnString_His, out dt_Presc);
            if (dt_Presc != null)
            {
                if (dt_Presc.Rows.Count > 0)
                {
                    foreach (DataRow row in dt_Presc.Rows)
                    {
                        string sql_Dream = "";

                        string mrNo = row["mrno"].ToString().Trim();
                        string prescNo = row["prescno"].ToString().Trim();
                        string payTime = row["paytime"].ToString().Trim();
                        //按病人发药
                        //判断处方主表病人是否已存在
                        string sql = "select * from Pat_prescInfo where prescno='{0}'";
                        sql = string.Format(sql, mrNo);
                        DataTable dtP;
                        csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtP);
                        if (dtP != null && dtP.Rows.Count > 0)
                        {
                        }
                        else //插入主表信息
                        {
                            string str_Insert_Column_Presc = ""; string str_Insert_Value_Presc = "";

                            #region

                            str_Insert_Column_Presc += ",prescno_his";
                            str_Insert_Value_Presc += ",'" + prescNo + "'";

                            string s = "receiptno";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "windowno";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "mrno";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "patname";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "patsex";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "patage";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "ageunit";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "depcode";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "depname";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "docname";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "diagnosis";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "presctime";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "paytime";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "dotype";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            #endregion
                            string p = @"insert into pat_Prescinfo (prescno{0},doflag) values ('{1}'{2},'W');";
                            sql_Dream = string.Format(p, str_Insert_Column_Presc, mrNo, str_Insert_Value_Presc);
                        }
                        //判断处方明细表处方是否存在
                        sql = "select * from Pat_drugInfo where prescno_his='{0}'";
                        sql = string.Format(sql, prescNo);
                        DataTable dtD;
                        csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtD);
                        if (dtD != null && dtD.Rows.Count > 0)
                        {
                        }
                        else
                        {
                            sql_His = "select * from view_interface_recipeinfojz where prescno='{0}'";
                            sql_His = string.Format(sql_His, prescNo);
                            DataTable dt1 = new DataTable();
                            DataTable dt2 = new DataTable();
                            csOracle.ExecuteSelect(sql_His, Config.Soft.ConnString_His, out dt2);
                            do
                            {
                                Thread.Sleep(20);
                                csOracle.ExecuteSelect(sql_His, Config.Soft.ConnString_His, out dt1);
                                Thread.Sleep(20);
                                csOracle.ExecuteSelect(sql_His, Config.Soft.ConnString_His, out dt2);
                            } while (dt1.Rows.Count != dt2.Rows.Count);

                            if (dt2.Rows.Count > 0)
                            {
                                sql = "update pat_prescinfo set paytime='{0}',hisflag='0' where prescno='{1}'";
                                sql = string.Format(sql, payTime, mrNo);
                                csSql.ExecuteSql(sql, Config.Soft.ConnString);

                                foreach (DataRow r in dt2.Rows)
                                {
                                    string str_Insert_Column_Drug = ""; string str_Insert_Value_Drug = "";
                                    #region

                                    str_Insert_Column_Drug += ",prescno_his";
                                    str_Insert_Value_Drug += ",'" + prescNo + "'";

                                    string s = "drugonlycode";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "drugnum";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "drugunit";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "mode1";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += ",mode";
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "frequency";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "dosage";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "dosageunit";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    #endregion
                                    string p = @"insert into pat_Druginfo (prescno{0},doflag) values ('{1}'{2},'N');";
                                    sql_Dream += string.Format(p, str_Insert_Column_Drug, mrNo, str_Insert_Value_Drug);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(sql_Dream))
                            csSql.ExecuteSql(sql_Dream, Config.Soft.ConnString);
                    }
                }
                //else
                //    MsgChanged("无更新数据");
            }
            else
                MsgChanged("查询出错");
        }
        void timer_Presc_Tick(object sender, EventArgs e)
        {
            ReceivePresc();
        }
        private void FreshPresc()
        {
            while (true)
            {
                ReceivePresc();
                Thread.Sleep(Config.Mac_S.RefreshSpan);
            }
        }

        private void btDebug_Click(object sender, RoutedEventArgs e)
        {
            ShowUC(new UCDebug());
        }
        private void btSet_Click(object sender, RoutedEventArgs e)
        {
            new WinConfig().Show();
            //this.Close();
        }
        private void btManual_Click(object sender, RoutedEventArgs e)
        {
            ShowUC(new UCManual());
        }
        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowUC(new UCSearch());
        }
        private void btAuto_Click(object sender, RoutedEventArgs e)
        {
            ShowUC(new UCAuto());
        }
        private void ShowUC(UserControl uc)
        {
            uc.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            uc.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            win_Child.Children.Add(uc);
            btBack.Visibility = win_Child.Visibility = Visibility.Visible;
        }

        private void btExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowGM(bool show)
        {
            BeginStoryboard bs;
            if (show)
                bs = (BeginStoryboard)this.TryFindResource("colorBGM");
            else
                bs = (BeginStoryboard)this.TryFindResource("colorSGM");
            bs.Storyboard.Begin();
        }
        private void ShowStop(bool show)
        {
            BeginStoryboard bs;
            if (show)
                bs = (BeginStoryboard)this.TryFindResource("colorBStop");
            else
                bs = (BeginStoryboard)this.TryFindResource("colorSStop");
            bs.Storyboard.Begin();
        }

        private void btBack_Click(object sender, RoutedEventArgs e)
        {
            btBack.Visibility = Visibility.Hidden;
            win_Child.Children.Clear();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ResourceDictionary rc = new ResourceDictionary();
            MenuItem menu = sender as MenuItem;
            string file = @"Themes\" + menu.Tag.ToString();
            rc.Source = new Uri(file, UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[1] = rc;
        }

        private void grid_Key_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source.GetType() == typeof(Button))
            {
                Button bt = e.Source as Button;
                string s = bt.Content.ToString();
                Key k = new Key();
                switch (s)
                {
                    #region"Keys"
                    case "A":
                        k = Key.A;
                        break;
                    case "B":
                        k = Key.B;
                        break;
                    case "C":
                        k = Key.C;
                        break;
                    case "D":
                        k = Key.D;
                        break;
                    case "E":
                        k = Key.E;
                        break;
                    case "F":
                        k = Key.F;
                        break;
                    case "G":
                        k = Key.G;
                        break;
                    case "H":
                        k = Key.H;
                        break;
                    case "I":
                        k = Key.I;
                        break;
                    case "J":
                        k = Key.J;
                        break;
                    case "K":
                        k = Key.K;
                        break;
                    case "L":
                        k = Key.L;
                        break;
                    case "M":
                        k = Key.M;
                        break;
                    case "N":
                        k = Key.N;
                        break;
                    case "O":
                        k = Key.O;
                        break;
                    case "P":
                        k = Key.P;
                        break;
                    case "Q":
                        k = Key.Q;
                        break;
                    case "R":
                        k = Key.R;
                        break;
                    case "S":
                        k = Key.S;
                        break;
                    case "T":
                        k = Key.T;
                        break;
                    case "U":
                        k = Key.U;
                        break;
                    case "V":
                        k = Key.V;
                        break;
                    case "W":
                        k = Key.W;
                        break;
                    case "X":
                        k = Key.X;
                        break;
                    case "Y":
                        k = Key.Y;
                        break;
                    case "Z":
                        k = Key.Z;
                        break;


                    case "0":
                        k = Key.NumPad0;
                        break;
                    case "1":
                        k = Key.NumPad1;
                        break;
                    case "2":
                        k = Key.NumPad2;
                        break;
                    case "3":
                        k = Key.NumPad3;
                        break;
                    case "4":
                        k = Key.NumPad4;
                        break;
                    case "5":
                        k = Key.NumPad5;
                        break;
                    case "6":
                        k = Key.NumPad6;
                        break;
                    case "7":
                        k = Key.NumPad7;
                        break;
                    case "8":
                        k = Key.NumPad8;
                        break;
                    case "9":
                        k = Key.NumPad9;
                        break;
                    case "Backspace":
                        k = Key.Back;
                        break;
                    #endregion
                    case "关闭":
                        grid_Key.Visibility = Visibility.Collapsed;
                        break;
                }
                Class.Keyboard.Press(k);
            }
        }
    }
}
