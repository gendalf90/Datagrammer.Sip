using Microsoft.Extensions.Primitives;
using System;

namespace Sip.Protocol
{
    public struct SipParameterEnumerator
    {
        private const char ParameterSeparatorChar = ';';
        private const char NameValueSeparatorChar = '=';
        private const char QuoteChar = '"';
        private const char EscapeChar = '\\';

        private SipParameter? currentParameter;
        private StringSegment remainingChars;

        internal SipParameterEnumerator(StringSegment message)
        {
            currentParameter = null;
            remainingChars = message;
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
        }

        private bool HasRemainigChars()
        {
            return remainingChars != StringSegment.Empty;
        }

        private bool TrySetAsCurrentIfValid(SipParameter parameter)
        {
            if(!IsValidName(parameter.Name))
            {
                return false;
            }

            if(!IsValidValue(parameter.Value))
            {
                return false;
            }

            currentParameter = parameter;
            return true;
        }

        private bool IsValidName(StringSegment name)
        {
            if (name == StringSegment.Empty)
            {
                return false;
            }

            if (!SipCharacters.IsValidToken(name.AsSpan()))
            {
                return false;
            }

            return true;
        }

        private bool IsValidValue(StringSegment value)
        {
            if (value == StringSegment.Empty)
            {
                return true;
            }

            if(IsQuoted(value))
            {
                return true;
            }

            if (!SipCharacters.IsValidToken(value.AsSpan()))
            {
                return false;
            }

            return true;
        }

        private bool IsQuoted(StringSegment chars)
        {
            return chars.Length > 1 &&
                   chars[0] == QuoteChar &&
                   chars[chars.Length - 1] == QuoteChar;
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

            var name = SliceName();

            if(TrySliceFirstChar(NameValueSeparatorChar))
            {
                parameter = new SipParameter(name.Trim(), SliceValue().Trim());
            }
            else
            {
                parameter = new SipParameter(name.Trim(), StringSegment.Empty);
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
            var separatorIndex = SipCharacters.IndexOfSeparator(remainingChars.AsSpan());

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
            if (TryReadQuotedValue(out var quotedValue))
            {
                return quotedValue;
            }

            var separatorIndex = SipCharacters.IndexOfSeparator(remainingChars);

            if(separatorIndex < 0)
            {
                return remainingChars;
            }

            return remainingChars.Subsegment(0, separatorIndex);
        }

        private bool TryReadQuotedValue(out StringSegment value)
        {
            value = StringSegment.Empty;

            if (remainingChars == StringSegment.Empty)
            {
                return false;
            }

            if (remainingChars[0] != QuoteChar)
            {
                return false;
            }

            var quotedChars = remainingChars.Subsegment(1);
            var endQuoteCharIndex = IndexOfNonEscapedQuoteChar(quotedChars);

            if (endQuoteCharIndex < 0)
            {
                return false;
            }

            var afterQuotedChars = quotedChars.Subsegment(endQuoteCharIndex + 1);
            var separatorIndex = SipCharacters.IndexOfSeparator(afterQuotedChars);

            if(afterQuotedChars.Length > 0 && separatorIndex != 0)
            {
                return false;
            }

            value = remainingChars.Subsegment(0, endQuoteCharIndex + 2);
            return true;
        }

        private int IndexOfNonEscapedQuoteChar(StringSegment chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if(i > 0 && chars[i - 1] == EscapeChar)
                {
                    continue;
                }

                if(chars[i] == QuoteChar)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
