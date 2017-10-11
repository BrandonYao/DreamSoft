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
using System.Collections.ObjectModel;
using System.Data;
using DreamSoft.Class;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;

namespace DreamSoft
{
    /// <summary>
    /// WinAdd.xaml 的交互逻辑
    /// </summary>
    public partial class UCAdd_CP : UserControl
    {
        static CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();
        //CSHelper.TKey csKey = new CSHelper.TKey();

        public delegate void DelScanChanged(string msg);
        public void ScanChanged(string code)
        {
            scanCode = code;
            scanNew = true;
        }

        public delegate void SetKey(bool show);
        public static SetKey ShowKey;

        public UCAdd_CP()
        {
            InitializeComponent();
        }
        public class Drug
        {
            public string DrugOnlyCode{get;set;}
            public string DrugName { get; set; }
            public string DrugSpec { get; set; }
            public string DrugFactory { get; set; }
        }
        //显示药品列表
        private void ShowDrug(string code, bool Barcode)
        {
            string sql = "";
            if (Barcode)
            {
                sql = @"select distinct p.drugonlycode,drugname,drugaliasname,drugpycode,drugspec,drugfactory from drug_info i,drug_pos p where i.drugonlycode=p.drugonlycode and maccode='{0}'
             and p.drugonlycode in(select drugonlycode from drug_infoannex where drugbarcode='{1}' or drugsupcode='{1}')";
                sql = string.Format(sql, Config.Soft.MacCode, code);
            }
            else
            {
                sql = @"select distinct p.drugonlycode,drugname,drugaliasname,drugpycode,drugspec,drugfactory from drug_info i,drug_pos p where i.drugonlycode=p.drugonlycode and maccode='{0}'
             and p.drugonlycode in(select drugonlycode from drug_info where drugpycode like '%{1}%' or drugaliaspycode like '%{1}%')";
                sql = string.Format(sql, Config.Soft.MacCode, code);
            }
            DataTable dtDrug = new DataTable();
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDrug);
            Drug[] ds = null;
            if (dtDrug != null && dtDrug.Rows.Count > 0)
            {
                ds = new Drug[dtDrug.Rows.Count];
                for (int r = 0; r < dtDrug.Rows.Count; r++)
                {
                    ds[r] = new Drug
                     {
                         DrugOnlyCode = dtDrug.Rows[r]["drugonlycode"].ToString(),
                         DrugName = dtDrug.Rows[r]["drugname"].ToString(),
                         DrugSpec = dtDrug.Rows[r]["drugspec"].ToString(),
                         DrugFactory = dtDrug.Rows[r]["drugfactory"].ToString()
                     };
                }
            }
            lvDrug.ItemsSource = ds;
            //如果只有一项，则直接显示
            if (lvDrug.Items.Count == 1)
            {
                Drug d = lvDrug.Items[0] as Drug;
                ShowSize(d.DrugOnlyCode);
                ShowPos(d.DrugOnlyCode, true);
            }
        }
        /// <summary>
        /// 显示尺寸和条码
        /// </summary>
        /// <param name="code">药品编码</param>
        private void ShowSize(string code)
        {
            ShowKey(false);
            //csKey.Close();

            tbCode.Text = code;
            tbSize.Text = "";
            string sql = "select length,width,height from drug_infoannex where drugonlycode='" + code + "'";
            DataTable dtDrug = new DataTable();
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDrug);
            if (dtDrug != null && dtDrug.Rows.Count > 0)
            {
                tbSize.Text = dtDrug.Rows[0]["length"].ToString().Trim() + "*" +
                dtDrug.Rows[0]["width"].ToString().Trim() + "*" + dtDrug.Rows[0]["height"].ToString().Trim();
            }
        }

        string color_Ok = Colors.LimeGreen.ToString();
        string color_Error = Colors.Gray.ToString();
        string color_Ying = Colors.LightSalmon.ToString();
        string color_Kui = Colors.Crimson.ToString();

        public class Pos : INotifyPropertyChanged
        {
            public string PosCode { get; set; }
            public string DrugOnlyCode { get; set; }

            private string drugNum;
            public string DrugNum
            {
                get { return drugNum; }
                set
                {
                    drugNum = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("DrugNum"));
                }
            }

            public string DrugNumMax { get; set; }

            public string PDNum { get; set; }

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
        /// <summary>
        /// 显示已分配储位和可存放量
        /// </summary>
        /// <param name="code">药品编码</param>
        private void ShowPos(string code, bool showAdd)
        {
            tbSum.Text = "";
            tbStock.Text = "";
            tbShort.Text = "";

            string sql = "select poscode,drugnum,drugnummax from drug_pos where drugonlycode='" + code + "' and maccode='" + Config.Soft.MacCode + "' order by poscode";
            DataTable dtPos = new DataTable();
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPos);
            Pos[] ps = null;
            if (dtPos != null && dtPos.Rows.Count > 0)
            {
                ps = new Pos[dtPos.Rows.Count];
                for (int r = 0; r < dtPos.Rows.Count; r++ )
                {
                    ps[r] = new Pos
                    {
                        PosCode = dtPos.Rows[r]["poscode"].ToString(),
                        DrugOnlyCode = code,
                        DrugNum = dtPos.Rows[r]["drugnum"].ToString(),
                        DrugNumMax = dtPos.Rows[r]["drugnummax"].ToString()
                    };
                }
            }
            lvPos.ItemsSource = ps;

            //储位量和可存放量
            sql = "select drugOnlyCode,sum(isnull(drugnummax,0)) as maxsumnum,sum(isnull(drugnum,0)) as sumnum,sum(isnull(drugnummax,0)-isnull(drugnum,0)) as shortnum from drug_pos where drugonlycode='" + code + "' and maccode='" + Config.Soft.MacCode + "' group by DrugOnlyCode";
            DataTable dtNum = new DataTable();
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtNum);
            if (dtNum != null && dtNum.Rows.Count > 0)
            {
                tbSum.Text = dtNum.Rows[0]["maxsumnum"].ToString().Trim();
                tbStock.Text = dtNum.Rows[0]["sumnum"].ToString().Trim();
                tbShort.Text = dtNum.Rows[0]["shortnum"].ToString().Trim();
                if (int.Parse(tbShort.Text) > 0 && showAdd)
                {
                    ShowAdd(code, int.Parse(tbShort.Text));
                }
            }
        }
        /// <summary>
        /// 显示加药信息
        /// </summary>
        /// <param name="code"></param>
        private void ShowAdd(string code, int shortNum)
        {
            //清除药品已分配仓位
            if (plate_Left.DrugOnlyCode == code)
            {
                plate_Left.DrugOnlyCode = "";
                plate_Left.Num = 0;
            }
            if (plate_Right.DrugOnlyCode == code)
            {
                plate_Right.DrugOnlyCode = "";
                plate_Right.Num = 0;
            }
            int maxnum = 0;
            //判断左侧
            if (plate_Left.DrugOnlyCode == "")
            {
                //仓位加药上限
                float height = float.Parse(tbSize.Text.Split('*')[2]);
                maxnum = (int)Math.Floor(Config.Mac_C.PlateHeight / height);

                //查询左臂可加储位缺药量(单元号<最大单元数，列号小于左臂上极限）
                string sql = @"select sum(isnull(drugnummax,0)-isnull(drugnum,0)) as shortnum from drug_pos where drugonlycode='{0}' and maccode='{1}'
                 and (substring(poscode,1,1)<{2} or substring(poscode,4,2)<={3}) group by DrugOnlyCode";
                sql = string.Format(sql, code, Config.Soft.MacCode, Config.Mac_C.Count_Unit, Config.Mac_C.MaxCol);
                string numLs;
                csSql.ExecuteScalar(sql, Config.Soft.ConnString, out numLs);
                int numLi = 0;
                if (!string.IsNullOrEmpty(numLs))
                    numLi = int.Parse(numLs);

                //确定左侧可加药数量
                int addLTo = 0;
                if (numLi > maxnum)
                    addLTo = maxnum;
                else addLTo = numLi;
                //确定左侧最终加药数量
                int addL = 0;
                if (shortNum > addLTo)
                    addL = addLTo;
                else addL = shortNum;
                //填充左仓
                if (addL > 0)
                    ShowPlate(PLC_Tcp_AP.PlateType.Left, code, addL);
                //计算右侧需加药量
                shortNum -= addL;
            }
            //判断右侧
            if (shortNum > 0 && plate_Right.DrugOnlyCode == "")
            {
                //仓位加药上限
                float height = float.Parse(tbSize.Text.Split('*')[2]);
                maxnum = (int)Math.Floor(Config.Mac_C.PlateHeight / height);
                //查询右臂可加储位缺药量(单元号>1,列号大于右臂下极限）
                string sql = @"select sum(isnull(drugnummax,0)-isnull(drugnum,0)) as shortnum from drug_pos where drugonlycode='{0}' and maccode='{1}'
                 and (substring(poscode,1,1)>1 or substring(poscode,4,2)>={2}) group by DrugOnlyCode";
                sql = string.Format(sql, code, Config.Soft.MacCode,Config.Mac_C.MinCol);
                string numRs;
                csSql.ExecuteScalar(sql, Config.Soft.ConnString, out numRs);
                int numRi = 0;
                if (!string.IsNullOrEmpty(numRs))
                    numRi = int.Parse(numRs);

                //确定右侧可加药数量
                int addRTo = 0;
                if (numRi > maxnum)
                    addRTo = maxnum;
                else addRTo = numRi;
                //确定右侧最终加药数量
                int addR = 0;
                if (shortNum > addRTo)
                    addR = addRTo;
                else addR = shortNum;
                //填充右仓
                if (addR > 0)
                    ShowPlate(PLC_Tcp_AP.PlateType.Right, code, addR);
            }
        }

        //仓位是否忙标识
        public bool IsBusy;
        class Plate
        {
            public string DrugOnlyCode;
            public int Num;
            public PLC_Tcp_AP.PlateType PlateType;
            public string Batch;
            public bool QZ;
        };
        Plate plate_Left = new Plate();
        Plate plate_Right = new Plate();
        Plate[] Plates = new Plate[2];
        //填充仓位
        void ShowPlate(PLC_Tcp_AP.PlateType type, string code, int num)
        {
            if (code != "" && num > 0)
            {
                if (type == PLC_Tcp_AP.PlateType.Left)
                {
                    plate_Left.DrugOnlyCode = code;
                    plate_Left.Num = num;
                    tbNumL.Text = plate_Left.Num.ToString();
                    btClearL.IsEnabled = true;
                    gd_L.Background = new SolidColorBrush(Colors.LightSalmon);
                    string sql = "select drugname,drugspec,drugfactory from drug_info where drugonlycode='{0}'";
                    sql = string.Format(sql, code);
                    DataTable dt;
                    csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
                    tbNameL.Text = dt.Rows[0]["drugname"].ToString();
                    tbSpecL.Text = dt.Rows[0]["drugspec"].ToString();
                    tbFactoryL.Text = dt.Rows[0]["drugfactory"].ToString();
                    //运动到预留位置
                    PLC_Tcp_CP.ChangeOut(1);
                    //获取药盒厚度
                    float drugThickness = 10;
                    sql = "select height from drug_infoannex where drugonlycode ='{0}'";
                    sql = string.Format(sql, code);
                    string s;
                    csSql.ExecuteScalar(sql, Config.Soft.ConnString, out s);
                    if (!string.IsNullOrEmpty(s))
                        drugThickness = float.Parse(s);
                    int pulse = Convert.ToInt32(num * drugThickness * Config.Mac_C.OneMMPulse_Plate);
                    PLC_Tcp_CP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Left, float.Parse(Config.Mac_C.Pulse_Plate_Max_Left) - pulse);
                }
                else if(type == PLC_Tcp_AP.PlateType.Right)
                {
                    plate_Right.DrugOnlyCode = code;
                    plate_Right.Num = num;
                    tbNumR.Text = plate_Right.Num.ToString();
                    btClearR.IsEnabled = true;
                    gd_R.Background = new SolidColorBrush(Colors.LightSalmon);
                    string sql = "select drugname,drugspec,drugfactory from drug_info where drugonlycode='{0}'";
                    sql = string.Format(sql, code);
                    DataTable dt;
                    csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
                    tbNameR.Text = dt.Rows[0]["drugname"].ToString();
                    tbSpecR.Text = dt.Rows[0]["drugspec"].ToString();
                    tbFactoryR.Text = dt.Rows[0]["drugfactory"].ToString();
                    //运动到预留位置
                    PLC_Tcp_CP.ChangeOut(1);
                    //获取药盒厚度
                    float drugThickness = 10;
                    sql = "select height from drug_infoannex where drugonlycode ='{0}'";
                    sql = string.Format(sql, code);
                    string s;
                    csSql.ExecuteScalar(sql, Config.Soft.ConnString, out s);
                    if (!string.IsNullOrEmpty(s))
                        drugThickness = float.Parse(s);
                    int pulse = Convert.ToInt32(num * drugThickness * Config.Mac_C.OneMMPulse_Plate);
                    PLC_Tcp_CP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Right, float.Parse(Config.Mac_C.Pulse_Plate_Max_Right) - pulse);
                }
            }
            else
            {
                if (type == PLC_Tcp_AP.PlateType.Left)
                {
                    plate_Left.DrugOnlyCode = "";
                    plate_Left.Num = 0;
                    tbNameL.Text = "";
                    tbSpecL.Text = "";
                    tbFactoryL.Text = "";
                    tbNumL.Text = "0";
                    btClearL.IsEnabled = false;
                    gd_L.Background = new SolidColorBrush(Colors.LightGreen);

                    PLC_Tcp_CP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Left, float.Parse(Config.Mac_C.Pulse_Plate_Min_Left));
                }
                else if (type == PLC_Tcp_AP.PlateType.Right)
                {
                    plate_Right.DrugOnlyCode = code;
                    plate_Right.Num = 0;
                    tbNameR.Text = "";
                    tbSpecR.Text = "";
                    tbFactoryR.Text = "";
                    tbNumR.Text = "0";
                    btClearR.IsEnabled = false;
                    gd_R.Background = new SolidColorBrush(Colors.LightGreen);

                    PLC_Tcp_CP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Right, float.Parse(Config.Mac_C.Pulse_Plate_Min_Right));
                }
            }
        }

        DispatcherTimer timer_Scan = new DispatcherTimer();
        string scanCode = "";
        bool scanNew = false;
        void timer_Scan_Tick(object sender, EventArgs e)
        {
            if (scanNew)
            {
                scanNew = false;
                ShowDrug(scanCode, true);
            }
        }
        void timer_Num_Tick(object sender, EventArgs e)
        {
            if (!IsBusy)
            {
                //ShowPlate(PLC_Tcp.PlateType.Left, "", 0);
                //ShowPlate(PLC_Tcp.PlateType.Right, "", 0);
                ShowPos(tbCode.Text.Trim(), false);
                Cursor = null;
                timer_Num.Stop();
            }
        }

        DispatcherTimer timer_Num = new DispatcherTimer();
        DispatcherTimer timer_PD = new DispatcherTimer();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Scanner.ThrowScan += new Scanner.ShowScan(ScanChanged);

            timer_Scan.Interval = TimeSpan.FromMilliseconds(200);
            timer_Scan.Tick += new EventHandler(timer_Scan_Tick);
            timer_Scan.Start();

            timer_Num.Interval = TimeSpan.FromMilliseconds(1000);
            timer_Num.Tick += new EventHandler(timer_Num_Tick);

            timer_PD.Interval = TimeSpan.FromSeconds(1);
            timer_PD.Tick += new EventHandler(timer_PD_Tick);

            plate_Left.DrugOnlyCode = "";
            plate_Left.Num = 0;
            plate_Left.PlateType = PLC_Tcp_AP.PlateType.Left;
            plate_Left.Batch = "";
            plate_Left.QZ = false;

            plate_Right.DrugOnlyCode = "";
            plate_Right.Num = 0;
            plate_Right.PlateType = PLC_Tcp_AP.PlateType.Right;
            plate_Right.Batch = "";
            plate_Right.QZ = false;

            Plates[0] = plate_Left;
            Plates[1] = plate_Right;

            Back();
        }

        private void tbPYCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowDrug(tbPYCode.Text.Trim(), false);
        }

        private void lvDrug_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvDrug.SelectedItems.Count > 0)
            {
                Drug d = lvDrug.SelectedItem as Drug;
                ShowSize(d.DrugOnlyCode);
                ShowPos(d.DrugOnlyCode, true);
            }
        }
        //清仓
        private void btClearL_Click(object sender, RoutedEventArgs e)
        {
            plate_Left.DrugOnlyCode = "";
            plate_Left.Num = 0;
            ShowPlate(PLC_Tcp_AP.PlateType.Left, "", 0);
        }
        private void btClearR_Click(object sender, RoutedEventArgs e)
        {
            plate_Right.DrugOnlyCode = "";
            plate_Right.Num = 0;
            ShowPlate(PLC_Tcp_AP.PlateType.Right, "", 0);
        }
        //故障返回
        private void btBack_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }
        private void Back()
        {
            SetErrorVisibility(Visibility.Hidden);

            ShowPlate(PLC_Tcp_AP.PlateType.Left, "", 0);

            ShowPlate(PLC_Tcp_AP.PlateType.Right, "", 0);

            PLC_Tcp_CP.ChangeAdd(1);
            //运行到接药口
            PLC_Tcp_CP.ExtramanAutoMoveToPulse(float.Parse(Config.Mac_C.Pulse_Meet_X), float.Parse(Config.Mac_C.Pulse_Meet_Z));
            //推药板复位
            PLC_Tcp_CP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Left, float.Parse(Config.Mac_C.Pulse_Plate_Min_Left));
            PLC_Tcp_CP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Right, float.Parse(Config.Mac_C.Pulse_Plate_Min_Right));
        }

        private void btPD_Click(object sender, RoutedEventArgs e)
        {
            PLC_Tcp_CP.LaserOn();
            toPD = true;

            PDItem();
            timer_PD.Start();
        }
        //加药
        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            upLeftIsOK = upRightIsOK = true;
            //推药板上移
            bool toUp = false;
            foreach (Plate p in Plates)
            {
                if (p.Num > 0)
                {
                    toUp = true;
                    break;
                }
            }
            if (toUp)
            {
                IsBusy = true;
                foreach (Plate p in Plates)
                {
                    if (p.Num > 0)
                    {
                        if (p.PlateType == PLC_Tcp_AP.PlateType.Left)
                        {
                            upLeftIsOK = false;
                            p.Batch = tbBatchL.Text.Trim();
                            p.QZ = (bool)chkBatchL.IsChecked;

                            PlateUp clspu = new PlateUp(p);
                            thUpL = new Thread(clspu.Up);
                            thUpL.IsBackground = true;
                            thUpL.Start();
                        }
                        else
                        {
                            upRightIsOK = false;
                            p.Batch = tbBatchR.Text.Trim();
                            p.QZ = (bool)chkBatchR.IsChecked;

                            PlateUp clspu = new PlateUp(p);
                            thUpR = new Thread(clspu.Up);
                            thUpR.IsBackground = true;
                            thUpR.Start();
                        }
                    }
                }

                DateTime timeBegin = DateTime.Now;
                //机械手加药
                while (!upLeftIsOK || !upRightIsOK)
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Up_Plate))
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
                if (upLeftIsOK && upRightIsOK)
                {
                    SetNumL(plate_Left.Num);
                    SetNumR(plate_Right.Num);

                    PLC_Tcp_CP.ChangeAdd(1);
                    //先设定当前位置
                    float pulse = PLC_Tcp_CP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Left);
                    PLC_Tcp_CP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Left, pulse);
                    pulse = PLC_Tcp_CP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Right);
                    PLC_Tcp_CP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Right, pulse);

                    bool toAdd = false;
                    foreach (Plate p in Plates)
                    {
                        if (p.Num > 0)
                        {
                            toAdd = true;
                            break;
                        }
                    }
                    if (toAdd)
                    {
                        Thread thAdd = new Thread(Add);
                        thAdd.IsBackground = true;
                        thAdd.Start();
                        //Add();
                    }
                    else
                    {
                        IsBusy = false;
                    }
                }
                else
                {
                    IsBusy = false;
                    Cursor = null;
                }
                if (IsBusy)
                    timer_Num.Start();
            }
            else
                Cursor = null;
        }

        class PlateUp
        {
            private Plate plate;

            public PlateUp(Plate p)
            {
                plate = p;
            }

            public void Up()
            {
                //获取药盒厚度
                double drugThickness = 10;
                string sql = "select height from drug_infoannex where drugonlycode='" + plate.DrugOnlyCode + "'";
                string s;
                csSql.ExecuteScalar(sql, Config.Soft.ConnString, out s);
                if (!string.IsNullOrEmpty(s))
                    drugThickness = double.Parse(s);

                PLC_Tcp_CP.ChangeAdd(2);
                //自动上推
                //PLC.PlateMoveUpBegin();
                PLC_Tcp_CP.PlateMoveUp(plate.PlateType);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(200);
                //等待就位
                while (!PLC_Tcp_CP.PlateMoveUpIsOK(plate.PlateType))
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Up_Plate))
                        return;
                    Thread.Sleep(200);
                }
                Thread.Sleep(100);
                //当前脉冲
                float nowPulse = PLC_Tcp_CP.ReadPlatePulse(plate.PlateType);
                //获取脉冲上限
                float maxPulse = 0f;
                if (plate.PlateType == PLC_Tcp_AP.PlateType.Left)
                {
                    maxPulse = float.Parse(Config.Mac_C.Pulse_Plate_Max_Left);
                }
                else
                    maxPulse = float.Parse(Config.Mac_C.Pulse_Plate_Max_Right);
                //计算仓内数量
                double sumHeight = Math.Round((maxPulse - nowPulse) * 1.0 / Config.Mac_C.OneMMPulse_Plate, 2);
                int boxNum = (int)Math.Round((double)sumHeight / drugThickness);
                plate.Num = boxNum;

                if (plate.PlateType == PLC_Tcp_AP.PlateType.Left)
                {
                    upLeftIsOK = true;
                }
                else
                {
                    upRightIsOK = true;
                }
            }
        }

        static bool upLeftIsOK;
        static bool upRightIsOK;

        Thread thUpL;
        Thread thUpR;
        //加药
        private void Add()
        {
            bool stop = false;
            if (plate_Left.Num > 0)
            {
                //查询缺药储位
                string sql = "select poscode from drug_pos where drugnummax>drugnum and (substring(poscode,1,1)<" + Config.Mac_C.Count_Unit + " or substring(poscode,4,2)<={2}) and maccode='{0}' and drugonlycode='{1}' order by (drugnummax-drugnum) desc";
                sql = string.Format(sql, Config.Soft.MacCode, plate_Left.DrugOnlyCode,Config.Mac_C.MaxCol);

                DataTable dt = new DataTable();
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
                List<string> poss = new List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    poss.Add(row["poscode"].ToString());
                }
                //获取左侧最短加药路径
                List<string> shortestPoss = poss;// GetShortestPath(poss, "A", "L");

                //int haveNum = plate_Left.Num;
                string drugOnlyCode = plate_Left.DrugOnlyCode;
                foreach (string pos in shortestPoss)
                {
                    if (plate_Left.Num > 0)
                    {
                        //先盘点
                        if (Config.Mac_C.PDBeforeAdd == "Y")
                        {
                            int n = PLC_Tcp_CP.PDNum(pos, false);
                            if (n >= 0)
                            {
                                sql = "update drug_pos set drugnum='{0}' where maccode='{1}' and poscode='{2}'";
                                sql = string.Format(sql, n, Config.Soft.MacCode, pos);
                                csSql.ExecuteSql(sql, Config.Soft.ConnString);
                            }
                        }
                        sql = "select (drugnummax-drugnum) from drug_pos where maccode='{0}' and poscode='{1}'";
                        sql = string.Format(sql, Config.Soft.MacCode, pos);
                        int shortNum = 0; string v;
                        csSql.ExecuteScalar(sql, Config.Soft.ConnString, out v);
                        if (!string.IsNullOrEmpty(v))
                            shortNum = int.Parse(v);
                        int addNum = 0;
                        if (shortNum > 0)
                        {
                            int n = int.Parse(v);
                            if (plate_Left.Num > n)
                            {
                                addNum = n;
                                //plate_Left.Num -= n;
                            }
                            else
                            {
                                addNum = plate_Left.Num;
                                //haveNum = 0;
                            }
                        }
                        int a = Add(pos, "L", addNum, plate_Left.Batch, plate_Left.QZ, ref stop);
                        plate_Left.Num -= a;
                        SetNumL(plate_Left.Num);

                        if (stop)
                            break;
                    }
                    else
                        break;
                }
                if (!stop)
                {
                    //推药板复位
                    //PLC_Tcp.ChangeAdd(1);
                    PLC_Tcp_CP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Left, float.Parse(Config.Mac_C.Pulse_Plate_Min_Left));
                }
                plate_Left.DrugOnlyCode = "";
                plate_Left.Num = 0;
            }
            if (plate_Right.Num > 0 && !stop)
            {
                //查询缺药储位
                string sql = "select poscode from drug_pos where drugnummax>drugnum and (substring(poscode,1,1)>1 or substring(poscode,4,2)>={2}) and maccode='{0}' and drugonlycode='{1}' order by (drugnummax-drugnum) desc";
                sql = string.Format(sql, Config.Soft.MacCode, plate_Right.DrugOnlyCode,Config.Mac_C.MinCol);

                DataTable dt = new DataTable();
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
                List<string> poss = new List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    poss.Add(row["poscode"].ToString());
                }
                //获取右侧最短加药路径
                List<string> shortestPoss = poss;// GetShortestPath(poss, "A", "R");

                //int haveNum = plate_Right.Num;
                string drugOnlyCode = plate_Right.DrugOnlyCode;
                foreach (string pos in shortestPoss)
                {
                    if (plate_Right.Num > 0)
                    {
                        //先盘点
                        if (Config.Mac_C.PDBeforeAdd == "1")
                        {
                            int n = PLC_Tcp_CP.PDNum(pos, false);
                            if (n >= 0)
                            {
                                sql = "update drug_pos set drugnum='{0}' where maccode='{1}' and poscode='{2}'";
                                sql = string.Format(sql, n, Config.Soft.MacCode, pos);
                                csSql.ExecuteSql(sql, Config.Soft.ConnString);
                            }
                        }
                        sql = "select (drugnummax-drugnum) from drug_pos where maccode='{0}' and poscode='{1}'";
                        sql = string.Format(sql, Config.Soft.MacCode, pos);
                        int shortNum = 0; string v;
                        csSql.ExecuteScalar(sql, Config.Soft.ConnString, out v);
                        if(!string.IsNullOrEmpty(v))
                            shortNum = int.Parse(v);
                        int addNum = 0;
                        if (shortNum > 0)
                        {
                            int n = int.Parse(v);
                            if (plate_Right.Num > n)
                            {
                                addNum = n;
                                //plate_Right.Num -= n;
                            }
                            else
                            {
                                addNum = plate_Right.Num;
                                //haveNum = 0;
                            }
                        }
                        int a = Add(pos, "R", addNum, plate_Right.Batch, plate_Right.QZ, ref stop);
                        //haveNum -= a;
                        plate_Right.Num -= a;
                        SetNumR(plate_Right.Num);

                        if (stop)
                            break;
                    }
                    else
                        break;
                } 
                if (!stop)
                {
                    //推药板复位
                    //PLC_Tcp.ChangeAdd(1);
                    PLC_Tcp_CP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Right, float.Parse(Config.Mac_C.Pulse_Plate_Min_Right));
                }
                plate_Right.DrugOnlyCode = "";
                plate_Right.Num = 0;
            }
            IsBusy = false;

            //ShowPlate(PLC.PlateType.Left, "", 0);
            //ShowPlate(PLC.PlateType.Right, "", 0);
            //ShowPos(tbCode.Text.Trim());

            if (!stop)
                PLC_Tcp_CP.ExtramanAutoMoveToPulse(float.Parse(Config.Mac_C.Pulse_Meet_X), float.Parse(Config.Mac_C.Pulse_Meet_Z));
        }

        public void SetNumL(int n)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { tbNumL.Text = n.ToString(); }); 
        }
        public void SetNumR(int n)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { tbNumR.Text = n.ToString(); });
        }

        public void SetErrorVisibility(Visibility v)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { tbError.Visibility = v; });
        }

        List<List<string>> paths = new List<List<string>>();

        //计算最短路径排列
        private List<string> GetShortestPath(List<string> poss, string type, string dir)
        {
            List<string> result = new List<string>();
            List<List<string>> paths = new List<List<string>>();
            List<string> possTemp = new List<string>();
            GetPaths(poss, ref paths, ref possTemp);
            double p = 0;
            foreach (List<string> ps in paths)
            {
                double path = 0;
                string inpos = "";
                foreach (string pos in ps)
                {
                    inpos += "'" + pos + "',";
                }
                if (inpos.Length > 0)
                {
                    inpos = inpos.Substring(0, inpos.Length - 1);
                }
                string sql = "select poscode,pulsex,pulsez from pos_pulse where maccode='{0}' and pulsetype='{1}' and pulselr='{2}' and poscode in ({3})";
                sql = string.Format(sql, Config.Soft.MacCode, type, dir, inpos);
                DataTable dt;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
                //接药口到第一个储位距离
                string poscode = ps[0];
                DataRow[] rows = dt.Select("poscode='" + poscode + "'");
                if (rows.Length > 0)
                    path += Math.Sqrt(Math.Pow(Math.Abs(float.Parse(rows[0]["pulsex"].ToString()) - float.Parse(Config.Mac_C.Pulse_Meet_X)), 2) + Math.Pow(Math.Abs(float.Parse(rows[0]["pulsez"].ToString()) - float.Parse(Config.Mac_C.Pulse_Meet_Z)), 2));
                for (int i = 0; i < ps.Count - 2; i++)
                {
                    string p1 = ps[i];
                    DataRow[] rows1 = dt.Select("poscode='" + p1 + "'");
                    string p2 = ps[i + 1];
                    DataRow[] rows2 = dt.Select("poscode='" + p2 + "'");
                    if (rows1.Length > 0 && rows2.Length > 0)
                        path += Math.Sqrt(Math.Pow(Math.Abs(float.Parse(rows1[0]["pulsex"].ToString()) - float.Parse(rows2[0]["pulsex"].ToString())), 2) + Math.Pow(Math.Abs(float.Parse(rows1[0]["pulsez"].ToString()) - float.Parse(rows2[0]["pulsez"].ToString())), 2));
                }

                if (p == 0)
                {
                    p = path;
                    result = ps;
                }
                else if (path < p)
                {
                    p = path;
                    result = ps;
                }
            }
            return result;
        }

        //添加排列
        private void GetPaths(List<string> poss, ref List<List<string>> paths, ref List<string> possTemp)
        {
            if (poss.Count == 1)
            {
                possTemp.Add(poss[0]);
                paths.Add(possTemp);
            }
            else if (poss.Count > 1)
            {
                for (int i = poss.Count - 1; i >= 0; i--)
                {
                    possTemp.Add(poss[i]);
                    List<string> pt = possTemp.ToList();
                    possTemp.Remove(poss[i]);
                    List<string> ps = poss.ToList();
                    ps.RemoveAt(i);
                    GetPaths(ps, ref paths, ref pt);
                }
            }
        }

        //指定储位加药
        private int Add(string posCode, string dir, int num, string batch, bool qz, ref bool stopAdd)
        {
            int result = 0;
            string sql = "select pulsex,pulsez from pos_pulse where maccode='{0}' and poscode='{1}' and pulselr='{2}' and pulsetype='A'";
            sql = string.Format(sql, Config.Soft.MacCode, posCode, dir);
            DataTable dt;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
            if (dt.Rows.Count <= 0)
            {
                return 0;
            }
            float targetX = float.Parse(dt.Rows[0][0].ToString());
            float targetZ = float.Parse(dt.Rows[0][1].ToString());

            //PLC.ChangeAdd(1);
            //运行到加药位置
            if (PLC_Tcp_CP.ExtramanAutoMoveToPulse(targetX, targetZ))
            {
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(200);
                while (!PLC_Tcp_CP.ExtramanAutoMoveIsOK())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Extraman))
                    {
                        csMsg.ShowWarning("加药定位失败", false);
                        return 0;
                    }
                    Thread.Sleep(200);
                }
            }
            else
            {
                csMsg.ShowWarning("加药定位指令发送失败", false);
                return 0;
            }

            float maxPulse = float.Parse(Config.Mac_C.Pulse_Plate_Max_Left);
            PLC_Tcp_AP.PlateType type = PLC_Tcp_AP.PlateType.Left;

            if (dir == "R")
            {
                type = PLC_Tcp_AP.PlateType.Right;
                maxPulse = float.Parse(Config.Mac_C.Pulse_Plate_Max_Right);
            }

            float nowPulse = PLC_Tcp_CP.ReadPlatePulse(type);
            //计数清零
            PLC_Tcp_CP.ResetPlateRecord(type);

            int oldRecord = 0, newRecord = 0;
            stopAdd = false;

            //获取药盒厚度
            float drugThickness = 10;
            sql = "select height from drug_infoannex where drugonlycode in(select drugonlycode from drug_pos where poscode='{0}')";
            sql = string.Format(sql, posCode);
            string s;
            csSql.ExecuteScalar(sql, Config.Soft.ConnString, out s);
            if (!string.IsNullOrEmpty(s))
                drugThickness = float.Parse(s);
            //上推脉冲
            float upPulse = Convert.ToInt32(drugThickness * Config.Mac_C.OneMMPulse_Plate);
            //补偿脉冲值
            //int bcPulse = 500;

            for (int i = 0; i < num; )
            {
                //有障碍物，暂停
                //int z = PLC.PlateErrorZHA();
                //while (z == 1 || z == 2)
                //{
                //    Thread.Sleep(200);
                //    if (z == 1)
                //        ThrowMsg("左侧加药手有障碍物");
                //    else if (z == 1)
                //        ThrowMsg("右侧加药手有障碍物");
                //    z = PLC.PlateErrorZHA();
                //}

                //加药+补偿
                for (int j = 1; j <= (Config.Mac_C.Count_BC+1); j++)
                {
                    if (j == 1)
                        nowPulse += upPulse;
                    else
                        nowPulse += Config.Mac_C.OneMMPulse_Plate * (drugThickness * Config.Mac_C.Height_BC);

                    if (nowPulse > maxPulse)
                    {
                        nowPulse = maxPulse;
                    }
                    //上推
                    PLC_Tcp_CP.PlateAutoMoveToPulse(type, nowPulse);
                    DateTime timeBegin = DateTime.Now;
                    Thread.Sleep(100);
                    while (!PLC_Tcp_CP.PlateAutoMoveToPulseIsOK(type))
                    {
                        if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Plate))
                        {
                            csMsg.ShowWarning("推板定位失败", false);
                            break;
                        }
                        Thread.Sleep(100);
                    }
                    //上推完成，判断计数
                    if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Plate))
                    {
                        DateTime t1 = DateTime.Now;
                        Thread.Sleep(200);
                        while (true)
                        {
                            newRecord = PLC_Tcp_CP.ReadPlateRecord(type);
                            if (newRecord > oldRecord)
                                break;
                            else if (DateTime.Now > t1.AddSeconds(2))
                                break;
                            Thread.Sleep(200);
                        }
                        if (newRecord > oldRecord)
                        {
                            oldRecord = newRecord;

                            if (dir == "L")
                            {
                                SetNumL(plate_Left.Num - newRecord);
                            }
                            else
                            {
                                SetNumR(plate_Right.Num - newRecord);
                            }

                            break;
                        }
                        else
                        {
                            if (j < (Config.Mac_C.Count_BC + 1))
                                continue;
                            else
                            {
                                //故障
                                SetErrorVisibility(Visibility.Visible);
                                stopAdd = true; 
                            }
                        }
                    }
                    else
                        break;
                }
                i = newRecord;
                if (nowPulse >= maxPulse)
                    break;
                if (stopAdd)
                    break;
            }
            result = newRecord;

            string drugBatch = "";
            if (qz)
                drugBatch = batch;
            else
            {
                sql = "select * from drug_pos where (drugbatch<'{0}' or drugbatch is null) and maccode='{1}' and poscode='{2}' order by drugbatch";
                sql = string.Format(sql, batch, Config.Soft.MacCode, posCode);
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
                if (dt.Rows.Count > 0)
                    drugBatch = dt.Rows[0]["drugbatch"].ToString().Trim();
                else
                    drugBatch = batch;
            }
            //更新储位库存
            s = "update drug_pos set drugnum=(drugnum+{0}),drugbatch='{1}' where maccode='{2}' and poscode='{3}';";
            sql = string.Format(s, result, drugBatch, Config.Soft.MacCode, posCode);
            //增加加药明细
            s = "insert into drug_import select '{0}',drugonlycode,'{1}','I','{2}',drugunit,'{3}',getdate(),'' from drug_pos where maccode='{0}' and poscode='{4}';";
            sql += string.Format(s, Config.Soft.MacCode, batch, result, Config.Soft.UserCode, posCode);

            csSql.ExecuteSql(sql, Config.Soft.ConnString);

            return result;
        }

        private void tbPYCode_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ShowKey(true);
            //csKey.Show(200, 400);
        }

        bool toPD = false;
        private void timer_PD_Tick(object sender, EventArgs e)
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
                if (index > lvPos.Items.Count - 10) lvPos.ScrollIntoView(lvPos.Items[lvPos.Items.Count - 1]);
                else lvPos.ScrollIntoView(lvPos.Items[index + 5]);

                int n = PLC_Tcp_CP.PDNum(p.PosCode, false);
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
                        string sql = "insert into sys_error values('{0}','{1}','{2}','PE',{3},{4},getdate())";
                        sql = string.Format(sql, Config.Soft.MacCode, p.PosCode, p.DrugOnlyCode, num, n);
                        csSql.ExecuteSql(sql, Config.Soft.ConnString);
                    }
                    string s = "update drug_pos set drugnum={2} where maccode='{0}' and poscode='{1}'";
                    s = string.Format(s, Config.Soft.MacCode, p.PosCode, p.PDNum);
                    csSql.ExecuteSql(s, Config.Soft.ConnString);
                    p.DrugNum = n.ToString();
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
                PLC_Tcp_CP.ExtramanAutoMoveToPulse(float.Parse(Config.Mac_C.Pulse_Meet_X), float.Parse(Config.Mac_C.Pulse_Meet_Z));
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer_Num.Stop(); timer_PD.Stop(); timer_Scan.Stop();
        }
    }
}
