using System;
using System.IO;

namespace DayEasy.MarkingTool.BLL.Entity.Paper
{
    /// <summary> 文件上传类 </summary>
    [Serializable]
    public class PaperMarkingFileData
    {
        public string Filename { get; set; }

        public string DirectoryName { get; set; }
        public FileStream Data { get; set; }
    }
}
