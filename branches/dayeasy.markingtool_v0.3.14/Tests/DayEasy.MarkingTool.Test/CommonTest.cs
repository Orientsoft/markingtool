using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Scanners;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class CommonTest
    {
        /// <summary>
        /// 切出答题卡
        /// </summary>
        [TestMethod]
        public void LineCutTest()
        {
            var line = new LineScanner();
            var path = Path.Combine(DeyiKeys.CurrentDir, "2.jpg");
            var npath = Path.Combine(DeyiKeys.CurrentDir, "1.jpg");
            var img = Image.FromFile(path);
            try
            {
                //img = ImageHelper.Resize(img, 780);
                img = ImageHelper.BinarizeImage(new Bitmap(img), 140);
                var result = line.BitsScan(img);
                if (!result.Status || result.Data.Count() < 2) return;
                var lines = result.Data.ToList();
                img = ImageHelper.MakeImage(img, 0, lines[0], img.Width, lines[1] - lines[0]);
                img.Save(npath);
            }
            finally
            {
                if (img != null)
                    img.Dispose();
            }
        }

        /// <summary>
        /// 答题卡辅助测试
        /// </summary>
        [TestMethod]
        public void AnswerSheetHelperTest()
        {
            string img = Path.Combine(DeyiKeys.CurrentDir, "1.jpg");
            using (var answer = new AnswerSheetHelper(ImageHelper.LoadImage(img), 16, 24))
            {
                var result = answer.GetResult();
                var list = new List<int>();
                for (var i = 0; i < result.Count; i++)
                {
                    if (result[i])
                        list.Add(i + 1);
                }
                Console.Write(list.ToJson());
            }
        }

        [TestMethod]
        public void QrcodeTest()
        {
            //const string path = "student.png";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test00001.jpg");
            var scaner = new PaperBasicInfoScanner();
            var result = scaner.Scan(new Bitmap(path));
            if (result.Status)
                Console.Write(scaner.Student);
            //using (var helper = new QrCodeHelper(new Bitmap(path)))
            //{
            //    var no = helper.Decoder();
            //    Console.Write(no);
            //}
        }
        private const int Width = 75;
        private const int Height = 75;
        private const int LineCount = 9;
        private const int RowCount = 13;

        [TestMethod]
        public void PrintPageTest()
        {
            int currentCount = 1, currentRow = 1, x, y = 15, size = 4, page = 1;
            for (var i = 0; i < 60; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    if (currentCount > LineCount)
                    {
                        currentRow++;
                        currentCount = 1;
                    }
                    x = (currentCount - 1) * Width + 30;
                    Console.WriteLine("{0}:{1}-{2}:{3},{4}", page, i, j, x, y);
                    currentCount++;
                }
                currentCount++;
                var left = LineCount - currentCount + 1;
                if (left < size)
                {
                    currentCount = 1;
                    y += Height + 5;
                    currentRow++;
                }
                if (currentRow >= RowCount)
                {
                    page++;
                    currentRow = 1;
                    currentCount = 1;
                    y = 15;
                }
            }
        }

        [TestMethod]
        public void RegComplate()
        {
            //const string email = "530384746@qq.com";914981315
            const string email = "linlin2015@dy.com";
            var time = DateTime.Now.Ticks;
            const string token = "&email={0}&time={1}dayeasy.net_得一";
            const string url = "http://reg.dayeasy.net/reg/RegComplete?email={0}&time={1}&ac={2}";
            var ac = Helper.Md5(string.Format(token, email, time));
            Console.WriteLine(url, email, time, ac);
        }

        [TestMethod]
        public void DeskewTest()
        {
            var img = Image.FromFile("test.jpg");
            var btm = new Bitmap(img);
            var watch = Stopwatch.StartNew();
            watch.Start();
            var angle = ImageHelper.GetSkewAngle(btm);
            watch.Stop();
            Console.WriteLine("获取偏移角度[{0}]，用时：{1}s", angle, Math.Round(watch.ElapsedMilliseconds / 1000F, 2));
            watch.Reset();
            watch.Start();
            btm = ImageHelper.RotateImage(btm, (float)(-angle));
            watch.Stop();
            Console.WriteLine("图片旋转，用时：{0}s", Math.Round(watch.ElapsedMilliseconds / 1000F, 2));

            watch.Restart();
            var lines = new LineScanner().BitsScan(btm);
            Console.WriteLine(lines.Data.ToJson());
            watch.Stop();
            Console.WriteLine("获取切线坐标，用时：{0}s", Math.Round(watch.ElapsedMilliseconds / 1000F, 2));

            btm.Save("test_dekew.jpg");
        }

        [TestMethod]
        public void SliceTest()
        {
            var lines = new List<int> { 354, 532 };
            var watch = new Stopwatch();
            watch.Start();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_dekew.jpg");
            using (var slice = new SliceMap((Bitmap)Image.FromFile(path), lines,
                new FileManager("ddd", 1), "test_dekew.jpg"))
            {
                var result = slice.Check();
                watch.Stop();
                Console.WriteLine("基础检测，用时：{0}s", Math.Round(watch.ElapsedMilliseconds / 1000F, 2));
                if (!result.Status)
                {
                    Console.WriteLine(result.Message);
                    return;
                }
                watch.Restart();
                slice.RecognitionCut();
                watch.Stop();
                Console.WriteLine("识别区域，用时：{0}s", Math.Round(watch.ElapsedMilliseconds / 1000F, 2));
                watch.Restart();
                var cResult = slice.CutMission();
                Console.WriteLine(cResult.ToJson());
                watch.Stop();
                Console.WriteLine("切线，用时：{0}s", Math.Round(watch.ElapsedMilliseconds / 1000F, 2));
            }
        }

        [TestMethod]
        public void BasicQrcodeTest()
        {
            var d = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errorCode"));
            var list = d.GetFiles();
            foreach (var info in list)
            {
                var scanner = new PaperBasicInfoScanner();
                var currentImage = (Bitmap)Image.FromFile(info.FullName);
                foreach (var threshold in DeyiKeys.QrcodeThresholds)
                {
                    var item = ImageHelper.BinarizeImage(currentImage, threshold);
                    scanner.ConvertBmp(item);
                    if (scanner.Student != null)
                    {
                        Console.WriteLine(threshold);
                        Console.WriteLine(scanner.Student.ToJson());
                        break;
                    }
                }
                if (scanner.Student == null)
                    Console.WriteLine("{0},识别失败", info.FullName);
            }
        }

        [TestMethod]
        public void AlgorithmTest()
        {
            const string name = "test_01.jpg";
            const string name01 = "test_01_a.jpg";
            var img = (Bitmap) Image.FromFile(name);
            img = ImageHelper.ThiningPic(img, 82);
            img.Save(name01);
        }
    }
}
