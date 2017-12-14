using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.Core;
using DayEasy.Open.Model.User;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow
    {
        private readonly Logger _logger = Logger.L<LoginWindow>();
        private string _userName;
        private string _userPwd;
        public LoginWindow()
        {
            InitializeComponent();
            TxtUsername.Focus();
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler) KeyDownHandler);
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _userName = TxtUsername.Text;
                _userPwd = TxtPassword.Password;
                ThreadPool.QueueUserWorkItem(UserLogin);
            }
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

        /// <summary>
        /// 按钮：取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (WindowsHelper.ShowQuestion("作业管理工具将退出并且关闭，确定吗？"))
            {
                DialogResult = false;
                Close();
            }
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
            //const string token = "2cb6c1b3dfd9429fbd105e3ff44f3e02";
            const string token = "8a6247478bd141cf86eacadc128bebbc";
            //const string token = "9b81d92f656e4139853c422f815a82af";
            var user = RestHelper.Instance.LoadUserInfo(token);
            if (!user.Status)
            {
                LoginCallback(new JsonResult<LoginInfo>(user.Description));
                return;
            }
            LoginCallback(new JsonResult<LoginInfo>(true, new LoginInfo
            {
                Token = token,
                User = user.Data
            }));
            return;
#endif

            if (string.IsNullOrWhiteSpace(_userName))
            {
                LoginCallback(new JsonResult<LoginInfo>("登录名称不能为空！"));
                return;
            }

            if (string.IsNullOrWhiteSpace(_userPwd))
            {
                LoginCallback(new JsonResult<LoginInfo>("登录密码不能为空！"));
                return;
            }

            try
            {
                JsonResult<LoginInfo> result = RestHelper.Instance.Login(_userName.Trim(), _userPwd.Trim());
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
                LoginCallback(new JsonResult<LoginInfo>(ex.Message));
            }
            catch (Exception ex)
            {
                LoginCallback(new JsonResult<LoginInfo>(ex.Message));
            }
        }

        private void LoginCallback(JsonResult<LoginInfo> result)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (!result.Status)
                {
                    WindowsHelper.ShowError(result.Description);
                    if (result.Description.IndexOf("登陆名称", StringComparison.Ordinal) >= 0)
                        TxtUsername.Focus();
                    if (result.Description.IndexOf("密码", StringComparison.Ordinal) >= 0)
                    {
                        TxtPassword.Clear();
                        TxtPassword.Focus();
                    }
                    SetControl(true);
                    return;
                }
                DeyiApp.Token = result.Data.Token;
                DeyiApp.CurrentUser = result.Data.User;
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
                BtnCancel.IsEnabled = isEnabled;
            }));
        }

        #endregion
    }
}
