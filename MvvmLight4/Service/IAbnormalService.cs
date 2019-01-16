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
    public interface IAbnormalService
    {
        ObservableCollection<AbnormalViewModel> SelectAll(List<int> list);
        int UpdateAbnormalType(int id, int type);
        string SelectFolder(int id);
        List<ComplexInfoModel> QueryVideo();
        List<AbnormalViewModel> ExportByVideoId(int id);
        int AddAbnormal(List<AbnormalModel> abnormalModels);
    }
}
