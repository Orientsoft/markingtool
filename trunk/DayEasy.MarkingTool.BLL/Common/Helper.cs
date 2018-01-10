using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Printing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DayEasy.Models.Open.Paper;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary>
    /// 通用辅助
    /// </summary>
    public abstract class Helper
    {
        private static Logger _logger = Logger.L<Helper>();

        public static readonly Func<int, IEnumerable<int>> EachMax = delegate (int max)
        {
            max = Math.Abs(max);
            return Enumerable.Range(0, max);
        };

        public static readonly Func<int, int, IEnumerable<int>> Each = delegate (int min, int max)
        {
            min = Math.Min(min, max);
            return Enumerable.Range(min, Math.Abs(max - min));
        };

        /// <summary>
        /// 对字符串进行3DES加密
        /// </summary>
        /// <param name="content">需要加密的内容</param>
        /// <returns>3DES加密结果</returns>
        public static string DesEncrypt(string content)
        {
            return DesEncrypt(content, DeyiKeys.Key, DeyiKeys.Iv);
        }

        /// <summary>
        /// 对字符串进行3DES加密
        /// </summary>
        /// <param name="content">需要加密的内容</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns>3DES加密结果</returns>
        public static string DesEncrypt(string content, string key, string iv)
        {
            if (String.IsNullOrWhiteSpace(content))
            {
                return String.Empty;
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var desHash = TripleDES.Create())
                {
                    var data = Encoding.UTF8.GetBytes(content);
                    desHash.Key = Convert.FromBase64String(key);
                    desHash.IV = Convert.FromBase64String(iv);
                    var encryptor = desHash.CreateEncryptor(desHash.Key, desHash.IV);

                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 对字符串进行3DES解密
        /// </summary>
        /// <param name="content">需要解密的内容</param>
        /// <returns>3DES解密结果</returns>
        public static string DesDecrypt(string content)
        {
            return DesDecrypt(content, DeyiKeys.Key, DeyiKeys.Iv);
        }

        /// <summary>
        /// 对字符串进行3DES解密
        /// </summary>
        /// <param name="content">需要解密的内容</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns>3DES解密结果</returns>
        public static string DesDecrypt(string content, string key, string iv)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(content))
                {
                    return String.Empty;
                }

                using (var memoryStream = new MemoryStream())
                {
                    using (var desHash = TripleDES.Create())
                    {
                        var data = Convert.FromBase64String(content);
                        desHash.Key = Convert.FromBase64String(key);
                        desHash.IV = Convert.FromBase64String(iv);
                        var decryptor = desHash.CreateDecryptor(desHash.Key, desHash.IV);

                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(data, 0, data.Length);
                            cryptoStream.FlushFinalBlock();
                            return Encoding.UTF8.GetString(memoryStream.ToArray());
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 按照SNS方式显示时间
        /// </summary>
        /// <param name="dateTime">需要显示的时间</param>
        /// <returns>按照SNS方式显示时间</returns>
        public static string DisplayDateTime(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;

            if (span.TotalSeconds < 1)
            {
                return "1秒前";
            }

            if (span.TotalMinutes < 1)
            {
                return String.Format("{0}秒前", span.Seconds);
            }

            if (span.TotalHours < 1)
            {
                return String.Format("{0}分钟前", span.Minutes);
            }

            if (dateTime.Year == DateTime.Now.Year)
            {
                if (dateTime.Month == DateTime.Now.Month && dateTime.Day == DateTime.Now.Day)
                {
                    return String.Format("今天 {0:t}", dateTime);
                }

                return String.Format("{0:M} {0:t}", dateTime);
            }

            return dateTime.ToString("f");
        }

        /// <summary>
        /// 获取本机Ip
        /// </summary>
        /// <returns></returns>
        public static string[] GetHostIp()
        {
            string hostName = Dns.GetHostName(); //本机名   
            return Dns.GetHostAddresses(hostName).Select(p => p.ToString()).ToArray();
        }

        public static string GetAppSetting(string keyName)
        {
            return ConfigurationManager.AppSettings[keyName];
        }

        public static List<string> GetAllImagePath(string path, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            var di = new DirectoryInfo(path);
            var files = di.GetFiles("*.*", SearchOption.TopDirectoryOnly);

            var picPaths = new List<string>();

            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    if (file.Extension.ToLower() == (".jpg") ||
                        file.Extension.ToLower() == (".png") ||
                        file.Extension.ToLower() == (".bmp") ||
                        file.Extension.ToLower() == (".gif"))
                    {
                        picPaths.Add(file.FullName);
                    }
                }
            }

            return picPaths;
        }

        public static void Serialize<T>(T model, string filename)
        {

            var fs = new FileStream(filename, FileMode.Create);
            var formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, model);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        public static T Deserialize<T>(string filePath)
        {

            if (!File.Exists(filePath))
            {
                return default(T);
            }

            T model;

            var fs = new FileStream(filePath, FileMode.Open);
            try
            {
                var formatter = new BinaryFormatter();
                model = (T)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
            return model;
        }

        public static Stream EdStream(Stream stream)
        {
            var ms = new MemoryStream();
            try
            {

                var reader = new BinaryReader(stream);

                byte[] buffer;

                var writer = new BinaryWriter(ms);
                long offset = ms.Length;
                writer.Seek((int)offset, SeekOrigin.Begin);

                do
                {

                    buffer = reader.ReadBytes(1024);

                    Array.ForEach(buffer, b => b = (byte)(Byte.MaxValue - b));

                    writer.Write(buffer);

                } while (buffer.Length > 0);

                return ms;
            }
            catch (Exception e)
            {
                ms.Close();
                ms.Dispose();
                throw e;
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
        }

        /// <summary>
        /// 判断是否是客观题
        /// </summary>
        /// <param name="questionInfo"></param>
        /// <returns></returns>
        public static bool IsObjective(MQuestionDto questionInfo)
        {
            if (questionInfo == null) return false;
            return questionInfo.IsObjective;
        }

        /// <summary>
        /// 获得当前应用软件的版本
        /// </summary>
        public static Version CurrentVersion()
        {
            var location = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;
            var version = FileVersionInfo.GetVersionInfo(location).ProductVersion;
            if (!String.IsNullOrWhiteSpace(version))
                return new Version(version);
            return new Version();
        }

        public static string CreateSavePath()
        {
            var path = Path.Combine(DeyiKeys.ItemPath, "DeyiScnaner", DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static DateTime DefaultTime = new DateTime(1970, 1, 1);

        public static DateTime ToDateTime(long time)
        {
            if (time <= 0) return DefaultTime;
            return new DateTime(DefaultTime.Ticks + time * 10000);
        }

        public static long ToLong(DateTime time)
        {
            if (time <= DefaultTime) return 0;
            return (time.Ticks - DefaultTime.Ticks) / 10000;
        }

        public static byte ToByte(string str, byte def = 0)
        {
            byte value;
            if (!byte.TryParse(str, out value))
                value = def;
            return value;
        }

        public static long StrToLong(string str, long def = 0)
        {
            long value;
            if (!Int64.TryParse(str, out value))
                value = def;
            return value;
        }

        public static int LengthCn(string str)
        {
            var reg = new Regex("[\u4e00-\u9fa5]", RegexOptions.Compiled);
            return str.Length + str.Count(c => reg.IsMatch(c.ToString(CultureInfo.InvariantCulture)));
        }

        public static int ToInt(object str, int def = 0)
        {
            int value;
            if (!int.TryParse((str ?? string.Empty).ToString(), out value))
                value = def;
            return value;
        }

        public static string Guid32
        {
            get { return Guid.NewGuid().ToString("N"); }
        }

        public static decimal ToDecimal(string txtWord, decimal def = 0)
        {
            var reg = new Regex("^([0-9]+)(\\.[0-9]+)?$");
            if (!reg.IsMatch(txtWord))
                return def;
            decimal score;
            if (!Decimal.TryParse(txtWord, out score))
                score = def;
            return score;
        }

        /// <summary> 
        /// MD5加密 
        /// </summary> 
        /// <param name="str"></param> 
        /// <returns></returns> 
        public static string Md5(string str)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] md5Source = Encoding.UTF8.GetBytes(str);
            byte[] md5Out = md5.ComputeHash(md5Source);
            string userPwdMd5 = BitConverter.ToString(md5Out).Replace("-", "");
            return userPwdMd5.ToUpper();
        }

        public static PageMediaSize GetSize(PageMediaSizeName name)
        {
            PageMediaSize size = null;
            try
            {
                size = LocalPrintServer.GetDefaultPrintQueue()

                   .GetPrintCapabilities()

                   .PageMediaSizeCapability

                   .FirstOrDefault(x => x.PageMediaSizeName == name);
            }
            catch { }
            return size ?? new PageMediaSize(PageMediaSizeName.ISOA4);
        }

        public static PageMediaSize A4Size
        {
            get { return GetSize(PageMediaSizeName.ISOA4); }
        }

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

        /// <summary>
        /// 是否有网络链接
        /// </summary>
        public static bool IsConnected
        {
            get
            {
                int i;
                return InternetGetConnectedState(out i, 0);
            }
        }

        private static readonly Regex PaperNumReg = new Regex("^\\d{11}$", RegexOptions.Singleline);

        /// <summary> 是否是试卷编号 </summary>
        /// <param name="paperNum"></param>
        /// <returns></returns>
        public static bool IsPaperNum(string paperNum)
        {
            return PaperNumReg.IsMatch(paperNum);
        }

        /// <summary>
        /// 异常信息格式化
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="isHideStackTrace"></param>
        /// <returns></returns>
        public static string Format(Exception ex, bool isHideStackTrace = false)
        {
            var sb = new StringBuilder();
            var count = 0;
            var appString = string.Empty;
            while (ex != null)
            {
                if (count > 0)
                {
                    appString += "  ";
                }
                sb.AppendLine(string.Format("{0}异常消息：{1}", appString, ex.Message));
                sb.AppendLine(string.Format("{0}异常类型：{1}", appString, ex.GetType().FullName));
                sb.AppendLine(string.Format("{0}异常方法：{1}", appString,
                    (ex.TargetSite == null ? null : ex.TargetSite.Name)));
                sb.AppendLine(string.Format("{0}异常源：{1}", appString, ex.Source));
                if (!isHideStackTrace && ex.StackTrace != null)
                {
                    sb.AppendLine(string.Format("{0}异常堆栈：{1}", appString, ex.StackTrace));
                }
                if (ex.InnerException != null)
                {
                    sb.AppendLine(string.Format("{0}内部异常：", appString));
                    count++;
                }
                ex = ex.InnerException;
            }
            return sb.ToString();
        }

        public static string UserAgent()
        {
            var agent = new StringBuilder();
            agent.AppendFormat("DayEasy-MarkingTool/{0}/{1}/{2}", CurrentVersion(), Environment.OSVersion,
                Environment.MachineName);
            return agent.ToString();
        }
    }
}
