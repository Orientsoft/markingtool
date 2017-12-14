using System.ComponentModel;
using DayEasy.MarkingTool.BLL.Annotations;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.Models.Open.Work;

namespace DayEasy.MarkingTool.BLL.Entity.Paper
{
    public class PrintBatchInfo : MPrintInfo, INotifyPropertyChanged
    {
        public string StatusCn
        {
            get { return ((MarkingStatus)MarkingStatus).GetText(); }
        }

        public string StatusColor
        {
            get
            {
                return IsEnabled ? "Green" : "Gray";
            }
        }

        public bool IsEnabled
        {
            get { return (MarkingStatus == (byte)Enum.MarkingStatus.AllFinished); }
        }

        public string DateCn
        {
            get { return MarkingTime.ToDateTime().ToString("yyyy-MM-dd HH:mm"); }
        }

        public bool PaperAb { get; set; }

        public bool NormalPaper { get; set; }

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
