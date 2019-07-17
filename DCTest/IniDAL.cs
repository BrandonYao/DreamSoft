using System.Runtime.InteropServices;
using System.Text;

namespace DCTest
{
    public class IniDAL
    {
        public string file;
        [DllImport("kernel32", CharSet = CharSet.Unicode, BestFitMapping =false, ThrowOnUnmappableChar =true)]
        private static extern int WritePrivateProfileString(string section, string key, string value, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public IniDAL(string path)
        {
            file = path;
        }

        public string ReadIni(string section, string key, string def)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, def, temp, 255, file);
            return temp.ToString();
        }
        public void WriteIni(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, file);
        }
        public int DeleteSection(string section)
        {
            return WritePrivateProfileString(section, null, null, file);
        }
        public int DeleteKey(string section, string key)
        {
            return WritePrivateProfileString(section, key, null, file);
        }
    }
}
