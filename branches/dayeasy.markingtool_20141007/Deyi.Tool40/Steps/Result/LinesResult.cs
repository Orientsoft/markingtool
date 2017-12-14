using Deyi.Tool.Step;
using System;
using System.Collections.Generic;

namespace Deyi.Tool.Steps
{
    class LinesResult : StepResult
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
