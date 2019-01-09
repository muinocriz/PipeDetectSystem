using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Model
{
    public class MetaModel : ObservableObject
    {
        private string pipeCode;
        public string PipeCode { get => pipeCode; set {pipeCode=value;RaisePropertyChanged(() => PipeCode); } }
        private int pipeType;
        public int PipeType { get => pipeType; set { pipeType = value; RaisePropertyChanged(() => PipeType); } }
        private string taskCode;
        public string TaskCode { get => taskCode; set { taskCode = value; RaisePropertyChanged(() => TaskCode); } }
        private string address;
        public string Address { get => address; set { address = value; RaisePropertyChanged(() => Address); } }
        private string charge;
        public string Charge { get => charge; set { charge = value; RaisePropertyChanged(() => Charge); } }
        private string startTime;
        public string StartTime { get => startTime; set { startTime = value; RaisePropertyChanged(() => StartTime); } }
        private string endTime;
        public string EndTime { get => endTime; set { endTime = value; RaisePropertyChanged(() => EndTime); } }
        private int? interval;
        public int? Interval { get => interval;set { interval = value;RaisePropertyChanged(() => Interval); } }
        private string videoPath;
        public string VideoPath { get => videoPath; set { videoPath = value; RaisePropertyChanged(() => VideoPath); } }
    }
}
