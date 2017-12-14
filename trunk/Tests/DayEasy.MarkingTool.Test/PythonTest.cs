using System;
using System.Diagnostics;
using DayEasy.MarkingTool.BLL.Common;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class PythonTest
    {
        [TestMethod]
        public void Main()
        {
            var runTime = Python.CreateRuntime();
            dynamic obj = runTime.UseFile("hello.py");
            Console.WriteLine(obj.hello("shay"));
            Console.WriteLine(obj.add(23, 45));

            //Python.CreateEngine().CreateModule("cv2", "cv2.pyd");
            //dynamic sheets = runTime.UseFile("sheets.py");
            //var rect = sheets.main();
            //Console.WriteLine(JsonHelper.ToJson(rect));
        }

        [TestMethod]
        public void SheetTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            var orgImg = SheetHelper.FindRect("sheet/62ae930001.jpg");
            watch.Stop();
            Console.WriteLine($"总耗时：{watch.ElapsedMilliseconds} ms");
            SheetHelper.ShowImage(orgImg);
            //var d = Cv2.Canny(orgImg)
        }
    }
}
