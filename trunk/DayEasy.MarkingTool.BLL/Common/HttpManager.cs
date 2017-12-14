using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DayEasy.MarkingTool.BLL.Config;
using DayEasy.MarkingTool.BLL.Entity.Paper;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary> Http请求管理器 </summary>
    internal class HttpManager
    {
        private readonly Logger _logger = Logger.L<HttpManager>();
        private readonly MarkingConfig _config;

        private HttpManager()
        {
            _config = DeyiKeys.MarkingConfig;
        }

        internal static HttpManager Instance
        {
            get { return Singleton<HttpManager>.Instance ?? (Singleton<HttpManager>.Instance = new HttpManager()); }
        }

        /// <summary>
        /// 参数签名
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        private string SignPartner(IDictionary<string, string> paras)
        {
            if (paras.Keys.Count == 0 || _config == null || string.IsNullOrWhiteSpace(_config.Rest))
                return string.Empty;
            var except = new[] { "partner", "sign" };
            var keys = paras.Keys.Except(except).ToArray().BubbleSort();
            string sign = string.Empty,
                url = string.Empty;
            sign = keys.Aggregate(sign,
                (current, reqKey) => current + (reqKey + "=" + paras[reqKey].UrlDecode() + "&"));
            url = keys.Aggregate(url,
                (current, reqKey) => current + (reqKey + "=" + paras[reqKey].UrlEncode() + "&"));
            sign = (sign.TrimEnd('&') + "+" + _config.SecretKey).Md5().ToLower();
            url += "partner=" + _config.Partner + "&sign=" + sign;
            return url;
        }

        /// <summary>
        /// 获取Api接口结果
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private string GetHttpResult(IDictionary<string, string> paras, HttpMethod method = HttpMethod.Get)
        {
            if (_config == null || string.IsNullOrWhiteSpace(_config.Rest))
                return string.Empty;
            if (!string.IsNullOrWhiteSpace(DeyiApp.Token) && !paras.ContainsKey("token"))
                paras.Add("token", DeyiApp.Token);
            var url = _config.Rest + "v3/" + paras["method"];
            paras.Remove("method");
            if (!paras.ContainsKey("tick"))
            {
                using (var tickHttp = new HttpHelper(_config.Rest + "ticks"))
                {
                    paras.Add("tick", tickHttp.GetHtml());
                }
            }
            var signParams = SignPartner(paras);
            if (string.IsNullOrWhiteSpace(signParams)) return string.Empty;
            HttpHelper http = (method == HttpMethod.Get
                ? new HttpHelper(url + "?" + signParams, Encoding.UTF8)
                : new HttpHelper(url, "POST", Encoding.UTF8, signParams));

            using (http)
            {
                return http.GetHtml();
            }
        }

        /// <summary>
        /// 获取API接口返回的实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        internal T GetResult<T>(object paras, HttpMethod method = HttpMethod.Get)
            where T : DResult
        {
            var html = GetHttpResult(paras.ToDictionary(), method);
            try
            {
                if (string.IsNullOrWhiteSpace(html) || !(new Regex("[\\{\\[]")).IsMatch(html))
                    return new DResult(string.IsNullOrWhiteSpace(html) ? "未获得接口数据" : html) as T;
                return html.JsonToObject<T>();
            }
            catch
            {
                return html.JsonToObject<DResult>() as T;
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal FileResult UploadFile(FileStream stream)
        {
            using (var http = new HttpHelper(_config.RestFile, "POST", Encoding.UTF8, string.Empty))
            {
                http.AddFiles(new Dictionary<string, Stream>
                {
                    {stream.Name, stream}
                });
                var html = http.GetHtml();
                if (string.IsNullOrWhiteSpace(html) || !(new Regex("[\\{\\[]")).IsMatch(html))
                    return new FileResult
                    {
                        state = 0,
                        msg = html
                    };
                return html.JsonToObject<FileResult>();
            }
        }
    }

    /// <summary> Http请求方式 </summary>
    public enum HttpMethod
    {
        Get = 0,
        Post = 1
    }
}