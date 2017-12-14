using System.IO;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using DayEasy.MarkingTool.BLL.Entity.Marking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class HttpManagerTest
    {
        private readonly RestHelper _manager = RestHelper.Instance;

        [TestMethod]
        public void SubjectsTest()
        {
            var result = _manager.GetSubjectInfos();
            Console.Write(result.ToJson2());
        }

        [TestMethod]
        public void LoginTest()
        {
            var result = _manager.Login("634330628@qq.com", "a123456");
            Console.Write(result.ToJson2());
        }

        [TestMethod]
        public void LoadUserTest()
        {
            const string token = "f32b62f81f06430ba0586fa9b49da106";
            var result = _manager.LoadUserInfo(token);
            Console.Write(result.ToJson2());
        }

        [TestMethod]
        public void TimeOutTest()
        {
            const string url =
                "http://open.dayeasy.dev/router?method=system.subjects&tick=7331275974440&partner=test&sign=a83e6e3ea4ec286de50db34c4e7417cc";
            using (var http = new HttpHelper(url,Encoding.UTF8))
            {
                http.SetContentType("application/json");
                Console.Write(http.GetHtml());
            }
        }

        [TestMethod]
        public void PaperLoadTest()
        {
            const string id = "a9c2033b43c44c9089197b9d640ba734";
            var paper = _manager.LoadPaper(id);
            Console.Write(paper.ToJson2());
        }

        [TestMethod]
        public void LoadPaperUsageTest()
        {
            const string batchNo = "14f7a60208634a9ab0009136fba5a44b";
            var result = _manager.LoadPaperUsage(batchNo);
            Console.Write(result.ToJson2());
        }

        [TestMethod]
        public void QuestionLoadTest()
        {
            const string id = "dc03dbf36ab647af8ced48ad12474157";
            var question = _manager.LoadQuestion(id);
            Console.Write(question.ToJson2());
        }

        [TestMethod]
        public void QuestionLoadsTest()
        {
            var ids = new[] { "dc03dbf36ab647af8ced48ad12474157", "b891748973084c80b0540153384650e8" };
            var questions = _manager.LoadQuestions(ids);
            Console.Write(questions.ToJson2());
        }

        [TestMethod]
        public void PacketTest()
        {
            Helper.PacketMarkedPicture("000002", "fefe");
        }

        [TestMethod]
        public void UploadFileTest()
        {
            var result = _manager.UpdateFile(new PaperMarkingFileData
            {
                Data = File.OpenRead(Path.Combine(DeyiKeys.CompressedPath, "fefe/000002"))
            });
            Console.Write(result.ToJson2());
        }

        [TestMethod]
        public void PrintUsagesTest()
        {
            DeyiApp.Token = "5086fefd27604d8c84b277d8cf4df003";
            var result = _manager.PrintUsages(true, 0, 15);
            Console.Write(result.ToJson2());
        }
    }
}
