# region 四川得一科技有限公司 版权所有
/* ================================================
 * 公司：四川得一科技有限公司
 * 作者：shoy
 * 创建：2013-10-30
 * 描述：工具类
 * ================================================
 */
# endregion

using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.Open.Model.Marking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using FlowDirection = System.Windows.FlowDirection;
using FontFamily = System.Windows.Media.FontFamily;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;

namespace DayEasy.MarkingTool.Core
{
    public class WindowsHelper : Helper
    {

        /// <summary>
        /// 显示错误提示框
        /// </summary>
        /// <param name="message">错误内容</param>
        public static void ShowError(string message)
        {
            MessageBox.Show(message, DeyiKeys.TipInfo, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// 显示问题提示框
        /// </summary>
        /// <param name="message">问题内容</param>
        /// <returns></returns>
        public static bool ShowQuestion(string message)
        {
            return MessageBox.Show(message, DeyiKeys.TipInfo, MessageBoxButton.OKCancel, MessageBoxImage.Question) ==
                   MessageBoxResult.OK;
        }

        /// <summary>
        /// 显示选择提示框
        /// </summary>
        /// <param name="message">问题内容</param>
        /// <returns></returns>
        public static bool ShowSure(string message)
        {
            return MessageBox.Show(message, DeyiKeys.TipInfo, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                   MessageBoxResult.Yes;
        }

        public static void ShowMsg(string message)
        {
            MessageBox.Show(message, DeyiKeys.TipInfo, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        /// <summary>
        /// 阅卷批量套打
        /// </summary>
        /// <param name="marked"></param>
        /// <param name="comment"></param>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        public static DrawingVisual PrintingMarked(IEnumerable<MarkingSymbolInfo> marked,
            IEnumerable<TeacherCommentInfo> comment, int currentPage = 0)
        {
            var drawingVisual = new DrawingVisual();
            var skipHeight = currentPage * Math.Ceiling(A4Size.Height ?? 1100);
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                CursorManager cursorManager;
                if (marked != null)
                {
                    foreach (var m in marked)
                    {
                        cursorManager = new CursorManager(m.SymbolType);
                        var size = cursorManager.GetImageSize();
                        var rectRange = new Rect(m.Position.X, m.Position.Y - skipHeight, size.Width, size.Height);
                        var bitmap = cursorManager.GetBitmapImage();
                        drawingContext.DrawImage(bitmap, rectRange);
                    }
                }

                if (comment != null)
                {
                    foreach (var c in comment)
                    {
                        if (c.EmotionType < 100)
                        {
                            var fontText = new FormattedText(c.CommentText ?? string.Empty,
                                CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight,
                                new Typeface(new FontFamily("宋体"), FontStyles.Normal, FontWeights.Light,
                                    FontStretches.Normal), 16, Brushes.DarkRed);
                            if (c.CommentText != null && c.CommentText.Contains("得分"))
                            {
                                fontText.SetFontSize(26);
                                fontText.SetFontWeight(FontWeights.Bold);
                                fontText.SetForegroundBrush(Brushes.Red);
                            }
                            drawingContext.DrawText(fontText, new Point(c.Position.X, c.Position.Y - skipHeight));
                        }
                        else
                        {
                            cursorManager = new CursorManager(c.EmotionType);
                            var size = cursorManager.GetImageSize();
                            var rectRange = new Rect(c.Position.X, c.Position.Y - skipHeight, size.Width, size.Height);
                            var bitmap = cursorManager.GetBitmapImage();
                            drawingContext.DrawImage(bitmap, rectRange);
                        }
                    }
                }
            }
            return drawingVisual;
        }



        /// <summary>
        /// 阅卷套打
        /// </summary>
        /// <param name="results"></param>
        /// <param name="pageCount"></param>
        public static JsonResultBase PrintMarkingResult(List<MarkingResult> results, int pageCount = 1)
        {
            if (results == null || !results.Any())
                return new JsonResultBase("阅卷信息为空！");
            var dialog = new System.Windows.Controls.PrintDialog();
            if (!dialog.ShowDialog().GetValueOrDefault())
                return new JsonResultBase(true, string.Empty);
            var pageHeight = (int)(A4Size.Height ?? 1100);
            var containerVisual = new ContainerVisual();

            foreach (var result in results)
            {
                var symbols = result.Details.SelectMany(t => t.MarkingSymbols ?? new List<MarkingSymbolInfo>()).ToList();
                var commentList =
                    result.Details.SelectMany(t => t.TeacherComments ?? new List<TeacherCommentInfo>()).ToList();
                if (!commentList.Any(t => t.CommentText != null && t.CommentText.Contains("得分")))
                {
                    commentList.Add(new TeacherCommentInfo
                    {
                        CommentText = "得分：" + result.TotalScore.ToString("0.#"),
                        Position = new System.Drawing.Point(480, 30)
                    });
                }
                var markeds = new List<MarkingSymbolInfo>();
                var comments = new List<TeacherCommentInfo>();
                for (int i = 0; i < pageCount; i++)
                {
                    int startY = i * pageHeight,
                        endY = (i + 1) * pageHeight;

                    if (symbols.Any())
                        markeds = symbols.Where(t => t.Position.Y >= startY && t.Position.Y < endY).ToList();
                    if (commentList.Any())
                        comments = commentList.Where(t => t.Position.Y >= startY && t.Position.Y < endY).ToList();
                    var vis = PrintingMarked(markeds, comments, i);
                    //SaveVisual(vis, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.png"));
                    containerVisual.Children.Add(vis);
                }
            }
            var doc = new VisualDocumentPaginator(containerVisual);
            dialog.PrintDocument(doc, "阅卷套打中...");
            return new JsonResultBase(true, string.Empty);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="directoryPaht"></param>
        /// <param name="ext"></param>
        public static void DeleteFiles(string directoryPaht, params string[] ext)
        {
            if (ext == null)
            {
                return;
            }
            var directory = new DirectoryInfo(directoryPaht);
            if (!directory.Exists)
            {
                return;
            }
            var files = directory.GetFiles().Where(f =>
            {
                if (ext.Length == 0)
                {
                    return true;
                }
                return ext.Contains(f.Extension);
            });
            try
            {
                foreach (var f in files)
                {
                    if (f.Exists)
                    {
                        f.Delete();
                    }
                }
                directory.Delete();
            }
            catch (Exception)
            {
                //TODO...
            }
        }


        public static void DeleteFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception)
                {
                    //TODO:
                }
            }

        }

        public static int StrToInt(string str, int def)
        {
            int val;
            if (!int.TryParse(str, out val))
                val = def;
            return val;
        }

        public static void SaveVisual(DrawingVisual visual, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                //dpi可以自己设定   // 获取dpi方法：PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice
                var bitmap = new RenderTargetBitmap((int)A4Size.Width, (int)A4Size.Height,
                    96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);
                var encode = new PngBitmapEncoder();
                encode.Frames.Add(BitmapFrame.Create(bitmap));
                encode.Save(fs);
            }
        }

