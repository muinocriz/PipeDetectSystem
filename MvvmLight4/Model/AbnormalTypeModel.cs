using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Model
{
    /// <summary>
    /// 存储了异常类型与名字和分类的对应关系的类
    /// </summary>
    public class AbnormalTypeModel : ObservableObject
    {
        public AbnormalTypeModel() { }

        public AbnormalTypeModel(int id, int type, string name, string category, bool isSelected)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.category = category;
            this.isSelected = isSelected;
        }

        public AbnormalTypeModel(int type, string name, string category, bool isSelected)
        {
            this.type = type;
            this.name = name;
            this.category = category;
            this.isSelected = isSelected;
        }

        public AbnormalTypeModel(int type, string name, string category)
        {
            Type = type;
            Name = name;
            Category = category;
        }

        private int id;
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get => id; set { id = value; RaisePropertyChanged(() => Id); } }

        private int type;
        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get => type; set { type = value; RaisePropertyChanged(() => Type); } }

        private string name;
        /// <summary>
        /// 异常名字
        /// </summary>
        public string Name { get => name; set { name = value; RaisePropertyChanged(() => Name); } }

        private string category;
        /// <summary>
        /// 异常分类 局部异常/全局异常
        /// </summary>
        public string Category { get => category; set { category = value; RaisePropertyChanged(() => Category); } }
        private bool isSelected;
        public bool IsSelected { get => isSelected; set { isSelected = value; RaisePropertyChanged(() => IsSelected); } }
    }
}
