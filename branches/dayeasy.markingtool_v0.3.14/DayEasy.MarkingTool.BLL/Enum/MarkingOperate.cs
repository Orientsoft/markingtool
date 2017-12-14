
using System.ComponentModel;

namespace DayEasy.MarkingTool.BLL.Enum
{
    public enum MarkingOperate : byte
    {
        /// <summary>
        /// 选择器
        /// </summary>
        [Description("选择器")] Pointer = 0,

        /// <summary>
        /// 正确(勾)
        /// </summary>
        [Description("正确(勾)")] Hook = 1,

        /// <summary>
        /// 错误(叉)
        /// </summary>
        [Description("错误(叉)")] Fork = 2,

        /// <summary>
        /// 注释
        /// </summary>
        [Description("注释")] Comment = 3,

        /// <summary>
        /// 橡皮擦
        /// </summary>
        [Description("橡皮擦")] Erase = 4,

        /// <summary>
        /// 半对(半勾)
        /// </summary>
        [Description("半勾")] HalfHook = 5,
        /// <summary>
        /// 表情
        /// </summary>
        [Description("表情")] Emotion = 100
    }
}