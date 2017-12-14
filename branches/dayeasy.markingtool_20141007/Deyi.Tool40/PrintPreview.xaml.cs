using Deyi.Tool.Common;
using PS = Deyi.Tool.PaperServiceReference;
using Deyi.Tool.UserServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Deyi.Tool.PaperServiceReference;
using Deyi.Tool.Entity.User;

namespace Deyi.Tool
{
    /// <summary>
    /// PrintPreview.xaml 的交互逻辑
    /// </summary>
    public partial class PrintPreview : Window
    {
        public PrintPreview()
        {
            InitializeComponent();
        }


        public PrintPreview(Guid paperId)
            : this()
        {
            InitParam(paperId);
            InitiPage();

        }


        public void InitiPage()
        {
            printPage.NavigateToString(showPage(_paperID, _bacthCode));
            datePicker.SelectedDate = DateTime.Now.AddDays(1);
        }

        private Guid _paperID = Guid.Empty;
        private string _bacthCode = string.Empty;
        private string _baseUrl = "http://localhost:6188";
        private int _optionsCount = 24;

        public void InitParam(Guid id)
        {
            this._paperID = id;
            this._bacthCode = Guid.NewGuid().ToString("N");
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //var dialog = new PrintDialog();

                //if (dialog.ShowDialog() == true)
                //{
                //    dialog.PrintVisual(panel, "试卷打印");
                //}
                var result = RecrodUsage();
                if (!result.IsError)
                {
                    mshtml.IHTMLDocument2 doc = printPage.Document as mshtml.IHTMLDocument2;
                     doc.execCommand("Print", false, null);
                    mshtml.IHTMLWindow2 win = (mshtml.IHTMLWindow2)doc.parentWindow;
                    win.execScript("doPrint()", "javascript");//使用JS
                }
                else
                {
                    MessageBox.Show(result.Description);
                }

            }
            catch (Exception ex)
            {
                Helper.ShowError(ex.Message);
            }
        }

        private string showPage(Guid paperId, string bacthCode)
        {

            PS.PaperDetail paper = null;
            Helper.CallWCF<PS.Paper>(client => paper = client.GetPaperSection(paperId));
            if (paper == null)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
            sb.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" /><title>试卷详情</title>");
            sb.AppendFormat("<link href=\"{0}/Style/Default/Paper/paperTool.css\" rel=\"stylesheet\" /></head><body>", _baseUrl);
            sb.AppendFormat("<script type=\"text/javascript\" src=\"{0}/Script/Paper/PrintPaper.js\"></script>", _baseUrl);
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
            sb.AppendFormat("<div class=\"info-area\"><div class=\"bacth-code\"><img src=\"{0}/tool/image/{1}\"/></div>", _baseUrl, bacthCode);
            sb.Append("<div id=\"parper-title\">");
            sb.AppendFormat("<b>{0}</b>", paper.Title.Length > 20 ? paper.Title.Substring(0, 20) : paper.Title);
            sb.Append("</div><div class =\"id-code\"></div></div>");
            sb.AppendFormat("</div><div class =\"id-code\"></div></div>", _baseUrl);

            sb.Append(RanderOptions(paper, _optionsCount));
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
                    sb.Append(ques.Base.Body ?? string.Empty);
                    sb.Append("<ul class=\"sub-wrap\">");
                    foreach (var de in ques.DetailList)
                    {
                        if (de.Base.IsObjective)
                        {
                            if (string.IsNullOrWhiteSpace(de.Base.Body))
                            {
                                foreach (var an in de.AnswerList)
                                {
                                    sb.AppendFormat("<li class=\"_objective\">{0}</li>", an.Body ?? String.Empty);
                                }
                                continue;
                            }
                            sb.AppendFormat("<li class=\"_complex\">{0}", de.Base.Body ?? string.Empty);
                            sb.Append("<ul class=\"sub-wrap\">");
                            foreach (var an in de.AnswerList)
                            {
                                sb.AppendFormat("  <li class=\"_objective\">{0}</li>", an.Body ?? string.Empty);
                            }
                            sb.Append("</ul></li>");
                        }
                        else
                        {
                            sb.AppendFormat(" <li class=\"_subjective\">{0}</li>", de.Base.Body ?? string.Empty);
                        }
                    }
                    sb.Append("</ul><div class=\"line-wrap\"><hr /></div></li>");
                }
                sb.Append("</ul></div>");
            }
            sb.Append("</div></div></body></html>");

            return sb.ToString();
        }

        private string RanderOptions(PS.PaperDetail detail, int rowCount)
        {
            var cells = new List<string>();

            int i = 1, j = 1, detailCount = 0, answerCount = 0;
            foreach (var section in detail.Sections)
            {

                foreach (var question in section.Questions)
                {
                    detailCount = question.DetailList.Count();
                    if (detailCount == 1)
                    {
                        if (question.DetailList[0].Base.IsObjective)
                        {
                            //if (answerCount < rowCount)
                            //{
                            answerCount = question.DetailList[0].AnswerList.Count;
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


            var rows = Math.Ceiling((decimal)cells.Count / (decimal)rowCount);
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

        private PS.ResultPacket RecrodUsage()
        {
            PaperUsage usage = new PaperUsage()
            {
                AddedBy = UserInfo.Current.ID,
                BatchNo = _bacthCode,
                ExpireTime = datePicker.SelectedDate ?? DateTime.Now.AddDays(1),
                CalculateScore = false,
                PaperID = _paperID,
                AddedIP = Helper.GetHostIP()[1],
               // Usage = PaperUsageType.Homework
            };

            PS.ResultPacket result = null;
            Helper.CallWCF<Paper>(client => result = client.GeneratedPaperUsage(usage));
            return result;
        }
    }
}
