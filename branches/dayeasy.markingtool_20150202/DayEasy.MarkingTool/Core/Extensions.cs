# region 四川得一科技有限公司 版权所有
/* ================================================
 * 公司：四川得一科技有限公司
 * 作者：文杰
 * 创建：2013-10-30
 * 描述：扩展方法类
 * ================================================
 */
# endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DayEasy.MarkingTool.Core
{
    public static class Extensions
    {
        #region 显示弹出对话框
        /// <summary>
        /// 显示弹出对话框
        /// </summary>
        /// <param name="window">当前窗口</param>
        /// <returns></returns>
        public static bool? ShowDeyiDialog(this Window window)
        {
            return ShowDeyiDialog(window, null);
        }

        /// <summary>
        /// 显示弹出对话框
        /// </summary>
        /// <param name="window">当前窗口</param>
        /// <param name="owner">弹出窗口拥有者</param>
        /// <returns></returns>
        public static bool? ShowDeyiDialog(this Window window, Window owner)
        {
            if (owner == null)
            {
                owner = Application.Current.MainWindow;
            }

            window.Owner = owner;
            window.SourceInitialized += (sender, e) => { window.ShowInTaskbar = false; };
            return window.ShowDialog();
        }
        #endregion

        private const int N = 3;

        /// <summary>
        /// 转化普通图像到2值图像
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="value">阀值</param>
        /// <returns></returns>
        public static Bitmap ConvertImageTo2Value(this Bitmap bmp, int value)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                // 将原始图片变成灰度二位数组
                var p = (byte*)data.Scan0;
                var vSource = new byte[w, h];
                int offset = data.Stride - w * N;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        vSource[x, y] = (byte)(((int)p[0] + (int)p[1] + (int)p[2]) / 3);
                        p += N;
                    }
                    p += offset;
                }

                bmp.UnlockBits(data);

                // 将灰度二位数组变成二值图像
                var bmpDest = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                BitmapData dataDest = bmpDest.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);

                p = (byte*)dataDest.Scan0;
                offset = dataDest.Stride - w * N;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        //p[0] = p[1] = p[2] = (int)vSource[x, y] > 160 ? (byte)255 : (byte)0;
                        p[0] = p[1] = p[2] = (int)GetAverageColor(vSource, x, y, w, h) > value ? (byte)255 : (byte)0;
                        p += N;
                    }
                    p += offset;
                }

                bmpDest.UnlockBits(dataDest);

                // return
                return bmpDest;
            }
        }

        private static byte GetAverageColor(byte[,] vSource, int x, int y, int w, int h)
        {
            int rs = vSource[x, y]
                + (x == 0 ? 255 : (int)vSource[x - 1, y])
                + (x == 0 || y == 0 ? 255 : (int)vSource[x - 1, y - 1])
                + (x == 0 || y == h - 1 ? 255 : (int)vSource[x - 1, y + 1])
                + (y == 0 ? 255 : (int)vSource[x, y - 1])
                + (y == h - 1 ? 255 : (int)vSource[x, y + 1])
                + (x == w - 1 ? 255 : (int)vSource[x + 1, y])
                + (x == w - 1 || y == 0 ? 255 : (int)vSource[x + 1, y - 1])
                + (x == w - 1 || y == h - 1 ? 255 : (int)vSource[x + 1, y + 1]);
            return (byte)(rs / 9);
        }

        /// <summary>
        /// 转换image
        /// </summary>
        /// <param name="srs"></param>
        /// <returns></returns>
        //public static System.Drawing.Bitmap BitmapSourceToBitmap(this BitmapSource srs)
        //{
        //    System.Drawing.Bitmap btm = null;

        //    int width = srs.PixelWidth;

        //    int height = srs.PixelHeight;

        //    int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);

        //    byte[] bits = new byte[height * stride];

        //    srs.CopyPixels(bits, stride, 0);

        //    unsafe
        //    {

        //        fixed (byte* pB = bits)
        //        {
        //            IntPtr ptr = new IntPtr(pB);

        //            btm = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed, ptr);
        //        }
        //    }
        //    return btm;
        //}


        public static Bitmap BitmapSourceToBitmap(this BitmapSource srs)
        {
            Bitmap btm = null;
            int width = srs.PixelWidth;
            int height = srs.PixelHeight;
            int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = Marshal.AllocHGlobal(height * stride);
            srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
            btm = new Bitmap(width, height, stride, PixelFormat.Format1bppIndexed, ptr);
            return btm;
        }

        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static Image CompressImageSize(this Image bmp, int maxLength)
        {

            var width = bmp.Width;
            var height = bmp.Height;

            if (width <= maxLength && height <= maxLength)
                return bmp;

            float max  = Math.Max(width, height);
            if (max <= 0) return bmp;

            float bl = maxLength/max;
            width = (int) Math.Round(bl*width);
            height = (int) Math.Round(bl*height);

            var d = new Bitmap(width, height);
            var g = Graphics.FromImage(d);
            try
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                g.DrawImage(bmp,
                    new RectangleF(0, 0, width, height),
                    new RectangleF(0, 0, bmp.Width, bmp.Height),
                    GraphicsUnit.Pixel);
            }
            catch (Exception)
            {
                d.Dispose();
            }
            finally
            {
                g.Dispose();
                bmp.Dispose();
            }
            return d;
        }


        public static string GetDescription(this Enum enumType)
        {
            object[] customAttributes = Array.Find<FieldInfo>(enumType.GetType().GetFields(), fieldInfo => enumType.ToString() == fieldInfo.Name).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (customAttributes.Length > 0)
            {
                return ((DescriptionAttribute)customAttributes[0]).Description;
            }
            return string.Empty;
        }
    }
}
