using System.Security.Cryptography;
using System.Text;

namespace RentCollection.Application.Helpers;

/// <summary>
/// Helper class for Time-based One-Time Password (TOTP) generation and verification
/// Implements RFC 6238 standard used by Google Authenticator, Authy, etc.
/// </summary>
public static class TotpHelper
{
    private const int CodeDigits = 6;
    private const int TimeStep = 30; // 30 seconds
    private const int WindowSize = 1; // Allow codes from ±1 time window (30 seconds before/after)

    /// <summary>
    /// Generate a random secret key for TOTP
    /// </summary>
    /// <returns>Base32-encoded secret</returns>
    public static string GenerateSecret()
    {
        var bytes = new byte[20]; // 160 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Base32Encode(bytes);
    }

    /// <summary>
    /// Generate a TOTP code from a secret
    /// </summary>
    /// <param name="secret">Base32-encoded secret</param>
    /// <returns>6-digit code</returns>
    public static string GenerateCode(string secret)
    {
        var secretBytes = Base32Decode(secret);
        var counter = GetCurrentCounter();
        return GenerateCodeInternal(secretBytes, counter);
    }

    /// <summary>
    /// Verify a TOTP code
    /// </summary>
    /// <param name="secret">Base32-encoded secret</param>
    /// <param name="code">6-digit code to verify</param>
    /// <returns>True if code is valid</returns>
    public static bool VerifyCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
            return false;

        if (code.Length != CodeDigits || !code.All(char.IsDigit))
            return false;

        var secretBytes = Base32Decode(secret);
        var currentCounter = GetCurrentCounter();

        // Check current window and ±WindowSize windows
        for (int i = -WindowSize; i <= WindowSize; i++)
        {
            var testCode = GenerateCodeInternal(secretBytes, currentCounter + i);
            if (testCode == code)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Generate QR code URI for authenticator apps
    /// </summary>
    /// <param name="secret">Base32-encoded secret</param>
    /// <param name="accountName">Account name (usually user's email)</param>
    /// <param name="issuer">Issuer name (app name)</param>
    /// <returns>otpauth:// URI</returns>
    public static string GenerateQrCodeUri(string secret, string accountName, string issuer = "RentCollection")
    {
        return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
    }

    // Internal helper methods

    private static long GetCurrentCounter()
    {
        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return unixTimestamp / TimeStep;
    }

    private static string GenerateCodeInternal(byte[] secret, long counter)
    {
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(counterBytes);

        using var hmac = new HMACSHA1(secret);
        var hash = hmac.ComputeHash(counterBytes);

        // Dynamic truncation
        var offset = hash[hash.Length - 1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) << 8)
                   | (hash[offset + 3] & 0xFF);

        var otp = binary % (int)Math.Pow(10, CodeDigits);
        return otp.ToString().PadLeft(CodeDigits, '0');
    }

    // Base32 encoding/decoding

    private static string Base32Encode(byte[] data)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder();
        int bits = 0;
        int value = 0;

        foreach (byte b in data)
        {
            value = (value << 8) | b;
            bits += 8;

            while (bits >= 5)
            {
                bits -= 5;
                result.Append(base32Chars[(value >> bits) & 0x1F]);
            }
        }

        if (bits > 0)
        {
            result.Append(base32Chars[(value << (5 - bits)) & 0x1F]);
        }

        // Padding
        while (result.Length % 8 != 0)
        {
            result.Append('=');
        }

        return result.ToString();
    }

    private static byte[] Base32Decode(string base32)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        base32 = base32.TrimEnd('=').ToUpperInvariant();

        var result = new List<byte>();
        int bits = 0;
        int value = 0;

        foreach (char c in base32)
        {
            int index = base32Chars.IndexOf(c);
            if (index < 0)
                throw new ArgumentException($"Invalid Base32 character: {c}");

            value = (value << 5) | index;
            bits += 5;

            if (bits >= 8)
            {
                bits -= 8;
                result.Add((byte)(value >> bits));
            }
        }

        return result.ToArray();
    }
}
