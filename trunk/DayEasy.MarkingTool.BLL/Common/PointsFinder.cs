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
        public bool IsPaperB { get; set; }
        public List<Rectangle> RectList { get; set; }
    }

    public class PointsFinder
    {
        public PointsResult ParseResult(List<Rectangle> recList)
        {
            var pr = new PointsResult();
            pr.RectList = recList;
            pr.PointsCount = recList.Count;

            if(recList.Count == 5)
            {
                // It is paper B
                pr.IsPaperB = true;

                var maxY = recList.Max(r => r.Y);
                var minY = recList.Min(r => r.Y);

                // Find the start point of paper B
                pr.PaperBPoint = recList.Where(
                    r => r.Y > minY
                    && r.Y < maxY
                    && Math.Abs(r.Y - minY) > 10
                    && Math.Abs(r.Y - maxY) > 10).FirstOrDefault();
            }
            else
            {
                pr.IsPaperB = false;
            }

            return pr;
        }

        public PointsResult Find(System.Drawing.Image bmp)
        {
            // locating objects
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 13;
            blobCounter.MinWidth = 13;
            blobCounter.MaxHeight = 20;
            blobCounter.MaxWidth = 20;
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
                    if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Square)
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
