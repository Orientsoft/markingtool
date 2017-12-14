using System;
using System.Windows;

namespace Deyi.AutoUpdater
{
    class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            if (args[0] == "update" && args.Length == 7)
            {
                try
                {
                    var app = new App();
                    var downUi = new UI.DownFileProcess(args[1], args[2], args[3], args[4], args[5], args[6])
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    app.Run(downUi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
