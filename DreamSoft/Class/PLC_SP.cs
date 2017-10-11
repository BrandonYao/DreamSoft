using System;
using System.IO.Ports;
using System.Threading;
using System.Data;

namespace DreamSoft
{
    public class PLC_SP
    {
        static CSHelper.LOG csLOG = new CSHelper.LOG();
        static CSHelper.Msg csMsg = new CSHelper.Msg();
        static CSHelper.SQL csSql = new CSHelper.SQL();

        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;

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

        public static SerialPort spPLC;
        const string PlcNo = "01";

        //PLC是否忙，用于光幕监控
        public static bool IsBusy = false;
        
        //初始化
        #region
        //初始化PLC串口
        public static void Initial()
        {
            if (spPLC != null)
            {
                spPLC.Close();
                spPLC.Dispose();
            }
            spPLC = new SerialPort(Config.Mac_S.Port_PLC, 9600, Parity.Even, 7, StopBits.One);

            if (!spPLC.IsOpen)
            {
                try
                {
                    spPLC.Open();
                }
                catch (Exception ex)
                {
                    csLOG.WriteLog(ex.Message);
                    ThrowMsg(ex.Message);
                }
            }
            Thread.Sleep(100);

                SetAutoSpeed(int.Parse(Config.Mac_S.Speed_Auto));

                SetManualSpeed(int.Parse(Config.Mac_S.Speed_Manual));
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
                if (c != "8" && c !="9")
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
                        csLOG.WriteLog(mark + "：" + error);
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
            //添加校验和结束符
            send = PlcNo + send;
            send = StartString + send + GetLRCCode(send) + EndString;
            //commLog.WriteLog("PC-->PLC:/r/n" + send);

                while (IsBusy) 
                {
                    Thread.Sleep(100);
                };

            IsBusy = true;
            try
            {
                if (spPLC == null)
                {
                    spPLC = new SerialPort(Config.Mac_S.Port_PLC, 9600, Parity.Even, 7, StopBits.One);
                }
                if (!spPLC.IsOpen)
                {
                    spPLC.Open();
                }
                //写入串口
                spPLC.Write(send);
                DateTime begin = DateTime.Now;
                do
                {
                    Thread.Sleep(50);
                    response = spPLC.ReadTo("\r\n");
                } while (response == "" && begin.AddSeconds(3) > DateTime.Now);

                //commLog.WriteLog("PLC-->PC:/r/n" + response);
                IsBusy = false;

                //检验返回数据
                if (CheckResponse(response, mark))
                    result = true;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                //记录错误信息

                csLOG.WriteLog(mark + "：PLC通讯出错---" + ex.Message);
                ThrowMsg(mark + "：PLC通讯出错---" + ex.Message);
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
        public static string GetAdrD_1000(int i)
        {
            return Get16String(i + 4096, 4);
        }
        public static string GetAdrM(int i)
        {
            return Get16String(i + 2048, 4);
        }
        #endregion

        //转动运行
        #region
        //手动转动类型（向上、向下、停止）
        public enum TurnType
        {
            Up,
            Down,
            Stop
        }
        //手动转动地址
        const int Turn_Manual_Up = 492;//D-1
        const int Turn_Manual_Down = 493;//D-1

        //手动转动
        public static void Turn(TurnType fx)
        {
            int adr = 0;

            switch (fx)
            {
                case TurnType.Up:
                    adr = Turn_Manual_Up;
                    break;
                case TurnType.Down:
                    adr = Turn_Manual_Down;
                    break;
            }
            string send = "";
            string response = "";
            if (fx == TurnType.Stop)
            {
                send = RegDMW + GetAdrD_1000(Turn_Manual_Up) + "000204" + "00000000";
            }
            else
            {
                send = RegDSW + GetAdrD_1000(adr) + "0001";
            }
            SendPLC(send, "手动转动", out response);
        }

        //自动运行速度（正）
        const int Speed_Auto = 604;//D-2
        const int Speed_Auto_3 = 412;//D-2
        //设置自动运行速度
        public static void SetAutoSpeed(int speed)
        {
            string v = Get16String(speed, 8);
            string response = "";
            string send = "";
            if (Config.Mac_S.Mode == "3")
                send = RegDMW + GetAdrD_1000(Speed_Auto_3) + "000204" + v.Substring(4, 4) + v.Substring(0, 4);
            else
                send = RegDMW + GetAdrD_1000(Speed_Auto) + "000204" + v.Substring(4, 4) + v.Substring(0, 4);
            SendPLC(send, "自动速度", out response);
        }

        //手动运行速度（值为负）
        const int Speed_Manual = 410;//D-2
        const int Speed_Manual_3 = 410;//D-2
        //设置手动运行速度
        public static void SetManualSpeed(int speed)
        {
            string v = Get16String(speed, 8);
            string response = "";
            string send = "";
            //if (Config.Mac_S.Mode == "3")
            //    send = RegDMW + GetAdrD_1000(Speed_Manual) + "000204" + v.Substring(4, 4) + v.Substring(0, 4);
            //else
                send = RegDMW + GetAdrD_1000(Speed_Manual) + "000204" + v.Substring(4, 4) + v.Substring(0, 4);
            SendPLC(send, "手动速度", out response);
        }

        const int TurnLay = 24;//D-1
        const int TurnLay_3 = 40;//M-1
        //转动到指定层
        public static void TurnTo(int lay)
        {
            if (Config.Mac_S.Mode == "1")
            {
                if (lay % 2 == 0)
                    lay--;
                //读取层对应脉冲
                string sql = "select pulse from lay_pulse where maccode='{0}' and layid={1}";
                sql = string.Format(sql, Config.Soft.MacCode, lay);
                string pulse;
                csSql.ExecuteScalar(sql, Config.Soft.ConnString, out pulse);

                TurnTo(pulse);
            }
            else if (Config.Mac_S.Mode == "2")
            {
                string response;
                string send = RegDSW + GetAdrD_1000(TurnLay) + Get16String(lay, 4);
                if (SendPLC(send, "自动定位", out response))
                {
                    send = RegDSW + GetAdrD_1000(Turn_Auto) + "0001";
                    SendPLC(send, "开始转动", out response);
                }
            }
            else if (Config.Mac_S.Mode == "3")
            {
                string response;
                int adr = (lay - 1) / 2 + TurnLay_3;
                string send = RegMSW + GetAdrM(adr) + "FF00";
                SendPLC(send, "自动定位", out response);
            }
        }

        //自动转动开始
        const int Turn_Auto = 332;//D-1
        //自动转动脉冲地址
        const int Pulse_Turn_Auto = 334;//D-2
        //转动到指定脉冲
        public static void TurnTo(string pulse)
        {
            int bzc = Config.Mac_S.Perimeter/2;
            int zc = Config.Mac_S.Perimeter;

            //读取当前脉冲
            int pulseNow = ReadNowPulse();
            int cz = int.Parse(pulse) - pulseNow;

            cz = cz % zc;

            if (cz >= 0)
            {
                if (cz >= bzc)
                {
                    cz -= zc;
                }
            }
            else
            {
                if (cz < 0 - bzc)
                {
                    cz += zc;
                }
            }

            //计算脉冲高低位
            string p16 = Get16String(cz, 8);
            string response = "";
            string send = RegDMW + GetAdrD_1000(Pulse_Turn_Auto) + "000204" + p16.Substring(4, 4) + p16.Substring(0, 4);
            if (SendPLC(send, "设定脉冲", out response))
            {
                send = RegDSW + GetAdrD_1000(Turn_Auto) + "0001";
                SendPLC(send, "开始转动", out response);
            }
        }

        /// <summary>
        /// 停止转动
        /// </summary>
        /// <param name="zy"></param>
        public static void StopTurn()
        {
            string response = "";
            string send = RegDSW + GetAdrD_1000(Turn_Auto) + "0002";
            SendPLC(send, "停止转动", out response);
        }
        #endregion

        //脉冲操作
        #region
        const int NowLay = 450;//D-1
        const int NowLay_3 = 426;//D-1
        //读取当前层
        public static int ReadNowLay()
        {
            int result = 0;
            if (Config.Mac_S.Mode == "1")
            {
                int zc = Config.Mac_S.Perimeter;
                //读取当前脉冲
                int pulse = ReadNowPulse();
                string s = "SELECT top 1 layid,ABS(pulse-{0}) % {1} from lay_pulse where maccode='{2}' order by ABS(pulse-{0}) % {1}";
                string sql = string.Format(s, pulse, zc, Config.Soft.MacCode);
                DataTable dtLay;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtLay);
                if (dtLay != null && dtLay.Rows.Count > 0)
                    result = int.Parse(dtLay.Rows[0]["layid"].ToString());
            }
            else if (Config.Mac_S.Mode == "2")
            {
                string response = "";
                string send = RegDR + GetAdrD_1000(NowLay) + "0001";
                if (SendPLC(send, "读取当前层位置", out response))
                {
                    int i = response.IndexOf(":");
                    if (i >= 0)
                    {
                        result = Get16Int(response.Substring(i + 7, 4));
                    }
                }
            }
            else if (Config.Mac_S.Mode == "3")
            {
                string response = "";
                string send = RegDR + GetAdrD_1000(NowLay_3) + "0001";
                if (SendPLC(send, "读取当前层位置", out response))
                {
                    int i = response.IndexOf(":");
                    if (i >= 0)
                    {
                        result = Get16Int(response.Substring(i + 7, 4));
                    }
                }
            }
            return result;
        }

        const int LayPulse_1 = 2000;//D-2
        const int LayPulse_3_1 = 50;//M-1
        //手动设定脉冲
        public static void SavePulse(int lay, int pulse)
        {
            if (Config.Mac_S.Mode == "1")
            {
                //保存对应脉冲
                string s = "update lay_pulse set pulse={0} where maccode='{1}' and layid={2}";
                string sql = string.Format(s, pulse, Config.Soft.MacCode, lay);
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
            }
            else if (Config.Mac_S.Mode == "2")
            {
                int adr = ((lay + 1) / 2 - 1) * 4 + LayPulse_1;
                string p16 = Get16String(pulse, 8);
                string response = "";
                string send = RegDMW + GetAdrD_1000(adr) + "000204" + p16.Substring(4, 4) + p16.Substring(0, 4);
                if (SendPLC(send, "设定层脉冲", out response))
                    csMsg.ShowInfo("操作成功", false);
            }
            else if (Config.Mac_S.Mode == "3")
            {
                string response;
                int adr = (lay - 1) / 2 + LayPulse_3_1;
                string send = RegMSW + GetAdrM(adr) + "FF00";
                SendPLC(send, "设定层脉冲", out response);
            }
        }

        //原点复位地址
        const int Zero = 482;
        //原点复位
        public static bool BackZero()
        {
            bool result = false;
            string response = "";
            string send = RegDSW + GetAdrD_1000(Zero) + "0001";
            result = SendPLC(send, "原点复位", out response);
            return result;
        }
        //判断原点复位是否完成
        public static bool BackZeroIsFinished()
        {
            bool result = false;
            string response = "";
            string send =RegDR + GetAdrD_1000(Zero) + "0001";
            if (SendPLC(send, "判断原点复位是否完成", out response))
            {
                int i = response.IndexOf(":");
                if (i >= 0)
                {
                    if (int.Parse(response.Substring(i + 7, 4)) == 2)
                        result = true;
                }
            }
            return result;
        }

        //测周长地址
        const int Perimeter = 330;//D-1
        const int Perimeter_3 = 13;//M-1
        //运转一圈测周长
        public static void TestZC()
        {
            if (Config.Mac_S.Mode == "3")
            {
                string response = "";
                string send = RegMSW + GetAdrM(Perimeter_3) + "FF00";
                SendPLC(send, "测周长", out response);
            }
            else
            {
                string response = "";
                string send = RegDSW + GetAdrD_1000(Perimeter) + "0001";
                SendPLC(send, "测周长", out response);
            }
        }
        //判断测周长是否完成
        public static bool TestZCIsFinished()
        {
            bool result = false;
            string response = "";
            string send = RegDR + GetAdrD_1000(Perimeter) + "0001";
            if (SendPLC(send, "判断测周长是否完成", out response))
            {
                int i = response.IndexOf(":");
                if (i >= 0)
                {
                    if (int.Parse(response.Substring(i + 7, 4)) == 2)
                        result = true;
                }
            }
            return result;
        }

        //脉冲地址
        const int Pulse_Now = 1030;//D-2
        //读当前脉冲
        public static int ReadNowPulse()
        {
            int result = 0;
            string response = "";
            string send = RegDR + GetAdrD_1000(Pulse_Now) + "0002";
            if (SendPLC(send, "读脉冲", out response))
            {
                int i = response.IndexOf(":");
                if (i >= 0)
                {
                    result = Convert.ToInt32(response.Substring(i + 11, 4) + response.Substring(i + 7, 4), 16);
                }
            }
            return result;
        }
        #endregion

        //激光操作
        #region
        //第一个激光地址
        const int Laser_Start = 801;
        const int Laser_Start_3 = 202;//M-1
        //开关类型
        public enum LightType
        {
            Open,
            Close
        }
        //控制单个激光
        public static void LightSingleNum(int lay, int col, LightType type)
        {
            if (Config.Mac_S.Mode == "3")
            {
                int adr = Laser_Start_3 + lay - 1 + (col - 1) * 3; 
                string value = "";
                switch (type)
                {
                    case LightType.Open:
                        value = "FF00";
                        break;
                    case LightType.Close:
                        value = "0000";
                        break;
                } 
                string send = RegMSW + GetAdrM(adr) + value;
                string response = "";
                SendPLC(send, "激光", out response);
            }
            else
            {
                int adr = Laser_Start + lay - 1 + (col - 1) * 4;

                string value = "";
                switch (type)
                {
                    case LightType.Open:
                        value = "0001";
                        break;
                    case LightType.Close:
                        value = "0000";
                        break;
                }

                string send = RegDSW + GetAdrD_1000(adr) + value;
                string response = "";
                SendPLC(send, "激光", out response);
            }
        }
        //点亮多个激光
        public static void LightMutilNum(params string[] poss)
        {
            LightAllNum(LightType.Close);
            foreach (string pos in poss)
            {
                LightSingleNum(int.Parse(pos.Substring(0, 2)), int.Parse(pos.Substring(2, 2)), LightType.Open);
            }
        }

        const int Laser_All_Off = 200;//M-1
        const int Laser_All_On = 201;//M-1
        //控制全部激光
        public static void LightAllNum(LightType type)
        {
            if (Config.Mac_S.Mode == "3")
            {
                int adr = Laser_All_Off;
                if (type == LightType.Open)
                    adr = Laser_All_On;
                string send = RegMSW + GetAdrM(adr) + "FF00";
                string response = "";
                SendPLC(send, "激光", out response);
            }
            else
            {
                string value = "";
                string v = "0000";
                if (type == LightType.Open)
                    v = "0001";
                for (int c = 1; c <= Config.Mac_S.Count_Col; c++)
                {
                    for (int r = 1; r <= 4; r++)
                    {
                        value += v;
                    }
                }
                int num = 4 * Config.Mac_S.Count_Col;

                string response = "";
                string send = RegDMW + GetAdrD_1000(Laser_Start) + Get16String(num, 4) + Get16String(num * 2, 2) + value;
                SendPLC(send, "激光", out response);
            }
        }
        #endregion

        //光幕
        #region
        //光幕监视地址
        const int GM_Status = 900;
        const int GM_Status_3 = 24;//M-1
        //清除光幕保护地址
        const int GM_Clear = 901;
       
        //急停状态监视地址
        const int Stop_Status = 902;
        const int Stop_Status_3 = 20;//M-1
        //清除急停地址
        const int Stop_Clear = 903;

        //判断光幕是否保护
        public static bool GMIsProtected()
        {
            bool result = false;
            if (Config.Mac_S.Mode == "3")
            {
                string send = RegMR + GetAdrM(GM_Status_3) + "0001";
                string response;
                bool b = SendPLC(send, "光幕监视", out response);
                csLOG.WriteLog(response);
                if (b)
                {
                    int i = response.IndexOf(":");
                    if (i >= 0)
                    {
                        string s = response.Substring(i + 8, 1);
                        string ejz = Convert.ToString(Get16Int(s), 2);
                        if (ejz.Substring(ejz.Length - 1, 1) == "0")
                            result = true;
                    }
                }
            }
            else
            {
                string response = "";
                string send = RegDR + GetAdrD_1000(GM_Status) + "0001";
                if (SendPLC(send, "光幕监视", out response))
                {
                    int i = response.IndexOf(":");
                    if (i >= 0)
                    {
                        if (int.Parse(response.Substring(i + 7, 4)) == 1)
                            result = true;
                    }
                }
            }
            return result;
        }
        //判断是否急停
        public static bool MacIsStopped()
        {
            bool result = false;
            if (Config.Mac_S.Mode == "3")
            {
                string send = RegMR + GetAdrM(Stop_Status_3) + "0001";
                string response;
                if (SendPLC(send, "急停监视", out response))
                {
                    int i = response.IndexOf(":");
                    if (i >= 0)
                    {
                        string s = response.Substring(i + 8, 1);
                        string ejz = Convert.ToString(Get16Int(s), 2);
                        if (ejz.Substring(ejz.Length - 1, 1) == "1")
                            result = true;
                    }
                }
            }
            else
            {
                string response = "";
                string send = RegDR + GetAdrD_1000(Stop_Status) + "0001";
                if (SendPLC(send, "急停监视", out response))
                {
                    int i = response.IndexOf(":");
                    if (i >= 0)
                    {
                        if (Convert.ToInt32(response.Substring(i + 7, 4), 16) == 1)
                            result = true;
                    }
                }
            }
            return result;
        }
        //清除光幕保护
        public static void ClearGMProtect()
        {
            string response = "";
            string send = RegDSW + GetAdrD_1000(GM_Clear) + "0001";
            SendPLC(send, "清除光幕保护", out response);
        }
        //清除急停报警
        public static void ClearStop()
        {
            string response = "";
            string send = RegDSW + GetAdrD_1000(Stop_Clear) + "0001";
            SendPLC(send, "清除急停保护", out response);
        }
        #endregion

        //报警
        #region
        //左伺服报警地址
        const int AdrWarningLeft = 907;
        //读取左右伺服报警
        public static bool[] ReadWarning()
        {
            bool[] ss = new bool[2];
            string response = "";
            string send = RegDR + GetAdrD_1000(AdrWarningLeft) + "0002";
            if (SendPLC(send, "读取报警", out response))
            {
                int i = response.IndexOf(":");
                if (i >= 0)
                {
                    if (int.Parse(response.Substring(i + 7, 4)) == 1)
                        ss[0] = true;
                    if (int.Parse(response.Substring(i + 11, 4)) == 1)
                        ss[1] = true;
                }
            }
            return ss;
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
