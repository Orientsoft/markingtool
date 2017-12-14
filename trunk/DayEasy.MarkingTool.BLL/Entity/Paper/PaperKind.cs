
using System.ComponentModel;

namespace DayEasy.MarkingTool.BLL.Entity.Paper
{
    /// <summary> 阅卷类型 </summary>
    public enum PaperKind
    {
        /// <summary> 作业 </summary>
        [Description("作业")]
        Paper = 1,
        /// <summary> 答题卡 </summary>
        [Description("答题卡")]
        AnswerCard = 2
    }
}