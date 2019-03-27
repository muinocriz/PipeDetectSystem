using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MvvmLight4.Common
{
    /// <summary>
    /// 时间转化类
    /// 帧号->时分秒
    /// </summary>
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int data = System.Convert.ToInt32(value);
            int totalSec = data / 25;//总秒数
            int hour = totalSec / 3600;
            int minute = (totalSec % 3600) / 60;
            int second = (totalSec % 3600) % 60;
            string time=string.Empty;
            if(hour!=0)
            {
                time += hour + "时";
            }
            time += minute + "分" + second + "秒";
            return time;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
