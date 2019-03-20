using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MvvmLight4.ViewModel
{
    public class FrameFileChooseViewModel : ViewModelBase
    {
        public FrameFileChooseViewModel()
        {
            InitData();
            AssignCommands();
        }

        #region property
        private ObservableCollection<MetaModel> metas;
        public ObservableCollection<MetaModel> Metas
        {
            get
            {
                return metas;
            }
            set
            {
                metas = value;
                RaisePropertyChanged(() => Metas);
            }
        }
        #endregion
        public RelayCommand<object> SubmitCmd { get; private set; }
        private void ExecuteSubmitCmd(Object o)
        {
            DataGrid d = (DataGrid)o;

            List<MetaModel> l = new List<MetaModel>();
            foreach (var item in d.SelectedItems)
            {
                l.Add(item as MetaModel);
            }
            foreach (var item in l)
            {
                Debug.WriteLine(item.PipeCode);
            }

            // send message
            Messenger.Default.Send(l, "FrameList");
            //close window
            Messenger.Default.Send("closeWindow", "FFCVM2FFCW");
        }

        #region helper function
        private void InitData()
        {
            metas = MetaService.GetService().SelectNotFrame();
        }

        private void AssignCommands()
        {
            SubmitCmd = new RelayCommand<object>((list) => ExecuteSubmitCmd(list));
        }
        #endregion
    }
}
