#define export

using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Data;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.MarkingTool.BLL.Scanner;
using DayEasy.MarkingTool.Core;
using DayEasy.Models.Open.Paper;
using DayEasy.Models.Open.Work;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;

namespace DayEasy.MarkingTool.UI.Scanner
{
    /// <summary> 扫描页面 </summary>
    public partial class MarkingPaper
    {
        #region 私有变量
        private readonly Logger _logger = Logger.L<MarkingPaper>();

        /// <summary> 识别结果 </summary>
        private ObservableCollection<PaperMarkedInfo> _markedInfoList = new ObservableCollection<PaperMarkedInfo>();

        //        private ScannerBuilderManager _scannerBuilder;
        private List<string> _paperPathsList;
        /// <summary> 合并试卷数 </summary>
        private int _combineCount = 1;
        private bool _isSingle = true;

        /// <summary> 扫描结果 </summary>
        private static MPictureList _markingInfo;

        /// <summary> 是否已保存 </summary>
        private bool _isSaved;

        /// <summary> 是否已修改 </summary>
        private bool _isModify;

        /// <summary> 是否正在上传 </summary>
        private bool _uploadRunning;

        private readonly MPaperDto _paperInfo;
        private readonly FileManager _fileManager;
        private MJointUsageDto _jointUsage;

        private PaperScanner _paperScanner;
        private Stopwatch _watcher;
        private const int MaxCount = 1000;
        private bool _windowLocked = false;

        #endregion

        #region 构造函数
        public MarkingPaper(MPaperDto paperInfo)
        {
            CloseWindow = true;
            _paperInfo = paperInfo;
            _fileManager = new FileManager(DeyiApp.UserId, paperInfo.Id);
            InitializeComponent();
            PBar.Visibility = LblForBar.Visibility = Visibility.Hidden;
            LblPaperTitle.Text = _paperInfo.PaperTitle;
            LblPaperTitle.Tag = _paperInfo.PaperTitle;
            List.MouseDoubleClick += List_MouseDoubleClick;
            ScannerType.SelectionChanged += ScannerType_SelectionChanged;
            ThreadPool.SetMaxThreads(4, 2);
        }
        #endregion

