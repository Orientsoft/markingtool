using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.Core;
using DayEasy.MarkingTool.UI.Print;
using DayEasy.MarkingTool.UI.Scanner;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// 主窗体 的交互逻辑
    /// </summary>
    public partial class Default
    {
        private string _paperNo;
        public Default()
        {
            InitializeComponent();
            InitializeWindow();
            ScannerBtn.Click += ScannerBtn_Click;
            PrintBtn.Click += PrintBtn_Click;
            Loaded += (o, args) =>
            {
                PaperNum.KeyDown += KeyDownHandler;
            };
            PaperNum.KeyUp += (o, args) =>
            {

            };
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            BtnAction(ScannerBtn);
        }

        /// <summary> 打印 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            BtnAction(PrintBtn);
        }

        /// <summary> 扫描 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScannerBtn_Click(object sender, RoutedEventArgs e)
        {
            BtnAction(ScannerBtn);
        }

        private void BtnAction(IFrameworkInputElement btn)
        {
            var num = PaperNum.Text.Trim();
            if (string.IsNullOrWhiteSpace(num))
            {
                WindowsHelper.ShowError("请输入试卷编号！");
                return;
            }
            if (!Helper.IsPaperNum(num))
            {
                WindowsHelper.ShowError("试卷编号格式不正确！");
                return;
            }
            EnableBtn(false);
            ThreadPool.QueueUserWorkItem(BeginProgress, new List<string> { num, btn.Name });
        }

        private void BeginProgress(object args)
        {
            var list = args as List<string>;
            if (list == null || list.Count != 2)
                return;
            var num = list[0];
            var btn = list[1];
            if (string.IsNullOrWhiteSpace(num) || btn == null)
                return;
            _paperNo = num;
            var result = DeyiApp.GetPaperInfo(num);
            if (!result.Status)
            {
                WindowsHelper.ShowError(result.Message);
                EnableBtn(true);
                return;
            }
            var paper = result.Data;
            UiInvoke(() =>
            {
                DeyiWindow win;
                switch (btn)
                {
                    case "PrintBtn":
                        win = new BatchList(paper);
                        break;
                    default:
                        win = new MarkingPaper(paper);
                        break;
                }
                win.DeyiWindow(this, hideOwner: true);
            });
            EnableBtn(true);
        }

        private void EnableBtn(bool enabled)
        {
            UiInvoke(() =>
            {
                PrintBtn.IsEnabled = enabled;
                ScannerBtn.IsEnabled = enabled;
            });
        }

        private void InitializeWindow()
        {
            Title = string.Format("{0} - [{1},您好！] - 当前版本：v{2}", Title, DeyiApp.DisplayName, Helper.CurrentVersion());
            PaperNum.Focus();
        }
    }
}
