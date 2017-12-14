using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DayEasy.MarkingTool.BLL
{
    /// <summary>
    /// 基础数据结果类
    /// </summary>
    [Serializable]
    [DataContract(Name = "result")]
    public class JsonResultBase
    {
        [DataMember(Name = "status", Order = 1)]
        public bool Status { get; set; }

        [DataMember(Name = "code",Order = 2)]
        public int Code { get; set; }

        [DataMember(Name = "msg", Order = 3)]
        public string Message { get; set; }

        [DataMember(Name = "desc", Order = 4)]
        public string Description { get; set; }

        public JsonResultBase(bool status, string message)
        {
            Status = status;
            Description = message;
        }

        public JsonResultBase(string message)
            : this(false, message)
        {
        }
    }

    [Serializable]
    [DataContract(Name = "result")]
    public class JsonResult<T> : JsonResultBase
    {
        [DataMember(Name = "data", Order = 3)]
        public T Data { get; set; }

        public JsonResult(bool status, T data)
            : base(status, string.Empty)
        {
            Data = data;
        }

        public JsonResult(string message)
            : base(false, message)
        {
        }
    }

    [Serializable]
    [DataContract(Name = "result")]
    public class JsonResults<T> : JsonResultBase
    {
        [DataMember(Name = "data",Order = 3)]
        public IEnumerable<T> Data { get; set; }

        [DataMember(Name = "count",Order = 4)]
        public int TotalCount { get; set; }

        public JsonResults(string message)
            : base(false, message)
        {
        }

        public JsonResults(IEnumerable<T> list, int totalCount)
            : base(true, string.Empty)
        {
            Data = list;
            TotalCount = totalCount;
        }
    }
}
