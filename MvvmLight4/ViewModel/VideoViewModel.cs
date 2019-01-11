using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmLight4.ViewModel
{
    public class VideoViewModel : ViewModelBase
    {
        public VideoViewModel()
        {
            InitDataGrid();
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
        /// 选中命令
        /// </summary>
        private RelayCommand<MetaViewModel> checkCmd;
        public RelayCommand<MetaViewModel> CheckCmd
        {
            get
            {
                if (checkCmd == null)
                    return new RelayCommand<MetaViewModel>((p) => ExecuteCheckCmd(p));
                return checkCmd;
            }
            set
            {
                CheckCmd = value;
            }
        }

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
        private RelayCommand chooseCmd;
        public RelayCommand ChooseCmd
        {
            get
            {
                if (chooseCmd == null)
                    return new RelayCommand(() => ExecuteChooseCmd());
                return chooseCmd;
            }
            set
            {
                ChooseCmd = value;
            }
        }

        private void ExecuteChooseCmd()
        {
            //MessageBox.Show("" + SelectList.Count);
            Messenger.Default.Send<ObservableCollection<MetaViewModel>>(selectList, "VideosChooseMessage");
        }

        #region 附属方法
        private void InitDataGrid()
        {
            VideoList = MetaService.GetService().SelectAll();
            SelectList = new ObservableCollection<MetaViewModel>();
        }
        #endregion
    }
}
