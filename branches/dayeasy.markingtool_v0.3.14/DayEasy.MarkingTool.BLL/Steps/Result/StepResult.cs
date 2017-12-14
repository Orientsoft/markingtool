using System;

namespace DayEasy.MarkingTool.BLL.Steps.Result
{
    /// <summary>
    /// 步骤结果
    /// </summary>
    public class StepResult
    {
        public StepResult(bool isSuccess, Exception ex)
        {
            IsSuccess = isSuccess;
            Exception = ex;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess
        {
            get;
            protected set;
        }

        /// <summary>
        /// 步骤期间发生的异常
        /// </summary>
        public Exception Exception
        {
            get;
            protected set;
        }

        /// <summary>
        /// 步骤成功
        /// </summary>
        public static StepResult Success
        {
            get
            {
                return new StepResult(true, null);
            }
        }

        public string Desc
        {
            get
            {
                if (IsSuccess)
                {
                    return "处理成功";
                }
                return "处理失败";
            }
        }


    }
}
