﻿using System;
using System.Drawing;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary> 图像纠偏辅助类 </summary>
    public class Deskew
    {
        private readonly Logger _logger = Logger.L<Deskew>();

        private class HougLine
        {
            public int Count;
            public int Index;
            public double Alpha;
        }

        public Deskew(Bitmap internalBmp, double threshold = 140)
        {
            _internalBmp = internalBmp;
            _threshold = threshold;
        }

        private readonly Bitmap _internalBmp;
        private readonly double _threshold;
        const double AlphaStart = -20;
        const double AlphaStep = 0.2;
        const int Steps = 40 * 5;
        const double Step = 1;
        double[] _sinA;
        double[] _cosA;
        // Range of d
        double _min;

        int _count;
        // Count of points that fit in a line.
        int[] _hMatrix;
        // Calculate the skew angle of the image cBmp.
        public double GetSkewAngle()
        {
            try
            {
                // Hough Transformation
                Calc();
                // Top 20 of the detected lines in the image.
                HougLine[] hl = GetTop(20);
                // Average angle of the lines
                double sum = 0;
                int count = 0;
                for (int i = 0; i <= 19; i++)
                {
                    sum += hl[i].Alpha;
                    count += 1;
                }
                return sum / count;
            }
            catch
            {
                return 0;
            }
        }

        // Calculate the Count lines in the image with most points.
        private HougLine[] GetTop(int count)
        {
            var hl = new HougLine[count];
            for (int i = 0; i <= count - 1; i++)
            {
                hl[i] = new HougLine();
            }
            for (int i = 0; i <= _hMatrix.Length - 1; i++)
            {
                if (_hMatrix[i] > hl[count - 1].Count)
                {
                    hl[count - 1].Count = _hMatrix[i];
                    hl[count - 1].Index = i;
                    int j = count - 1;
                    while (j > 0 && hl[j].Count > hl[j - 1].Count)
                    {
                        HougLine tmp = hl[j];
                        hl[j] = hl[j - 1];
                        hl[j - 1] = tmp;
                        j -= 1;
                    }
                }
            }
            for (int i = 0; i <= count - 1; i++)
            {
                int dIndex = hl[i].Index / Steps;
                int alphaIndex = hl[i].Index - dIndex * Steps;
                hl[i].Alpha = GetAlpha(alphaIndex);
                //hl[i].D = dIndex + _min;
            }
            return hl;
        }

        // Hough Transforamtion:
        private void Calc()
        {
            int hMin = _internalBmp.Height / 4;
            int hMax = _internalBmp.Height * 3 / 4;
            Init();
            for (int y = hMin; y <= hMax; y++)
            {
                for (int x = 1; x <= _internalBmp.Width - 2; x++)
                {
                    // Only lower edges are considered.
                    if (IsBlack(x, y))
                    {
                        if (!IsBlack(x, y + 1))
                        {
                            Calc(x, y);
                        }
                    }
                }
            }
        }
        // Calculate all lines through the point (x,y).
        private void Calc(int x, int y)
        {
            int alpha;
            for (alpha = 0; alpha <= Steps - 1; alpha++)
            {
                double d = y * _cosA[alpha] - x * _sinA[alpha];
                var calculatedIndex = (int)CalcDIndex(d);
                int index = calculatedIndex * Steps + alpha;
                try
                {
                    _hMatrix[index] += 1;
                }
                catch (Exception ex)
                {
                    _logger.E("扫描出错", ex);
                }
            }
        }
        private double CalcDIndex(double d)
        {
            return Convert.ToInt32(d - _min);
        }

        private bool IsBlack(int x, int y)
        {
            Color c = _internalBmp.GetPixel(x, y);
            double luminance = (c.R * 0.299) + (c.G * 0.587) + (c.B * 0.114);
            return luminance < _threshold;
        }

        private void Init()
        {
            _cosA = new double[Steps];
            _sinA = new double[Steps];
            for (int i = 0; i < Steps; i++)
            {
                double angle = GetAlpha(i) * Math.PI / 180.0;
                _sinA[i] = Math.Sin(angle);
                _cosA[i] = Math.Cos(angle);
            }
            _min = -_internalBmp.Width;
            _count = (int)(2 * (_internalBmp.Width + _internalBmp.Height) / Step);
            _hMatrix = new int[_count * Steps];
        }
        private static double GetAlpha(int index)
        {
            return AlphaStart + index * AlphaStep;
        }
    }
}