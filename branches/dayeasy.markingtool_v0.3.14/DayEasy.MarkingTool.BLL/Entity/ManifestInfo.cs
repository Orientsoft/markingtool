using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DayEasy.MarkingTool.BLL.Entity
{
    [Serializable]
    [XmlRoot("root")]
    [DataContract(Name = "manifest")]
    public class ManifestInfo
    {
        /// <summary>
        /// 版本标识
        /// </summary>
        [XmlElement("version_code")]
        [DataMember(Name = "version_code")]
        public int VersionCode { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        [XmlElement("version")]
        [DataMember(Name = "version")]
        public string Version { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public Version AppVersion { get { return new Version(Version); } }

        private string _upgradeInstructions;

        /// <summary>
        /// 更新内容
        /// </summary>
        [XmlElement("upgrade_instructions")]
        [DataMember(Name = "upgrade_instructions")]
        public string UpgradeInstructions
        {
            get { return _upgradeInstructions; }
            set
            {
                _upgradeInstructions = string.Join(Environment.NewLine,
                    value.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        /// <summary>
        /// 下载链接
        /// </summary>
        [XmlElement("download_url")]
        [DataMember(Name = "download_url")]
        public string DownloadUrl { get; set; }

        /// <summary>
        /// 更新包大小
        /// </summary>
        [XmlElement("apk_size")]
        [DataMember(Name = "apk_size")]
        public string ApkSize { get; set; }

        /// <summary>
        /// 是否强制更新
        /// </summary>
        [XmlElement("mandatory")]
        [DataMember(Name = "mandatory")]
        public bool Mandatory { get; set; }

        /// <summary>
        /// 文件MD5码
        /// </summary>
        [XmlElement("md5")]
        [DataMember(Name = "md5")]
        public Guid Md5 { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        [XmlElement("upgrade_date")]
        [DataMember(Name = "upgrade_date")]
        public string UpgradeDate { get; set; }
    }
}
