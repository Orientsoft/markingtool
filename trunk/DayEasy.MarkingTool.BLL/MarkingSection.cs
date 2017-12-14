using System;
using System.Configuration;

namespace DayEasy.MarkingTool.BLL
{
    /// <summary> 工具配置 </summary>
    [Serializable]
    public class MarkingSection : ConfigurationSection
    {
        [ConfigurationProperty("Marking", IsDefaultCollection = false)]
        public MarkingConfig MarkingConfig
        {
            get { return (MarkingConfig)base["Marking"]; }
            set { base["Marking"] = value; }
        }

        [ConfigurationProperty("Path", IsDefaultCollection = false)]
        public PathConfig PathConfig
        {
            get { return (PathConfig)base["Path"]; }
            set { base["Path"] = value; }
        }

        [ConfigurationProperty("Size", IsDefaultCollection = false)]
        public SizeConfig SizeConfig
        {
            get { return (SizeConfig)base["Size"]; }
            set { base["Size"] = value; }
        }
    }

    /// <summary>
    /// 接口类配置
    /// </summary>
    [Serializable]
    public class MarkingConfig : ConfigurationElement
    {
        /// <summary>
        /// 文件服务器链接
        /// </summary>
        [ConfigurationProperty("fileUrl", DefaultValue = "", IsRequired = true)]
        public string FileUrl
        {
            get { return (string)base["fileUrl"]; }
            set { base["fileUrl"] = value; }
        }

        /// <summary>
        /// 接口路由链接
        /// </summary>
        [ConfigurationProperty("routeUrl", DefaultValue = "", IsRequired = true)]
        public string RouteUrl
        {
            get { return (string)base["routeUrl"]; }
            set { base["routeUrl"] = value; }
        }

        /// <summary>
        /// 合作商户
        /// </summary>
        [ConfigurationProperty("partner", DefaultValue = "", IsRequired = true)]
        public string Partner
        {
            get { return (string)base["partner"]; }
            set { base["partner"] = value; }
        }

        /// <summary>
        /// 签名密钥
        /// </summary>
        [ConfigurationProperty("secretKey", DefaultValue = "", IsRequired = true)]
        public string SecretKey
        {
            get { return (string)base["secretKey"]; }
            set { base["secretKey"] = value; }
        }

        /// <summary>
        /// 在线批阅网址
        /// </summary>
        [ConfigurationProperty("markingUrl", DefaultValue = "", IsRequired = false)]
        public string MarkingUrl
        {
            get { return (string)base["markingUrl"]; }
            set { base["markingUrl"] = value; }
        }

        /// <summary>
        /// 管理员邮箱
        /// </summary>
        [ConfigurationProperty("adminEmail", DefaultValue = "", IsRequired = false)]
        public string AdminEmail
        {
            get { return (string)base["adminEmail"]; }
            set { base["adminEmail"] = value; }
        }
    }

    /// <summary>
    /// 文件类配置
    /// </summary>
    [Serializable]
    public class PathConfig : ConfigurationElement
    {
        /// <summary>
        /// 是否调试模式
        /// </summary>
        [ConfigurationProperty("isDebug", DefaultValue = false)]
        public bool IsDebug
        {
            get { return (bool)base["isDebug"]; }
            set { base["isDebug"] = value; }
        }

        /// <summary>
        /// 是否自动纠偏
        /// </summary>
        [ConfigurationProperty("isCorrection", DefaultValue = false)]
        public bool IsCorrection
        {
            get { return (bool)base["isCorrection"]; }
            set { base["isCorrection"] = value; }
        }

        ///// <summary>
        ///// 剪切图片文件
        ///// </summary>
        //[ConfigurationProperty("marking", DefaultValue = "Marking", IsRequired = true)]
        //public string Marking
        //{
        //    get { return (string)base["marking"]; }
        //    set { base["marking"] = value; }
        //}

        ///// <summary>
        ///// 试卷保存文件
        ///// </summary>
        //[ConfigurationProperty("paperSave", DefaultValue = "PaperSave", IsRequired = true)]
        //public string PaperSave
        //{
        //    get { return (string) base["paperSave"]; }
        //    set { base["paperSave"] = value; }
        //}

        ///// <summary>
        ///// 压缩文件
        ///// </summary>
        //[ConfigurationProperty("compressed", DefaultValue = "Compressed", IsRequired = true)]
        //public string Compressed
        //{
        //    get { return (string) base["compressed"]; }
        //    set { base["compressed"] = value; }
        //}

