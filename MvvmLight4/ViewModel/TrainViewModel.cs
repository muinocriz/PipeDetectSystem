using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MvvmLight4.Common;
using MvvmLight4.Model;
using MvvmLight4.Service;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MvvmLight4.ViewModel
{
    public class TrainViewModel : ViewModelBase
    {
        public TrainViewModel()
        {
            Model = new ModelModel();
            Model.Lteration = 201000;
            Model.Rate = 85;
            DispatcherHelper.Initialize();
        }

        public BackgroundWorker worker;
        public NamedPipeServerStream pipeReader;
        public string errorMsg = "";
        public bool pipeFlag = true;
        //训练进程PID
        public int trainProcessPID = -1;

        private Visibility trainProgVb;
        /// <summary>
        /// 训练可见性
        /// </summary>
        public Visibility TrainProgVb
        {
            get
            {
                return trainProgVb;
            }
            set
            {
                trainProgVb = value;
                RaisePropertyChanged(() => TrainProgVb);
            }
        }

        private int trainProgNum;
        /// <summary>
        /// 训练进度
        /// </summary>
        public int TrainProgNum
        {
            get
            {
                return trainProgNum;
            }
            set
            {
                trainProgNum = value;
                RaisePropertyChanged(() => TrainProgNum);
            }
        }


        private ModelModel model;
        public ModelModel Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
                RaisePropertyChanged(() => Model);
            }
        }

        private string outputText;
        /// <summary>
        /// 输出框显示字符串
        /// </summary>
        public string OutputText
        {
            get
            {
                return outputText;
            }
            set
            {
                outputText = value;
                RaisePropertyChanged(() => OutputText);
            }
        }

        private RelayCommand loadedCmd;
        /// <summary>
        /// 加载函数
        /// </summary>
        public RelayCommand LoadedCmd
        {
            get
            {
                if (loadedCmd == null)
                    return new RelayCommand(() =>
                    {
                        //进度条初始化
                        //TrainProgVb = Visibility.Hidden;
                        //TrainProgNum = 0;

                        errorMsg = "";
                    });
                return loadedCmd;
            }
            set
            {
                loadedCmd = value;
            }
        }

        private RelayCommand<string> preprocessCmd;
        /// <summary>
        /// 预处理函数
        /// </summary>
        public RelayCommand<string> PreprocessCmd
        {
            get
            {
                if (preprocessCmd == null)
                {
                    return new RelayCommand<string>((directory) => ExecutePreprocessCmd(directory), CanExecutePreprocessCmd);
                }
                return preprocessCmd;
            }
            set
            {
                preprocessCmd = value;
            }
        }

        private void ExecutePreprocessCmd(string directory)
        {
            var t = new Task(() =>
            {
                Process p = CmdHelper.RunProcess("Util/try.exe", directory);
                p.Start();
                Console.WriteLine("pid:" + p.Id);
                Console.WriteLine("python is start");
                //string outputData = string.Empty;
                //string errorData = string.Empty;
                //p.BeginOutputReadLine();
                //p.BeginErrorReadLine();
                //p.OutputDataReceived += (sender, e) =>
                //  {
                //      outputData += (e.Data + "\n");
                //  };
                //p.ErrorDataReceived += (sender, e) =>
                //{
                //    errorData += (e.Data + "\n");
                //};
                p.WaitForExit();
                Console.WriteLine("python is exit");
                p.Close();
                Console.WriteLine("python is closed");
            });
            t.Start();
            t.ContinueWith((task) =>
            {
                if (task.IsCompleted)
                    MessageBox.Show("已完成");
                else if (task.IsCanceled)
                    MessageBox.Show("已取消");
                else if (task.IsFaulted)
                    MessageBox.Show("任务失败");
            });
        }

        private bool CanExecutePreprocessCmd(string directory)
        {
            return !string.IsNullOrEmpty(directory);
        }

        private RelayCommand<string> folderBrowserCmd;
        public RelayCommand<string> FolderBrowserCmd
        {
            get
            {
                if (folderBrowserCmd == null)
                    return new RelayCommand<string>((p) => ExecuteFolderBrowserCmd(p));
                return folderBrowserCmd;
            }
            set
            {
                FolderBrowserCmd = value;
            }
        }

        private void ExecuteFolderBrowserCmd(string p)
        {
            string path = FileDialogService.GetService().OpenFolderBrowserDialog();
            switch (p)
            {
                case "S":
                    {
                        Model.Simple = path;
                        break;
                    }
                case "O":
                    {
                        Model.Location = path;
                        break;
                    }
            }
        }

        private RelayCommand<string> openFileCmd;
        public RelayCommand<string> OpenFileCmd
        {
            get
            {
                if (openFileCmd == null)
                    return new RelayCommand<string>((p) => ExecuteOpenFileCmd(p));
                return openFileCmd;
            }
            set
            {
                OpenFileCmd = value;
            }
        }

        private void ExecuteOpenFileCmd(string p)
        {
            Model.SourceModel = FileDialogService.GetService().OpenFileDialog();
        }

        private RelayCommand stopTrainCmd;
        public RelayCommand StopTrainCmd
        {
            get
            {
                if (stopTrainCmd == null)
                    return new RelayCommand(() =>
                    {
                        ColseFun(trainProcessPID);

                        pipeFlag = false;


                    });
                return stopTrainCmd;
            }
            set
            { stopTrainCmd = value; }
        }

        /// <summary>
        /// 通过停止python方式停止task
        /// </summary>
        /// <param name="trainProcessPID">线程号</param>
        private void ColseFun(int id)
        {
            Process process = Process.GetProcessById(id);
            ManagementObjectSearcher searcher
                = new ManagementObjectSearcher("select * from Win32_Process where ParentProcessID=" + id);
            ManagementObjectCollection moc = searcher.Get();
            foreach (var mo in moc)
            {
                Process proc = Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                Console.WriteLine("proc id: " + proc.Id);
                Console.WriteLine("proc is null: " + proc == null);
                Console.WriteLine("proc.HasExited: " + proc.HasExited);
                if (!proc.HasExited)
                    proc.Kill();
            }
        }

        private RelayCommand trainCmd;
        public RelayCommand TrainCmd
        {
            get
            {
                if (trainCmd == null)
                    return new RelayCommand(() => ExecuteTrainCmd(), CanExecuteTrainCmd);
                return trainCmd;
            }
            set
            {
                TrainCmd = value;
            }
        }

        private bool CanExecuteTrainCmd()
        {
            return !(string.IsNullOrEmpty(Model.Simple) || string.IsNullOrEmpty(Model.Location) || string.IsNullOrEmpty(Model.ModelName));
        }

        /// <summary>
        /// 1存储模型数据到数据库
        /// 2处理模型
        /// </summary>
        private void ExecuteTrainCmd()
        {
            bool hasPre = CheckPre(Model.Simple);
            if(hasPre)
            {
                MessageBox.Show("您还未进行预处理，请先点击预处理按钮。");
                return;
            }
            // 显示终止按钮
            Messenger.Default.Send<string>("showStopButton", "trainMessage");

            if (string.IsNullOrEmpty(Model.SourceModel))
                Model.SourceModel = "";

            if (string.IsNullOrEmpty(Model.CreateTime))
                Model.CreateTime = "";

            if (string.IsNullOrEmpty(Model.UpdateTime))
                Model.UpdateTime = "";

            string json = JsonConvert.SerializeObject(Model).Replace("\"", "'");

            var t = new Task(() =>
            {
                Process p = CmdHelper.RunProcess(@"Util/1.exe", "c");
                p.Start();
                trainProcessPID = p.Id;
                Console.WriteLine("trainProcessPID:" + trainProcessPID);
                Console.WriteLine("wait for exit");
                p.WaitForExit();
                Console.WriteLine("exited");
                p.Close();
                Console.WriteLine("closed");
            });
            t.Start();
            t.ContinueWith((task) =>
            {
                if (task.IsCompleted)
                    Console.WriteLine("已完成");
                else if (task.IsCanceled)
                    MessageBox.Show("已取消");
                else if (task.IsFaulted)
                    MessageBox.Show("任务失败");
            });

            //新建后台进程
            worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();

            //timer.Start();
        }

        /// <summary>
        /// 检查样本位置是否有预处理之后的文件
        /// </summary>
        /// <param name="simple">样本位置</param>
        /// <returns></returns>
        private bool CheckPre(string simple)
        {
            // 如果预处理成功，那么改文件会存放在/bigimg目录下
            string fileName = simple + @"\" + "val.tfrecords";
            if(File.Exists(fileName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            pipeReader = new NamedPipeServerStream("train_inception", PipeDirection.InOut);
            Console.WriteLine("byte reader connecting");
            pipeReader.WaitForConnection();
            Console.WriteLine("byte reader connected");

            //TrainProgVb = Visibility.Visible;

            bool completed = false;
            pipeFlag = true;
            //int progress = 0;
            const int BUFFERSIZE = 256;
            int messageType = 0;
            errorMsg = "";

            while (pipeFlag &&!completed)
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
                        //if (messages.Length == 2 && int.TryParse(messages[1], out progress))
                        //{
                        //    worker.ReportProgress(progress);
                        //}
                        break;
                    case 2:
                        break;
                    case 4:
                        Console.WriteLine(messages[1]);
                        //普通消息-训练次数
                        if (messages.Length == 3 && "s".Equals(messages[1]))
                        {
                            string text = "当前训练次数：" + messages[2];
                            worker.ReportProgress(0, text);
                        }
                        //普通消息-训练准确率
                        if (messages.Length == 3 && "v".Equals(messages[1]))
                        {
                            string text = "当前训练准确率：0." + messages[2];
                            worker.ReportProgress(0, text);
                        }
                        //训练完成
                        else if (messages.Length == 2 && "Done".Equals(messages[1]))
                        {
                            //progress = 100;
                            //worker.ReportProgress(progress);
                            //检测结束
                            //Thread.Sleep(1500);
                            //TrainProgVb = Visibility.Hidden;
                            completed = true;
                        }
                        break;
                    case 8:
                        {
                            string text = String.Empty;
                            if (messages.Length > 1)
                            {
                                for (int i = 1; i < messages.Length; i++)
                                    text += messages[i]+" ";
                            }
                            worker.ReportProgress(0, text);
                        }
                        break;
                    case 16:
                        break;
                    case 32:
                        errorMsg = "本次任务由于后台而中断";
                        if (messages.Length > 1)
                            errorMsg += "\r\n消息：" + messages[1];
                        e.Cancel = true;
                        return;
                    case 64:
                        break;
                    default:
                        //sleep()
                        break;
                }
            }
        }

        //private void Timer_Tick(object sender, EventArgs e)
        //{
        //    DispatcherTimer _timer = (DispatcherTimer)sender;
        //    _timer.Stop();
        //    if (!pipeReader.IsConnected)
        //    {
        //        Console.WriteLine("计时器：调用管道失败");
        //        NamedPipeClientStream npcs = new NamedPipeClientStream("SamplePipe");
        //        npcs.Connect();
        //        worker.CancelAsync();
        //    }
        //    else
        //        Console.WriteLine("计时器：调用管道成功");
        //}
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //TrainProgNum = e.ProgressPercentage;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (e.UserState != null)
                {
                    string text = (string)e.UserState;
                    string log = DateTime.Now.ToString() + "\r\n" + text + "\r\n\r\n";
                    OutputText += log;
                }
            });
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // 隐藏终止按钮
            Messenger.Default.Send<string>("hideStopButton", "trainMessage");

            if (e.Cancelled || e.Error != null)
            {
                MessageBox.Show(errorMsg);
                //TrainProgVb = Visibility.Hidden;
            }
            else
            {
                pipeReader.Close();
                //存储逻辑
                DateTime dt = DateTime.Now;
                Model.CreateTime = dt.ToString();
                int result = ModelService.GetService().AddModel(Model);
                if (result > 0)
                {
                    MessageBox.Show(result + "个模型已生成");
                    //训练部分
                }
            }
            Thread.Sleep(200);
            Messenger.Default.Send<string>("closeTrainWindow", "trainMessage");
        }
    }
}
