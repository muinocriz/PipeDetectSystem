using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmLight4.ViewModel
{
    public class ImportViewModel : ViewModelBase
    {
        public ImportViewModel()
        {
            Meta = new MetaModel();
        }
        private MetaModel meta;
        public MetaModel Meta
        {
            get
            {
                return meta;
            }
            set
            {
                meta = value;
                RaisePropertyChanged(() => Meta);
            }
        }

        private RelayCommand submitCmd;
        public RelayCommand SubmitCmd
        {
            get
            {
                if(submitCmd==null)
                {
                    return new RelayCommand(() => ExecuteSubmitCmd());
                }
                return submitCmd;
            }
            set
            {
                SubmitCmd = value;
            }
        }

        private void ExecuteSubmitCmd()
        {
            int result = MetaService.GetService().InsertData(Meta);
            if(result ==1)
            {
                MessageBox.Show("导入成功");
            }
        }

        private RelayCommand openFileDialogCmd;
        public RelayCommand OpenFileDialogCmd
        {
            get
            {
                if(openFileDialogCmd == null)
                {
                    return new RelayCommand(() => ExecuteOpenFileDialogCmd());
                }
                return openFileDialogCmd;
            }
            set
            {
                OpenFileDialogCmd = value;
            }
        }

        private void ExecuteOpenFileDialogCmd()
        {
            Meta.VideoPath = FileDialogService.GetService().OpenFileDialog();
        }
    }

}
