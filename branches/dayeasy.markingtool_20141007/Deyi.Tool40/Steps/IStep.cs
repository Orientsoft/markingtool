using Deyi.Tool.Step;

namespace Deyi.Tool.Steps
{
    interface IStep
    {

        /// <summary>
        /// 实际操作
        /// </summary>
        /// <param name="args">参数</param>
        /// <returns>操作结果</returns>
        StepResult Process(params object[] args);

    }
}
