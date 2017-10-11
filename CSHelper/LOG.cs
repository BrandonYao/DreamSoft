using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CSHelper
{
    public class LOG
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="log">日志内容</param>
        public void WriteLog(string log)
        {
            string dirPath = Application.StartupPath + @"\Log";
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            string fileName = DateTime.Now.ToString("yyyyMMddHH");
            string filePath = dirPath + @"\" + fileName + ".log";
            if (!File.Exists(filePath))
                File.Create(filePath).Close();
            log = "----------" + DateTime.Now.ToString() + "----------\r\n" +
                log +
                "\r\n---------------------------------------\r\n\r\n\r\n";
            File.AppendAllText(filePath, log);
        }
    }
}
