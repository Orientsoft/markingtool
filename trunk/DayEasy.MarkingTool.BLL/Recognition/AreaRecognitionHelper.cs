
using DayEasy.MarkingTool.BLL.Common;
using System;
using System.Drawing;

namespace DayEasy.MarkingTool.BLL.Recognition
{
    /// <summary> 边框区域截取器 </summary>
    public class AreaRecognitionHelper : IDisposable
    {
        /// <summary> 半十字判定阀值 </summary>
        private const float Scale = 0.90F;
        private readonly Bitmap _sourceBmp;
        private int _width;
        private int _height;
        private readonly byte[,] _bmpArray;

        public AreaRecognitionHelper(Bitmap bmp, int thresholdDiff = 0)
        {
            _sourceBmp = bmp;
            _width = bmp.Width;
            _height = bmp.Height;
            _bmpArray = bmp.ToBinaryArray(diff: thresholdDiff);
        }

        /// <summary> 截取边框区域 </summary>
        public byte[,] GetArea()
        {
            PointF pointX = GetLeftTopPointF(),
                pointY = GetRightBottomPointF();
            int x = (int)(pointX.X + 1),
                y = (int)(pointX.Y + 2);
            _width = (int)(pointY.X - x - 1);
            _height = (int)(pointY.Y - y - 1);

            return _bmpArray.Area(x, y, _width, _height);
        }

        private PointF GetLeftTopPointF()
        {
            for (var x = 0; x < _width / 2; x++)
            {
                for (var y = 0; y < _height / 2; y++)
                {
                    var isBlack = ScanerBlackCount(x, y, 1);
                    if (isBlack)
                    {
                        return new PointF(x + 1, y + 1);
                    }
                }
            }
            return new PointF(0, 0);
        }

        private PointF GetRightBottomPointF()
        {
            for (var x = _width - 1; x > _width / 2; x--)
            {
                for (var y = _height; y > _height / 2; y--)
                {

                    var isBlack = ScanerBlackCount(x, y, -1);
                    if (isBlack)
                    {
                        return new PointF(x, y);
                    }
                }
            }
            return new PointF(_width, _height);
        }

        /// <summary> 十字扫描黑点 </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="step">区间值</param>
        /// <returns></returns>
        private bool ScanerBlackCount(int x, int y, int step)
        {
            int xBlack = 0, yBlack = 0;
            var xl = DeyiKeys.ScannerConfig.AreaX;
            var yl = DeyiKeys.ScannerConfig.AreaY;
            //横向黑点
            for (var bx = 0; bx < xl; bx++)
            {
                if (IsBlack(x + step * bx, y))
                    xBlack++;
            }
            //纵向黑点
            for (var by = 0; by < yl; by++)
            {
                if (IsBlack(x, y + step * by))
                    yBlack++;
            }
            if (xBlack > 0 && yBlack > 0)
            {
                return xBlack + yBlack >= (xl + yl) * Scale;
            }
            return false;
        }

        private bool IsBlack(int x, int y)
        {
            if (x < 0 || y < 0 || y >= _height || x >= _width)
                return false;
            return _bmpArray[y, x] == 0;
        }

        public void Dispose()
        {
            _sourceBmp?.Dispose();
        }
    }
}
