
using System.ComponentModel;

namespace DayEasy.MarkingTool.BLL.Enum
{
    public enum JointStatus : byte
    {
        [Description("批阅中")]
        Normal = 0,
        [Description("批阅完成")]
        Finished = 2,
        [Description("已删除")]
        Delete = 4
    }
}
