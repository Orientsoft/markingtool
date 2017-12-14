using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Recognition;
using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace DayEasy.MarkingTool.Test.Scanner
{
    public class OpenCv : DRecognition
    {
        private Mat _sourceMat;
        public OpenCv(string imagePath, List<ObjectiveItem> objectives) : base(imagePath, objectives)
        {
            _sourceMat = Cv2.ImRead(imagePath);
        }

        protected override void Resize()
        {
            var width = DeyiKeys.ScannerConfig.PaperWidth;
            var height = (int)Math.Ceiling(SourceBmp.Height * width / (double)SourceBmp.Width);
            _sourceMat = _sourceMat.Resize(new Size(width, height));
        }

        protected override void Binary()
        {
            _sourceMat = _sourceMat.Threshold(170, 255, ThresholdTypes.Binary);
        }

        protected override void FindLines()
        {

        }

        protected override void Rotate()
        {

        }
    }
}
