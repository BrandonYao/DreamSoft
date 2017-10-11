using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;
using System.Drawing;
using System.IO;

namespace DreamSoft
{
    /// <summary>
    /// WinZKFinger.xaml 的交互逻辑
    /// </summary>
    public partial class WinZKFinger : Window
    {
        CSHelper.SQL csSql = new CSHelper.SQL();
        AxZKFPEngXControl.AxZKFPEngX zkFinger;

        public WinZKFinger()
        {
            InitializeComponent();

            WindowsFormsHost host = new WindowsFormsHost();
            zkFinger = new AxZKFPEngXControl.AxZKFPEngX();
            zkFinger.OnImageReceived +=new AxZKFPEngXControl.IZKFPEngXEvents_OnImageReceivedEventHandler(zkFinger_OnImageReceived);
            zkFinger.OnFeatureInfo +=new AxZKFPEngXControl.IZKFPEngXEvents_OnFeatureInfoEventHandler(zkFinger_OnFeatureInfo);
            zkFinger.OnEnroll +=new AxZKFPEngXControl.IZKFPEngXEvents_OnEnrollEventHandler(zkFinger_OnEnroll);
            zkFinger.OnCapture += new AxZKFPEngXControl.IZKFPEngXEvents_OnCaptureEventHandler(zkFinger_OnCapture);

            host.Child = zkFinger;
            grid1.Children.Add(host);
        }
        public string userCode;
        public string funType;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            zkFinger.FakeFunOn = 0;
            zkFinger.FPEngineVersion = "9";
            //Cursor = Cursors.Wait;
            int i = zkFinger.InitEngine();
            //Cursor = null;
            switch(i)
            {
                case 0:
                tbIni.Text = "初始化成功,请按指纹";
                if (funType == "R")
                {
                    //int fpcHandle;
                    //fpcHandle = zkFinger.CreateFPCacheDBEx();
                    //zkFinger.AddRegTemplateStrToFPCacheDB(fpcHandle, 0, "1");
                    //int score=9; int num=0;
                    //zkFinger.IdentificationFromStrInFPCacheDB(fpcHandle, "1", ref score, ref num);
                    if (zkFinger.IsRegister)
                    {
                        zkFinger.CancelEnroll();
                    }
                    tbReg.Visibility = tbNum.Visibility = Visibility.Visible;
                    zkFinger.EnrollCount = 3;
                    zkFinger.BeginEnroll();
                }
                else
                {
                    if (zkFinger.IsRegister)
                    {
                        zkFinger.CancelEnroll();
                    }
                    tbReg.Visibility = tbNum.Visibility = Visibility.Collapsed;
                }
                    break;
                case 1:
                    tbIni.Text = "指纹识别驱动程序加载失败";
                    break;
                case 2:
                    tbIni.Text = "没有连接指纹识别器";
                    break;
                case 3:
                    tbIni.Text = "属性SensorIndex指定的指纹仪不存在";
                    break;
            }
        }

        public void zkFinger_OnImageReceived(object sender, AxZKFPEngXControl.IZKFPEngXEvents_OnImageReceivedEvent e)
        {
            object img = new object();
            zkFinger.GetFingerImage(ref img);
            byte[] bytes = img as byte[];
            BitmapImage bmp = ByteArrayToBitmapImage(bytes);
            imageFinger.Source = bmp;
        }
        public BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            BitmapImage bmp = null;
            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(byteArray);
                bmp.EndInit();
            }
            catch
            {
                bmp = null;
            }
            return bmp;
        }

        public void zkFinger_OnFeatureInfo(object sender, AxZKFPEngXControl.IZKFPEngXEvents_OnFeatureInfoEvent e)
        {
            if (zkFinger.IsRegister)
                tbNum.Text = (4 - zkFinger.EnrollIndex).ToString() + "/3";
            switch (e.aQuality)
            {
                case 0:
                    tbStatus.Text = "指纹质量好";
                    break;
                case 1:
                    tbStatus.Text = "指纹特征点不够";
                    break;
                case 2:
                    tbStatus.Text = "不能取到指纹";
                    break;
                case -1:
                    tbStatus.Text = "可疑指纹";
                    break;
            }
        }

        public void zkFinger_OnEnroll(object sender, AxZKFPEngXControl.IZKFPEngXEvents_OnEnrollEvent e)
        {
            if (e.actionResult)
            {
                //object s = zkFinger.GetTemplate();
                string s = zkFinger.GetTemplateAsString();
                string sql = "update sys_user set finger='{1}' where usercode='{0}'";
                sql = string.Format(sql, userCode, s);
                csSql.ExecuteSql(sql, Config.Soft.ConnString);
                tbStatus.Text = "注册成功";
            }
            else
            {
                tbStatus.Text = "注册失败";
            }
        }

        public void zkFinger_OnCapture(object sender, AxZKFPEngXControl.IZKFPEngXEvents_OnCaptureEvent e)
        {
            string sTemp = zkFinger.GetTemplateAsString();
            string sVerTemplate;
            string sql = "select finger from sys_user where usercode='{0}'";
            sql = string.Format(sql, userCode);
            csSql.ExecuteScalar(sql, Config.Soft.ConnString, out sVerTemplate);
            bool regChanged = false;
            if (zkFinger.VerFingerFromStr(ref sVerTemplate, sTemp, false, ref regChanged))
            {
                tbStatus.Text = "验证通过";
            }
            else
            {
                tbStatus.Text = "验证未通过";
            }          
        }
    }
}
