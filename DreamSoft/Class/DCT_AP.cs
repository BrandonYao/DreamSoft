using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

namespace DreamSoft
{
    class DCT_AP
    {
        private static TcpClient tcp;
        private static Socket skt;
        private static IPAddress fIPAddrSick;
        private static int PORT = 2111;
        private static readonly object myLock = new object();
        public static bool IsConnected { get { return tcp == null ? false : tcp.Connected; } }

        #region error
        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;
        private static readonly object myErrorLock = new object();
        private static void SendError(string error)
        {
            lock (myErrorLock)
            {
                ThrowMsg?.Invoke(error);
            }
        }
        #endregion

       private static HslCommunication.LogNet.LogNetDateTime fLog =
            new HslCommunication.LogNet.LogNetDateTime( Environment.CurrentDirectory + @"/Log/DCT", HslCommunication.LogNet.GenerateMode.ByEveryDay);
        public static void DCT_AP_Initial(string strIP, int port)
        {
            fIPAddrSick = IPAddress.Parse(strIP);
            PORT = port;
        }
        private static bool IpIsOK(IPAddress ipa)
        {
            bool result = false;
            Ping pingSender = new Ping();
            if (pingSender != null)
            {
                try
                {
                    PingReply reply = pingSender.Send(ipa, 200);//第一个参数为ip地址，第二个参数为ping的时间 
                    if (reply.Status == IPStatus.Success)
                    {
                        result = true;
                    }
                }
                catch (Exception)
                {
                }
            }
            return result;
        }
        private static bool Initial()
        {
            if (IpIsOK(fIPAddrSick))
            {
                if (tcp != null)
                    tcp.Close();
                if (skt != null)
                {
                    skt.Dispose();
                    skt.Close();
                }

                tcp = new TcpClient();
                try
                {
                    tcp.Connect(fIPAddrSick, PORT);
                    if (tcp.Connected)
                    {
                        skt = tcp.Client;
                        skt.SendTimeout = skt.ReceiveTimeout = 500;

                        blnToReceive = false;
                        Thread.Sleep(300);
                        blnToReceive = true;
                        new Thread(ReceiveData) { IsBackground = true }.Start();
                        return true;
                    }
                    else
                    {
                        SendError("TCP链接失败");//连接失败
                    }
                }
                catch (Exception)
                {
                    tcp = null; skt = null;
                    //初始化异常
                    SendError("TCP初始化异常");
                }
            }
            else
            {
                //未检测到
                SendError("IP地址不通");
            }
            return false;
        }
        private static void ModBusCRC16(ref byte[] cmd, int len)
        {
            ushort i, j, tmp, CRC16;

            CRC16 = 0xFFFF;             //CRC寄存器初始值
            for (i = 0; i < len; i++)
            {
                CRC16 ^= cmd[i];
                for (j = 0; j < 8; j++)
                {
                    tmp = (ushort)(CRC16 & 0x0001);
                    CRC16 >>= 1;
                    if (tmp == 1)
                    {
                        CRC16 ^= 0xA001;    //异或多项式
                    }
                }
            }
            cmd[i++] = (byte)(CRC16 & 0x00FF);
            cmd[i++] = (byte)((CRC16 & 0xFF00) >> 8);
        }
        private static bool ExecuteCmd(byte[] bts, string flag)
        {
            bool res = false;
            lock (myLock)
            {
                try
                {
                    if (skt == null)
                    {
                        if (!Initial())
                            return false;
                    }

                    ModBusCRC16(ref bts, 7);
                    byte[] sendDatas = bts;
                    int i = skt.Send(sendDatas);
                    if (i > 0)
                    {
                        res = true;
                    }
                }
                catch (Exception ex)
                {
                    //通讯异常
                    SendError("控制板通讯异常");
                    if (ex.GetType().Equals(typeof(SocketException)))
                    {
                        Initial();
                    }
                }
            }
            return res;
        }

        public class PosNum
        {
            public DateTime NumDate { get; set; }
            public int Num { get; set; }
        }

