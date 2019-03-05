using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Aden.Util.Common
{
    public static class SecurityHelper
    {
        #region MD5加密

        /// <summary>
        /// 返回 System.String 对象进行MD5加密后的32字符十六进制格式字符串
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <param name="encode">编码</param>
        /// <returns>返回加密字符串</returns>
        public static string ToMD5(this string str, Encoding encode)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(encode.GetBytes(str));
                StringBuilder sb = new StringBuilder();
                foreach (var i in bytes)
                {
                    sb.Append(i.ToString("x2"));
                }
                return sb.ToString().ToUpper();
            }
        }

        /// <summary>
        /// 返回 System.String 对象进行MD5加密后的32字符十六进制格式字符串,默认编码UTF8
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>返回加密字符串</returns>
        public static string ToMD5(this string str)
        {
            return str.ToMD5(Encoding.UTF8);
        }

        /// <summary>
        /// 返回 System.String 对象进行MD5加密后的16字符十六进制格式字符串
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <param name="encode">编码</param>
        /// <returns>返回加密字符串</returns>
        public static string ToMD5Bit16(this string str, Encoding encode)
        {
            return str.ToMD5(encode).Substring(8, 16);
        }

        /// <summary>
        /// 返回 System.String 对象进行MD5加密后的16字符十六进制格式字符串,默认编码GB2312
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>返回加密字符串</returns>
        public static string ToMD5Bit16(this string str)
        {
            return str.ToMD5Bit16(Encoding.UTF8);
        }

        #endregion

        #region Base64加密
        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="str">待加密字符串</param>
        /// <param name="encode">字符编码，默认UTD-8</param>
        /// <returns></returns>
        public static string ToBase64Encode(this string str, string encode = "UTF-8")
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return Convert.ToBase64String(Encoding.GetEncoding(encode).GetBytes(str));
        }
        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="str">待解密字符串</param>
        /// <param name="encode">字符编码，默认UTD-8</param>
        /// <returns></returns>
        public static string FromBase64String(this string str, string encode = "UTF-8")
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            try
            {
                return Encoding.GetEncoding(encode).GetString(Convert.FromBase64String(str));
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region DES加解密

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="text">需解密的密文</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string ToDesDecrypt(this string str, string key = "adenservices")
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                byte[] strBytes = new byte[str.Length / 2];
                for (int x = 0; x < str.Length / 2; x++)
                {
                    int i = (Convert.ToInt32(str.Substring(x * 2, 2), 16));
                    strBytes[x] = (byte)i;
                }
                if (key.Length >= des.Key.Length)
                    des.Key = ASCIIEncoding.UTF8.GetBytes(key.Substring(0, des.Key.Length));
                else
                {
                    int len = des.Key.Length - key.Length;
                    while (len-- > 0) { key += 'a'; }
                    des.Key = ASCIIEncoding.UTF8.GetBytes(key);
                }
                des.IV = des.Key;
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(strBytes, 0, strBytes.Length);
                cs.FlushFinalBlock();
                return System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="str">需加密的明文文本</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string ToDesEncrypt(this string str, string key = "adenservices")
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            using (DESCryptoServiceProvider provider = new DESCryptoServiceProvider())
            {
                byte[] strBytes = Encoding.UTF8.GetBytes(str);
                if (key.Length >= provider.Key.Length)
                    provider.Key = ASCIIEncoding.UTF8.GetBytes(key.Substring(0, provider.Key.Length));
                else
                {
                    int len = provider.Key.Length - key.Length;
                    while (len-- > 0) { key += 'a'; }
                    provider.Key = ASCIIEncoding.UTF8.GetBytes(key);
                }
                provider.IV = provider.Key;
                MemoryStream ms = new MemoryStream();
                CryptoStream stream = new CryptoStream(ms, provider.CreateEncryptor(), CryptoStreamMode.Write);
                stream.Write(strBytes, 0, strBytes.Length);
                stream.FlushFinalBlock();
                byte[] data = ms.ToArray();
                StringBuilder tempStringBuilder = new StringBuilder(data.Length);
                foreach (byte b in ms.ToArray())
                    tempStringBuilder.AppendFormat("{0:X2}", b);
                return tempStringBuilder.ToString();
            }
        }
        #endregion

        #region RSA加解密
        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="encryptString">需要加密的文本</param>
        /// <param name="xmlPrivateKey">加密私钥</param>
        /// <returns>RSA公钥加密后的数据</returns>
        public static string ToRSAEncrypt(this string encryptString, string xmlPrivateKey)
        {
            string result;
            try
            {
                RSACryptoServiceProvider.UseMachineKeyStore = true;
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(xmlPrivateKey);
                byte[] bytes = new UnicodeEncoding().GetBytes(encryptString);
                result = Convert.ToBase64String(provider.Encrypt(bytes, false));
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return result;
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="decryptString">需要解密的文本</param>
        /// <param name="xmlPublicKey">解密公钥</param>
        /// <returns>解密后的数据</returns>
        public static string ToRSADecrypt(this string decryptString, string xmlPublicKey)
        {
            string result;
            try
            {
                RSACryptoServiceProvider.UseMachineKeyStore = true;
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(xmlPublicKey);
                byte[] rgb = Convert.FromBase64String(decryptString);
                byte[] buffer2 = provider.Decrypt(rgb, false);
                result = new UnicodeEncoding().GetString(buffer2);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return result;
        }

        /// <summary>
        /// 生成公钥、私钥
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <param name="publicKey">公钥</param>
        public static void CreateKey(out string privateKey, out string publicKey)
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            privateKey = provider.ToXmlString(true);
            publicKey = provider.ToXmlString(false);
        }
        #endregion
    }
}
