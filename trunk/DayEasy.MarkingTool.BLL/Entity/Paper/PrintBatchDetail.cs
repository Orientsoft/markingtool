using System.ComponentModel;
using DayEasy.MarkingTool.BLL.Annotations;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.Models.Open.Work;

namespace DayEasy.MarkingTool.BLL.Entity.Paper
{
    public class PrintBatchDetail : MPrintDetail, INotifyPropertyChanged
    {
        public string SectionTypeCn
        {
            get { return SectionType == 0 ? "常规" : ((ScannerType)SectionType).GetText(); }
        }

        private bool _isPrint;

        public bool IsPrint
        {
            get { return _isPrint; }
            set
            {
                _isPrint = value;
                OnPropertyChanged("PrintStatusCn");
                OnPropertyChanged("PrintStatusColor");
            }
        }

        public string PrintStatusCn
        {
            get { return IsPrint ? "已打印" : "未打印"; }
        }

        public string PrintStatusColor
        {
            get { return IsPrint ? "Green" : "Gray"; }
        }

        private bool _isChecked = true;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
