using GalaSoft.MvvmLight.Messaging;
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
    /// BackTrackWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BackTrackWindow
    {
        public BackTrackWindow()
        {
            InitializeComponent();
            IsNormal = 0;
            this.Unloaded += (sender, e) => Messenger.Default.Unregister(this);
        }
        #region 全屏处理
        /// <summary>
        /// 是否全屏
        /// </summary>
        public int IsNormal { get; set; }

        /// <summary>
        /// 点击全屏按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            ChangeScreen();
        }

        /// <summary>
        /// 全屏
        /// </summary>
        private void ChangeScreen()
        {
            DataGrid.Visibility = Visibility.Collapsed;
            ControlSP.Visibility = Visibility.Collapsed;
            DetailSP.Visibility = Visibility.Collapsed;
            MainWin.WindowState = WindowState.Maximized;
            MainWin.WindowStyle = WindowStyle.None;
            MainWin.ShowTitleBar = false;
            MainWin.ShowMinButton = false;
            MainWin.ShowMaxRestoreButton = false;
            MainWin.ShowCloseButton = false;
            grid.Width = SystemParameters.PrimaryScreenWidth;
            grid.Height = SystemParameters.PrimaryScreenHeight;
            IsNormal = 1;
        }

        private void MainWin_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BackToNormal();
        }

        /// <summary>
        /// 全屏->默认
        /// </summary>
        private void BackToNormal()
        {
            DataGrid.Visibility = Visibility.Visible;
            ControlSP.Visibility = Visibility.Visible;
            DetailSP.Visibility = Visibility.Visible;
            MainWin.WindowStyle = WindowStyle.SingleBorderWindow;
            MainWin.WindowState = WindowState.Normal;
            MainWin.ShowTitleBar = true;
            MainWin.ShowMinButton = true;
            MainWin.ShowMaxRestoreButton = true;
            MainWin.ShowCloseButton = true;
            grid.Width = 400;
            grid.Height = 300;
            IsNormal = 0;
        }

        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsNormal == 1 && e.Key == Key.Escape)
                BackToNormal();
        }
        #endregion

    }
}
