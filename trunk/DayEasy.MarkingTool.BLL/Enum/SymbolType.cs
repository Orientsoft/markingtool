using System.ComponentModel;

namespace DayEasy.MarkingTool.BLL.Enum
{
    public enum SymbolType : byte
    {
        /// <summary> 正确 </summary>
        [Description("正确")]
        Right = 0,

        /// <summary> 半对 </summary>
        [Description("半对")]
        HalfRight = 1,

        /// <summary> 错误 </summary>
        [Description("错误")]
        Wrong = 2,

        /// <summary>  图片批注 </summary>
        [Description("图片批注")]
        Comment = 3,

        /// <summary>  自定义批注 </summary>
        [Description("自定义批注")]
        Custom = 4,

        /// <summary> 表情 </summary>
        [Description("表情")]
        Emotion = 5,


        /// <summary> 客观题信息 </summary>
        [Description("客观题信息")]
        Objective = 6
    }
}
