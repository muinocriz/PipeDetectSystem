using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MvvmLight4.Common
{
    public class WayConverter : IValueConverter
    {
        /// <summary>
        /// 导出界面
        /// 导出方式转换器
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            int data = (int)value;
            return data == System.Convert.ToInt32(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isCheck = (bool)value;
            if(!isCheck)
            {
                return null;
            }
            return System.Convert.ToInt32(parameter);
        }
    }
}
