# region 四川得一科技有限公司 版权所有
/* ================================================
 * 公司：四川得一科技有限公司
 * 作者：文杰
 * 创建：2013-10-30
 * 描述：登录窗口逻辑代码
 * ================================================
 */
# endregion

using Deyi.Tool.Common;
using Deyi.Tool.Entity.User;
using Deyi.Tool.UserServiceReference;

using System;
using System.Windows;
using System.Windows.Input;


namespace Deyi.Tool
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly Logger _logger = Logger.L<LoginWindow>();
        public LoginWindow()
        {
            InitializeComponent();
            txtUsername.Focus();
        }

        #region 界面事件
        /// <summary>
        /// 按钮：登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            UserLogin();
        }

        /// <summary>
        /// 按钮：取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Helper.ShowQuestion("作业管理工具将退出并且关闭，确定吗？"))
            {
                CloseWindow(false);
            }

        }
        #endregion

        #region 重写事件
        /// <summary>
        /// 键盘回车事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UserLogin();
            }

            base.OnKeyDown(e);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 用户登录
        /// </summary>
        private void UserLogin()
        {
            SetControl(false);

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                Helper.ShowError("登录名称不能为空");
                SetControl(true);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                Helper.ShowError("登录密码不能为空");
                SetControl(true);
                txtPassword.Focus();
                return;
            }

            var loginInfo = new UserLoginInfo
            {
                Email = txtUsername.Text.Trim(),
                Password = txtPassword.Password.Trim(),
                VerifyCode = "000000"
            };
            loginInfo.VerifyCodeInSession = loginInfo.VerifyCode;
            UserLoginResult userInfo = null;

            try
            {
                ResultPacket result = null;
                Helper.CallWCF<User>(service => result = service.UserLogin(out userInfo, loginInfo));

                if (result.IsError)
                {
                    Helper.ShowError(result.Description);
                    SetControl(true);
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }
            }
            catch (NullReferenceException ex)
            {
                _logger.E("Login", ex);
                Helper.ShowQuestion("登录失败,请检查的你网络!");
                SetControl(true);
                txtPassword.Clear();
                txtPassword.Focus();
                return;
            }
            catch (Exception ex)
            {
                _logger.E("Login", ex);
                Helper.ShowError(ex.Message);
                SetControl(true);
                txtPassword.Clear();
                txtPassword.Focus();
                return;
            }

            UserInfo.Current.ID = userInfo.ID;
            UserInfo.Current.Nickname = userInfo.Nickname;
            UserInfo.Current.Email = userInfo.Email;
            UserInfo.Current.TrueName = userInfo.TrueName;
            CloseWindow(true);
        }

        // 关闭本窗口
        private void CloseWindow(bool showMainWindow)
        {
            DialogResult = showMainWindow;
            this.Close();
        }

        // 设置控件可用
        private void SetControl(bool isEnabled)
        {
            txtUsername.IsEnabled = isEnabled;
            txtPassword.IsEnabled = isEnabled;
            btnLogin.IsEnabled = isEnabled;
            btnCancel.IsEnabled = isEnabled;
        }
        #endregion
    }
}
