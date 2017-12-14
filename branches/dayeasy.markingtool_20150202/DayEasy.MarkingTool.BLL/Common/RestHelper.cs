
using System;
using System.Text;
using DayEasy.MarkingTool.BLL.Entity;
using DayEasy.MarkingTool.BLL.Entity.Marking;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using DayEasy.Open.Model.Marking;
using DayEasy.Open.Model.Paper;
using DayEasy.Open.Model.Question;
using DayEasy.Open.Model.System;
using DayEasy.Open.Model.User;
using System.Collections.Generic;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary>
    /// 接口辅助类
    /// </summary>
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

        public JsonResults<SubjectInfo> GetSubjectInfos()
        {
            return _manager.GetResult<JsonResults<SubjectInfo>>(new
            {
                method = DeyiKeys.MSubjects
            }.ToDictionary());
        }

        public JsonResult<ManifestInfo> Manifest()
        {
            return _manager.GetResult<JsonResult<ManifestInfo>>(new
            {
                method = DeyiKeys.MSystemManifest,
                type = 20
            }.ToDictionary());
        }

        public JsonResult<LoginInfo> Login(string account, string password)
        {
            return _manager.GetResult<JsonResult<LoginInfo>>(new
            {
                method = DeyiKeys.MUserLogin,
                email = account,
                password
            }.ToDictionary(), HttpMethod.Post);
        }

        public JsonResult<UserInfo> LoadUserInfo(string token)
        {
            return _manager.GetResult<JsonResult<UserInfo>>(new
            {
                method = DeyiKeys.MLoadUserInfo,
                token
            }.ToDictionary());
        }

        public JsonResult<QuestionInfo> LoadQuestion(string questionId)
        {
            return _manager.GetResult<JsonResult<QuestionInfo>>(new
            {
                method = DeyiKeys.MQuestionLoad,
                question_id = questionId
            }.ToDictionary());
        }

        public JsonResults<QuestionInfo> LoadQuestions(IEnumerable<string> questionIds)
        {
            return _manager.GetResult<JsonResults<QuestionInfo>>(new
            {
                method = DeyiKeys.MQuestionsLoad,
                question_ids = questionIds.ToJson()
            }.ToDictionary());
        }

        public JsonResult<PaperInfo> LoadPaper(string paperId)
        {
            var result = _manager.GetResult<JsonResult<PaperInfo>>(new
            {
                method = DeyiKeys.MPaperLoad,
                paper_id = paperId
            }.ToDictionary());
            return result;
        }

        public JsonResult<PaperUsageInfo> LoadPaperUsage(string batchNo)
        {
            var result = _manager.GetResult<JsonResult<PaperUsageInfo>>(new
            {
                method = DeyiKeys.MPaperUsage,
                batch = batchNo
            }.ToDictionary());
            return result;
        }

        public FileResult UpdateFile(PaperMarkingFileData fileData)
        {
            return _manager.UploadFile(fileData.Data);
        }

        public JsonResults<MarkedResult> MarkingResult(List<MarkingResult> results)
        {
            try
            {
                var json = new StringBuilder();
                json.Append("[");
                foreach (var result in results)
                {
                    json.Append(result.ToJson());
                    json.Append(",");
                }
                json.Remove(json.Length - 1, 1);
                json.Append("]");
                _logger.I(json.ToString());
                return _manager.GetResult<JsonResults<MarkedResult>>(new
                {
                    method = DeyiKeys.MPaperMarking,
                    results = json.ToString()
                }.ToDictionary(), HttpMethod.Post);
            }
            catch (Exception ex)
            {
                _logger.E("阅卷上传", ex);
                return new JsonResults<MarkedResult>("网络服务器请求异常！");
            }
        }

        public JsonResults<PrintUsageInfo> PrintUsages(bool isClose, int index, int size)
        {
            return _manager.GetResult<JsonResults<PrintUsageInfo>>(new
            {
                method = DeyiKeys.MPaperPrintUsages,
                isClose,
                index,
                size
            }.ToDictionary(), HttpMethod.Post);
        }

        public JsonResultBase CloseUsage(string batch)
        {
            return _manager.GetResult<JsonResultBase>(new
            {
                method = DeyiKeys.MPaperCloseUsage,
                batch
            }.ToDictionary(), HttpMethod.Post);
        }
    }
}