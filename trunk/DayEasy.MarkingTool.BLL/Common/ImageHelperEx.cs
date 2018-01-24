using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace DayEasy.MarkingTool.BLL.Common
{
    public class ImageHelperEx
    {
        //public static Bitmap RotateImage(Bitmap bmp, float angle)
        //{
        //    var filter = new RotateBilinear(angle, true);
        //    return filter.Apply(UnmanagedImage.FromManagedImage(bmp)).ToManagedImage();
        //}

        public static Bitmap RotateImage(Bitmap bmp, float angle)
        {
            if (Math.Abs(angle) < 0.001)
            {
                return bmp;
            }

            if (angle > 1)
            {
                angle = angle / 10;
            }

            Bitmap rotatedImage = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppRgb);
            rotatedImage.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                // Set the rotation point to the center in the matrix
                g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);

                //g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
                // Rotate
                g.RotateTransform((float)(360 + angle));
                // Restore rotation point in the matrix
                //g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
                // Draw the image on the bitmap
                g.DrawImage(bmp, 0, 0);
            }

            return rotatedImage;
        }
    }
}
