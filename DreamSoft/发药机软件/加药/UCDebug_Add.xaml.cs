using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DreamSoft.Class;
using System.Threading;
using System.Data;

namespace DreamSoft
{
    /// <summary>
    /// WinDebug_Add.xaml 的交互逻辑
    /// </summary>
    public partial class UCDebug_Add : UserControl
    {
        CSHelper.Msg csMsg = new CSHelper.Msg();
        CSHelper.SQL csSql = new CSHelper.SQL();

        public delegate void SetKey(bool show);
        public static SetKey ShowKey;

        public UCDebug_Add()
        {
            InitializeComponent();

            cbUnitCode.Items.Clear();
            for (int i = 1; i <= Config.Mac_A.Count_Unit; i++)
            {
                cbUnitCode.Items.Add(i.ToString());
            } 
            cbLayerCode.Items.Clear();
            for (int i = 1; i <= Config.Mac_A.Count_Lay; i++)
            {
                cbLayerCode.Items.Add(i.ToString().PadLeft(2,'0'));
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            rbAdd.IsChecked = true; rbHandL.IsChecked = true;
            cbUnitCode.SelectedIndex = cbLayerCode.SelectedIndex = cbColumnCode.SelectedIndex = 0;
            
            tbNowX.Text = PLC_Tcp_AP.ReadExtramanPulseX().ToString();
            tbNowZ.Text = PLC_Tcp_AP.ReadExtramanPulseZ().ToString();
            tbNowL.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Left).ToString();
            tbNowR.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Right).ToString();

            tbMeetX.Text = Config.Mac_A.Pulse_Meet_X;
            tbMeetZ.Text = Config.Mac_A.Pulse_Meet_Z;
            tbPlatePulseMaxL.Text = Config.Mac_A.Pulse_Plate_Max_Left;
            tbPlatePulseMaxR.Text = Config.Mac_A.Pulse_Plate_Max_Right;
        }

        //显示配置脉冲
        private void ShowSavedPulse()
        {
            tbSaveX.Text = "";
            tbSaveZ.Text = "";

            //改变下拉框选项
            string dir = "";
            if ((bool)rbHandL.IsChecked)
            {
                dir= "L";
            }
            if ((bool)rbHandR.IsChecked)
            {
                dir = "R";
            }

            string u = cbUnitCode.Text;
            string l = cbLayerCode.Text;
            string c = cbColumnCode.Text;
            if (!string.IsNullOrEmpty(u) && !string.IsNullOrEmpty(l) && !string.IsNullOrEmpty(c))
            {
                string poscode = u + l + c;
                string type = "A";
                if ((bool)rbPD.IsChecked)
                    type = "P";
                string sql = "select pulsex,pulsez from pos_pulse where maccode='" + Config.Soft.MacCode + "' and poscode='" + poscode + "' and pulsetype='" + type + "' and pulselr='" + dir + "'";
                DataTable dt = new DataTable();
                csSql.ExecuteSelect(sql, Config.Soft.ConnString, out dt);
                if (dt.Rows.Count > 0)
                {
                    tbSaveX.Text = dt.Rows[0][0].ToString().Trim();
                    tbSaveZ.Text = dt.Rows[0][1].ToString().Trim();
                }
            }
        }
        //保存配置脉冲
        private void SavedPulse(int x, int z)
        {
            string u = cbUnitCode.Text.Trim();
            string l = cbLayerCode.Text.Trim();
            string c = cbColumnCode.Text.Trim();
            if (!string.IsNullOrEmpty(u) && !string.IsNullOrEmpty(l) && !string.IsNullOrEmpty(c))
            {
                string poscode = u + l + c;
                string type = "A";
                if ((bool)rbPD.IsChecked)
                    type = "P";
                string dir = "L";
                if ((bool)rbHandR.IsChecked)
                    dir = "R";
                string sql = @"if exists (select pulsex,pulsez from pos_pulse where maccode='{0}' and poscode='{1}' and pulsetype='{2}' and pulselr='{3}'
update pos_pulse set pulsex={4},pulsez={5} where maccode='{0}' and poscode='{1}' and pulsetype='{2}' and pulselr='{3}'
                else insert into pos_pulse (maccode,poscode,pulsetype,pulselr,pulsex,pulsez) values ('{0}','{1}','{2}','{3}',{4},{5})";
                sql = string.Format(sql, Config.Soft.MacCode, poscode, type, dir, x, z);
                if (csSql.ExecuteSql(sql, Config.Soft.ConnString))
                {
                    tbSaveX.Text = x.ToString();
                    tbSaveZ.Text = z.ToString();
                }
                else
                    csMsg.ShowWarning("保存失败", false);
            }
        }

