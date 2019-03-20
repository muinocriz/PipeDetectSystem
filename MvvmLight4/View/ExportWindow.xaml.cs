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
    /// ExportWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExportWindow
    {
        public ExportWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<String>(this, "EVM2EV", GetMsg);
            this.Unloaded += (sender, e) => Messenger.Default.Unregister(this);
        }

        private void GetMsg(string msg)
        {
            if(!string.IsNullOrEmpty(msg))
            {
                switch (msg)
                {
                    case "exportIsRunning":
                        ExportBtn.IsEnabled = false;
                        ExportProg.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
