using DayEasy.MarkingTool.BLL.Common;
using DayEasy.Open.Model.Marking;
using DayEasy.Open.Model.Question;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Scanners
{
    /// <summary>
    /// 客观题扫描
    /// </summary>
    public class ObjectiveScanner
    {
        private readonly Logger _logger = Logger.L<ObjectiveScanner>();

        private readonly List<QuestionInfo> _questions;

        public ObjectiveScanner(IEnumerable<QuestionInfo> questions)
        {
            _questions = questions.ToList();
        }

        private List<QuestionInfo> Objectives
        {
            get
            {
                return (_questions == null
                    ? _questions
                    : _questions.Where(q => q.IsObjective).ToList());
            }
        }

        /// <summary>
        /// 开始扫描
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public JsonResult<MarkingResult> Scan(Image img)
        {
            var answer = new MarkingResult();

            //没有客观题就直接返回
            if (Objectives == null || !Objectives.Any())
                return new JsonResult<MarkingResult>(true, answer);
            List<bool> checkedList;
            img = ImageHelper.Resize(img, DeyiKeys.PaperWidth);
            //获取答题卡结果
            using (var helper = new AnswerSheetHelper(new Bitmap(img), 18, 24))
            {
                checkedList = helper.GetResult();
                if (DeyiKeys.WriteFile)
                    _logger.I(checkedList.ToJson());
            }

            try
            {
                int currentblockindex = 0, index = 0;
                var errorTag = new List<string>();
                //只阅客观题
                foreach (var item in Objectives)
                {
                    index = _questions.IndexOf(item) + 1;
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
                        if (detail.IsCorrect)
                            detail.Score = item.Score;
                        else
                        {
                            errorTag.Add(index.ToString(CultureInfo.InvariantCulture));
                        }
                        answer.Details.Add(detail);
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
                            if (detail.IsCorrect)
                                detail.Score = detailInfo.Score;
                            else
                            {
                                errorTag.Add(string.Format("{0}-{1}", index, detailIndex));
                            }
                            answer.Details.Add(detail);
                            detailIndex++;
                        }
                    }
                }
                answer.Details[0].TeacherComments.Add(new TeacherCommentInfo
                {
                    Position = new Point(1, 1),
                    CommentText = errorTag.Any() ? string.Format("错题：{0}", string.Join(",", errorTag)) : "全对"
                });
                return new JsonResult<MarkingResult>(true, answer);
            }
            catch (Exception ex)
            {
                return new JsonResult<MarkingResult>(ex.Message);
            }
        }

        //根据答题卡阅卷
        public void Marking(MarkingDetail detail, List<bool> checkedList, List<AnswerInfo> answers)
        {
            //绑定原题
            //本应为 currentblockindex -1, 但考虑题目间必有一空格，而 +1，选中第一格内容必然是题号，遂Skip之。
            //var part = checkedList.Skip(currentblockindex + 1).Take(item.Answers.Count).ToArray();

            //因为排除了题号和空格，所以就剩下类似于 "[1]True [2]False [3]False [4]False" 这样的数据, 正好可以跟
            //[1]A [2]B [3]C [4]D 对应上。
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

        public Color GetPixel(Bitmap b, int x, int y)
        {
            return b.GetPixel(x >= b.Width ? b.Width - 1 : x, y >= b.Height ? b.Height - 1 : y);
        }
    }
}
