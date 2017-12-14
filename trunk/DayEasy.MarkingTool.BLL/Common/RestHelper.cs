using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.Models.Open.Group;
using DayEasy.Models.Open.Paper;
using DayEasy.Models.Open.System;
using DayEasy.Models.Open.User;
using DayEasy.Models.Open.Work;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary> 接口辅助类 </summary>
    public class RestHelper
    {
        private readonly HttpManager _manager;
        private readonly Logger _logger = Logger.L<RestHelper>();

        private RestHelper()
        {
            _manager = HttpManager.Instance;
        }

        public static RestHelper Instance
        {
            get { return (Singleton<RestHelper>.Instance ?? (Singleton<RestHelper>.Instance = new RestHelper())); }
        }

        /// <summary> 获取科目列表 </summary>
        public DResults<MSubjectDto> GetSubjectInfos()
        {
            return _manager.GetResult<DResults<MSubjectDto>>(new
            {
                method = DeyiKeys.MSubjects
            });
        }

        /// <summary> 获取更新配置 </summary>
        public DResult<MManifestDto> Manifest()
        {
            return _manager.GetResult<DResult<MManifestDto>>(new
            {
                method = DeyiKeys.MSystemManifest,
                type = 20
            });
        }

        /// <summary> 用户登录 </summary>
        /// <param name="account">帐号</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public DResult<MLoginDto> Login(string account, string password)
        {
            return _manager.GetResult<DResult<MLoginDto>>(new
            {
                method = DeyiKeys.MUserLogin,
                account,
                password = password.EncodePwd()
            }, HttpMethod.Post);
        }

        /// <summary> 获取用户信息 </summary>
        /// <returns></returns>
        public DResult<MUserDto> LoadUserInfo()
        {
            return _manager.GetResult<DResult<MUserDto>>(new
            {
                method = DeyiKeys.MLoadUserInfo
            });
        }

        /// <summary> 学生信息 </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public DResult<StudentDto> Student(string code)
        {
            return _manager.GetResult<DResult<StudentDto>>(new
            {
                method = DeyiKeys.MStudent,
                code
            });
        }

        public DResults<StudentClassDto> StudentSearch(string name)
        {
            return _manager.GetResult<DResults<StudentClassDto>>(new
            {
                method = DeyiKeys.MStudentSearch,
                keyword = name
            });
        }

        public DResult<MPaperDto> LoadPaperByNum(string paperNum)
        {
            var result = _manager.GetResult<DResult<MPaperDto>>(new
            {
                method = DeyiKeys.MPaperInfo,
                keyword = paperNum
            });
            return result;
        }

        public DResults<MGroupDto> TeacherGroups()
        {
            var result = _manager.GetResult<DResults<MGroupDto>>(new
            {
                method = DeyiKeys.MTeacherGroups,
                type = 0
            });
            return result;
        }

        public DResult<MStudentListDto> StudentList(string code)
        {
            var result = _manager.GetResult<DResult<MStudentListDto>>(new
            {
                method = DeyiKeys.MGroupStudents,
                code
            });
            return result;
        }

        /// <summary> 上传文件 </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        public FileResult UpdateFile(PaperMarkingFileData fileData)
        {
            return _manager.UploadFile(fileData.Data);
        }

        /// <summary> 协同批次 </summary>
        /// <param name="paperId"></param>
        /// <returns></returns>
        public DResults<MJointUsageDto> JointUsages(string paperId)
        {
            return _manager.GetResult<DResults<MJointUsageDto>>(new
            {
                method = DeyiKeys.MWorkJointUsages,
                paperId
            });
        }

        public DResults<MHandinResult> HandinPictures(MPictureList results)
        {
            try
            {
                var json = results.ToJson();
                return _manager.GetResult<DResults<MHandinResult>>(new
                {
                    method = DeyiKeys.MPaperHandinPictures,
                    results = json
                }, HttpMethod.Post);
            }
            catch (Exception ex)
            {
                _logger.E("扫描图片上传", ex);
                return DResult.Errors<MHandinResult>("网络服务器请求异常！");
            }
        }

        /// <summary> 套打列表 </summary>
        /// <param name="paperId"></param>
        /// <returns></returns>
        public DResults<PrintBatchInfo> BatchPrints(string paperId)
        {
            return _manager.GetResult<DResults<PrintBatchInfo>>(new
            {
                method = DeyiKeys.MWorkBatchPrint,
                paper_id = paperId
            });
        }

        /// <summary> 套打详情 </summary>
        /// <param name="batch">批次号</param>
        /// <param name="type"></param>
        /// <param name="skip"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public DResults<PrintBatchDetail> PrintDetails(string batch, byte type = 0, int skip = 0, int size = 100)
        {
            return _manager.GetResult<DResults<PrintBatchDetail>>(new
            {
                method = DeyiKeys.MWorkPrintDetails,
                batch,
                type,
                skip,
                size
            });
        }

        /// <summary> 协同套打列表 </summary>
        /// <param name="paperId"></param>
        /// <returns></returns>
        public DResults<JointPrintInfo> JointPrintList(string paperId)
        {
            return _manager.GetResult<DResults<JointPrintInfo>>(new
            {
                method = DeyiKeys.MWorkJointPrintList,
                paper_id = paperId
            });
        }

        /// <summary> 协同机构 </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        public DResult<Dictionary<string, string>> JointAgencies(string joint)
        {
            return _manager.GetResult<DResult<Dictionary<string, string>>>(new
            {
                method = DeyiKeys.MWorkJointAgencies,
                joint
            });
        }

        /// <summary> 协同套打详情 </summary>
        /// <param name="joint">批次号</param>
        /// <param name="type"></param>
        /// <param name="skip"></param>
        /// <param name="size"></param>
        /// <param name="agencyId"></param>
        /// <returns></returns>
        public DResults<PrintBatchDetail> JointPrintDetails(string joint, byte type = 0, int skip = 0, int size = 50, string agencyId = null)
        {
            return _manager.GetResult<DResults<PrintBatchDetail>>(new
            {
                method = DeyiKeys.MWorkJointPrintDetails,
                joint,
                type,
                skip,
                size,
                agencyId
            });
        }

        public void SendEmailAsync(string title, Exception ex)
        {
            var msg = Helper.Format(ex);
            SendEmailAsync(msg, title);
        }

        public void SendEmailAsync(string body, string title)
        {
            var receiver = DeyiKeys.MarkingConfig.AdminEmail;
            if (string.IsNullOrWhiteSpace(receiver))
                return;
            title = "扫描工具 - " + title;
            Task.Factory.StartNew(() =>
            {
                _manager.GetResult<DResult>(
                    new
                    {
                        method = DeyiKeys.MMessageEmail,
                        receiver = DeyiKeys.MarkingConfig.AdminEmail,
                        body,
                        title
                    }, HttpMethod.Post);
            });
        }
    }
}