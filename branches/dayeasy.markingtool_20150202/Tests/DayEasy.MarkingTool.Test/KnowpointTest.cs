using DayEasy.MarkingTool.BLL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace DayEasy.MarkingTool.Test
{
    [TestClass]
    public class KnowpointTest
    {
        [TestMethod]
        public void GetDateTest()
        {
            const string url = "http://yw.zxxk.com/GetTreeNode.aspx/GetTreeNodeList";
            const string data =
                "{'ChannelID':'0','ClassID':'0','CharterID':'0','SpecialID':'19','Level':'2','UserKeyWords':'','ChannelClassID':'0'}";
            const string cookie =
                "Hm_lvt_0e522924b4bbb2ce3f663e505b2f1f9c=1418718925; Hm_lpvt_0e522924b4bbb2ce3f663e505b2f1f9c=1418718941; XUEYI_TOKEN=cdd751ca-126c-4da7-9452-ba3758b5bdbc; ASP.NET_SessionId=vjznwcwucdb0ja1upulcjick; CNZZDATA1759807=cnzz_eid%3D1543865107-1418715948-http%253A%252F%252Fwww.zxxk.com%252F%26ntime%3D1418715948";
            using (var http = new HttpHelper(url, "POST", Encoding.UTF8, cookie, "http://yw.zxxk.com", data))
            {
                var html = http.GetHtml();
                Console.Write(html);
            }
        }
    }
}
