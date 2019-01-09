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
    /// MarkFileChooseWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MarkFileChooseWindow
    {
        public MarkFileChooseWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<bool>(this, "CloseMarkFileChooseWindow", CloseWin);
            this.Unloaded += (sender, e) => Messenger.Default.Unregister(this);
        }

        private void CloseWin(bool b)
        {
            if (b == true)
                this.Close();
        }
    }
}
