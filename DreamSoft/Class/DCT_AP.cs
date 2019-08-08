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
using System.Data;

namespace DreamSoft
{
    class DCT_AP
    {
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
             new HslCommunication.LogNet.LogNetDateTime(Environment.CurrentDirectory + @"/Log/DCT", HslCommunication.LogNet.GenerateMode.ByEveryDay);

        private static CSHelper.SQL csSql = new CSHelper.SQL();
        private static List<ControlCard> lstCard = new List<ControlCard>();
        public static void DCT_AP_Initial(string connStr, int colNum)
        {
            ColumnCount = colNum;
            DataTable dt;
            string sql = string.Format("select * from dct_ip where maccode='{0}'", Config.Soft.MacCode);
            if (csSql.ExecuteSelect(sql, connStr, out dt))
            {
                foreach (DataRow row in dt.Rows)
                {
                    string ip = row["IP"].ToString();
                    int unitCode, beginLayer, endLayer, port;
                    if (int.TryParse(row["UnitCode"].ToString(), out unitCode) && int.TryParse(row["BeginLayer"].ToString(), out beginLayer)
                        && int.TryParse(row["EndLayer"].ToString(), out endLayer) && int.TryParse(row["Port"].ToString(), out port))
                    {
                        DCT_Single dsg = new DCT_Single(ip, port, string.Format("_{0}_{1}_{2}", unitCode, beginLayer, endLayer));
                        ControlCard cc = new ControlCard()
                        { UnitCode = unitCode, BeginLayer = beginLayer, EndLayer = endLayer, IP = ip, Port = port, Card = dsg };
                        lstCard.Add(cc);
                    }
                }
            }
        }
        class ControlCard
        {
            public int UnitCode { get; set; }
            public int BeginLayer { get; set; }
            public int EndLayer { get; set; }
            public string IP { get; set; }
            public int Port { get; set; }
            public DCT_Single Card { get; set; }
        }

        private static int ColumnCount = 0;
        public static bool DCTMoveDownSingle(int unit, int master, int dct, int time)
        {
            DCT_Single dsg; int code;
            GetCode(unit, master, dct, out dsg, out code);
            if (dsg != null)
                return dsg.DCTMoveDownSingle(code, time);
            return false;
        }
        public static bool ReadRecordSingle(int unit, int master, int dct, out int record)
        {
            record = 0;
            DCT_Single dsg; int code;
            GetCode(unit, master, dct, out dsg, out code);
            if (dsg != null)
                return dsg.ReadRecordSingle(code, out record);
            return false;
        }
        public static bool ClearRecordSingle(int unit, int master, int dct)
        {
            DCT_Single dsg; int code;
            GetCode(unit, master, dct, out dsg, out code);
            if (dsg != null)
                return dsg.ClearRecordSingle(code);
            return false;
        }
        private static void GetCode(int unit, int master, int dct, out DCT_Single dsg, out int code)
        {
            dsg = null; code = 0;
            var array = lstCard.Where(p => p.UnitCode == unit && p.BeginLayer <= master && master <= p.EndLayer);
            if (array.Count() > 0)
            {
                ControlCard cc = array.FirstOrDefault();
                dsg = cc.Card;
                code = (master - cc.BeginLayer) * ColumnCount + dct;
            }
        }
    }
}
