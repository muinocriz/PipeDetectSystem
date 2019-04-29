using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using MvvmLight4.Model;
using MvvmLight4.Service;
using MvvmLight4.View;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmLight4.ViewModel
{
    public class ImportViewModel : ViewModelBase
    {
        public ImportViewModel()
        {
            AssignCommands();

            Meta = new MetaModel();

            Messenger.Default.Register<string>(this, "HeadTime", GetHeadTime);
            Messenger.Default.Register<string>(this, "TailTime", GetTailTime);
        }

        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private void GetTailTime(string msg)
        {
            Meta.TailTime = msg;
        }

        private void GetHeadTime(string msg)
        {
            Meta.HeadTime = msg;
        }

        private MetaModel meta;
        /// <summary>
        /// 元数据
        /// </summary>
        public MetaModel Meta
        {
            get
            {
                return meta;
            }
            set
            {
                meta = value;
                RaisePropertyChanged(() => Meta);
            }
        }

        public RelayCommand SubmitCmd { get; private set; }

        private bool CanExecuteSubmitCmd()
        {
            return !(string.IsNullOrEmpty(Meta.VideoPath) || string.IsNullOrEmpty(Meta.PipeCode) || string.IsNullOrEmpty(Meta.TaskCode) || string.IsNullOrEmpty(Meta.Addr) || string.IsNullOrEmpty(Meta.Charge));
        }

        private void ExecuteSubmitCmd()
        {
            List<string> errorMsgs = new List<string>();

            string tag = Meta.PipeCode;
            if (tag.Split('-').Length != 2)
            {
                errorMsgs.Add(@"管线编号不正确,应为“起始井号-终止井号”");
            }

            string hTag = Meta.HeadTime;
            //如果hTag不为空，就要判断是否符合规则，否则直接退出
            if (!string.IsNullOrEmpty(hTag))
            {
                if (hTag.Split(':').Length != 2)
                {
                    errorMsgs.Add(@"开头时间格式不正确,应为：分分:秒秒（':'为英文冒号）");
                }
                else if (!(IsInt(hTag.Split(':')[0]) && IsInt(hTag.Split(':')[1])))
                {
                    errorMsgs.Add(@"输入的开头时间未能转换为整数");
                }
            }
            else
            {
                Meta.HeadTime = "None";
            }

            string tTag = Meta.TailTime;
            if (!string.IsNullOrEmpty(tTag))
            {
                if (tTag.Split(':').Length != 2)
                {
                    errorMsgs.Add(@"尾部时间格式不正确,应为：分分:秒秒（':'为英文冒号）");
                }
                else if (!(IsInt(tTag.Split(':')[0]) && IsInt(tTag.Split(':')[1])))
                {
                    errorMsgs.Add(@"输入的尾部时间未能转换为整数");
                }
            }
            else
            {
                Meta.TailTime = "None";
            }

            string msg = string.Empty;
            if (errorMsgs != null && errorMsgs.Count() > 0)
            {
                for (int i = 0; i < errorMsgs.Count(); i++)
                {
                    msg = msg + "\n" + (i + 1) + " " + errorMsgs[i];
                }
                MessageBox.Show(msg);
                return;
            }

            int result = MetaService.GetService().InsertData(Meta);
            if (result == 1)
            {
                MessageBox.Show("导入成功");
                log.Info(JsonConvert.SerializeObject(Meta));
                CleanMetaWhenImported();
            }
        }

        /// <summary>
        /// 打开文件选择对话框
        /// </summary>
        public RelayCommand OpenFileDialogCmd { get; private set; }

        private void ExecuteOpenFileDialogCmd()
        {
            string filter = @"视频文件|*.avi;*.mp4;*.wmv;*.mpeg|所有文件|*.*";
            Meta.VideoPath = FileDialogService.GetService().OpenFileDialog(filter);
        }

        /// <summary>
        /// 打开视频首位标注对话框
        /// </summary>
        public RelayCommand OpenVideoCmd { get; private set; }

        private void ExecuteOpenVideoCmd()
        {
            string videoPath = Meta.VideoPath;
            if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
            {
                MessageBox.Show("视频文件路径出错");
                return;
            }
            VideoPlayWindow window = new VideoPlayWindow();
            Messenger.Default.Send<string>(Meta.VideoPath, "VideoPlay");
            window.ShowDialog();

        }

        private bool CanExecuteOpenVideoCmd()
        {
            return !string.IsNullOrEmpty(Meta.VideoPath);
        }

        #region helper function
        private void AssignCommands()
        {
            OpenFileDialogCmd = new RelayCommand(() => ExecuteOpenFileDialogCmd());
            SubmitCmd = new RelayCommand(() => ExecuteSubmitCmd(), CanExecuteSubmitCmd);
            OpenVideoCmd = new RelayCommand(() => ExecuteOpenVideoCmd(), CanExecuteOpenVideoCmd);
        }


        public static bool IsInt(string value)
        {
            Console.WriteLine("value:" + value);
            try
            {
                Convert.ToInt32(value);
                Console.WriteLine("true");
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("false");
                return false;
            }
        }

        /// <summary>
        /// 重置TextBox
        /// </summary>
        private void CleanMetaWhenImported()
        {
            Meta.VideoPath = "";
            Meta.PipeCode = "";
            Meta.TaskCode = "";
            Meta.GC = "";
            Meta.Addr = "";
            Meta.Charge = "";
            Meta.StartTime = DateTime.Now.ToString();
            Meta.HeadTime = "";
            Meta.TailTime = "";
        }
        #endregion
    }

}