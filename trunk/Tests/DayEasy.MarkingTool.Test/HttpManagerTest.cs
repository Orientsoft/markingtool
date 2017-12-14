using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class HttpManagerTest
    {
        private readonly RestHelper _manager = RestHelper.Instance;

        public HttpManagerTest()
        {
            DeyiApp.Token = "5d006b93979744c18ffee2ef5e90e149";
        }

        [TestMethod]
        public void SubjectsTest()
        {
            var result = _manager.GetSubjectInfos();
            Console.Write(result.ToJson());
        }

        [TestMethod]
        public void LoginTest()
        {
            var result = _manager.Login("634330628@qq.com", "a123456");
            Console.Write(result.ToJson());
        }

        [TestMethod]
        public void LoadUserTest()
        {
            var result = _manager.LoadUserInfo();
            Console.Write(result.ToJson());
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
        public void JointUsagesTest()
        {
            var paperId = "2210763414684e0bab6cde6ff64ea72a";
            var result = _manager.JointUsages(paperId);
            WriteEntity(result);
        }

        [TestMethod]
        public void BatchPrintsTest()
        {
            var result = _manager.BatchPrints("2210763414684e0bab6cde6ff64ea72a");
            WriteEntity(result);
        }

        [TestMethod]
        public void PrintDetailsTest()
        {
            var result = _manager.PrintDetails("91aeadaf04d7450c862d93aab617bcde");
            WriteEntity(result);
        }

        [TestMethod]
        public void JointPrintListTest()
        {
            const string paperId = "46d1524807d54efaa11231de44c079ad";
            var result = _manager.JointPrintList(paperId);
            WriteEntity(result);
        }

        [TestMethod]
        public void JointAgenciesTest()
        {
            const string joint = "0d48c11c8f254dfcafba4cad6bdf3a02";
            var result = _manager.JointAgencies(joint);
            WriteEntity(result);
        }

        [TestMethod]
        public void JointPrintDetailsTest()
        {
            const string joint = "0d48c11c8f254dfcafba4cad6bdf3a02";
            var result = _manager.JointPrintDetails(joint, 1);
            WriteEntity(result);
        }

        [TestMethod]
        public void TeacherClassTest()
        {
            var result = _manager.TeacherGroups();
            WriteEntity(result);
        }

        [TestMethod]
        public void StudentListTest()
        {
            var result = _manager.StudentList("GC80340");
            WriteEntity(result);
        }

        [TestMethod]
        public void ManifestTest()
        {
            var result = _manager.Manifest();
            WriteEntity(result);
        }

        [TestMethod]
        public void PaperTest()
        {
            const string id = "86579e6b91b04f458c397936b0c25c84";
            var result = _manager.LoadPaperByNum(id);
            WriteEntity(result);
        }

        private void WriteEntity(DResult result)
        {
            Console.WriteLine("-----ToJson-----");
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }
    }
}
