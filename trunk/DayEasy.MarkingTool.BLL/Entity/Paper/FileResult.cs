using System;
using System.Collections.Generic;

namespace DayEasy.MarkingTool.BLL.Entity.Paper
{
    /// <summary> 文件上传返回结果 </summary>
    [Serializable]
    public class FileResult
    {
        public int state { get; set; }
        public string msg { get; set; }
        public List<string> urls { get; set; }
    }
}
