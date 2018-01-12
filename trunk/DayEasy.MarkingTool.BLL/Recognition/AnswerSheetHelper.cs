using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Scanner;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Recognition
{
    /// <summary> 答题卡辅助类 </summary>
    public class AnswerSheetHelper : IDisposable
    {
        private readonly Logger _logger = Logger.L<AnswerSheetHelper>();
        /// <summary> 答题卡画布 </summary>
        private readonly Bitmap _sheetBitmap;

        /// <summary> 宽度 </summary>
        private int _width;

        /// <summary> 高度 </summary>
        private int _height;

        /// <summary> 每行格子数 </summary>
        private readonly int _cols;

        private readonly int _rows;

        /// <summary> 每个格子的高度 </summary>
        private float _lineHeight;

        private float _preWidth;
        private byte[,] _bmpArray;
        private readonly AnswerSheetPtr _sheetPtr;
        private readonly List<ObjectiveItem> _objectives;
        private readonly int _guid;

        /// <summary> 构造函数 </summary>
        /// <param name="sheetBitmap"></param>
        /// <param name="objectives">客观题信息</param>
        public AnswerSheetHelper(Bitmap sheetBitmap, List<ObjectiveItem> objectives)
        {
            _guid = DeyiKeys.Guid;
            if (sheetBitmap == null)
                return;
            _objectives = objectives;
            var dicts = objectives.ToDictionary(k => k.Sort.ToString(), v => v.Count);
            var sheetType = (AnswerSheetType)DeyiKeys.ScannerConfig.SheetType;
            _sheetPtr = new AnswerSheetPtr(dicts, sheetType);
            _rows = _sheetPtr.Rows;
            _cols = AnswerSheetPtr.Colunms;
            //重置到试卷大小
            _sheetBitmap = sheetBitmap;
            if (DeyiKeys.MarkingConfig.IsDebug)
            {
                _sheetBitmap.Save(Path.Combine(DeyiKeys.MarkingPath, DeyiKeys.DebugName,
                    $"{_guid}_sheet.jpg"));
            }
        }

        private bool InitArea(int thresholdDiff = 0)
        {
            if (_rows <= 0 || _cols <= 0 || _sheetBitmap == null)
                return false;
            var prefix = string.Concat($"{_guid}_sheet", (thresholdDiff != 0
                ? thresholdDiff > 0
                    ? $"_{thresholdDiff}"
                    : $"__{Math.Abs(thresholdDiff)}"
                : string.Empty));
            _bmpArray = new AreaRecognitionHelper(_sheetBitmap, thresholdDiff).GetArea();
            if (DeyiKeys.MarkingConfig.IsDebug)
            {
                _bmpArray.ToBitmap().Save(Path.Combine(DeyiKeys.MarkingPath, DeyiKeys.DebugName,
                    $"{prefix}_area.jpg"));
            }

            _width = _bmpArray.GetLength(1);
            _height = _bmpArray.GetLength(0);
            _preWidth = _width / (float)_cols;
            _lineHeight = _height / (float)_rows;
            if (Math.Abs(_lineHeight - DeyiKeys.ScannerConfig.LineHeight) > 5 || _width < DeyiKeys.ScannerConfig.PaperWidth - 150)
            {
                return false;
            }
            _bmpArray = _bmpArray.Corrosion().Corrosion();
            if (DeyiKeys.MarkingConfig.IsDebug)
            {
                _bmpArray.ToBitmap().Save(Path.Combine(DeyiKeys.MarkingPath, DeyiKeys.DebugName, $"{prefix}_corrosion.jpg"));
            }

            return true;
        }

        /// <summary> 获取阅卷结果 </summary>
        public List<int[]> GetResult(int diff = 0)
        {
            var list = new List<int[]>();
            var check = InitArea(diff);
            foreach (var item in _objectives)
            {
                if (!check)
                {
                    list.Add(new[] { -1 });
                    continue;
                }
                var point = _sheetPtr.GetPoint(item.Sort);
                _sheetPtr.Set(point);
                _sheetPtr.Update();
                if (DeyiKeys.ScannerConfig.RecognitionType == 0)
                {
                    var ranks = new Dictionary<int, double>();
                    for (var i = 0; i < item.Count; i++)
                    {
                        ranks.Add(i, BlackRank(_sheetPtr.Row, _sheetPtr.Colunm));
                        _sheetPtr.Update();
                    }
                    var answers = ranks.Answers(item.Single);
                    //if (Convert.ToInt32(item.Sort) >= 41 && Convert.ToInt32(item.Sort) <= 45 && answers.All(t => t < 0))
                    //    answers = new[] { 4 };
                    list.Add(answers);
                }
                else
                {
                    var answers = new List<int>();
                    for (var i = 0; i < item.Count; i++)
                    {
                        if (CheckArea(point.Row, point.Colunm + i + 1))
                            answers.Add(i);
                        _sheetPtr.Update();
                    }
                    list.Add(answers.ToArray());
                }
            }
            return list;
        }

        /// <summary> 检测答题格 </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private double BlackRank(int row, int col)
        {
            int x = (int)Math.Floor(col * _preWidth) + 2,
                y = (int)Math.Floor(row * _lineHeight) + 2,
                width = (int)Math.Ceiling(_preWidth) - 4,
                height = (int)Math.Ceiling(_lineHeight) - 4;
            int count = 0, black = 0;
            for (var sy = y; sy < y + height; sy++)
            {
                for (var sx = x; sx < x + width; sx++)
                {
                    count++;
                    if (sy < _height && sx < _width && _bmpArray[sy, sx] == 0)
                        black++;
                }
            }

            return black / (double)count;
        }


        /// <summary> 检测答题格 </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private bool CheckArea(int row, int col)
        {
            int x = (int)Math.Floor(col * _preWidth),
                y = (int)Math.Floor(row * _lineHeight),
                width = (int)Math.Ceiling(_preWidth),
                height = (int)Math.Ceiling(_lineHeight);
            var colArray = new List<int[]>();
            for (var sy = y; sy < y + height; sy++)
            {
                var line = new int[width];
                for (var sx = x; sx < x + width; sx++)
                {
                    if (sy < _height && sx < _width)
                        line[sx - x] = (_bmpArray[sy, sx] == 0 ? 1 : 0);
                    else
                        line[sx - x] = 0;
                }
                colArray.Add(line);
            }
            return ImageHelper.IsFilled(colArray);
        }

        public void Dispose()
        {
            _sheetBitmap?.Dispose();
        }
    }
}