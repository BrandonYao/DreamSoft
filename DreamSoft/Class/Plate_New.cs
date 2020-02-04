using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static DreamSoft.PLC_Tcp_AP;

namespace DreamSoft
{
    class Plate_New
    {
        private static TcpClient tcp;
        private static Socket skt;
        private static IPAddress fIPAddrSick;
        private static int PORT = 2111;
        private static readonly object myLock = new object();
        public bool IsConnected { get { return tcp == null ? false : tcp.Connected; } }
        public int NowPos = 0, //当前位置
            Record = 0;//加药计数

        #region error
        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;
        private static readonly object myErrorLock = new object();
        private static void SendError(string error)
        {
            lock (myErrorLock)
            {
                ThrowMsg?.Invoke(string.Format("加药仓-", error));
            }
        }
        #endregion

        private static HslCommunication.LogNet.LogNetDateTime fLog;

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
        public static bool Initial()
        {
            fIPAddrSick = IPAddress.Parse(Config.Mac_A.IP_IND);
            PORT = Config.Mac_A.Port_IND;
            if (fLog == null)
                fLog = new HslCommunication.LogNet.LogNetDateTime(Environment.CurrentDirectory + @"/Log/Plate",
                    HslCommunication.LogNet.GenerateMode.ByEveryDay);
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


        private static bool blnToReceive = false;
        private static List<byte> RecvDatas = new List<byte>();
        private static void ReceiveData()
        {
            while (blnToReceive)
            {
                if (skt == null)
                    Initial();
                else if (tcp.Connected)
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
                            string recvStr = GetStrFromBytes(bts_recv.ToArray());
                            fLog.WriteDebug("接收数据\t", recvStr);
                            bts_recv = null;
                            //处理接收的数据
                            if (RecvDatas.Count > 8)
                            {
                                int endIndex = 0;
                                for (int i = 0; i < RecvDatas.Count; i++)
                                {
                                    if ((RecvDatas[i] == 0xB1 || RecvDatas[i] == 0xB2 || RecvDatas[i] == 0xB3 || RecvDatas[i] == 0xB4 || RecvDatas[i] == 0xB5)
                                        && i + 8 < RecvDatas.Count)//报文头，且后面有数据
                                    {
                                        byte[] bts = new byte[9], crc = new byte[2];
                                        RecvDatas.CopyTo(i, bts, 0, 9);
                                        string oneStr = GetStrFromBytes(bts.ToArray());
                                        fLog.WriteDebug("完整报文\t", oneStr);
                                        RecvDatas.CopyTo(i + 7, crc, 0, 2);
                                        fLog.WriteDebug("校验位\t", GetStrFromBytes(crc.ToArray()));
                                        ModBusCRC16(ref bts, 7);
                                        if (bts[7] == crc[0] && bts[8] == crc[1])//校验成功，数据完整
                                        {
                                            if (RecvDatas[i] == 0xB1 && RecvDatas[i + 1] == 0x01)
                                                bln_Plate_Origin_Left = true;
                                            else if (RecvDatas[i] == 0xB2 && RecvDatas[i + 1] == 0x01)
                                                bln_Plate_Origin_Right = true;
                                            else if (RecvDatas[i] == 0xB3)
                                            {
                                                bln_Plate_Up_Left = true;
                                                if (RecvDatas[i + 1] == 0x01)
                                                    int_PlateRecord_Left++;
                                            }
                                            else if (RecvDatas[i] == 0xB4)
                                            {
                                                bln_Plate_Up_Right = true;
                                                if (RecvDatas[i + 1] == 0x01)
                                                    int_PlateRecord_Right++;
                                            }
                                            if (RecvDatas[i + 2] == 0x01)
                                                bln_Plate_Error = true;
                                            else bln_Plate_Error = false;

                                            i += 8;//跳过当前报文
                                            endIndex = i;
                                        }
                                    }
                                }
                                RecvDatas.RemoveRange(0, endIndex + 1);
                            }
                        }
                    }
                    catch (SocketException ex)
                    {
                        //通讯异常
                        SendError("控制板通讯异常");
                        fLog.WriteException("接收数据", ex);
                        Initial();
                    }
                    catch (Exception ex)
                    {
                        fLog.WriteException("接收数据", ex);
                    }
                }
                Thread.Sleep(100);
            }
        }


        public static bool PlateOriginReset(PlateType type)
        {
            bool result = false;
            byte[] bts = new byte[9];
            if (type == PlateType.Left)
            {
                bln_Plate_Origin_Left = false;
                bts[0] = 0xB1;
            }
            else
            {
                bln_Plate_Origin_Right = false;
                bts[0] = 0xB2;
            }
            bts[1] = 0x00;
            bts[2] = 0x00;
            bts[3] = 0x00;
            bts[4] = 0x00;
            bts[5] = 0x01;
            bts[6] = 0xEF;
            fLog.WriteDebug("指令\t", GetStrFromBytes(bts));
            result = ExecuteCmd(bts, new StackTrace().GetFrame(0).GetMethod().ToString());
            return result;
        }
        private static bool bln_Plate_Origin_Left = false, bln_Plate_Origin_Right = false;
        public static bool PlateOriginResetIsOK(PlateType type)
        {
            if (type == PlateType.Left)
                return bln_Plate_Origin_Left;
            else return bln_Plate_Origin_Right;
        }

        public static bool PlateUpPulse(PlateType type, int pulse)
        {
            bool result = false;
            byte[] bts = new byte[9];
            if (type == PlateType.Left)
            {
                bln_Plate_Up_Left = false;
                bts[0] = 0xB3;
            }
            else
            {
                bln_Plate_Up_Right = false;
                bts[0] = 0xB4;
            }
            bts[1] = (byte)(pulse / 256);
            bts[2] = (byte)(pulse % 256);
            bts[3] = 0x00;
            bts[4] = 0x00;
            bts[5] = 0x01;
            bts[6] = 0xEF;
            fLog.WriteDebug("指令\t", GetStrFromBytes(bts));
            result = ExecuteCmd(bts, new StackTrace().GetFrame(0).GetMethod().ToString());
            return result;
        }
        private static bool bln_Plate_Up_Left = false, bln_Plate_Up_Right = false;
        public static bool PlateUpPulseIsOK(PlateType type)
        {
            if (type == PlateType.Left)
                return bln_Plate_Up_Left;
            else return bln_Plate_Up_Right;
        }

        private static int int_PlateRecord_Left = 0, int_PlateRecord_Right = 0;
        public static int ReadPlateRecord(PlateType type)
        {
            if (type == PlateType.Left)
                return int_PlateRecord_Left;
            else return int_PlateRecord_Right;
        }
        public static void ResetPlateRecord(PlateType type)
        {
            if (type == PlateType.Left)
                int_PlateRecord_Left = 0;
            else int_PlateRecord_Right = 0;
        }
        private static bool bln_Plate_Error = false;
        public static bool PlateStateIsOK()
        {
            if (bln_Plate_Error) return false;
            else return true;
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
