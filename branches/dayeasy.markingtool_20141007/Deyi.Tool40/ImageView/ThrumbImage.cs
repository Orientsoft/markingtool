using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Draw = System.Drawing;

namespace Deyi.Tool
{
    public delegate void DeleImageSource(Image image, string fileName);

    public class ThrumbImage : Image
    {
        private static DeleImageSource _imageSourceHandler;

        public string ThrumbImageSource
        {
            get { return GetValue(ThrumbImageSourceProperty).ToString(); }
            set { SetValue(ThrumbImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ThrumbImageSourceProperty = DependencyProperty.Register("ThrumbImageSource",
            typeof(string), typeof(ThrumbImage), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThrumbImageSourcePropertyChanged)));

        private static void OnThrumbImageSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is Image) || !(args.NewValue is string))
            {
                return;
            }

            var image = sender as Image;
            _imageSourceHandler = new DeleImageSource(SetImageSource);
            image.Dispatcher.BeginInvoke(_imageSourceHandler, DispatcherPriority.ApplicationIdle, new object[] { image, args.NewValue.ToString() });
        }

        private static void SetImageSource(Image image, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            {
                return;
            }

            var sourceImage = Draw.Image.FromFile(fileName);
            int width = 0, height = 0;
            ResizeImage(sourceImage, image, out width, out height);
            var sourceBitmap = new Draw.Bitmap(sourceImage, width, height);
            var hBitmap = sourceBitmap.GetHbitmap();
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();
            var bmp = new WriteableBitmap(bitmapSource);
            sourceImage.Dispose();
            sourceBitmap.Dispose();
            image.Source = bmp;
        }

        private static void ResizeImage(Draw.Image sourceImage, Image image, out int width, out int height)
        {
            var aspect = (float)sourceImage.Width / (float)sourceImage.Height;
            height = 100;
            width = (int)(height * aspect);

            if (image.Height != double.NaN)
            {
                height = (int)image.Height;
                width = (int)(image.Height * aspect);
            }
            else if (image.Width != double.NaN)
            {
                height = (int)(image.Width / aspect);
                width = (int)image.Width;
            }
        }
    }
}
