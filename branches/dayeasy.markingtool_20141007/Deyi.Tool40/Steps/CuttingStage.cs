using Deyi.Tool.Common;
using Deyi.Tool.Step;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace Deyi.Tool.Steps
{
    public class CuttingStage : IStep
    {
        private Bitmap _imageToProcess;
        private List<int> _listY;

        private string _directoryName = string.Empty, _imgFilePath = string.Empty;

        private StepResult PreAction(object[] args)
        {
            _imgFilePath = (String)args[0];
            if(string.IsNullOrWhiteSpace(_imgFilePath))
                return new StepResult(false, new Exception(string.Format("图片:{0}没有找到！", _imgFilePath)));

            _listY = args[1] as List<int>;
            if (_listY == null || _listY.Count < 1)
                return new StepResult(false, new Exception(string.Format("图片:{0}没分割线！", _imgFilePath)));

            //FileInfo fi = new FileInfo(imgFilePath);
            //if (!fi.Exists)
            //    ex = new FileNotFoundException(imgFilePath);
            //else if (!ConfigMgr.Instance["SupportedImageFileType"].Contains(fi.Extension.ToLower()))
            //    ex = new FormatException(imgFilePath + " 不是一个图片文件");

            _directoryName = Path.GetFileNameWithoutExtension(_imgFilePath);

            //if (imageToProcess != null)
            //{
            //    imageToProcess.Dispose();
            //}
            if (!File.Exists(_imgFilePath))
            {
                return new StepResult(false, new FileNotFoundException(string.Format("图片:{0}不存在.", _imgFilePath)));
            }
            _imageToProcess = (Bitmap)Image.FromFile(_imgFilePath);

            //imageToProcess.is
            if (!string.IsNullOrWhiteSpace(_directoryName))
            {
                var path = Path.Combine(DeyiKeys.SavePath, _directoryName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            //if (!Directory.Exists(SavePath))
            //    Directory.CreateDirectory(SavePath);

            //if (!Directory.Exists(SavePath + "\\" + _fileName))
            //    Directory.CreateDirectory(SavePath + "\\" + _fileName);

            return StepResult.Success;

        }

        public StepResult Process(object[] args)
        {
            using (_imageToProcess)
            {
                var result = PreAction(args);
                if (!result.IsSuccess)
                {
                    return result;
                }
                int totalCutted = 0;
                return Cut(_imageToProcess, _directoryName, DeyiKeys.SavePath, out totalCutted);
            }
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
        private StepResult Cut(Bitmap origBmp, String origFileName, string saveFilePath, out int i)
        {
            const int imgLeftCut = 0;

            int origY = 0, height = 0, j = 0;
            var filePath = saveFilePath + "\\" + origFileName + "\\{0}.jpg";
            Image cutedImg = null;
            try
            {
                //保存图片
                _listY.ForEach(y =>
                {
                    if (origY != y)
                    {
                        height = y - origY;

                        cutedImg = ImageHelper.MakeImage(origBmp, imgLeftCut, origY, origBmp.Width - imgLeftCut, height);
                        cutedImg = ImageHelper.Resize(cutedImg, DeyiKeys.PaperWidth);
                        cutedImg.Save(string.Format(filePath, j), ImageFormat.Jpeg);
                        origY = y;
                        j++;
                    }
                });
                i = j;
                return StepResult.Success;
            }
            catch (Exception ex)
            {
                i = j;
                ex = new Exception("未找到分割线，您选择的图片 " + origFileName + " 可能不是作业的扫描图片", ex);
                return new StepResult(false, ex);
            }
            finally
            {
                if (origBmp != null)
                    origBmp.Dispose();
            }
        }

        // Func<int, int, IEnumerable<int>> Each = delegate(int min, int max) { return Enumerable.Range(min, max - min); };

        // private List<>

        //class PaperLine
        //{
        //    public int CenterY { get; set; }
        //    public List<Color> Colors { get; set; }

        //    public static bool IsLine()
        //    {

        //    }
        //}

        //        private List<int> GetY(Bitmap bmp)
        //        {
        //            var list = new List<int>();
        //            var startY = 0;
        //            var allLine = Each(0, bmp.Height).Select(y => new { y, colors = Each((bmp.Width / 2 - 200), (bmp.Width / 2 + 200)).Select(x => GetPix(bmp, y, x)) }).ToArray();

        //#if false
        //            System.Data.DataTable ds = new System.Data.DataTable();

        //            ds.Columns.Add("Y");
        //            foreach (var i in allLine.First().colors)
        //            {
        //                ds.Columns.Add();
        //            }

        //            foreach (var i in allLine)
        //            {
        //                System.Collections.ArrayList al = new System.Collections.ArrayList();
        //                al.Add(i.y);

        //                foreach (var color in i.colors)
        //                    al.Add(String.Format(color.R + ", "+color.G  + ", " + color.B));

        //                ds.Rows.Add(al.ToArray());
        //            }
        //#endif

        //            var listLine = allLine.Where(line => line.colors.Count(color => color.R + color.B + color.G < 100) > 240);
        //            // var listLine = allLine.Where(line => line.colors.Aggregate(0, (total, next) =>next.Any(c => (c.R + c.G + c.B) / 3 < 160) ? total += 1 : total) > 200);


        //            foreach (var line in listLine)
        //            {
        //                if (line.y - startY < 14)//TODO:....
        //                {
        //                    continue;
        //                }

        //                list.Add(line.y > bmp.Height ? bmp.Height : line.y);
        //                startY = line.y;
        //            }

        //            if (list[list.Count - 1] != bmp.Height)
        //            {
        //                list.Add(bmp.Height);
        //            }

        //            return list;
        //        }



        //int offset = 3;

        //private static List<Color> GetPix(Bitmap bmp, int y, int x)
        //{

        //    List<Color> colors = new List<Color>();
        //    colors.Add(bmp.GetPixel(x, y));
        //    for (int i = 1; i <= 3; i++)
        //    {
        //        colors.Add(bmp.GetPixel(x, y - i));
        //        colors.Add(bmp.GetPixel(x, y + i));
        //    }
        //    return colors;
        //    //  return Color.FromArgb((color.R + color.G + color.B) / 3);
        //}


        //private static Color GetPix(Bitmap bmp, int y, int x)
        //{
        //    var color = bmp.GetPixel(x, y);
        //    return Color.FromArgb(color.R / 3, color.G / 3, color.B / 3);
        //}

        //private void MakeImage(Bitmap origBmp, int x, int y, int width, int height, string filePath)
        //{

        //    using (var newBmp = new Bitmap(width, height))// 生成新的画布
        //    {
        //        using (var g = Graphics.FromImage(newBmp))  // 将画布读取到图像中
        //        {
        //            var origRect = new Rectangle(x, y, width, height); // 原始图像矩形框
        //            var destRect = new Rectangle(0, 0, width, height); // 新画布图像矩形框
        //            g.DrawImage(origBmp, destRect, origRect, GraphicsUnit.Pixel); // 将原始图像矩形框中的内容生成到新画布中去
        //            newBmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg); // 保存新画布中的图像
        //        }
        //    }
        //}
    }
}
