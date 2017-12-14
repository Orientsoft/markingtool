using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.Open.Model.Marking;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary>
    /// 文件管理
    /// </summary>
    public class FileManager
    {
        private readonly Logger _logger = Logger.L<FileManager>();

        private readonly string _baseName;
        /// <summary>
        /// 基础文件夹
        /// </summary>
        private string _baseDirectory;

        /// <summary>
        /// 预处理文件
        /// </summary>
        private string _scannerDirectory;

        /// <summary>
        /// 碎片文件
        /// </summary>
        private string _sliceDirectory;

        /// <summary>
        /// 阅卷保存文件
        /// </summary>
        private string _savedDirectory;

        /// <summary>
        /// 压缩文件
        /// </summary>
        private string _compressDirectory;

        private const string ImageNameFormat = "{0}.jpg";

        public FileManager(string batch, byte printType)
        {
            _baseName = string.Format("{0}_{1}", batch, printType);
            InitDirectory();
        }
        public string SliceDirectory { get { return _sliceDirectory; } }

        /// <summary>
        /// 初始化文件夹
        /// </summary>
        private void InitDirectory()
        {
            //基础文件夹
            _baseDirectory = Path.Combine(DeyiKeys.MarkingPath, _baseName);
            if (!Directory.Exists(_baseDirectory))
                Directory.CreateDirectory(_baseDirectory);

            //阅卷文件
            _savedDirectory = Path.Combine(_baseDirectory, DeyiKeys.SaveDirectory);
            if (!Directory.Exists(_savedDirectory))
                Directory.CreateDirectory(_savedDirectory);

            //压缩文件
            _compressDirectory = Path.Combine(_baseDirectory, DeyiKeys.CompressName);
            if (!Directory.Exists(_compressDirectory))
                Directory.CreateDirectory(_compressDirectory);
            //预处理
            _scannerDirectory = Path.Combine(_baseDirectory, DeyiKeys.ScannerName);
            if (!Directory.Exists(_scannerDirectory))
                Directory.CreateDirectory(_scannerDirectory);

            //碎片文件
            _sliceDirectory = Path.Combine(_baseDirectory, DeyiKeys.SliceName);
            if (!Directory.Exists(_sliceDirectory))
                Directory.CreateDirectory(_sliceDirectory);
        }

        /// <summary>
        /// 保存扫描图片
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="imageName"></param>
        public void SavePicture(Bitmap bmp,string imageName)
        {
            bmp.Save(Path.Combine(_scannerDirectory, imageName));
        }

        /// <summary>
        /// 获取扫描图片
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public Bitmap GetPicture(string imageName)
        {
            return (Bitmap) Image.FromFile(Path.Combine(_scannerDirectory, imageName));
        }

        /// <summary>
        /// 获取识别图片
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Bitmap GetRecognitionImage(string imageName, RecognitionType type = RecognitionType.Qrcode)
        {
            imageName = Path.GetFileNameWithoutExtension(imageName) ?? string.Empty;
            var path = Path.Combine(_sliceDirectory, imageName, DeyiKeys.RecognitionName,
                ImageNameFormat.FormatWith((byte)type));
            if (!File.Exists(path))
                return null;
            return (Bitmap)Image.FromFile(path);
        }

        /// <summary>
        /// 保存识别图片
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="image"></param>
        /// <param name="type"></param>
        public void SaveRecognitionImage(string imageName, Image image, RecognitionType type = RecognitionType.Qrcode)
        {
            if (image == null)
                return;
            var path = Path.Combine(_sliceDirectory, imageName, DeyiKeys.RecognitionName,
                ImageNameFormat.FormatWith((byte)type));
            image.Save(path, ImageFormat.Jpeg);
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="image"></param>
        /// <param name="index"></param>
        public void SaveImage(string imageName, Image image, int index)
        {
            if (image == null)
                return;
            var path = Path.Combine(_sliceDirectory, imageName, ImageNameFormat.FormatWith(index));
            image.Save(path, ImageFormat.Jpeg);
        }

        /// <summary>
        /// 单个扫描图片文件检测
        /// </summary>
        /// <param name="imageName"></param>
        public void CheckDirectory(string imageName)
        {
            //文件夹检测
            var directory = Path.Combine(_sliceDirectory, imageName);

            //识别文件
            var regDirectory = Path.Combine(directory, DeyiKeys.RecognitionName);
            if (!Directory.Exists(regDirectory))
                Directory.CreateDirectory(regDirectory);
            //压缩文件
            var compressDirectory = Path.Combine(directory, DeyiKeys.CompressName);
            if (!Directory.Exists(compressDirectory))
                Directory.CreateDirectory(compressDirectory);
        }

        /// <summary>
        /// 获取批次号保存文件路径
        /// </summary>
        /// <returns></returns>
        private List<string> LoadPath()
        {
            return new List<string>
            {
                Path.Combine(_savedDirectory, "saved.dayeasy"),
                Path.Combine(_savedDirectory, "saved.mark")
            };
        }

        /// <summary>
        /// 保存阅卷结果
        /// </summary>
        /// <param name="markedInfos"></param>
        /// <param name="markingResults"></param>
        public void SaveMarkedResult(ObservableCollection<PaperMarkedInfo> markedInfos, List<MarkingResult> markingResults)
        {
            if (!markedInfos.Any())
                return;
            try
            {
                List<MarkingResult> results;
                var list = LoadMarkedResult(out results);
                var paths = LoadPath();
                Helper.Serialize(markedInfos, paths[0]);
                Helper.Serialize(markingResults, paths[1]);
                //删除差集文件夹
                var names = new List<string>();
                foreach (var paperName in list.Select(t => t.PaperName))
                {
                    if (markedInfos.All(t => t.PaperName != paperName))
                        names.Add(paperName);
                }
                DeleteMarkingDirectory(names);
            }
            catch (Exception ex)
            {
                _logger.E("保存阅卷异常", ex);
            }
        }

        /// <summary>
        /// 加载阅卷结果
        /// </summary>
        /// <param name="markingResults"></param>
        /// <returns></returns>
        public ObservableCollection<PaperMarkedInfo> LoadMarkedResult(out List<MarkingResult> markingResults)
        {
            var paths = LoadPath();
            markingResults = (File.Exists(paths[1])
                ? Helper.Deserialize<List<MarkingResult>>(paths[1])
                : new List<MarkingResult>());
            return File.Exists(paths[0])
                ? Helper.Deserialize<ObservableCollection<PaperMarkedInfo>>(paths[0])
                : new ObservableCollection<PaperMarkedInfo>();
        }

        /// <summary>
        /// 清空批阅文件的文件
        /// </summary>
        /// <param name="isDepth">是否深度清空，将删除整个批次号的批阅文件</param>
        public void ClearFile(bool isDepth = false)
        {
            try
            {
                if (isDepth)
                {
                    Directory.Delete(_baseDirectory, true);
                    return;
                }
                List<MarkingResult> results;
                var list = LoadMarkedResult(out results);
                var paths = LoadPath();
                foreach (var path in paths.Where(File.Exists))
                {
                    File.Delete(path);
                }
                //删除文件夹
                DeleteMarkingDirectory(list.Select(t => t.PaperName));
            }
            catch (Exception ex)
            {
                _logger.E("删除保存文件异常", ex);
            }
        }

        /// <summary>
        /// 删除阅卷文件夹
        /// </summary>
        /// <param name="nameList"></param>
        private void DeleteMarkingDirectory(IEnumerable<string> nameList)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var name in nameList)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(name))
                            continue;
                        var path = Path.Combine(_baseDirectory, name);
                        if (Directory.Exists(path))
                            Directory.Delete(path, true);
                        else
                            _logger.E(string.Format("没有找到文件夹:{0}", path));
                    }
                    catch (Exception ex)
                    {
                        _logger.E(string.Format("删除文件夹异常:{0}", name), ex);
                    }
                }
            });
        }

        /// <summary>
        /// 获取所有的切片路径
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public List<string> GetMarkingFileList(string imageName)
        {
            var basePath = Path.Combine(_sliceDirectory, imageName);
            return Helper.GetAllImagePath(basePath)
                .OrderBy(t => Convert.ToInt32(Path.GetFileNameWithoutExtension(t)))
                .ToList();
        }

        /// <summary>
        /// 打包阅卷图片
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public string PacketMarkedPicture(string imageName)
        {
            var zipFile = Path.Combine(_compressDirectory, imageName);
            var path = Path.Combine(_sliceDirectory, imageName, DeyiKeys.CompressName, DeyiKeys.MarkedName);
            var crc = new Crc32();
            using (var file = new FileStream(zipFile, FileMode.Create, FileAccess.ReadWrite))
            {
                var stream = new ZipOutputStream(file);
                try
                {
                    stream.SetLevel(9);
                    var dir = new DirectoryInfo(path);
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
                return zipFile;
            }
        }

        /// <summary>
        /// 基础文件夹检测
        /// </summary>
        public static void CheckBaseDirectory()
        {
            var directory = DeyiKeys.ItemPath;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            directory = Path.Combine(DeyiKeys.MarkingPath, DeyiKeys.DebugName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// 识别图片类型
    /// </summary>
    public enum RecognitionType : byte
    {
        /// <summary>
        /// 二维码
        /// </summary>
        Qrcode = 0,
        /// <summary>
        /// 答题卡
        /// </summary>
        AnswerSheet = 1
    }
}
