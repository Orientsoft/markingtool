using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.Core;
using DayEasy.Open.Model.Paper;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private readonly Timer _timer = new Timer(1000);
        private delegate void ShowProgressBarDelegate(DependencyProperty dependencyProperty, object value);

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        #region 界面事件
        // 目录：新建 扫描
        private void menuNewScan_Click(object sender, RoutedEventArgs e)
        {

        }

        // 目录：新建 打印
        private void menuNewPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        // 目录：新建 阅卷
        private void menuNewMarking_Click(object sender, RoutedEventArgs e)
        {

        }

        // 目录：新建 套打
        private void menuNewTao_Click(object sender, RoutedEventArgs e)
        {

        }

        // 目录：退出登录
        private void menuLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = new LoginWindow().ShowDeyiDialog();

            if (result.HasValue && !result.Value)
            {
                this.Close();
            }
        }

        // 目录：退出程序
        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            _timer.Dispose();
            this.Close();
        }

        // 目录：关于
        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDeyiDialog();
        }

        // 按钮：刷新
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindPaperTitleList();
        }

        // 按钮：查看
        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            btnPrint.IsEnabled = false;

            //var sb = new StringBuilder();

            //for (var i = 0; i < 20; i++)
            //{
            //    sb.Append("这里是作业详情。Here is the paper detail. Just for test");
            //}

            //lblPaperDetail.Text = sb.ToString();

            BindPaperDetail();

            btnPrint.IsEnabled = true;
        }

        // 按钮：打印
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new PrintDialog();

                if (dialog.ShowDialog() == true)
                {
                    dialog.PrintVisual(prtArea, "123");
                }
            }
            catch (Exception ex)
            {
                WindowsHelper.ShowError(ex.Message);
            }
        }
        #endregion

        #region 私有方法
        // 初始化窗口
        private void InitializeWindow()
        {
            Title += " - " + DeyiApp.CurrentUser.RealName + "(" + DeyiApp.CurrentUser.NickName + ")";
            prgBar.Visibility = Visibility.Hidden;
            lblCurrentDateTime.Text = "当前时间：" + DateTime.Now.ToString("F");
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
            btnPrint.IsEnabled = false;
            BindPaperTitleList();
        }

        // 当前时间
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lblCurrentDateTime.Dispatcher.Invoke(new Action(delegate { lblCurrentDateTime.Text = "当前时间：" + DateTime.Now.ToString("F"); }));
        }

        // 将进度条显示为等待状态
        private void ShowProgressBarWaiting()
        {
            HideProgressBar();
            prgBar.IsIndeterminate = true;
            prgBar.Visibility = Visibility.Visible;
        }

        // 显示进度条
        private void ShowProgressBar(double maxValue)
        {
            HideProgressBar();
            prgBar.IsIndeterminate = false;
            prgBar.Maximum = maxValue;
            prgBar.Value = 0;
            prgBar.Visibility = Visibility.Visible;
            var showDelegate = new ShowProgressBarDelegate(prgBar.SetValue);

            for (var i = 0D; i < maxValue; i++)
            {
                Dispatcher.Invoke(showDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, Convert.ToDouble(i + 1) });
            }
        }

        // 隐藏进度条
        private void HideProgressBar()
        {
            prgBar.Visibility = Visibility.Hidden;
            prgBar.Maximum = 0;
            prgBar.Value = 0;
        }

        // 绑定作业标题下拉菜单
        private void BindPaperTitleList()
        {
            var list = new List<PaperInfo>();
            string id;

            for (var i = 0; i < 10; i++)
            {
                id = Guid.NewGuid().ToString("N");
                list.Add(new PaperInfo()
                {
                    PaperId = id,
                    Title = id
                });
            }

            cmbPaperTitle.ItemsSource = list;
            cmbPaperTitle.DisplayMemberPath = "Title";
            cmbPaperTitle.SelectedValuePath = "ID";
            cmbPaperTitle.SelectedIndex = 0;
        }

        private void BindPaperDetail()
        {
            //var paper = GetPaper();
            //lblPaperTitle.Content = paper[0].Title;
            //dtgDetail.ItemsSource = paper[0].Body;
        }

        //private List<PaperDetail> GetPaper()
        //{
        //    var listAnswer = new List<AnswerBody>();
        //    for (var i = 0; i < 4; i++)
        //    {
        //        listAnswer.Add(new AnswerBody()
        //        {
        //            Body = string.Concat("Answer ", i, " 答案哟 ", i, " 嘿嘿")
        //        });
        //    }

        //    var listQuestionDetail = new List<QuestionDetail>();
        //    listQuestionDetail.Add(new QuestionDetail()
        //    {
        //        Body = "Question detail body. 问题详情内容。测试哟",
        //        Answer = listAnswer
        //    });

        //    var listQuestion = new List<Question>();
        //    listQuestion.Add(new Question()
        //    {
        //        Body = "题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 题干内容 body 1 ",
        //        QuestionType = 1,
        //        Detail = listQuestionDetail
        //    });

        //    listQuestion.Add(new Question()
        //    {
        //        Body = "题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二题干内容二",
        //        QuestionType = 1,
        //        Detail = listQuestionDetail
        //    });

        //    listQuestion.Add(new Question()
        //    {
        //        Body = "题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 题干内容 body 3 ",
        //        QuestionType = 1,
        //        Detail = listQuestionDetail
        //    });

        //    listQuestion.Add(new Question()
        //    {
        //        Body = "题干内容题干内容题干内容题干内容题干内容题干内容题干内容题干内容题干内容题干内容题干内容题干内容题干内容题干内容题干内容",
        //        QuestionType = 1,
        //        Detail = listQuestionDetail
        //    });

        //    //var listPaper = new List<PaperDetail>();
        //    //listPaper.Add(new PaperDetail()
        //    //{
        //    //    Title = DateTime.Now.ToLongDateString() + " 语文作业",
        //    //    Body = listQuestion
        //    //});

        //    return listPaper;
        //}
        #endregion

        private void lnSep_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1 || sender == e.Source)
            {
                return;
            }

            
        }

        private void lnSep_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void lnSep_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(this);
                var lnSep = (Line)sender;
                MessageBox.Show(point.Y.ToString());
            }
        }
    }

    //internal class PaperInfo
    //{
    //    public Guid ID { get; set; }
    //    public string Title { get; set; }
    //}

    ////internal class PaperDetail
    ////{
    ////    public string Title { get; set; }

    ////    public List<Question> Body { get; set; }
    ////}

    internal class Question
    {
        public string Body { get; set; }

        public byte QuestionType { get; set; }

        public List<QuestionDetail> Detail { get; set; }
    }

    internal class QuestionDetail
    {
        public string Body { get; set; }

        public List<AnswerBody> Answer { get; set; }
    }

    internal class AnswerBody
    {
        public string Body { get; set; }
    }
}
