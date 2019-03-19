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
using System.Windows;

namespace MvvmLight4.ViewModel
{
    //not change
    public class VideoViewModel : ViewModelBase
    {
        public VideoViewModel()
        {
            InitData();
            AssignCommands();

            Messenger.Default.Register<int>(this, "DVM2VVM", msg =>
            {
                if (msg == 0 && VideoList != null)
                {
                    foreach (var item in VideoList)
                    {
                        item.IsChoose = -1;
                    }
                }
                if (SelectList != null && SelectList.Count > 0)
                {
                    SelectList.Clear();
                }
            });
        }



        private ObservableCollection<MetaViewModel> selectList;
        /// <summary>
        /// 选中文件列表
        /// </summary>
        public ObservableCollection<MetaViewModel> SelectList
        {
            get { return selectList; }
            set
            {
                selectList = value;
                RaisePropertyChanged(() => SelectList);
            }
        }
        private ObservableCollection<MetaViewModel> videoList;
        /// <summary>
        /// 文件列表
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

        /// <summary>
        /// 加载
        /// </summary>
        public RelayCommand WinLoadedCommand { get; private set; }

        /// <summary>
        /// 选中命令
        /// </summary>
        public RelayCommand<MetaViewModel> CheckCmd { get; private set; }

        private void ExecuteCheckCmd(MetaViewModel p)
        {
            switch (p.IsChoose)
            {
                //未选中->选中
                case 1:
                    {
                        if (!SelectList.Contains(p))
                            SelectList.Add(p);
                        break;
                    }
                default:
                    {
                        //选中->未选中
                        //MessageBox.Show("选中->未选中");
                        if (SelectList.Contains(p))
                            SelectList.Remove(p);
                        break;
                    }
            }
        }

        /// <summary>
        /// 选择完毕命令
        /// </summary>
        public RelayCommand<Window> ChooseCmd { get; private set; }

        private void ExecuteChooseCmd(Window window)
        {
            Messenger.Default.Send(selectList, "VideosChooseMessage");
            window.Close();
        }

        #region 附属方法
        private void InitData()
        {
            SelectList = new ObservableCollection<MetaViewModel>();
        }

        private void ExecuteLoadCmd()
        {
            VideoList = MetaService.GetService().SelectAllFramed();

        }

        private void AssignCommands()
        {
            WinLoadedCommand = new RelayCommand(() => ExecuteLoadCmd());
            CheckCmd = new RelayCommand<MetaViewModel>((p) => ExecuteCheckCmd(p));
            ChooseCmd = new RelayCommand<Window>((window) => ExecuteChooseCmd(window));
        }
        #endregion
    }
}
