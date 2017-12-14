using System;
using System.Collections.Generic;

namespace DayEasy.MarkingTool.BLL.Steps.Result
{
    public class LinesResult : StepResult
    {
        public IList<int> Lines { get; set; }

        public LinesResult(List<int> lines)
            : base(true, null)
        {
            Lines = lines;
        }
        public LinesResult(bool isSuccess, Exception exception, IList<int> line)
            : base(isSuccess, exception)
        {
            Lines = line;
        }
    }
}
