using System;
using System.Collections.Generic;
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
                targetHeight = (int)Math.Floor((targetWidth / (double)width) * height);
            }
            else if (targetHeight > 0 && targetWidth == 0)
            {
                targetWidth = (int)Math.Floor((targetHeight / (double)height) * width);
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
                g.Clear(Color.White);

                // 将原始图像矩形框中的内容生成到新画布中去
                g.DrawImage(origBmp, destRect, origRect, GraphicsUnit.Pixel);
                g.Dispose();
                origBmp.Dispose();
                GC.Collect();
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
                    for (var x = 0; x < width; x++)
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

        public static void Resize(Bitmap sourceBmp, string path, int width = 0, int height = 0, int qt = 85)
        {
            var bmp = Resize(sourceBmp, width, height);
            var encoder = GetEncoderInfo(Path.GetExtension(path));
            if (encoder == null)
                return;
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, qt);
            bmp.Save(path, encoder, encoderParameters);
        }

        public static Image MakeImage(Image origBmp, int x, int y)
        {
            return MakeImage(origBmp, x, y, origBmp.Width, origBmp.Height);
        }

        public static Bitmap RotateA3Image(Bitmap bmp)
        {
            //create an object that we can use to examine an image file
            Image img = bmp;

            //rotate the picture by 90 degrees and re-save the picture as a Jpeg
            img.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return (Bitmap)img;
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
            if (Math.Abs(angle) < 0.001)
            {
                return bmp;
            }

            int w = bmp.Width, h = bmp.Height;

            float transformX = w / 2F,
                transformY = h / 2F;
            var pixelFormat = bmp.PixelFormat;
            var pixelFormatOld = pixelFormat;
            if (bmp.Palette.Entries.Count() > 0)
            {
                pixelFormat = PixelFormat.Format24bppRgb;
            }
            var tmpBitmap = new Bitmap(w, h, pixelFormat);
            tmpBitmap.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
            using (var g = Graphics.FromImage(tmpBitmap))
            {
                g.FillRectangle(bgColor, 0, 0, w, h);
                g.TranslateTransform(transformX, transformY);
                g.RotateTransform(angle);
                g.TranslateTransform(-transformX, -transformY);
                g.DrawImage(bmp, 0, 0);
                g.Dispose();
                GC.Collect();
            }
            switch (pixelFormatOld)
            {
                case PixelFormat.Format8bppIndexed:
                    tmpBitmap = CopyTo8Bpp(tmpBitmap);
                    break;
                case PixelFormat.Format1bppIndexed:
                    tmpBitmap = CopyTo1Bpp(tmpBitmap);
                    break;
            }
            bmp.Dispose();
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
        public static double GetSkewAngle(Bitmap bmp, byte threshold = 140)
        {
            if (bmp == null) return 0F;
            if (bmp.Width > 800)
            {
                bmp = (Bitmap)MakeImage(bmp, 0, 0, bmp.Width, bmp.Height, DeyiKeys.ScannerConfig.PaperWidth);
            }
            var deskew = new Deskew(bmp, threshold);
            return deskew.GetSkewAngle();
        }

        private static Bitmap CopyTo1Bpp(Bitmap b)
        {
            int w = b.Width, h = b.Height;
            var r = new Rectangle(0, 0, w, h);
            if (b.PixelFormat != PixelFormat.Format32bppPArgb)
            {
                var temp = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
                temp.SetResolution(b.HorizontalResolution, b.VerticalResolution);
                var g = Graphics.FromImage(temp);
                g.DrawImage(b, r, 0, 0, w, h, GraphicsUnit.Pixel);
                g.Dispose();
                b = temp;
                GC.Collect();
            }
            var bdat = b.LockBits(r, ImageLockMode.ReadOnly, b.PixelFormat);
            var b0 = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
            b0.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            var b0Dat = b0.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var index = y * bdat.Stride + (x * 4);
                    if (
                        Color.FromArgb(Marshal.ReadByte(bdat.Scan0, index + 2), Marshal.ReadByte(bdat.Scan0, index + 1),
                            Marshal.ReadByte(bdat.Scan0, index)).GetBrightness() > 0.5f)
                    {
                        var index0 = y * b0Dat.Stride + (x >> 3);
                        var p = Marshal.ReadByte(b0Dat.Scan0, index0);
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
            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

            var width = bmpData.Width;
            var height = bmpData.Height;
            var stride = bmpData.Stride;
            var offset = stride - width * 3;
            var ptr = bmpData.Scan0;
            var scanBytes = stride * height;

            int posScan = 0, posDst = 0;
            var rgbValues = new byte[scanBytes];
            Marshal.Copy(ptr, rgbValues, 0, scanBytes);
            var grayValues = new byte[width * height];

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var temp = rgbValues[posScan++] * 0.11 +
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
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            var offset0 = bitmapData.Stride - bitmapData.Width;
            var scanBytes0 = bitmapData.Stride * bitmapData.Height;
            var rawValues = new byte[scanBytes0];

            var posSrc = 0;
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
            for (var i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;

            return bitmap;
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

        /// <summary> 合并图像 </summary>
        /// <param name="bmps"></param>
        /// <returns></returns>
        public static Bitmap CombineBitmaps(List<Bitmap> bmps)
        {
            if (bmps == null || !bmps.Any())
                return null;
            if (bmps.Count() == 1)
                return bmps[0];
            var height = bmps.Sum(t => t.Height);
            var width = bmps.Max(t => t.Width);
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                //呈现质量
                g.CompositingQuality = CompositingQuality.HighQuality;
                //像素偏移方式
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //平滑处理
                g.SmoothingMode = SmoothingMode.HighQuality;
                //插补模式,双三次插值法
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                const int x = 0;
                var y = 0;
                foreach (var bitmap in bmps)
                {
                    g.DrawImage(bitmap, new RectangleF(x, y, bitmap.Width, bitmap.Height));
                    y += bitmap.Height + 1;
                }
            }
            return bmp;
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            //根据 mime 类型，返回编码器
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            mimeType = "image/" + mimeType.Replace(".", string.Empty).ToLower();
            mimeType = mimeType.Replace("jpg", "jpeg");
            return encoders.FirstOrDefault(t => t.MimeType == mimeType);
        }

        public static double BlackRank(Bitmap bmp)
        {
            var total = (bmp.Width * bmp.Height);
            if (total <= 0)
                return 0;
            var black = 0;
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                const int bpp = 4;
                var ptr = (byte*)data.Scan0;
                var remain = data.Stride - data.Width * bpp;
                for (var y = 0; y < data.Height; y++)
                {
                    for (var x = 0; x < data.Width; x++)
                    {
                        if (IsBlack(ptr))
                            black++;
                        ptr += bpp;
                    }
                    ptr += remain;
                }
            }
            bmp.UnlockBits(data);
            return black / (double)total;
        }

        public static List<int[]> BmpArray(Bitmap bmp)
        {
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var blackList = new List<int[]>();
            unsafe
            {
                const int bpp = 4;
                var ptr = (byte*)data.Scan0;
                var remain = data.Stride - data.Width * bpp;
                for (var y = 0; y < data.Height; y++)
                {
                    var line = new int[data.Width];
                    for (var x = 0; x < data.Width; x++)
                    {
                        line[x] = IsBlack(ptr) ? 1 : 0;
                        ptr += bpp;
                    }
                    blackList.Add(line);
                    ptr += remain;
                }
            }
            bmp.UnlockBits(data);
            return blackList;
        }

        public static bool IsFilled(Bitmap bmp)
        {
            var blackList = BmpArray(bmp);
            return IsFilled(blackList);
        }
        public static bool IsFilled(List<int[]> blackList)
        {
            if (blackList == null || !blackList.Any())
                return false;
            int fillWidth = DeyiKeys.ScannerConfig.SmearWidth,
                fillHeight = DeyiKeys.ScannerConfig.SmearHeight;

            Func<int, int, bool> isBlack = (x, y) =>
            {
                if (x < 0 || y < 0 || y >= blackList.Count || x >= blackList[y].Length)
                    return false;
                return blackList[y][x] == 1;
            };

            for (var y = 0; y < blackList.Count - fillHeight; y++)
            {
                for (var x = 0; x < blackList[y].Length - fillWidth; x++)
                {
                    if (!isBlack(x, y))
                        continue;
                    var black = 0;
                    for (var i = 0; i < fillHeight; i++)
                    {
                        for (var j = 0; j < fillWidth; j++)
                        {
                            if (isBlack(x + j, y + i))
                                black++;
                        }
                    }
                    if (black >= fillHeight * fillWidth * DeyiKeys.ScannerConfig.SheetTolerance)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static unsafe bool IsBlack(byte* ptr)
        {
            return ptr[2] + ptr[1] + ptr[0] == 0;
        }
    }
}
