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
using Deyi.Tool.Common;
using Deyi.Tool.PaperServiceReference;
using System.Windows.Navigation;

namespace Deyi.Tool
{
    /// <summary>
    /// PrintDetail.xaml 的交互逻辑
    /// </summary>
    public partial class PrintDetail : Window
    {
        public PrintDetail(string paperID)
        {
            InitializeComponent();
            txtResult.Visibility = Visibility.Hidden;
            printArea.Visibility = Visibility.Hidden;
            BindData(paperID);
        }

        private void BindData(string paperID)
        {
            Guid id = Guid.Empty;
            if (!Guid.TryParse(paperID, out id) || id == Guid.Empty)
            {
                ShowError("暂时不支持该试卷的打印预览");
                return;
            }

            PaperDetail paperDetail = null;
            Helper.CallWCF<Paper>(service => paperDetail = service.GetPaperSection(id));
            if (paperDetail == null)
            {
                ShowError("暂时不支持该试卷的打印预览");
                return;
            }

            lblPaperTitle.Content = paperDetail.Title;
            WebBrowser webContent;
            var content = new StringBuilder();
            var html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"><base href=\"http://file.dayez.net\"/></head>";
            html += "<body scroll=\"no\" style=\"font-size: 14px; line-height: 1;\">{0}</body></html>";
            //var i = 1;
            GridSplitter line;

            paperDetail.Sections.ForEach(section =>
                   {
                       section.Questions.ForEach(question =>
                           {
                               webContent = new WebBrowser();
                               webContent.NavigateToString(string.Format(html, question.Base.Body));
                               webContent.MinHeight = 20;
                               webContent.MinWidth = 600;
                               webContent.MaxWidth = 710;
                               //webContent.Margin = new Thickness(40, i * 100, 0, 0);
                               grdQuestion.RowDefinitions.Add(new RowDefinition());
                               grdQuestion.Children.Add(webContent);
                               webContent.SetValue(Grid.RowProperty, grdQuestion.RowDefinitions.Count - 1);
                               line = new GridSplitter();
                               line.Background = Brushes.Black;
                               line.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
                               line.HorizontalAlignment = HorizontalAlignment.Stretch;
                               line.ShowsPreview = true;
                               line.Width = 710;
                               line.Height = 2;
                               grdQuestion.RowDefinitions.Add(new RowDefinition());
                               //grdQuestion.RowDefinitions[grdQuestion.RowDefinitions.Count - 1].Height = new GridLength(2);
                               grdQuestion.Children.Add(line);
                               line.SetValue(Grid.RowProperty, grdQuestion.RowDefinitions.Count - 1);
                               //i++;
                           });
                   });

            //paperDetail.Sections[0].Questions[0].Base.

            printArea.Visibility = Visibility.Visible;
        }

        private void ShowError(string message)
        {
            txtResult.Text = message;
            printWrap.HorizontalAlignment = HorizontalAlignment.Center;
            printWrap.VerticalAlignment = VerticalAlignment.Center;
            txtResult.Visibility = Visibility.Visible;
        }
    }
}
