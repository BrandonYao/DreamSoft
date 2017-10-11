using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace DreamSoft
{
    class Scanner
    {
        static CSHelper.Msg csMsg = new CSHelper.Msg();
        static CSHelper.LOG csLOG = new CSHelper.LOG();

        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;

        public delegate void ShowScan(string msg);
        public static ShowScan ThrowScan;

        //扫描枪串口
        static SerialPort spScan;
        
        public static DateTime LastScanTime;

        //初始化端口
        public static void InitialScanPort()
        {
            if (spScan != null)
            {
                spScan.DataReceived -= new SerialDataReceivedEventHandler(spScan_DataReceived);
                spScan.Close();
                spScan.Dispose();
            }
            try
            {
                spScan = new SerialPort(Config.Mac_A.Port_Scanner, 9600, Parity.None, 8, StopBits.One);

                spScan.DtrEnable = true;
                spScan.RtsEnable = true;
                spScan.ReadTimeout = 500;
                spScan.WriteTimeout = 500;
                if (!spScan.IsOpen)
                    spScan.Open();
                spScan.DataReceived += new SerialDataReceivedEventHandler(spScan_DataReceived);
            }
            catch (Exception ex)
            {
                csLOG.WriteLog(ex.Message);
                if (ThrowMsg == null)
                {
                    csMsg.ShowWarning(ex.Message, false);
                }
                else ThrowMsg(ex.Message);
            }
        }
        //扫描事件
        static void spScan_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DateTime begin = DateTime.Now;
            int m = 0, n = 0;
            do
            {
                Thread.Sleep(50);
                m = spScan.BytesToRead;
                Thread.Sleep(50);
                n = spScan.BytesToRead;
            }
            while ((m == 0 || m < n) && DateTime.Now < begin.AddSeconds(1));
            byte[] buffer_response = new byte[m];
            spScan.Read(buffer_response, 0, m);
            char[] cs = Encoding.ASCII.GetChars(buffer_response);

            string response = "";
            for (int i = 0; i < cs.Length; i++)
            {
                response += cs[i].ToString();
            }
            if (response.Length >= 13)
            {
                //13位为商品码，否则为监管码（取前8位）
                string code = "";
                if (response.Length >= 20)
                    code = response.Substring(0, 8);
                else
                    code = response.Substring(0, 13);

                DateTime now = DateTime.Now;
                if (now > LastScanTime.AddSeconds(Config.Mac_A.ScanSpan))
                {
                    LastScanTime = now;
                    if (ThrowScan != null)
                        ThrowScan(code);
                }
            }
        }
    }
}
