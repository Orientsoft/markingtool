using DayEasy.MarkingTool.BLL.Steps.Result;

namespace DayEasy.MarkingTool.BLL.Steps
{
    /// <summary>
    /// 步骤接口
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// 实际操作
        /// </summary>
        /// <param name="args">参数</param>
        /// <returns>操作结果</returns>
        StepResult Process(params object[] args);
    }
}
