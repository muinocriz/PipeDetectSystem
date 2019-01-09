using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MvvmLight4.Model;
using MvvmLight4.Service;
using MvvmLight4.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.ViewModel
{
    public class DetectViewModel : ViewModelBase
    {
        public DetectViewModel()
        {
            InitComboBox();
            ReceiveStr = "点击按钮选择视频文件";
            VideoList = new ObservableCollection<MetaViewModel>();
            Messenger.Default.Register<ObservableCollection<MetaViewModel>>(this, "VideosChooseMessage", ShowReceiveInfo);
        }

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
                    return new RelayCommand(() => ExecuteOpenBTCmd());
                return openBTC;
            }
        }
        private void ExecuteOpenBTCmd()
        {
            BackTrackWindow sender = new BackTrackWindow();
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
                    return new RelayCommand(() => ExecuteOpenECmd());
                return openEC;
            }
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
