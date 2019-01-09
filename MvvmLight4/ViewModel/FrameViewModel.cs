using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.ViewModel
{
    public class FrameViewModel : ViewModelBase
    {
        public FrameViewModel()
        {
            InitCombbox();
        }

        private string sourcePath;
        public string SourcePath
        {
            get
            {
                return sourcePath;
            }
            set
            {
                sourcePath = value;
                RaisePropertyChanged(() => SourcePath);
            }
        }
        private string targetPath;
        public string TargetPath
        {
            get
            {
                return targetPath;
            }
            set
            {
                targetPath = value;
                RaisePropertyChanged(() => TargetPath);
            }
        }

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
        private RelayCommand<string> openFileDialogCmd;
        public RelayCommand<string> OpenFileDialogCmd
        {
            get
            {
                if (openFileDialogCmd == null)
                    return new RelayCommand<string>((p) => ExecuteOpenFileDialogCmd(p));
                return openFileDialogCmd;
            }
            set
            {
                OpenFileDialogCmd = value;
            }
        }

        private void ExecuteOpenFileDialogCmd(string p)
        {
            SourcePath = FileDialogService.GetService().OpenFileDialog();
        }

        private RelayCommand<string> folderBrowserDialogCmd;
        public RelayCommand<string> FolderBrowserDialogCmd
        {
            get
            {
                if (folderBrowserDialogCmd == null)
                    return new RelayCommand<string>((p) => ExecuteFolderBrowserDialogCmd(p));
                return folderBrowserDialogCmd;
            }
            set
            {
                FolderBrowserDialogCmd = value;
            }
        }

        private void ExecuteFolderBrowserDialogCmd(string p)
        {
            TargetPath = FileDialogService.GetService().OpenFolderBrowserDialog();
        }
        private RelayCommand frameCmd;
        public RelayCommand FrameCmd
        {
            get
            {
                if (frameCmd == null)
                    return new RelayCommand(() => ExecuteFrameCmd());
                return frameCmd;
            }
            set
            {
                FrameCmd = value;
            }
        }
        /// <summary>
        /// 存储分帧间隔
        /// 开始分帧
        /// </summary>
        private void ExecuteFrameCmd()
        {
            //存储分帧间隔
            //根据模型路径，设置该模型的分帧间隔
            int result = MetaService.GetService().UpdateInterval(SourcePath, Convert.ToInt32(CombboxItem.Key));
            Console.WriteLine(result);
        }
        #region 辅助函数
        /// <summary>
        /// 填充下拉框
        /// </summary>
        private void InitCombbox()
        {
            CombboxList = new List<ComplexInfoModel>() {
              new ComplexInfoModel(){ Key="5",Text="1/5" },
              new ComplexInfoModel(){ Key="10",Text="1/10" },
              new ComplexInfoModel(){ Key="20",Text="1/20" },
              new ComplexInfoModel(){ Key="25",Text="1/25" },
            };
        }
        #endregion

    }
}
