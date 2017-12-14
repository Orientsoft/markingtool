using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Scanner;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Recognition
{
    /// <summary> 得一号识别 </summary>
    public class DCodeHelper : IDisposable
    {
        private readonly Bitmap _sourceBmp;
        private byte[,] _bmpArray;
        private const int Rows = 5;
        private const int Cols = 11;
        private readonly int _guid;
        public DCodeHelper(Bitmap bmp)
        {
            _guid = DeyiKeys.Guid;
            _sourceBmp = bmp;
            if (DeyiKeys.MarkingConfig.IsDebug)
            {
                _sourceBmp.Save(Path.Combine(DeyiKeys.MarkingPath, DeyiKeys.DebugName,
                    $"{_guid}_dcode.jpg"));
            }
        }

        public DResult<string> Decoder(int thresholdDiff = 0)
        {
            _bmpArray = new AreaRecognitionHelper(_sourceBmp, thresholdDiff).GetArea();
            var prefix = string.Concat($"{_guid}_dcode", (thresholdDiff != 0
                ? thresholdDiff > 0
                    ? $"_{thresholdDiff}"
                    : $"__{Math.Abs(thresholdDiff)}"
                : string.Empty));
            if (DeyiKeys.MarkingConfig.IsDebug)
            {
                _bmpArray.ToBitmap().Save(Path.Combine(DeyiKeys.MarkingPath, DeyiKeys.DebugName, $"{prefix}_area.jpg"));
            }
            _bmpArray = _bmpArray.Corrosion().Corrosion();
            if (DeyiKeys.MarkingConfig.IsDebug)
            {
                _bmpArray.ToBitmap().Save(Path.Combine(DeyiKeys.MarkingPath, DeyiKeys.DebugName, $"{prefix}_corrosion.jpg"));
            }
            var code = string.Empty;
            int width = _bmpArray.GetLength(1),
                height = _bmpArray.GetLength(0);

            double preWidth = width / (double)Cols,
                preHeight = height / (double)Rows;

            var errorMsg = new List<string>();

            for (var i = 0; i < Rows; i++)
            {
                var codes = Codes(i, preWidth, preHeight);
                if (codes.All(t => t < 0))
                {
                    errorMsg.Add("漏涂");
                    continue;
                }
                if (codes.Count() > 1)
                {
                    errorMsg.Add("多涂");
                }
                code += string.Join(string.Empty, codes.Select(t => t - 1));
            }
            errorMsg = errorMsg.Distinct().ToList();
            if (errorMsg.Any())
                return new DResult<string>(false, code) { Message = string.Join("、", errorMsg) + "得一号" };
            return DResult.Succ(code);

        }

        private int[] Codes(int row, double preWidth, double preHeight)
        {
            if (DeyiKeys.ScannerConfig.RecognitionType == 0)
            {
                var ranks = new Dictionary<int, double>();
                for (var j = 1; j < Cols; j++)
                {
                    int x = (int)Math.Floor(j * preWidth) + 2,
                        y = (int)Math.Floor(row * preHeight) + 2,
                        width = (int)Math.Ceiling(preWidth) - 4,
                        height = (int)Math.Ceiling(preHeight) - 4;
                    int count = 0, black = 0, w = _bmpArray.GetLength(1), h = _bmpArray.GetLength(0);
                    for (var sy = y; sy < y + height; sy++)
                    {
                        for (var sx = x; sx < x + width; sx++)
                        {
                            count++;
                            if (sy < h && sx < w && _bmpArray[sy, sx] == 0)
                                black++;
                        }
                    }

                    var rank = black / (double)count;
                    ranks.Add(j, rank);
                }
                return ranks.Answers();
            }
            var codes = new List<int>();
            for (var j = 1; j < Cols; j++)
            {
                int x = (int)Math.Floor(j * preWidth) + 2,
                    y = (int)Math.Floor(row * preHeight) + 2,
                    width = (int)Math.Ceiling(preWidth) - 4,
                    height = (int)Math.Ceiling(preHeight) - 4;
                var colArray = new List<int[]>();
                int w = _bmpArray.GetLength(1), h = _bmpArray.GetLength(0);
                for (var sy = y; sy < y + height; sy++)
                {
                    var line = new int[width];
                    for (var sx = x; sx < x + width; sx++)
                    {
                        if (sy < h && sx < w)
                            line[sx - x] = (_bmpArray[sy, sx] == 0 ? 1 : 0);
                        else
                            line[sx - x] = 0;
                    }
                    colArray.Add(line);
                }
                if (ImageHelper.IsFilled(colArray))
                    codes.Add(j);
            }
            return codes.ToArray();
        }

        public void Dispose()
        {
            _sourceBmp?.Dispose();
        }
    }
}
