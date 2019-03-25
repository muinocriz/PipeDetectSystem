using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmLight4.Model;
using MvvmLight4.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmLight4.ViewModel
{
    public class ImportViewModel : ViewModelBase
    {
        public ImportViewModel()
        {
            AssignCommands();

            Meta = new MetaModel();
        }

        private MetaModel meta;
        /// <summary>
        /// 元数据
        /// </summary>
        public MetaModel Meta
        {
            get
            {
                return meta;
            }
            set
            {
                meta = value;
                RaisePropertyChanged(() => Meta);
            }
        }

        public RelayCommand SubmitCmd { get; private set; }

        private bool CanExecuteSubmitCmd()
        {
            return !(string.IsNullOrEmpty(Meta.VideoPath) || string.IsNullOrEmpty(Meta.PipeCode) || string.IsNullOrEmpty(Meta.TaskCode) || string.IsNullOrEmpty(Meta.Addr) || string.IsNullOrEmpty(Meta.Charge));
        }

        private void ExecuteSubmitCmd()
        {
            List<string> errorMsgs = new List<string>();

            string tag = Meta.PipeCode;
            if (tag.Split('-').Length != 2)
            {
                errorMsgs.Add(@"管线编号不正确,应为“起始井号-终止井号”");
            }

            string hTag = Meta.HeadTime;
            if (hTag.Split(':').Length != 2)
            {
                errorMsgs.Add(@"开头时间格式不正确,应为：分分:秒秒（':'为英文冒号）");
            }
            else if (!(IsInt(hTag.Split(':')[0]) && IsInt(hTag.Split(':')[1])))
            {
                errorMsgs.Add(@"输入的开头时间未能转换为整数");
            }

            string tTag = Meta.TailTime;
            if (tTag.Split(':').Length != 2)
            {
                errorMsgs.Add(@"尾部时间格式不正确,应为：分分:秒秒（':'为英文冒号）");
            }
            else if (!(IsInt(tTag.Split(':')[0]) && IsInt(tTag.Split(':')[1])))
            {
                errorMsgs.Add(@"输入的尾部时间未能转换为整数");
            }

            string msg = string.Empty;
            if (errorMsgs!=null && errorMsgs.Count()>0)
            {
                for(int i=0;i< errorMsgs.Count();i++)
                {
                    msg = msg + "\n" + (i+1) + " " + errorMsgs[i];
                }
                MessageBox.Show(msg);
                return;
            }

            int result = MetaService.GetService().InsertData(Meta);
            if (result == 1)
            {
                MessageBox.Show("导入成功");
            }
        }

        public RelayCommand OpenFileDialogCmd { get; private set; }

        private void ExecuteOpenFileDialogCmd()
        {
            string filter = @"视频文件|*.avi;*.mp4;*.wmv;*.mpeg|所有文件|*.*";
            Meta.VideoPath = FileDialogService.GetService().OpenFileDialog(filter);
        }

        #region helper function
        private void AssignCommands()
        {
            OpenFileDialogCmd = new RelayCommand(() => ExecuteOpenFileDialogCmd());
            SubmitCmd = new RelayCommand(() => ExecuteSubmitCmd(), CanExecuteSubmitCmd);
        }

        public static bool IsInt(string value)
        {
            Console.WriteLine("value:"+value);
            try
            {
                Convert.ToInt32(value);
                Console.WriteLine("true");
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("false");
                return false;
            }
        }
        #endregion
    }

}
