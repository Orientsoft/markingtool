using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Data;
using DayEasy.MarkingTool.Core;
using DayEasy.Models.Open.User;
using Deyi.AutoUpdater.Core;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Login
    {
        private readonly Logger _logger = Logger.L<Login>();
        private string _userName;
        private string _userPwd;
        public Login()
        {
            InitializeComponent();
            FileManager.CheckBaseDirectory();
            Loaded += LoginWindow_Loaded;
            TxtPassword.KeyUp += KeyDownHandler;
            TxtPassword.PasswordChanged += TxtPassword_PasswordChanged;
            PasswordPlace.Visibility = (string.IsNullOrWhiteSpace(TxtPassword.Password)
                ? Visibility.Visible
                : Visibility.Hidden);

            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)KeyDownHandler);
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(CheckVersion);
        }

        private void CheckVersion(object args)
        {
            try
            {
                var helper = UpdateHelper.Instance;
                var update = helper.CheckVersion();
                if (!update)
                {
                    UiInvoke(() =>
                    {
                        LblVersion.Content = string.Concat("版本：", Helper.CurrentVersion().ToString());
                        TxtUsername.IsEnabled = true;
                        TxtPassword.IsEnabled = true;
                        BtnLogin.IsEnabled = true;
                    });
                    return;
                }
                Updater.Instance.StartUpdate(helper.Manifest);
                if (helper.Manifest.Mandatory)
                {
                    UiInvoke(Close);
                }
            }
            catch (Exception ex)
            {
                _logger.E(ex.Message, ex);
            }
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                TxtPassword.Background = new SolidColorBrush(Colors.Transparent);
                PasswordPlace.Visibility = Visibility.Visible;
                return;
            }
            TxtPassword.Background = new SolidColorBrush(Colors.White);
            PasswordPlace.Visibility = Visibility.Hidden;
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            _userName = TxtUsername.Text;
            _userPwd = TxtPassword.Password;
            ThreadPool.QueueUserWorkItem(UserLogin);
        }

        #region 界面事件
        /// <summary>
        /// 按钮：登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            _userName = TxtUsername.Text;
            _userPwd = TxtPassword.Password;
            ThreadPool.QueueUserWorkItem(UserLogin);
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 用户登录
        /// </summary>
        private void UserLogin(object args)
        {
            SetControl(false);
#if DEBUG
            const string token = "9d981f67d5304c448d368f50cce6a3dc";// 9d981f67d5304c448d368f50cce6a3dc,d49620da93744f77b060c2d994f9b6a7
            DeyiApp.Token = token;
            var user = RestHelper.Instance.LoadUserInfo();
            if (user == null || !user.Status)
            {
                LoginCallback(new DResult<MLoginDto>(user == null ? "登录失败" : user.Message));
                return;
            }
            LoginCallback(new DResult<MLoginDto>(true, new MLoginDto
            {
                Token = token,
                User = user.Data
            }));
            return;
#endif

            if (string.IsNullOrWhiteSpace(_userName))
            {
                LoginCallback(new DResult<MLoginDto>("登录名称不能为空！"));
                return;
            }

            if (string.IsNullOrWhiteSpace(_userPwd))
            {
                LoginCallback(new DResult<MLoginDto>("登录密码不能为空！"));
                return;
            }

            try
            {
                var result = RestHelper.Instance.Login(_userName.Trim(), _userPwd.Trim());
                if (!result.Status)
                {
                    LoginCallback(result);
                    return;
                }
                LoginCallback(result);
            }
            catch (NullReferenceException ex)
            {
                _logger.E("Login", ex);
                LoginCallback(new DResult<MLoginDto>(ex.Message));
            }
            catch (Exception ex)
            {
                LoginCallback(new DResult<MLoginDto>(ex.Message));
            }
        }

        private void LoginCallback(DResult<MLoginDto> result)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (!result.Status)
                {
                    WindowsHelper.ShowError(result.Message);
                    if (result.Message.IndexOf("登陆名称", StringComparison.Ordinal) >= 0)
                        TxtUsername.Focus();
                    if (result.Message.IndexOf("密码", StringComparison.Ordinal) >= 0)
                    {
                        TxtPassword.Clear();
                        TxtPassword.Focus();
                    }
                    SetControl(true);
                    return;
                }
                if ((result.Data.User.Role & 4) == 0)
                {
                    WindowsHelper.ShowError("扫描工具只针对教师用户开放！");
                    SetControl(true);
                    return;
                }
                DeyiApp.Token = result.Data.Token;
                DeyiApp.CurrentUser = result.Data.User;
                using (var utils = new CacheUtils())
                {
                    utils.Set(DeyiApp.CurrentUser.Email);
                }
                SetControl(true);
                DialogResult = true;
                Close();
            }));
        }

        // 设置控件可用
        private void SetControl(bool isEnabled)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                TxtUsername.IsEnabled = isEnabled;
                TxtPassword.IsEnabled = isEnabled;
                BtnLogin.IsEnabled = isEnabled;
                BtnLogin.Content = isEnabled ? "登 录" : "登录中...";
            }));
        }

        #endregion
    }
}
