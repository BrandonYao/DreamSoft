using System;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Data;
using System.Windows.Threading;
using System.Windows.Controls;

namespace DreamSoft
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UCData : UserControl
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        CSHelper.Msg csMsg = new CSHelper.Msg();
        CSHelper.Oracle csOracle = new CSHelper.Oracle();

        public UCData()
        {
            InitializeComponent();
        }

        private void UpdateDrug(string code)
        {
            string sql_His = "select * from view_interface_druginfojz where isenable=1 and drugonlycode like '%{0}%'";
            sql_His = string.Format(sql_His, code);
            DataTable dt_Drug;
            csOracle.ExecuteSelect(sql_His, Config.Soft.ConnString_His, out dt_Drug);
            if (dt_Drug != null)
            {
                if (dt_Drug.Rows.Count > 0)
                {
                    string sql_Dream = "";
                    foreach (DataRow row in dt_Drug.Rows)
                    {
                        string str_Update = ""; string str_Insert_Column = ""; string str_Insert_Value = "";
                        #region "拼接语句"
                        string s = "drugcode";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugname";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugaliasname";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugpycode";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugaliaspycode";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugspec";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugtype";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugpackunit";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugpacknum";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugsplitunit";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugsplitnum";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugfacid";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "drugfactory";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "remark";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "base_dose";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        s = "dose_unit";
                        if (dt_Drug.Columns.Contains(s))
                        {
                            str_Update += "," + s + "='" + row[s].ToString().Trim() + "'";
                            str_Insert_Column += "," + s;
                            str_Insert_Value += ",'" + row[s].ToString().Trim() + "'";
                        }
                        #endregion
                        string p = @"if exists(select * from drug_info where drugonlycode='{0}') 
update drug_info set drugonlycode='{0}' {1} where drugonlycode='{0}'
else insert into drug_info (drugonlycode{2}) values ('{0}'{3});";
                        sql_Dream += string.Format(p, row["drugonlycode"].ToString().Trim(), str_Update, str_Insert_Column, str_Insert_Value);
                    }
                    if (!string.IsNullOrEmpty(sql_Dream))
                        csSql.ExecuteSql(sql_Dream, Config.Soft.ConnString);
                }
                else
                    csMsg.ShowInfo("无更新数据", false);
            }
            else
                csMsg.ShowWarning("查询出错", false);
        }

        private void ReceivePresc()
        {
            string sql_His = @"select * from view_interface_recipejz
where state='1' and to_char(sysdate,'yyyy-mm-dd')=to_char(paytime,'yyyy-mm-dd') 
and to_char(sysdate,'yyyy-mm-dd HH24:MI:SS')<=to_char(paytime+(1/24/60)*{0},'yyyy-mm-dd HH24:MI:SS') order by paytime";
            sql_His = string.Format(sql_His, Config.Sys.EnableTime);
            DataTable dt_Presc;
            csOracle.ExecuteSelect(sql_His, Config.Soft.ConnString_His, out dt_Presc);
            if (dt_Presc != null)
            {
                if (dt_Presc.Rows.Count > 0)
                {
                    foreach (DataRow row in dt_Presc.Rows)
                    {
                        string sql_Dream = "";

                        string mrNo = row["mrno"].ToString().Trim();
                        string prescNo = row["prescno"].ToString().Trim();
                        string payTime = row["paytime"].ToString().Trim();
                        //按病人发药
                        //判断处方主表病人是否已存在
                        string sql = "select * from Pat_prescInfo where prescno='{0}'";
                        sql = string.Format(sql, mrNo);
                        DataTable dtP;
                        csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtP);
                        if (dtP != null && dtP.Rows.Count > 0)
                        {
                        }
                        else //插入主表信息
                        {
                            string str_Insert_Column_Presc = ""; string str_Insert_Value_Presc = "";

                            #region

                            str_Insert_Column_Presc += ",prescno_his";
                            str_Insert_Value_Presc += ",'" + prescNo + "'";

                            string s = "receiptno";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "windowno";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "mrno";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "patname";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "patsex";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "patage";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "ageunit";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "depcode";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "depname";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "docname";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "diagnosis";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "presctime";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "paytime";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            s = "dotype";
                            if (dt_Presc.Columns.Contains(s))
                            {
                                str_Insert_Column_Presc += "," + s;
                                str_Insert_Value_Presc += ",'" + row[s].ToString().Trim() + "'";
                            }
                            #endregion
                            string p = @"insert into pat_Prescinfo (prescno{0},doflag) values ('{1}'{2},'W');";
                            sql_Dream = string.Format(p, str_Insert_Column_Presc, mrNo, str_Insert_Value_Presc);
                        }
                        //判断处方明细表处方是否存在
                        sql = "select * from Pat_drugInfo where prescno_his='{0}'";
                        sql = string.Format(sql, prescNo);
                        DataTable dtD;
                        csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dtD);
                        if (dtD != null && dtD.Rows.Count > 0)
                        {
                        }
                        else
                        {
                            sql_His = "select * from view_interface_recipeinfojz where prescno='{0}'";
                            sql_His = string.Format(sql_His, prescNo);
                            DataTable dt1 = new DataTable();
                            DataTable dt2 = new DataTable();
                            csOracle.ExecuteSelect(sql_His, Config.Soft.ConnString_His, out dt2);
                            do
                            {
                                Thread.Sleep(20);
                                csOracle.ExecuteSelect(sql_His, Config.Soft.ConnString_His, out dt1);
                                Thread.Sleep(20);
                                csOracle.ExecuteSelect(sql_His, Config.Soft.ConnString_His, out dt2);
                            } while (dt1.Rows.Count != dt2.Rows.Count);

                            if (dt2.Rows.Count > 0)
                            {
                                sql = "update pat_prescinfo set paytime='{0}',hisflag='0' where prescno='{1}'";
                                sql = string.Format(sql, payTime, mrNo);
                                csSql.ExecuteSql(sql, Config.Soft.ConnString);

                                foreach (DataRow r in dt2.Rows)
                                {
                                    string str_Insert_Column_Drug = ""; string str_Insert_Value_Drug = "";
                                    #region

                                    str_Insert_Column_Drug += ",prescno_his";
                                    str_Insert_Value_Drug += ",'" + prescNo + "'";

                                    string s = "drugonlycode";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "drugnum";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "drugunit";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "mode1";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += ",mode";
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "frequency";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "dosage";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    s = "dosageunit";
                                    if (dt2.Columns.Contains(s))
                                    {
                                        str_Insert_Column_Drug += "," + s;
                                        str_Insert_Value_Drug += ",'" + r[s].ToString().Trim() + "'";
                                    }
                                    #endregion
                                    string p = @"insert into pat_Druginfo (prescno{0},doflag) values ('{1}'{2},'N');";
                                    sql_Dream += string.Format(p, str_Insert_Column_Drug, mrNo, str_Insert_Value_Drug);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(sql_Dream))
                            csSql.ExecuteSql(sql_Dream, Config.Soft.ConnString);
                    }
                }
                //else
                //    MsgChanged("无更新数据");
            }
            else
                csMsg.ShowWarning("查询出错", false);
        }

        DispatcherTimer timer_Drug = new DispatcherTimer();
        DispatcherTimer timer_Presc = new DispatcherTimer();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer_Drug.Interval = TimeSpan.FromMinutes(1);
            timer_Drug.Tick += new EventHandler(timer_Drug_Tick);

            timer_Presc.Interval = TimeSpan.FromSeconds(30);
            timer_Presc.Tick += new EventHandler(timer_Presc_Tick);
        }
        void timer_Drug_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.ToString("HH:mm") == Config.Sys.DrugTime)
                UpdateDrug("");
        }
        void timer_Presc_Tick(object sender, EventArgs e)
        {
            ReceivePresc();
        }

        private void cmdUpdate_Drug_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            UpdateDrug("");
            Cursor = null;
        }

        private void cmdUpdate_Presc_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            ReceivePresc();
            Cursor = null;
        }

        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            timer_Presc.Start();
        }
        private void btStop_Click(object sender, RoutedEventArgs e)
        {
            timer_Presc.Stop();
        }
    }
}
