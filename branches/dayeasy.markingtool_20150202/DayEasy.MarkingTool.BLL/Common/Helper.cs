using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.Open.Model.Enum;
using DayEasy.Open.Model.Marking;
using DayEasy.Open.Model.Question;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Serialization;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary>
    /// 通用辅助
    /// </summary>
    public abstract class Helper
    {
        public static readonly Func<int, IEnumerable<int>> EachMax = delegate(int max)
        {
            max = Math.Abs(max);
            return Enumerable.Range(0, max);
        };

        public static readonly Func<int, int, IEnumerable<int>> Each = delegate(int min, int max)
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

        public static string ConvertObject2Xml(object obj)
        {
            StringWriter writer = null;
            string str;
            try
            {
                var sb = new StringBuilder();
                writer = new StringWriter(sb);
                new XmlSerializer(obj.GetType()).Serialize(writer, obj);
                str = sb.ToString();
            }
            catch (Exception)
            {
                return String.Empty;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
            return str;
        }

        public static T ConvertXml2Object<T>(string filePath) where T : class, new()
        {
            StreamReader stream = null;
            T obj;
            try
            {
                stream = File.OpenText(filePath);

                obj = new XmlSerializer(typeof(T)).Deserialize(stream) as T;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return obj;
        }

        public static string GetAppSetting(string keyName)
        {
            return ConfigurationManager.AppSettings[keyName];
        }

        public static List<string> GetAllImagePath(string path, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            var di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles("*.*", SearchOption.TopDirectoryOnly);

            var picPaths = new List<string>();

            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    if (file.Extension == (".jpg") ||
                        file.Extension == (".png") ||
                        file.Extension == (".bmp") ||
                        file.Extension == (".gif"))
                    {
                        picPaths.Add(file.FullName);
                    }
                }
            }

            return picPaths;
        }

        //public static T GetElementUnderMouse<T>() where T : UIElement
        //{
        //    return FindVisualParent<T>(Mouse.DirectlyOver as UIElement);
        //}

        /// <summary>
        /// 压缩阅卷图片
        /// </summary>
        /// <param name="directoryName"></param>
        /// <param name="batchNo"></param>
        public static void PacketMarkedPicture(string directoryName, string batchNo)
        {
            string path = Path.Combine(DeyiKeys.CompressedPath, batchNo);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var crc = new Crc32();
            using (var file = new FileStream(Path.Combine(path, directoryName), FileMode.Create, FileAccess.ReadWrite))
            {
                var stream = new ZipOutputStream(file);
                try
                {
                    stream.SetLevel(9);
                    var dir =
                        new DirectoryInfo(Path.Combine(DeyiKeys.SavePath, directoryName, DeyiKeys.CompressName,
                            DeyiKeys.MarkedName));
                    foreach (FileInfo f in dir.GetFiles())
                    {
                        FileStream singleFile = f.OpenRead();
                        var buffer = new byte[singleFile.Length];
                        singleFile.Read(buffer, 0, buffer.Length);
                        singleFile.Close();

                        crc.Reset();
                        crc.Update(buffer);
                        var entry = new ZipEntry(f.Name) { Crc = crc.Value, DateTime = DateTime.Now };

                        stream.PutNextEntry(entry);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
                finally
                {
                    stream.Flush();
                    stream.Close();
                    stream.Dispose();
                }
            }
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

        public static void LoadMarkedPaper(List<MarkingResult> markingResultList, string markedKey, long userId)
        {
            // System.IO.Path.Combine(Helper.UnfinishedPaperSavePath,
            var path = Path.Combine(DeyiKeys.UnfinishedPaperSavePath, userId.ToString(),
                String.Format("{0}.mark", markedKey));
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("没有保存本地阅卷记录");
            }
            var temp = Deserialize<List<MarkingResult>>(path);
            if (temp != null && temp.Count > 0)
            {
                temp.ForEach(mark =>
                {
                    if (markingResultList.All(mrl => mrl.Id != mark.Id))
                    //(mrl => mrl.Batch == mark.Batch && mrl.StudentIdentity == mark.StudentIdentity))
                    {
                        markingResultList.Add(mark);
                    }
                });
            }
        }

        /// <summary>
        /// 判断是否是客观题
        /// </summary>
        /// <param name="questionInfo"></param>
        /// <returns></returns>
        public static bool IsObjective(QuestionInfo questionInfo)
        {
            if (questionInfo == null) return false;
            return questionInfo.IsObjective;
        }

        /// <summary>
        /// 获得当前应用软件的版本
        /// </summary>
        public static Version CurrentVersion()
        {
            var location = Assembly.GetEntryAssembly().Location;
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

        public static byte ToByte(string str, byte def)
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

        /// <summary>
        /// 获取批次号保存文件路径
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        private static List<string> LoadPath(PaperKind kind, string batchNo)
        {
            var tempPath = Path.Combine(DeyiKeys.UnfinishedPaperSavePath,
                DeyiApp.CurrentUser.UserId.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            return new List<string>
            {
                Path.Combine(tempPath, String.Format("{0}_{1}.dayeasy", batchNo, kind)),
                Path.Combine(tempPath, String.Format("{0}_{1}.mark", batchNo, kind))
            };
        }

        /// <summary>
        /// 保存阅卷结果
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="batchNo"></param>
        /// <param name="markedInfos"></param>
        /// <param name="markingResults"></param>
        public static void SaveMarkedResult(PaperKind kind, string batchNo,
            ObservableCollection<PaperMarkedInfo> markedInfos, List<MarkingResult> markingResults)
        {
            if (!markedInfos.Any())
                return;
            var paths = LoadPath(kind, batchNo);
            Serialize(markedInfos, paths[0]);
            Serialize(markingResults, paths[1]);
        }

        /// <summary>
        /// 加载阅卷结果
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="batchNo"></param>
        /// <param name="markingResults"></param>
        /// <returns></returns>
        public static ObservableCollection<PaperMarkedInfo> LoadMarkedResult(PaperKind kind, string batchNo,
            out List<MarkingResult> markingResults)
        {
            var paths = LoadPath(kind, batchNo);
            markingResults = (File.Exists(paths[1])
                ? Deserialize<List<MarkingResult>>(paths[1])
                : new List<MarkingResult>());
            return File.Exists(paths[0])
                ? Deserialize<ObservableCollection<PaperMarkedInfo>>(paths[0])
                : new ObservableCollection<PaperMarkedInfo>();
        }

        public static void DeleteSave(PaperKind kind, string batchNo)
        {
            try
            {
                var paths = LoadPath(kind, batchNo);
                foreach (var path in paths.Where(File.Exists))
                {
                    File.Delete(path);
                }
            }
            catch
            {
            }
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
            var size = LocalPrintServer.GetDefaultPrintQueue()

                .GetPrintCapabilities()

                .PageMediaSizeCapability

                .FirstOrDefault(x => x.PageMediaSizeName == name);
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

        public static int ResetObjectiveResult(string errorWords, MarkingResult result,
            List<QuestionInfo> objectiveQuestions)
        {
            var reg = new Regex("(\\d+(?:-\\d+)?(?:[,，]\\d+(?:-\\d+)?)*)", RegexOptions.IgnoreCase);
            var qids = objectiveQuestions.Select(t => t.QuestionId).Distinct();
            var details = result.Details.Where(d => !d.IsCorrect && qids.Contains(d.QuestionId));
            if (!reg.IsMatch(errorWords))
            {
                #region 全对

                foreach (var detail in details)
                {
                    detail.IsCorrect = true;
                    //重置客观题标记
                    detail.MarkingSymbols = new List<MarkingSymbolInfo>();
                    var qItem = objectiveQuestions.FirstOrDefault(t => t.QuestionId == detail.QuestionId);
                    if (qItem == null) continue;
                    if (string.IsNullOrWhiteSpace(detail.SmallQuestionId))
                    {
                        detail.Score = qItem.Score;
                        SetCorrectAnswers(detail, qItem.Answers);
                    }
                    else
                    {
                        var dItem = qItem.Details.FirstOrDefault(t => t.DetailId == detail.SmallQuestionId);
                        if (dItem == null)
                            continue;
                        detail.Score = dItem.Score;
                        SetCorrectAnswers(detail, dItem.Answers);
                    }
                }

                #endregion

                return 0;
            }
            var list = reg.Match(errorWords).Groups[1].Value.Split(new[] {",", "，"},
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var detail in details)
            {
                if (!list.Contains(detail.MarkingTag))
                {
                    detail.MarkingSymbols = new List<MarkingSymbolInfo>();
                    var qItem = objectiveQuestions.FirstOrDefault(t => t.QuestionId == detail.QuestionId);
                    if (qItem == null) continue;
                    if (string.IsNullOrWhiteSpace(detail.SmallQuestionId))
                    {
                        detail.Score = qItem.Score;
                        SetCorrectAnswers(detail, qItem.Answers);
                    }
                    else
                    {
                        var dItem = qItem.Details.FirstOrDefault(t => t.DetailId == detail.SmallQuestionId);
                        if (dItem == null)
                            continue;
                        detail.Score = dItem.Score;
                        SetCorrectAnswers(detail, dItem.Answers);
                    }
                }
            }
            return list.Count();
        }

        /// <summary>
        /// 设置正确答案
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="answers"></param>
        private static void SetCorrectAnswers(MarkingDetail detail, IEnumerable<AnswerInfo> answers)
        {
            answers = answers.Where(t => t.IsCorrect).ToList();
            detail.AnswerIds = answers.Select(t => t.AnswerId).ToArray();
            detail.StudentAnswer =
                answers.Select(t => t.Sort)
                    .Aggregate("", (c, t) => c + DeyiKeys.OptionWords[t] + ",")
                    .TrimEnd(',');
        }

        private static readonly byte[] PaperTypes =
        {
            (byte) PrintType.HomeWork, (byte) PrintType.PaperAHomeWork, (byte) PrintType.PaperBHomeWork,
            (byte) PrintType.PaperAbHomeWork
        };

        public static PaperKind ConvertPaperHand(byte printType)
        {
            if (PaperTypes.Contains(printType))
                return PaperKind.Paper;
            return PaperKind.AnswerCard;
        }
    }
}
