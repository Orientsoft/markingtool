using System;

namespace Deyi.Tool.Entity.Paper
{
    class PaperUsage
    {
        /// <summary>
        /// 批次号
        /// </summary>
        public long BatchNo
        {
            get;
            set;
        }

        /// <summary>
        /// 试卷ID
        /// </summary>
        public Guid PaperID
        {
            get;
            set;
        }

        /// <summary>
        /// 使用者IP
        /// </summary>
        public string AddedIP
        {
            get;
            set;
        }

        /// <summary>
        /// 使用者ID
        /// </summary>
        public long AddedBy
        {
            get;
            set;
        }

        /// <summary>
        /// 使用时间
        /// </summary>
        public DateTime AddedAt
        {
            get;
            set;
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpireTime
        {
            get;
            set;
        }

        ///// <summary>
        ///// 试卷用法
        ///// </summary>
        //public PaperUsageType Usage
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// 是否计算总分
        /// </summary>
        public bool CalculateScore
        {
            get;
            set;
        }
    }
}
