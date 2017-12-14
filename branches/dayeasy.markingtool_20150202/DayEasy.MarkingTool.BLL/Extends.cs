using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace DayEasy.MarkingTool.BLL
{
    public static class Extends
    {
        /// <summary>
        /// 字符串格式化
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="arg0">参数0</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string str, object arg0)
        {
            return string.Format(str, arg0);
        }

        /// <summary>
        /// 字符串格式化
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="arg0">参数0</param>
        /// <param name="arg1">参数1</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string str, object arg0, object arg1)
        {
            return string.Format(str, arg0, arg1);
        }

        /// <summary>
        /// 字符串格式化
        /// </summary>
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
            var serializer = new JavaScriptSerializer();
            var section =
                ConfigurationManager.GetSection("system.web.extensions/scripting/webServices/jsonSerialization")
                    as ScriptingJsonSerializationSection;
            if (section != null)
            {
                serializer.MaxJsonLength = section.MaxJsonLength;
                serializer.RecursionLimit = section.RecursionLimit;
            }
            return serializer.Deserialize<T>(json);
        }

        /// <summary>
        /// 将obj转换为json字符
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            var serializer = new JavaScriptSerializer {MaxJsonLength = Int32.MaxValue};
            var section =
                ConfigurationManager.GetSection("system.web.extensions/scripting/webServices/jsonSerialization")
                    as ScriptingJsonSerializationSection;
            if (section != null)
            {
                serializer.MaxJsonLength = section.MaxJsonLength;
                serializer.RecursionLimit = section.RecursionLimit;
            }
            return serializer.Serialize(obj);
        }

        public static T JsonToObject2<T>(this string json, Encoding encoding)
        {
            //DateTime 转换
            var reg = new Regex("(\\d{4}-\\d{2}-\\d{2}T\\d{1,2}:\\d{1,2}:\\d{1,2}(\\.\\d{1,3})?)[^\\.\\d]");
            if (reg.IsMatch(json))
            {
                foreach (Match match in reg.Matches(json))
                {
                    DateTime dt;
                    if (DateTime.TryParse(match.Groups[1].Value.Replace("T", " "), out dt))
                    {
                        json = json.Replace(match.Groups[1].Value, dt.ToJson2().Replace("\"", ""));
                    }
                }
            }
            var ser = new DataContractJsonSerializer(typeof (T));
            using (var mStream = new MemoryStream(encoding.GetBytes(json)))
            {
                return (T) ser.ReadObject(mStream);
            }
        }

        public static T JsonToObject2<T>(this string json)
        {
            return json.JsonToObject2<T>(Encoding.UTF8);
        }

        public static string ToJson2(this object obj, Encoding encoding)
        {
            var ser = new DataContractJsonSerializer(obj.GetType());
            using (var stream = new MemoryStream())
            {
                ser.WriteObject(stream, obj);
                var dataBytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(dataBytes, 0, (int) stream.Length);
                return encoding.GetString(dataBytes);
            }
        }

        public static string ToJson2(this object obj)
        {
            return obj.ToJson2(Encoding.UTF8);
        }

        public static T ObjectToT<T>(this object obj, T def)
        {
            try
            {
                var type = typeof (T);
                if (type.Name == "Nullable`1")
                    type = type.GetGenericArguments()[0];
                return (T) Convert.ChangeType(obj, type, CultureInfo.InvariantCulture);
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
            var type = typeof (T);

            if (type.IsArray || type.Name == "List`1")
            {
                try
                {
                    Type st = typeof (string);
                    bool isList = false;
                    if (type.IsArray)
                        st = Type.GetType(type.FullName.TrimEnd('[', ']'));
                    else if (type.Name == "List`1")
                    {
                        isList = true;
                        var reg = Regex.Match(type.FullName, "System.Collections.Generic.List`1\\[\\[([^,]+),");
                        st = Type.GetType(reg.Groups[1].Value);
                    }
                    var arr = str.Split(new[] {splitor}, StringSplitOptions.RemoveEmptyEntries);
                    if (st != typeof (string) && st != null)
                    {
                        if (st == typeof (int))
                        {
                            var rt = Array.ConvertAll(arr, s => s.ObjectToT(0));
                            return (isList ? (T) (object) rt.ToList() : (T) (object) rt);
                        }
                        if (st == typeof (double))
                        {
                            var rt = Array.ConvertAll(arr, s => s.ObjectToT(0.0));
                            return (isList ? (T) (object) rt.ToList() : (T) (object) rt);
                        }
                        if (st == typeof (decimal))
                        {
                            var rt = Array.ConvertAll(arr, s => s.ObjectToT(0M));
                            return (isList ? (T) (object) rt.ToList() : (T) (object) rt);
                        }
                        if (st == typeof (float))
                        {
                            var rt = Array.ConvertAll(arr, s => s.ObjectToT(0F));
                            return (isList ? (T) (object) rt.ToList() : (T) (object) rt);
                        }
                        if (st == typeof (DateTime))
                        {
                            var rt = Array.ConvertAll(arr, s => s.ObjectToT(DateTime.MinValue));
                            return (isList ? (T) (object) rt.ToList() : (T) (object) rt);
                        }
                    }
                    return (isList ? (T) (object) arr.ToList() : (T) (object) arr);
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
                bool exchange = false;

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
    }
}
