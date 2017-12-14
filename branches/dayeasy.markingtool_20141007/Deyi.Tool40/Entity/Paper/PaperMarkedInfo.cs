
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Deyi.Tool.Entity.Paper
{
    [XmlType(TypeName = "PaperMarkedInfo")]
    [Serializable]
    public class PaperMarkedInfo : INotifyPropertyChanged
    {
        [XmlIgnore]
        private string sutdentName;
        [XmlIgnore]
        private string sutdentNo;
        [XmlIgnore]
        private string idNo;
        [XmlIgnore]
        private string imagePath;
        [XmlIgnore]
        private string bacthCode;
        [XmlIgnore]
        private int erorrCount;
        [XmlIgnore]
        private int totalCount;
        [XmlIgnore]
        private int totalScore;
        [XmlIgnore]
        private bool isSuccess;
        [XmlIgnore]
        private string desc;
        [XmlIgnore]
        private string sutdentAnswer;
        [XmlIgnore]
        private bool isMarked;

        [XmlAttribute("mid")]
        public static string MarkedID = Guid.NewGuid().ToString("N");

        [XmlAttribute("name")]
        public string SutdentName { get { return sutdentName; } set { sutdentName = value; } }

        [XmlAttribute("student")]
        public string SutdentNo { get { return sutdentNo; } set { sutdentNo = value; } }

        [XmlAttribute("id")]
        public string IDNo { get { return idNo; } set { idNo = value; } }

        [XmlAttribute("paper")]
        public string PaperName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImagePath))
                {
                    return string.Empty;
                }
                return Path.GetFileNameWithoutExtension(imagePath);
            }
        }

        [XmlAttribute("path")]
        public string ImagePath
        {
            get{ return imagePath;}
            set { 
                imagePath = value;
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
            get { return bacthCode; }
            set { bacthCode = value; }
        }

        [XmlAttribute("erorr")]
        public int ErorrCount
        {
            get { return erorrCount; }
            set
            {
                erorrCount = value;
                OnPropertyChanged("ErorrCount");
                OnPropertyChanged("Ratios");
            }
        }

        [XmlAttribute("total")]
        public int TotalCount
        {
            get { return totalCount; }
            set
            {
                totalCount = value;
                OnPropertyChanged("TotalCount");
                OnPropertyChanged("Ratios");
            }
        }

        [XmlIgnore]
        public string Ratios { get { return string.Format("{0}/{1}", ErorrCount, TotalCount); } }

        [XmlAttribute("score")]
        public int TotalScore
        {
            get { return totalScore; }
            set
            {
                totalScore = value;
                OnPropertyChanged("TotalScore");
            }
        }

        [XmlAttribute("satus")]
        public bool IsSuccess
        {
            get { return isSuccess; }
            set
            {
                isSuccess = value;
                OnPropertyChanged("IsSuccess");
                OnPropertyChanged("Result");
                OnPropertyChanged("ResultColor");
            }
        }

        [XmlAttribute("desc")]
        public string Desc { get { return desc; } set { desc = value; OnPropertyChanged("Desc"); } }

        [XmlAttribute("studentanswer")]
        public string SutdentAnswer
        {
            get { return sutdentAnswer; }
            set
            {
                sutdentAnswer = value;
                OnPropertyChanged("SutdentAnswer");
            }
        }

        [XmlIgnore]
        public Brush ResultColor
        {
            get
            {
                if (IsSuccess)
                {
                    return Brushes.Green;
                }
                else
                {
                    return Brushes.Red;
                }
            }
        }
        [XmlIgnore]
        public string Result
        {
            get
            {
                if (IsSuccess)
                {
                    return "成功";
                }
                else
                {
                    return "失败";
                }
            }
        }

        [XmlAttribute("ismark")]
        public bool IsMarked
        {
            get { return isMarked; }
            set
            {
                isMarked = value;
                OnPropertyChanged("IsMarked");
                OnPropertyChanged("MarkedSatusDesc");
            }
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
                else
                {
                    return "未阅";
                }
            }
        }

        //public string StudentNoWithBachCode
        //{
        //    get
        //    {
        //        return string.Format("{0}|{1}", IDNo, BacthCode);
        //    }
        //}

        public string PaperTitle { get; set; }

        public PaperMarkedInfo()
        {
            IsSuccess = true;
            //  SortFiled = DateTime.Now;
            // MarkedID = Guid.NewGuid().ToString("N");
        }

        //  public DateTime SortFiled { get; set; }

        public Guid MarkedResultID { get; set; }

        /// <summary>
        /// 试卷是否被上传 
        /// </summary>
        public bool IsUpload { get; set; }

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
