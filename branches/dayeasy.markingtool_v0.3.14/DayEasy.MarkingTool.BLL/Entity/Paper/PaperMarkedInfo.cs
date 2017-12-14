using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace DayEasy.MarkingTool.BLL.Entity.Paper
{
    [XmlType(TypeName = "PaperMarkedInfo")]
    [Serializable]
    public class PaperMarkedInfo : INotifyPropertyChanged
    {
        public PaperMarkedInfo()
        {
            IsSuccess = true;
        }

        [XmlIgnore]
        private string _studentName;
        [XmlIgnore]
        private long _studentId;
        [XmlIgnore]
        private string _imagePath;
        [XmlIgnore]
        private string _bacthCode;
        [XmlIgnore]
        private int _erorrCount;
        [XmlIgnore]
        private int _totalCount;
        [XmlIgnore]
        private decimal _totalScore;
        [XmlIgnore]
        private bool _isSuccess;
        [XmlIgnore]
        private string _desc;
        [XmlIgnore]
        private string _sutdentAnswer;
        [XmlIgnore]
        private bool _isMarked;

        [XmlAttribute("index")]
        public int Index { get; set; }

        [XmlAttribute("mid")]
        public static string MarkedId = Guid.NewGuid().ToString("N");

        [XmlAttribute("name")]
        public string StudentName { get { return _studentName; } set { _studentName = value; } }

        [XmlAttribute("id")]
        public long StudentId { get { return _studentId; } set { _studentId = value; } }

        [XmlAttribute("paper")]
        public string PaperName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImagePath))
                {
                    return string.Empty;
                }
                return Path.GetFileNameWithoutExtension(_imagePath);
            }
        }

        [XmlAttribute("path")]
        public string ImagePath
        {
            get{ return _imagePath;}
            set { 
                _imagePath = value;
            }
        }

        //[XmlIgnore]
        //public string PaperNameWithBachCode
        //{
        //    get { return string.Format("{0}|{1}", PaperName, BacthCode); }
        //}

        [XmlAttribute("batch")]
        public string BacthCode
        {
            get { return _bacthCode; }
            set { _bacthCode = value; }
        }

        [XmlAttribute("erorr")]
        public int ErorrCount
        {
            get { return _erorrCount; }
            set
            {
                _erorrCount = value;
                OnPropertyChanged("ErorrCount");
                OnPropertyChanged("Ratios");
            }
        }

        [XmlAttribute("pageCount")]
        public int PageCount { get; set; }

        [XmlAttribute("total")]
        public int TotalCount
        {
            get { return _totalCount; }
            set
            {
                _totalCount = value;
                OnPropertyChanged("TotalCount");
                OnPropertyChanged("Ratios");
            }
        }

        [XmlIgnore]
        public string Ratios { get { return string.Format("{0}/{1}", ErorrCount, TotalCount); } }

        [XmlAttribute("score")]
        public decimal TotalScore
        {
            get { return _totalScore; }
            set
            {
                _totalScore = value;
                OnPropertyChanged("TotalScore");
            }
        }

        [XmlAttribute("status")]
        public bool IsSuccess
        {
            get { return _isSuccess; }
            set
            {
                _isSuccess = value;
                OnPropertyChanged("IsSuccess");
                OnPropertyChanged("Result");
                OnPropertyChanged("ResultColor");
            }
        }

        [XmlAttribute("desc")]
        public string Desc
        {
            get { return _desc; }
            set
            {
                _desc = value;
                OnPropertyChanged("Result");
            }
        }

        [XmlAttribute("studentanswer")]
        public string SutdentAnswer
        {
            get { return _sutdentAnswer; }
            set
            {
                _sutdentAnswer = value;
                OnPropertyChanged("SutdentAnswer");
            }
        }

        [XmlIgnore]
        public string ResultColor
        {
            get { return IsSuccess ? "Green" : "Red"; }
        }

        [XmlIgnore]
        public string Result
        {
            get { return IsSuccess ? "识别成功" : Desc; }
        }

        [XmlAttribute("ismark")]
        public bool IsMarked
        {
            get { return _isMarked; }
            set
            {
                _isMarked = value;
                OnPropertyChanged("IsMarked");
                OnPropertyChanged("MarkedColor");
                OnPropertyChanged("MarkedSatusDesc");
            }
        }

        public string MarkedColor
        {
            get { return IsMarked ? "Green" : "Gray"; }
        }

        [XmlIgnore]
        public string MarkedSatusDesc
        {
            get
            {
                if (IsMarked)
                {
                    return "已阅";
                }
                return "未阅";
            }
        }

        public string PaperTitle { get; set; }

        public string MarkedResultId { get; set; }

        /// <summary>
        /// 试卷图片是否被上传 
        /// </summary>
        public bool IsUpload { get; set; }

        /// <summary>
        /// 上传状态
        /// </summary>
        public string MarkedStatus
        {
            get
            {
                return !IsMarkedSuccess.HasValue
                    ? "未上传"
                    : (IsMarkedSuccess.Value ? "上传成功" : MarkedMessage);
            }
        }

        public string MarkedResultColor
        {
            get
            {
                if (IsMarkedSuccess.HasValue)
                {
                    return IsMarkedSuccess.Value ? "Green" : "Red";
                }
                return "Gray";
            }
        }

        public string CanLook
        {
            get { return IsSuccess ? "Hidden" : "Visible"; }
        }

        private bool? _isMarkedSuccess;

        /// <summary>
        /// 是否上传成功
        /// </summary>
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

        /// <summary>
        /// 上传返回信息
        /// </summary>
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
    }
}
