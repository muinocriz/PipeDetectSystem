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
            Messenger.Default.Register<List<int>>(this, "DVM2BTVM", msg =>
            {
                VideoIds = msg;
            });

            InitCombobox();

            DispatcherHelper.Initialize();
        }

        //图像列表
        public List<string> imagePath = new List<string>();
        //视频列表
        public List<int> VideoIds = new List<int>();
        private bool stop = false;

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
        /// 界面加载时函数
        /// </summary>
        public RelayCommand LoadedCmd
        {
            get
            {
                if (loadedCmd == null)
                    return new RelayCommand(() =>
                    {
                        //初始化元数据+异常信息的组合
                        AbnormalVMs = AbnormalService.GetService().SelectAll(VideoIds);
                        ErrorNum = 0;
                        foreach (var item in AbnormalVMs)
                        {
                            if (item.Abnormal.Type != 0 && item.Abnormal.Type != 6)
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
        private readonly RelayCommand<AbnormalViewModel> selectCommand;
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
        }

        private bool CanExecuteSelectCommand(AbnormalViewModel arg)
        {
            return true;
        }

        private void ExecuteSelectCommand(AbnormalViewModel p)
        {
            stop = true;
            //更新左下角显示区域
            SelectedAVM = p;
            CombboxItem = CombboxList[p.Abnormal.Type];

            //播放视频
            //找到文件
            //string folderPath = AbnormalService.GetService().SelectFolder(p.Abnormal.VideoId);
            string folderPath = p.Meta.FramePath;

            player = new PlayerModel
            {
                Target = Convert.ToInt32(p.Abnormal.Position)//目标帧号
            };

            DirectoryInfo root = new DirectoryInfo(folderPath);
            FileInfo[] files = root.GetFiles("*.jpg");
            files = files.OrderBy(y => y.Name, new FileComparer()).ToArray();
            foreach (var item in files)
            {
                string name = item.FullName;
                imagePath.Add(name);
            }
            player.Calculate(imagePath.Count,120);

            //播放函数

            
            var t = new Task(() =>
            {
                int nowPic = player.StartNum;
                int end = player.EndNum;
                stop = false;

                while(!stop && nowPic <= end)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        ImgSource = imagePath[nowPic];
                    });
                    nowPic++;
                    Thread.Sleep(player.Speed);
                }
                stop = true;
            });
            t.Start();
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
                typeChangedCmd = value;
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

                //重新计算现有的总异常
                ErrorNum = 0;
                foreach (var item in abnormalVMs)
                {
                    if (item.Abnormal.Type != 0 && item.Abnormal.Type != 6)
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
            return;
        }

        #region 辅助方法
        private void InitCombobox()
        {
            CombboxList = new List<ComplexInfoModel>() {
              new ComplexInfoModel(){ Key="0",Text="局部正常" },
              new ComplexInfoModel(){ Key="1",Text="破裂" },
              new ComplexInfoModel(){ Key="2",Text="腐蚀" },
              new ComplexInfoModel(){ Key="3",Text="树根" },
              new ComplexInfoModel(){ Key="4",Text="结垢" },
              new ComplexInfoModel(){ Key="5",Text="局部异常5" },
              new ComplexInfoModel(){ Key="6",Text="全局正常" },
              new ComplexInfoModel(){ Key="7",Text="障碍" },
              new ComplexInfoModel(){ Key="8",Text="起伏" },
              new ComplexInfoModel(){ Key="9",Text="沉积" },
              new ComplexInfoModel(){ Key="10",Text="错口" },
              new ComplexInfoModel(){ Key="11",Text="全局异常5"}
            };
        }
        #endregion
    }
}
