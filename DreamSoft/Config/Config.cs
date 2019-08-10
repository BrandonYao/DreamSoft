using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Data;
using System.Configuration;

namespace DreamSoft
{
    class Config
    {
        static CSHelper.SQL csSql = new CSHelper.SQL();
        static CSHelper.INI csIni = new CSHelper.INI();

        //软件配置
        static Dictionary<string, string> DicsSoft = new Dictionary<string, string>();
        //服务器配置
        static Dictionary<string, string> DicsSys = new Dictionary<string, string>();
        //LED配置
        static Dictionary<string, string> DicsLed = new Dictionary<string, string>();


        //设备配置
        static Dictionary<string, string> DicsMac_A = new Dictionary<string, string>();
        //设备配置
        static Dictionary<string, string> DicsMac_S = new Dictionary<string, string>();
        //设备配置
        static Dictionary<string, string> DicsMac_C = new Dictionary<string, string>();

        public class Soft
        {
            public static string ConnString;
            public static string ConnString_His;

            public static string Config;

            public static string Server;
            public static string Local;
            public static string UserID;
            public static string Password;
            public static string Database;

            public static string SoftType;
            public static string WindowNo;
            public static string MacCode;
            public static string Function;

            public static string UserCode = "";
            public static string UserName = "";
        }

        public class Mac_A
        {
            public static string IP_PLC;
            public static string Port_PLC;

            public static string IP_DCT;
            public static int Port_DCT;

            public static string Port_DPJ;
            public static string Port_Scanner;
            public static string Port_Laser_Left;
            public static string Port_Laser_Right;

            public static int Length_Pos;
            public static float Length_Laser;
            public static int Count_Unit;
            public static int Count_Lay;
            public static int Count_Col;

            public static string Speed_Manual_X;
            public static string Speed_Manual_Z;
            public static string Speed_Manual_Plate;

            public static string Speed_Manual_Baffle_Belt;
            public static string Speed_Manual_Lift;
            public static string Speed_Manual_Baffle_Lift;
            public static string Speed_Up_Plate;

            public static string Speed_Auto_X;
            public static string Speed_Auto_Z;
            public static string Speed_Auto_Plate;

            public static string Speed_Auto_Baffle_Belt;
            public static string Speed_Auto_Lift;
            public static string Speed_Auto_Baffle_Lift;

            public static string Acc_Manual_X;
            public static string Acc_Manual_Z;
            public static string Acc_Manual_Plate;

            public static string Acc_Manual_Lift;
            public static string Acc_Manual_Baffle;
            public static string Acc_Up_Plate;

            public static string Acc_Auto_X;
            public static string Acc_Auto_Z;
            public static string Acc_Auto_Plate;
            public static string Acc_Auto_Lift;
            public static string Acc_Auto_Baffle;

            public static string Pulse_Meet_X;
            public static string Pulse_Meet_Z;

            public static string Pulse_Plate_Max_Left;
            public static string Pulse_Plate_Max_Right;
            public static string Pulse_Plate_Min_Left;
            public static string Pulse_Plate_Min_Right;

            public static string Pulse_Lift_Top;
            public static string Pulse_Lift_Up;
            public static string Pulse_Lift_Down;
            public static string Pulse_Lift_Meet;

            public static string Pulse_Baffle_Open;
            public static string Pulse_Baffle_Close;

            public static float PlateHeight;
            public static int MinCol;
            public static int MaxCol;

            public static string PLC_Tcp;
            public static string PLC_Com;
            public static string Scanner;
            public static string Laser_Left;
            public static string Laser_Right;
            public static string DPJ;

            public static int ScanSpan;
            public static int WaitTime_Start;
            public static int WaitTime_Stop;

            public static int DelayTime_Record;
            public static int StopTime_Record;

            public static float Height_BC;
            public static int Count_BC;

            public static int WaitTime_Reset_Extraman;
            public static int WaitTime_Reset_Plate;
            public static int WaitTime_Reset_Lift;
            public static int WaitTime_Reset_Baffle;

            public static int WaitTime_Auto_Extraman;
            public static int WaitTime_Auto_Plate;
            public static int WaitTime_Auto_Lift;
            public static int WaitTime_Auto_Baffle;
            public static int WaitTime_Up_Plate;

