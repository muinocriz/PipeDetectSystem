using GalaSoft.MvvmLight;
using MvvmLight4.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Model
{
    public class PlayerModel: ObservableObject
    {
        private int target;
        //目标帧号
        public int Target { get => target; set { target = value; RaisePropertyChanged(() => Target); } }
        private int speed;
        //模仿速度
        public int Speed { get => speed; set { speed = value; RaisePropertyChanged(() => Speed); } }
        private string folder;
        //该帧所在的文件夹绝对路径
        public string Folder { get => folder; set { folder = value; RaisePropertyChanged(() => Folder); } }
        private int startNum;
        //开始帧帧号
        public int StartNum { get => startNum; set { startNum = value; RaisePropertyChanged(() => StartNum); } }
        private int endNum;
        //结束帧帧号
        public int EndNum { get => endNum; set { endNum = value; RaisePropertyChanged(() => EndNum); } }

        public PlayerModel()
        {
            Target = 0;
            Speed = 40;
            Folder = "";
        }
        //计算StartNum和EndNum
        public void Calculate(int length,int Duration=120)
        {
            Console.WriteLine("长度：" + length);
            if(length<=250)
            {
                StartNum = 0;
                EndNum = length - 1;
            }
            else if (length-Target<250)
            {
                StartNum = length - Duration;
                EndNum = 2*target-Duration-(length-1);
            }
            else
            {
                StartNum = Target - Duration;
                endNum = Target + Duration;
            }
            Console.WriteLine("Target：" + Target);
            Console.WriteLine("StartNum：" + StartNum);
            Console.WriteLine("EndNum：" + EndNum);
        }
    }
}
