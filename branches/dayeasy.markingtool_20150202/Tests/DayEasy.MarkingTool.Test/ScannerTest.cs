using System;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class ScannerTest
    {
        [TestMethod]
        public void PaperScannerTest()
        {
            var img = ImageHelper.ScoreImage(85.5M);
            img.Save("score.jpg");
        }
    }
}
