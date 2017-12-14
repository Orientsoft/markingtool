using DayEasy.MarkingTool.BLL;
using System;
using System.ComponentModel;
using System.Windows;

namespace DayEasy.MarkingTool.Core
{
    /// <summary>
    /// 基础Window
    /// </summary>
    public class DeyiWindow:Window
    {
        public DeyiWindow()
        {
            InitializeTheme();
            InitializeStyle();
            Loaded += delegate
            {
                InitializeEvent();
            };
        }

        protected virtual void MinWindown()
        {
            WindowState = WindowState.Minimized;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        /// <summary>
        /// 初始化样式
        /// </summary>
        private void InitializeStyle()
        {

        }

        /// <summary>
        /// 初始化模版
        /// </summary>
        private void InitializeTheme()
        {
            Application.Current.Resources.MergedDictionaries.Add(
                Application.LoadComponent(new Uri(string.Format("/Themes/{0}/Style.xaml", DeyiKeys.Theme),
                    UriKind.Relative)) as ResourceDictionary);
        }

        /// <summary>
        /// 初始化事件
        /// </summary>
        private void InitializeEvent()
        {
        }
    }
}
