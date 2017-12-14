using Deyi.AutoUpdater.Core;
using System;
using System.IO;
using System.Windows;

namespace Deyi.AutoUpdater.UI
{
    /// <summary>
    /// Interaction logic for UpdateInfo.xaml
    /// </summary>
    public partial class UpdateInfoWin : WindowBase
    {
        public UpdateInfoWin()
        {
            InitializeComponent();
            Loaded += UpdateInfoWin_Loaded;
        }

        void UpdateInfoWin_Loaded(object sender, RoutedEventArgs e)
        {
            Title = "软件更新提醒";
            //MessageBox.Show(Updater.Instance.CurrentVersion.ToString()),
            //if (MessageBox.Show("发现新版本,更新程序将自动关闭当前运用程序,如果必要,请保存当前文档!是否现在更新?", "软件更新提醒", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            //{
            //    return;
            //}
            //return;
            //txtDes.Text = Info.Desc;

        }

        public ManifestInfo Info
        {
            get;
            set;
        }

        public string CallExeName
        {
            get;
            set;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //更新程序复制到缓存文件夹
            string startUpDir1 =
                Path.Combine(System.Reflection.Assembly.GetEntryAssembly()
                    .Location.Substring(0,
                        System.Reflection.Assembly.GetEntryAssembly()
                            .Location.LastIndexOf(Path.DirectorySeparatorChar)));
            string startUpDir2 =
                Path.Combine(startUpDir1.Substring(0, startUpDir1.LastIndexOf(Path.DirectorySeparatorChar)));
            string updateFileDir = Path.Combine(startUpDir2, "Update");
            if (!Directory.Exists(updateFileDir))
            {
                Directory.CreateDirectory(updateFileDir);
            }
            string exeDir = Guid.NewGuid().ToString();
            string updateExeDir = Path.Combine(updateFileDir, exeDir);
            if (!Directory.Exists(updateExeDir))
            {
                Directory.CreateDirectory(updateExeDir);
            }

            string exePath = Path.Combine(updateExeDir, "Deyi.AutoUpdater.exe");
            File.Copy(Path.Combine(startUpDir1, "Deyi.AutoUpdater.exe"), exePath, true);

            var info = new System.Diagnostics.ProcessStartInfo(exePath)
            {
                UseShellExecute = true,
                WorkingDirectory = exePath.Substring(0, exePath.LastIndexOf(Path.DirectorySeparatorChar)),
                Arguments = "update " + CallExeName + " " + updateFileDir + " " + startUpDir1 + " " + exePath
            };
            System.Diagnostics.Process.Start(info);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
