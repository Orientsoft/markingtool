using DayEasy.MarkingTool.BLL.Entity;
using System.Collections.Generic;

namespace DayEasy.MarkingTool.BLL.Recognition
{
    public class DefaultRecognition : DRecognition
    {
        public DefaultRecognition(string imagePath, List<ObjectiveItem> objectives)
            : base(imagePath, objectives)
        {
        }
    }
}
