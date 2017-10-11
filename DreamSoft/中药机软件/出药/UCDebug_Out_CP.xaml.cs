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
using DreamSoft.Class;
using System.Threading;
using System.Windows.Threading;
using System.Data;

namespace DreamSoft
{
    /// <summary>
    /// WinDebug_Out.xaml 的交互逻辑
    /// </summary>
    public partial class UCDebug_Out_CP : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();
        //CSHelper.TKey csKey = new CSHelper.TKey();

        public delegate void SetKey(bool show);
        public static SetKey ShowKey;

        public UCDebug_Out_CP()
        {
            InitializeComponent();
        }

        DispatcherTimer timer_Presc = new DispatcherTimer();
        DispatcherTimer timer_Test = new DispatcherTimer();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer_Presc.Interval = TimeSpan.FromSeconds(3);
            timer_Presc.Tick += new EventHandler(timer_Presc_Tick);

            timer_Test.Tick += new EventHandler(timer_Test_Tick);

            cbLayer.SelectedIndex = 0;
            if (Config.Mac_C.PLC_Tcp == "Y")
                tbPulse_Lift_Now.Text = PLC_Tcp_CP.ReadLiftPulse().ToString();
            if (Config.Mac_C.PLC_Com == "Y")
                tbPulse_Baffle_Lift_Now.Text = PLC_Com_CP.ReadBafflePulse_Lift().ToString();

            tbTop.Text = Config.Mac_C.Pulse_Lift_Top;
            tbUp.Text = Config.Mac_C.Pulse_Lift_Up;
            tbDown.Text = Config.Mac_C.Pulse_Lift_Down;
            tbMeet.Text = Config.Mac_C.Pulse_Lift_Meet;

            tbOpen.Text = Config.Mac_C.Pulse_Baffle_Open;
            tbClose.Text = Config.Mac_C.Pulse_Baffle_Close;

