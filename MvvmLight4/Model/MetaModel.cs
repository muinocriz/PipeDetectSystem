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
        /// 起始井号-终止井号
        /// </summary>
        public string PipeCode { get => pipeCode; set { pipeCode = value; RaisePropertyChanged(() => PipeCode); } }

        private int pipeType;
        /// <summary>
        /// 管线类型
        /// </summary>
        public int PipeType { get => pipeType; set { pipeType = value; RaisePropertyChanged(() => PipeType); } }

        private string taskCode;
        /// <summary>
        /// 项目名称
        /// </summary>
        public string TaskCode { get => taskCode; set { taskCode = value; RaisePropertyChanged(() => TaskCode); } }

        private string address;
        /// <summary>
        /// 道路名称
        /// </summary>
        public string Address { get => address; set { address = value; RaisePropertyChanged(() => Address); } }

        private string charge;
        /// <summary>
        /// 任务负责人
        /// </summary>
        public string Charge { get => charge; set { charge = value; RaisePropertyChanged(() => Charge); } }

        private string startTime;
        /// <summary>
        /// 项目时间
        /// </summary>
        public string StartTime { get => startTime; set { startTime = value; RaisePropertyChanged(() => StartTime); } }

        //private string endTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        //public string EndTime { get => endTime; set { endTime = value; RaisePropertyChanged(() => EndTime); } }

        private int? interval;
        /// <summary>
        /// 分帧间隔
        /// </summary>
        public int? Interval { get => interval; set { interval = value; RaisePropertyChanged(() => Interval); } }

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
        /// <summary>
        /// 视频头部时间
        /// </summary>
        public string HeadTime { get => headTime; set { headTime = value; RaisePropertyChanged(() => HeadTime); } }

        private string tailTime;
        /// <summary>
        /// 视频尾部时间
        /// </summary>
        public string TailTime { get => tailTime; set { tailTime = value; RaisePropertyChanged(() => TailTime); } }

        private int sfzd;
        /// <summary>
        /// 有无管线点
        /// </summary>
        public int SFZD { get => sfzd; set => sfzd = value; }

        private string gxddh;
        /// <summary>
        /// 管线点点号
        /// </summary>
        public string GXDDH
        {
            get { return gxddh; }
            set { gxddh = value; }
        }

        private int gxdgs;
        /// <summary>
        /// 管线点个数
        /// </summary>
        public int GXDGS
        {
            get { return gxdgs; }
            set { gxdgs = value; }
        }

        private string gc;
        /// <summary>
        /// 管材
        /// </summary>
        public string GC
        {
            get { return gc; }
            set { gc = value; }
        }

        private double gj;
        /// <summary>
        /// 管径
        /// 单位：mm
        /// </summary>
        public double GJ
        {
            get { return gj; }
            set { gj = value; }
        }

        private int jcfx;
        /// <summary>
        /// 检测方向
        /// </summary>
        public int JCFX
        {
            get { return jcfx; }
            set { jcfx = value; }
        }

        private double gxc;
        /// <summary>
        /// 管线长
        /// 单位：m
        /// </summary>
        public double GXC
        {
            get { return gxc; }
            set { gxc = value; }
        }

        private double qsms;
        /// <summary>
        /// 起始埋深
        /// 单位：m
        /// </summary>
        public double QSMS
        {
            get { return qsms; }
            set { qsms = value; }
        }

        private double zzms;
        /// <summary>
        /// 终止埋深
        /// 单位：m
        /// </summary>
        public double ZZMS
        {
            get { return zzms; }
            set { zzms = value; }
        }
    }
}
