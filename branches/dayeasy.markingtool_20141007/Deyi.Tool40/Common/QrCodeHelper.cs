using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Data;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;

namespace Deyi.Tool.Common
{
    /// <summary>
    /// 二维码辅助类
    /// </summary>
    internal class QrCodeHelper:IDisposable
    {
        private readonly string _code;
        private readonly Bitmap _bitmap;

        internal QrCodeHelper(string code)
        {
            _code = code;
        }

        internal QrCodeHelper(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        internal Bitmap Encoder(int width,int height)
        {
            var writer = new MultiFormatWriter();
            var matrix = writer.encode(_code, BarcodeFormat.QR_CODE, width, height);
            return ToBitmap(matrix);
        }

        internal string Decoder()
        {
            var reader = new MultiFormatReader();
            var source = new BitmapLuminanceSource(_bitmap);
            var binaryBitmap = new BinaryBitmap(new HybridBinarizer(source));
            var result = reader.decode(binaryBitmap);
            if (result != null) return result.Text;
            var qrDecoder = new QRCodeDecoder();
            string word = string.Empty;
            while (string.IsNullOrWhiteSpace(word))
            {
                var content = qrDecoder.decode(new QRCodeBitmapImage(_bitmap));
                word = Helper.DESDecrypt(content);
                if (!string.IsNullOrWhiteSpace(word)) word = content;
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
