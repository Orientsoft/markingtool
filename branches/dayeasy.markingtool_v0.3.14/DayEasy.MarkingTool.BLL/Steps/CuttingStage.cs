using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Steps.Result;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DayEasy.MarkingTool.BLL.Steps
{
    /// <summary>
    /// 切图步骤
    /// </summary>
    public class CuttingStage : IStep
    {
        private Bitmap _imageToProcess;
        private List<int> _listY;

        private string _directoryName = string.Empty,
            _imgFilePath = string.Empty;

        private StepResult PreAction(object[] args)
        {
            _imgFilePath = (String)args[0];
            if(string.IsNullOrWhiteSpace(_imgFilePath))
                return new StepResult(false, new Exception(string.Format("图片:{0}没有找到！", _imgFilePath)));

            _listY = args[1] as List<int>;
            if (_listY == null || _listY.Count < 1)
                return new StepResult(false, new Exception(string.Format("图片:{0}没分割线！", _imgFilePath)));

            if (!File.Exists(_imgFilePath))
            {
                return new StepResult(false, new FileNotFoundException(string.Format("图片:{0}不存在.", _imgFilePath)));
            }
            _directoryName = Path.GetFileNameWithoutExtension(_imgFilePath);

            _imageToProcess = (Bitmap)Image.FromFile(_imgFilePath);

            if (!string.IsNullOrWhiteSpace(_directoryName))
            {
                var path = Path.Combine(DeyiKeys.SavePath, _directoryName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var regPath = Path.Combine(path, DeyiKeys.RecognitionName);
                if (!Directory.Exists(regPath))
                    Directory.CreateDirectory(regPath);
                var compressPath = Path.Combine(path, DeyiKeys.CompressName);
                if (!Directory.Exists(compressPath))
                    Directory.CreateDirectory(compressPath);
            }

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
                return Cut(_imageToProcess, _directoryName, DeyiKeys.SavePath);
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
        private StepResult Cut(Bitmap origBmp, String origFileName, string saveFilePath)
        {
            var startY = 0;
            using (origBmp)
            {
                try
                {
                    var basePath = Path.Combine(saveFilePath, origFileName);
                    var regPath = Path.Combine(basePath, DeyiKeys.RecognitionName);
                    //截取识别区
                    var regImage = ImageHelper.MakeImage(origBmp, 0, 0, origBmp.Width, _listY[0] + 6);
                    regImage.Save(Path.Combine(regPath, "0.jpg"));
                    regImage = ImageHelper.MakeImage(origBmp, 0, _listY[0] - 8, origBmp.Width,
                        _listY[1] - _listY[0] + 16);
                    regImage.Save(Path.Combine(regPath, "1.jpg"));
                    if (!_listY.Contains(origBmp.Height))
                        _listY.Add(origBmp.Height);
                    for (var j = 0; j < _listY.Count; j++)
                    {
                        var y = _listY[j];
                        var cutedImg = ImageHelper.MakeImage(origBmp, 0, startY, origBmp.Width, y - startY);
                        startY = y;
                        cutedImg = ImageHelper.Resize(cutedImg, DeyiKeys.PaperWidth);
                        cutedImg.Save(Path.Combine(basePath, j + ".jpg"), ImageFormat.Jpeg);
                    }
                    return StepResult.Success;
                }
                catch (Exception ex)
                {
                    ex = new Exception("未找到分割线，您选择的图片 " + origFileName + " 可能不是作业的扫描图片", ex);
                    return new StepResult(false, ex);
                }
            }
        }
    }
}