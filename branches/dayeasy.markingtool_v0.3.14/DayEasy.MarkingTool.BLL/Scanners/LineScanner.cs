using DayEasy.MarkingTool.BLL.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Scanners
{
    /// <summary>
    /// 线条扫描
    /// </summary>
    public class LineScanner
    {
        private const int StartLineY = 200;
        private const int LineHeight = 60;

        /// <summary>
        /// 普通图片分割线扫描
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public JsonResults<int> NormalScan(Image img)
        {
            var bmp = ImageHelper.BinarizeImage(new Bitmap(img), DeyiKeys.AnswerThreshold);
            if (bmp == null) return new JsonResults<int>("图片为空！");
            var list = new List<int>();
            int startY = 0, width = bmp.Width;
            var arrY = Helper.Each(StartLineY, bmp.Height);
            var scanWidth = DeyiKeys.Size.LineScanWidth;

            foreach (var y in arrY)
            {
                if (y - startY < LineHeight || y > bmp.Height - LineHeight)
                    continue;
                var colors = Helper.Each((width - scanWidth) / 2, (width + scanWidth) / 2)
                    .Select(x => bmp.GetPixel(x, y)).ToArray();
                var blacks = colors.Count(c => (c.R + c.B + c.G) == 0);
                if (blacks < (scanWidth * DeyiKeys.Size.BlackScale))
                    continue;
                list.Add(y);
                startY = y;
            }

            if (list.Count > 2)
            {
                return new JsonResults<int>(list, list.Count);
            }
            return new JsonResults<int>("未找到图片分割线，您选择的图片");
        }

        /// <summary>
        /// LockBits图片分割线扫描
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public JsonResults<int> BitsScan(Image img)
        {
            var bmp = ImageHelper.BinarizeImage(new Bitmap(img), DeyiKeys.AnswerThreshold);
            if (bmp == null) return new JsonResults<int>("图片为空！");
            var list = new List<int>();

            var data = bmp.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var scanWidth = DeyiKeys.Size.LineScanWidth;
            var blackCount = scanWidth*DeyiKeys.Size.BlackScale;
            int startX = (data.Width - scanWidth) / 2,
                endX = (data.Width + scanWidth) / 2;
            unsafe
            {
                var ptr = (byte*)data.Scan0;
                int remain = data.Stride - data.Width*3,
                    linePtr = remain + data.Width*3;
                //跳过最小Y坐标
                ptr += linePtr*StartLineY;

                //遍历每行
                for (int i = StartLineY; i < data.Height; i++)
                {
                    //黑点数
                    int black = 0;
                    //跳过左则X坐标
                    ptr += startX * 3;
                    //遍历行内像素点
                    for (int j = startX; j < endX; j++)
                    {
                        //黑点判断
                        if (ptr[0] + ptr[1] + ptr[2] == 0)
                            black++;
                        ptr += 3;
                    }
                    //跳过当前行
                    ptr += (data.Width - endX) * 3 + remain;
                    //行内黑点数判断
                    if (black >= blackCount)
                    {
                        list.Add(i);
                        //if (list.Count == 2)
                        //    break;
                        i += LineHeight;
                        //跳过最小行间距
                        ptr += linePtr * LineHeight;
                    }
                }
            }
            bmp.UnlockBits(data);

            if (list.Count >= 2)
            {
                return new JsonResults<int>(list, list.Count);
            }
            return new JsonResults<int>("未找到图片分割线，您选择的图片");
        }
    }
}
