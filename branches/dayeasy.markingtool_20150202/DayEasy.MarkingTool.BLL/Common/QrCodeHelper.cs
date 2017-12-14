using System;
using System.Drawing;
using System.Text;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Data;
using ThoughtWorks.QRCode.Codec.Util;
using ZXing;
using ZXing.Common;
using BitMatrix = ZXing.Common.BitMatrix;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary>
    /// 二维码辅助类
    /// </summary>
    public class QrCodeHelper:IDisposable
    {
        private readonly string _code;
        private readonly Bitmap _bitmap;

        public QrCodeHelper(string code)
        {
            _code = code;
        }

        public QrCodeHelper(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public Bitmap Encoder(int width, int height)
        {
            var writer = new MultiFormatWriter();
            var matrix = writer.encode(_code, BarcodeFormat.QR_CODE, width, height);
            return ToBitmap(matrix);
        }

        public string Decoder()
        {
            var reader = new MultiFormatReader();
            var source = new BitmapLuminanceSource(_bitmap);
            var binaryBitmap = new BinaryBitmap(new HybridBinarizer(source));
            var result = reader.decode(binaryBitmap);
            if (result != null) return result.Text;
            //QRCode识别

            var qrDecoder = new QRCodeDecoder();
            string word = string.Empty;
            var img = new QRCodeBitmapImage(_bitmap);
            while (string.IsNullOrWhiteSpace(word))
            {
                try
                {
                    var content = qrDecoder.decode(img, Encoding.UTF8);
                    if (string.IsNullOrWhiteSpace(content) || content.IndexOf("�", StringComparison.Ordinal) >= 0)
                        continue;
                    if (QRCodeUtility.IsUniCode(content)) continue;
                    word = content;
                    break;
                }
                catch
                {
                    break;
                }
            }
            return word;
        }

        public static Bitmap ToBitmap(BitMatrix matrix)
        {
            int width = matrix.Width;
            int height = matrix.Height;
            var bmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmap.SetPixel(x, y,
                        matrix[x, y]
                            ? ColorTranslator.FromHtml("0xFF000000")
                            : ColorTranslator.FromHtml("0xFFFFFFFF"));
                }
            }
            return bmap;
        }

        public void Dispose()
        {
            if (_bitmap != null)
                _bitmap.Dispose();
        }
    }
}
