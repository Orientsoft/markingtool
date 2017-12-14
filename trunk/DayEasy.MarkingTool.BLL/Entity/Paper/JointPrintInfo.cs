using System.ComponentModel;
using DayEasy.MarkingTool.BLL.Annotations;
using DayEasy.MarkingTool.BLL.Enum;
using DayEasy.Models.Open.Work;

namespace DayEasy.MarkingTool.BLL.Entity.Paper
{
    public class JointPrintInfo : MJointPrintInfo, INotifyPropertyChanged
    {
        public string StatusCn
        {
            get { return ((JointStatus)MarkingStatus).GetText(); }
        }

        public string StatusColor
        {
            get { return MarkingStatus == (byte)JointStatus.Finished ? "Green" : "Gray"; }
        }

        public bool IsEnabled
        {
            get { return MarkingStatus == (byte)JointStatus.Finished; }
        }

        public string MarkingCountCn
        {
            get
            {
                if (PaperAb)
                {
                    return string.Format("A:{0} B:{1}", ACount, BCount);
                }
                return ACount.ToString();
            }
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
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
