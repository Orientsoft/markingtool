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
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.MarkingTool.UI.Controls;
using DayEasy.Models.Open.Work;
using System;
using System.Collections.Generic;
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
        internal static Window OwnerWindow = Application.Current.MainWindow;

        /// <summary> 显示错误提示框 </summary>
        /// <param name="message">错误内容</param>
        public static void ShowError(string message)
        {
            try
            {
                DeyiDialog.Alert(message);
            }
            catch
            {
                MessageBox.Show(message, DeyiKeys.TipInfo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 显示问题提示框
        /// </summary>
        /// <param name="message">问题内容</param>
        /// <returns></returns>
        public static bool ShowQuestion(string message)
        {
            try
            {
                return DeyiDialog.Confirm(message);
            }
            catch
            {
                return MessageBox.Show(message, DeyiKeys.TipInfo, MessageBoxButton.OKCancel, MessageBoxImage.Question) ==
                       MessageBoxResult.OK;
            }
        }

        /// <summary>
        /// 显示选择提示框
        /// </summary>
        /// <param name="message">问题内容</param>
        /// <param name="yes"></param>
        /// <param name="no"></param>
        /// <returns></returns>
        public static bool ShowSure(string message, string yes = "是", string no = "否")
        {
            try
            {
                return DeyiDialog.Confirm(message, yes, no);
            }
            catch
            {
                return MessageBox.Show(message, DeyiKeys.TipInfo, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                       MessageBoxResult.Yes;
            }
        }

        public static MessageBoxResult ShowSureOrCancel(string message)
        {
            var result = DeyiDialog.SureOrCancel(message);
            if (!result.HasValue)
                return MessageBoxResult.Cancel;
            return result.Value ? MessageBoxResult.Yes : MessageBoxResult.No;
            //return MessageBox.Show(message, DeyiKeys.TipInfo, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        }

        public static void ShowMsg(string message, int width = 260, int height = 160)
        {
            DeyiDialog.Alert(message, width, height);
            //MessageBox.Show(message, DeyiKeys.TipInfo, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [Obsolete]
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
        public static DrawingVisual PrintingMarked(IEnumerable<SymbolInfo> marked,
            IEnumerable<CommentInfo> comment, int currentPage = 0)
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
                        var rectRange = new Rect(m.Point.X, m.Point.Y - skipHeight, size.Width, size.Height);
                        var bitmap = cursorManager.GetBitmapImage();
                        drawingContext.DrawImage(bitmap, rectRange);
                    }
                }

                if (comment != null)
                {
                    foreach (var c in comment)
                    {
                        if (c.SymbolType < 100)
                        {
                            var fontText = new FormattedText(c.Words ?? string.Empty,
                                CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight,
                                new Typeface(new FontFamily("宋体"), FontStyles.Normal, FontWeights.Light,
                                    FontStretches.Normal), 16, Brushes.DarkRed);
                            if (c.Words != null && c.Words.Contains("得分"))
                            {
                                fontText.SetFontSize(26);
                                fontText.SetFontWeight(FontWeights.Bold);
                                fontText.SetForegroundBrush(Brushes.Red);
                            }
                            drawingContext.DrawText(fontText, new Point(c.Point.X, c.Point.Y - skipHeight));
                        }
                        else
                        {
                            cursorManager = new CursorManager(c.SymbolType);
                            var size = cursorManager.GetImageSize();
                            var rectRange = new Rect(c.Point.X, c.Point.Y - skipHeight, size.Width, size.Height);
                            var bitmap = cursorManager.GetBitmapImage();
                            drawingContext.DrawImage(bitmap, rectRange);
                        }
                    }
                }
            }
            return drawingVisual;
        }

        public static string ObjectiveError(string word, decimal? score = null)
        {
            var objectInfo = string.Empty;
            if (score.HasValue)
                objectInfo = "客观题 得" + score.Value + "分";
            //客观题信息
            if (string.IsNullOrWhiteSpace(word))
                return objectInfo;
            var prefix = (score.HasValue ? string.Empty : "客观题");
            if (!new[] { "全对", "全错" }.Contains(word))
                prefix += "错题：";
            objectInfo += string.Concat("    ", prefix, word);
            return objectInfo;
        }

        /// <summary> 新版套打 </summary>
        /// <param name="details"></param>
        /// <param name="isOdd">是否是奇数页</param>
        /// <returns></returns>
        public static DResults<string> PrintDetails(List<PrintBatchDetail> details, bool? isOdd = null)
        {
            if (details == null || !details.Any())
                return new DResults<string>("套打列表为空！");
            var dialog = new System.Windows.Controls.PrintDialog();
            if (!dialog.ShowDialog().GetValueOrDefault())
                return new DResults<string>("套打操作已取消！");
            var pageHeight = (int)(A4Size.Height ?? 1100);
            var containerVisual = new ContainerVisual();
            var list = new List<string>();
            int pageIndex = 0;
            foreach (var detail in details)
            {
                var marks = new List<SymbolBaseInfo>();
                if (detail.CommentInfos != null && detail.CommentInfos.Any())
                    marks.AddRange(detail.CommentInfos);
                if (detail.SymbolInfos != null && detail.SymbolInfos.Any())
                    marks.AddRange(detail.SymbolInfos);

                //客观题信息
                marks.Add(new CommentInfo
                {
                    Point = DeyiKeys.ErrorPointF,
                    SymbolType = (int)SymbolType.Objective,
                    Words = ObjectiveError(detail.ObjectiveErrorInfo, detail.ObjectiveScore)
                });
                //得分
                marks.Add(new CommentInfo
                {
                    Point = DeyiKeys.ScorePointF,
                    SymbolType = (int)SymbolType.Custom,
                    Words = "得分：" + detail.Score.ToString("0.#")
                });
                for (var i = 0; i < detail.PageCount; i++)
                {
                    pageIndex++;
                    if (isOdd.HasValue)
                    {
                        if ((isOdd.Value && pageIndex % 2 == 0) || (!isOdd.Value && pageIndex % 2 == 1))
                            continue;
                    }
                    int startY = i * pageHeight,
                        endY = (i + 1) * pageHeight;
                    var markItems = marks.Where(t => t.Point.Y >= startY && t.Point.Y < endY).ToList();
                    var vis = PrintVisual(markItems, i);
#if DEBUG
                    SaveVisual(vis,
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                            string.Format("test-{0}_{1}.png", pageIndex, i + 1)));
#endif
                    containerVisual.Children.Add(vis);
                    if (!list.Contains(detail.Id))
                        list.Add(detail.Id);
                }
            }
            var doc = new VisualDocumentPaginator(containerVisual);
            dialog.PrintDocument(doc, "阅卷套打中...");
            return new DResults<string>(list, list.Count);
        }

        private static DrawingVisual PrintVisual(IEnumerable<SymbolBaseInfo> commentInfos, int currentPage = 0)
        {
            var drawingVisual = new DrawingVisual();
            var skipHeight = currentPage * Math.Ceiling(A4Size.Height ?? 1100);
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                if (commentInfos != null)
                {
                    foreach (var m in commentInfos)
                    {
                        string word = string.Empty;
                        double score = 0;
                        if (m is SymbolInfo)
                        {
                            score = (m as SymbolInfo).Score;
                        }
                        else if (m is CommentInfo)
                        {
                            word = (m as CommentInfo).Words;
                        }
                        var manager = new MarksManager(m.SymbolType, word);
                        if (manager.IsImage)
                        {
                            var size = manager.GetSize();
                            var rectRange = new Rect(m.Point.X, m.Point.Y - skipHeight, size.Width, size.Height);
                            var bitmap = manager.GetBitmapImage();
                            drawingContext.DrawImage(bitmap, rectRange);
                            if (score <= 0)
                                continue;
                            //扣分
                            var fontText = new FormattedText("-" + score, CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight,
                                new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold,
                                    FontStretches.Normal), 24, Brushes.Red);
                            drawingContext.DrawText(fontText,
                                new Point(m.Point.X + size.Width, m.Point.Y - 3 - skipHeight));
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(word))
                                continue;
                            var fontText = new FormattedText(word,
                                CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight,
                                new Typeface(new FontFamily("宋体"), FontStyles.Normal, FontWeights.Light,
                                    FontStretches.Normal), 16, Brushes.Red);
                            switch (m.SymbolType)
                            {
                                case (int)SymbolType.Objective:
                                    fontText.MaxTextWidth = 150;
                                    break;
                                case (int)SymbolType.Custom:
                                    var max = (int)Math.Floor(770 - m.Point.X);
                                    if (max > 0)
                                        fontText.MaxTextWidth = max;
                                    if (max > 200)
                                        fontText.MaxTextWidth = 200;
                                    fontText.SetFontSize(22);
                                    fontText.SetFontWeight(FontWeights.Bold);
                                    fontText.SetForegroundBrush(Brushes.Red);
                                    break;
                            }
                            //得分
                            if (word.Contains("得分"))
                            {
                                fontText.SetFontSize(26);
                            }
                            var x = Math.Max(m.Point.X, 10);
                            var y = Math.Max(m.Point.Y - skipHeight, 10);
                            drawingContext.DrawText(fontText, new Point(x, y));
                        }
                    }
                }
            }
            return drawingVisual;
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
                var bitmap = new RenderTargetBitmap((int)(A4Size.Width ?? 780), (int)(A4Size.Height ?? 1100),
                    96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);
                var encode = new PngBitmapEncoder();
                encode.Frames.Add(BitmapFrame.Create(bitmap));
                encode.Save(fs);
            }
        }
    }
}
