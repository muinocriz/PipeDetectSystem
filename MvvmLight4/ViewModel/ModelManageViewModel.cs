using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MvvmLight4.ViewModel
{
    public class ModelManageViewModel : ViewModelBase
    {
        public ModelManageViewModel()
        {
            //Models = ModelService.GetService().LoadData();
            
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

        private RelayCommand loadedCmd;
        public RelayCommand LoadedCmd
        {
            get
            {
                if (loadedCmd == null)
                    return new RelayCommand(() =>
                    {
                        Models = ModelService.GetService().LoadData();
                    });
                return loadedCmd;
            }
            set
            {
                loadedCmd = value;
            }
        }

        private RelayCommand<DataGridCellEditEndingEventArgs> updateModelCmd;
        public RelayCommand<DataGridCellEditEndingEventArgs> UpdateModelCmd
        {
            get
            {
                if(updateModelCmd == null)
                {
                    return new RelayCommand<DataGridCellEditEndingEventArgs>((p) => ExecuteUpdateModelCmd(p));
                }
                return updateModelCmd;
            }
            set
            {
                UpdateModelCmd = value;
            }
        }

        private void ExecuteUpdateModelCmd(DataGridCellEditEndingEventArgs p)
        {
            ModelViewModel mvm = p.Row.DataContext as ModelViewModel;
             mvm.ModelModel.ModelName = (p.EditingElement as TextBox).Text;
            int result = ModelService.GetService().UpdateModel(mvm);
        }

        private RelayCommand<ModelViewModel> deleteModelCmd;
        public RelayCommand<ModelViewModel>  DeleteModelCmd
        {
            get
            {
                if (deleteModelCmd == null)
                    return new RelayCommand<ModelViewModel>((p) => ExecuteDeleteModelCmd(p),CanExecuteDeleteModelCmd);
                return deleteModelCmd;
            }
            set
            {
                DeleteModelCmd = value;
            }
        }

        private bool CanExecuteDeleteModelCmd(ModelViewModel arg)
        {
            return arg != null;
        }

        private void ExecuteDeleteModelCmd(ModelViewModel p)
        {
            if(p==null)
            {
                MessageBox.Show("还未选择要删除的对象");
                return;
            }
            int result = ModelService.GetService().DeleteModel(p);
            if(result>0)
            {
                foreach (var item in Models)
                {
                    if(item.Id == p.Id)
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
    }

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
}
