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
using System.Windows.Threading;
using System.Data;

namespace DreamSoft
{
    /// <summary>
    /// UCPresc.xaml 的交互逻辑
    /// </summary>
    public partial class UCVirtualPresc : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();

        public UCVirtualPresc()
        {
            InitializeComponent();
        }

        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            btPresc_Start.IsEnabled = false; btPresc_Stop.IsEnabled = true;
            CreatePresc();
            timer_Presc.Start();
        }
        private void btStop_Click(object sender, RoutedEventArgs e)
        {
            btPresc_Start.IsEnabled = true; btPresc_Stop.IsEnabled = false;
            timer_Presc.Stop();
        }

        DispatcherTimer timer_Presc = new DispatcherTimer();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer_Presc.Interval = TimeSpan.FromSeconds(3);
            timer_Presc.Tick += new EventHandler(timer_Presc_Tick);

            btPresc_Start.IsEnabled = true; btPresc_Stop.IsEnabled = false;
        }

        void timer_Presc_Tick(object sender, EventArgs e)
        {
            CreatePresc();
        }
        private void CreatePresc()
        {
            string prescNo = Guid.NewGuid().ToString();
            string payTime = DateTime.Now.ToString();

            //分配窗口号
            string win = "0"; string name = GetName();
            string w = "select windowno from sys_window where openflag=1";
            DataTable dtWin; csSql.ExecuteSelect(w, Config.Soft.ConnString, out dtWin);
            if (dtWin != null && dtWin.Rows.Count > 0)
            {
                Random rd_Win = new Random();
                win = dtWin.Rows[rd_Win.Next(0, dtWin.Rows.Count)][0].ToString().Trim();

                //查询设备号
                string m = string.Format("select distinct maccode from sys_window_mac where windowno='{0}'", win);
                DataTable dtMac; csSql.ExecuteSelect(m, Config.Soft.ConnString, out dtMac);
                if (dtMac != null && dtMac.Rows.Count > 0)
                {
                    string sql = "";
                    //每个设备分配药
                    foreach (DataRow row in dtMac.Rows)
                    {
                        string maccode = row["MacCode"].ToString().Trim();
                        string d = "select distinct drugonlycode from drug_pos where maccode='{0}' and isuse='Y'";
                        d = string.Format(d, maccode);
                        DataTable dtDrug;
                        csSql.ExecuteSelect(d, Config.Soft.ConnString, out dtDrug);
                        if (dtDrug != null && dtDrug.Rows.Count > 0)
                        {
                            //默认种数
                            int num = 2;
                            int c = dtDrug.Rows.Count;
                            if (c < num)
                                num = c;

                            List<string> ds = new List<string>();
                            //随机出药
                            for (int i = 1; i <= num; i++)
                            {
                                Random rd_Drug = new Random();
                                int n = rd_Drug.Next(0, c);
                                string drugonlycode = dtDrug.Rows[n]["drugonlycode"].ToString().Trim();
                                if (!ds.Contains(drugonlycode))
                                    ds.Add(drugonlycode);
                                else
                                {
                                    i--;
                                    continue;
                                }
                                Random rd_Count = new Random();
                                int count = rd_Count.Next(1, 3);
                                string s = "insert into pat_druginfo(prescno,drugonlycode,drugnum,drugunit,doflag) select '{0}','{1}','{2}',drugpackunit,'N' from drug_info where drugonlycode='{1}';";
                                sql += string.Format(s, prescNo, drugonlycode, count);
                            }
                        }
                        else
                            csMsg.ShowWarning("设备未分配任何药品", false);
                    }
                    if (!string.IsNullOrEmpty(sql))
                    {
                        string s = "insert into pat_prescinfo (prescno,windowno,patname,paytime,doflag,createtime) values ('{0}',{1},'{2}','{3}','{4}',getdate());";
                        sql += string.Format(s, prescNo, win, name, payTime, "W");

                        if (csSql.ExecuteSql(sql, Config.Soft.ConnString))
                            csMsg.ShowInfo("生成处方", false);
                    }
                }
                else
                    csMsg.ShowWarning("窗口未绑定设备", false);
            }
            else
                csMsg.ShowWarning("窗口数量为0", false);
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
            Random rd = new Random();
            return FirstNames[rd.Next(FirstNames.Length)] + SecondNames[rd.Next(SecondNames.Length)];
        }
        #endregion
    }
}
