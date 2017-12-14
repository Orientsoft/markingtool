using Deyi.Tool.Common;
using Deyi.Tool.Step;
using Deyi.Tool.Steps;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Deyi.Tool.Scanners
{
    /// <summary>
    /// 线条扫描
    /// </summary>
    public class LineScanner : IScanner
    {
        private const int LineWidth = 400;
        private const float BlackScale = 0.85F;
        /// <summary>
        /// 获取图片分割线
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public StepResult Scan(Image img)
        {
            var bmp = img as Bitmap;
            if (bmp == null) return new StepResult(false, new Exception("图片为空！"));
            var list = new List<int>();
            var startY = 0;
            var allLine =
                Helper.EachMax(bmp.Height)
                    .Select(
                        y =>
                            new
                            {
                                y,
                                colors =
                                    Helper.Each((bmp.Width - LineWidth)/2, (bmp.Width + LineWidth)/2)
                                        .Select(x => bmp.GetPixel(x, y))
                            })
                    .ToArray();

            var listLine =
                allLine.Where(
                    line =>
                        line.y > DeyiKeys.PaperLineHeight &&
                        line.colors.Count(color => (color.R + color.B + color.G) == 0) > LineWidth*BlackScale)
                    .Select(line => line.y).ToList();
            listLine.Add(bmp.Height);
            listLine = listLine.Distinct().OrderBy(t => t).ToList();

            foreach (var y in listLine)
            {
                if (y - startY < DeyiKeys.PaperLineHeight)
                    continue;
                list.Add(y > bmp.Height ? bmp.Height : y);
                startY = y;
            }
            if (list.Count > 0)
            {
                //保证答题卡区域的完整性
                list[0] -= 4;
                list[1] += 4;
                return new LinesResult(list);
            }
            return new StepResult(false, new Exception("未找到图片分割线，您选择的图片"));
        }
    }
}
