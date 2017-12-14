using System.ComponentModel;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.Open.Model.Paper;
using System;

namespace DayEasy.MarkingTool.BLL.Entity.Paper
{
    public class PrintUsage : PrintUsageInfo, INotifyPropertyChanged
    {
        public DateTime StartTimeDate
        {
            get { return Helper.ToDateTime(StartTime); }
        }

        public string StartTimeCn
        {
            get { return StartTimeDate.ToString("yyyy-MM-dd HH:mm"); }
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        public PaperKind Kind { get; set; }

        public string KindDesc
        {
            get { return Kind.GetText(); }
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
