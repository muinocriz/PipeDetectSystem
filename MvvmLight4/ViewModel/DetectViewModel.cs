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
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MvvmLight4.ViewModel
{
    public class DetectViewModel : ViewModelBase
    {
        public DetectViewModel()
        {
            
            DispatcherHelper.Initialize();
            ReceiveStr = "点击按钮选择视频文件";
            Messenger.Default.Register<ObservableCollection<MetaViewModel>>(this, "VideosChooseMessage", ShowReceiveInfo);
            
        }

        public BackgroundWorker worker;
        public NamedPipeServerStream pipeReader;
        public List<AbnormalModel> abnormalModels;
        public bool canBackTrackOrExport;
        public string errorMsg = "";

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

        private ModelViewModel modelItem;
        /// <summary>
        /// 选中的模型对象
        /// </summary>
        public ModelViewModel ModelItem
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

        private RelayCommand loadedCmd;
        public RelayCommand LoadedCmd
        {
            get
            {
                if (loadedCmd == null)
                    return new RelayCommand(() =>
                    {
                        InitComboBox();
                        VideoList = new ObservableCollection<MetaViewModel>();
                        abnormalModels = new List<AbnormalModel>();

                        FilterProgVb = Visibility.Hidden;
                        DetectProgVb = Visibility.Hidden;
                        DetectProgNum = 0;
                        LogText = "";
                        canBackTrackOrExport = false;

                        errorMsg = "";
                    });
                return loadedCmd;
            }
            set
            {
                loadedCmd = value;
            }
        }

        private RelayCommand detectCmd;
        public RelayCommand DetectCmd
        {
            get
            {
                if (detectCmd == null)
                    return new RelayCommand(() => ExecuteDetectCmd(), CanExecuteDetectCmd);
                return detectCmd;
            }
            set
            {
                detectCmd = value;
            }
        }

        private bool CanExecuteDetectCmd()
        {
            return ModelItem != null && VideoList != null;
        }

        private void ExecuteDetectCmd()
        {
            //检测逻辑
            Dictionary<int, string> dict = new Dictionary<int, string>();
            foreach (var item in VideoList)
            {
                dict[(int)item.Id] = item.Meta.FramePath;
            }

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Tick += new EventHandler(Timer_Tick);

            //使用cmd运行Python
            string cmdStringTest = ConfigurationManager.ConnectionStrings["DetectCmdString"].ConnectionString;
            Console.WriteLine(JsonConvert.SerializeObject(dict));
            string cmdString = "test.exe" + " " + JsonConvert.SerializeObject(dict) + " " + ModelItem.ModelModel.Location + @"\" + ModelItem.ModelModel.ModelName;
            Console.WriteLine("cmdstring: "+cmdString);
            CmdHelper.RunCmd(cmdStringTest);

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
            abnormalModels.Clear();

            pipeReader = new NamedPipeServerStream("SamplePipe", PipeDirection.InOut);
            Console.WriteLine("byte reader connecting");
            pipeReader.WaitForConnection();
            Console.WriteLine("byte reader connected");

            FilterProgVb = Visibility.Visible;
            DetectProgVb = Visibility.Hidden;

            bool completed = false;
            int progress = 0;
            const int BUFFERSIZE = 256;
            int messageType = 0;
            LogText = "";
            errorMsg = "";
            canBackTrackOrExport = false;

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
                        //异常类型
                        if (messages.Length == 4)
                        {
                            int _videoId=0, _type=0;
                            string _position="";
                            int.TryParse(messages[1], out _videoId);
                            _position = messages[2];
                            int.TryParse(messages[3], out _type);
                            //存起来，最后一期打包存
                            AbnormalModel abnormalModel = new AbnormalModel(_videoId, _position, _type);
                            abnormalModels.Add(abnormalModel);
                            worker.ReportProgress(progress,abnormalModel);
                        }
                        //初筛结束消息
                        else if(messages.Length == 2 && !string.IsNullOrEmpty(messages[1])&& "filter done".Equals(messages[1]))
                        {
                            Console.WriteLine("filter done true");
                            //初筛结束
                            FilterProgVb = Visibility.Hidden;
                            DetectProgVb = Visibility.Visible;
                        }
                        //检测结束消息
                        else if(messages.Length == 2 && "Done".Equals(messages[1]))
                        {
                            progress = 100;
                            worker.ReportProgress(progress);
                            Thread.Sleep(1500);
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
            DispatcherHelper.CheckBeginInvokeOnUI(() => 
            {
                if (e.UserState != null)
                {
                    AbnormalModel am = (AbnormalModel)e.UserState;
                    string log = DateTime.Now.ToString() + "\r\n帧号：\t" + am.Position + "\t异常类型\t" + am.Type+"\r\n\r\n";
                    LogText += log;
                }
                DetectProgNum = e.ProgressPercentage;
            });
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
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
                if (abnormalModels!=null && abnormalModels.Count>0)
                AbnormalService.GetService().AddAbnormal(abnormalModels);
                //清空
                abnormalModels.Clear();
                canBackTrackOrExport = true;
            }
        }

        private RelayCommand openVSC;
        /// <summary>
        /// 打开选择视频文件对话框
        /// Open Video Select Cmd
        /// </summary>
        public RelayCommand OpenVSC
        {
            get
            {
                if (openVSC == null)
                    return new RelayCommand(() => ExecuteOpenVSCmd());
                return openVSC;
            }
        }
        /// <summary>
        /// 打开窗口
        /// </summary>
        private void ExecuteOpenVSCmd()
        {
            DetectFileChooseWindow sender = new DetectFileChooseWindow();
            Messenger.Default.Send<int>(VideoList.Count, "DVM2VVM");
            VideoList.Clear();//每次打开视频选择窗口时，将已选择的视频列表删除，防止重复添加
            sender.Show();
        }

        private RelayCommand openBTC;
        /// <summary>
        /// 打开人工回溯界面
        /// Open Back Track Cmd
        /// </summary>
        public RelayCommand OpenBTC
        {
            get
            {
                if (openBTC == null)
                    return new RelayCommand(() => ExecuteOpenBTCmd(), CanExecuteOpenBTCmd);
                return openBTC;
            }
        }

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
            Messenger.Default.Send<List<int>>(list, "DVM2BTVM");
            sender.Show();
        }

        private RelayCommand openEC;
        /// <summary>
        /// 打开导出界面
        /// Open Back Track Cmd
        /// </summary>
        public RelayCommand OpenEC
        {
            get
            {
                if (openEC == null)
                    return new RelayCommand(() => ExecuteOpenECmd(), CanExecuteOpenECmd);
                return openEC;
            }
        }

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

        #region 辅助函数
        private void InitComboBox()
        {
            ModelList = ModelService.GetService().LoadData();
        }
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

        #endregion
    }
}
