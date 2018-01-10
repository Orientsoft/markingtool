using DayEasy.MarkingTool.BLL.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace DayEasy.MarkingTool.BLL
{
    /// <summary> 扩展方法 </summary>
    public static class Extends
    {
        /// <summary> 字符串格式化 </summary>
        /// <param name="str">字符串</param>
        /// <param name="arg0">参数0</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string str, object arg0)
        {
            return string.Format(str, arg0);
        }

        /// <summary> 字符串格式化 </summary>
        /// <param name="str">字符串</param>
        /// <param name="arg0">参数0</param>
        /// <param name="arg1">参数1</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string str, object arg0, object arg1)
        {
            return string.Format(str, arg0, arg1);
        }

        /// <summary> 字符串格式化 </summary>
        /// <param name="str">字符串</param>
        /// <param name="arg0">参数0</param>
        /// <param name="arg1">参数1</param>
        /// <param name="arg2">参数2</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string str, object arg0, object arg1, object arg2)
        {
            return string.Format(str, arg0, arg1, arg2);
        }

        /// <summary>
        /// 字符串格式化
        /// </summary>
        /// <param name="str"></param>
        /// <param name="args">参数集</param>
        /// <returns></returns>
        public static string FormatWith(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        public static string UrlEncode(this string str, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return HttpUtility.UrlEncode(str, encoding ?? Encoding.UTF8);
        }

        public static string UrlEncode(this string str)
        {
            return str.UrlEncode(null);
        }

        public static string AppendFileName(this string origin, string append)
        {
            var result = origin.Split('.');
            return string.Format("{0}{1}.{2}", result[0], append, result[1]);
        }

        public static string UrlDecode(this string str, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return HttpUtility.UrlDecode(str, encoding ?? Encoding.UTF8);
        }

        public static string UrlDecode(this string str)
        {
            return str.UrlDecode(null);
        }

        /// <summary>
        /// json字符串转换为obj
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(this string json)
        {
            return JsonHelper.Json<T>(json, NamingType.CamelCase);
        }

        /// <summary>
        /// 将obj转换为json字符
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            return JsonHelper.ToJson(obj, NamingType.CamelCase);
        }

        public static T ObjectToT<T>(this object obj, T def)
        {
            try
            {
                var type = typeof(T);
                if (type.Name == "Nullable`1")
                    type = type.GetGenericArguments()[0];
                return (T)Convert.ChangeType(obj, type, CultureInfo.InvariantCulture);
            }
            catch
            {
                return def;
            }
        }

        public static T ObjectToT<T>(this object obj)
        {
            return obj.ObjectToT(default(T));
        }

        /// <summary>
        /// 获取该值的MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Md5(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            var md5 = new MD5CryptoServiceProvider();
            byte[] md5Source = Encoding.UTF8.GetBytes(str);
            byte[] md5Out = md5.ComputeHash(md5Source);
            string userPwdMd5 = BitConverter.ToString(md5Out).Replace("-", "");
            return userPwdMd5.ToUpper();
        }

        public static T To<T>(this string str, T def = default(T), string splitor = ",")
        {
            var type = typeof(T);

            if (type.IsArray || type.Name == "List`1")
            {
                try
                {
                    Type st = typeof(string);
                    bool isList = false;
                    if (type.IsArray)
                        st = Type.GetType(type.FullName.TrimEnd('[', ']'));
                    else if (type.Name == "List`1")
                    {
                        isList = true;
                        var reg = Regex.Match(type.FullName, "System.Collections.Generic.List`1\\[\\[([^,]+),");
                        st = Type.GetType(reg.Groups[1].Value);
                    }
                    var arr = str.Split(new[] { splitor }, StringSplitOptions.RemoveEmptyEntries);
                    if (st != typeof(string) && st != null)
                    {
                        if (st == typeof(int))
                        {
                            var rt = Array.ConvertAll(arr, s => s.ObjectToT(0));
                            return (isList ? (T)(object)rt.ToList() : (T)(object)rt);
                        }
                        if (st == typeof(double))
                        {
                            var rt = Array.ConvertAll(arr, s => s.ObjectToT(0.0));
                            return (isList ? (T)(object)rt.ToList() : (T)(object)rt);
                        }
                        if (st == typeof(decimal))
                        {
                            var rt = Array.ConvertAll(arr, s => s.ObjectToT(0M));
                            return (isList ? (T)(object)rt.ToList() : (T)(object)rt);
                        }
                        if (st == typeof(float))
                        {
                            var rt = Array.ConvertAll(arr, s => s.ObjectToT(0F));
                            return (isList ? (T)(object)rt.ToList() : (T)(object)rt);
                        }
                        if (st == typeof(DateTime))
                        {
                            var rt = Array.ConvertAll(arr, s => s.ObjectToT(DateTime.MinValue));
                            return (isList ? (T)(object)rt.ToList() : (T)(object)rt);
                        }
                    }
                    return (isList ? (T)(object)arr.ToList() : (T)(object)arr);
                }
                catch
                {
                    return def;
                }
            }
            return str.ObjectToT(def);
        }

        /// <summary>
        /// 冒泡排序法
        /// 按照字母序列从a到z的顺序排列
        /// </summary>
        public static string[] BubbleSort(this string[] array)
        {
            //交换标志 
            int i;
            //最多做R.Length-1趟排序
            for (i = 0; i < array.Length; i++)
            {
                var exchange = false;

                int j; //交换标志 
                for (j = array.Length - 2; j >= i; j--)
                {
                    //交换条件
                    if (String.CompareOrdinal(array[j + 1], array[j]) < 0)
                    {
                        string temp = array[j + 1];
                        array[j + 1] = array[j];
                        array[j] = temp;
                        //发生了交换，故将交换标志置为真
                        exchange = true;
                    }
                }
                //本趟排序未发生交换，提前终止算法
                if (!exchange)
                {
                    break;
                }
            }
            return array;
        }

        public static IDictionary<string, string> ToDictionary(this object source)
        {
            var type = source.GetType();
            var dict = new Dictionary<string, string>();
            if (!type.IsValueType)
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    dict.Add(prop.Name, prop.GetValue(source, null).ToString());
                }
            }
            return dict;
        }

        public static bool IsBlack(this Color color)
        {
            return color.R + color.G + color.B == 0;
        }

        private static DateTime _defaultTime = new DateTime(1970, 1, 1);

        public static DateTime ToDateTime(this long time)
        {
            if (time <= 0) return _defaultTime;
            return new DateTime(_defaultTime.Ticks + time * 10000);
        }

        public static string ToAnswer(this int[] sheet)
        {
            return sheet.Where(c => c >= 0 && c < 26)
                .Aggregate(string.Empty, (c, t) => c + DeyiKeys.OptionWords[t]);
        }

        public static string ToWord(this IList<int[]> choose)
        {
            if (choose == null) return string.Empty;
            var words = new List<string>();
            foreach (var ch in choose)
            {
                if (!ch.Any() || ch.All(c => c < 0))
                    words.Add(string.Empty);
                else
                    words.Add(ch.ToAnswer());
            }
            return string.Join(",", words);
        }

        public static IList<int[]> ToSheet(this string words)
        {
            if (string.IsNullOrWhiteSpace(words))
                return new List<int[]>();
            var list = words.Split(',');
            var sheets = new List<int[]>();
            var options = DeyiKeys.OptionWords.ToList();
            foreach (var s in list)
            {
                if (string.IsNullOrWhiteSpace(s))
                    sheets.Add(new[] { -1 });
                else
                    sheets.Add(s.Select(t => options.IndexOf(t.ToString().ToUpper())).ToArray());
            }
            return sheets;
        }

        /// <summary> 小驼峰命名法 </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (!char.IsUpper(s[0]))
                return s;

            var chars = s.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                var hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                    break;
                chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
            }

            return new string(chars);
        }

        /// <summary> url命名法 </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToUrlCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            var chars = s.ToCharArray();
            var str = new StringBuilder();
            for (var i = 0; i < chars.Length; i++)
            {
                if (char.IsUpper(chars[i]))
                {
                    if (i > 0 && !char.IsUpper(chars[i - 1]))
                        str.Append("_");
                    str.Append(char.ToLower(chars[i], CultureInfo.InvariantCulture));
                }
                else
                {
                    str.Append(chars[i]);
                }
            }
            return str.ToString();
        }

        public static double WatchLog(this Stopwatch watch, bool restart = true)
        {
            watch.Stop();
            var time = watch.ElapsedMilliseconds / 1000D;
            if (restart)
                watch.Restart();
            return time;
        }
    }
}
