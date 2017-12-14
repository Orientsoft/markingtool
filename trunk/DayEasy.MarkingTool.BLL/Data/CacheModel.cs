
using System;

namespace DayEasy.MarkingTool.BLL.Data
{
    public enum CacheType
    {
        Account = 0,
        PaperNum = 1,
        SelectPath = 2,
        GroupCode = 3
    }

    public class CacheModel
    {
        public string Id { get; set; }
        public int Type { get; set; }
        public string Value { get; set; }
        public DateTime Created { get; set; }
    }
}
