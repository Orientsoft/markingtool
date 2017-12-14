
using AForge.Imaging.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Drawing.Imaging;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class AForgeTest
    {
        [TestMethod]
        public void ImageTest()
        {
            var bmp = Image.FromFile("vcode.jpg");
            //转化图片像素格式
            var b = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawImage(bmp, 0, 0);
            }
            //灰度化
            b = new Grayscale(0.2125, 0.7154, 0.0721).Apply(b);
            //二值化
            b = new Threshold(145).Apply(b);
            //去噪点
            b = new BlobsFiltering(3, 3, b.Width, b.Height).Apply(b);
            
            b.Save("vcode-01.jpg");
        }
    }
}
