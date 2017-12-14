using DayEasy.MarkingTool.BLL.Entity;
using System;

namespace DayEasy.MarkingTool.BLL.Recognition
{
    /// <summary> 扫描接口 </summary>
    public interface IRecognition
    {
        RecognitionResult Start(Action<string> logAction = null);
    }
}
