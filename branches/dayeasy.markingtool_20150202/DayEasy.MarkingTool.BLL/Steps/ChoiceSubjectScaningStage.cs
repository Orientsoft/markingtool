using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Scanners;
using DayEasy.MarkingTool.BLL.Steps.Result;
using DayEasy.Open.Model.Question;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DayEasy.MarkingTool.BLL.Steps
{
    /// <summary>
    /// 选择题扫描步骤
    /// </summary>
    public class ChoiceSubjectScaningStage : IStep
    {
        private ObjectiveScanner _scanner;
        private String _fileNameToCut = "";
        private Bitmap _bmp;

        private StepResult PreAction(params object[] args)
        {
            var orgiFileName = (String)args[0];
            var fileName = Path.GetFileNameWithoutExtension(orgiFileName);
            if (string.IsNullOrWhiteSpace(fileName))
                return new StepResult(false, new Exception("答题卡区域图片获取失败"));
            var basePath = Path.Combine(DeyiKeys.SavePath, fileName, DeyiKeys.RecognitionName);

            //1 —— 选择题答题区
            _fileNameToCut = Path.Combine(basePath, "1.jpg");

            if (!File.Exists(_fileNameToCut))
                return new StepResult(false, new FileNotFoundException(_fileNameToCut));

            _bmp = (Bitmap)Image.FromFile(_fileNameToCut);
            _bmp = ImageHelper.BinarizeImage(_bmp, DeyiKeys.AnswerThreshold);
            _scanner = new ObjectiveScanner(args[1] as IEnumerable<QuestionInfo> ?? new List<QuestionInfo>());
            return StepResult.Success;
        }

        public StepResult Process(params object[] args)
        {
            StepResult result;
            if (!(result = PreAction(args)).IsSuccess)
            {
                return result;
            }
            using (_bmp)
            {
                return _scanner.Scan(_bmp);
            }
        }
    }
}
