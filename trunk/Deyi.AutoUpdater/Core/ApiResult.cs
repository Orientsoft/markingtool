using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Deyi.AutoUpdater.Core
{
    /// <summary>
    /// API统一返回基类
    /// </summary>
    [Serializable]
    [DataContract(Name = "result")]
    public class ResultBase
    {
        /// <summary>
        /// api状态标志
        /// </summary>
        [DataMember(Name = "status", Order = 1)]
        public bool Status { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        [DataMember(Name = "code", Order = 101)]
        public int Code { get; set; }

        /// <summary>
        /// 错误描述
        /// </summary>
        [DataMember(Name = "desc", Order = 103)]
        public string Desc { get; set; }
    }

    [Serializable]
    [DataContract(Name = "result")]
    public class ApiResult<T> : ResultBase
    {
        /// <summary>
        /// 返回数据项
        /// </summary>
        [DataMember(Name = "data", Order = 2)]
        public T Data { get; set; }
    }

    [Serializable]
    [DataContract(Name = "results")]
    public class ApiResults<T> : ResultBase
    {
        /// <summary>
        /// 返回数据列表
        /// </summary>
        [DataMember(Name = "data", Order = 2)]
        public List<T> Data { get; set; }

        /// <summary>
        /// 数据列表总数
        /// </summary>
        [DataMember(Name = "count", Order = 3)]
        public int Count { get; set; }
    }
}
