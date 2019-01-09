using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Model
{
    public class MarkModel : ObservableObject
    {
        public MarkModel() { }
        public MarkModel(string _p,  int _t, double _x, double _y, string _path)
        {
            Position = _p;
            Type = _t;
            X = _x;
            Y = _y;
            Path = _path;
        }
        private string position;
        /// <summary>
        /// 异常所在帧号
        /// </summary>
        public string Position { get => position; set { position = value; RaisePropertyChanged(() => Position); } }
        private int type;
        /// <summary>
        /// 异常类型
        /// 0 - 无异常
        /// 其他 - 其他异常
        /// </summary>
        public int Type { get => type; set { type = value; RaisePropertyChanged(() => Type); } }
        public double x;
        /// <summary>
        /// 中心x坐标
        /// </summary>
        public double X { get => x; set { x = value; RaisePropertyChanged(() => X); } }
        public double y;
        /// <summary>
        /// 中心y坐标
        /// </summary>
        public double Y { get => y; set { y = value; RaisePropertyChanged(() => Y); } }
        private string path;
        /// <summary>
        /// 保存路径
        /// </summary>
        public string Path { get => path; set { path = value; RaisePropertyChanged(() => Path); } }
        //public int w;
        ///// <summary>
        ///// 截图宽
        ///// </summary>
        //public int W { get => w; set { w = value; RaisePropertyChanged(() => W); } }
        //public int h;
        ///// <summary>
        ///// 截图高
        ///// </summary>
        //public int H { get => h; set { h = value; RaisePropertyChanged(() => H); } }

    }
}
