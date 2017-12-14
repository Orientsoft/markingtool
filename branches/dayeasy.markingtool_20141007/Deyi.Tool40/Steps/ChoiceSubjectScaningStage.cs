using Deyi.Tool.Common;
using Deyi.Tool.PaperServiceReference;
using Deyi.Tool.Scanners;
using Deyi.Tool.Step;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;


namespace Deyi.Tool.Steps
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

            //1 —— 选择题答题区
            _fileNameToCut = string.Format("{0}\\{1}\\1.jpg", DeyiKeys.SavePath,
                Path.GetFileNameWithoutExtension(orgiFileName));

            if (!File.Exists(_fileNameToCut))
                return new StepResult(false, new FileNotFoundException(_fileNameToCut));

            _bmp = (Bitmap)Image.FromFile(_fileNameToCut);
            _bmp = ImageHelper.BinarizeImage(_bmp, DeyiKeys.AnswerThreshold);
            _scanner = new ObjectiveScanner(args[1] as IEnumerable<QuestionEntity> ?? new List<QuestionEntity>());
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
