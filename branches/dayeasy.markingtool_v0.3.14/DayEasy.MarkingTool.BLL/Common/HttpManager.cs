using System.Runtime.InteropServices;
using DayEasy.MarkingTool.BLL.Entity.Paper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DayEasy.MarkingTool.BLL.Common
{
    /// <summary>
    /// Http请求管理器
    /// </summary>
    internal class HttpManager
    {
        private readonly Logger _logger = Logger.L<HttpManager>();
        private readonly MarkingConfig _config;

        private HttpManager()
        {
            if (SectionManager.Instance.Section != null)
                _config = SectionManager.Instance.Section.MarkingConfig;
        }

        internal static HttpManager Instance
        {
            get { return Singleton<HttpManager>.Instance ?? (Singleton<HttpManager>.Instance = new HttpManager()); }
        }

        private string SignPartner(IDictionary<string, string> paras)
        {
            if (paras.Keys.Count == 0 || _config == null || string.IsNullOrWhiteSpace(_config.RouteUrl))
                return string.Empty;
            var except = new[] {"partner", "sign"};
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
            if (_config == null || string.IsNullOrWhiteSpace(_config.RouteUrl))
                return string.Empty;
            if (!string.IsNullOrWhiteSpace(DeyiApp.Token) && !paras.ContainsKey("token"))
                paras.Add("token", DeyiApp.Token);
            if (!paras.ContainsKey("tick"))
            {
                using (var tickHttp = new HttpHelper(_config.RouteUrl.Replace("router", "ticks")))
                {
                    paras.Add("tick", tickHttp.GetHtml());
                }
            }
            var url = SignPartner(paras);
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            HttpHelper http = (method == HttpMethod.Get
                ? new HttpHelper(_config.RouteUrl + "?" + url, Encoding.UTF8)
                : new HttpHelper(_config.RouteUrl, "POST", Encoding.UTF8, url));
            //Console.Write(_config.RouteUrl + "?" + url);
//#if DEBUG
//            _logger.I(_config.RouteUrl + "?" + url);
//#endif
            using (http)
            {
                return http.GetHtml();
            }
        }

        internal T GetResult<T>(IDictionary<string, string> paras, HttpMethod method = HttpMethod.Get)
            where T : JsonResultBase
        {
            var html = GetHttpResult(paras, method);
            try
            {
                if (string.IsNullOrWhiteSpace(html) || !(new Regex("[\\{\\[]")).IsMatch(html))
                    return new JsonResultBase(string.IsNullOrWhiteSpace(html) ? "未获得接口数据" : html) as T;
                return html.JsonToObject2<T>();
            }
            catch
            {
                return new JsonResultBase(html) as T;
            }
        }

        internal FileResult UploadFile(FileStream stream)
        {
            using (var http = new HttpHelper(_config.FileUrl, "POST", Encoding.UTF8, string.Empty))
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

    public enum HttpMethod
    {
        Get = 0,
        Post = 1
    }
}