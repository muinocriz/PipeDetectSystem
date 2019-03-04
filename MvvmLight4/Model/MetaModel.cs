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
        /// <summary>
        /// 管道编号
        /// </summary>
        public string PipeCode { get => pipeCode; set {pipeCode=value;RaisePropertyChanged(() => PipeCode); } }
        private int pipeType;
        /// <summary>
        /// 管道类型
        /// </summary>
        public int PipeType { get => pipeType; set { pipeType = value; RaisePropertyChanged(() => PipeType); } }
        private string taskCode;
        /// <summary>
        /// 任务编号
        /// </summary>
        public string TaskCode { get => taskCode; set { taskCode = value; RaisePropertyChanged(() => TaskCode); } }
        private string address;
        /// <summary>
        /// 任务地址
        /// </summary>
        public string Address { get => address; set { address = value; RaisePropertyChanged(() => Address); } }
        private string charge;
        /// <summary>
        /// 任务负责人
        /// </summary>
        public string Charge { get => charge; set { charge = value; RaisePropertyChanged(() => Charge); } }
        private string startTime;
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get => startTime; set { startTime = value; RaisePropertyChanged(() => StartTime); } }
        private string endTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get => endTime; set { endTime = value; RaisePropertyChanged(() => EndTime); } }
        private int? interval;
        /// <summary>
        /// 分帧间隔
        /// </summary>
        public int? Interval { get => interval;set { interval = value;RaisePropertyChanged(() => Interval); } }
        private string videoPath;
        /// <summary>
        /// 视频路径
        /// </summary>
        public string VideoPath { get => videoPath; set { videoPath = value; RaisePropertyChanged(() => VideoPath); } }
        private string framePath;
        /// <summary>
        /// 分帧图片路径
        /// </summary>
        public string FramePath { get => framePath; set { framePath = value; RaisePropertyChanged(() => FramePath); } }
        private string headTime;
        public string HeadTime { get => headTime; set { headTime = value; RaisePropertyChanged(() => HeadTime); } }
        private string tailTime;
        public string TailTime { get => tailTime; set { tailTime = value; RaisePropertyChanged(() => TailTime); } }
    }
}
