using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace DreamSoft
{
    class Laser
    {
        static CSHelper.LOG csLOG = new CSHelper.LOG();
        static CSHelper.Msg csMsg = new CSHelper.Msg();

        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;

        //左侧激光串口
        static SerialPort spLaserLeft;
        //右侧激光串口
        static SerialPort spLaserRight;

        //初始化端口
        public static void InitialLaserPort_Left()
        {
            //左
            //*
            if (spLaserLeft != null)
            {
                spLaserLeft.Close();
                spLaserLeft.Dispose();
            }
            try
            {
                spLaserLeft = new SerialPort(Config.Mac_A.Port_Laser_Left, 38400, Parity.None, 8, StopBits.One);
                spLaserLeft.DtrEnable = true;
                spLaserLeft.RtsEnable = true;
                if (!spLaserLeft.IsOpen)
                    spLaserLeft.Open();
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
            //*/
        }
        public static void InitialLaserPort_Right()
        {
            //右
            //*
            if (spLaserRight != null)
            {
                spLaserRight.Close();
                spLaserRight.Dispose();
            }
            try
            {
                spLaserRight = new SerialPort(Config.Mac_A.Port_Laser_Right, 38400, Parity.None, 8, StopBits.One);
                spLaserRight.DtrEnable = true;
                spLaserRight.RtsEnable = true;
                spLaserRight.ReadTimeout = 500;
                spLaserRight.WriteTimeout = 500;
                if (!spLaserRight.IsOpen)
                    spLaserRight.Open();
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
            //*/
        }
       static bool spIsBusy = false;
        //读取盘点激光数据
        public static int ReadLaserData(PLC_Tcp_AP.LaserType type)
        {
            int result = -1;
            while (spIsBusy)
                Thread.Sleep(100);
            spIsBusy = true;
            SerialPort spLaser = spLaserLeft;
            if (type == PLC_Tcp_AP.LaserType.Right)
                spLaser = spLaserRight;

            if (spLaser != null)
            {
                string send = "24" + "00" + "01" + "00" + "2000" + "0000" + "00000000" +
                   "0A" + "00" + "0000" + "0000" + "0000" + "00000000" + "00000000";
                send += GetFCS(send) + "2E3B";

                string response = "";
                try
                {
                    spLaser.DiscardInBuffer();
                    spLaser.DiscardOutBuffer();
                    byte[] buffer_send = PLC_Tcp_AP.GetBytes(send);
                    spLaser.Write(buffer_send, 0, buffer_send.Length);

                    DateTime begin = DateTime.Now;
                    int m = 0, n = 0;

                    Thread.Sleep(200);
                    do
                    {
                        Thread.Sleep(50);
                        m = spLaser.BytesToRead;
                        Thread.Sleep(50);
                        n = spLaser.BytesToRead;
                    }
                    while ((m == 0 || m < n) && DateTime.Now < begin.AddSeconds(2));
                    byte[] buffer_response = new byte[m];
                    spLaser.Read(buffer_response, 0, m);
                    spIsBusy = false;
                    response = PLC_Tcp_AP.GetString(buffer_response);
                    if (response.Length >= 79)
                    {
                        string s = response.Substring(72, 8);
                        string value = s.Substring(6, 2) + s.Substring(4, 2) + s.Substring(2, 2) + s.Substring(0, 2);
                        result = Convert.ToInt32(value, 16);
                    }
                }
                catch (Exception ex)
                {
                    spIsBusy = false;
                    csLOG.WriteLog(ex.Message);
                    if (ThrowMsg == null)
                    {
                        csMsg.ShowWarning(ex.Message, false);
                    }
                    else ThrowMsg(ex.Message);
                }
            }
            else if (ThrowMsg != null)
                ThrowMsg("激光串口不存在");
            return result;
        }

        //打开盘点激光
        public static void OnOffLaser(PLC_Tcp_AP.LaserType type, int value)
        {
            while (spIsBusy)
                Thread.Sleep(100);
            spIsBusy = true;
            SerialPort spLaser = spLaserLeft;
            if (type == PLC_Tcp_AP.LaserType.Right)
                spLaser = spLaserRight;

            string send = "24" + "00" + "01" + "00" + "2000" + "0000" + "00000000" +
               "0A" + "09" + "0000" + value.ToString().PadLeft(4, '0') + "0000" + "00000000" + "00000000";
            send += GetFCS(send) + "2E3B";

            string response = "";
            try
            {
                spLaser.DiscardInBuffer();
                spLaser.DiscardOutBuffer();
                byte[] buffer_send = PLC_Tcp_AP.GetBytes(send);
                spLaser.Write(buffer_send, 0, buffer_send.Length);

                DateTime begin = DateTime.Now;
                int m = 0, n = 0;
                do
                {
                    Thread.Sleep(50);
                    m = spLaser.BytesToRead;
                    Thread.Sleep(50);
                    n = spLaser.BytesToRead;
                }
                while ((m == 0 || m < n) && DateTime.Now < begin.AddSeconds(1));
                byte[] buffer_response = new byte[m];
                spLaser.Read(buffer_response, 0, m);
                spIsBusy = false;
                response = PLC_Tcp_AP.GetString(buffer_response);
                //if (response.Length >= 79)
                //{
                //    string s = response.Substring(72, 8);
                //}
            }
            catch (Exception ex)
            {
                spIsBusy = false;
                csLOG.WriteLog(ex.Message);
                if (ThrowMsg == null)
                {
                    csMsg.ShowWarning(ex.Message, false);
                }
                else ThrowMsg(ex.Message);
            }
        }

        //计算FCS码
        static string GetFCS(String value)
        {
            int x;
            int f = 0;
            int j = value.Length;
            for (int i = 0; i < j; i+=2)
            {
                x = PLC_Tcp_AP.Get16Int(value.Substring(i, 2));
                //每次进行异或运算
                f = f ^ x;
            }
            //转换为16进制
            string s = f.ToString("X").PadLeft(2, '0');
            return s.Substring(1, 1).PadLeft(2, '0') + s.Substring(0, 1).PadLeft(2, '0');
        }
    }
}
