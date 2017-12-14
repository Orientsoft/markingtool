using System.IO;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity;
using System;
using System.Drawing;

namespace DayEasy.MarkingTool.BLL.Scanners
{
    public class PaperBasicInfoScanner
    {
        public StudentInfo Student { get; private set; }

        /// <summary>
        /// 图片扫描程序
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public JsonResultBase Scan(Image img)
        {
            var wd = (int)Math.Ceiling(img.Width * DeyiKeys.QrcodeWidth);
            using (var qrcodeImage = (Bitmap)ImageHelper.MakeImage(img, img.Width - wd, 0, wd, img.Height))
            {
                Bitmap currentImage = null;
                foreach (var threshold in DeyiKeys.QrcodeThresholds)
                {
                    currentImage = ImageHelper.BinarizeImage(qrcodeImage.Clone() as Bitmap, threshold);
                    ConvertBmp(currentImage);
                    if (Student != null)
                        break;
                }
                if (currentImage != null)
                {
                    currentImage.Dispose();
                }
#if DEBUG
                if (Student == null)
                {
                    var path = Path.Combine(DeyiKeys.MarkingPath, DeyiKeys.DebugName);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    var name = Helper.Guid32.Substring(5, 6);
                    var itemName = string.Format("error_{0}.png", name);
                    path = Path.Combine(path, itemName);
                    qrcodeImage.Save(path);
                }
#endif
                return new JsonResultBase(true, string.Empty);
            }
        }

        public void ConvertBmp(Bitmap studentNo)
        {
            var helper = new QrCodeHelper(studentNo);
            var word = helper.Decoder();
            if (string.IsNullOrWhiteSpace(word) || string.IsNullOrWhiteSpace(word = Helper.DesDecrypt(word)))
                return;
            var words = word.Split(',');
            if (words.Length == 3)
            {
                var id = Helper.StrToLong(words[1]);
                if (id > 0)
                {
                    Student = new StudentInfo
                    {
                        Id = id,
                        Name = words[0],
                        ClassId = words[2]
                    };
                }
            }
        }
    }
}
