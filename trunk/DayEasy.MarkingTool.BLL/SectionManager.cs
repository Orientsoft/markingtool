using System.Configuration;

namespace DayEasy.MarkingTool.BLL
{
    /// <summary> 配置管理器 </summary>
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
