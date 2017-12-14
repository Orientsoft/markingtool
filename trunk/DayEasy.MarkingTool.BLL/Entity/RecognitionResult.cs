
using System.Collections.Generic;

namespace DayEasy.MarkingTool.BLL.Entity
{
    public class RecognitionResult
    {
        public bool Status { get; set; }
        /// <summary> 学生信息 </summary>
        public StudentInfo Student { get; set; }

        /// <summary> 答题卡信息 </summary>
        public List<int[]> Sheets { get; set; }

        public RecognitionResult()
        {
            Status = true;
            Student = new StudentInfo();
            Sheets = new List<int[]>();
        }
    }
}
