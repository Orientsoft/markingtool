using System.IO;

namespace DayEasy.MarkingTool.BLL.Entity.Marking
{
    public class PaperMarkingFileData
    {
        public string Filename { get; set; }

        public string DirectoryName { get; set; }
        public FileStream Data { get; set; }
    }
}
