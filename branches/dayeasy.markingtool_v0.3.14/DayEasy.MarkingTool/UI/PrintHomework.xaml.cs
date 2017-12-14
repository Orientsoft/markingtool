using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.Core;
using DayEasy.Open.Model.Paper;
using DayEasy.Open.Model.Question;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ZXing;
using ZXing.Common;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// PrintHomework.xaml 的交互逻辑
    /// </summary>
    public partial class PrintHomework
    {
        public PrintHomework(string paperId)
        {
            InitializeComponent();
            InitializeWindow(paperId);
        }

        public bool IsLoadFormual = true;

        private const string MathLoad = "<script type=\"text/x-mathjax-config\">MathJax.Hub.Config({messageStyle: \"none\",showMathMenu: false, showMathMenuMSIE: false, tex2jax: { inlineMath: [[\"\\\\(\", \"\\\\)\"],[\"\\\\[\",\"\\\\]\"]],displayMath:[[\"$\", \"$\"],[\"$$\",\"$$\"]] }});</script><script type=\"text/javascript\" src=\"/web_formula/MathJax.js?config=TeX-AMS-MML_HTMLorMML\"></script>";
        private const string TableStyle = "#answer-area{ margin: 0 auto;border-collapse: collapse;border-spacing: 0;}#answer-area td{width: 30px;text-align: center; line-height: 15px;height: 15px;font-size: 10px;border:1px solid #000;border-top-width:2px;border-bottom-width: 2px;}";


        private const string QuestionStyle = @".answer-area tr{height:18px}#parper-title{text-align:center;height:80px;line-height:80px;width:525px}.table-sytle table{border-collapse:collapse;border-spacing:0}.table-sytle table td{border:1px solid#000000}#body{width:688px;line-height:18px;min-height:500px;margin:auto;height:auto}hr{clear:both;border-color:#666}#body div.section-normal{cursor:pointer}#body div.section-normal:after{content:'.';height:0;visibility:hidden;display:block;clear:both}ul.question-wraper{margin:0 0 0 20px}ul.question-wraper li._question-wraper{clear:both;list-style:decimal outside}ul.question-wraper li._question-wraper:after{content:'.';height:0;line-height:0;visibility:hidden;display:block;clear:both}ul.sub-wrap{clear:both;padding-left:5px}._objective{float:left;margin-left:40px;list-style:upper-alpha}._subjective,._complex{clear:both;list-style:none;padding-left:0}div.line-wrap{width:688px;list-style:none;position:relative;left:-10%}.question-title{font-size:14px;font-weight:bold;outline:none;color:#333}.bacth-code,id-code{width:80px;height:80px}";


        private void InitializeWindow(string paperId)
        {
            GrdPrint.RowDefinitions.Clear();
            BtnPrint.Visibility = Visibility.Hidden;

            var helper = RestHelper.Instance;
            var result = helper.LoadPaper(paperId);
            
            if (!result.Status)
            {
                ShowError("对不起！暂时不支持该试卷的打印预览。");
                return;
            }
            PaperInfo paperDetail = result.Data;

            InitializeTitle(paperDetail.Title);
            InitializeBody(paperDetail.Sections.ToArray());
        }

        private void ShowError(string message)
        {
            var label = new Label {Content = message, FontSize = 18};

            var wrap = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            wrap.Children.Add(label);

            GrdPrint.Children.Add(wrap);
        }

        private void InitializeTitle(string title)
        {
            #region 生成批次号二维码
            var imgBatchCode = new Image {Width = 80, Height = 80, Margin = new Thickness(46, 20, 0, 0)};

            var options = new EncodingOptions()
            {
                Width = 200,
                Height = 200,
                Margin = 1,
                PureBarcode = true
            };

            var writer = new BarcodeWriter {Format = BarcodeFormat.QR_CODE, Options = options};
            var bmp = writer.Write(Guid.NewGuid().ToString("N") + ",00");
            var ms = new System.IO.MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            var img = new ImageBrush();
            var converter = new ImageSourceConverter();
            img.ImageSource = (ImageSource)converter.ConvertFrom(ms);
            imgBatchCode.Source = img.ImageSource;
            #endregion


            #region 生成标题
            //var lblTitle = new Label();
            //lblTitle.Content = title;
            //lblTitle.FontFamily = new FontFamily("宋体");
            //lblTitle.FontSize = 16;
            //lblTitle.Margin = new Thickness(20, 17, 0, 0);
            var html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"><base href=\"http://www.dayez.net\"/>{_mathLoad}</head><body scroll=\"no\" style=\"font-size: 16px;font-family:'宋体';\">{_title}</body></html>";
            if (IsLoadFormual)
            {
                html = html.Replace("{_mathLoad}", MathLoad);
            }
            var webTitle = new WebBrowser();
            webTitle.NavigateToString(html.Replace("{_title}", title));
            webTitle.Height = 80;
            webTitle.Width = 400;
            //webTitle.MinWidth = 600;
            //webTitle.MaxWidth = 710;
            webTitle.Margin = new Thickness(20, 17, 0, 0);

            #endregion

            var wrap = new WrapPanel();
            wrap.Children.Add(imgBatchCode);
            wrap.Children.Add(webTitle);

            GrdPrint.RowDefinitions.Add(new RowDefinition());
            var rowIndex = GrdPrint.RowDefinitions.Count - 1;
            GrdPrint.RowDefinitions[rowIndex].Height = new GridLength();
            GrdPrint.Children.Add(wrap);
            wrap.SetValue(Grid.RowProperty, rowIndex);

            // TODO: 测试分割线
            //var line = new GridSplitter();
            //line.HorizontalAlignment = HorizontalAlignment.Stretch;
            //line.ShowsPreview = true;
            //line.Height = 2;
            //line.Background = Brushes.Black;
            //line.ResizeBehavior = GridResizeBehavior.PreviousAndNext;

            //grdPrint.RowDefinitions.Add(new RowDefinition());
            //rowIndex = grdPrint.RowDefinitions.Count - 1;
            //grdPrint.RowDefinitions[rowIndex].Height = new GridLength();
            //grdPrint.Children.Add(line);
            //line.SetValue(Grid.RowProperty, rowIndex);
        }

        private void InitializeBody(PaperSectionInfo[] arrSection)
        {
            //var content = new StringBuilder();
            var html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"><base href=\"http://file.dayez.net\"/><style>{_table-sytle}</style></head>";
            html += "<body scroll=\"no\" style=\"font-size: 12px; line-height: 1;\">{_content}</body></html>";
            List<QuestionInfo> arrQuestion = null;
            var addLine = false;
            int rowCount = 0;
            var answerArea = RanderOptions(arrSection, 24, out rowCount);
            if (!string.IsNullOrWhiteSpace(answerArea))
            {
                var htmlVontent = html.Replace("{_content}", answerArea).Replace("{_table-sytle}", TableStyle + QuestionStyle);
                RenderSection(htmlVontent, addLine, 40 * rowCount);
            }

            html = html.Replace("{_table-sytle}", QuestionStyle);

            for (var i = 0; i < arrSection.Length; i++)
            {
                var result = RestHelper.Instance.LoadQuestions(arrSection[i].Questions.Select(t => t.QuestionId));
                arrQuestion = result.Data.ToList();

                for (var j = 0; j < arrQuestion.Count; j++)
                {
                    addLine = !(i == arrSection.Length - 1 && j == arrSection[i].Questions.Count - 1);
                    var content = ReanderQuestion(arrQuestion[j], (j == 0 ? arrSection[i].Title : string.Empty));
                    RenderSection(html.Replace("{_content}", content), addLine, 0);
                }
            }

            BtnPrint.Visibility = Visibility.Visible;
        }

        private void RenderSection(string htmlContent, bool addLine, int height)
        {
            WebBrowser webContent;
            var rowIndex = 0;
            GridSplitter line;

            webContent = new WebBrowser();
            webContent.NavigateToString(htmlContent);
            if (height == 0)
            {
                webContent.MinHeight = 20;
            }
            else
            {
                webContent.Height = height;
            }
            webContent.MinWidth = 600;
            webContent.MaxWidth = 710;
            GrdPrint.RowDefinitions.Add(new RowDefinition());
            rowIndex = GrdPrint.RowDefinitions.Count - 1;
            GrdPrint.RowDefinitions[rowIndex].Height = new GridLength();
            GrdPrint.Children.Add(webContent);
            webContent.SetValue(Grid.RowProperty, rowIndex);

            if (addLine)
            {
                line = new GridSplitter();
                line.HorizontalAlignment = HorizontalAlignment.Stretch;
                line.ShowsPreview = true;
                line.Background = Brushes.Black;
                line.Height = 2;
                line.Margin = new Thickness(0, 1, 0, 0);
                line.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
                GrdPrint.RowDefinitions.Add(new RowDefinition());
                rowIndex = GrdPrint.RowDefinitions.Count - 1;
                GrdPrint.RowDefinitions[rowIndex].Height = new GridLength(3);
                GrdPrint.Children.Add(line);
                line.SetValue(Grid.RowProperty, rowIndex);
            }
        }

        private string RanderOptions(PaperSectionInfo[] detail, int cellCount, out int rowCount)
        {
            var cells = new List<string>();
            rowCount = 0;
            int i = 1, j = 1, detailCount = 0, answerCount = 0;
            foreach (var section in detail)
            {
                foreach (var question in section.Questions.Select(q=>q.Info))
                {
                    detailCount = question.Details.Count();
                    if (detailCount == 1)
                    {
                        if (question.Details[0].IsObjective)
                        {

                            answerCount = question.Details[0].Answers.Count;

                            cells.Add(string.Format("<td style='background-color:#333;'>{0}-{1}</td>", i, j));
                            for (var st = 0; st < answerCount; st++)
                            {
                                cells.Add(string.Format("<td>[{0}]</td>", Convert.ChangeType(65 + st, typeof(char))));
                            }
                            cells.Add("<td></td>");

                        }
                    }
                    j++;
                }
                i++;
            }


            var rows = Math.Ceiling((decimal)cells.Count / (decimal)cellCount);
            if (rows < 1)
            {
                return string.Empty;
            }

            var relCount = rows * cellCount - cells.Count;

            for (int e = 0; e < relCount; e++)
            {
                cells.Add("<td></td>");
            }

            rowCount = (int)rows;

            var sb = new StringBuilder();
            sb.Append("<table id='answer-area'>");
            for (int r = 0; r < rows; r++)
            {
                sb.Append(string.Format("<tr>{0}</tr>", cells.Skip(cellCount * r).Take(cellCount).Aggregate((a, b) => a + b)));
            }

            sb.Append("<table>");
            return sb.ToString();
        }

        private string ReanderQuestion(QuestionInfo ques, string title)
        {
            var sb = new StringBuilder();
            sb.Append("<div id=\"body\" class=\"table-sytle\">");

            sb.Append("<div class=\"section-normal\">");
            if (string.IsNullOrWhiteSpace(title))
            {
                sb.AppendFormat("<div class=\"question-title\">{0}</div>", title);
            }
            sb.AppendFormat(" <ul class=\"question-wraper\">");
            sb.Append("<li class=\"_question-wraper\">");
            sb.Append(ques.Body ?? string.Empty);
            sb.Append("<ul class=\"sub-wrap\">");
            foreach (var de in ques.Details)
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
            sb.Append("</ul></li>");

            sb.Append("</ul></div>");
            sb.Append("</div>");

            return sb.ToString();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new PrintDialog();
                dialog.PageRangeSelection = PageRangeSelection.AllPages;
                IDocumentPaginatorSource dps = new FlowDocument() { };
                //dialog.PrintQueue = new System.Printing.PrintQueue();
                this.Background = Brushes.White;
                PrintArea.Background = Brushes.White;
                GrdPrint.Background = Brushes.White;
                PrintArea.ScrollToTop();

                if (dialog.ShowDialog() == true)
                {
                    dialog.PrintVisual(PrintArea.Content as Visual, "正在打印作业");
                }

                // PrinterSettings ps = new PrinterSettings();
                // PrintDocument printDt = new PrintDocument();
                // ps. 

                //System.IO.Path.GetPathRoot()

            }
            catch (Exception ex)
            {
                WindowsHelper.ShowError(ex.Message);
            }
        }

        //       private void PrintTest()
        //       {
        //           FlowDocument document;
        //           Window window;
        //           CreateWindowToPrint(out document, out window);
        //           PrintDialog printDialog = new PrintDialog();
        //           window.Show();
        //           IDocumentPaginatorSource dps = document;
        //           if (printDialog.ShowDialog() == true)
        //           {
        //               printDialog.PrintDocument(dps.DocumentPaginator, "test");
        //           }
        //       }

        //       private void CreateWindowToPrint(out FlowDocument document, out
        //Window window)
        //       {
        //           document = new FlowDocument() { };
        //           document.Blocks.Add(new BlockUIContainer { Child = printArea });
        //           window = new Window { Content = document, Visibility = System.Windows.Visibility.Hidden };
        //       }
    }
}
