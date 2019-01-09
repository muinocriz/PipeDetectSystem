using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MvvmLight4.Common
{
    /// <summary>
    /// 颜色修改器
    /// 异常->红色
    /// 无异常->不变
    /// </summary>
    class BGConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int data = (int)value;
            if (data != 0)
                return Brushes.LightCoral;
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#70C5E9"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
