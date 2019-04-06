using MvvmLight4.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    /// VideoManageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VideoManageWindow
    {
        public VideoManageWindow()
        {
            InitializeComponent();
        }

        MetaViewModel copyData;

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            copyData = VideoManageDG.SelectedItem as MetaViewModel;
            //MessageBox.Show(Convert.ToString(data.Id));
            VideoFlyout.IsOpen = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            //VideoManageDG.SelectedItem = copyData;
            VideoFlyout.IsOpen = false;
        }
    }
}
