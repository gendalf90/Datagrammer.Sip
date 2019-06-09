using Microsoft.Extensions.Primitives;
using System;

namespace Datagrammer.Sip
{
    public readonly struct SipResponse
    {
        private readonly StringSegment headers;

        internal SipResponse(StringSegment headers, 
                             StringSegment statusCode,
                             StringSegment reasonPhrase,
                             StringSegment version,
                             ReadOnlyMemory<byte> body)
        {
            Version = version;
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            Body = body;

            this.headers = headers;
        }

        public StringSegment StatusCode { get; }

        public StringSegment ReasonPhrase { get; }

        public StringSegment Version { get; }

        public ReadOnlyMemory<byte> Body { get; }

        public SipHeaderEnumerator GetEnumerator()
        {
            return new SipHeaderEnumerator(headers);
        }

        public override string ToString()
        {
            return headers.Value;
        }
    }
}
