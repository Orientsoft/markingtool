
using System.ComponentModel;

namespace Deyi.Tool.Entity.Paper
{
    public enum PaperKind
    {
        [Description("作业")]
        Paper = 1,
        [Description("答题卡")]
        AnswerCard = 2
    }
}
