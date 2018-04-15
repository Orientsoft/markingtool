using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace DayEasy.MarkingTool.BLL.Config
{
    [Serializable]
    [FileName("scanner.config")]
    [XmlRoot("root")]
    public class ScannerConfig : ConfigBase
    {
        [XmlAttribute("type")]
        public int Type { get; set; }

        /// <summary> 线条扫描 - 黑点比例 </summary>
        [XmlElement("blackScale")]
        public double BlackScale { get; set; }

        /// <summary> 试卷宽度 </summary>
        [XmlElement("paperWidth")]
        public int PaperWidth { get; set; }

        /// <summary> 压缩之后行高 </summary>
        [XmlElement("lineHeight")]
        public int LineHeight { get; set; }

        [XmlElement("qrcodeWidth")]
        public double QrcodeWidth { get; set; }

        /// <summary> 基础灰阶阀值 </summary>
        [XmlElement("threshold")]
        public double BasicThreshold { get; set; }

        /// <summary> 线条灰阶阀值 </summary>
        [XmlElement("lineThreshold")]
        public int LineTreshold { get; set; }

        /// <summary> 答题卡容差 </summary>
        [XmlElement("sheetTolerance")]
        public double SheetTolerance { get; set; }

        [XmlElement("columnCount")]
        public int ColumnCount { get; set; }

        [XmlElement("smearWidth")]
        public int SmearWidth { get; set; }

        [XmlElement("smearHeight")]
        public int SmearHeight { get; set; }

        [XmlElement("areaX")]
        public int AreaX { get; set; }

        [XmlElement("areaY")]
        public int AreaY { get; set; }

        /// <summary> 答题卡类型 </summary>
        [XmlElement("sheetType")]
        public int SheetType { get; set; }

        /// <summary> 识别类型 </summary>
        [XmlElement("recognitionType")]
        public int RecognitionType { get; set; }

        /// <summary> 同时执行的任务数 </summary>
        [XmlElement("asyncCount")]
        public int AsyncCount { get; set; } = 5;

        public ScannerConfig()
        {
            SheetType = 1;
            RecognitionType = 0;
            BasicThreshold = -1;
            LineTreshold = 180;
            ColumnCount = 24;
            AreaX = 40;
            AreaY = 20;
        }
    }

    public enum RecognitionType : byte { Rank = 0, Area = 1 }

    public enum ScannerType
    {
        [Description("默认")]
        Default = 0,
        [Description("浅色")]
        Light = 1,
        [Description("横线")]
        Line = 2,
        [Description("自定义")]
        Custom = 9
    }
}
