using Microsoft.Extensions.Primitives;

namespace Datagrammer.Sip
{
    public readonly struct SipHeader
    {
        public SipHeader(StringSegment name, StringSegment value)
        {
            Name = name;
            Value = value;
        }

        public StringSegment Name { get; }

        public StringSegment Value { get; }
    }
}
