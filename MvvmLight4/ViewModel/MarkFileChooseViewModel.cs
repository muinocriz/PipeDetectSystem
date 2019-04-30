﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using MvvmLight4.Common;
using MvvmLight4.Model;
using MvvmLight4.Service;
using MvvmLight4.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmLight4.ViewModel
{
    public class MarkFileChooseViewModel : ViewModelBase
    {
        public MarkFileChooseViewModel()
        {
            AssignCommands();
            InitData();
        }

        #region 属性
        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string[] StringMessage;

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
        private string savePath;
        public string SavePath { get => savePath; set { savePath = value; RaisePropertyChanged(() => SavePath); } }
        #endregion

        #region 命令
        /// <summary>
        /// 打开文件夹
        /// </summary>
        public RelayCommand<string> FolderBrowserCmd { get; private set; }


        private void ExecuteFolderBrowserCmd(string p)
        {
            string path = FileDialogService.GetService().OpenFolderBrowserDialog();
            if (!string.IsNullOrEmpty(path))
                SavePath = path;
        }

        /// <summary>
        /// 点击确定
        /// </summary>
        public RelayCommand SubmitCmd { get; private set; }

        private bool CanExecuteSubmitCmd()
        {
            return CombboxItem != null && !string.IsNullOrEmpty(SavePath);
        }

        private void ExecuteSubmitCmd()
        {
            string framePath = MetaService.GetService().QueryFramePathById(Convert.ToInt32(CombboxItem.Key));

            bool check = CheckFramePath(framePath);

            if (check)
            {
                List<string> msg = new List<string>
                {
                    framePath,
                    SavePath
                };

                StringMessage[0] = framePath;
                StringMessage[1] = SavePath;

                MessageHelper.FramePath = framePath;
                MessageHelper.SavePath = SavePath;

                MarkWindow sender = new MarkWindow();
                Messenger.Default.Send<List<string>>(msg, "markMessage");

                log.Info("开始标注\n" + "FramePath：\t" + framePath + "\nSavePath：\t" + SavePath);
                sender.Show();
            }
        }

        private bool CheckFramePath(string framePath)
        {
            try
            {
                DirectoryInfo root = new DirectoryInfo(framePath);
                FileInfo[] files = root.GetFiles("*.jpg");
                return files.Count() > 0;
            }
            catch (Exception e)
            {
                MessageBox.Show("读取文件时发生异常,请查看文件夹中分帧文件是否正常存放，在关闭标注窗口后请重新选择文件:\n异常描述：\n" + e.ToString());
                log.Warn(framePath + "位置文件损坏", e);
                return false;
            }
        }
        #endregion

        #region 辅助方法
        private void InitData()
        {
            CombboxList = MetaService.GetService().QueryVideoFramed();
            if (CombboxList.Count > 0)
            {
                CombboxItem = CombboxList[0];
            }

            StringMessage = new string[2];
            //默认保存路径为桌面
            SavePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        private void AssignCommands()
        {
            FolderBrowserCmd = new RelayCommand<string>((p) => ExecuteFolderBrowserCmd(p));
            SubmitCmd = new RelayCommand(() => ExecuteSubmitCmd(), CanExecuteSubmitCmd);
        }
        #endregion
    }
}
