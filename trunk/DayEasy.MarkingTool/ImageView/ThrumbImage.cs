using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace DayEasy.MarkingTool.ImageView
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

        public static readonly DependencyProperty ThrumbImageSourceProperty =
            DependencyProperty.Register("ThrumbImageSource", typeof(string), typeof(ThrumbImage),
                new FrameworkPropertyMetadata(OnThrumbImageSourcePropertyChanged));

        private static void OnThrumbImageSourcePropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is Image) || !(args.NewValue is string))
            {
                return;
            }

            var image = sender as Image;
            _imageSourceHandler = SetImageSource;
            image.Dispatcher.BeginInvoke(_imageSourceHandler, DispatcherPriority.ApplicationIdle,
                new object[] { image, args.NewValue.ToString() });
        }

        private static void SetImageSource(Image image, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            {
                return;
            }

            var sourceImage = System.Drawing.Image.FromFile(fileName);
            int width, height;
            ResizeImage(sourceImage, image, out width, out height);
            var sourceBitmap = new System.Drawing.Bitmap(sourceImage, width, height);
            var hBitmap = sourceBitmap.GetHbitmap();
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();
            var bmp = new WriteableBitmap(bitmapSource);
            sourceImage.Dispose();
            sourceBitmap.Dispose();
            image.Source = bmp;
        }

        private static void ResizeImage(System.Drawing.Image sourceImage, Image image, out int width, out int height)
        {
            height = 0;
            width = 0;
            if (sourceImage.Height <= 0)
                return;
            var aspect = sourceImage.Width / (float)sourceImage.Height;
            height = 100;
            width = (int)(height * aspect);

            if (!double.IsNaN(image.Height))
            {
                height = (int)image.Height;
                width = (int)(image.Height * aspect);
            }
            else if (!double.IsNaN(image.Width))
            {
                height = (int)(image.Width / aspect);
                width = (int)image.Width;
            }
        }
    }
}
