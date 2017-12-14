using Deyi.AutoUpdater.Core;
using ICCEmbedded.SharpZipLib.Zip;
using System;
using System.Diagnostics;
using System.IO;

namespace Deyi.AutoUpdater.UI
{
    public partial class DownFileProcess
    {
        private readonly string _updateFileDir;//更新文件存放的文件夹
        private readonly string _callExeName;
        private readonly string _appDir;
        private readonly string _downloadUrl;
        private readonly string _appVersion;
        private readonly string _desc;

        public DownFileProcess(string mandatory, string callExeName, string downloadUrl,
            string appVersion, string desc, string md5)
        {
            InitializeComponent();
            _callExeName = Const.Base64(callExeName);
            _appDir = AppDomain.CurrentDomain.BaseDirectory;
            _updateFileDir = Path.Combine(_appDir, "Update", Const.Base64(md5));
            _downloadUrl = Const.Base64(downloadUrl);
            _appVersion = Const.Base64(appVersion);
            if (!Directory.Exists(_updateFileDir))
            {
                Directory.CreateDirectory(_updateFileDir);
            }

            var sDesc = Const.Base64(desc);
            if (string.IsNullOrWhiteSpace(sDesc))
                _desc = string.Empty;
            else
                _desc = "更新内容如下:\r\n" + sDesc;
            YesButton.Focus();

            Loaded += (sl, el) =>
            {
                NoButton.IsEnabled = (Const.Base64(mandatory) == "0");

                YesButton.Click += (sender, e) =>
                {
                    YesButton.IsEnabled = false;
                    Process[] processes = Process.GetProcessesByName(_callExeName);

                    if (processes.Length > 0)
                    {
                        foreach (var p in processes)
                        {
                            p.Kill();
                        }
                    }

                    DownloadUpdateFile();
                };

                NoButton.Click += (sender, e) => Close();

                TxtProcess.Text = "发现新的版本(" + _appVersion + "),是否现在更新?";
                TxtDes.Text = _desc;
            };
        }

        public void DownloadUpdateFile()
        {
            var fileName = Path.GetFileName(_downloadUrl);
            if (string.IsNullOrWhiteSpace(fileName)) return;
            var client = new System.Net.WebClient();
            client.DownloadProgressChanged += (sender, e) => UpdateProcess(e.BytesReceived, e.TotalBytesToReceive);
            client.DownloadDataCompleted += (sender, e) =>
            {
                string installPath = Path.Combine(_updateFileDir, fileName);
                byte[] data = e.Result;
                var writer = new BinaryWriter(new FileStream(installPath, FileMode.OpenOrCreate));
                writer.Write(data);
                writer.Flush();
                writer.Close();

                System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                {
                    Action f = () =>
                    {
                        TxtProcess.Text = "开始更新程序...";
                    };
                    Dispatcher.Invoke(f);

                    f = () =>
                    {
                        TxtProcess.Text = "下载完成!";
                        //try
                        //{
                        //    //清空缓存文件夹
                        //    Directory.Delete(_updateFileDir, true);
                        //}
                        //catch (Exception ex)
                        //{
                        //    //MessageBox.Show(ex.Message);
                        //}
                    };
                    Dispatcher.Invoke(f);
                    try
                    {
                        f = () =>
                        {
                            //覆盖安装软件
                            var info = new ProcessStartInfo(installPath)
                            {
                                UseShellExecute = true,
                                WorkingDirectory = _appDir
                            };
                            Process.Start(info);
                            Close();
                        };
                        Dispatcher.Invoke(f);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                });

            };
            client.DownloadDataAsync(new Uri(_downloadUrl));
        }

        private static void UnZipFile(string zipFilePath, string targetDir)
        {
            var fz = new FastZip(new FastZipEvents());
            fz.ExtractZip(zipFilePath, targetDir, "");
        }

        public void UpdateProcess(long current, long total)
        {
            string status = (int)((float)current * 100 / total) + "%";
            TxtProcess.Text = status;
            RectProcess.Width = ((float)current / total) * BProcess.ActualWidth;
        }

        public void CopyDirectory(string sourceDirName, string destDirName)
        {
            try
            {
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                    File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
                }
                if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
                    destDirName = destDirName + Path.DirectorySeparatorChar;
                string[] files = Directory.GetFiles(sourceDirName);
                foreach (string file in files)
                {
                    File.Copy(file, destDirName + Path.GetFileName(file), true);
                    File.SetAttributes(destDirName + Path.GetFileName(file), FileAttributes.Normal);
                }
                string[] dirs = Directory.GetDirectories(sourceDirName);
                foreach (string dir in dirs)
                {
                    CopyDirectory(dir, destDirName + Path.GetFileName(dir));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("复制文件错误");
            }
        }
    }
}
