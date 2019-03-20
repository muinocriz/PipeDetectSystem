using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MvvmLight4.Common;
using MvvmLight4.Model;
using MvvmLight4.Service;
using MvvmLight4.View;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MvvmLight4.ViewModel
{
    public class FrameViewModel : ViewModelBase
    {
        public FrameViewModel()
        {
            AssignCommands();
            InitWork();
            InitData();
            DispatcherHelper.Initialize();
            Messenger.Default.Register<List<MetaModel>>(this, "FrameList", GetMsg);
        }


        #region property
        public BackgroundWorker worker;
        public NamedPipeServerStream pipeReader;
        public string errorMsg = "";
        public int VideoId = 0;
        public List<MetaModel> MetaList;

        private int progressValue;
        public int ProgressValue
        {
            get
            {
                return progressValue;
            }
            set
            {
                progressValue = value;
                RaisePropertyChanged(() => ProgressValue);
            }
        }
        private string btnContent;
        /// <summary>
        /// 打开分帧文件选择按钮的文本
        /// </summary>
        public string BtnContent
        {
            get
            {
                return btnContent;
            }
            set
            {
                btnContent = value;
                RaisePropertyChanged(() => BtnContent);
            }
        }

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
        #region useless
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
        #endregion
        #endregion

        #region command
        public RelayCommand CloingCmd { get; private set; }
        public RelayCommand<string> OpenFileDialogCmd { get; private set; }
        public RelayCommand FolderBrowserDialogCmd { get; private set; }
        public RelayCommand FrameCmd { get; private set; }
        public RelayCommand<Button> OpenFrameVideosCmd { get; private set; }
        public RelayCommand FrameListCmd { get; private set; }
        #endregion

        private void ExecuteFrameListCmd()
        {
            //Task<int> task = new Task<int>(() => MyTaskBig());
            //Task<int> task1 = new Task<int>(() => MyTaskSml());
            //task.Start();
            //task1.Start();
            //Task.WaitAll(task, task1);
            //int result = task.Result;
            //int result1 = task1.Result;
            //Debug.WriteLine("Task返回结果：{0}======={1}", result,result1);
            Messenger.Default.Send("frameIsRunning", "FVM2FV");
            worker.RunWorkerAsync();
        }

        private bool CanExecuteFrameListCmd()
        {
            return MetaList.Count > 0 && !string.IsNullOrEmpty(TargetPath);
        }

        private int MyTaskSml()
        {
            for (int i = 0; i < MetaList.Count; i++)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                string framePathWithDirectory = Path.GetFileNameWithoutExtension(MetaList[i].VideoPath);
                string target = TargetPath + @"\" + framePathWithDirectory + @"\smallimg";

                Directory.CreateDirectory(target);
                Cut(MetaList[i].VideoPath, target, 0);
                watch.Stop();
                TimeSpan timespan = watch.Elapsed;  //获取当前实例测量得出的总时间
                Debug.WriteLine("打开窗口代码执行时间：{0}(毫秒)", timespan.TotalMilliseconds);  //总毫秒数
            }
            return 0;
        }

        private int MyTaskBig()
        {
            for (int i = 0; i < MetaList.Count; i++)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                string framePathWithDirectory = Path.GetFileNameWithoutExtension(MetaList[i].VideoPath);
                string target = TargetPath + @"\" + framePathWithDirectory + @"\bigimg";

                Directory.CreateDirectory(target);
                Cut(MetaList[i].VideoPath, target, 1);
                watch.Stop();
                TimeSpan timespan = watch.Elapsed;  //获取当前实例测量得出的总时间
                Debug.WriteLine("打开窗口代码执行时间：{0}(毫秒)", timespan.TotalMilliseconds);  //总毫秒数
                MetaService.GetService().UpdateFramePathByVideoPath(target, MetaList[i].VideoPath);
            }
            return 0;
        }

        private void Cut(string videoPath, string target, int fommat)
        {
            Process p = new Process();
            string command;
            switch (fommat)
            {
                case 0:
                    //大图
                    command = "-i" + " " + videoPath + " " + "-q:v 2 " + target + "\\%6d.jpg";
                    break;
                case 1:
                    //小图
                    command = "-i" + " " + videoPath + " " + "-s 9x8 " + target + "\\%6d.jpg";
                    break;
                default:
                    return;
            }
            //command = "-i" + " " + videoPath + " " + target + "\\%6d.jpg";
            Debug.WriteLine(DateTime.Now);
            p.StartInfo.FileName = "ffmpeg.exe";
            p.StartInfo.Arguments = command;
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            p.WaitForExit();
            p.Close();
            p.Dispose();
        }


        private void ExecuteCloingCmd()
        {
            Debug.WriteLine("ExecuteCloingCmd");
            if (pipeReader != null && pipeReader.IsConnected)
            {
                Debug.WriteLine("pipeReader.Close");
                pipeReader.Close();
            }

            if (worker != null && worker.IsBusy)
            {
                Debug.WriteLine("worker.CancelAsync");
                worker.CancelAsync();
            }
            Process[] processes = Process.GetProcessesByName("ffmpeg");
            if(processes!=null && processes.Length>0)
            {
                foreach (var item in processes)
                {
                    Debug.WriteLine("进程 {0} 被销毁", item.Id);
                    item.Kill();
                }
            }
            //重置界面状态
            MetaList.Clear();
            //按钮
            Messenger.Default.Send("frameClosing", "FVM2FV");
        }

        private void ExecuteOpenFileDialogCmd(string p)
        {
            string filter = @"视频文件|*.avi;*.mp4;*.wmv;*.mpeg|所有文件|*.*";
            SourcePath = FileDialogService.GetService().OpenFileDialog(filter);
        }

        private void ExecuteFolderBrowserDialogCmd()
        {
            TargetPath = FileDialogService.GetService().OpenFolderBrowserDialog();
        }



        private bool CanExecuteFrameCmd()
        {
            return !string.IsNullOrEmpty(TargetPath);
        }

        /// <summary>
        /// 存储分帧间隔
        /// 开始分帧
        /// </summary>
        private void ExecuteFrameCmd()
        {
            List<string> list = new List<string>();
            foreach (var item in MetaList)
            {
                list.Add(item.VideoPath);
            }

            string fileName = @"Util/frame.txt";

            try
            {
                FileStream aFile = new FileStream(fileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(aFile);
                string data = JsonConvert.SerializeObject(list).Replace("\"", "'");
                sw.WriteLine(data);
                sw.WriteLine(TargetPath);
                sw.Close();
                aFile.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("创建文件失败");
                Debug.WriteLine(e.ToString());
                return;
            }

            return;

            //分帧逻辑
            //使用cmd运行Python
            string pythonFilePosition = @"Util/testpipe.py";

            string cmdString = @"python " + pythonFilePosition + " " + fileName;
            CmdHelper.RunCmd(cmdString);
            //运行后台进程
            worker.RunWorkerAsync();

        }

        //private void Worker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    try
        //    {
        //        pipeReader = new NamedPipeServerStream("cutfram_result1", PipeDirection.InOut);
        //        Debug.WriteLine("字节读取管道正在连接...");
        //        pipeReader.WaitForConnection();
        //        Debug.WriteLine("字节读取管道已连接");

        //        //ProgV = Visibility.Visible;

        //        const int BUFFERSIZE = 256;
        //        int messageType = 0;
        //        bool completed = false;

        //        while (!completed)
        //        {
        //            if (worker.CancellationPending)
        //            {
        //                e.Cancel = true;
        //                pipeReader.Close();
        //                return;
        //            }

        //            byte[] buffer = new byte[BUFFERSIZE];
        //            int nRead = 0;
        //            try
        //            {
        //                nRead = pipeReader.Read(buffer, 0, BUFFERSIZE);
        //            }
        //            catch (Exception readE)
        //            {
        //                Debug.WriteLine("读取管道发生异常");
        //                Debug.WriteLine(readE.ToString());
        //                nRead = 0;
        //            }

        //            string line = string.Empty;
        //            try
        //            {
        //                line = Encoding.UTF8.GetString(buffer, 0, nRead);
        //            }
        //            catch (Exception getE)
        //            {
        //                Debug.WriteLine("转换string发生异常");
        //                Debug.WriteLine(getE.ToString());
        //                continue;
        //            }

        //            Debug.WriteLine("line: " + line);

        //            if (string.IsNullOrEmpty(line))
        //                continue;

        //            string[] messages = line.Split('_');
        //            int.TryParse(messages[0], out messageType);
        //            switch (messageType)
        //            {
        //                case 0:
        //                    break;
        //                case 1:
        //                    //收到进度
        //                    break;
        //                case 2:
        //                    break;
        //                case 4:
        //                    //普通消息
        //                    if (messages.Length == 2 && "Done".Equals(messages[1]))
        //                    {
        //                        completed = true;
        //                    }
        //                    break;
        //                case 8:
        //                    break;
        //                case 16:
        //                    break;
        //                case 32:
        //                    errorMsg = "本次任务由于后台而中断";
        //                    if (messages.Length > 1)
        //                        errorMsg += "\r\n消息：" + messages[1];
        //                    completed = true;
        //                    e.Cancel = true;
        //                    return;
        //                case 64:
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        pipeReader.Close();
        //    }
        //    catch (Exception pipeE)
        //    {
        //        Debug.WriteLine("管道发生异常");
        //        Debug.WriteLine(pipeE.ToString());
        //        pipeReader.Close();
        //    }

        //}

        //private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    Debug.WriteLine("in Worker_RunWorkerCompleted");
        //    //ProgV = Visibility.Collapsed;

        //    if (pipeReader.IsConnected)
        //    {
        //        pipeReader.Close();
        //    }

        //    if (e.Cancelled || e.Error != null || !string.IsNullOrEmpty(errorMsg))
        //    {
        //        MessageBox.Show("失败：" + errorMsg);
        //    }
        //    else
        //    {
        //        string framePathWithDirectory = Path.GetFileNameWithoutExtension(SourcePath);
        //        Debug.WriteLine("framePathWithDirectory: " + framePathWithDirectory);
        //        string target = TargetPath + @"\" + framePathWithDirectory + @"\bigimg";
        //        int result = MetaService.GetService().UpdateFramePathByVideoPath(target, SourcePath);
        //        Debug.WriteLine("分帧完成");
        //    }
        //    errorMsg = "";
        //}

        private void ExecuteOpenFrameVideosCmd(Button button)
        {
            FrameFileChooseWindow frameFileChooseWindow = new FrameFileChooseWindow();
            frameFileChooseWindow.ShowDialog();
        }

        #region helper function
        private void InitWork()
        {
            try
            {
                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true,
                    WorkerReportsProgress = true
                };
                worker.DoWork += Worker_DoWork;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            }
            catch (Exception e)
            {
                Debug.WriteLine("后台任务初始化失败");
                Debug.WriteLine(e.ToString());
            }
        }



        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ProgV = Visibility.Visible;
            });
            worker.ReportProgress(0);
            for (int i = 0; i < MetaList.Count; i++)
            {
                if (!File.Exists(MetaList[i].VideoPath))
                {
                    Debug.WriteLine("视频 {0} 不存在", MetaList[i].VideoPath);
                    continue;
                }

                Stopwatch watch = new Stopwatch();
                watch.Start();

                string framePathWithDirectory = Path.GetFileNameWithoutExtension(MetaList[i].VideoPath);

                //小图
                string target = TargetPath + @"\" + framePathWithDirectory + @"\smallimg";
                //大图
                string target1 = TargetPath + @"\" + framePathWithDirectory + @"\bigimg";

                try
                {
                    Directory.CreateDirectory(target);
                    Directory.CreateDirectory(target1);
                }
                catch (Exception fileExpt)
                {
                    Debug.WriteLine("创建文件夹失败：" + fileExpt.ToString());
                    return;
                }

                Cut(MetaList[i].VideoPath, target, 1);
                Cut(MetaList[i].VideoPath, target1, 0);

                watch.Stop();
                TimeSpan timespan = watch.Elapsed;  //获取当前实例测量得出的总时间
                Debug.WriteLine("本次视频分帧执行时间：{0}(毫秒)", timespan.TotalMilliseconds);  //总毫秒数

                MetaService.GetService().UpdateFramePathByVideoPath(target1, MetaList[i].VideoPath);
                worker.ReportProgress(100 * (i + 1) / MetaList.Count);
            }
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ProgressValue = e.ProgressPercentage;
            });
        }
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                MessageBox.Show("失败");
            }
            else
            {
                MessageBox.Show("完成");
            }

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ProgV = Visibility.Hidden;
            });
            Messenger.Default.Send("frameIsFinished", "FVM2FV");
        }
        private void AssignCommands()
        {
            OpenFileDialogCmd = new RelayCommand<string>((str) => ExecuteOpenFileDialogCmd(str));
            FolderBrowserDialogCmd = new RelayCommand(() => ExecuteFolderBrowserDialogCmd());
            FrameCmd = new RelayCommand(() => ExecuteFrameCmd(), CanExecuteFrameCmd);
            CloingCmd = new RelayCommand(() => ExecuteCloingCmd());
            OpenFrameVideosCmd = new RelayCommand<Button>((btn) => ExecuteOpenFrameVideosCmd(btn));
            FrameListCmd = new RelayCommand(() => ExecuteFrameListCmd(), CanExecuteFrameListCmd);
        }

        private void GetMsg(List<MetaModel> list)
        {
            MetaList = list;
            string content = "共选择" + list.Count + "个样本";
            Debug.WriteLine(content);
            BtnContent = content;
        }

        private void InitData()
        {
            try
            {
                TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            catch (Exception)
            {
                TargetPath=@"C:\";
            }
            BtnContent = "请点击";
            ProgV = Visibility.Hidden;
            MetaList = new List<MetaModel>();
        }
        #endregion
    }
}
