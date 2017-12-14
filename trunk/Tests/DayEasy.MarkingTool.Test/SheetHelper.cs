using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Util;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace DayEasy.MarkingTool.Test
{
    public class SheetHelper
    {
        public static Mat FindRect(string path, int minWidth = 260)
        {
            var org = Cv2.ImRead(path);
            //缩放
            org = org.Resize(new Size(780, org.Height * 780F / org.Width));

            var source = org.ToBitmap();
            var lineFinder = new LineFinder(source);
            var lines = lineFinder.Find(300, 15, skip: 100);
            var bmp = ImageHelper.RotateImage(source, 0 - (float)lines.Average(t => t.Angle));
            org = bmp.ToMat();
            var dst = new Mat();
            Cv2.CvtColor(org, dst, ColorConversionCodes.BGR2GRAY);

            Cv2.Canny(org, dst, 140, 300, 3, true);
            //return dst;

            Point[][] contours;
            HierarchyIndex[] hi;
            Cv2.FindContours(dst, out contours, out hi, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            //Console.WriteLine(contours.Length);
            //Cv2.DrawContours(org, contours, -1, Scalar.FromRgb(0, 0, 255));
            //return org;

            var rects = new List<Rect>();
            foreach (var points in contours)
            {
                var rect = Cv2.BoundingRect(points);
                if (rect.Y < 15 || rect.Width < minWidth)
                    continue;
                //var approx = Cv2.ApproxPolyDP(points, Math.Max(rect.Width, rect.Height), true);
                //rect = Cv2.BoundingRect(approx);
                //if (rect.Width > minWidth)
                rects.Add(rect);
            }
            Console.WriteLine(rects.Count);
            //var random = new Random();
            //var colors = new[]
            //{
            //    Scalar.FromRgb(0,0,255),
            //    Scalar.FromRgb(0,255,0),
            //    Scalar.FromRgb(255,0,0)
            //};
            //foreach (var rect in rects)
            //{
            //    Cv2.Rectangle(org, rect, colors[random.Next(0, colors.Length)]);
            //}
            //return org;
            var sheetRect = rects[rects.Count - 2];
            org = org[sheetRect];
            var dest = org.Threshold(180, 255, ThresholdTypes.BinaryInv);
            return dest;
        }

        public List<int[]> Sheets(Mat sheetImage, int row, int col = 24)
        {
            if (sheetImage == null || row <= 0 || col <= 0)
                return null;
            var lines = (int)Math.Round(sheetImage.Height / (float)row);
            var px = sheetImage.Width / (float)col;
            var py = sheetImage.Height / (float)lines;
            Console.WriteLine($"lines:{lines},px:{px},py:{py}");
            for (var j = 0; j < lines; j++)
            {
                for (var i = 0; i < 24; i++)
                {
                    var x = (int)Math.Round(i * px);
                    var y = (int)Math.Round(j * py);
                    var w = (int)Math.Ceiling(px);
                    var h = (int)Math.Ceiling(py);
                    if (x + w > sheetImage.Width)
                    {
                        w = sheetImage.Width - x;
                    }
                    if (y + h > sheetImage.Height)
                    {
                        h = sheetImage.Height - y;
                    }
                    var cellRect = new Rect(x, y, w, h);
                    var cell = sheetImage[cellRect];
                    cell.SaveImage($"sheet/cols/{j}_{i}.jpg");
                    //return cell;
                    //Console.WriteLine(cellRect);
                    //var sheet = dest[cellRect];
                    //var count = sheet.CountNonZero();
                    //Console.WriteLine($"{i}:{j}->{count}");
                }
            }
            return null;
        }

        public static void ShowImage(Mat image, string title = "picture")
        {
            Cv2.ImShow(title, image);
            Cv2.ImWrite($"sheet/{title}.jpg", image);
            Cv2.WaitKey();
        }
    }
}
