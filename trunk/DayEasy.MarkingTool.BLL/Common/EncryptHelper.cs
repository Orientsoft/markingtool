using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DayEasy.MarkingTool.BLL.Common
{
    public static class EncryptHelper
    {
        private const string Key = "e9dv3n1t";
        private const string Iv = "ac3f6n2d";

        private static string Encode(string data, string key, string iv)
        {
            try
            {
                var byKey = Encoding.UTF8.GetBytes(key);
                var byIv = Encoding.UTF8.GetBytes(iv);
                var dataByte = Encoding.UTF8.GetBytes(data);

                using (var des = new DESCryptoServiceProvider())
                {
                    using (var ms = new MemoryStream())
                    {
                        using (
                            var cst = new CryptoStream(ms, des.CreateEncryptor(byKey, byIv),
                                CryptoStreamMode.Write))
                        {
                            cst.Write(dataByte, 0, dataByte.Length);
                            cst.FlushFinalBlock();
                            var msg = Convert.ToBase64String(ms.ToArray());
                            msg = msg.Replace("+", "-");
                            msg = msg.Replace("/", "_");
                            msg = msg.Replace("=", "~");
                            return msg.UrlEncode(Encoding.UTF8);
                        }
                    }
                }
            }
            catch
            {
                return data;
            }
        }

        public static string EncodePwd(this string pwd)
        {
            return string.IsNullOrWhiteSpace(pwd) ? pwd : Encode(pwd, Key, Iv);
        }
    }
}
