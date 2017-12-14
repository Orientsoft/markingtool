using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.Core;
using DayEasy.Open.Model.Paper;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// PrintPreview.xaml 的交互逻辑
    /// </summary>
    public partial class PrintPreview
    {
        public PrintPreview()
        {
            InitializeComponent();
        }


        public PrintPreview(string paperId)
            : this()
        {
            InitParam(paperId);
            InitiPage();

        }


        public void InitiPage()
        {
            PrintPage.NavigateToString(ShowPage(_paperId, _bacthCode));
            DatePicker.SelectedDate = DateTime.Now.AddDays(1);
        }

        private string _paperId = string.Empty;
        private string _bacthCode = string.Empty;
        private const string BaseUrl = "http://localhost:6188";
        private const int OptionsCount = 24;

        public void InitParam(string id)
        {
            _paperId = id;
            _bacthCode = Guid.NewGuid().ToString("N");
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = RecrodUsage();
                if (!result.Status)
                {
                    var doc = PrintPage.Document as mshtml.IHTMLDocument2;
                    if (doc == null) return;
                    doc.execCommand("Print", false, null);
                    var win = doc.parentWindow;
                    win.execScript("doPrint()", "javascript");//使用JS
                }
                else
                {
                    MessageBox.Show(result.Message);
                }

            }
            catch (Exception ex)
            {
                WindowsHelper.ShowError(ex.Message);
            }
        }

        private string ShowPage(string paperId, string bacthCode)
        {
            var result = RestHelper.Instance.LoadPaper(paperId);
            if (!result.Status)
            {
                return string.Empty;
            }
            PaperInfo paper = result.Data;
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
            sb.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" /><title>试卷详情</title>");
            sb.AppendFormat("<link href=\"{0}/Style/Default/Paper/paperTool.css\" rel=\"stylesheet\" /></head><body>", BaseUrl);
            sb.AppendFormat("<script type=\"text/javascript\" src=\"{0}/Script/Paper/PrintPaper.js\"></script>", BaseUrl);
            //  @if (ViewBag.IsLoadFormula)
            //  {
            //      <script type="text/x-mathjax-config">
            //  MathJax.Hub.Config({
            //   messageStyle: "none",
            //   menuSettings: { zoom: "Hover" },
            //   showMathMenu: false,
            //   showMathMenuMSIE: false,
            //   tex2jax: { inlineMath: [["$", "$"], ["\\(", "\\)"]] }
            //});
            //      </script>
            //      <script type="text/javascript" src="/web_formula/MathJax.js?config=TeX-AMS-MML_HTMLorMML"></script>
            //  }
            sb.Append("<div id=\"paper-detail\" class=\"table-sytle\">");
            sb.AppendFormat("<div class=\"info-area\"><div class=\"bacth-code\"><img src=\"{0}/tool/image/{1}\"/></div>", BaseUrl, bacthCode);
            sb.Append("<div id=\"parper-title\">");
            sb.AppendFormat("<b>{0}</b>", paper.Title.Length > 20 ? paper.Title.Substring(0, 20) : paper.Title);
            sb.Append("</div><div class =\"id-code\"></div></div>");
            sb.AppendFormat("</div><div class =\"id-code\"></div></div>", BaseUrl);

            sb.Append(RanderOptions(paper, OptionsCount));
            sb.Append("<hr />");

            sb.Append("<div id=\"body\" class=\"table-sytle\">");

            //return string.Empty;
            foreach (var item in paper.Sections)
            {
                sb.Append("<div class=\"section-normal\">");
                sb.AppendFormat("<div class=\"question-title\">{0}</div>", item.Title);
                sb.AppendFormat(" <ul class=\"question-wraper\">");

                foreach (var ques in item.Questions)
                {
                    sb.Append("<li class=\"_question-wraper\">");
                    sb.Append(ques.Info.Body ?? string.Empty);
                    sb.Append("<ul class=\"sub-wrap\">");
                    foreach (var de in ques.Info.Details)
                    {
                        if (de.IsObjective)
                        {
                            if (string.IsNullOrWhiteSpace(de.Body))
                            {
                                foreach (var an in de.Answers)
                                {
                                    sb.AppendFormat("<li class=\"_objective\">{0}</li>", an.Body ?? String.Empty);
                                }
                                continue;
                            }
                            sb.AppendFormat("<li class=\"_complex\">{0}", de.Body ?? string.Empty);
                            sb.Append("<ul class=\"sub-wrap\">");
                            foreach (var an in de.Answers)
                            {
                                sb.AppendFormat("  <li class=\"_objective\">{0}</li>", an.Body ?? string.Empty);
                            }
                            sb.Append("</ul></li>");
                        }
                        else
                        {
                            sb.AppendFormat(" <li class=\"_subjective\">{0}</li>", de.Body ?? string.Empty);
                        }
                    }
                    sb.Append("</ul><div class=\"line-wrap\"><hr /></div></li>");
                }
                sb.Append("</ul></div>");
            }
            sb.Append("</div></div></body></html>");

            return sb.ToString();
        }

        private string RanderOptions(PaperInfo detail, int rowCount)
        {
            var cells = new List<string>();

            int i = 1, j = 1, detailCount = 0, answerCount = 0;
            foreach (var section in detail.Sections)
            {

                foreach (var question in section.Questions)
                {
                    detailCount = question.Info.Details.Count();
                    if (detailCount == 1)
                    {
                        var dItem = question.Info.Details[0];
                        if (dItem.IsObjective)
                        {
                            //if (answerCount < rowCount)
                            //{
                            answerCount = dItem.Answers.Count;
                            //    if ((cells.Count % rowCount + answerCount) > rowCount)
                            //    {
                            //        while (cells.Count % rowCount != 0)
                            //        {
                            //            cells.Add("<td></td>");
                            //        }
                            //    }
                            //}
                            cells.Add(string.Format("<td style='background-color:#333;'>{0}-{1}</td>", i, j));
                            for (var st = 0; st < answerCount; st++)
                            {
                                cells.Add(string.Format("<td>[{0}]</td>", Convert.ChangeType(65 + st, typeof(char))));
                            }
                            //if (cells.Count % 27 != 0)
                            //{
                            cells.Add("<td></td>");
                            // }

                        }
                    }
                    j++;
                }
                i++;
            }


            var rows = Math.Ceiling((decimal)cells.Count / rowCount);
            if (rows < 1)
            {
                return string.Empty;
            }

            var relCount = rows * rowCount - cells.Count;

            for (int e = 0; e < relCount; e++)
            {
                cells.Add("<td></td>");
            }

            var sb = new StringBuilder();
            sb.Append("<hr/><table class='answer-area'>");
            for (int r = 0; r < rows; r++)
            {
                sb.Append(string.Format("<tr>{0}</tr>", cells.Skip(rowCount * r).Take(rowCount).Aggregate((a, b) => a + b)));
            }

            sb.Append("<table>");


            return sb.ToString();
        }

        private void printPage_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //    dynamic browser = sender;
            //    //mshtml.HTMLDocument dom = (mshtml.HTMLDocument)browser.Document; //定义HTML
            //    browser.Document.documentElement.style.overflow = "hidden"; //隐藏浏览器的滚动条 
            //    browser.Document.documentElement.style.border = "none";//隐藏边框
        }

        private JsonResultBase RecrodUsage()
        {
            var usage = new
            {
                //AddedBy = UserInfo.Current.ID,
                BatchNo = _bacthCode,
                ExpireTime = DatePicker.SelectedDate ?? DateTime.Now.AddDays(1),
                CalculateScore = false,
                PaperID = _paperId,
                AddedIP = Helper.GetHostIp()[1],
               // Usage = PaperUsageType.Homework
            };

            //PS.ResultPacket result = null;
            //Helper.CallWCF<Paper>(client => result = client.GeneratedPaperUsage(usage));
            //return result;
            return new JsonResultBase("");
        }
    }
}
