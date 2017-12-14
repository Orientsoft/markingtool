using System.Windows.Media;

namespace DayEasy.MarkingTool.UI
{
    /// <summary>
    /// PaperImageDetail.xaml 的交互逻辑
    /// </summary>
    public partial class PaperImageDetail
    {
        public PaperImageDetail(ImageSource img)
            : this()
        {
            detail.Source = img;
        }

        public PaperImageDetail()
        {
            InitializeComponent();
        }


        public void SetDisplay(ImageSource img)
        {
            detail.Source = img;
        }

        //private static PaperImageDetail detailWindow = null;

        //public static Window GetDetailWindow()
        //{
        //    var detailWin = (Window)CallContext.GetData("detailWindow");
        //    if (detailWin == null)
        //    {
        //        detailWin = new PaperImageDetail();
        //        CallContext.SetData("detailWindow", detailWin);
        //    }
        //    return detailWin;
        //}
    }
}