            public static int PrescWait;
            public static string SplitModel;
            public static string PDBeforeAdd;
            public static int PCNum;
            public static int MaxNum;

            public static float OneMMPulse_Plate;
            public static float OneMMPulse_Extraman_X;
            public static float OneMMPulse_Extraman_Z;
            public static int OpenTime_Baffle;
            public static float Laser_Offset_Left_X;
            public static float Laser_Offset_Left_Z;
            public static float Laser_Offset_Right_X;
            public static float Laser_Offset_Right_Z;

            public static int DrugCount;
            public static int DrugNum;

            public static string ShowTest;
        }

        public class Mac_S
        {
            public static string Port_PLC;

            public static int Count_Unit;
            public static int Count_Lay;
            public static int Count_Col;
            public static int Perimeter;

            public static string Speed_Manual;
            public static string Speed_Auto;

            public static string PLCIsEnable;
            public static int RefreshSpan;
            public static int OverTime;

            public static int ShowRowCount;
            public static string AutoTurn;
            public static int TurnSpan;

            public static string Mode = "3";
            public static string ShowTest = "N";
        }

        public class Mac_C
        {
            public static string IP_PLC;
            public static string Port_PLC;

            public static string Port_DPJ;
            public static string Port_Scanner;
            public static string Port_Laser_Left;
            public static string Port_Laser_Right;

            public static int Length_Pos;
            public static float Length_Laser;
            public static int Count_Unit;
            public static int Count_Lay;
            public static int Count_Col;

            public static string Speed_Manual_X;
            public static string Speed_Manual_Z;
            public static string Speed_Manual_Plate;

            public static string Speed_Manual_Baffle_Belt;
            public static string Speed_Manual_Lift;
            public static string Speed_Manual_Baffle_Lift;
            public static string Speed_Up_Plate;

            public static string Speed_Auto_X;
            public static string Speed_Auto_Z;
            public static string Speed_Auto_Plate;

            public static string Speed_Auto_Baffle_Belt;
            public static string Speed_Auto_Lift;
            public static string Speed_Auto_Baffle_Lift;

            public static string Acc_Manual_X;
            public static string Acc_Manual_Z;
            public static string Acc_Manual_Plate;

            public static string Acc_Manual_Lift;
            public static string Acc_Manual_Baffle;
            public static string Acc_Up_Plate;

            public static string Acc_Auto_X;
            public static string Acc_Auto_Z;
            public static string Acc_Auto_Plate;
            public static string Acc_Auto_Lift;
            public static string Acc_Auto_Baffle;

            public static string Pulse_Meet_X;
            public static string Pulse_Meet_Z;

            public static string Pulse_Plate_Max_Left;
            public static string Pulse_Plate_Max_Right;
            public static string Pulse_Plate_Min_Left;
            public static string Pulse_Plate_Min_Right;

            public static string Pulse_Lift_Top;
            public static string Pulse_Lift_Up;
            public static string Pulse_Lift_Down;
            public static string Pulse_Lift_Meet;

            public static string Pulse_Baffle_Open;
            public static string Pulse_Baffle_Close;

            public static float PlateHeight;
            public static int MinCol;
            public static int MaxCol;

            public static string PLC_Tcp;
            public static string PLC_Com;
            public static string Scanner;
            public static string Laser_Left;
            public static string Laser_Right;
            public static string DPJ;

            public static int ScanSpan;
            public static int WaitTime_Start;
            public static int WaitTime_Stop;

            public static int DelayTime_Record;
            public static int StopTime_Record;

            public static float Height_BC;
            public static int Count_BC;

            public static int WaitTime_Reset_Extraman;
            public static int WaitTime_Reset_Plate;
            public static int WaitTime_Reset_Lift;
            public static int WaitTime_Reset_Baffle;

            public static int WaitTime_Auto_Extraman;
            public static int WaitTime_Auto_Plate;
            public static int WaitTime_Auto_Lift;
            public static int WaitTime_Auto_Baffle;
            public static int WaitTime_Up_Plate;

            public static int PrescWait;
            public static string SplitModel;
            public static string PDBeforeAdd;
            public static int PCNum;
            public static int MaxNum;

