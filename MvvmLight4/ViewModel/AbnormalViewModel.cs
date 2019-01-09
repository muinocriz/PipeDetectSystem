using GalaSoft.MvvmLight;
using MvvmLight4.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.ViewModel
{
    public class AbnormalViewModel:ViewModelBase
    {
        private AbnormalModel abnormal;
        public AbnormalModel Abnormal { get => abnormal; set { abnormal = value; RaisePropertyChanged(() => Abnormal); } }
        private MetaModel meta;
        public MetaModel Meta { get => meta; set { meta = value; RaisePropertyChanged(() => Meta); } }
        private int abnormalId;
        public int AbnormalId { get => abnormalId; set { abnormalId = value; RaisePropertyChanged(() => AbnormalId); } }

    }
}
