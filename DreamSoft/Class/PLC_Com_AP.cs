using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Data;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace DreamSoft
{
    class PLC_Com_AP
    {
        static CSHelper.Msg csMsg = new CSHelper.Msg();
        static CSHelper.SQL csSql = new CSHelper.SQL();
        static CSHelper.LOG csLog = new CSHelper.LOG();

        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;

        public static SerialPort spPLC;
        const string PLCNo = "01";

        //功能码
        #region
        /// <summary>
        /// 起始字符
        /// </summary>
        public const string StartString = ":";

        /// <summary>
        /// 结束字符
        /// </summary>
        public const string EndString = "\r\n";

        //读位装置寄存器
        //发送：  ：（起始符符）+01（站号）+   01（功能码）+0800（起始地址）+0014（地址数量，20位）   +**（LRC校验）+CR LF（结束字符）
        //接收：  ：（起始符符）+01（站号）+   01（功能码）+03（字节数，20/8取整）+数据{81（1000 0001）+18（0001 1000）+06（0110）}   +**（LRC校验）+CR LF（结束字符）
        const string RegMR = "01";

        //读字装置寄存器
        //发送：  ：（起始符符）+01（站号）+   03（功能码）+1000（装置首地址）+0002（地址数量）   +**（LRC校验）+CR LF（结束字符）
        //接收：  ：（起始符符）+01（站号）+   03（功能码）+04（字节数，地址数量*2）+0100（地址内容）+0200（地址内容）   +**（LRC校验）+CR LF（结束字符）
        const string RegDR = "03";

        //写单个位装置寄存器
        //发送：  ：（起始符符）+01（站号）+   05（功能码）+0800（装置地址）+FF00（地址写入值，0000：0，FF00：1）   +**（LRC校验）+CR LF（结束字符）
        //接收：  ：（起始符符）+01（站号）+   05（功能码）+0800（装置地址）+FF00（地址写入值）   +**（LRC校验）+CR LF（结束字符）
        const string RegMSW = "05";

        //写单个字装置寄存器
        //发送：  ：（起始符符）+01（站号）+   06（功能码）+1000（装置地址）+0100（地址写入值）   +**（LRC校验）+CR LF（结束字符）
        //接收：  ：（起始符符）+01（站号）+   06（功能码）+1000（装置地址）+0100（地址写入值）   +**（LRC校验）+CR LF（结束字符）
        const string RegDSW = "06";

        //写多个位装置寄存器（256）
        //发送：  ：（起始符符）+01（站号）+   0F（功能码）+0800（装置首地址）+0014（地址数量，20位）+03（字节数，20/8取整）+81（1000 0001）+18（0001 1000）+06（0110）   +**（LRC校验）+CR LF（结束字符）
        //接收：  ：（起始符符）+01（站号）+   0F（功能码）+0800（装置首地址）+0014（地址数量）   +**（LRC校验）+CR LF（结束字符）
        const string RegMMW = "0F";

        //写多个字装置寄存器（64）
        //发送：  ：（起始符符）+01（站号）+10（功能码）+1000（装置首地址）+0002（地址数量）+04（字节数，地址数量*2）+0100（地址内容）+0200（地址内容）   +**（LRC校验）+CR LF（结束字符）
        //接收：  ：（起始符符）+01（站号）+10（功能码）+1000（装置首地址）+0002（地址数量）   +**（LRC校验）+CR LF（结束字符）
        const string RegDMW = "10";
        //异常回应：
        //：（起始符符）+01（站号）+82（80+功能码）+01（异常回应码）
        //异常回应码：
        //01: 非法命令码
        //02:非法的装置地址
        //03:非法装置值
        //07:校验和错误
        #endregion

        //初始化
        #region

        static bool PLCIsBusy = false;

        //初始化PLC串口
        public static void Initial()
        {
            if (spPLC != null)
            {
                spPLC.Close();
                spPLC.Dispose();
            }
            spPLC = new SerialPort(Config.Mac_A.Port_PLC, 9600, Parity.Even, 7, StopBits.One);

            if (!spPLC.IsOpen)
            {
                try
                {
                    spPLC.Open();
                }
                catch (Exception ex)
                {
                    csLog.WriteLog(ex.Message);
                    ThrowMsg(ex.Message);
                }
            }
            Thread.Sleep(100);
        }
        //转换LRC码
        public static string GetLRCCode(string value)
        {
            string result = "";
            int len = value.Length;
            int sum = 0;
            //每两位转为16进制求和
            for (int i = 0; i < len; i += 2)
            {
                string s = value.Substring(i, 2);
                sum += Convert.ToInt32(s, 16);
            }
            //对结果，取补数，再+1
            result = (256 - sum % 256).ToString("X").PadLeft(2, '0');
            return result;
        }
        //检查指令是否执行成功
        public static bool CheckResponse(string response, string mark)
        {
            bool result = false;
            string error = "";
            int i = response.IndexOf(":");
            if (i >= 0)
            {
                string c = response.Substring(i + 3, 1);
                if (c != "8" && c != "9")
                    result = true;
                else
                {
                    string s = response.Substring(i + 5, 2);
                    switch (s)
                    {
                        case "01":
                            error = "非法命令码";
                            break;
                        case "02":
                            error = "非法的装置地址";
                            break;
                        case "03":
                            error = "非法装置值";
                            break;
                        case "07":
                            error = "校验和错误";
                            break;
                        default:
                            error = "系统错误";
                            break;
                    }
                        csLog.WriteLog(mark + "：" + error);
                        ThrowMsg(mark + "：" + error);
                }
            }
            return result;
        }
        /// <summary>
        /// 向PLC发送指令，返回指令是否执行成功
        /// </summary>
        /// <param name="send">指令内容，不包括起始符</param>
        /// <param name="mark">备注</param>
        /// <param name="response">返回信息</param>
        /// <returns></returns>
        public static bool SendPLC(string send, string mark, out string response)
        {
            bool result = false;
            response = "";
            send = PLCNo + send;
            //添加校验和结束符
            send = StartString + send + GetLRCCode(send) + EndString;

            while (PLCIsBusy)
            {
                Thread.Sleep(100);
            };

            PLCIsBusy = true;
            try
            {
                if (spPLC == null)
                {
                    spPLC = new SerialPort(Config.Mac_A.Port_PLC, 9600, Parity.Even, 7, StopBits.One);
                }
                if (!spPLC.IsOpen)
                {
                    spPLC.Open();
                }

                spPLC.DiscardInBuffer();
                spPLC.DiscardOutBuffer();
                //写入串口
                spPLC.Write(send);

                DateTime begin = DateTime.Now;

                do
                {
                    Thread.Sleep(50);
                    response = spPLC.ReadExisting();
                    //response = spPLC.ReadLine();
                    //response = spPLC.ReadTo("\r\n");
                } while (response == "" && begin.AddSeconds(3) > DateTime.Now);

                //int m = 0, n = 0;
                //do
                //{
                //    Thread.Sleep(50);
                //    m = spPLC.BytesToRead;
                //    Thread.Sleep(50);
                //    n = spPLC.BytesToRead;
                //}
                //while ((m == 0 || m < n) && DateTime.Now < begin.AddMilliseconds(500));
                //if (m > 0)
                //{
                //    byte[] buffer_response = new byte[m];
                //    spPLC.Read(buffer_response, 0, m);
                //    response = PLC_Tcp.GetString(buffer_response);
                //}

                PLCIsBusy = false;

                //检验返回数据
                if (CheckResponse(response, mark))
                    result = true;
            }
            catch (Exception ex)
            {
                PLCIsBusy = false;
                //记录错误信息
                csLog.WriteLog(mark + "：PLC通讯出错---" + ex.Message);
                ThrowMsg(mark + "：PLC通讯出错---" + ex.Message);
            }
            return result;
        }

        #endregion

        //设定速度
        #region

        //const int Speed_Auto_Baffle_Belt = 520;//D-2
        const int Speed_Auto_Baffle_Lift = 522;//D-2

        //const int Speed_Manual_Baffle_Belt = 500;//D-2
        const int Speed_Manual_Baffle_Lift = 504;//D-2

        public static void SetSpeeds()
        {
            string speed = "";
            string send = "";
            string response;

            //速度
            //speed = PLC_Tcp_AP.Get16String(int.Parse(Config.Mac_A.Speed_Auto_Baffle_Belt), 8);
            //send = RegDMW + PLC_Tcp_AP.GetAdrD_1000(Speed_Auto_Baffle_Belt) + "0002" + "04" + speed.Substring(4, 4) + speed.Substring(0, 4);
            //if (!SendPLC(send, "传送带挡板自动速度", out response)) return;
            //speed = PLC_Tcp_AP.Get16String(int.Parse(Config.Mac_A.Speed_Manual_Baffle_Belt), 8);
            //send = RegDMW + PLC_Tcp_AP.GetAdrD_1000(Speed_Manual_Baffle_Belt) + "0002" + "04" + speed.Substring(4, 4) + speed.Substring(0, 4);
            //if (!SendPLC(send, "传送带挡板手动速度", out response)) return;

            speed = PLC_Tcp_AP.Get16String(int.Parse(Config.Mac_A.Speed_Auto_Baffle_Lift), 8);
            send = RegDMW + PLC_Tcp_AP.GetAdrD_1000(Speed_Auto_Baffle_Lift) + "0002" + "04" + speed.Substring(4, 4) + speed.Substring(0, 4);
            if (!SendPLC(send, "出药斗挡板自动速度", out response)) return;
            speed = PLC_Tcp_AP.Get16String(int.Parse(Config.Mac_A.Speed_Manual_Baffle_Lift), 8);
            send = RegDMW + PLC_Tcp_AP.GetAdrD_1000(Speed_Manual_Baffle_Lift) + "0002" + "04" + speed.Substring(4, 4) + speed.Substring(0, 4);
            if (!SendPLC(send, "出药斗挡板手动速度", out response)) return;
        }

        #endregion

        //提升机挡板
        #region
        //挡板动作类型（打开、关闭）
        public enum BaffleMoveType_Lift
        {
            Up,
            Down,
            Stop
        }

        const int Baffle_Auto = 101;//M-1
        const int Baffle_Manual = 102;//M-1
        //挡板手自动切换（与传送带挡板共用）
        public static void Baffle_Change(int auto)
        {
            int adr = Baffle_Auto;
            if (auto == 0)
                adr = Baffle_Manual;

            string send = RegMSW + PLC_Tcp_AP.GetAdrM(adr) + "FF00";
            string response;
            SendPLC(send, "挡板手自动切换", out response);
        }

        //挡板手动运行
        const int Baffle_Manual_Lift_Up = 6;//M-1
        const int Baffle_Manual_Lift_Down = 7;//M-1
        public static void BaffleMove_Lift(BaffleMoveType_Lift type)
        {
            int adr = 0;
            switch (type)
            {
                case BaffleMoveType_Lift.Up:
                    adr = Baffle_Manual_Lift_Up;
                    break;
                case BaffleMoveType_Lift.Down:
                    adr = Baffle_Manual_Lift_Down;
                    break;
            }
            string send = "";
            string response;
            if (type == BaffleMoveType_Lift.Stop) //停止，两个地址置0
                send = RegMMW + PLC_Tcp_AP.GetAdrM(Baffle_Manual_Lift_Up) + "0002" + "01" + "00";
            else
                send = RegMSW + PLC_Tcp_AP.GetAdrM(adr) + "FF00";
            SendPLC(send, "挡板手动运行使能开启", out response);
        }

        //挡板开关
        public static void BaffleOpen()
        {
            BaffleAutoMoveByPulse_Lift(int.Parse(Config.Mac_A.Pulse_Baffle_Open));
        }
        public static void BaffleClose()
        {
            BaffleAutoMoveByPulse_Lift(int.Parse(Config.Mac_A.Pulse_Baffle_Close));
        }

        //自动运行
        public static void BaffleAutoMoveByPulse_Lift(int pulse)
        {
            Baffle_Change(1);

            int p = ReadBafflePulse_Lift();
            if (pulse > p)
                BaffleAutoMoveByPulse_Up_Lift(pulse);
            else if (pulse < p)
                BaffleAutoMoveByPulse_Down_Lift(pulse);
        }

        const int BaffleAutoMovePulse_Up_Lift = 512;//D-2
        const int BaffleAutoMovePulse_Up_Lift_On = 12;//M-1
        public static void BaffleAutoMoveByPulse_Up_Lift(int pulse)
        {
            string s = PLC_Tcp_AP.Get16String(pulse, 8);
            string send = RegDMW + PLC_Tcp_AP.GetAdrD_1000(BaffleAutoMovePulse_Up_Lift) + "0002" + "04" + s.Substring(4, 4) + s.Substring(0, 4);
            string response;
            if (SendPLC(send, "挡板自动运行脉冲", out response))
            {
                send = RegMSW + PLC_Tcp_AP.GetAdrM(BaffleAutoMovePulse_Up_Lift_On) + "FF00";
                SendPLC(send, "挡板自动运行使能", out response);
            }
        }

        const int BaffleAutoMovePulse_Down_Lift = 514;//D-2
        const int BaffleAutoMovePulse_Down_Lift_On = 13;//M-1
        public static void BaffleAutoMoveByPulse_Down_Lift(int pulse)
        {
            string s = PLC_Tcp_AP.Get16String(pulse, 8);
            string send = RegDMW + PLC_Tcp_AP.GetAdrD_1000(BaffleAutoMovePulse_Down_Lift) + "0002" + "04" + s.Substring(4, 4) + s.Substring(0, 4);
            string response;
            if (SendPLC(send, "挡板自动运行脉冲", out response))
            {
                send = RegMSW + PLC_Tcp_AP.GetAdrM(BaffleAutoMovePulse_Down_Lift_On) + "FF00";
                SendPLC(send, "挡板自动运行使能", out response);
            }
        }

        //自动运行是否完成
        const int BaffleAutoMoveStatus_Up = 9;//D-1
        public static bool BaffleAutoMoveIsOK_Up()
        {
            bool result = false;
            string send = RegDR + PLC_Tcp_AP.GetAdrD_1000(BaffleAutoMoveStatus_Up) + "0001";
            string response;
            if (SendPLC(send, "挡板自动运行状态", out response))
            {
                int i = response.IndexOf(":");
                if (i >= 0)
                {
                    if (PLC_Tcp_AP.Get16Int(response.Substring(i + 7, 4)) == 1)
                        result = true;
                }
            }
            return result;
        }
        const int BaffleAutoMoveStatus_Down = 10;//D-1
        public static bool BaffleAutoMoveIsOK_Down()
        {
            bool result = false;
            string send = RegDR + PLC_Tcp_AP.GetAdrD_1000(BaffleAutoMoveStatus_Down) + "0001";
            string response;
            if (SendPLC(send, "挡板自动运行状态", out response))
            {
                int i = response.IndexOf(":");
                if (i >= 0)
                {
                    if (PLC_Tcp_AP.Get16Int(response.Substring(i + 7, 4)) == 1)
                        result = true;
                }
            }
            return result;
        }

        //挡板是否打开
        public static bool BaffleOnOpen()
        {
            bool result = false;
            int nowPulse = ReadBafflePulse_Lift();
            int openPulse = int.Parse(Config.Mac_A.Pulse_Baffle_Open);
            if (Math.Abs(nowPulse - openPulse) < 100)
                result = true;
            return result;
        }
        //挡板是否关闭
        public static bool BaffleOnClose()
        {
            bool result = false;
            int nowPulse = ReadBafflePulse_Lift();
            int closePulse = int.Parse(Config.Mac_A.Pulse_Baffle_Close);
            if (Math.Abs(nowPulse - closePulse) < 100)
                result = true;
            return result;
        }

        const int BafflePulse_Lift = 1336;//D-2
        //读取脉冲
        public static int ReadBafflePulse_Lift()
        {
            int result = 0;
            string send = RegDR + PLC_Tcp_AP.GetAdrD_1000(BafflePulse_Lift) + "0002";
            string response;
            if (SendPLC(send, "挡板脉冲", out response))
            {
                int i = response.IndexOf(":");
                if (i >= 0)
                {
                    string s = response.Substring(i + 7, 8);
                    result = PLC_Tcp_AP.Get16Int(s.Substring(4, 4) + s.Substring(0, 4));
                }
            }
            return result;
        }
        //原点复位
        const int BaffleOrigin = 2;//M-1
        public static bool BaffleOriginReset_Lift()
        {
            //BaffleOriginResetOFF_Lift();
            //PLC_Tcp.Origin_Out_On();

            string send = RegMSW + PLC_Tcp_AP.GetAdrM(BaffleOrigin) + "FF00";
            string response;
            return 
                SendPLC(send, "提升机挡板原点复位", out response);

                //send = RegMR + PLC_Tcp.GetAdrM(BaffleOrigin) + "0001";
                //SendPLC(send, "提升机挡板原点复位", out response);

                //return true;
        }
        public static bool BaffleOriginResetOFF_Lift()
        {
            string send = RegMSW + PLC_Tcp_AP.GetAdrM(BaffleOrigin) + "0000";
            string response;
            return SendPLC(send, "提升机挡板原点复位", out response);
        }

        const int BaffleOrigin_State = 2;//D-1
        //挡板原点复位是否完成
        public static bool BaffleOriginResetIsOK()
        {
            bool result = false;
            string send = RegDR + PLC_Tcp_AP.GetAdrD_1000(BaffleOrigin_State) + "0001";
            string response;
            if (SendPLC(send, "挡板原点复位完成状态", out response))
            {
                int i = response.IndexOf(":");
                if (i >= 0)
                {
                    if (PLC_Tcp_AP.Get16Int(response.Substring(i + 7, 4)) == 1)
                        result = true;
                }
            }
            return result;
        }

        //出药挡板状态
        const int BaffleManualMoveStatus_Up = 5;//D-1
        public static bool Baffle_Lift_IsMaxUp()
        {
            bool result = false;
            string send = RegDR + PLC_Tcp_AP.GetAdrD_1000(BaffleManualMoveStatus_Up) + "0001";
            string response;
            if (SendPLC(send, "出药挡板状态", out response))
            {
                int i = response.IndexOf(":");
                if (i >= 0)
                {
                    if (PLC_Tcp_AP.Get16Int(response.Substring(i + 7, 4)) == 1)
                        result = true;
                }
            }
            if (!result)
            {
                send = RegDR + PLC_Tcp_AP.GetAdrD_1000(BaffleAutoMoveStatus_Up) + "0001";
                if (SendPLC(send, "出药挡板状态", out response))
                {
                    int i = response.IndexOf(":");
                    if (i >= 0)
                    {
                        if (PLC_Tcp_AP.Get16Int(response.Substring(i + 7, 4)) == 2)
                            result = true;
                    }
                }
            }
            return result;
        }

        const int BaffleManualMoveStatus_Down = 6;//D-1
        public static bool Baffle_Lift_IsMinDown()
        {
            bool result = false;
            string send = RegDR + PLC_Tcp_AP.GetAdrD_1000(BaffleManualMoveStatus_Down) + "0001";
            string response;
            if (SendPLC(send, "出药挡板状态", out response))
            {
                int i = response.IndexOf(":");
                if (i >= 0)
                {
                    if (PLC_Tcp_AP.Get16Int(response.Substring(i + 7, 4)) == 1)
                        result = true;
                }
            }
            if (!result)
            {
                send = RegDR + PLC_Tcp_AP.GetAdrD_1000(BaffleAutoMoveStatus_Down) + "0001";
                if (SendPLC(send, "出药挡板状态", out response))
                {
                    int i = response.IndexOf(":");
                    if (i >= 0)
                    {
                        if (PLC_Tcp_AP.Get16Int(response.Substring(i + 7, 4)) == 2)
                            result = true;
                    }
                }
            }
            return result;
        }
        #endregion

        //关闭串口
        #region
        public static void Close()
        {
            if (spPLC != null)
            {
                spPLC.Close();
                spPLC.Dispose();
            }
        }
        #endregion

    }
}