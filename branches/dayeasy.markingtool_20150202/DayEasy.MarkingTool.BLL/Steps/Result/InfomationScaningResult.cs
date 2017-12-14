using DayEasy.MarkingTool.BLL.Entity;
using System;

namespace DayEasy.MarkingTool.BLL.Steps.Result
{
    /// <summary>
    /// 作业基本信息扫描结果
    /// </summary>
    public class InfomationScaningResult : StepResult
    {
        public InfomationScaningResult(StudentInfo scanResult)
            : base(true, null)
        {
            ScanResult = scanResult;
        }

        public InfomationScaningResult(bool isSuccess, Exception exception, StudentInfo scanResult)
            : base(isSuccess, exception)
        {
            ScanResult = scanResult;
        }

        /// <summary>
        /// 基本信息扫描详细结果. Key是内容的名称, Value是结果值.
        /// </summary>
        public StudentInfo ScanResult { get; private set; }
    }
}
