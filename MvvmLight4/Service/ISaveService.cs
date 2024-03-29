﻿using MvvmLight4.Model;
using MvvmLight4.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Service
{
    interface ISaveService
    {
        void SaveXlsxFile(string filePath, Dictionary<string,string> dict, List<AbnormalViewModel> list);
        void SaveXlsxFile(string targetSource, List<ExportModel> exportModelsForExcel, List<AbnormalViewModel> list);
        void SaveDocxFile(string filePath, Object contain);
        void SaveXlsxFileBatch(string targetSource, List<ExportData> exportDatas, Dictionary<int, AbnormalTypeModel> typeDict);
    }
}
