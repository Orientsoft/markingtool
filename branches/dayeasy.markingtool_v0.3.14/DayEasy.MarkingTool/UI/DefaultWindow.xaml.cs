using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// 主窗体 的交互逻辑
    /// </summary>
    public partial class DefaultWindow
    {
        private ObservableCollection<PrintUsage> _printUsages = new ObservableCollection<PrintUsage>();
        private int _refreshTime = 9;
        private Timer _timer;
        public DefaultWindow()
        {
            InitializeComponent();
            InitializeWindow();
            FileManager.CheckBaseDirectory();
            BtnRefresh.Click += BtnRefresh_Click;
            UnMarkList.SelectionMode = SelectionMode.Single;
            UnMarkList.MouseDoubleClick += UnMarkList_MouseDoubleClick;
            UnMarkList.SelectionChanged += UnMarkList_SelectionChanged;
        }

        void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BtnRefresh.IsEnabled = false;
            RefreshList();
            _timer = new Timer(1000) {Enabled = true};
            _timer.Start();
            _timer.Elapsed += timer_Elapsed;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (_refreshTime == 0)
                {
                    BtnRefresh.IsEnabled = true;
                    BtnRefresh.Content = "刷新列表";
                    _refreshTime = 9;
                    _timer.Stop();
                    _timer.Dispose();
                    return;
                }
                BtnRefresh.Content = string.Format("恢复中[{0}]", _refreshTime--);
            }));
        }

        private void UnMarkList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = UnMarkList.SelectedItem as PrintUsage;

            Dispatcher.Invoke(new Action(() =>
            {
                foreach (var usage in _printUsages)
                {
                    usage.IsChecked = (item != null && item.Batch == usage.Batch);
                }
            }));
        }

        private void UnMarkList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartMarkingPaper();
        }

        private void InitializeWindow()
        {
            Title = string.Format("{0}-[{1},您好！] - v{2}", Title, DeyiApp.DisplayName,Helper.CurrentVersion());
            RefreshList();
        }

        public void RefreshList()
        {
            _printUsages = new ObservableCollection<PrintUsage>();
            var list = RestHelper.Instance.PrintUsages(false, 0, 15);
            if (list.Status)
            {
                list.Data.ToList().ForEach(t => _printUsages.Add(new PrintUsage
                {
                    Batch = t.Batch,
                    ClassId = t.ClassId,
                    ClassName = t.ClassName,
                    ExpireTime = t.ExpireTime,
                    PaperId = t.PaperId,
                    PaperTitle = t.PaperTitle,
                    StartTime = t.StartTime,
                    StudentCount = t.StudentCount,
                    PrintType = t.PrintType,
                    Kind = Helper.ConvertPaperHand(t.PrintType)
                }));
            }
            UnMarkList.ItemsSource = _printUsages;
        }

        ///// <summary>
        ///// 答题卡
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void BtnMarkingAnswer_Click(object sender, RoutedEventArgs e)
        //{
        //    StartMarkingPaper(PaperKind.AnswerCard);
        //}

        ///// <summary>
        ///// 作业
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void btnMarking_Click(object sender, RoutedEventArgs e)
        //{
        //    StartMarkingPaper(PaperKind.Paper);
        //}

        private void StartMarkingPaper()
        {
            var item = UnMarkList.SelectedItem;
            PrintUsage usage;
            if (item == null || (usage = item as PrintUsage) == null)
            {
                WindowsHelper.ShowError("请先选择一套作业！");
                return;
            }
            new MarkingPaper(usage).ShowDeyiDialog(this);
        }
    }
}
