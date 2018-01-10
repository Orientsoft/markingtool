using DayEasy.MarkingTool.BLL.Data;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.Models.Open.Work;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary> 文件管理 </summary>
    public class FileManager : IDisposable
    {
        private readonly Logger _logger = Logger.L<FileManager>();
        private bool _isInitDirectory;

        private readonly string _baseName;

        /// <summary> 基础文件夹 </summary>
        private string _baseDirectory;

        /// <summary> 扫描文件 </summary>
        private string _scannerDirectory;

        /// <summary> 压缩文件 </summary>
        private string _compressDirectory;

        private readonly long _userId;
        private readonly string _paperId;

        public FileManager(long userId, string paperId)
        {
            _userId = userId;
            _paperId = paperId;
            _baseName = string.Format("{0}_{1}", userId, paperId);
            InitDirectory();
        }

        /// <summary> 初始化文件夹 </summary>
        public void InitDirectory()
        {
            if (_isInitDirectory)
                return;
            //基础文件夹
            _baseDirectory = Path.Combine(DeyiKeys.MarkingPath, _baseName);
            if (!Directory.Exists(_baseDirectory))
                Directory.CreateDirectory(_baseDirectory);

            //压缩文件
            _compressDirectory = Path.Combine(_baseDirectory, DeyiKeys.CompressName);
            if (!Directory.Exists(_compressDirectory))
                Directory.CreateDirectory(_compressDirectory);
            //预处理
            _scannerDirectory = Path.Combine(_baseDirectory, DeyiKeys.ScannerName);
            if (!Directory.Exists(_scannerDirectory))
                Directory.CreateDirectory(_scannerDirectory);

            _isInitDirectory = true;                   
        }

        public void SaveImage(Bitmap[] bmps, string imageName)
        {
            var bmp = ImageHelper.CombineBitmaps(bmps.ToList());
            var path = Path.Combine(_scannerDirectory, imageName);
            //ImageHelper.Resize(bmp, path, DeyiKeys.ScannerConfig.PaperWidth);
            Image img = bmp;
            img.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            bmps.ToList().ForEach(t => t.Dispose());
            bmp.Dispose();
        }

        public void SaveDebugImage(Bitmap bmp, string name)
        {
            var path = Path.Combine(DeyiKeys.MarkingPath, DeyiKeys.DebugName, name);
            bmp.Save(path);
        }

        /// <summary> 获取扫描图片 </summary>
        public Bitmap GetPicture(string imageName)
        {
            return (Bitmap)Image.FromFile(GetImagePath(imageName));
        }

        public string GetImagePath(string imageName)
        {
            return Path.Combine(_scannerDirectory, imageName);
        }

        /// <summary> 保存阅卷结果 </summary>
        /// <param name="markedInfos"></param>
        /// <param name="markingPictures"></param>
        public void SaveMarkedResult(ObservableCollection<PaperMarkedInfo> markedInfos, IList<MPictureInfo> markingPictures)
        {
            if (!markedInfos.Any())
                return;
            try
            {
                using (var utils = new CacheUtils())
                {
                    var names = utils.SaveScanner(_userId, _paperId, markedInfos, markingPictures);
                    DeleteMarkingDirectory(names);
                }
            }
            catch (Exception ex)
            {
                _logger.E("保存阅卷异常", ex);
            }
        }

        /// <summary> 加载阅卷结果 </summary>
        /// <param name="markingPictures"></param>
        /// <returns></returns>
        public ObservableCollection<PaperMarkedInfo> LoadMarkedResult(out List<MPictureInfo> markingPictures)
        {
            using (var utils = new CacheUtils())
            {
                var model = utils.GetScanner(_userId, _paperId);
                markingPictures = new List<MPictureInfo>();
                if (model == null || string.IsNullOrWhiteSpace(model.ScannerList) ||
                    string.IsNullOrWhiteSpace(model.Pictures))
                    return new ObservableCollection<PaperMarkedInfo>();
                markingPictures = model.Pictures.JsonToObject<List<MPictureInfo>>();
                return XmlHelper.XmlDeserialize<ObservableCollection<PaperMarkedInfo>>(
                    model.ScannerList ?? string.Empty,
                    Encoding.UTF8);
            }
        }

        /// <summary> 清空批阅文件的文件 </summary>
        /// <param name="isDepth">是否深度清空，将删除整个批次号的批阅文件</param>
        public void ClearFile(bool isDepth = false)
        {
            try
            {
                if (isDepth)
                {
                    Directory.Delete(_baseDirectory, true);
                    _isInitDirectory = false;
                    return;
                }
                List<MPictureInfo> results;
                var list = LoadMarkedResult(out results);
                using (var utils = new CacheUtils())
                {
                    utils.ClearScanner(_userId, _paperId);
                }
                //删除文件夹
                DeleteMarkingDirectory(list.Select(t => t.PaperName));
            }
            catch (Exception ex)
            {
                _logger.E("删除保存文件异常", ex);
            }
        }

        /// <summary> 删除阅卷文件夹 </summary>
        /// <param name="nameList"></param>
        public void DeleteMarkingDirectory(IEnumerable<string> nameList)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var name in nameList)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(name))
                            continue;
                        var file = Path.Combine(_scannerDirectory, name + ".jpg");
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.E(string.Format("删除文件夹异常:{0}", name), ex);
                    }
                }
            });
        }

        /// <summary> 打包图片文件 </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public string PacketPictures(IEnumerable<string> names)
        {
            if (!Directory.Exists(_compressDirectory))
                Directory.CreateDirectory(_compressDirectory);
            var zipFile = Path.Combine(_compressDirectory, Helper.Guid32.Substring(5, 6));
            var crc = new Crc32();
            using (var file = new FileStream(zipFile, FileMode.Create, FileAccess.ReadWrite))
            {
                var stream = new ZipOutputStream(file);
                try
                {
                    stream.SetLevel(9);
                    foreach (string name in names)
                    {
                        var f = new FileInfo(Path.Combine(_scannerDirectory, name + ".jpg"));
                        using (FileStream singleFile = f.OpenRead())
                        {
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
                }
                finally
                {
                    stream.Flush();
                    stream.Close();
                    stream.Dispose();
                }
                return zipFile;
            }
        }

        /// <summary> 基础文件夹检测 </summary>
        public static void CheckBaseDirectory()
        {
            var directory = DeyiKeys.ItemPath;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            directory = Path.Combine(DeyiKeys.MarkingPath, DeyiKeys.DebugName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public void Dispose()
        {
        }
    }
}
