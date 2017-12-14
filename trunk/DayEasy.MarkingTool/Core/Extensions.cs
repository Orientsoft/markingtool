# region 四川得一科技有限公司 版权所有
/* ================================================
 * 公司：四川得一科技有限公司
 * 作者：shoy
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

        private static void InitWindow(this Window window, Window owner = null, bool showInTaskbar = true, bool topMost = false)
        {
            if (owner == null)
            {
                owner = WindowsHelper.OwnerWindow;
            }
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.GotKeyboardFocus += (o, arg) =>
            {
                window.Topmost = topMost;
            };
            if (owner != null)
                window.Owner = owner;
            window.ShowActivated = true;
            window.SourceInitialized += (sender, e) => { window.ShowInTaskbar = showInTaskbar; };
        }

        public static void DeyiWindow(this Window window, Window owner = null, bool showInTaskbar = true,
            bool topMost = false, bool hideOwner = false)
        {
            window.InitWindow(owner, showInTaskbar, topMost);
            window.Show();
            if (window.Owner != null && hideOwner)
                window.Owner.Hide();
        }

        /// <summary>
        /// 显示弹出对话框
        /// </summary>
        /// <param name="window">当前窗口</param>
        /// <param name="owner">弹出窗口拥有者</param>
        /// <param name="showInTaskbar">是否在任务栏显示</param>
        /// <param name="topMost"></param>
        /// <returns></returns>
        public static bool? DeyiDialog(this Window window, Window owner = null, bool showInTaskbar = true, bool topMost = true)
        {
            window.InitWindow(owner, showInTaskbar, topMost);
            return window.ShowDialog();
        }

        #endregion

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

            float max = Math.Max(width, height);
            if (max <= 0) return bmp;

            float bl = maxLength / max;
            width = (int)Math.Round(bl * width);
            height = (int)Math.Round(bl * height);

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
