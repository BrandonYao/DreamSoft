using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OracleClient;

namespace CSHelper
{
    public class Oracle
    {
        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;

        LOG log = new LOG();

        public bool OracleIsConnected(string connStr)
        {
            bool result = false;
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                try
                {
                    conn.Open();
                    result = true;
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    log.WriteLog(error);
                    ThrowMsg(error);
                }
            }
            return result;
        }

        /// <summary>
        /// 执行操作语句
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="connStr">ConnectionString</param>
        /// <returns></returns>
        public bool ExecuteOracle(string sql, string connStr)
        {
            bool result = false;
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(sql, conn);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception ex)
                {
                    string error = ex.Message + "\r\n" + sql;
                    log.WriteLog(error);
                    ThrowMsg(error);
                }
            }
            return result;
        }

        /// <summary>
        /// 查询第一行的第一列
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="connStr">ConnectionString</param>
        /// <param name="value">String</param>
        /// <returns></returns>
        public bool ExecuteScalar(string sql, string connStr, out string value)
        {
            bool result = false;
            value = null;
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                OracleCommand cmd = new OracleCommand(sql, conn);
                try
                {
                    conn.Open();
                    object ob = cmd.ExecuteScalar();
                    if (ob != null)
                        value = ob.ToString();
                }
                catch (Exception ex)
                {
                    string error = ex.Message + "\r\n" + sql;
                    log.WriteLog(error);
                    ThrowMsg(error);
                }
            }
            return result;
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="connStr">ConnectionString</param>
        /// <param name="dt">DataTable</param>
        /// <returns></returns>
        public bool ExecuteSelect(string sql, string connStr, out DataTable dt)
        {
            bool result = false;
            dt = new DataTable();
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                try
                {
                    OracleDataAdapter sda = new OracleDataAdapter(sql, conn);
                    sda.Fill(dt);
                    result = true;
                }
                catch (Exception ex)
                {
                    string error = ex.Message + "\r\n" + sql;
                    log.WriteLog(error);
                    ThrowMsg(error);
                }
            }
            return result;
        }
    }
}
