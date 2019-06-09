using System;

namespace Datagrammer.Sip
{
    public static class SipCharacters
    {
        private const int ArrayLength = 127;

        private static readonly bool[] token = CreateToken();
        private static readonly bool[] digits = CreateDigits();

        private static void InitializeLetters(bool[] chars)
        {
            for (var c = 'A'; c <= 'Z'; c++)
            {
                chars[c] = true;
            }

            for (var c = 'a'; c <= 'z'; c++)
            {
                chars[c] = true;
            }
        }

        private static void InitializeDigits(bool[] chars)
        {
            for (var c = '0'; c <= '9'; c++)
            {
                chars[c] = true;
            }
        }

        private static void InitializeTokenSpecials(bool[] chars)
        {
            chars['!'] = true;
            chars['#'] = true;
            chars['$'] = true;
            chars['%'] = true;
            chars['&'] = true;
            chars['\''] = true;
            chars['*'] = true;
            chars['+'] = true;
            chars['-'] = true;
            chars['.'] = true;
            chars['^'] = true;
            chars['_'] = true;
            chars['`'] = true;
            chars['|'] = true;
            chars['~'] = true;
        }

        private static bool[] CreateToken()
        {
            var result = new bool[ArrayLength];

            InitializeDigits(result);
            InitializeLetters(result);
            InitializeTokenSpecials(result);

            return result;
        }

        private static bool[] CreateDigits()
        {
            var result = new bool[ArrayLength];

            InitializeDigits(result);

            return result;
        }

        public static bool HasValidTokenChars(ReadOnlySpan<byte> bytes)
        {
            foreach(var b in bytes)
            {
                if(b < 0 || b >= token.Length || !token[b])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidField(ReadOnlySpan<char> chars)
        {
            foreach(var c in chars)
            {
                if(c < 32 || c > 126)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasOnlyDigits(ReadOnlySpan<byte> bytes)
        {
            foreach (var b in bytes)
            {
                if (b < 0 || b >= token.Length || !digits[b])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
