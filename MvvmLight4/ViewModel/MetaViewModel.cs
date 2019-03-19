using GalaSoft.MvvmLight;
using MvvmLight4.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.ViewModel
{
    public class MetaViewModel : ViewModelBase
    {
        public MetaViewModel()
        {
            IsChoose = 0;
        }
        private int? isChoose;
        public int? IsChoose { get => isChoose; set { isChoose = value; RaisePropertyChanged(() => IsChoose); } }
        private int? id;
        public int? Id { get { return id; } set { id = value; } }

        private MetaModel meta;
        /// <summary>
        /// 所有模型列表
        /// </summary>
        public MetaModel Meta
        {
            get { return meta; }
            set
            {
                meta = value;
                RaisePropertyChanged(() => Meta);
            }
        }
    }
}