        #region Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _markedInfoList.Clear();
#if !export
            BtnExport.Visibility = Visibility.Hidden;
#endif
            _markingInfo = new MPictureList
            {
                UserId = DeyiApp.UserId,
                PaperId = _paperInfo.Id,
                PaperNum = _paperInfo.PaperNo,
                SubjectId = _paperInfo.SubjectId,
                Pictures = new List<MPictureInfo>()
            };
            if (_paperInfo.PaperType == (byte)PaperType.PaperAb)
            {
                SectionType.Visibility = Visibility.Visible;
                SectionType.SelectedIndex = 0;
                LblType.Content = "AB卷";
            }
            else
            {
                SectionType.Visibility = Visibility.Collapsed;
            }
            _paperPathsList = new List<string>();
            LoadOldData();
            var usages = RestHelper.Instance.JointUsages(_paperInfo.Id);
            if (usages.Status && usages.Data.Any())
            {
                BtnScanner.IsEnabled = false;
                UploadPaper.IsEnabled = false;
                BtnOpen.IsEnabled = false;
                foreach (var dto in usages.Data)
                {
                    JointUsage.Items.Add(new ComboBoxItem
                    {
                        Content = string.Format("{0}[{1}] - {2}", dto.GroupName, dto.GroupCode, dto.UserName),
                        DataContext = dto
                    });
                }

                #region 更换阅卷方式

                JointUsage.SelectionChanged += (obj, args) =>
                {
                    if (JointUsage.SelectedIndex == 0)
                    {
                        BtnScanner.IsEnabled = false;
                        UploadPaper.IsEnabled = false;
                        BtnOpen.IsEnabled = false;
                        return;
                    }
                    if (JointUsage.SelectedIndex > 1)
                    {
                        var boxItem = JointUsage.SelectedItem as ComboBoxItem;
                        if (boxItem != null)
                        {
                            var usage = boxItem.DataContext as MJointUsageDto;
                            if (usage != null)
                            {
                                _jointUsage = usage;
                                //协同阅卷班级检测
                                if (_markingInfo != null && _markingInfo.Pictures.Any())
                                {
                                    foreach (var info in _markingInfo.Pictures)
                                    {
                                        if (!usage.ClassList.Contains(info.GroupId))
                                        {
                                            var marked = _markedInfoList.FirstOrDefault(t => t.MarkedId == info.Id);
                                            if (marked != null)
                                            {
                                                marked.IsSuccess = false;
                                                marked.Desc = "班级不在协同范围内";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (_markedInfoList.Any(t => t.IsSuccess))
                        UploadPaper.IsEnabled = true;
                    BtnScanner.IsEnabled = true;
                    JointUsage.IsEnabled = false;
                    BtnOpen.IsEnabled = true;
                };

                #endregion
            }
            else
            {
                JointUsage.Visibility = Visibility.Collapsed;
            }
            List.ItemsSource = _markedInfoList;
            List.SetDefault("Index");
            List.Sort();
            SizeChanged += (obj, arg) =>
            {
                var gv = List.View as GridView;
                if (gv == null || gv.Columns.Count < 4) return;
                gv.Columns[3].Width = Width - 800;
            };
            //            Closing += MarkingPaper_Closing;
            //            Closed += MarkingPaper_Closed;
        }

        private void LoadOldData()
        {
            List<MPictureInfo> pictures;
            _markedInfoList = _fileManager.LoadMarkedResult(out pictures);
            if (!_markedInfoList.Any())
                return;
            BtnDeleteAll.IsEnabled = true;
            if (_markedInfoList.Any(t => t.IsSuccess && !(t.IsMarkedSuccess.HasValue && t.IsMarkedSuccess.Value)))
                UploadPaper.IsEnabled = true;
            for (var i = 0; i < _markedInfoList.Count; i++)
            {
                _markedInfoList[i].Index = i + 1;
                //                CheckSheet(_markedInfoList[i], pictures.First(t => t.Id == _markedInfoList[i].MarkedId).SheetAnwers);
            }
            _markingInfo.Pictures = pictures;
#if export
            if (_markingInfo.Pictures.Any())
                BtnExport.IsEnabled = true;
#endif
            ShowMsg();
        }
        #endregion

        #region 事件处理

        /// <summary> 选择扫描类型 </summary>
        private void ScannerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var type = ScannerType.SelectedIndex;
            if (type == 0)
            {
                ComboPage.SelectedIndex = 0;
                foreach (ComboBoxItem item in ComboPage.Items)
                {
                    item.IsEnabled = true;
                }
            }
            else
            {
                ComboPage.SelectedIndex = 1;
                foreach (ComboBoxItem item in ComboPage.Items)
                {
                    item.IsEnabled = (Helper.ToInt(item.DataContext, 1) % 2 == 0);
                }
            }
        }

        /// <summary> 查看图片 </summary>
        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = List.SelectedItem as PaperMarkedInfo;
            if (item == null)
                return;
            var img = _fileManager.GetImagePath(Path.GetFileName(item.ImagePath));
            if (!File.Exists(img))
            {
                WindowsHelper.ShowError("图片已被删除！");
                return;
            }
            var win = new PaperImageDetail(img);
            win.DeyiWindow(this, hideOwner: false);
        }


        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            ShowPicture();
        }

        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            var setting = new Setting(this);
            setting.DeyiWindow(this, hideOwner: false);
        }

        #region 驱动扫描仪直接扫描

        private void btnScanner_Click(object sender, RoutedEventArgs e)
        {
            EnabledButton(false);
            try
            {
                var type = Helper.ToInt(((ComboBoxItem)ScannerType.SelectedItem).DataContext, 1);
                _isSingle = (type == 1);
                if (!ScannerHelper.CheckOcx())
                {
                    if (!ScannerHelper.RegistOcx())
                    {
                        WindowsHelper.ShowError("组件注册失败！");
                        //if (WindowsHelper.ShowQuestion("需要重新启动扫描页，是否重启？"))
                        //    Close();
                        EnabledButton(true);
                        return;
                    }
                }
                _paperPathsList.Clear();
                using (var helper = new ScannerHelper(Wrap, Guid.NewGuid().ToString("N").Substring(5, 6), _isSingle))
                {

                    var list = helper.Scnaner();
                    if (list == null || !list.Any())
                    {
                        EnabledButton(true);
                        return;
                    }
                    _paperPathsList = list;
                    if (_isSingle)
                    {
                        FinishedScanner(helper);
                    }
                    else
                    {
                        StartRecognition();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.E("扫描异常:" + ex.Message, ex);
            }
            finally
            {
                EnabledButton(true);
            }
        }

        private void FinishedScanner(ScannerHelper helper)
        {
            //StartRecognition();
            if (WindowsHelper.ShowSure("扫描已完成,是否开始识别?", "开始识别", "继续扫描"))
            {
                StartRecognition();
            }
            else
            {
                if (WindowsHelper.ShowSure("请将试卷放置好后，点击【开始扫描】！", "开始扫描", "取消扫描"))
                {
                    _paperPathsList.AddRange(helper.Scnaner(_paperPathsList.Count + 1));
                    FinishedScanner(helper);
                }
                else
                {
                    StartRecognition();
                }
            }
        }

        /// <summary> 开始识别 </summary>
        private void StartRecognition()
        {
            _paperPathsList = _paperPathsList.Distinct().ToList();
            UiInvoke(() =>
            {
                _combineCount = Helper.ToInt((((ComboBoxItem)ComboPage.SelectedItem).DataContext), 1);
                var sectionType = 0;
                if (_paperInfo.PaperType == (byte)PaperType.PaperAb)
                {
                    if (!SectionType.IsVisible)
                    {
                        WindowsHelper.ShowError("试卷类型不匹配~！");
                        return;
                    }
                    sectionType = SectionType.SelectedIndex + 1;
                }
                _paperScanner = new PaperScanner(_paperInfo, _fileManager, (byte)sectionType);
                BeginPorgress();
            });
        }

        #endregion

        /// <summary> 删除 </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;
            if (!WindowsHelper.ShowQuestion("确认删除该阅卷结果？"))
            {
                return;
            }
            var id = btn.Tag.ToString();
            DeleteMarking(m => m.MarkedId == id);
            ShowMsg();
        }

        /// <summary> 查看试卷 </summary>
        private void btnSee_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
                return;
            var imgPath = btn.Tag.ToString();
            var img = _fileManager.GetImagePath(Path.GetFileName(imgPath));
            if (!File.Exists(img))
            {
                WindowsHelper.ShowError("图片已被删除！");
                return;
            }
            var win = new PaperImageDetail(img);
            win.DeyiWindow(this);
        }

        /// <summary> 重新识别 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;
            if (!WindowsHelper.ShowQuestion("确认重新识别？"))
                return;
            var id = btn.Tag.ToString();
            var markedInfo = _markedInfoList.FirstOrDefault(t => t.MarkedId == id);
            if (markedInfo == null) return;
            RescannerMission(markedInfo);
        }

        private void BtnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowsHelper.ShowSure("是否清空所有试卷？"))
                return;
            _markedInfoList.Clear();
            _markingInfo.Pictures.Clear();
            _fileManager.ClearFile(true);
            using (var utils = new CacheUtils())
            {
                utils.ClearScanner(DeyiApp.UserId, _paperInfo.Id);
            }
            ShowMsg();
            BtnDeleteAll.IsEnabled = false;
            UploadPaper.IsEnabled = false;
        }

