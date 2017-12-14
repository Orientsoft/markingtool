using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.Open.Model.Marking;
using DayEasy.Open.Model.Paper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Scanners.Builder
{
    /// <summary>
    /// 阅作业Builder类
    /// </summary>
    public class PaperScannerBuidler : ScannerBuilder
    {
        private readonly Logger _logger = Logger.L<PaperScannerBuidler>();

        public PaperScannerBuidler(string imagePath, PaperUsageInfo usage, string classId, FileManager fileManager)
            : base(imagePath, usage, classId, fileManager)
        {
        }

        /// <summary>
        /// 预处理图片
        /// 纠偏、保存
        /// </summary>
        /// <returns></returns>
        public override JsonResultBase LoadImage()
        {
            var bmp = LoadPicture();
            if (bmp == null)
                return new JsonResultBase("试卷扫描图片异常");
            //自动纠偏
            if (DeyiKeys.Section.PathConfig.IsCorrection)
            {
                var angle = ImageHelper.GetSkewAngle(bmp);
                if (angle > 0 || angle < 0)
                    bmp = ImageHelper.RotateImage(bmp, (float) -angle);
            }
            using (bmp)
            {
                BatchFile.SavePicture(bmp, ImageName);
            }
            return new JsonResultBase(true, string.Empty);
        }

        /// <summary>
        /// 切线
        /// </summary>
        /// <returns></returns>
        public override JsonResults<int> FindLines()
        {
            using (var paperImage = BatchFile.GetPicture(ImageName))
            {
                var scanner = new LineScanner();
                var result = scanner.BitsScan(paperImage);
                if (!result.Status)
                {
                    return new JsonResults<int>(result.Description);
                }
                if (!result.Data.Any())
                    return new JsonResults<int>("试卷图片分割线未识别");
                return result;
            }
        }

        /// <summary>
        /// 切图
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public override JsonResultBase CuttingStage(IEnumerable<int> lines)
        {
            using (var slice = new SliceMap(BatchFile.GetPicture(ImageName), lines.ToList(), BatchFile, ImageName))
            {
                var result = slice.StartAction();
                if (result.Status)
                {
                    PaperMarkedInfo.PageCount = result.Data;
                }
                return result;
            }
        }

        /// <summary>
        /// 基础信息识别
        /// </summary>
        /// <returns></returns>
        public override JsonResult<StudentInfo> RecognitionBasicInfo()
        {
            var basiceImage = BatchFile.GetRecognitionImage(ImageName);
            if (basiceImage == null)
                return new JsonResult<StudentInfo>("基础信息识别错误！");
            using (basiceImage)
            {
                var scanner = new PaperBasicInfoScanner();
                var result = scanner.Scan(basiceImage);
                if (!result.Status)
                    return new JsonResult<StudentInfo>(result.Description);

                var student = scanner.Student;
                if (student == null)
                    return new JsonResult<StudentInfo>("学生信息未识别");
                PaperMarkedInfo.StudentId = student.Id;
                PaperMarkedInfo.StudentName = student.Name;
#if !DEBUG
                if (!string.Equals(student.ClassId, ClassId, StringComparison.CurrentCultureIgnoreCase))
                    return new JsonResult<StudentInfo>("学生班级不匹配");
#endif
                return new JsonResult<StudentInfo>(true, student);
            }
        }

        /// <summary>
        /// 答题卡识别
        /// </summary>
        /// <returns></returns>
        public override JsonResults<bool> RecognitionAnswerSheet()
        {
            var answerSheetImage = BatchFile.GetRecognitionImage(ImageName, RecognitionType.AnswerSheet);
            if (answerSheetImage == null)
                return new JsonResults<bool>("答题卡识别区域异常！");
            try
            {
                using (answerSheetImage)
                {
                    using (var helper = new AnswerSheetHelper(answerSheetImage, 18, 24))
                    {
                        var result = helper.GetResult();
                        return new JsonResults<bool>(result, result.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.E("答题卡识别区域异常", ex);
                return new JsonResults<bool>("答题卡识别区域异常！");
            }
        }

        /// <summary>
        /// 填充结果
        /// </summary>
        /// <param name="student"></param>
        /// <param name="checkedList"></param>
        /// <returns></returns>
        public override JsonResultBase FillMarkingInfo(StudentInfo student, List<bool> checkedList)
        {
            FillAnswerSheet(checkedList);

            MarkingResult.AddedAt = DateTime.Now;
            var ips = Helper.GetHostIp();
            MarkingResult.AddedIp = (ips.Length > 1 ? ips[1] : ips[0]);
            MarkingResult.AddedBy = UsageInfo.CreatorId;
            MarkingResult.MarkingBy = DeyiApp.UserId;
            MarkingResult.PaperId = UsageInfo.PaperId;
            MarkingResult.StudentId = student.Id;
            MarkingResult.IsFinished = false;

            PaperMarkedInfo.PaperTitle = UsageInfo.Title;

            var questions = UsageInfo.Sections.SelectMany(d => d.Questions).ToList();
            var markedIds = MarkingResult.Details.Select(t => t.QuestionId).Distinct();
            //填满阅卷结果里面的题目
            foreach (var item in questions.Where(q => !markedIds.Contains(q.QuestionId)))
            {
                if (item.Info.Details != null && item.Info.Details.Any())
                {
                    var detailIds = item.Info.Details.Select(t => t.DetailId).ToList();
                    MarkingResult.Details.AddRange(
                        detailIds.Select(d => new MarkingDetail
                        {
                            Id = Helper.Guid32,
                            QuestionId = item.QuestionId,
                            SmallQuestionId = d,
                            TeacherComments = new List<TeacherCommentInfo>(),
                            MarkingSymbols = new List<MarkingSymbolInfo>(),
                            IsCorrect = true //主观题默认视为正确
                        }));
                }
                else
                {
                    MarkingResult.Details.Add(new MarkingDetail
                    {
                        Id = Helper.Guid32,
                        QuestionId = item.QuestionId,
                        TeacherComments = new List<TeacherCommentInfo>(),
                        MarkingSymbols = new List<MarkingSymbolInfo>(),
                        IsCorrect = true //主观题默认视为正确
                    });
                }
            }
            PaperMarkedInfo.IsSuccess = true;
            return new JsonResultBase(true, string.Empty);
        }
    }
}