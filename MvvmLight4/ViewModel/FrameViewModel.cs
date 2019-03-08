using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmLight4.Common;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MvvmLight4.ViewModel
{
    public class FrameViewModel : ViewModelBase
    {
        public FrameViewModel()
        {
            InitCombbox();
            TargetPath= Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            ProgV = Visibility.Collapsed;
            errorMsg = "";
        }
        public BackgroundWorker worker;
        public NamedPipeServerStream pipeReader;
        public string errorMsg = "";

        public int VideoId=0;

        private Visibility progV;
        //进度条可视状态
        public Visibility ProgV
        {
            get
            {
                return progV;
            }
            set
            {
                progV = value;
                RaisePropertyChanged(() => ProgV);
            }
        }

        private string sourcePath;
        public string SourcePath
        {
            get
            {
                return sourcePath;
            }
            set
            {
                sourcePath = value;
                RaisePropertyChanged(() => SourcePath);
            }
        }
        private string targetPath;
        public string TargetPath
        {
            get
            {
                return targetPath;
            }
            set
            {
                targetPath = value;
                RaisePropertyChanged(() => TargetPath);
            }
        }

        private List<ComplexInfoModel> combboxList;
        /// <summary>
        /// 下拉框列表
        /// </summary>
        public List<ComplexInfoModel> CombboxList
        {
            get { return combboxList; }
            set { combboxList = value; RaisePropertyChanged(() => CombboxList); }
        }
        private ComplexInfoModel combboxItem;
        /// <summary>
        /// 下拉框选中信息
        /// </summary>
        public ComplexInfoModel CombboxItem
        {
            get { return combboxItem; }
            set { combboxItem = value; RaisePropertyChanged(() => CombboxItem); }
        }
        private RelayCommand<string> openFileDialogCmd;
        public RelayCommand<string> OpenFileDialogCmd
        {
            get
            {
                if (openFileDialogCmd == null)
                    return new RelayCommand<string>((p) => ExecuteOpenFileDialogCmd(p));
                return openFileDialogCmd;
            }
            set
            {
                OpenFileDialogCmd = value;
            }
        }

        private void ExecuteOpenFileDialogCmd(string p)
        {
            string filter = @"视频文件|*.avi;*.mp4;*.wmv;*.mpeg|所有文件|*.*"; 
            SourcePath = FileDialogService.GetService().OpenFileDialog(filter);
        }

        private RelayCommand<string> folderBrowserDialogCmd;
        public RelayCommand<string> FolderBrowserDialogCmd
        {
            get
            {
                if (folderBrowserDialogCmd == null)
                    return new RelayCommand<string>((p) => ExecuteFolderBrowserDialogCmd(p));
                return folderBrowserDialogCmd;
            }
            set
            {
                FolderBrowserDialogCmd = value;
            }
        }

        private void ExecuteFolderBrowserDialogCmd(string p)
        {
            TargetPath = FileDialogService.GetService().OpenFolderBrowserDialog();
        }
        private RelayCommand frameCmd;
        public RelayCommand FrameCmd
        {
            get
            {
                if (frameCmd == null)
                    return new RelayCommand(() => ExecuteFrameCmd(), CanExecuteFrameCmd);
                return frameCmd;
            }
            set
            {
                FrameCmd = value;
            }
        }

        private bool CanExecuteFrameCmd()
        {
            return !string.IsNullOrEmpty(SourcePath) && !string.IsNullOrEmpty(TargetPath);
        }

        /// <summary>
        /// 存储分帧间隔
        /// 开始分帧
        /// </summary>
        private void ExecuteFrameCmd()
        {
            int hasData = MetaService.GetService().HasVideoPath(SourcePath);
            if(hasData<=0)
            {
                MessageBox.Show("该文件未导入，请重新选择");
                SourcePath = "";
                return;
            }
            //存储分帧间隔
            //根据模型路径，设置该模型的分帧间隔
            int result = MetaService.GetService().UpdateInterval(SourcePath, Convert.ToInt32(CombboxItem.Key));

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Tick += new EventHandler(Timer_Tick);

            //分帧逻辑
            //使用cmd运行Python
            string pythonFilePosition = @"Util/testpipe.py";
            string cmdString = @"python "+ pythonFilePosition + " " + sourcePath + " " + targetPath;
            //MessageBox.Show("cmdstring: " + cmdString);
            CmdHelper.RunCmd(cmdString);

            //新建后台进程
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            worker.RunWorkerAsync();
            timer.Start();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            using (pipeReader = new NamedPipeServerStream("cutfram_result1", PipeDirection.InOut))
            {
                Console.WriteLine("字节读取管道正在连接...");
                pipeReader.WaitForConnection();
                Console.WriteLine("字节读取管道已连接");
                
                ProgV = Visibility.Visible;

                int progress = 0;
                const int BUFFERSIZE = 256;
                int messageType = 0;
                bool completed = false;

                while (!completed)
                {
                    if (worker.CancellationPending)
                    {
                        errorMsg = @"管道未能正确连接";
                        e.Cancel = true;
                        return;
                    }
                    byte[] buffer = new byte[BUFFERSIZE];
                    int nRead = pipeReader.Read(buffer, 0, BUFFERSIZE);
                    string line = Encoding.UTF8.GetString(buffer, 0, nRead);
                    Console.WriteLine("line: " + line);
                    string[] messages = line.Split('_');
                    int.TryParse(messages[0], out messageType);
                    switch (messageType)
                    {
                        case 0:
                            break;
                        case 1:
                            //收到进度
                            if (messages.Length == 2 && int.TryParse(messages[1], out progress))
                            {
                                Console.WriteLine(messages[1]);
                                worker.ReportProgress(progress);
                            }
                            break;
                        case 2:
                            break;
                        case 4:
                            //普通消息
                            if (messages.Length == 2 && "Done".Equals(messages[1]))
                            {
                                completed = true;
                            }
                            break;
                        case 8:
                            break;
                        case 16:
                            break;
                        case 32:
                            errorMsg = "本次任务由于后台而中断";
                            if (messages.Length > 1)
                                errorMsg +="\r\n消息：" + messages[1];
                            e.Cancel = true;
                            return;
                        case 64:
                            break;
                        default:
                            break;
                    }
                }
                pipeReader.Disconnect();
                pipeReader.Close();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("进入计时器");
            DispatcherTimer _timer = (DispatcherTimer)sender;
            _timer.Stop();
            if (!pipeReader.IsConnected)
            {
                Console.WriteLine("调用管道失败");
                errorMsg = "调用管道失败";
                NamedPipeClientStream npcs = new NamedPipeClientStream("cutfram_result1");
                npcs.Connect();
                worker.CancelAsync();
            }
            else
                Console.WriteLine("调用管道成功");
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            return;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgV = Visibility.Collapsed;
            pipeReader.Close();

            if(e.Cancelled || e.Error!=null || !string.IsNullOrEmpty(errorMsg))
            {
                MessageBox.Show(errorMsg);
            }
            else
            {
                string framePathWithDirectory = Path.GetFileNameWithoutExtension(SourcePath);
                Console.WriteLine("framePathWithDirectory: " + framePathWithDirectory);
                int result = MetaService.GetService().UpdateFramePathByVideoPath(TargetPath + @"\" + framePathWithDirectory + @"\bigimg", SourcePath);
                MessageBox.Show("分帧完成");
            }
            errorMsg = "";
        }
        #region 辅助函数
        /// <summary>
        /// 填充下拉框
        /// </summary>
        private void InitCombbox()
        {
            CombboxList = new List<ComplexInfoModel>() {
              new ComplexInfoModel(){ Key="5",Text="1/5" },
              new ComplexInfoModel(){ Key="10",Text="1/10" },
              new ComplexInfoModel(){ Key="20",Text="1/20" },
              new ComplexInfoModel(){ Key="25",Text="1/25" },
            };
            CombboxItem = CombboxList[3];
        }
        #endregion

    }
}
