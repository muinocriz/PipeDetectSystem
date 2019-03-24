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
        /// <summary>
        /// 训练界面
        /// 装换学习率和检测次数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            { return null; }
            switch (System.Convert.ToInt32(parameter))
            {
                case 0:
                    if ((int)value < 1 || (int)value > 100)
                        return 1.0;
                    else
                        return System.Convert.ToDouble(value);
                case 1:
                    if ((int)value < 200000 || (int)value >1000000)
                        return 200000.0;
                    else
                        return System.Convert.ToDouble(value);
                default:
                    return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int data = System.Convert.ToInt32(value);
            return data;
        }
    }
}
