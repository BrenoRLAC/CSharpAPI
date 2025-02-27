using API.Constants;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace API.Utilities
{
    public static class AssistantHelpers
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static string EncryptInt(this int value)
        {
            return value.ToString().Encrypt();
        }

        private static Aes? _cryptoProvider;
        private static Aes CryptoProvider
        {
            get
            {
                if (_cryptoProvider != null) return _cryptoProvider;

                var md5HashProvider = MD5.Create();
                var keyHash = md5HashProvider.ComputeHash(Encoding.UTF8.GetBytes(Constant.TunnelKey));

                _cryptoProvider = Aes.Create();
                _cryptoProvider.Key = keyHash;
                _cryptoProvider.Mode = CipherMode.ECB;
                _cryptoProvider.Padding = PaddingMode.PKCS7;

                return _cryptoProvider;
            }
        }

        public static string Encrypt(this string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            byte[] inArray = CryptoProvider.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(inArray);
        }

        private static ICryptoTransform? _decryptor;
        private static ICryptoTransform Decryptor => _decryptor ??= CryptoProvider.CreateDecryptor();
        public static string Decrypt(this string value)
        {
            byte[] bytes = Convert.FromBase64String(value);
            byte[] inArray = Decryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            return Encoding.UTF8.GetString(inArray);
        }

        public static int DecryptInt(this string value)
        {
            return Convert.ToInt32(value.Decrypt());
        }
        
    }
}
