using Microsoft.Extensions.Primitives;

namespace Sip.Protocol
{
    public readonly struct SipParameter
    {
        internal SipParameter(StringSegment name, StringSegment value)
        {
            Name = name;
            Value = value;
        }

        public StringSegment Name { get; }

        public StringSegment Value { get; }
    }
}
