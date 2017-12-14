using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Scanners.Builder;
using DayEasy.MarkingTool.UI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DayEasy.MarkingTool.Core
{
    /// <summary>
    /// 阅卷管理类
    /// </summary>
    public class ScannerBuilderManager
    {
        private readonly Logger _logger = Logger.L<ScannerBuilderManager>();

        /// <summary>
        /// 建造者
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="markingPaper"></param>
        /// <returns></returns>
        public JsonResultBase Construct(ScannerBuilder builder, MarkingPaper markingPaper)
        {
            const string logFormat = "{0},耗时：{1}s";
            double total = 0, time;
            var watch = new Stopwatch();
            var sb = new StringBuilder();
            sb.AppendLine(string.Empty);
            watch.Start();

            //预处理图片
            JsonResultBase result = builder.LoadImage();
            ChangeProcess(markingPaper, 3);
            if (!result.Status)
            {
                ChangeProcess(markingPaper, 7);
                return new JsonResultBase(result.Description);
            }

            total += time = watch.WatchLog();
            sb.AppendLine(logFormat.FormatWith("1.预处理图片", time));

            //寻找分割线
            result = builder.FindLines();
            ChangeProcess(markingPaper, 1);
            if (!result.Status)
            {
                ChangeProcess(markingPaper, 6);
                return new JsonResultBase(result.Description);
            }
            var lines = (result as JsonResults<int>).Data.ToList();

            total += time = watch.WatchLog();
            sb.AppendLine(logFormat.FormatWith("2.寻找分割线", time));

            //分割线剪切
            result = builder.CuttingStage(lines);
            ChangeProcess(markingPaper, 3);
            if (!result.Status)
            {
                ChangeProcess(markingPaper, 3);
                return new JsonResultBase(result.Description);
            }

            total += time = watch.WatchLog();
            sb.AppendLine(logFormat.FormatWith("3.分割线剪切", time));

            //识别基础信息
            result = builder.RecognitionBasicInfo();
            ChangeProcess(markingPaper, 1);
            if (!result.Status)
            {
                ChangeProcess(markingPaper, 2);
                return new JsonResultBase(result.Description);
            }
            var student = (result as JsonResult<StudentInfo>).Data;

            total += time = watch.WatchLog();
            sb.AppendLine(logFormat.FormatWith("4.识别基础信息", time));

            //答题卡识别
            result = builder.RecognitionAnswerSheet();
            ChangeProcess(markingPaper, 1);
            if (!result.Status)
            {
                ChangeProcess(markingPaper, 1);
                return new JsonResultBase(result.Description);
            }
            var answers = (result as JsonResults<bool>);

            total += time = watch.WatchLog();
            sb.AppendLine(logFormat.FormatWith("5.答题卡识别", time));

            //填充阅卷结果
            var jsonResult = builder.FillMarkingInfo(student, answers.Data.ToList());

            total += time = watch.WatchLog(false);
            sb.AppendLine(logFormat.FormatWith("6.填充阅卷结果", time));
            sb.AppendLine(logFormat.FormatWith("扫描识别", total));
            if (DeyiKeys.WriteFile)
                _logger.I(sb.ToString());

            ChangeProcess(markingPaper, 1);
            return jsonResult;
        }

        private void ChangeProcess(MarkingPaper markingPaper, int count)
        {
            markingPaper.Dispatcher.Invoke(new Action(() =>
            {
                markingPaper.PBar.Value += count;
            }));
        }
    }

    public static class WatchExtension
    {
        public static double WatchLog(this Stopwatch watch, bool restart = true)
        {
            watch.Stop();
            var time = watch.ElapsedMilliseconds/1000D;
            if (restart)
                watch.Restart();
            return time;
        }
    }
}