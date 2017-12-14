using System.Drawing;
using System.Globalization;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.Open.Model.Marking;
using DayEasy.Open.Model.Paper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DayEasy.Open.Model.Question;

namespace DayEasy.MarkingTool.BLL.Scanners.Builder
{
    /// <summary>
    /// 基础Builder类，即抽象建造者类
    /// </summary>
    public abstract class ScannerBuilder
    {

        #region 构造函数

        private string _imagePath;
        protected readonly string ClassId;
        public readonly PaperUsageInfo UsageInfo;
        public PaperMarkedInfo PaperMarkedInfo;
        public MarkingResult MarkingResult;
        private string _imageName;

        public string ImagePath
        {
            get { return _imagePath; }
        }

        public string ImageName
        {
            get { return _imageName; }
        }

        public void SetImagePath(string path)
        {
            _imagePath = path;
            _imageName = Path.GetFileNameWithoutExtension(_imagePath);
        }

        protected ScannerBuilder(string imagePath, PaperUsageInfo usage,string classId)
        {
            _imagePath = imagePath;
            _imageName = Path.GetFileName(ImagePath);
            ClassId = classId;
            UsageInfo = usage;
            PaperMarkedInfo = new PaperMarkedInfo
            {
                MarkedResultId = Helper.Guid32,
                ImagePath = ImagePath,
                BacthCode = usage.Batch,
                IsSuccess = false,
                PageCount = 1
            };
            MarkingResult = new MarkingResult
            {
                Id = PaperMarkedInfo.MarkedResultId,
                Batch = usage.Batch
            };
        }

        #endregion

        /// <summary>
        /// step 1. 预处理图片 权重：3
        /// </summary>
        public abstract JsonResultBase LoadImage();

        /// <summary>
        /// step 2. 寻找分割线条 权重：1
        /// </summary>
        public abstract JsonResults<int> FindLines();

        /// <summary>
        /// step 3. 切图 权重：1
        /// </summary>
        /// <param name="lines"></param>
        public abstract JsonResultBase CuttingStage(IEnumerable<int> lines);

        /// <summary>
        /// step 4. 识别基本信息 权重：1
        /// </summary>
        public abstract JsonResult<StudentInfo> RecognitionBasicInfo();

        /// <summary>
        /// step 5. 识别答题卡 权重：2
        /// </summary>
        public abstract JsonResults<bool> RecognitionAnswerSheet();

        /// <summary>
        /// step 6. 填充阅卷信息 权重：2
        /// </summary>
        public abstract JsonResultBase FillMarkingInfo(StudentInfo student, List<bool> answerSheet);

        /// <summary>
        /// 根据答题卡填充阅卷结果
        /// </summary>
        /// <param name="checkedList"></param>
        protected virtual void FillAnswerSheet(List<bool> checkedList)
        {
            //获取客观题
            var questions =
                UsageInfo.Sections.SelectMany(s => s.Questions.Select(q => q.Info))
                    .Where(q => q.IsObjective).ToList();
            PaperMarkedInfo.TotalCount = questions.Count;
            if (!questions.Any())
                return;

            int currentblockindex = 0;
            var errorTag = new List<string>();

            foreach (var item in questions)
            {
                var qItem = UsageInfo.Sections.SelectMany(s => s.Questions)
                    .FirstOrDefault(t => t.QuestionId == item.QuestionId);
                if (qItem == null) continue;
                int index = qItem.Index;
                if (item.Details == null || !item.Details.Any())
                {
                    //无小问
                    var detail = new MarkingDetail
                    {
                        Id = Helper.Guid32,
                        QuestionId = item.QuestionId,
                        MarkingSymbols = new List<MarkingSymbolInfo>(),
                        TeacherComments = new List<TeacherCommentInfo>(),
                    };
                    var part = checkedList.Skip(currentblockindex + 1).Take(item.Answers.Count).ToList();
                    currentblockindex += item.Answers.Count + 2;
                    Marking(detail, part, item.Answers);
                    detail.MarkingTag = index.ToString(CultureInfo.InvariantCulture);
                    if (detail.IsCorrect)
                        detail.Score = item.Score;
                    else
                    {
                        errorTag.Add(detail.MarkingTag);
                    }
                    MarkingResult.Details.Add(detail);
                }
                else
                {
                    var detailIndex = 1;
                    //有小问
                    foreach (var detailInfo in item.Details)
                    {
                        var detail = new MarkingDetail
                        {
                            Id = Helper.Guid32,
                            QuestionId = item.QuestionId,
                            SmallQuestionId = detailInfo.DetailId,
                            MarkingSymbols = new List<MarkingSymbolInfo>(),
                            TeacherComments = new List<TeacherCommentInfo>(),
                        };
                        var part = checkedList.Skip(currentblockindex + 1).Take(detailInfo.Answers.Count).ToList();
                        currentblockindex += detailInfo.Answers.Count + 2;
                        //无小问
                        Marking(detail, part, detailInfo.Answers);
                        detail.MarkingTag = string.Format("{0}-{1}", index, detailIndex);

                        if (detail.IsCorrect)
                            detail.Score = detailInfo.Score;
                        else
                        {
                            errorTag.Add(detail.MarkingTag);
                        }
                        MarkingResult.Details.Add(detail);
                        detailIndex++;
                    }
                }
            }
            MarkingResult.Details[0].TeacherComments.Add(new TeacherCommentInfo
            {
                Position = new Point(1, 1),
                CommentText = errorTag.Any() ? string.Format("客观题错题：{0}", string.Join(",", errorTag)) : "客观题全对"
            });
            //计算总分及错题数
            MarkingResult.TotalScore = MarkingResult.Details.Where(d => d.IsCorrect).Sum(d => d.Score);
            PaperMarkedInfo.TotalScore = MarkingResult.TotalScore;
            PaperMarkedInfo.ErorrCount = MarkingResult.Details.Count(d => !d.IsCorrect);
        }

        //根据答题卡阅卷
        private void Marking(MarkingDetail detail, List<bool> checkedList, List<AnswerInfo> answers)
        {
            //绑定原题
            //本应为 currentblockindex -1, 但考虑题目间必有一空格，而 +1，选中第一格内容必然是题号，遂Skip之。
            //var part = checkedList.Skip(currentblockindex + 1).Take(item.Answers.Count).ToArray();

            //因为排除了题号和空格，所以就剩下类似于 "[1]True [2]False [3]False [4]False" 这样的数据, 正好可以跟
            //[1]A [2]B [3]C [4]D 对应上。
            if (checkedList.All(t => !t))
            {
                //
                detail.IsCorrect = false;
                detail.AnswerIds = new string[] {};
                detail.StudentAnswer = string.Empty;
                return;
            }

            string studentanswer = string.Empty;
            var answerIds = new List<string>();
            for (int optionIndex = 0; optionIndex < checkedList.Count; optionIndex++)
            {
                //选择了该项
                if (checkedList[optionIndex])
                {
                    detail.IsCorrect = answers[optionIndex].IsCorrect;
                    //多选有一个选错则错
                    if (!detail.IsCorrect)
                        break;
                }
                else
                {
                    if (answers[optionIndex].IsCorrect)
                    {
                        detail.IsCorrect = false;
                        break;
                    }
                }
            }
            for (var i = 0; i < checkedList.Count; i++)
            {
                if (checkedList[i])
                {
                    answerIds.Add(answers[i].AnswerId);
                    studentanswer += DeyiKeys.OptionWords[i];
                }
            }

            detail.AnswerIds = answerIds.ToArray();
            detail.StudentAnswer = studentanswer;
        }
    }
}
