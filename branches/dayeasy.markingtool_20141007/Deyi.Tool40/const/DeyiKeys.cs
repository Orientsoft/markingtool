
using System;
using System.IO;
using Deyi.Tool.Common;
namespace Deyi.Tool
{
    public class DeyiKeys
    {

        internal const string NotPaperImage = "为识别的试卷";
        internal const string Success = "成功";
        internal const string Failed = "失败";
        internal const string BaseInfoError = "试卷的基本信息不正确";
        internal const string NotFoundPaper = "没有改作业的相关数据";
        internal const string UnKonwnError = "未知原因识别错误";

        internal const string _KEY = "z2zeC9crPgG4uDxXxz2aiB3dbdb3bvN1";
        internal const string _IV = "DL+XxX123bs=";
        internal const string _TIP_INFO = "提示信息";

        internal static string PicturePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pictures");

        internal static string SavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            Helper.GetAppSetting("CuttedPicsDirName"));

        internal static string ItemPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            Helper.GetAppSetting("ItemDirectory"));

        internal static string UnfinishedPaperSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            Helper.GetAppSetting("PaperSaveDirName"));

        internal static string CompressedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            Helper.GetAppSetting("CompressedPath"));

        /// <summary>
        /// 页面宽度
        /// </summary>
        internal static int PaperWidth = Helper.StrToInt(Helper.GetAppSetting("PaperWidth"), 780);

        /// <summary>
        /// 答题涂抹宽度
        /// </summary>
        internal static int SmearWidth = Helper.StrToInt(Helper.GetAppSetting("SmearWidth"), 6);

        /// <summary>
        /// 答题涂抹高度
        /// </summary>
        internal static int SmearHeight = Helper.StrToInt(Helper.GetAppSetting("SmearHeight"), 5);

        /// <summary>
        /// 基本信息栏灰阶阀值
        /// </summary>
        internal static byte BasicThreshold = (byte) Helper.StrToInt(Helper.GetAppSetting("BasicThreshold"), 140);

        /// <summary>
        /// 答题卡栏灰阶阀值
        /// </summary>
        internal static byte AnswerThreshold = (byte) Helper.StrToInt(Helper.GetAppSetting("AnswerThreshold"), 140);

        internal static bool WriteFile = Helper.GetAppSetting("WriteFile") == "True";

        /// <summary>
        /// 试卷行高
        /// </summary>
        internal const int PaperLineHeight = 32;

        internal const int QrcodeWidth = 440;

        /// <summary>
        /// 答题选项
        /// </summary>
        internal const string AnswerOptions = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //图片相关~

        /// <summary>
        /// 答题卡区域填充比例阀值
        /// </summary>
        internal const float AnswerDetermineThreshold = 0.8f;

    }
}
