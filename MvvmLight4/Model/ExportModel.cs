using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Model
{
    public class ExportModel : ObservableObject
    {
        private string alternative;
        private int isChoose;
        private string byname;
        public string Alternative { get => alternative; set { alternative = value;RaisePropertyChanged(() => Alternative); } }
        public int IsChoose { get => isChoose; set { isChoose = value; RaisePropertyChanged(() => IsChoose); } }
        public string Byname { get => byname; set { byname = value; RaisePropertyChanged(() => Byname); } }

    }
}
