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
using DreamSoft.Class;
using System.Windows.Threading;
using System.Data;
using System.IO.Ports;
using System.Threading;
using System.Windows.Media.Animation;

namespace DreamSoft
{
    /// <summary>
    /// WinMain.xaml 的交互逻辑
    /// </summary>
    public partial class WinMain_AP : Window
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

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

        public WinMain_AP()
        {
            InitializeComponent();

            if (!csSql.SQLIsConnected(Config.Soft.ConnString))
            {
                csMsg.ShowWarning("服务器未连接", true);

                Application.Current.Shutdown();
            }
            Config.InitialConfig_Mac_A();

            if (Config.Mac_A.PLC_Tcp == "Y")
            {
                PLC_Tcp_AP.Initial(true);
            }

            string[] ports = SerialPort.GetPortNames();
            if (Config.Soft.Function == "I")
            {
                if (Config.Mac_A.Scanner == "Y")
                {
                    if (ports.Contains(Config.Mac_A.Port_Scanner))
                        Scanner.InitialScanPort();
                    else
                        csMsg.ShowWarning("扫描枪串口不存在", true);
                }

                if (Config.Mac_A.Laser_Left == "Y")
                {
                    if (ports.Contains(Config.Mac_A.Port_Laser_Left))
                        Laser.InitialLaserPort_Left();
                    else
                        csMsg.ShowWarning("左侧激光串口不存在", true);
                }
                if(Config.Mac_A.Laser_Right == "Y")
                {
                    if (ports.Contains(Config.Mac_A.Port_Laser_Right))
                        Laser.InitialLaserPort_Right();
                    else
                        csMsg.ShowWarning("右侧激光串口不存在", true);
                }
            }
            else
            {
                if (Config.Mac_A.PLC_Com == "Y")
                {
                    if (ports.Contains(Config.Mac_A.Port_PLC))
                    PLC_Com_AP.Initial();
                }
                if (Config.Mac_A.DPJ == "Y")
                {
                    DCT_AP.Initial();
                    //if (ports.Contains(Config.Mac_A.Port_DPJ))
                    //    DPJ_AP.Initial();
                    //else
                    //    csMsg.ShowWarning("单片机串口不存在", true);
                }
            }
            grid_Key.Visibility = Visibility.Collapsed;
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

         bool gm_old = false;
         bool gm_new = false;

        bool stop_old = false;
        bool stop_new = false;

        DispatcherTimer timer_Monitor = new DispatcherTimer();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer_Monitor.Interval = TimeSpan.FromSeconds(3);
            timer_Monitor.Tick += new EventHandler(timer_Monitor_Tick);
            timer_Monitor.Start();

            grid_Monitor.Visibility = Visibility.Collapsed;
            if (Config.Soft.Function == "I")
            {
                sp_Out_Auto.Visibility = sp_Out_Manul.Visibility = sp_Error_Out.Visibility = Visibility.Hidden;
                grid_Lift.Visibility = grid_Max_Baffle.Visibility = grid_Min_Baffle.Visibility = Visibility.Collapsed;
            }
            else
            {
                sp_Add_List.Visibility = sp_Add.Visibility = sp_PD.Visibility = sp_Error_PD.Visibility = Visibility.Hidden;
                grid_X.Visibility = grid_Z.Visibility = grid_Left.Visibility = grid_Right.Visibility = grid_ZHA.Visibility = Visibility.Collapsed;

                ellipseGM.Visibility = tb_GM.Visibility = Visibility.Collapsed;
            }

            PLC_Tcp_AP.ThrowMsg += new PLC_Tcp_AP.ShowMsg(MsgChanged);
            PLC_Com_AP.ThrowMsg += new PLC_Com_AP.ShowMsg(MsgChanged);
            DCT_AP.ThrowMsg += new DPJ_AP.ShowMsg(MsgChanged);
            Scanner.ThrowMsg += new Scanner.ShowMsg(MsgChanged);
            Laser.ThrowMsg += new Laser.ShowMsg(MsgChanged);
            CSHelper.SQL.ThrowMsg += new CSHelper.SQL.ShowMsg(MsgChanged);
            OutDrug_AP.ThrowMsg += new OutDrug_AP.ShowMsg(MsgChanged);

            UCDebug_Add.ShowKey += new UCDebug_Add.SetKey(KeyShow);
            UCAdd.ShowKey += new UCAdd.SetKey(KeyShow);
            UCOut_Manual.ShowKey += new UCOut_Manual.SetKey(KeyShow);
            UCDebug_Out.ShowKey += new UCDebug_Out.SetKey(KeyShow);

            if (Config.Soft.Function == "O")
            {
                ellipseGM.Visibility = Visibility.Collapsed;
                tb_GM.Visibility = Visibility.Collapsed;
            }

            if(csMsg.ShowQuestion("设备在断电后的第一次启动时，运动部件需要原点复位。\r\n是否要原点复位？",false))
            {
                if (Config.Soft.Function == "I")
                {
                    PLC_Tcp_AP.ExtramanOriginReset();
                    Thread.Sleep(1000);
                    PLC_Tcp_AP.PlateOriginReset();
                }
                else
                {
                    PLC_Tcp_AP.LiftOriginReset(); 
                    Thread.Sleep(1000);
                    PLC_Com_AP.BaffleOriginReset_Lift();
                }
            }
        }

