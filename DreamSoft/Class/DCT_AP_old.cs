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
        private CSHelper.Msg csMsg = new CSHelper.Msg();
        private CSHelper.LOG csLog = new CSHelper.LOG();
        private static TcpClient tcp;
        private static Socket skt;
        private static IPAddress fIPAddrSick;
        private static int PORT = 2111;
        private static readonly object myLock = new object();

        #region error
        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;
        private const string LOGTYPE = "DCT";
        private static readonly object myErrorLock = new object();
        private static string fPreErrorCode = "";
        private static void SendError(string error)
        {
            if (error == fPreErrorCode) return;
            fPreErrorCode = error;
            string errorCode = string.IsNullOrEmpty(error) ? "" : LOGTYPE + error;
            lock (myErrorLock)
            {
                ThrowMsg?.Invoke(errorCode);
            }
        }
        public void ClearError()
        { fPreErrorCode = ""; }
        #endregion
        
        public DCT_AP(string strIP, int port)
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
                    SendError("");
                }
            }
            return result;
        }
        public static void Initial()
        {
            fIPAddrSick = IPAddress.Parse("192.168.1.101");
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
                        SendError("");

                        blnToReceive = true;
                        new Thread(ReceiveData) { IsBackground = true }.Start();
                    }
                    else
                    {
                        SendError("005");//连接失败
                    }
                }
                catch (Exception)
                {
                    tcp = null; skt = null;
                    //初始化异常
                    SendError("002");
                }
            }
            else
            {
                //未检测到
                SendError("001");
            }
        }
        private static byte[] GetCRC_Check(byte[] datas)
        {
            byte[] res = new byte[2];
            //添加校验
            ushort[] calcuDatas = new ushort[datas.Length / 2 + 1];
            for (int i = 0; i < calcuDatas.Length; i++)
            {
                if (i == 0)
                    calcuDatas[0] = Convert.ToUInt16(datas[0] * 256 + datas[1]);
                else if (i == calcuDatas.Length - 1)
                    calcuDatas[i] = 0x0000;
                else calcuDatas[i] = Convert.ToUInt16(datas[i * 2 + 1] * 256 + datas[i * 2]);
            }
            ushort crc = GetCRC(calcuDatas);
            res[0] = Convert.ToByte(crc % 256);
            res[1] = Convert.ToByte(crc / 256);
            return res;
        }
        private static ushort GetCRC(ushort[] pDataArray)
        {
            ushort shifter, c, carry;
            ushort crc = 0;
            for (int n = 0; n < pDataArray.Length; n++)
            {
                shifter = 0x8000;
                c = pDataArray[n];
                do
                {
                    carry = (ushort)(crc & 0x8000);
                    crc <<= 1;
                    if ((ushort)(c & shifter) > 0) crc++;
                    if (carry > 0) crc ^= 0x1021;
                    shifter >>= 1;
                }
                while (shifter > 0);
            }
            return crc;
        }
        private static bool ExecuteCmd(byte[] bts, string flag)
        {
            bool res = false;
            lock (myLock)
            {
                try
                {
                    if (skt == null)
                        Initial();

                    byte[] crc = GetCRC_Check(bts);
                    byte[] sendDatas = bts.Concat(crc).ToArray();
                    //清空历史数据
                    int num = skt.Available;
                    if (num > 0)
                    {
                        byte[] bts_recv_dis = new byte[num];
                        skt.Receive(bts_recv_dis);
                    }
                    int i = skt.Send(sendDatas);
                    if (i > 0) res = true;
                }
                catch (Exception ex)
                {
                    //导航仪通讯异常
                    SendError("003");
                    if (ex.GetType().Equals(typeof(SocketException)))
                        Initial();
                }
            }
            return res;
        }

        private static bool blnToReceive = false;
        private static List<byte> RecvDatas = new List<byte>();
        private static Dictionary<string, int> Dic_Pos_Num = new Dictionary<string, int>();
        private static void ReceiveData()
        {
            while (blnToReceive)
            {
                if (skt == null)
                    Initial();
                else
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
                                for (int i = 0; i < RecvDatas.Count; i++)
                                {
                                    if (RecvDatas[i] == 0xA1 && i + 3 < RecvDatas.Count)//报文头，且后面有数据
                                    {
                                        if (RecvDatas[i + 3] == 0xFF)
                                        {
                                            string pos = RecvDatas[i + 2].ToString().PadLeft(2, '0') + RecvDatas[i + 1].ToString().PadLeft(2, '0');
                                            if (Dic_Pos_Num.Keys.Contains(pos)) Dic_Pos_Num[pos] += 1;
                                            else Dic_Pos_Num.Add(pos, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //通讯异常
                        SendError("0003");
                        if (ex.GetType().Equals(typeof(SocketException)))
                            Initial();
                    }
                }
                Thread.Sleep(10);
            }
        }

        public static bool DCTMoveDownSingle(int master, int dct, int time)
        {
            bool result = false;
            byte[] bts = new byte[9];
            bts[0] = 0xA1;
            bts[1] = (byte)dct;
            bts[2] = (byte)master;
            bts[3] = (byte)(time / 10);
            bts[4] = 0x01;
            bts[5] = 0x00;
            bts[6] = 0xEF;
            result = ExecuteCmd(bts, new StackTrace().GetFrame(0).GetMethod().ToString());
            return result;
        }
        public static bool ReadRecordSingle(int master, int dct, out int record)
        {
            string pos = master.ToString().PadLeft(2, '0') + dct.ToString().PadLeft(2, '0');
            record = Dic_Pos_Num.Keys.Contains(pos) ? Dic_Pos_Num[pos] : 0;
            return true;
        }
        public static bool ClearRecordSingle(int master, int dct)
        {
            string pos = master.ToString().PadLeft(2, '0') + dct.ToString().PadLeft(2, '0');
            if (Dic_Pos_Num.Keys.Contains(pos)) Dic_Pos_Num[pos] = 0;
            return true;
        }
    }
}
