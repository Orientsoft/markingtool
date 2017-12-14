using System.ComponentModel;

namespace DayEasy.MarkingTool.BLL.Enum
{
    /// <summary> 试卷类型 </summary>
    public enum PaperType : byte
    {
        /// <summary> 普通卷 </summary>
        [Description("常规卷")]
        Normal = 1,

        /// <summary> AB卷 </summary>
        [Description("AB卷")]
        PaperAb = 2
    }
}
