using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Enum;

namespace DayEasy.MarkingTool.BLL.Entity.Paper
{
    /// <summary> 扫描结果 </summary>
    [XmlType(TypeName = "PaperMarkedInfo")]
    [Serializable]
    public class PaperMarkedInfo : INotifyPropertyChanged
    {
        public PaperMarkedInfo()
            : this(string.Empty) { }
        public PaperMarkedInfo(string path)
        {
            IsSuccess = false;
            PageCount = 1;
            MarkedId = Helper.Guid32;
            ImagePath = path;
        }

        /// <summary> 阅卷ID </summary>
        [XmlAttribute("h")]
        public string MarkedId { get; set; }

        /// <summary> 扫描序号 </summary>
        [XmlAttribute("a")]
        public int Index { get; set; }

        [XmlAttribute("b")]
        public int SectionType { get; set; }

        [XmlIgnore]
        public string SectionTypeCn
        {
            get
            {
                if (SectionType <= 0)
                    return "常规";
                return ((ScannerType)SectionType).GetText();
            }
        }

        /// <summary> 学生姓名 </summary>
        [XmlAttribute("c")]
        public string StudentName { get; set; }

        
        [XmlIgnore]
        public string StudentColor
        {
            get { return StudentId > 0 ? "Black" : "Red"; }
        }

        /// <summary> 学生ID </summary>
        [XmlAttribute("d")]
        public long StudentId { get; set; }

        [XmlAttribute("m")]
        public string StudentCode { get; set; }

        /// <summary> 图片路径 </summary>
        [XmlAttribute("e")]
        public string ImagePath { get; set; }

        /// <summary> 试卷ID </summary>
        [XmlAttribute("f")]
        public string PaperId { get; set; }

        /// <summary> 试卷编号 </summary>
        [XmlAttribute("g")]
        public string PaperNum { get; set; }

        /// <summary> 扫描图片名称 </summary>
        [XmlAttribute("i")]
        public string PaperName
        {
            get
            {
                return (string.IsNullOrWhiteSpace(ImagePath)
                    ? string.Empty
                    : Path.GetFileNameWithoutExtension(ImagePath));
            }
        }

        /// <summary> 扫描页数 </summary>
        [XmlAttribute("j")]
        public int PageCount { get; set; }

        private string _ratios;

        [XmlAttribute("n")]
        public string Ratios
        {
            get { return _ratios; }
            set
            {
                _ratios = value;
                OnPropertyChanged("Ratios");
                OnPropertyChanged("RatiosColor");
            }
        }

        private string _ratiosColor = "Black";

        [XmlAttribute("o")]
        public string RatiosColor
        {
            //            get { return (Ratios.StartsWith(",") || Ratios.EndsWith(",") || Ratios.Contains(",,")) ? "Red" : "Black"; }
            get { return _ratiosColor; }
            set
            {
                _ratiosColor = value;
                OnPropertyChanged("RatiosColor");
            }
        }

        [XmlIgnore]
        private bool _isSuccess;

        /// <summary> 是否识别成功 </summary>
        [XmlAttribute("k")]
        public bool IsSuccess
        {
            get { return _isSuccess; }
            set
            {
                _isSuccess = value;
                OnPropertyChanged("StudentName");
                OnPropertyChanged("StudentColor");
                OnPropertyChanged("Desc");
                OnPropertyChanged("IsSuccess");
                OnPropertyChanged("MarkedStatus");
                OnPropertyChanged("MarkedResultColor");
            }
        }

        [XmlIgnore]
        private string _desc;

        /// <summary> 描述 </summary>
        [XmlAttribute("l")]
        public string Desc
        {
            get { return _desc; }
            set
            {
                _desc = value;
                OnPropertyChanged("Result");
                OnPropertyChanged("MarkedStatus");
            }
        }

        [XmlElement("p")]
        public string PaperTitle { get; set; }

        /// <summary> 试卷图片是否被上传 </summary>
        public bool IsUpload { get; set; }

        /// <summary> 上传状态 </summary>
        public string MarkedStatus
        {
            get
            {
                if (!IsMarkedSuccess.HasValue)
                    return IsSuccess ? "可上传" : Desc;
                if (IsMarkedSuccess.Value)
                    return "上传成功";
                return MarkedMessage;
            }
        }

        /// <summary> 上传状态颜色 </summary>
        public string MarkedResultColor
        {
            get
            {
                if (!IsMarkedSuccess.HasValue)
                    return IsSuccess ? "Black" : "Red";
                return IsMarkedSuccess.Value ? "Green" : "Red";
            }
        }

        private bool? _isMarkedSuccess;

        /// <summary> 是否上传成功 </summary>
        public bool? IsMarkedSuccess
        {
            get { return _isMarkedSuccess; }
            set
            {
                _isMarkedSuccess = value;
                OnPropertyChanged("MarkedStatus");
                OnPropertyChanged("MarkedResultColor");
            }
        }

        private string _markedMessage;

        /// <summary> 上传返回信息 </summary>
        public string MarkedMessage
        {
            get { return _markedMessage; }
            set
            {
                _markedMessage = value;
                OnPropertyChanged("MarkedStatus");
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //        public ScannerBuilder Builder { get; set; }
    }
}
