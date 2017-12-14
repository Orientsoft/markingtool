using System.Collections.Generic;

namespace DayEasy.MarkingTool.BLL.Entity.Paper
{
    public class FileResult
    {
        public int state { get; set; }
        public string msg { get; set; }
        public List<string> urls { get; set; }
    }
}
