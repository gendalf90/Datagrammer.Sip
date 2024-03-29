﻿using System;

namespace Sip.Protocol
{
    internal static class SipCharacters
    {
        private const int ArrayLength = 127;

        private static readonly bool[] token = CreateToken();
        private static readonly bool[] digits = CreateDigits();
        private static readonly bool[] separators = CreateSeparators();
        private static readonly bool[] uri = CreateUri();
        private static readonly bool[] host = CreateHost();

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

        private static void InitializeUri(bool[] chars)
        {
            chars['-'] = true;
            chars['.'] = true;
            chars['_'] = true;
            chars['~'] = true;
            chars[':'] = true;
            chars['/'] = true;
            chars['?'] = true;
            chars['#'] = true;
            chars['['] = true;
            chars[']'] = true;
            chars['@'] = true;
            chars['!'] = true;
            chars[']'] = true;
            chars['$'] = true;
            chars['&'] = true;
            chars['\''] = true;
            chars['('] = true;
            chars[')'] = true;
            chars['*'] = true;
            chars['+'] = true;
            chars[','] = true;
            chars[';'] = true;
            chars['='] = true;
        }

        private static void InitializeHost(bool[] chars)
        {
            chars['-'] = true;
            chars['.'] = true;
        }

        private static bool[] CreateToken()
        {
            var result = new bool[ArrayLength];

            InitializeDigits(result);
            InitializeLetters(result);
            InitializeTokenSpecials(result);

            return result;
        }

        private static bool[] CreateUri()
        {
            var result = new bool[ArrayLength];

            InitializeDigits(result);
            InitializeLetters(result);
            InitializeUri(result);

            return result;
        }

        private static bool[] CreateHost()
        {
            var result = new bool[ArrayLength];

            InitializeDigits(result);
            InitializeLetters(result);
            InitializeHost(result);

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

        public static bool IsValidUri(ReadOnlySpan<byte> bytes)
        {
            foreach (var b in bytes)
            {
                if (b >= ArrayLength || !uri[b])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidUri(ReadOnlySpan<char> chars)
        {
            foreach (var c in chars)
            {
                if (c >= ArrayLength || !uri[c])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidToken(ReadOnlySpan<byte> bytes)
        {
            foreach(var b in bytes)
            {
                if(b >= ArrayLength || !token[b])
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

        public static bool IsValidHost(ReadOnlySpan<char> chars)
        {
            foreach (var c in chars)
            {
                if (c >= ArrayLength || !host[c])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidTokenExcludeWhitespase(ReadOnlySpan<char> chars)
        {
            foreach (var c in chars)
            {
                if (c >= ArrayLength)
                {
                    return false;
                }

                if(!token[c] && c != ' ' && c != '\t')
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
                if(c < 33 || c > 126)
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
                if (b >= ArrayLength || !digits[b])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsDigits(ReadOnlySpan<char> chars)
        {
            foreach (var c in chars)
            {
                if (c >= ArrayLength || !digits[c])
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

        public static bool HasCROrLF(ReadOnlySpan<char> chars)
        {
            foreach (var c in chars)
            {
                if (c == '\n' || c == '\r')
                {
                    return true;
                }
            }

            return false;
        }

        public static int IndexOfSeparator(ReadOnlySpan<char> chars, int offset = 0)
        {
            if(offset < 0 ||  offset > chars.Length)
            {
                throw new IndexOutOfRangeException(nameof(offset));
            }

            for(int i = offset; i < chars.Length; i++)
            {
                if(chars[i] < ArrayLength && separators[chars[i]])
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOfWhitespace(ReadOnlySpan<char> chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == ' ' || chars[i] == '\t')
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOfSeparatorExcludeWhitespace(ReadOnlySpan<char> chars, int offset = 0)
        {
            if (offset < 0 || offset > chars.Length)
            {
                throw new IndexOutOfRangeException(nameof(offset));
            }

            for (int i = offset; i < chars.Length; i++)
            {
                if (chars[i] < ArrayLength && chars[i] != ' ' && chars[i] != '\t' && separators[chars[i]])
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOfNonWhitespace(ReadOnlySpan<char> chars, int offset = 0)
        {
            if (offset < 0 || offset > chars.Length)
            {
                throw new IndexOutOfRangeException(nameof(offset));
            }

            for (int i = offset; i < chars.Length; i++)
            {
                if (chars[i] != ' ' && chars[i] != '\t')
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOfNonEscaped(ReadOnlySpan<char> chars, char c, int offset = 0)
        {
            if (offset < 0 || offset > chars.Length)
            {
                throw new IndexOutOfRangeException(nameof(offset));
            }

            for (int i = offset; i < chars.Length; i++)
            {
                if (i > 0 && chars[i - 1] == '\\')
                {
                    continue;
                }

                if (chars[i] == c)
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool IsValidQuoted(ReadOnlySpan<char> chars)
        {
            var isQuoted = chars.Length > 1 &&
                chars[0] == '"' &&
                chars[chars.Length - 1] == '"';

            if(!isQuoted)
            {
                return false;
            }

            var unquotedValue = chars.Slice(1, chars.Length - 2);
            var nonEscapedQuoteCharIndex = IndexOfNonEscaped(unquotedValue, '"');
            return nonEscapedQuoteCharIndex < 0;
        }
    }
}
