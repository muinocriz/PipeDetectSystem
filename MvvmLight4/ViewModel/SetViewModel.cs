using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmLight4.Model;
using MvvmLight4.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmLight4.ViewModel
{
    public class SetViewModel : ViewModelBase
    {
        public SetViewModel()
        {
            AssignCommands();
            AbnormalTypes = AbnormalService.GetService().GetAbnormalTypeModels();
        }

        private ObservableCollection<AbnormalTypeModel> abnormalTypes;
        public ObservableCollection<AbnormalTypeModel> AbnormalTypes
        {
            get => abnormalTypes; set { abnormalTypes = value; RaisePropertyChanged(() => AbnormalTypes); }
        }
        public ObservableCollection<AbnormalTypeModel> SelectedAbnormalTypes { get; private set; }

        public RelayCommand SelectAbnormal { get; private set; }
        private void ExecuteSelect()
        {
            SelectedAbnormalTypes = new ObservableCollection<AbnormalTypeModel>();
            foreach (var item in AbnormalTypes)
            {
                if (item.IsSelected)
                    SelectedAbnormalTypes.Add(item);
            }

            string json = JsonConvert.SerializeObject(SelectedAbnormalTypes);

            try
            {
                File.WriteAllText("test.txt", json);
            }
            catch (Exception e)
            {
                MessageBox.Show("存储异常！\n" + e.ToString());
            }

            MessageBox.Show("已保存");
        }

        #region helper function
        private void AssignCommands()
        {
            SelectAbnormal = new RelayCommand(() =>ExecuteSelect());
        }
        #endregion
    }
}
