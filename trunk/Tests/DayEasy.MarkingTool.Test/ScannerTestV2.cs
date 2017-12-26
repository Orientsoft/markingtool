using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class ScannerTestV2
    {
        [TestMethod]
        public void Main()
        {
            var objectives = new List<ObjectiveItem>();
            for (int i = 0; i < 15; i++)
            {
                objectives.Add(new ObjectiveItem { Sort = (i + 1).ToString(), Single = true, Count = 4 });
            }

            //const string directoryPath = @"C:\Users\luoyong\Desktop\金牛三年制物理（样本）";
            //const string directoryPath = @"C:\Users\luoyong\Desktop\原始扫描图片";
            const string directoryPath = @"v3\sheets";
            const int combine = 1;
            var paths = Directory.GetFiles(directoryPath);
            var count = (int)Math.Ceiling(paths.Length / (double)combine);
            var tasks = new List<Task>();
            for (var i = 0; i < count; i++)
            {
                var list = paths.Skip(i * combine).Take(combine).ToArray();
                var index = i + 1;
                var task = Task.Factory.StartNew(() =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(index.ToString());
                    Action<string> logAction = msg => { sb.AppendLine(msg); };
                    using (var scanner = new BLL.Recognition.DefaultRecognition(list[0], objectives, false))
                    {
                        var result = scanner.Start(logAction);
                        logAction(result.ToJson());
                    }
                    Console.Write(sb.ToString());
                    GC.Collect();
                    Thread.Sleep(50);
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
        }

        [TestMethod]
        public void Test()
        {
            //var bmp = (Bitmap)Image.FromFile("v3/01.jpg");
            //bmp.MedianFilter();
            //bmp.Save("v3/01_m_01.jpg");

            var bmp = (Bitmap)Image.FromFile("v3/sheet_002.jpg");
            bmp = bmp.Corrosion();
            bmp.Save("v3/sheet_002a.jpg");
            bmp = bmp.Corrosion();
            bmp.Save("v3/sheet_002b.jpg");
            bmp = bmp.Corrosion();
            bmp.Save("v3/sheet_002c.jpg");
        }
    }
}
