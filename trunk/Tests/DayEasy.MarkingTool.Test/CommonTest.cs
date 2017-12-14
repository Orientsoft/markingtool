using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Recognition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;


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
            //var line = new LineScanner();
            //var path = Path.Combine(DeyiKeys.CurrentDir, "2.jpg");
            //var npath = Path.Combine(DeyiKeys.CurrentDir, "1.jpg");
            //var img = Image.FromFile(path);
            //try
            //{
            //    //img = ImageHelper.Resize(img, 780);
            //    img = ImageHelper.BinarizeImage(new Bitmap(img), 140);
            //    var result = line.BitsScan(img);
            //    if (!result.Status || result.Data.Count() < 2) return;
            //    var lines = result.Data.ToList();
            //    img = ImageHelper.MakeImage(img, 0, lines[0], img.Width, lines[1] - lines[0]);
            //    img.Save(npath);
            //}
            //finally
            //{
            //    if (img != null)
            //        img.Dispose();
            //}
        }

        /// <summary>
        /// 答题卡辅助测试
        /// </summary>
        [TestMethod]
        public void AnswerSheetHelperTest()
        {
            //string img = Path.Combine(DeyiKeys.CurrentDir, "1.jpg");
            //using (var answer = new AnswerSheetHelper(ImageHelper.LoadImage(img), 16, 24))
            //{
            //    var result = answer.GetResult();
            //    var list = new List<int>();
            //    for (var i = 0; i < result.Count; i++)
            //    {
            //        if (result[i])
            //            list.Add(i + 1);
            //    }
            //    Console.Write(list.ToJson());
            //}
        }

        [TestMethod]
        public void QrcodeTest()
        {
            //const string path = "student.png";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test00001.jpg");
            //            var scaner = new PaperBasicInfoScanner();
            //            var result = scaner.Scan(new Bitmap(path));
            //            if (result.Status)
            //                Console.Write(scaner.Student);
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
            //            var img = Image.FromFile("test.jpg");
            //            var btm = new Bitmap(img);
            //            var watch = Stopwatch.StartNew();
            //            watch.Start();
            //            var angle = ImageHelper.GetSkewAngle(btm);
            //            watch.Stop();
            //            Console.WriteLine("获取偏移角度[{0}]，用时：{1}s", angle, Math.Round(watch.ElapsedMilliseconds / 1000F, 2));
            //            watch.Reset();
            //            watch.Start();
            //            btm = ImageHelper.RotateImage(btm, (float)(-angle));
            //            watch.Stop();
            //            Console.WriteLine("图片旋转，用时：{0}s", Math.Round(watch.ElapsedMilliseconds / 1000F, 2));
            //
            //            watch.Restart();
            //            var lines = new LineScanner().BitsScan(btm);
            //            Console.WriteLine(lines.Data.ToJson());
            //            watch.Stop();
            //            Console.WriteLine("获取切线坐标，用时：{0}s", Math.Round(watch.ElapsedMilliseconds / 1000F, 2));
            //
            //            btm.Save("test_dekew.jpg");
        }

        [TestMethod]
        public void SliceTest()
        {
            //            var lines = new List<LineInfo>
            //            {
            //                new LineInfo
            //                {
            //                    StartY = 354,
            //                    BlackCount = 2000,
            //                    Move = 0
            //                },
            //                new LineInfo
            //                {
            //                    StartY = 532,
            //                    BlackCount = 2000,
            //                    Move = 0
            //                }
            //            };
            //            var watch = new Stopwatch();
            //            watch.Start();
            //            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_dekew.jpg");
            //            using (var slice = new SliceMap((Bitmap)Image.FromFile(path), lines,
            //                new FileManager(2563252, "2dfdcccc"), "test_dekew.jpg"))
            //            {
            //                var result = slice.Check();
            //                watch.Stop();
            //                Console.WriteLine("基础检测，用时：{0}s", Math.Round(watch.ElapsedMilliseconds / 1000F, 2));
            //                if (!result.Status)
            //                {
            //                    Console.WriteLine(result.Message);
            //                    return;
            //                }
            //                watch.Restart();
            //                slice.RecognitionCut();
            //                watch.Stop();
            //                Console.WriteLine("识别区域，用时：{0}s", Math.Round(watch.ElapsedMilliseconds / 1000F, 2));
            //                watch.Stop();
            //                //var cResult = slice.CutMission();
            //                //Console.WriteLine(cResult.ToJson());
            //                //watch.Stop();
            //                //Console.WriteLine("切线，用时：{0}s", Math.Round(watch.ElapsedMilliseconds / 1000F, 2));
            //            }
        }

        [TestMethod]
        public void BasicQrcodeTest()
        {
            //            var d = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errorCode"));
            //            var list = d.GetFiles();
            //            foreach (var info in list)
            //            {
            //                var scanner = new PaperBasicInfoScanner();
            //                var currentImage = (Bitmap)Image.FromFile(info.FullName);
            //                foreach (var threshold in DeyiKeys.QrcodeThresholds)
            //                {
            //                    var item = ImageHelper.BinarizeImage(currentImage, threshold);
            //                    scanner.ConvertBmp(item);
            //                    if (scanner.Student != null)
            //                    {
            //                        Console.WriteLine(threshold);
            //                        Console.WriteLine(scanner.Student.ToJson());
            //                        break;
            //                    }
            //                }
            //                if (scanner.Student == null)
            //                    Console.WriteLine("{0},识别失败", info.FullName);
            //            }
        }

        //[TestMethod]
        //public void AlgorithmTest()
        //{
        //    const string name = "test_01.jpg";
        //    const string name01 = "test_01_a.jpg";
        //    var bmps = new List<Bitmap> { (Bitmap)Image.FromFile(name), (Bitmap)Image.FromFile(name01) };
        //    for (int i = 0; i < bmps.Count; i++)
        //    {
        //        var item = ImageHelper.RemoveBlackEdge(bmps[i]);
        //        bmps[i] = item;
        //        var angle = ImageHelper.GetSkewAngle(item);
        //        if (angle > 0 || angle < 0)
        //            bmps[i] = ImageHelper.RotateImage(item, (float)-angle);
        //    }
        //    var bmp = ImageHelper.CombineBitmaps(bmps);
        //    bmp = (Bitmap)ImageHelper.Resize(bmp, 780);
        //    bmp.Save("test_combine.jpg");
        //}

        [TestMethod]
        public void ResizeTest()
        {
            Console.WriteLine("http://passport.deyi.com".UrlEncode());
            //            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_resize.jpg");
            //            var bmp = Image.FromFile("2.jpg");
            //            ImageHelper.Resize((Bitmap)bmp, path, DeyiKeys.PaperWidth, 0, 85);
        }

        [TestMethod]
        public void Test()
        {
            var ms = new MemoryStream();
            var watch = new Stopwatch();
            var random = new Random();
            const int maxValue = 120000;
            const int length = 70000;
            var arrayA = new int[length];
            var arrayB = new int[length];
            var temp = new bool[maxValue];
            var exists = new bool[length];
            for (var i = 0; i < length; i++)
            {
                arrayA[i] = random.Next(maxValue);
                arrayB[i] = random.Next(maxValue);
            }
            watch.Start();
            foreach (var i in arrayB)
            {
                temp[i] = true;
            }
            for (var i = 0; i < arrayA.Length; i++)
            {
                exists[i] = temp[arrayA[i]];
            }
            watch.Stop();
            Console.WriteLine("用时：{0} ms", watch.ElapsedMilliseconds);
            Console.Write(exists.ToJson());
        }

        [TestMethod]
        public void JsonTest()
        {
            Console.WriteLine(Guid.NewGuid().ToString().Replace("-", "").ToLower());
            //            var result =
            //                "{\"status\":true,\"message\":\"\",\"count\":11,\"data\":[{\"id\":1,\"name\":\"语文\"},{\"id\":2,\"name\":\"数学\"},{\"id\":3,\"name\":\"英语\"},{\"id\":4,\"name\":\"物理\"},{\"id\":5,\"name\":\"化学\"},{\"id\":6,\"name\":\"政治\"},{\"id\":7,\"name\":\"历史\"},{\"id\":8,\"name\":\"地理\"},{\"id\":9,\"name\":\"生物\"},{\"id\":10,\"name\":\"计算机\"},{\"id\":11,\"name\":\"体育\"}]}";
            //            //            var json = JsonConvert.DeserializeObject<DResults<MSubjectDto>>(result);
            //            var json = result.JsonToObject<DResults<MSubjectDto>>();
            //            Console.Write(json);
        }
    }
}
