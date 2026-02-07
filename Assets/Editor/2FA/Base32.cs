// Assets/Editor/2FA/Base32.cs
using System;
using System.Collections.Generic;

internal static class Base32
{
    // RFC 4648 alphabet
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static byte[] Decode(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Array.Empty<byte>();

        string s = input.Trim().Replace(" ", "").TrimEnd('=').ToUpperInvariant();

        int bitBuffer = 0;
        int bitCount = 0;
        var output = new List<byte>(s.Length * 5 / 8);

        foreach (char c in s)
        {
            int val = Alphabet.IndexOf(c);
            if (val < 0) throw new FormatException($"Invalid Base32 char: '{c}'");

            bitBuffer = (bitBuffer << 5) | val;
            bitCount += 5;

            while (bitCount >= 8)
            {
                bitCount -= 8;
                output.Add((byte)((bitBuffer >> bitCount) & 0xFF));
            }
        }

        return output.ToArray();
    }
}
