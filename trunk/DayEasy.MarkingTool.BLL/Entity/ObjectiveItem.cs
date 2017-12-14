namespace DayEasy.MarkingTool.BLL.Entity
{
    /// <summary> 客观题信息 </summary>
    public class ObjectiveItem
    {
        /// <summary> 序号 </summary>
        public string Sort { get; set; }

        /// <summary> 选项数 </summary>
        public int Count { get; set; }

        /// <summary> 是否单选 </summary>
        public bool Single { get; set; }
    }
}
