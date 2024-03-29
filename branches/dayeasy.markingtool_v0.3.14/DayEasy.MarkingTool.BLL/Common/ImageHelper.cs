﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary>
    /// 图片辅助类
    /// </summary>
    public class ImageHelper
    {
        public static Bitmap LoadImage(string fileName)
        {
            return new Bitmap(fileName);
        }

        public static void MakeImage(Bitmap origBmp, int x, int y, int width, int height,
            string filePath, ImageFormat format)
        {
            using (var newImg = MakeImage(origBmp, x, y, width, height))
            {
                newImg.Save(filePath, format);
            }
        }

        /// <summary>
        /// 裁剪图片
        /// </summary>
        /// <param name="origBmp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        public static Image MakeImage(Image origBmp, int x, int y, int width, int height, int targetWidth = 0,
            int targetHeight = 0)
        {
            if (targetWidth > 0 && targetHeight == 0)
            {
                targetHeight = (int)Math.Round((targetWidth / (double)width) * height);
            }
            else if (targetHeight > 0 && targetWidth == 0)
            {
                targetWidth = (int)Math.Round((targetHeight / (double)height) * width);
            }
            else if (targetWidth == 0 && targetHeight == 0)
            {
                targetWidth = width;
                targetHeight = height;
            }

            // 生成新的画布
            var newBmp = new Bitmap(targetWidth, targetHeight);
            // 将画布读取到图像中
            using (var g = Graphics.FromImage(newBmp))
            {
                // 原始图像矩形框
                var origRect = new Rectangle(x, y, width, height);
                // 新画布图像矩形框
                var destRect = new Rectangle(0, 0, targetWidth, targetHeight);
                //呈现质量
                g.CompositingQuality = CompositingQuality.HighQuality;
                //像素偏移方式
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //平滑处理
                g.SmoothingMode = SmoothingMode.HighQuality;
                //插补模式,双三次插值法
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // 将原始图像矩形框中的内容生成到新画布中去
                g.DrawImage(origBmp, destRect, origRect, GraphicsUnit.Pixel);
            }
            return newBmp;
        }

        /// <summary>
        /// 图像二值化
        /// </summary>
        /// <param name="b"></param>
        /// <param name="threshold">阈值</param>
        public static Bitmap BinarizeImage(Bitmap b, byte threshold = 125)
        {
            int width = b.Width;
            int height = b.Height;
            BitmapData data = b.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            unsafe
            {
                var p = (byte*)data.Scan0;
                int offset = data.Stride - width * 4;
                byte R, G, B, gray;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        R = p[2];
                        G = p[1];
                        B = p[0];
                        gray = (byte)((R * 19595 + G * 38469 + B * 7472) >> 16);
                        if (gray >= threshold)
                        {
                            p[0] = p[1] = p[2] = 255;
                        }
                        else
                        {
                            p[0] = p[1] = p[2] = 0;
                        }
                        p += 4;
                    }
                    p += offset;
                }
                b.UnlockBits(data);
                return b;
            }
        }

        public static Image Resize(Image origBmp, int width = 0, int height = 0)
        {
            return MakeImage(origBmp, 0, 0, origBmp.Width, origBmp.Height, width, height);
        }

        public static Image MakeImage(Image origBmp, int x, int y)
        {
            return MakeImage(origBmp, x, y, origBmp.Width, origBmp.Height);
        }

        /// <summary>
        /// 图像旋转
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="angle">角度</param>
        /// <param name="bgColor"></param>
        /// <returns></returns>
        public static Bitmap RotateImage(Bitmap bmp, float angle, Brush bgColor)
        {
            PixelFormat pixelFormat = bmp.PixelFormat;
            PixelFormat pixelFormatOld = pixelFormat;
            if (bmp.Palette.Entries.Count() > 0)
            {
                pixelFormat = PixelFormat.Format24bppRgb;
            }
            var tmpBitmap = new Bitmap(bmp.Width, bmp.Height, pixelFormat);
            tmpBitmap.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
            using (var g = Graphics.FromImage(tmpBitmap))
            {
                //呈现质量
                g.CompositingQuality = CompositingQuality.HighQuality;
                //像素偏移方式
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //平滑处理
                g.SmoothingMode = SmoothingMode.HighQuality;
                //插补模式,双三次插值法
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.FillRectangle(bgColor, 0, 0, bmp.Width, bmp.Height);
                g.RotateTransform(angle);
                g.DrawImage(bmp, 0, 0);
            }
            if (pixelFormatOld == PixelFormat.Format8bppIndexed) tmpBitmap = CopyTo8Bpp(tmpBitmap);
            else if (pixelFormatOld == PixelFormat.Format1bppIndexed) tmpBitmap = CopyTo1Bpp(tmpBitmap);

            return tmpBitmap;
        }

        /// <summary>
        /// 图像旋转
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="angle">角度</param>
        /// <returns></returns>
        public static Bitmap RotateImage(Bitmap bmp, float angle)
        {
            return RotateImage(bmp, angle, Brushes.White);
        }

        /// <summary>
        /// 自动纠偏
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static double GetSkewAngle(Bitmap bmp, byte threshold = 125)
        {
            var tempBmp = bmp.Clone() as Bitmap;
            if (tempBmp == null) return 0F;
            if (tempBmp.Width > 800)
            {
                tempBmp = (Bitmap)MakeImage(tempBmp, 0, 0, tempBmp.Width, tempBmp.Height, DeyiKeys.PaperWidth);
            }
            tempBmp = BinarizeImage(tempBmp, threshold);
            var deskew = new Deskew(tempBmp);
            return deskew.GetSkewAngle();
        }

        private static Bitmap CopyTo1Bpp(Bitmap b)
        {
            int w = b.Width, h = b.Height; var r = new Rectangle(0, 0, w, h);
            if (b.PixelFormat != PixelFormat.Format32bppPArgb)
            {
                var temp = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
                temp.SetResolution(b.HorizontalResolution, b.VerticalResolution);
                Graphics g = Graphics.FromImage(temp);
                g.DrawImage(b, r, 0, 0, w, h, GraphicsUnit.Pixel);
                g.Dispose();
                b = temp;
            }
            BitmapData bdat = b.LockBits(r, ImageLockMode.ReadOnly, b.PixelFormat);
            var b0 = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
            b0.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            BitmapData b0Dat = b0.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int index = y * bdat.Stride + (x * 4);
                    if (Color.FromArgb(Marshal.ReadByte(bdat.Scan0, index + 2), Marshal.ReadByte(bdat.Scan0, index + 1), Marshal.ReadByte(bdat.Scan0, index)).GetBrightness() > 0.5f)
                    {
                        int index0 = y * b0Dat.Stride + (x >> 3);
                        byte p = Marshal.ReadByte(b0Dat.Scan0, index0);
                        var mask = (byte)(0x80 >> (x & 0x7));
                        Marshal.WriteByte(b0Dat.Scan0, index0, (byte)(p | mask));
                    }
                }
            }
            b0.UnlockBits(b0Dat);
            b.UnlockBits(bdat);
            return b0;
        }

        private static Bitmap CopyTo8Bpp(Bitmap bmp)
        {
            if (bmp == null) return null;

            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

            int width = bmpData.Width;
            int height = bmpData.Height;
            int stride = bmpData.Stride;
            int offset = stride - width * 3;
            IntPtr ptr = bmpData.Scan0;
            int scanBytes = stride * height;

            int posScan = 0, posDst = 0;
            var rgbValues = new byte[scanBytes];
            Marshal.Copy(ptr, rgbValues, 0, scanBytes);
            var grayValues = new byte[width * height];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double temp = rgbValues[posScan++] * 0.11 +
                                  rgbValues[posScan++] * 0.59 +
                                  rgbValues[posScan++] * 0.3;
                    grayValues[posDst++] = (byte)temp;
                }
                posScan += offset;
            }

            Marshal.Copy(rgbValues, 0, ptr, scanBytes);
            bmp.UnlockBits(bmpData);

            var bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            bitmap.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            int offset0 = bitmapData.Stride - bitmapData.Width;
            int scanBytes0 = bitmapData.Stride * bitmapData.Height;
            var rawValues = new byte[scanBytes0];

            int posSrc = 0;
            posScan = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    rawValues[posScan++] = grayValues[posSrc++];
                }
                posScan += offset0;
            }

            Marshal.Copy(rawValues, 0, bitmapData.Scan0, scanBytes0);
            bitmap.UnlockBits(bitmapData);

            ColorPalette palette;
            using (var bmp0 = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                palette = bmp0.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;

            return bitmap;
        }

        /// <summary>
        /// 是否是黑阶色
        /// </summary>
        /// <param name="bmap"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsBlack(Bitmap bmap, int x, int y)
        {
            Color c = bmap.GetPixel(x, y);
            return IsBlack(c);
        }

        /// <summary>
        /// 是否是黑阶色
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsBlack(Color color)
        {
            double luminance = (color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114);
            return luminance < 140;
        }

        /// <summary>
        /// 图像灰度化
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap ToGray(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    //利用公式计算灰度值
                    var gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    Color newColor = Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        /// <summary>
        /// 图像灰度反转
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap GrayReverse(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    Color newColor = Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        /// <summary>
        /// 比较图片相似度
        /// </summary>
        /// <param name="sourceBitmap">原图片</param>
        /// <param name="distBitmap">比较的图片</param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static float CheckSimilarity(Bitmap sourceBitmap, Bitmap distBitmap, int size = 0)
        {
            //判断图像是否相同大小
            if (sourceBitmap.Width != distBitmap.Width || sourceBitmap.Height != distBitmap.Height || size > 0)
            {
                size = (size == 0 ? 256 : size);
                sourceBitmap = new Bitmap(sourceBitmap, size, size);
                distBitmap = new Bitmap(distBitmap, size, size);
            }
            return CompareHisogram(GetHisogram(sourceBitmap), GetHisogram(distBitmap));
        }

        /// <summary>
        /// 计算图片直方图
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static int[] GetHisogram(Bitmap img)
        {
            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            var histogram = new int[256];
            unsafe
            {
                var ptr = (byte*)data.Scan0;
                int remain = data.Stride - data.Width * 3;
                for (int i = 0; i < histogram.Length; i++)
                    histogram[i] = 0;
                for (int i = 0; i < data.Height; i++)
                {
                    for (int j = 0; j < data.Width; j++)
                    {
                        int mean = ptr[0] + ptr[1] + ptr[2];
                        mean /= 3;
                        histogram[mean]++;
                        ptr += 3;
                    }
                    ptr += remain;
                }
            }
            img.UnlockBits(data);
            return histogram;
        }

        /// <summary>
        /// 比较数组相似度
        /// </summary>
        /// <param name="firstNum"></param>
        /// <param name="scondNum"></param>
        /// <returns></returns>
        public static float CompareHisogram(int[] firstNum, int[] scondNum)
        {
            if (firstNum.Length != scondNum.Length)
                return 0;
            float result = 0;
            int j = firstNum.Length;
            for (int i = 0; i < j; i++)
            {
                result += 1 - GetAbs(firstNum[i], scondNum[i]);
            }
            return result / j;
        }

        /// <summary>
        /// 计算相减后的绝对值
        /// </summary>
        /// <param name="firstNum"></param>
        /// <param name="secondNum"></param>
        /// <returns></returns>
        private static float GetAbs(int firstNum, int secondNum)
        {
            int abs = Math.Abs(firstNum - secondNum);
            int result = Math.Max(firstNum, secondNum);
            if (result == 0)
                result = 1;
            return abs / (float)result;
        }

        /// <summary>
        /// 判断图形里是否存在另外一个图形 并返回所在位置
        /// </summary>
        /// <param name="pSourceBitmap">原始图形</param>
        /// <param name="pPartBitmap">小图形</param>
        /// <param name="pFloat">溶差</param>
        /// <returns>坐标</returns>
        public static Point ImageContains(Bitmap pSourceBitmap, Bitmap pPartBitmap, int pFloat)
        {
            int sourceWidth = pSourceBitmap.Width;
            int sourceHeight = pSourceBitmap.Height;

            int partWidth = pPartBitmap.Width;
            int partHeight = pPartBitmap.Height;

            var sourceBitmap = new Bitmap(sourceWidth, sourceHeight);
            Graphics graphics = Graphics.FromImage(sourceBitmap);
            graphics.DrawImage(pSourceBitmap, new Rectangle(0, 0, sourceWidth, sourceHeight));
            graphics.Dispose();
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceWidth, sourceHeight),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var sourceByte = new byte[sourceData.Stride * sourceHeight];
            //复制出p_SourceBitmap的相素信息 
            Marshal.Copy(sourceData.Scan0, sourceByte, 0, sourceByte.Length);

            for (int i = 2; i < sourceHeight; i++)
            {
                //如果 剩余的高 比需要比较的高 还要小 就直接返回
                if (sourceHeight - i < partHeight) return new Point(-1, -1);
                //临时存放坐标 需要保证找到的是在一个X点上
                int pointX = -1;
                //是否都比配的上
                bool sacnOver = true;
                //循环目标进行比较
                for (int z = 0; z < partHeight - 1; z++)
                {
                    int trueX = GetImageContains(sourceByte, i * sourceData.Stride, sourceWidth, partWidth, pFloat);
                    //如果没找到
                    if (trueX == -1)
                    {
                        //设置坐标为没找到
                        pointX = -1;
                        //设置不进行返回
                        sacnOver = false;
                        break;
                    }
                    if (z == 0) pointX = trueX;
                    //如果找到了 也的保证坐标和上一行的坐标一样 否则也返回
                    if (pointX != trueX)
                    {
                        //设置坐标为没找到
                        pointX = -1;
                        //设置不进行返回
                        sacnOver = false;
                        break;
                    }
                }

                if (sacnOver) return new Point(pointX, i);
            }
            return new Point(-1, -1);
        }

        /// <summary>
        /// 判断图形里是否存在另外一个图形 所在行的索引
        /// </summary>
        /// <param name="pSource">原始图形数据</param>
        /// <param name="pSourceIndex">开始位置</param>
        /// <param name="pSourceWidth">原始图形宽</param>
        /// <param name="pPartWidth">小图宽</param>
        /// <param name="pFloat">溶差</param>
        /// <returns>所在行的索引 如果找不到返回-1</returns>
        private static int GetImageContains(byte[] pSource, int pSourceIndex, int pSourceWidth, int pPartWidth, int pFloat)
        {
            int sourceIndex = pSourceIndex;
            for (int i = 0; i < pSourceWidth; i++)
            {
                if (pSourceWidth - i < pPartWidth) return -1;
                Color currentlyColor = Color.FromArgb(pSource[sourceIndex + 3], pSource[sourceIndex + 2],
                    pSource[sourceIndex + 1], pSource[sourceIndex]);
                sourceIndex += 4;
                bool scanColor = ScanColor(currentlyColor, pFloat);

                if (scanColor)
                {
                    int sourceRva = sourceIndex;
                    bool equals = true;
                    for (int z = 0; z != pPartWidth - 1; z++)
                    {
                        currentlyColor = Color.FromArgb(pSource[sourceRva + 3], pSource[sourceRva + 2],
                            pSource[sourceRva + 1], pSource[sourceRva]);
                        if (!ScanColor(currentlyColor, pFloat))
                        {
                            equals = false;
                            break;
                        }
                        sourceRva += 4;
                    }
                    if (equals) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 检查色彩(可以根据这个更改比较方式)
        /// </summary>
        /// <param name="pCurrentlyColor">当前色彩</param>
        /// <param name="pFloat">溶差</param>
        /// <returns></returns>
        private static bool ScanColor(Color pCurrentlyColor, int pFloat)
        {
            return (pCurrentlyColor.R + pCurrentlyColor.G + pCurrentlyColor.B) / 3 < pFloat;
        }

        public static Image ScoreImage(decimal score, int size = 24)
        {
            var bitmap = new Bitmap(120, 55);
            var g = Graphics.FromImage(bitmap);
            g.Clear(Color.Transparent);
            var font = new Font("Georgia", size, FontStyle.Bold);
            var brush = new SolidBrush(Color.Red);
            g.DrawString(score.ToString("N1"), font, brush, 0, 0);
            return bitmap;
        }

        /// <summary>
        /// 该函数用于对图像进行腐蚀运算。结构元素为水平方向或垂直方向的三个点，
        /// 中间点位于原点；或者由用户自己定义3×3的结构元素。
        /// </summary>
        /// <param name="bmpobj"></param>
        /// <param name="dgGrayValue">前后景临界值</param>
        /// <param name="nMode">腐蚀方式：0表示水平方向，1垂直方向，2自定义结构元素。</param>
        /// <param name="structure"> 自定义的3×3结构元素</param>
        public static Bitmap ErosionPic(Bitmap bmpobj, int dgGrayValue, int nMode, bool[,] structure = null)
        {
            int lWidth = bmpobj.Width;
            int lHeight = bmpobj.Height;
            Bitmap newBmp = new Bitmap(lWidth, lHeight);
            int i, j, n, m;            //循环变量
            Color pixel;    //像素颜色值
            if (nMode == 0)
            {
                //使用水平方向的结构元素进行腐蚀
                // 由于使用1×3的结构元素，为防止越界，所以不处理最左边和最右边
                // 的两列像素
                for (j = 0; j < lHeight; j++)
                {
                    for (i = 1; i < lWidth - 1; i++)
                    {
                        //目标图像中的当前点先赋成黑色
                        newBmp.SetPixel(i, j, Color.Black);
                        //如果源图像中当前点自身或者左右有一个点不是黑色，
                        //则将目标图像中的当前点赋成白色
                        if (bmpobj.GetPixel(i - 1, j).R > dgGrayValue ||
                        bmpobj.GetPixel(i, j).R > dgGrayValue ||
                        bmpobj.GetPixel(i + 1, j).R > dgGrayValue)
                            newBmp.SetPixel(i, j, Color.White);
                    }
                }
            }
            else if (nMode == 1)
            {
                //使用垂真方向的结构元素进行腐蚀
                // 由于使用3×1的结构元素，为防止越界，所以不处理最上边和最下边
                // 的两行像素
                for (j = 1; j < lHeight - 1; j++)
                {
                    for (i = 0; i < lWidth; i++)
                    {
                        //目标图像中的当前点先赋成黑色
                        newBmp.SetPixel(i, j, Color.Black);
                        //如果源图像中当前点自身或者左右有一个点不是黑色，
                        //则将目标图像中的当前点赋成白色
                        if (bmpobj.GetPixel(i, j - 1).R > dgGrayValue ||
                        bmpobj.GetPixel(i, j).R > dgGrayValue ||
                        bmpobj.GetPixel(i, j + 1).R > dgGrayValue)
                            newBmp.SetPixel(i, j, Color.White);
                    }
                }
            }
            else
            {
                if (structure == null || structure.Length != 9) //检查自定义结构
                    return bmpobj;
                //使用自定义的结构元素进行腐蚀
                // 由于使用3×3的结构元素，为防止越界，所以不处理最左边和最右边
                // 的两列像素和最上边和最下边的两列像素
                for (j = 1; j < lHeight - 1; j++)
                {
                    for (i = 1; i < lWidth - 1; i++)
                    {
                        //目标图像中的当前点先赋成黑色
                        newBmp.SetPixel(i, j, Color.Black);
                        //如果原图像中对应结构元素中为黑色的那些点中有一个不是黑色，
                        //则将目标图像中的当前点赋成白色
                        for (m = 0; m < 3; m++)
                        {
                            for (n = 0; n < 3; n++)
                            {
                                if (!structure[m, n])
                                    continue;
                                if (bmpobj.GetPixel(i + m - 1, j + n - 1).R > dgGrayValue)
                                {
                                    newBmp.SetPixel(i, j, Color.White);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return newBmp;
        }

        /// <summary>
        /// 该函数用于对图像进行细化运算。要求目标图像为灰度图像
        /// </summary>
        /// <param name="bmpobj"></param>
        /// <param name="dgGrayValue"></param>
        public static Bitmap ThiningPic(Bitmap bmpobj, int dgGrayValue)
        {
            int lWidth = bmpobj.Width;
            int lHeight = bmpobj.Height;
            //   Bitmap newBmp = new Bitmap(lWidth, lHeight);
            bool bModified; //脏标记    
            int i, j, n, m; //循环变量
            Color pixel; //像素颜色值
            //四个条件
            bool bCondition1;
            bool bCondition2;
            bool bCondition3;
            bool bCondition4;
            int nCount; //计数器    
            int[,] neighbour = new int[5, 5]; //5×5相邻区域像素值
            bModified = true;
            while (bModified)
            {
                bModified = false;
                //由于使用5×5的结构元素，为防止越界，所以不处理外围的几行和几列像素
                for (j = 2; j < lHeight - 2; j++)
                {
                    for (i = 2; i < lWidth - 2; i++)
                    {
                        bCondition1 = false;
                        bCondition2 = false;
                        bCondition3 = false;
                        bCondition4 = false;
                        if (bmpobj.GetPixel(i, j).R > dgGrayValue)
                        {
                            if (bmpobj.GetPixel(i, j).R < 255)
                                bmpobj.SetPixel(i, j, Color.White);
                            continue;
                        }
                        //获得当前点相邻的5×5区域内像素值，白色用0代表，黑色用1代表
                        for (m = 0; m < 5; m++)
                        {
                            for (n = 0; n < 5; n++)
                            {
                                neighbour[m, n] = bmpobj.GetPixel(i + m - 2, j + n - 2).R < dgGrayValue ? 1 : 0;
                            }
                        }
                        //逐个判断条件。
                        //判断2<=NZ(P1)<=6
                        nCount = neighbour[1, 1] + neighbour[1, 2] + neighbour[1, 3]
                                 + neighbour[2, 1] + neighbour[2, 3] +
                                 +neighbour[3, 1] + neighbour[3, 2] + neighbour[3, 3];
                        if (nCount >= 2 && nCount <= 6)
                        {
                            bCondition1 = true;
                        }
                        //判断Z0(P1)=1
                        nCount = 0;
                        if (neighbour[1, 2] == 0 && neighbour[1, 1] == 1)
                            nCount++;
                        if (neighbour[1, 1] == 0 && neighbour[2, 1] == 1)
                            nCount++;
                        if (neighbour[2, 1] == 0 && neighbour[3, 1] == 1)
                            nCount++;
                        if (neighbour[3, 1] == 0 && neighbour[3, 2] == 1)
                            nCount++;
                        if (neighbour[3, 2] == 0 && neighbour[3, 3] == 1)
                            nCount++;
                        if (neighbour[3, 3] == 0 && neighbour[2, 3] == 1)
                            nCount++;
                        if (neighbour[2, 3] == 0 && neighbour[1, 3] == 1)
                            nCount++;
                        if (neighbour[1, 3] == 0 && neighbour[1, 2] == 1)
                            nCount++;
                        if (nCount == 1)
                            bCondition2 = true;
                        //判断P2*P4*P8=0 or Z0(p2)!=1
                        if (neighbour[1, 2] * neighbour[2, 1] * neighbour[2, 3] == 0)
                        {
                            bCondition3 = true;
                        }
                        else
                        {
                            nCount = 0;
                            if (neighbour[0, 2] == 0 && neighbour[0, 1] == 1)
                                nCount++;
                            if (neighbour[0, 1] == 0 && neighbour[1, 1] == 1)
                                nCount++;
                            if (neighbour[1, 1] == 0 && neighbour[2, 1] == 1)
                                nCount++;
                            if (neighbour[2, 1] == 0 && neighbour[2, 2] == 1)
                                nCount++;
                            if (neighbour[2, 2] == 0 && neighbour[2, 3] == 1)
                                nCount++;
                            if (neighbour[2, 3] == 0 && neighbour[1, 3] == 1)
                                nCount++;
                            if (neighbour[1, 3] == 0 && neighbour[0, 3] == 1)
                                nCount++;
                            if (neighbour[0, 3] == 0 && neighbour[0, 2] == 1)
                                nCount++;
                            if (nCount != 1)
                                bCondition3 = true;
                        }
                        //判断P2*P4*P6=0 or Z0(p4)!=1
                        if (neighbour[1, 2] * neighbour[2, 1] * neighbour[3, 2] == 0)
                        {
                            bCondition4 = true;
                        }
                        else
                        {
                            nCount = 0;
                            if (neighbour[1, 1] == 0 && neighbour[1, 0] == 1)
                                nCount++;
                            if (neighbour[1, 0] == 0 && neighbour[2, 0] == 1)
                                nCount++;
                            if (neighbour[2, 0] == 0 && neighbour[3, 0] == 1)
                                nCount++;
                            if (neighbour[3, 0] == 0 && neighbour[3, 1] == 1)
                                nCount++;
                            if (neighbour[3, 1] == 0 && neighbour[3, 2] == 1)
                                nCount++;
                            if (neighbour[3, 2] == 0 && neighbour[2, 2] == 1)
                                nCount++;
                            if (neighbour[2, 2] == 0 && neighbour[1, 2] == 1)
                                nCount++;
                            if (neighbour[1, 2] == 0 && neighbour[1, 1] == 1)
                                nCount++;
                            if (nCount != 1)
                                bCondition4 = true;
                        }
                        if (bCondition1 && bCondition2 && bCondition3 && bCondition4)
                        {
                            bmpobj.SetPixel(i, j, Color.White);
                            bModified = true;
                        }
                        else
                        {
                            bmpobj.SetPixel(i, j, Color.Black);
                        }
                    }
                }
            }
            return bmpobj;
        }
    }
}