            if (Config.Mac_C.ShowTest == "Y")
                gbTest.Visibility = Visibility.Visible;
            else gbTest.Visibility = Visibility.Hidden;
        }

        private void btTransfer_Turn_Click(object sender, RoutedEventArgs e)
        {
            btBelt_Out_Start.IsEnabled = false; btBelt_Out_Stop.IsEnabled = true;
            PLC_Tcp_CP.TransferBeltMove(PLC_Tcp_AP.TransferBeltMoveType.Right);
        }
        private void btTransfer_Stop_Click(object sender, RoutedEventArgs e)
        {
            btBelt_Out_Start.IsEnabled = true; btBelt_Out_Stop.IsEnabled = false;
            PLC_Tcp_CP.TransferBeltMove(PLC_Tcp_AP.TransferBeltMoveType.Stop);
        }

        private void btTop_Turn_Click(object sender, RoutedEventArgs e)
        {
            btBelt_Win_Start.IsEnabled = false; btBelt_Win_Stop.IsEnabled = true;
            PLC_Tcp_CP.TopBeltMove(PLC_Tcp_AP.TopBeltMoveType.Turn);
        }
        private void btTop_Stop_Click(object sender, RoutedEventArgs e)
        {
            btBelt_Win_Start.IsEnabled = true; btBelt_Win_Stop.IsEnabled = false;
            PLC_Tcp_CP.TopBeltMove(PLC_Tcp_AP.TopBeltMoveType.Stop);
        }
       
        private void btZero_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            if (PLC_Tcp_CP.LiftOriginReset())
            {
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(1000);

                while(! PLC_Tcp_CP.LiftOriginResetIsOK())
                {
                    if(DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Reset_Lift))
                        break;
                    Thread.Sleep(500);
                }

                if (PLC_Tcp_CP.LiftOriginResetIsOK())
                {
                    csMsg.ShowInfo("原点返回完成", false);
                    tbPulse_Lift_Now.Text = PLC_Tcp_CP.ReadLiftPulse().ToString();
                }
                else
                    csMsg.ShowWarning("原点返回失败", false);
            }
            else
                csMsg.ShowWarning("指令发送失败", false);
            Cursor = null;
        }

        private void btLift_Up_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_CP.ChangeOut(0);
            PLC_Tcp_CP.LiftManualMove(PLC_Tcp_AP.LiftMoveDir.Up);
        }
        private void btLift_Down_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_CP.ChangeOut(0);
            PLC_Tcp_CP.LiftManualMove(PLC_Tcp_AP.LiftMoveDir.Down);
        }
        private void btLift_Stop_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_CP.LiftManualMove(PLC_Tcp_AP.LiftMoveDir.Stop);
            tbPulse_Lift_Now.Text = PLC_Tcp_CP.ReadLiftPulse().ToString();
        }

        private void btLift_Run_Top_Click(object sender, RoutedEventArgs e)
        {
            string p = tbTop.Text.Trim();
            if (!string.IsNullOrEmpty(p))
            {
                Lift_Run_Auto(float.Parse(p));
            }
        }
        private void btLift_Run_Up_Click(object sender, RoutedEventArgs e)
        {
            string p = tbUp.Text.Trim();
            if (!string.IsNullOrEmpty(p))
            {
                Lift_Run_Auto(float.Parse(p));
            }
        }
        private void btLift_Run_Down_Click(object sender, RoutedEventArgs e)
        {
            string p = tbDown.Text.Trim();
            if (!string.IsNullOrEmpty(p))
            {
                Lift_Run_Auto(float.Parse(p));
            }
        }
        private void btLift_Run_Meet_Click(object sender, RoutedEventArgs e)
        {
            string p = tbMeet.Text.Trim();
            if (!string.IsNullOrEmpty(p))
            {
                Lift_Run_Auto(float.Parse(p));
            }
        }
        private void Lift_Run_Auto(float p)
        {
            Cursor = Cursors.Wait;
            PLC_Tcp_CP.ChangeOut(1);
            PLC_Tcp_CP.LiftAutoMoveByPulse(p);
            DateTime timeBegin = DateTime.Now;
            Thread.Sleep(200);
            while (!PLC_Tcp_CP.LiftAutoMoveIsOK())
            {
                if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Lift))
                    break;
                Thread.Sleep(200);
            }
            if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Lift))
                tbPulse_Lift_Now.Text = PLC_Tcp_CP.ReadLiftPulse().ToString();
            else csMsg.ShowWarning("提升机未运行到指定位置", false);
            Cursor = null;
        }
        private void btLift_Save_Top_Click(object sender, RoutedEventArgs e)
        {
            float p = PLC_Tcp_CP.ReadLiftPulse();
            Config.SaveConfig(Config.Soft.MacCode, "Pulse_Lift_Top", p.ToString());
            tbTop.Text = p.ToString();
        }
        private void btLift_Save_Up_Click(object sender, RoutedEventArgs e)
        {
            float p = PLC_Tcp_CP.ReadLiftPulse();
            Config.SaveConfig(Config.Soft.MacCode, "Pulse_Lift_Up", p.ToString());
            tbUp.Text = p.ToString();
        }
        private void btLift_Save_Down_Click(object sender, RoutedEventArgs e)
        {
            float p = PLC_Tcp_CP.ReadLiftPulse();
            Config.SaveConfig(Config.Soft.MacCode, "Pulse_Lift_Down", p.ToString());
            tbDown.Text = p.ToString();
        }
        private void btLift_Save_Meet_Click(object sender, RoutedEventArgs e)
        {
            float p = PLC_Tcp_CP.ReadLiftPulse();
            Config.SaveConfig(Config.Soft.MacCode, "Pulse_Lift_Meet", p.ToString());
            tbMeet.Text = p.ToString();
        }

        private void btPos_DCT_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Button b = sender as Button;
            string unit = cbUnit.Text;
            string lay = cbLayer.Text;
            string col = b.Tag.ToString();
            string pos = unit + lay + col;
            if (e.ChangedButton == MouseButton.Right)
            {
                int record = PLC_Tcp_CP.ReadRecordSingle(pos);
                b.Content = b.Tag.ToString() + ":  " + record.ToString();
            }
            else if (e.ChangedButton == MouseButton.Middle)
            {
                PLC_Tcp_CP.ClearRecordSingle(pos);
                int record = PLC_Tcp_CP.ReadRecordSingle(pos);
                b.Content = b.Tag.ToString() + ":  " + record.ToString();

            }
        }

        private void btPos_DCT_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string unit = cbUnit.Text;
            string lay = cbLayer.Text;
            string col = b.Tag.ToString();
            string pos = unit + lay + col;
            PLC_Tcp_CP.DCTMoveDownSingle(pos);
        }
        private void btPos_ReadRecord_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string unit = cbUnit.Text;
            string lay = cbLayer.Text;
            string col = b.Tag.ToString();
            string pos = unit + lay + col;
            b.Content = PLC_Tcp_CP.ReadRecordSingle(pos);
        }
        private void btPos_ClearRecord_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string unit = cbUnit.Text;
            string lay = cbLayer.Text;
            string col = b.Tag.ToString();
            string pos = unit + lay + col;
            PLC_Tcp_CP.ClearRecordSingle(pos);
        }

        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            btPresc_Start.IsEnabled = false; btPresc_Stop.IsEnabled = true;
            timer_Presc.Start();
        }
        private void btStop_Click(object sender, RoutedEventArgs e)
        {
            btPresc_Start.IsEnabled = true; btPresc_Stop.IsEnabled = false;
            timer_Presc.Stop();
        }

        void timer_Presc_Tick(object sender, EventArgs e)
        {
            string prescNo = Guid.NewGuid().ToString();
            string payTime = DateTime.Now.ToString();

            string sql = "select distinct drugonlycode from drug_pos where maccode='{0}'";
            sql = string.Format(sql, Config.Soft.MacCode);
            DataTable dtDrug;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtDrug);
            if (dtDrug != null && dtDrug.Rows.Count > 0)
            {
                string win = "1"; string name = GetName();
                string w = string.Format("select windowno from sys_window_mac where maccode='{0}'", Config.Soft.MacCode);
                DataTable dtWin; csSql.ExecuteSelect(w, Config.Soft.ConnString, out dtWin);
                if (dtWin != null && dtWin.Rows.Count > 0)
                {
                    Random rd = new Random();
                    win = dtWin.Rows[rd.Next(0, dtWin.Rows.Count)][0].ToString().Trim();
                }
                string s = "insert into pat_prescinfo (prescno,windowno,mrno,patname,depname,paytime,doflag,createtime) values ('{0}',{1},'001','{2}','科室A','{3}','W',getdate());";
                sql = string.Format(s, prescNo, win, name, payTime);
                //默认种数
                int num = Config.Mac_C.DrugCount;
                int m = dtDrug.Rows.Count;
                if (m < num)
                    num = m;

                List<string> ds = new List<string>();
                //随机出药
                for (int i = 1; i <= num; i++)
                {
                    Random rd = new Random();
                    int n = rd.Next(0, m);
                    string drugonlycode = dtDrug.Rows[n]["drugonlycode"].ToString().Trim();
                    if (!ds.Contains(drugonlycode))
                        ds.Add(drugonlycode);
                    else
                    {
                        i--;
                        continue;
                    }
                    int count = rd.Next(1, Config.Mac_C.DrugNum);
                    s = "insert into pat_druginfo(prescno,drugonlycode,drugnum,drugunit,doflag) select '{0}','{1}','{2}',drugpackunit,'N' from drug_info where drugonlycode='{1}'";
                    sql += string.Format(s, prescNo, drugonlycode, count);
                }
                if(csSql.ExecuteSql(sql, Config.Soft.ConnString))
                    csMsg.ShowInfo("生成处方", false);
            }
            else
                csMsg.ShowWarning("本机未分配任何药品", false);
        }

        private void btZero_Baffle_Lift_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            if (PLC_Com_CP.BaffleOriginReset_Lift())
            {
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(1000);

                while (!PLC_Com_CP.BaffleOriginResetIsOK())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Reset_Baffle))
                        break;
                    Thread.Sleep(500);
                }

                if (PLC_Com_CP.BaffleOriginResetIsOK())
                {
                    csMsg.ShowInfo("原点返回完成", false); 
                    tbPulse_Baffle_Lift_Now.Text = PLC_Com_CP.ReadBafflePulse_Lift().ToString();
                }
                else
                    csMsg.ShowWarning("原点返回失败", false);
            }
            else
                csMsg.ShowWarning("指令发送失败", false);
            Cursor = null;
        }

        private void btBaffle_Lift_Up_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Com_CP.Baffle_Change(0);
            PLC_Com_CP.BaffleMove_Lift(PLC_Com_AP.BaffleMoveType_Lift.Up);
        }
        private void btBaffle_Lift_Down_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Com_CP.Baffle_Change(0);
            PLC_Com_CP.BaffleMove_Lift(PLC_Com_AP.BaffleMoveType_Lift.Down);
        }
        private void btBaffle_Lift_Stop_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PLC_Com_CP.BaffleMove_Lift(PLC_Com_AP.BaffleMoveType_Lift.Stop);
            tbPulse_Baffle_Lift_Now.Text = PLC_Com_CP.ReadBafflePulse_Lift().ToString();
        }

        private void btBaffle_Lift_Run_Open_Click(object sender, RoutedEventArgs e)
        {
            string p = tbOpen.Text.Trim();
            if (!string.IsNullOrEmpty(p))
            {
                int n;
                if (int.TryParse(p, out n))
                    Baffle_Lift_AutoRun(n);
                else
                    Msg.ShowMsg("目标脉冲的格式不正确", Colors.Red);
            }
        }
        private void btBaffle_Lift_Run_Close_Click(object sender, RoutedEventArgs e)
        {
            string p = tbClose.Text.Trim();
            if (!string.IsNullOrEmpty(p))
            {
                int n;
                if (int.TryParse(p, out n))
                    Baffle_Lift_AutoRun(n);
                else
                    Msg.ShowMsg("目标脉冲的格式不正确", Colors.Red);
            }
        }
        private void Baffle_Lift_AutoRun(int pulse)
        {
            Cursor = Cursors.Wait;
            //PLC_Tcp.ChangeOut(1);
            PLC_Com_CP.Baffle_Change(1);

            int pulse_Now = PLC_Com_CP.ReadBafflePulse_Lift();
            if (pulse > pulse_Now)
            {
                PLC_Com_CP.BaffleAutoMoveByPulse_Up_Lift(pulse);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(500);
                while (!PLC_Com_CP.BaffleAutoMoveIsOK_Up())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Baffle))
                        break;
                    Thread.Sleep(500);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Baffle))
                    tbPulse_Baffle_Lift_Now.Text = pulse.ToString();
                else csMsg.ShowWarning("挡板未运行到指定位置", false);
            }
            else
            {
                PLC_Com_CP.BaffleAutoMoveByPulse_Down_Lift(pulse);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(500);
                while (!PLC_Com_CP.BaffleAutoMoveIsOK_Down())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Baffle))
                        break;
                    Thread.Sleep(500);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Baffle))
                    tbPulse_Baffle_Lift_Now.Text = pulse.ToString();
                else csMsg.ShowWarning("挡板未运行到指定位置", false);
            }
            Cursor = null;
        }
        private void btBaffle_Lift_Save_Open_Click(object sender, RoutedEventArgs e)
        {
            int p = PLC_Com_CP.ReadBafflePulse_Lift();
            Config.SaveConfig(Config.Soft.MacCode, "Pulse_Baffle_Open", p.ToString());
            tbOpen.Text = p.ToString();
        }
        private void btBaffle_Lift_Save_Close_Click(object sender, RoutedEventArgs e)
        {
            int p = PLC_Com_CP.ReadBafflePulse_Lift();
            Config.SaveConfig(Config.Soft.MacCode, "Pulse_Baffle_Close", p.ToString());
            tbClose.Text = p.ToString();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer_Presc.Stop();
        }

        int num = 0;
        private void Test()
        {
            string unit = cbUnit.Text;
            string lay = cbLayer.Text;
            string record = "";
            for (int i = 23; i >= 1; i--)
            {
                string pos = unit + lay + i.ToString().PadLeft(2, '0');
                PLC_Tcp_CP.DCTMoveDownSingle(pos);
                Thread.Sleep(100);
                record += PLC_Tcp_CP.ReadRecordSingle(pos) + " ";
                Thread.Sleep(100);
            }
            tbRecord.Text = record;
            num++;
            tbNum.Text = num.ToString();
        }
        void timer_Test_Tick(object sender, EventArgs e)
        {
            Test();
        }
        private void btTest_Start_Click(object sender, RoutedEventArgs e)
        {
            Test();
            timer_Test.Interval = TimeSpan.FromSeconds(20);
            timer_Test.Start();
        }
        private void btTest_Stop_Click(object sender, RoutedEventArgs e)
        {
            timer_Test.Stop();
        }


        #region"随机姓名"
        string[] FirstNames = new string[]{
                               "赵","钱","孙","李","周","吴","郑","王","冯","陈","褚","卫","蒋","沈","韩","杨","朱","秦","尤","许",
                               "何","吕","施","张","孔","曹","严","华","金","魏","陶","姜","戚","谢","邹","喻","柏","水","窦","章",
                               "云","苏","潘","葛","奚","范","彭","郎","鲁","韦","昌","马","苗","凤","花","方","俞","任","袁","柳",
                               "酆","鲍","史","唐","费","廉","岑","薛","雷","贺","倪","汤","滕","殷","罗","毕","郝","邬","安","常",
                               "乐","于","时","傅","皮","卞","齐","康","伍","余","元","卜","顾","孟","平","黄","和","穆","萧","尹",
                               "姚","邵","湛","汪","祁","毛","禹","狄","米","贝","明","臧","计","伏","成","戴","谈","宋","茅","庞",
                               "熊","纪","舒","屈","项","祝","董","梁","杜","阮","蓝","闵","席","季","麻","强","贾","路","娄","危",
                               "江","童","颜","郭","梅","盛","林","刁","钟","徐","邱","骆","高","夏","蔡","田","樊","胡","凌","霍",
                               "虞","万","支","柯","昝","管","卢","莫","经","房","裘","缪","干","解","应","宗","丁","宣","贲","邓",
                               "郁","单","杭","洪","包","诸","左","石","崔","吉","钮","龚","程","嵇","邢","滑","裴","陆","荣","翁",
                               "荀","羊","於","惠","甄","曲","家","封","芮","羿","储","靳","汲","邴","糜","松","井","段","富","巫",
                               "乌","焦","巴","弓","牧","隗","山","谷","车","侯","宓","蓬","全","郗","班","仰","秋","仲","伊","宫",
                               "宁","仇","栾","暴","甘","钭","厉","戎","祖","武","符","刘","景","詹","束","龙","叶","幸","司","韶",
                               "郜","黎","蓟","薄","印","宿","白","怀","蒲","台","丛","鄂","索","咸","籍","赖","卓","蔺","屠","蒙",
                               "池","乔","阴","郁","胥","能","苍","双","闻","莘","党","翟","谭","贡","劳","逄","姬","申","扶","堵",
                               "冉","宰","郦","雍","却","璩","桑","桂","濮","牛","寿","通","边","扈","燕","冀","郏","浦","尚","农",
                               "温","别","庄","晏","柴","瞿","阎","充","慕","连","茹","习","宦","艾","鱼","容","向","古","易","慎",
                               "戈","廖","庚","终","暨","居","衡","步","都","耿","满","弘","匡","国","文","寇","广","禄","阙","东",
                               "殴","殳","沃","利","蔚","越","夔","隆","师","巩","厍","聂","晁","勾","敖","融","冷","訾","辛","阚",
                               "那","简","饶","空","曾","毋","沙","乜","养","鞠","须","丰","巢","关","蒯","相","查","后","荆","红",
                               "游","竺","权逯","盖益","桓","公","万俟","司马","上官","欧阳","夏侯","诸葛",
                               "闻人","东方","赫连","皇甫","尉迟","公羊","澹台","公冶","宗政","濮阳",
                               "淳于","单于","太叔","申屠","公孙","仲孙","轩辕","令狐","钟离","宇文",
                               "长孙","慕容","鲜于","闾丘","司徒","司空","亓官","司寇","仉","督","子车",
                               "颛孙","端木","巫马","公西","漆雕","乐正","壤驷","公良","拓跋","夹谷",
                               "宰父","谷粱","晋","楚","闫","法","汝","鄢","涂","钦","段干","百里","东郭","南门",
                               "呼延","归海","羊舌","微生","岳","帅","缑","亢","况","郈","有","琴","梁丘","左丘",
                               "东门","西门","商","牟","佘","佴","伯","赏","宫","墨","哈","谯","笪","年","爱","阳","佟",
                               "第五","言福"       
                             };
        string[] SecondNames = new string[]{
                                  "白", "赤", "凉", "靖", "剑", "谙", "仪", "翔", "遐", "翚", "桓", "鸠", "梅", "美", "笛", "古",
                                  "弘", "勋", "秀", "晴", "子", "竞", "溢", "澜", "云", "启", "宣", "恭", "劲", "聪", "冀", "洪", 
                                  "景", "炎", "昌", "久", "零", "落", "千", "言", "弼", "光", "缘", "逸", "欣", "宥", "远", "霞", 
                                  "碧", "空", "长", "虹", "耀", "月", "鹏", "飞", "宗", "翰", "毓", "灵", "星", "辉", "辅", "国", 
                                  "靖", "初", "君", "让", "昭", "寒", "攸", "讳", "天", "佑", "晨", "曦", "北", "辰", "敬", "弦", 
                                  "起", "乾", "承", "嗣", "云", "啸", "海", "潜", "百", "炼", "万", "言", "炳", "之", "语", "晴", 
                                  "无", "咎", "不", "疑", "复", "生", "鸢", "戾", "申", "曦", "益", "川", "休", "雨", "毅", "玄", 
                                  "宏", "述", "汤", "浩", "震", "岳", "晓", "岚", "天", "邃", "之", "嘉", "鲲", "鹏", "颜", "承", 
                                  "若", "子", "谅", "霖", "朔", "风", "凯", "逍", "遥", "欢", "芷", "庆", "哲", "有", "涯", "公", 
                                  "焕", "海", "程", "城", "龙", "雪", "君" 
                              };
        private string GetName()
        {
            Random rd=new Random();
            return FirstNames[rd.Next(FirstNames.Length)] + SecondNames[rd.Next(SecondNames.Length)];
        }
        #endregion
    }
}
