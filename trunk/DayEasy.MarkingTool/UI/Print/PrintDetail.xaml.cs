using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.MarkingTool.Core;
using DayEasy.Models.Open.Work;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace DayEasy.MarkingTool.UI.Print
{
    /// <summary> 套打预览页 </summary>
    public partial class PrintDetail
    {
        private int _index;
        private readonly int _count;
        private readonly string _paperTitle;
        private readonly string _groupName;
        private PrintBatchDetail _currentDetail;
        public event NextOrPrevHandler ObtianDirctoryHandler;

        public PrintDetail(string groupName, string paperTitle, int index, int count)
        {
            InitializeComponent();
            _groupName = groupName;
            _paperTitle = paperTitle;
            _index = index;
            _count = count;
            Loaded += (o, args) => BindImage();
            Closing += (o, args) =>
            {
                DialogResult = true;
            };
            KeyDown += (o, args) =>
            {
                switch (args.Key)
                {
                    case Key.Left:
                        _index--;
                        BindImage();
                        break;
                    case Key.Right:
                        _index++;
                        BindImage();
                        break;
                }
            };
        }

        private void BindImage()
        {
            if (ObtianDirctoryHandler == null)
                return;
            ScrollWrap.ScrollToTop();
            if (_index < 0) _index = _count - 1;
            if (_index > _count - 1) _index = 0;
            _currentDetail = ObtianDirctoryHandler(_index);
            ComboPrintType.Visibility = (_currentDetail.PageCount > 1 ? Visibility.Visible : Visibility.Hidden);
            LblPaperTitle.Text = _paperTitle;
            LblClass.Text = string.Format("{3}   {0}({1}/{2})", _currentDetail.StudentName, _index + 1, _count,
                _groupName);
            if (_currentDetail.SectionType > 0)
            {
                BdType.Visibility = Visibility.Visible;
                LblType.Content = _currentDetail.SectionTypeCn;
                BdType.Margin = new Thickness(Helper.LengthCn(_paperTitle) * 10 + 20, 11, 0, -14);
            }
            else
            {
                BdType.Visibility = Visibility.Hidden;
            }
            ImageWrap.Children.Clear();
            ReadWrap.Children.RemoveRange(1, ReadWrap.Children.Count - 1);
            var bgImg = new Image
            {
                Source = new BitmapImage(new Uri(_currentDetail.ImagePath)),
                Width = 780
            };
            ImageWrap.Children.Add(bgImg);
            ReadWrap.Height = _currentDetail.PageCount * (Helper.A4Size.Height ?? 1100);
            if (_currentDetail.SymbolInfos != null && _currentDetail.SymbolInfos.Any())
            {
                foreach (SymbolInfo symbol in _currentDetail.SymbolInfos)
                {
                    AppendImage(symbol.Point, symbol.SymbolType, null, null, symbol.Score);
                }
            }
            if (_currentDetail.CommentInfos != null && _currentDetail.CommentInfos.Any())
            {
                foreach (CommentInfo info in _currentDetail.CommentInfos)
                {
                    AppendImage(info.Point, info.SymbolType, info.Words);
                }
            }
#if DEBUG
            //_currentDetail.ObjectiveErrorInfo = "错题：1，5，8，15，12，45，46";
            //_currentDetail.ObjectiveScore = 20;
#endif
            //客观题信息

            var objectiveError = WindowsHelper.ObjectiveError(_currentDetail.ObjectiveErrorInfo,
                _currentDetail.ObjectiveScore);
            if (!string.IsNullOrWhiteSpace(objectiveError))
            {
                AppendImage(DeyiKeys.ErrorPointF, (byte)SymbolType.Objective, objectiveError,
                    "Lbl-Objective-Error");
            }
            //总分
            AppendImage(DeyiKeys.ScorePointF, (byte)SymbolType.Custom, "得分：" + _currentDetail.Score.ToString("0.#"),
                "Lbl-TotalScore");
        }

        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="point"></param>
        /// <param name="symbolType"></param>
        /// <param name="words"></param>
        /// <param name="styleName"></param>
        /// <param name="score"></param>
        private void AppendImage(PointF point, int symbolType, string words = null, string styleName = null, double score = 0)
        {
            int maxWidth = 0;
            if (symbolType == (int)SymbolType.Objective)
            {
                maxWidth = 150;
            }
            else if (symbolType == (int)SymbolType.Custom)
            {
                maxWidth = (int)Math.Floor(770 - point.X);
                if (maxWidth < 0)
                    maxWidth = 0;
                if (maxWidth > 200)
                    maxWidth = 200;
            }
            var manager = new MarksManager(symbolType, words, maxWidth);
            var control = manager.GetControl();
            Canvas.SetLeft(control, point.X);
            Canvas.SetTop(control, point.Y);
            if (!manager.IsImage && control is Label)
            {
                (control as Label).SetResourceReference(StyleProperty, styleName ?? "Lbl-Words");
            }
            ReadWrap.Children.Add(control);
            //扣分
            if (score <= 0)
                return;
            var scoreLabel = new TextBlock
            {
                Text = "-" + score
            };
            scoreLabel.SetResourceReference(StyleProperty, "Lbl-SubScore");
            var size = manager.GetSize();
            Canvas.SetLeft(scoreLabel, point.X + size.Width);
            Canvas.SetTop(scoreLabel, point.Y - 3);
            ReadWrap.Children.Add(scoreLabel);
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            _index--;
            BindImage();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            _index++;
            BindImage();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDetail == null)
                return;
            bool? isOdd = null;
            var type = Helper.ToInt(((ComboBoxItem)ComboPrintType.SelectedItem).DataContext);
            if (type > 0)
                isOdd = (type == 1);
            var result = WindowsHelper.PrintDetails(new List<PrintBatchDetail> { _currentDetail }, isOdd);
            if (result.Status)
                _currentDetail.IsPrint = true;
        }

        public delegate PrintBatchDetail NextOrPrevHandler(int index);
    }
}
