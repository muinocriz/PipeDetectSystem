using GalaSoft.MvvmLight.Messaging;
using MvvmLight4.Model;
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
    /// FrameFileChooseWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FrameFileChooseWindow
    {
        public FrameFileChooseWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<String>(this, "FFCVM2FFCW", GetMsg);
            this.Unloaded += (sender, e) => Messenger.Default.Unregister(this);
        }



        private void GetMsg(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                switch (msg)
                {
                    case "closeWindow":
                        this.Close();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
