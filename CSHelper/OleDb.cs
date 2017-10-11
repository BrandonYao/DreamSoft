using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace CSHelper
{
    public class OleDb
    {
        LOG log = new LOG();

        /// <summary>
        /// 执行SQL语句，返回执行结果
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="connStr">SQL连接字符串</param>
        /// <returns></returns>
        public bool ExecuteOleDb(string sql, string connStr)
        {
            bool result = false;
            using (OleDbConnection conn = new OleDbConnection(connStr))
            {
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception ex)
                {
                    string error = ex.Message + "---" + sql;
                    log.WriteLog(error);
                }
            }
            return result;
        }
        /// <summary>
        /// 读取第一行第一列
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="connStr">SQL连接字符串</param>
        /// <param name="value">返回值</param>
        /// <returns></returns>
        public bool ExecuteScalar(string sql, string connStr, out string value)
        {
            bool result = false;
            value = "";
            using (OleDbConnection conn = new OleDbConnection(connStr))
            {
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                try
                {
                    conn.Open();
                    object ob = cmd.ExecuteScalar();
                    if (ob != null)
                        value = ob.ToString();
                    result = true;
                }
                catch (Exception ex)
                {
                   string  error = ex.Message + "---" + sql;
                    log.WriteLog(error);
                }
            }
            return result;
        }
        /// <summary>
        /// 读取查询数据表
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="connStr">SQL连接字符串</param>
        /// <returns></returns>
        public bool ExecuteSelect(string sql, string connStr, out DataTable dt)
        {
            bool result = false;
            dt = new DataTable();
            using (OleDbConnection conn = new OleDbConnection(connStr))
            {
                try
                {
                    OleDbDataAdapter sda = new OleDbDataAdapter(sql, conn);
                    sda.Fill(dt);
                    result = true;
                }
                catch (Exception ex)
                {
                    string error = ex.Message + "---" + sql;
                    log.WriteLog(error);
                }
            }
            return result;
        }
    }
}
