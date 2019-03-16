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
            ModelItem = new ObservableCollection<ModelViewModel>
            {
                null,
                null
            };
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
            return ModelItem[0] != null && VideoList != null;
        }

        private void ExecuteDetectCmd()
        {
            //检测逻辑
            Dictionary<int, string> dict = new Dictionary<int, string>();
            foreach (var item in VideoList)
            {
                dict[(int)item.Id] = item.Meta.FramePath;
            }

            //DispatcherTimer timer = new DispatcherTimer
            //{
            //    Interval = new TimeSpan(0, 0, 5)
            //};
            //timer.Tick += new EventHandler(Timer_Tick);

            //使用cmd运行Python
            string cmdStringTest = ConfigurationManager.ConnectionStrings["DetectCmdString"].ConnectionString;
            Console.WriteLine(JsonConvert.SerializeObject(dict));
            string cmdString = string.Empty;
            //cmdString = "test.exe" + " " + JsonConvert.SerializeObject(dict) + " " + ModelItem.ModelModel.Location + @"\" + ModelItem.ModelModel.ModelName;
            try
            {
                FileStream aFile = new FileStream("Util/detect.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(aFile);
                string data = JsonConvert.SerializeObject(dict).Replace("\"", "'");
                sw.WriteLine(data);
                sw.WriteLine(ModelItem[0].ModelModel.Location + "\\" + ModelItem[0].ModelModel.ModelName);
                if(ModelItem[1]!=null && !string.IsNullOrEmpty(ModelItem[1].ModelModel.Location))
                {
                    sw.WriteLine(ModelItem[1].ModelModel.Location+"\\"+ ModelItem[1].ModelModel.ModelName);
                }
                else
                {
                    sw.WriteLine("None");
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
            Console.WriteLine("cmdstring: "+cmdString);

            var t = new Task(() =>
            {
                Process p = CmdHelper.RunProcess(@"Util/detect.exe", "detect.txt");
                p.Start();
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
                WorkerReportsProgress = true
            };
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
            //timer.Start();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            abnormalModels.Clear();

            //管道名称
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
                byte[] buffer = new byte[BUFFERSIZE];
                int nRead = pipeReader.Read(buffer, 0, BUFFERSIZE);
                Console.WriteLine("buffer:"+buffer);
                Console.WriteLine("0x16 message:");
                Console.WriteLine(String.Format("{0:X}", buffer));
                string line = Encoding.UTF8.GetString(buffer, 0, nRead);
                Console.WriteLine("line: " + line);
                Console.WriteLine(String.Format("{0:X}", line));
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
                            string _position = String.Empty;
                            int.TryParse(messages[1], out int _videoId);
                            _position = messages[2];
                            int.TryParse(messages[3], out int _type);
                            //存起来，最后一期打包存
                            AbnormalModel abnormalModel = new AbnormalModel(_videoId, _position, _type);
                            abnormalModels.Add(abnormalModel);

                            //日志
                            log = "帧号：\t" + _position + "\t异常类型\t" + _type;

                            worker.ReportProgress(progress, log);
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
