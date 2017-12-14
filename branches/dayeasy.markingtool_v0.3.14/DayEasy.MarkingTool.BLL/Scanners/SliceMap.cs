using DayEasy.MarkingTool.BLL.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Scanners
{
    /// <summary>
    /// 切图
    /// </summary>
    public class SliceMap : IDisposable
    {
        private readonly Bitmap _sourceBmp;
        private readonly List<int> _lines;
        private string _imageName;
        private readonly FileManager _fileManager;

        public SliceMap(Bitmap sourceBmp, List<int> lines,FileManager fileManager,string imageName)
        {
            _sourceBmp = sourceBmp;
            _imageName = imageName;
            _lines = lines;
            _fileManager = fileManager;
        }

        /// <summary>
        /// 执行切线任务
        /// </summary>
        /// <returns></returns>
        public JsonResult<int> StartAction()
        {
            var result = Check();
            if (!result.Status)
                return new JsonResult<int>(result.Message);
            RecognitionCut();
            return CutMission();
        }

        /// <summary>
        /// 基础检测
        /// </summary>
        /// <returns></returns>
        public JsonResultBase Check()
        {
            if (_sourceBmp == null)
                return new JsonResultBase(string.Format("图片没有找到！"));

            if (_lines == null || !_lines.Any())
                return new JsonResultBase(string.Format("图片没分割线！"));
            if (_lines.Count < 2)
                return new JsonResultBase(string.Format("没有识别区域分割线！"));
            _imageName = Path.GetFileNameWithoutExtension(_imageName) ?? Helper.Guid32.Substring(5, 6);
            //文件夹检测
            _fileManager.CheckDirectory(_imageName);
            return new JsonResultBase(true, string.Empty);
        }

        /// <summary>
        /// 切出需识别的图片
        /// </summary>
        /// <returns></returns>
        public void RecognitionCut()
        {
            //截取识别区
            int width = _sourceBmp.Width,
                qrHeight = _lines[0] + 6,
                sheetY = _lines[0] - 8,
                sheetHeight = _lines[1] - _lines[0] + 16;

            //二维码识别图
            var regImage = ImageHelper.MakeImage(_sourceBmp, 0, 0, width, qrHeight);

            _fileManager.SaveRecognitionImage(_imageName,regImage);

            //答题卡识别图
            regImage = ImageHelper.MakeImage(_sourceBmp, 0, sheetY, width, sheetHeight);
            _fileManager.SaveRecognitionImage(_imageName, regImage, RecognitionType.AnswerSheet);
            regImage.Dispose();
        }

        /// <summary>
        /// 切图操作
        /// </summary>
        /// <returns></returns>
        public JsonResult<int> CutMission()
        {
            var startY = 0;
            int totalHeight = 0;
            try
            {
                if (!_lines.Contains(_sourceBmp.Height))
                    _lines.Add(_sourceBmp.Height);
                Image cutedImg = null;
                for (var j = 0; j < _lines.Count; j++)
                {
                    var y = _lines[j];
                    cutedImg = ImageHelper.MakeImage(_sourceBmp, 0, startY, _sourceBmp.Width, y - startY,
                        DeyiKeys.PaperWidth);
                    startY = y;
                    totalHeight += cutedImg.Height;
                    _fileManager.SaveImage(_imageName, cutedImg, j);
                }
                var pageCount = (int) Math.Ceiling(totalHeight/(Helper.A4Size.Height ?? 1100D));
                if (cutedImg != null)
                    cutedImg.Dispose();
                return new JsonResult<int>(true, pageCount);
            }
            catch (Exception)
            {
                return new JsonResult<int>(string.Format("未找到分割线，您选择的图片 {0} 可能不是作业的扫描图片", _imageName));
            }
        }

        public void Dispose()
        {
            if (_sourceBmp != null)
                _sourceBmp.Dispose();
        }
    }
}
