using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using MvvmLight4.Common;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
            InitWorker();
            DispatcherHelper.Initialize();

            AssignCommands();
        }

        #region property
        private BackgroundWorker worker;
        private List<ComplexInfoModel> combboxList;
        private Dictionary<string, string> dict;
        private List<ExportModel> exportModelsForExcel;
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
        #endregion

        public RelayCommand LoadedCmd { get; private set; }

        public RelayCommand<ExportModel> CheckCmd { get; private set; }

        private bool CanExecuteCheckCmd(ExportModel arg)
        {
            return arg != null;
        }

        private void ExecuteCheckCmd(ExportModel p)
        {
            switch (p.IsChoose)
            {
                //选中->未选中
                case -1:
                    {
                        break;
                    }
                //未选中->选中
                case 1:
                    {
                        if (string.IsNullOrEmpty(p.Byname))
                            p.Byname = p.Alternative;
                        break;
                    }
            }
        }

        /// <summary>
        /// 全选
        /// </summary>
        public RelayCommand SelectAllCmd { get; private set; }

        private void ExecuteSelectAllCmd()
        {
            foreach (ExportModel item in Exports)
            {
                item.IsChoose = 1;
                ExecuteCheckCmd(item);
            }
        }

        /// <summary>
        /// 全不选
        /// </summary>
        public RelayCommand UnSelectAllCmd { get; private set; }

        private void ExecuteUnSelectAllCmd()
        {
            foreach (ExportModel item in Exports)
            {
                item.IsChoose = -1;
                ExecuteCheckCmd(item);
            }
        }

        public RelayCommand ExportCmd { get; private set; }

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

            exportModelsForExcel = new List<ExportModel>();
            exportModelsForExcel = ExportService.GetService().SelectChooseToList();

            //读取所有属性
            list = AbnormalService.GetService().ExportByVideoId(Convert.ToInt32(combboxItem.Key));
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ProVisiable = Visibility.Visible;
            });

            worker.RunWorkerAsync();
        }

        public RelayCommand FolderBrowserCmd { get; private set; }

        private void ExecuteFolderBrowserCmd()
        {
            TargetSource = FileDialogService.GetService().OpenSaveFileDialog();
        }

        #region helper function
        private void LoadData()
        {
            CombboxList = AbnormalService.GetService().QueryVideo();
            if (CombboxList.Count > 0)
            {
                CombboxItem = CombboxList[0];
            }

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


        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ProVisiable = Visibility.Hidden;
            });
            MessageBox.Show("导出 " + TargetSource + " 已完成");
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            switch (Way)
            {
                case 0:
                    SaveService.GetService().SaveXlsxFile(TargetSource, exportModelsForExcel, list);
                    break;
                case 1:
                    break;
                default:
                    break;
            }

            //调用Python执行Excel的完善
            Process process = CmdHelper.RunProcess("excel.exe", TargetSource);
            process.Start();
            process.WaitForExit();
            process.Close();
        }
        /// <summary>
        /// 初始化worker
        /// </summary>
        private void InitWorker()
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
        }


        private void AssignCommands()
        {
            LoadedCmd = new RelayCommand(() => LoadData());
            CheckCmd = new RelayCommand<ExportModel>((p) => ExecuteCheckCmd(p), CanExecuteCheckCmd);
            SelectAllCmd = new RelayCommand(() => ExecuteSelectAllCmd());
            UnSelectAllCmd = new RelayCommand(() => ExecuteUnSelectAllCmd());
            ExportCmd = new RelayCommand(() => ExecuteExportCmd(), CanExecuteExportCmd);
            FolderBrowserCmd = new RelayCommand(() => ExecuteFolderBrowserCmd());
        }
        #endregion
    }
}
