using System;
using System.Text;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class HttpManagerTest
    {
        private readonly RestHelper _manager = RestHelper.Instance;

        public HttpManagerTest()
        {
            DeyiApp.Token = "d6865f8c0f7a49c3a96c5c49a4b94862";
        }

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
            var result = _manager.LoadUserInfo(DeyiApp.Token);
            Console.Write(result.ToJson2());
        }

        [TestMethod]
        public void TimeOutTest()
        {
            const string url =
                "http://open.dayeasy.dev/router?method=system.subjects&tick=7331275974440&partner=test&sign=a83e6e3ea4ec286de50db34c4e7417cc";
            using (var http = new HttpHelper(url, Encoding.UTF8))
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
        public void PrintUsagesTest()
        {
            var result = _manager.PrintUsages(true, 0, 15);
            Console.Write(result.ToJson2());
        }


        [TestMethod]
        public void BatchPrintsTest()
        {
            var result = _manager.BatchPrints("04bf6102dd7c423789d05513e4cccfeb");
            WriteEntity(result);
        }

        [TestMethod]
        public void PrintDetailsTest()
        {
            var result = _manager.PrintDetails("a1de61f3ce2a4f9ca7e3e724dec904d2");
            WriteEntity(result);
        }

        [TestMethod]
        public void TeacherClassTest()
        {
            var result = _manager.TeacherClass();
            WriteEntity(result);
        }

        [TestMethod]
        public void AgenciesTest()
        {
            var result = _manager.Agencies();
            WriteEntity(result);
        }

        [TestMethod]
        public void AgencyClassesTest()
        {
            const string agencyId = "9e04bfc885504baea722f6a283c3e599";
            var result = _manager.AgencyClasses(agencyId);
            WriteEntity(result);
        }

        private void WriteEntity(JsonResultBase result)
        {
            Console.WriteLine("-----ToJson-----");
            Console.WriteLine(result.ToJson());
            Console.WriteLine("-----ToJson2-----");
            Console.WriteLine(result.ToJson2());
        }
    }
}
