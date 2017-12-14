using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace Deyi.Tool.Common
{
    public class ImageHelper
    {
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
        /// <param name="angle"></param>
        /// <param name="bgColor"></param>
        /// <returns></returns>
        public static Image MakeImage(Image origBmp, int x, int y, int width, int height, float angle,Color bgColor)
        {
            var newBmp = new Bitmap(width, height); // 生成新的画布
            var g = Graphics.FromImage(newBmp); // 将画布读取到图像中

            try
            {
                var origRect = new Rectangle(x, y, width, height); // 原始图像矩形框
                var destRect = new Rectangle(0, 0, width, height); // 新画布图像矩形框
                //呈现质量
                g.CompositingQuality = CompositingQuality.HighQuality;
                //像素偏移方式
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //平滑处理
                g.SmoothingMode = SmoothingMode.HighQuality;
                //插补模式,双三次插值法
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (Math.Abs(angle) > 0)
                {
                    var brush = new SolidBrush(bgColor);
                    g.FillRectangle(brush, 0, 0, newBmp.Width, newBmp.Height);
                    g.RotateTransform(angle);
                }
                // 将原始图像矩形框中的内容生成到新画布中去
                g.DrawImage(origBmp, destRect, origRect, GraphicsUnit.Pixel);
            }
            catch (Exception)
            {
                newBmp.Dispose();
            }

            finally
            {
                g.Dispose();
            }

            return newBmp;
        }

        /// <summary>
        /// 图像二值化
        /// </summary>
        /// <param name="b"></param>
        /// <param name="threshold">阈值</param>
        public static Bitmap BinarizeImage(Bitmap b, byte threshold)
        {
            int width = b.Width;
            int height = b.Height;
            BitmapData data = b.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            unsafe
            {
                var p = (byte*) data.Scan0;
                int offset = data.Stride - width*4;
                byte R, G, B, gray;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        R = p[2];
                        G = p[1];
                        B = p[0];
                        gray = (byte) ((R*19595 + G*38469 + B*7472) >> 16);
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

        /// <summary>
        /// 图像二值化
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Bitmap BinarizeImage(Bitmap b)
        {
            int width = b.Width;
            int height = b.Height;
            var list = new List<byte>();
            BitmapData data = b.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            unsafe
            {
                var p = (byte*) data.Scan0;
                int offset = data.Stride - width*4;
                byte R, G, B, gray;
                //只判段下1/3部分
                int startY = height*2/3;
                p += offset*startY;
                for (int y = startY; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        R = p[2];
                        G = p[1];
                        B = p[0];
                        gray = (byte) ((R*19595 + G*38469 + B*7472) >> 16);
                        list.Add(gray);
                    }
                    p += offset;
                }
                var item = list.GroupBy(t => t).OrderByDescending(t => t.Count()).FirstOrDefault();
                byte threshold = 120;
                if (item != null)
                {
                    threshold = (byte) (Math.Sin(item.Key/(double) 255)*item.Key + 20);
                    if (threshold > 180) threshold = 180;
                }
                p = (byte*) data.Scan0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        R = p[2];
                        G = p[1];
                        B = p[0];
                        gray = (byte) ((R*19595 + G*38469 + B*7472) >> 16);
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

        public static Image MakeImage(Image origBmp, int x, int y, int width, int height)
        {
            return MakeImage(origBmp, x, y, width, height, 0,Color.White);
        }

        public static Image Resize(Image origBmp, int width)
        {
            int w = origBmp.Width,
                h = origBmp.Height;
            h = (int) Math.Round((width/(double) w)*h);
            var d = new Bitmap(width, h);
            using (var g = Graphics.FromImage(d))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.DrawImage(origBmp,
                    new RectangleF(0, 0, width, h),
                    new RectangleF(0, 0, origBmp.Width, origBmp.Height),
                    GraphicsUnit.Pixel);
            }
            return d;
        }

        public static Image MakeImage(Image origBmp, int x, int y)
        {
            return MakeImage(origBmp, x, y, origBmp.Width, origBmp.Height);
        }

        /// <summary>
        /// 任意角度旋转
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="angle"></param>
        /// <param name="bgColor"></param>
        /// <returns></returns>
        public static Image RotateBitmap(Bitmap bmp, float angle, Color bgColor)
        {
            return MakeImage(bmp, 0, 0, bmp.Width, bmp.Height, angle, bgColor);
        }

        /// <summary>
        /// 任意角度旋转
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Image RotateBitmap(Bitmap bmp, float angle)
        {
            return RotateBitmap(bmp, angle, Color.White);
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
            double luminance = (color.R*0.299) + (color.G*0.587) + (color.B*0.114);
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
                    int gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
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
        /// 图像二值化
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Bitmap ConvertTo1Bpp(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            var bmp = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite,
                PixelFormat.Format1bppIndexed);
            for (int y = 0; y < h; y++)
            {
                var scan = new byte[(w + 7)/8];
                for (int x = 0; x < w; x++)
                {
                    Color c = img.GetPixel(x, y);
                    if (c.GetBrightness() >= 0.5) scan[x/8] |= (byte) (0x80 >> (x%8));
                }
                Marshal.Copy(scan, 0, (IntPtr) ((int) data.Scan0 + data.Stride*y), scan.Length);
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
                var ptr = (byte*) data.Scan0;
                int remain = data.Stride - data.Width*3;
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
            return result/j;
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
            return abs/(float) result;
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
            return (pCurrentlyColor.R + pCurrentlyColor.G + pCurrentlyColor.B)/3 < pFloat;
        }
    }
}
