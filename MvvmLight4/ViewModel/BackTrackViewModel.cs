using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MvvmLight4.Common;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MvvmLight4.ViewModel
{
    public class BackTrackViewModel : ViewModelBase
    {
        public BackTrackViewModel()
        {
            Messenger.Default.Register<List<int>>(this, "DVM2BTVM", msg=>
            {
                VideoIds = msg;
            });
            InitCombobox();
            InitData();
            DispatcherHelper.Initialize();
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        public List<string> imagePath = new List<string>();
        public List<int> VideoIds = new List<int>();
        public BackgroundWorker worker;
        private ManualResetEvent manualReset = new ManualResetEvent(true);

        private PlayerModel player;
        public PlayerModel Player { get { return player; } set { player = value; RaisePropertyChanged(() => Player); } }
        private string imgSource;
        public string ImgSource { get { return imgSource; } set { imgSource = value; RaisePropertyChanged(() => ImgSource); } }
        #region 右下展示板
        //总数由abnormalVMs.Count直接生成切无需更改
        private int errorNum;
        /// <summary>
        /// 异常数目
        /// </summary>
        public int ErrorNum { get { return errorNum; } set { errorNum = value; RaisePropertyChanged(() => ErrorNum); } }
        private int changeNum;
        /// <summary>
        /// 修改数目
        /// </summary>
        public int ChangeNum { get { return changeNum; } set { changeNum = value; RaisePropertyChanged(() => ChangeNum); } }
        #endregion
        #region 列表区域
        private AbnormalViewModel selectedAVM;
        /// <summary>
        /// 被选中的项
        /// 用于显示视频下方的详细信息和对异常类型进行修改
        /// </summary>
        public AbnormalViewModel SelectedAVM
        {
            get
            {
                return selectedAVM;
            }
            set
            {
                selectedAVM = value;
                RaisePropertyChanged(() => SelectedAVM);
            }
        }
        private ObservableCollection<AbnormalViewModel> abnormalVMs;
        /// <summary>
        /// 带有元信息的异常列表
        /// </summary>
        public ObservableCollection<AbnormalViewModel> AbnormalVMs
        {
            get
            {
                return abnormalVMs;
            }
            set
            {
                abnormalVMs = value;
                RaisePropertyChanged(() => AbnormalVMs);
            }
        }
        #endregion
        #region 下拉框相关
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
                        AbnormalVMs = AbnormalService.GetService().SelectAll(VideoIds);
                        ErrorNum = 0;
                        foreach (var item in AbnormalVMs)
                        {
                            if (item.Abnormal.Type != 0)
                                ErrorNum++;
                        }
                    });
                return loadedCmd;
            }
            set
            {
                loadedCmd = value;
            }
        }
        #region 选择了DataGrid的某一项
        private RelayCommand<AbnormalViewModel> selectCommand;
        /// <summary>
        /// 选择了DataGrid的某一项
        /// </summary>
        public RelayCommand<AbnormalViewModel> SelectCommand
        {
            get
            {
                if (selectCommand == null)
                    return new RelayCommand<AbnormalViewModel>((p) => ExecuteSelectCommand(p),CanExecuteSelectCommand);
                return selectCommand;
            }
            set
            {
                SelectCommand = value;
            }
        }

        private bool CanExecuteSelectCommand(AbnormalViewModel arg)
        {
            if (worker.IsBusy)
            {
                worker.CancelAsync();
                Console.WriteLine("ExecuteSelectCommand done");
            }
            return !worker.IsBusy;
        }

        private void ExecuteSelectCommand(AbnormalViewModel p)
        {
            //更新左下角显示区域
            SelectedAVM = p;
            CombboxItem = CombboxList[p.Abnormal.Type];
            //播放视频
            //找到文件
            string folderPath = AbnormalService.GetService().SelectFolder(p.Abnormal.VideoId);
            //逻辑判断
            if(string.IsNullOrEmpty(folderPath))
            {
                MessageBox.Show("未找到视频分帧文件夹存放位置");
                return;
            }

            player = new PlayerModel();
            player.Target = Convert.ToInt32(p.Abnormal.Position);//目标帧号

            DirectoryInfo root = new DirectoryInfo(folderPath);
            FileInfo[] files = root.GetFiles("*.png");
            files = files.OrderBy(y => y.Name, new FileComparer()).ToArray();
            foreach (var item in files)
            {
                string name = item.FullName;
                imagePath.Add(name);
            }
            player.Calculate(imagePath.Count,120);
            if(worker.IsBusy)
            {
                manualReset.Set();
                worker.CancelAsync();
                //MessageBox.Show("正在播放中……");
            }
            else
            {
            worker.RunWorkerAsync(player);
            }
        }
        #endregion
        #region ComboxItem被更改
        private RelayCommand typeChangedCmd;
        /// <summary>
        /// ComboxItem被更改
        /// </summary>
        public RelayCommand TypeChangedCmd
        {
            get
            {
                if (typeChangedCmd == null)
                    return new RelayCommand(() => ExecuteTypeChangedCmd());
                return typeChangedCmd;
            }
            set
            {
                TypeChangedCmd = value;
            }
        }

        private void ExecuteTypeChangedCmd()
        {
            //修改数据库
            int type = Convert.ToInt32(CombboxItem.Key);
            int result = AbnormalService.GetService().UpdateAbnormalType(SelectedAVM.AbnormalId, type);
            //更改右下角显示
            if(result == 1)
            {
                ChangeNum++;
                SelectedAVM.Abnormal.Type = type;
                ErrorNum = 0;
                foreach (var item in abnormalVMs)
                {
                    if (item.Abnormal.Type != 0)
                        ErrorNum++;
                }
            }
        }
        #endregion
        private RelayCommand startCmd;
        public RelayCommand StartCmd
        {
            get
            {
                if (startCmd == null)
                    return new RelayCommand(() => ExecuteStartCmd());
                return startCmd;
            }
            set
            {
                startCmd = value;
            }
        }

        private void ExecuteStartCmd()
        {
            manualReset.Set();
        }
        private RelayCommand pauseCmd;
        public RelayCommand PauseCmd
        {
            get
            {
                if (pauseCmd == null)
                    return new RelayCommand(() => ExecutePauseCmd());
                return pauseCmd;
            }
            set
            {
                pauseCmd = value;
            }
        }
        private void ExecutePauseCmd()
        {
            manualReset.Reset();
            //worker.CancelAsync();
        }

        #region 辅助方法
        void InitData()
        {
            ChangeNum = 0;
        }

        private void InitCombobox()
        {
            CombboxList = new List<ComplexInfoModel>() {
              new ComplexInfoModel(){ Key="0",Text="无异常" },
              new ComplexInfoModel(){ Key="1",Text="1" },
              new ComplexInfoModel(){ Key="2",Text="2" },
              new ComplexInfoModel(){ Key="3",Text="3" },
              new ComplexInfoModel(){ Key="4",Text="4" },
              new ComplexInfoModel(){ Key="5",Text="5" }
            };
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            PlayerModel p = (PlayerModel)e.Argument;
            int start = p.StartNum;
            int end = p.EndNum;
            while (start<=end)
            {
                if (worker.CancellationPending)
                {
                    Console.WriteLine("here arrived");
                    e.Cancel = true;
                    return;
                }
                
                worker.ReportProgress(start++);
                Thread.Sleep(p.Speed);

                manualReset.WaitOne();
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ImgSource = imagePath[e.ProgressPercentage];
            });
        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("work done!");
        }
        #endregion
    }
}
