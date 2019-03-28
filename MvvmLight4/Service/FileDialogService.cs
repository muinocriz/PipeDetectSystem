﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MvvmLight4.Service
{
    class FileDialogService : IFileDialogService
    {
        private static FileDialogService fileDialogService = new FileDialogService();
        private FileDialogService(){ }
        public static FileDialogService GetService() { return fileDialogService; }



        /// <summary>
        /// 打开文件选择对话框，选择一个文件
        /// </summary>
        /// <param name="srcFilter">默认初始位置，可为空</param>
        /// <returns>文件绝对位置字符串</returns>
        public string OpenFileDialog(string srcFilter = "")
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if(!string.IsNullOrEmpty(srcFilter))
                dialog.Filter = srcFilter;
            dialog.ShowDialog();
            return dialog.FileName;
        }

        public IList<string> OpenFileDialogMultiselect(string srcFilter = "")
        {
            return null;
        }

        /// <summary>
        /// 打开路径选择对话框
        /// </summary>
        /// <returns></returns>
        public string OpenFolderBrowserDialog()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            return dialog.SelectedPath;
        }

        /// <summary>
        /// 打开另存为对话框
        /// </summary>
        /// <returns></returns>
        public string OpenSaveFileDialog(int way)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "export";
            if(way==1)
            {
                dialog.DefaultExt = ".docx";
                dialog.Filter = "Word 文档(*.docx)|*.docx";
            }
            else
            {
                dialog.DefaultExt = ".xlsx";
                dialog.Filter = "Excel 工作簿(*.xlsx)|*.xlsx";
            }
            
            var result = dialog.ShowDialog();
            if (result == true)
                return dialog.FileName;
            return "";
        }
    }
}
