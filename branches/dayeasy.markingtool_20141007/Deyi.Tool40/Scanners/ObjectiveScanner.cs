using System.Globalization;
using Deyi.Tool.Common;
using Deyi.Tool.PaperServiceReference;
using Deyi.Tool.Step;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Deyi.Tool.Scanners
{
    /// <summary>
    /// 主观题扫描
    /// </summary>
    public class ObjectiveScanner : IScanner
    {
        private readonly Logger _logger = Logger.L<ObjectiveScanner>();

        private readonly List<QuestionEntity> _questions;

        public ObjectiveScanner(IEnumerable<QuestionEntity> questions)
        {
            _questions = questions.ToList();
        }

        private List<QuestionEntity> Objectives
        {
            get
            {
                if (_questions == null)
                    return _questions;
                return
                    _questions.Where(q => q.DetailList.Count > 0 && Helper.IsObjective(q.Base.TypeID, q.Base.SubjectID))
                        .ToList();
            }
        }

        /// <summary>
        /// 开始扫描
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public StepResult Scan(Image img)
        {
            var answer = new MarkingResult {Detail = new List<MarkingDetail>()};
            
            //没有客观题就直接返回
            if (Objectives == null || !Objectives.Any())
                return new ObjectiveScanningResult(answer);
            List<bool> checkedList;
            using (var helper = new AnswerSheetHelper(new Bitmap(img), 16, 24))
            {
                checkedList = helper.GetResult();
                if (DeyiKeys.WriteFile)
                    _logger.I(checkedList.ToJson());
            }

            try
            {
                int currentblockindex = 0, sort = 0;
                MarkingDetail detail;
                StringBuilder studentanswer, errorSort = new StringBuilder();

                foreach (var item in _questions)
                {
                    sort++;
                    if (!Helper.IsObjective(item.Base.TypeID, item.Base.SubjectID))
                        continue;
                    detail = new MarkingDetail
                    {
                        ID = Guid.NewGuid(),
                        MarkingSymbols = new List<MarkingSymbolInfo>(),
                        TeacherComments = new List<TeacherCommentInfo>()
                    };
                    studentanswer = new StringBuilder();
                    //绑定原题
                    foreach (var qDetail in item.DetailList)
                    {
                        //本应为 currentblockindex -1, 但考虑题目间必有一空格，而 +1，选中第一格内容必然是题号，遂Skip之。
                        var part = checkedList.Skip(currentblockindex + 1).Take(qDetail.AnswerList.Count).ToArray();

                        //因为排除了题号和空格，所以就剩下类似于 "[1]True [2]False [3]False [4]False" 这样的数据, 正好可以跟
                        //[1]A [2]B [3]C [4]D 对应上。
                        int count = 0;
                        for (int optionIndex = 0; optionIndex < part.Length; optionIndex++)
                        {
                            //选择了该项
                            if (part[optionIndex])
                            {
                                count++;
                                studentanswer.AppendFormat("{0},", DeyiKeys.AnswerOptions[optionIndex]);
                                detail.IsCorrect = qDetail.AnswerList[optionIndex].IsCorrect;
                                //多选有一个选错则错
                                if (!detail.IsCorrect)
                                {
                                    break;
                                }
                            }
                            if (count != qDetail.AnswerList.Count(a => a.IsCorrect))
                            {
                                detail.IsCorrect = false;
                            }

                        }

                        if (detail.IsCorrect)
                        {
                            detail.Score += qDetail.Base.Score;
                        }
                        else
                        {
                            errorSort.AppendFormat("{0},",
                                item.DetailList.Count > 1
                                    ? string.Format("{0}-{1}", sort, qDetail.Base.Sort)
                                    : sort.ToString(CultureInfo.InvariantCulture));
                        }
                        if (count == 0)
                        {
                            studentanswer.Append("-,");
                        }
                        currentblockindex += qDetail.AnswerList.Count + 2;
                    }

                    detail.StudentAnswer = studentanswer.ToString().TrimEnd(',');
                    detail.QuestionID = item.Base.ID;
                    detail.QuestionVersion = item.Base.EntireVersionID;
                    answer.Detail.Add(detail);

                }

                answer.Detail[0].TeacherComments.Add(new TeacherCommentInfo
                {
                    CommentText = string.Format("错题:{0}", errorSort.ToString().TrimEnd(',')),
                    Position = new Point(-1, -1)
                });

                return new ObjectiveScanningResult(answer);
            }
            catch (Exception ex)
            {
                return new ObjectiveScanningResult(false, ex, answer);
            }
        }

        public Color GetPixel(Bitmap b, int x, int y)
        {

            var p = b.GetPixel(x >= b.Width ? b.Width - 1 : x, y >= b.Height ? b.Height - 1 : y);

            return p;
        }
    }
}
