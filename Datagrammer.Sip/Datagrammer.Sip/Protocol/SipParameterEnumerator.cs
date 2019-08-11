using Microsoft.Extensions.Primitives;
using System;

namespace Sip.Protocol
{
    public struct SipParameterEnumerator
    {
        private const char ParameterSeparatorChar = ';';
        private const char NameValueSeparatorChar = '=';
        private const char QuoteChar = '"';

        private static readonly char[] ParameterSeparatorChars = new[] { ParameterSeparatorChar, NameValueSeparatorChar };

        private SipParameter? currentParameter;
        private StringSegment remainingChars;
        private StringSegment currentName;
        private StringSegment currentValue;
        private bool isSingleName;

        internal SipParameterEnumerator(StringSegment message)
        {
            currentParameter = null;
            remainingChars = message;
            currentName = StringSegment.Empty;
            currentValue = StringSegment.Empty;
            isSingleName = false;
        }

        public SipParameter Current => currentParameter ?? throw new ArgumentOutOfRangeException(nameof(Current));

        public bool MoveNext()
        {
            ClearCurrentParameter();

            while(TrySliceParameter(out var parameter))
            {
                if(TrySetAsCurrentIfValid(parameter))
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearCurrentParameter()
        {
            currentParameter = null;
            currentName = StringSegment.Empty;
            currentValue = StringSegment.Empty;
            isSingleName = false;
        }

        private bool TrySetAsCurrentIfValid(SipParameter parameter)
        {
            if(!IsValidName())
            {
                return false;
            }

            if(!IsValidValue())
            {
                return false;
            }

            currentParameter = new SipParameter(currentName, currentValue);
            return true;
        }

        private bool IsValidName()
        {
            if (currentName == StringSegment.Empty)
            {
                return false;
            }

            if (!SipCharacters.IsValidToken(currentName))
            {
                return false;
            }

            return true;
        }

        private bool IsValidValue()
        {
            if(isSingleName)
            {
                return true;
            }

            if (currentValue == StringSegment.Empty)
            {
                return false;
            }

            if(SipCharacters.IsValidQuoted(currentValue))
            {
                return true;
            }

            if(!SipCharacters.IsValidToken(currentValue))
            {
                return false;
            }

            return true;
        }

        private bool TrySliceParameter(out SipParameter parameter)
        {
            parameter = new SipParameter();

            if(remainingChars == StringSegment.Empty)
            {
                return false;
            }

            if(!TrySliceFirstChar(ParameterSeparatorChar))
            {
                return false;
            }

            currentName = SliceName().Trim();
            isSingleName = !TrySliceFirstChar(NameValueSeparatorChar);

            if(!isSingleName)
            {
                currentValue = SliceValue().Trim();
            }

            return true;
        }

        private StringSegment SliceName()
        {
            var name = ReadName();
            remainingChars = remainingChars.Subsegment(name.Length);
            return name;
        }

        private StringSegment SliceValue()
        {
            var value = ReadValue();
            remainingChars = remainingChars.Subsegment(value.Length);
            return value;
        }

        private StringSegment ReadName()
        {
            var separatorIndex = remainingChars.IndexOfAny(ParameterSeparatorChars);

            if (separatorIndex < 0)
            {
                return remainingChars;
            }

            return remainingChars.Subsegment(0, separatorIndex);
        }

        private bool TrySliceFirstChar(char first)
        {
            if (remainingChars == StringSegment.Empty)
            {
                return false;
            }

            if (remainingChars[0] != first)
            {
                return false;
            }

            remainingChars = remainingChars.Subsegment(1);
            return true;
        }

        private StringSegment ReadValue()
        {
            var hasQuoted = TryReadQuotedValue(out var startIndex, out var length);
            var skipQuotedOffset = hasQuoted ? startIndex + length : 0;
            var separatorIndex = remainingChars.IndexOf(ParameterSeparatorChar, skipQuotedOffset);

            if (separatorIndex < 0)
            {
                return remainingChars;
            }

            return remainingChars.Subsegment(0, separatorIndex);
        }

        private bool TryReadQuotedValue(out int startIndex, out int length)
        {
            startIndex = -1;
            length = 0;

            if (remainingChars == StringSegment.Empty)
            {
                return false;
            }

            startIndex = SipCharacters.IndexOfNonWhitespace(remainingChars);

            if (startIndex < 0)
            {
                return false;
            }

            if (remainingChars[startIndex] != QuoteChar)
            {
                return false;
            }

            var endQuoteCharIndex = SipCharacters.IndexOfNonEscaped(remainingChars, QuoteChar, startIndex + 1);

            if (endQuoteCharIndex < 0)
            {
                return false;
            }

            length = endQuoteCharIndex - startIndex;
            return true;
        }
    }
}