        void timer_Monitor_Tick(object sender, EventArgs e)
        {
            GC.Collect();

            bool error = false;
            if (Config.Soft.Function == "I")
            {
                if (Config.Mac_A.PLC_Tcp == "Y")
                {
                    //X轴
                    if (PLC_Tcp_AP.TrackStateIsOK(PLC_Tcp_AP.TrackErrorType.X1) && PLC_Tcp_AP.TrackStateIsOK(PLC_Tcp_AP.TrackErrorType.X2))// && PLC.ExtramanOriginResetState(PLC.TrackErrorType.X1) && PLC.ExtramanOriginResetState(PLC.TrackErrorType.X2))
                    {
                        grid_X.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        error = true;
                        tbX_Up.Text = PLC_Tcp_AP.ReadTrackErrorCode(PLC_Tcp_AP.TrackErrorType.X1);
                        tbX_Down.Text = PLC_Tcp_AP.ReadTrackErrorCode(PLC_Tcp_AP.TrackErrorType.X2);
                        grid_X.Visibility = Visibility.Visible;
                    }
                    //Z轴 
                    if (PLC_Tcp_AP.TrackStateIsOK(PLC_Tcp_AP.TrackErrorType.Z))// && PLC.ExtramanOriginResetState(PLC.TrackErrorType.Z))
                    {
                        grid_Z.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        error = true;
                        tbZ.Text = PLC_Tcp_AP.ReadTrackErrorCode(PLC_Tcp_AP.TrackErrorType.Z);
                        grid_Z.Visibility = Visibility.Visible;
                    }
                    //左臂
                    if (PLC_Tcp_AP.PlateStateIsOK(PLC_Tcp_AP.PlateType.Left))// && PLC.PlateOriginResetState(PLC.PlateType.Left))
                    {
                        grid_Left.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        error = true;
                        tbLeft.Text = PLC_Tcp_AP.ReadPlateErrorCode(PLC_Tcp_AP.PlateType.Left);
                        grid_Left.Visibility = Visibility.Visible;
                    }
                    //右臂
                    if (PLC_Tcp_AP.PlateStateIsOK(PLC_Tcp_AP.PlateType.Right))// && PLC.PlateOriginResetState(PLC.PlateType.Right))
                    {
                        grid_Right.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        error = true;
                        tbRight.Text = PLC_Tcp_AP.ReadPlateErrorCode(PLC_Tcp_AP.PlateType.Right);
                        grid_Right.Visibility = Visibility.Visible;
                    }
                    //加药障碍
                    if (PLC_Tcp_AP.PlateErrorZHA())
                    {
                        grid_ZHA.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        error = true;
                        grid_ZHA.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                if (Config.Mac_A.PLC_Tcp == "Y")
                {
                    //提升机
                    if (PLC_Tcp_AP.LiftStateIsOK())// && PLC.LiftOriginResetState())
                    {
                        grid_Lift.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        error = true;
                        tbLift.Text = PLC_Tcp_AP.ReadLiftErrorCode();
                        grid_Lift.Visibility = Visibility.Visible;
                    }
                }
                if (Config.Mac_A.PLC_Com == "Y")
                {
                    //提升机挡板
                    if (!PLC_Com_AP.Baffle_Lift_IsMaxUp())
                    {
                        grid_Max_Baffle.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        error = true;
                        grid_Max_Baffle.Visibility = Visibility.Visible;
                    }
                    if (!PLC_Com_AP.Baffle_Lift_IsMinDown())
                    {
                        grid_Min_Baffle.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        error = true;
                        grid_Min_Baffle.Visibility = Visibility.Visible;
                    }
                }
            }
            if (error)
                grid_Monitor.Visibility = Visibility.Visible;
            else
                grid_Monitor.Visibility = Visibility.Collapsed;

            if (Config.Mac_A.PLC_Tcp == "Y")
            {
                //光幕监控
                if (Config.Soft.Function == "I")
                {
                    gm_new = PLC_Tcp_AP.GMIsStop();
                    if (gm_new != gm_old)
                    {
                        gm_old = gm_new;
                        ShowGM(gm_new);
                    }
                }
                //急停监控
                if (Config.Soft.Function == "I")
                    stop_new = PLC_Tcp_AP.MacIsStop(PLC_Tcp_AP.MacType.Add);
                else stop_new = PLC_Tcp_AP.MacIsStop(PLC_Tcp_AP.MacType.Out);
                if (stop_new != stop_old)
                {
                    stop_old = stop_new;
                    ShowStop(stop_new);
                }
            }
        }

        private void config_Click(object sender, RoutedEventArgs e)
        {
            new WinConfig().Show();
            //this.Close();
        }
        private void debug_Click(object sender, RoutedEventArgs e)
        {
            UserControl uc;
            if (Config.Soft.Function == "I")
                uc = new UCDebug_Add();
            else
                uc = new UCDebug_Out();
            ShowUC(uc);
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            UserControl uc = new UCAdd();
            ShowUC(uc);
        }
        private void add_List_Click(object sender, RoutedEventArgs e)
        {
            UserControl uc = new UCAdd_List();
            ShowUC(uc);
        }
        private void pd_Click(object sender, RoutedEventArgs e)
        {
            UserControl uc = new UCPD();
            ShowUC(uc);
        }
        private void error_PD_Click(object sender, RoutedEventArgs e)
        {
            UserControl uc = new UCError_PD();
            ShowUC(uc);
        }

        private void out_Manual_Click(object sender, RoutedEventArgs e)
        {
            UserControl uc = new UCOut_Manual();
            ShowUC(uc);
        }
        private void out_Auto_Click(object sender, RoutedEventArgs e)
        {
            UserControl uc = new UCOut_Auto();
            ShowUC(uc);
        }
        private void error_Out_Click(object sender, RoutedEventArgs e)
        {
            UserControl uc = new UCError_Out();
            ShowUC(uc);
        }

        private void ShowUC(UserControl uc)
        {
            btBack.Visibility = grid_Win.Visibility = Visibility.Visible;
            grid_Win.Children.Clear();
            uc.Width = grid_Win.Width;
            uc.Height = grid_Win.Height;
            grid_Win.Children.Add(uc);
        }

        private void btBack_Click(object sender, RoutedEventArgs e)
        {
            btBack.Visibility = Visibility.Hidden;
            grid_Win.Children.Clear();
        }

        private void btExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //报警复位
        private void btX_Click(object sender, RoutedEventArgs e)
        {
            PLC_Tcp_AP.TrackErrorReset(PLC_Tcp_AP.TrackType.X);
        }
        private void btZ_Click(object sender, RoutedEventArgs e)
        {
            PLC_Tcp_AP.TrackErrorReset(PLC_Tcp_AP.TrackType.Z);
        }
        private void btL_Click(object sender, RoutedEventArgs e)
        {
            PLC_Tcp_AP.PlateErrorReset(PLC_Tcp_AP.PlateType.Left);
        }
        private void btR_Click(object sender, RoutedEventArgs e)
        {
            PLC_Tcp_AP.PlateErrorReset(PLC_Tcp_AP.PlateType.Right);
        }
        private void btLift_Click(object sender, RoutedEventArgs e)
        {
            PLC_Tcp_AP.LiftErrorReset();
        }

        private void tbCode_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            tbError.Text = tbHandle.Text = "";
            TextBox tb = sender as TextBox;
            if (!string.IsNullOrEmpty(tb.Text))
            {
                string code = tb.Text.Trim();
                string sql = "select errorinfo,handle from mac_error where errorpos='{0}' and errorcode='{1}'";
                sql = string.Format(sql, tb.Tag.ToString(), code);
                DataTable dt;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
                if (dt.Rows.Count > 0)
                {
                    tbError.Text = dt.Rows[0][0].ToString();
                    tbHandle.Text = dt.Rows[0][1].ToString();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;
            string s = bt.Content.ToString();
            Key k = new Key();
            switch (s)
            {
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
            }
            Class.Keyboard.Press(k);
        }

        private void btBackspace_Click(object sender, RoutedEventArgs e)
        {
            Class.Keyboard.Press(Key.Back);
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            grid_Key.Visibility = Visibility.Collapsed;
        }
    }
}
