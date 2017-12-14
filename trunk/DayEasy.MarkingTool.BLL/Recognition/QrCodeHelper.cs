using System;
using System.Drawing;
using ZXing;
using ZXing.Common;
using BitMatrix = ZXing.Common.BitMatrix;

namespace DayEasy.MarkingTool.BLL.Recognition
{
    /// <summary> 二维码辅助类 </summary>
    public class QrCodeHelper : IDisposable
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
            return result != null ? result.Text : null;
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
