using System;
using System.Xml.Serialization;

namespace DayEasy.MarkingTool.BLL.Config
{
    [Serializable]
    [FileName("marking.config")]
    [XmlRoot("root")]
    public class MarkingConfig : ConfigBase
    {
        [XmlAttribute("partner")]
        public string Partner { get; set; }

        [XmlAttribute("secretKey")]
        public string SecretKey { get; set; }

        /// <summary> 接口路由 </summary>
        [XmlElement("rest")]
        public string Rest { get; set; }

        /// <summary> 文件上传接口 </summary>
        [XmlElement("restFile")]
        public string RestFile { get; set; }

        /// <summary> 阅卷地址 </summary>
        [XmlElement("markingUrl")]
        public string MarkingUrl { get; set; }

        /// <summary> 是否开启调试 </summary>
        [XmlElement("debug")]
        public bool IsDebug { get; set; }

        /// <summary> 管理员邮箱 </summary>
        [XmlElement("adminEmail")]
        public string AdminEmail { get; set; }
    }
}
