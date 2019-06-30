using Microsoft.Extensions.Primitives;

namespace Sip.Protocol
{
    public readonly struct NameAddressHeader
    {
        private const char QuoteChar = '"';
        private const char UriStartChar = '<';
        private const char UriEndChar = '>';
        private const char ParameterSeparatorChar = ';';

        private static readonly char[] UriOrParametersStart = new char[] { UriStartChar, ParameterSeparatorChar };

        internal NameAddressHeader(StringSegment displayName, 
                                   StringSegment uri, 
                                   SipParameters parameters)
        {
            DisplayName = displayName;
            Uri = uri;
            Parameters = parameters;
        }

        public StringSegment DisplayName { get; }

        public StringSegment Uri { get; }

        public SipParameters Parameters { get; }

        public static bool TryParse(StringSegment chars, out NameAddressHeader header)
        {
            header = new NameAddressHeader();

            var displayName = ReadDisplayName(chars);
            var trimmedDisplayName = displayName.Trim();

            if(!IsDisplayNameValid(trimmedDisplayName))
            {
                return false;
            }

            var afterDisplayNameChars = chars.Subsegment(displayName.Length).TrimStart();
            var uri = ReadUri(afterDisplayNameChars);
            var trimmedUri = uri.TrimEnd();

            if(!IsUriValid(trimmedUri))
            {
                return false;
            }

            var afterUriChars = afterDisplayNameChars.Subsegment(uri.Length).Trim();

            if(!IsParametersOrEmpty(afterUriChars))
            {
                return false;
            }

            header = new NameAddressHeader(trimmedDisplayName,
                                           trimmedUri,
                                           new SipParameters(afterUriChars));
            return true;
        }

        private static StringSegment ReadUri(StringSegment chars)
        {
            var uriStartIndex = chars.IndexOf(UriStartChar);
            var uriEndIndex = chars.IndexOf(UriEndChar);
            
            if(uriStartIndex == 0 && uriEndIndex > 0)
            {
                return chars.Subsegment(0, uriEndIndex + 1);
            }

            var parametersStartIndex = chars.IndexOf(ParameterSeparatorChar);

            if(parametersStartIndex > 0)
            {
                return chars.Subsegment(0, parametersStartIndex);
            }

            return chars;
        }

        private static StringSegment ReadDisplayName(StringSegment chars)
        {
            var isQuoted = TryReadQuotedDisplayName(chars, out var startIndex, out var length);
            var skipQuotedOffset = isQuoted ? startIndex + length : 0;
            var displayNameEndIndex = chars.IndexOfAny(UriOrParametersStart, skipQuotedOffset);

            if (displayNameEndIndex < 0)
            {
                return StringSegment.Empty;
            }

            return chars.Subsegment(0, displayNameEndIndex);
        }

        private static bool TryReadQuotedDisplayName(StringSegment chars, out int startIndex, out int length)
        {
            startIndex = -1;
            length = 0;

            if (chars == StringSegment.Empty)
            {
                return false;
            }

            startIndex = SipCharacters.IndexOfSeparatorExcludeWhitespace(chars);

            if(startIndex < 0)
            {
                return false;
            }
            
            if(chars[startIndex] != QuoteChar)
            {
                return false;
            }

            var endQuoteCharIndex = SipCharacters.IndexOfNonEscaped(chars, QuoteChar, startIndex + 1);

            if(endQuoteCharIndex < 0)
            {
                return false;
            }

            length = endQuoteCharIndex - startIndex;
            return true;
        }

        private static bool IsDisplayNameValid(StringSegment chars)
        {
            if(chars == StringSegment.Empty)
            {
                return true;
            }

            if(IsQuoted(chars))
            {
                return true;
            }

            return SipCharacters.IsValidTokenExcludeWhitespase(chars);
        }

        private static bool IsQuoted(StringSegment chars)
        {
            return chars.Length > 1 &&
                   chars[0] == QuoteChar &&
                   chars[chars.Length - 1] == QuoteChar;
        }

        private static bool IsUriValid(StringSegment chars)
        {
            var uri = chars;

            if(IsInUriBrackets(chars))
            {
                uri = chars.Subsegment(1, chars.Length - 2);
            }

            return uri != StringSegment.Empty && SipCharacters.IsValidUri(uri);
        }

        private static bool IsInUriBrackets(StringSegment chars)
        {
            return chars.Length > 1 &&
                chars[0] == UriStartChar &&
                chars[chars.Length - 1] == UriEndChar;
        }

        private static bool IsParametersOrEmpty(StringSegment chars)
        {
            if (chars == StringSegment.Empty)
            {
                return true;
            }

            if (chars[0] == ParameterSeparatorChar)
            {
                return true;
            }

            return false;
        }
    }
}