        ///// <summary>
        ///// 临时文件
        ///// </summary>
        //[ConfigurationProperty("paperTemp", DefaultValue = "PaperTemp", IsRequired = true)]
        //public string PaperTemp
        //{
        //    get { return (string) base["paperTemp"]; }
        //    set { base["paperTemp"] = value; }
        //}
    }

    /// <summary>
    /// 尺寸类配置
    /// </summary>
    [Serializable]
    public class SizeConfig : ConfigurationElement
    {
        /// <summary>
        /// 扫描开启线程数
        /// </summary>
        [ConfigurationProperty("taskCount", DefaultValue = 1, IsRequired = true)]
        public int TaskCount
        {
            get { return (int)base["taskCount"]; }
            set { base["taskCount"] = value; }
        }

        /// <summary>
        /// 线条扫描宽度
        /// </summary>
        [ConfigurationProperty("scanWidth", DefaultValue = 500, IsRequired = true)]
        public int LineScanWidth
        {
            get { return (int)base["scanWidth"]; }
            set { base["scanWidth"] = value; }
        }

        /// <summary>
        /// 黑点比例
        /// </summary>
        [ConfigurationProperty("blackScale", DefaultValue = 0.85, IsRequired = true)]
        public double BlackScale
        {
            get { return (double)base["blackScale"]; }
            set { base["blackScale"] = value; }
        }

        /// <summary>
        /// 页面宽度
        /// </summary>
        [ConfigurationProperty("paperWidth", DefaultValue = 780, IsRequired = true)]
        public int PaperWidth
        {
            get { return (int)base["paperWidth"]; }
            set { base["paperWidth"] = value; }
        }

        /// <summary>
        /// 答题涂抹宽度
        /// </summary>
        [ConfigurationProperty("smearWidth", DefaultValue = 6, IsRequired = true)]
        public int SmearWidth
        {
            get { return (int)base["smearWidth"]; }
            set { base["smearWidth"] = value; }
        }

        /// <summary>
        /// 答题涂抹高度
        /// </summary>
        [ConfigurationProperty("smearHeight", DefaultValue = 5, IsRequired = true)]
        public int SmearHeight
        {
            get { return (int)base["smearHeight"]; }
            set { base["smearHeight"] = value; }
        }

        /// <summary>
        /// 试卷行高
        /// </summary>
        [ConfigurationProperty("lineHeight", DefaultValue = 20)]
        public int LineHeight
        {
            get { return (int)base["lineHeight"]; }
            set { base["lineHeight"] = value; }
        }

        /// <summary>
        /// 试卷行高
        /// </summary>
        [ConfigurationProperty("sheetHeight", DefaultValue = 18.5F)]
        public float SheetLineHeight
        {
            get { return (float)base["sheetHeight"]; }
            set { base["sheetHeight"] = value; }
        }

        /// <summary>
        /// 二维码宽度
        /// </summary>
        [ConfigurationProperty("qrcodeWidth", DefaultValue = 0.2)]
        public double QrcodeWidth
        {
            get { return (double)base["qrcodeWidth"]; }
            set { base["qrcodeWidth"] = value; }
        }


        /// <summary>
        /// 基本信息栏灰阶阀值
        /// </summary>
        [ConfigurationProperty("basicThreshold", DefaultValue = (byte)140, IsRequired = true)]
        public byte BasicThreshold
        {
            get { return (byte)base["basicThreshold"]; }
            set { base["basicThreshold"] = value; }
        }

        /// <summary>
        /// 答题卡栏灰阶阀值
        /// </summary>
        [ConfigurationProperty("answerThreshold", DefaultValue = (byte)120, IsRequired = true)]
        public byte AnswerThreshold
        {
            get { return (byte)base["answerThreshold"]; }
            set { base["answerThreshold"] = value; }
        }

        ///// <summary>
        ///// 答题卡栏灰阶阀值
        ///// </summary>
        //[ConfigurationProperty("answerThresholdLow", DefaultValue = (byte)80, IsRequired = true)]
        //public byte AnswerThresholdLow
        //{
        //    get { return (byte)base["answerThresholdLow"]; }
        //    set { base["answerThresholdLow"] = value; }
        //}

        ///// <summary>
        ///// 二维码灰阶阀值
        ///// </summary>
        //[ConfigurationProperty("qrcodeThresholds", DefaultValue = "120,150,90,180", IsRequired = true)]
        //public string QrcodeThresholds
        //{
        //    get { return (string)base["qrcodeThresholds"]; }
        //    set { base["qrcodeThresholds"] = value; }
        //}
    }
}
