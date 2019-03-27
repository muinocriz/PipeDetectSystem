using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmLight4.Common;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MvvmLight4.ViewModel
{
    //not change
    public class ModelManageViewModel : ViewModelBase
    {
        public ModelManageViewModel()
        {
            AssignCommands();
            AddModel = new ModelModel();
        }

        #region 属性
        private ModelModel addModel;
        public ModelModel AddModel
        {
            get
            {
                return addModel;
            }
            set
            {
                addModel = value;
                RaisePropertyChanged(() => AddModel);
            }
        }
        private ObservableCollection<ModelViewModel> models;
        /// <summary>
        /// 所有模型
        /// </summary>
        public ObservableCollection<ModelViewModel> Models
        {
            get
            {
                return models;
            }
            set
            {
                models = value;
                RaisePropertyChanged(() => Models);
            }
        }
        #endregion

        #region 命令
        public RelayCommand AddModelCmd { get; private set; }

        public RelayCommand LoadedCmd { get; private set; }

        private void ExecuteLoadCmd()
        {
            Models = ModelService.GetService().LoadData();
        }

        public RelayCommand<DataGridCellEditEndingEventArgs> UpdateModelCmd { get; private set; }


        private void ExecuteAddModelCmd()
        {
            if (!Directory.Exists(AddModel.Location))
            {
                MessageBox.Show("文件目录不存在");
            }
            AddModel.CreateTime = addModel.UpdateTime = DateTime.Now.ToString();
            int result = ModelService.GetService().TransferModel(AddModel);
            if (result > 0)
            {
                MessageBox.Show(result + "个模型已迁移");
            }
            Models = ModelService.GetService().LoadData();
        }

        private bool CanExecuteAddModelCmd()
        {
            return !string.IsNullOrEmpty(AddModel.ModelName) && !string.IsNullOrEmpty(AddModel.Location);
        }

        private void ExecuteUpdateModelCmd(DataGridCellEditEndingEventArgs p)
        {
            ModelViewModel mvm = p.Row.DataContext as ModelViewModel;
            string oldName = mvm.ModelModel.Location + "\\" + mvm.ModelModel.ModelName;
            mvm.ModelModel.ModelName = (p.EditingElement as TextBox).Text;
            string newName = (p.EditingElement as TextBox).Text;
            string args = oldName + " " + newName;
            //调用PYTHON修改文件夹
            var t = new Task(() =>
            {
                Process process = CmdHelper.RunProcess(@"Util/changeModelName.exe", args);
                process.Start();

                Console.WriteLine("wait for exit");
                process.WaitForExit();
                Console.WriteLine("exited");
                process.Close();
                Console.WriteLine("closed");
            });
            t.Start();
            t.ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    mvm.ModelModel.UpdateTime = DateTime.Now.ToString();
                    int result = ModelService.GetService().UpdateModel(mvm);
                }
                else if (task.IsCanceled)
                    MessageBox.Show("已取消");
                else if (task.IsFaulted)
                    MessageBox.Show("任务失败");
            });
        }

        public RelayCommand<ModelViewModel> DeleteModelCmd { get; private set; }

        private bool CanExecuteDeleteModelCmd(ModelViewModel arg)
        {
            return arg != null;
        }

        private void ExecuteDeleteModelCmd(ModelViewModel p)
        {
            if (p == null)
            {
                MessageBox.Show("还未选择要删除的对象");
                return;
            }
            int result = ModelService.GetService().DeleteModel(p);
            if (result > 0)
            {
                foreach (var item in Models)
                {
                    if (item.Id == p.Id)
                    {
                        models.Remove(item);
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("删除失败，该数据可能已被更改");
            }
        }
        #endregion

        #region 辅助方法
        private void AssignCommands()
        {
            LoadedCmd = new RelayCommand(() => ExecuteLoadCmd());
            AddModelCmd = new RelayCommand(() => ExecuteAddModelCmd(), CanExecuteAddModelCmd);
            UpdateModelCmd = new RelayCommand<DataGridCellEditEndingEventArgs>((p) => ExecuteUpdateModelCmd(p));
            DeleteModelCmd = new RelayCommand<ModelViewModel>((p) => ExecuteDeleteModelCmd(p), CanExecuteDeleteModelCmd);
        }
        #endregion
    }

    #region 辅助类
    public class ModelViewModel : ViewModelBase
    {
        private ModelModel modelModel;
        public ModelModel ModelModel
        {
            get
            {
                return modelModel;
            }
            set
            {
                modelModel = value;
                RaisePropertyChanged(() => ModelModel);
            }
        }

        private int id;
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
    }
    #endregion
}
