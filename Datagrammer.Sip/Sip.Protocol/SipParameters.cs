using Microsoft.Extensions.Primitives;

namespace Sip.Protocol
{
    public readonly struct SipParameters
    {
        private readonly StringSegment parameters;

        public SipParameters(StringSegment parameters)
        {
            this.parameters = parameters;
        }

        public SipParameterEnumerator GetEnumerator()
        {
            return new SipParameterEnumerator(parameters);
        }
    }
}
