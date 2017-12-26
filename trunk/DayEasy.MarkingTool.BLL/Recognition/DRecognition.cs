using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Scanner;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Recognition
{
    public abstract class DRecognition : IRecognition, IDisposable
    {
        protected Bitmap SourceBmp { get; set; }
        protected List<LineInfo> Lines { get; set; }
        protected List<ObjectiveItem> Objectives { get; }
        protected string ImagePath;

        protected bool IgnoreCode { get; set; }

        protected virtual int[] ThresholdDiffs => new[] { 10, -10, 20, -20 };

        protected DRecognition(string imagePath, List<ObjectiveItem> objectives, bool ignoreCode)
        {
            ImagePath = imagePath;
            if (File.Exists(ImagePath))
                SourceBmp = (Bitmap)Image.FromFile(imagePath);
            Objectives = objectives;
            IgnoreCode = ignoreCode;
        }

        /// <summary> 压缩 </summary>
        protected virtual void Resize()
        {
            if (SourceBmp.Width <= DeyiKeys.ScannerConfig.PaperWidth)
                return;
            SourceBmp = (Bitmap)ImageHelper.Resize(SourceBmp, DeyiKeys.ScannerConfig.PaperWidth);
        }

        /// <summary> 二值化 </summary>
        protected virtual void Binary()
        {
            SourceBmp = SourceBmp.ToBinaryBitmap();
        }

        /// <summary> 找线 </summary>
        protected virtual void FindLines()
        {
            using (var finder = new LineFinder(SourceBmp))
            {
                var width = (int)Math.Ceiling(SourceBmp.Width * DeyiKeys.ScannerConfig.BlackScale);
                Lines = finder.Find(width, DeyiKeys.ScannerConfig.LineHeight, 2, 100);
            }
        }

        /// <summary> 纠偏 </summary>
        protected virtual void Rotate()
        {
            var angle = Lines.Average(t => t.Angle);
            if (Math.Abs(angle) < 0.001 || Lines.Max(t => t.Move) <= 2)
                return;
            SourceBmp = ImageHelper.RotateImage(SourceBmp, -(float)angle);
        }

        /// <summary> 识别得一号 </summary>
        protected virtual DResult<string> Dcode()
        {
            if (Lines == null || !Lines.Any())
                return DResult.Error<string>("识别异常");
            const int skip = 5;
            const int maxHeight = 180;
            var line = Lines.First();
            var height = line.StartY + line.Move - skip;
            var y = skip;
            if (height > maxHeight)
            {
                y += height - maxHeight;
                height = maxHeight;
            }
            var codeBmp = (Bitmap)ImageHelper.MakeImage((Bitmap)SourceBmp.Clone(), SourceBmp.Width / 2, y, SourceBmp.Width / 2, height);
            using (codeBmp = ImageHelper.RotateImage(codeBmp, -(float)line.Angle))
            {
                using (var helper = new DCodeHelper(codeBmp))
                {
                    var result = helper.Decoder();
                    //二次处理
                    if (result.Status)
                        return result;
                    foreach (var diff in ThresholdDiffs)
                    {
                        var item = helper.Decoder(diff);
                        if (!item.Status)
                            continue;
                        result = item;
                        break;
                    }
                    return result;
                }
            }
        }

        /// <summary> 识别答题卡 </summary>
        protected virtual List<int[]> Sheets()
        {
            if (Objectives == null || !Objectives.Any() || Lines == null || Lines.Count < 2)
                return new List<int[]>();
            var y = Lines[0].StartY;
            var height = Lines[1].StartY - Lines[0].StartY + 4;
            if (Lines[0].Move < 0)
            {
                var move = (int)Math.Floor(Lines[0].Move / 2F);
                y += move;
                height -= move;
            }
            else if (Lines[0].Move > 0)
            {
                var move = (int)Math.Floor(Lines[0].Move / 2F);
                height += move;
            }
            var sheetBmp =
                (Bitmap)ImageHelper.MakeImage((Bitmap)SourceBmp.Clone(), 0, y - 10, SourceBmp.Width, height + 20);
            List<int[]> sheets;
            using (sheetBmp = ImageHelper.RotateImage(sheetBmp, -(float)Lines.Average(t => t.Angle)))
            {
                using (var helper = new AnswerSheetHelper(sheetBmp, Objectives))
                {
                    sheets = helper.GetResult();
                    var errorCount = Objectives.ExceptionCount(sheets);
                    //二次识别
                    if (errorCount <= 0)
                        return sheets;
                    foreach (var diff in ThresholdDiffs)
                    {
                        var list = helper.GetResult(diff);
                        var count = Objectives.ExceptionCount(list);
                        if (count >= errorCount)
                            continue;
                        sheets = list;
                        errorCount = count;
                        if (count == 0)
                            break;
                    }
                }
            }
            return sheets;
        }

        public virtual RecognitionResult Start(Action<string> logAction = null)
        {
            var result = new RecognitionResult();
            if (SourceBmp == null)
                return result;
            WatchAction($"{ImagePath},总共", () =>
            {
                //WatchAction("压缩试卷", Resize, logAction);
                //WatchAction("二值化", () =>
                //{
                //    Binary();
                //    //SourceBmp.Save("v3/binary01.jpg");
                //}, logAction);
                WatchAction("查找横线", () =>
                {
                    FindLines();
                    logAction?.Invoke(Lines.ToJson());
                }, logAction);
                //WatchAction("纠偏", Rotate, logAction);
                WatchAction("得一号", () =>
                {
                    if(!IgnoreCode)
                    {
                        var codeResult = Dcode();
                        result.Student = new StudentInfo
                        {
                            Code = codeResult.Data
                        };
                        if (codeResult.Status)
                            return;
                        result.Status = false;
                        result.Student.Name = codeResult.Message;
                    }
                }, logAction);
                WatchAction("答题卡", () =>
                {
                    result.Sheets = Sheets();
                    if (result.Status)
                    {
                        result.Status = !result.Sheets.Any(s => s.All(t => t < 0));
                    }
                }, logAction);
            }, logAction);
            WatchAction("学生信息", () =>
            {
                if (!IgnoreCode)
                {
                    LoadStudent(result);
                }
            }, logAction);
            return result;
        }

        private static void LoadStudent(RecognitionResult result)
        {
            if (result == null) return;
            if (result.Student == null)
                result.Student = new StudentInfo();
            var student = result.Student;
            var code = student.Code;
            if (string.IsNullOrWhiteSpace(code) || code.Length != 5)
            {
                return;
            }
            var dto = DeyiApp.Student(code);
            if (!dto.Status)
            {
                student.Name = dto.Message;
                return;
            }
            if (dto.Data.ClassList.Count > 1)
            {
                student.Name = "学生有多个班级";
                return;
            }
            student.Id = dto.Data.Id;
            student.Name = dto.Data.Name;
            student.ClassId = dto.Data.ClassList.First().Key;
        }

        public void Dispose()
        {
            SourceBmp?.Dispose();
            GC.Collect();
        }

        protected static void WatchAction(string name, Action action, Action<string> logAction = null)
        {
            var watcher = new Stopwatch();
            watcher.Start();
            action();
            watcher.Stop();
            logAction?.Invoke($"{name},耗时：{watcher.ElapsedMilliseconds}ms");
        }
    }
}
