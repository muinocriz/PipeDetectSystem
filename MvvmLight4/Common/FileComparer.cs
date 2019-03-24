using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Common
{
    class FileComparer : IComparer<string>
    {
        /// <summary>
        /// 文件逻辑排序
        /// </summary>
        /// <param name="psz1"></param>
        /// <param name="psz2"></param>
        /// <returns></returns>
        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
    }
}
