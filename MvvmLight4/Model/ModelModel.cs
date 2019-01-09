using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Model
{
    public class ModelModel : ObservableObject
    {
        private string modelName;
        private string location;
        private string createTime;
        private string updateTime;
        private string simple;
        private int? rate;
        private int? lteration;
        private string sourceModel;


        /// <summary>
        /// 模型名称
        /// </summary>
        public string ModelName { get { return modelName; } set { modelName = value; RaisePropertyChanged(() => ModelName); } }

        /// <summary>
        /// 模型位置
        /// </summary>
        public string Location { get { return location; } set { location = value; RaisePropertyChanged(() => Location); } }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get { return createTime; } set { createTime = value; RaisePropertyChanged(() => CreateTime); } }
        
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public string UpdateTime { get => updateTime; set { updateTime = value; RaisePropertyChanged(() => UpdateTime); } }
        /// <summary>
        /// 样本位置
        /// </summary>
        public string Simple { get => simple; set { simple = value; RaisePropertyChanged(() => Simple); } }
        /// <summary>
        /// 学习率
        /// </summary>
        public int? Rate { get => rate; set { rate = value; RaisePropertyChanged(() => Rate); } }
        /// <summary>
        /// 迭代次数
        /// </summary>
        public int? Lteration { get => lteration; set { lteration = value; RaisePropertyChanged(() => Lteration); } }
        /// <summary>
        /// 来自已有模型
        /// 非null：  表示已有模型位置
        /// null：    不是来自已有模型
        /// </summary>
        public string SourceModel { get => sourceModel; set { sourceModel = value; RaisePropertyChanged(() => SourceModel); } }

    }
}
