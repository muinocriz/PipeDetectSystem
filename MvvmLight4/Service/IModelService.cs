﻿using MvvmLight4.Model;
using MvvmLight4.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Service
{
    interface IModelService
    {
        ObservableCollection<ModelViewModel> LoadData();
        int UpdateModel(ModelViewModel modelViewModel);
        int DeleteModel(ModelViewModel modelViewModel);
        int AddModel(ModelModel modelModel);
        int TransferModel(ModelModel addModel);
    }
}
