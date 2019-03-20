using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Model
{
    public class AbnormalModel : ObservableObject
    {
        public AbnormalModel() { }
        public AbnormalModel(int _videoId, string _position, int _type)
        {
            VideoId = _videoId;
            Position = _position;
            Type = _type;
        }
        private int videoId;
        /// <summary>
        /// 异常所属于的视频的Id
        /// </summary>
        public int VideoId { get => videoId; set { videoId = value; RaisePropertyChanged(() => VideoId); } }

        private int type;
        /// <summary>
        /// 异常类型
        /// 0 - 无异常
        /// 其他 - 其他异常
        /// </summary>
        public int Type { get => type; set { type = value; RaisePropertyChanged(() => Type); } }

        private string qpwz;
        /// <summary>
        /// 切片位置
        /// </summary>
        public string QPWZ
        {
            get { return qpwz; }
            set { qpwz = value; }
        }

        private string position;
        /// <summary>
        /// 异常所在帧号
        /// </summary>
        public string Position { get => position; set { position = value; RaisePropertyChanged(() => Position); } }

        private double qxwz;
        /// <summary>
        /// 缺陷位置
        /// 单位：m
        /// </summary>
        public double QXWZ
        {
            get { return qxwz; }
            set { qxwz = value; }
        }

        private int dj;
        /// <summary>
        /// 等级
        /// </summary>
        public int DJ
        {
            get { return dj; }
            set { dj = value; }
        }

        private string szbs;
        /// <summary>
        /// 时钟表示
        /// </summary>
        public string SZBS
        {
            get { return szbs; }
            set { szbs = value; }
        }

        private string bz;
        /// <summary>
        /// 备注
        /// </summary>
        public string BZ
        {
            get { return bz; }
            set { bz = value; }
        }

        private string sfxf;
        /// <summary>
        /// 是否修复
        /// </summary>
        public string SFXF
        {
            get { return sfxf; }
            set { sfxf = value; }
        }

        private string ms;
        /// <summary>
        /// 描述
        /// </summary>
        public string MS
        {
            get { return ms; }
            set { ms = value; }
        }

        private string tpmc;
        /// <summary>
        /// 图片名称
        /// </summary>
        public string TPMC
        {
            get { return tpmc; }
            set { tpmc = value; }
        }
    }
}
