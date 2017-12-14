# region 四川得一科技有限公司 版权所有
/* ================================================
 * 公司：四川得一科技有限公司
 * 作者：黄代毅
 * 创建：2014-02-19
 * 描述： MarkingPaper.xaml 的交互逻辑
 * ================================================
 */
# endregion

using Deyi.Tool.Common;
using Deyi.Tool.Entity.Paper;
using Deyi.Tool.Entity.User;
using Deyi.Tool.PaperServiceReference;
using Deyi.Tool.Step;
using Deyi.Tool.Steps;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using SWC = System.Windows.Controls;
//using AxMTKTWOCXLib;

namespace Deyi.Tool
{
    /// <summary>
    /// 作业
    /// </summary>
    public partial class MarkingPaper : Window
    {
        private readonly Logger _logger = Logger.L<MarkingPaper>();
        public MarkingPaper(PaperKind kind)
        {
            InitializeComponent();
            _kind = kind;
            this.Title = string.Format("{0}-[{1}]", this.Title, _kind.GetDescription());

            #region 初始化扫描控件
            //if (ax == null)
            //{
            //    ax = new AxMTKTWOCX();

            //}

            //// 创建 host 对象
            //System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();

            //// 创建OCX的对象
            //if (ax == null)
            //{
            //    ax = new AxMTKTWOCX();
            //}

            //host.Child = ax;
            //ax.BeginInit();

            //// 将对象加入到面板中
            //wrap.Children.Add(host);
            //host.Width = 0;
            //host.Height = 0;
            ////host.SetValue(Grid);
            //Grid.SetRow(host, 0); 
            #endregion
        }
        /// <summary>
        /// 试卷类型
        /// </summary>
        private readonly PaperKind _kind = PaperKind.Paper;
        readonly ObservableCollection<PaperMarkedInfo> _markedInfoList = new ObservableCollection<PaperMarkedInfo>();
        List<string> _paperPathsList;//, singleScan = null;
        string _markingKey = string.Empty;
        static List<PaperBatchDetail> _batchDetails;
        static List<MarkingResult> _markedResult;
        bool _isSaved = false;
        bool _isModify = false;
        //bool isUpload = false;
        bool _uploadRunning = false;

        readonly IStep[] _steps = new IStep[]
        {
             new FindLineStage(),
             new BasicInfoScaningStage(),
             new CuttingStage(),
             new ChoiceSubjectScaningStage()
        };

        // static AxMTKTWOCX ax = null;

        private string UserTempPath
        {
            get
            {
                return Path.Combine(DeyiKeys.UnfinishedPaperSavePath, UserInfo.Current.ID.ToString(), "temp");
            }
        }


        

        //void ax_PostScanEveryPage(object sender, _DMTKTWOCXEvents_PostScanEveryPageEvent e)
        //{
        //    if (e.bSuccess == 1)
        //    {
        //        var temp = ax.GetCurrentScanImagePath();
        //        if (!paperPathsList.Contains(temp))
        //        {
        //            singleScan.Add(temp);
        //        }
        //        paperPathsList.Add(temp);

        //    }
        //}


        #region 驱动扫描仪直接扫描
        //private void btnScan_Click(object sender, RoutedEventArgs e)
        //{
        //    // btnScan.IsEnabled = false;
        //    singleScan = new List<string>();
        //    // ShowPicture();
        //    // try
        //    //{
        //    ax.PostScanEveryPage += ax_PostScanEveryPage;
        //    //if(ax.GetState==0)

        //    if (ax.OpenScanner() == 1)
        //    {
        //        if (paperPathsList.Count < 1)    //第一次配置
        //        {
        //            Helper.SetDefaultForScan(ax, _markingKey);
        //        }
        //        else
        //        {
        //            Helper.SetNamingRule(ax, _markingKey, paperPathsList.Count + 1);
        //        }

        //        ax.Scan(-1, 0);
        //    }
        //    if (ax.CloseScanner() == 1)
        //    {
        //        if (singleScan.Count < 1)
        //        {
        //            Helper.ShowError("未能扫到任何图片。");
        //            // btnScan.IsEnabled = true;
        //            return;
        //        }

