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
    public class TrainViewModel:ViewModelBase
    {
        public TrainViewModel()
        {
            Model = new ModelModel();
        }
        private ModelModel model;
        public ModelModel Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
                RaisePropertyChanged(() => Model);
            }
        }

        private RelayCommand<string> folderBrowserCmd;
        public RelayCommand<string> FolderBrowserCmd
        {
            get
            {
                if (folderBrowserCmd == null)
                    return new RelayCommand<string>((p) => ExecuteFolderBrowserCmd(p));
                return folderBrowserCmd;
            }
            set
            {
                FolderBrowserCmd = value;
            }
        }

        private void ExecuteFolderBrowserCmd(string p)
        {
            string path = FileDialogService.GetService().OpenFolderBrowserDialog();
            switch(p)
            {
                case "S":
                    {
                        Model.Simple = path;
                        break;
                    }
                case "O":
                    {
                        Model.Location = path;
                        break;
                    }
            }
        }

        private RelayCommand<string> openFileCmd;
        public RelayCommand<string> OpenFileCmd
        {
            get
            {
                if (openFileCmd == null)
                    return new RelayCommand<string>((p) => ExecuteOpenFileCmd(p));
                return openFileCmd;
            }
            set
            {
                OpenFileCmd = value;
            }
        }

        private void ExecuteOpenFileCmd(string p)
        {
            Model.SourceModel = FileDialogService.GetService().OpenFileDialog();
        }

        private RelayCommand trainCmd;
        public RelayCommand TrainCmd
        {
            get
            {
                if (trainCmd == null)
                    return new RelayCommand(() => ExecuteTrainCmd());
                return trainCmd;
            }
            set
            {
                TrainCmd = value;
            }
        }

        /// <summary>
        /// 1存储模型数据到数据库
        /// 2处理模型
        /// </summary>
        private void ExecuteTrainCmd()
        {
            //存储逻辑
            DateTime dt = DateTime.Now;
            model.CreateTime = dt.ToString();
            int result = ModelService.GetService().AddModel(Model);
            if(result>0)
            {
                MessageBox.Show(""+result);
                //训练部分
            }
        }
    }
}
