using Microsoft.Extensions.Primitives;
using System;

namespace Datagrammer.Sip
{
    public struct SipHeaderEnumerator
    {
        private const string CRLFString = "\r\n";
        private const char SemicolonChar = ':';

        private SipHeader? currentHeader;
        private StringSegment remainingChars;
        private StringSegment? currentLine;

        internal SipHeaderEnumerator(StringSegment message)
        {
            currentHeader = null;
            remainingChars = message;
            currentLine = null;
        }

        public SipHeader Current => currentHeader ?? throw new ArgumentOutOfRangeException(nameof(Current));

        public bool MoveNext()
        {
            ClearCurrentHeader();

            while (!IsCurrentLineEmpty && TrySkipLine() && TrySliceLine())
            {
                if(TryParseHeader())
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearCurrentHeader()
        {
            currentHeader = null;
        }

        private bool IsCurrentLineEmpty
        {
            get => currentLine.HasValue && currentLine.Value == StringSegment.Empty;
        }

        private bool TryParseHeader()
        {
            if(!currentLine.HasValue)
            {
                return false;
            }

            var semicolonIndex = currentLine.Value.IndexOf(SemicolonChar);

            if(semicolonIndex < 0)
            {
                return false;
            }

            var name = currentLine.Value.Subsegment(0, semicolonIndex).Trim();

            if(name == StringSegment.Empty)
            {
                return false;
            }

            if(!SipCharacters.IsValidField(name.AsSpan()))
            {
                return false;
            }

            var value = currentLine.Value.Subsegment(semicolonIndex + 1).Trim();

            if(value == StringSegment.Empty)
            {
                return false;
            }

            currentHeader = new SipHeader(currentLine.Value, name, value);
            return true;
        }

        private bool TrySliceLine()
        {
            currentLine = null;

            var lineBreakIndex = remainingChars.AsSpan().IndexOf(CRLFString.AsSpan());

            if(lineBreakIndex < 0)
            {
                return false;
            }

            currentLine = remainingChars.Subsegment(0, lineBreakIndex);
            return true;
        }

        private bool TrySkipLine()
        {
            var lineBreakIndex = remainingChars.AsSpan().IndexOf(CRLFString.AsSpan());

            if (lineBreakIndex < 0)
            {
                return false;
            }

            remainingChars = remainingChars.Subsegment(lineBreakIndex + CRLFString.Length);
            return true;
        }
    }
}
