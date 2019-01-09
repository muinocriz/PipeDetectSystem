using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MvvmLight4.Common;
using MvvmLight4.Model;
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MvvmLight4.ViewModel
{
    public class MarkViewModel : ViewModelBase
    {
        public MarkViewModel()
        {
            Messenger.Default.Register<string[]>(this, "MFSVM2MVM", message =>
            {
                FolderPath = message[0];
                SavePath = message[1];

                InitSaveDirectory();
                player = new PlayerModel();
                DirectoryInfo root = new DirectoryInfo(FolderPath);
                FileInfo[] files = root.GetFiles("*.png");
                files = files.OrderBy(y => y.Name, new FileComparer()).ToArray();
                foreach (var item in files)
                {
                    string name = item.FullName;
                    imagePath.Add(name);
                }
                player.StartNum = 0;
                player.EndNum = imagePath.Count - 1;
                //加载之后自动开始进程
                worker.RunWorkerAsync(player);
                //暂停，让用户手动点击开始
                manualReset.Reset();
            });

            InitData();
            DispatcherHelper.Initialize();
            InitWorker();

            Messenger.Default.Send<bool>(true, "CloseMarkFileChooseWindow");
        }
        #region 属性
        public BackgroundWorker worker;
        private ManualResetEvent manualReset = new ManualResetEvent(true);
        public PlayerModel player;
        public List<string> imagePath = new List<string>();
        public string FolderPath { get; set; }
        public string TargetPath { get; set; }
        public string SavePath { get; set; }

        private int currentAbnormalType;
        /// <summary>
        /// 当前选中的异常类型
        /// </summary>
        public int CurrentAbnormalType { get => currentAbnormalType; set { currentAbnormalType = value; RaisePropertyChanged(() => CurrentAbnormalType); } }
        private int currentFramePosition;
        /// <summary>
        /// 当前帧号（该图片在列表里的位置）
        /// </summary>
        public int CurrentFramePosition { get => currentFramePosition; set { currentFramePosition = value; RaisePropertyChanged(() => CurrentFramePosition); } }
        private ObservableCollection<int> abnormalNums;
        /// <summary>
        /// 各个异常的数目，初始化全0
        /// </summary>
        public ObservableCollection<int> AbnormalNums { get => abnormalNums; set { abnormalNums = value; RaisePropertyChanged(() => AbnormalNums); } }
        private string currentThumbnailPath;
        /// <summary>
        /// 右下角缩略图绝对路径，初始化为default.png
        /// </summary>
        public string CurrentThumbnailPath { get => currentThumbnailPath; set { currentThumbnailPath = value; RaisePropertyChanged(() => CurrentThumbnailPath); } }

        private ImageSource currentThumbnailPathNew;
        /// <summary>
        /// 右下角缩略图绝对路径，初始化为default.png    新
        /// </summary>
        public ImageSource CurrentThumbnailPathNew { get => currentThumbnailPathNew; set { currentThumbnailPathNew = value; RaisePropertyChanged(() => CurrentThumbnailPathNew); } }

        private MarkModel markItem;
        /// <summary>
        /// 显示在右下角的mark model
        /// </summary>
        public MarkModel MarkItem { get => markItem; set { markItem = value; RaisePropertyChanged(() => MarkItem); } }
        private ObservableCollection<MarkModel> marks;
        /// <summary>
        /// 存储标记的异常
        /// </summary>
        public ObservableCollection<MarkModel> Marks { get => marks; set { marks = value; RaisePropertyChanged(() => Marks); } }
        private string imgSource;
        /// <summary>
        /// 当前视频图片的路径
        /// </summary>
        public string ImgSource { get => imgSource; set { imgSource = value; RaisePropertyChanged(() => ImgSource); } }
        #endregion
        #region 命令
        private RelayCommand playCmd;
        /// <summary>
        /// 播放
        /// </summary>
        public RelayCommand PlayCmd
        {
            get
            {
                if (playCmd == null)
                    return new RelayCommand(() =>
                    {
                        manualReset.Set();
                    });
                return playCmd;
            }
            set
            {
                PlayCmd = value;
            }
        }
        private RelayCommand pauseCmd;
        /// <summary>
        /// 暂停
        /// </summary>
        public RelayCommand PauseCmd
        {
            get
            {
                if (pauseCmd == null)
                    return new RelayCommand(() =>
                    {
                        manualReset.Reset();
                    });
                return pauseCmd;
            }
            set
            {
                pauseCmd = value;
            }
        }
        private RelayCommand ffCmd;
        /// <summary>
        /// 快进
        /// </summary>
        public RelayCommand FFCmd
        {
            get
            {
                if (ffCmd == null)
                    return new RelayCommand(() =>
                    {
                        if (imagePath.Count - currentFramePosition > 125)
                            currentFramePosition += 125;
                    });
                return ffCmd;
            }
            set
            {
                ffCmd = value;
            }
        }
        private RelayCommand rewCmd;
        /// <summary>
        /// 快退
        /// </summary>
        public RelayCommand RewCmd
        {
            get
            {
                if (rewCmd == null)
                    return new RelayCommand(() =>
                    {
                        if (currentFramePosition > 125)
                            currentFramePosition -= 125;
                        else
                            currentFramePosition = 0;
                    });
                return rewCmd;
            }
            set
            {
                rewCmd = value;
            }
        }
        private RelayCommand cancelCmd;
        /// <summary>
        /// 撤销
        /// </summary>
        public RelayCommand CancelCmd
        {
            get
            {
                if (cancelCmd == null)
                    return new RelayCommand(() => ExecuteCancelCmd(), CanExecuteCancelCmd);
                return cancelCmd;
            }
            set
            {
                cancelCmd = value;
            }
        }

        private bool CanExecuteCancelCmd()
        {
            return Marks.Count != 0;
        }

        private void ExecuteCancelCmd()
        {
            //修改缩略图 修改
            if (Marks.Count > 1)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    CurrentThumbnailPath = Marks[Marks.Count - 2].Path;
                    CurrentThumbnailPathNew =  BitmapFrame.Create(new Uri(Marks[Marks.Count - 2].Path), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                });
            }
            else
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    CurrentThumbnailPath = @"../Image/default.png";
                    CurrentThumbnailPathNew = BitmapFrame.Create(new Uri("../../Image/default.png", UriKind.Relative), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                });
            }
            int a = Marks[Marks.Count - 1].Type;
            string p = Marks[Marks.Count - 1].Path;
            //减少该异常的数量
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                AbnormalNums[a]--;
                Marks.RemoveAt(Marks.Count - 1);
            });
            
            //删除文件
            if (File.Exists(p))
                File.Delete(p);
            
        }

        private RelayCommand<Image> markCmd;
        /// <summary>
        /// 点击标注
        /// </summary>
        public RelayCommand<Image> MarkCmd
        {
            get
            {
                if (markCmd == null)
                    return new RelayCommand<Image>((img) => ExecuteMarkCmd(img), CanExecuteMarkCmd);
                return markCmd;
            }
            set
            {
                markCmd = value;
            }
        }

        private bool CanExecuteMarkCmd(Image img)
        {
            Point p = Mouse.GetPosition(img);
            return !(p.X < 30 || p.Y < 30 || img.ActualWidth - p.X < 30 || img.ActualHeight - p.Y < 30);
        }

        private void ExecuteMarkCmd(Image img)
        {
            Point p = Mouse.GetPosition(img);
            //拍照
            //MessageBox.Show("" + img.ActualHeight + "-" + img.ActualWidth + " " + p.Y + "-" + p.X);
            //添加到结果集合

            //"201901081636_1234_3_157.png"
            string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" +
                                Convert.ToString(CurrentFramePosition) + "_" +
                                Convert.ToString(currentAbnormalType) + "_" +
                                Convert.ToString(Marks.Count) + ".png";

            //"C:\Users\超\Desktop\2019-01-02 16-08-00" + "/2/"
            string path = SavePath + @"/" + Convert.ToString(currentAbnormalType) + @"/" + filename;
            ImageHelper.caijianpic(imagePath[CurrentFramePosition], path, p.X * 1.0 / img.ActualWidth, p.Y * 1.0 / img.ActualHeight, 30, 30);
            Console.WriteLine("" + p.X * 1.0 / img.ActualWidth + "---" + p.Y * 1.0 / img.ActualHeight);
            MarkModel mark = new MarkModel(Convert.ToString(currentFramePosition), currentAbnormalType, p.X * 1.0 / img.ActualWidth, p.Y * 1.0 / img.ActualHeight, path);
            Marks.Add(mark);
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                CurrentThumbnailPath = mark.Path;

                CurrentThumbnailPathNew = BitmapFrame.Create(new Uri(mark.Path), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            });
            AbnormalNums[mark.Type]++;
        }
        private RelayCommand winClosedCmd;
        /// <summary>
        /// 退出
        /// </summary>
        public RelayCommand WinClosedCmd
        {
            get
            {
                if (winClosedCmd == null)
                    return new RelayCommand(() =>
                    {
                        worker.CancelAsync();
                        if (CurrentFramePosition != imagePath.Count - 1)
                            MessageBox.Show("您并未完全标注视频");
                        //if (System.Windows.MessageBox.Show("确定关闭吗?", "", MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                        //{
                        //    worker.CancelAsync();
                        //}
                    });
                return winClosedCmd;
            }
            set
            {
                winClosedCmd = value;
            }
        }

        #endregion
        #region 辅助函数



        public void InitData()
        {


            currentAbnormalType = 1;
            currentFramePosition = 0;
            AbnormalNums = new ObservableCollection<int>();
            for (int i = 0; i < 6; i++)
            {
                AbnormalNums.Add(0);
            }
            CurrentThumbnailPath = @"../Image/default.png";
            CurrentThumbnailPathNew = BitmapFrame.Create(new Uri("../../Image/default.png", UriKind.Relative), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

            Marks = new ObservableCollection<MarkModel>();
        }

        public void InitWorker()
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);


        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                MessageBox.Show("任务已取消");
            else
                MessageBox.Show("本视频标注完成");
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ImgSource = imagePath[e.ProgressPercentage];
            });
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            PlayerModel p = (PlayerModel)e.Argument;
            int start = p.StartNum;
            int end = p.EndNum;
            while (currentFramePosition < end)
            {
                if (worker.CancellationPending)
                {
                    Console.WriteLine("worker.CancellationPending");
                    e.Cancel = true;
                    return;
                }

                manualReset.WaitOne();

                worker.ReportProgress(currentFramePosition++);
                Thread.Sleep(p.Speed);
            }
        }
        private void InitSaveDirectory()
        {
            //为5类异常新建文件夹
            for (int i = 1; i < 6; i++)
            {
                string path = SavePath;
                path = path + @"\" + Convert.ToInt32(i);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }
        #endregion
    }
}
