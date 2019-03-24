using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MvvmLight4.Common
{
    class IntToAbnormalConverter : IValueConverter
    {
        /// <summary>
        /// 异常转换
        /// </summary>
        /// <param name="value">异常值</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture">异常、无异常</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            string result;
            switch (value)
            {
                case 0:
                case 6:
                    result = "无异常";
                    break;
                default:
                    result = "异常";
                    break;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
