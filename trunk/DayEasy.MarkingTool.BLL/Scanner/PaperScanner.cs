using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.BLL.Recognition;
using DayEasy.Models.Open.Paper;
using DayEasy.Models.Open.Work;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DayEasy.MarkingTool.BLL.Scanner
{
    public class PaperScanner
    {
        /// <summary> 试卷信息 </summary>
        private readonly MPaperDto _paper;
        /// <summary> 试卷类型 </summary>
        private readonly byte _sectionType;
        ///// <summary> 答题卡行数 </summary>
        //private int _rowCount;
        /// <summary> 答题卡每题选项数 </summary>
        private List<ObjectiveItem> _sheets;
        /// <summary> 文件管理 </summary>
        private readonly FileManager _fileManager;

        private readonly Logger _logger = Logger.L<PaperScanner>();

        public PaperScanner(MPaperDto paper, FileManager fileManager, byte sectionType = 0)
        {
            _paper = paper;
            _sectionType = sectionType;
            _fileManager = fileManager;
            LoadObjectives();
        }

        /// <summary> 压缩 & 纠偏 & 合并
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        public string Resize(List<string> images)
        {
            var name = Path.GetFileName(images.First());
            var bmps = new List<Bitmap>();
            for (var j = 0; j < images.Count; j++)
            {
                var bmp = (Bitmap)Image.FromFile(images[j]);
                bmp = (Bitmap)ImageHelper.Resize(bmp, DeyiKeys.ScannerConfig.PaperWidth);
                var lines = new LineFinder(bmp).Find(
                    (int)Math.Ceiling(bmp.Width * DeyiKeys.ScannerConfig.BlackScale),
                    DeyiKeys.ScannerConfig.LineHeight, 1, 100);
                if (lines != null && lines.Any())
                {
                    bmp = ImageHelper.RotateImage(bmp, -(float)lines.Average(t => t.Angle));
                }
                //bmp = bmp.ToBinaryArray(diff: 10).ToBitmap();
                bmps.Add(bmp);
            }
            _fileManager.SaveImage(bmps.ToArray(), name);
            return _fileManager.GetImagePath(name);
        }

        public void Scanner(string imagePath, PaperMarkedInfo markedInfo, MPictureInfo picture)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || picture == null)
                return;
            markedInfo.SectionType = _sectionType;
            markedInfo.PaperId = _paper.Id;
            markedInfo.PaperTitle = _paper.PaperTitle;
            markedInfo.PaperNum = _paper.PaperNo;
            var isNew = false;

            if (string.IsNullOrWhiteSpace(picture.Id))
            {
                isNew = true;
                picture.Id = markedInfo.MarkedId;
                picture.SectionType = _sectionType;
            }
            //var sb = new StringBuilder();
            //Action<string> logAction = msg => { sb.AppendLine(msg); };
            try
            {
                using (var scanner = new DefaultRecognition(imagePath, _sheets))
                {
                    var result = scanner.Start();
                    markedInfo.IsSuccess = true;
                    markedInfo.StudentId = result.Student.Id;
                    markedInfo.StudentName = result.Student.Name;
                    markedInfo.StudentCode = result.Student.Code;

                    picture.StudentId = result.Student.Id;
                    picture.StudentName = result.Student.Name;
                    picture.GroupId = result.Student.ClassId;

                    picture.SheetAnwers = result.Sheets;
                    markedInfo.Ratios = picture.SheetAnwers.ToWord();
                    if (isNew)
                    {
                        markedInfo.ImagePath = imagePath;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.E(ex.Message, ex);
                markedInfo.IsSuccess = false;
                markedInfo.Desc = "识别异常";
            }
            finally
            {
                //_logger.I(sb.ToString());
            }
        }

        private void LoadObjectives()
        {
            _sheets = new List<ObjectiveItem>();
            if (_paper == null || _sectionType > 2)
                return;
            _sheets = ObjectiveHelper.GetObjectives(_paper, _sectionType);
        }
    }
}
