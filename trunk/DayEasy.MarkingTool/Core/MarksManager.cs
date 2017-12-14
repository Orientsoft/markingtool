using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DayEasy.MarkingTool.BLL.Enum;
using Image = System.Windows.Controls.Image;
using Size = System.Drawing.Size;

namespace DayEasy.MarkingTool.Core
{
    public class MarksManager
    {
        private const string RelativeFormat = "/Images/Marking/{0}";
        private const string AbsoluteFormat = "pack://application:,,,/Images/Marking/{0}";

        private readonly int[] _imageTypes =
        {
            (int) SymbolType.Right, (int) SymbolType.Wrong, (int) SymbolType.HalfRight,
            (int) SymbolType.Emotion,(int)SymbolType.Comment
        };
        private readonly int _symbolType;
        private readonly string _word;
        private readonly int _maxWidth;

        public bool IsImage
        {
            get { return _imageTypes.Contains(_symbolType); }
        }

        public MarksManager(int symbolType, string word, int maxWidth = 0)
        {
            _symbolType = symbolType;
            switch (_symbolType)
            {
                case (int)SymbolType.Right:
                    _word = "full.png";
                    break;
                case (int)SymbolType.Wrong:
                    _word = "error.png";
                    break;
                case (int)SymbolType.HalfRight:
                    _word = "semi.png";
                    break;
                default:
                    _word = word;
                    break;
            }
            _maxWidth = maxWidth;
        }

        public Size GetSize()
        {
            switch (_word)
            {
                case "rk-1.png":
                    return new Size(147, 33);
                case "rk-2.png":
                    return new Size(182, 32);
                case "rk-3.png":
                    return new Size(200, 32);
                case "rk-4.png":
                    return new Size(148, 32);
                case "rk-5.png":
                    return new Size(33, 32);
                case "rk-6.png":
                    return new Size(33, 32);
                case "cry.png":
                    return new Size(32, 32);
                case "doubt.png":
                    return new Size(32, 32);
                case "praise.png":
                    return new Size(30, 32);
                case "smile.png":
                    return new Size(32, 32);
                case "line.png":
                    return new Size(160, 2);
                case "oval.png":
                    return new Size(181, 46);
                case "wavy.png":
                    return new Size(163, 6);

            }
            return new Size(48, 48);
        }

        public int MaxWidth { get { return _maxWidth; } }

        public UIElement GetControl()
        {
            if (_imageTypes.Contains(_symbolType))
            {
                var size = GetSize();
                var image = new Image
                {
                    Source = GetBitmapImage(false),
                    Width = size.Width,
                    Height = size.Height
                };
                return image;
            }
            if (_maxWidth > 0)
            {
                var label = new Label
                {
                    MaxWidth = _maxWidth,
                    Content = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = _word
                    }
                };
                return label;
            }
            return new Label
            {
                Content = _word
            };
        }

        public BitmapImage GetBitmapImage(bool isAbsolute = true)
        {
            string path;
            if (isAbsolute)
            {
                path = string.Format(AbsoluteFormat, _word);
                return new BitmapImage(new Uri(path, UriKind.Absolute));
            }
            path = string.Format(RelativeFormat, _word);
            return new BitmapImage(new Uri(path, UriKind.Relative));
        }
    }
}
