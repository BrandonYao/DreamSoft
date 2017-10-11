using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace CSHelper
{
    public class COMM
    {
        /// <summary>
        /// 判断字符串是否为数字型
        /// </summary>
        /// <param name="message">字符串</param>
        /// <returns></returns>
        public bool isNumberic(string message)
        {
            System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^-*\d+\.?\d*$");
            if (rex.IsMatch(message))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 设定是否开机启动
        /// </summary>
        /// <param name="started">是否开机启动</param>
        /// <param name="exeName">节点名</param>
        /// <param name="path">程序路径</param>
        /// <returns></returns>
        public bool RunWhenStart(bool started, string exeName, string path)
        {
            RegistryKey key = null;
            try
            {
                //打开注册表子项 
                key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                //如果该项不存在的话，则创建该子项 
                if (key == null)
                {
                    key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                }
                if (started == true)
                {
                    key.SetValue(exeName, path);//设置为开机启动 
                    key.Close();
                }
                else
                {
                    object obj = key.GetValue(exeName);
                    if (obj != null)
                    {
                        key.DeleteValue(exeName);//取消开机启动 
                        key.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                new Msg().ShowWarning(ex.Message, true);
                return false;
            }
            finally
            {
                if (key != null)
                    key.Close();
            }
        }
    }
}
