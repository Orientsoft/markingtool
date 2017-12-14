using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.UI.Controls;
using System;
using System.ComponentModel;
using System.Windows;

namespace DayEasy.MarkingTool.Core
{
    /// <summary>
    /// 基础Window
    /// </summary>
    public class DeyiWindow : Window
    {
        /// <summary> 是否关闭窗口 </summary>
        protected bool CloseWindow = false;
        public DeyiWindow()
        {
            InitializeTheme();
            InitializeStyle();
            Loaded += delegate
            {
                InitializeEvent();

                #region 设置全屏

                //// 设置全屏    
                //WindowState = WindowState.Normal;
                //WindowStyle = WindowStyle.None;
                //ResizeMode = ResizeMode.NoResize;
                //Topmost = true;

                //Left = 0.0;
                //Top = 0.0;
                //Width = SystemParameters.PrimaryScreenWidth;
                //Height = SystemParameters.PrimaryScreenHeight; 

                #endregion
            };
        }

        protected virtual void MinWindown()
        {
            WindowState = WindowState.Minimized;
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
            if (!(this is DeyiDialog))
                WindowsHelper.OwnerWindow = this;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Owner == null)
                return;
            if (CloseWindow)
            {
                Owner.Close();
                Application.Current.Shutdown();
            }
            else
            {
                Owner.Topmost = true;
                WindowsHelper.OwnerWindow = Owner;
                if (Owner.Visibility == Visibility.Hidden)
                    Owner.Show();
            }
            base.OnClosing(e);
        }

        protected void UiInvoke(Action action)
        {
            Dispatcher.Invoke(action);
        }
    }
}