        /// <summary>
        /// 加载阅卷批注
        /// </summary>
        /// <param name="result"></param>
        /// <param name="baseDirectory"></param>
        /// <param name="paperName"></param>
        public static void SetMarkedSymbols(MarkingResult result, string baseDirectory, string paperName)
        {
            string basePath = Path.Combine(baseDirectory, paperName, DeyiKeys.CompressName);
            var markPath = Path.Combine(basePath, DeyiKeys.MarkedName);
            if (!Directory.Exists(markPath))
                Directory.CreateDirectory(markPath);
            var font = new Font("宋体", 16);
            foreach (var detail in result.Details)
            {
                var imgName = detail.StudentAnswerSnapshot;
                if (string.IsNullOrWhiteSpace(imgName))
                    continue;
                var qid = detail.QuestionId;
                string imgPath = Path.Combine(basePath, imgName);
                var details = result.Details.Where(d => d.QuestionId == qid).ToList();
                var symbols = details.SelectMany(t => t.MarkingSymbols);
                var teacherSymbols = details.SelectMany(t => t.TeacherComments);
                using (var bmp = new Bitmap(imgPath))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        foreach (var symbol in symbols)
                        {
                            var manager = new CursorManager(symbol.SymbolType);
                            var symImg = manager.GetBitmap();

                            //var symImg = GetSymbolsImage(symbol.SymbolType);
                            if (symImg == null)
                                continue;
                            var size = manager.GetImageSize();
                            var point = symbol.Position;
                            g.DrawImage(symImg,
                                new RectangleF(point.X, point.Y - detail.MarginTop, size.Width, size.Height));
                        }
                        foreach (var symbol in teacherSymbols)
                        {
                            var point = new PointF(symbol.Position.X, symbol.Position.Y - detail.MarginTop);
                            if (symbol.EmotionType < 100)
                                g.DrawString(symbol.CommentText, font, System.Drawing.Brushes.Red,
                                    new PointF(point.X, point.Y));
                            else
                            {
                                var manager = new CursorManager(symbol.EmotionType);
                                var symImg = manager.GetBitmap();
                                if (symImg == null)
                                    continue;
                                var size = manager.GetImageSize();
                                g.DrawImage(symImg,
                                    new RectangleF(point.X, point.Y, size.Width, size.Height));
                            }
                        }
                    }
                    bmp.Save(Path.Combine(markPath, detail.StudentAnswerSnapshot));
                }
            }
        }

        ///// <summary>
        ///// 获取批注图片
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //private static Bitmap GetSymbolsImage(MarkingSymbolType type)
        //{
        //    var imgPath = Path.Combine(DeyiKeys.CurrentDir, "config");
        //    switch (type)
        //    {
        //        case MarkingSymbolType.Right:
        //            return new Bitmap(Path.Combine(imgPath, "Correct.png"));
        //        case MarkingSymbolType.HalfRight:
        //            return new Bitmap(Path.Combine(imgPath, "HalfCorrect.png"));
        //        case MarkingSymbolType.Wrong:
        //            return new Bitmap(Path.Combine(imgPath, "Incorrect.png"));
        //        default:
        //            return null;
        //    }
        //}
    }
}
