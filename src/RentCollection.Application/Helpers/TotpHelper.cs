using System.Security.Cryptography;
using System.Text;

namespace RentCollection.Application.Helpers
{
    public static class TotpHelper
    {
        private const int SecretLength = 20; // 160 bits
        private const int CodeLength = 6;
        private const int TimeStep = 30; // seconds

        public static string GenerateSecret()
        {
            var bytes = new byte[SecretLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Base32Encode(bytes);
        }

        public static string GenerateCode(string secret)
        {
            var secretBytes = Base32Decode(secret);
            var counter = GetCurrentCounter();
            var hash = GenerateHash(secretBytes, counter);
            var offset = hash[hash.Length - 1] & 0x0F;
            var binary = ((hash[offset] & 0x7F) << 24)
                       | ((hash[offset + 1] & 0xFF) << 16)
                       | ((hash[offset + 2] & 0xFF) << 8)
                       | (hash[offset + 3] & 0xFF);
            
            var code = binary % (int)Math.Pow(10, CodeLength);
            return code.ToString().PadLeft(CodeLength, '0');
        }

        public static bool VerifyCode(string secret, string code)
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code))
                return false;

            if (code.Length != CodeLength)
                return false;

            // Check current time window and ±1 time window (to account for clock drift)
            var currentCounter = GetCurrentCounter();
            
            for (int i = -1; i <= 1; i++)
            {
                var testCounter = currentCounter + i;
                var secretBytes = Base32Decode(secret);
                var hash = GenerateHash(secretBytes, testCounter);
                var offset = hash[hash.Length - 1] & 0x0F;
                var binary = ((hash[offset] & 0x7F) << 24)
                           | ((hash[offset + 1] & 0xFF) << 16)
                           | ((hash[offset + 2] & 0xFF) << 8)
                           | (hash[offset + 3] & 0xFF);
                
                var testCode = (binary % (int)Math.Pow(10, CodeLength)).ToString().PadLeft(CodeLength, '0');
                
                if (testCode == code)
                    return true;
            }

            return false;
        }

        public static string GenerateQrCodeUri(string secret, string accountName, string issuer)
        {
            var encodedIssuer = Uri.EscapeDataString(issuer);
            var encodedAccount = Uri.EscapeDataString(accountName);
            return $"otpauth://totp/{encodedIssuer}:{encodedAccount}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits={CodeLength}&period={TimeStep}";
        }

        private static long GetCurrentCounter()
        {
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return unixTimestamp / TimeStep;
        }

        private static byte[] GenerateHash(byte[] secret, long counter)
        {
            var counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counterBytes);

            using (var hmac = new HMACSHA1(secret))
            {
                return hmac.ComputeHash(counterBytes);
            }
        }

        private static string Base32Encode(byte[] data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (byte b in data)
            {
                buffer = (buffer << 8) | b;
                bitsLeft += 8;

                while (bitsLeft >= 5)
                {
                    result.Append(alphabet[(buffer >> (bitsLeft - 5)) & 0x1F]);
                    bitsLeft -= 5;
                }
            }

            if (bitsLeft > 0)
            {
                result.Append(alphabet[(buffer << (5 - bitsLeft)) & 0x1F]);
            }

            return result.ToString();
        }

        private static byte[] Base32Decode(string encoded)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            encoded = encoded.ToUpper().Replace(" ", "").Replace("-", "");

            var bytes = new List<byte>();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (char c in encoded)
            {
                int value = alphabet.IndexOf(c);
                if (value < 0)
                    throw new ArgumentException("Invalid Base32 character");

                buffer = (buffer << 5) | value;
                bitsLeft += 5;

                if (bitsLeft >= 8)
                {
                    bytes.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                    bitsLeft -= 8;
                }
            }

            return bytes.ToArray();
        }
    }
}
