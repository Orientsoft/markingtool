using System;
using System.IO;
using System.Linq;
using System.Printing;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PrintPaperTemplate
{
    public static class Helper
    {
        private const string Key = "z2zeC9crPgG4uDxXxz2aiB3dbdb3bvN1";
        private const string Iv = "DL+XxX123bs=";

        /// <summary>
        /// 对字符串进行3DES加密
        /// </summary>
        /// <param name="content">需要加密的内容</param>
        /// <returns>3DES加密结果</returns>
        public static string DesEncrypt(string content)
        {
            return DesEncrypt(content, Key, Iv);
        }

        /// <summary>
        /// 对字符串进行3DES加密
        /// </summary>
        /// <param name="content">需要加密的内容</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns>3DES加密结果</returns>
        public static string DesEncrypt(string content, string key, string iv)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var desHash = TripleDES.Create())
                {
                    var data = Encoding.UTF8.GetBytes(content);
                    desHash.Key = Convert.FromBase64String(key);
                    desHash.IV = Convert.FromBase64String(iv);
                    var encryptor = desHash.CreateEncryptor(desHash.Key, desHash.IV);

                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 显示错误提示框
        /// </summary>
        /// <param name="message">错误内容</param>
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "提示信息", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// 转换为长整型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static long ToLong(string str, long def = 0)
        {
            long value;
            if (!long.TryParse(str, out value))
                value = def;
            return value;
        }

        public static int ToInt(string str, int def = 0)
        {
            int value;
            if (!int.TryParse(str, out value))
                value = def;
            return value;
        }

        public static PageMediaSize GetSize(PageMediaSizeName name)
        {
            var size = LocalPrintServer.GetDefaultPrintQueue()

                .GetPrintCapabilities()

                .PageMediaSizeCapability

                .FirstOrDefault(x => x.PageMediaSizeName == name);
            return size ?? new PageMediaSize(PageMediaSizeName.ISOA4);
        }

        public static PageMediaSize A4Size
        {
            get { return GetSize(PageMediaSizeName.ISOA4); }
        }

        public static void SaveVisual(DrawingVisual visual, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                //dpi可以自己设定   // 获取dpi方法：PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice
                var bitmap = new RenderTargetBitmap((int) A4Size.Width, (int) A4Size.Height,
                    96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);
                var encode = new PngBitmapEncoder();
                encode.Frames.Add(BitmapFrame.Create(bitmap));
                encode.Save(fs);
            }
        }
    }
}