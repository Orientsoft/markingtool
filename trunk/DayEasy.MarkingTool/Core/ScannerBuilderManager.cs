using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.BLL.Scanners.Builder;
using DayEasy.MarkingTool.UI;
using DayEasy.Models.Open.Paper;
using DayEasy.Models.Open.Work;

namespace DayEasy.MarkingTool.Core
{
    /// <summary>
    /// 阅卷管理类
    /// </summary>
    public class ScannerBuilderManager
    {
        private readonly Logger _logger = Logger.L<ScannerBuilderManager>();
        private readonly byte _sectionType;
        private readonly MPaperDto _paper;
        private List<int> _sheets;

        public ScannerBuilderManager(MPaperDto paper, byte sectionType = 0)
        {
            _paper = paper;
            _sectionType = sectionType;
            GetSheets();
        }

        public byte SectionType { get { return _sectionType; } }

        /// <summary>
        /// 建造者
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="markingPaper"></param>
        /// <returns></returns>
        public DResult Construct(ScannerBuilder builder, MarkingPaper markingPaper)
        {
            const string logFormat = "{0},耗时：{1}s";
            double total = 0, time;
            var watch = new Stopwatch();
            var sb = new StringBuilder();
            sb.AppendLine(string.Empty);
            watch.Start();
            builder.Init(_sheets, _paper.PaperTitle, _sectionType);
            //预处理图片
            var result = builder.LoadImage();
            ChangeProcess(markingPaper, 3);
            if (!result.Status)
            {
                ChangeProcess(markingPaper, 7);
                return result;
            }

            total += time = watch.WatchLog();
            sb.AppendLine(logFormat.FormatWith("1.预处理图片", time));

            //寻找分割线
            result = builder.FindLines();
            ChangeProcess(markingPaper, 1);
            if (!result.Status)
            {
                ChangeProcess(markingPaper, 6);
                return result;
            }
            var lines = ((DResults<LineInfo>)result).Data.ToList();
            if (DeyiKeys.MarkingConfig.IsDebug)
                _logger.I("lines:" + lines.ToJson());

            total += time = watch.WatchLog();
            sb.AppendLine(logFormat.FormatWith("2.寻找分割线", time));

            //分割线剪切
            result = builder.CuttingStage(lines);
            ChangeProcess(markingPaper, 3);
            if (!result.Status)
            {
                ChangeProcess(markingPaper, 3);
                return result;
            }

            total += time = watch.WatchLog();
            sb.AppendLine(logFormat.FormatWith("3.分割线剪切", time));

            //识别基础信息
            result = builder.RecognitionBasicInfo();
            ChangeProcess(markingPaper, 1);
            StudentInfo student = null;
            if (result.Status)
                student = ((DResult<StudentInfo>)result).Data;

            total += time = watch.WatchLog();
            sb.AppendLine(logFormat.FormatWith("4.识别基础信息", time));

            //答题卡识别
            result = builder.RecognitionAnswerSheet();
            ChangeProcess(markingPaper, 1);
            var answers = new List<bool>();
            if (result.Status)
                answers = ((DResults<bool>)result).Data.ToList();

            total += time = watch.WatchLog();
            sb.AppendLine(logFormat.FormatWith("5.答题卡识别", time));

            //填充阅卷结果
            var dResult = builder.FillMarkingInfo(student, answers, _sectionType);

            total += time = watch.WatchLog(false);
            sb.AppendLine(logFormat.FormatWith("6.填充阅卷结果", time));
            sb.AppendLine(logFormat.FormatWith("扫描识别", total));
            if (DeyiKeys.MarkingConfig.IsDebug)
                _logger.I(sb.ToString());

            ChangeProcess(markingPaper, 1);
            return dResult;
        }

        public void ReScanner(ScannerBuilder builder, MPictureInfo picture, PaperMarkedInfo markedInfo)
        {
            //答题卡识别
            var result = builder.RecognitionAnswerSheet();
            builder.FillAnswerSheet(result.Data.ToList());
        }

        private void ChangeProcess(MarkingPaper markingPaper, int count)
        {
            markingPaper.Dispatcher.Invoke(new Action(() =>
            {
                markingPaper.PBar.Value += count;
            }));
        }

        /// <summary> 获取答题卡信息 </summary>
        /// <returns></returns>
        private void GetSheets()
        {
            if (_paper == null || _sectionType > 2)
                return;
            var type = _sectionType == 0 ? 1 : _sectionType;
            var questions = _paper.Sections.Where(t => t.PaperSectionType == type).OrderBy(t => t.Sort)
                .SelectMany(s => s.Questions.OrderBy(q => q.Sort)).Where(q => q.IsObjective).ToList();
            if (!questions.Any())
                return;
            var sheets = new List<int>();
            foreach (var dto in questions)
            {
                if (dto.Details != null && dto.Details.Any())
                {
                    //                    sheets.AddRange(dto.Details.Select(detail => 4));
                    sheets.AddRange(dto.Details.Select(detail => detail.Answers.Count));
                }
                else
                {
                    //                    sheets.Add(4);
                    sheets.Add(dto.Answers.Count);
                }
            }
            _sheets = sheets;
        }
    }

    public static class WatchExtension
    {
        public static double WatchLog(this Stopwatch watch, bool restart = true)
        {
            watch.Stop();
            var time = watch.ElapsedMilliseconds / 1000D;
            if (restart)
                watch.Restart();
            return time;
        }
    }
}