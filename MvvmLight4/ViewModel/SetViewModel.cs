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
            InitAbnormalType();
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
                MessageBox.Show("已保存");
            }
            catch (Exception e)
            {
                MessageBox.Show("在存储设置时发生错误：" + e.ToString());
            }
        }

        #region helper function
        private void AssignCommands()
        {
            SelectAbnormal = new RelayCommand(() => ExecuteSelect());
        }

        private void InitAbnormalType()
        {
            AbnormalTypes = AbnormalService.GetService().GetAbnormalTypeModels();
            if (File.Exists("test.txt"))
            {
                try
                {
                    string data = File.ReadAllText("test.txt");
                    foreach (var item in JsonConvert.DeserializeObject<ObservableCollection<AbnormalTypeModel>>(data))
                    {
                        for (int i = 0; i < AbnormalTypes.Count; i++)
                        {
                            if (AbnormalTypes[i].Id == item.Id)
                            {
                                AbnormalTypes[i].IsSelected = true;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    foreach (var item in AbnormalTypes)
                    {
                        item.IsSelected = false;
                    }
                }
            }
            if (AbnormalTypes == null || AbnormalTypes.Count == 0)
            {
                AbnormalTypes = AbnormalService.GetService().GetAbnormalTypeModels();
            }
        }
        #endregion
    }
}
