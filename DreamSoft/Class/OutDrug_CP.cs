using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Windows.Media;

namespace DreamSoft.Class
{
    class OutDrug_CP
    {
        CSHelper.SQL csSql = new CSHelper.SQL();

        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;

        public static bool IsOut = false; //正在出药标识

        public PLC_Tcp_AP.MovePos OutPosition; //出药位置
        public DataTable dtToOut; //预出药数据表：poscode,num,record,error
        public bool BC; //补偿出药标识
        public string PrescNo; //处方号
        public int PC; //出药批次

        public DataTable dtDrug;

        //构造函数
        public OutDrug_CP(PLC_Tcp_AP.MovePos type, DataTable dt, bool bc, string presc, int pc, DataTable dtD)
        {
            OutPosition = type;
            dtToOut = dt;
            BC = bc;
            dicsOut = new Dictionary<string, int>();
            PrescNo = presc;
            PC = pc;
            dtDrug = dtD;
        }

        public void GoOut()
        {
            Thread thOut = new Thread(Out);
            thOut.IsBackground = true;
            thOut.Start();
        }

        public Dictionary<string, int> dicsOut; //实际出药数据表
        //开始出药
        public void Out()
        {
            //锁定出药
            IsOut = true;

            //添加计数列，初始化为0
            dtToOut.Columns.Add("Record");
            dtToOut.Columns.Add("Error");
            foreach (DataRow row in dtToOut.Rows)
            {
                row["Record"] = 0;
                row["Error"] = 0;
            }
            
            bool haveOut = false;

            //检查每个储位库存是否足够出药，库存不足error=100，忽略出药
            foreach (DataRow row in dtToOut.Rows)
            {
                string poscode = row["poscode"].ToString().Trim();
                int num = int.Parse(row["num"].ToString().Trim());
                //查询是否缺药
                string s;
                string sql = "select isnull(drugnum,0) from drug_pos where maccode='{0}' and poscode='{1}' and errorNum<3";
                sql = string.Format(sql, Config.Soft.MacCode, poscode);
                csSql.ExecuteScalar(sql, Config.Soft.ConnString, out s);
                int kc = 0;
                if (s != null)
                    kc = int.Parse(s);
                if (num > kc)
                    row["error"] = 100;
            }

            List<string> poss = new List<string>();
            if (NeedOut(dtToOut, out poss))
            {
                PLC_Tcp_CP.ChangeOut(1);

                if (!PLC_Tcp_CP.LiftOnMeet())
                {
                    PLC_Tcp_CP.LiftAutoMoveToPos(PLC_Tcp_AP.MovePos.Meet);
                }
                if (!PLC_Com_CP.BaffleOnClose())
                {
                    PLC_Com_CP.BaffleClose();
                }

                //ThrowMsg("出药准备");
                PLC_Tcp_CP.TransferBeltMove(PLC_Tcp_AP.TransferBeltMoveType.Right);
                Thread.Sleep(Config.Mac_C.WaitTime_Start);

                //ThrowMsg("等待提升机到位");
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(200);
                while (!PLC_Tcp_CP.LiftOnMeet())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Lift))
                    {
                        ThrowMsg("提升机未到位");
                        IsOut = false;
                        return;
                    }
                    Thread.Sleep(200);
                }
                //ThrowMsg("等待挡板关闭");
                timeBegin = DateTime.Now;
                Thread.Sleep(200);
                while (!PLC_Com_CP.BaffleOnClose())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Baffle))
                    {
                        ThrowMsg("挡板未关闭");
                        IsOut = false;
                        return;
                    }
                    Thread.Sleep(200);
                }
            }

        //出药开始标签
        outStart:
            NeedOut(dtToOut, out poss);
            //计数清零
            PLC_Tcp_CP.ClearRecordMutil(poss);
            Dictionary<string, int> dicsOldRecord = PLC_Tcp_CP.ReadRecordMutil(poss);
            Dictionary<string,int> dicsNewRecord;
            //ThrowMsg("开始出药");
            while (NeedOut(dtToOut, out poss))
            {
                //ThrowMsg("出药动作");
                PLC_Tcp_CP.DCTMoveDownMutil(poss);
                //ThrowMsg("等待计数");
                Thread.Sleep(1000);
                dicsNewRecord = PLC_Tcp_CP.ReadRecordMutil(poss);
                //ThrowMsg("更新出药数据表");
                foreach (string p in dicsNewRecord.Keys)
                {
                    int oldRecord = 0;
                    if (dicsOldRecord.Keys.Contains(p))
                        oldRecord = dicsOldRecord[p];
                    else
                        dicsOldRecord.Add(p, 0);
                    int newRecord = dicsNewRecord[p];
                    DataRow[] rows = dtToOut.Select("poscode ='" + p + "'");
                    rows[0]["record"] = newRecord;
                    if (newRecord - oldRecord != 1)
                    {
                        rows[0]["error"] = int.Parse(rows[0]["error"].ToString()) + 1;

                        ThrowMsg("记录出药异常");
                        string sql = "insert into sys_error select '{0}','{1}',drugonlycode,'OE',1,'{2}',getdate() from drug_pos where poscode='{1}'";
                        sql = string.Format(sql, Config.Soft.MacCode, p, newRecord - oldRecord);
                        sql += ";update drug_pos set errornum=(errornum+1) where maccode='{0}' and poscode='{1}'";
                        sql = string.Format(sql, Config.Soft.MacCode, p);
                        csSql.ExecuteSql(sql, Config.Soft.ConnString);
                    }
                }
                dicsOldRecord = dicsNewRecord;
            }
            //ThrowMsg("出药数据表");
            foreach (DataRow row in dtToOut.Rows)
            {
                string poscode = row["poscode"].ToString().Trim();
                int record = int.Parse(row["record"].ToString().Trim());
                if (record > 0)
                {
                    haveOut = true;
                    if (dicsOut.Keys.Contains(poscode))
                        dicsOut[poscode] += record;
                    else
                        dicsOut.Add(poscode, record);
                }
            }

            //ThrowMsg("补偿出药");
            if (BC)
            {
                DataTable dtBC = dtToOut.DefaultView.ToTable();
                dtBC.Clear();
                foreach (DataRow row in dtToOut.Rows)
                {
                    string poscode = row["poscode"].ToString().Trim();
                    int num = int.Parse(row["num"].ToString().Trim());
                    int record = int.Parse(row["record"].ToString().Trim());
                    if (num > record)
                    {
                        string drugOnlyCode;
                        string sql = "select drugonlycode from drug_pos where maccode='{0}' and poscode='{1}'";
                        sql = string.Format(sql, Config.Soft.MacCode, poscode);
                        csSql.ExecuteScalar(sql, Config.Soft.ConnString, out drugOnlyCode);
                        if (!string.IsNullOrEmpty(drugOnlyCode))
                            BCOut(drugOnlyCode, num - record, ref dtBC);
                    }
                }
                dtToOut = dtBC;
                BC = false;
                goto outStart;
            }

            //ThrowMsg("更新库存");
            UpdateStock(dicsOut);
            //ThrowMsg("更新出药结果（多出，少出）");
            if (!string.IsNullOrEmpty(PrescNo))
            {
                UpdateOutResult(dicsOut, dtDrug);
            }

            if (haveOut)
                Thread.Sleep(Config.Mac_C.WaitTime_Stop);

            PLC_Tcp_CP.TransferBeltMove(PLC_Tcp_AP.TransferBeltMoveType.Stop);
            if (haveOut)
            {
                if (OutPosition == PLC_Tcp_AP.MovePos.Top)
                    PLC_Tcp_CP.TopBeltMove(PLC_Tcp_AP.TopBeltMoveType.Turn);

                //ThrowMsg("送至出药口");
                PLC_Tcp_CP.LiftAutoMoveToPos(OutPosition);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(500);
                while (!PLC_Tcp_CP.LiftAutoMoveIsOK())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_C.WaitTime_Auto_Lift))
                    {
                        ThrowMsg("提升机未到位");
                        IsOut = false;
                        return;
                    }
                    Thread.Sleep(200);
                }

                //ThrowMsg("出药");
                PLC_Com_CP.BaffleOpen();
                while (!PLC_Com_CP.BaffleOnOpen())
                    Thread.Sleep(200);
                Thread.Sleep(Config.Mac_C.OpenTime_Baffle);
                PLC_Com_CP.BaffleClose();

                if (OutPosition == PLC_Tcp_AP.MovePos.Top)
                {
                    Thread.Sleep(2000);
                    PLC_Tcp_CP.TopBeltMove(PLC_Tcp_AP.TopBeltMoveType.Stop);
                }
                //ThrowMsg("出药返回");
                PLC_Tcp_CP.LiftAutoMoveToPos(PLC_Tcp_AP.MovePos.Meet);

                IsOut = false;
            }
            else
            {
                IsOut = false;
            }
        }

        //判断是否需要继续出药
        private bool NeedOut(DataTable dt, out List<string> poss)
        {
            poss = new List<string>();
            bool result = false;
            foreach (DataRow row in dt.Rows)
            {
                string poscode = row["poscode"].ToString().Trim();
                int num = int.Parse(row["num"].ToString().Trim());
                int record = int.Parse(row["record"].ToString().Trim());
                int error = int.Parse(row["error"].ToString().Trim());
                if (num > record && error < 3)
                {
                    poss.Add(poscode);
                    result = true;
                }
            }
            return result;
        }
        //添加补偿数据
        private void BCOut(string code, int num, ref DataTable dt)
        {
            //查询是否缺药
            string s;
            string sql = "select sum(ISNULL(drugnum,0)-ISNULL(drugnummin,1)) as kc from drug_pos where maccode='{0}' and DrugonlyCode='{1}' and errorNum<3 and (ISNULL(drugnum,0)-ISNULL(drugnummin,1))>0";
            sql = string.Format(sql, Config.Soft.MacCode, code);
            csSql.ExecuteScalar(sql, Config.Soft.ConnString, out s);
            int kc = 0;
            if (!string.IsNullOrEmpty(s))
                kc = int.Parse(s);
            if (num <= kc)
            {
                //查询快发信息
                sql = "select poscode,(ISNULL(drugnum,0)-ISNULL(drugnummin,1)) as drugnum from drug_pos where maccode='{0}' and DrugonlyCode='{1}' and errorNum<3 and (ISNULL(drugnum,0)-ISNULL(drugnummin,1))>0 order by drugbatch,drugnum desc";
                sql = string.Format(sql, Config.Soft.MacCode, code);
                DataTable dtKF;
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtKF);

                //库存从高到低，轮流分配出药
                while (num > 0)
                {
                    foreach (DataRow dr_Num in dtKF.Rows)
                    {
                        if (num > 0)
                        {
                            int num_Pos = int.Parse(dr_Num["drugnum"].ToString());
                            if (num_Pos > 0)
                            {
                                string posCode = dr_Num["poscode"].ToString().Trim();
                                //分配一个
                                dr_Num["drugnum"] = int.Parse(dr_Num["drugnum"].ToString()) - 1;
                                num -= 1;
                                AddOut(ref dt, posCode, 1);
                            }
                        }
                        else
                            break;
                    }
                }
                num = 0;
            }
        }
        //添加出药数据表
        void AddOut(ref DataTable dt, string posCode, int num)
        {
            bool add = true;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["poscode"].ToString().Trim() == posCode)
                {
                    add = false;
                    dr["num"] = int.Parse(dr["num"].ToString()) + num;
                    break;
                }
            }
            if (add)
            {
                DataRow dr = dt.NewRow();
                dr["poscode"] = posCode;
                dr["num"] = num; 
                dr["record"] = 0; 
                dr["error"] = 0;
                dt.Rows.Add(dr);
            }
        }

        //更新库存
        private void UpdateStock(Dictionary<string, int> dics)
        {
            string sql = "";
            if (dics.Count > 0)
            {
                foreach (string p in dics.Keys)
                {
                    string s = "update drug_pos set drugnum=(case when (drugnum-{0})<0 then 0 else (drugnum-{0}) end) where maccode='{1}' and poscode='{2}';";
                    sql += string.Format(s, dics[p], Config.Soft.MacCode, p);

                    //出入记录
                    s = "insert into drug_import select '{0}',drugonlycode,drugbatch,'O','{1}',drugunit,'{2}',getdate(),'{3}' from drug_pos where maccode='{0}' and poscode='{4}';";
                    sql += string.Format(s, Config.Soft.MacCode, dics[p], Config.Soft.UserCode, PrescNo, p);
                }
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
            }
        }

        //更新出药结果
        private void UpdateOutResult(Dictionary<string, int> dics, DataTable dt)
        {
            //应出数据
            Dictionary<string, int> dicsTo = new Dictionary<string, int>();
            foreach (DataRow row in dt.Rows)
            {
                dicsTo.Add(row["drugonlycode"].ToString().Trim(), int.Parse(row["num"].ToString()));
            }
            //实出数据
            Dictionary<string, int> dicsReal = new Dictionary<string, int>();
            foreach (string pos in dics.Keys)
            {
                string sql = "select drugonlycode from drug_pos where maccode='{0}' and poscode='{1}'";
                sql = string.Format(sql, Config.Soft.MacCode, pos);
                string v = "";
                csSql.ExecuteScalar(sql, Config.Soft.ConnString, out v);
                if (!string.IsNullOrEmpty(v))
                {
                    if (dicsReal.ContainsKey(v))
                        dicsReal[v] += dics[pos];
                    else dicsReal.Add(v, dics[pos]);
                }
            }
            //比较
            foreach (string p in dicsTo.Keys)
            {
                int m = dicsTo[p];
                int n = 0;
                if (dicsReal.ContainsKey(p))
                    n = dicsReal[p];

                string flag = "N";
                if (m == n)//准确（Y）
                    flag = "Y";
                else if (m > n)//少出（Q）
                    flag = "Q";
                else//多出（M）
                    flag = "M";
                string sql = "update pat_druginfo set doflag='{0}' where prescno='{1}' and drugonlycode='{2}'";
                sql = string.Format(sql, flag, PrescNo, p);
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
            }
        }
    }
}