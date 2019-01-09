using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Model
{
    public class ComplexInfoModel : ObservableObject
    {
        private string key;
        public string Key
        {
            get { return key; }
            set { key = value; RaisePropertyChanged(() => Key); }

        }
        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; RaisePropertyChanged(() => Text); }
        }
    }
}
