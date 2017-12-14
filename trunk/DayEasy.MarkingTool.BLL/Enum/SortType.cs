
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Enum
{
    public enum SortType
    {
        Default = 0,
        IndexDesc = 1,
        Type = 2,
        TypeDesc = 3,
        Name = 4,
        NameDesc = 5
    }

    public static class SortTypeExtend
    {
        private static readonly SortType[] DescSort = {SortType.IndexDesc, SortType.TypeDesc, SortType.NameDesc};

        public static bool IsDesc(this SortType sortType)
        {
            return DescSort.Contains(sortType);
        }
    }
}
