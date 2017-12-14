using System.Linq;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Scanners;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
            var line = new LineScanner();
            var path = Path.Combine(DeyiKeys.CurrentDir, "2.jpg");
            var npath = Path.Combine(DeyiKeys.CurrentDir, "1.jpg");
            var img = Image.FromFile(path);
            try
            {
                //img = ImageHelper.Resize(img, 780);
                img = ImageHelper.BinarizeImage(new Bitmap(img), 140);
                var result = line.Scan(img);
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
                    x = (currentCount - 1)*Width + 30;
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
            const string email = "914981315@qq.com";
            var time = DateTime.Now.Ticks;
            const string token = "&email={0}&time={1}dayeasy.net_得一";
            const string url = "http://reg.dayeasy.net/reg/RegComplete?email={0}&time={1}&ac={2}";
            var ac = Helper.Md5(string.Format(token, email, time));
            Console.WriteLine(url, email, time, ac);
        }
    }
}
