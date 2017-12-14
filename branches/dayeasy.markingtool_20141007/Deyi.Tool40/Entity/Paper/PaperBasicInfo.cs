# region 四川得一科技有限公司 版权所有
/* ================================================
 * 公司：四川得一科技有限公司
 * 作者：文杰
 * 创建：2013-12-28
 * 描述：作业基本信息实体
 * ================================================
 */
# endregion

using System;

namespace Deyi.Tool.Entity.Paper
{
    /// <summary>
    /// 作业基本信息实体
    /// </summary>
    public class PaperBasicInfo
    {
        /// <summary>
        /// 作业编号
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 作业标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddedAt { get; set; }

        /// <summary>
        /// 显示添加时间
        /// </summary>
        public string DisplayAddedAt { get; set; }
    }
}
