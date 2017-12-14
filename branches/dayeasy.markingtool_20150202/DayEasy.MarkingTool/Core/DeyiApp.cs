using DayEasy.Open.Model.User;

namespace DayEasy.MarkingTool.Core
{
    internal static class DeyiApp
    {
        internal static UserInfo CurrentUser;

        internal static long UserId
        {
            get
            {
                if (CurrentUser == null) return 0;
                return CurrentUser.UserId;
            }
        }

        internal static string DisplayName
        {
            get
            {
                if (CurrentUser == null) return string.Empty;
                if (!string.IsNullOrWhiteSpace(CurrentUser.TrueName))
                    return CurrentUser.TrueName;
                if (!string.IsNullOrWhiteSpace(CurrentUser.NickName))
                    return CurrentUser.NickName;
                return CurrentUser.Email;
            }
        }
    }
}
