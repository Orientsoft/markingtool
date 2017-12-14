# region 四川得一科技有限公司 版权所有
/* ================================================
 * 公司：四川得一科技有限公司
 * 作者：文杰
 * 创建：2013-10-30
 * 描述：工具类
 * ================================================
 */
# endregion

//using AxMTKTWOCXLib;
using Deyi.Tool.PaperServiceReference;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using FlowDirection = System.Windows.FlowDirection;
using MessageBox = System.Windows.MessageBox;

namespace Deyi.Tool.Common
{
    public class Helper
    {
        internal static readonly Func<int, IEnumerable<int>> EachMax = delegate(int max)
        {
            max = Math.Abs(max);
            return Enumerable.Range(0, max);
        };

        internal static readonly Func<int, int, IEnumerable<int>> Each = delegate(int min, int max)
        {
            min = Math.Min(min, max);
            return Enumerable.Range(min, Math.Abs(max - min));
        };

        /// <summary>
        /// 对字符串进行3DES加密
        /// </summary>
        /// <param name="content">需要加密的内容</param>
        /// <returns>3DES加密结果</returns>
        public static string DESEncrypt(string content)
        {
            return DESEncrypt(content, DeyiKeys._KEY, DeyiKeys._IV);
        }

        /// <summary>
        /// 对字符串进行3DES加密
        /// </summary>
        /// <param name="content">需要加密的内容</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns>3DES加密结果</returns>
        public static string DESEncrypt(string content, string key, string iv)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
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
        public static string DESDecrypt(string content)
        {
            return DESDecrypt(content, DeyiKeys._KEY, DeyiKeys._IV);
        }

