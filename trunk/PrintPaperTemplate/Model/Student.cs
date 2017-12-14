
namespace PrintPaperTemplate.Model
{
    /// <summary> 学生信息 </summary>
    public class Student
    {
        /// <summary> 学生ID </summary>
        public long Id { get; set; }

        /// <summary> 学生姓名 </summary>
        public string Name { get; set; }

        /// <summary> 用户名 </summary>
        public string Email { get; set; }
        /// <summary> 班级ID </summary>
        public string ClassId { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Name) ||
                Id <= 0)
            {
                return string.Empty;
            }
            return string.Format("{0},{1},{2}", Name.Trim(), Id, ClassId);
        }
    }
}
