using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DreamSoft
{
    /// <summary>
    /// WinTest_DCT.xaml 的交互逻辑
    /// </summary>
    public partial class WinTest_DCT : Window
    {
        private DispatcherTimer tmr_dct;
        private TestData fTestD;
        private Dictionary<string, int> dicRecord;
        public WinTest_DCT()
        {
            InitializeComponent();
            tmr_dct = new DispatcherTimer();
            tmr_dct.Tick += Tmr_dct_Tick;
            fTestD = new DreamSoft.WinTest_DCT.TestData()
            { BeginUnit = 1, BeginLayer = 1, BeginCol = 1, EndUnit = 1, EndLayer = 1, EndCol = 20, Delay = 1000 };
            grdTest.DataContext = fTestD;
            dicRecord = new Dictionary<string, int>();
        }

        class TestData
        {
            public int BeginUnit { get; set; }
            public int BeginLayer { get; set; }
            public int BeginCol { get; set; }
            public int NowUnit { get; set; }
            public int NowLayer { get; set; }
            public int NowCol { get; set; }
            public int EndUnit { get; set; }
            public int EndLayer { get; set; }
            public int EndCol { get; set; }
            public int Delay { get; set; }
        }

        private void Tmr_dct_Tick(object sender, EventArgs e)
        {
            tmr_dct.Stop();
            Test();
        }
        private void Test()
        {
            bool todo = false;
            if (fTestD.NowUnit < fTestD.EndUnit) todo = true;
            else if (fTestD.NowUnit > fTestD.EndUnit) todo = false;
            else if (fTestD.NowLayer < fTestD.EndLayer) todo = true;
            else if (fTestD.NowLayer > fTestD.EndLayer) todo = false;
            else if (fTestD.NowCol < fTestD.EndCol) todo = true;
            else if (fTestD.NowCol > fTestD.EndCol) todo = false;
            else todo = true;
            if (todo)
            {
                string posCode = fTestD.NowUnit.ToString() + fTestD.NowLayer.ToString().PadLeft(2, '0') + fTestD.NowCol.ToString().PadLeft(2, '0');
                PLC_Tcp_AP.DCTMoveDownSingle(posCode);
                Thread.Sleep(fTestD.Delay);//间隔时间
                int m = PLC_Tcp_AP.ReadRecordSingle(posCode);
                if (dicRecord.Keys.Contains(posCode))
                    dicRecord[posCode] = m;
                else dicRecord.Add(posCode, m);
                RefreshList();
                bool isover = false;
                if (fTestD.NowCol < Config.Mac_A.Count_Col) fTestD.NowCol += 1;
                else
                {
                    fTestD.NowCol = 1;
                    if (fTestD.NowLayer < Config.Mac_A.Count_Lay) fTestD.NowLayer += 1;
                    else
                    {
                        fTestD.NowLayer = 1;
                        if (fTestD.NowUnit < Config.Mac_A.Count_Unit) fTestD.NowUnit += 1;
                        else isover = true;
                    }
                }
                if (!isover)
                    tmr_dct.Start();
            }
        }
        private void RefreshList()
        {
            List<string> lstNum = new List<string>();
            if (dicRecord.Keys.Count > 0)
            {
                var lst = dicRecord.OrderBy(p => p.Key);
                foreach (var item in lst)
                {
                    lstNum.Add(string.Format("{0}: {1}", item.Key, item.Value));
                }
            }
            lsbNum.ItemsSource = lstNum;
            svNum.ScrollToEnd();
        }

        private CSHelper.Msg csMsg = new CSHelper.Msg();
        private void btn_start_click(object sender, RoutedEventArgs e)
        {
            if (csMsg.ShowQuestion("自动测试仅限调试时使用，此操作会有出药动作，可能会导致库存错乱。\r\n确定要开始自动测试吗？", false))
            {
                dicRecord.Clear();
                fTestD.NowUnit = fTestD.BeginUnit;
                fTestD.NowLayer = fTestD.BeginLayer;
                fTestD.NowCol = fTestD.BeginCol;
                tmr_dct.Interval = TimeSpan.FromMilliseconds(fTestD.Delay);
                tmr_dct.Start();
            }
        }

        private void btn_stop_click(object sender, RoutedEventArgs e)
        {
            tmr_dct.Stop();
        }
    }
}
