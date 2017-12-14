using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DayEasy.MarkingTool.BLL.Common;

namespace DayEasy.MarkingTool.BLL
{
    public static class DeyiKeys
    {
        public static readonly MarkingSection Section = SectionManager.Instance.Section;

        public static SizeConfig Size
        {
            get { return Section.SizeConfig; }
        }

        public const string NotPaperImage = "未识别的试卷";
        public const string Success = "成功";
        public const string Failed = "失败";
        public const string BaseInfoError = "试卷的基本信息不正确";
        public const string NotFoundPaper = "没有该作业的相关数据";
        public const string UnKonwnError = "识别失败";

        public const string NoMarkedInfoError = "没有可以上传的批阅信息！";
        public const string HasNotMarkedError = "存在未批阅的试卷！";
        public const string AllMarkedUpdate = "所有试卷已成功上传！";


        public const string Key = "z2zeC9crPgG4uDxXxz2aiB3dbdb3bvN1";
        public const string Iv = "DL+XxX123bs=";
        public const string TipInfo = "提示信息";

        public static string Theme = "Default";

        internal static Dictionary<string, string> ContentTypes = new Dictionary<string, string>
        {
            {"*", "application/octet-stream"},
            {".doc", "application/msword"},
            {".ico", "image/x-icon"},
            {".gif", "image/gif"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".png", "image/x-png"},
            {".mp3", "audio/mpeg"},
            {".mpeg", "audio/mpeg"},
            {".flv", "video/x-flv"},
            {".mp4", "application/octet-stream"},
        };

        public static string CurrentDir
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static string PicturePath = Path.Combine(CurrentDir, "Pictures");



        public static string SavePath = Path.Combine(CurrentDir,
            Section.PathConfig.CuttedPics);

        public const string RecognitionName = "recognition";
        public const string CompressName = "compress";
        public const string MarkedName = "marked";

        public static string ItemPath = Path.Combine(CurrentDir,
            Section.PathConfig.ItemDirectory);

        public static string UnfinishedPaperSavePath = Path.Combine(CurrentDir,
            Section.PathConfig.PaperSave);

        public static string CompressedPath = Path.Combine(CurrentDir,
            Section.PathConfig.Compressed);

        /// <summary>
        /// 页面宽度
        /// </summary>
        public static int PaperWidth = Section.SizeConfig.PaperWidth;

        /// <summary>
        /// 答题涂抹宽度
        /// </summary>
        public static int SmearWidth = Section.SizeConfig.SmearWidth;

        /// <summary>
        /// 答题涂抹高度
        /// </summary>
        public static int SmearHeight = Section.SizeConfig.SmearHeight;

        /// <summary>
        /// 基本信息栏灰阶阀值
        /// </summary>
        public static byte BasicThreshold = Section.SizeConfig.BasicThreshold;

        /// <summary>
        /// 答题卡栏灰阶阀值
        /// </summary>
        public static byte AnswerThreshold = Section.SizeConfig.AnswerThreshold;

        public static bool WriteFile = Section.PathConfig.IsDebug;

        /// <summary>
        /// 试卷行高
        /// </summary>
        public static int PaperLineHeight = Section.SizeConfig.LineHeight;

        public static double QrcodeWidth = Section.SizeConfig.QrcodeWidth;

        public static byte[] QrcodeThresholds
        {
            get
            {
                return
                    Section.SizeConfig.QrcodeThresholds.Split(',')
                        .Select(t => Helper.ToByte(t, 120))
                        .Distinct()
                        .ToArray();
            }
        }

        ///// <summary>
        ///// 答题选项
        ///// </summary>
        //public const string AnswerOptions = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// 选项字母集
        /// </summary>
        public static string[] OptionWords
        {
            get
            {
                var list = new List<string>();
                for (var i = 65; i < 91; i++)
                    list.Add(Convert.ToChar(i).ToString(CultureInfo.InvariantCulture));
                return list.ToArray();
            }
        }

        /// <summary>
        /// 答题卡区域填充比例阀值
        /// </summary>
        public const float AnswerDetermineThreshold = 0.8f;

        public const string MSystemManifest = "system.manifest";
        public const string MUserLogin = "user.login";
        public const string MLoadUserInfo = "user.load";
        public const string MSubjects = "system.subjects";
        public const string MQuestionLoad = "question.item";
        public const string MQuestionsLoad = "question.list";
        public const string MPaperLoad = "paper.load";
        public const string MPaperUsage = "paper.usage";
        public const string MPaperUpload = "system.upload";
        public const string MPaperMarking = "paper.handinandmarking";
        public const string MPaperPrintUsages = "paper.printusages";
        public const string MPaperCloseUsage = "paper.closeusage";
    }
}
