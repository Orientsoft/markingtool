using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Deyi.AutoUpdater.Core
{
    public class WindowBase : Window
    {
        public WindowBase()
        {
            InitializeTheme();
            InitializeStyle();

            Loaded += delegate
            {
                InitializeEvent();
            };
        }

        protected virtual void MinWin()
        {
            WindowState = WindowState.Minimized;
        }

        public Button YesButton
        {
            get;
            set;
        }
        public Button NoButton
        {
            get;
            set;
        }

        private void InitializeEvent()
        {
            var baseWindowTemplate = (ControlTemplate)Application.Current.Resources["BaseWindowControlTemplate"];

            var borderTitle = (Border)baseWindowTemplate.FindName("borderTitle", this);
            var closeBtn = (Button)baseWindowTemplate.FindName("btnClose", this);
            var minBtn = (Button)baseWindowTemplate.FindName("btnMin", this);
            YesButton = (Button)baseWindowTemplate.FindName("btnYes", this);
            NoButton = (Button)baseWindowTemplate.FindName("btnNo", this);

            minBtn.Click += delegate
            {
                MinWin();
            };

            closeBtn.Click += delegate
            {
                Close();
            };

            borderTitle.MouseMove += delegate(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };


        }

        public Canvas GridContent
        {
            get;
            set;
        }


        private void InitializeStyle()
        {
            Style = (Style)Application.Current.Resources["BaseWindowStyle"];
        }

        private void InitializeTheme()
        {
            const string themeName = Const.Theme;
            Application.Current.Resources.MergedDictionaries.Add(
                Application.LoadComponent(new Uri(string.Format("../Theme/{0}/WindowBaseStyle.xaml", themeName),
                    UriKind.Relative)) as ResourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(
                Application.LoadComponent(new Uri(string.Format("../Theme/{0}/Style.xaml", themeName), UriKind.Relative))
                    as ResourceDictionary);
        }

        private bool _allowSizeToContent;
        /// <summary>
        /// 自定义属性，用于标记该窗体是否允许按内容适应，设此属性是为了解决最大化按钮当SizeToContent属性为WidthAndHeight时不能最大化，从而最大、最小化必须变更SizeToContent的值的问题
        /// </summary>
        public bool AllowSizeToContent
        {
            get
            {
                return _allowSizeToContent;
            }
            set
            {
                SizeToContent = (value ? SizeToContent.WidthAndHeight : SizeToContent.Manual);
                _allowSizeToContent = value;
            }
        }
    }
}
