using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmLight4.Common
{
    public class ImageHelper
    {
        /// <summary>
        /// 裁剪图片
        /// </summary>
        /// <param name="picPath">原始图片路径</param>
        /// <param name="x">显示图像的中心横坐标  /   显示图像宽度</param>
        /// <param name="y">显示图像中心纵坐标    /   显示图像高度</param>
        /// <param name="width">截图宽</param>
        /// <param name="height">截图高</param>
        public static void Caijianpic(String picPath, String savePath, double x, double y, int width, int height)
        {
            using (System.Drawing.Image img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(System.IO.File.ReadAllBytes(picPath))))
            {
                //定义截取矩形
                int a = Convert.ToInt32(img.Width * x - width / 2);
                int b = Convert.ToInt32(img.Height * y - height / 2);
                System.Drawing.Rectangle cropArea = new System.Drawing.Rectangle(a, b, width, height);

                //判断超出的位置否
                if ((img.Width < a + width / 2) || img.Height < b + height / 2)
                {
                    img.Dispose();
                    return;
                }

                //定义Bitmap对象
                using (System.Drawing.Bitmap bmpImage = new System.Drawing.Bitmap(img))
                {
                    using (System.Drawing.Bitmap bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat))
                    {
                        bmpCrop.Save(savePath);
                        bmpCrop.Dispose();
                    }
                    bmpImage.Dispose();
                }
            }
        }

        /// <summary>
        /// 对于全局异常，直接在新位置存储照片，并重命名即可
        /// </summary>
        /// <param name="picPath">原始文件绝对路径</param>
        /// <param name="savePath">新位置</param>
        public static void SavePic(String picPath, String savePath)
        {
            try
            {
                File.Copy(picPath, savePath, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine("保存 {0} 到 {1} 失败", picPath, savePath);
                Debug.WriteLine("原因：{0}", e.ToString());
            }
        }
    }
}
