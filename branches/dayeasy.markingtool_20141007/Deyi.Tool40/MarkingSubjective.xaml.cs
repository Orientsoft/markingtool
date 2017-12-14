using Deyi.Tool.Common;
using Deyi.Tool.Entity.Paper;
using Deyi.Tool.PaperServiceReference;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Deyi.Tool
{
    /// <summary>
    /// MarkingSubjective.xaml 的交互逻辑
    /// </summary>
    public partial class MarkingSubjective : Window
    {
        public MarkingSubjective(PaperMarkedInfo marked, PaperBatchDetail batch, ref List<MarkingResult> markedResults, PaperKind kind)
        {
            InitializeComponent();
            _markingResultList = markedResults ?? new List<MarkingResult>();
            _currentMarked = marked;
            _currentBatch = batch;
            _kind = kind;
            // Init(marked, batch);
        }


        private PaperKind _kind = PaperKind.Paper;

        Operate _currentOperate = Operate.Pointer;

        private string _markedKey = string.Empty;

        void Init(PaperMarkedInfo marked, PaperBatchDetail batch)
        {
            if (!marked.IsSuccess)
            {
                Helper.ShowQuestion("该试卷客观题阅卷失败");
                return;
            }
            _currentMarked = marked;

            if (batch != null)
            {
                _currentBatch = batch;
            }

            //if (isFrist)
            //{
            //    //if (objectiveMarkingRestul != null)
            //    //{
            //    //    _markingResultList.AddRange(objectiveMarkingRestul);
            //    //}
            //    Helper.LoadMarkedPaper(_markingResultList, PaperMarkedInfo.MarkedID);
            //}
            curpic.Content = string.Format("当前批阅图片名：{0}", marked.PaperName);
            var tempResult = _markingResultList.Find(m => m.ID == marked.MarkedResultID);//(m => m.Batch == marked.BacthCode && marked.IDNo == m.StudentIdentity);

            if (tempResult != null)
            {
                _markingResult = tempResult;
                if (InitPicture(marked.PaperName, tempResult))
                {
                    ReappearMarked(_markingResult);
                }
                else
                {
                    _markingResult.IsFinished = false;
                }
            }
            else
            {
                Helper.ShowQuestion("该试卷不能被批阅...");
                // this.Close();
            }


        }

        /// <summary>
        /// 当前阅卷结果
        /// </summary>
        public MarkingResult _markingResult = null;

        /// <summary>
        ///所有阅卷结果 
        /// </summary>
        public List<MarkingResult> _markingResultList;

        // public List<QuestionEntity> questions = null;

        //static MarkingSubjective()
        //{
        //    _markingResultList = new List<MarkingResult>();
        //}

        string placeHolder = "请在这里输入批注...";

        private PaperMarkedInfo _currentMarked = null;

        private List<QuestionEntity> _question
        {

            get
            {
                if (_currentBatch == null)
                {
                    return null;
                }
                else
                {
                    switch (_kind)
                    {
                        case PaperKind.Paper: return _currentBatch.Sections.SelectMany(s => s.Questions).ToList();
                        case PaperKind.AnswerCard: return _currentBatch.Sections.SelectMany(s => s.Questions).Where(q => !Helper.IsObjective(q.Base.TypeID, q.Base.SubjectID)).ToList();
                        default:
                            return _currentBatch.Sections.SelectMany(s => s.Questions).ToList();
                    }
                }
            }
        }

        private PaperBatchDetail _currentBatch = null;

        private bool hasObjective
        {
            get
            {
                if (_question != null)
                {
                    return _currentBatch.Sections.SelectMany(s => s.Questions).Any(q => Helper.IsObjective(q.Base.TypeID, q.Base.SubjectID));
                }
                return false;
            }
        }

        public event NextOrPrevHandler ObtianDirctoryHandler;

        private bool InitPicture(string dirc, MarkingResult currentMarkingResult)
        {
            if (_question == null || _question.Count < 1)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(dirc))
            {
                return false;
            }
            var basePath = Path.Combine(DeyiKeys.SavePath, dirc);
            var paperImages = Helper.GetAllImagePath(basePath).OrderBy(t => t).ToList();
            MarkingDetail tempDtail;
            int codeImageHeight = 0;
            Image img;
            double totalHeight = 0d, autoMarkedX = 80d, autoMarkedY = 0d;
            ImageWrap.Children.Clear();
            readWrap.Children.RemoveRange(1, readWrap.Children.Count - 1);
            QuestionEntity temp;
            int skip = (hasObjective ? 2 : 1);

            for (int i = 0; i < paperImages.Count; i++)
            {
                img = new Image
                {
                    Source = new BitmapImage(new Uri(Path.Combine(basePath, string.Format("{0}.jpg", i)))),
                    Width = 780
                };
                totalHeight += img.Source.Height;
                if (i == 0) // 保存第一张图片的高度
                {
                    codeImageHeight = (int) img.Source.Height;
                }
                ImageWrap.Children.Add(img);
                //如果有客观题，则会有答题卡图片，故i-2
                if (i < skip) continue;
                int index = i - skip;
                temp = _question[index < _question.Count ? index : (_question.Count - 1)];

                img.Tag = string.Format("{0}|{1}|{2}", temp.Base.ID, temp.Base.EntireVersionID,
                    Path.GetFileName(paperImages[i]));
                tempDtail =
                    currentMarkingResult.Detail.Find(
                        od => od.QuestionID == temp.Base.ID && od.QuestionVersion == temp.Base.EntireVersionID);
                var isObjective = Helper.IsObjective(temp.Base.TypeID, temp.Base.SubjectID);
                if (!isObjective)
                {
                    tempDtail.StudentAnswerSnapshot = Path.GetFileName(paperImages[i]);
                    continue;
                }
                if (tempDtail.IsCorrect || _kind != PaperKind.Paper)
                    continue;

                autoMarkedY = (totalHeight - img.Source.Height +
                               (img.Source.Height > 100 ? 50 : img.Source.Height/2));

                if (tempDtail.MarkingSymbols == null)
                {
                    tempDtail.MarkingSymbols = new List<MarkingSymbolInfo>();
                }

                if (tempDtail.MarkingSymbols.Count == 0)
                {
                    tempDtail.MarkingSymbols.Add(new MarkingSymbolInfo
                    {
                        SymbolType = MarkingSymbolType.Wrong,
                        Position = new System.Drawing.Point((int) autoMarkedX, (int) autoMarkedY)
                    });
                }

                #region old

                //if (hasObjective)
                //{
                //    if (i > 1)
                //    {
                //        temp = _question[(i - 2) < _question.Count ? (i - 2) : (_question.Count - 1)];
                //        img.Tag = string.Format("{0}|{1}|{2}", temp.Base.ID, temp.Base.EntireVersionID, Path.GetFileName(paperImages[i]));
                //        // currentMarkingResult.Detail.Find(tem);
                //        tempDtail = currentMarkingResult.Detail.Find(od => od.QuestionID == temp.Base.ID && od.QuestionVersion == temp.Base.EntireVersionID);
                //        tempDtail.StudentAnswerSnapshot = Path.GetFileName(paperImages[i - 2]);

                //        if (Helper.IsObjective(temp.Base.TypeID, temp.Base.SubjectID))
                //        {
                //            if (!tempDtail.IsCorrect && _kind == PaperKind.Paper)
                //            {
                //                autoMarkedY = (totalHeight - img.Source.Height + (img.Source.Height > 100 ? 50 : img.Source.Height / 2));

                //                if (tempDtail.MarkingSymbols == null)
                //                {
                //                    tempDtail.MarkingSymbols = new List<MarkingSymbolInfo>();
                //                }

                //                if (tempDtail.MarkingSymbols.Count == 0)
                //                {
                //                    tempDtail.MarkingSymbols.Add(new MarkingSymbolInfo()
                //                    {
                //                        SymbolType = MarkingSymbolType.Wrong,
                //                        Position = new System.Drawing.Point((int)autoMarkedX, (int)autoMarkedY)
                //                    });
                //                }
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    if (i > 0) //==1
                //    {
                //        temp = _question[(i - 1) < _question.Count ? (i - 1) : (_question.Count - 1)];
                //        tempDtail =
                //            currentMarkingResult.Detail.FirstOrDefault(
                //                od => od.QuestionID == temp.Base.ID && od.QuestionVersion == temp.Base.EntireVersionID);
                //        tempDtail.StudentAnswerSnapshot = Path.GetFileName(paperImages[i - 1]);
                //        //tempDtail.IsCorrect = true;
                //        img.Tag = string.Format("{0}|{1}|{2}", temp.Base.ID, temp.Base.EntireVersionID, Path.GetFileName(paperImages[i]));
                //    }
                //}

                #endregion
            }
            var autoComment = currentMarkingResult.Detail.FirstOrDefault();
            if (autoComment != null && autoComment.TeacherComments != null && autoComment.TeacherComments.Count > 0)
            {
                autoComment.TeacherComments[0].Position = new System.Drawing.Point((int) autoMarkedX + 8,
                    codeImageHeight - 38);
            }
            readWrap.Height = totalHeight;
            _markingResult.IsFinished = true;

            return true;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            ChangePaper(false, "已经是第一张了");
        }

        private void ChangePaper(bool isNext, string msg)
        {
            object[] objs;
            if (ObtianDirctoryHandler != null)
            {
                objs = ObtianDirctoryHandler(isNext, _currentMarked.MarkedResultID);
                var temp = objs[0] as PaperMarkedInfo;
                if (temp != null)
                {
                    Init(temp, objs[1] as PaperBatchDetail);
                }
                else
                {
                    Helper.ShowQuestion(msg);
                }
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            ChangePaper(true, "已经是最后一张了");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init(_currentMarked, _currentBatch);
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            _currentOperate = (Operate)Byte.Parse((sender as Button).Tag.ToString());

            //switch (_currentOperate)
            //{
            //    case Operate.Pointer:
            //        Cursor = Cursors.Arrow;
            //        return;
            //    case Operate.Mark:
            //        _currentCursor.Source = new BitmapImage(new Uri("Images/DrawCheckmark.png"));
            //        break;
            //    case Operate.Comment:
            //        _currentCursor.Source = new BitmapImage(new Uri("Images/Comment.png"));
            //        break;
            //    case Operate.Erase:
            //        _currentCursor.Source = new BitmapImage(new Uri("Images/Erase.png"));
            //        break;
            //    default:
            //        Cursor = Cursors.Arrow;
            //        return;
            //}
        }

        private void readWrap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MarkingDetail detail;
            switch (_currentOperate)
            {
                case Operate.Pointer:
                    break;
                case Operate.Hook:
                    detail = RecordDetail(Mouse.DirectlyOver as UIElement, true);
                    if (detail != null)
                    {
                        Mark(Mouse.GetPosition(readWrap), detail, true);
                        //_markingResult.IsFinished = true;
                    }
                    break;
                case Operate.Fork:
                    detail = RecordDetail(Mouse.DirectlyOver as UIElement, false);
                    if (detail != null)
                    {
                        Mark(Mouse.GetPosition(readWrap), detail, false);
                        //_markingResult.IsFinished = true;
                    }
                    break;
                case Operate.Comment:
                    detail = RecordDetail(Mouse.DirectlyOver as UIElement, null);
                    if (detail != null)
                    {
                        Comment(Mouse.GetPosition(readWrap), detail);
                        //_markingResult.IsFinished = true;
                    }
                    break;
                case Operate.Erase:
                    break;
                default:
                    break;
            }
        }

        private void Mark(Point p, MarkingDetail detail, bool isCorrect)
        {
            //object[] objs = new object[2];
            //objs[0] = detail.ID;
            //objs[1] = mark;

            string markPaht = isCorrect ? "/Images/Correct.png" : "/Images/Incorrect.png";
            Image img = new Image();
            img.Source = new BitmapImage(new Uri(markPaht, UriKind.Relative));
            img.Width = 40d;
            img.Height = 40d;
            var mark = new MarkingSymbolInfo()
            {
                Position = new System.Drawing.Point((int)(p.X - 20), (int)(p.Y - 20)),
                SymbolType = isCorrect ? MarkingSymbolType.Right : MarkingSymbolType.Wrong,
            };
            detail.MarkingSymbols.Add(mark);

            img.DataContext = mark;
            img.Tag = detail.ID;
            Canvas.SetLeft(img, mark.Position.X);
            Canvas.SetTop(img, mark.Position.Y);
            img.MouseDown += Erase;
            readWrap.Children.Add(img);

            detail.IsCorrect = detail.MarkingSymbols.All(bols => bols.SymbolType == MarkingSymbolType.Right);  //都为勾才视为正确
            if (!detail.IsCorrect)
            {
                detail.Score = 0;
            }
        }

        private MarkingDetail RecordDetail(UIElement currentQ, bool? isCorrect)
        {

            MarkingDetail detail; Guid qId; long vId; QuestionEntity curQuest = null;

            //var infos = img.Tag.ToString();

            var qImg = Helper.FindVisualParent<Image>(currentQ);
            if (qImg != null && qImg.Tag != null)
            {

                var temp = qImg.Tag.ToString();
                if (!string.IsNullOrWhiteSpace(temp))
                {

                    var qRef = temp.Split('|');
                    if (qRef.Length > 1)
                    {

                        if (!Guid.TryParse(qRef[0], out qId)) //视为无效标记
                        {
                            return null;
                        }
                        curQuest = _question.First(q => q.Base.ID == qId);
                        if (!long.TryParse(qRef[1], out vId))
                        {
                            return null;
                        }
                        detail = _markingResult.Detail.Find(m => m.QuestionID == qId && m.QuestionVersion == vId);
                        if (detail == null)
                        {
                            detail = new MarkingDetail();
                            detail.TeacherComments = new List<TeacherCommentInfo>();
                            detail.MarkingSymbols = new List<MarkingSymbolInfo>();

                            detail.ID = Guid.NewGuid();
                            _markingResult.Detail.Add(detail);
                            detail.QuestionID = qId;
                            detail.QuestionVersion = vId;

                            //detail.StudentAnswerSnapshot = qRef.Length > 2 ? qRef[2] : string.Empty;
                        }
                        else
                        {
                            if (detail.MarkingSymbols == null)
                            {
                                detail.MarkingSymbols = new List<MarkingSymbolInfo>();
                            }
                            if (detail.TeacherComments == null)
                            {
                                detail.TeacherComments = new List<TeacherCommentInfo>();
                            }
                        }

                        if (curQuest != null)
                        {
                            if (isCorrect != null)
                            {
                                detail.IsCorrect = (bool)isCorrect;
                                detail.Score = detail.IsCorrect ? curQuest.DetailList.Sum(d => d.Base.Score) : 0;
                            }
                        }

                        return detail;
                    }

                }
            }
            return null;
        }

        private void Comment(Point p, MarkingDetail detail)
        {
            TextBox txt = new TextBox();
            Canvas.SetLeft(txt, p.X);
            Canvas.SetTop(txt, p.Y);
            txt.Foreground = Brushes.Red;
            // txt.MouseDown += Erase;
            //txt.PreviewMouseMove += txt_PreviewMouseMove;
            txt.PreviewMouseUp += Erase;
            // txt.LostFocus += txt_LostFocus;
            txt.GotFocus += txt_GotFocus;

            txt.AllowDrop = true;

            var comment = new TeacherCommentInfo()
            {
                CommentText = placeHolder,
                Position = new System.Drawing.Point((int)p.X, (int)p.Y)
            };
            txt.DataContext = comment;
            var binding = new Binding("CommentText");
            txt.SetBinding(TextBox.TextProperty, binding);
            txt.TextChanged += txt_TextChanged;
            txt.Tag = detail.ID;

            detail.TeacherComments.Add(comment);

            readWrap.Children.Add(txt);
            //txt.Text = placeHolder;
            //txt.Focus();
            _currentOperate = Operate.Pointer;
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = sender as TextBox;
            (txt.DataContext as TeacherCommentInfo).CommentText = txt.Text;
        }

        private void txt_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !e.Handled)
            {
                var p = Mouse.GetPosition(readWrap);
                var txt = sender as TextBox;

                //var top = (double)txt.GetValue(Canvas.TopProperty);
                //var left = (double)txt.GetValue(Canvas.LeftProperty);
                txt.SetValue(Canvas.TopProperty, p.Y);
                txt.SetValue(Canvas.LeftProperty, p.X);
            }

        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt.Text == placeHolder)
            {
                txt.Text = string.Empty;
            }
        }

        private void Erase(object sender, MouseEventArgs e)
        {
            if (_currentOperate == Operate.Erase)
            {
               // _markingResult.IsFinished = true;

                Guid id;
                if (sender is TextBox)
                {
                    var txt = sender as TextBox;
                    if (Guid.TryParse(txt.Tag.ToString(), out id))
                    {
                        _markingResult.Detail.Find(d => d.ID == id).TeacherComments.Remove(txt.DataContext as TeacherCommentInfo);
                    }
                }
                else if (sender is Image)
                {
                    var img = sender as Image;
                    if (Guid.TryParse(img.Tag.ToString(), out id))
                    {

                        var detail = _markingResult.Detail.Find(d => d.ID == id);
                        //if (symbol.SymbolType == MarkingSymbolType.Wrong)
                        //{
                        //    if (detail.MarkingSymbols.Count == 1)
                        //    {
                        //        detail.IsCorrect = true;
                        //    }
                        //}
                        detail.MarkingSymbols.Remove(img.DataContext as MarkingSymbolInfo);
                        detail.IsCorrect = !detail.MarkingSymbols.Any(symbol => symbol.SymbolType == MarkingSymbolType.Wrong);
                    }
                }

                readWrap.Children.Remove(sender as UIElement);
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
            this.Cursor = Cursors.Arrow;
        }

        private void readWrap_MouseEnter(object sender, MouseEventArgs e)
        {
            //  Cursor = Cursors.None;

            // FileStream cur = new FileStream(@"..\..\Images\cur\Hook.cur", FileMode.Open, FileAccess.Read);
            // string ss = Resources.Source.AbsolutePath;

            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = null;

            switch (_currentOperate)
            {
                case Operate.Hook:
                    myStream = myAssembly.GetManifestResourceStream("Deyi.Tool.Images.cur.Hook.cur");
                    break;
                case Operate.Fork:
                    myStream = myAssembly.GetManifestResourceStream("Deyi.Tool.Images.cur.Forks.cur");
                    break;
                case Operate.Comment:
                    myStream = myAssembly.GetManifestResourceStream("Deyi.Tool.Images.cur.Comment.cur");
                    break;
                case Operate.Erase:
                    myStream = myAssembly.GetManifestResourceStream("Deyi.Tool.Images.cur.Erase.cur");
                    break;
                default:
                    myStream = null;
                    break;
            }

            if (myStream != null)
            {
                Cursor = new Cursor(myStream);
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void ReappearMarked(MarkingResult result)
        {
            // var sobles = result.Detail.

            Image img; TextBox txt;
            string markPaht = string.Empty;
            foreach (var detail in result.Detail)
            {
                if (detail.MarkingSymbols != null)
                {
                    foreach (var mark in detail.MarkingSymbols)
                    {
                        img = new Image();
                        img.Tag = detail.ID; //string.Format("{0}|{1}", detail.QuestionID, detail.QuestionVersion);
                        img.DataContext = mark;
                        img.Width = 40d;
                        img.Height = 40d;

                        if (mark.SymbolType == MarkingSymbolType.Right)
                        {
                            markPaht = "Images/Correct.png";
                        }
                        else if (mark.SymbolType == MarkingSymbolType.Wrong)
                        {
                            markPaht = "Images/Incorrect.png";
                        }
                        img.Source = new BitmapImage(new Uri(markPaht, UriKind.Relative));
                        Canvas.SetLeft(img, mark.Position.X);
                        Canvas.SetTop(img, mark.Position.Y);
                        img.MouseDown += Erase;
                        readWrap.Children.Add(img);

                    }
                }
                if (detail.TeacherComments != null)
                {
                    foreach (var comment in detail.TeacherComments)
                    {
                        txt = new TextBox();
                        txt.Tag = detail.ID;
                        txt.DataContext = comment;
                        Canvas.SetLeft(txt, comment.Position.X);
                        Canvas.SetTop(txt, comment.Position.Y);
                        txt.Foreground = Brushes.Red;
                        // txt.MouseDown += Erase;
                        //txt.PreviewMouseMove += txt_PreviewMouseMove;
                        txt.PreviewMouseUp += Erase;
                        // txt.LostFocus += txt_LostFocus;
                        txt.AllowDrop = true;

                        //Helper.FindVisualParent<>
                        txt.Text = comment.CommentText;
                        readWrap.Children.Add(txt);
                    }
                }
            }
        }

        private void btnEndRead_Click(object sender, RoutedEventArgs e)
        {
            //if (!_isSaved)
            //{
            //    if (Helper.ShowQuestion("阅卷结果没有被保存，是否保存？"))
            //    {
            //        SaveMarked();
            //    }
            //}

            DialogResult = true;
            //MarkingPaper.SetMarkingResult(_markingResultList);
            //_markingResultList = null;
            _markingResult = null;
            this.Close();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new PrintDialog();
                //dialog.PageRangeSelection = PageRangeSelection.AllPages;
                //IDocumentPaginatorSource dps = new FlowDocument() { };
                //dialog.PrintQueue = new System.Printing.PrintQueue();
                //this.Background = Brushes.White;
                // readWrap.Background = Brushes.White;
                //grdPrint.Background = Brushes.White;
                //scrollWrap.ScrollToTop();

                if (dialog.ShowDialog().GetValueOrDefault())
                {
                    var markeds = _markingResult.Detail.SelectMany(d => d.MarkingSymbols).ToArray();
                    var comments = _markingResult.Detail.SelectMany(d =>
                    {
                        if (d.TeacherComments == null)
                        {
                            return new List<TeacherCommentInfo>();
                        }
                        return d.TeacherComments;
                    }
                    ).ToArray();

                    var vis = Helper.PrintingMarked(markeds, comments);

                    dialog.PrintVisual(vis, "正在打印作业");

                }

            }
            catch (Exception ex)
            {
                Helper.ShowError(ex.Message);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                ChangePaper(false, "已经是第一张了");
            }
            else if (e.Key == Key.Right)
            {
                ChangePaper(true, "已经是最后一张了");
            }
        }

    }

    public delegate object[] NextOrPrevHandler(bool isNext, Guid resultId);

    public enum Operate : byte
    {
        Pointer = 0,
        Hook = 1,
        Fork = 2,
        Comment = 3,
        Erase = 4
    }
}
