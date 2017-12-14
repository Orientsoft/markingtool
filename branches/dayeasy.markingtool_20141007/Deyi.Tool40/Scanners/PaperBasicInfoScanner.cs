using System.IO;
using Deyi.Tool.Common;
using Deyi.Tool.Step;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Data;
using ZXing;
using ZXing.Common;

namespace Deyi.Tool.Scanners
{
    public class PaperBasicInfoScanner : IScanner
    {
        public string BatchNo { get; private set; }

        public string StudentNo { get; private set; }

        /// <summary>
        /// 图片扫描程序
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public StepResult Scan(Image img)
        {
            //img.Save("img.png");
            var batchImg = ImageHelper.MakeImage(img, 0, 0, DeyiKeys.QrcodeWidth, img.Height);
            //batchImg.Save("bacth01.png");

            var studentNo = ImageHelper.MakeImage(img, img.Width - DeyiKeys.QrcodeWidth, 0, DeyiKeys.QrcodeWidth,
                img.Height);
            //studentNo.Save("student01.png");
            IBarcodeReader reader = new BarcodeReader();
            var opts = new DecodingOptions
            {
                CharacterSet = "UTF-8",
                PossibleFormats = new[] {BarcodeFormat.QR_CODE},
                TryHarder = true
            };
            reader.Options = opts;
            using (batchImg)
            {
                if (DeyiKeys.WriteFile)
                    batchImg.Save(Path.Combine(DeyiKeys.PicturePath, "batchImg.png"));
                var result = reader.Decode(batchImg as Bitmap);
                if (result != null)
                    BatchNo = result.Text;
                else
                {
                    var qrDecoder = new QRCodeDecoder();
                    var bitmap = new QRCodeBitmapImage((Bitmap)batchImg);
                    while (string.IsNullOrWhiteSpace(BatchNo))
                    {
                        try
                        {
                            var content = qrDecoder.decode(bitmap, Encoding.UTF8);
                            if (Regex.IsMatch(content, "^[a-z0-9]{32}$"))
                                BatchNo = content;
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }
            using (studentNo)
            {
                if (DeyiKeys.WriteFile)
                    studentNo.Save(Path.Combine(DeyiKeys.PicturePath, "student.png"));
                var result = reader.Decode(studentNo as Bitmap);
                if (result != null)
                    StudentNo = Helper.DESDecrypt(result.Text);
                else
                {
                    var qrDecoder = new QRCodeDecoder();
                    var bitmap = new QRCodeBitmapImage((Bitmap) studentNo);
                    while (string.IsNullOrWhiteSpace(StudentNo))
                    {
                        try
                        {
                            var content = qrDecoder.decode(bitmap, Encoding.UTF8);
                            StudentNo = Helper.DESDecrypt(content);
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }
            return StepResult.Success;
        }

    }
}
