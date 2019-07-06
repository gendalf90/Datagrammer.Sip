using Microsoft.Extensions.Primitives;
using System;

namespace Sip.Protocol
{
    public struct SipHeaderRowValueEnumerator
    {
        private const char QuoteChar = '"';
        private const char UriStartChar = '<';
        private const char UriEndChar = '>';
        private const char EscapeChar = '\\';
        private const char ValueSeparatorChar = ',';

        private StringSegment remainingChars;
        private StringSegment? currentValue;

        internal SipHeaderRowValueEnumerator(StringSegment values)
        {
            remainingChars = values;
            currentValue = null;
        }

        public StringSegment Current => currentValue ?? throw new ArgumentOutOfRangeException(nameof(Current));

        public bool MoveNext()
        {
            currentValue = null;

            if (TrySliceValue(out var value))
            {
                currentValue = value.Trim();
            }

            return currentValue.HasValue;
        }

        private bool TrySliceValue(out StringSegment value)
        {
            value = StringSegment.Empty;

            if(remainingChars == StringSegment.Empty)
            {
                return false;
            }

            remainingChars = SliceValue(remainingChars, out value);
            remainingChars = SliceSeparator(remainingChars);
            remainingChars = remainingChars.Trim();
            return true;
        }

        private StringSegment SliceSeparator(StringSegment chars)
        {
            if(chars == StringSegment.Empty)
            {
                return chars;
            }

            if(chars[0] != ValueSeparatorChar)
            {
                return chars;
            }

            return chars.Subsegment(1);
        }

        private StringSegment SliceValue(StringSegment chars, out StringSegment value)
        {
            value = chars;
            var separatorIndex = IndexOfNonQuotedSeparator(chars);

            if (separatorIndex < 0)
            {
                return StringSegment.Empty;
            }

            value = chars.Subsegment(0, separatorIndex);
            return chars.Subsegment(separatorIndex);
        }

        private int IndexOfNonQuotedSeparator(StringSegment chars)
        {
            var isQuoted = false;
            var isUri = false;

            for(int i = 0; i < chars.Length; i++)
            {
                if(chars[i] == QuoteChar && !(i > 0 && chars[i - 1] == EscapeChar))
                {
                    isQuoted = !isQuoted && !isUri && chars.IndexOf(QuoteChar, i + 1) >= 0;
                }
                else if(chars[i] == UriStartChar && !isUri && !isQuoted && chars.IndexOf(UriEndChar, i + 1) >= 0)
                {
                    isUri = true;
                }
                else if(chars[i] == UriEndChar && isUri && !isQuoted)
                {
                    isUri = false;
                }
                else if(chars[i] == ValueSeparatorChar && !isUri && !isQuoted)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
