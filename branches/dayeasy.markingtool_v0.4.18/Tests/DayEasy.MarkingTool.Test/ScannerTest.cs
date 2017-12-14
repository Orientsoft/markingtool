using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Linq;

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

        [TestMethod]
        public void HoughTest()
        {
            var bmp = (Bitmap)Image.FromFile("test.jpg");
            //CvInvoke.cvHoughLines2()
            //var lineBmp = lineTransform.ToBitmap();
            //lineBmp.Save("test_hough.jpg");
        }

        [TestMethod]
        public void LineScannerTest()
        {
            var bmp = (Bitmap)Image.FromFile("test.jpg");
            using (var scanner = new LineFinder(bmp))
            {
                var list = scanner.Find((bmp.Width * 8) / 10, 50, -1, 160);

                for (int i = 0; i < list.Count(); i++)
                {
                    var item = list[i];
                    var y = 0;
                    if (i > 0)
                        y = (list[i - 1].Start + (item.Move > 0 ? 0 : item.Move)) - 5;
                    var h = item.Start - y + Math.Abs(item.Move) + 5;
                    var temp = (Bitmap)ImageHelper.MakeImage(bmp.Clone() as Bitmap, 0, y, bmp.Width, h);
                    temp = ImageHelper.RotateImage(temp, (float)-item.Angle);
                    var absMove = Math.Abs(item.Move);
                    y = (i == 0 ? 0 : absMove / 2);
                    temp =
                        (Bitmap)
                            ImageHelper.MakeImage(temp, 0, y, temp.Width, h - (item.Move > 0 ? item.Move / 2 : absMove));
                    temp.Save(string.Format("test_{0}.jpg", i + 1));
                }
                Console.WriteLine(list.ToJson());
            }
        }

        [TestMethod]
        public void ResetBackgroundTest()
        {
            var bmp = (Bitmap)Image.FromFile("1.jpg");
            var threshold = ImageHelper.GetThreshold(bmp);
            Console.WriteLine(threshold);
            //bmp = ImageHelper.BinarizeImage(bmp, 221);
            //bmp.Save("1_1.jpg");
        }

        [TestMethod]
        public void AnswerSheetTest()
        {
            var bmp = (Bitmap) Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "\\sheet\\2.jpg");
            using (var helper = new AnswerSheetHelper(bmp, 17, 24))
            {
                Console.Write(helper.GetResult().ToJson());
            }
        }
    }
}
