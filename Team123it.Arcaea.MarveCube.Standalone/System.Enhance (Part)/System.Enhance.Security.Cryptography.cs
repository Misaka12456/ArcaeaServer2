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
            byte[] encrypted = md5.ComputeHash(data);
            md5.Clear();
            string r = BitConverter.ToString(encrypted).Replace("-", string.Empty).ToLower();
            return r;
        }

        public static string MD5Encrypt(string dataStr,Encoding? encoding = null)
        {
            var md5 = MD5.Create();
            encoding ??= Encoding.UTF8;
            byte[] encrypted = md5.ComputeHash(encoding.GetBytes(dataStr));
            md5.Clear();
            string r = BitConverter.ToString(encrypted).Replace("-", string.Empty).ToLower();
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
            int[] s = new int[256];
            for (int i = 0; i < 256; i++)
            {
                s[i] = i;
            }
            //密钥转数组
            char[] keys = ckey.ToCharArray();//密钥转字符数组
            int[] key = new int[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                key[i] = keys[i];
            }
            //明文转数组
            char[] datas = str.ToCharArray();
            int[] mingwen = new int[datas.Length];
            for (int i = 0; i < datas.Length; i++)
            {
                mingwen[i] = datas[i];
            }

            //通过循环得到256位的数组(密钥)
            int j = 0;
            int k = 0;
            int length = key.Length;
            int a;
            for (int i = 0; i < 256; i++)
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
            int length2 = mingwen.Length;
            int[] miwen = new int[length2];
            for (int i = 0; i < length2; i++)
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
            char[] mi = new char[miwen.Length];
            for (int i = 0; i < miwen.Length; i++)
            {
                mi[i] = (char)miwen[i];
            }
            string miwenstr = new string(mi);
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
            int[] s = new int[256];
            for (int i = 0; i < 256; i++)
            {
                s[i] = i;
            }
            //密钥转数组
            char[] keys = ckey.ToCharArray();//密钥转字符数组
            int[] key = new int[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                key[i] = keys[i];
            }
            //密文转数组
            char[] datas = str.ToCharArray();
            int[] miwen = new int[datas.Length];
            for (int i = 0; i < datas.Length; i++)
            {
                miwen[i] = datas[i];
            }

            //通过循环得到256位的数组(密钥)
            int j = 0;
            int k = 0;
            int length = key.Length;
            int a;
            for (int i = 0; i < 256; i++)
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
            int length2 = miwen.Length;
            int[] mingwen = new int[length2];
            for (int i = 0; i < length2; i++)
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
            char[] ming = new char[mingwen.Length];
            for (int i = 0; i < mingwen.Length; i++)
            {
                ming[i] = (char)mingwen[i];
            }
            string mingwenstr = new string(ming);
            return mingwenstr;
        }
    }
}