        //        if (Helper.ShowSure("扫描成功，是否查看图片，“是”查看图片，“否”直接阅卷。"))
        //        {
        //            ShowPicture();
        //        }
        //        else
        //        {
        //            ThreadPool.QueueUserWorkItem(new WaitCallback(BeginPorgress));
        //        }
        //    }
        //    else
        //    {
        //        Helper.ShowError("扫描完成，但可能存在错误。");
        //    }
        //    // btnScan.IsEnabled = true;

        //} 
        #endregion

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            ShowPicture();
        }


        //private void ShowPicture()
        //{
        //    var picWin = new ImageView(singleScan);
        //    if (picWin.ShowDialog() == true)
        //    {
        //        singleScan = picWin.SelectPaths;
        //        ThreadPool.QueueUserWorkItem(new WaitCallback(BeginPorgress));
        //    }
        //    else
        //    {
        //        if (singleScan != null)
        //        {
        //            singleScan.Clear();
        //        }
        //    }
        //}


        private void ShowPicture()
        {
            var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string rootPath = fbd.SelectedPath;
                //MessageBox.Show(rootPath);
                // paperPathsList = Helper.GetAllImagePath(rootPath).Where(p => !_markedInfoList.Any(m => m.PaperName == Path.GetFileNameWithoutExtension(p))).ToList();
                //var picWin = new PictureListWindow(ref paperPathsList);

                var picWin = new ImageView(rootPath, _markedInfoList.Select(m => m.PaperName).ToArray());
                if (picWin.ShowDialog() == true)
                {
                    _paperPathsList = picWin.SelectPaths;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(BeginPorgress));
                }
                else
                {
                    if (_paperPathsList != null)
                    {
                        _paperPathsList.Clear();
                    }
                }
                // System.GC.Collect();
            }
        }

        private void ShowResult(PaperMarkedInfo markedInfo)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                _markedInfoList.Add(markedInfo);
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this._markedInfoList.Clear();
            this._markingKey = Guid.NewGuid().ToString("N");
            _markedResult = new List<MarkingResult>();
            _batchDetails = new List<PaperBatchDetail>();
            _paperPathsList = new List<string>();

            list.ItemsSource = _markedInfoList;
            PaperMarkedInfo.MarkedID = _markingKey;
            this.Closing += MarkingPaper_Closing;
            this.Closed += MarkingPaper_Closed;
            string path = UserTempPath;
            if (Directory.Exists(path))
            {
                var directory = new DirectoryInfo(path);
                var file = directory.GetFiles().FirstOrDefault(f => f.Extension == ".dayeasy");
                if (file != null)
                {
                    if (Helper.ShowQuestion("你阅卷上传并未成功，是否上传？"))
                    {
                        LoadOldData(file.FullName);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(UploadMarkedPaper), true);
                    }
                    else
                    {
                        Helper.DeleteFiles(UserTempPath, ".dayeasy");
                    }
                }
            }

        }

        void MarkingPaper_Closed(object sender, EventArgs e)
        {
            this._markedInfoList.Clear();
            this._markingKey = string.Empty;
            _paperPathsList = null;
           // singleScan = null;
            _markedResult = null;
            _batchDetails = null;
        }

        void MarkingPaper_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_markedResult.Count > 0 && _isModify && !_isSaved)
            {
                if (Helper.ShowQuestion("阅卷没有被保存,是否保存？"))
                {
                    SaveResultWhitChoosePath();
                }

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
            InitBar(_paperPathsList.Count * 5, Visibility.Visible);
            StepResult result = null;
            InfomationScaningResult Info;
            MarkingResult markResult;
            PaperBatchDetail detail;
            PaperMarkedInfo marked = null;
            LinesResult linesResult = null;
            IEnumerable<QuestionEntity> questions;
            foreach (var item in _paperPathsList)
            {
                try
                {
                    //自动纠偏
                    var bmp = Image.FromFile(item);
                    var deskew = new Deskew((Bitmap) bmp);
                    var angle = deskew.GetSkewAngle();
                    bmp = ImageHelper.RotateBitmap((Bitmap) bmp, (float) -angle);
                    //bmp = ImageHelper.BinarizeImage((Bitmap) bmp);
                    var path = Path.Combine(DeyiKeys.ItemPath, Path.GetFileName(item));
                    bmp.Save(path);

                    //第一步，获取线条布局
                    marked = new PaperMarkedInfo {MarkedResultID = Guid.NewGuid(), ImagePath = item};
                    result = _steps[0].Process(path);
                    if (!result.IsSuccess)
                    {
                        marked.IsSuccess = false;
                        marked.Desc = result.Exception.Message;
                        ShowResult(marked);
                        ChangeBar(4);
                        continue;
                    }
                    ChangeBar(1);
                    linesResult = result as LinesResult;
                    //第二部，获取顶部基本信息
                    result = _steps[1].Process(path, linesResult.Lines.FirstOrDefault());
                    if (!result.IsSuccess)
                    {
                        marked.Desc = result.Exception.Message;
                        marked.IsSuccess = false;
                        ShowResult(marked);
                        ChangeBar(3);
                        continue;
                    }
                    ChangeBar(1);
                    Info = (result as InfomationScaningResult);
                    marked.BacthCode = Info.ScanResult["BatchCode"];
                    marked.SutdentNo = Info.ScanResult["StudentNo"];
                    marked.SutdentName = Info.ScanResult["Name"];
                    marked.IDNo = Info.ScanResult["IDNo"];
                    if (string.IsNullOrWhiteSpace(marked.BacthCode) && marked.BacthCode.Length < 32)
                    {
                        marked.Desc = "未识别批次号";
                        marked.IsSuccess = false;
                        ShowResult(marked);
                        ChangeBar(1);
                        continue;
                    }

                    marked.BacthCode = marked.BacthCode.Substring(0, 32);
                    //第三步，
                    result = _steps[2].Process(path, linesResult.Lines);
                    if (!result.IsSuccess)
                    {
                        marked.IsSuccess = false;
                        marked.Desc = result.Exception.Message;
                        ShowResult(marked);
                        ChangeBar(2);
                        continue;
                    }
                    ChangeBar(1);

                    detail = GetBatchDetails(marked.BacthCode);
                    if (detail == null)
                    {
                        marked.IsSuccess = false;
                        marked.IsMarked = false;
                        marked.Desc = DeyiKeys.NotFoundPaper;
                        ShowResult(marked);
                        ChangeBar(1);
                        continue;
                    }
                    marked.PaperTitle = detail.Title;
                    //TODO:
                    questions = detail.Sections.SelectMany(d => d.Questions);
                    marked.TotalCount = questions.Count();
                    result = _steps[3].Process(path, questions);
                    if (!result.IsSuccess)
                    {
                        marked.IsSuccess = false;
                        marked.Desc = result.Exception.Message;
                        ShowResult(marked);
                        ChangeBar(1);
                        continue;
                    }
                    //marked.IsSuccess = true;
                    marked.Desc = DeyiKeys.Success;

                    markResult = FullMarkingResult(result as ObjectiveScanningResult, detail, marked);

                    _markedResult.Add(markResult);
                    ShowResult(marked);
                    ChangeBar(1);
                    Helper.PacketMarkedPicture(marked.PaperName, marked.BacthCode);
                    ChangeBar(1);
                }
                catch (Exception ex)
                {
                    Helper.ShowError(ex.Message);
                    _logger.E("批阅异常", ex);
                    marked.IsSuccess = false;
                    marked.IsMarked = false;
                    marked.Desc = DeyiKeys.UnKonwnError;
                    ShowResult(marked);
                    ChangeBar(1);
                    continue;
                }
            }


            InitBar(0d, Visibility.Hidden);
            EnabledButton(true);

            var fCount = _markedInfoList.Count(m => !m.IsSuccess);
            var all = _markedInfoList.Count;

            ShowMsg(string.Format("阅卷结果：总试卷数{0}张，成功{1}张，失败{2}张。", _markedInfoList.Count, all - fCount, fCount));
            //singleScan.Clear();
        }

        /// <summary>
        /// 填充阅卷信息
        /// </summary>
        /// <param name="result"></param>
        /// <param name="detail"></param>
        /// <param name="marked"></param>
        /// <returns></returns>
        private MarkingResult FullMarkingResult(ObjectiveScanningResult result, PaperBatchDetail detail, PaperMarkedInfo marked)
        {
            MarkingResult markResult = result.Anwsers;
            markResult.AddedAt = DateTime.Now;
            markResult.AddedIP = Helper.GetHostIP()[1];
            markResult.AddedBy = detail.AddedBy;
            markResult.Batch = marked.BacthCode;
            markResult.MarkingBy = UserInfo.Current.ID;
            markResult.PaperID = detail.PaperID;
            markResult.StudentIdentity = marked.IDNo;
            markResult.AddedBy = detail.AddedBy;

            //markResult.LastUpdateAt = DateTime.Now;
            markResult.IsFinished = false;
            markResult.ID = marked.MarkedResultID;

            markResult.TotalScore = markResult.Detail.Aggregate(0, (t, b) => b.IsCorrect ? t + b.Score : t);

            marked.TotalScore = markResult.TotalScore;
            marked.ErorrCount = markResult.Detail.Count(d => !d.IsCorrect);

            var questions = detail.Sections.SelectMany(d => d.Questions);

            foreach (var question in questions)
            {
                if (!markResult.Detail.Any(mr => mr.QuestionID == question.Base.ID))
                {
                    markResult.Detail.Add(new MarkingDetail()
                    {
                        ID = Guid.NewGuid(),
                        QuestionID = question.Base.ID,
                        QuestionVersion = question.Base.EntireVersionID,
                        TeacherComments = new List<TeacherCommentInfo>(),
                        MarkingSymbols = new List<MarkingSymbolInfo>(),
                        IsCorrect = true       //主观题默认视为正确
                    });
                }
            }

            return markResult;
        }

        private void InitBar(double max, Visibility vb)
        {
            Dispatcher.Invoke(
                new Action(() =>
                {
                    pBar.Maximum = max;
                    pBar.Value = 0;
                    pBar.Visibility = vb;
                }));
        }

        private void ChangeBar(int val)
        {
            this.Dispatcher.Invoke(new Action(() => pBar.Value += val));
        }

        private PaperBatchDetail GetBatchDetails(string batchNo)
        {
            PaperBatchDetail detail = null;
            detail = _batchDetails.FirstOrDefault(batch => batch.BatchNo == batchNo);
            if (detail == null)
            {
                Helper.CallWCF<Paper>(client => detail = client.GetPaperBatchDetail(batchNo));
                if (detail != null)
                {
                    detail.BatchNo = batchNo;
                    _batchDetails.Add(detail);
                }
            }
            return detail;
        }

        private void btnOperate_Click(object sender, RoutedEventArgs e)
        {
            if (_uploadRunning)
            {
                Helper.ShowQuestion("正在上传，暂时不能操作...");
                return;
            }
            _isModify = true;
            var btn = sender as SWC.Button;
            //var paramters = btn.Tag.ToString().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            var resultId = Guid.Parse(btn.Tag.ToString());

            var markedInfo = _markedInfoList.First(m => m.MarkedResultID == resultId);

            if (markedInfo.IsSuccess)
            {

                var detail = GetBatchDetails(markedInfo.BacthCode);
                if (detail == null)
                {
                    Helper.ShowQuestion("未能找到该试卷的相关信息");
                }

                var marking = new MarkingSubjective(markedInfo, detail, ref _markedResult, _kind);
                marking.ObtianDirctoryHandler += marking_ObtianDirctoryHandler;

                if (marking.ShowDeyiDialog(this) == true)
                {
                    Statistics(_markedResult);
                }
            }
            else
            {
                Helper.ShowQuestion("不能操作为成功的试卷");
            }

        }

        object[] marking_ObtianDirctoryHandler(bool isNext, Guid resultId)
        {
            object[] objs = new object[2];
            var current = _markedInfoList.First(m => m.MarkedResultID == resultId);
            PaperMarkedInfo marked = null;
        NextAgain: var index = _markedInfoList.IndexOf(current);
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
                    objs[1] = _batchDetails.First(b => b.BatchNo == marked.BacthCode);
                }
            }
            return objs;
        }

        private void SavePaper_Click(object sender, RoutedEventArgs e)
        {
            SaveResultWhitChoosePath();
        }

        private void SaveResultWhitChoosePath()
        {
            using (var dia = new SaveFileDialog())
            {
                dia.Title = @"保存到本地";
                dia.Filter = @"试卷 (*.dayeasy)|*.dayeasy";
                if (dia.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        SaveResult(dia.FileName);
                        Helper.ShowQuestion("保存成功");
                    }
                    catch (Exception ex)
                    {
                        Helper.ShowError(ex.Message);
                    }
                }
            }
        }

        private void SaveResult(string fileName)
        {
            _isSaved = true;
            if (_markedInfoList.Count > 0)
            {
                var saveResult = new KeyValuePair<string, PaperMarkedInfo[]>(_markingKey, _markedInfoList.ToArray());
                try
                {
                    Helper.Serialize(saveResult, fileName);
                    var tempPath = Path.Combine(DeyiKeys.UnfinishedPaperSavePath, UserInfo.Current.ID.ToString());
                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }
                    Helper.Serialize(_markedResult, Path.Combine(tempPath, string.Format("{0}.mark", _markingKey)));

                    //return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = @"试卷 (*.dayeasy)|*.dayeasy";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // isUpload = false;
                    try
                    {
                        LoadOldData(dialog.FileName);
                    }
                    catch (FileNotFoundException ex1)
                    {
                        Helper.ShowError(ex1.Message);
                        if (_markedInfoList != null)
                        {
                            foreach (var item in _markedInfoList)
                            {
                                item.IsMarked = false;
                            }
                        }

                    }
                    catch (Exception ex2)
                    {
                        //isUpload = true;
                        Helper.ShowError("问题格式有误...");
                        // Helper.ShowQuestion(ex.Message);
                    }

                }
            }
        }

        private void LoadOldData(string fileName)
        {
            var temp = Helper.Deserialize<KeyValuePair<string, PaperMarkedInfo[]>>(fileName);
            if (!string.IsNullOrWhiteSpace(temp.Key) && temp.Value != null && temp.Value.Length > 0)
            {
                _markingKey = temp.Key;
                PaperMarkedInfo.MarkedID = _markingKey;
                //_markedInfoList = temp.Value;
                _markedInfoList.Clear();
                Array.ForEach(temp.Value, s => { if (s != null) { _markedInfoList.Add(s); } });
                Helper.LoadMarkedPaper(_markedResult, _markingKey, UserInfo.Current.ID);
            }
            else
            {
                Helper.ShowError("问题格式有误...");
            }
        }

        /// <summary>
        /// 上传阅卷
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadPaper_Click(object sender, RoutedEventArgs e)
        {
            //Helper.CallWCF<Paper>(client=>client.UploadFile());
            //var paperByBatch = _markedInfoList.GroupBy(md => md.BacthCode);
            // DirectoryInfo directory = null;
            if (_markedInfoList.All(m => m.IsUpload))
            {
                Helper.ShowQuestion("本次阅卷已被全部上传...");
                return;
            }
            if (_markedInfoList.Any(md => !md.IsMarked))
            {
                Helper.ShowQuestion("存在未批阅的试卷");
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
                SaveResult(tempPath);
                ThreadPool.QueueUserWorkItem(new WaitCallback(UploadMarkedPaper), false);
            }
            catch (Exception ex)
            {
                Helper.DeleteFiles(tempPath);
                Helper.ShowError(ex.Message);
            }


        }

        private void UploadMarkedPaper(object arg)
        {

            EnabledButton(false);
            _uploadRunning = true;

            var basePath = DeyiKeys.SavePath;
            PaperMarkingFileData data;
            InitBar(_markedInfoList.Count + 1, Visibility.Visible);
            ResultPacket result = null;
            var results = new List<MarkingResult>();
            try
            {
                foreach (var item in _markedInfoList)
                {
                    if (item.IsUpload)
                    {
                        continue;
                    }
                    data = new PaperMarkingFileData
                    {
                        Data = File.OpenRead(Path.Combine(DeyiKeys.CompressedPath, item.BacthCode, item.PaperName)),
                        Filename = item.IDNo,
                        DirectoryName = string.Format("{0}/{1}/{2}", item.IDNo, item.BacthCode, item.MarkedResultID)
                    };
                    Helper.CallWCF<Paper>(client => client.UploadFile(data));
                    item.IsUpload = true;
                    var resultItem = _markedResult.FirstOrDefault(m => m.ID == item.MarkedResultID);
                    results.Add(resultItem);
                    ChangeBar(1);
                }


                Helper.CallWCF<Paper>(client => result = client.GeneratedMarkingResults(_markedResult));
                if (result == null || result.IsError)
                {
                    throw new Exception();
                }
                ChangeBar(1);
                Helper.ShowQuestion("上传成功...");
                Helper.DeleteFiles(Path.Combine(DeyiKeys.UnfinishedPaperSavePath, UserInfo.Current.ID.ToString(), "temp"), ".dayeasy");
            }
            catch
            {
                Helper.ShowError("上传试卷失败...");

                if ((bool)arg)
                {
                    Helper.DeleteFiles(Path.Combine(DeyiKeys.UnfinishedPaperSavePath, UserInfo.Current.ID.ToString(), "temp"), ".dayeasy");
                }

                return;
            }
            finally
            {
                InitBar(0, Visibility.Hidden);
                EnabledButton(true);
                _uploadRunning = false;
            }


        }

        //重新统计分数，及披阅状态
        void Statistics(List<MarkingResult> markeds)
        {
            PaperMarkedInfo info;
            List<QuestionEntity> questions;
            MarkingDetail detialTemp;


            // byte[] objective = new byte[] { 1, 2, 3 };

            int error = 0;
            int score = 0;
            int totalScore = 0;
            var markByBatch = markeds.GroupBy(md => md.Batch);
            foreach (var grop in markByBatch)
            {
                questions = GetBatchDetails(grop.Key).Sections.SelectMany(s => s.Questions).ToList();
                totalScore = questions.Aggregate(0, (t, q) =>
                {
                    return t += q.DetailList.Sum(d => d.Base.Score);
                });
                foreach (var item in grop)
                {
                    //if (item == null)
                    //{
                    //    continue;
                    //}
                    info = _markedInfoList.FirstOrDefault(mi => mi.MarkedResultID == item.ID);//(mi => mi.BacthCode == item.Batch && mi.IDNo == item.StudentIdentity);
                    if (info == null)
                    {
                        info = new PaperMarkedInfo();
                    }
                    info.IsMarked = item.IsFinished;

                    //question = questions.Find(q=>q.Base.ID ==item.Detail.);
                    questions.ForEach(q =>
                    {

                        detialTemp = item.Detail.Find(d => d.QuestionID == q.Base.ID && d.QuestionVersion == q.Base.EntireVersionID);
                        if (detialTemp != null && detialTemp.MarkingSymbols != null &&
                            detialTemp.MarkingSymbols.Any(sy => sy.SymbolType == MarkingSymbolType.Wrong))
                        {
                            if (Helper.IsObjective(q.Base.TypeID, q.Base.SubjectID))
                            {
                                error++;
                            }

                            detialTemp.Score = q.DetailList.Sum(d => d.Base.Score);
                            score += detialTemp.Score;

                        }
                        //else
                        //{
                        //    detialTemp.Score = q.DetailList.Sum(d => d.Base.Score);
                        //    score += detialTemp.Score;
                        //}
                    });
                    info.ErorrCount = error;
                    info.TotalScore = totalScore - score;
                    item.TotalScore = info.TotalScore;
                    error = 0;
                    score = 0;
                }
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SWC.PrintDialog();
                var btn = sender as SWC.Button;
                var resultID = Guid.Parse(btn.Tag.ToString());

                if (dialog.ShowDialog().GetValueOrDefault())
                {
                    var marking = _markedResult.Find(m => m.ID == resultID);
                    var markeds = marking.Detail.SelectMany(d => d.MarkingSymbols).ToArray();
                    var comments = marking.Detail.SelectMany(d =>
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

        private void btnPrintAll_Click(object sender, RoutedEventArgs e)
        {

            if (_markedResult.Count(m => m.IsFinished) < 1)
            {
                Helper.ShowQuestion("没有批阅过的试卷，请先批阅试卷...");
                return;
            }
            if (!Helper.ShowQuestion("一键打印需要保证试卷从打印机出来的顺序一致！"))
            {
                return;
            }
            EnabledButton(false);
            var dialog = new SWC.PrintDialog();
            DrawingVisual vis;

            try
            {
                if (dialog.ShowDialog().GetValueOrDefault())
                {
                    foreach (var result in _markedResult)
                    {
                        if (result == null && !result.IsFinished)
                        {
                            continue;
                        }
                        var markeds = result.Detail.SelectMany(d => d.MarkingSymbols).ToArray();
                        var comments = result.Detail.SelectMany(d =>
                        {
                            if (d.TeacherComments == null)
                            {
                                return new List<TeacherCommentInfo>();
                            }
                            return d.TeacherComments;
                        }
                        ).ToArray();

                        vis = Helper.PrintingMarked(markeds, comments);
                        dialog.PrintVisual(vis, "正在打印作业");
                    }
                }
            }
            catch (Exception ex)
            {

                Helper.ShowError(ex.Message);
            }
            finally
            {
                EnabledButton(true);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var id = Guid.Parse((sender as SWC.Button).Tag.ToString());
            _markedResult.Remove(_markedResult.Find(m => m.ID == id));
            var tempInfo = _markedInfoList.First(m => m.MarkedResultID == id);

            Helper.DeleteFiles(Path.Combine(DeyiKeys.SavePath, tempInfo.PaperName));
            _markedInfoList.Remove(tempInfo);

        }

        private void EnabledButton(bool isDisable)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {

                btnImport.IsEnabled = isDisable;
                btnPrintAll.IsEnabled = isDisable;
                //btnPrintAll2.IsEnabled = isDisable;
                btnScan.IsEnabled = isDisable;
                UploadPaper.IsEnabled = isDisable;
                SavePaper.IsEnabled = isDisable;
                sequence.IsEnabled = isDisable;
                reverse.IsEnabled = isDisable;
            }));

        }

        private void ShowMsg(string msg)
        {

            txtTip.Dispatcher.Invoke(new Action(() =>
            {
                txtTip.Text = msg;
            }));

        }

        private void Reverselist()
        {

            //var tempInfos = _markedInfoList.Reverse();
            //_markedInfoList = new ObservableCollection<PaperMarkedInfo>();
            //list.ItemsSource = _markedInfoList;
            //foreach (var item in tempInfos)
            //{
            //    _markedInfoList.Add(item);
            //}

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
            if (this.IsLoaded)
            {
                EnabledButton(false);
                Reverselist();
            }
        }

        private void reverse_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                EnabledButton(false);
                Reverselist();
            }
        }

        private void btnSee_Click(object sender, RoutedEventArgs e)
        {
            var btn = (SWC.Button)sender;
            var win = new PaperImageDetail(new System.Windows.Media.Imaging.BitmapImage(new Uri(btn.Tag.ToString())));
            win.ShowDeyiDialog();
        }






        //void FullMarkResult(MarkingResult markingResult,List<Question> question)
        //{

        //    List<MarkingDetail> details = new List<MarkingDetail>();


        //    question.ForEach(q => { 

        //      markedResult.det.Any(m=>m.q)
        //    });

        //    markingResult.ForEach(q =>
        //        {
        //            details.Add(new MarkingDetail()
        //            {
        //                ID = Guid.NewGuid(),
        //                IsCorrect = false,
        //                QuestionID = q.Base.ID,
        //                QuestionVersion = q.Base.EntireVersionID
        //            });

        //        });

        //    return details;
        //}

    }
}
