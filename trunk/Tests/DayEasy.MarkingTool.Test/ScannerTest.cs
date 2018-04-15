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

        [TestMethod]
        public void FindePointsTest()
        {
            var bmp = (Bitmap)Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "\\paper\\image00099.JPG");
            bmp = ImageHelper.RotateA3Image(bmp);
            bmp = (Bitmap)ImageHelper.Resize(bmp, 780 * 2);
            bmp = ImageHelper.RotateImage(bmp);
            bmp.Save(AppDomain.CurrentDomain.BaseDirectory + $"\\paper\\a3.jpg");
            var centerX = bmp.Width / 2;
            var b1 = ImageHelper.MakeImage((Bitmap)bmp.Clone(), 0, 0, centerX, bmp.Height);
            var b2 = ImageHelper.MakeImage((Bitmap)bmp.Clone(), centerX, 0, centerX, bmp.Height);
            b1.Save(AppDomain.CurrentDomain.BaseDirectory + "\\paper\\b1.JPG");
            b2.Save(AppDomain.CurrentDomain.BaseDirectory + "\\paper\\b2.JPG");
            //var finder = new PointsFinder();
            //var result = finder.Find(bmp);  
            //foreach (var point in result.HorizonPoints)
            //{
            //    var b = ImageHelper.MakeImage(bmp, point.X, point.Y, point.Width, point.Height);
            //    b.Save(AppDomain.CurrentDomain.BaseDirectory + $"\\paper\\point{point.X}_{point.Y}.jpg");
            //    b.Dispose();
            //}
            //bmp.Dispose();
            //Console.WriteLine(JsonHelper.ToJson(result));
        }
    }
}
