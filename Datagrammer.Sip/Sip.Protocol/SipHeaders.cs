using Microsoft.Extensions.Primitives;

namespace Sip.Protocol
{
    public readonly struct SipHeaders
    {
        private readonly StringSegment headers;

        internal SipHeaders(StringSegment headers)
        {
            this.headers = headers;
        }

        public SipHeaderEnumerator GetEnumerator()
        {
            return new SipHeaderEnumerator(headers);
        }
    }
}
