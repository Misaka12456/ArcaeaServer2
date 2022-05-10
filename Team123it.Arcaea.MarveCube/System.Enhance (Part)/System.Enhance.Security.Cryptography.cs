#nullable enable
using System.Security.Cryptography;
using System.Text;

namespace System.Enhance.Security.Cryptography
{
	public static class MD5Helper
    {
        public static string MD5Encrypt(byte[] data)
        {
            var md5 = MD5.Create();
            var encrypted = md5.ComputeHash(data);
            md5.Clear();
            var r = BitConverter.ToString(encrypted).Replace("-", string.Empty).ToLower();
            return r;
        }

        public static string MD5Encrypt(string dataStr,Encoding? encoding = null)
        {
            var md5 = MD5.Create();
            encoding ??= Encoding.UTF8;
            var encrypted = md5.ComputeHash(encoding.GetBytes(dataStr));
            md5.Clear();
            var r = BitConverter.ToString(encrypted).Replace("-", string.Empty).ToLower();
            return r;
        }
    }

    /// <summary>
    /// RC4加密算法类(RC4 Cryptography Helper)
    /// </summary>
    public static class RC4Helper
    {
        /// <summary>RC4加密算法
        /// 返回进过rc4加密过的字符
        /// </summary>
        /// <param name="str">被加密的字符</param>
        /// <param name="ckey">密钥</param>
        public static string Encrypt(string str, string ckey)
        {
            var s = new int[256];
            for (var i = 0; i < 256; i++)
            {
                s[i] = i;
            }
            //密钥转数组
            var keys = ckey.ToCharArray();//密钥转字符数组
            var key = new int[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                key[i] = keys[i];
            }
            //明文转数组
            var datas = str.ToCharArray();
            var mingwen = new int[datas.Length];
            for (var i = 0; i < datas.Length; i++)
            {
                mingwen[i] = datas[i];
            }

            //通过循环得到256位的数组(密钥)
            var j = 0;
            var k = 0;
            var length = key.Length;
            int a;
            for (var i = 0; i < 256; i++)
            {
                a = s[i];
                j = (j + a + key[k]);
                if (j >= 256)
                {
                    j = j % 256;
                }
                s[i] = s[j];
                s[j] = a;
                if (++k >= length)
                {
                    k = 0;
                }
            }
            //根据上面的256的密钥数组 和 明文得到密文数组
            int x = 0, y = 0, a2, b, c;
            var length2 = mingwen.Length;
            var miwen = new int[length2];
            for (var i = 0; i < length2; i++)
            {
                x = x + 1;
                x = x % 256;
                a2 = s[x];
                y = y + a2;
                y = y % 256;
                s[x] = b = s[y];
                s[y] = a2;
                c = a2 + b;
                c = c % 256;
                miwen[i] = mingwen[i] ^ s[c];
            }
            //密文数组转密文字符
            var mi = new char[miwen.Length];
            for (var i = 0; i < miwen.Length; i++)
            {
                mi[i] = (char)miwen[i];
            }
            var miwenstr = new string(mi);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(miwenstr));
        }

        /// <summary>RC4解密算法
        /// 返回进过rc4解密过的字符
        /// </summary>
        /// <param name="str">被解密的字符</param>
        /// <param name="ckey">密钥</param>
        public static string Decrypt(string str, string ckey)
        {
            str = Encoding.UTF8.GetString(Convert.FromBase64String(str));
            var s = new int[256];
            for (var i = 0; i < 256; i++)
            {
                s[i] = i;
            }
            //密钥转数组
            var keys = ckey.ToCharArray();//密钥转字符数组
            var key = new int[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                key[i] = keys[i];
            }
            //密文转数组
            var datas = str.ToCharArray();
            var miwen = new int[datas.Length];
            for (var i = 0; i < datas.Length; i++)
            {
                miwen[i] = datas[i];
            }

            //通过循环得到256位的数组(密钥)
            var j = 0;
            var k = 0;
            var length = key.Length;
            int a;
            for (var i = 0; i < 256; i++)
            {
                a = s[i];
                j = (j + a + key[k]);
                if (j >= 256)
                {
                    j = j % 256;
                }
                s[i] = s[j];
                s[j] = a;
                if (++k >= length)
                {
                    k = 0;
                }
            }
            //根据上面的256的密钥数组 和 密文得到明文数组
            int x = 0, y = 0, a2, b, c;
            var length2 = miwen.Length;
            var mingwen = new int[length2];
            for (var i = 0; i < length2; i++)
            {
                x = x + 1;
                x = x % 256;
                a2 = s[x];
                y = y + a2;
                y = y % 256;
                s[x] = b = s[y];
                s[y] = a2;
                c = a2 + b;
                c = c % 256;
                mingwen[i] = miwen[i] ^ s[c];
            }
            //明文数组转明文字符
            var ming = new char[mingwen.Length];
            for (var i = 0; i < mingwen.Length; i++)
            {
                ming[i] = (char)mingwen[i];
            }
            var mingwenstr = new string(ming);
            return mingwenstr;
        }
    }
}