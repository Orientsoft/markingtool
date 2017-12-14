using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Deyi.Tool.Step
{
    /// <summary>
    /// 步骤结果
    /// </summary>
    public class StepResult
    {
        public StepResult(bool isSuccess, Exception ex)
        {
            this.IsSuccess = isSuccess;
            this.Exception = ex;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public virtual bool IsSuccess
        {
            get;
            protected set;
        }

        /// <summary>
        /// 步骤期间发生的异常
        /// </summary>
        public virtual Exception Exception
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
                else
                {
                    return "处理失败";
                }
            }
        }


    }
}
