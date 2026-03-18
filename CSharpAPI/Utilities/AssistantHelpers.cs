using API.Constants;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Utilities
{
    public static class AssistantHelpers
    {
        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj);
        }
        public static bool IsValidPassword(this string pwd, out string error)
        {

            error = pwd switch
            {
                null => "Password cannot be null.",
                _ when pwd.Length < 10 || pwd.Length > 100 => "Password must be between 10 characters and 100 characters",
                _ when !pwd.Any(char.IsDigit) => "Password must have at least one number",
                _ when !pwd.Any(char.IsUpper) => "Password must have at least one uppercase letter.",
                _ when !pwd.Any(char.IsLower) => "Password must have at least one lowercase letter.",
                _ when pwd.Count(c => !char.IsLetterOrDigit(c)) < 1 => "Password must have at least one special character. Example: \"! @ # $ % &\"",
                _ => null
            };

            return error is null;
        }
        public static string EncryptCookie(this string input)
        {
           
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input cannot be null");

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = Constant.CryptoKeyBytes;
            aes.IV = Constant.Biv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            using var mStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write);

            byte[] bText = Encoding.UTF8.GetBytes(input);
            cryptoStream.Write(bText, 0, bText.Length);
            cryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(mStream.ToArray());
        }
        public static bool TryDecryptCookie(this string vl, out string decripted)
        {
            bool success = false;            
            try
            {
                decripted = DecryptCookie(vl);
                success = true;
            }
            catch (Exception)
            {
                decripted = null;
            }

            return success;
        }
        public static string DecryptCookie(this string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input cannot be null");

            byte[] encryptedBytes = Convert.FromBase64String(input);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = Constant.CryptoKeyBytes;
            aes.IV = Constant.Biv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var memoryStream = new MemoryStream(encryptedBytes);
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();

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
                var keyHash = MD5.HashData(Encoding.UTF8.GetBytes(Constant.TunnelKey));

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

        public static string PasswordEncryption(this string value, int workFactor = 12)
        {
            return BCrypt.Net.BCrypt.HashPassword(value, workFactor);
        }

        public static bool PasswordValidation(this string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public static string GenerateRandomCodeNumeric()
        {
            return Random.Shared.Next(100000, 1000000).ToString();
        }

        public static string GenerateRandomCodeAlphanumeric(int length)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()-_+=";

            ArgumentOutOfRangeException.ThrowIfNegative(length);

            if (length == 0)
                return string.Empty;

            Span<char> result = stackalloc char[length];
            Span<byte> randomBytes = stackalloc byte[length];

            RandomNumberGenerator.Fill(randomBytes);

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[randomBytes[i] % chars.Length];
            }

            return new string(result);
        }
       
    }
}
