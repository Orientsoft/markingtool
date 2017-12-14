using System.ComponentModel;

namespace DayEasy.MarkingTool.BLL.Enum
{
    public enum MarkingStatus : byte
    {
        [Description("未批阅")]
        NotMarking = 0,
        [Description("A卷完成")]
        FinishedA = 1,
        [Description("B卷完成")]
        FinishedB = 2,
        [Description("批阅完成")]
        AllFinished = 3
    }
}
