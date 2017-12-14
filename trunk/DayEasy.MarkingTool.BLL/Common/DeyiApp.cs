using System;
using System.Collections.Generic;
using System.Windows;
using DayEasy.MarkingTool.BLL.Data;
using DayEasy.Models.Open.Group;
using DayEasy.Models.Open.Paper;
using DayEasy.Models.Open.User;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary> APP全局静态存储资源类 </summary>
    public class DeyiApp
    {
        public static MUserDto CurrentUser;

        private static readonly Dictionary<string, string> PaperInfoDicts
            = new Dictionary<string, string>();

        private static readonly Dictionary<string, MStudentListDto> StudentListCache =
            new Dictionary<string, MStudentListDto>();

        private static readonly Dictionary<string, StudentDto> StudentCache =
            new Dictionary<string, StudentDto>();
        
        public static long UserId
        {
            get
            {
                if (CurrentUser == null) return 0;
                return CurrentUser.Id;
            }
        }

        /// <summary> 是否是扫描套打员 </summary>
        public static bool IsScanner
        {
            get
            {
                if (CurrentUser == null) return false;
                return (CurrentUser.Role & 64) > 0;
            }
        }

        public static string Token = string.Empty;

        public static string DisplayName
        {
            get
            {
                if (CurrentUser == null) return string.Empty;
                if (!string.IsNullOrWhiteSpace(CurrentUser.Name))
                    return CurrentUser.Name;
                if (!string.IsNullOrWhiteSpace(CurrentUser.Nick))
                    return CurrentUser.Nick;
                return string.Empty;
            }
        }

        public static DResult<MPaperDto> GetPaperInfo(string paperNum)
        {
            MPaperDto info;
            if (PaperInfoDicts.ContainsKey(paperNum))
            {
                info = PaperInfoDicts[paperNum].JsonToObject<MPaperDto>();
            }
            else
            {
                var result = RestHelper.Instance.LoadPaperByNum(paperNum);
                if (!result.Status)
                {
                    return DResult.Error<MPaperDto>(result.Message);
                }
                using (var utils = new CacheUtils())
                {
                    utils.Set(paperNum, CacheType.PaperNum);
                }
                info = result.Data;
                PaperInfoDicts.Add(paperNum, info.ToJson());
            }
            return DResult.Succ(info);
        }

        public static DResult<MStudentListDto> Students(string code)
        {
            if (StudentListCache.ContainsKey(code))
            {
                return DResult.Succ(StudentListCache[code]);
            }
            var students = RestHelper.Instance.StudentList(code);
            if (!students.Status)
                return students;
            StudentListCache.Add(code, students.Data);
            return students;
        }

        public static DResult<StudentDto> Student(string code)
        {
            if (StudentCache.ContainsKey(code))
            {
                return DResult.Succ(StudentCache[code]);
            }
            var studentResult = RestHelper.Instance.Student(code);
            if (!studentResult.Status)
                return studentResult;
            StudentCache.Add(code, studentResult.Data);
            return studentResult;
        }

        /// <summary>
        /// 是否64位
        /// </summary>
        /// <returns></returns>
        public static bool Is64BitProcess()
        {
            return IntPtr.Size == 8;
        }
    }
}