        private void btZero_Extraman_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            if (PLC_Tcp_AP.ExtramanOriginReset())
            {
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(1000);
                while (!PLC_Tcp_AP.ExtramanOriginResetIsOK())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Reset_Extraman))
                        break;
                    Thread.Sleep(200);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Reset_Extraman))
                {
                    //关闭使能
                    //PLC.ExtramanOriginReset_OFF();
                    tbNowX.Text = PLC_Tcp_AP.ReadExtramanPulseX().ToString();
                    tbNowZ.Text = PLC_Tcp_AP.ReadExtramanPulseZ().ToString();
                    csMsg.ShowInfo("原点返回完成", false);
                }
            }
            else
                csMsg.ShowWarning("指令发送失败", false);
            Cursor = null;
        }

        #region"Z轴手动运行"
        //上升
        private void btExtraman_Manual_Up_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.ChangeAdd(0);
            PLC_Tcp_AP.ExtramanManualMove(PLC_Tcp_AP.ExtramanMoveDir.Up);
        }
        //下降
        private void btExtraman_Manual_Down_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.ChangeAdd(0);
            PLC_Tcp_AP.ExtramanManualMove(PLC_Tcp_AP.ExtramanMoveDir.Down);
        }
        //停止
        private void btExtraman_Manual_Z_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.ExtramanManualMove(PLC_Tcp_AP.ExtramanMoveDir.ZStop);
            Thread.Sleep(500);
            tbNowZ.Text = PLC_Tcp_AP.ReadExtramanPulseZ().ToString();
        }
        #endregion

        #region"X轴手动运行"
        private void btExtraman_Manual_Left_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.ChangeAdd(0);
            PLC_Tcp_AP.ExtramanManualMove(PLC_Tcp_AP.ExtramanMoveDir.Left);
        }
        //向右
        private void btExtraman_Manual_Right_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.ChangeAdd(0);
            PLC_Tcp_AP.ExtramanManualMove(PLC_Tcp_AP.ExtramanMoveDir.Right);
        }
        //停止
        private void btExtraman_Manual_X_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.ExtramanManualMove(PLC_Tcp_AP.ExtramanMoveDir.XStop);
            Thread.Sleep(500);
            tbNowX.Text = PLC_Tcp_AP.ReadExtramanPulseX().ToString();
        }
        #endregion

        //机械手运行到指定脉冲
        private void btRunAuto_Click(object sender, RoutedEventArgs e)
        {
            ShowKey(false);
            //csKey.Close();
            Cursor = Cursors.Wait;
            PLC_Tcp_AP.ChangeAdd(1);
            string x = tbTargetX.Text.Trim();
            string z = tbTargetZ.Text.Trim();
            if (string.IsNullOrEmpty(x))
            {
                csMsg.ShowWarning("X轴脉冲不能为空", false);
                Cursor = null;
                return;
            }
            if (string.IsNullOrEmpty(z))
            {
                csMsg.ShowWarning("Z轴脉冲不能为空", false);
                Cursor = null;
                return;
            }
            float xp; float zp;
            if (float.TryParse(x,out xp) && float.TryParse(z, out zp))
            {
                PLC_Tcp_AP.ExtramanAutoMoveToPulse(xp, zp);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(200);
                while (!PLC_Tcp_AP.ExtramanAutoMoveIsOK())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Extraman))
                        break;
                    Thread.Sleep(200);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Extraman))
                {
                    tbNowX.Text = PLC_Tcp_AP.ReadExtramanPulseX().ToString();
                    tbNowZ.Text = PLC_Tcp_AP.ReadExtramanPulseZ().ToString();
                }
                else
                    csMsg.ShowWarning("机械手未运行到指定位置", false);
            }
            else
                csMsg.ShowWarning("脉冲值格式不正确", false);

            Cursor = null;
        }

        //选择储位
        private void Pos_DropDownClosed(object sender, EventArgs e)
        {
            ShowSavedPulse();
        }

        //运行到保存位置
        private void btRunSave_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            PLC_Tcp_AP.ChangeAdd(1);
            string x = tbSaveX.Text.Trim();
            string z = tbSaveZ.Text.Trim();
            if (string.IsNullOrEmpty(x))
            {
                csMsg.ShowWarning("X轴脉冲不能为空", false);
                Cursor = null;
                return;
            }
            if (string.IsNullOrEmpty(z))
            {
                csMsg.ShowWarning("Z轴脉冲不能为空", false);
                Cursor = null;
                return;
            }
            float xp; float zp;
            if (float.TryParse(x, out xp) && float.TryParse(z, out zp))
            {
                PLC_Tcp_AP.ExtramanAutoMoveToPulse(xp, zp);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(200);
                while (!PLC_Tcp_AP.ExtramanAutoMoveIsOK())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Extraman))
                        break;
                    Thread.Sleep(200);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Extraman))
                {
                    tbNowX.Text = PLC_Tcp_AP.ReadExtramanPulseX().ToString();
                    tbNowZ.Text = PLC_Tcp_AP.ReadExtramanPulseZ().ToString();
                }
                else
                    csMsg.ShowWarning("机械手未运行到指定位置", false);
            }
            else
                csMsg.ShowWarning("脉冲值格式不正确", false);
            Cursor = null;
        }
        //保存配置脉冲
        private void btSavePulse_Click(object sender, RoutedEventArgs e)
        {
            string u = cbUnitCode.Text.Trim();
            string l = cbLayerCode.Text.Trim();
            string c = cbColumnCode.Text.Trim();
            if (!string.IsNullOrEmpty(u) && !string.IsNullOrEmpty(l) && !string.IsNullOrEmpty(c))
            {
                string poscode = u + l + c;
                string type = "A";
                if ((bool)rbPD.IsChecked)
                    type = "P";
                string dir = "L";
                if ((bool)rbHandR.IsChecked)
                    dir = "R";

                float x = PLC_Tcp_AP.ReadExtramanPulseX();
                float z = PLC_Tcp_AP.ReadExtramanPulseZ();

                string sql = @"if exists(select * from pos_pulse where maccode='{0}' and poscode='{1}' and pulsetype='{2}' and pulselr='{3}')
                update pos_pulse set pulsex={4},pulsez={5} where maccode='{0}' and poscode='{1}' and pulsetype='{2}' and pulselr='{3}'
                else insert into pos_pulse values('{0}','{1}','{2}','{3}',{4},{5})";
                sql = string.Format(sql, Config.Soft.MacCode, poscode, type, dir, x, z);
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
                //多个储位一起设定
                sql = "";
                if ((bool)chkRow.IsChecked)
                {
                    //同层一起设定Z轴脉冲
                    //所有单元
                    for (int unit = 1; unit <= Config.Mac_A.Count_Unit; unit++)
                    {
                        for (int col = 1; col <= Config.Mac_A.Count_Col; col++)
                        {
                            string pos = unit + poscode.Substring(1, 2) + col.ToString().PadLeft(2, '0');
                            string s = @"if exists(select * from pos_pulse where maccode='{0}' and poscode='{1}' and pulsetype='{2}' and pulselr='{3}')
                update pos_pulse set pulsez={4} where maccode='{0}' and poscode='{1}' and pulsetype='{2}' and pulselr='{3}'
                else insert into pos_pulse values('{0}','{1}','{2}','{3}',null,{4});";
                            sql += string.Format(s, Config.Soft.MacCode, pos, type, dir, z);
                        }
                    }
                    csSql.ExecuteSql(sql, Config.Soft.ConnString);
                }
                if ((bool)chkCol.IsChecked)
                {
                    //同列一起设定X轴脉冲 
                    for (int row = 1; row <= Config.Mac_A.Count_Lay; row++)
                    {
                        string pos = poscode.Substring(0, 1) + row.ToString().PadLeft(2, '0') + poscode.Substring(3, 2);
                        string s = @"if exists(select * from pos_pulse where maccode='{0}' and poscode='{1}' and pulsetype='{2}' and pulselr='{3}')
                update pos_pulse set pulsex={4} where maccode='{0}' and poscode='{1}' and pulsetype='{2}' and pulselr='{3}'
                else insert into pos_pulse values('{0}','{1}','{2}','{3}',{4},null);";
                        sql += string.Format(s, Config.Soft.MacCode, pos, type, dir, x);
                    }
                    csSql.ExecuteSql(sql, Config.Soft.ConnString);
                }
                tbSaveX.Text = x.ToString();
                tbSaveZ.Text = z.ToString();
            }
        }

        //运行到接药位置
        private void btRunMeet_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            PLC_Tcp_AP.ChangeAdd(1);
            string x = tbMeetX.Text.Trim();
            string z = tbMeetZ.Text.Trim();
            if (string.IsNullOrEmpty(x))
            {
                csMsg.ShowWarning("X轴脉冲不能为空", false);
                Cursor = null;
                return;
            }
            if (string.IsNullOrEmpty(z))
            {
                csMsg.ShowWarning("Z轴脉冲不能为空", false);
                Cursor = null;
                return;
            }
            float xp; float zp;
            if (float.TryParse(x, out xp) && float.TryParse(z, out zp))
            {
                PLC_Tcp_AP.ExtramanAutoMoveToPulse(xp, zp);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(200);
                while (!PLC_Tcp_AP.ExtramanAutoMoveIsOK())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Extraman))
                        break;
                    Thread.Sleep(200);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Extraman))
                {
                    tbNowX.Text = PLC_Tcp_AP.ReadExtramanPulseX().ToString();
                    tbNowZ.Text = PLC_Tcp_AP.ReadExtramanPulseZ().ToString();
                }
                else
                    csMsg.ShowWarning("机械手未运行到指定位置", false);
            }
            Cursor = null;
        }
        //保存接药位置脉冲
        private void btSaveMeet_Click(object sender, RoutedEventArgs e)
        {
            float x = PLC_Tcp_AP.ReadExtramanPulseX();
            Config.SaveConfig(Config.Soft.MacCode, "Pulse_Meet_X", x.ToString());
            tbMeetX.Text = x.ToString();
            float z = PLC_Tcp_AP.ReadExtramanPulseZ();
            Config.SaveConfig(Config.Soft.MacCode, "Pulse_Meet_Z", z.ToString());
            tbMeetZ.Text = z.ToString();
        }

        //推药板原点返回
        private void btZero_Plate_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            if (PLC_Tcp_AP.PlateOriginReset())
            {
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(1000);
                while (!PLC_Tcp_AP.PlateOriginResetIsOK())
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Reset_Plate))
                        break;
                    Thread.Sleep(200);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Reset_Plate))
                {
                    tbNowL.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Left).ToString();
                    tbNowR.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Right).ToString();
                    csMsg.ShowInfo("原点返回完成", false);
                }
            }
            else
                csMsg.ShowWarning("指令发送失败", false);
            Cursor = null;
        }

        private void btUp_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            PLC_Tcp_AP.ChangeAdd(2);

            //自动上推
            //PLC.PlateMoveUpBegin();
            PLC_Tcp_AP.PlateMoveUp(PLC_Tcp_AP.PlateType.Left);
            PLC_Tcp_AP.PlateMoveUp(PLC_Tcp_AP.PlateType.Right);
            DateTime timeBegin = DateTime.Now;
            Thread.Sleep(1000);
            while (!PLC_Tcp_AP.PlateMoveUpIsOK(PLC_Tcp_AP.PlateType.Left) || !PLC_Tcp_AP.PlateMoveUpIsOK(PLC_Tcp_AP.PlateType.Right))
            {
                if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Up_Plate))
                    break;
                Thread.Sleep(200);
            }
            if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Up_Plate))
            {
                tbNowL.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Left).ToString();
                tbNowR.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Right).ToString();
            }
            else
                csMsg.ShowWarning("推板上推失败", false);
            Cursor = null;
        }

        //推药板向上
        private void btPlateUpL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.ChangeAdd(0);
            PLC_Tcp_AP.PlateManualMove(PLC_Tcp_AP.PlateType.Left, PLC_Tcp_AP.PlateMoveDir.Up);
        }
        //推药板向下
        private void btPlateDownL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.ChangeAdd(0);
            PLC_Tcp_AP.PlateManualMove(PLC_Tcp_AP.PlateType.Left, PLC_Tcp_AP.PlateMoveDir.Down);
        }
        //推药板停止
        private void btPlateL_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.PlateManualMove(PLC_Tcp_AP.PlateType.Left, PLC_Tcp_AP.PlateMoveDir.Stop);
            Thread.Sleep(500);
            tbNowL.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Left).ToString();
        }
        //推药板自动运行
        private void btRunL_Click(object sender, RoutedEventArgs e)
        {
            ShowKey(false);
            //csKey.Close();
            Cursor = Cursors.Wait;
            PLC_Tcp_AP.ChangeAdd(1);
            string p = tbTargetL.Text.Trim();
            if (string.IsNullOrEmpty(p))
            {
                csMsg.ShowWarning("脉冲不能为空", false);
                Cursor = null;
                return;
            }
            float pp;
            if (float.TryParse(p,out pp))
            {
                PLC_Tcp_AP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Left, pp);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(200);
                while (!PLC_Tcp_AP.PlateAutoMoveToPulseIsOK(PLC_Tcp_AP.PlateType.Left))
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Plate))
                        break;
                    Thread.Sleep(200);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Plate))
                {
                    tbNowL.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Left).ToString();
                }
                else
                    csMsg.ShowWarning("推药板未运行到指定位置", false);
            }
            else
                csMsg.ShowWarning("脉冲格式不正确", false);
            Cursor = null;
        }

        //推药板向上
        private void btPlateUpR_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.ChangeAdd(0);
            PLC_Tcp_AP.PlateManualMove(PLC_Tcp_AP.PlateType.Right, PLC_Tcp_AP.PlateMoveDir.Up);
        }
        //推药板向下
        private void btPlateDownR_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.ChangeAdd(0);
            PLC_Tcp_AP.PlateManualMove(PLC_Tcp_AP.PlateType.Right, PLC_Tcp_AP.PlateMoveDir.Down);
        }
        //推药板停止
        private void btPlateR_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PLC_Tcp_AP.PlateManualMove(PLC_Tcp_AP.PlateType.Right, PLC_Tcp_AP.PlateMoveDir.Stop);
            Thread.Sleep(500);
            tbNowR.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Right).ToString();
        }
        //推药板自动运行
        private void btRunR_Click(object sender, RoutedEventArgs e)
        {
            ShowKey(false);
            //csKey.Close();
            Cursor = Cursors.Wait;
            PLC_Tcp_AP.ChangeAdd(1);
            string p = tbTargetR.Text.Trim();
            if (string.IsNullOrEmpty(p))
            {
                csMsg.ShowWarning("脉冲不能为空", false);
                Cursor = null;
                return;
            }
            float pp;
            if (float.TryParse(p, out pp))
            {
                PLC_Tcp_AP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Right, pp);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(200);
                while (!PLC_Tcp_AP.PlateAutoMoveToPulseIsOK(PLC_Tcp_AP.PlateType.Right))
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Plate))
                        break;
                    Thread.Sleep(200);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Plate))
                {
                    tbNowR.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Right).ToString();
                }
                else
                    csMsg.ShowWarning("推药板未运行到指定位置", false);
            }
            else
                csMsg.ShowWarning("脉冲格式不正确", false);
            Cursor = null;
        }

        private void btRun_Save_L_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            PLC_Tcp_AP.ChangeAdd(1);
            string p = tbPlatePulseMaxL.Text.Trim();
            if (string.IsNullOrEmpty(p))
            {
                csMsg.ShowWarning("脉冲不能为空", false);
                Cursor = null;
                return;
            }
            float pp;
            if (float.TryParse(p, out pp))
            {
                PLC_Tcp_AP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Left, pp);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(200);
                while (!PLC_Tcp_AP.PlateAutoMoveToPulseIsOK(PLC_Tcp_AP.PlateType.Left))
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Plate))
                        break;
                    Thread.Sleep(200);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Plate))
                {
                    tbNowL.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Left).ToString();
                }
                else
                    csMsg.ShowWarning("推药板未运行到指定位置", false);
            }
            else
                csMsg.ShowWarning("脉冲格式不正确", false);
            Cursor = null;
        }

        private void btSaveL_Click(object sender, RoutedEventArgs e)
        {
            float x = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Left);
            Config.SaveConfig(Config.Soft.MacCode, "Pulse_Plate_Max_Left", x.ToString());
            tbPlatePulseMaxL.Text = x.ToString();
        }

        private void btRun_Save_R_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            PLC_Tcp_AP.ChangeAdd(1);
            string p = tbPlatePulseMaxR.Text.Trim();
            if (string.IsNullOrEmpty(p))
            {
                csMsg.ShowWarning("脉冲不能为空", false);
                Cursor = null;
                return;
            }
            float pp;
            if (float.TryParse(p, out pp))
            {
                PLC_Tcp_AP.PlateAutoMoveToPulse(PLC_Tcp_AP.PlateType.Right, pp);
                DateTime timeBegin = DateTime.Now;
                Thread.Sleep(200);
                while (!PLC_Tcp_AP.PlateAutoMoveToPulseIsOK(PLC_Tcp_AP.PlateType.Right))
                {
                    if (DateTime.Now > timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Plate))
                        break;
                    Thread.Sleep(200);
                }
                if (DateTime.Now <= timeBegin.AddSeconds(Config.Mac_A.WaitTime_Auto_Plate))
                {
                    tbNowR.Text = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Right).ToString();
                }
                else
                    csMsg.ShowWarning("推药板未运行到指定位置", false);
            }
            else
                csMsg.ShowWarning("脉冲格式不正确", false);
            Cursor = null;
        }

        private void btSaveR_Click(object sender, RoutedEventArgs e)
        {
            float x = PLC_Tcp_AP.ReadPlatePulse(PLC_Tcp_AP.PlateType.Right);
            Config.SaveConfig(Config.Soft.MacCode, "Pulse_Plate_Max_Right", x.ToString());
            tbPlatePulseMaxR.Text = x.ToString();
        }

        //读取激光测距
        private void btReadLaserL_Click(object sender, RoutedEventArgs e)
        {
            tbLaserLeft.Text = Laser.ReadLaserData(PLC_Tcp_AP.LaserType.Left).ToString();
        }
        private void btReadLaserR_Click(object sender, RoutedEventArgs e)
        {
            tbLaserRight.Text = Laser.ReadLaserData(PLC_Tcp_AP.LaserType.Right).ToString();
        }

        //打开激光
        private void btOpen_Click(object sender, RoutedEventArgs e)
        {
            PLC_Tcp_AP.LaserOn();
        }
        //关闭激光
        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            PLC_Tcp_AP.LaserOff();
        }

        private void tbTarget_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ShowKey(true);
            //csKey.Show(200, 300);
        }

        private void RB_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)rbHandL.IsChecked)
            {
                cbColumnCode.Items.Clear();
                for (int i = 1; i <= Config.Mac_A.MaxCol; i++)
                {
                    cbColumnCode.Items.Add(i.ToString().PadLeft(2, '0'));
                }
            }
            if ((bool)rbHandR.IsChecked)
            {
                cbColumnCode.Items.Clear();
                for (int i = Config.Mac_A.MinCol; i <= Config.Mac_A.Count_Col; i++)
                {
                    cbColumnCode.Items.Add(i.ToString().PadLeft(2, '0'));
                }
            }
            cbColumnCode.SelectedIndex = 0;

            ShowSavedPulse();
        }
    }
}
