using System;
using System.Collections.Generic;


namespace Deyi.Tool.Step
{
    /// <summary>
    /// 作业基本信息扫描结果
    /// </summary>
    public class InfomationScaningResult: StepResult
    {
        public InfomationScaningResult(Dictionary<String, String> scanResult)
            :base(true, null)
        {
            this.ScanResult = scanResult;
        }

        public InfomationScaningResult(bool isSuccess, Exception exception, Dictionary<String, String> scanResult)
            : base(isSuccess, exception)
        {
            this.ScanResult = scanResult;
        }

        /// <summary>
        /// 基本信息扫描详细结果. Key是内容的名称, Value是结果值.
        /// </summary>
        public Dictionary<String, String> ScanResult
        {
            get;
            private set;
        }
    }
}
