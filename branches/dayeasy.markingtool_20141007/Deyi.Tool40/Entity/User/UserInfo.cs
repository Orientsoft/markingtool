# region 四川得一科技有限公司 版权所有
/* ================================================
 * 公司：四川得一科技有限公司
 * 作者：文杰
 * 创建：2013-10-30
 * 描述：用户实体
 * ================================================
 */
# endregion

namespace Deyi.Tool.Entity.User
{
    public class UserInfo
    {
        private static readonly UserInfo _instance = new UserInfo();

        private UserInfo()
        { }

        /// <summary>
        /// 当前登录用户信息
        /// </summary>
        public static UserInfo Current { get { return _instance; } }

        /// <summary>
        /// 用户编号
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string TrueName { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        public string Email { get; set; }
    }
}
