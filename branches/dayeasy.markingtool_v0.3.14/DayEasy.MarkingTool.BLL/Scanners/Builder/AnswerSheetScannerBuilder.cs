using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.Open.Model.Paper;
using System.Collections.Generic;

namespace DayEasy.MarkingTool.BLL.Scanners.Builder
{
    /// <summary>
    /// 阅答题卡Builder类
    /// </summary>
    public class AnswerSheetScannerBuilder
        : ScannerBuilder
    {
        public AnswerSheetScannerBuilder(string imagePath, PaperUsageInfo bacth, string classId, FileManager fileManager)
            : base(imagePath, bacth, classId, fileManager)
        {
        }

        public override JsonResultBase LoadImage()
        {
            throw new System.NotImplementedException();
        }

        public override JsonResults<int> FindLines()
        {
            throw new System.NotImplementedException();
        }

        public override JsonResultBase CuttingStage(IEnumerable<int> lines)
        {
            throw new System.NotImplementedException();
        }

        public override JsonResult<StudentInfo> RecognitionBasicInfo()
        {
            throw new System.NotImplementedException();
        }

        public override JsonResults<bool> RecognitionAnswerSheet()
        {
            throw new System.NotImplementedException();
        }

        public override JsonResultBase FillMarkingInfo(StudentInfo student, List<bool> answerSheet)
        {
            throw new System.NotImplementedException();
        }
    }
}
