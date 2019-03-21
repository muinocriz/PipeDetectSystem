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
    interface IExportService
    {
        ObservableCollection<ExportModel> SelectAll();
        void UpdateExport(ObservableCollection<ExportModel> exports);
        Dictionary<string, string> SelectChoose();
        List<ExportModel> SelectChooseToList();
        List<ExportData> GetExportListData(List<ExportMeta> l);
    }
}
