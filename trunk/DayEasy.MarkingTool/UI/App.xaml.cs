using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.Core;

namespace DayEasy.MarkingTool.UI
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
            if (!Helper.IsConnected)
            {
                WindowsHelper.ShowMsg(MarkingTool.Properties.Resources.NoConnect);
                Shutdown();
                return;
            }
            var dialogResult = new Login().ShowDialog();

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

        /// <summary> 异常处理 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(MarkingTool.Properties.Resources.ExceptionMessage, e.Exception.Message);
            if (e.Exception.InnerException != null)
            {
                stringBuilder.AppendLine(e.Exception.InnerException.Message);
            }
            stringBuilder.AppendLine(e.Exception.StackTrace);
            _logger.E(stringBuilder.ToString());
            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (Directory.Exists(DeyiKeys.ItemPath))
            {
                Directory.Delete(DeyiKeys.ItemPath, true);
            }
        }
    }
}
