using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity.Marking;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.MarkingTool.BLL.Scanners.Builder;
using DayEasy.MarkingTool.Core;
using DayEasy.Open.Model.Enum;
using DayEasy.Open.Model.Marking;
using DayEasy.Open.Model.Paper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// 作业
    /// </summary>
    public partial class MarkingPaper
    {
        private readonly Logger _logger = Logger.L<MarkingPaper>();
        /// <summary>
        /// 试卷类型
        /// </summary>
        private readonly PaperKind _kind = PaperKind.Paper;

        private SortType _sortType = SortType.Default;

        private ObservableCollection<PaperMarkedInfo> _markedInfoList = new ObservableCollection<PaperMarkedInfo>();
        private List<string> _paperPathsList;
        private string _markingKey = string.Empty;
        private static PaperUsageInfo _batchDetail;
        private static List<MarkingResult> _markedResult;
        private bool _hasPrint;
        private bool _isSaved;
        private bool _isModify;
        private bool _uploadRunning;
        private readonly PrintUsage _usage;

        private readonly byte[] _paperAPrintTypes =
        {
            (byte) PrintType.PaperAHomeWork, (byte) PrintType.PaperAAnswerSheet
        };

        private readonly byte[] _paperBPrintTypes =
        {
            (byte) PrintType.PaperBHomeWork, (byte) PrintType.PaperBAnswerSheet
        };

        public MarkingPaper(PrintUsage usage)
        {
             _usage = usage;
            _batchDetail = DeyiApp.GetBatchDetail(usage.Batch);
            //区分AB卷
            if (_paperAPrintTypes.Contains(usage.PrintType))
            {
                _batchDetail.Sections = _batchDetail.Sections.Where(t => t.SectionType == 1).ToList();
            }
            if (_paperBPrintTypes.Contains(usage.PrintType))
            {
                _batchDetail.Sections = _batchDetail.Sections.Where(t => t.SectionType == 2).ToList();
            }
            InitializeComponent();
            _kind = usage.Kind;
            Title = string.Format("{0}_{1} - [阅{2}]", _usage.PaperTitle, _usage.ClassName, _kind.GetText());
            StudentTip.Text = string.Format("班级人数：{0}人", _usage.StudentCount);
        }

        private string UserTempPath
        {
            get
            {
                return Path.Combine(DeyiKeys.UnfinishedPaperSavePath,
                    DeyiApp.UserId.ToString(CultureInfo.InvariantCulture), "temp");
            }
        }


        #region 驱动扫描仪直接扫描

        private void btnScanner_Click(object sender, RoutedEventArgs e)
        {
            var type = (((ComboBoxItem)ScannerType.SelectedItem).Content.ToString() == "单面" ? 0 : 1);
            if (!ScannerHelper.CheckOcx())
            {
                if (ScannerHelper.RegistOcx())
                {
                    if (WindowsHelper.ShowQuestion("需要重新启动扫描页，是否重启？"))
                        Close();
                }
                return;
            }
            using (var helper = new ScannerHelper(Wrap, Guid.NewGuid().ToString("N").Substring(5, 6), type))
            {
                var list = helper.Scnaner();
                Dispatcher.Invoke(new Action(() =>
                {
                    _paperPathsList = list;
                    ThreadPool.QueueUserWorkItem(BeginPorgress);
                }));
            }
        }

        #endregion

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            ShowPicture();
        }

        /// <summary>
        /// 选择图片文件
        /// </summary>
        private void ShowPicture()
        {
            var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string rootPath = fbd.SelectedPath;

                var picWin = new ImageView(rootPath, _markedInfoList.Select(m => m.PaperName).ToArray());
                if (picWin.ShowDialog() == true)
                {
                    _paperPathsList = picWin.SelectPaths;
                    ThreadPool.QueueUserWorkItem(BeginPorgress);
                }
                else
                {
                    if (_paperPathsList != null)
                    {
                        _paperPathsList.Clear();
                    }
                }
            }
        }

        private void ShowResult(PaperMarkedInfo markedInfo)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                markedInfo.Index = (_markedInfoList.Any() ? _markedInfoList.Max(t => t.Index) + 1 : 1);
                _markedInfoList.Add(markedInfo);
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _markedInfoList.Clear();
            _markingKey = Guid.NewGuid().ToString("N");
            _markedResult = new List<MarkingResult>();
            _paperPathsList = new List<string>();
            LoadOldData();

            List.ItemsSource = _markedInfoList;
            PaperMarkedInfo.MarkedId = _markingKey;
            Closing += MarkingPaper_Closing;
            Closed += MarkingPaper_Closed;
        }

        private void MarkingPaper_Closed(object sender, EventArgs e)
        {
            _markedInfoList.Clear();
            _markingKey = string.Empty;
            _paperPathsList = null;
            _markedResult = null;
            if (Owner != null)
            {
                var def = (DefaultWindow)Owner;
                def.RefreshList();
            }
        }

        private void MarkingPaper_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_markedInfoList.Any(t => t.IsSuccess) || !_isModify || _isSaved)
                return;

            if (WindowsHelper.ShowQuestion("阅卷没有被保存,是否保存？"))
            {
                Helper.SaveMarkedResult(_kind, _usage.Batch + _usage.PrintType, _markedInfoList, _markedResult);
            }
        }

        /// <summary>
        /// 扫描试卷
        /// </summary>
        /// <param name="args"></param>
        private void BeginPorgress(object args)
        {
            if (null == _paperPathsList || _paperPathsList.Count < 1)
            {
                return;
            }
            EnabledButton(false);
            _isModify = true;
            InitBar(_paperPathsList.Count, Visibility.Visible, 10);
            var watch = new Stopwatch();
            var manager = new ScannerBuilderManager();
            if (_batchDetail == null) return;
            var i = 0;
            foreach (var item in _paperPathsList)
            {
                watch.Start();
                var marked = new PaperMarkedInfo
                {
                    ImagePath = item
                };
                var builder = new PaperScannerBuidler(item, _batchDetail, _usage.ClassId);
                try
                {
                    var scannerResult = manager.Construct(builder, this);
                    marked = builder.PaperMarkedInfo;
                    if (!scannerResult.Status)
                    {
                        marked.IsSuccess = false;
                        marked.Desc = scannerResult.Description;
                    }
                    else
                    {
                        if (_markedInfoList.Any(t => t.StudentId == marked.StudentId))
                        {
                            marked.IsSuccess = false;
                            marked.Desc = string.Format("学生[{0}]重复", marked.StudentName);
                        }
                        else
                        {
                            _markedResult.Add(builder.MarkingResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.E("批阅异常", ex);
                    marked.IsSuccess = false;
                    marked.Desc = string.Format("{0}", DeyiKeys.UnKonwnError);
                }
                finally
                {
                    i++;
                    watch.Stop();
                    marked.UseTime = Math.Round(watch.ElapsedMilliseconds / 1000M, 2) + "s";
                    watch.Reset();
                    ShowResult(marked);
                    ProcessTo(i * 10);
                }
            }

            InitBar(0, Visibility.Hidden);
            EnabledButton(true);
            ShowMsg();
        }

        #region 进度条控制

        /// <summary>
        /// 初始化进度条
        /// </summary>
        /// <param name="max"></param>
        /// <param name="size"></param>
        /// <param name="vb"></param>
        /// <param name="labelVisibility"></param>
        private void InitBar(double max, Visibility vb, int size = 1, bool labelVisibility = true)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                PBar.Maximum = max * size;
                PBar.Value = 0;
                PBar.Visibility = vb;
                if (!labelVisibility)
                    LblForBar.Visibility = Visibility.Hidden;
                else
                {
                    LblForBar.Visibility = Visibility.Visible;
                    LblForBar.Visibility = vb;
                    LblForBar.Content = "1/" + max;
                    LblForBar.Tag = size;
                }
            }));
        }

        private void ProcessTo(int val)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                PBar.Value = val;
                if (LblForBar.Tag != null)
                {
                    var size = (int) LblForBar.Tag;
                    LblForBar.Content = string.Format("{0}/{1}", Math.Round(PBar.Value/size) + 1, PBar.Maximum/size);
                }
            }));
        }

        private void ChangeBar(int val)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                PBar.Value += val;
                if (LblForBar.Tag != null)
                {
                    var size = (int) LblForBar.Tag;
                    LblForBar.Content = string.Format("{0}/{1}", Math.Round(PBar.Value/size) + 1, PBar.Maximum/size);
                }
            }));
        }

        #endregion

        private void btnOperate_Click(object sender, RoutedEventArgs e)
        {
            if (_uploadRunning)
            {
                WindowsHelper.ShowQuestion("正在上传，暂时不能操作...");
                return;
            }
            _isModify = true;
            var btn = sender as System.Windows.Controls.Button;
            if (btn == null) return;

            var resultId = btn.Tag.ToString();

            var markedInfo = _markedInfoList.First(m => m.MarkedResultId == resultId);

            if (markedInfo.IsSuccess)
            {
                if (_batchDetail == null)
                {
                    WindowsHelper.ShowQuestion("未能找到该试卷的相关信息");
                }

                var marking = new MarkingSubjective(markedInfo, _batchDetail, ref _markedResult, _kind);
                marking.ObtianDirctoryHandler += marking_ObtianDirctoryHandler;

                if (marking.ShowDeyiDialog(this) == true)
                {
                    Statistics(_markedResult);
                }
            }
            else
            {
                WindowsHelper.ShowQuestion("不能操作为成功的试卷");
            }
        }

        private object[] marking_ObtianDirctoryHandler(bool isNext, string resultId)
        {
            var objs = new object[2];
            var current = _markedInfoList.First(m => m.MarkedResultId == resultId);
            PaperMarkedInfo marked = null;
        NextAgain:
            var index = _markedInfoList.IndexOf(current);
            if (isNext)
            {
                if (index < _markedInfoList.Count - 1)
                {
                    marked = _markedInfoList.ElementAtOrDefault(index + 1);
                }
            }
            else
            {
                if (index > 0)
                {
                    marked = _markedInfoList.ElementAtOrDefault(index - 1);
                }
            }
            if (marked != null)
            {
                if (!marked.IsSuccess)
                {
                    current = marked;
                    marked = null;
                    goto NextAgain;
                }
                objs[0] = marked;

                if (marked.BacthCode != current.BacthCode)
                {
                    objs[1] = _batchDetail;
                }
            }
            return objs;
        }

        private void SavePaper_Click(object sender, RoutedEventArgs e)
        {
            if (SaveResult())
                WindowsHelper.ShowMsg("批阅结果已保存成功！");
        }

        private bool SaveResult()
        {
            _isSaved = true;
            if (!_markedInfoList.Any(t => t.IsSuccess))
            {
                WindowsHelper.ShowMsg("没有可以保存的阅卷信息！");
                return false;
            }
            Helper.SaveMarkedResult(_kind, _usage.Batch + _usage.PrintType, _markedInfoList, _markedResult);
            return true;
        }

        private void LoadOldData()
        {
            _markedInfoList = Helper.LoadMarkedResult(_kind, _usage.Batch + _usage.PrintType, out _markedResult);
            if (_markedInfoList.Any())
            {
                for (var i = 0; i < _markedInfoList.Count; i++)
                {
                    _markedInfoList[i].Index = i + 1;
                }
                ShowMsg();
            }
        }

        /// <summary>
        /// 上传阅卷
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadPaper_Click(object sender, RoutedEventArgs e)
        {
            if (!_markedInfoList.Any())
            {
                WindowsHelper.ShowMsg(DeyiKeys.NoMarkedInfoError);
                return;
            }
            if (_markedInfoList.Any(md => !md.IsMarked))
            {
                WindowsHelper.ShowMsg(DeyiKeys.HasNotMarkedError);
                return;
            }
            if (_markedInfoList.All(t => t.IsMarkedSuccess.HasValue && t.IsMarkedSuccess.Value))
            {
                WindowsHelper.ShowMsg(DeyiKeys.AllMarkedUpdate);
                return;
            }
            var showQuestion = false;
            var questionMsg = new List<string>();
            if (_markedInfoList.Count(t => t.IsSuccess) < _usage.StudentCount)
            {
                showQuestion = true;
                questionMsg.Add("阅卷的人数少于班级人数！");
            }
            if (!_hasPrint)
            {
                showQuestion = true;
                questionMsg.Add("尚未批量套打，若确认提交，在提交成功之后将会删除本地批阅记录，可能会导致无法批量套打！");
            }

            if (showQuestion)
            {
                questionMsg.Add("是否继续？");
                if (!WindowsHelper.ShowQuestion(string.Join("\r\n", questionMsg)))
                    return;
            }

            string path = UserTempPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var tempPath = Path.Combine(path, string.Format("{0}.dayeasy", _markingKey));
            try
            {
                SaveResult();
                ThreadPool.QueueUserWorkItem(UploadMarkedPaper, false);
            }
            catch (Exception ex)
            {
                WindowsHelper.DeleteFiles(tempPath);
                _logger.E("上传阅卷结果", ex);
            }
        }

        /// <summary>
        /// 上传阅卷
        /// </summary>
        /// <param name="arg"></param>
        private void UploadMarkedPaper(object arg)
        {
            try
            {
                EnabledButton(false);
                _uploadRunning = true;

                InitBar(_markedInfoList.Count + 5, Visibility.Visible, 1, false);

                //只上传未成功的~
                var markedList = _markedInfoList.Where(t => !t.IsMarkedSuccess.HasValue || !t.IsMarkedSuccess.Value);
                var results = new List<MarkingResult>();

                foreach (var item in markedList)
                {
                    var resultItem = _markedResult.FirstOrDefault(m => m.Id == item.MarkedResultId);
                    if (resultItem == null)
                        continue;
                    if (!item.IsUpload)
                    {
                        WindowsHelper.SetMarkedSymbols(resultItem, item.PaperName);
                        Helper.PacketMarkedPicture(item.PaperName, item.BacthCode);
                        var data = new PaperMarkingFileData
                        {
                            Data = File.OpenRead(Path.Combine(DeyiKeys.CompressedPath, item.BacthCode, item.PaperName))
                        };
                        FileResult fileResult = RestHelper.Instance.UpdateFile(data);
                        //fileResult = new FileResult { state = 1, urls = new List<string> { "http://ddd.com/xxxxxx" } };
                        if (fileResult == null || fileResult.state == 0) continue;
                        item.IsUpload = true;
                        //设置上传后的url路径
                        resultItem.Details.ForEach(d =>
                        {
                            if (!string.IsNullOrWhiteSpace(d.StudentAnswerSnapshot))
                                d.StudentAnswerSnapshot = fileResult.urls[0] + "/" + d.StudentAnswerSnapshot;
                        });
                    }
                    //if (IsClose.IsChecked ?? false)
                    //{
                    //    resultItem.IsClose = true;
                    //}
                    ChangeBar(1);
                    results.Add(resultItem);
                }

                JsonResults<MarkedResult> result = RestHelper.Instance.MarkingResult(results);
                if (result != null && result.Status)
                {
                    ChangeBar(5);
                    foreach (var markingResult in result.Data)
                    {
                        var item = _markedInfoList.FirstOrDefault(t => t.MarkedResultId == markingResult.Id);
                        if (item == null) continue;
                        item.IsMarkedSuccess = markingResult.Status;
                        item.MarkedMessage = markingResult.Message;
                    }
                    var succ = result.Data.Count(t => t.Status);
                    var msg = string.Format("上传成功{0}份，失败{1}份，详情请查看列表信息！", succ,
                        result.TotalCount - succ);
                    WindowsHelper.ShowMsg(msg);
                    ////关闭作业
                    //if (WindowsHelper.ShowQuestion(msg))
                    //{
                    //    RestHelper.Instance.CloseUsage(_usage.Batch);
                    //}
                    Helper.DeleteSave(_kind, _usage.Batch + _usage.PrintType);
                }
                else
                {
                    WindowsHelper.ShowError("提交阅卷信息失败&_&." + (result == null ? "" : result.Description));
                }
            }
            catch (Exception ex)
            {
                _logger.E("上传阅卷结果", ex);
                WindowsHelper.ShowError("上传阅卷结果失败~，请稍后重试！");
            }
            finally
            {
                InitBar(0, Visibility.Hidden, 1, false);
                EnabledButton(true);
                _uploadRunning = false;
            }
        }

        /// <summary>
        /// 重新统计分数，及批阅状态
        /// </summary>
        /// <param name="markeds"></param>
        private void Statistics(IEnumerable<MarkingResult> markeds)
        {
            var markByBatch = markeds.GroupBy(md => md.Batch);
            foreach (var grop in markByBatch)
            {
                foreach (var item in grop)
                {
                    PaperMarkedInfo info = _markedInfoList.FirstOrDefault(mi => mi.MarkedResultId == item.Id) ??
                                           new PaperMarkedInfo();
                    if (!info.IsMarked)
                    {
                        info.IsMarked = item.IsFinished;
                    }
                    info.TotalScore = item.TotalScore = item.Details.Sum(t => t.Score);
                }
            }
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as System.Windows.Controls.Button;
                if (btn == null) return;
                var resultId = btn.Tag.ToString();
                var marking = _markedResult.FirstOrDefault(m => m.Id == resultId);
                var size = (((ComboBoxItem)ScannerType.SelectedItem).Content.ToString() == "单面" ? 1 : 2);
                if (marking != null)
                    WindowsHelper.PrintMarkingResult(new List<MarkingResult> { marking }, size);
            }
            catch (Exception ex)
            {
                WindowsHelper.ShowError(ex.Message);
            }
        }

        private void BtnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowsHelper.ShowQuestion("是否确认删除所有识别失败的试卷？"))
                return;
            var fails = _markedInfoList.Where(t => !t.IsSuccess).ToList();
            foreach (var info in fails)
            {
                DeleteMarking(info);
            }
            ShowMsg();
        }

        /// <summary>
        /// 一键打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrintAll_Click(object sender, RoutedEventArgs e)
        {
            if (_markedResult.Count(m => m.IsFinished) < 1)
            {
                WindowsHelper.ShowError("存在未批阅过的试卷，请先批阅试卷...");
                return;
            }
            if (!WindowsHelper.ShowQuestion("批量套打需要保证试卷从打印机出来的顺序一致！"))
            {
                return;
            }
            EnabledButton(false);
            try
            {
                var size = (((ComboBoxItem)ScannerType.SelectedItem).Content.ToString() == "单面" ? 1 : 2);
                var result = WindowsHelper.PrintMarkingResult(_markedResult, size);
                if (!result.Status)
                {
                    WindowsHelper.ShowError(result.Description);
                }
                else
                {
                    _hasPrint = true;
                }
            }
            finally
            {
                EnabledButton(true);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowsHelper.ShowQuestion("确认删除该阅卷结果？"))
                return;
            var btn = sender as System.Windows.Controls.Button;
            if (btn == null || btn.Tag == null) return;
            var id = btn.Tag.ToString();
            var tempInfo = _markedInfoList.FirstOrDefault(m => m.MarkedResultId == id);
            DeleteMarking(tempInfo);
            ShowMsg();
        }

        private void DeleteMarking(PaperMarkedInfo tempInfo)
        {
            if (tempInfo == null)
                return;
            if (string.IsNullOrWhiteSpace(tempInfo.PaperName))
                return;
            _markedInfoList.Remove(tempInfo);
            var path = Path.Combine(DeyiKeys.SavePath, tempInfo.PaperName);
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch{}
            }
            var item = _markedResult.FirstOrDefault(m => m.Id == tempInfo.MarkedResultId);
            if (item == null) return;
            _markedResult.Remove(item);
        }

        private void EnabledButton(bool isDisable)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                //BtnImport.IsEnabled = isDisable;
                ScannerType.IsEnabled = isDisable;
                BtnPrintAll.IsEnabled = isDisable;
                BtnScanner.IsEnabled = isDisable;
                BtnDeleteAll.IsEnabled = isDisable;
                BtnScan.IsEnabled = isDisable;
                UploadPaper.IsEnabled = isDisable;
                SavePaper.IsEnabled = isDisable;
                Sequence.IsEnabled = isDisable;
                Reverse.IsEnabled = isDisable;
                ScannerType.IsEnabled = isDisable;
            }));
        }

        private void ShowMsg()
        {
            TxtTip.Dispatcher.Invoke(new Action(() =>
            {
                int success = _markedInfoList.Count(t => t.IsSuccess),
                    error = _markedInfoList.Count(t => !t.IsSuccess);
                TxtTip.Text = string.Format("阅卷结果：总试卷数{0}张，成功{1}张，失败{2}张。", _markedInfoList.Count, success, error);
            }));
        }

        private void Reverselist()
        {
            int i = 0, count = _markedInfoList.Count;
            while (i < count)
            {
                i++;
                _markedInfoList.Move(0, count - i);
            }
            _markedResult.Reverse();
            EnabledButton(true);
        }

        private void sequence_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                EnabledButton(false);
                Reverselist();
            }
        }

        private void reverse_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                EnabledButton(false);
                Reverselist();
            }
        }

        /// <summary>
        /// 查看试卷
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSee_Click(object sender, RoutedEventArgs e)
        {
            var btn = (System.Windows.Controls.Button)sender;
            var win = new PaperImageDetail(new System.Windows.Media.Imaging.BitmapImage(new Uri(btn.Tag.ToString())));
            win.ShowDeyiDialog();
        }

        private void HeaderClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader)
            {
                //获得点击的列  
                GridViewColumn clickedColumn = (e.OriginalSource as GridViewColumnHeader).Column;
                //数据源集合为ObservableCollection<T>类型，保证数据源和ListView试图同步。
                var columns = new[] { "序号", "客观题(错/总)", "总得分" };
                var columnName = clickedColumn == null ? string.Empty : clickedColumn.Header.ToString();
                if (string.IsNullOrWhiteSpace(columnName) || !columns.Contains(columnName))
                    return;
                var tempList = _markedInfoList.ToList();
                var isDesc = false;
                var lamda = new Func<PaperMarkedInfo, decimal>(t => t.Index);
                switch (columnName)
                {
                    case "序号":
                        _sortType = (_sortType == SortType.Default
                            ? SortType.IndexDesc
                            : SortType.Default);
                        isDesc = _sortType == SortType.IndexDesc;
                        break;
                    case "客观题(错/总)":
                        lamda = info => info.ErorrCount;
                        _sortType = (_sortType == SortType.ErrorCount
                            ? SortType.ErrorCountDesc
                            : SortType.ErrorCount);
                        isDesc = _sortType == SortType.ErrorCountDesc;
                        break;
                    case "总得分":
                        lamda = info => info.TotalScore;
                        _sortType = (_sortType == SortType.Score
                            ? SortType.ScoreDesc
                            : SortType.Score);
                        isDesc = _sortType == SortType.ScoreDesc;
                        break;
                }
                tempList = (isDesc
                    ? tempList.OrderByDescending(lamda).ToList()
                    : tempList.OrderBy(lamda).ToList());
                _markedInfoList.Clear();
                foreach (var markedInfo in tempList)
                {
                    _markedInfoList.Add(markedInfo);
                }
                _markedResult.Sort((x, y) =>
                {
                    var infoX = _markedInfoList.FirstOrDefault(t => t.MarkedResultId == x.Id);
                    var infoY = _markedInfoList.FirstOrDefault(t => t.MarkedResultId == y.Id);
                    return _markedInfoList.IndexOf(infoX).CompareTo(_markedInfoList.IndexOf(infoY));
                });
            }
        }
    }
}
