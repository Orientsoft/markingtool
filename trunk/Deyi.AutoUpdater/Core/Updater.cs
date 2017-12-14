using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Threading;
using DayEasy.Models.Open.System;

namespace Deyi.AutoUpdater.Core
{
    public class Updater
    {
        private static Updater _instance;
        public static Updater Instance
        {
            get
            {
                return _instance ?? (_instance = new Updater());
            }
        }

        public static void CheckUpdateStatus()
        {
            ThreadPool.QueueUserWorkItem(s =>
            {
                string url = Const.ManifestUrl;
                var client = new WebClient();
                client.Headers.Add("ContentType", "application/json");
                client.DownloadDataCompleted += (x, y) =>
                {
                    try
                    {
                        using (var stream = new MemoryStream(y.Result))
                        {
                            var ser = new DataContractJsonSerializer(typeof(ApiResult<MManifestDto>));
                            var result = (ApiResult<MManifestDto>)ser.ReadObject(stream);
                            if (result.Status && result.Data != null)
                            {
                                result.Data.Md5 = Guid.NewGuid();
                                Instance.StartUpdate(result.Data);
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                };
                client.DownloadDataAsync(new Uri(url));
            });

        }

        public void StartUpdate(MManifestDto updateInfo)
        {
            if (Instance.CurrentVersion >= new Version(updateInfo.Version))
            {
                //当前版本是最新的，不更新
                return;
            }

            //更新程序复制到缓存文件夹
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            //    updateFileDir = Path.Combine(appDir, "Update", updateInfo.Md5.ToString("N"));
            //if (!Directory.Exists(updateFileDir))
            //    Directory.CreateDirectory(updateFileDir);

            //string exePath = Path.Combine(updateFileDir, "Deyi.AutoUpdater.exe");
            //File.Copy(Path.Combine(appDir, "Deyi.AutoUpdater.exe"), exePath, true);

            Const.UpdateProcess(appDir, updateInfo.Mandatory, CallExeName, updateInfo.DownloadUrl, updateInfo.Version,
                updateInfo.UpgradeInstructions, updateInfo.Md5.ToString("N"));
        }
        public bool UpdateFinished = false;

        private string _callExeName;
        public string CallExeName
        {
            get
            {
                if (string.IsNullOrEmpty(_callExeName))
                {
                    _callExeName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
                }
                return _callExeName;
            }
        }

        /// <summary>
        /// 获得当前应用软件的版本
        /// </summary>
        public virtual Version CurrentVersion
        {
            get
            {
                return new Version(FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion);
            }
        }

        /// <summary>
        /// 获得当前应用程序的根目录
        /// </summary>
        public virtual string CurrentApplicationDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); }
        }
    }
}
