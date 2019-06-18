using Microsoft.Extensions.Primitives;

namespace Datagrammer.Sip
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
