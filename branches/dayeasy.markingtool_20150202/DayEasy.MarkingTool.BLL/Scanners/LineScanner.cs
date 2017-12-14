using DayEasy.MarkingTool.BLL.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Scanners
{
    /// <summary>
    /// 线条扫描
    /// </summary>
    public class LineScanner
    {
        private const int StartLineY = 120;
        private const int LineHeight = 60;

        /// <summary>
        /// 获取图片分割线
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public JsonResults<int> Scan(Image img)
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
                var colors = Helper.Each((width - scanWidth)/2, (width + scanWidth)/2)
                    .Select(x => bmp.GetPixel(x, y)).ToArray();
                var blacks = colors.Count(c => (c.R + c.B + c.G) == 0);
                if (blacks < (scanWidth*DeyiKeys.Size.BlackScale))
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
    }
}
