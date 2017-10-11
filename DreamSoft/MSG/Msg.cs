using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DreamSoft
{
    class Msg
    {
        public static void ShowMsg(string msg, Color c)
        {
            WinMsg w = WinMsg.GetMsg();
            w.ShowMsg(msg, c);
        }
    }
}
