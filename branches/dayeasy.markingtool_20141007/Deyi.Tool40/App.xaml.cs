# region 四川得一科技有限公司 版权所有
/* ================================================
 * 公司：四川得一科技有限公司
 * 作者：文杰
 * 创建：2013-10-30
 * 描述：应用程序逻辑代码
 * ================================================
 */
# endregion

using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Deyi.Tool.Common;

namespace Deyi.Tool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private readonly Logger _logger = Logger.L<App>();
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var dialogResult = new LoginWindow().ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                base.OnStartup(e);
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            else
            {
                Shutdown();
            }
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("应用程序出现了未捕获的异常，{0}/n", e.Exception.Message);
            if (e.Exception.InnerException != null)
            {
                stringBuilder.AppendFormat("/n {0}", e.Exception.InnerException.Message);
            }
            stringBuilder.AppendFormat("/n {0}", e.Exception.StackTrace);
            _logger.E(stringBuilder.ToString());
            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (Directory.Exists(DeyiKeys.ItemPath))
                Directory.Delete(DeyiKeys.ItemPath, true);
        }
    }
}
