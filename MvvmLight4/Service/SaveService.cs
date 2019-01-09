using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using System.Collections.Generic;
using System.Text;
using MvvmLight4.ViewModel;
using MvvmLight4.Common;

namespace MvvmLight4.Service
{
    class SaveService : ISaveService
    {
        private static SaveService saveService = new SaveService();
        private SaveService() { }
        public static SaveService GetService() { return saveService; }

        /// <summary>
        /// 保存Word文件,暂未实现
        /// </summary>
        /// <param name="filePath">位置</param>
        /// <param name="contain">内容</param>
        public void SaveDocxFile(string filePath, object contain)
        {

        }

        /// <summary>
        /// 保存为excel
        /// </summary>
        /// <param name="filePath">保存位置</param>
        /// <param name="dict">要保存的属性</param>
        /// <param name="list">内容</param>
        public void SaveXlsxFile(string filePath, Dictionary<string, string> dict, List<AbnormalViewModel> list)
        {
            bool hasAType = false;//是否输出异常类型
            int abnormalTypeColumn = 0;
            bool hasPType = false;//是否输出管道类型
            int pipeTypeColumn = 0;
            IWorkbook workbook = new XSSFWorkbook();
            ISheet s1 = workbook.CreateSheet("Sheet1");
            IRow row = s1.CreateRow(0);
            int i = 0;
            int j = 0;
            foreach (var item in dict)
            {
                if (item.Key == "Type")
                {
                    hasAType = true;
                    abnormalTypeColumn = j;//异常类型存在第j列
                    break;
                }
                j++;
            }
            j = 0;
            foreach (var item in dict)
            {
                if (item.Key == "PipeType")
                {
                    hasPType = true;
                    pipeTypeColumn = j;//管道类型存在第j列
                    break;
                }
                j++;
            }
            j = 0;

            foreach (var item in dict)
            {
                ICell Cell = row.CreateCell(j);
                if (!string.IsNullOrEmpty(item.Value))
                    Cell.SetCellValue(item.Value);
                else
                    Cell.SetCellValue(item.Key);
                j++;
            }

            i = 1;
            foreach (var l in list)
            {
                row = s1.CreateRow(i);
                j = 0;
                Type t = l.GetType();
                Type t1 = l.Meta.GetType();
                Type t2 = l.Abnormal.GetType();
                foreach (var d in dict)
                {
                    ICell Cell = row.CreateCell(j);
                    if (l.Meta.GetType().GetProperty(d.Key) != null)
                    {
                        Cell.SetCellValue(Convert.ToString(t1.GetProperty(d.Key).GetValue(l.Meta)));
                    }
                    else
                    {
                        Cell.SetCellValue(Convert.ToString(t2.GetProperty(d.Key).GetValue(l.Abnormal)));
                    }
                    j++;
                }
                i++;
            }

            if (hasAType)
            {
                s1.GetRow(0).CreateCell(dict.Count).SetCellValue("异常类型");
                for (int k = 1; k < i; k++)
                {
                    ICell Cell = s1.GetRow(k).GetCell(abnormalTypeColumn);
                    s1.GetRow(k).CreateCell(dict.Count).SetCellValue(Convert.ToInt32(Cell.ToString()));
                    switch (Convert.ToInt32(Cell.ToString()))
                    {
                        case 0:
                            {
                                Cell.SetCellValue("无异常");
                                break;
                            }
                        default:
                            {
                                Cell.SetCellValue("异常");
                                break;
                            }
                    }
                }
            }

            if (hasPType)
            {
                for (int k = 1; k < i; k++)
                {
                    ICell Cell = s1.GetRow(k).GetCell(pipeTypeColumn);
                    switch (Convert.ToInt32(Cell.ToString()))
                    {
                        case 0:
                            Cell.SetCellValue("污水");
                            break;
                        case 1:
                            Cell.SetCellValue("雨水");
                            break;
                        default:
                            Cell.SetCellValue("未设置");
                            break;
                    }
                }
            }
            FileStream sw = File.Create(filePath);
            workbook.Write(sw);
            sw.Close();
        }
    }
}
