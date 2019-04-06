﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmLight4.ViewModel
{
    public class VideoManageViewModel : ViewModelBase
    {
        public VideoManageViewModel()
        {
            AssignCommands();
            InitData();
            DispatcherHelper.Initialize();
        }

        #region property
        const int COUNT = 1000;
        private ObservableCollection<MetaViewModel> videos;
        /// <summary>
        /// 负责展示的视频列表
        /// </summary>
        public ObservableCollection<MetaViewModel> Videos
        {
            get
            {
                return videos;
            }
            set
            {
                videos = value;
                RaisePropertyChanged(() => Videos);
            }
        }

        private MetaViewModel selectedVideo;
        /// <summary>
        /// 负责展示的视频列表
        /// </summary>
        public MetaViewModel SelectedVideo
        {
            get
            {
                return selectedVideo;
            }
            set
            {
                selectedVideo = value;
                RaisePropertyChanged(() => SelectedVideo);
            }
        }

        private string searchText;
        public string SearchText
        {
            get
            {
                return searchText;
            }
            set
            {
                searchText = value;
                RaisePropertyChanged(() => SearchText);
            }
        }
        #endregion

        #region command
        public RelayCommand SearchCmd { get; private set; }
        public RelayCommand RefreshCmd { get; private set; }
        public RelayCommand<int> DeleteCmd { get; private set; }
        #endregion

        #region helper function
        private void AssignCommands()
        {
            RefreshCmd = new RelayCommand(() => ExecuteRefreshCmd());
            SearchCmd = new RelayCommand(() => ExecuteSearchCmd(), CanExecuteSearchCmd);
            DeleteCmd = new RelayCommand<int>((i) => ExecuteDeleteCmd(i));
        }

        private void ExecuteDeleteCmd(int id)
        {
            int result = MetaService.GetService().DeleteById(id);
            if (result > 0)
            {
                for (int i = 0; i < Videos.Count; i++)
                {
                    if(Videos[i].Id == id)
                    {
                        Videos.RemoveAt(i);
                    }
                }
            }
        }

        private void ExecuteSearchCmd()
        {
            //空查询条件返回全部数据
            if (SearchText.Equals(""))
            {
                Videos = MetaService.GetService().SelectAll(COUNT);
                return;
            }

            ObservableCollection<MetaViewModel> selectedVideos = new ObservableCollection<MetaViewModel>();
            foreach (var item in Videos)
            {
                if (HasText(item.Meta.TaskCode) || HasText(item.Meta.PipeCode) || HasText(item.Meta.Addr))
                {
                    selectedVideos.Add(item);
                }
            }

            if (selectedVideos.Count <= 0)
            {
                MessageBox.Show("查询无结果，请重新输入查询条件");
            }
            else
            {
                Debug.WriteLine(selectedVideos.Count);
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    Videos = selectedVideos;
                });
            }
        }
        private bool HasText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                return text.Contains(SearchText);
            }
            return false;
        }

        private bool CanExecuteSearchCmd()
        {
            return SearchText != null;
        }

        private void ExecuteRefreshCmd()
        {
            InitData();
        }


        private void InitData()
        {
            Videos = MetaService.GetService().SelectAll(COUNT);
        }
        #endregion
    }
}