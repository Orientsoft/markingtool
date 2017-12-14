using DayEasy.Models.Open;
using System.Collections.Generic;

namespace DayEasy.MarkingTool.BLL.Entity
{
    public class ResizeResult : DDto
    {
        public string ImagePath { get; set; }
        public List<LineInfo> Lines { get; set; }
    }
}