        private void BtnSheetEdit(object sender, MouseButtonEventArgs e)
        {
            if (_windowLocked) return;
            _windowLocked = true;
            var id = ((TextBlock)sender).Tag.ToString();
            var picture = _markingInfo.Pictures.FirstOrDefault(t => t.Id == id);
            if (picture == null)
                return;
            var objectives = ObjectiveHelper.GetObjectives(_paperInfo, picture.SectionType);
            var name = picture.StudentName;
            if (!picture.IsSuccess)
                name = "学生" + picture.Index;
            var win = new SheetEdit(picture.Id, picture.SheetAnwers, name, objectives);
            win.DeyiWindow(this);
            _windowLocked = false;
        }

        private void BindStudentInfo(object sender, MouseButtonEventArgs e)
        {
            var id = ((TextBlock)sender).Tag.ToString();
            ShowBindStudent(id);
        }

        private void BindStudentHandler(object sender, RoutedEventArgs e)
        {
            var id = ((Button)sender).Tag.ToString();
            ShowBindStudent(id);
        }

        private void ShowBindStudent(string id)
        {
            var picture = _markingInfo.Pictures.FirstOrDefault(t => t.Id == id);
            if (picture == null)
                return;
            var marked = _markedInfoList.FirstOrDefault(t => t.MarkedId == picture.Id);
            if (marked == null)
                return;
            var code = marked.StudentCode;
            var existList =
                _markedInfoList.Where(t => t.StudentId > 0 && t.SectionType == marked.SectionType)
                    .Select(t => t.StudentId)
                    .ToList();
            var win = new BindStudent(picture.Id, this, existList, code);
            win.DeyiDialog(this);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow = false;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_markingInfo != null && _markedInfoList != null && _markedInfoList.Any() && (_isModify || !_isSaved))
            {
                if (!CloseWindow || WindowsHelper.ShowSure("是否保存识别结果？"))
                {
                    _fileManager.SaveMarkedResult(_markedInfoList, _markingInfo.Pictures);
                }
            }
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _markedInfoList.Clear();
            _paperPathsList = null;
            _markingInfo = null;
            base.OnClosed(e);
        }

        #endregion

        #region 选择图片文件
        /// <summary> 选择图片文件 </summary>
        private void ShowPicture()
        {
            using (var utils = new CacheUtils())
            {
                var fbd = new FolderBrowserDialog();
                var root = utils.Get(CacheType.SelectPath, null, 1).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(root))
                    fbd.SelectedPath = root;
                else
                {
                    fbd.RootFolder = Environment.SpecialFolder.Desktop;
                }
                if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                var rootPath = fbd.SelectedPath;
                utils.Set(rootPath, CacheType.SelectPath);
                var picWin = new ImageView(rootPath, _markedInfoList.Select(m => m.PaperName).ToArray());
                if (picWin.DeyiDialog(this) == true)
                {
                    _paperPathsList = picWin.SelectPaths;
                    if (_paperPathsList.Count > MaxCount)
                    {
                        _paperPathsList = _paperPathsList.Take(MaxCount).ToList();
                    }
                    _combineCount = Helper.ToInt((((ComboBoxItem)ComboPage.SelectedItem).DataContext), 1);
                    var sectionType = 0;
                    if (_paperInfo.PaperType == (byte)PaperType.PaperAb)
                    {
                        if (!SectionType.IsVisible)
                        {
                            WindowsHelper.ShowError("试卷类型不匹配~！");
                            return;
                        }
                        sectionType = SectionType.SelectedIndex + 1;
                    }
                    _paperScanner = new PaperScanner(_paperInfo, _fileManager, (byte)sectionType);
                    BeginPorgress();
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
        #endregion

        #region 扫描任务

        /// <summary> 扫描试卷 </summary>
        private void BeginPorgress()
        {
            if (null == _paperPathsList || _paperPathsList.Count < 1)
            {
                return;
            }
            if (_markedInfoList.Count >= MaxCount)
            {
                WindowsHelper.ShowMsg($"已扫描超过{MaxCount}张试卷\r\n请先上传后再进行扫描！");
                return;
            }
            EnabledButton(false);
            _isModify = true;
            var count = (int)Math.Ceiling(_paperPathsList.Count / (double)_combineCount);
            InitBar(count, Visibility.Visible, 10);
            if (_paperInfo == null)
                return;
            _fileManager.InitDirectory();
            _watcher = new Stopwatch();
            _watcher.Start();
            var currentIndex = _markedInfoList.Any() ? _markedInfoList.Max(t => t.Index) : 0;
            for (var i = 0; i < count; i++)
            {
                var imgArr = _paperPathsList.Skip(i * _combineCount).Take(_combineCount).ToList();
                ThreadPool.QueueUserWorkItem(arg =>
                {
                    var arr = (object[])arg;
                    var index = (int)arr[1];
                    var imagePath = _paperScanner.Resize((List<string>)arr[0]);
                    SingleProcess(imagePath, index);
                    TaskFinished();
                }, new object[] { imgArr, ++currentIndex });
            }

        }

        /// <summary> 任务完成 </summary>
        private void TaskFinished()
        {
            UiInvoke(() =>
            {
                var size = (int)(LblForBar.Tag ?? 1);
                var count = PBar.Maximum / size;
                var current = (int)(LblForBar.DataContext ?? 0);
                current++;
                LblForBar.Content = current + "/" + count;
                LblForBar.DataContext = current;
                ShowMsg();
                if (current < count)
                    return;
                _isSaved = false;
                InitBar(0, Visibility.Hidden);
                LblForBar.DataContext = 0;
                EnabledButton(true);
                if (_watcher != null)
                {
                    _watcher.Stop();
                    _logger.I(
                        $"共扫描{count}张试卷，耗时{_watcher.ElapsedMilliseconds}ms,均耗{_watcher.ElapsedMilliseconds / (double)count}ms");
                }
                _watcher = null;
                _fileManager.Dispose();
            });
        }

        /// <summary> 单个扫描任务,支持多张 </summary>
        /// <param name="imagePath"></param>
        /// <param name="index"></param>
        private void SingleProcess(string imagePath, int index)
        {
            var markedInfo = new PaperMarkedInfo(imagePath);
            var picture = new MPictureInfo { Id = null };
            _paperScanner.Scanner(imagePath, markedInfo, picture);
            if (markedInfo.StudentId > 0)
            {
                if (_jointUsage != null && !string.IsNullOrWhiteSpace(picture.GroupId) &&
                    !_jointUsage.ClassList.Contains(picture.GroupId))
                {
                    //协同班级判断
                    markedInfo.IsSuccess = false;
                    markedInfo.Desc = "班级不在协同范围内";
                }
                else
                {
                    var exist = _markedInfoList.Any(
                        t =>
                            t.StudentId > 0 && t.StudentId == markedInfo.StudentId &&
                            t.SectionType == markedInfo.SectionType);
                    if (exist)
                    {
                        markedInfo.IsSuccess = false;
                        markedInfo.Desc = "学生重复";
                    }
                }
            }
            else
            {
                markedInfo.IsSuccess = false;
                markedInfo.Desc = markedInfo.StudentName;
                markedInfo.StudentName = "未识别";
                picture.GroupId = string.Empty;
            }
            picture.IsSingle = _isSingle;
            picture.PageCount = _combineCount;
            picture.Index = index;
            markedInfo.Index = index;
            _markingInfo.Pictures.Add(picture);
            CheckSheet(markedInfo, picture.SheetAnwers);
            ShowResult(markedInfo);
            ChangeBar(10);
            GC.Collect();
        }

        private void RescannerMission(params PaperMarkedInfo[] infos)
        {
            InitBar(infos.Length, Visibility.Visible, 10);
            foreach (var info in infos)
            {
                ThreadPool.QueueUserWorkItem(arg =>
                {
                    SingleReScanner(info);
                    ChangeBar(10);
                    TaskFinished();
                });
            }
        }

        /// <summary> 重新识别 </summary>
        private void SingleReScanner(PaperMarkedInfo markedInfo)
        {
            var picture = _markingInfo.Pictures.FirstOrDefault(t => t.Id == markedInfo.MarkedId);
            if (picture == null)
                return;
            _combineCount = markedInfo.PageCount;
            var sectionType = picture.SectionType;
            var scanner = new PaperScanner(_paperInfo, _fileManager, sectionType);
            scanner.Scanner(markedInfo.ImagePath, markedInfo, picture);
            if (markedInfo.StudentId > 0)
            {
                var exist = _markedInfoList.Any(
                    t =>
                        t.StudentId > 0 && t.MarkedId != markedInfo.MarkedId && t.StudentId == markedInfo.StudentId &&
                        t.SectionType == markedInfo.SectionType);
                if (exist)
                {
                    markedInfo.IsSuccess = false;
                    markedInfo.Desc = "学生重复";
                }
                else if (_jointUsage != null && !string.IsNullOrWhiteSpace(picture.GroupId) &&
                         !_jointUsage.ClassList.Contains(picture.GroupId))
                {
                    //协同班级判断
                    markedInfo.IsSuccess = false;
                    markedInfo.Desc = "班级不在协同范围内";
                }
            }
            else
            {
                markedInfo.IsSuccess = false;
                markedInfo.Desc = markedInfo.StudentName;
                markedInfo.StudentName = "未识别";
                picture.GroupId = string.Empty;
            }
            CheckSheet(markedInfo, picture.SheetAnwers);
            ShowMsg();
        }

        private void ShowResult(PaperMarkedInfo markedInfo)
        {
            UiInvoke(() =>
            {
                _markedInfoList.Add(markedInfo);
            });
        }

        private void ShowMsg()
        {
            TxtTip.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                TxtTip.Visibility = Visibility.Visible;
                var count =
                    _markedInfoList.Count(t => (!t.IsSuccess && t.StudentCode.Length != 5) || t.RatiosColor == "Red");
                TxtTip.Text = string.Format("试卷总数：{0}张，异常：{1}张", _markedInfoList.Count, count);
            }));
        }
        #endregion

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
            UiInvoke(() =>
            {
                PBar.Maximum = max * size;
                PBar.Value = 0;
                PBar.Visibility = vb;
                if (vb == Visibility.Visible)
                {
                    TxtTip.Visibility = Visibility.Hidden;
                }
                if (!labelVisibility)
                    LblForBar.Visibility = Visibility.Hidden;
                else
                {
                    LblForBar.Visibility = vb;
                    LblForBar.Content = "1/" + max;
                    LblForBar.DataContext = 0;
                    LblForBar.Tag = size;
                }
            });
        }

        private void ChangeBar(int val, string msg = null)
        {
            UiInvoke(() =>
            {
                PBar.Value += val;
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    LblForBar.Visibility = Visibility.Visible;
                    LblForBar.Content = msg;
                    return;
                }
                if (LblForBar.Tag == null)
                {
                    LblForBar.Visibility = Visibility.Hidden;
                    return;
                }
                var size = (int)LblForBar.Tag;
                LblForBar.Content = string.Format("{0}/{1}", Math.Round(PBar.Value / size) + 1, PBar.Maximum / size);
            });
        }

        #endregion

        #region 上传试卷
        /// <summary> 上传阅卷 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadPaper_Click(object sender, RoutedEventArgs e)
        {
            if (!_markedInfoList.Any())
            {
                WindowsHelper.ShowMsg(DeyiKeys.NoMarkedInfoError);
                return;
            }
            if (_markedInfoList.All(t => t.IsMarkedSuccess.HasValue && t.IsMarkedSuccess.Value))
            {
                WindowsHelper.ShowMsg(DeyiKeys.AllMarkedUpdate);
                return;
            }
            if (_markedInfoList.Any(t => !t.IsSuccess))
            {
                if (!WindowsHelper.ShowSure(DeyiKeys.ExistsFail))
                    return;
            }
            try
            {
                //SaveResult();
                ThreadPool.QueueUserWorkItem(UploadMarkedPaper, false);
            }
            catch (Exception ex)
            {
                _logger.E("上传阅卷结果", ex);
                RestHelper.Instance.SendEmailAsync("上传试卷异常", ex);
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
                if (_uploadRunning)
                    return;
                _uploadRunning = true;

                //只上传未成功的~
                var markedList =
                    _markedInfoList.Where(t => t.IsSuccess &&
                        !(t.IsMarkedSuccess.HasValue && t.IsMarkedSuccess.Value))
                        .ToList();
                var results = new MPictureList
                {
                    UserId = _markingInfo.UserId,
                    PaperId = _markingInfo.PaperId,
                    PaperNum = _markingInfo.PaperNum,
                    SubjectId = _paperInfo.SubjectId,
                };
                if (_jointUsage != null)
                {
                    results.JointBatch = _jointUsage.JointBatch;
                    results.GroupId = _jointUsage.GroupId;
                }

                //上传图片
                var pictures = markedList.Where(t => !t.IsUpload).ToList();
                if (!pictures.Any())
                {
                    UiInvoke(() => WindowsHelper.ShowError("所有试卷均已上传！"));
                    return;
                }
                if ((_paperInfo.PaperType == (byte)PaperType.Normal && _markingInfo.Pictures.Any(t => t.SectionType != 0)) ||
                    (_paperInfo.PaperType == (byte)PaperType.PaperAb && _markingInfo.Pictures.Any(t => t.SectionType == 0)))
                {
                    UiInvoke(() => WindowsHelper.ShowError("试卷类型不匹配！"));
                    return;
                }
                InitBar(10, Visibility.Visible, 1, false);
                ChangeBar(0, "打包试卷...");
                var zip = _fileManager.PacketPictures(pictures.Select(t => t.PaperName));
                ChangeBar(2, "上传试卷...");
                var data = new PaperMarkingFileData
                {
                    Data = File.OpenRead(zip)
                };
                var fileResult = RestHelper.Instance.UpdateFile(data);
                if (fileResult == null || fileResult.state == 0)
                {
                    UiInvoke(() => WindowsHelper.ShowError("试卷上传失败！"));
                    _logger.I("试卷上传" + (fileResult != null ? fileResult.msg : string.Empty));
                    return;
                }
                ChangeBar(5, "上传扫描结果...");

                var index = 0;
                foreach (var info in pictures.OrderBy(p => p.Index))
                {
                    var item = _markingInfo.Pictures.FirstOrDefault(t => t.Id == info.MarkedId);
                    if (item == null)
                        continue;
                    item.ImagePath = string.Format("{0}/{1}", fileResult.urls[0], Path.GetFileName(info.ImagePath));
                    item.Index = index;
                    results.Pictures.Add(item);
                    index++;
                }
                if (results.Pictures == null || !results.Pictures.Any())
                {
                    UiInvoke(() => WindowsHelper.ShowError("没有需要上传的试卷！"));
                    return;
                }

                var result = RestHelper.Instance.HandinPictures(results);
                if (result != null && result.Status)
                {
                    ChangeBar(3, "上传完成");
                    foreach (var markingResult in result.Data)
                    {
                        var item = _markedInfoList.FirstOrDefault(t => t.MarkedId == markingResult.Id);
                        if (item == null) continue;
                        item.IsMarkedSuccess = markingResult.Status;
                        item.MarkedMessage = markingResult.Message;
                    }
                    var succ = result.Data.Count(t => t.Status);
                    var msg = string.Format("上传成功{0}份,是否立即批阅？", succ);
                    _isSaved = true;
                    RemoveResults();
                    UiInvoke(() =>
                    {
                        if (!WindowsHelper.ShowQuestion(msg))
                            return;
                        try
                        {
                            Process.Start(DeyiKeys.MarkingConfig.MarkingUrl);
                        }
                        catch
                        {
                            WindowsHelper.ShowError("打开浏览器失败！");
                        }
                    });
                }
                else
                {
                    var message = result == null ? "上传失败~！" : result.Message;
                    WindowsHelper.ShowError(message);
                }
            }
            catch (Exception ex)
            {
                _logger.E("上传试卷", ex);
                RestHelper.Instance.SendEmailAsync("上传试卷异常", ex);
                WindowsHelper.ShowError("上传结果失败~，请稍后重试！");
            }
            finally
            {
                InitBar(0, Visibility.Hidden, 1, false);
                EnabledButton(true);
                _uploadRunning = false;
            }
        }

        /// <summary> 逐渐删除成功的~ </summary>
        private void RemoveResults()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        var item =
                            _markedInfoList.FirstOrDefault(
                                t => t.IsMarkedSuccess.HasValue && t.IsMarkedSuccess.Value);
                        if (item == null)
                            break;
                        var pic = _markingInfo.Pictures.FirstOrDefault(t => t.Id == item.MarkedId);
                        if (pic != null)
                            _markingInfo.Pictures.Remove(pic);
                        UiInvoke(() =>
                        {
                            _fileManager.DeleteMarkingDirectory(new[] { item.PaperName });
                            _markedInfoList.Remove(item);
                        });
                        ShowMsg();
                        Thread.Sleep(200);
                    }
                    catch (Exception ex)
                    {
                        _logger.E("逐条删除异常", ex);
                    }
                }
                using (var utils = new CacheUtils())
                {
                    utils.SaveScanner(DeyiApp.UserId, _paperInfo.Id, _markedInfoList,
                        _markingInfo.Pictures);
                }
            });
        }
        #endregion

        #region 删除扫描结果
        /// <summary> 删除扫描结果 </summary>
        private void DeleteMarking(Func<PaperMarkedInfo, bool> condition = null)
        {
            if (condition == null)
            {
                _markedInfoList.Clear();
                _markingInfo.Pictures.Clear();
                _isModify = true;
                return;
            }
            var list = _markedInfoList.Where(condition).ToList();
            if (!list.Any())
                return;
            var deleteDict =
                list.Where(t => t.StudentId > 0 && t.IsSuccess)
                    .Select(t => new { id = t.StudentId, type = t.SectionType });
            foreach (PaperMarkedInfo tempInfo in list)
            {
                if (tempInfo == null)
                    continue;
                _fileManager.DeleteMarkingDirectory(new[] { tempInfo.PaperName });
                _markedInfoList.Remove(tempInfo);
                var item = _markingInfo.Pictures.FirstOrDefault(m => m.Id == tempInfo.MarkedId);
                if (item == null)
                    continue;
                _markingInfo.Pictures.Remove(item);
            }
            //更新删除之后的状态
            foreach (var deleted in deleteDict)
            {
                var item = deleted;
                var temp = _markedInfoList.FirstOrDefault(t => t.StudentId == item.id && t.SectionType == item.type);
                if (temp != null)
                {
                    temp.IsSuccess = true;
                    temp.Desc = string.Empty;
                }
            }
            ResetUploadBtn();
            _isModify = true;
        }
        #endregion

        #region 按钮重置
        private void EnabledButton(bool isEnabled, bool classCheck = false)
        {
            UiInvoke(() =>
            {
                ScannerType.IsEnabled = isEnabled;
                BtnScanner.IsEnabled = isEnabled;
                BtnOpen.IsEnabled = isEnabled;
                BtnSetting.IsEnabled = isEnabled;
                BtnBack.IsEnabled = isEnabled || classCheck;
                ComboPage.IsEnabled = isEnabled;
                SectionType.IsEnabled = isEnabled;
                ScannerType.IsEnabled = isEnabled;
                if (!_markedInfoList.Any())
                    isEnabled = false;
                BtnDeleteAll.IsEnabled = isEnabled;
                ResetUploadBtn(isEnabled);
            });
        }

        /// <summary> 重置上传按钮状态 </summary>
        private void ResetUploadBtn(bool enable = true)
        {
            if (!enable)
                UploadPaper.IsEnabled = false;
            else
            {
                UploadPaper.IsEnabled =
                    (_markedInfoList != null &&
                     _markedInfoList.Any(
                         t => t.IsSuccess
                              && !(t.IsMarkedSuccess.HasValue && t.IsMarkedSuccess.Value)));
            }
#if export
            BtnExport.IsEnabled = _markingInfo.Pictures.Any();
#endif
        }
        #endregion

        #region 接口

        private void CheckSheet(PaperMarkedInfo info, IList<int[]> sheets)
        {
            if (info == null || sheets == null || !sheets.Any())
                return;
            ObjectiveHelper.CheckAnswer(_paperInfo, info, sheets);
        }

        /// <summary> 手动修改答题卡 </summary>
        /// <param name="id"></param>
        /// <param name="sheets"></param>
        public void SetSheets(string id, IList<int[]> sheets)
        {
            var picture = _markingInfo.Pictures.FirstOrDefault(t => t.Id == id);
            var info = _markedInfoList.FirstOrDefault(t => t.MarkedId == id);
            if (picture == null || info == null)
                return;
            picture.SheetAnwers = sheets;
            info.Ratios = sheets.ToWord();
            //检测选项匹配
            CheckSheet(info, sheets);
            _isModify = true;
            ShowMsg();
        }

        /// <summary> 手动绑定学生 </summary>
        /// <param name="pictureId"></param>
        /// <param name="groupId"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public void BindStudent(string pictureId, string groupId, long id, string name)
        {
            var picture = _markingInfo.Pictures.FirstOrDefault(t => t.Id == pictureId);
            if (picture == null)
                return;
            picture.GroupId = groupId;
            picture.StudentId = id;
            picture.StudentName = name;
            var marked = _markedInfoList.FirstOrDefault(t => t.MarkedId == pictureId);
            if (marked == null)
                return;
            //:不在协同范围判断
            if (_jointUsage != null && !_jointUsage.ClassList.Contains(groupId))
            {
                marked.IsSuccess = false;
                marked.Desc = "班级不在协同范围内";
            }
            else
            {
                marked.StudentId = id;
                marked.StudentName = name;
                marked.IsSuccess = true;
                if (!UploadPaper.IsEnabled)
                    UploadPaper.IsEnabled = true;
                marked.Desc = string.Empty;
            }
            _isModify = true;
            ShowMsg();
        }

        public void ReRecognition(bool msg = true)
        {
            var list = _markedInfoList.Where(t => (!t.IsSuccess && t.StudentCode.Length != 5) || t.RatiosColor == "Red").ToArray();
            if (!list.Any()) return;
            if (msg && !WindowsHelper.ShowSure("是否重新识别异常的试卷？"))
                return;
            RescannerMission(list);
        }
        #endregion

        /// <summary> 导出报表 </summary>
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var questions = _paperInfo.Sections.SelectMany(s => s.Questions.Where(q => q.IsObjective)).ToList();
            if (!questions.Any())
                WindowsHelper.ShowError("试卷没有客观题！");
            if (!_markedInfoList.Any())
                WindowsHelper.ShowError("没有扫描结果！");
            var dialog = new SaveFileDialog { Filter = @"(xls)|*.xls", DefaultExt = "xls" };
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            var detailTable = ExcelHelper.SheetDetails(questions, _markingInfo.Pictures);
            var statisticTable = ExcelHelper.SheetStatistic(questions, _markingInfo.Pictures);

            ExcelHelper.Export(new DataSet { Tables = { detailTable, statisticTable } }, null, dialog.FileName);
            WindowsHelper.ShowMsg("导出成功！");
        }
    }
}
