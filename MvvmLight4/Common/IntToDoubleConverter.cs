using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MvvmLight4.Common
{
    class IntToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            { return null; }
            switch ((int)parameter)
            {
                case 0:
                    if ((int)value < 1000 || (int)value > 2000)
                        return (double)1500;
                    else
                        return (double)value;
                    break;
                case 1:
                    if ((int)value < 85 || (int)value > 98)
                        return (double)85;
                    else
                        return (double)value;
                    break;
                default:
                    break;
            }
            if ((int)value < 1000 || (int)value > 2000)
                return (double)1500;
            else
                return (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int data = System.Convert.ToInt32(value);
            return data;
        }
    }
}
