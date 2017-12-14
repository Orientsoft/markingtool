using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Color = System.Drawing.Color;

namespace Deyi.Tool.Common
{
    /// <summary>
    /// 答题卡辅助类
    /// </summary>
    public class AnswerSheetHelper : IDisposable
    {
        /// <summary>
        /// 答题卡画布
        /// </summary>
        private Bitmap _sheetBitmap;

        private readonly Bitmap _smallChose;

        /// <summary>
        /// 宽度
        /// </summary>
        private int _width = 0;

        /// <summary>
        /// 高度
        /// </summary>
        private int _height;

        /// <summary>
        /// 每行格子数
        /// </summary>
        private readonly int _lineCount;

        /// <summary>
        /// 每个格子的高度
        /// </summary>
        private readonly int _lineHeight;

        private float _preWidth;

        /// <summary>
        /// 半十字判定阀值
        /// </summary>
        private const int Threshold = 10;
        private const float Scale = 1.6F;

        /// <summary>
        /// 答题区域阀值
        /// </summary>
        private const int AnswerThreshold = 120;

        /// <summary>
        /// 横向内间距
        /// </summary>
        private const byte PaddingX = 2;

        /// <summary>
        /// 纵向内间距
        /// </summary>
        private const byte PaddingY = 2;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sheetBitmap"></param>
        /// <param name="lineHeight"></param>
        /// <param name="lineCount"></param>
        public AnswerSheetHelper(Bitmap sheetBitmap, int lineHeight, int lineCount)
        {
            _sheetBitmap = sheetBitmap;
            if (sheetBitmap != null)
            {
                _width = _sheetBitmap.Width;
                _height = _sheetBitmap.Height;
                _sheetBitmap = ImageHelper.BinarizeImage(_sheetBitmap, DeyiKeys.AnswerThreshold);
            }
            _smallChose = new Bitmap(DeyiKeys.SmearWidth, DeyiKeys.SmearHeight);
            using (var g = Graphics.FromImage(_smallChose))
            {
                g.FillRectangle(Brushes.Black, 0, 0, DeyiKeys.SmearWidth, DeyiKeys.SmearHeight);
            }
            _lineHeight = lineHeight;
            _lineCount = lineCount;
        }

        /// <summary>
        /// 获取阅卷结果
        /// </summary>
        /// <returns></returns>
        public List<bool> GetResult()
        {
            var list = new List<bool>();
            if (_lineCount <= 0 || _lineHeight <= 0 || _sheetBitmap == null) return list;
            CutAnswerArea();
            _preWidth = (_width - PaddingX*2)/(float) _lineCount - 1.1F;
            var rowsCount = (int) Math.Round(_height/(decimal) _lineHeight);
            for (var i = 0; i < rowsCount; i++)
            {
                for (var j = 0; j < _lineCount; j++)
                {
                    list.Add(CheckArea(i, j));
                }
            }
            return list;
        }

        /// <summary>
        /// 检测答题格
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private bool CheckArea(int row, int col)
        {
            int x = (int) Math.Floor(col*_preWidth) + col + 1,
                y = row*_lineHeight + 1,
                width = (int) Math.Round(_preWidth) - 2,
                height = _lineHeight - 2;

            using (var bmap = ImageHelper.MakeImage(_sheetBitmap, x, y, width, height))
            {
                if (DeyiKeys.WriteFile)
                    bmap.Save(Path.Combine(DeyiKeys.PicturePath, string.Format("answer_{0}_{1}.png", row + 1, col + 1)));
                var point = ImageHelper.ImageContains(new Bitmap(bmap), _smallChose, AnswerThreshold);
                return point.X >= 0 && point.Y >= 0;
            }
        }

        /// <summary>
        /// 截取出答题格区域,并出去边框
        /// </summary>
        private void CutAnswerArea()
        {
            PointF pointX = GetLeftTopPointF(),
                pointY = GetRightBottomPointF();
            int x = (int) (pointX.X + PaddingX),
                y = (int) (pointX.Y + PaddingY);
            _width = (int) (pointY.X - pointX.X - PaddingX*2);
            _height = (int) (pointY.Y - pointX.Y - PaddingY*2) + 1;

            _sheetBitmap = (Bitmap) ImageHelper.MakeImage(_sheetBitmap, x, y, _width, _height);
            if (DeyiKeys.WriteFile)
                _sheetBitmap.Save(Path.Combine(DeyiKeys.PicturePath, "answer.png"));
        }

        private PointF GetLeftTopPointF()
        {
            for (var x = 0; x < _width - Threshold; x++)
            {
                for (var y = 0; y < _height - Threshold; y++)
                {
                    var count = ScanerBlackCount(x, y, Threshold);
                    if (count > Threshold*Scale)
                    {
                        return new PointF(x, y);
                    }
                }
            }
            return new PointF(0, 0);
        }

        private PointF GetRightBottomPointF()
        {
            for (var y = _height - 1; y > Threshold; y--)
            {
                for (var x = _width; x > Threshold; x--)
                {
                    var count = ScanerBlackCount(x, y, -Threshold);
                    if (count > Threshold*Scale)
                    {
                        return new PointF(x, y);
                    }
                }
            }
            return new PointF(_width, _height);
        }

        /// <summary>
        /// 十字扫描黑点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="threshold">区间值</param>
        /// <returns></returns>
        private int ScanerBlackCount(int x, int y, int threshold)
        {
            var colors = new List<Color>();
            int w = (threshold >= 0 ? 1 : -1);
            for (int i = 0; i <= Math.Abs(threshold); i++)
            {
                colors.Add(GetPixel(x + w*i, y));
                colors.Add(GetPixel(x, y + w*i));
            }
            return colors.Count(c => (c.R + c.G + c.B == 0));
        }

        private Color GetPixel(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height) return Color.White;
            return _sheetBitmap.GetPixel(x, y);
        }

        public void Dispose()
        {
            if (_sheetBitmap != null)
                _sheetBitmap.Dispose();
            if (_smallChose != null)
                _smallChose.Dispose();
        }
    }
}
