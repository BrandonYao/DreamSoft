using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Data;

namespace DreamSoft
{
   public sealed class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = "";
            string t = value.GetType().ToString();
            switch (t)
            {
                case "DreamSoft.UCMachine+Mac":
                    UCMachine.Mac v1 = value as UCMachine.Mac;
                    s = v1.BackColor;
                    break;
                case "DreamSoft.UCWindow+Window":
                    UCWindow.Window v2 = value as UCWindow.Window;
                    s = v2.BackColor;
                    break;

                case "DreamSoft.UCOut_Manual+Drug":
                    UCOut_Manual.Drug v3 = value as UCOut_Manual.Drug;
                    s = v3.BackColor;
                    break;
                case "DreamSoft.UCOut_Auto+PC":
                    UCOut_Auto.PC v4 = value as UCOut_Auto.PC;
                    s = v4.BackColor;
                    break;
                case "DreamSoft.UCOut_Auto+PrescDetails":
                    UCOut_Auto.PrescDetails v5 = value as UCOut_Auto.PrescDetails;
                    s = v5.BackColor;
                    break;
                case "DreamSoft.UCPD+Pos":
                    UCPD.Pos v6 = value as UCPD.Pos;
                    s = v6.BackColor;
                    break;
                case "DreamSoft.UCAdd+Pos":
                    UCAdd.Pos v7 = value as UCAdd.Pos;
                    s = v7.BackColor;
                    break;
                case "DreamSoft.UCAuto+Drug":
                    UCAuto.Drug v8 = value as UCAuto.Drug;
                    s = v8.BackColor;
                    break;
                case "DreamSoft.UCAuto+Presc":
                    UCAuto.Presc v9 = value as UCAuto.Presc;
                    s = v9.BackColor;
                    break;
            }
            Color c = Colors.White;
            if (!string.IsNullOrEmpty(s))
            {
                c = Color.FromArgb(System.Convert.ToByte(s.Substring(1, 2), 16), System.Convert.ToByte(s.Substring(3, 2), 16), System.Convert.ToByte(s.Substring(5, 2), 16), System.Convert.ToByte(s.Substring(7, 2), 16));
            }
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
