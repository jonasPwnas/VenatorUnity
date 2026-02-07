// Assets/Editor/2FA/Totp.cs
using System;
using System.Security.Cryptography;

internal static class Totp
{
    // Defaults: 30s period, 6 digits, HMAC-SHA1 (standard TOTP)
    public static string Generate(string base32Secret, DateTimeOffset now, int digits = 6, int periodSeconds = 30)
    {
        byte[] key = Base32.Decode(base32Secret);
        long counter = now.ToUnixTimeSeconds() / periodSeconds;

        // 8-byte big-endian counter
        byte[] counterBytes = new byte[8];
        for (int i = 7; i >= 0; i--)
        {
            counterBytes[i] = (byte)(counter & 0xFF);
            counter >>= 8;
        }

        byte[] hash;
        using (var hmac = new HMACSHA1(key))
            hash = hmac.ComputeHash(counterBytes);

        // Dynamic truncation
        int offset = hash[hash.Length - 1] & 0x0F;
        int binary =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        int mod = 1;
        for (int i = 0; i < digits; i++) mod *= 10;

        int otp = binary % mod;
        return otp.ToString().PadLeft(digits, '0');
    }

    // Accept a small drift window to tolerate clock differences
    public static bool Verify(string base32Secret, string userCode, DateTimeOffset now, int allowedStepDrift = 1, int digits = 6, int periodSeconds = 30)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            return false;

        userCode = userCode.Trim();

        for (int i = -allowedStepDrift; i <= allowedStepDrift; i++)
        {
            var t = now.AddSeconds(i * periodSeconds);
            if (Generate(base32Secret, t, digits, periodSeconds) == userCode)
                return true;
        }

        return false;
    }

    // Handy for QR generation / authenticator apps
    public static string MakeOtpAuthUri(string issuer, string accountName, string base32Secret, int digits = 6, int periodSeconds = 30)
    {
        string label = Uri.EscapeDataString($"{issuer}:{accountName}");
        string iss = Uri.EscapeDataString(issuer);

        return $"otpauth://totp/{label}?secret={base32Secret}&issuer={iss}&digits={digits}&period={periodSeconds}";
    }
}
