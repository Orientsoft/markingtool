using DayEasy.Open.Model.Marking;
using System;

namespace DayEasy.MarkingTool.BLL.Steps.Result
{

    /// <summary>
    /// 题目扫描结果
    /// </summary>
    public class ObjectiveScanningResult : StepResult
    {
        public ObjectiveScanningResult(bool isSuccess, Exception exception, MarkingResult anwsers)
            : base(isSuccess, exception)
        {
            Anwsers = anwsers;
        }

        public ObjectiveScanningResult(MarkingResult anwsers)
            : base(true, null)
        {
            Anwsers = anwsers;
        }

        /// <summary>
        /// 学生答案
        /// </summary>
        public MarkingResult Anwsers
        {
            get;
            set;
        }
    }

}
