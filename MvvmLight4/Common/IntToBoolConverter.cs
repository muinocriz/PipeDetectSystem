using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MvvmLight4.Common
{
    class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value==null)
            { return null; }
            int data = (int)value;
            //Console.WriteLine("data: " + data + "  parameter: " +  "   return: " + (data == System.Convert.ToInt32(parameter)));
            return data == System.Convert.ToInt32(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool data = (bool)value;
            if (!data)
            {
                return -1;
            }
            return System.Convert.ToInt32(parameter);
        }
    }
}
