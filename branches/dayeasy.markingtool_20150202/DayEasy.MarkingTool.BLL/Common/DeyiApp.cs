using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.Open.Model.Paper;
using DayEasy.Open.Model.User;

namespace DayEasy.MarkingTool.BLL.Common
{
    public class DeyiApp
    {
        public static UserInfo CurrentUser;

        private static readonly Dictionary<string, string> PaperUsageDicts
            = new Dictionary<string, string>();

        public static long UserId
        {
            get
            {
                if (CurrentUser == null) return 0;
                return CurrentUser.UserId;
            }
        }

        public static string Token = string.Empty;

        public static string DisplayName
        {
            get
            {
                if (CurrentUser == null) return string.Empty;
                if (!string.IsNullOrWhiteSpace(CurrentUser.RealName))
                    return CurrentUser.RealName;
                if (!string.IsNullOrWhiteSpace(CurrentUser.NickName))
                    return CurrentUser.NickName;
                return CurrentUser.Email;
            }
        }

        public static PaperUsageInfo GetBatchDetail(string batchNo)
        {
            var info = new PaperUsageInfo();
            if (PaperUsageDicts.ContainsKey(batchNo))
            {
                info = PaperUsageDicts[batchNo].JsonToObject<PaperUsageInfo>();
            }
            else
            {
                var result = RestHelper.Instance.LoadPaperUsage(batchNo);
                if (result.Status)
                {
                    info = result.Data;
                    PaperUsageDicts.Add(batchNo, info.ToJson());
                }
            }
            return info;
        }

        /// <summary>
        /// 是否64位
        /// </summary>
        /// <returns></returns>
        public static bool Is64BitProcess()
        {
            return IntPtr.Size == 8;
        }
    }
}