        private static bool blnToReceive = false;
        private static List<byte> RecvDatas = new List<byte>();
        private static Dictionary<int, PosNum> Dic_Pos_Num = new Dictionary<int, PosNum>();
        private static void ReceiveData()
        {
            while (blnToReceive)
            {
                if (skt == null)
                    Initial();
                else if(tcp.Connected)
                {
                    try
                    {
                        //接收数据
                        int num = skt.Available;
                        if (num > 0)
                        {
                            byte[] bts_recv = new byte[num];
                            skt.Receive(bts_recv);
                            RecvDatas.AddRange(bts_recv);
                            bts_recv = null;
                            //处理接收的数据
                            if (RecvDatas.Count > 8)
                            {
                                string recvStr = GetStrFromBytes(bts_recv.ToArray());
                                fLog.WriteDebug("接收数据\t", recvStr);
                                SendError("接收数据：" + recvStr);
                                int endIndex = 0;
                                for (int i = 0; i < RecvDatas.Count; i++)
                                {
                                    if (RecvDatas[i] == 0xA8 && i + 8 < RecvDatas.Count)//报文头，且后面有数据
                                    {
                                        byte[] bts = new byte[9], crc = new byte[2];
                                        RecvDatas.CopyTo(i, bts, 0, 9);
                                        string oneStr = GetStrFromBytes(bts.ToArray());
                                        fLog.WriteDebug("完整报文\t", oneStr);
                                        SendError("完整报文：" + oneStr);
                                        RecvDatas.CopyTo(i + 7, crc, 0, 2);
                                        fLog.WriteDebug("校验位\t", GetStrFromBytes(crc.ToArray()));
                                        ModBusCRC16(ref bts, 7);
                                        if (bts[7] == crc[0] && bts[8] == crc[1])//校验成功，数据完整
                                        {
                                            if (RecvDatas[i + 3] == 0x01)
                                            {
                                                int code = RecvDatas[i + 2] * 256 + RecvDatas[i + 1];
                                                if (Dic_Pos_Num.Keys.Contains(code))
                                                {
                                                    if ((DateTime.Now - Dic_Pos_Num[code].NumDate).TotalMilliseconds < 2000)
                                                        Dic_Pos_Num[code].Num += 1;
                                                }
                                                else Dic_Pos_Num.Add(code, new PosNum() { Num = 1, NumDate = DateTime.Now });
                                                fLog.WriteDebug("计数成功\t" + code);
                                                SendError("计数成功\t" + code);
                                            }
                                            endIndex = i + 8;
                                        }
                                        else
                                        {
                                            fLog.WriteDebug("校验失败");
                                            SendError("校验失败");
                                        }
                                    }
                                }
                                RecvDatas.RemoveRange(0, endIndex + 1);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //通讯异常
                        SendError("控制板通讯异常");
                        if (ex.GetType().Equals(typeof(SocketException)))
                            Initial();
                    }
                }
                Thread.Sleep(100);
            }
        }

        private static int ColumnCount = 25;
        public static bool DCTMoveDownSingle(int master, int dct, int time)
        {
            int code = (master - 1) * ColumnCount + dct;
            return DCTMoveDownSingle(code, time);
        }
        public static bool DCTMoveDownSingle(int code, int time)
        {
            bool result = false;
            byte[] bts = new byte[9];
            bts[0] = 0xA7;
            bts[1] = (byte)(code % 256);
            bts[2] = (byte)(code / 256);
            bts[3] = 0x01;
            bts[4] = (byte)(time / 10);
            bts[5] = 0x01;
            bts[6] = 0xEF;
            fLog.WriteDebug("指令\t", GetStrFromBytes(bts));
            result = ExecuteCmd(bts, new StackTrace().GetFrame(0).GetMethod().ToString());
            if (Dic_Pos_Num.Keys.Contains(code))
            {
                Dic_Pos_Num[code].NumDate = DateTime.Now; 
            }
            else Dic_Pos_Num.Add(code, new PosNum() { Num = 0, NumDate = DateTime.Now });
            return result;
        }
        public static bool ReadRecordSingle(int master, int dct, out int record)
        {
            int code = (master - 1) * ColumnCount + dct;
            return ReadRecordSingle(code, out record);
        }
        public static bool ReadRecordSingle(int code, out int record)
        {
            record = Dic_Pos_Num.Keys.Contains(code) ? Dic_Pos_Num[code].Num : 0;
            return true;
        }
        public static bool ClearRecordSingle(int master, int dct)
        {
            int code = (master - 1) * ColumnCount + dct;
            if (Dic_Pos_Num.Keys.Contains(code)) Dic_Pos_Num.Remove(code);//直接清除得了
            return true;
        }

        private static string GetStrFromBytes(byte[] bts)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte bt in bts)
            {
                sb.Append(Convert.ToString(bt, 16).PadLeft(2, '0').ToUpper() + " ");
            }
            return sb.ToString();
        }
    }
}
