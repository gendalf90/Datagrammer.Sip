using System;

namespace Sip.Protocol
{
    internal static class SipCharacters
    {
        private const int ArrayLength = 127;

        private static readonly bool[] token = CreateToken();
        private static readonly bool[] digits = CreateDigits();
        private static readonly bool[] separators = CreateSeparators();

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

        private static void InitializeSeparators(bool[] chars)
        {
            chars['('] = true;
            chars[')'] = true;
            chars['<'] = true;
            chars['>'] = true;
            chars['@'] = true;
            chars[','] = true;
            chars[';'] = true;
            chars[':'] = true;
            chars['\\'] = true;
            chars['\"'] = true;
            chars['/'] = true;
            chars['['] = true;
            chars[']'] = true;
            chars['?'] = true;
            chars['='] = true;
            chars['{'] = true;
            chars['}'] = true;
            chars[' '] = true;
            chars['\t'] = true;
        }

        private static bool[] CreateToken()
        {
            var result = new bool[ArrayLength];

            InitializeDigits(result);
            InitializeLetters(result);
            InitializeTokenSpecials(result);

            return result;
        }

        private static bool[] CreateSeparators()
        {
            var result = new bool[ArrayLength];

            InitializeSeparators(result);

            return result;
        }

        private static bool[] CreateDigits()
        {
            var result = new bool[ArrayLength];

            InitializeDigits(result);

            return result;
        }

        public static bool IsValidToken(ReadOnlySpan<byte> bytes)
        {
            foreach(var b in bytes)
            {
                if(b < 0 || b >= ArrayLength || !token[b])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidToken(ReadOnlySpan<char> chars)
        {
            foreach (var c in chars)
            {
                if (c >= ArrayLength || !token[c])
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

        public static bool IsDigits(ReadOnlySpan<byte> bytes)
        {
            foreach (var b in bytes)
            {
                if (b < 0 || b >= ArrayLength || !digits[b])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasCROrLF(ReadOnlySpan<byte> bytes)
        {
            foreach(var b in bytes)
            {
                if(b == 10 || b == 13)
                {
                    return true;
                }
            }

            return false;
        }

        public static int IndexOfSeparator(ReadOnlySpan<char> chars)
        {
            for(int i = 0; i < chars.Length; i++)
            {
                if(chars[i] < ArrayLength && separators[chars[i]])
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
