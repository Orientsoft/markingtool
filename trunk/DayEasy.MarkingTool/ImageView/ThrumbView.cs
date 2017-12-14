using System.Windows;
using System.Windows.Controls;

namespace DayEasy.MarkingTool.ImageView
{
    public class ThrumbView : ViewBase
    {
        protected override object DefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "ThrumbView"); }
        }

        protected override object ItemContainerDefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "ThrumbViewItem"); }
        }
    }    
}
