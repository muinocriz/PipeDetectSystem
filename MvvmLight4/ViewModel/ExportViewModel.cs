using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmLight4.ViewModel
{
    public class ExportViewModel : ViewModelBase
    {
        public ExportViewModel()
        {
            LoadData();
            LoadWorker();
            DispatcherHelper.Initialize();
        }
        private BackgroundWorker worker;
        private List<ComplexInfoModel> combboxList;
        private Dictionary<string, string> dict;
        private List<AbnormalViewModel> list;
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
        private int way;
        /// <summary>
        /// 输出方式
        /// </summary>
        public int Way
        {
            get { return way; }
            set
            {
                way = value;
                RaisePropertyChanged(() => Way);
            }
        }
        private string targetSource;
        /// <summary>
        /// 导出文件存放位置
        /// </summary>
        public string TargetSource
        {
            get { return targetSource; }
            set
            {
                targetSource = value;
                RaisePropertyChanged(() => TargetSource);
            }
        }

        private bool? selectAll;
        //全选
        public bool? SelectAll
        {
            get
            {
                return selectAll;
            }
            set
            {
                selectAll = value;
                RaisePropertyChanged(() => SelectAll);
            }
        }

        private ObservableCollection<ExportModel> exports;
        public ObservableCollection<ExportModel> Exports
        {
            get { return exports; }
            set
            {
                exports = value;
                RaisePropertyChanged(() => Exports);
            }
        }

        private Visibility proVisiable;
        /// <summary>
        /// 进度条可视状态
        /// </summary>
        public Visibility ProVisiable
        {
            get { return proVisiable; }
            set
            {
                proVisiable = value;
                RaisePropertyChanged(() => ProVisiable);
            }
        }

        private RelayCommand<ExportModel> checkCmd;
        public RelayCommand<ExportModel> CheckCmd
        {
            get
            {
                if (checkCmd == null)
                    return new RelayCommand<ExportModel>((p) => ExecuteCheckCmd(p), CanExecuteCheckCmd);
                return checkCmd;
            }
            set
            {
                CheckCmd = value;
            }
        }

        private bool CanExecuteCheckCmd(ExportModel arg)
        {
            return arg != null;
        }

        private void ExecuteCheckCmd(ExportModel p)
        {
            switch (p.IsChoose)
            {
                //选中->未选中
                case -1: break;
                //未选中->选中
                case 1:
                    {
                        if (string.IsNullOrEmpty(p.Byname))
                            p.Byname = p.Alternative;
                        break;
                    }
            }
        }
        private RelayCommand selectAllCmd;
        /// <summary>
        /// 全选
        /// </summary>
        public RelayCommand SelectAllCmd
        {
            get
            {
                if (selectAllCmd == null)
                    return new RelayCommand(() => ExecuteSelectAllCmd());
                return selectAllCmd;
            }
            set
            { selectAllCmd = value; }
        }

        private void ExecuteSelectAllCmd()
        {
            foreach (ExportModel item in Exports)
            {
                item.IsChoose = 1;
                ExecuteCheckCmd(item);
            }
        }

        private RelayCommand unSelectAllCmd;
        /// <summary>
        /// 全不选
        /// </summary>
        public RelayCommand UnSelectAllCmd
        {
            get
            {
                if (unSelectAllCmd == null)
                    return new RelayCommand(() => ExecuteUnSelectAllCmd());
                return unSelectAllCmd;
            }
            set
            { unSelectAllCmd = value; }
        }

        private void ExecuteUnSelectAllCmd()
        {
            foreach (ExportModel item in Exports)
            {
                item.IsChoose = -1;
                ExecuteCheckCmd(item);
            }
        }

        private RelayCommand exportCmd;
        public RelayCommand ExportCmd
        {
            get
            {
                if (exportCmd == null)
                    return new RelayCommand(() => ExecuteExportCmd(), CanExecuteExportCmd);
                return exportCmd;
            }
            set
            { exportCmd = value; }
        }

        private bool CanExecuteExportCmd()
        {
            return combboxItem != null && !string.IsNullOrEmpty(TargetSource);
        }

        private void ExecuteExportCmd()
        {
            //保存对输出选项的更改到数据库
            ExportService.GetService().UpdateExport(Exports);
            //输出逻辑
            //获得所有要选择的项-别名
            dict = new Dictionary<string, string>();
            dict = ExportService.GetService().SelectChoose();
            //读取所有属性
            list = AbnormalService.GetService().ExportByVideoId(Convert.ToInt32(combboxItem.Key));
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ProVisiable = Visibility.Visible;
            });
            //完成写入
            //switch (Way)
            //{
            //    case 0:
            //        SaveService.GetService().SaveXlsxFile(TargetSource, dict, list);
            //        break;
            //    case 1:
            //        break;
            //    default:
            //        break;
            //}
            worker.RunWorkerAsync();
        }

        private RelayCommand folderBrowserCmd;
        public RelayCommand FolderBrowserCmd
        {
            get
            {
                if (folderBrowserCmd == null)
                    return new RelayCommand(() => ExecuteFolderBrowserCmd());
                return folderBrowserCmd;
            }
            set
            {
                FolderBrowserCmd = value;
            }
        }

        private void ExecuteFolderBrowserCmd()
        {
            TargetSource = FileDialogService.GetService().OpenSaveFileDialog();
        }

        #region 附属方法
        private void LoadData()
        {
            CombboxList = AbnormalService.GetService().QueryVideo();
            if (CombboxList.Count > 0)
                combboxItem = CombboxList[0];

            Way = 0;

            ProVisiable = Visibility.Hidden;

            Exports = ExportService.GetService().SelectAll();
            foreach (var item in Exports)
            {
                //对选中但是还未有别名的项，将别名初始化为原名
                if (item.IsChoose != 0 && string.IsNullOrEmpty(item.Byname))
                    item.Byname = item.Alternative;
            }
        }
        private void LoadWorker()
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ProVisiable = Visibility.Hidden;
            });
            MessageBox.Show("导出 " + TargetSource + " 已完成");
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            switch (Way)
            {
                case 0:
                    SaveService.GetService().SaveXlsxFile(TargetSource, dict, list);
                    break;
                case 1:
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