            public static float OneMMPulse_Plate;
            public static float OneMMPulse_Extraman_X;
            public static float OneMMPulse_Extraman_Z;
            public static int OpenTime_Baffle;
            public static float Laser_Offset_Left_X;
            public static float Laser_Offset_Left_Z;
            public static float Laser_Offset_Right_X;
            public static float Laser_Offset_Right_Z;

            public static int DrugCount;
            public static int DrugNum;

            public static string ShowTest;
        }

        public class Sys
        {
            public static int WidthSpan;
            public static int HeightSpan; 
            public static int EnableTime;
            public static string DrugTime;
        }

        public class Led
        {
            public static int Margin_Left;
            public static int Margin_Top;
            public static int Width;
            public static int Height;
            public static int Top;
            public static int Left;
            public static int FontSize;
            public static int RefreshTime;
            public static string OrderType;
        }

        public static string file = Environment.CurrentDirectory + @"\Config\config.ini";
        private static CSHelper.LOG fLog = new CSHelper.LOG();
        //初始化配置
        public static void InitialConfig_Client()
        {
            Soft.Config = csIni.ReadIni("SYS", "Config", "", file);

            Soft.Server = csIni.ReadIni("SYS", "Server", "", file);
            Soft.Local = csIni.ReadIni("SYS", "Local", "", file);
            Soft.UserID = csIni.ReadIni("SYS", "UserID", "", file);
            Soft.Password = csIni.ReadIni("SYS", "Password", "", file);
            Soft.Database = csIni.ReadIni("SYS", "Database", "", file);
            if (Soft.Local != "1")
                Soft.ConnString = "Server=" + Soft.Server + ";User ID=" + Soft.UserID + ";Password=" + Soft.Password + ";Database=" + Soft.Database + ";connect timeout=1";
            else Soft.ConnString = "Server=" + Soft.Server + ";Database=" + Soft.Database + ";Trusted_Connection=Yes";
            Soft.ConnString_His = csIni.ReadIni("HIS", "ConnString", "", file);

            Soft.SoftType = csIni.ReadIni("SOFT", "SoftType", "0", file);
            Soft.WindowNo = csIni.ReadIni("SOFT", "WindowNo", "0", file);
            Soft.MacCode = csIni.ReadIni("SOFT", "MacCode", "", file);
            Soft.Function = csIni.ReadIni("SOFT", "Function", "", file);

            InitialConfig_Server();
        }
        public static void InitialConfig_Server()
        {
            DicsSys = ReadConfig("Sys");
            Sys.WidthSpan = int.Parse(DicsSys["WidthSpan"]);
            Sys.HeightSpan = int.Parse(DicsSys["HeightSpan"]);
            Sys.EnableTime = int.Parse(DicsSys["EnableTime"]);
            Sys.DrugTime = DicsSys["DrugTime"];

        }
        public static void InitialConfig_LED()
        {
            DicsLed = ReadConfig("Led");
            Led.Margin_Left = int.Parse(DicsLed["Margin_Left"]);
            Led.Margin_Top = int.Parse(DicsLed["Margin_Top"]);
            Led.Width = int.Parse(DicsLed["Width"]);
            Led.Height = int.Parse(DicsLed["Height"]);
            Led.Left = int.Parse(DicsLed["Left"]);
            Led.Top = int.Parse(DicsLed["Top"]);
            Led.FontSize = int.Parse(DicsLed["FontSize"]);
            Led.RefreshTime = int.Parse(DicsLed["RefreshTime"]);
            Led.OrderType = DicsLed["OrderType"];
        }
        public static void InitialConfig_Mac_A()
        {
            try
            {
                DicsMac_A = ReadConfig(Soft.MacCode);
                Mac_A.IP_PLC = DicsMac_A["IP_PLC"];
                Mac_A.Port_PLC = DicsMac_A["Port_PLC"];
                //Mac_A.IP_DCT = DicsMac_A.Keys.Contains("IP_DCT") ? DicsMac_A["IP_DCT"] : "192.168.3.200";
                //Mac_A.Port_DCT = DicsMac_A.Keys.Contains("Port_DCT") ? int.Parse(DicsMac_A["Port_DCT"]) : 2000;
                Mac_A.Port_DPJ = DicsMac_A["Port_DPJ"];
                Mac_A.Port_Scanner = DicsMac_A["Port_Scanner"];
                Mac_A.Port_Laser_Left = DicsMac_A["Port_Laser_Left"];
                Mac_A.Port_Laser_Right = DicsMac_A["Port_Laser_Right"];

                Mac_A.Length_Pos = int.Parse(DicsMac_A["Length_Pos"]);
                Mac_A.Length_Laser = float.Parse(DicsMac_A["Length_Laser"]);
                Mac_A.Count_Unit = int.Parse(DicsMac_A["Count_Unit"]);
                Mac_A.Count_Lay = int.Parse(DicsMac_A["Count_Lay"]);
                Mac_A.Count_Col = int.Parse(DicsMac_A["Count_Col"]);

                Mac_A.Speed_Manual_X = DicsMac_A["Speed_Manual_X"];
                Mac_A.Speed_Auto_X = DicsMac_A["Speed_Auto_X"];
                Mac_A.Speed_Manual_Z = DicsMac_A["Speed_Manual_Z"];
                Mac_A.Speed_Auto_Z = DicsMac_A["Speed_Auto_Z"];
                Mac_A.Speed_Manual_Plate = DicsMac_A["Speed_Manual_Plate"];
                Mac_A.Speed_Up_Plate = DicsMac_A["Speed_Up_Plate"];
                Mac_A.Speed_Auto_Plate = DicsMac_A["Speed_Auto_Plate"];

                Mac_A.Pulse_Meet_X = DicsMac_A["Pulse_Meet_X"];
                Mac_A.Pulse_Meet_Z = DicsMac_A["Pulse_Meet_Z"];

                Mac_A.Pulse_Lift_Top = DicsMac_A["Pulse_Lift_Top"];
                Mac_A.Pulse_Lift_Up = DicsMac_A["Pulse_Lift_Up"];
                Mac_A.Pulse_Lift_Down = DicsMac_A["Pulse_Lift_Down"];
                Mac_A.Pulse_Lift_Meet = DicsMac_A["Pulse_Lift_Meet"];

                Mac_A.Pulse_Baffle_Open = DicsMac_A["Pulse_Baffle_Open"];
                Mac_A.Pulse_Baffle_Close = DicsMac_A["Pulse_Baffle_Close"];

                Mac_A.Speed_Auto_Lift = DicsMac_A["Speed_Auto_Lift"];
                Mac_A.Speed_Manual_Lift = DicsMac_A["Speed_Manual_Lift"];
                Mac_A.Speed_Auto_Baffle_Lift = DicsMac_A["Speed_Auto_Baffle_Lift"];
                Mac_A.Speed_Manual_Baffle_Lift = DicsMac_A["Speed_Manual_Baffle_Lift"];
                Mac_A.Speed_Auto_Baffle_Belt = DicsMac_A["Speed_Auto_Baffle_Belt"];
                Mac_A.Speed_Manual_Baffle_Belt = DicsMac_A["Speed_Manual_Baffle_Belt"];

                Mac_A.Pulse_Plate_Max_Left = DicsMac_A["Pulse_Plate_Max_Left"];
                Mac_A.Pulse_Plate_Max_Right = DicsMac_A["Pulse_Plate_Max_Right"];

                Mac_A.Acc_Manual_X = DicsMac_A["Acc_Manual_X"];
                Mac_A.Acc_Auto_X = DicsMac_A["Acc_Auto_X"];
                Mac_A.Acc_Manual_Z = DicsMac_A["Acc_Manual_Z"];
                Mac_A.Acc_Auto_Z = DicsMac_A["Acc_Auto_Z"];
                Mac_A.Acc_Manual_Plate = DicsMac_A["Acc_Manual_Plate"];
                Mac_A.Acc_Up_Plate = DicsMac_A["Acc_Up_Plate"];
                Mac_A.Acc_Auto_Plate = DicsMac_A["Acc_Auto_Plate"];
                Mac_A.Acc_Manual_Lift = DicsMac_A["Acc_Manual_Lift"];
                Mac_A.Acc_Auto_Lift = DicsMac_A["Acc_Auto_Lift"];
                Mac_A.Acc_Manual_Baffle = DicsMac_A["Acc_Manual_Baffle"];
                Mac_A.Acc_Auto_Baffle = DicsMac_A["Acc_Auto_Baffle"];

                Mac_A.Pulse_Plate_Min_Left = DicsMac_A["Pulse_Plate_Min_Left"];
                Mac_A.Pulse_Plate_Min_Right = DicsMac_A["Pulse_Plate_Min_Right"];

                Mac_A.PlateHeight = float.Parse(DicsMac_A["PlateHeight"]);
                Mac_A.MinCol = int.Parse(DicsMac_A["MinCol"]);
                Mac_A.MaxCol = int.Parse(DicsMac_A["MaxCol"]);

                Mac_A.PLC_Tcp = DicsMac_A["PLC_Tcp"];
                Mac_A.PLC_Com = DicsMac_A["PLC_Com"];
                Mac_A.Scanner = DicsMac_A["Scanner"];
                Mac_A.Laser_Left = DicsMac_A["Laser_Left"];
                Mac_A.Laser_Right = DicsMac_A["Laser_Right"];
                Mac_A.DPJ = DicsMac_A["DPJ"];

                Mac_A.ScanSpan = int.Parse(DicsMac_A["ScanSpan"]);
                Mac_A.WaitTime_Start = int.Parse(DicsMac_A["WaitTime_Start"]);
                Mac_A.WaitTime_Stop = int.Parse(DicsMac_A["WaitTime_Stop"]);

                Mac_A.DelayTime_Record = int.Parse(DicsMac_A["DelayTime_Record"]);
                Mac_A.StopTime_Record = int.Parse(DicsMac_A["StopTime_Record"]);
                Mac_A.Height_BC = float.Parse(DicsMac_A["Height_BC"]);
                Mac_A.Count_BC = int.Parse(DicsMac_A["Count_BC"]);

                Mac_A.WaitTime_Reset_Extraman = int.Parse(DicsMac_A["WaitTime_Reset_Extraman"]);
                Mac_A.WaitTime_Reset_Plate = int.Parse(DicsMac_A["WaitTime_Reset_Plate"]);
                Mac_A.WaitTime_Reset_Lift = int.Parse(DicsMac_A["WaitTime_Reset_Lift"]);
                Mac_A.WaitTime_Reset_Baffle = int.Parse(DicsMac_A["WaitTime_Reset_Baffle"]);

                Mac_A.WaitTime_Auto_Extraman = int.Parse(DicsMac_A["WaitTime_Auto_Extraman"]);
                Mac_A.WaitTime_Auto_Plate = int.Parse(DicsMac_A["WaitTime_Auto_Plate"]);
                Mac_A.WaitTime_Auto_Lift = int.Parse(DicsMac_A["WaitTime_Auto_Lift"]);
                Mac_A.WaitTime_Auto_Baffle = int.Parse(DicsMac_A["WaitTime_Auto_Baffle"]);
                Mac_A.WaitTime_Up_Plate = int.Parse(DicsMac_A["WaitTime_Up_Plate"]);

                Mac_A.PrescWait = int.Parse(DicsMac_A["PrescWait"]);
                Mac_A.SplitModel = DicsMac_A["SplitModel"];
                Mac_A.PDBeforeAdd = DicsMac_A["PDBeforeAdd"];
                Mac_A.PCNum = int.Parse(DicsMac_A["PCNum"]);
                Mac_A.MaxNum = int.Parse(DicsMac_A["MaxNum"]);

                Mac_A.OneMMPulse_Plate = float.Parse(DicsMac_A["OneMMPulse_Plate"]);
                Mac_A.OneMMPulse_Extraman_X = float.Parse(DicsMac_A["OneMMPulse_Extraman_X"]);
                Mac_A.OneMMPulse_Extraman_Z = float.Parse(DicsMac_A["OneMMPulse_Extraman_Z"]);
                Mac_A.OpenTime_Baffle = int.Parse(DicsMac_A["OpenTime_Baffle"]);
                Mac_A.Laser_Offset_Left_X = int.Parse(DicsMac_A["Laser_Offset_Left_X"]);
                Mac_A.Laser_Offset_Left_Z = int.Parse(DicsMac_A["Laser_Offset_Left_Z"]);
                Mac_A.Laser_Offset_Right_X = int.Parse(DicsMac_A["Laser_Offset_Right_X"]);
                Mac_A.Laser_Offset_Right_Z = int.Parse(DicsMac_A["Laser_Offset_Right_Z"]);

                Mac_A.DrugCount = int.Parse(DicsMac_A["DrugCount"]);
                Mac_A.DrugNum = int.Parse(DicsMac_A["DrugNum"]);
                Mac_A.ShowTest = DicsMac_A["ShowTest"];
            }
            catch (Exception ex)
            {
                fLog.WriteLog(ex.ToString());
            }
        }
        public static void InitialConfig_Mac_S()
        {
            DicsMac_S = ReadConfig(Soft.MacCode);
            Mac_S.Port_PLC = DicsMac_S["Port_PLC"];

            Mac_S.Count_Unit = int.Parse(DicsMac_S["Count_Unit"]);
            Mac_S.Count_Lay = int.Parse(DicsMac_S["Count_Lay"]);
            Mac_S.Count_Col = int.Parse(DicsMac_S["Count_Col"]);
            Mac_S.Perimeter = int.Parse(DicsMac_S["Perimeter"]);

            Mac_S.Speed_Manual = DicsMac_S["Speed_Manual"];
            Mac_S.Speed_Auto = DicsMac_S["Speed_Auto"];

            Mac_S.PLCIsEnable = DicsMac_S["PLCIsEnable"];
            Mac_S.RefreshSpan = int.Parse(DicsMac_S["RefreshSpan"]);
            Mac_S.OverTime = int.Parse(DicsMac_S["OverTime"]);

            Mac_S.ShowRowCount = int.Parse(DicsMac_S["ShowRowCount"]);
            Mac_S.AutoTurn = DicsMac_S["AutoTurn"];
            Mac_S.TurnSpan = int.Parse(DicsMac_S["TurnSpan"]);
            Mac_S.Mode = DicsMac_S["Mode"];
            Mac_S.ShowTest = DicsMac_S["ShowTest"];
        }
        public static void InitialConfig_Mac_C()
        {
            DicsMac_C = ReadConfig(Soft.MacCode);
            Mac_C.IP_PLC = DicsMac_C["IP_PLC"];
            Mac_C.Port_PLC = DicsMac_C["Port_PLC"];
            Mac_C.Port_DPJ = DicsMac_C["Port_DPJ"];
            Mac_C.Port_Scanner = DicsMac_C["Port_Scanner"];
            Mac_C.Port_Laser_Left = DicsMac_C["Port_Laser_Left"];
            Mac_C.Port_Laser_Right = DicsMac_C["Port_Laser_Right"];

            Mac_C.Length_Pos = int.Parse(DicsMac_C["Length_Pos"]);
            Mac_C.Length_Laser = float.Parse(DicsMac_C["Length_Laser"]);
            Mac_C.Count_Unit = int.Parse(DicsMac_C["Count_Unit"]);
            Mac_C.Count_Lay = int.Parse(DicsMac_C["Count_Lay"]);
            Mac_C.Count_Col = int.Parse(DicsMac_C["Count_Col"]);

            Mac_C.Speed_Manual_X = DicsMac_C["Speed_Manual_X"];
            Mac_C.Speed_Auto_X = DicsMac_C["Speed_Auto_X"];
            Mac_C.Speed_Manual_Z = DicsMac_C["Speed_Manual_Z"];
            Mac_C.Speed_Auto_Z = DicsMac_C["Speed_Auto_Z"];
            Mac_C.Speed_Manual_Plate = DicsMac_C["Speed_Manual_Plate"];
            Mac_C.Speed_Up_Plate = DicsMac_C["Speed_Up_Plate"];
            Mac_C.Speed_Auto_Plate = DicsMac_C["Speed_Auto_Plate"];

            Mac_C.Pulse_Meet_X = DicsMac_C["Pulse_Meet_X"];
            Mac_C.Pulse_Meet_Z = DicsMac_C["Pulse_Meet_Z"];

            Mac_C.Pulse_Lift_Top = DicsMac_C["Pulse_Lift_Top"];
            Mac_C.Pulse_Lift_Up = DicsMac_C["Pulse_Lift_Up"];
            Mac_C.Pulse_Lift_Down = DicsMac_C["Pulse_Lift_Down"];
            Mac_C.Pulse_Lift_Meet = DicsMac_C["Pulse_Lift_Meet"];

            Mac_C.Pulse_Baffle_Open = DicsMac_C["Pulse_Baffle_Open"];
            Mac_C.Pulse_Baffle_Close = DicsMac_C["Pulse_Baffle_Close"];

            Mac_C.Speed_Auto_Lift = DicsMac_C["Speed_Auto_Lift"];
            Mac_C.Speed_Manual_Lift = DicsMac_C["Speed_Manual_Lift"];
            Mac_C.Speed_Auto_Baffle_Lift = DicsMac_C["Speed_Auto_Baffle_Lift"];
            Mac_C.Speed_Manual_Baffle_Lift = DicsMac_C["Speed_Manual_Baffle_Lift"];
            Mac_C.Speed_Auto_Baffle_Belt = DicsMac_C["Speed_Auto_Baffle_Belt"];
            Mac_C.Speed_Manual_Baffle_Belt = DicsMac_C["Speed_Manual_Baffle_Belt"];

            Mac_C.Pulse_Plate_Max_Left = DicsMac_C["Pulse_Plate_Max_Left"];
            Mac_C.Pulse_Plate_Max_Right = DicsMac_C["Pulse_Plate_Max_Right"];

            Mac_C.Acc_Manual_X = DicsMac_C["Acc_Manual_X"];
            Mac_C.Acc_Auto_X = DicsMac_C["Acc_Auto_X"];
            Mac_C.Acc_Manual_Z = DicsMac_C["Acc_Manual_Z"];
            Mac_C.Acc_Auto_Z = DicsMac_C["Acc_Auto_Z"];
            Mac_C.Acc_Manual_Plate = DicsMac_C["Acc_Manual_Plate"];
            Mac_C.Acc_Up_Plate = DicsMac_C["Acc_Up_Plate"];
            Mac_C.Acc_Auto_Plate = DicsMac_C["Acc_Auto_Plate"];
            Mac_C.Acc_Manual_Lift = DicsMac_C["Acc_Manual_Lift"];
            Mac_C.Acc_Auto_Lift = DicsMac_C["Acc_Auto_Lift"];
            Mac_C.Acc_Manual_Baffle = DicsMac_C["Acc_Manual_Baffle"];
            Mac_C.Acc_Auto_Baffle = DicsMac_C["Acc_Auto_Baffle"];

            Mac_C.Pulse_Plate_Min_Left = DicsMac_C["Pulse_Plate_Min_Left"];
            Mac_C.Pulse_Plate_Min_Right = DicsMac_C["Pulse_Plate_Min_Right"];

            Mac_C.PlateHeight = float.Parse(DicsMac_C["PlateHeight"]);
            Mac_C.MinCol = int.Parse(DicsMac_C["MinCol"]);
            Mac_C.MaxCol = int.Parse(DicsMac_C["MaxCol"]);

            Mac_C.PLC_Tcp = DicsMac_C["PLC_Tcp"];
            Mac_C.PLC_Com = DicsMac_C["PLC_Com"];
            Mac_C.Scanner = DicsMac_C["Scanner"];
            Mac_C.Laser_Left = DicsMac_C["Laser_Left"];
            Mac_C.Laser_Right = DicsMac_C["Laser_Right"];
            Mac_C.DPJ = DicsMac_C["DPJ"];

            Mac_C.ScanSpan = int.Parse(DicsMac_C["ScanSpan"]);
            Mac_C.WaitTime_Start = int.Parse(DicsMac_C["WaitTime_Start"]);
            Mac_C.WaitTime_Stop = int.Parse(DicsMac_C["WaitTime_Stop"]);

            Mac_C.DelayTime_Record = int.Parse(DicsMac_C["DelayTime_Record"]);
            Mac_C.StopTime_Record = int.Parse(DicsMac_C["StopTime_Record"]);
            Mac_C.Height_BC = float.Parse(DicsMac_C["Height_BC"]);
            Mac_C.Count_BC = int.Parse(DicsMac_C["Count_BC"]);

            Mac_C.WaitTime_Reset_Extraman = int.Parse(DicsMac_C["WaitTime_Reset_Extraman"]);
            Mac_C.WaitTime_Reset_Plate = int.Parse(DicsMac_C["WaitTime_Reset_Plate"]);
            Mac_C.WaitTime_Reset_Lift = int.Parse(DicsMac_C["WaitTime_Reset_Lift"]);
            Mac_C.WaitTime_Reset_Baffle = int.Parse(DicsMac_C["WaitTime_Reset_Baffle"]);

            Mac_C.WaitTime_Auto_Extraman = int.Parse(DicsMac_C["WaitTime_Auto_Extraman"]);
            Mac_C.WaitTime_Auto_Plate = int.Parse(DicsMac_C["WaitTime_Auto_Plate"]);
            Mac_C.WaitTime_Auto_Lift = int.Parse(DicsMac_C["WaitTime_Auto_Lift"]);
            Mac_C.WaitTime_Auto_Baffle = int.Parse(DicsMac_C["WaitTime_Auto_Baffle"]);
            Mac_C.WaitTime_Up_Plate = int.Parse(DicsMac_C["WaitTime_Up_Plate"]);

            Mac_C.PrescWait = int.Parse(DicsMac_C["PrescWait"]);
            Mac_C.SplitModel = DicsMac_C["SplitModel"];
            Mac_C.PDBeforeAdd = DicsMac_C["PDBeforeAdd"];
            Mac_C.PCNum = int.Parse(DicsMac_C["PCNum"]);
            Mac_C.MaxNum = int.Parse(DicsMac_C["MaxNum"]);

            Mac_C.OneMMPulse_Plate = float.Parse(DicsMac_C["OneMMPulse_Plate"]);
            Mac_C.OneMMPulse_Extraman_X = float.Parse(DicsMac_C["OneMMPulse_Extraman_X"]);
            Mac_C.OneMMPulse_Extraman_Z = float.Parse(DicsMac_C["OneMMPulse_Extraman_Z"]);
            Mac_C.OpenTime_Baffle = int.Parse(DicsMac_C["OpenTime_Baffle"]);
            Mac_C.Laser_Offset_Left_X = int.Parse(DicsMac_C["Laser_Offset_Left_X"]);
            Mac_C.Laser_Offset_Left_Z = int.Parse(DicsMac_C["Laser_Offset_Left_Z"]);
            Mac_C.Laser_Offset_Right_X = int.Parse(DicsMac_C["Laser_Offset_Right_X"]);
            Mac_C.Laser_Offset_Right_Z = int.Parse(DicsMac_C["Laser_Offset_Right_Z"]);

            Mac_C.DrugCount = int.Parse(DicsMac_C["DrugCount"]);
            Mac_C.DrugNum = int.Parse(DicsMac_C["DrugNum"]);
            Mac_C.ShowTest = DicsMac_C["ShowTest"];
        }

        //根据类型读取配置
        public static Dictionary<string, string> ReadConfig(string type)
        {
            Dictionary<string, string> dics = new Dictionary<string, string>();

            DataTable dt = new DataTable();
            string sql = "select paracode,paravalue from Sys_Parameter where paragroup='" + type + "'";
            csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
            foreach (DataRow row in dt.Rows)
            {
                dics.Add(row[0].ToString().Trim(), row[1].ToString().Trim());
            }
            return dics;
        }

        //保存配置值
        public static void SaveConfig(string group, string code, string value)
        {
            string sql = "update Sys_Parameter set paravalue='" + value + "' where paragroup='" + group + "' and paracode='" + code + "'";
            csSql.ExecuteSql(sql, Soft.ConnString);

            switch (Soft.SoftType)
            {
                case "发药机软件":
                    InitialConfig_Mac_A(); 
                    break;
                case "滚筒柜软件":
                    InitialConfig_Mac_S();
                    break;
                case "中药机软件":
                    InitialConfig_Mac_C();
                    break;
            }
        }
    }
}
