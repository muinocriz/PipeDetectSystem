using MvvmLight4.Model;
using MvvmLight4.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Service
{
    interface IMetaService
    {
        int InsertData(MetaModel meta);
        int UpdateInterval(string path, int i);
        ObservableCollection<MetaViewModel> SelectAll();
        List<ComplexInfoModel> QueryVideoFramed();
        string QueryFramePathById(int id);
    }
}
