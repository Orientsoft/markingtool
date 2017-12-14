
using DayEasy.Models.Open.Paper;
using DayEasy.Models.Open.Work;
using org.in2bits.MyXls;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Common
{
    public static class ExcelHelper
    {
        /// <summary>
        /// 导出Excel - 支持多Sheet
        /// DataTable = Sheet
        /// DataTable.TableName = Sheet.Name
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="fileName">文件名.xls</param>
        /// <param name="savePath">保存路径，为空则使用流的方式下载</param>
        public static void Export(DataSet ds, string fileName, string savePath = "")
        {
            if (ds == null || ds.Tables.Count == 0 ||
                (string.IsNullOrWhiteSpace(fileName) && string.IsNullOrWhiteSpace(savePath)))
                return;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = Path.GetFileName(savePath);
            }
            var xls = new XlsDocument
            {
                FileName = fileName
            };
            xls.SummaryInformation.Author = "DayEasy"; //文件作者
            xls.SummaryInformation.Subject = "DayEasy-DataExport"; //文件主题
            xls.DocumentSummaryInformation.Company = "www.dayeasy.net"; //文件公司信息

            //Excel首行样式
            var xf = xls.NewXF();
            xf.HorizontalAlignment = HorizontalAlignments.Centered; //居中
            xf.Font.ColorIndex = 0;
            xf.Font.FontName = "宋体";
            xf.Font.Bold = true;
            xf.Font.Height = 13 * 20; //大小
            xf.Pattern = 1; //背景 0(无色) 1(没有间隙的实色)
            xf.UseBorder = true;
            xf.TopLineStyle = xf.LeftLineStyle = xf.RightLineStyle = xf.BottomLineStyle = 1;
            xf.TopLineColor = xf.LeftLineColor = xf.RightLineColor = xf.BottomLineColor = Colors.Black;
            xf.PatternBackgroundColor = Colors.Default29;
            xf.PatternColor = Colors.Default29;

            var normalXf = xls.NewXF();
            normalXf.HorizontalAlignment = HorizontalAlignments.Centered; //居中
            normalXf.Font.ColorIndex = 0;
            normalXf.Font.FontName = "宋体";
            normalXf.Font.Height = 13 * 20; //大小
            normalXf.Pattern = 0; //背景 0(无色) 1(没有间隙的实色)
            normalXf.UseBorder = true;
            normalXf.TopLineStyle = normalXf.LeftLineStyle = normalXf.RightLineStyle = normalXf.BottomLineStyle = 1;
            normalXf.TopLineColor = normalXf.LeftLineColor = normalXf.RightLineColor = normalXf.BottomLineColor = Colors.Black;
            for (var i = 0; i < ds.Tables.Count; i++)
            {
                var dt = ds.Tables[i];
                if (dt == null || dt.Rows.Count == 0)
                    continue;
                var sheetName = !string.IsNullOrWhiteSpace(dt.TableName) ? dt.TableName : "sheet" + (i + 1);
                var sheet = xls.Workbook.Worksheets.Add(sheetName); //sheet 页
                var cells = sheet.Cells;
                for (var j = 0; j < dt.Rows.Count; j++)
                {
                    var drCells = dt.Rows[j].ItemArray;
                    for (var k = 0; k < drCells.Length; k++)
                    {
                        cells.Add(j + 1, k + 1, drCells[k] ?? string.Empty, j == 0 ? xf : normalXf);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(savePath))
            {
                xls.Save(Path.GetDirectoryName(savePath));
                return;
            }
            xls.Send();
        }

        public static DataTable SheetDetails(IList<MPaperQuestionDto> questions, IList<MPictureInfo> pictures)
        {
            var dt = new DataTable("扫描详情");
            var header = new List<object> { "学生" };
            dt.Columns.Add("学生");
            foreach (var question in questions)
            {
                if (question.Details != null && question.Details.Any())
                {
                    foreach (var detail in question.Details)
                    {
                        dt.Columns.Add(Helper.Guid32);
                        header.Add(string.Concat(detail.Sort, "题"));
                    }
                }
                else
                {
                    dt.Columns.Add(Helper.Guid32);
                    header.Add(string.Concat(question.Sort, "题"));
                }
            }
            dt.Rows.Add(header.ToArray());
            foreach (var picture in pictures.OrderBy(t => t.Index))
            {
                var row = new List<object> { picture.StudentName ?? string.Empty };
                for (var i = 0; i < header.Count - 1; i++)
                {
                    if (picture.SheetAnwers.Count > i)
                    {
                        var answer = picture.SheetAnwers[i].Where(t => t >= 0 && t < 26).ToList();
                        row.Add(answer.Any()
                            ? answer.Aggregate(string.Empty, (c, t) => c + DeyiKeys.OptionWords[t])
                            : string.Empty);
                    }
                    else
                    {
                        row.Add(string.Empty);
                    }
                }
                dt.Rows.Add(row.ToArray());
            }
            return dt;
        }

        private static List<object> ExportRow(int index, int questionSort, int maxCount, IList<MPictureInfo> pictures)
        {
            var row = new List<object> { string.Concat(questionSort, "题") };
            var answers =
                pictures.Select(
                    p =>
                        (p.SheetAnwers == null || p.SheetAnwers.Count < (index + 1))
                            ? new int[] { }
                            : p.SheetAnwers[index])
                    .ToList();
            for (var i = 0; i < maxCount; i++)
            {
                row.Add(answers.Count(a => a.Contains(i)));
            }
            return row;
        }

        public static DataTable SheetStatistic(IList<MPaperQuestionDto> questions, IList<MPictureInfo> pictures)
        {
            var dt = new DataTable("统计报表");
            var max = 0;
            var qList = questions.Where(q => q.Answers != null && q.Answers.Any()).ToList();
            if (qList.Any())
            {
                max = Math.Max(max, qList.Max(q => q.Answers.Count));
            }
            var details =
                questions.Where(q => q.Details != null && q.Details.Any())
                    .SelectMany(q => q.Details).ToList();
            if (details.Any())
            {
                max = Math.Max(max, details.Max(d => d.Answers == null ? 0 : d.Answers.Count));
            }
            var header = new List<object> { "题号" };
            dt.Columns.Add("question");
            for (var i = 0; i < max; i++)
            {
                dt.Columns.Add(DeyiKeys.OptionWords[i]);
                header.Add(DeyiKeys.OptionWords[i]);
            }
            dt.Rows.Add(header.ToArray());
            var index = 0;
            foreach (var dto in questions)
            {
                List<object> row;
                if (dto.Details != null && dto.Details.Any())
                {
                    foreach (var detail in dto.Details)
                    {
                        row = ExportRow(index++, detail.Sort, max, pictures);
                        dt.Rows.Add(row.ToArray());
                    }
                }
                else
                {
                    row = ExportRow(index++, dto.Sort, max, pictures);
                    dt.Rows.Add(row.ToArray());
                }
            }
            return dt;
        }
    }
}
