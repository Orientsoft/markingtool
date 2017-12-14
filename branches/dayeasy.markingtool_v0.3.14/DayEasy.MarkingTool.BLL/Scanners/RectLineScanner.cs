using System;
using System.Drawing;

namespace DayEasy.MarkingTool.BLL.Scanners
{
    /// <summary>
    /// 矩阵式线条识别
    /// </summary>
    public class RectLineScanner : IDisposable
    {
        private Image _image;
        //矩阵高度
        private int _rectHeight;
        //图片高度
        private int _height;
        private int _width;

        public RectLineScanner(Bitmap image, int rectHeight = 5)
        {
            _image = image;
            _rectHeight = rectHeight;
            _width = image.Width;
            _height = image.Height;
        }

        public JsonResults<int> FindLines()
        {
            return new JsonResults<int>(string.Empty);
        }

        public void Dispose()
        {
            if (_image != null)
                _image.Dispose();
        }
    }
}
