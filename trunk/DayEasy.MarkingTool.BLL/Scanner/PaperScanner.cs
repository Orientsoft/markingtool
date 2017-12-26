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
        private byte _sectionType;
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

        public byte SectionType {
            get { return _sectionType; }
            set { _sectionType = value; }
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
        /// Merge and return results for PaperA4
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        private List<PreProcessResult> ProcessPaperA4(string name, List<string> images, byte paperCategory)
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

        /// <summary>
        /// Merge and return results for PaperA3 with non-AB type
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        private List<PreProcessResult> ProcessPaperA3WithNonAB(string name, List<string> images, byte paperCategory)
        {
            var results = new List<PreProcessResult>();
            var bmps = new List<Bitmap>();

            for (var j = 0; j < images.Count; j++)
            {
                var bmp = Resize(images, paperCategory, j);

                // Split image into 2 pieces, then merge them together.
                var pr = FindLocatingPoints(bmp);
                var centerX = pr.GetCenterX();

                // Merge images together
                bmps.Add((Bitmap)ImageHelper.MakeImage(bmp, 0, 0, centerX, bmp.Height, 0, 0, false));
                bmps.Add((Bitmap)ImageHelper.MakeImage(bmp, centerX, 0, centerX, bmp.Height, 0 , 0, false));
            }

            _fileManager.SaveImage(bmps.ToArray(), name);
            var ppr = new PreProcessResult() { ImagePath = _fileManager.GetImagePath(name), IsPaperB = false };
            results.Add(ppr);

            return results;
        }

        /// <summary>
        /// Merge and return results for PaperA3 withAB type
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        private List<PreProcessResult> ProcessPaperA3WithAB(string name, List<string> images, byte paperCategory)
        {
            var results = new List<PreProcessResult>();
            var paperA = new List<Bitmap>();
            var paperB = new List<Bitmap>();
            var paperAFinished = false;

            for (var j = 0; j < images.Count; j++)
            {
                var bmp = Resize(images, paperCategory, j);
                // Split image into 2 pieces, detect it is paperA or paperB, then merge them.
                var pr = FindLocatingPoints(bmp);
                var centerX = pr.GetCenterX();

                if(!pr.HasPaperBPoint && !paperAFinished)
                {
                    // For paperA, split them into 2 pieces, then merge.
                    paperA.Add((Bitmap)ImageHelper.MakeImage(bmp, 0, 0, centerX, bmp.Height, 0, 0, false));
                    paperA.Add((Bitmap)ImageHelper.MakeImage(bmp, centerX, 0, centerX, bmp.Height, 0, 0, false));
                }
                else
                {
                    // For paperB
                    // Check the position of the paper B point (left or right)
                    if(pr.HasPaperBPoint && pr.PaperBPoint.X < centerX)
                    {
                        // Paper B point is on left side
                        // 1. Split paper into left & right parts.
                        var leftSide = ImageHelper.MakeImage(bmp, 0, 0, centerX, bmp.Height, 0, 0, false);
                        var rightSide = (Bitmap)ImageHelper.MakeImage(bmp, centerX, 0, centerX, bmp.Height, 0, 0, false);


                        // 2. Split left side into 2 pieces.
                        var topSide = (Bitmap)ImageHelper.MakeImage(leftSide, 0, 0, leftSide.Width, pr.PaperBPoint.Y, 0, 0, false);
                        var bottomSide = (Bitmap)ImageHelper.MakeImage(leftSide, 0, pr.PaperBPoint.Y, leftSide.Width, leftSide.Height, 0, 0, false);

                        paperA.Add(topSide);
                        // Set paperA as finished status.
                        paperAFinished = true;
                        paperB.Add(bottomSide);
                        paperB.Add(rightSide);
                    }
                    else if (pr.HasPaperBPoint && pr.PaperBPoint.X > centerX)
                    {
                        // Paper B point is on right side
                        // Paper B point is on left side
                        // 1. Split paper into left & right parts.
                        var leftSide = (Bitmap)ImageHelper.MakeImage(bmp, 0, 0, centerX, bmp.Height, 0, 0, false);
                        var rightSide = ImageHelper.MakeImage(bmp, centerX, 0, centerX, bmp.Height, 0, 0, false);


                        // 2. Split left side into 2 pieces.
                        var topSide = (Bitmap)ImageHelper.MakeImage(rightSide, 0, 0, rightSide.Width, pr.PaperBPoint.Y, 0, 0, false);
                        var bottomSide = (Bitmap)ImageHelper.MakeImage(rightSide, 0, pr.PaperBPoint.Y, rightSide.Width, rightSide.Height, 0, 0, false);

                        paperA.Add(leftSide);
                        paperA.Add(topSide);
                        // Set paperA as finished status.
                        paperAFinished = true;
                        paperB.Add(bottomSide);
                    }
                    else
                    {
                        // For the left paperB images
                        paperB.Add((Bitmap)ImageHelper.MakeImage(bmp, 0, 0, centerX, bmp.Height, 0, 0, false));
                        paperB.Add((Bitmap)ImageHelper.MakeImage(bmp, centerX, 0, centerX, bmp.Height, 0, 0, false));
                    }
                }
            }

            _fileManager.SaveImage(paperA.ToArray(), name.AppendFileName("a"));
            var pprA = new PreProcessResult() { ImagePath = _fileManager.GetImagePath(name.AppendFileName("a")), IsPaperB = false };
            results.Add(pprA);

            if (paperB.Count > 0 )
            {
                _fileManager.SaveImage(paperB.ToArray(), name.AppendFileName("b"));
                var pprB = new PreProcessResult() { ImagePath = _fileManager.GetImagePath(name.AppendFileName("b")), IsPaperB = true };
                results.Add(pprB);
            }

            return results;
        }

        /// <summary> 压缩 & 纠偏 & 合并
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        public List<PreProcessResult> PreProcess(List<string> images, byte paperCategory, byte paperType)
        {
            var name = Path.GetFileName(images.First());
            var results = new List<PreProcessResult>();

            if (paperCategory == (byte)PaperCategory.A4)
            {
                // For A4 paper, merge images together directly
                results = ProcessPaperA4(name, images, paperCategory);
            }

            if (paperCategory == (byte)PaperCategory.A3 &&
                paperType == (byte)PaperType.Normal)
            {
                // For A3 paper with non-AB type
                results = ProcessPaperA3WithNonAB(name, images, paperCategory);
            }

            if (paperCategory == (byte)PaperCategory.A3 &&
                paperType == (byte)PaperType.PaperAb)
            {
                // For A3 paper with AB type
                results = ProcessPaperA3WithAB(name, images, paperCategory);
            }

            return results;
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

        public void ScanPaper(string imagePath, PaperMarkedInfo markedInfo, MPictureInfo picture)
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

            try
            {
                if (string.IsNullOrEmpty(markedInfo.StudentCode))
                {
                    using (var scanner = new DefaultRecognition(imagePath, _sheets, false))
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
                else
                {
                    // For A3 with paperB, ignore student code scanning.
                    _sheets.Clear();
                    LoadObjectives();
                    using (var scanner = new DefaultRecognition(imagePath, _sheets, true))
                    {
                        var result = scanner.Start();
                        markedInfo.IsSuccess = true;
                        //markedInfo.StudentId = result.Student.Id;
                        //markedInfo.StudentName = result.Student.Name;
                        //markedInfo.StudentCode = result.Student.Code;

                        picture.StudentId = markedInfo.StudentId;
                        picture.StudentName = markedInfo.StudentName;
                        //picture.GroupId = result.Student.ClassId;

                        picture.SheetAnwers = result.Sheets;
                        markedInfo.Ratios = picture.SheetAnwers.ToWord();

                        if (isNew)
                        {
                            markedInfo.ImagePath = imagePath;
                        }
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
