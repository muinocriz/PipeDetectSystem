using MvvmLight4.Model;
using MvvmLight4.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
            ss = new string[6];
        }
        string[] ss;
        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            var data = VideoManageDG.SelectedItem as MetaViewModel;
            ss[0] = data.Meta.TaskCode;
            ss[1] = data.Meta.Addr;
            ss[2] = data.Meta.PipeCode;
            ss[3] = data.Meta.VideoPath;
            ss[4] = data.Meta.GC;
            ss[5] = data.Meta.FramePath;
            VideoFlyout.IsOpen = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            var data = VideoManageDG.SelectedItem as MetaViewModel;
            data.Meta.TaskCode = ss[0];
            data.Meta.Addr = ss[1];
            data.Meta.PipeCode = ss[2];
            data.Meta.VideoPath = ss[3];
            data.Meta.GC = ss[4];
            data.Meta.FramePath = ss[5];
            VideoFlyout.IsOpen = false;

        }

        private void CheckBtn_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(300);
            VideoFlyout.IsOpen = false;
        }
    }
}
