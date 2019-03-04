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
            AbnormalTypes = AbnormalService.GetService().GetAbnormalTypeModels();
        }

        private ObservableCollection<AbnormalTypeModel> abnormalTypes;
        public ObservableCollection<AbnormalTypeModel> AbnormalTypes
        {
            get => abnormalTypes; set { abnormalTypes = value; RaisePropertyChanged(() => AbnormalTypes); }
        }
        public ObservableCollection<AbnormalTypeModel> SelectedAbnormalTypes { get; set; }

        private RelayCommand selectAbnormal;
        public RelayCommand SelectAbnormal
        {
            get
            {
                if (selectAbnormal == null)
                    return new RelayCommand(() =>
                    {
                        SelectedAbnormalTypes = new ObservableCollection<AbnormalTypeModel>();
                        foreach (var item in AbnormalTypes)
                        {
                            if (item.IsSelected)
                                SelectedAbnormalTypes.Add(item);
                        }
                        string json = JsonConvert.SerializeObject(SelectedAbnormalTypes);
                        File.WriteAllText("test.txt", json);
                        MessageBox.Show("已保存");
                    });
                return selectAbnormal;
            }
            set
            {
                selectAbnormal = value;
            }
        }


    }
}
