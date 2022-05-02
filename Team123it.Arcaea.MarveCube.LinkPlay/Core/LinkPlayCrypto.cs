using System.Security.Cryptography;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public class LinkPlayCrypto
    {
        private static readonly byte[] DefaultKey = {0x11, 0x45, 0x14, 0x19, 0x19, 0x19, 0x18, 0x00, 0x11, 0x45, 0x14, 0x19, 0x19, 0x19, 0x18, 0x00};

        public static byte[] EncryptPack(byte[] token, byte[] body)
        {
            var random = new Random();
            var iv = new byte[12]; random.NextBytes(iv);
            var pad = 16 - (body.Length % 16);  // pkcs7 padding
            var padding = Enumerable.Repeat((byte)pad, pad).ToArray(); var padded = body.Concat(padding).ToArray();
            var cipher = new byte[body.Length+pad]; var authTag = new byte[12];
            using var aes = new AesGcm(DefaultKey);
            aes.Encrypt(iv, padded, cipher, authTag);
            var returnBytes = token.Concat(iv).Concat(authTag).Concat(cipher);
            return returnBytes.ToArray();
        }

        public static byte[] DecryptPack(byte[] data)
        {
            var iv = data[8..20];
            var authTag = data[20..32];
            var cipher = data[32..];
            var decrypted = new byte[cipher.Length];
            using var aes = new AesGcm(DefaultKey);
            aes.Decrypt(iv, cipher, authTag, decrypted);
            
            var pad = decrypted[^1]; // removal of pkcs7 padding
            return decrypted.Take(decrypted.Length - pad).ToArray();
        }
    }
}

