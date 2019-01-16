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
        public AbnormalModel(int _videoId,string _position,int _type)
        {
            VideoId = _videoId;
            Position = _position;
            Type = _type;
        }
        private int videoId;
        /// <summary>
        /// 异常所属于的视频的Id
        /// </summary>
        public int VideoId { get => videoId; set { videoId = value;RaisePropertyChanged(() => VideoId); } }
        private int type;
        /// <summary>
        /// 异常类型
        /// 0 - 无异常
        /// 其他 - 其他异常
        /// </summary>
        public int Type { get => type; set { type = value; RaisePropertyChanged(() => Type); } }
        private string position;
        /// <summary>
        /// 异常所在帧号
        /// </summary>
        public string Position { get => position; set { position = value;RaisePropertyChanged(() => Position); } }
    }
}
