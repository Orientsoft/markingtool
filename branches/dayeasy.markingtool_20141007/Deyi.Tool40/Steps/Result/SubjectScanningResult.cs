
using System;
using Deyi.Tool.PaperServiceReference;


namespace Deyi.Tool.Step
{

    /// <summary>
    /// 题目扫描结果
    /// </summary>
    public class ObjectiveScanningResult : StepResult
    {
        public ObjectiveScanningResult(bool isSuccess, Exception exception, MarkingResult anwsers)
            : base(isSuccess, exception)
        {
            this.Anwsers = anwsers;
        }

        public ObjectiveScanningResult(MarkingResult anwsers)
            : base(true, null)
        {
            this.Anwsers = anwsers;
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
