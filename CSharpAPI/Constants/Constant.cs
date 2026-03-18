namespace API.Constants
{
    public class Constant
    {
        public static string TunnelKey = new("73ctu$wN7wcAzZ$@$a%@K^h26TMpNyoPym6oyM5m7d4%QHW7ZzeQV2CiNUnCtbJ!LY4cGdqg^Zfadx^4AHGfwa9aN^WptHmMBdvVgZfRxS4ABn6stsVRBx6QXzBLj8t&".Reverse().ToArray());

        public static string CryptoKey = "WadTsTmwhE4bN6TfvnNSGYVVFbabL9k53/ZKrImj0vQ=";

        public static byte[] Biv = { 0x50, 0x08, 0xF1, 0xDD, 0xDE, 0x3C, 0xF2, 0x18, 0x44, 0x74, 0x19, 0x2C, 0x53, 0x49, 0xAB, 0xBC };
        private static byte[] _cryptoKeyBytes;
        public static byte[] CryptoKeyBytes => _cryptoKeyBytes ?? (_cryptoKeyBytes = Convert.FromBase64String(CryptoKey));

    }
}

