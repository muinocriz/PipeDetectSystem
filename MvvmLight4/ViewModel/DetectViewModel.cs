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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MvvmLight4.ViewModel
{
    //not change
    public class DetectViewModel : ViewModelBase
    {
        public DetectViewModel()
        {
            AssignCommands();
            DispatcherHelper.Initialize();

            Messenger.Default.Register<ObservableCollection<MetaViewModel>>(this, "VideosChooseMessage", ShowReceiveInfo);
            ModelItem = new ObservableCollection<ModelViewModel>
            {
                null,
                null
            };
        }


        #region property
        /// <summary>
        /// 后台工作类
        /// </summary>
        public BackgroundWorker worker;

        /// <summary>
        /// 管道服务端
        /// </summary>
        public NamedPipeServerStream pipeReader;

        /// <summary>
        /// 异常信息
        /// </summary>
        public List<AbnormalModel> abnormalModels;

        /// <summary>
        /// 可以回溯或导出吗
        /// 即是否训练完成
        /// </summary>
        public bool canBackTrackOrExport = false;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string errorMsg = "";

        /// <summary>
        /// 进程号
        /// </summary>
        public int processId = -1;

        private string receiveStr;
        /// <summary>
        /// 按钮文本
        /// </summary>
        public string ReceiveStr
        {
            get
            {
                return receiveStr;
            }
            set
            {
                receiveStr = value;
                RaisePropertyChanged(() => ReceiveStr);
            }
        }
        private ObservableCollection<MetaViewModel> videoList;
        /// <summary>
        /// 选中的视频列表
        /// </summary>
        public ObservableCollection<MetaViewModel> VideoList
        {
            get { return videoList; }
            set
            {
                videoList = value;
                RaisePropertyChanged(() => VideoList);
            }
        }

        private ObservableCollection<ModelViewModel> modelList;
        /// <summary>
        /// 模型选择下拉框填充列表
        /// </summary>
        public ObservableCollection<ModelViewModel> ModelList
        {
            get { return modelList; }
            set
            {
                modelList = value;
                RaisePropertyChanged(() => ModelList);
            }
        }
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

        private Visibility detectProgVb;
        /// <summary>
        /// 检测进度可视状态
        /// </summary>
        public Visibility DetectProgVb
        {
            get { return detectProgVb; }
            set
            {
                detectProgVb = value;
                RaisePropertyChanged(() => DetectProgVb);
            }
        }

        private int detectProgNum;
        /// <summary>
        /// 检测进度值
        /// </summary>
        public int DetectProgNum
        {
            get { return detectProgNum; }
            set
            {
                detectProgNum = value;
                RaisePropertyChanged(() => DetectProgNum);
            }
        }

        private ObservableCollection<ModelViewModel> modelItem;
        /// <summary>
        /// 选中的模型对象
        /// </summary>
        public ObservableCollection<ModelViewModel> ModelItem
        {
            get { return modelItem; }
            set
            {
                modelItem = value;
                RaisePropertyChanged(() => ModelItem);
            }
        }

        private string logText;
        /// <summary>
        /// 日志展示
        /// </summary>
        public string LogText
        {
            get { return logText; }
            set
            {
                logText = value;
                RaisePropertyChanged(() => LogText);
            }
        }

        private ObservableCollection<Instrument> instruments;
        /// <summary>
        /// 仪器列表
        /// </summary>
        public ObservableCollection<Instrument> Instruments
        {
            get { return instruments; }
            set
            {
                instruments = value;
                RaisePropertyChanged(() => Instruments);
            }
        }

        private Instrument selectedInstrument;
        /// <summary>
        /// 选择的仪器名称
        /// </summary>
        public Instrument SelectedInstrument
        {
            get { return selectedInstrument; }
            set
            {
                selectedInstrument = value;
                RaisePropertyChanged(() => SelectedInstrument);
            }
        }
        #endregion

        #region command
        /// <summary>
        /// 加载界面命令
        /// </summary>
        public RelayCommand LoadedCmd { get; private set; }

        /// <summary>
        /// 加载函数
        /// </summary>
        private void ExecuteLoadedCmd()
        {
            InitComboBox();
            InitData();
        }

        /// <summary>
        /// 关闭命令
        /// </summary>
        public RelayCommand ClosedCmd { get; private set; }

        /// <summary>
        /// 关闭函数
        /// </summary>
        private void ExecuteClosedCmd()
        {
            if (pipeReader != null && pipeReader.IsConnected)
            {
                pipeReader.Close();
            }

            if (worker != null && worker.IsBusy)
            {
                worker.CancelAsync();
            }

            if (processId >= 0)
            {
                ColseFun(processId);
            }
        }
        /// <summary>
        /// 检测命令
        /// </summary>
        public RelayCommand DetectCmd { get; private set; }

        private bool CanExecuteDetectCmd()
        {
            return ModelItem[0] != null && VideoList != null && SelectedInstrument != null;
        }

        /// <summary>
        /// 检测函数
        /// </summary>
        private void ExecuteDetectCmd()
        {
            //检测逻辑
            Dictionary<int, string> dict = new Dictionary<int, string>();
            List<Video> videos = new List<Video>();
            foreach (var item in VideoList)
            {
                //dict[(int)item.Id] = item.Meta.FramePath;
                videos.Add(new Video() { Id = (int)item.Id, Path = item.Meta.FramePath, Head = item.Meta.HeadTime, Tail = item.Meta.TailTime });
            }

            //使用cmd运行Python
            string cmdStringTest = ConfigurationManager.ConnectionStrings["DetectCmdString"].ConnectionString;
            Console.WriteLine(JsonConvert.SerializeObject(dict));
            string cmdString = string.Empty;

            try
            {
                FileStream aFile = new FileStream("Util/detect.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(aFile);
                string data = JsonConvert.SerializeObject(videos).Replace("\"", "'");
                sw.WriteLine(data);
                sw.WriteLine(ModelItem[0].ModelModel.Location + "\\" + ModelItem[0].ModelModel.ModelName);
                if (ModelItem[1] != null && !string.IsNullOrEmpty(ModelItem[1].ModelModel.Location))
                {
                    sw.WriteLine(ModelItem[1].ModelModel.Location + "\\" + ModelItem[1].ModelModel.ModelName);
                }
                else
                {
                    sw.WriteLine("None");
                }
                string[] positions = SelectedInstrument.Path.Split('-');
                for (int i = 0; i < positions.Length; i++)
                {
                    sw.WriteLine(positions[i]);
                }
                sw.Close();
                aFile.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("创建文件失败");
                Console.WriteLine(e.ToString());
                return;
            }

            var t = new Task(() =>
            {
                Process p = CmdHelper.RunProcess(@"Util/main.exe", "-predict -empty");
                //Process p = new Process();
                //p.StartInfo.FileName = @"Util /main.exe";
                //p.StartInfo.Arguments = @"-predict";
                //p.StartInfo.UseShellExecute = false;
                //p.StartInfo.CreateNoWindow = false;
                //p.StartInfo.RedirectStandardOutput = true;
                //p.StartInfo.RedirectStandardError = true;

                //string eOut = null;
                //string sOut = null;

                //p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                //{ eOut += e.Data; });
                //p.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                //{ sOut += e.Data; });

                p.Start();
                processId = p.Id;
                Console.WriteLine("process has started");

                //p.BeginOutputReadLine();
                //p.BeginErrorReadLine();

                Console.WriteLine("wait for exit");
                p.WaitForExit();
                Console.WriteLine("exited");
                p.Close();
                Console.WriteLine("closed");
                //Console.WriteLine($"\nError stream: {eOut}");
                //Console.WriteLine($"\nOutput stream: {sOut}");
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
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            abnormalModels.Clear();

            //管道名称
            try
            {
                pipeReader = new NamedPipeServerStream("SamplePipe", PipeDirection.InOut);
                Console.WriteLine("byte reader connecting");
                pipeReader.WaitForConnection();
                Console.WriteLine("byte reader connected");
            }
            catch (Exception)
            {
                MessageBox.Show("管道连接失败");
                pipeReader.Close();
                e.Cancel = true;
                return;
            }

            FilterProgVb = Visibility.Visible;
            DetectProgVb = Visibility.Hidden;

            bool completed = false;
            int progress = 0;
            const int BUFFERSIZE = 256;
            LogText = "";
            errorMsg = "";
            string log = "";
            canBackTrackOrExport = false;

            while (!completed)
            {
                if (worker.CancellationPending)
                {
                    errorMsg = @"管道未能正确连接";
                    e.Cancel = true;
                    return;
                }

                try
                {
                    byte[] buffer = new byte[BUFFERSIZE];
                    int nRead = pipeReader.Read(buffer, 0, BUFFERSIZE);
                    Console.WriteLine("buffer:" + buffer);
                    string line = Encoding.UTF8.GetString(buffer, 0, nRead);
                    Console.WriteLine("line: " + line);
                    string[] messages = line.Split('_');
                    int.TryParse(messages[0], out int messageType);
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
                            //异常类型
                            if (messages.Length == 5)
                            {
                                int.TryParse(messages[1], out int _videoId);
                                string _position = messages[2];
                                int.TryParse(messages[3], out int _type);
                                double.TryParse(messages[4], out double m);

                                AbnormalModel abnormalModel = new AbnormalModel(_videoId, _position, _type, m);
                                abnormalModels.Add(abnormalModel);

                                //日志
                                log = "帧号：\t" + _position + "\t异常类型\t" + _type + "\t缺陷位置\t" + m;

                                worker.ReportProgress(progress, log);
                            }
                            //初筛结束消息
                            else if (messages.Length == 2 && !string.IsNullOrEmpty(messages[1]) && "filter done".Equals(messages[1]))
                            {
                                Console.WriteLine("filter done true");
                                //初筛结束
                                FilterProgVb = Visibility.Hidden;
                                DetectProgVb = Visibility.Visible;
                            }
                            //检测结束消息
                            else if (messages.Length == 2 && "Done".Equals(messages[1]))
                            {
                                progress = 100;
                                worker.ReportProgress(progress);
                                Thread.Sleep(1500);
                                completed = true;
                            }
                            break;
                        case 8:
                            string msg = string.Empty;
                            for (int i = 1; i < messages.Length; i++)
                            {
                                msg += messages[i] + " ";
                            }

                            log = msg;
                            worker.ReportProgress(progress, log);
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
                catch (Exception)
                {
                    MessageBox.Show("处理管道数据发生错误");
                    pipeReader.Close();
                    e.Cancel = true;
                    return;
                }

            }
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (e.UserState != null)
                {
                    string log = (string)e.UserState;
                    log = DateTime.Now.ToString() + "\r\n" + log + "\r\n\r\n";
                    LogText += log;
                }
                DetectProgNum = e.ProgressPercentage;
            });
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            processId = -1;
            pipeReader.Close();
            DetectProgVb = Visibility.Hidden;
            LogText += "\r\n检测完成";
            if (e.Cancelled || e.Error != null)
            {
                MessageBox.Show(errorMsg);
            }
            else
            {
                //批量存入
                if (abnormalModels != null && abnormalModels.Count > 0)
                {
                    AbnormalService.GetService().AddAbnormal(abnormalModels);
                }

                //清空
                abnormalModels.Clear();
                canBackTrackOrExport = true;
            }
        }

        /// <summary>
        /// 打开选择视频文件对话框
        /// Open Video Select Cmd
        /// </summary>
        public RelayCommand OpenVSC { get; private set; }

        /// <summary>
        /// 打开窗口
        /// </summary>
        private void ExecuteOpenVSCmd()
        {
            DetectFileChooseWindow sender = new DetectFileChooseWindow();
            Messenger.Default.Send(VideoList.Count, "DVM2VVM");
            VideoList.Clear();//每次打开视频选择窗口时，将已选择的视频列表删除，防止重复添加
            sender.Show();
        }

        /// <summary>
        /// 打开人工回溯界面
        /// Open Back Track Cmd
        /// </summary>
        public RelayCommand OpenBTC { get; private set; }

        private bool CanExecuteOpenBTCmd()
        {
            return canBackTrackOrExport;
        }

        private void ExecuteOpenBTCmd()
        {
            BackTrackWindow sender = new BackTrackWindow();
            List<int> list = new List<int>();
            foreach (var item in VideoList)
            {
                if (item.Id != null)
                    list.Add((int)item.Id);
            }
            Messenger.Default.Send(list, "DVM2BTVM");
            sender.Show();
        }

        /// <summary>
        /// 打开导出界面
        /// Open Back Track Cmd
        /// </summary>
        public RelayCommand OpenEC { get; private set; }

        private bool CanExecuteOpenECmd()
        {
            return canBackTrackOrExport;
        }

        /// <summary>
        /// 打开窗口
        /// </summary>
        private void ExecuteOpenECmd()
        {
            ExportWindow sender = new ExportWindow();
            sender.Show();
            Messenger.Default.Send<bool>(true, "CloseDectWindow"); //注意：token参数一致
        }

        #endregion

        #region 辅助函数
        /// <summary>
        /// 注册命令
        /// </summary>
        private void AssignCommands()
        {
            LoadedCmd = new RelayCommand(() => ExecuteLoadedCmd());
            DetectCmd = new RelayCommand(() => ExecuteDetectCmd(), CanExecuteDetectCmd);
            OpenVSC = new RelayCommand(() => ExecuteOpenVSCmd());
            OpenBTC = new RelayCommand(() => ExecuteOpenBTCmd(), CanExecuteOpenBTCmd);
            OpenEC = new RelayCommand(() => ExecuteOpenECmd(), CanExecuteOpenECmd);
            ClosedCmd = new RelayCommand(() => ExecuteClosedCmd());
        }

        /// <summary>
        /// 初始化下拉列表
        /// </summary>
        private void InitComboBox()
        {
            //初始化模型列表
            ModelList = ModelService.GetService().LoadData();

            //初始化仪器列表
            Instruments = new ObservableCollection<Instrument>
            {
                new Instrument() { Name = "中仪1", Path = @"910-1062-1035-1030-32-125" },
                new Instrument() { Name = "其它仪器", Path = @"910-1062-1035-1030-32-125" }
            };

            //设置仪器列表默认选中项目
            SelectedInstrument = Instruments[0];
        }

        private void InitData()
        {
            VideoList = new ObservableCollection<MetaViewModel>();
            abnormalModels = new List<AbnormalModel>();

            FilterProgVb = Visibility.Hidden;
            DetectProgVb = Visibility.Hidden;
            DetectProgNum = 0;
            LogText = "";
            canBackTrackOrExport = false;
            ReceiveStr = "点击按钮选择视频文件";
            errorMsg = "";
        }

        /// <summary>
        /// 展示选择检测列表数量
        /// </summary>
        /// <param name="obj"></param>
        private void ShowReceiveInfo(ObservableCollection<MetaViewModel> obj)
        {
            if (obj.Count == 0)
            {
                ReceiveStr = "您还未选择文件";
            }
            else
            {
                ReceiveStr = "共选择了" + obj.Count + "个文件";
                foreach (var item in obj)
                {
                    VideoList.Add(item);
                }
            }
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
        #endregion
    }

    #region helper class
    public class Instrument
    {
        private string _path;
        private string _name;

        public string Name { get => _name; set => _name = value; }
        public string Path { get => _path; set => _path = value; }
    }

    public class Video
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Head { get; set; }
        public string Tail { get; set; }
    }
    #endregion
}
