using System.Drawing;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Enum;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DayEasy.Open.Model.Enum;

namespace DayEasy.MarkingTool.Core
{
    public class CursorManager
    {
        private readonly MarkingOperate _markingOperate = MarkingOperate.Pointer;
        private readonly string _curName;
        private const string CurFormat = "DayEasy.MarkingTool.Images.cur.{0}.cur";
        private const string PngFormat = "/Images/{0}.png";
        private const string AbsoluteFormat = "pack://application:,,,/Images/{0}.png";
        private readonly int _type;

        public CursorManager(MarkingOperate operate, int type = 0)
        {
            _markingOperate = operate;
            _type = type > 100 ? type : 0;
            _curName = (_markingOperate != MarkingOperate.Emotion
                ? _markingOperate.ToString()
                : string.Format("emotion_{0}", type));
        }

        public  CursorManager(MarkingSymbolType symbolType):
            this(ConvertOperate(symbolType))
        {
        }

        private static MarkingOperate ConvertOperate(MarkingSymbolType symbolType)
        {
            MarkingOperate operate;
            switch (symbolType)
            {
                case MarkingSymbolType.Right:
                    operate = MarkingOperate.Hook;
                    break;
                case MarkingSymbolType.HalfRight:
                    operate = MarkingOperate.HalfHook;
                    break;
                default:
                    operate = MarkingOperate.Fork;
                    break;
            }
            return operate;
        }

        public CursorManager(int emotionType) :
            this(emotionType == 0 ? MarkingOperate.Comment : MarkingOperate.Emotion, emotionType)
        {
        }

        public int EmotionType
        {
            get { return _type; }
        }

        public Cursor GetCursor()
        {
            if (_markingOperate == MarkingOperate.Pointer)
                return Cursors.Arrow;
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            var curPath = string.Format(CurFormat, _curName);
            Stream myStream = myAssembly.GetManifestResourceStream(curPath);
            return myStream == null ? Cursors.Arrow : new Cursor(myStream);
        }

        public BitmapImage GetBitmapImage()
        {
            var path = string.Format(AbsoluteFormat, _curName);
            return new BitmapImage(new Uri(path, UriKind.Absolute));
        }

        public Bitmap GetBitmap()
        {
            var path = string.Format(PngFormat, _curName);
            path = Path.Combine(DeyiKeys.CurrentDir, "config", path.TrimStart('/'));
            return new Bitmap(path);
        }

        public MarkingSymbolType GetMarkingSymbolType()
        {
            switch (_markingOperate)
            {
                case MarkingOperate.Hook:
                    return MarkingSymbolType.Right;
                case MarkingOperate.Fork:
                    return MarkingSymbolType.Wrong;
                case MarkingOperate.HalfHook:
                    return MarkingSymbolType.HalfRight;
                default:
                    return MarkingSymbolType.Right;
            }
        }

        public Size GetImageSize()
        {
            if (_markingOperate != MarkingOperate.Emotion)
                return new Size(40, 40);
            switch (_type)
            {
                case 101:
                    return new Size(25, 27);
                case 102:
                    return new Size(26, 26);
                case 103:
                    return new Size(127, 24);
                case 104:
                    return new Size(127,24);
                case 105:
                    return new Size(154, 25);
                case 106:
                    return new Size(162, 24);
            }
            return new Size(40, 40);
        }

        public Point OffsetPoint()
        {
            if (_markingOperate == MarkingOperate.HalfHook)
                return new Point(3, 3);
            if (_markingOperate == MarkingOperate.Emotion)
            {
                switch (_type)
                {
                    case 101:
                        return new Point(0, 33);
                    case 102:
                        return new Point(2, 32);
                    case 103:
                        return new Point(50, 20);
                    case 104:
                        return new Point(45, 20);
                    case 105:
                        return new Point(45, 20);
                    case 106:
                        return new Point(40, 20);
                }
            }
            return new Point(20, 20);
        }
    }
}
