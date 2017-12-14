
using DayEasy.MarkingTool.BLL.Config;
using DayEasy.MarkingTool.BLL.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void Init()
        {
            var config = new MarkingConfig
            {
                Rest = "http://open.dayeasy.dev/router",
                RestFile = "http://file.dayez.net/uploader?type=5",
                Partner = "marking_tool",
                SecretKey = "dy123456",
                IsDebug = false,
                MarkingUrl = "http://www.dayeasy.dev/work",
                AdminEmail = ""
            };
            ConfigUtils<MarkingConfig>.Instance.Set(config);

            var config1 = new ScannerConfig
            {
                TaskCount = 3,
                PaperWidth = 780,
                BasicThreshold = 120,
                AnswerThreshold = 140,
                BlackScale = 0.75,
                IsCorrection = true,
                LineHeight = 20,
                QrcodeWidth = 1.8,
                SheetLineHeight = 1,
                SmearWidth = 10,
                SmearHeight = 10
            };
            ConfigUtils<ScannerConfig>.Instance.Set(config1);
        }
    }
}
