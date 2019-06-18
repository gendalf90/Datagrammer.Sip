using Microsoft.Extensions.Primitives;
using System;

namespace Datagrammer.Sip
{
    public readonly struct SipRequest
    {
        private readonly StringSegment headers;

        internal SipRequest(StringSegment headers,
                            StringSegment method,
                            StringSegment uri,
                            StringSegment version,
                            ReadOnlyMemory<byte> body)
        {
            Method = method;
            Uri = uri;
            Version = version;
            Body = body;

            Headers = new SipHeaders(headers);

            this.headers = headers;
        }

        public StringSegment Method { get; }

        public StringSegment Uri { get; }

        public StringSegment Version { get; }

        public SipHeaders Headers { get; }

        public ReadOnlyMemory<byte> Body { get; }

        public override string ToString()
        {
            return headers.Value;
        }
    }
}
