# region 四川得一科技有限公司 版权所有
/* ================================================
 * 公司：四川得一科技有限公司
 * 作者：文杰
 * 创建：2013-12-28
 * 描述：默认窗口逻辑代码
 * ================================================
 */
# endregion

using Deyi.Tool.Common;
using Deyi.Tool.Entity.Paper;
using Deyi.Tool.Entity.User;
using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Deyi.Tool
{
    /// <summary>
    /// 主窗体 的交互逻辑
    /// </summary>
    public partial class DefaultWindow : Window
    {
        public DefaultWindow()
        {
            InitializeComponent();
            InitializeWindow();
            if (!Directory.Exists(DeyiKeys.ItemPath))
                Directory.CreateDirectory(DeyiKeys.ItemPath);
            if (DeyiKeys.WriteFile && !Directory.Exists(DeyiKeys.PicturePath))
                Directory.CreateDirectory(DeyiKeys.PicturePath);
        }

        private readonly Timer _timer = new Timer(1000);

        private void InitializeWindow()
        {
            lblCurrentDateTime.Text = "当前时间：" + DateTime.Now.ToString("F");
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
            Title += " - " + UserInfo.Current.TrueName + "(" + UserInfo.Current.Nickname + ")";
           // BindPaperList();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lblCurrentDateTime.Dispatcher.Invoke(new Action(delegate { lblCurrentDateTime.Text = DateTime.Now.ToString("F"); }));
        }

        private void BindPaperList()
        {
            //var totalCount = 0;
            //List<PaperFeed> arrPaper = null;
            //Helper.CallWCF<Paper>(service => arrPaper = service.GetPaperList4Page(out totalCount, 7, 0, new List<long> { UserInfo.Current.ID }, null));

            //if (arrPaper == null)
            //{
            //    return;
            //}

            //var listPaper = new List<PaperBasicInfo>();
            //if (arrPaper == null)
            //{
            //    return;
            //}

            //arrPaper.ForEach(paper =>
            //     {
            //         listPaper.Add(new PaperBasicInfo()
            //         {
            //             ID = paper.ID,
            //             Title = paper.Body,
            //             AddedAt = paper.AddedAt,
            //             DisplayAddedAt = Helper.DisplayDateTime(paper.AddedAt)
            //         });
            //     });

            //lvPaperList.ItemsSource = listPaper;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var paperId = Guid.Parse(btn.Tag.ToString());

            //new PrintPreview(paperId).ShowDeyiDialog(this);
            // new PrintDetail(btn.Tag.ToString()).ShowDeyiDialog();
            new PrintHomework(btn.Tag.ToString()).ShowDeyiDialog();
        }

        /// <summary>
        /// 作业
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMarking_Click(object sender, RoutedEventArgs e)
        {
            new MarkingPaper(PaperKind.Paper).ShowDeyiDialog(this);
        }


        private void btnPrintTemplate_Click(object sender, RoutedEventArgs e)
        {
            new PrintPaperTemplate().ShowDeyiDialog(this);
        }

        /// <summary>
        /// 答题卡
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMarkingCrad_Click(object sender, RoutedEventArgs e)
        {
            new MarkingPaper(PaperKind.AnswerCard).ShowDeyiDialog(this);
        }
    }
}
