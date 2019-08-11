using Microsoft.Extensions.Primitives;

namespace Sip.Protocol
{
    public readonly struct CSeqHeader
    {
        internal CSeqHeader(StringSegment method, StringSegment sequenceNumber)
        {
            Method = method;
            SequenceNumber = sequenceNumber;
        }

        public StringSegment Method { get; }

        public StringSegment SequenceNumber { get; }

        public static bool TryParse(StringSegment chars, out CSeqHeader header)
        {
            header = new CSeqHeader();

            var trimmedChars = chars.Trim();
            var sequenceNumber = ReadSequenceNumber(trimmedChars);

            if(!IsSequenceNumberValid(sequenceNumber))
            {
                return false;
            }

            var methodChars = trimmedChars.Subsegment(sequenceNumber.Length).TrimStart();

            if(!IsMethodValid(methodChars))
            {
                return false;
            }

            header = new CSeqHeader(methodChars, sequenceNumber);
            return true;
        }

        private static StringSegment ReadSequenceNumber(StringSegment chars)
        {
            var separatorIndex = SipCharacters.IndexOfWhitespace(chars);

            if(separatorIndex < 0)
            {
                return chars;
            }

            return chars.Subsegment(0, separatorIndex);
        }

        private static bool IsSequenceNumberValid(StringSegment chars)
        {
            return chars.Length > 0 && SipCharacters.IsDigits(chars);
        }

        private static bool IsMethodValid(StringSegment chars)
        {
            return chars.Length > 0 && SipCharacters.IsValidToken(chars);
        }
    }
}