        /// <summary>
        /// 对字符串进行3DES解密
        /// </summary>
        /// <param name="content">需要解密的内容</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns>3DES解密结果</returns>
        public static string DESDecrypt(string content, string key, string iv)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return string.Empty;
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
        /// 显示错误提示框
        /// </summary>
        /// <param name="message">错误内容</param>
        public static void ShowError(string message)
        {
            MessageBox.Show(message, DeyiKeys._TIP_INFO, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// 显示问题提示框
        /// </summary>
        /// <param name="message">问题内容</param>
        /// <returns></returns>
        public static bool ShowQuestion(string message)
        {
            return MessageBox.Show(message, DeyiKeys._TIP_INFO, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK;
        }

        /// <summary>
        /// 显示选择提示框
        /// </summary>
        /// <param name="message">问题内容</param>
        /// <returns></returns>
        public static bool ShowSure(string message)
        {
            return MessageBox.Show(message, DeyiKeys._TIP_INFO, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        /// <summary>
        /// 调用WCF服务
        /// </summary>
        /// <typeparam name="TContract">需要调用的WCF契约</typeparam>
        /// <param name="action">需要调用的WCF方法</param>
        public static void CallWCF<TContract>(Action<TContract> action)
        {
            var factory = new ChannelFactory<TContract>("*");
            var channel = factory.CreateChannel();
            var client = ((IClientChannel)channel);

            try
            {
                client.Open();
                action(channel);
                client.Close();
            }
            catch
            {
                client.Abort();
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
                return string.Format("{0}秒前", span.Seconds);
            }

            if (span.TotalHours < 1)
            {
                return string.Format("{0}分钟前", span.Minutes);
            }

            if (dateTime.Year == DateTime.Now.Year)
            {
                if (dateTime.Month == DateTime.Now.Month && dateTime.Day == DateTime.Now.Day)
                {
                    return string.Format("今天 {0:t}", dateTime);
                }

                return string.Format("{0:M} {0:t}", dateTime);
            }

            return dateTime.ToString("f");
        }

        /// <summary>
        /// 获取本机Ip
        /// </summary>
        /// <returns></returns>
        public static string[] GetHostIP()
        {
            List<String> ips = new List<string>();
            string hostName = Dns.GetHostName();//本机名   
            return Dns.GetHostAddresses(hostName).Select(p => p.ToString()).ToArray();
        }

        public static string ConvertObject2XML(object obj)
        {
            StringWriter writer = null;
            string str;
            try
            {
                StringBuilder sb = new StringBuilder();
                writer = new StringWriter(sb);
                new XmlSerializer(obj.GetType()).Serialize((TextWriter)writer, obj);
                str = sb.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
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

        public static T ConvertXML2Object<T>(string filePath) where T : class,new()
        {
            StreamReader stream = null;
            T obj;
            try
            {
                stream = File.OpenText(filePath);

                obj = new XmlSerializer(typeof(T)).Deserialize(stream) as T;
            }
            catch (Exception e)
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

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        //public static T GetElementUnderMouse<T>() where T : UIElement
        //{
        //    return FindVisualParent<T>(Mouse.DirectlyOver as UIElement);
        //}

        public static void PacketMarkedPicture(string directoryName, string batchNo)
        {

            string path = Path.Combine(DeyiKeys.CompressedPath, batchNo);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var crc = new ICSharpCode.SharpZipLib.Checksums.Crc32();
            using (var file = new FileStream(Path.Combine(path, directoryName), FileMode.Create, FileAccess.ReadWrite))
            {
                var stream = new ZipOutputStream(file);
                try
                {
                    stream.SetLevel(9);
                    var dir = new DirectoryInfo(Path.Combine(DeyiKeys.SavePath, directoryName));
                    foreach (FileInfo f in dir.GetFiles())
                    {
                        FileStream singleFile = f.OpenRead();
                        var buffer = new byte[singleFile.Length];
                        singleFile.Read(buffer, 0, buffer.Length);
                        singleFile.Close();

                        crc.Reset();
                        crc.Update(buffer);
                        var entry = new ZipEntry(f.Name) {Crc = crc.Value, DateTime = DateTime.Now};

                        stream.PutNextEntry(entry);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception)
                {

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

            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
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

            FileStream fs = new FileStream(filePath, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
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

        public static Stream EDStream(Stream stream)
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

                    Array.ForEach(buffer, b => b = (byte)(byte.MaxValue - b));

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
            var path = Path.Combine(DeyiKeys.UnfinishedPaperSavePath, userId.ToString(), string.Format("{0}.mark", markedKey));
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("没有保存本地阅卷记录");
            }
            var temp = Deserialize<List<MarkingResult>>(path);
            if (temp != null && temp.Count > 0)
            {
                temp.ForEach(mark =>
                {
                    if (!markingResultList.Any(mrl => mrl.ID == mark.ID))//(mrl => mrl.Batch == mark.Batch && mrl.StudentIdentity == mark.StudentIdentity))
                    {
                        markingResultList.Add(mark);
                    }
                });
            }
        }

        /// <summary>
        /// 判断是否是客观题
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public static bool IsObjective(byte typeId, byte subjectId)
        {
            //byte[] objectiveType = new byte[] { 1,2,3,4,5,6,18 };
            //return objectiveType.Contains(typeId);
            if (subjectId == 3)
            {
                if (typeId > 6 && typeId != 18)
                {
                    return false;
                }
                return true;
            }
            if (typeId > 5)
            {
                return false;
            }
            return true;
        }

        public static DrawingVisual PrintingMarked(IEnumerable<MarkingSymbolInfo> marked, IEnumerable<TeacherCommentInfo> comment)
        {
            // Create an instance of a DrawingVisual.
            var drawingVisual = new DrawingVisual();
            // Retrieve the DrawingContext from the DrawingVisual.
            var drawingContext = drawingVisual.RenderOpen();
            Rect rectRange;
            if (marked != null)
            {
                ImageSource hookImg = new BitmapImage(new Uri("pack://application:,,,/Images/Correct.png", UriKind.Absolute)),
                    forksImg = new BitmapImage(new Uri("pack://application:,,,/Images/Incorrect.png", UriKind.Absolute));
                foreach (var m in marked)
                {
                    rectRange = new Rect(m.Position.X, m.Position.Y, 40d, 40d);
                    if (m.SymbolType == MarkingSymbolType.Right)
                    {
                        drawingContext.DrawImage(hookImg, rectRange);
                    }
                    else if (m.SymbolType == MarkingSymbolType.Wrong)
                    {
                        drawingContext.DrawImage(forksImg, rectRange);
                    }
                }
            }

            if (comment != null)
            {
                foreach (var c in comment)
                {

                    rectRange = new Rect(c.Position.X, c.Position.Y, 40d, 40d);
                    drawingContext.DrawText(new FormattedText(c.CommentText ?? string.Empty,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("宋体"), FontStyles.Normal, FontWeights.Light, FontStretches.Normal),
                            16,
                            Brushes.DarkRed),
                            new Point(c.Position.X, c.Position.Y));
                }
            }
            drawingContext.Close();

            return drawingVisual;
        }

        public static void DeleteFiles(string directoryPaht, params string[] ext)
        {
            if (ext == null)
            {
                return;
            }
            var directory = new DirectoryInfo(directoryPaht);
            if (!directory.Exists)
            {
                return;
            }
            var files = directory.GetFiles().Where(f =>
            {
                if (ext.Length == 0)
                {
                    return true;
                }
                return ext.Contains(f.Extension);
            });
            try
            {
                foreach (var f in files)
                {
                    if (f.Exists)
                    {
                        f.Delete();
                    }
                }
                directory.Delete();
            }
            catch (Exception)
            {
                //TODO...
            }
        }


        public static void DeleteFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception)
                {
                    //TODO:
                }
            }

        }

        public static string CreateSavePath()
        {
            var path = GetAppSetting("SaveImagePath");
            var directoryName = GetAppSetting("DirectoryName");
            if (string.IsNullOrWhiteSpace(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
            }

            if (string.IsNullOrWhiteSpace(directoryName))
            {
                directoryName = "DeyiFile";
            }

            //string.Format("{0}{1}" DateTime.Now.ToString("yyyy-MM-dd"));
            path = Path.Combine(path, directoryName, DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static int StrToInt(string str, int def)
        {
            int val;
            if (!int.TryParse(str, out val))
                val = def;
            return val;
        }


        //private static string GetFileNameStart()
        //{
        //    string fileNameStartWith = GetAppSetting("NamingRules");
        //    if (string.IsNullOrWhiteSpace(fileNameStartWith))
        //    {
        //        fileNameStartWith = "";
        //    }
        //    return fileNameStartWith;
        //}

        ///// <summary>
        ///// 设置扫描仪默认配置
        ///// </summary>
        ///// <param name="amx">AxMTKTWOCX</param>
        //public static void SetDefaultForScan(AxMTKTWOCX amx, string imagePrefix)
        //{

        //    amx.SetScanImageLayout(0, 0, 8.27, 11.69);

        //    //0:bmp
        //    //1:jpg
        //    //2.单页tif
        //    //3.多页tif
        //    //4.单页pdf
        //    //5.多页pdf
        //    //6.可检索的单页pdf，需要支持OCR识别。
        //    //7.可检索的多页pdf，需要支持OCR识别。
        //    amx.ImageFormat = 1;

        //    // 是否开启自动纠偏。
        //    //1：开启。
        //    //2：不开启。
        //    amx.AutoDeskew = 1;

        //    //设置为黑白扫描
        //    amx.ScanPixelType = 0;

        //    //命名规则
        //    SetNamingRule(amx, imagePrefix);

        //    //阀值
        //    amx.SetScanThreshold(128);

        //    //设置扫描分辨300dpi
        //    // mx.SetMultiStreamResolution(3);
        //    amx.ScanResolution = 300;

        //    //0 反射稿 (普通纸张)
        //    //1 透明稿（胶片、X-Ray）
        //    amx.Material = 0;

           
        //    //..
        //    amx.MultiStream = 1;

        //    //自动纠偏
        //    amx.AutoCrop = 1;

        //    //自动出去空白页
        //    amx.AutoDiscardBlankPages = 1;

        //}

        //public static void SetNamingRule(AxMTKTWOCX amx, string imagePrefix, int nStartIndex = 1, int indexLenght = 4)
        //{
        //    amx.SetImageName(CreateSavePath(), imagePrefix, nStartIndex, indexLenght);
        //}

    }
}
