using AForge;
using AForge.Imaging;
using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Common
{
    public class PointsResult
    {
        public int PointsCount { get; set; }
        public Rectangle PaperBPoint { get; set; }
        public bool HasPaperBPoint { get; set; }
        public List<Rectangle> RectList { get; set; }
        public List<Rectangle> HorizonPoints { get; set; }
        public int GetCenterX()
        {
            if (HorizonPoints == null | HorizonPoints.Count < 2)
                return 0;
            return HorizonPoints[0].X + HorizonPoints[0].Width / 2
                   + (HorizonPoints[1].X + HorizonPoints[1].Width / 2
                   - (HorizonPoints[0].X + HorizonPoints[0].Width / 2)) / 2;
        }

        private bool IsValidPoint(Rectangle rect)
        {
            var x = rect.X;
            var y = rect.Y;
            //左边定位点
            if ((x >= 0 && x < 120) || x > 1350)
                return true;
            return false;
        }

        private void FilterX()
        {
            var orders = RectList.OrderBy(t => t.X).ToList();
            int v = 0, c = 0;
            foreach (var i in orders)
            {
                if (c > 0)
                    v = i.X - c;
                if (v >= 12 && v < 700)
                    RectList.Remove(i);
                c = i.X;
            }
            if (RectList.Count < 5)
            {
                //4个基础点
                var rectA = orders.FirstOrDefault(t => t.X < 100 && t.Y < 100);
                var rectB = orders.FirstOrDefault(t => t.X > 1350 && t.Y < 100);
                var rectC = orders.FirstOrDefault(t => t.X < 100 && t.Y > 960);
                var rectD = orders.FirstOrDefault(t => t.X > 1350 && t.Y > 960);
                //补全定位点
                if (rectA.IsEmpty && !rectB.IsEmpty && !rectC.IsEmpty)
                    RectList.Add(new Rectangle(rectC.X, rectB.Y, rectC.Width, rectB.Height));
                if (rectB.IsEmpty && !rectA.IsEmpty && !rectD.IsEmpty)
                    RectList.Add(new Rectangle(rectD.X, rectA.Y, rectD.Width, rectA.Height));
                if (rectC.IsEmpty && !rectA.IsEmpty && !rectD.IsEmpty)
                    RectList.Add(new Rectangle(rectA.X, rectD.Y, rectA.Width, rectD.Height));
                if (rectD.IsEmpty && !rectB.IsEmpty && !rectC.IsEmpty)
                    RectList.Add(new Rectangle(rectB.X, rectC.Y, rectB.Width, rectC.Height));
            }


        }


        public void FilterPoints(List<Rectangle> rectList)
        {
            if (rectList == null || !rectList.Any())
                return;
            RectList = rectList.Where(t => IsValidPoint(t)).ToList();
            FilterX();

            //组合过滤
            PointsCount = RectList.Count();

            if (RectList.Count == 5)
            {
                // It is paper B
                HasPaperBPoint = true;

                // Find the start point of paper B
                var horizonPointsMax = RectList.OrderByDescending(r => r.Y).Take(2).ToList();
                HorizonPoints = RectList.OrderBy(r => r.Y).Take(2).ToList();
                var unionPoints = horizonPointsMax.Union(HorizonPoints);

                foreach (var point in RectList)
                {
                    if (!unionPoints.Contains(point))
                    {
                        PaperBPoint = point;
                        break;
                    }
                }
            }
            else
            {
                HasPaperBPoint = false;
                HorizonPoints = RectList.OrderBy(r => r.Y).Take(2).ToList();
            }
        }
    }

    public class PointsFinder
    {
        public PointsResult ParseResult(List<Rectangle> recList)
        {
            var pr = new PointsResult();

            if (recList.Count < 4)
            {
                pr.PointsCount = 0;
                return pr;
            }

            var maxX = recList.Max(r => r.X);
            var minX = recList.Min(r => r.X);

            // Remove the wrong points
            for (int i = recList.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(recList[i].X - minX) >= 20 && Math.Abs(recList[i].X - maxX) >= 20 && recList[i].X < maxX)
                {
                    recList.RemoveAt(i);
                }
            }

            //pr.RectList = recList;
            //pr.PointsCount = recList.Count;

            //if (recList.Count == 5)
            //{
            //    // It is paper B
            //    pr.HasPaperBPoint = true;

            //    // Find the start point of paper B
            //    var horizonPointsMax = recList.OrderByDescending(r => r.Y).Take(2).ToList<Rectangle>();
            //    pr.HorizonPoints = recList.OrderBy(r => r.Y).Take(2).ToList<Rectangle>();
            //    var unionPoints = horizonPointsMax.Union(pr.HorizonPoints);

            //    foreach (var point in recList)
            //    {
            //        if (!unionPoints.Contains(point))
            //        {
            //            pr.PaperBPoint = point;
            //            break;
            //        }
            //    }
            //}
            //else
            //{
            //    pr.HasPaperBPoint = false;
            //    pr.HorizonPoints = recList.OrderBy(r => r.Y).Take(2).ToList<Rectangle>();
            //}
            pr.FilterPoints(recList);

            return pr;
        }

        public PointsResult Find(System.Drawing.Image bmp)
        {
            // locating objects
            BlobCounter blobCounter = new BlobCounter
            {
                FilterBlobs = true,
                MinHeight = 14,
                MinWidth = 14,
                MaxHeight = 22,
                MaxWidth = 22
            };
            var rawImg = (Bitmap)bmp;

            var grayImg = AForge.Imaging.Filters.Grayscale.CommonAlgorithms.BT709.Apply(rawImg);
            var bwImg = new AForge.Imaging.Filters.OtsuThreshold().Apply(grayImg);
            //bwImg.Save($"d:/bw_{Guid.NewGuid().ToString("N")}.png");
            var openingFilter = new AForge.Imaging.Filters.Opening();
            openingFilter.ApplyInPlace(bwImg);
            var bwInvImg = new AForge.Imaging.Filters.Invert().Apply(bwImg);
            //bwInvImg.Save($"d:/inv_{Guid.NewGuid().ToString("N")}.png");
            bwImg.Dispose();
            grayImg.Dispose();

            blobCounter.ProcessImage(bwInvImg);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            // check for rectangles
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            List<Rectangle> locPoints = new List<Rectangle>();

            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blob);
                List<IntPoint> cornerPoints;

                // use the shape checker to extract the corner points
                //if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints))
                //{
                cornerPoints = PointsCloud.FindQuadrilateralCorners(edgePoints);
                // only do things if the corners form a square
                if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Square ||
                    shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Rectangle ||
                    shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Trapezoid)
                {
                    var rect = new Rectangle(cornerPoints[0].X,
                            cornerPoints[0].Y,
                            Math.Abs(cornerPoints[2].X - cornerPoints[0].X),
                            Math.Abs(cornerPoints[2].Y - cornerPoints[0].Y));

                    locPoints.Add(rect);

                    // For debug and output test image.
                    //List<System.Drawing.Point> Points = new List<System.Drawing.Point>();
                    //foreach (var point in cornerPoints)
                    //{
                    //    Points.Add(new System.Drawing.Point(point.X, point.Y));
                    //}

                    //Graphics g = Graphics.FromImage(rawImg);
                    //g.DrawPolygon(new Pen(Color.Red, 2.0f), Points.ToArray());
                    //rawImg.Save("d:\\result.png");
                }
                //}
            }

            return ParseResult(locPoints);
        }
    }
}
