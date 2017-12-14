using System.Printing;
using System.Web.UI;
using DayEasy.MarkingTool.BLL.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Scanners
{
    /// <summary>
    /// 切图
    /// </summary>
    public class SliceMap
    {
        public JsonResult<int> SliceAction(string imgPath, List<int> lines)
        {
            if (string.IsNullOrWhiteSpace(imgPath))
                return new JsonResult<int>(string.Format("图片:{0}没有找到！", imgPath));

            if (lines == null || !lines.Any())
                return new JsonResult<int>(string.Format("图片:{0}没分割线！", imgPath));

            if (!File.Exists(imgPath))
            {
                return new JsonResult<int>(string.Format("图片:{0}不存在.", imgPath));
            }
            var directoryName = Path.GetFileNameWithoutExtension(imgPath);

            if (!string.IsNullOrWhiteSpace(directoryName))
            {
                var path = Path.Combine(DeyiKeys.SavePath, directoryName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var regPath = Path.Combine(path, DeyiKeys.RecognitionName);
                if (!Directory.Exists(regPath))
                    Directory.CreateDirectory(regPath);
                var compressPath = Path.Combine(path, DeyiKeys.CompressName);
                if (!Directory.Exists(compressPath))
                    Directory.CreateDirectory(compressPath);
            }
            var imageToProcess = (Bitmap)Image.FromFile(imgPath);
            return CutMission(imageToProcess, lines, directoryName);
        }

        /*
         * 切图思路
         * 1,获取原图成 Bitmap 格式
         * 2,找到第一个分割线（获取其纵坐标：y）
         * 3,从(0,0)点到(0,y)点，画出矩形
         * 4,从原图中扣下上述矩形的图像
         * 5,在新图中生成上述矩形的图像
         * 6,保存新生成的图像
         * 7,将(0,0)点的坐标，替换成(0,y)点坐标
         * 8,从第3步开始重复执行
         * 注：因为只需要找横线，所以，直接按照横坐标是0的方式来找
         *     如果不能满足横线贯穿整个页面，则调整横坐标的起始值即可
         * 测试：目前在PS生成的图片上，已经切割成功，可自行测试
         * 
         * 
         * 可能导致切图不全或者误切！
         */

        private JsonResult<int> CutMission(Bitmap origBmp, List<int> lines, string directoryName)
        {
            var startY = 0;
            int totalHeight = 0;
            using (origBmp)
            {
                try
                {
                    var basePath = Path.Combine(DeyiKeys.SavePath, directoryName);
                    var regPath = Path.Combine(basePath, DeyiKeys.RecognitionName);
                    //截取识别区
                    var regImage = ImageHelper.MakeImage(origBmp, 0, 0, origBmp.Width, lines[0] + 6);
                    regImage.Save(Path.Combine(regPath, "0.jpg"));
                    regImage = ImageHelper.MakeImage(origBmp, 0, lines[0] - 8, origBmp.Width,
                        lines[1] - lines[0] + 16);
                    regImage.Save(Path.Combine(regPath, "1.jpg"));
                    if (!lines.Contains(origBmp.Height))
                        lines.Add(origBmp.Height);
                    for (var j = 0; j < lines.Count; j++)
                    {
                        var y = lines[j];
                        var cutedImg = ImageHelper.MakeImage(origBmp, 0, startY, origBmp.Width, y - startY);
                        startY = y;
                        cutedImg = ImageHelper.Resize(cutedImg, DeyiKeys.PaperWidth);
                        totalHeight += cutedImg.Height;
                        cutedImg.Save(Path.Combine(basePath, j + ".jpg"), ImageFormat.Jpeg);
                    }
                    var pageCount =
                        (int) Math.Ceiling((double) totalHeight/Helper.A4Size.Height ?? 1100);
                    return new JsonResult<int>(true, pageCount);
                }
                catch (Exception ex)
                {
                    return new JsonResult<int>(string.Format("未找到分割线，您选择的图片 {0} 可能不是作业的扫描图片", directoryName));
                }
            }
        }
    }
}
