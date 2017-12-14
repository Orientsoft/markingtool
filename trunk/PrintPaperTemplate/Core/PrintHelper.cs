using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using PrintPaperTemplate.Model;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Point = System.Windows.Point;

namespace PrintPaperTemplate.Core
{
    public class PrintHelper
    {
        private const int FontSize = 13;

        private const int LineCount = 9;
        private const int RowCount = 11;

        private const int QrcodeWidth = 74;
        private const int NameWidth = 15;

        /// <summary>
        /// 打印纸上每个二维码宽度
        /// </summary>
        private const int PreWidth = 80;

        /// <summary>
        /// 打印纸上每行的高度
        /// </summary>
        private static readonly int LineHeight =
            (int)Math.Ceiling((double)PreWidth * QrcodeWidth / (QrcodeWidth + NameWidth));

        private static readonly Typeface TitleFace = new Typeface(new FontFamily("宋体"), FontStyles.Normal, FontWeights.Bold,
            FontStretches.Normal);

        public static Image Resize(Image origBmp, int width)
        {
            int w = origBmp.Width,
                h = origBmp.Height;
            h = (int)Math.Round((width / (double)w) * h);
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

        public static Bitmap ToBitmap(BitMatrix matrix)
        {
            int width = matrix.Width;
            int height = matrix.Height;
            var bmap = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);
                    //bmap.SetPixel(x, y,
                    //    !matrix[x, y]
                    //        ? ColorTranslator.FromHtml("Purple")
                    //        : ColorTranslator.FromHtml("0xFFFFFFFF"));
                    //可以自定义颜色和背景色
                }
            }
            return bmap;
        }

        ///  <summary>  
        ///  获取二维码  
        ///  </summary>  
        ///  <param name="content">待编码的字符</param>
        /// <param name="size"></param>
        /// <returns></returns>  
        public static Bitmap GetQrCode(string content, int size)
        {
            var hints = new Dictionary<EncodeHintType, object>
            {
                {EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.L},
                {EncodeHintType.CHARACTER_SET, "UTF-8"},
                {EncodeHintType.MARGIN, 0},
                {EncodeHintType.PURE_BARCODE, false},
                {EncodeHintType.DISABLE_ECI, true}
            };
            var wqWriter = new QRCodeWriter();
            var byteMatrix = wqWriter.encode(content, BarcodeFormat.QR_CODE,
                size, size, hints);
            return ToBitmap(byteMatrix);
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        private static ImageSource GenerateQrcode(Student student)
        {
            var content = Helper.DesEncrypt(student.ToString());
            var sourceBmp = GetQrCode(content, QrcodeWidth);
            var bmp = new Bitmap(QrcodeWidth + NameWidth + 5, QrcodeWidth);
            var g = Graphics.FromImage(bmp);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(sourceBmp, new RectangleF(NameWidth + 5, 0, QrcodeWidth, QrcodeWidth));

            var drawFormat = new StringFormat(StringFormatFlags.DirectionVertical);
            int y = NameWidth + 5;
            var len = student.Name.Length;
            if (len > 2)
                y = y - (len - 2) * 5;
            g.DrawString(student.Name, new Font("宋体", 9), new SolidBrush(Color.Black), 0, y, drawFormat);
            g.Dispose();
            var ms = new MemoryStream();
#if DEBUG
            bmp.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("q-{0}.png", student.Name)));
#endif
            bmp.Save(ms, ImageFormat.Png);
            var converter = new ImageSourceConverter();
            return (ImageSource)converter.ConvertFrom(ms);
        }

        /// <summary>
        /// 打印模版
        /// </summary>
        /// <param name="students">学生列表</param>
        /// <param name="size">每人打印多少份</param>
        /// <returns></returns>
        public static ContainerVisual PrintingTemplates(List<Student> students, int size)
        {
            var vises = new ContainerVisual();
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            const int skipX = 30, skipY = 20;
            int currentCount = 1, currentRow = 1, x, y = skipY;
            int endX, current = 0;
            //一行多少个学生s
            foreach (var stu in students)
            {
                current++;
                var img = GenerateQrcode(stu);
                int startX = (currentCount - 1) * PreWidth + skipX;
                for (var j = 0; j < size; j++)
                {
                    if (currentCount > LineCount)
                    {
                        currentRow++;
                        y += PreWidth + 2;
                        currentCount = 1;
                    }
                    x = (currentCount - 1) * PreWidth + skipX;
                    //二维码
                    var rectRange = new Rect(x, y, PreWidth, LineHeight);
                    drawingContext.DrawImage(img, rectRange);
                    currentCount++;
                }
                endX = (currentCount - 2) * PreWidth + skipX;
                currentCount++;
                x = startX + (endX - startX) / 2 - skipX;
                var word = string.Format("{0}[{1}]", stu.Name, stu.Email);
                drawingContext.DrawText(
                    new FormattedText(word, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, TitleFace, FontSize,
                        Brushes.Black), new Point(x, y + LineHeight + 7));
                var left = LineCount - currentCount + 1;
                if (left < size)
                {
                    currentCount = 1;
                    y += LineHeight + 33;
                    currentRow++;
                }
                if (currentRow > RowCount || students.Count == current)
                {
                    currentRow = 1;
                    currentCount = 1;
                    y = skipY;
                    drawingContext.Close();
                    vises.Children.Add(drawingVisual);
                    drawingVisual = new DrawingVisual();
                    drawingContext = drawingVisual.RenderOpen();
                }
            }
            drawingContext.Close();
            return vises;
        }

        public static ContainerVisual PrintTemplates(List<Student> students, int size)
        {
            var vises = new ContainerVisual();
            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            foreach (var student in students)
            {
                var img = GenerateQrcode(student);
                for (int i = 0; i < size; i++)
                {
                    //二维码
                    //var rectRange = new Rect(18, 10, 80, 65);
                    var rectRange = new Rect(18, 10, 80, 65);
                    drawingContext.DrawImage(img, rectRange);
                    drawingContext.Close();
                    vises.Children.Add(drawingVisual);
                    drawingVisual = new DrawingVisual();
                    drawingContext = drawingVisual.RenderOpen();
                }
            }
            return vises;
        }
    }
}
