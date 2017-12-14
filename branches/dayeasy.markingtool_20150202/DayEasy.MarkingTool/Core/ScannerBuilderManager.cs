using System;
using System.Linq;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Scanners.Builder;
using DayEasy.MarkingTool.UI;

namespace DayEasy.MarkingTool.Core
{
    /// <summary>
    /// 阅卷管理类
    /// </summary>
    public class ScannerBuilderManager
    {
        /// <summary>
        /// 建造者
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="markingPaper"></param>
        /// <returns></returns>
        public JsonResultBase Construct(ScannerBuilder builder, MarkingPaper markingPaper)
        {
            //预处理图片
            JsonResultBase result = builder.LoadImage();
            ChangeProcess(markingPaper, 3);
            if (!result.Status) return new JsonResultBase(result.Description);

            //寻找分割线
            result = builder.FindLines();
            ChangeProcess(markingPaper, 1);
            if (!result.Status) return new JsonResultBase(result.Description);
            var lines = (result as JsonResults<int>).Data.ToList();

            //分割线剪切
            result = builder.CuttingStage(lines);
            ChangeProcess(markingPaper, 1);
            if (!result.Status) return new JsonResultBase(result.Description);

            //识别基础信息
            result = builder.RecognitionBasicInfo();
            ChangeProcess(markingPaper, 1);
            if (!result.Status) return new JsonResultBase(result.Description);
            var student = (result as JsonResult<StudentInfo>).Data;

            //答题卡识别
            result = builder.RecognitionAnswerSheet();
            ChangeProcess(markingPaper, 2);
            if (!result.Status) return new JsonResultBase(result.Description);
            var answers = (result as JsonResults<bool>);

            //填充阅卷结果
            return builder.FillMarkingInfo(student, answers.Data.ToList());
        }

        private void ChangeProcess(MarkingPaper markingPaper, int count)
        {
            markingPaper.Dispatcher.Invoke(new Action(() =>
            {
                markingPaper.PBar.Value += count;
            }));
        }
    }
}