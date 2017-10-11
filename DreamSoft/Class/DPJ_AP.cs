using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;

namespace DreamSoft
{
    class DPJ_AP
    {
        static CSHelper.Msg csMsg = new CSHelper.Msg();
        static CSHelper.LOG csLog = new CSHelper.LOG();

        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;

        public static SerialPort spDPJ;
        //初始化
        public static void Initial()
        {
            if (spDPJ != null)
            {
                spDPJ.Close();
                spDPJ.Dispose();
            }
            try
            {
                spDPJ = new SerialPort(Config.Mac_A.Port_DPJ, 9600, Parity.None, 8, StopBits.One);
                spDPJ.DtrEnable = true;
                spDPJ.RtsEnable = true;
                spDPJ.ReadTimeout = 500;
                spDPJ.WriteTimeout = 500;
                if (!spDPJ.IsOpen)
                    spDPJ.Open();
            }
            catch (Exception ex)
            {
                csMsg.ShowWarning(ex.Message, true);
            }
        }

        const string BeginString = "AA";
        const string EndString = "CC";
        static bool DPJIsBusy = false;

        //计算FCS码
        static string GetFCS(String value)
        {
            int f = 0;
            int j = value.Length;
            for (int i = 0; i < j; i += 2)
            {
                int x = PLC_Tcp_AP.Get16Int(value.Substring(i, 2));
                //每次进行异或运算
                f = f ^ x;
            }
            //转换为16进制
            return f.ToString("X");
        }
        //检查指令是否执行成功
        static bool CheckResponse(string response ,out int err, out string errStr)
        {
            bool result = true;
            err = 0; errStr = "";
            if (response.Length < 14)
            {
                err = 1;
                result = false;
                errStr = "数据太短";
            }
            else if (response.Substring(response.Length - 6, 2) == "FF")
            {
                err = 2;
                result = false;
                errStr = "执行错误";
            }
            else
            {
                string send = response.Substring(2, response.Length - 6);
                string fcs = response.Substring(response.Length - 4, 2);
                if (GetFCS(send) != fcs)
                {
                    err = 3;
                    result = false;
                    errStr = "校验错误";
                }
            }
            return result;
        }

        static bool SendPLC_DPJ_485(string send, string mark, out string response,out int err, out string errStr)
        {
            bool result = false;
            err = 0; errStr = "";
            response = "";
            //添加起始符
            send = PLC_Tcp_AP.Get16String(send.Length / 2 + 1, 2) + send;
            send += GetFCS(send);
            send = BeginString + send + EndString;

            try
            {
                while (DPJIsBusy)
                    Thread.Sleep(50);
                DPJIsBusy = true;
                spDPJ.DiscardInBuffer();
                spDPJ.DiscardOutBuffer();

                //写入串口
                byte[] buffer_send = PLC_Tcp_AP.GetBytes(send);
                spDPJ.Write(buffer_send, 0, buffer_send.Length);


                DateTime begin = DateTime.Now;
                int m = 0, n = 0;
                do
                {
                    Thread.Sleep(50);
                    m = spDPJ.BytesToRead;
                    Thread.Sleep(50);
                    n = spDPJ.BytesToRead;
                }
                while ((m == 0 || m < n) && DateTime.Now <= begin.AddMilliseconds(1000));
                if (DateTime.Now > begin.AddMilliseconds(1000))
                {
                    errStr = "通讯超时"; 
                    //记录错误信息
                    string msg = mark + ":" + errStr;
                    csLog.WriteLog(msg);
                }
                if (m > 0)
                {
                    byte[] buffer_response = new byte[m];
                    spDPJ.Read(buffer_response, 0, m);
                    response = PLC_Tcp_AP.GetString(buffer_response);
                }

                DPJIsBusy = false;

                if (CheckResponse(response, out err, out errStr))
                    result = true;
                else
                {
                    //记录错误信息
                    string msg = response + "---" + mark + ":" + errStr;
                    csLog.WriteLog(msg);
                }
            }
            catch (Exception ex)
            {
                DPJIsBusy = false;
                //记录错误信息
                string msg = response + "---" + mark + ":" + ex.Message;
                csLog.WriteLog(msg);
            }
            return result;
        }

        const string Order_Adr_Write_Master = "A0";
        const string Order_Adr_Read_Master = "A3";
        const string Order_DCT_Master = "A4";
        const string Order_Record_Read_Master = "AD";
        const string Order_Record_Write_Master = "AC";

        public static bool DCTMoveDownSingle(int master, int dct, int time)
        {
            bool result = false;
            try
            {
                string send = PLC_Tcp_AP.Get16String(master, 2) + Order_DCT_Master + PLC_Tcp_AP.Get16String(dct, 2) + PLC_Tcp_AP.Get16String(time / 10, 2) + PLC_Tcp_AP.Get16String(Config.Mac_A.DelayTime_Record / 10, 2) + PLC_Tcp_AP.Get16String(Config.Mac_A.StopTime_Record / 10, 2);
                string response;
                int err; string errStr;
                int num = 0;
                do
                {
                    num++;
                    bool b = SendPLC_DPJ_485(send, new StackTrace().GetFrame(0).GetMethod().ToString(), out response, out err, out errStr);
                    if (b)
                    {
                        result = true;
                        break;
                    }
                    else if (err != 3)
                    {
                        result = false;
                        //csLog.WriteLog(errStr);
                        break;
                    }
                } while (err == 3 && num < 3);
            }
            catch (Exception ex)
            {
                csMsg.ShowWarning(ex.Message, true);
            }
            return result;
        }
        public static bool ReadRecordSingle(int master, int dct, out int record)
        {
            bool result = false;
            record = -1;
            try
            {
                string send = PLC_Tcp_AP.Get16String(master, 2) + Order_Record_Read_Master + PLC_Tcp_AP.Get16String(dct, 2);
                string response;
                int err; string errStr;
                int num = 0;
                do
                {
                    num++;
                    bool b = SendPLC_DPJ_485(send, new StackTrace().GetFrame(0).GetMethod().ToString(), out response, out err, out errStr);
                    if (b)
                    {
                        result = false;
                        record = PLC_Tcp_AP.Get16Int(response.Substring(10, 2));
                        break;
                    }
                    else if (err != 3)
                    {
                        result = false;
                        //csLog.WriteLog(errStr);
                        break;
                    }
                } while (err == 3 && num < 3);
            }
            catch (Exception ex)
            {
               csLog.WriteLog(ex.Message);
            }
            return result;
        }
        public static bool ClearRecordSingle(int master, int dct)
        {
            bool result = false;
            try
            {
                string send = PLC_Tcp_AP.Get16String(master, 2) + Order_Record_Write_Master + PLC_Tcp_AP.Get16String(dct, 2) + "00";
                string response;
                int err; string errStr;
                int num = 0;
                do
                {
                    num++;
                    bool b = SendPLC_DPJ_485(send, new StackTrace().GetFrame(0).GetMethod().ToString(), out response, out err, out errStr);
                    if (b)
                    {
                        result = true;
                        break;
                    }
                    else if (err != 3)
                    {
                        result = false;
                        //csLog.WriteLog(errStr);
                        break;
                    }
                } while (err == 3 && num < 3);
            }
            catch (Exception ex)
            {
                csLog.WriteLog(ex.Message);
            }
            return result;
        }
    }
}
