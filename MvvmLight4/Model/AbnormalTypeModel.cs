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

        public AbnormalTypeModel(int id, int type, string name, string category)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.category = category;
        }

        public AbnormalTypeModel(int type, string name, string category)
        {
            this.type = type;
            this.name = name;
            this.category = category;
        }

        /// <summary>
        /// 编号
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 异常名字
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 异常分类 局部异常/全局异常
        /// </summary>
        public string category { get; set; }
    }
}
