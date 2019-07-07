using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace CSHelper
{
    public class SQL
    {
        public delegate void ShowMsg(string msg);
        public static ShowMsg ThrowMsg;
        private static void SendError(string str)
        {
            ThrowMsg?.Invoke(str);
        }

        LOG log = new LOG();

        public bool SQLIsConnected(string connStr)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(connStr))
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
                    SendError(error);
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
        public bool ExecuteSql(string sql, string connStr)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
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
                    SendError(error);
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
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
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
                    SendError(error);
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
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                try
                {
                    SqlDataAdapter sda = new SqlDataAdapter(sql, conn);
                    sda.Fill(dt);
                    result = true;
                }
                catch (Exception ex)
                {
                    string error = ex.Message + "\r\n" + sql;
                    log.WriteLog(error);
                    SendError(error);
                }
            }
            return result;
        }

        /// <summary>
        /// 执行SLQ存储过程
        /// </summary>
        /// <param name="sp">存储过程名称</param>
        /// <param name="connStr">SQL连接</param>
        /// <param name="input">输入参数数组</param>
        /// <param name="output">输出参数数组</param>
        /// <param name="dt">数据表</param>
        /// <returns></returns>
        public bool ExecuteSqlSP(string sp, string connStr, string[] input, out string[] output, out DataTable dt)
        {
            bool result=false;
            dt = new DataTable();
            string sql = @"select 'Parameter_name'=name,'Type'=type_name(xusertype),'Length'=length,   
                          'Prec'=case when type_name(xtype)='uniqueidentifier' then xprec else OdbcPrec(xtype,length,xprec) end,     
                          'Scale'=OdbcScale(xtype,xscale),'Param_order'=colid,status
                          from syscolumns where id=object_id('" + sp + "')";
            DataTable dt_Prec;
            ExecuteSelect(sql, connStr, out dt_Prec);
            SqlParameter[] sps = new SqlParameter[dt_Prec.Rows.Count];
            for (int i = 0; i < dt_Prec.Rows.Count; i++)
            {
                sps[i] = Sql_CanShu(dt_Prec, i, input);
            }
            //输出参数 赋值
            DataRow[] dr = dt_Prec.Select("status='72'");
            output = new string[dr.Length];

            using (SqlConnection conn = new SqlConnection(connStr))
            {

                SqlCommand cmd = new SqlCommand(sp, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    conn.Open();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.SelectCommand.Parameters.AddRange(sps);
                    sda.Fill(dt);

                    for (int j = 0; j < dr.Length; j++)
                    {
                        output[j] = cmd.Parameters[dr[j][0].ToString()].Value.ToString();
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    string error = ex.Message + "\r\n" + sql;
                    log.WriteLog(error);
                    SendError(error);
                }
            }
            return result;
        }
        public SqlParameter Sql_CanShu(DataTable dt, int i, string[] input)
        {
            string type = dt.Rows[i]["Type"].ToString();
            SqlDbType st = new SqlDbType();

            if (SqlDbType.BigInt.ToString().ToLower() == type)
            {
                st = SqlDbType.BigInt;
            }
            else if (SqlDbType.Int.ToString().ToLower() == type)
            {
                st = SqlDbType.Int;
            }
            else if (SqlDbType.Binary.ToString().ToLower() == type)
            {
                st = SqlDbType.Binary;
            }
            else if (SqlDbType.Bit.ToString().ToLower() == type)
            {
                st = SqlDbType.Bit;
            }
            else if (SqlDbType.Char.ToString().ToLower() == type)
            {
                st = SqlDbType.Char;
            }
            else if (SqlDbType.Date.ToString().ToLower() == type)
            {
                st = SqlDbType.Date;
            }
            else if (SqlDbType.DateTime.ToString().ToLower() == type)
            {
                st = SqlDbType.DateTime;
            }
            else if (SqlDbType.DateTime2.ToString().ToLower() == type)
            {
                st = SqlDbType.DateTime2;
            }
            else if (SqlDbType.DateTimeOffset.ToString().ToLower() == type)
            {
                st = SqlDbType.DateTimeOffset;
            }
            else if (SqlDbType.Decimal.ToString().ToLower() == type)
            {
                st = SqlDbType.Decimal;
            }
            else if (SqlDbType.VarChar.ToString().ToLower() == type)
            {
                st = SqlDbType.VarChar;
            }
            else if (SqlDbType.NVarChar.ToString().ToLower() == type)
            {
                st = SqlDbType.NVarChar;
            }
            else
            {
                st = SqlDbType.NVarChar;
            }
            SqlParameter sp = new SqlParameter();
            if (dt.Rows[i]["status"].ToString() == "8")
            {
                sp = new SqlParameter(dt.Rows[i]["Parameter_name"].ToString(), input[i]);
                sp.Direction = ParameterDirection.Input;
            }
            else if (dt.Rows[i]["status"].ToString() == "72")
            {
                sp = new SqlParameter(dt.Rows[i]["Parameter_name"].ToString(), st);
                sp.Direction = ParameterDirection.Output;
            }

            if (dt.Rows[i]["Type"].ToString().ToLower() == "decimal" || dt.Rows[i]["Type"].ToString().ToLower() == "numeric")
            {
                sp.Precision = byte.Parse(dt.Rows[i]["Prec"].ToString());
                sp.Scale = byte.Parse(dt.Rows[i]["Scale"].ToString());
            }
            else
            {
                //当数据类型是Max的时候
                if (int.Parse(dt.Rows[i]["Prec"].ToString()) == 0)
                {
                    sp.Size = 8000;
                }
                else
                {
                    sp.Size = int.Parse(dt.Rows[i]["Prec"].ToString());
                }
            }
            return sp;
        }
    }
}
