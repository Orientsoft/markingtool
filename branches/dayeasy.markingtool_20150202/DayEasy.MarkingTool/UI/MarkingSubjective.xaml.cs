using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.MarkingTool.Core;
using DayEasy.Open.Model.Enum;
using DayEasy.Open.Model.Marking;
using DayEasy.Open.Model.Paper;
using DayEasy.Open.Model.Question;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// 主观题阅卷 的交互逻辑
    /// </summary>
    public partial class MarkingSubjective
    {
        private readonly bool _hasScore = true;

        /// <summary>
        /// 试卷信息
        /// </summary>
        private PaperInfo _currentBatch;

        /// <summary>
        /// 批阅方式
        /// </summary>
        private readonly PaperKind _kind = PaperKind.Paper;

        /// <summary>
        /// 当前操作
        /// </summary>
        private MarkingOperate _currentOperate = MarkingOperate.Pointer;

        private CursorManager _cursorManager;

        /// <summary>
        /// 当前阅卷结果
        /// </summary>
        private MarkingResult _currentMarkingResult;

        /// <summary>
        ///所有阅卷结果 
        /// </summary>
        private readonly List<MarkingResult> _markingResultList;

        private const string PlaceHolder = "请在这里输入批注...";
        private const string ScoreComboPrefix = "combo{0}{1}";

        private PaperMarkedInfo _currentPaperMarkedInfo;

        private List<QuestionInfo> Question
        {
            get
            {
                if (_currentBatch == null)
                {
                    return null;
                }
                switch (_kind)
                {
                    case PaperKind.AnswerCard:
                        return
                            _currentBatch.Sections.SelectMany(s => s.Questions.Select(q => q.Info))
                                .Where(q => !q.IsObjective).ToList();
                    default:
                        return _currentBatch.Sections.SelectMany(s => s.Questions.Select(q => q.Info)).ToList();
                }
            }
        }

        //private bool HasObjective
        //{
        //    get
        //    {
        //        if (Question != null)
        //        {
        //            return
        //                _currentBatch.Sections.SelectMany(s => s.Questions.Select(q => q.Info))
        //                    .Any(Helper.IsObjective);
        //        }
        //        return false;
        //    }
        //}

        public event NextOrPrevHandler ObtianDirctoryHandler;

        public MarkingSubjective(PaperMarkedInfo paperMarkedInfo, PaperInfo batch,
            ref List<MarkingResult> markedResults, PaperKind kind)
        {
            InitializeComponent();
            _markingResultList = markedResults ?? new List<MarkingResult>();
            _currentPaperMarkedInfo = paperMarkedInfo;
            _currentBatch = batch;
            _kind = kind;
            _hasScore = (batch.Sections.SelectMany(s => s.Questions).Sum(q => q.Score) > 0);
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
        }

        /// <summary>
        /// 按键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                ChangePaper(false, "已经是第一张了");
            }
            else if (e.Key == Key.Right)
            {
                ChangePaper(true, "已经是最后一张了");
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control) return;
            var operate = MarkingOperate.Pointer;
            switch (e.Key)
            {
                case Key.D:
                    operate = MarkingOperate.Pointer;
                    break;
                case Key.R:
                    operate = MarkingOperate.Hook;
                    break;
                case Key.H:
                    operate = MarkingOperate.HalfHook;
                    break;
                case Key.W:
                    operate = MarkingOperate.Fork;
                    break;
                case Key.E:
                    operate = MarkingOperate.Erase;
                    break;
                case Key.Q:
                    operate = MarkingOperate.Comment;
                    break;
            }
            if (operate != _currentOperate)
            {
                _currentOperate = operate;
                _cursorManager = new CursorManager(operate);
                SetCursor();
            }
        }

        /// <summary>
        /// 初始化试卷
        /// </summary>
        /// <param name="marked"></param>
        /// <param name="batch"></param>
        private void Init(PaperMarkedInfo marked, PaperInfo batch)
        {
            if (!marked.IsSuccess)
            {
                WindowsHelper.ShowQuestion("该试卷客观题阅卷失败");
                return;
            }
            _currentPaperMarkedInfo = marked;

            if (batch != null)
            {
                _currentBatch = batch;
            }
            var tempResult = _markingResultList.Find(m => m.Id == marked.MarkedResultId);

            if (tempResult != null)
            {
                _currentMarkingResult = tempResult;
                if (InitPicture(tempResult))
                {
                    ReappearMarked(_currentMarkingResult);
                }
                else
                {
                    _currentMarkingResult.IsFinished = false;
                }
                Curpic.Content = string.Format("当前学生[{0}/{1}]：{2}", _markingResultList.IndexOf(_currentMarkingResult) + 1,
                    _markingResultList.Count, marked.StudentName);
            }
            else
            {
                WindowsHelper.ShowQuestion("该试卷不能被批阅...");
            }
        }


        /// <summary>
        /// 初始化图片
        /// </summary>
        /// <param name="currentMarkingResult"></param>
        /// <returns></returns>
        private bool InitPicture(MarkingResult currentMarkingResult)
        {
            if (Question == null || Question.Count < 1)
                return false;
            if (string.IsNullOrWhiteSpace(_currentPaperMarkedInfo.PaperName))
                return false;

            var basePath = Path.Combine(DeyiKeys.SavePath, _currentPaperMarkedInfo.PaperName);
            var paperImages =
                Helper.GetAllImagePath(basePath)
                    .OrderBy(t => Convert.ToInt32(Path.GetFileNameWithoutExtension(t)))
                    .ToList();
            double totalHeight = 0d;
            const double autoMarkedX = 80d;
            ImageWrap.Children.Clear();
            ReadWrap.Children.RemoveRange(1, ReadWrap.Children.Count - 1);

            //如果有客观题，则会有答题卡图片，故i-2
            const int skip = 2; //(HasObjective ? 2 : 1);

            for (int i = 0; i < paperImages.Count; i++)
            {
                string itemPath = paperImages[i],
                    itemName = Path.GetFileName(itemPath);
                if (string.IsNullOrWhiteSpace(itemName))
                    continue;
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(itemPath)),
                    Width = DeyiKeys.PaperWidth,
                };
                var marginTop = totalHeight;
                totalHeight += img.Source.Height;
                ImageWrap.Children.Add(img);
                if (i < skip) continue;
                var questionIndex = i - skip;
                if (questionIndex < 0 || questionIndex >= Question.Count) continue;
                var temp = Question[questionIndex];

                img.Tag = string.Format("{0}|{1}", temp.QuestionId, itemName);
                var details = currentMarkingResult.Details.Where(d => d.QuestionId == temp.QuestionId).ToList();
                var tempDtail = details.FirstOrDefault();
                if (tempDtail == null) continue;
                if (!temp.IsObjective)
                {
                    tempDtail.StudentAnswerSnapshot = itemName;
                    tempDtail.MarginTop = (int)Math.Ceiling(marginTop);
                    var tempDir = Path.GetDirectoryName(itemPath);
                    if (string.IsNullOrWhiteSpace(tempDir))
                        continue;
                    //复制主观题到压缩文件
                    File.Copy(itemPath, Path.Combine(tempDir, DeyiKeys.CompressName, itemName),
                        true);
                    if (!_hasScore)
                        continue;
                    var top = totalHeight - img.Source.Height / 2 - 10;
                    //主观题显示分数
                    var scoreCombo = new ComboBox
                    {
                        Tag = tempDtail.QuestionId,
                        DataContext = temp,
                        Padding = new Thickness(3, 5, 3, 5)
                    };
                    var max = (int)Math.Ceiling(temp.Score);
                    var scores = Enumerable.Range(0, max + 1);
                    var autoChange = !_currentPaperMarkedInfo.IsMarked;
                    var dataSource = scores.Select(score => string.Format("{0}分", score)).ToList();
                    scoreCombo.ItemsSource = dataSource;
                    var currentScore = details.Sum(t => t.Score);
                    if (currentScore > max)
                    {
                        currentScore = max;
                        autoChange = true;
                    }
                    scoreCombo.SelectedValue = string.Format("{0}分",
                        (int)Math.Ceiling(_currentPaperMarkedInfo.IsMarked
                            ? currentScore
                            : temp.Score));

                    var name = string.Format(ScoreComboPrefix, _currentPaperMarkedInfo.PaperName, tempDtail.QuestionId);
                    if (ReadWrap.FindName(name) != null)
                        ReadWrap.UnregisterName(name);
                    ReadWrap.RegisterName(name, scoreCombo);
                    Canvas.SetLeft(scoreCombo, 5);
                    Canvas.SetTop(scoreCombo, top);
                    scoreCombo.SelectionChanged += scoreCombo_SelectionChanged;
                    if (autoChange)
                        scoreCombo_SelectionChanged(scoreCombo, null);
                    ReadWrap.Children.Add(scoreCombo);
                    continue;
                }
                if (tempDtail.IsCorrect || _kind != PaperKind.Paper)
                    continue;

                var autoMarkedY = (totalHeight - img.Source.Height +
                                   (img.Source.Height > 100 ? 50 : img.Source.Height / 2));

                if (tempDtail.MarkingSymbols == null)
                {
                    tempDtail.MarkingSymbols = new List<MarkingSymbolInfo>();
                }

                if (tempDtail.MarkingSymbols.Count == 0)
                {
                    tempDtail.MarkingSymbols.Add(new MarkingSymbolInfo
                    {
                        SymbolType = MarkingSymbolType.Wrong,
                        Position = new System.Drawing.Point((int)autoMarkedX, (int)autoMarkedY)
                    });
                }
            }
            var autoComment = currentMarkingResult.Details.FirstOrDefault();
            if (autoComment != null && autoComment.TeacherComments != null && autoComment.TeacherComments.Count > 0)
            {
                //错题信息
                autoComment.TeacherComments[0].Position = new System.Drawing.Point((int)autoMarkedX + 155, 15);
            }
            ReadWrap.Height = totalHeight;
            if (!currentMarkingResult.IsFinished)
            {
                currentMarkingResult.IsFinished = true;
            }
            return true;
        }

        /// <summary>
        /// 分数下拉变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scoreCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var txt = sender as ComboBox;
            if (txt == null || txt.SelectedValue == null) return;
            var item = txt.DataContext as QuestionInfo;
            if (item == null) return;
            var id = txt.Tag.ToString();
            var details = _currentMarkingResult.Details.Where(t => t.QuestionId == id).ToList();
            if (!details.Any()) return;
            var score = Helper.ToDecimal(txt.SelectedValue.ToString().TrimEnd('分'), -1);
            if (item.Details != null && item.Details.Any())
            {
                //有小问
                foreach (var detailInfo in item.Details)
                {
                    var detail = details.FirstOrDefault(t => t.SmallQuestionId == detailInfo.DetailId);
                    if (detail == null)
                        continue;
                    var dScore = detailInfo.Score;
                    if (item.Details.Count == 1)
                        dScore = item.Score;
                    //一个一个的往下匹配
                    if (score >= dScore)
                    {
                        detail.IsCorrect = true;
                        detail.Score = dScore;
                        score -= detail.Score;
                    }
                    else
                    {
                        detail.IsCorrect = false;
                        detail.Score = score;
                        score = 0;
                    }
                }
            }
            else
            {
                var detail = details[0];
                detail.Score = score;
                detail.IsCorrect = score == item.Score;
            }
            _currentMarkingResult.TotalScore = _currentMarkingResult.Details.Sum(d => d.Score);
            ShowTotalScore(_currentMarkingResult.TotalScore);
        }

        /// <summary>
        /// 显示总分数
        /// </summary>
        /// <param name="score"></param>
        private void ShowTotalScore(decimal score)
        {
            TotalScore.Content = string.Format("总得分：{0}分", score);
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            ChangePaper(false, "已经是第一张了");
        }

        /// <summary>
        /// 切换试卷
        /// </summary>
        /// <param name="isNext"></param>
        /// <param name="msg"></param>
        private void ChangePaper(bool isNext, string msg)
        {
            if (ObtianDirctoryHandler != null)
            {
                object[] objs = ObtianDirctoryHandler(isNext, _currentPaperMarkedInfo.MarkedResultId);
                var temp = objs[0] as PaperMarkedInfo;
                if (temp != null)
                {
                    Init(temp, objs[1] as PaperInfo);
                }
                else
                {
                    WindowsHelper.ShowMsg(msg);
                }
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            ChangePaper(true, "已经是最后一张了");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init(_currentPaperMarkedInfo, _currentBatch);
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            var tag = string.Empty;
            if (sender is Button)
            {
                tag = (sender as Button).Tag.ToString();
            }
            else if (sender is MenuItem)
            {
                tag = (sender as MenuItem).Tag.ToString();
            }
            if (string.IsNullOrWhiteSpace(tag))
                return;

            byte type;
            if (!byte.TryParse(tag, out type))
                return;
            if (type > 100)
                _currentOperate = MarkingOperate.Emotion;
            else
                _currentOperate = (MarkingOperate)type;
            _cursorManager = new CursorManager(_currentOperate, type);
            SetCursor();
        }

        /// <summary>
        /// 阅卷点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void readWrap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MarkingDetail detail;
            switch (_currentOperate)
            {
                case MarkingOperate.Pointer:
                case MarkingOperate.Erase:
                    break;
                case MarkingOperate.Comment:
                case MarkingOperate.Emotion:
                    detail = RecordDetail(Mouse.DirectlyOver as UIElement);
                    if (detail != null)
                    {
                        Comment(Mouse.GetPosition(ReadWrap), detail);
                    }
                    break;
                default:
                    detail = RecordDetail(Mouse.DirectlyOver as UIElement);
                    if (detail != null)
                    {
                        Mark(Mouse.GetPosition(ReadWrap), detail);
                    }
                    break;
            }
        }

        /// <summary>
        /// 阅卷操作
        /// </summary>
        /// <param name="p"></param>
        /// <param name="detail"></param>
        private void Mark(Point p, MarkingDetail detail)
        {
            if (detail == null)
                return;
            var symbolType = _cursorManager.GetMarkingSymbolType();
            var offset = _cursorManager.OffsetPoint();
            var mark = new MarkingSymbolInfo
            {
                Position = new System.Drawing.Point((int)(p.X - offset.X), (int)(p.Y - offset.Y)),
                //半勾也算错
                SymbolType = symbolType,
            };
            detail.MarkingSymbols.Add(mark);

            AppendImage(detail.Id, mark, mark.Position);
            //全错默认归零
            if (symbolType == MarkingSymbolType.Wrong &&
                detail.MarkingSymbols.All(t => t.SymbolType == MarkingSymbolType.Wrong))
            {
                var name = string.Format(ScoreComboPrefix, _currentPaperMarkedInfo.PaperName, detail.QuestionId);
                var cob = ReadWrap.FindName(name) as ComboBox;
                if (cob != null)
                {
                    cob.SelectedValue = "0分";
                    scoreCombo_SelectionChanged(cob, null);
                }
            }
        }

        /// <summary>
        /// 阅卷操作
        /// </summary>
        /// <param name="currentQ"></param>
        /// <returns></returns>
        private MarkingDetail RecordDetail(UIElement currentQ)
        {
            var qImg = WindowsHelper.FindVisualParent<Image>(currentQ);
            if (qImg == null || qImg.Tag == null) return null;
            var temp = qImg.Tag.ToString();
            if (string.IsNullOrWhiteSpace(temp)) return null;

            var qRef = temp.Split('|');
            if (qRef.Length <= 1) return null;
            string qId = qRef[0];
            QuestionInfo curQuest = Question.FirstOrDefault(q => q.QuestionId == qId);
            //客观题不支持修改
            if (curQuest == null || Helper.IsObjective(curQuest)) return null;
            return _currentMarkingResult.Details.FirstOrDefault(m => m.QuestionId == qId);
        }

        /// <summary>
        /// 添加备注
        /// </summary>
        /// <param name="p"></param>
        /// <param name="detail"></param>
        private void Comment(Point p, MarkingDetail detail)
        {
            var comment = new TeacherCommentInfo
            {
                EmotionType = _cursorManager.EmotionType
            };
            if (_currentOperate == MarkingOperate.Comment)
            {
                comment.CommentText = PlaceHolder;
                comment.Position = new System.Drawing.Point((int) p.X, (int) p.Y);
            }
            else
            {
                var offset = _cursorManager.OffsetPoint();
                comment.Position = new System.Drawing.Point((int) (p.X - offset.X), (int) (p.Y - offset.Y));
            }
            detail.TeacherComments.Add(comment);
            if (comment.EmotionType ==0)
                AppendTextBox(detail.Id, comment, comment.Position, true);
            else
                AppendImage(detail.Id, comment, comment.Position);
        }

        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="context"></param>
        /// <param name="point"></param>
        /// <param name="manager"></param>
        private void AppendImage(string tag, object context, System.Drawing.Point point,CursorManager manager=null)
        {
            manager = manager ?? _cursorManager;
            var bitmap = manager.GetBitmapImage();
            var size = manager.GetImageSize();
            var img = new Image
            {
                Source = bitmap,
                Width = size.Width,
                Height = size.Height,
                DataContext = context,
                Tag = tag
            };

            Canvas.SetLeft(img, point.X);
            Canvas.SetTop(img, point.Y);
            img.MouseDown += Erase;
            ReadWrap.Children.Add(img);
        }

        /// <summary>
        /// 添加Textbox
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="context"></param>
        /// <param name="point"></param>
        /// <param name="isOperate"></param>
        /// <param name="isErrorBox"></param>
        private void AppendTextBox(string tag, object context, System.Drawing.Point point, bool isOperate = false,
            bool isErrorBox = false)
        {
            var txt = new TextBox();
            Canvas.SetLeft(txt, point.X);
            Canvas.SetTop(txt, point.Y);
            txt.Foreground = Brushes.Red;

            txt.AllowDrop = true;
            txt.DataContext = context;
            var binding = new Binding("CommentText");
            txt.SetBinding(TextBox.TextProperty, binding);

            if (isErrorBox)
            {
                txt.LostFocus += ErrorCommentLostFocus;
            }
            else
            {
                txt.PreviewMouseUp += Erase;
                txt.GotFocus += txt_GotFocus;
                txt.TextChanged += txt_TextChanged;
                txt.Tag = tag;
            }
            ReadWrap.Children.Add(txt);
            if (isOperate)
            {
                _currentOperate = MarkingOperate.Pointer;
                _cursorManager = new CursorManager(_currentOperate);
                SetCursor();
            }
        }

        /// <summary>
        /// 备注改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;
            var info = txt.DataContext as TeacherCommentInfo;
            if (info != null)
            {
                info.CommentText = txt.Text;
            }
        }

        /// <summary>
        /// 备注输入框失去焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;
            if (txt.Text == PlaceHolder)
            {
                txt.Text = string.Empty;
            }
        }

        /// <summary>
        /// 橡皮擦
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Erase(object sender, MouseEventArgs e)
        {
            if (_currentOperate != MarkingOperate.Erase) return;
            string id;
            if (sender is TextBox)
            {
                var txt = sender as TextBox;
                id = txt.Tag.ToString();
                _currentMarkingResult.Details.Find(d => d.Id == id)
                    .TeacherComments.Remove(txt.DataContext as TeacherCommentInfo);
                ReadWrap.Children.Remove(sender as UIElement);
                return;
            }
            if (!(sender is Image)) return;
            var img = sender as Image;
            id = img.Tag.ToString();
            var markDetail = _currentMarkingResult.Details.FirstOrDefault(t => t.Id == id);
            if (markDetail == null) return;
            if (img.DataContext is TeacherCommentInfo)
            {
                markDetail.TeacherComments.Remove(img.DataContext as TeacherCommentInfo);
                ReadWrap.Children.Remove(sender as UIElement);
                return;
            }
            var current = Question.FirstOrDefault(t => t.QuestionId == markDetail.QuestionId);
            if (current == null || Helper.IsObjective(current))
                return;
            markDetail.MarkingSymbols.Remove(img.DataContext as MarkingSymbolInfo);
            ReadWrap.Children.Remove(sender as UIElement);
            if (markDetail.MarkingSymbols == null || !markDetail.MarkingSymbols.Any()
                || markDetail.MarkingSymbols.All(t => t.SymbolType == MarkingSymbolType.Right))
            {
                var name = string.Format(ScoreComboPrefix, _currentPaperMarkedInfo.PaperName, markDetail.QuestionId);
                var cob = ReadWrap.FindName(name) as ComboBox;
                if (cob != null)
                {
                    cob.SelectedValue = (int)Math.Ceiling(current.Score) + "分";
                    scoreCombo_SelectionChanged(cob, null);
                }
            }
        }

        private void readWrap_MouseMove(object sender, MouseEventArgs e)
        {
            //var p = Mouse.GetPosition(readWrap);
            //Canvas.SetLeft(_currentCursor, p.X);
            //Canvas.SetLeft(_currentCursor, p.Y);
            //if (!readWrap.Children.Contains(_currentCursor))
            //{
            //    readWrap.Children.Add(_currentCursor);
            //}
        }

        private void readWrap_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void readWrap_MouseEnter(object sender, MouseEventArgs e)
        {
            SetCursor();
        }

        /// <summary>
        /// 设置鼠标
        /// </summary>
        private void SetCursor()
        {
            if (_cursorManager != null)
                Cursor = _cursorManager.GetCursor();
        }

        /// <summary>
        /// 加载批阅图片
        /// </summary>
        /// <param name="result"></param>
        /// <param name="loadComments"></param>
        private void ReappearMarked(MarkingResult result, bool loadComments = true)
        {
            var i = 0;
            foreach (var detail in result.Details)
            {
                if (detail.MarkingSymbols != null)
                {
                    foreach (var mark in detail.MarkingSymbols)
                    {
                        AppendImage(detail.Id, mark, mark.Position, new CursorManager(mark.SymbolType));
                    }
                }
                if (detail.TeacherComments != null && loadComments)
                {
                    foreach (var comment in detail.TeacherComments)
                    {
                        if (comment.EmotionType == 0)
                            AppendTextBox(detail.Id, comment, comment.Position, false, i == 0);
                        else
                            AppendImage(detail.Id, comment, comment.Position, new CursorManager(comment.EmotionType));
                        i++;
                    }
                }
            }
            result.TotalScore = result.Details.Sum(d => d.Score);
            ShowTotalScore(result.TotalScore);
        }

        /// <summary>
        /// 错题失去焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorCommentLostFocus(object sender, RoutedEventArgs e)
        {
            if (!WindowsHelper.ShowQuestion("是否重置客观题阅卷结果？"))
                return;
            var txt = sender as TextBox;
            if (txt == null)
                return;
            var word = txt.Text;
            var qList = _currentBatch.Sections.SelectMany(s => s.Questions.Select(q => q.Info))
                .Where(q => q.IsObjective).ToList();
            var errorCount = Helper.ResetObjectiveResult(word, _currentMarkingResult, qList);
            _currentPaperMarkedInfo.ErorrCount = errorCount;
            _currentMarkingResult.TotalScore = _currentMarkingResult.Details.Sum(d => d.Score);
            ShowTotalScore(_currentMarkingResult.TotalScore);
            if (errorCount == 0)
                word = "客观题全对";
            var comment = txt.DataContext as TeacherCommentInfo;
            if (comment != null)
                comment.CommentText = word;
            if (_kind == PaperKind.Paper)
            {
                var eleList = ReadWrap.Children;
                foreach (UIElement child in eleList)
                {
                    if (child is Image)
                        ReadWrap.Children.Remove(child);
                }
                ReappearMarked(_currentMarkingResult, false);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ReadWrap.Children.Clear();
            ImageWrap.Children.Clear();
            DialogResult = true;
            _currentMarkingResult = null;
            _currentPaperMarkedInfo = null;
            base.OnClosing(e);
        }
    }

    /// <summary>
    /// 图片切换
    /// </summary>
    /// <param name="isNext"></param>
    /// <param name="resultId"></param>
    /// <returns></returns>
    public delegate object[] NextOrPrevHandler(bool isNext, string resultId);
}
