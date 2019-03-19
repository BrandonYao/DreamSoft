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
    class PLC_Tcp_AP
    {
        static CSHelper.Msg csMsg = new CSHelper.Msg();
        static CSHelper.SQL csSql = new CSHelper.SQL();
        static CSHelper.LOG csLog = new CSHelper.LOG();

        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;

        public static TcpClient tct;
        public static Socket skt;
        const string PLCNo = "01";

        //功能码
        #region
        //读位装置寄存器
        //发送：0000（事物标识符）+0000（协议标识符）+0006（数据长度）+01（站号）+02（功能码）+0800（装置首地址）+0014（地址数量，20位）
        //接收：0000（事物标识符）+0000（协议标识符）+0006（数据长度）+01（站号）+02（功能码）+03（字节数，20/8取整）+81（1000 0001）+18（0001 1000）+06（0110）
        const string RegMR = "02";

        //读字装置寄存器
        //发送：0000（事物标识符）+0000（协议标识符）+0006（数据长度）+01（站号）+03（功能码）+1000（装置首地址）+0002（地址数量）
        //接收：0000（事物标识符）+0000（协议标识符）+0007（数据长度）+01（站号）+03（功能码）+04（字节数，地址数量*2）+0100（地址内容）+0200（地址内容）
        const string RegDR = "03";

        //写单个位装置寄存器
        //发送：0000（事物标识符）+0000（协议标识符）+0006（数据长度）+01（站号）+05（功能码）+0800（装置地址）+FF00（地址写入值，0000：0，FF00：1）
        //接收：0000（事物标识符）+0000（协议标识符）+0006（数据长度）+01（站号）+05（功能码）+0800（装置地址）+FF00（地址写入值）
        const string RegMSW = "05";

        //写单个字装置寄存器
        //发送：0000（事物标识符）+0000（协议标识符）+0006（数据长度）+01（站号）+06（功能码）+1000（装置地址）+0100（地址写入值）
        //接收：0000（事物标识符）+0000（协议标识符）+0006（数据长度）+01（站号）+06（功能码）+1000（装置地址）+0100（地址写入值）
        const string RegDSW = "06";

        //写多个位装置寄存器（256）
        //发送：0000（事物标识符）+0000（协议标识符）+000A（数据长度）+01（站号）+0F（功能码）+0800（装置首地址）+0014（地址数量，20位）+03（字节数，20/8取整）+81（1000 0001）+18（0001 1000）+06（0110）
        //接收：0000（事物标识符）+0000（协议标识符）+0006（数据长度）+01（站号）+0F（功能码）+0800（装置首地址）+0014（地址数量）
        const string RegMMW = "0F";

        //写多个字装置寄存器（64）
        //发送：0000（事物标识符）+0000（协议标识符）+000B（数据长度）+01（站号）+10（功能码）+1000（装置首地址）+0002（地址数量）+04（字节数，地址数量*2）+0100（地址内容）+0200（地址内容）
        //接收：0000（事物标识符）+0000（协议标识符）+0006（数据长度）+01（站号）+10（功能码）+1000（装置首地址）+0002（地址数量）
        const string RegDMW = "10";
        //异常回应：
        //0000（事物标识符）+0000（协议标识符）+0003（数据长度，可变）+01（站号）+82（80+功能码）+01（异常回应码）
        //异常回应码：
        //01：不支持的功能码
        //02：不支持的地址
        //03：数据长度超出范围
        #endregion

        //初始化
        #region
        static bool IpIsOK(string ip, int time)
        {
            bool result = false;
            Ping pingSender = new Ping();
            if (pingSender != null)
            {
                PingReply reply = pingSender.Send(ip, time);//第一个参数为ip地址，第二个参数为ping的时间 
                if (reply.Status == IPStatus.Success)
                {
                    result = true;
                }
            }
            return result;
        }

        //初始化PLC
        public static void Initial(bool first)
        {
            if (IpIsOK(Config.Mac_A.IP_PLC, 1000))
            {
                try
                {
                    if (tct != null)
                        tct.Close();
                    if (skt != null)
                    {
                        skt.Dispose();
                        skt.Close();
                    }
                    //string IP_PLC = "192.168.1.3";

                    tct = new TcpClient(Config.Mac_A.IP_PLC, 502);
                    skt = tct.Client;
                    skt.SendTimeout = skt.ReceiveTimeout = 1000;

                    //skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //IPEndPoint iep = new IPEndPoint(IPAddress.Parse(Config.Mac_A.IP_PLC), 502);
                    //skt.Connect(iep);
                    //skt.SendTimeout = skt.ReceiveTimeout = 1000;

                    if (skt.Connected && first)
                    {
                        //设定速度
                        SetSpeeds();

                        if (Config.Soft.Function == "O" && Config.Mac_A.PLC_Com == "Y")
                            PLC_Com_AP.SetSpeeds();

                        //全部使能
                        if (Config.Soft.Function == "I")
                        {
                            PlC_Add_OFF_All();
                            Thread.Sleep(100);
                            PlC_Add_ON_All();
                        }
                        else
                        {
                            PlC_Out_OFF_All();
                            Thread.Sleep(100);
                            PlC_Out_ON_All();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ThrowMsg == null)
                    {
                        csMsg.ShowWarning(ex.Message, false);
                    }
                    else ThrowMsg(ex.Message);
                    tct = null; skt = null;
                }
            }
            else
                csMsg.ShowWarning("IP地址未连接", true);
        }

        //检查指令是否执行成功
        static bool CheckResponse(string response, string mark)
        {
            bool result = false;
            string error = "";
            int i = int.Parse(response.Substring(14, 1));
            if (i >= 8)
            {
                string s = response.Substring(16, 2);

                switch (s)
                {
                    case "01":
                        error = "不支持的功能码";
                        break;
                    case "02":
                        error = "不支持的ModBus地址";
                        break;
                    case "03":
                        error = "数据长度超出范围";
                        break;
                }
                csLog.WriteLog(mark + ":" + error);
                if (ThrowMsg != null)
                    ThrowMsg(mark + ":" + error);

            }
            else
                result = true;

            return result;
        }

        static bool PLCIsBusy = false;

        static bool SendPLC(string send, string mark, out string response)
        {
            bool result = false;
            response = "";
            if (skt != null)
            {
                try
                {
                    send = PLCNo + send;
                    send = "00000000" + Get16String(send.Length / 2, 4) + send;
                    byte[] buffer_send = GetBytes(send);

                    while (PLCIsBusy)
                        Thread.Sleep(20);
                    PLCIsBusy = true;
                    skt.Send(buffer_send);
                    //Thread.Sleep(50);
                    int i = 0, j = 0;
                    DateTime begin = DateTime.Now;
                    DateTime end;
                    do
                    {
                        Thread.Sleep(50);
                        i = skt.Available;
                        Thread.Sleep(50);
                        j = skt.Available;
                        end = DateTime.Now;
                    } while (i != j && end <= begin.AddSeconds(1));
                    if (end <= begin.AddSeconds(1) && j > 0)
                    {
                        byte[] buffer_response = new byte[j];
                        skt.Receive(buffer_response);
                        response = GetString(buffer_response);
                        if (response.Length >= 12 && int.Parse(response.Substring(8, 4)) > 0)
                            result = CheckResponse(response, mark);
                    }
                    PLCIsBusy = false;
                }
                catch (Exception ex)
                {
                    string msg = mark + ":" + ex.Message;
                    if(ex.GetType().Equals(typeof(System.Net.Sockets.SocketException)))
                    {
                        Initial(false);
                    }

                    //记录错误信息
                    csLog.WriteLog(msg);
                    if (ThrowMsg == null)
                    {
                        csMsg.ShowWarning(msg, false);
                    }
                    else ThrowMsg(msg);

                    PLCIsBusy = false;
                }
            }
            else
            {
                string msg = "PLC未连接";
                //记录错误信息
                csLog.WriteLog(msg);
                if (ThrowMsg != null)
                    ThrowMsg(msg);
            }
            return result;
        }
        //整型转换为16进制字符串
        public static string Get16String(int value, int len)
        {
            return value.ToString("X").ToUpper().PadLeft(len, '0');
        }
        //16进制字符串转换为整型
        public static int Get16Int(string value)
        {
            return Convert.ToInt32(value, 16);
        }
        //字符串转字节
        public static Byte[] GetBytes(string send)
        {
            byte[] buffer_Send = new byte[send.Length / 2];
            for (int i = 0; i < buffer_Send.Length; i++)
            {
                byte b = Convert.ToByte(Convert.ToInt32(send.Substring(i * 2, 2), 16));
                buffer_Send[i] = b;
            }
            return buffer_Send;
        }
        //字节转字符串
        public static string GetString(byte[] data)
        {
            string result = "";
            foreach (byte b in data)
            {
                result += Get16String(Convert.ToInt32(b), 2);
            }
            return result;
        }

        public static string GetAdrD_1000(int i)
        {
            return Get16String(i + 4096, 4);
        }
        static string GetAdrD_8000(int i)
        {
            return Get16String(i + 32768, 4);
        }
        public static string GetAdrM(int i)
        {
            return Get16String(i + 2048, 4);
        }

        static string GetReal(float f)
        {
            string result = "";
            byte[] bs = BitConverter.GetBytes(f);
            string s = BitConverter.ToString(bs).Replace("-", "");
            for (int i = s.Length - 2; i >= 0; i -= 2)
            {
                result += s.Substring(i, 2);
            }
            return result;
        }
        static string GetReal(int i)
        {
            float f = i;
            return GetReal(f);
        }
        static float GetFloat(string s)
        {
            float f = 0;
            byte[] bs = new byte[s.Length / 2];
            for (int i = 0; i < bs.Length; i++)
            {
                bs[i] = Convert.ToByte(Get16Int(s.Substring(s.Length - 2 - 2 * i, 2)));
            }
            f = BitConverter.ToSingle(bs, 0);
            return f;
        }
        #endregion

        //加药切换手动/自动
        #region
        const int Add_Manual_On = 52;//M-1
        const int Add_Up_On = 53;//M-1
        const int Add_Auto_On = 57;//M-1
        const int Add_Plate_On = 58;//M-1
        public static void ChangeAdd(int auto)
        {
            string value = "FF00";
            int adr = 0;
            switch (auto)
            {
                case 0:
                    adr = Add_Manual_On;
                    break;
                case 1:
                    adr = Add_Auto_On;
                    break;
                case 2:
                    adr = Add_Up_On;
                    break;
            }
            string send = RegMSW + GetAdrM(adr) + value;
            string response;
            SendPLC(send, "加药手自动切换", out response);
            if (auto == 1)
            {
                send = RegMSW + GetAdrM(Add_Plate_On) + value;
                SendPLC(send, "加药推板自动", out response);
            }
        }

        public static void Add_OFF()
        {
            string send = RegMSW + GetAdrM(Add_Manual_On) + "0000";
            string response;
            SendPLC(send, "加药关闭", out response);
            send = RegMSW + GetAdrM(Add_Auto_On) + "0000";
            SendPLC(send, "加药关闭", out response);
        }
        #endregion

        //出药切换手动/自动
        #region
        const int Out_Manual_On = 70;//M-1
        const int Out_Auto_On = 54;//M-1
        public static void ChangeOut(int auto)
        {
            string value = "FF00";
            int adr = 0;
            switch (auto)
            {
                case 0:
                    adr = Out_Manual_On;
                    break;
                case 1:
                    adr = Out_Auto_On;
                    break;
            }
            string send = RegMSW + GetAdrM(adr) + value;
            string response;
            SendPLC(send, "出药手自动切换", out response);
        }
        #endregion

        //设定速度/加减速
        #region

        const int Speed_X_Auto = 72;//D-2
        const int Speed_Z_Auto = 84;//D-2
        const int Speed_Left_Auto = 90;//D-2
        const int Speed_Right_Auto = 96;//D-2

        const int Speed_Lift_Auto = 48;//D-2

        //加速度
        const int Acc_X_Manual = 112;//D-2
        const int Acc_Z_Manual = 114;//D-2
        const int Acc_Left_Manual = 116;//D-2
        const int Acc_Right_Manual = 118;//D-2

        const int Acc_X_Auto = 126;//D-2
        const int Acc_Z_Auto = 128;//D-2
        const int Acc_Left_Auto = 130;//D-2
        const int Acc_Right_Auto = 130;//D-2

        const int Acc_Left_Up = 124;//D-2
        const int Acc_Right_Up = 124;//D-2

        const int Acc_Lift_Manual = 120;//D-2
        const int Acc_Lift_Auto = 132;//D-2

        static void SetSpeeds()
        {
            string speed = "";
            string acc = "";
            string send = "";
            string response;

            if (Config.Soft.Function == "I")
            {
                //速度
                speed = GetReal(int.Parse(Config.Mac_A.Speed_Auto_X));
                send = RegDMW + GetAdrD_1000(Speed_X_Auto) + "0002" + "04" + speed.Substring(4, 4) + speed.Substring(0, 4);
                if (!SendPLC(send, "X轴自动速度", out response)) return;
                speed = GetReal(int.Parse(Config.Mac_A.Speed_Auto_Z));
                send = RegDMW + GetAdrD_1000(Speed_Z_Auto) + "0002" + "04" + speed.Substring(4, 4) + speed.Substring(0, 4);
                if (!SendPLC(send, "Z轴自动速度", out response)) return;
                speed = GetReal(int.Parse(Config.Mac_A.Speed_Auto_Plate));
                send = RegDMW + GetAdrD_1000(Speed_Left_Auto) + "0002" + "04" + speed.Substring(4, 4) + speed.Substring(0, 4);
                if (!SendPLC(send, "左臂自动速度", out response)) return;
                send = RegDMW + GetAdrD_1000(Speed_Right_Auto) + "0002" + "04" + speed.Substring(4, 4) + speed.Substring(0, 4);
                if (!SendPLC(send, "右臂自动速度", out response)) return;

                //加减速
                acc = GetReal(int.Parse(Config.Mac_A.Acc_Manual_X));
                send = RegDMW + GetAdrD_1000(Acc_X_Manual) + "0002" + "04" + acc.Substring(4, 4) + acc.Substring(0, 4);
                if (!SendPLC(send, "X轴手动加速度", out response)) return;
                acc = GetReal(int.Parse(Config.Mac_A.Acc_Manual_Z));
                send = RegDMW + GetAdrD_1000(Acc_Z_Manual) + "0002" + "04" + acc.Substring(4, 4) + acc.Substring(0, 4);
                if (!SendPLC(send, "Z轴手动加速度", out response)) return;
                acc = GetReal(int.Parse(Config.Mac_A.Acc_Manual_Plate));
                send = RegDMW + GetAdrD_1000(Acc_Left_Manual) + "0002" + "04" + acc.Substring(4, 4) + acc.Substring(0, 4);
                if (!SendPLC(send, "左臂手动加速度", out response)) return;
                send = RegDMW + GetAdrD_1000(Acc_Right_Manual) + "0002" + "04" + acc.Substring(4, 4) + acc.Substring(0, 4);
                if (!SendPLC(send, "右臂手动加速度", out response)) return;

                acc = GetReal(int.Parse(Config.Mac_A.Acc_Auto_X));
                send = RegDMW + GetAdrD_1000(Acc_X_Auto) + "0002" + "04" + acc.Substring(4, 4) + acc.Substring(0, 4);
                if (!SendPLC(send, "X轴自动加速度", out response)) return;
                acc = GetReal(int.Parse(Config.Mac_A.Acc_Auto_Z));
                send = RegDMW + GetAdrD_1000(Acc_Z_Auto) + "0002" + "04" + acc.Substring(4, 4) + acc.Substring(0, 4);
                if (!SendPLC(send, "Z轴自动加速度", out response)) return;
                acc = GetReal(int.Parse(Config.Mac_A.Acc_Auto_Plate));
                send = RegDMW + GetAdrD_1000(Acc_Left_Auto) + "0002" + "04" + acc.Substring(4, 4) + acc.Substring(0, 4);
                if (!SendPLC(send, "左臂自动加速度", out response)) return;

                acc = GetReal(int.Parse(Config.Mac_A.Acc_Up_Plate));
                send = RegDMW + GetAdrD_1000(Acc_Left_Up) + "0002" + "04" + acc.Substring(4, 4) + acc.Substring(0, 4);
                if (!SendPLC(send, "左臂计数上推加速度", out response)) return;
            }
            else
            {
                //速度
                speed = GetReal(int.Parse(Config.Mac_A.Speed_Auto_Lift));
                send = RegDMW + GetAdrD_1000(Speed_Lift_Auto) + "0002" + "04" + speed.Substring(4, 4) + speed.Substring(0, 4);
                if (!SendPLC(send, "提升机自动速度", out response)) return;

                //加速度
                acc = GetReal(int.Parse(Config.Mac_A.Acc_Manual_Lift));
                send = RegDMW + GetAdrD_1000(Acc_Lift_Manual) + "0002" + "04" + acc.Substring(4, 4) + acc.Substring(0, 4);
                if (!SendPLC(send, "提升机手动加速度", out response)) return;
                acc = GetReal(int.Parse(Config.Mac_A.Acc_Auto_Lift));
                send = RegDMW + GetAdrD_1000(Acc_Lift_Auto) + "0002" + "04" + acc.Substring(4, 4) + acc.Substring(0, 4);
                if (!SendPLC(send, "提升机自动加速度", out response)) return;
            }
        }

        #endregion

        //全部使能
        #region
        const int X_On = 80;//M-1
        const int Z_On = 81;//M-1
        const int Left_On = 82;//M-1
        const int Right_On = 83;//M-1

        static void PlC_Add_ON_All()
        {
            string send = RegMMW + GetAdrM(X_On) + "0004" + "01" + "0F";//(0000 1111)
            string response;
            SendPLC(send, "PLC加药使能", out response);
        }
        static void PlC_Add_OFF_All()
        {
            string send = RegMMW + GetAdrM(X_On) + "0004" + "01" + "00";//(0000 0000)
            string response;
            SendPLC(send, "PLC加药使能", out response);
        }

        const int Lift_On = 84;//M-1
        static void PlC_Out_ON_All()
        {
            string send = RegMSW + GetAdrM(Lift_On) + "FF00";
            string response;
            SendPLC(send, "PLC出药使能", out response);
        }
        static void PlC_Out_OFF_All()
        {
            string send = RegMSW + GetAdrM(Lift_On) + "0000";
            string response;
            SendPLC(send, "PLC出药使能", out response);
        }

        const int Origin_Add = 120;//M-1
        const int Origin_Out = 121;//M-1
        static void Origin_Add_On()
        {
            string send = RegMSW + GetAdrM(Origin_Add) + "FF00";
            string response;
            SendPLC(send, "加药原点触发", out response);
        }
        public static void Origin_Out_On()
        {
            string send = RegMSW + GetAdrM(Origin_Out) + "FF00";
            string response;
            SendPLC(send, "出药原点触发", out response);
        }
        #endregion

        //机械手
        #region
        //导轨类型
        #region
        public enum TrackType
        {
            X,
            Z
        }
        #endregion
        //机械手运动方向
        #region
        public enum ExtramanMoveDir
        {
            Up,
            Down,
            Left,
            Right,
            XStop,
            ZStop
        }
        #endregion
        //机械手原点复位
        const int Extraman_Origin_X = 8;//M-1
        const int Extraman_Origin_Z = 9;//M-1
        public static bool ExtramanOriginReset()
        {
            ExtramanOriginReset_OFF();
            Thread.Sleep(100);
            //Origin_Add_On();
            //Thread.Sleep(100);
            PlC_Add_OFF_All();
            Thread.Sleep(100);
            PlC_Add_ON_All();
            Thread.Sleep(100);

            string response;
            //开起使能
            string send = RegMMW + GetAdrM(Extraman_Origin_X) + "0002" + "01" + "03";//(0011)
            return SendPLC(send, "机械手原点复位", out response);
        }
        public static bool ExtramanOriginReset_OFF()
        {
            string response;
            //关闭使能
            string send = RegMMW + GetAdrM(Extraman_Origin_X) + "0002" + "01" + "00";//(0000)
            return SendPLC(send, "机械手原点复位", out response);
        }
        //机械手原点复位是否完成
        const int Extraman_Origin_State_X_Up = 0;//M-1
        const int Extraman_Origin_State_X_Down = 1;//M-1
        const int Extraman_Origin_State_Z = 2;//M-1
        public static bool ExtramanOriginResetIsOK()
        {
            bool result = false;
            string send = RegMR + GetAdrM(Extraman_Origin_State_X_Up) + "0003";
            string response;
            if (SendPLC(send, "机械手原点复位状态", out response))
            {
                string s = response.Substring(18, 2);
                if (Get16Int(s) == 7)
                {
                    result = true;
                }
            }
            return result;
        }
        //机械手原点返回是否报错
        const int TrackErrorCode_X1_Origin = 2;//D-1
        const int TrackErrorCode_X2_Origin = 4;//D-1
        const int TrackErrorCode_Z_Origin = 6;//D-1
        public static bool ExtramanOriginResetState(TrackErrorType type)
        {
            bool result = true;
            int adr = 0;
            //原点返回报警代码
            switch (type)
            {
                case TrackErrorType.X1:
                    adr = TrackErrorCode_X1_Origin;
                    break;
                case TrackErrorType.X2:
                    adr = TrackErrorCode_X2_Origin;
                    break;
                case TrackErrorType.Z:
                    adr = TrackErrorCode_Z_Origin;
                    break;
            }
            string send = RegDR + GetAdrD_1000(adr) + "0001";
            string response;
            if (SendPLC(send, "机械手原点返回报警", out response))
            {
                if (Get16Int(response.Substring(18, 4)) > 0)
                    result = false;
            }
            return result;
        }

        //机械手手动运行
        const int Speed_X_Manual = 26;//D-2
        const int Speed_Z_Manual = 30;//D-2
        const int ExtramanManualMove_X_FX = 28;//D-1
        const int ExtramanManualMove_X_On = 28;//M-1
        const int ExtramanManualMove_Z_FX = 32;//D-1
        const int ExtramanManualMove_Z_On = 29;//M-1
        public static void ExtramanManualMove(ExtramanMoveDir dir)
        {
            int speed = 0;
            int adr_Speed = 0;
            int adr_FX = 0;
            int fx = 0;
            int adr_On = 0;
            switch (dir)
            {
                case ExtramanMoveDir.Up:
                    adr_Speed = Speed_Z_Manual;
                    speed = int.Parse(Config.Mac_A.Speed_Manual_Z);
                    adr_FX = ExtramanManualMove_Z_FX;
                    fx = 1;
                    adr_On = ExtramanManualMove_Z_On;
                    break;
                case ExtramanMoveDir.Down:
                    adr_Speed = Speed_Z_Manual;
                    speed = int.Parse(Config.Mac_A.Speed_Manual_Z);
                    adr_FX = ExtramanManualMove_Z_FX;
                    fx = -1;
                    adr_On = ExtramanManualMove_Z_On;
                    break;
                case ExtramanMoveDir.Left:
                    adr_Speed = Speed_X_Manual;
                    speed = int.Parse(Config.Mac_A.Speed_Manual_X);
                    adr_FX = ExtramanManualMove_X_FX;
                    fx = 1;
                    adr_On = ExtramanManualMove_X_On;
                    break;
                case ExtramanMoveDir.Right:
                    adr_Speed = Speed_X_Manual;
                    speed = int.Parse(Config.Mac_A.Speed_Manual_X);
                    adr_FX = ExtramanManualMove_X_FX;
                    fx = -1;
                    adr_On = ExtramanManualMove_X_On;
                    break;
                case ExtramanMoveDir.XStop:
                    adr_Speed = Speed_X_Manual;
                    adr_On = ExtramanManualMove_X_On;
                    break;
                case ExtramanMoveDir.ZStop:
                    adr_Speed = Speed_Z_Manual;
                    adr_On = ExtramanManualMove_Z_On;
                    break;
            }
            string send = "";
            string response;
            string s = "";
            //方向
            if (dir != ExtramanMoveDir.XStop && dir != ExtramanMoveDir.ZStop)
            {
                s = Get16String(fx, 4);
                send = RegDSW + GetAdrD_1000(adr_FX) + s;
                SendPLC(send, "X、Z手动运行", out response);
            }
            //速度
            s = GetReal(speed);
            send = RegDMW + GetAdrD_1000(adr_Speed) + "0002" + "04" + s.Substring(4, 4) + s.Substring(0, 4);
            SendPLC(send, "X、Z手动速度", out response);
            //使能
            send = RegMSW + GetAdrM(adr_On) + "FF00";
            SendPLC(send, "X、Z手动运行使能开启", out response);
        }

        //机械手运行到指定储位，加药/盘点，左/右
        //public static void ExtramanMoveToPos(string pos, string type, string dir)
        //{
        //    string x = "0";
        //    string z = "0";
        //    try
        //    {
        //        string sql = "select x,z from pospulse where drugpos='" + pos + "' and postype='" + type + "' and posdir='" + dir + "'";
        //        DataTable dtPulse = new DataTable();
        //        csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPulse);
        //         if (dtPulse != null && dtPulse.Rows.Count > 0)
        //        {
        //            x = dtPulse.Rows[0]["x"].ToString().Trim();
        //            z = dtPulse.Rows[0]["z"].ToString().Trim();
        //        }
        //        if (x != "0" && z != "0")
        //        {
        //            ExtramanAutoMoveToPulse(int.Parse(x), int.Parse(z));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        csLog.WriteLog("机械手运行到指定储位：" + ex.Message);
        //    }
        //}
        //机械手运行到指定脉冲
        public static bool ExtramanAutoMoveToPulse(float x, float z)
        {
            //ExtramanMoveOFF();

            return SetExtramanMovePulseX(x) && SetExtramanMovePulseZ(z) && ExtramanMoveOn();
        }
        //设定X轴目标脉冲
        const int ExtramanMovePulse_X = 70;//D-2
        public static bool SetExtramanMovePulseX(float x)
        {
            string pulse = GetReal(x);
            string send = RegDMW + GetAdrD_1000(ExtramanMovePulse_X) + "0002" + "04" + pulse.Substring(4, 4) + pulse.Substring(0, 4);
            string response;
            return SendPLC(send, "X轴运行脉冲", out response);
        }
        //设定Z轴目标脉冲
        const int ExtramanMovePulse_Z = 82;//D-2
        public static bool SetExtramanMovePulseZ(float z)
        {
            string pulse = GetReal(z);
            string send = RegDMW + GetAdrD_1000(ExtramanMovePulse_Z) + "0002" + "04" + pulse.Substring(4, 4) + pulse.Substring(0, 4);
            string response;
            return SendPLC(send, "Z轴运行脉冲", out response);
        }
        //机械手运行使能
        const int ExtramanMovePulse_X_On = 59;//M-1
        const int ExtramanMovePulse_Z_On = 60;//M-1
        public static bool ExtramanMoveOn()
        {
            string send = RegMMW + GetAdrM(ExtramanMovePulse_X_On) + "0002" + "01" + "03";//(0011)
            string response;
            return SendPLC(send, "机械手自动运行使能开启", out response);
        }
        public static bool ExtramanMoveOFF()
        {
            string send = RegMMW + GetAdrM(ExtramanMovePulse_X_On) + "0002" + "01" + "00";//(0000)
            string response;
            return SendPLC(send, "机械手自动运行使能关闭", out response);
        }
        //机械手自动运行是否完成
        const int ExtramanAutoMoveIsOK_X = 152;//D-1 48;//M-1
        const int ExtramanAutoMoveIsOK_Z = 153;//D-1 49;//M-1
        public static bool ExtramanAutoMoveIsOK()
        {
            bool result = false;
            string send = RegDR + GetAdrD_1000(ExtramanAutoMoveIsOK_X) + "0002";
            string response;
            if (SendPLC(send, "机械手自动运行完成状态", out response))
            {
                string s = response.Substring(18, 8);
                if (Get16Int(s.Substring(0, 4)) == 1 && Get16Int(s.Substring(4, 4)) == 1)
                {
                    result = true;
                }
            }
            return result;
        }

        //读取机械手X轴脉冲
        const int ExtramanPulse_X = 24602;//D-2
        public static float ReadExtramanPulseX()
        {
            float result = 0;
            string send = RegDR + GetAdrD_8000(ExtramanPulse_X) + "0002";
            string response;
            if (SendPLC(send, "X轴脉冲", out response))
            {
                string s = response.Substring(18, 8);
                int i = Get16Int(s.Substring(4, 4) + s.Substring(0, 4));
                float f = i * 360.0f / 10000;
                result = (float)Math.Round(f, 2);
            }
            return result;
        }
        //读取机械手Z轴脉冲
        const int ExtramanPulse_Z = 25114;//D-2
        public static float ReadExtramanPulseZ()
        {
            float result = 0;
            string send = RegDR + GetAdrD_8000(ExtramanPulse_Z) + "0002";
            string response;
            if (SendPLC(send, "Z轴脉冲", out response))
            {
                string s = response.Substring(18, 8);
                int i = Get16Int(s.Substring(4, 4) + s.Substring(0, 4));
                float f = i * 360.0f / 10000;
                result = (float)Math.Round(f, 2);
            }
            return result;
        }

        //读取导轨报警复位状态
        const int TrackReset_X_Status_Top = 15;//M-1
        const int TrackReset_X_Status_Bottom = 17;//M-1
        const int TrackReset_Z_Status = 19;//M-1
        public static bool TrackErrorResetIsOK(TrackType type)
        {
            bool result = false;
            string send;
            string response;
            if (type == TrackType.X)
            {
                send = RegMR + GetAdrM(TrackReset_X_Status_Bottom) + "0003";
                if (SendPLC(send, "X导轨报警复位状态", out response)) //(0101/0111)
                {
                    string s = response.Substring(18, 2);
                    int i = Get16Int(s);
                    if (i == 5 || i == 7)
                        result = true;
                }
            }
            else
            {
                send = RegMR + GetAdrM(TrackReset_Z_Status) + "0001";
                if (SendPLC(send, "Z导轨报警复位状态", out response))
                {
                    int i = Get16Int(response.Substring(19, 1));
                    if (i == 1)
                        result = true;
                }
            }
            return result;
        }
        //导轨报警复位
        const int TrackReset_X = 105;//M-1
        const int TrackReset_Z = 106;//M-1
        public static void TrackErrorReset(TrackType type)
        {
            int adr = 0;
            switch (type)
            {
                case TrackType.X:
                    adr = TrackReset_X;
                    break;
                case TrackType.Z:
                    adr = TrackReset_Z;
                    break;
            }
            string send = RegMSW + GetAdrM(adr) + "0000";
            string response;
            SendPLC(send, "导轨报警复位", out response);
            send = RegMSW + GetAdrM(adr) + "FF00";
            SendPLC(send, "导轨报警复位", out response);

            Thread.Sleep(500);
            if (TrackErrorResetIsOK(type))
            {
                csMsg.ShowInfo("操作成功", false);
                //PlC_Add_OFF_All();
                //Thread.Sleep(200);
                //PlC_Add_ON_All();
            }
        }

        public enum TrackErrorType
        {
            X1,
            X2,
            Z
        }
        //读取报警代码
        const int TrackErrorCode_X1 = 14;//D-1
        const int TrackErrorCode_X2 = 16;//D-1
        const int TrackErrorCode_Z = 18;//D-1
        public static string ReadTrackErrorCode(TrackErrorType type)
        {
            string result = "";
            //报警代码
            int adr = 0;
            switch (type)
            {
                case TrackErrorType.X1:
                    adr = Track_State_X_Top;
                    break;
                case TrackErrorType.X2:
                    adr = Track_State_X_Bottom;
                    break;
                case TrackErrorType.Z:
                    adr = Track_State_Z;
                    break;
            }
            string send = RegDR + GetAdrD_8000(adr) + "0001";
            string response;
            if (SendPLC(send, "机械手状态3", out response))
            {
                result = response.Substring(18, 4);
            }
            return result;
        }


        //机械手运行状态
        const int Track_State_X_Top = 24606;//D-1
        const int Track_State_X_Bottom = 24862;//D-1
        const int Track_State_Z = 25118;//D-1
        //public static string ReadTrackState(TrackErrorType type)
        //{
        //    string result = "";
        //    int adr = 0;
        //    switch (type)
        //    {
        //        case TrackErrorType.X1:
        //            adr = Track_State_X_Top;
        //            break;
        //        case TrackErrorType.X2:
        //            adr = Track_State_X_Bottom;
        //            break;
        //        case TrackErrorType.Z:
        //            adr = Track_State_Z;
        //            break;
        //    }
        //    string send = RegDR + GetAdrD_8000(adr) + "0001";
        //    string response;
        //    if (SendPLC(send, "机械手状态1", out response))
        //    {
        //        result = Get16Int(response.Substring(18, 4)).ToString();
        //    }
        //    return result;
        //}
        public static bool TrackStateIsOK(TrackErrorType type)
        {
            bool result = true;
            int adr = 0;
            switch (type)
            {
                case TrackErrorType.X1:
                    adr = Track_State_X_Top;
                    break;
                case TrackErrorType.X2:
                    adr = Track_State_X_Bottom;
                    break;
                case TrackErrorType.Z:
                    adr = Track_State_Z;
                    break;
            }
            string send = RegDR + GetAdrD_8000(adr) + "0001";
            string response;
            if (SendPLC(send, "机械手状态2", out response))
            {
                if (Get16Int(response.Substring(18, 4)) == 2)
                    result = false;
            }
            return result;
        }
        #endregion

        //加药仓
        #region
        //加药仓类型
        #region
        public enum PlateType
        {
            Left,
            Right
        }
        #endregion

        //推药板运行方向
        #region
        public enum PlateMoveDir
        {
            Up,
            Down,
            Stop
        }
        #endregion
        const int Speed_Left_Manual = 34;//D-2
        const int Speed_Right_Manual = 38;//D-2
        const int PlateManualMove_Left_FX = 36;//D-1
        const int PlateManualMove_Left_On = 30;//M-1
        const int PlateManualMove_Right_FX = 40;//D-1
        const int PlateManualMove_Right_On = 31;//M-1
        public static void PlateManualMove(PlateType type, PlateMoveDir dir)
        {
            int adr_FX = 0;
            int fx = 0;
            int adr_speed = 0;
            int speed = 0;
            int adr_On = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr_FX = PlateManualMove_Left_FX;
                    adr_speed = Speed_Left_Manual;
                    adr_On = PlateManualMove_Left_On;
                    break;
                case PlateType.Right:
                    adr_FX = PlateManualMove_Right_FX;
                    adr_speed = Speed_Right_Manual;
                    adr_On = PlateManualMove_Right_On;
                    break;
            }
            switch (dir)
            {
                case PlateMoveDir.Up:
                    fx = 1;
                    speed = int.Parse(Config.Mac_A.Speed_Manual_Plate);
                    break;
                case PlateMoveDir.Down:
                    fx = -1;
                    speed = int.Parse(Config.Mac_A.Speed_Manual_Plate);
                    break;
                case PlateMoveDir.Stop:
                    fx = 0;
                    break;
            }
            string send = "";
            string response;
            //方向
            if (dir != PlateMoveDir.Stop)
            {
                send = RegDSW + GetAdrD_1000(adr_FX) + Get16String(fx, 4);//"0006" + 
                SendPLC(send, "加药仓手动运行", out response);
            }
            //速度
            string s = GetReal(speed);
            send = RegDMW + GetAdrD_1000(adr_speed) + "0002" + "04" + s.Substring(4, 4) + s.Substring(0, 4);
            SendPLC(send, "加药仓手动速度", out response);
            send = RegMSW + GetAdrM(adr_On) + "FF00";
            SendPLC(send, "加药仓手动运行使能开启", out response);
        }

        const int PlateOrigin = 10;//M-1
        //推药板原点复位
        public static bool PlateOriginReset()
        {
            PlateOriginReset_OFF();
            //Origin_Add_On();
            //PlC_OFF_All();
            //PlC_ON_All();
            //Thread.Sleep(500);

            string send = RegMSW + GetAdrM(PlateOrigin) + "FF00";
            string response;
            return SendPLC(send, "加药仓原点复位", out response);
        }
        public static void PlateOriginReset_OFF()
        {
            string send = RegMSW + GetAdrM(PlateOrigin) + "0000";
            string response;
            SendPLC(send, "加药仓原点复位", out response);
        }
        //推药板原点复位是否完成
        const int Plate_Origin_Left = 3;//M-1
        const int Plate_Origin_Right = 4;//M-1
        public static bool PlateOriginResetIsOK()
        {
            bool result = false;
            string send = RegMR + GetAdrM(Plate_Origin_Left) + "0002";
            string response;
            if (SendPLC(send, "加药仓原点复位状态", out response))
            {
                if (Get16Int(response.Substring(18, 2)) == 3)
                {
                    result = true;
                }
            }
            return result;
        }
        //推药板原点复位状态
        const int PlateErrorCode_Left_Origin = 8;//D-1
        const int PlateErrorCode_Right_Origin = 10;//D-1
        public static bool PlateOriginResetState(PlateType type)
        {
            bool result = true;
            int adr = 0;
            //推药板原点复位状态
            switch (type)
            {
                case PlateType.Left:
                    adr = PlateErrorCode_Left_Origin;
                    break;
                case PlateType.Right:
                    adr = PlateErrorCode_Right_Origin;
                    break;
            }
            string send = RegDR + GetAdrD_1000(adr) + "0001";
            string response;
            if (SendPLC(send, "推药板原点复位状态", out response))
            {
                if (Get16Int(response.Substring(18, 4)) > 0)
                    result = false;
            }
            return result;
        }

        const int PlatePulse_Left = 25370;//D-2
        const int PlatePulse_Right = 25626;//D-2
        //读取推药板当前脉冲
        public static float ReadPlatePulse(PlateType type)
        {
            float result = 0;
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = PlatePulse_Left;
                    break;
                case PlateType.Right:
                    adr = PlatePulse_Right;
                    break;
            }
            string send = RegDR + GetAdrD_8000(adr) + "0002";
            string response;
            if (SendPLC(send, "加药仓脉冲", out response))
            {
                string s = response.Substring(18, 8);
                int i = Get16Int(s.Substring(4, 4) + s.Substring(0, 4));
                float f = i * 360.0f / 10000;
                result = (float)Math.Round(f, 2);
            }
            return result;
        }

        const int Speed_Left_Up = 54;//D-2
        const int Speed_Right_Up = 58;//D-2
        const int PlateAutoUp_Left = 44;//M-1
        const int PlateAutoUp_Right = 45;//M-1
        //加药仓算数量上推
        public static void PlateMoveUp(PlateType type)
        {
            int adr_Speed = 0;
            int adr_On = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr_Speed = Speed_Left_Up;
                    adr_On = PlateAutoUp_Left;
                    break;
                case PlateType.Right:
                    adr_Speed = Speed_Right_Up;
                    adr_On = PlateAutoUp_Right;
                    break;
            }
            string response;
            string speed = GetReal(int.Parse(Config.Mac_A.Speed_Up_Plate));
            string send = RegDMW + GetAdrD_1000(adr_Speed) + "0002" + "04" + speed.Substring(4, 4) + speed.Substring(0, 4);
            if (SendPLC(send, "左臂计数上推速度", out response))
            {
                send = RegMSW + GetAdrM(adr_On) + "FF00";
                SendPLC(send, "加药仓自动上推", out response);
            }
        }

        //加药仓算数量上推是否完成
        public static bool PlateMoveUpIsOK(PlateType type)
        {
            bool result = false;
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = Speed_Left_Up;
                    break;
                case PlateType.Right:
                    adr = Speed_Right_Up;
                    break;
            }
            string send = RegDR + GetAdrD_1000(adr) + "0002";
            string response;
            if (SendPLC(send, "加药仓原点复位状态", out response))
            {
                string s = response.Substring(18, 8);
                if (GetFloat(s) == 0.0f)
                    result = true;
            }
            return result;
        }

        //加药仓按脉冲自动运行
        const int PlateAutoMoveByPulse_Left = 88;//D-2
        const int PlateAutoMoveByPulse_Right = 94;//D-2
        public static void PlateAutoMoveToPulse(PlateType type, float pulse)
        {
            //PlateAutoMoveToPulseOFF(type);
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = PlateAutoMoveByPulse_Left;
                    break;
                case PlateType.Right:
                    adr = PlateAutoMoveByPulse_Right;
                    break;
            }
            string s = GetReal(pulse);
            string send = RegDMW + GetAdrD_1000(adr) + "0002" + "04" + s.Substring(4, 4) + s.Substring(0, 4);
            string response;
            if (SendPLC(send, "加药仓自动运行脉冲", out response))
                PlateAutoMoveToPulseOn(type);
        }
        //加药仓自动运行使能
        const int PlateAutoMove_On_Left = 61;//M-1
        const int PlateAutoMove_On_Right = 62;//M-1
        static void PlateAutoMoveToPulseOn(PlateType type)
        {
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = PlateAutoMove_On_Left;
                    break;
                case PlateType.Right:
                    adr = PlateAutoMove_On_Right;
                    break;
            }
            string send = RegMSW + GetAdrM(adr) + "FF00";
            string response;
            SendPLC(send, "加药仓自动运行使能开启", out response);
        }
        public static void PlateAutoMoveToPulseOFF(PlateType type)
        {
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = PlateAutoMove_On_Left;
                    break;
                case PlateType.Right:
                    adr = PlateAutoMove_On_Right;
                    break;
            }
            string send = RegMSW + GetAdrM(adr) + "0000";
            string response;
            SendPLC(send, "加药仓自动运行使能关闭", out response);
        }
        //加药仓按脉冲自动运行是否完成
        const int PlateAutoMoveByPulseStatus_Left = 154;//D-1 50;//M-1
        const int PlateAutoMoveByPulseStatus_Right = 155;//D-1 51;//M-1
        public static bool PlateAutoMoveToPulseIsOK(PlateType type)
        {
            bool result = false;
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = PlateAutoMoveByPulseStatus_Left;
                    break;
                case PlateType.Right:
                    adr = PlateAutoMoveByPulseStatus_Right;
                    break;
            }
            string send = RegDR + GetAdrD_1000(adr) + "0001";
            string response;
            if (SendPLC(send, "加药仓按脉冲自动运行状态", out response))
            {
                if (Get16Int(response.Substring(18, 4)) == 1)
                {
                    result = true;
                }
            }
            return result;
        }

        //推药板报警复位完成状态
        const int PlateReset_Left_Status = 21;//M-1
        const int PlateReset_Right_Status = 23;//M-1
        public static bool PlateErrorResetIsOK(PlateType type)
        {
            bool result = false;
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = PlateReset_Left_Status;
                    break;
                case PlateType.Right:
                    adr = PlateReset_Right_Status;
                    break;
            }
            string send = RegMR + GetAdrM(adr) + "0001";
            string response;
            if (SendPLC(send, "推药板报警复位状态", out response))
            {
                string s = response.Substring(18, 2);
                if (Get16Int(s) == 1)
                    result = true;
            }
            return result;
        }
        //推药板报警复位
        const int PlateReset_Left = 107;//M-1
        const int PlateReset_Right = 108;//M-1
        public static void PlateErrorReset(PlateType type)
        {
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = PlateReset_Left;
                    break;
                case PlateType.Right:
                    adr = PlateReset_Right;
                    break;
            }
            string send = RegMSW + GetAdrM(adr) + "0000";
            string response;
            SendPLC(send, "推药板报警复位", out response);
            send = RegMSW + GetAdrM(adr) + "FF00";
            SendPLC(send, "推药板报警复位", out response);

            Thread.Sleep(500);

            if (PlateErrorResetIsOK(type))
            {
                csMsg.ShowInfo("操作成功", false);
                //PlC_Add_OFF_All();
                //Thread.Sleep(200);
                //PlC_Add_ON_All();
            }
        }

        const int PlateErrorCode_Left = 20;//D-1
        const int PlateErrorCode_Right = 22;//D-1
        //仓位报警代码
        public static string ReadPlateErrorCode(PlateType type)
        {
            string result = "";
            //报警代码
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = PlateErrorCode_Left;
                    break;
                case PlateType.Right:
                    adr = PlateErrorCode_Right;
                    break;
            }
            string send = RegDR + GetAdrD_1000(adr) + "0001";
            string response;
            if (SendPLC(send, "加药仓报警代码", out response))
            {
                result = response.Substring(18, 4);
            }
            return result;
        }

        //读取加药计数
        const int PlateRecord_Read_Left = 64;//D-1
        const int PlateRecord_Read_Right = 68;//D-1
        public static int ReadPlateRecord(PlateType type)
        {
            int result = 0;
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = PlateRecord_Read_Left;
                    break;
                case PlateType.Right:
                    adr = PlateRecord_Read_Right;
                    break;
            }
            string send = RegDR + GetAdrD_1000(adr) + "0001";
            string response;
            if (SendPLC(send, "读取加药计数", out response))
            {
                result = Get16Int(response.Substring(18, 4));
            }
            return result;
        }
        //加药计数清零
        public static void ResetPlateRecord(PlateType type)
        {
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = PlateRecord_Read_Left;
                    break;
                case PlateType.Right:
                    adr = PlateRecord_Read_Right;
                    break;
            }
            string send = RegDSW + GetAdrD_1000(adr) + "0000";
            string response;
            SendPLC(send, "加药计数清零", out response);
        }

        //加药障碍报警
        const int Error_ZHA_Left = 98;//D-1
        const int Error_ZHA_Right = 99;//D-1
        public static bool PlateErrorZHA()
        {
            bool result = true;
            string send = RegDR + GetAdrD_1000(Error_ZHA_Left) + "0002";
            string response;
            if (SendPLC(send, "加药障碍状态", out response))
            {
                string s = response.Substring(18, 8);
                if (Get16Int(s.Substring(0, 4)) == 1 || Get16Int(s.Substring(4, 4)) == 1)
                    result = false;
            }
            return result;
        }

        //推药板运行状态
        const int Plate_State_Left = 25374;//D-1
        const int Plate_State_Right = 25630;//D-1
        //public static string ReadPlateState(PlateType type)
        //{
        //    string result = "";
        //    int adr = 0;
        //    switch (type)
        //    {
        //        case PlateType.Left:
        //            adr = Plate_State_Left;
        //            break;
        //        case PlateType.Right:
        //            adr = Plate_State_Right;
        //            break;
        //    }
        //    string send = RegDR + GetAdrD_8000(adr) + "0001";
        //    string response;
        //    if (SendPLC(send, "推药板状态", out response))
        //    {
        //        result = Get16Int(response.Substring(18, 4)).ToString();
        //    }
        //    return result;
        //}
        public static bool PlateStateIsOK(PlateType type)
        {
            bool result = true;
            int adr = 0;
            switch (type)
            {
                case PlateType.Left:
                    adr = Plate_State_Left;
                    break;
                case PlateType.Right:
                    adr = Plate_State_Right;
                    break;
            }
            string send = RegDR + GetAdrD_8000(adr) + "0001";
            string response;
            if (SendPLC(send, "推药板状态", out response))
            {
                if (Get16Int(response.Substring(18, 4)) == 2)
                    result = false;
            }
            return result;
        }
        #endregion

        //盘点激光
        #region
        public enum LaserType
        {
            Left,
            Right
        }

        const int LaserOnOff = 40;//M-1
        public static void LaserOn()
        {
            string send = RegMSW + GetAdrM(LaserOnOff) + "FF00";
            string response;
            SendPLC(send, "激光开关", out response);
        }
        public static void LaserOff()
        {
            string send = RegMSW + GetAdrM(LaserOnOff) + "0000";
            string response;
            SendPLC(send, "激光开关", out response);
        }
        #endregion

        //提升机
        #region
        //提升机机运动方向（上升、下降、停止）
        #region
        public enum LiftMoveDir
        {
            Up,
            Down,
            Stop
        }
        #endregion

        //提升机皮带转动类型（向前、向后、停止）
        #region
        public enum LiftBeltMoveDir
        {
            Forward,
            Back,
            Stop
        }
        #endregion

        //提升机移动位置（顶、上、下、接）
        #region
        public enum MovePos
        {
            Top,
            Up,
            Down,
            Meet
        }
        #endregion

        //提升机手动运动
        const int Speed_Lift_Manual = 42;//D-2
        const int LiftMove_Manual_FX = 44;//D-1
        const int LiftMove_Manual_On = 32;//M-1
        public static void LiftManualMove(LiftMoveDir dir)
        {
            int fx = 0;
            int speed = 0;
            switch (dir)
            {
                case LiftMoveDir.Up:
                    fx = 1;
                    speed = int.Parse(Config.Mac_A.Speed_Manual_Lift);
                    break;
                case LiftMoveDir.Down:
                    fx = -1;
                    speed = int.Parse(Config.Mac_A.Speed_Manual_Lift);
                    break;
                case LiftMoveDir.Stop:
                    fx = 0;
                    break;
            }
            string send = "";
            string response;
            if (dir != LiftMoveDir.Stop)//方向
            {
                send = RegDSW + GetAdrD_1000(LiftMove_Manual_FX) + Get16String(fx, 4);
                SendPLC(send, "提升机手动运行", out response);
            }
            //速度
            string s = GetReal(speed);
            send = RegDMW + GetAdrD_1000(Speed_Lift_Manual) + "0002" + "04" + s.Substring(4, 4) + s.Substring(0, 4);
            SendPLC(send, "提升机手动速度", out response);

            send = RegMSW + GetAdrM(LiftMove_Manual_On) + "FF00";
            SendPLC(send, "提升机手动运行使能开启", out response);
        }

        const int LiftPulse = 25882;//D-2
        //读取提升机脉冲值
        public static float ReadLiftPulse()
        {
            float result = 0;
            string send = RegDR + GetAdrD_8000(LiftPulse) + "0002";
            string response;
            if (SendPLC(send, "提升机脉冲", out response))
            {
                string s = response.Substring(18, 8);
                int i = Get16Int(s.Substring(4, 4) + s.Substring(0, 4));
                float f = i * 360.0f / 10000;
                result = (float)Math.Round(f, 2);
            }
            return result;
        }

        //设定提升机目标脉冲
        const int LiftAutoMovePulse = 46;//D-2
        public static void LiftAutoMoveByPulse(float pulse)
        {
            //LiftAutoMoveOFF();
            string s = GetReal(pulse);
            string send = RegDMW + GetAdrD_1000(LiftAutoMovePulse) + "0002" + "04" + s.Substring(4, 4) + s.Substring(0, 4);
            string response;
            if (SendPLC(send, "提升机自动运行脉冲", out response))
                LiftAutoMoveOn();
        }
        //提升机自动运动使能
        const int LiftAutoMovePulse_On = 56;//M-1
        public static bool LiftAutoMoveOn()
        {
            string send = RegMSW + GetAdrM(LiftAutoMovePulse_On) + "FF00";
            string response;
            return SendPLC(send, "提升机自动运行使能", out response);
        }
        public static bool LiftAutoMoveOFF()
        {
            string send = RegMSW + GetAdrM(LiftAutoMovePulse_On) + "0000";
            string response;
            return SendPLC(send, "提升机自动运行使能", out response);
        }
        //提升机自动运行是否完成
        const int LiftAutoMoveStatus = 150;//D-1 41;//M-1
        public static bool LiftAutoMoveIsOK()
        {
            bool result = false;
            string send = RegDR + GetAdrD_1000(LiftAutoMoveStatus) + "0001";
            string response;
            if (SendPLC(send, "提升机自动运行状态", out response))
            {
                if (Get16Int(response.Substring(18, 4)) == 1)
                    result = true;
            }
            return result;
        }
        //提升机运行到指定位置
        public static void LiftAutoMoveToPos(MovePos pos)
        {
            float value = 0;
            switch (pos)
            {
                case MovePos.Top:
                    value = float.Parse(Config.Mac_A.Pulse_Lift_Top);
                    break;
                case MovePos.Up:
                    value = float.Parse(Config.Mac_A.Pulse_Lift_Up);
                    break;
                case MovePos.Down:
                    value = float.Parse(Config.Mac_A.Pulse_Lift_Down);
                    break;
                case MovePos.Meet:
                    value = float.Parse(Config.Mac_A.Pulse_Lift_Meet);
                    break;
            }
            LiftAutoMoveByPulse(value);
        }

        //提升机原点复位
        const int LiftOrigin = 11;//M-1
        public static bool LiftOriginReset()
        {
            LiftOriginResetOFF();

            //Origin_Out_On();
            //PlC_OFF_All();
            //PlC_ON_All();
            //Thread.Sleep(500);

            string send = RegMSW + GetAdrM(LiftOrigin) + "FF00";
            string response;
            return SendPLC(send, "提升机原点复位", out response);
        }
        public static bool LiftOriginResetOFF()
        {
            string send = RegMSW + GetAdrM(LiftOrigin) + "0000";
            string response;
            return SendPLC(send, "提升机原点复位", out response);
        }
        const int LiftOrigin_State = 5;//M-1
        //提升机原点复位是否完成
        public static bool LiftOriginResetIsOK()
        {
            bool result = false;
            string send = RegMR + GetAdrM(LiftOrigin_State) + "0001";
            string response;
            if (SendPLC(send, "提升机原点复位状态", out response))
            {
                if (Get16Int(response.Substring(18, 2)) == 1)
                    result = true;
            }
            return result;
        }
        //提升机原点复位状态
        const int Lift_Origin = 12;//D-1
        public static bool LiftOriginResetState()
        {
            bool result = true;
            string send = RegDR + GetAdrD_1000(Lift_Origin) + "0001";
            string response;
            if (SendPLC(send, "提升机原点复位状态", out response))
            {
                if (Get16Int(response.Substring(18, 4)) > 0)
                    result = false;
            }
            return result;
        }

        //提升机报警代码地址
        const int LiftWarningCode_Origin = 12;//D-1
        const int LiftWarningCode = 24;//D-1
        public static string ReadLiftErrorCode()
        {
            string result = "";
            //报警代码
            string send = RegDR + GetAdrD_1000(LiftWarningCode) + "0001";
            string response;
            string code = "0000";
            if (SendPLC(send, "提升机报警代码", out response))
            {
                code = response.Substring(18, 4);
            }
            //原点报警
            send = RegDR + GetAdrD_1000(LiftWarningCode_Origin) + "0001";
            string code_Origin = "0000";
            if (SendPLC(send, "提升机原点返回报警代码", out response))
            {
                code_Origin = response.Substring(18, 4);
            }
            result = code.CompareTo(code_Origin) > 0 ? code : code_Origin;
            return result;
        }

        //提升机复位状态
        const int LiftReset_Status = 25;//M-1
        public static bool LiftErrorResetIsOK()
        {
            bool result = false;
            string send = RegMR + GetAdrM(LiftReset_Status) + "0001";
            string response;
            if (SendPLC(send, "提升机报警复位状态", out response))
            {
                string s = response.Substring(18, 2);
                if (Get16Int(s) == 1)
                    result = true;
            }
            return result;
        }
        //提升机复位
        const int LiftReset = 109;//M-1
        public static void LiftErrorReset()
        {
            string send = RegMSW + GetAdrM(LiftReset) + "0000";
            string response;
            SendPLC(send, "提升机报警复位", out response);
            send = RegMSW + GetAdrM(LiftReset) + "FF00";
            SendPLC(send, "提升机报警复位", out response);

            Thread.Sleep(500);

            if (LiftErrorResetIsOK())
            {
                csMsg.ShowInfo("操作成功", false);
                //PlC_Out_OFF_All();
                //Thread.Sleep(200);
                //PlC_Out_ON_All();
            }
        }

        //提升机是否在接药位置
        public static bool LiftOnMeet()
        {
            bool result = false;
            float nowPulse = ReadLiftPulse();
            float meetPulse = float.Parse(Config.Mac_A.Pulse_Lift_Meet);
            if (Math.Abs(nowPulse - meetPulse) < 100)
                result = true;
            return result;
        }

        //提升机状态
        const int Lift_State = 25886;//D-1
        //public static string ReadLiftState()
        //{
        //    string result = "";
        //    string send = RegDR + GetAdrD_8000(Lift_State) + "0001";
        //    string response;
        //    if (SendPLC(send, "提升机状态", out response))
        //    {
        //        result = Get16Int(response.Substring(18, 4)).ToString();
        //    }
        //    return result;
        //}
        public static bool LiftStateIsOK()
        {
            bool result = true;
            string send = RegDR + GetAdrD_8000(Lift_State) + "0001";
            string response;
            if (SendPLC(send, "提升机状态", out response))
            {
                string s = response.Substring(18, 4);
                if (Get16Int(s) == 2)
                    result = false;
            }
            return result;
        }
        #endregion

        //传送皮带
        #region
        //传送皮带转动类型（左、右、停）
        public enum TransferBeltMoveType
        {
            Left,
            Right,
            Stop
        }
        //传送皮带转动
        const int TransferBeltMove_On = 38;//M-1
        public static void TransferBeltMove(TransferBeltMoveType type)
        {
            string value = "";
            switch (type)
            {
                case TransferBeltMoveType.Left:
                    value = "FF00";
                    break;
                case TransferBeltMoveType.Right:
                    value = "FF00";
                    break;
                case TransferBeltMoveType.Stop:
                    value = "0000";
                    break;
            }
            string send = RegMSW + GetAdrM(TransferBeltMove_On) + value;
            string response;
            SendPLC(send, "传送皮带转动使能", out response);
        }
        #endregion

        //天顶皮带
        #region
        //天顶皮带转动类型（转、停）
        public enum TopBeltMoveType
        {
            Turn,
            Stop
        }
        //窗口皮带转动
        const int TopBeltMove_On = 39;//M-1
        public static void TopBeltMove(TopBeltMoveType type)
        {
            string value = "";
            switch (type)
            {
                case TopBeltMoveType.Turn:
                    value = "FF00";
                    break;
                case TopBeltMoveType.Stop:
                    value = "0000";
                    break;
            }
            string send = RegMSW + GetAdrM(TopBeltMove_On) + value;
            string response;
            SendPLC(send, "窗口皮带转动使能", out response);
        }
        #endregion

        //电磁铁
        #region
        //最小储位10111的电磁铁动作地址
        public const int AdrOutPos10111 = 7001;
        //单独电磁铁动作
        public static void DCTMoveDownSingle(string poscode)
        {
            string v;
            string sql = @"select outtime from drug_infoannex di,drug_pos dp 
where di.drugonlycode=dp.drugonlycode and dp.maccode='{0}' and dp.poscode='{1}'";
            sql = string.Format(sql, Config.Soft.MacCode, poscode);
            csSql.ExecuteScalar(sql, Config.Soft.ConnString, out v);
            int t = 300;
            if (!string.IsNullOrEmpty(v))
                t = int.Parse(v);

            int master = int.Parse(poscode.Substring(1, 2));
            int dct = int.Parse(poscode.Substring(3, 2));
            if (DCT_AP.DCTMoveDownSingle(master, dct, t))
            {
                //弹跳成功，记录次数
                sql = "update sys_posinfo set record=(record+1) where maccode='{0}' and poscode='{1}'"; 
                sql = string.Format(sql, Config.Soft.MacCode, poscode);
                csSql.ExecuteScalar(sql, Config.Soft.ConnString, out v);
            }
            else
            {
                //弹跳失败
            }
        }
        //多个电磁铁动作
        public static void DCTMoveDownMutil(List<string> poss)
        {
            foreach (string pos in poss)
            {
                DCTMoveDownSingle(pos);
                Thread.Sleep(100);
            }
        }

        #endregion

        //计数器
        #region
        //读取单独计数
        public static int ReadRecordSingle(string poscode)
        {
            int master = int.Parse(poscode.Substring(1, 2));
            int dct = int.Parse(poscode.Substring(3, 2));
            int num;
            if (!DCT_AP.ReadRecordSingle(master, dct, out num))
            {
            }
            return num;
        }
        //读取多个计数
        public static Dictionary<string, int> ReadRecordMutil(List<string> poss)
        {
            Dictionary<string, int> dics = new Dictionary<string, int>();
            foreach (string pos in poss)
            {
                int n = ReadRecordSingle(pos);
                if (n > 0)
                    dics.Add(pos, n);
                else
                    dics.Add(pos, 0);
                Thread.Sleep(100);
            }
            return dics;
        }

        //单独计数器清零
        public static void ClearRecordSingle(string poscode)
        {
            int master = int.Parse(poscode.Substring(1, 2));
            int dct = int.Parse(poscode.Substring(3, 2));
            if (!DCT_AP.ClearRecordSingle(master, dct))
            {
            }
        }
        //多个计数器清零
        public static void ClearRecordMutil(List<string> poss)
        {
            foreach (string pos in poss)
            {
                ClearRecordSingle(pos);
                Thread.Sleep(100);
            }
        }

        #endregion

        //关闭连接
        #region
        //public static void Close()
        //{
        //    if (tct != null)
        //        tct.Close();
        //    if (skt != null)
        //    {
        //        skt.Dispose();
        //        skt.Close();
        //    }
        //}
        #endregion

        //盘点指定储位
        public static int PDNum(string posCode, bool isNull)
        {
            int result = -1;
            float x = 0f;
            float z = 0f;
            string dir = "";
            string sql = "";

            //右臂盘点
            if (int.Parse(posCode.Substring(0, 1)) == Config.Mac_A.Count_Unit && int.Parse(posCode.Substring(3, 2)) > Config.Mac_A.MaxCol)
            {
                sql = "select pulsex,pulsez from pos_pulse where maccode='{0}' and poscode='{1}' and pulsetype='{2}' and pulselr='{3}'";
                sql = string.Format(sql, Config.Soft.MacCode, posCode, "P", "R");
                DataTable dt;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dt.Rows[0][0].ToString()) && !string.IsNullOrEmpty(dt.Rows[0][1].ToString()))
                    {
                        x = float.Parse(dt.Rows[0][0].ToString());
                        z = float.Parse(dt.Rows[0][1].ToString());
                        dir = "R";
                    }
                    else
                    {
                        GetPDPulseFromAdd(posCode, "R", ref x, ref z);
                    }
                }
                else
                {
                    GetPDPulseFromAdd(posCode, "R", ref x, ref z);
                    //csMsg.ShowInfo(posCode + "   " + x.ToString() + "   " + z.ToString(), true);
                }
            }
            //左臂盘点
            else if (posCode.Substring(0, 1) == "1" && int.Parse(posCode.Substring(3, 2)) < Config.Mac_A.MinCol)
            {
                sql = "select pulsex,pulsez from pos_pulse where maccode='{0}' and poscode='{1}' and pulsetype='{2}' and pulselr='{3}'";
                sql = string.Format(sql, Config.Soft.MacCode, posCode, "P", "L");
                DataTable dt;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dt.Rows[0][0].ToString()) && !string.IsNullOrEmpty(dt.Rows[0][1].ToString()))
                    {
                        x = float.Parse(dt.Rows[0][0].ToString());
                        z = float.Parse(dt.Rows[0][1].ToString());
                        dir = "L";
                    }
                    else
                    {
                        GetPDPulseFromAdd(posCode, "L", ref x, ref z);
                    }
                }
                else
                {
                    GetPDPulseFromAdd(posCode, "L", ref x, ref z);
                }
            }
            //判断左右臂哪个近
            else
            {
                sql = "select pulsex,pulsez,pulselr from pos_pulse where maccode='{0}' and poscode='{1}' and pulsetype='{2}' order by pulselr";
                sql = string.Format(sql, Config.Soft.MacCode, posCode, "P");
                DataTable dtPulse;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtPulse);

                float targetLX = 0f;
                float targetLZ = 0f;
                float targetRX = 0f;
                float targetRZ = 0f;
                if (dtPulse != null && dtPulse.Rows.Count > 0)
                {
                    foreach (DataRow row in dtPulse.Rows)
                    {
                        string zy = row[2].ToString().Trim();
                        if (zy == "L")
                        {
                            if (!string.IsNullOrEmpty(row[0].ToString()) && !string.IsNullOrEmpty(row[1].ToString()))
                            {
                                targetLX = float.Parse(row[0].ToString());
                                targetLZ = float.Parse(row[1].ToString());
                            }
                        }
                        else if (zy == "R")
                        {
                            if (!string.IsNullOrEmpty(row[0].ToString()) && !string.IsNullOrEmpty(row[1].ToString()))
                            {
                                targetRX = float.Parse(row[0].ToString());
                                targetRZ = float.Parse(row[1].ToString());
                            }
                        }
                    }
                    if (targetLX == 0f || targetLZ == 0f)
                    {
                        GetPDPulseFromAdd(posCode, "L", ref targetLX, ref targetLZ);
                    }
                    if (targetRX == 0f || targetRZ == 0f)
                    {
                        GetPDPulseFromAdd(posCode, "R", ref targetRX, ref targetRZ);
                    }
                }
                else
                {
                    GetPDPulseFromAdd(posCode, "L", ref targetLX, ref targetLZ);
                    GetPDPulseFromAdd(posCode, "R", ref targetRX, ref targetRZ);
                }
                //判断左右脉冲是否为空
                if ((targetLX == 0f || targetLZ == 0f) && (targetRX != 0f && targetRZ != 0f))
                {
                    x = targetRX;
                    z = targetRZ;
                    dir = "R";
                }
                else if ((targetLX != 0f && targetLZ != 0f) && (targetRX == 0f || targetRZ == 0f))
                {
                    x = targetLX;
                    z = targetLZ;
                    dir = "L";
                }
                else if (targetLX != 0f && targetLZ != 0f && targetRX != 0f && targetRZ != 0f)
                {
                    float nowx = PLC_Tcp_AP.ReadExtramanPulseX();
                    float nowz = PLC_Tcp_AP.ReadExtramanPulseZ();
                    if ((Math.Pow(Math.Abs(targetLX - nowx), 2) + Math.Pow(Math.Abs(targetLZ - nowz), 2)) >
                        (Math.Pow(Math.Abs(targetRX - nowx), 2) + Math.Pow(Math.Abs(targetRZ - nowz), 2)))
                    {
                        x = targetRX;
                        z = targetRZ;
                        dir = "R";
                    }
                    else
                    {
                        x = targetLX;
                        z = targetLZ;
                        dir = "L";
                    }
                }
            }
            if (x != 0f && z != 0f)
            {
                //运行到盘点位置
                PLC_Tcp_AP.ChangeAdd(1);
                if (PLC_Tcp_AP.ExtramanAutoMoveToPulse(x, z))
                {
                    DateTime timeBegin = DateTime.Now;
                    Thread.Sleep(200);
                    while (!PLC_Tcp_AP.ExtramanAutoMoveIsOK())
                    {
                        if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Extraman))
                            return -1;
                        Thread.Sleep(100);
                    }
                    //运行到位
                    if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Extraman))
                    {
                        //读取激光测距
                        int laserData = 0;
                        if (dir == "L")
                            laserData = Laser.ReadLaserData(LaserType.Left);
                        else laserData = Laser.ReadLaserData(LaserType.Right);
                        if (laserData >= 0)
                        {
                            if (!isNull)
                            {
                                sql = "select isnull(length,0) from drug_infoannex a,drug_pos dp where a.drugonlycode=dp.drugonlycode and dp.maccode='{0}' and dp.poscode='{1}'";
                                sql = string.Format(sql, Config.Soft.MacCode, posCode);
                                string s;
                                csSql.ExecuteScalar(sql, Config.Soft.ConnString, out s);
                                if (!string.IsNullOrEmpty(s))
                                {
                                    float length;
                                    if (float.TryParse(s, out length))
                                    {
                                        if (length > 0)
                                        {
                                            int n = (int)Math.Round((Config.Mac_A.Length_Laser - laserData) / length);
                                            if (n >= 0)
                                                result = n;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                if (Math.Abs(Config.Mac_A.Length_Laser - laserData) > 50)
                                    result = 1;
                                else result = 0;
                            }
                        }
                    }
                }
            }
            return result;
        }

        static void GetPDPulseFromAdd(string posCode, string dir, ref float x, ref float z)
        {
            //通过加药脉冲计算盘点脉冲
            string sql = "select pulsex,pulsez from pos_pulse where maccode='{0}' and poscode='{1}' and pulsetype='A' and pulselr='{2}'";
            sql = string.Format(sql, Config.Soft.MacCode, posCode, dir);
            DataTable dtAdd;
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtAdd);
            if (dtAdd != null && dtAdd.Rows.Count > 0)
            {
                string s = "select posmaxwidth from sys_posinfo where maccode='{0}' and poscode='{1}'";
                s = string.Format(s, Config.Soft.MacCode, posCode);
                string v; 
                csSql.ExecuteScalar(s, Config.Soft.ConnString, out v);
                if (!string.IsNullOrEmpty(v))
                {
                    if (dir == "L")
                        x = float.Parse(dtAdd.Rows[0][0].ToString()) - (float.Parse(v) / 2 + Config.Mac_A.Laser_Offset_Left_X) * Config.Mac_A.OneMMPulse_Extraman_X;
                    else
                        x = float.Parse(dtAdd.Rows[0][0].ToString()) + (float.Parse(v) / 2 + Config.Mac_A.Laser_Offset_Right_X) * Config.Mac_A.OneMMPulse_Extraman_X;

                    z = float.Parse(dtAdd.Rows[0][1].ToString()) - (Config.Mac_A.Laser_Offset_Right_Z - 8) * Config.Mac_A.OneMMPulse_Extraman_Z;
                }
            }
        }

        //光幕/急停
        #region
        public enum MacType
        {
            Add,
            Out
        }
        const int StopState_Add = 26;//M-1
        const int StopState_Out = 27;//M-1
        public static bool MacIsStop(MacType type)
        {
            bool result = false;
            int adr = 0;
            switch (type)
            {
                case MacType.Add:
                    adr = StopState_Add;
                    break;
                case MacType.Out:
                    adr = StopState_Out;
                    break;
            }
            string send = RegMR + GetAdrM(adr) + "0001";
            string response;
            if (SendPLC(send, "设备急停状态", out response))
            {
                if (Get16Int(response.Substring(18, 2)) == 1)
                    result = true;
            }
            return result;
        }

        const int GMState = 156;//D-1
        public static bool GMIsStop()
        {
            bool result = false;
            string send = RegDR + GetAdrD_1000(GMState) + "0001";
            string response;
            if (SendPLC(send, "光幕状态", out response))
            {
                if (Get16Int(response.Substring(18, 4)) == 1)
                    result = true;
            }
            return result;
        }
        #endregion

        //传送带状态
        const int TransferBelt_State = 6005;//D-1
        public static string ReadTransferBeltState()
        {
            string result = "";
            string send = RegDR + GetAdrD_8000(TransferBelt_State) + "0001";
            string response;
            if (SendPLC(send, "传送带状态", out response))
            {
                result = Get16Int(response.Substring(18, 4)).ToString();
            }
            return result;
        }

    }
}