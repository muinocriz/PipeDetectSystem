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
            AssignCommands();
            InitCombbox();
            InitWork();
            TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            ProgV = Visibility.Collapsed;
            errorMsg = "";
        }

        public BackgroundWorker worker;
        public NamedPipeServerStream pipeReader;
        public string errorMsg = "";
        public int VideoId = 0;

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
        /// <summary>
        /// 视频地址
        /// </summary>
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
        /// <summary>
        /// 分帧目的地址
        /// </summary>
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

        #region command

        public RelayCommand CloingCmd { get; private set; }
        public RelayCommand<string> OpenFileDialogCmd { get; private set; }
        public RelayCommand<string> FolderBrowserDialogCmd { get; private set; }
        public RelayCommand FrameCmd { get; private set; }
        #endregion

        private void ExecuteCloingCmd()
        {
            Console.WriteLine("ExecuteCloingCmd");
            if (pipeReader != null && pipeReader.IsConnected)
            {
                Console.WriteLine("pipeReader.Close");
                pipeReader.Close();
            }

            if (worker != null && worker.IsBusy)
            {
                Console.WriteLine("worker.CancelAsync");
                worker.CancelAsync();
            }
        }

        private void ExecuteOpenFileDialogCmd(string p)
        {
            string filter = @"视频文件|*.avi;*.mp4;*.wmv;*.mpeg|所有文件|*.*";
            SourcePath = FileDialogService.GetService().OpenFileDialog(filter);
        }

        private void ExecuteFolderBrowserDialogCmd(string p)
        {
            TargetPath = FileDialogService.GetService().OpenFolderBrowserDialog();
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
            //检查文件是否已经导入
            int hasData = MetaService.GetService().HasVideoPath(SourcePath);
            if (hasData <= 0)
            {
                MessageBox.Show("该文件未导入，请重新选择");
                SourcePath = "";
                return;
            }
            //存储分帧间隔
            //根据模型路径，设置该模型的分帧间隔
            int result = MetaService.GetService().UpdateInterval(SourcePath, Convert.ToInt32(CombboxItem.Key));

            //分帧逻辑
            //使用cmd运行Python
            string pythonFilePosition = @"Util/testpipe.py";
            string cmdString = @"python " + pythonFilePosition + " " + sourcePath + " " + targetPath;
            CmdHelper.RunCmd(cmdString);

            //运行后台进程
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                pipeReader = new NamedPipeServerStream("cutfram_result1", PipeDirection.InOut);
                Console.WriteLine("字节读取管道正在连接...");
                pipeReader.WaitForConnection();
                Console.WriteLine("字节读取管道已连接");

                ProgV = Visibility.Visible;

                const int BUFFERSIZE = 256;
                int messageType = 0;
                bool completed = false;

                while (!completed)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        pipeReader.Close();
                        return;
                    }

                    byte[] buffer = new byte[BUFFERSIZE];
                    int nRead = 0;
                    try
                    {
                        nRead = pipeReader.Read(buffer, 0, BUFFERSIZE);
                    }
                    catch (Exception readE)
                    {
                        Console.WriteLine("读取管道发生异常");
                        Console.WriteLine(readE.ToString());
                        nRead = 0;
                    }

                    string line = string.Empty;
                    try
                    {
                        line = Encoding.UTF8.GetString(buffer, 0, nRead);
                    }
                    catch (Exception getE)
                    {
                        Console.WriteLine("转换string发生异常");
                        Console.WriteLine(getE.ToString());
                        continue;
                    }

                    Console.WriteLine("line: " + line);

                    if (string.IsNullOrEmpty(line))
                        continue;

                    string[] messages = line.Split('_');
                    int.TryParse(messages[0], out messageType);
                    switch (messageType)
                    {
                        case 0:
                            break;
                        case 1:
                            //收到进度
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
                                errorMsg += "\r\n消息：" + messages[1];
                            completed = true;
                            e.Cancel = true;
                            return;
                        case 64:
                            break;
                        default:
                            break;
                    }
                }
                pipeReader.Close();
            }
            catch (Exception pipeE)
            {
                Console.WriteLine("管道发生异常");
                Console.WriteLine(pipeE.ToString());
                pipeReader.Close();
            }

        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgV = Visibility.Collapsed;

            if (pipeReader.IsConnected)
            {
                pipeReader.Close();
            }

            if (e.Cancelled || e.Error != null || !string.IsNullOrEmpty(errorMsg))
            {
                MessageBox.Show("失败：" + errorMsg);
            }
            else
            {
                string framePathWithDirectory = Path.GetFileNameWithoutExtension(SourcePath);
                Console.WriteLine("framePathWithDirectory: " + framePathWithDirectory);
                string target = TargetPath + @"\" + framePathWithDirectory + @"\bigimg";
                int result = MetaService.GetService().UpdateFramePathByVideoPath(target, SourcePath);
                MessageBox.Show("分帧完成");
            }
            errorMsg = "";
        }

        #region helper function
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


        private void InitWork()
        {
            try
            {
                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            }
            catch (Exception e)
            {
                Console.WriteLine("后台任务初始化失败");
                Console.WriteLine(e.ToString());
            }
        }

        private void AssignCommands()
        {
            OpenFileDialogCmd = new RelayCommand<string>((str) => ExecuteOpenFileDialogCmd(str));
            FolderBrowserDialogCmd = new RelayCommand<string>((p) => ExecuteFolderBrowserDialogCmd(p));
            FrameCmd = new RelayCommand(() => ExecuteFrameCmd(), CanExecuteFrameCmd);
            CloingCmd = new RelayCommand(() => ExecuteCloingCmd());
        }

        #endregion

    }
}
