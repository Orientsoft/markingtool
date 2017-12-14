
using System;

namespace DayEasy.MarkingTool.BLL.Entity
{
    /// <summary> 学生信息 </summary>
    [Serializable]
    public class StudentInfo
    {
        /// <summary> 学生ID </summary>
        public long Id { get; set; }

        public string Code { get; set; }

        /// <summary> 学生姓名 </summary>
        public string Name { get; set; }

        /// <summary> 班级ID </summary>
        public string ClassId { get; set; }
    }
}
