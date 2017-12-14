
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Enum
{
    public enum SortType
    {
        Default = 0,
        IndexDesc = 1,
        ErrorCount = 2,
        ErrorCountDesc = 3,
        Score = 4,
        ScoreDesc = 5
    }

    public static class SortTypeExtend
    {
        private static readonly SortType[] DescSort = {SortType.IndexDesc, SortType.ScoreDesc, SortType.ErrorCountDesc};

        public static bool IsDesc(this SortType sortType)
        {
            return DescSort.Contains(sortType);
        }
    }
}
