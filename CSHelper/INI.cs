using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSHelper
{
    public class INI
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public string ReadIni(string section, string key, string def, string file)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, def, temp, 255, file);
            return temp.ToString();
        }
        public void WriteIni(string section, string key, string value, string file)
        {
            WritePrivateProfileString(section, key, value, file);
        }
        public long DeleteSection(string section, string file)
        {
            return WritePrivateProfileString(section, null, null, file);
        }
        public long DeleteKey(string section, string key, string file)
        {
            return WritePrivateProfileString(section, key, null, file);
        }
    }
}
