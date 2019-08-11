using Microsoft.Extensions.Primitives;

namespace Sip.Protocol
{
    public readonly struct SipHeader
    {
        private readonly StringSegment header;

        internal SipHeader(StringSegment header, StringSegment name, StringSegment value)
        {
            Name = name;
            Value = value;

            this.header = header;
        }

        public StringSegment Name { get; }

        public StringSegment Value { get; }

        public StringSegment ToStringSegment()
        {
            return header;
        }
    }
}
