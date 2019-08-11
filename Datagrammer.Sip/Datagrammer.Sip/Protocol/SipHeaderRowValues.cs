using Microsoft.Extensions.Primitives;

namespace Sip.Protocol
{
    public readonly struct SipHeaderRowValues
    {
        private readonly StringSegment values;

        public SipHeaderRowValues(StringSegment values)
        {
            this.values = values;
        }

        public SipHeaderRowValueEnumerator GetEnumerator()
        {
            return new SipHeaderRowValueEnumerator(values);
        }
    }
}
