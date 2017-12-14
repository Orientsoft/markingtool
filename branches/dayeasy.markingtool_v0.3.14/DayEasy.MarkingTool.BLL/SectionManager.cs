using System.Configuration;

namespace DayEasy.MarkingTool.BLL
{
    public class SectionManager
    {
        private readonly MarkingSection _section;

        private SectionManager()
        {
            _section = ConfigurationManager.GetSection("MarkingSection") as MarkingSection;
        }

        public static SectionManager Instance
        {
            get
            {
                return Singleton<SectionManager>.Instance ?? (Singleton<SectionManager>.Instance = new SectionManager());
            }
        }

        public MarkingSection Section
        {
            get { return _section; }
        }
    }
}
