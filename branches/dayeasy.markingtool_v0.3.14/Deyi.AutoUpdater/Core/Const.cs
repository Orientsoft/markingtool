using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Deyi.AutoUpdater.Core
{
    public static class Const
    {
        private const string ManifestUrlKey = "manifestUrl";

        internal static string ManifestUrl
        {
            get { return ConfigurationManager.AppSettings.Get(ManifestUrlKey); }
        }

        internal const string Theme = "Black";

        internal static void UpdateProcess(string appDir, bool mandatory, params string[] paras)
        {
            if (string.IsNullOrWhiteSpace(appDir))
                return;
            var exePath = Path.Combine(appDir, "Deyi.AutoUpdater.exe");
            var arg = new StringBuilder();
            arg.Append("update ");
            arg.Append(ToBase64(mandatory ? "1" : "0"));
            arg.Append(" ");
            foreach (var para in paras)
            {
                if (string.IsNullOrWhiteSpace(para)) continue;
                arg.Append(ToBase64(para));
                arg.Append(" ");
            }
            var processStartInfo = new ProcessStartInfo(exePath)
            {
                UseShellExecute = true,
                WorkingDirectory = appDir,
                Arguments = arg.ToString().TrimEnd(' ')
            };
            Process.Start(processStartInfo);
        }

        private static string ToBase64(string arg)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(arg));
        }

        internal static string Base64(string arg)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(arg));
        }
    }
}
