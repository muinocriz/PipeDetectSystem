using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmLight4.Common;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO.Pipes;
using System.Linq;
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
        }

        public BackgroundWorker worker;
        public NamedPipeServerStream pipeReader;
        public string errorMsg = "";

        private Visibility filterProgVb;
        /// <summary>
        /// 初筛可见性
        /// </summary>
        public Visibility FilterProgVb
        {
            get
            {
                return filterProgVb;
            }
            set
            {
                filterProgVb = value;
                RaisePropertyChanged(() => FilterProgVb);
            }
        }

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
                        FilterProgVb = Visibility.Hidden;
                        TrainProgVb = Visibility.Hidden;
                        TrainProgNum = 0;

                        errorMsg = "";
                    });
                return loadedCmd;
            }
            set
            {
                loadedCmd = value;
            }
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
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Tick += new EventHandler(Timer_Tick);

            //分帧逻辑
            //使用cmd运行Python
            string cmdString = ConfigurationManager.ConnectionStrings["TrainCmdString"].ConnectionString;
            CmdHelper.RunCmd(cmdString);

            //新建后台进程
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();

            timer.Start();
        }


        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            pipeReader = new NamedPipeServerStream("SamplePipe", PipeDirection.InOut);
            Console.WriteLine("byte reader connecting");
            pipeReader.WaitForConnection();
            Console.WriteLine("byte reader connected");

            FilterProgVb = Visibility.Visible;
            TrainProgVb = Visibility.Hidden;

            bool completed = false;
            int progress = 0;
            const int BUFFERSIZE = 256;
            int messageType = 0;
            errorMsg = "";

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
                            worker.ReportProgress(progress);
                        }
                        break;
                    case 2:
                        break;
                    case 4:
                        Console.WriteLine(messages[1]);
                        //普通消息
                        if (messages.Length == 2 && !string.IsNullOrEmpty(messages[1]))
                        {
                            if ("filter done".Equals(messages[1]))
                            {
                                Console.WriteLine("filter done true");
                                //初筛结束
                                FilterProgVb = Visibility.Hidden;
                                TrainProgVb = Visibility.Visible;
                            }
                            else if ("Done".Equals(messages[1]))
                            {
                                progress = 100;
                                worker.ReportProgress(progress);
                                //检测结束
                                Thread.Sleep(1500);
                                FilterProgVb = Visibility.Hidden;
                                TrainProgVb = Visibility.Hidden;
                                completed = true;
                            }
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
                        e.Cancel = true;
                        return;
                    case 64:
                        break;
                    default:
                        break;
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer _timer = (DispatcherTimer)sender;
            _timer.Stop();
            if (!pipeReader.IsConnected)
            {
                Console.WriteLine("diaoyong shibai ");
                NamedPipeClientStream npcs = new NamedPipeClientStream("SamplePipe");
                npcs.Connect();
                worker.CancelAsync();
            }
            else
                Console.WriteLine("diaoyong chenggong");
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TrainProgNum = e.ProgressPercentage;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                MessageBox.Show(errorMsg);
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
        }
    }
}
