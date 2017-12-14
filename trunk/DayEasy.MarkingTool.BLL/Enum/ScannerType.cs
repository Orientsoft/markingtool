using System.ComponentModel;

namespace DayEasy.MarkingTool.BLL.Enum
{
    public enum ScannerType : byte
    {
        /// <summary>
        /// A卷
        /// </summary>
        [Description("A卷")]
        PaperA = 1,

        /// <summary>
        /// B卷
        /// </summary>
        [Description("B卷")]
        PaperB = 2
    }
}
