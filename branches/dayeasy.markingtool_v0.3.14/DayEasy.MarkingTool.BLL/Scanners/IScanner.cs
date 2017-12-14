using System.Drawing;
using DayEasy.MarkingTool.BLL.Steps.Result;

namespace DayEasy.MarkingTool.BLL.Scanners
{
    /// <summary>
    /// 扫描接口
    /// </summary>
    interface IScanner
    {
        StepResult Scan(Image img);
    }
}
