using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Recognition;
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
            var bmp = (Bitmap)Image.FromFile("b1016a0001.jpg");
            using (var scanner = new LineFinder(bmp))
            {
                var list = scanner.Find((bmp.Width * 85) / 100, 50, -1, 400);

                for (int i = 0; i < list.Count(); i++)
                {
                    var item = list[i];
                    var y = 0;
                    if (i > 0)
                        y = (list[i - 1].StartY + (item.Move > 0 ? 0 : item.Move)) - 5;
                    var h = item.StartY - y + Math.Abs(item.Move) + 5;
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
            var bmp = (Bitmap)Image.FromFile("sheet/s1.jpg");
            bmp = bmp.ToBinaryBitmap();
            bmp.Save("sheet/s1_binary.jpg");
            //bmp = ImageHelper.BinarizeImage(bmp, 221);
            //bmp.Save("1_1.jpg");
        }

        [TestMethod]
        public void AnswerSheetTest()
        {
            var bmp = (Bitmap)Image.FromFile("sheet/e3.jpg");
            var lines = new LineFinder(bmp).Find((int)Math.Ceiling(bmp.Width * DeyiKeys.ScannerConfig.BlackScale),
                DeyiKeys.ScannerConfig.LineHeight, skip: 100);
            Console.WriteLine(lines.ToJson());
            if (lines.Count < 2)
                return;
            var y = lines[0].StartY - 20;
            var x = 0;
            var height = lines[1].StartY - y + 40;
            bmp = (Bitmap)ImageHelper.MakeImage(bmp, x, y, bmp.Width, height);
            var angle = (float)lines.Average(t => t.Angle);
            bmp = ImageHelper.RotateImage(bmp, -angle);
            bmp.Save("sheet/test.png");

            //var finder = new LineFinder(bmp);
            //var sheetLine = (int)Math.Ceiling(bmp.Width * 0.42);
            //var result = finder.Find(sheetLine, 10, -1, 10);

            //Console.WriteLine(result.Count);
            //Console.WriteLine(result.ToJson());

            //using (var helper = new AnswerSheetHelper(bmp, 5, 24))
            //{
            //    Console.Write(helper.GetResult().ToJson());
            //}
        }

        [TestMethod]
        public void SheetFilledTest()
        {
            var bmp = (Bitmap)Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "\\sheet\\sheet.png");
            var filled = ImageHelper.IsFilled(bmp);
            Console.WriteLine(filled);
        }
    }
}
