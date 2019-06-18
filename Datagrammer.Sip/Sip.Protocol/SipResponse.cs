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
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            Version = version;
            Body = body;
            
            Headers = new SipHeaders(headers);

            this.headers = headers;
        }

        public StringSegment StatusCode { get; }

        public StringSegment ReasonPhrase { get; }

        public StringSegment Version { get; }

        public SipHeaders Headers { get; }

        public ReadOnlyMemory<byte> Body { get; }

        public override string ToString()
        {
            return headers.Value;
        }
    }
}
