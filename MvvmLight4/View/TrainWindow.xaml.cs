using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MvvmLight4.View
{
    /// <summary>
    /// TrainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TrainWindow
    {
        public TrainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载已有模型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModelCB_Checked(object sender, RoutedEventArgs e)
        {
            SP.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 不加载已有模型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModelCB_Unchecked(object sender, RoutedEventArgs e)
        {
            SP.Visibility = Visibility.Hidden;
        }
    }
}
