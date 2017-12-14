using DayEasy.MarkingTool.BLL.Common;
using DayEasy.Open.Model.Paper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// PrintDetail.xaml 的交互逻辑
    /// </summary>
    public partial class PrintDetail
    {
        public PrintDetail(string paperId)
        {
            InitializeComponent();
            TxtResult.Visibility = Visibility.Hidden;
            PrintArea.Visibility = Visibility.Hidden;
            BindData(paperId);
        }

        private void BindData(string paperId)
        {
            if (string.IsNullOrWhiteSpace(paperId))
            {
                ShowError("暂时不支持该试卷的打印预览");
                return;
            }
            var helper = RestHelper.Instance;
            var result = helper.LoadPaper(paperId);
            if (!result.Status)
            {
                ShowError("暂时不支持该试卷的打印预览");
                return;
            }
            PaperInfo paperDetail = result.Data;

            LblPaperTitle.Content = paperDetail.Title;
            var html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"><base href=\"http://file.dayez.net\"/></head>";
            html += "<body scroll=\"no\" style=\"font-size: 14px; line-height: 1;\">{0}</body></html>";
            //var i = 1;

            paperDetail.Sections.ForEach(section => section.Questions.ForEach(q =>
            {
                var question = helper.LoadQuestion(q.QuestionId).Data;
                var webContent = new WebBrowser();
                webContent.NavigateToString(string.Format(html, question.Body));
                webContent.MinHeight = 20;
                webContent.MinWidth = 600;
                webContent.MaxWidth = 710;
                //webContent.Margin = new Thickness(40, i * 100, 0, 0);
                GrdQuestion.RowDefinitions.Add(new RowDefinition());
                GrdQuestion.Children.Add(webContent);
                webContent.SetValue(Grid.RowProperty, GrdQuestion.RowDefinitions.Count - 1);
                var line = new GridSplitter
                {
                    Background = Brushes.Black,
                    ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ShowsPreview = true,
                    Width = 710,
                    Height = 2
                };
                GrdQuestion.RowDefinitions.Add(new RowDefinition());
                //grdQuestion.RowDefinitions[grdQuestion.RowDefinitions.Count - 1].Height = new GridLength(2);
                GrdQuestion.Children.Add(line);
                line.SetValue(Grid.RowProperty, GrdQuestion.RowDefinitions.Count - 1);
                //i++;
            }));

            //paperDetail.Sections[0].Questions[0].Base.

            PrintArea.Visibility = Visibility.Visible;
        }

        private void ShowError(string message)
        {
            TxtResult.Text = message;
            PrintWrap.HorizontalAlignment = HorizontalAlignment.Center;
            PrintWrap.VerticalAlignment = VerticalAlignment.Center;
            TxtResult.Visibility = Visibility.Visible;
        }
    }
}
