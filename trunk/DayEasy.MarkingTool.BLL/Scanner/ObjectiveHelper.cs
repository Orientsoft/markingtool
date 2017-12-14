//#define sheets

using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.Models.Open.Paper;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Scanner
{
    public static class ObjectiveHelper
    {
        private static readonly ConcurrentDictionary<string, List<ObjectiveItem>> ObjectiveCache;

        static ObjectiveHelper()
        {
            ObjectiveCache = new ConcurrentDictionary<string, List<ObjectiveItem>>();
        }

        public static List<ObjectiveItem> GetObjectives(MPaperDto paper, byte type)
        {
            if (paper == null || string.IsNullOrWhiteSpace(paper.Id))
                return null;
            type = (byte)(type == 0 ? 1 : type);
            var key = string.Concat(paper.Id, "_", type);
            if (ObjectiveCache.ContainsKey(key))
            {
                return ObjectiveCache[key];
            }
            var sheets = new List<ObjectiveItem>();

#if sheets
            for (var i = 0; i < 75; i++)
            {
                if (i >= 25 && i <= 29)
                    continue;
                if (i >= 5 && i <= 9)
                    sheets.Add(new ObjectiveItem { Sort = (i + 1).ToString(), Count = 5, Single = true });
                else if (i >= 40 && i <= 44)
                    sheets.Add(new ObjectiveItem { Sort = (i + 1).ToString(), Count = 6, Single = true });
                else
                    sheets.Add(new ObjectiveItem { Sort = (i + 1).ToString(), Count = 3, Single = true });
            }
            //for (var i = 0; i < 85; i++)
            //{
            //    sheets.Add(new ObjectiveItem { Sort = (i + 1).ToString(), Count = 4, Single = true });
            //}
#else
            var questions = paper.Sections.Where(t => t.PaperSectionType == type).OrderBy(t => t.Sort)
                .SelectMany(s => s.Questions.OrderBy(q => q.Sort)).Where(q => q.IsObjective).ToList();
            if (!questions.Any())
                return null;
            var smallRow = paper.SmallRow(type);
            foreach (var dto in questions)
            {
                if (dto.Details != null && dto.Details.Any())
                {
                    sheets.AddRange(dto.Details.Select(detail => new ObjectiveItem
                    {
                        Sort = smallRow ? detail.Sort.ToString() : string.Concat(dto.Sort, ".", detail.Sort),
                        Count = detail.Answers.Count,
                        Single = !dto.HasMulti
                    }));
                }
                else
                {
                    sheets.Add(new ObjectiveItem
                    {
                        Sort = dto.Sort.ToString(),
                        Count = dto.Answers.Count,
                        Single = !dto.HasMulti
                    });
                }
            }
#endif
            ObjectiveCache.TryAdd(key, sheets);
            return sheets;
        }

        public static void CheckAnswer(MPaperDto paper, PaperMarkedInfo info, IList<int[]> sheets)
        {
            var list = GetObjectives(paper, (byte)info.SectionType);
            if (list == null || !list.Any())
                return;
            var index = 0;
            info.RatiosColor = "Black";
            foreach (var item in list)
            {
                var sheet = sheets[index];
                if (sheet.All(t => t < 0) || (item.Single && sheet.Count() > 1))
                {
                    info.RatiosColor = "Red";
                    break;
                }
                index++;
            }
        }

        /// <summary> 异常数量 </summary>
        /// <param name="objectives"></param>
        /// <param name="sheets"></param>
        /// <returns></returns>
        public static int ExceptionCount(this List<ObjectiveItem> objectives, List<int[]> sheets)
        {
            int index = 0, count = 0;
            foreach (var item in objectives)
            {
                if (index >= sheets.Count())
                {
                    count++;
                    continue;
                }
                var sheet = sheets[index];
                if (sheet.All(t => t < 0) || (item.Single && sheet.Count() > 1))
                    count++;
                index++;
            }
            return count;
        }

        public static int[] Answers(this Dictionary<int, double> ranks, bool isSingle = true)
        {
            //var logger = Logger.L("ranks");
            //logger.I(ranks.ToJson());
            //var checkList = ranks.Where(t => t.Value > 0).ToList();
            //if (!checkList.Any())
            //    return new[] { -1 };
            //return checkList.Select(t => t.Key).ToArray();

            if (ranks.All(t => t.Value < 0.01D))
                return new[] { -1 };
            //多选，不过滤
            if (!isSingle && ranks.Any(t => t.Value > 0.03D))
                return ranks.Where(t => t.Value > 0.03D).Select(t => t.Key).OrderBy(t => t).ToArray();
            var answers = new List<int>();
            var min = ranks.Min(t => t.Value);
            //中值过滤
            var mid = min + (ranks.Max(t => t.Value) - min) / 2;
            var checkList = ranks.Where(t => t.Value > mid).OrderByDescending(t => t.Value).ToList();
            if (!checkList.Any())
            {
                answers.Add(-1);
            }
            else if (checkList.Count == 1)
            {
                answers.Add(checkList[0].Key);
            }
            else
            {
                //var list = new List<double>();
                for (var i = 0; i < checkList.Count(); i++)
                {
                    var item = checkList[i];
                    var diff = 0D;
                    if (i > 0)
                    {
                        diff = checkList[i - 1].Value - item.Value;
                        //list.Add(diff);
                    }
                    if (i == 0 || diff < 0.05D)
                        answers.Add(item.Key);
                    else
                        break;
                }
                //logger.I($"diffs:{list.ToJson()}");
            }
            return answers.OrderBy(t => t).ToArray();
        }

        /// <summary> 是否小问通排 </summary>
        /// <param name="paper"></param>
        /// <param name="sectionType"></param>
        /// <returns></returns>
        private static bool SmallRow(this MPaperDto paper, byte sectionType)
        {
            //语文整卷，英语A卷
            return (paper.SubjectId == 1 || (paper.SubjectId == 3 && sectionType == 1));
        }
    }
}
