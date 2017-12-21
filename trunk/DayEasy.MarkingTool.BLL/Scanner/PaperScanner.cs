using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.MarkingTool.BLL.Enum;
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

        /// <summary>
        /// Find all the locating points in the paper image
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private PointsResult FindLocatingPoints(Image bmp)
        {
            return new PointsFinder().Find(bmp);
        }

        /// <summary>
        /// Merge and return results for PaperA
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        private List<PreProcessResult> ProcessPaperA(string name, List<string> images, byte paperCategory)
        {
            var results = new List<PreProcessResult>();
            var bmps = new List<Bitmap>();

            for (var j = 0; j < images.Count; j++)
            {
                var bmp = Resize(images, paperCategory, j);
                bmps.Add(bmp);
            }

            _fileManager.SaveImage(bmps.ToArray(), name);
            var ppr = new PreProcessResult() { ImagePath = _fileManager.GetImagePath(name), IsPaperB = false };
            results.Add(ppr);

            return results;
        }

        /// <summary> 压缩 & 纠偏 & 合并
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        public string PreProcess(List<string> images, byte paperCategory, byte paperType)
        {
            var name = Path.GetFileName(images.First());
            List<PreProcessResult> results;

            if (paperCategory == (byte)PaperCategory.A4)
            {
                // For A4 paper, merge directly
                results = ProcessPaperA(name, images, paperCategory);
            }

            for (var j = 0; j < images.Count; j++)
            {
                // Resize & rotate image
                //Bitmap bmp = Resize(images, paperCategory, j);

                //if (paperCategory == (byte)PaperCategory.A4)
                //{
                //    // For A4 paper, merge directly
                //    bmps.Add(bmp);
                //}
                //else if (paperCategory == (byte)PaperCategory.A3 &&
                //    paperType == (byte)PaperType.Normal)
                //{
                //    // For A3 paper with non-AB type

                //}
                //else if (paperCategory == (byte)PaperCategory.A3 &&
                //  paperType == (byte)PaperType.PaperAb)
                //{
                //    // A3 paper with AB type
                //    var points = FindLocatingPoints(bmp);
                //}
            }

            // Needs refactor
            //_fileManager.SaveImage(bmps.ToArray(), name);
            return _fileManager.GetImagePath(name);
        }

        private static Bitmap Resize(List<string> images, int paperCategory, int index)
        {
            var bmp = (Bitmap)Image.FromFile(images[index]);
            List<LineInfo> lines = new List<LineInfo>();

            // For A3 paper, check the direction and rotate image
            if (paperCategory == (byte)PaperCategory.A3 && bmp.Width < bmp.Height)
            {
                bmp = ImageHelper.RotateA3Image(bmp);
            }

            // For A4 paper
            if (paperCategory == (byte)PaperCategory.A4 && bmp.Width >= DeyiKeys.ScannerConfig.PaperWidth)
            {
                if (bmp.Width >= DeyiKeys.ScannerConfig.PaperWidth)
                {
                    bmp = (Bitmap)ImageHelper.Resize(bmp, DeyiKeys.ScannerConfig.PaperWidth);
                }

                lines = new LineFinder(bmp).Find(
                    (int)Math.Ceiling(bmp.Width * DeyiKeys.ScannerConfig.BlackScale),
                    DeyiKeys.ScannerConfig.LineHeight, 1, 100);
            }
            else
            {
                // Resize for A3 paper
                if (bmp.Width >= (DeyiKeys.ScannerConfig.PaperWidth * 2))
                {
                    bmp = (Bitmap)ImageHelper.Resize(bmp, DeyiKeys.ScannerConfig.PaperWidth * 2);
                }

                lines = new LineFinder(bmp).Find(
                    (int)Math.Ceiling(bmp.Width * (DeyiKeys.ScannerConfig.BlackScale / 2)),
                    DeyiKeys.ScannerConfig.LineHeight, 1, 100);
            }

            if (lines.Any())
            {
                bmp = ImageHelper.RotateImage(bmp, -(float)lines.Average(t => t.Angle));
            }

            return bmp;
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
