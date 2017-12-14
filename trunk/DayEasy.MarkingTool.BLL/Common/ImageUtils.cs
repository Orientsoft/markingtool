using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary>
    /// 图像二值化方法：大津法和迭代法
    /// </summary>
    public enum BinarizationMethods
    {
        Otsu,       // 大津法
        Iterative   // 迭代法
    }

    /// <summary>
    /// WinForm：图像的二值化
    /// </summary>
    public static class ImageUtils
    {
        /// <summary>
        /// 将位图转换为灰度数组（256级灰度）
        /// </summary>
        /// <param name="bmp">原始位图</param>
        /// <returns>灰度数组</returns>
        public static byte[,] ToGrayArray(this Bitmap bmp)
        {
            var pixelHeight = bmp.Height; // 图像高度
            var pixelWidth = bmp.Width;   // 图像宽度
            var stride = ((pixelWidth * 3 + 3) >> 2) << 2;    // 跨距宽度
            var pixels = new byte[pixelHeight * stride];

            // 锁定位图到系统内存
            var bmpData = bmp.LockBits(new Rectangle(0, 0, pixelWidth, pixelHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(bmpData.Scan0, pixels, 0, pixels.Length);  // 从非托管内存拷贝数据到托管内存
            bmp.UnlockBits(bmpData);    // 从系统内存解锁位图

            // 将像素数据转换为灰度数组
            var grayArray = new byte[pixelHeight, pixelWidth];
            for (var i = 0; i < pixelHeight; i++)
            {
                var index = i * stride;
                for (var j = 0; j < pixelWidth; j++)
                {
                    grayArray[i, j] = Convert.ToByte((pixels[index + 2] * 19595 + pixels[index + 1] * 38469 + pixels[index] * 7471 + 32768) >> 16);
                    index += 3;
                }
            }

            return grayArray;
        }

        /// <summary>
        /// 全局阈值图像二值化
        /// </summary>
        /// <param name="bmp">原始图像</param>
        /// <param name="method">二值化方法</param>
        /// <param name="diff"></param>
        /// <returns>二值化后的图像数组</returns>        
        public static byte[,] ToBinaryArray(this Bitmap bmp, BinarizationMethods method = BinarizationMethods.Otsu, int diff = 0)
        {   // 位图转换为灰度数组
            var grayArray = bmp.ToGrayArray();
            //grayArray = MedianFilter(grayArray);
            int threshold;
            // 计算全局阈值
            if (DeyiKeys.ScannerConfig.BasicThreshold > 0)
            {
                threshold = (int)Math.Round(255 * DeyiKeys.ScannerConfig.BasicThreshold);
            }
            else
            {
                switch (method)
                {
                    case BinarizationMethods.Otsu:
                        threshold = OtsuThreshold(grayArray);
                        break;
                    case BinarizationMethods.Iterative:
                        threshold = IterativeThreshold(grayArray);
                        break;
                    default:
                        threshold = OtsuThreshold(grayArray);
                        break;
                }
            }
            threshold += diff;
            if (threshold < 80) threshold = 80;
            if (threshold > 220) threshold = 220;

            // 根据阈值进行二值化
            var pixelHeight = bmp.Height;
            var pixelWidth = bmp.Width;
            var binaryArray = new byte[pixelHeight, pixelWidth];
            for (var i = 0; i < pixelHeight; i++)
            {
                for (var j = 0; j < pixelWidth; j++)
                {
                    binaryArray[i, j] =
                        Convert.ToByte((grayArray[i, j] > threshold) ? 255 : 0);
                }
            }

            return binaryArray;
        }

        /// <summary>
        /// 将二值化数组转换为二值化图像
        /// </summary>
        /// <param name="binaryArray">二值化数组</param>
        /// <returns>二值化图像</returns>
        public static Bitmap ToBitmap(this byte[,] binaryArray)
        {
            // 将二值化数组转换为二值化数据
            var pixelHeight = binaryArray.GetLength(0);
            var pixelWidth = binaryArray.GetLength(1);
            var stride = ((pixelWidth + 31) >> 5) << 2;
            var pixels = new byte[pixelHeight * stride];
            for (var i = 0; i < pixelHeight; i++)
            {
                var Base = i * stride;
                for (var j = 0; j < pixelWidth; j++)
                {
                    if (binaryArray[i, j] != 0)
                    {
                        pixels[Base + (j >> 3)] |= Convert.ToByte(0x80 >> (j & 0x7));
                    }
                }
            }

            // 创建黑白图像
            var binaryBmp = new Bitmap(pixelWidth, pixelHeight, PixelFormat.Format1bppIndexed);

            // 设置调色表
            var cp = binaryBmp.Palette;
            cp.Entries[0] = Color.Black; // 黑色
            cp.Entries[1] = Color.White; // 白色
            binaryBmp.Palette = cp;

            // 设置位图图像特性
            var binaryBmpData = binaryBmp.LockBits(new Rectangle(0, 0, pixelWidth, pixelHeight), ImageLockMode.WriteOnly,
                PixelFormat.Format1bppIndexed);
            Marshal.Copy(pixels, 0, binaryBmpData.Scan0, pixels.Length);
            binaryBmp.UnlockBits(binaryBmpData);

            return binaryBmp;
        }

        /// <summary> 中值滤波 </summary>
        public static void MedianFilter(this Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            var ptr = data.Scan0;
            var totalPixels = Math.Abs(data.Stride) * data.Height;
            var rgbValues = new byte[totalPixels];
            Marshal.Copy(ptr, rgbValues, 0, totalPixels); //RGB=>rgbValus
            var bpp = (bmp.RawFormat.Equals(ImageFormat.Png) ? 4 : 3);
            for (var i = 0; i < totalPixels - bpp; i += bpp)
            {
                //反转
                //rgbValues[i] = (byte)(255 - rgbValues[i]); //b
                //rgbValues[i + 1] = (byte)(255 - rgbValues[i + 1]); ; //g
                //rgbValues[i + 2] = (byte)(255 - rgbValues[i + 2]); ; //r
                var gray = (byte)((rgbValues[i] + rgbValues[i + 1] + rgbValues[i + 2]) / 3);
                rgbValues[i] = rgbValues[i + 1] = rgbValues[i + 2] = gray;
            }
            Marshal.Copy(rgbValues, 0, ptr, totalPixels);
            bmp.UnlockBits(data);
        }

        public static byte[,] Area(this byte[,] binaryArray, int x, int y, int width, int height)
        {
            int h = binaryArray.GetLength(0),
                w = binaryArray.GetLength(1);
            var areaArray = new byte[height, width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    int sx = j + x, sy = i + y;
                    if (sx < 0 || sy < 0 || sy >= h || sx >= w)
                        areaArray[i, j] = 255;
                    else
                        areaArray[i, j] = binaryArray[sy, sx];
                }
            }
            return areaArray;
        }

        public static byte[,] Corrosion(this byte[,] binaryArray)
        {
            int width = binaryArray.GetLength(1),
                height = binaryArray.GetLength(0);
            var corrosionArray = new byte[height, width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    if (i < 2 || j < 2 || i >= height - 2 || j >= width - 2)
                    {
                        //排除边缘
                        corrosionArray[i, j] = 255;
                    }
                    else
                    {
                        var list = new byte[7];//九宫格腐蚀
                        list[0] = Convert.ToByte(binaryArray[i, j - 1]);
                        list[1] = Convert.ToByte(binaryArray[i, j]);
                        list[2] = Convert.ToByte(binaryArray[i - 1, j]);
                        list[3] = Convert.ToByte(binaryArray[i + 1, j]);
                        list[4] = Convert.ToByte(binaryArray[i, j + 1]);
                        list[3] = Convert.ToByte(binaryArray[i, j - 2]);
                        list[4] = Convert.ToByte(binaryArray[i, j + 2]);
                        //for (var m = 0; m < 3; m++)
                        //{
                        //    if (m == 1)
                        //    {
                        //        list[1] = Convert.ToByte(binaryArray[i + m - 1, j - 1]);
                        //        //list[2] = Convert.ToByte(binaryArray[i + m - 1, j]);
                        //        list[3] = Convert.ToByte(binaryArray[i + m - 1, j + 1]);
                        //        list[5] = Convert.ToByte(binaryArray[i + m - 1, j - 2]);
                        //        list[6] = Convert.ToByte(binaryArray[i + m - 1, j + 2]);
                        //    }
                        //    list[m * 2] = Convert.ToByte(binaryArray[i + m - 1, j]);
                        //}
                        corrosionArray[i, j] = Convert.ToByte(list.All(t => t == 0) ? 0 : 255);
                    }
                }
            }
            return corrosionArray;
        }

        /// <summary> 腐蚀 </summary>
        public static Bitmap Corrosion(this Bitmap binaryBmp)
        {
            using (binaryBmp)
            {
                var binaryArray = binaryBmp.ToBinaryArray();
                return ToBitmap(binaryArray.Corrosion());
            }
        }

        /// <summary> 全局阈值图像二值化 </summary>
        /// <param name="bmp">原始图像</param>
        /// <param name="method">二值化方法</param>
        /// <returns>二值化图像</returns>
        public static Bitmap ToBinaryBitmap(this Bitmap bmp, BinarizationMethods method = BinarizationMethods.Otsu)
        {   // 位图转换为灰度数组
            var grayArray = bmp.ToGrayArray();

            // 计算全局阈值
            int threshold;
            if (DeyiKeys.ScannerConfig.BasicThreshold > 0 && DeyiKeys.ScannerConfig.BasicThreshold < 1)
            {
                threshold = (int)Math.Round(255 * DeyiKeys.ScannerConfig.BasicThreshold);
            }
            else
            {
                // 计算全局阈值
                switch (method)
                {
                    case BinarizationMethods.Otsu:
                        threshold = OtsuThreshold(grayArray);
                        break;
                    case BinarizationMethods.Iterative:
                        threshold = IterativeThreshold(grayArray);
                        break;
                    default:
                        threshold = OtsuThreshold(grayArray);
                        break;
                }
            }
            // 将灰度数组转换为二值数据
            var pixelHeight = bmp.Height;
            var pixelWidth = bmp.Width;
            var stride = ((pixelWidth + 31) >> 5) << 2;
            var pixels = new byte[pixelHeight * stride];
            for (var i = 0; i < pixelHeight; i++)
            {
                var Base = i * stride;
                for (var j = 0; j < pixelWidth; j++)
                {
                    if (grayArray[i, j] > threshold)
                    {
                        pixels[Base + (j >> 3)] |= Convert.ToByte(0x80 >> (j & 0x7));
                    }
                }
            }

            // 从二值数据中创建黑白图像
            var binaryBmp = new Bitmap(pixelWidth, pixelHeight, PixelFormat.Format1bppIndexed);

            // 设置调色表
            var cp = binaryBmp.Palette;
            cp.Entries[0] = Color.Black;    // 黑色
            cp.Entries[1] = Color.White;    // 白色
            binaryBmp.Palette = cp;

            // 设置位图图像特性
            var binaryBmpData = binaryBmp.LockBits(new Rectangle(0, 0, pixelWidth, pixelHeight), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
            Marshal.Copy(pixels, 0, binaryBmpData.Scan0, pixels.Length);
            binaryBmp.UnlockBits(binaryBmpData);

            return binaryBmp;
        }

        /// <summary>
        /// 大津法计算阈值
        /// </summary>
        /// <param name="grayArray">灰度数组</param>
        /// <returns>二值化阈值</returns> 
        public static int OtsuThreshold(byte[,] grayArray)
        {   // 建立统计直方图
            var histogram = new int[256];
            Array.Clear(histogram, 0, 256);     // 初始化
            foreach (var b in grayArray)
            {
                histogram[b]++;                 // 统计直方图
            }

            // 总的质量矩和图像点数
            var sumC = grayArray.Length;    // 总的图像点数
            double sumU = 0;                  // 双精度避免方差运算中数据溢出
            for (var i = 1; i < 256; i++)
            {
                sumU += i * histogram[i];     // 总的质量矩                
            }

            // 灰度区间
            var minGrayLevel = Array.FindIndex(histogram, NonZero);       // 最小灰度值
            var maxGrayLevel = Array.FindLastIndex(histogram, NonZero);   // 最大灰度值

            // 计算最大类间方差
            var threshold = minGrayLevel;
            var maxVariance = 0.0;       // 初始最大方差
            double u0 = 0;                  // 初始目标质量矩
            double c0 = 0;                   // 初始目标点数
            for (var i = minGrayLevel; i < maxGrayLevel; i++)
            {
                if (histogram[i] == 0) continue;

                // 目标的质量矩和点数                
                u0 += i * histogram[i];
                c0 += histogram[i];

                // 计算目标和背景的类间方差
                var diference = u0 * sumC - sumU * c0;
                var variance = diference * diference / c0 / (sumC - c0); // 方差
                if (!(variance > maxVariance))
                    continue;
                maxVariance = variance;
                threshold = i;
            }

            // 返回类间方差最大阈值
            return threshold;
        }

        /// <summary>
        /// 检测非零值
        /// </summary>
        /// <param name="value">要检测的数值</param>
        /// <returns>
        ///     true：非零
        ///     false：零
        /// </returns>
        private static bool NonZero(int value)
        {
            return (value != 0);
        }

        /// <summary> 迭代法计算阈值 </summary>
        /// <param name="grayArray">灰度数组</param>
        /// <returns>二值化阈值</returns> 
        public static int IterativeThreshold(byte[,] grayArray)
        {   // 建立统计直方图
            int[] histogram = new int[256];
            Array.Clear(histogram, 0, 256);     // 初始化
            foreach (var b in grayArray)
            {
                histogram[b]++;                 // 统计直方图
            }

            // 总的质量矩和图像点数
            var sumC = grayArray.Length;    // 总的图像点数
            var sumU = 0;
            for (var i = 1; i < 256; i++)
            {
                sumU += i * histogram[i];     // 总的质量矩                
            }

            // 确定初始阈值
            var minGrayLevel = Array.FindIndex(histogram, NonZero);       // 最小灰度值
            var maxGrayLevel = Array.FindLastIndex(histogram, NonZero);   // 最大灰度值
            var t0 = (minGrayLevel + maxGrayLevel) >> 1;
            if (minGrayLevel == maxGrayLevel) return t0;
            for (var iteration = 0; iteration < 100; iteration++)
            {   // 计算目标的质量矩和点数
                var u0 = 0;
                int c0 = 0;
                for (var i = minGrayLevel; i <= t0; i++)
                {   // 目标的质量矩和点数                
                    u0 += i * histogram[i];
                    c0 += histogram[i];
                }

                // 目标的平均灰度值和背景的平均灰度值的中心值
                var t1 = (u0 / c0 + (sumU - u0) / (sumC - c0)) >> 1;
                if (t0 == t1) break;
                t0 = t1;
            }

            // 返回最佳阈值
            return t0;
        }
    }
}
