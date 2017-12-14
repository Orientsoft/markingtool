using DayEasy.MarkingTool.BLL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class SectionManagerTest
    {
        [TestMethod]
        public void LoadTest()
        {
            var section = SectionManager.Instance;
            Assert.IsNotNull(section);
        }
    }
}
