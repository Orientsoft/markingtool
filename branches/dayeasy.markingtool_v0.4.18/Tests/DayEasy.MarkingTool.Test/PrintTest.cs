using System;
using System.Collections.ObjectModel;
using DayEasy.MarkingTool.BLL;
using DayEasy.MarkingTool.BLL.Data;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.Open.Model.Marking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Text;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class PrintTest
    {
        [TestMethod]
        public void SocketTest()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i % 2);
            }
            //var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IPv4);
            //socket.Connect("192.168.10.254", 9100);
            //if (socket.Connected)
            //    socket.Send(Encoding.UTF8.GetBytes("test"));
        }

        [TestMethod]
        public void DbTest()
        {
            using (var utils = new CacheUtils())
            {
                for (var i = 0; i < 20; i++)
                {
                    utils.Set(string.Format("test-{0}", i));
                }
                var list = utils.GetModels(CacheType.Account, "test-1", 50);
                Console.Write(list.ToJson());
            }
        }

        [TestMethod]
        public void ScannerTest()
        {
            const long userId = 900000000002L;
            const string paperId = "49082c85df6144e09014cae3b1f5ed1f";
            using (var utils = new CacheUtils())
            {
                Console.Write(utils.GetScanner(userId, paperId).ToJson());
            }
        }
    }
}
