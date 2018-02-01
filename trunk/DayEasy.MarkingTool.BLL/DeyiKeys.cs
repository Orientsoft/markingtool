using DayEasy.MarkingTool.BLL.Config;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace DayEasy.MarkingTool.BLL
{
    /// <summary> 全局静态资源类 </summary>
    public static class DeyiKeys
    {
        public const string Version = "0.6.32";
        public static MarkingConfig MarkingConfig
        {
            get { return ConfigUtils<MarkingConfig>.Config; }
        }

        private static int _guid;

        public static int Guid
        {
            get { return _guid = ++_guid; }
        }

        public static ScannerConfig ScannerConfig
        {
            get { return ConfigUtils<ScannerConfig>.Config; }
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
        public const string ExistsFail = "存在不可上传的试卷，是否继续上传？";


        public const string Key = "z2zeC9crPgG4uDxXxz2aiB3dbdb3bvN1";
        public const string Iv = "DL+XxX123bs=";
        public const string TipInfo = "提示信息";

        public const string Theme = "Default";

        public const string LiteDbName = "markingCache.db";

        internal static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>
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

        /// <summary> 识别文件 </summary>
        internal const string RecognitionName = "recognition";

        /// <summary> 压缩文件 </summary>
        public const string CompressName = "compress";

        /// <summary> 批阅文件 </summary>
        public const string MarkedName = "marked";

        /// <summary> 扫描文件 </summary>
        internal const string ScannerName = "scanner";

        /// <summary> 碎片文件 </summary>
        internal const string SliceName = "slice";

        /// <summary> 调试文件 </summary>
        internal const string DebugName = "debug";

        internal const string SaveDirectory = "dsave";

        /// <summary> 阅卷基础文件 </summary>
        public static readonly string MarkingPath = Path.Combine(CurrentDir, "DMarking");

        /// <summary> 临时文件 </summary>
        public static readonly string ItemPath = Path.Combine(CurrentDir, "DTemp");
        /// <summary> 总得分坐标 </summary>
        public static PointF ScorePointF = new PointF(320, 20);
        /// <summary> 客观题错题坐标 </summary>
        public static PointF ErrorPointF = new PointF(300, 60);

        public static readonly byte[] QrcodeThresholds =
        {
            120, 150, 90, 180, 95, 100, 105, 110, 115, 125, 130, 135, 140, 145, 155, 160, 165, 170, 175, 185,
            190, 195, 200, 205, 210, 215, 220, 225, 230, 235, 240, 245
        };

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

        /// <summary> 应用版本清单 </summary>
        public const string MSystemManifest = "system_manifest";
        /// <summary> 用户登录 </summary>
        public const string MUserLogin = "user_login";
        /// <summary> 获取用户信息 </summary>
        public const string MLoadUserInfo = "user_load";
        /// <summary> 获取学生信息 </summary>
        public const string MStudent = "user_student";

        public const string MStudentSearch = "user_searchStudent";
        /// <summary> 教师班级 </summary>
        public const string MTeacherGroups = "group_groups";
        /// <summary> 班级圈学生列表 </summary>
        public const string MGroupStudents = "group_students";
        /// <summary> 科目列表 </summary>
        public const string MSubjects = "system_subjects";
        /// <summary> (试卷编号)试卷详情 </summary>
        public const string MPaperInfo = "paper_info";
        /// <summary> 提交试卷 </summary>
        public const string MPaperHandinPictures = "work_handinPictures";
        /// <summary> 批次号下的打印列表 </summary>
        public const string MWorkBatchPrint = "work_batchprint";
        public const string MWorkPrintDetails = "work_printdetails";
        public const string MWorkJointUsages = "work_jointUsages";
        public const string MWorkJointPrintList = "work_jointBatchPrint";
        public const string MWorkJointAgencies = "work_jointAgencies";
        public const string MWorkJointPrintDetails = "work_jointPrintDetails";

        /// <summary> 发送邮件 </summary>
        public const string MMessageEmail = "message_email";
    }
}
