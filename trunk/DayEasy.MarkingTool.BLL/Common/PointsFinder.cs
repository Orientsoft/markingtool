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
            return HorizonPoints[0].X + HorizonPoints[0].Width / 2
                   + (HorizonPoints[1].X +HorizonPoints[1].Width / 2
                   - (HorizonPoints[0].X + HorizonPoints[0].Width / 2)) / 2;
        }
    }
     
    public class PointsFinder
    {
        public PointsResult ParseResult(List<Rectangle> recList)
        {
            var pr = new PointsResult();
            var maxX = recList.Max(r => r.X);
            var minX = recList.Min(r => r.X);

            // Remove the wrong points
            for (int i = recList.Count - 1; i >= 0; i--)
            {
                if(Math.Abs(recList[i].X - minX) > 20 && Math.Abs(recList[i].X - maxX) > 20 && recList[i].X < maxX)
                {
                    recList.RemoveAt(i);
                } 
            }

            pr.RectList = recList;
            pr.PointsCount = recList.Count;

            if (recList.Count == 5)
            {
                // It is paper B
                pr.HasPaperBPoint = true;

                // Find the start point of paper B
                var horizonPointsMax = recList.OrderByDescending(r => r.Y).Take(2).ToList<Rectangle>();
                pr.HorizonPoints = recList.OrderBy(r => r.Y).Take(2).ToList<Rectangle>();
                var unionPoints = horizonPointsMax.Union(pr.HorizonPoints);

                foreach(var point in recList)
                {
                    if (!unionPoints.Contains(point))
                    {
                        pr.PaperBPoint = point;
                        break;
                    }
                }
            }
            else
            {
                pr.HasPaperBPoint = false;
                pr.HorizonPoints = recList.OrderBy(r => r.Y).Take(2).ToList<Rectangle>();
            }

            return pr;
        }

        public PointsResult Find(System.Drawing.Image bmp)
        {
            // locating objects
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 14;
            blobCounter.MinWidth = 14;
            blobCounter.MaxHeight = 22;
            blobCounter.MaxWidth = 22;
            var rawImg = (Bitmap)bmp;

            var grayImg = AForge.Imaging.Filters.Grayscale.CommonAlgorithms.BT709.Apply(rawImg);
            var bwImg = new AForge.Imaging.Filters.OtsuThreshold().Apply(grayImg);
            var openingFilter = new AForge.Imaging.Filters.Opening();
            openingFilter.ApplyInPlace(bwImg);
            var bwInvImg = new AForge.Imaging.Filters.Invert().Apply(bwImg);
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
                if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints)) 
                {
                    // only do things if the corners form a square
                    if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Square || 
                        shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Rectangle)
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
                }
            }

            return ParseResult(locPoints);
        }
    }
}
